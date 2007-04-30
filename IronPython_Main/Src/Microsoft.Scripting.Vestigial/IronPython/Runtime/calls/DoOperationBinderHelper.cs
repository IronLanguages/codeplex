/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Diagnostics;

using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Internal.Ast;

using IronPython.Compiler.Generation;
using IronPython.Compiler;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Actions;
using System.Threading;

namespace IronPython.Runtime.Calls {
    public class DoOperationBinderHelper<T> : BinderHelper<T> {
        private ActionBinder _binder;
        private CodeContext _context;
        protected DoOperationAction _action;

        public DoOperationBinderHelper(ActionBinder binder, CodeContext context, DoOperationAction action) {
            this._binder = binder;
            this._context = context;
            this._action = action;
        }

        public StandardRule<T> MakeRule(object[] args) {
            return MakeNewRule(CompilerHelpers.ObjectTypes(args));
        }

        public bool IsComparision {
            get {
                return _action.IsComparision;
            }
        }

        private static bool MatchesMethodSignature(ParameterInfo[] pis, MethodBase mb) {
            ParameterInfo[] pis1 = mb.GetParameters();
            if (pis.Length == pis1.Length) {
                for (int i = 0; i < pis.Length; i++) {
                    if (pis[i].ParameterType != pis1[i].ParameterType) return false;
                }
                return true;
            } else {
                return false;
            }
        }

        private static bool ContainsMethodSignature(IList<MethodBase> existing, MethodBase check) {
            ParameterInfo[] pis = check.GetParameters();
            foreach (MethodBase mb in existing) {
                if (MatchesMethodSignature(pis, mb)) return true;
            }
            return false;
        }

        private StandardRule<T> MakeDynamicMatchRule(DynamicType[] types) {
            //TODO figure out caching strategy for these
            StandardRule<T> ret = new StandardRule<T>();
            ret.MakeTest(types);
            if (_action.IsUnary) {
                MakeDynamicTarget(DynamicInvokeUnaryOperation, ret);
            } else if (_action.IsInPlace) {
                MakeDynamicTarget(DynamicInvokeInplaceOperation, ret);
            } else if (IsComparision) {
                MakeDynamicTarget(DynamicInvokeCompareOperation, ret);
            } else {
                MakeDynamicTarget(DynamicInvokeBinaryOperation, ret);
            }

            return ret;
        }

        private delegate object DynamicOperationMethod(CodeContext context, Operators op, object x, object y);
        private delegate object DynamicUnaryOperationMethod(CodeContext context, Operators op, object x);
        private void MakeDynamicTarget(DynamicOperationMethod action, StandardRule<T> rule) {
            Expression expr = MethodCallExpression.Call(null, action.Method,
                    new CodeContextExpression(),
                    ConstantExpression.Constant(this._action.Operation),
                    rule.GetParameterExpression(0),
                    rule.GetParameterExpression(1));
            rule.SetTarget(rule.MakeReturn(_binder, expr));
        }
        private void MakeDynamicTarget(DynamicUnaryOperationMethod action, StandardRule<T> rule) {
            Expression expr = MethodCallExpression.Call(null, action.Method,
                    new CodeContextExpression(),
                    ConstantExpression.Constant(this._action.Operation),
                    rule.GetParameterExpression(0));
            rule.SetTarget(rule.MakeReturn(_binder, expr));
        }


        private StandardRule<T> MakeRuleForNoMatch(DynamicType[] types) {
            if (IsComparision) {
                return MakeDynamicMatchRule(types);
            } else {
                return StandardRule<T>.TypeError(
                       MakeBinaryOpErrorMessage(_action.ToString(), types[0], types[1]),
                       types);
            }
        }

