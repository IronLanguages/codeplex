/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

#if !SILVERLIGHT
using Microsoft.Scripting.Actions.ComDispatch;
#endif

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

using IronPython.Compiler;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Calls {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class PythonDoOperationBinderHelper<T> : BinderHelper<T, DoOperationAction> {
        private object[] _args;
        private bool _testCoerceRecursionCheck, _disallowCoercion;

        public PythonDoOperationBinderHelper(CodeContext context, DoOperationAction action)
            : base(context, action) {
        }

        public RuleBuilder<T> MakeRule(object[] args) {
            // we're using the user defined flag here because there's no DLR custom actions yet.
            if ((Action.Operation & Operators.UserDefinedFlag) != 0) {
                _disallowCoercion = true;
            }

            _args = args;
            return MakeNewRule(PythonTypeOps.ObjectTypes(args));
        }

        public bool IsComparision {
            get {
                return CompilerHelpers.IsComparisonOperator(Operation);
            }
        }

        public bool IsInPlace {
            get {
                return CompilerHelpers.InPlaceOperatorToOperator(Operation) != Operators.None;
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


        private RuleBuilder<T> MakeRuleForNoMatch(PythonType[] types) {
            // we get the error message w/ {0}, {1} so that TypeError formats it correctly
            return PythonBinderHelper.TypeError<T>(
                   MakeBinaryOpErrorMessage(Operation, "{0}", "{1}"),
                   types);
        }
        
        protected RuleBuilder<T> MakeNewRule(PythonType[] types) {

#if !SILVERLIGHT
            if (ComObject.IsGenericComObject(_args[0])) {
                return null;
            }
#endif

            if (Operation == Operators.IsCallable) {
                // This will break in cross-language cases. Eg, if this rule applies to x,
                // then Python's callable(x) will invoke this rule, but Ruby's callable(x) 
                // will use the Ruby language binder instead and miss this rule, and thus 
                // may get a different result than python.
                return PythonBinderHelper.MakeIsCallableRule<T>(this.Context, _args[0]);
            }

            if (Operation == Operators.GetItem || Operation == Operators.SetItem ||
                Operation == Operators.GetSlice || Operation == Operators.SetSlice ||
                Operation == Operators.DeleteItem || Operation == Operators.DeleteSlice) {
                // Indexers need to see if the index argument is an expandable tuple.  This will
                // be captured in the AbstractValue in the future but today is captured in the
                // real value.
                return MakeIndexerRule(types);
            }

            Operators op = Operation;
            if (op == Operators.MemberNames) {
                return MakeMemberNamesRule(types);
            } else if (op == Operators.CallSignatures) {
                return MakeCallSignatureRule(Binder, CompilerHelpers.GetMethodTargets(_args[0]), types);
            }

            if (Action.IsUnary) {
                Debug.Assert(types.Length == 1);
                return MakeUnaryRule(types, op);
            }

            if (IsComparision) {
                return MakeComparisonRule(types, op);
            }

            return MakeSimpleRule(types, op);
        }

        internal static RuleBuilder<T> MakeCallSignatureRule(ActionBinder binder, IList<MethodBase> targets, params PythonType[] types) {
            List<string> arrres = new List<string>();
            foreach (MethodBase mb in targets) {
                StringBuilder res = new StringBuilder();
                string comma = "";

                Type retType = CompilerHelpers.GetReturnType(mb);
                if (retType != typeof(void)) {
                    res.Append(PythonTypeOps.GetName(retType));
                    res.Append(" ");
                }

                MethodInfo mi = mb as MethodInfo;
                if (mi != null) {
                    string name;
                    NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mb.DeclaringType), mi, out name);
                    res.Append(name);
                } else {
                    res.Append(PythonTypeOps.GetName(mb.DeclaringType));
                }

                res.Append("(");
                if (!CompilerHelpers.IsStatic(mb)) {
                    res.Append("self");
                    comma = ", ";
                }

                foreach (ParameterInfo pi in mb.GetParameters()) {
                    if (pi.ParameterType == typeof(CodeContext)) continue;

                    res.Append(comma);
                    res.Append(PythonTypeOps.GetName(pi.ParameterType) + " " + pi.Name);
                    comma = ", ";
                }
                res.Append(")");
                arrres.Add(res.ToString());
            }
            RuleBuilder<T> rule = new RuleBuilder<T>();
            PythonBinderHelper.MakeTest(rule, types);
            rule.Target = rule.MakeReturn(binder, Ast.RuntimeConstant(arrres.ToArray()));
            return rule;
        }

        private RuleBuilder<T> MakeMemberNamesRule(PythonType[] types) {
            if (typeof(IMembersList).IsAssignableFrom(types[0].UnderlyingSystemType)) {
                return null;
            }

            IList<SymbolId> names = types[0].GetMemberNames(Context);
            RuleBuilder<T> rule = new RuleBuilder<T>();
            PythonBinderHelper.MakeTest(rule, types);

            rule.Target = rule.MakeReturn(
                Binder,
                Ast.RuntimeConstant(SymbolTable.IdsToStrings(names))
            );
            return rule;
        }

        private RuleBuilder<T> MakeSimpleRule(PythonType[] types, Operators oper) {
            SymbolId op, rop;

            if (Action.IsInPlace) {
                oper = Action.DirectOperation;
            }

            if (!IsReverseOperator(oper)) {
                op = Symbols.OperatorToSymbol(oper);
                rop = Symbols.OperatorToReversedSymbol(oper);
            } else {
                // coming back after coercion, just try reverse operator.
                rop = Symbols.OperatorToSymbol(oper);
                op = Symbols.OperatorToReversedSymbol(oper);
            }

            MethodBinder fbinder, rbinder;
            PythonTypeSlot fSlot = null, rSlot = null;
            if (!TryGetBinder(types, op, SymbolId.Empty, out fbinder)) {
                types[0].TryResolveSlot(Context, op, out fSlot);
            }

            if (!TryGetBinder(types, SymbolId.Empty, rop, out rbinder)) {
                types[1].TryResolveSlot(Context, rop, out rSlot);
                if (types[1].IsSubclassOf(types[0])) {
                    // Python says if x + subx and subx defines __r*__ we should call r*.
                    fbinder = null;
                }
            }

            BindingTarget fCand = ComparisonTargetFromBinder(fbinder, types);
            BindingTarget rCand = ComparisonTargetFromBinder(rbinder, types);

            return MakeRuleForBinaryOperator(types, oper, fCand, rCand, fSlot, rSlot);
        }

        #region Comparisons

        private RuleBuilder<T> MakeComparisonRule(PythonType[] types, Operators op) {
            if (op == Operators.Compare) {
                return MakeSortComparisonRule(types, op);
            }

            PythonType xType = types[0];
            PythonType yType = types[1];
            SymbolId opSym = Symbols.OperatorToSymbol(op);
            SymbolId ropSym = Symbols.OperatorToReversedSymbol(op);
            // reverse
            PythonType[] rTypes = new PythonType[] { types[1], types[0] };

            SlotOrFunction fop, rop, cmp, rcmp;
            fop = GetSlotOrFunction(types, opSym);
            rop = GetSlotOrFunction(rTypes, ropSym);
            cmp = GetSlotOrFunction(types, Symbols.Cmp);
            rcmp = GetSlotOrFunction(rTypes, Symbols.Cmp);

            SlotOrFunction.GetCombinedTargets(fop, rop, out fop, out rop);
            SlotOrFunction.GetCombinedTargets(cmp, rcmp, out cmp, out rcmp);

            RuleBuilder<T> rule = new RuleBuilder<T>();
            PythonBinderHelper.MakeTest(rule, types);

            List<Expression> stmts = new List<Expression>();
            // first try __op__ or __rop__ and return the value
            if (MakeOneCompareGeneric(fop, rule, stmts, false, types, MakeCompareReturn)) {
                if (MakeOneCompareGeneric(rop, rule, stmts, true, types, MakeCompareReturn)) {
                    
                    // then try __cmp__ or __rcmp__ and compare the resulting int appropriaetly
                    if (ShouldCoerce(xType, yType, true)) {
                        stmts.Add(DoCoerce(rule, Operators.Compare, types, false, delegate(Expression e) {
                            return GetCompareTest(e, false);
                        }));
                    }
                    
                    if (MakeOneCompareGeneric(cmp, rule, stmts, false, types, MakeCompareTest)) {
                        if (ShouldCoerce(yType, xType, true)) {
                            stmts.Add(DoCoerce(rule, Operators.Compare, rTypes, true, delegate(Expression e) {
                                return GetCompareTest(e, true);
                            }));
                        }

                        if (MakeOneCompareGeneric(rcmp, rule, stmts, true, types, MakeCompareTest)) {
                            stmts.Add(MakeFallbackCompare(rule));
                        }
                    }
                }
            }

            rule.Target = Ast.Block(stmts);
            return rule;
        }

        /// <summary>
        /// Makes the comparison rule which returns an int (-1, 0, 1).  TODO: Better name?
        /// </summary>
        private RuleBuilder<T> MakeSortComparisonRule(PythonType[] types, Operators op) {
            // Python compare semantics: 
            //      if the types are the same invoke __cmp__ first.
            //      If __cmp__ is not defined or the types are different:
            //          try rich comparisons (eq, lt, gt, etc...) 
            //      If the types are not the same and rich cmp didn't work finally try __cmp__
            //      If __cmp__ isn't defined return a comparison based upon the types.
            //
            // Along the way we try both forward and reverse versions (try types[0] and then
            // try types[1] reverse version).  For these comparisons __cmp__ and __eq__ are their
            // own reversals and __gt__ is the opposite of __lt__.

            // collect all the comparison methods, most likely we won't need them all.
            PythonType[] rTypes = new PythonType[] { types[1], types[0] };
            SlotOrFunction cfunc, rcfunc, eqfunc, reqfunc, ltfunc, gtfunc, rltfunc, rgtfunc;

            cfunc = GetSlotOrFunction(types, Symbols.Cmp);
            rcfunc = GetSlotOrFunction(rTypes, Symbols.Cmp);
            eqfunc = GetSlotOrFunction(types, Symbols.OperatorEquals);
            reqfunc = GetSlotOrFunction(rTypes, Symbols.OperatorEquals);
            ltfunc = GetSlotOrFunction(types, Symbols.OperatorLessThan);
            gtfunc = GetSlotOrFunction(types, Symbols.OperatorGreaterThan);
            rltfunc = GetSlotOrFunction(rTypes, Symbols.OperatorLessThan);
            rgtfunc = GetSlotOrFunction(rTypes, Symbols.OperatorGreaterThan);

            // inspect forward and reverse versions so we can pick one or both.
            SlotOrFunction cTarget, rcTarget, eqTarget, reqTarget, ltTarget, rgtTarget, gtTarget, rltTarget;
            SlotOrFunction.GetCombinedTargets(cfunc, rcfunc, out cTarget, out rcTarget);
            SlotOrFunction.GetCombinedTargets(eqfunc, reqfunc, out eqTarget, out reqTarget);
            SlotOrFunction.GetCombinedTargets(ltfunc, rgtfunc, out ltTarget, out rgtTarget);
            SlotOrFunction.GetCombinedTargets(gtfunc, rltfunc, out gtTarget, out rltTarget);

            PythonType xType = types[0];
            PythonType yType = types[1];

            // now build the rule from the targets.
            RuleBuilder<T> rule = new RuleBuilder<T>();
            PythonBinderHelper.MakeTest(rule, types);

            // bail if we're comparing to null and the rhs can't do anything special...
            if (xType.IsNull) {
                if (yType.IsNull) {
                    rule.Target = rule.MakeReturn(Binder, Ast.Zero());
                    return rule;
                } else if (yType.UnderlyingSystemType.IsPrimitive || yType.UnderlyingSystemType == typeof(Microsoft.Scripting.Math.BigInteger)) {
                    rule.Target = rule.MakeReturn(Binder, Ast.Constant(-1));
                    return rule;
                }
            }

            List<Expression> stmts = new List<Expression>();

            bool tryRich = true, more = true;
            if (xType == yType && cTarget != null) {
                // if the types are equal try __cmp__ first
                if (ShouldCoerce(xType, yType, true)) {
                    // need to try __coerce__ first.
                    stmts.Add(DoCoerce(rule, Operators.Compare, types, false));
                }

                more = more && MakeOneCompareGeneric(cTarget, rule, stmts, false, types, MakeCompareReverse);

                if (xType != TypeCache.OldInstance) {
                    // try __cmp__ backwards for new-style classes and don't fallback to
                    // rich comparisons if available
                    more = more && MakeOneCompareGeneric(rcTarget, rule, stmts, true, types, MakeCompareReverse);
                    tryRich = false;
                }
            }

            if (tryRich) {
                // try eq
                if (more) {
                    MakeOneCompareGeneric(eqTarget, rule, stmts, false, types, MakeCompareToZero);
                    MakeOneCompareGeneric(reqTarget, rule, stmts, true, types, MakeCompareToZero);

                    // try less than & reverse
                    MakeOneCompareGeneric(ltTarget, rule, stmts, false, types, MakeCompareToNegativeOne);
                    MakeOneCompareGeneric(rgtTarget, rule, stmts, true, types, MakeCompareToNegativeOne);

                    // try greater than & reverse
                    MakeOneCompareGeneric(gtTarget, rule, stmts, false, types, MakeCompareToOne);
                    MakeOneCompareGeneric(rltTarget, rule, stmts, true, types, MakeCompareToOne);
                }
            }

            if (xType != yType) {
                if (more && ShouldCoerce(xType, yType, true)) {
                    // need to try __coerce__ first.
                    stmts.Add(DoCoerce(rule, Operators.Compare, types, false));
                }

                more = more && MakeOneCompareGeneric(cTarget, rule, stmts, false, types, MakeCompareReverse);

                if (more && ShouldCoerce(yType, xType, true)) {
                    // try __coerce__ first
                    stmts.Add(DoCoerce(rule, Operators.Compare, rTypes, true, delegate(Expression e) {
                        return ReverseCompareValue(e);
                    }));
                }

                more = more && MakeOneCompareGeneric(rcTarget, rule, stmts, true, types, MakeCompareReverse);
            }

            if (more) {
                // fall back to compare types
                stmts.Add(MakeFallbackCompare(rule));
            }

            rule.Target = Ast.Block(stmts);
            return rule;
        }

        private class SlotOrFunction {
            private readonly BindingTarget _function;
            private readonly PythonTypeSlot _slot;
            public static readonly SlotOrFunction Empty = new SlotOrFunction();

            private SlotOrFunction() {
            }

            public SlotOrFunction(BindingTarget function) {
                _function = function;
            }

            public SlotOrFunction(PythonTypeSlot slot) {
                _slot = slot;
            }

            public NarrowingLevel NarrowingLevel {
                get {
                    if (_function != null) {
                        return _function.NarrowingLevel;
                    }

                    return NarrowingLevel.None;
                }
            }

            public Type ReturnType {
                get {
                    if (_function != null) return _function.ReturnType;

                    return typeof(object);
                }
            }

            public bool MaybeNotImplemented {
                get {
                    if (_function != null) {
                        return ReturnsNotImplemented(_function);
                    }

                    return true;
                }
            }

            public bool Success {
                get {
                    return _slot != null || _function != null;
                }
            }

            public Expression MakeCall(RuleBuilder<T> rule, IList<Expression> args) {
                if (_function != null) {
                    return _function.MakeExpression(rule, args);
                } else {
                    VariableExpression tmp = rule.GetTemporary(typeof(object), "slotVal");

                    return Ast.Comma(
                        PythonBinderHelper.MakeTryGetTypeMember<T>(
                            rule, 
                            _slot, 
                            tmp, 
                            args[0], 
                            Ast.Call(
                                typeof(DynamicHelpers).GetMethod("GetPythonType"),
                                args[0]
                            )
                        ),
                        Ast.Action.Call(typeof(object), ArrayUtils.Insert<Expression>(Ast.Read(tmp), ArrayUtils.RemoveFirst(args)))
                    );
                }
            }

            public static void GetCombinedTargets(SlotOrFunction fCand, SlotOrFunction rCand, out SlotOrFunction fTarget, out SlotOrFunction rTarget) {
                fTarget = rTarget = null;

                if (fCand.Success) {
                    if (rCand.Success) {
                        if (fCand.NarrowingLevel <= rCand.NarrowingLevel) {
                            fTarget = fCand;
                            rTarget = rCand;
                        } else {
                            fTarget = null;
                            rTarget = rCand;
                        }
                    } else {
                        fTarget = fCand;
                    }
                } else if (rCand.Success) {
                    rTarget = rCand;
                } 
            }
        }

        private SlotOrFunction GetSlotOrFunction(PythonType[] types, SymbolId op) {
            MethodBinder binder;
            PythonTypeSlot slot;

            if (TryGetBinder(types, op, SymbolId.Empty, out binder)) {
                if (binder != null) {
                    BindingTarget bt = binder.MakeBindingTarget(CallTypes.None, PythonTypeOps.ConvertToTypes(types));
                    if (bt != null && bt.Success) {
                        return new SlotOrFunction(bt);
                    }
                }
            } else if (types[0].TryResolveSlot(Context, op, out slot)) {
                return new SlotOrFunction(slot);
            }

            return SlotOrFunction.Empty;
        }

        /// <summary>
        /// Helper to handle a comparison operator call.  Checks to see if the call can
        /// return NotImplemented and allows the caller to modify the expression that
        /// is ultimately returned (e.g. to turn __cmp__ into a bool after a comparison)
        /// </summary>
        private bool MakeOneCompareGeneric(SlotOrFunction target, RuleBuilder<T> rule, List<Expression> stmts, bool reverse, PythonType[] types, Function<Expression, RuleBuilder<T>, bool, Expression> returner) {
            if (target == null || !target.Success) return true;

            VariableExpression tmp = rule.GetTemporary(target.ReturnType, "compareRetValue");
            Expression call = target.MakeCall(rule, CheckTypesAndReverse(rule, reverse, types));
            Expression assign = Ast.Assign(tmp, call);
            Expression ret = returner(Ast.ReadDefined(tmp), rule, reverse);

            if (target.MaybeNotImplemented) {
                stmts.Add(
                    Ast.IfThen(
                        Ast.NotEqual(
                            assign,
                            Ast.ReadField(null, typeof(PythonOps).GetField("NotImplemented"))
                        ),
                        ret)
                    );
                return true;
            } else {
                stmts.Add(Ast.Comma(assign, ret));
                return false;
            }
        }
        
        private static BinaryExpression ReverseCompareValue(Expression retVal) {
            return Ast.Multiply(
                Ast.ConvertHelper(
                    retVal,
                    typeof(int)
                ),
                Ast.Constant(-1)
            );
        }

        /// <summary>
        /// Checks if a coercion check should be performed.  We perform coercion under the following
        /// situations:
        ///     1. Old instances performing a binary operator (excluding rich comparisons)
        ///     2. User-defined new instances calling __cmp__ but only if we wouldn't dispatch to a built-in __coerce__ on the parent type
        ///     
        /// This matches the behavior of CPython.
        /// </summary>
        /// <returns></returns>
        private bool ShouldCoerce(PythonType xType, PythonType yType, bool isCompare) {
            if (_disallowCoercion) return false;

            if (xType == TypeCache.OldInstance) return true;

            if (isCompare && !xType.IsSystemType && yType.IsSystemType) {
                if (yType == TypeCache.Int32 ||
                    yType == TypeCache.BigInteger ||
                    yType == TypeCache.Double ||
                    yType == TypeCache.Complex64) {

                    // only coerce new style types that define __coerce__ and
                    // only when comparing against built-in types which
                    // define __coerce__
                    PythonTypeSlot pts;
                    if (xType.TryResolveSlot(Context, Symbols.Coerce, out pts)) {
                        // don't call __coerce__ if it's declared on the base type
                        BuiltinMethodDescriptor bmd = pts as BuiltinMethodDescriptor;
                        if (bmd == null)  return true;

                        if (bmd.__name__ != "__coerce__" &&
                            bmd.DeclaringType != typeof(int) &&
                            bmd.DeclaringType != typeof(BigInteger) &&
                            bmd.DeclaringType != typeof(double) &&
                            bmd.DeclaringType != typeof(Complex64)) {
                            return true;
                        }

                        foreach (PythonType pt in xType.ResolutionOrder) {
                            if (pt.UnderlyingSystemType == bmd.DeclaringType) {
                                // inherited __coerce__
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private Expression GetCompareTest(Expression expr, bool reverse) {
            if (expr.Type == typeof(int)) {
                // fast path, just do a compare in IL
                return GetCompareNode(reverse, expr);
            } else {
                return Ast.Call(
                    GetCompareMethod(reverse),
                    Ast.Call(
                        typeof(PythonOps).GetMethod("CompareToZero"),
                        Ast.ConvertHelper(expr, typeof(object))
                    )
                );
            }
        }

        private Expression MakeCompareReturn(Expression expr, RuleBuilder<T> rule, bool reverse) {
            return rule.MakeReturn(Binder, expr);
        }

        private Expression MakeCompareReverse(Expression expr, RuleBuilder<T> rule, bool reverse) {
            Expression res = expr;
            if (reverse) {
                res = ReverseCompareValue(expr);
            }

            return MakeCompareReturn(res, rule, reverse);
        }

        private Expression MakeCompareTest(Expression expr, RuleBuilder<T> rule, bool reverse) {
            return MakeCompareReturn(GetCompareTest(expr, reverse), rule, reverse);
        }

        private Expression MakeCompareToZero(Expression expr, RuleBuilder<T> rule, bool reverse) {
            return MakeValueCheck(rule, 0, expr);
        }

        private Expression MakeCompareToOne(Expression expr, RuleBuilder<T> rule, bool reverse) {
            return MakeValueCheck(rule, 1, expr);
        }

        private Expression MakeCompareToNegativeOne(Expression expr, RuleBuilder<T> rule, bool reverse) {
            return MakeValueCheck(rule, -1, expr);
        }

        private Expression MakeValueCheck(RuleBuilder<T> rule, int val, Expression test) {
            if (test.Type != typeof(bool)) {
                test = Ast.Action.ConvertTo(typeof(bool), ConversionResultKind.ExplicitCast, test);
            }
            return Ast.IfThen(
                test,
                rule.MakeReturn(Binder, Ast.Constant(val))
            );
        }

        private Expression GetCompareNode(bool reverse, Expression expr) {
            switch (reverse ? CompilerHelpers.OperatorToReverseOperator(Operation) : Operation) {
                case Operators.Equals: return Ast.Equal(expr, Ast.Constant(0));
                case Operators.NotEquals: return Ast.NotEqual(expr, Ast.Constant(0));
                case Operators.GreaterThan: return Ast.GreaterThan(expr, Ast.Constant(0));
                case Operators.GreaterThanOrEqual: return Ast.GreaterThanEquals(expr, Ast.Constant(0));
                case Operators.LessThan: return Ast.LessThan(expr, Ast.Constant(0));
                case Operators.LessThanOrEqual: return Ast.LessThanEquals(expr, Ast.Constant(0));
                default: throw new InvalidOperationException();
            }
        }

        private MethodInfo GetCompareMethod(bool reverse) {
            string name;

            switch (reverse ? CompilerHelpers.OperatorToReverseOperator(Operation) : Operation) {
                case Operators.Equals: name = "CompareEqual"; break;
                case Operators.NotEquals: name = "CompareNotEqual"; break;
                case Operators.GreaterThan: name = "CompareGreaterThan"; break;
                case Operators.GreaterThanOrEqual: name = "CompareGreaterThanOrEqual"; break;
                case Operators.LessThan: name = "CompareLessThan"; break;
                case Operators.LessThanOrEqual: name = "CompareLessThanOrEqual"; break;
                default: throw new InvalidOperationException();
            }

            return typeof(PythonOps).GetMethod(name);
        }

        private Expression MakeFallbackCompare(RuleBuilder<T> block) {
            return block.MakeReturn(Binder,
                Ast.Call(
                    GetComparisonFallbackMethod(Operation),
                    Ast.ConvertHelper(block.Parameters[0], typeof(object)),
                    Ast.ConvertHelper(block.Parameters[1], typeof(object))
                )
            );
        }

        #endregion

        private RuleBuilder<T> MakeRuleForBinaryOperator(PythonType[] types, Operators oper, BindingTarget fCand, BindingTarget rCand, PythonTypeSlot fSlot, PythonTypeSlot rSlot) {
            BindingTarget fTarget, rTarget;

            RuleBuilder<T> rule = new RuleBuilder<T>();
            PythonBinderHelper.MakeTest(rule, types);
            List<Expression> stmts = new List<Expression>();

            if (Action.IsInPlace) {
                // in place operator, see if there's a specific method that handles it.
                SlotOrFunction function = GetSlotOrFunction(types, Symbols.OperatorToSymbol(Operation));

                // we don't do a coerce for in place operators if the lhs implements __iop__
                if (!MakeOneCompareGeneric(function, rule, stmts, false, types, MakeCompareReturn)) {
                    // the method handles it and always returns a useful value.
                    rule.Target = Ast.Block(stmts);
                    return rule;
                }
            }

            if (!GetCombinedTargets(fCand, rCand, out fTarget, out rTarget) &&
                fSlot == null &&
                rSlot == null &&
                !ShouldCoerce(types[0], types[1], false) &&
                !ShouldCoerce(types[1], types[0], false) &&
                stmts.Count == 0) {
                return MakeRuleForNoMatch(types);
            }
            
            if (ShouldCoerce(types[0], types[1], false) && (oper != Operators.Mod || !types[0].IsSubclassOf(TypeCache.String))) {
                // need to try __coerce__ first.
                stmts.Add(DoCoerce(rule, oper, types, false));
            }

            if (MakeOneTarget(fTarget, fSlot, rule, stmts, false, types)) {
                if (ShouldCoerce(types[1], types[0], false)) {
                    // need to try __coerce__ on the reverse first                    
                    stmts.Add(DoCoerce(rule, oper, new PythonType[] { types[1], types[0] }, true));
                }

                if (rSlot != null) {
                    stmts.Add(MakeSlotCall(rSlot, rule, true));
                    stmts.Add(MakeBinaryThrow(rule));
                } else if (MakeOneTarget(rTarget, rSlot, rule, stmts, false, types)) {
                    // need to fallback to throwing or coercion
                    stmts.Add(MakeBinaryThrow(rule));
                } 
            }

            rule.Target = Ast.Block(stmts);
            return rule;
        }

        private Expression DoCoerce(RuleBuilder<T> rule, Operators op, PythonType[] types, bool reverse) {
            return DoCoerce(rule, op, types, reverse, delegate(Expression e) {
                return e;
            });
        }

        /// <summary>
        /// calls __coerce__ for old-style classes and performs the operation if the coercion is successful.
        /// </summary>
        private Expression DoCoerce(RuleBuilder<T> rule, Operators op, PythonType[] types, bool reverse, Function<Expression, Expression> returnTransform) {
            VariableExpression coerceResult = rule.GetTemporary(typeof(object), "coerceResult");
            VariableExpression coerceTuple = rule.GetTemporary(typeof(PythonTuple), "coerceTuple");

            if (!_testCoerceRecursionCheck) {
                // during coercion we need to enforce recursion limits if
                // they're enabled and the rule's test needs to reflect this.
                rule.AddTest(
                    Ast.Equal(
                        Ast.Call(typeof(PythonOps).GetMethod("ShouldEnforceRecursion")),
                        Ast.Constant(PythonFunction.EnforceRecursion)
                    )
                );

                _testCoerceRecursionCheck = true; 
            }

            Expression self, other;
            if (reverse) {
                self = Ast.ConvertHelper(rule.Parameters[1], types[0].UnderlyingSystemType);
                other = Ast.ConvertHelper(rule.Parameters[0], types[1].UnderlyingSystemType);
            } else {
                self = Ast.ConvertHelper(rule.Parameters[0], types[0].UnderlyingSystemType);
                other = Ast.ConvertHelper(rule.Parameters[1], types[1].UnderlyingSystemType);
            }

            // tmp = self.__coerce__(other)
            // if tmp != null && tmp != NotImplemented && (tuple = PythonOps.ValidateCoerceResult(tmp)) != null:
            //      return operation(tuple[0], tuple[1])                        
            SlotOrFunction slot = GetSlotOrFunction(types, Symbols.Coerce);
            
            if (slot.Success) {
                return Ast.If(
                    Ast.AndAlso(
                        Ast.Not(
                            Ast.TypeIs(
                                Ast.Assign(
                                    coerceResult,
                                    slot.MakeCall(
                                        rule,
                                        new Expression[] { self, other }
                                    )
                                ),
                                typeof(OldInstance)
                            )
                        ),
                        Ast.NotEqual(
                            Ast.Assign(
                                coerceTuple,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("ValidateCoerceResult"),
                                    Ast.Read(coerceResult)
                                )
                            ),
                            Ast.Constant(null)
                        )
                    ),
                    PythonBinderHelper.AddRecursionCheck(
                        rule.MakeReturn(
                            Binder,
                            returnTransform(
                                Ast.Action.Operator(
                                    op | Operators.UserDefinedFlag,     // TODO: Replace w/ custom action
                                    typeof(object),
                                    reverse ? CoerceTwo(coerceTuple) : CoerceOne(coerceTuple),
                                    reverse ? CoerceOne(coerceTuple) : CoerceTwo(coerceTuple)
                                )
                            )
                        )
                    )
                );
            }

            return Ast.Empty();
        }

        private static MethodCallExpression CoerceTwo(VariableExpression coerceTuple) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("GetCoerceResultTwo"),
                Ast.Read(coerceTuple)
            );
        }

        private static MethodCallExpression CoerceOne(VariableExpression coerceTuple) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("GetCoerceResultOne"),
                Ast.Read(coerceTuple)
            );
        }

        #region Indexer rule support

        /// <summary>
        /// Python has three protocols for slicing:
        ///    Simple Slicing x[i:j]
        ///    Extended slicing x[i,j,k,...]
        ///    Long Slice x[start:stop:step]
        /// 
        /// The first maps to __*slice__ (get, set, and del).  
        ///    This takes indexes - i, j - which specify the range of elements to be
        ///    returned.  In the slice variants both i, j must be numeric data types.  
        /// The 2nd and 3rd are both __*item__.  
        ///    This receives a single index which is either a Tuple or a Slice object (which 
        ///    encapsulates the start, stop, and step values) 
        /// 
        /// This is in addition to a simple indexing x[y].
        /// 
        /// For simple slicing and long slicing Python generates Operators.*Slice.  For
        /// the extended slicing and simple indexing Python generates a Operators.*Item
        /// action.
        /// 
        /// Extended slicing maps to the normal .NET multi-parameter input.  
        /// 
        /// So our job here is to first determine if we're to call a __*slice__ method or
        /// a __*item__ method.  
        private RuleBuilder<T> MakeIndexerRule(PythonType[] types) {
            SymbolId item, slice;
            PythonType indexedType = types[0];
            BuiltinFunction itemFunc = null;
            PythonTypeSlot itemSlot = null;
            bool callSlice = false;
            int mandatoryArgs;

            GetIndexOperators(out item, out slice, out mandatoryArgs);

            if (types.Length == mandatoryArgs + 1 && IsSlice && HasOnlyNumericTypes(types, Operation == Operators.SetSlice)) {
                // two slice indexes, all int arguments, need to call __*slice__ if it exists
                callSlice = TryGetStaticFunction(slice, indexedType, out itemFunc);
                if(itemFunc == null || !callSlice) {
                    callSlice = indexedType.TryResolveSlot(Context, slice, out itemSlot);
                }
            }

            if (!callSlice) {
                // 1 slice index (simple index) or multiple slice indexes or no __*slice__, call __*item__, 
                if (!TryGetStaticFunction(item, indexedType, out itemFunc)) {
                    indexedType.TryResolveSlot(Context, item, out itemSlot);
                }
            }

            // make the Callable object which does the actual call to the function or slot
            Callable callable = Callable.MakeCallable(Binder, Operation, itemFunc, itemSlot);
            if (callable == null) {
                return PythonBinderHelper.TypeError<T>("'{0}' object is unsubscriptable", types[0]);
            }
            
            // prepare the arguments and make the builder which will
            // call __*slice__ or __*item__
            Expression[] args;
            IndexBuilder builder;
            RuleBuilder<T> rule = new RuleBuilder<T>();
            if (callSlice) {
                // we're going to call a __*slice__ method, we pass the args as is.
                Debug.Assert(IsSlice);

                builder = new SliceBuilder(rule, types, callable);
                args = ConvertArgs(types, rule.Parameters);
            } else {
                // we're going to call a __*item__ method.
                builder = new ItemBuilder(rule, types, callable);
                if (IsSlice) {
                    // we need to create a new Slice object.
                    args = GetItemSliceArguments(rule, types);
                } else {
                    // we just need to pass the arguments as they are
                    args = ConvertArgs(types, rule.Parameters);
                }
            }

            // finally make the rule
            builder.MakeRule(args);
            return rule;
        }

        /// <summary>
        /// Helper to convert all of the arguments to their known types.
        /// </summary>
        private static Expression[] ConvertArgs(PythonType[] types, IList<Expression> args) {
            Expression[] res = new Expression[args.Count];
            for (int i = 0; i < args.Count; i++) {
                res[i] = Ast.ConvertHelper(args[i], CompilerHelpers.GetVisibleType(types[i].UnderlyingSystemType));
            }
            return res;
        }

        /// <summary>
        /// Gets the arguments that need to be provided to __*item__ when we need to pass a slice object.
        /// </summary>
        private Expression[] GetItemSliceArguments(RuleBuilder<T> rule, PythonType[] types) {
            Expression[] args;
            if (Operation == Operators.SetSlice) {
                args = new Expression[] { 
                            Ast.ConvertHelper(rule.Parameters[0], types[0]), 
                            GetSetSlice(rule), 
                            Ast.ConvertHelper(rule.Parameters[rule.Parameters.Count - 1], types[types.Length - 1])
                        };
            } else {
                Debug.Assert(Operation == Operators.GetSlice || Operation == Operators.DeleteSlice);

                args = new Expression[] { 
                    Ast.ConvertHelper(rule.Parameters[0], types[0]),
                    GetGetOrDeleteSlice(rule)
                };
            }
            return args;
        }

        /// <summary>
        /// Base class for calling indexers.  We have two subclasses that target built-in functions & user defined callable objects.
        /// 
        /// The Callable objects get handed off to ItemBuilder's which then call them with the appropriate arguments.
        /// </summary>
        abstract class Callable {
            private readonly ActionBinder/*!*/ _binder;
            private readonly Operators _op;

            protected Callable(ActionBinder/*!*/ binder, Operators op) {
                Assert.NotNull(binder);

                _binder = binder;
                _op = op;
            }

            /// <summary>
            /// Creates a new CallableObject.  If BuiltinFunction is available we'll create a BuiltinCallable otherwise
            /// we create a SlotCallable.
            /// </summary>
            public static Callable MakeCallable(ActionBinder binder, Operators op, BuiltinFunction itemFunc, PythonTypeSlot itemSlot) {
                if (itemFunc != null) {
                    // we'll call a builtin function to produce the rule
                    return new BuiltinCallable(binder, op, itemFunc);
                } else if (itemSlot != null) {
                    // we'll call a PythonTypeSlot to produce the rule
                    return new SlotCallable(binder, op, itemSlot);
                } 

                return null;
            }

            /// <summary>
            /// Gets the arguments in a form that should be used for extended slicing.
            /// 
            /// Python defines that multiple tuple arguments received (x[1,2,3]) get 
            /// packed into a Tuple.  For most .NET methods we just want to expand
            /// this into the multiple index arguments.  For slots and old-instances
            /// we want to pass in the tuple
            /// </summary>
            public virtual Expression[] GetTupleArguments(Expression[] arguments) {
                if (IsSetter) {
                    if (arguments.Length == 3) {
                        // simple setter, no extended slicing, no need to pack arguments into tuple
                        return arguments;
                    }

                    // we want self, (tuple, of, args, ...), value
                    Expression[] tupleArgs = new Expression[arguments.Length - 2];
                    for (int i = 1; i < arguments.Length - 1; i++) {
                        tupleArgs[i - 1] = Ast.ConvertHelper(arguments[i], typeof(object));
                    }
                    return new Expression[] {
                        arguments[0],
                        Ast.Call(
                            typeof(PythonOps).GetMethod("MakeTuple"),
                            Ast.NewArray(typeof(object[]), tupleArgs)
                        ),
                        arguments[arguments.Length-1]
                    };
                } else if (arguments.Length == 2) {
                    // simple getter, no extended slicing, no need to pack arguments into tuple
                    return arguments;
                } else {
                    // we want self, (tuple, of, args, ...)
                    Expression[] tupleArgs = new Expression[arguments.Length - 1];
                    for (int i = 1; i < arguments.Length; i++) {
                        tupleArgs[i - 1] = Ast.ConvertHelper(arguments[i], typeof(object));
                    }
                    return new Expression[] {
                        arguments[0],
                        Ast.Call(
                            typeof(PythonOps).GetMethod("MakeTuple"),
                            Ast.NewArray(typeof(object[]), tupleArgs)
                        )
                    };
                }
            }

            /// <summary>
            /// Adds the target of the call to the rule.
            /// </summary>
            public abstract void CompleteRuleTarget(RuleBuilder<T> rule, Expression[] args, Function<bool> customFailure);

            protected ActionBinder Binder {
                get { return _binder; }
            }

            protected Operators Operator {
                get { return _op; }
            }

            protected bool IsSetter {
                get { return _op == Operators.SetItem || _op == Operators.SetSlice; }
            }
        }

        /// <summary>
        /// Subclass of Callable for a built-in function.  This calls a .NET method performing
        /// the appropriate bindings.
        /// </summary>
        class BuiltinCallable : Callable {
            private readonly BuiltinFunction/*!*/ _bf;            

            public BuiltinCallable(ActionBinder/*!*/ binder, Operators op, BuiltinFunction/*!*/ func)
                : base(binder, op) {
                Assert.NotNull(func);

                _bf = func;
            }

            public override Expression[] GetTupleArguments(Expression[] arguments) {
                if (arguments[0].Type == typeof(OldInstance)) {
                    // old instances are special in that they take only a single parameter
                    // in their indexer but accept multiple parameters as tuples.
                    return base.GetTupleArguments(arguments);
                }
                return arguments;
            }

            public override void CompleteRuleTarget(RuleBuilder<T>/*!*/ rule, Expression/*!*/[]/*!*/ args, Function<bool> customFailure) {
                Assert.NotNull(args);
                Assert.NotNullItems(args);
                Assert.NotNull(rule);

                MethodBinder binder = MethodBinder.MakeBinder(Binder,
                    Name,
                    _bf.Targets,
                    PythonNarrowing.None,
                    PythonNarrowing.IndexOperator);

                Type[] types = CompilerHelpers.GetExpressionTypes(args);
                BindingTarget target = binder.MakeBindingTarget(CallTypes.ImplicitInstance, types);

                if (target.Success) {
                    Expression call = target.MakeExpression(rule, args);

                    if (IsSetter) {
                        call = Ast.Comma(call, rule.Parameters[rule.Parameters.Count - 1]);
                    }

                    rule.Target = rule.MakeReturn(Binder, call);
                } else if (customFailure == null || !customFailure()) {
                    rule.Target = Binder.MakeInvalidParametersError(target).MakeErrorForRule(rule, Binder);
                }
            }

            private string Name {
                get {
                    switch (Operator) {
                        case Operators.GetSlice: return "__getslice__";
                        case Operators.SetSlice: return "__setslice__";
                        case Operators.DeleteSlice: return "__delslice__";
                        case Operators.GetItem: return "__getitem__";
                        case Operators.SetItem: return "__setitem__";
                        case Operators.DeleteItem: return "__delitem__";
                        default: throw new InvalidOperationException();
                    }
                }
            }
        }

        /// <summary>
        /// Callable to a user-defined callable object.  This could be a Python function,
        /// a class defining __call__, etc...
        /// </summary>
        class SlotCallable : Callable {
            private PythonTypeSlot _slot;

            public SlotCallable(ActionBinder binder, Operators op, PythonTypeSlot slot)
                : base(binder, op) {
                _slot = slot;
            }

            public override void CompleteRuleTarget(RuleBuilder<T> rule, Expression[] args, Function<bool> customFailure) {
                VariableExpression callable = rule.GetTemporary(typeof(object), "slot");

                Expression retVal = Ast.Action.Call(
                    typeof(object),
                    ArrayUtils.Insert<Expression>(
                        Ast.Read(callable),
                        ArrayUtils.RemoveFirst(args)
                    )
                );

                if (IsSetter) {
                    retVal = Ast.Comma(retVal, rule.Parameters[rule.Parameters.Count - 1]);
                }

                rule.Target =
                    Ast.Comma(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("SlotTryGetValue"),
                            Ast.CodeContext(),
                            Ast.ConvertHelper(Ast.WeakConstant(_slot), typeof(PythonTypeSlot)),
                            Ast.ConvertHelper(args[0], typeof(object)),
                            Ast.Call(
                                typeof(DynamicHelpers).GetMethod("GetPythonType"),
                                Ast.ConvertHelper(args[0], typeof(object))
                            ),
                            Ast.Read(callable)
                        ),
                        rule.MakeReturn(Binder, retVal)
                    );
            }
        }

        /// <summary>
        /// Base class for building a __*item__ or __*slice__ call.
        /// </summary>
        abstract class IndexBuilder {
            private RuleBuilder<T> _rule;
            private Callable _callable;
            private PythonType[] _types;

            public IndexBuilder(RuleBuilder<T> rule, PythonType[] types, Callable callable) {
                _rule = rule;
                _callable = callable;
                _types = types;
                PythonBinderHelper.MakeTest(Rule, types);
            }
            
            public abstract RuleBuilder<T> MakeRule(Expression[] args);

            protected RuleBuilder<T> Rule {
                get { return _rule; }
            }

            protected Callable Callable {
                get { return _callable; }
            }

            protected PythonType[] Types {
                get { return _types; }
            }
        }

        /// <summary>
        /// Derived IndexBuilder for calling __*slice__ methods
        /// </summary>
        class SliceBuilder : IndexBuilder {
            private VariableExpression _lengthVar;        // Nullable<int>, assigned if we need to calculate the length of the object during the call.

            public SliceBuilder(RuleBuilder<T> rule, PythonType[] types, Callable callable)
                : base(rule, types, callable) {
            }

            public override RuleBuilder<T> MakeRule(Expression[] args) {
                // the semantics of simple slicing state that if the value
                // is less than 0 then the length is added to it.  The default
                // for unprovided parameters are 0 and maxint.  The callee
                // is responsible for ignoring out of range values but slicing
                // is responsible for doing this initial transformation.

                Debug.Assert(args.Length > 2);  // index 1 and 2 should be our slice indexes, we might have another arg if we're a setter
                for (int i = 1; i < 3; i++) {
                    if (args[i].Type == typeof(MissingParameter)) {
                        switch(i) {
                            case 1: args[i] = Ast.Constant(0);  break;
                            case 2: args[i] = Ast.Constant(Int32.MaxValue); break;
                        }
                    } else if (args[i].Type == typeof(int)) {
                        args[i] = MakeIntTest(args[0], args[i]);
                    } else if (args[i].Type.IsSubclassOf(typeof(Extensible<int>))) {
                        args[i] = MakeIntTest(args[0], Ast.ReadProperty(args[i], args[i].Type.GetProperty("Value")));
                    } else if (args[i].Type == typeof(BigInteger)) {
                        args[i] = MakeBigIntTest(args[0], args[i]);
                    } else if (args[i].Type.IsSubclassOf(typeof(Extensible<BigInteger>))) {
                        args[i] = MakeBigIntTest(args[0], Ast.ReadProperty(args[i], args[i].Type.GetProperty("Value")));
                    } else if (args[i].Type == typeof(bool)) {
                        args[i] = Ast.Condition(args[i], Ast.Constant(1), Ast.Constant(0));
                    } else {
                        // this type defines __index__, otherwise we'd have an ItemBuilder constructing a slice
                        args[i] = MakeIntTest(args[0],
                            Ast.Action.Call(
                                typeof(int),
                                Ast.Action.GetMember(
                                    Symbols.Index,
                                    typeof(object),
                                    args[i]
                                )
                            )
                        );
                    }
                }
                
                if (_lengthVar != null) {
                    // we need the length which we should only calculate once, calculate and
                    // store it in a temporary.  Note we only calculate the length if we'll
                    args[0] = Ast.Comma(
                        Ast.Assign(_lengthVar, Ast.Constant(null)),
                        args[0]
                    );
                }

                Callable.CompleteRuleTarget(Rule, args, null);
                return Rule;
            }

            private Expression MakeBigIntTest(Expression self, Expression bigInt) {
                EnsureLengthVariable();

                return Ast.Call(
                    typeof(PythonOps).GetMethod("NormalizeBigInteger"),
                    self,
                    bigInt,
                    Ast.Read(_lengthVar)
                );
            }

            private Expression MakeIntTest(Expression self, Expression intVal) {
                return Ast.Condition(
                    Ast.LessThan(intVal, Ast.Constant(0)),
                    Ast.Add(intVal, MakeGetLength(self)),
                    intVal
                );
            }

            private Expression MakeGetLength(Expression self) {
                EnsureLengthVariable();

                return Ast.Call(
                    typeof(PythonOps).GetMethod("GetLengthOnce"),
                    self,
                    Ast.Read(_lengthVar)
                );
            }

            private void EnsureLengthVariable() {
                if (_lengthVar == null) {
                    _lengthVar = Rule.GetTemporary(typeof(Nullable<int>), "objLength");
                }
            }
        }

        /// <summary>
        /// Derived IndexBuilder for calling __*item__ methods.
        /// </summary>
        class ItemBuilder : IndexBuilder {
            public ItemBuilder(RuleBuilder<T> rule, PythonType[] types, Callable callable)
                : base(rule, types, callable) {
            }

            public override RuleBuilder<T> MakeRule(Expression[] args) {                
                Expression[] tupleArgs = Callable.GetTupleArguments(args);
                Callable.CompleteRuleTarget(Rule, tupleArgs, delegate() {
                    PythonTypeSlot indexSlot;
                    if (args[1].Type != typeof(Slice) && Types[1].TryResolveSlot(DefaultContext.Default, Symbols.Index, out indexSlot)) {
                        args[1] = Ast.Action.Call(
                            typeof(int),
                            Ast.Action.GetMember(Symbols.Index, typeof(object), args[1])
                        );

                        Callable.CompleteRuleTarget(Rule, tupleArgs, null);
                        return true;
                    }
                    return false;
                });
                return Rule;
            }
        }

        private bool HasOnlyNumericTypes(PythonType[] types, bool skipLast) {
            bool onlyNumeric = true;

            for (int i = 1; i < (skipLast ? types.Length - 1 : types.Length); i++) {
                PythonTypeSlot dummy;

                if (types[i].UnderlyingSystemType != typeof(MissingParameter) && 
                    !PythonOps.IsNumericType(types[i].UnderlyingSystemType) &&
                    !types[i].TryResolveSlot(Context, Symbols.Index, out dummy)) {
                    onlyNumeric = false;
                    break;
                }
            }
            return onlyNumeric;
        }

        private bool IsSlice {
            get {
                return Operation == Operators.GetSlice || Operation == Operators.SetSlice || Operation == Operators.DeleteSlice;
            }
        }

        private void FixArgsForIndex(PythonType[] types, Expression[] args, Type[] callTypes) {
            if (Operation == Operators.GetItem || Operation == Operators.SetItem || Operation == Operators.DeleteItem) {
                Debug.Assert(args.Length == types.Length && types.Length == callTypes.Length);

                for (int i = 1; i < types.Length; i++) {
                    if (PythonOps.IsNumericType(types[i].UnderlyingSystemType)) continue;

                    PythonTypeSlot indexSlot;
                    if (types[i].TryResolveSlot(Context, Symbols.Index, out indexSlot)) {
                        args[i] = Ast.Action.Call(
                            typeof(int),
                            Ast.Action.GetMember(Symbols.Index, typeof(object), args[i])
                        );

                        callTypes[i] = typeof(int);
                    }
                }
            }
        }

        /// <summary>
        /// Helper to get the symbols for __*item__ and __*slice__ based upon if we're doing
        /// a get/set/delete and the minimum number of arguments required for each of those.
        /// </summary>
        private void GetIndexOperators(out SymbolId item, out SymbolId slice, out int mandatoryArgs) {
            switch (Operation) {
                case Operators.GetItem:
                case Operators.GetSlice:
                    item = Symbols.GetItem;
                    slice = Symbols.GetSlice;
                    mandatoryArgs = 2;
                    return;
                case Operators.SetItem:
                case Operators.SetSlice:
                    item = Symbols.SetItem;
                    slice = Symbols.SetSlice;
                    mandatoryArgs = 3;
                    return;
                case Operators.DeleteItem:
                case Operators.DeleteSlice:
                    item = Symbols.DelItem;
                    slice = Symbols.DeleteSlice;
                    mandatoryArgs = 2;
                    return;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Checks to see if any of the index types define __index__ and updates the types to indicate this.  Returns
        /// true if at least one __index__ is found, false otherwise.
        /// </summary>
        private bool ResolveOperatorIndex(PythonType[] types, RuleBuilder<T> ret, Expression[] indexArgs) {
            bool foundIndexable = false;

            for (int i = 1; i < types.Length; i++) {
                if (types[i].IsSubclassOf(TypeCache.Int32) || types[i].IsSubclassOf(TypeCache.BigInteger)) {
                    // these are inherently indexes...
                    continue;
                }

                PythonTypeSlot pts;
                if (types[i].TryResolveSlot(Context, Symbols.Index, out pts)) {
                    foundIndexable = true;
                    VariableExpression tmp = ret.GetTemporary(typeof(object), "slotVal");
                    indexArgs[i] = Ast.Comma(
                        PythonBinderHelper.MakeTryGetTypeMember<T>(ret, pts, tmp, indexArgs[i], Ast.RuntimeConstant(types[i])),
                        Ast.Action.Call(typeof(int), Ast.Read(tmp))
                    );
                    types[i] = TypeCache.Int32;
                }
            }
            return foundIndexable;
        }                     

        private static Expression GetSetSlice(RuleBuilder<T> ret) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeSlice"),
                Ast.ConvertHelper(GetSetParameter(ret, 1), typeof(object)),
                Ast.ConvertHelper(GetSetParameter(ret, 2), typeof(object)),
                Ast.ConvertHelper(GetSetParameter(ret, 3), typeof(object))
            );
        }

        private static Expression GetGetOrDeleteSlice(RuleBuilder<T> ret) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeSlice"),
                Ast.ConvertHelper(GetGetOrDeleteParameter(ret, 1), typeof(object)),
                Ast.ConvertHelper(GetGetOrDeleteParameter(ret, 2), typeof(object)),
                Ast.ConvertHelper(GetGetOrDeleteParameter(ret, 3), typeof(object))
            );
        }

        private static Expression GetGetOrDeleteParameter(RuleBuilder<T> ret, int index) {
            if (ret.Parameters.Count > index) {
                return CheckMissing(ret.Parameters[index]);
            }
            return Ast.Null();
        }

        private static Expression GetSetParameter(RuleBuilder<T> ret, int index) {
            if (ret.Parameters.Count > (index + 1)) {
                return CheckMissing(ret.Parameters[index]);
            }

            return Ast.Null();
        }

        internal static Expression CheckMissing(Expression toCheck) {
            if (toCheck.Type == typeof(MissingParameter)) {
                return Ast.Null();
            }
            if (toCheck.Type != typeof(object)) {
                return toCheck;
            }

            return Ast.Condition(
                Ast.TypeIs(toCheck, typeof(MissingParameter)),
                Ast.Null(),
                toCheck
            );
        }

        #endregion

        private Expression MakeCall(BindingTarget target, RuleBuilder<T> block, bool reverse) {
            return MakeCall(target, block, reverse, null);
        }

        private Expression MakeCall(BindingTarget target, RuleBuilder<T> block, bool reverse, PythonType[] types) {
            IList<Expression> vars = CheckTypesAndReverse(block, reverse, types);

            return target.MakeExpression(block, vars);
        }

        private static IList<Expression> CheckTypesAndReverse(RuleBuilder<T> block, bool reverse, PythonType[] types) {
            IList<Expression> vars = ReverseArgs(block, reverse, ref types);

            // add casts to the known types to avoid full conversions that MakeExpression will emit.
            vars = CheckTypes(types, vars);
            return vars;
        }

        private static IList<Expression> CheckTypes(PythonType[] types, IList<Expression> vars) {
            if (types != null) {
                vars = ArrayUtils.MakeArray(vars);
                for (int i = 0; i < types.Length; i++) {
                    if (types[i] != null) {
                        vars[i] = Ast.ConvertHelper(vars[i], CompilerHelpers.GetVisibleType(types[i].UnderlyingSystemType));
                    }
                }
            }
            return vars;
        }

        private static IList<Expression> ReverseArgs(RuleBuilder<T> block, bool reverse, ref PythonType[] types) {
            IList<Expression> vars = block.Parameters;
            if (reverse) {
                Expression[] newVars = new Expression[2];
                newVars[0] = vars[1];
                newVars[1] = vars[0];
                vars = newVars;

                if (types != null) {
                    PythonType[] newTypes = new PythonType[2];
                    newTypes[0] = types[1];
                    newTypes[1] = types[0];
                    types = newTypes;
                }
            }
            return vars;
        }

        private bool MakeOneTarget(BindingTarget target, RuleBuilder<T> block, List<Expression> stmts, bool reverse, PythonType[] types) {
            return MakeOneTarget(target,
                null,
                block,
                stmts,
                reverse,
                types);
        }

        private bool MakeOneTarget(BindingTarget target, PythonTypeSlot slotTarget, RuleBuilder<T> block, List<Expression> stmts, bool reverse, PythonType[] types) {
            if (target == null && slotTarget == null) return true;

            if (slotTarget != null) {
                stmts.Add(MakeSlotCall(slotTarget, block, reverse));
                return true;
            } else if (ReturnsNotImplemented(target)) {
                stmts.Add(CheckNotImplemented(block, MakeCall(target, block, reverse, types)));
                return true;
            } else {
                stmts.Add(block.MakeReturn(Binder, MakeCall(target, block, reverse, types)));
                return false;
            }
        }

        private Expression MakeSlotCall(PythonTypeSlot/*!*/ slotTarget, RuleBuilder<T>/*!*/ block, bool reverse) {
            Debug.Assert(slotTarget != null);
            Debug.Assert(block != null);

            Expression self, other;
            if (reverse) {
                self = block.Parameters[1];
                other = block.Parameters[0];
            } else {
                self = block.Parameters[0];
                other = block.Parameters[1];
            }

            return MakeSlotCallWorker(slotTarget, block, self, other);
        }

        private Expression MakeSlotCallWorker(PythonTypeSlot slotTarget, RuleBuilder<T> block, Expression self, params Expression[] args) {
            VariableExpression callable = block.GetTemporary(typeof(object), "slot");
            return Ast.IfThen(
                Ast.Call(
                    typeof(PythonOps).GetMethod("SlotTryGetValue"),
                    Ast.CodeContext(),
                    Ast.ConvertHelper(Ast.WeakConstant(slotTarget), typeof(PythonTypeSlot)),
                    Ast.ConvertHelper(self, typeof(object)),
                    Ast.Call(
                        typeof(DynamicHelpers).GetMethod("GetPythonType"),
                        Ast.ConvertHelper(self, typeof(object))
                    ),
                    Ast.Read(callable)
                ),
                CheckNotImplemented(
                    block,
                    Ast.Action.Call(
                        typeof(object),
                        ArrayUtils.Insert<Expression>(
                            Ast.Read(callable),
                            args
                        )
                    )
                )
            );
        }

        private Expression CheckNotImplemented(RuleBuilder<T> block, Expression call) {
            VariableExpression tmp = block.GetTemporary(call.Type, "tmp");

            Expression notImplCheck = Ast.IfThen(
                Ast.NotEqual(
                    Ast.Assign(tmp, call),
                    Ast.ReadField(null, typeof(PythonOps).GetField("NotImplemented"))),
                block.MakeReturn(Binder, Ast.ReadDefined(tmp)));

            return notImplCheck;
        }

        private static bool ReturnsNotImplemented(BindingTarget target) {
            if (target.ReturnType == typeof(object)) {
                MethodInfo mi = target.Method as MethodInfo;
                if (mi != null) {
                    return mi.ReturnTypeCustomAttributes.IsDefined(typeof(MaybeNotImplementedAttribute), false);
                }
            }

            return false;
        }

        private Expression MakeUnaryThrow(RuleBuilder<T> block) {
            return Ast.Throw(
                Ast.Call(
                    typeof(PythonOps).GetMethod("TypeErrorForUnaryOp"),
                    Ast.Constant(SymbolTable.IdToString(Symbols.OperatorToSymbol(Operation))),
                    Ast.ConvertHelper(block.Parameters[0], typeof(object))
                )
            );
        }

        private Expression MakeBinaryThrow(RuleBuilder<T> block) {
            return Ast.Throw(
                Ast.Call(
                    typeof(PythonOps).GetMethod("TypeErrorForBinaryOp"),
                    Ast.Constant(SymbolTable.IdToString(Symbols.OperatorToSymbol(Operation))),
                    Ast.ConvertHelper(block.Parameters[0], typeof(object)),
                    Ast.ConvertHelper(block.Parameters[1], typeof(object))
                )
            );
        }
        
        /// <summary>
        /// Gets the logically combined targets.  If the 1st target is preferred over the 2nd one 
        /// we'll return both.
        /// </summary>
        private static bool GetCombinedTargets(BindingTarget fCand, BindingTarget rCand, out BindingTarget fTarget, out BindingTarget rTarget) {
            fTarget = rTarget = null;

            if (fCand != null && fCand.Success) {
                if (rCand != null && rCand.Success) {
                    if (fCand.NarrowingLevel <= rCand.NarrowingLevel) {
                        fTarget = fCand;
                        rTarget = rCand;
                    } else {
                        fTarget = null;
                        rTarget = rCand;
                    }
                } else {
                    fTarget = fCand;
                }
            } else if (rCand != null && rCand.Success) {
                rTarget = rCand;
            } else {
                return false;
            }
            return true;
        }

        private BindingTarget ComparisonTargetFromBinder(MethodBinder binder, PythonType[] types) {
            if (binder == null) return null;
            return binder.MakeBindingTarget(CallTypes.None, PythonTypeOps.ConvertToTypes(types));
        }

        private MethodInfo GetComparisonFallbackMethod(Operators op) {
            string name;
            switch (op) {
                case Operators.Equals: name = "CompareTypesEqual"; break;
                case Operators.NotEquals: name = "CompareTypesNotEqual"; break;
                case Operators.GreaterThan: name = "CompareTypesGreaterThan"; break;
                case Operators.LessThan: name = "CompareTypesLessThan"; break;
                case Operators.GreaterThanOrEqual: name = "CompareTypesGreaterThanOrEqual"; break;
                case Operators.LessThanOrEqual: name = "CompareTypesLessThanOrEqual"; break;
                case Operators.Compare: name = "CompareTypes"; break;
                default: throw new InvalidOperationException();
            }
            return typeof(PythonOps).GetMethod(name);
        }

        /// <summary>
        /// Trys to geta MethodBinder associated the slot for the specified type.
        /// 
        /// If a method is found the binder is set and true is returned.
        /// If nothing is found binder is null and true is returned.
        /// If something other than a method is found false is returned.
        /// 
        /// TODO: Remove rop
        /// </summary>
        private bool TryGetBinder(PythonType[] types, SymbolId op, SymbolId rop, out MethodBinder binder) {
            PythonType xType = types[0];
            binder = null;

            BuiltinFunction xBf;
            if (!TryGetStaticFunction(op, xType, out xBf)) {
                return false;
            }

            PythonType yType = null;
            BuiltinFunction yBf = null;
            if (types.Length > 1) {
                yType = types[1];
                if (!xType.IsSubclassOf(yType) && !TryGetStaticFunction(rop, yType, out yBf)) {
                    return false;
                }
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
                    binder = MethodBinder.MakeBinder(Binder, op.ToString(), yBf.Targets, PythonNarrowing.None, PythonNarrowing.BinaryOperator);
                }
            } else {
                if (yBf == null) {
                    binder = MethodBinder.MakeBinder(Binder, op.ToString(), xBf.Targets, PythonNarrowing.None, PythonNarrowing.BinaryOperator);
                } else {
                    List<MethodBase> targets = new List<MethodBase>();
                    targets.AddRange(xBf.Targets);
                    foreach (MethodBase mb in yBf.Targets) {
                        if (!ContainsMethodSignature(targets, mb)) targets.Add(mb);
                    }
                    binder = MethodBinder.MakeBinder(Binder, op.ToString(), targets.ToArray(), PythonNarrowing.None, PythonNarrowing.BinaryOperator);
                }
            }
            return true;
        }

        private bool TryGetStaticFunction(SymbolId op, PythonType type, out BuiltinFunction function) {
            function = null;
            if (op != SymbolId.Empty) {
                PythonTypeSlot xSlot;
                object val;
                if (type.TryResolveSlot(Context, op, out xSlot) &&
                    xSlot.TryGetValue(Context, null, type, out val)) {
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


        internal static string MakeBinaryOpErrorMessage(Operators op, string xType, string yType) {
            return string.Format("unsupported operand type(s) for {2}: '{0}' and '{1}'",
                                xType, yType, GetOperatorDisplay(op));
        }

        private static string GetOperatorDisplay(Operators op) {
            switch (op) {
                case Operators.Add: return "+";
                case Operators.Subtract: return "-";
                case Operators.Power: return "**";
                case Operators.Multiply: return "*";
                case Operators.FloorDivide: return "/";
                case Operators.Divide: return "/";
                case Operators.TrueDivide: return "//";
                case Operators.Mod: return "%";
                case Operators.LeftShift: return "<<";
                case Operators.RightShift: return ">>";
                case Operators.BitwiseAnd: return "&";
                case Operators.BitwiseOr: return "|";
                case Operators.ExclusiveOr: return "^";
                case Operators.LessThan: return "<";
                case Operators.GreaterThan: return ">";
                case Operators.LessThanOrEqual: return "<=";
                case Operators.GreaterThanOrEqual: return ">=";
                case Operators.Equals: return "==";
                case Operators.NotEquals: return "!=";
                case Operators.LessThanGreaterThan: return "<>";
                case Operators.InPlaceAdd: return "+=";
                case Operators.InPlaceSubtract: return "-=";
                case Operators.InPlacePower: return "**=";
                case Operators.InPlaceMultiply: return "*=";
                case Operators.InPlaceFloorDivide: return "/=";
                case Operators.InPlaceDivide: return "/=";
                case Operators.InPlaceTrueDivide: return "//=";
                case Operators.InPlaceMod: return "%=";
                case Operators.InPlaceLeftShift: return "<<=";
                case Operators.InPlaceRightShift: return ">>=";
                case Operators.InPlaceBitwiseAnd: return "&=";
                case Operators.InPlaceBitwiseOr: return "|=";
                case Operators.InPlaceExclusiveOr: return "^=";
                case Operators.ReverseAdd: return "+";
                case Operators.ReverseSubtract: return "-";
                case Operators.ReversePower: return "**";
                case Operators.ReverseMultiply: return "*";
                case Operators.ReverseFloorDivide: return "/";
                case Operators.ReverseDivide: return "/";
                case Operators.ReverseTrueDivide: return "//";
                case Operators.ReverseMod: return "%";
                case Operators.ReverseLeftShift: return "<<";
                case Operators.ReverseRightShift: return ">>";
                case Operators.ReverseBitwiseAnd: return "&";
                case Operators.ReverseBitwiseOr: return "|";
                case Operators.ReverseExclusiveOr: return "^";
                default: return op.ToString();
            }
        }

        internal static string MakeUnaryOpErrorMessage(string op, string xType) {
            return string.Format("unsupported operand type for {1}: '{0}'", xType, op);
        }

        private static bool FinishCompareOperation(int cmp, Operators op) {
            switch (op) {
                case Operators.LessThan: return cmp < 0;
                case Operators.LessThanOrEqual: return cmp <= 0;
                case Operators.GreaterThan: return cmp > 0;
                case Operators.GreaterThanOrEqual: return cmp >= 0;
                case Operators.Equals: return cmp == 0;
                case Operators.NotEquals: return cmp != 0;
            }
            throw new ArgumentException("op");
        }       

        #region Unary Operators

        private RuleBuilder<T> MakeUnaryRule(PythonType[] types, Operators op) {
            if (op == Operators.Not) {
                return MakeUnaryNotRule(types);
            } else if (op == Operators.Documentation) {
                return MakeDocumentationRule(types);
            }

            SlotOrFunction func = GetSlotOrFunction(types, Symbols.OperatorToSymbol(op));

            if (!func.Success) {
                // we get the error message w/ {0} so that PythonBinderHelper.TypeError formats it correctly
                return PythonBinderHelper.TypeError<T>(MakeUnaryOpErrorMessage(Action.ToString(), "{0}"), types);
            }

            RuleBuilder<T> rule = new RuleBuilder<T>();
            rule.Target = rule.MakeReturn(Binder, func.MakeCall(rule, rule.Parameters));
            PythonBinderHelper.MakeTest(rule, types);

            return rule;
        }

        private RuleBuilder<T> MakeDocumentationRule(PythonType[] types) {
            RuleBuilder<T> rule = new RuleBuilder<T>();
            rule.Target = rule.MakeReturn(Binder, Ast.Action.GetMember(Symbols.Doc, typeof(string), rule.Parameters[0]));
            PythonBinderHelper.MakeTest(rule, types);
            return rule;
        }

        private RuleBuilder<T> MakeUnaryNotRule(PythonType[] types) {
            SlotOrFunction nonzero = GetSlotOrFunction(types, Symbols.NonZero);
            SlotOrFunction length = GetSlotOrFunction(types, Symbols.Length);

            Expression notExpr;
            RuleBuilder<T> rule = new RuleBuilder<T>();
            PythonBinderHelper.MakeTest(rule, types);

            if (!nonzero.Success && !length.Success) {
                // always False or True for None
                notExpr = types[0].IsNull ? Ast.True() : Ast.False();
            } else {
                SlotOrFunction target = nonzero.Success ? nonzero : length;

                notExpr = target.MakeCall(rule, rule.Parameters);

                if (nonzero.Success) {
                    // call non-zero and negate it
                    if (notExpr.Type == typeof(bool)) {
                        notExpr = Ast.Equal(notExpr, Ast.False());
                    } else {
                        notExpr = Ast.Call(
                            typeof(PythonOps).GetMethod("Not"),
                            Ast.ConvertHelper(notExpr, typeof(object))
                        );
                    }
                } else {
                    // call len, compare w/ zero
                    if (notExpr.Type == typeof(int)) {
                        notExpr = Ast.Equal(notExpr, Ast.Zero());
                    } else {
                        notExpr = Ast.Action.Operator(Operators.Compare, typeof(int), notExpr, Ast.Zero());
                    }
                }
            }
            rule.Target = rule.MakeReturn(Binder, notExpr);
            return rule;
        }       

        #endregion

        public override string ToString() {
            return string.Format("BinaryOperatorAction({0})", Action);
        }

        private static BuiltinFunction TryConvertToBuiltinFunction(object o) {
            BuiltinMethodDescriptor md = o as BuiltinMethodDescriptor;

            if (md != null) {
                return md.Template;
            }

            BoundBuiltinFunction bbf = o as BoundBuiltinFunction;
            if (bbf != null) {
                return bbf.Target;
            }

            return o as BuiltinFunction;
        }

        private Operators Operation {
            get {
                return (Action.Operation & ~Operators.UserDefinedFlag);
            }
        }
        
        private static bool IsReverseOperator(Operators op) {
            return op >= Operators.ReverseAdd && op <= Operators.ReverseExclusiveOr;
        }
    }
}