        protected StandardRule<T> MakeNewRule(DynamicType[] types) {
            for (int i = 0; i < types.Length; i++) {
                if (types[i].Version == DynamicType.DynamicVersion) {
                    return MakeDynamicMatchRule(types);
                }
            }

            Operators op = _action.Operation;
            if (_action.IsInPlace) {
                DynamicType xType = types[0];
                DynamicTypeSlot xSlot;
                if (xType.TryLookupSlot(_context, Symbols.OperatorToSymbol(op), out xSlot)) {
                    // TODO optimize calls to explicit inplace methods
                    return MakeDynamicMatchRule(types);
                }
                op = _action.DirectOperation;
            }

            if (_action.IsUnary) {
                Debug.Assert(types.Length == 1);
                return MakeUnaryRule(types, op);
            }

            if (types[0] == TypeCache.Object && types[1] == TypeCache.Object) {
                return MakeDynamicMatchRule(types);
            }

            if (IsComparision) {
                return MakeComparisonRule(types, op);
            }
            return MakeSimpleRule(types, op);
        }
        
        private StandardRule<T> MakeSimpleRule(DynamicType[] types, Operators oper) {
            SymbolId op = Symbols.OperatorToSymbol(oper);
            SymbolId rop = Symbols.OperatorToReversedSymbol(oper);

            MethodBinder fbinder, rbinder;
            if (!TryGetBinder(types, op, SymbolId.Empty, out fbinder)) {
                return MakeDynamicMatchRule(types);
            }

            if (!TryGetBinder(types, SymbolId.Empty, rop, out rbinder)) {
                return MakeDynamicMatchRule(types);
            }

            MethodCandidate fCand = ComparisonTargetFromBinder(fbinder, types);
            MethodCandidate rCand = ComparisonTargetFromBinder(rbinder, types);

            return MakeRuleForBinaryOperator(types, oper, fCand, rCand);
        }

        private StandardRule<T> MakeComparisonRule(DynamicType[] types, Operators op) {
            DynamicType xType = types[0];
            DynamicType yType = types[1];
            SymbolId opSym = Symbols.OperatorToSymbol(op);
            SymbolId ropSym = Symbols.OperatorToReversedSymbol(op);

            MethodBinder fbind, rbind, cbind, rcbind;
            // forward
            if (!TryGetBinder(types, opSym, SymbolId.Empty, out fbind)) {
                return MakeDynamicMatchRule(types);
            }

            // reverse
            DynamicType[] rTypes = new DynamicType[] { types[1], types[0] };
            if (!TryGetBinder(rTypes, ropSym, SymbolId.Empty, out rbind)) {
                return MakeDynamicMatchRule(types);
            }

            // __cmp__ 
            if (!TryGetBinder(types, Symbols.Cmp, SymbolId.Empty, out cbind)) {
                return MakeDynamicMatchRule(types);
            }

            // reversed __cmp__ 
            if (!TryGetBinder(rTypes, Symbols.Cmp, SymbolId.Empty, out rcbind)) {
                return MakeDynamicMatchRule(types);
            }

            // fallback binder, depending on what comparison call a helper
            // which always yields a value.
            MethodCandidate forward = ComparisonTargetFromBinder(fbind, types);
            MethodCandidate reverse = ComparisonTargetFromBinder(rbind, rTypes);
            MethodCandidate fcmp = ComparisonTargetFromBinder(cbind, types);
            MethodCandidate rcmp = ComparisonTargetFromBinder(rcbind, rTypes);

            MethodTarget fTarget, rTarget, fCmpTarget, rCmpTarget;
            GetCombinedTargets(forward, reverse, out fTarget, out rTarget);
            GetCombinedTargets(fcmp, rcmp, out fCmpTarget, out rCmpTarget);

            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(types);

            List<Statement> stmts = new List<Statement>();
            if (MakeOneTarget(fTarget, rule, stmts, false)) {
                if (MakeOneTarget(rTarget, rule, stmts, true)) {
                    if (MakeOneCompare(fCmpTarget, rule, stmts, false)) {
                        if (MakeOneCompare(rCmpTarget, rule, stmts, true)) {
                            stmts.Add(MakeFallbackCompare(rule));
                        }
                    }
                }
            }

            rule.SetTarget(BlockStatement.Block(stmts));
            return rule;
        }

        private StandardRule<T> MakeRuleForBinaryOperator(DynamicType[] types, Operators oper, MethodCandidate fCand, MethodCandidate rCand) {
            MethodTarget fTarget, rTarget;

            if (!GetCombinedTargets(fCand, rCand, out fTarget, out rTarget)) {
                return MakeRuleForNoMatch(types);
            }

            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(types);

            List<Statement> stmts = new List<Statement>();
            if (MakeOneTarget(fTarget, rule, stmts, false)) {
                if (MakeOneTarget(rTarget, rule, stmts, false)) {
                    stmts.Add(MakeBinaryThrow(rule));
                }
            }
            rule.SetTarget(BlockStatement.Block(stmts));
            return rule;
        }

        private Expression MakeCall(MethodTarget target, StandardRule<T> block, bool reverse) {
            VariableReference[] vars = block.Parameters;
            if (reverse) {
                VariableReference[] newVars = new VariableReference[2];
                newVars[0] = vars[1];
                newVars[1] = vars[0];
                vars = newVars;
            }
            return target.MakeExpression(_binder, vars);
        }

        private bool MakeOneTarget(MethodTarget target, StandardRule<T> block, List<Statement> stmts, bool reverse) {
            if (target == null) return true;

            if (ReturnsNotImplemented(target)) {
                VariableReference tmp = block.GetTemporary(target.ReturnType, "tmp");
                stmts.Add(IfStatement.IfThen(
                    BinaryExpression.NotEqual(
                        BoundAssignment.Assign(tmp, MakeCall(target, block, reverse)),
                        MemberExpression.Field(null, typeof(Ops).GetField("NotImplemented"))),
                    block.MakeReturn(_binder, BoundExpression.Defined(tmp))));
                return true;
            } else {
                stmts.Add(block.MakeReturn(_binder, MakeCall(target, block, reverse)));
                return false;
            }
        }

        private static bool ReturnsNotImplemented(MethodTarget target) {
            MethodInfo mi = target.Method as MethodInfo;
            if (mi != null) {
                return mi.ReturnTypeCustomAttributes.IsDefined(typeof(MaybeNotImplementedAttribute), false);
            }

            return false;
        }

        private Statement MakeUnaryThrow(StandardRule<T> block) {
            return new ExpressionStatement(new ThrowExpression(
                MethodCallExpression.Call(null, typeof(Ops).GetMethod("TypeErrorForUnaryOp"),
                    ConstantExpression.Constant(SymbolTable.IdToString(Symbols.OperatorToSymbol(_action.Operation))),
                    block.GetParameterExpression(0))));
        }

        private Statement MakeBinaryThrow(StandardRule<T> block) {
            return new ExpressionStatement(new ThrowExpression(
                MethodCallExpression.Call(null, typeof(Ops).GetMethod("TypeErrorForBinaryOp"),
                    ConstantExpression.Constant(SymbolTable.IdToString(Symbols.OperatorToSymbol(_action.Operation))),
                    block.GetParameterExpression(0), block.GetParameterExpression(1))));
        }

        private bool MakeOneCompare(MethodTarget target, StandardRule<T> block, List<Statement> stmts, bool reverse) {
            if (target == null) return true;

            if (ReturnsNotImplemented(target)) {
                VariableReference tmp = block.GetTemporary(target.ReturnType, "tmp");
                stmts.Add(IfStatement.IfThen(
                    BinaryExpression.NotEqual(
                        BoundAssignment.Assign(tmp, MakeCall(target, block, reverse)),
                        MemberExpression.Field(null, typeof(Ops).GetField("NotImplemented"))),
                    MakeCompareTest(BoundExpression.Defined(tmp), block, reverse)));
                return true;
            } else {
                stmts.Add(MakeCompareTest(MakeCall(target, block, reverse), block, reverse));
                return false;
            }
        }

        private Statement MakeCompareTest(Expression expr, StandardRule<T> block, bool reverse) {
            return block.MakeReturn(_binder, 
                MethodCallExpression.Call(null, GetCompareMethod(reverse),
                    MethodCallExpression.Call(null, typeof(Ops).GetMethod("CompareToZero"),
                        expr)));
        }

        private MethodInfo GetCompareMethod(bool reverse) {
            string name;

            switch (reverse ? CompilerHelpers.OperatorToReverseOperator(_action.Operation) : _action.Operation) {
                case Operators.Equal: name = "CompareEqual"; break;
                case Operators.NotEqual: name = "CompareNotEqual"; break;
                case Operators.GreaterThan: name = "CompareGreaterThan"; break;
                case Operators.GreaterThanOrEqual: name = "CompareGreaterThanOrEqual"; break;
                case Operators.LessThan: name = "CompareLessThan"; break;
                case Operators.LessThanOrEqual: name = "CompareLessThanOrEqual"; break;
                default: throw new InvalidOperationException();
            }

            return typeof(Ops).GetMethod(name);
        }

        private Statement MakeFallbackCompare(StandardRule<T> block) {
            return block.MakeReturn(_binder, 
                MethodCallExpression.Call(null, GetComparisonFallbackMethod(_action.Operation),
                    block.GetParameterExpression(0),
                    block.GetParameterExpression(1)));
        }



        /// <summary>
        /// Gets the logically combined targets.  If the 1st target is preferred over the 2nd one 
        /// we'll return both.
        /// </summary>
        private static bool GetCombinedTargets(MethodCandidate fCand, MethodCandidate rCand, out MethodTarget fTarget, out MethodTarget rTarget) {
            fTarget = rTarget = null;

            if (fCand != null) {
                if (rCand != null) {
                    if (fCand.NarrowingLevel <= rCand.NarrowingLevel) {
                        fTarget = fCand.Target;
                        rTarget = rCand.Target;
                    } else {
                        fTarget = null;
                        rTarget = rCand.Target;
                    }
                } else {
                    fTarget = fCand.Target;
                }
            } else if (rCand != null) {
                rTarget = rCand.Target;
            } else {
                return false;
            }
            return true;
        }

        private MethodCandidate ComparisonTargetFromBinder(MethodBinder binder, DynamicType[] types) {
            if (binder == null) return null;
            return binder.MakeBindingTarget(CallType.None, types);
        }

        private MethodInfo GetComparisonFallbackMethod(Operators op) {
            string name;
            switch (op) {
                case Operators.Equal: name = "CompareTypesEqual"; break;
                case Operators.NotEqual: name = "CompareTypesNotEqual"; break;
                case Operators.GreaterThan: name = "CompareTypesGreaterThan"; break;
                case Operators.LessThan: name = "CompareTypesLessThan"; break;
                case Operators.GreaterThanOrEqual: name = "CompareTypesGreaterThanOrEqual"; break;
                case Operators.LessThanOrEqual: name = "CompareTypesLessThanOrEqual"; break;
                default: throw new InvalidOperationException();
            }
            return typeof(Ops).GetMethod(name);
        }

        // TODO: Remove rop
        private bool TryGetBinder(DynamicType[] types, SymbolId op, SymbolId rop, out MethodBinder binder) {
            DynamicType xType = types[0];
            DynamicType yType = types[1];
            binder = null;

            BuiltinFunction xBf;
            if (!TryGetStaticFunction(op, xType, out xBf)) {
                return false;
            }

            BuiltinFunction yBf = null;
            if (!xType.IsSubclassOf(yType) && !TryGetStaticFunction(rop, yType, out yBf)) {
                return false;
            }

            if (yBf == xBf) {
                yBf = null;
            } else if (yBf != null && yType.IsSubclassOf(xType)) {
                xBf = null;
            }

            if (xBf == null) {
                if (yBf == null) {
                    binder = null;
                } else {
                    binder = MethodBinder.MakeBinder(_binder, op.ToString(),
                        yBf.Targets, MethodBinderType);
                }
            } else {
                if (yBf == null) {
                    binder = MethodBinder.MakeBinder(_binder, op.ToString(),
                        xBf.Targets, MethodBinderType);
                } else {
                    List<MethodBase> targets = new List<MethodBase>();
                    targets.AddRange(xBf.Targets);
                    foreach (MethodBase mb in yBf.Targets) {
                        if (!ContainsMethodSignature(targets, mb)) targets.Add(mb);
                    }
                    binder = MethodBinder.MakeBinder(_binder, op.ToString(),
                        targets.ToArray(), MethodBinderType);
                }
            }
            return true;
        }

        private bool TryGetStaticFunction(SymbolId op, DynamicType type, out BuiltinFunction function) {
            function = null;
            if (op != SymbolId.Empty) {
                DynamicTypeSlot xSlot;
                object val;
                if (type.TryResolveSlot(_context, op, out xSlot) &&
                    xSlot.TryGetValue(_context, null, type, out val)) {
                    function = TryConvertToBuiltinFunction(val);
                    if (function == null) return false;
                }
            }
            return true;
        }

        private BinderType MethodBinderType {
            get {
                return IsComparision ? BinderType.ComparisonOperator : BinderType.BinaryOperator;
            }
        }

        public static object DynamicInvokeBinaryOperation(CodeContext context, Operators op, object x, object y) {
            DynamicType xDType = Ops.GetDynamicType(x);
            DynamicType yDType = Ops.GetDynamicType(y);
            object ret;

            if (xDType == yDType || !yDType.IsSubclassOf(xDType)) {
                if (xDType.TryInvokeBinaryOperator(context, op, x, y, out ret) &&
                    ret != Ops.NotImplemented) {
                    return ret;
                }
            }

            if (xDType != yDType) {
                if (yDType.TryInvokeBinaryOperator(context, CompilerHelpers.OperatorToReverseOperator(op), y, x, out ret) &&
                    ret != Ops.NotImplemented) {
                    return ret;
                }
            }

            throw Ops.TypeError(MakeBinaryOpErrorMessage(op.ToString(), xDType, yDType));
        }

        internal static string MakeBinaryOpErrorMessage(string op, DynamicType xType, DynamicType yType) {
            return string.Format("unsupported operand type(s) for {2}: '{0}' and '{1}'",
                                xType.Name, yType.Name, op);
        }

        internal static string MakeUnaryOpErrorMessage(string op, DynamicType xType) {
            return string.Format("unsupported operand type for {1}: '{0}'",
                                xType.Name, op);
        }

        private static bool FinishCompareOperation(int cmp, Operators op) {
            switch (op) {
                case Operators.LessThan: return cmp < 0;
                case Operators.LessThanOrEqual: return cmp <= 0;
                case Operators.GreaterThan: return cmp > 0;
                case Operators.GreaterThanOrEqual: return cmp >= 0;
                case Operators.Equal: return cmp == 0;
                case Operators.NotEqual: return cmp != 0;
            }
            throw new ArgumentException("op");
        }

        public static object DynamicInvokeCompareOperation(CodeContext context, Operators op, object x, object y) {
            object ret;

            DynamicType dt1 = Ops.GetDynamicType(x);
            DynamicType dt2 = Ops.GetDynamicType(y);

            if (dt1.TryInvokeBinaryOperator(context, op, x, y, out ret) &&
                ret != Ops.NotImplemented) {
                return ret;
            }
            if (dt2.TryInvokeBinaryOperator(context,
                CompilerHelpers.OperatorToReverseOperator(op), y, x, out ret) &&
                ret != Ops.NotImplemented) {
                return ret;
            }

            if (dt1.TryInvokeBinaryOperator(context, Operators.Compare, x, y, out ret) && ret != Ops.NotImplemented)
                return Ops.Bool2Object(FinishCompareOperation(Ops.CompareToZero(ret), op));

            if (dt2.TryInvokeBinaryOperator(context, Operators.Compare, y, x, out ret) && ret != Ops.NotImplemented)
                return Ops.Bool2Object(FinishCompareOperation(-1 * Ops.CompareToZero(ret), op));

            if (dt1 == dt2) {
                return Ops.Bool2Object(FinishCompareOperation((int)(IdDispenser.GetId(x) - IdDispenser.GetId(y)), op));
            } else {
                if (dt1 == TypeCache.None) return FinishCompareOperation(-1, op);
                if (dt2 == TypeCache.None) return FinishCompareOperation(+1, op);
                return Ops.Bool2Object(FinishCompareOperation(string.CompareOrdinal(dt1.Name, dt2.Name), op));
            }
        }

        public static object DynamicInvokeInplaceOperation(CodeContext context, Operators op, object x, object y) {
            object ret;
            DynamicType dt = Ops.GetDynamicType(x);
            if (dt.TryInvokeBinaryOperator(context, op, x, y, out ret) && ret != Ops.NotImplemented)
                return ret;

            //TODO something's backwards here - shouldn't need to make a new action
            return DynamicInvokeBinaryOperation(context, DoOperationAction.Make(op).DirectOperation, x, y);
        }

        #region Unary Operators

        private StandardRule<T> MakeUnaryRule(DynamicType[] types, Operators op) {
            if (op == Operators.Not) {
                return MakeUnaryNotRule(types);
            }

            BuiltinFunction func;
            if (!TryGetStaticFunction(Symbols.OperatorToSymbol(op), types[0], out func)) {
                return MakeDynamicMatchRule(types);
            }

            if (func == null) {
                return StandardRule<T>.TypeError(MakeUnaryOpErrorMessage(_action.ToString(), types[0]), types);
            }

            MethodBinder binder = MethodBinder.MakeBinder(_binder,
                op.ToString(),
                func.Targets,
                MethodBinderType);

            Debug.Assert(binder != null);

            MethodCandidate cand = binder.MakeBindingTarget(CallType.None, types);
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(types);
            rule.SetTarget(rule.MakeReturn(_binder, MakeCall(cand.Target, rule, false)));
            return rule;
        }

        private StandardRule<T> MakeUnaryNotRule(DynamicType[] types) {
            BuiltinFunction nonzero;
            if (!TryGetStaticFunction(Symbols.NonZero, types[0], out nonzero)) {
                return MakeDynamicNotRule(types);
            }

            BuiltinFunction len;
            if (!TryGetStaticFunction(Symbols.Length, types[0], out len)) {
                return MakeDynamicNotRule(types);
            }

            Expression notExpr;
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(types);

            if (nonzero == null && len == null) {
                // always False or True for None
                notExpr = ConstantExpression.Constant(types[0].IsNull ? true : false);
            } else {
                MethodBinder binder = MethodBinder.MakeBinder(_binder,
                    "Not",
                    nonzero != null ? nonzero.Targets : len.Targets,
                    MethodBinderType);

                Debug.Assert(binder != null);
                MethodCandidate cand = binder.MakeBindingTarget(CallType.None, types);
                notExpr = MakeCall(cand.Target, rule, false);

                if (nonzero != null) {
                    // call non-zero and negate it
                    if (notExpr.ExpressionType == typeof(bool)) {
                        notExpr = BinaryExpression.Equal(notExpr, new ConstantExpression(false));
                    } else {
                        notExpr = new StaticUnaryExpression(Operators.Not, notExpr, typeof(Ops).GetMethod("Not"));
                    }
                } else {
                    // call len, compare w/ zero
                    if (notExpr.ExpressionType == typeof(int)) {
                        notExpr = BinaryExpression.Equal(notExpr, new ConstantExpression(0));
                    } else {
                        // TODO: nested site - we hit this on OldInstance's.
                        notExpr = MethodCallExpression.Call(null, typeof(Ops).GetMethod("Compare"), notExpr, new ConstantExpression(0));
                    }
                }
            }
            rule.SetTarget(rule.MakeReturn(_binder, notExpr));
            return rule;
        }

        private StandardRule<T> MakeDynamicNotRule(DynamicType[] types) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(types);
            rule.SetTarget(
                rule.MakeReturn(_binder,
                    MethodCallExpression.Call(null,
                        typeof(Ops).GetMethod("Not"),
                        rule.GetParameterExpression(0))));
            return rule;
        }

        public static object DynamicInvokeUnaryOperation(CodeContext context, Operators op, object x) {
            object ret;
            DynamicType dt = Ops.GetDynamicType(x);
            if (dt.TryInvokeUnaryOperator(context, op, x, out ret) && ret != Ops.NotImplemented)
                return ret;

            throw Ops.TypeError(MakeUnaryOpErrorMessage(op.ToString(), Ops.GetDynamicType(x)));
        }

        #endregion

        public override string ToString() {
            return string.Format("BinaryOperatorAction({0})", _action);
        }
    }



}
