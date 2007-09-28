/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using System.Threading;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;

using IronPython.Compiler.Generation;
using IronPython.Compiler;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;


namespace IronPython.Runtime.Calls {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Utils;

    public class DoOperationBinderHelper<T> : BinderHelper<T, DoOperationAction> {
        public DoOperationBinderHelper(ActionBinder binder, CodeContext context, DoOperationAction action)
            : base(context, action) {
        }

        public StandardRule<T> MakeRule(object[] args) {
            return MakeNewRule(DynamicTypeOps.ObjectTypes(args));
        }

        public bool IsComparision {
            get {
                return Action.IsComparision;
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
            PythonBinderHelper.MakeTest(ret, types);
            if (Action.IsUnary) {
                MakeDynamicTarget(DynamicInvokeUnaryOperation, ret);
            } else if (Action.IsInPlace) {
                MakeDynamicTarget(DynamicInvokeInplaceOperation, ret);
            } else if (IsComparision) {
                if (Action.Operation == Operators.Compare) {
                    MakeDynamicTarget(DynamicInvokeSortCompareOperation, ret);
                } else {
                    MakeDynamicTarget(DynamicInvokeCompareOperation, ret);
                }
            } else if (Action.Operation == Operators.GetItem || Action.Operation == Operators.SetItem || Action.Operation == Operators.DeleteItem) {
                return MakeDynamicIndexRule(Action, Context, types);
            } else {
                MakeDynamicTarget(DynamicInvokeBinaryOperation, ret);
            }

            return ret;
        }

        private delegate object DynamicOperationMethod(CodeContext context, Operators op, object x, object y);
        private delegate object DynamicUnaryOperationMethod(CodeContext context, Operators op, object x);
        private void MakeDynamicTarget(DynamicOperationMethod action, StandardRule<T> rule) {
            Expression expr = Ast.Call(
                    null,
                    action.Method,
                    Ast.CodeContext(),
                    Ast.Constant(this.Action.Operation),
                    rule.Parameters[0],
                    rule.Parameters[1]);
            rule.SetTarget(rule.MakeReturn(Binder, expr));
        }
        private void MakeDynamicTarget(DynamicUnaryOperationMethod action, StandardRule<T> rule) {
            Expression expr = Ast.Call(null, action.Method,
                    Ast.CodeContext(),
                    Ast.Constant(Action.Operation),
                    rule.Parameters[0]);
            rule.SetTarget(rule.MakeReturn(Binder, expr));
        }


        private StandardRule<T> MakeRuleForNoMatch(DynamicType[] types) {
            if (IsComparision) {
                return MakeDynamicMatchRule(types);
            } else {
                // we get the error message w/ {0}, {1} so that TypeError formats it correctly
                return PythonBinderHelper.TypeError<T>(
                       MakeBinaryOpErrorMessage(Action.ToString(), "{0}", "{1}"),
                       types);
            }
        }

        protected StandardRule<T> MakeNewRule(DynamicType[] types) {
            for (int i = 0; i < types.Length; i++) {
                if (types[i].Version == DynamicType.DynamicVersion) {
                    return MakeDynamicMatchRule(types);
                }
            }

            Operators op = Action.Operation;
            if (Action.IsInPlace) {
                DynamicType xType = types[0];
                DynamicTypeSlot xSlot;
                if (xType.TryLookupSlot(Context, Symbols.OperatorToSymbol(op), out xSlot)) {
                    // TODO optimize calls to explicit inplace methods
                    return MakeDynamicMatchRule(types);
                }
                op = Action.DirectOperation;
            }

            if (Action.IsUnary) {
                Debug.Assert(types.Length == 1);
                return MakeUnaryRule(types, op);
            }

            if (types[0] == TypeCache.Object && types[1] == TypeCache.Object) {
                return MakeDynamicMatchRule(types);
            }

            if (IsComparision) {
                return MakeComparisonRule(types, op);
            }

            if (Action.Operation == Operators.GetItem || Action.Operation == Operators.SetItem ||
                Action.Operation == Operators.GetSlice || Action.Operation == Operators.SetSlice) {
                // Indexers need to see if the index argument is an expandable tuple.  This will
                // be captured in the AbstractValue in the future but today is captured in the
                // real value.
                return MakeIndexerRule(types);
            } else if (Action.Operation == Operators.DeleteItem || Action.Operation == Operators.DeleteSlice) {
                return MakeDynamicIndexRule(Action, Context, types);
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
            if (op == Operators.Compare) {
                return MakeSortComparisonRule(types, op);
            }

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
            PythonBinderHelper.MakeTest(rule, types);

            List<Statement> stmts = new List<Statement>();
            if (MakeOneTarget(fTarget, rule, stmts, false, null)) {
                if (MakeOneTarget(rTarget, rule, stmts, true, null)) {
                    if (MakeOneCompare(fCmpTarget, rule, stmts, false, types)) {
                        if (MakeOneCompare(rCmpTarget, rule, stmts, true, types)) {
                            stmts.Add(MakeFallbackCompare(rule));
                        }
                    }
                }
            }

            rule.SetTarget(Ast.Block(stmts));
            return rule;
        }

        /// <summary>
        /// Makes the comparison rule which returns an int (-1, 0, 1).  TODO: Better name?
        /// </summary>
        private StandardRule<T> MakeSortComparisonRule(DynamicType[] types, Operators op) {
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
            DynamicType[] rTypes = new DynamicType[] { types[1], types[0] };
            MethodBinder cbind, rcbind, eqbind, reqbind, ltbind, gtbind, rltbind, rgtbind;

            if (!TryGetBinder(types, Symbols.Cmp, SymbolId.Empty, out cbind)) return MakeDynamicMatchRule(types);
            if (!TryGetBinder(rTypes, Symbols.Cmp, SymbolId.Empty, out rcbind)) return MakeDynamicMatchRule(types);
            if (!TryGetBinder(types, Symbols.OperatorEquals, SymbolId.Empty, out eqbind)) return MakeDynamicMatchRule(types);
            if (!TryGetBinder(rTypes, Symbols.OperatorEquals, SymbolId.Empty, out reqbind)) return MakeDynamicMatchRule(types);
            if (!TryGetBinder(types, Symbols.OperatorLessThan, SymbolId.Empty, out ltbind)) return MakeDynamicMatchRule(types);
            if (!TryGetBinder(rTypes, Symbols.OperatorGreaterThan, SymbolId.Empty, out rgtbind)) return MakeDynamicMatchRule(types);
            if (!TryGetBinder(types, Symbols.OperatorGreaterThan, SymbolId.Empty, out gtbind)) return MakeDynamicMatchRule(types);
            if (!TryGetBinder(rTypes, Symbols.OperatorLessThan, SymbolId.Empty, out rltbind)) return MakeDynamicMatchRule(types);

            // then resolve any overloads down to candidates based upon our argument types...
            MethodCandidate fcand = ComparisonTargetFromBinder(cbind, types);
            MethodCandidate rcand = ComparisonTargetFromBinder(rcbind, rTypes);
            MethodCandidate eqcand = ComparisonTargetFromBinder(eqbind, types);
            MethodCandidate reqcand = ComparisonTargetFromBinder(reqbind, rTypes);
            MethodCandidate ltcand = ComparisonTargetFromBinder(ltbind, types);
            MethodCandidate rgtcand = ComparisonTargetFromBinder(rgtbind, rTypes);
            MethodCandidate gtcand = ComparisonTargetFromBinder(gtbind, types);
            MethodCandidate rltcand = ComparisonTargetFromBinder(rltbind, rTypes);

            // inspect forward and reverse versions so we can pick one or both.
            MethodTarget cTarget, rcTarget, eqTarget, reqTarget, ltTarget, rgtTarget, gtTarget, rltTarget;
            GetCombinedTargets(fcand, rcand, out cTarget, out rcTarget);
            GetCombinedTargets(eqcand, reqcand, out eqTarget, out reqTarget);
            GetCombinedTargets(ltcand, rgtcand, out ltTarget, out rgtTarget);
            GetCombinedTargets(gtcand, rltcand, out gtTarget, out rltTarget);

            DynamicType xType = types[0];
            DynamicType yType = types[1];

            // now build the rule from the targets.
            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, types);

            // bail if we're comparing to null and the rhs can't do anything special...
            if (xType.IsNull) {
                if (yType.IsNull) {
                    rule.SetTarget(rule.MakeReturn(Binder, Ast.Zero()));
                    return rule;
                } else if (yType.UnderlyingSystemType.IsPrimitive || yType.UnderlyingSystemType == typeof(Microsoft.Scripting.Math.BigInteger)) {
                    rule.SetTarget(rule.MakeReturn(Binder, Ast.Constant(-1)));
                    return rule;
                }
            } 

            List<Statement> stmts = new List<Statement>();

            bool tryRich = true, more = true;
            if (xType == yType && cTarget != null) {
                // if the types are equal try __cmp__ first
                more = more &&
                    MakeOneComparisonTarget(cTarget, rule, stmts, false);

                if (xType != TypeCache.OldInstance) {
                    // try __cmp__ backwards for new-style classes and don't fallback to
                    // rich comparisons if available
                    more = more && MakeOneComparisonTarget(rcTarget, rule, stmts, true);
                    tryRich = false;
                }
            }

            if (tryRich) {
                // try eq
                more = more &&
                    MakeOneComparisonTarget(eqTarget, rule, stmts, false, 0) &&
                    MakeOneComparisonTarget(reqTarget, rule, stmts, true, 0);

                // try less than & reverse
                more = more &&
                    MakeOneComparisonTarget(ltTarget, rule, stmts, false, -1) &&
                    MakeOneComparisonTarget(rgtTarget, rule, stmts, true, -1);

                // try greater than & reverse
                more = more &&
                    MakeOneComparisonTarget(gtTarget, rule, stmts, false, 1) &&
                    MakeOneComparisonTarget(rltTarget, rule, stmts, true, 1);
            }

            if (xType != yType) {
                more = more &&
                    MakeOneComparisonTarget(cTarget, rule, stmts, false) &&
                    MakeOneComparisonTarget(rcTarget, rule, stmts, true);
            }

            if (more) {
                // fall back to compare types
                stmts.Add(MakeFallbackCompare(rule));
            }

            rule.SetTarget(Ast.Block(stmts));
            return rule;
        }

        private StandardRule<T> MakeRuleForBinaryOperator(DynamicType[] types, Operators oper, MethodCandidate fCand, MethodCandidate rCand) {
            MethodTarget fTarget, rTarget;

            if (!GetCombinedTargets(fCand, rCand, out fTarget, out rTarget)) {
                return MakeRuleForNoMatch(types);
            }

            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, types);

            List<Statement> stmts = new List<Statement>();
            if (MakeOneTarget(fTarget, rule, stmts, false, types)) {
                if (MakeOneTarget(rTarget, rule, stmts, false, types)) {
                    stmts.Add(MakeBinaryThrow(rule));
                }
            }
            rule.SetTarget(Ast.Block(stmts));
            return rule;
        }

        private StandardRule<T> MakeIndexerRule(DynamicType[] types) {
            Debug.Assert(types.Length >= 1);

            DynamicType indexedType = types[0];
            SymbolId getOrSet = (Action.Operation == Operators.GetItem || Action.Operation == Operators.GetSlice) ? Symbols.GetItem : Symbols.SetItem;
            SymbolId altAction = (Action.Operation == Operators.GetItem || Action.Operation == Operators.GetSlice) ? Symbols.GetSlice : Symbols.SetSlice;
            BuiltinFunction bf;
            for (int i = 0; i < types.Length; i++) {
                if (!types[i].UnderlyingSystemType.IsPublic && !types[i].UnderlyingSystemType.IsNestedPublic) {
                    return MakeDynamicIndexRule(Action, Context, types);
                }
            }

            if (!HasBadSlice(indexedType, altAction) &&
                TryGetStaticFunction(getOrSet, indexedType, out bf) && 
                bf != null) {

                MethodBinder binder = MethodBinder.MakeBinder(Binder,
                    "index",
                    bf.Targets,
                    BinderType.Normal);

                StandardRule<T> ret = new StandardRule<T>();
                PythonBinderHelper.MakeTest(ret, types);

                Type[] callTypes = GetIndexerCallTypes(types);

                MethodCandidate cand = binder.MakeBindingTarget(CallType.ImplicitInstance, callTypes);
                if (cand != null) {
                    Expression call;

                    if (Action.Operation == Operators.GetItem || Action.Operation == Operators.GetSlice) {
                        call = cand.Target.MakeExpression(Binder,
                            ret,
                            GetIndexArguments(ret),
                            callTypes);
                    } else {
                        call = Ast.Comma(0,
                            ret.Parameters[ret.Parameters.Length - 1],
                            cand.Target.MakeExpression(
                                Binder,
                                ret,
                                GetIndexArguments(ret),
                                callTypes)
                        );
                    }
                    ret.SetTarget(ret.MakeReturn(Binder, call));
                    return ret;
                } 
            }

            return MakeDynamicIndexRule(Action, Context, types);
        }

        private Type[] GetIndexerCallTypes(DynamicType[] types) {
            Type[] callTypes;
            if (Action.Operation == Operators.GetItem || Action.Operation == Operators.SetItem) {
                callTypes = DynamicTypeOps.ConvertToTypes(types);
            } else if (Action.Operation == Operators.GetSlice) {
                callTypes = new Type[] { types[0].UnderlyingSystemType, typeof(Slice) };
            } else {
                Debug.Assert(Action.Operation == Operators.SetSlice);
                callTypes = new Type[] { 
                        types[0].UnderlyingSystemType, 
                        typeof(Slice),
                        types[types.Length-1].UnderlyingSystemType
                    };
            }
            return callTypes;
        }

        private Expression[] GetIndexArguments(StandardRule<T> ret) {
            Expression[] args = ret.Parameters;
            if (Action.Operation == Operators.GetSlice) {
                Expression slice = GetGetOrDeleteSlice(ret);

                args = new Expression[] { ret.Parameters[0], slice };
            } else if (Action.Operation == Operators.SetSlice) {
                // construct a slice object from the arguments
                Expression slice = GetSetSlice(ret);

                args = new Expression[] { ret.Parameters[0], slice, ret.Parameters[ret.Parameters.Length - 1] };
            }
            return args;
        }

        private static Expression GetSetSlice(StandardRule<T> ret) {
            return Ast.Call(null,
                typeof(PythonOps).GetMethod("MakeSlice"),
                GetSetParameter(ret, 1),
                GetSetParameter(ret, 2),
                GetSetParameter(ret, 3)
            );
        }

        private static Expression GetGetOrDeleteSlice(StandardRule<T> ret) {
            return Ast.Call(null,
                typeof(PythonOps).GetMethod("MakeSlice"),
                GetGetOrDeleteParameter(ret, 1),
                GetGetOrDeleteParameter(ret, 2),
                GetGetOrDeleteParameter(ret, 3)
            );
        }

        private static Expression GetGetOrDeleteParameter(StandardRule<T> ret, int index) {
            if (ret.Parameters.Length > index) {
                return OldInstance.CheckMissing(ret.Parameters[index]);
            }
            return Ast.Null();
        }

        private static Expression GetSetParameter(StandardRule<T> ret, int index) {
            if (ret.Parameters.Length > (index + 1)) {
                return OldInstance.CheckMissing(ret.Parameters[index]);
            }

            return Ast.Null(); 
        }

        /// <summary>
        /// Checks for __getslice__/__setslice__ which prevents an optimized index operation.
        /// 
        /// getslice/setslice are legacy operations but the user can still override them on a subclass
        /// of a built-in type.  We just don't the optimized call on these types right now.
        /// </summary>
        private bool HasBadSlice(DynamicType indexedType, SymbolId altAction) {
            BuiltinFunction bf;
            return !TryGetStaticFunction(altAction, indexedType, out bf) ||
                        (bf != null && 
                        DynamicHelpers.GetDynamicTypeFromType(bf.DeclaringType) != indexedType);
        }        

        internal static StandardRule<T> MakeDynamicIndexRule(DoOperationAction action, CodeContext context, DynamicType[] types) {
            StandardRule<T> ret = new StandardRule<T>();
            PythonBinderHelper.MakeTest(ret, types);
            Expression retExpr;
            if (action.Operation == Operators.GetItem || 
                action.Operation == Operators.DeleteItem ||
                action.Operation == Operators.GetSlice ||
                action.Operation == Operators.DeleteSlice) {
                Expression arg;
                if (action.Operation == Operators.GetSlice || action.Operation == Operators.DeleteSlice) {
                    arg = GetGetOrDeleteSlice(ret);
                } else if (types.Length == 2) {
                    arg = ret.Parameters[1];
                } else {
                    arg = Ast.Call(null, typeof(PythonOps).GetMethod("MakeExpandableTuple"), GetGetIndexParameters(ret));
                }
                
                string call = (action.Operation == Operators.GetItem || action.Operation == Operators.GetSlice) ? "GetIndex" : "DelIndex";
                retExpr = Ast.Call(null, typeof(PythonOps).GetMethod(call), ret.Parameters[0], arg);
            } else {
                Expression arg;
                if (action.Operation == Operators.SetSlice) {
                    arg = GetSetSlice(ret);
                } else if (types.Length == 3) {
                    arg = ret.Parameters[1];
                } else {
                    arg = Ast.Call(null, typeof(PythonOps).GetMethod("MakeExpandableTuple"), GetSetIndexParameters(ret));
                }
                retExpr = Ast.Call(null, typeof(PythonOps).GetMethod("SetIndex"), ret.Parameters[0], arg, ret.Parameters[ret.Parameters.Length - 1]);
            }
            ret.SetTarget(ret.MakeReturn(context.LanguageContext.Binder, retExpr));
            return ret;
        }

        private static Expression[] GetSetIndexParameters(StandardRule<T> ret) {
            return ArrayUtils.RemoveLast(ArrayUtils.RemoveFirst(ret.Parameters));
        }

        private static Expression[] GetGetIndexParameters(StandardRule<T> ret) {
            return ArrayUtils.RemoveFirst(ret.Parameters);
        }

        private Expression MakeCall(MethodTarget target, StandardRule<T> block, bool reverse) {
            return MakeCall(target, block, reverse, null);
        }

        private Expression MakeCall(MethodTarget target, StandardRule<T> block, bool reverse, DynamicType[] types) {
            Expression[] vars = block.Parameters;
            if (reverse) {
                Expression[] newVars = new Expression[2];
                newVars[0] = vars[1];
                newVars[1] = vars[0];
                vars = newVars;
            }
            
            Type[] clrtypes = null;
            if (types != null) {
                clrtypes = DynamicTypeOps.ConvertToTypes(types);
                if (reverse) {
                    Type temp = clrtypes[0];
                    clrtypes[0] = clrtypes[1];
                    clrtypes[1] = temp;
                }
            }
            return target.MakeExpression(Binder, block, vars, clrtypes);
        }

        private bool MakeOneComparisonTarget(MethodTarget target, StandardRule<T> rule, List<Statement> stmts, bool reverse) {
            return MakeOneComparisonTarget(target, rule, stmts, reverse, null);
        }

        private bool MakeOneComparisonTarget(MethodTarget target, StandardRule<T> rule, List<Statement> stmts, bool reverse, int? val) {
            if (target == null) return true;

            if (ReturnsNotImplemented(target)) {
                Variable tmp = rule.GetTemporary(target.ReturnType, "tmp");                
                Statement body;
                if(val != null){
                    body = MakeValueCheck(rule, val, tmp);
                }else{
                    body = reverse ?                        
                        rule.MakeReturn(Binder, 
                            Ast.Multiply(
                                Ast.Convert(
                                    Ast.ReadDefined(tmp), 
                                    typeof(int)), 
                                Ast.Constant(-1))) :
                        rule.MakeReturn(Binder, Ast.ReadDefined(tmp));
                }
                stmts.Add(Ast.IfThen(
                    Ast.NotEqual(
                        Ast.Assign(tmp, MakeCall(target, rule, reverse)),
                        Ast.ReadField(null, typeof(PythonOps).GetField("NotImplemented"))),
                    body));
            } else {
                Expression call = MakeCall(target, rule, reverse);
                if (val == null) {                    
                    if (reverse) {
                        call = Ast.Multiply(call, Ast.Constant(-1));
                    }
                    stmts.Add(rule.MakeReturn(Binder, call));
                    return false;
                }

                Debug.Assert(call.Type == typeof(bool));
                Variable var = rule.GetTemporary(call.Type, "tmp");
                stmts.Add(Ast.Statement(Ast.Assign(var, call)));
                stmts.Add(MakeValueCheck(rule, val, var));
            }
            return true;
        }

        private IfStatement MakeValueCheck(StandardRule<T> rule, int? val, Variable var) {
            return Ast.IfThen(
                Ast.Equal(
                    Ast.ReadDefined(var),
                    Ast.True()),

                rule.MakeReturn(Binder, Ast.Constant(val))
            );
        }

        private bool MakeOneTarget(MethodTarget target, StandardRule<T> block, List<Statement> stmts, bool reverse, DynamicType [] types) {
            if (target == null) return true;

            if (ReturnsNotImplemented(target)) {
                Variable tmp = block.GetTemporary(target.ReturnType, "tmp");
                stmts.Add(Ast.IfThen(
                    Ast.NotEqual(
                        Ast.Assign(tmp, MakeCall(target, block, reverse, types)),
                        Ast.ReadField(null, typeof(PythonOps).GetField("NotImplemented"))),
                    block.MakeReturn(Binder, Ast.ReadDefined(tmp))));
                return true;
            } else {
                stmts.Add(block.MakeReturn(Binder, MakeCall(target, block, reverse, types)));
                return false;
            }
        }

        private static bool ReturnsNotImplemented(MethodTarget target) {
            if (target.ReturnType == typeof(object)) {
                MethodInfo mi = target.Method as MethodInfo;
                if (mi != null) {
                    return mi.ReturnTypeCustomAttributes.IsDefined(typeof(MaybeNotImplementedAttribute), false);
                }
            }

            return false;
        }

        private Statement MakeUnaryThrow(StandardRule<T> block) {
            return Ast.Statement(Ast.Throw(
                Ast.Call(null, typeof(PythonOps).GetMethod("TypeErrorForUnaryOp"),
                    Ast.Constant(SymbolTable.IdToString(Symbols.OperatorToSymbol(Action.Operation))),
                    block.Parameters[0])));
        }

        private Statement MakeBinaryThrow(StandardRule<T> block) {
            return Ast.Statement(Ast.Throw(
                Ast.Call(null, typeof(PythonOps).GetMethod("TypeErrorForBinaryOp"),
                    Ast.Constant(SymbolTable.IdToString(Symbols.OperatorToSymbol(Action.Operation))),
                    block.Parameters[0], block.Parameters[1])));
        }

        private bool MakeOneCompare(MethodTarget target, StandardRule<T> block, List<Statement> stmts, bool reverse, DynamicType[] types) {
            if (target == null) return true;

            if (ReturnsNotImplemented(target)) {
                Variable tmp = block.GetTemporary(target.ReturnType, "tmp");
                stmts.Add(Ast.IfThen(
                    Ast.NotEqual(
                        Ast.Assign(tmp, MakeCall(target, block, reverse, types)),
                        Ast.ReadField(null, typeof(PythonOps).GetField("NotImplemented"))),
                    MakeCompareTest(Ast.ReadDefined(tmp), block, reverse)));
                return true;
            } else {
                stmts.Add(MakeCompareTest(MakeCall(target, block, reverse, types), block, reverse));
                return false;
            }
        }

        private Statement MakeCompareTest(Expression expr, StandardRule<T> block, bool reverse) {
            return block.MakeReturn(Binder, 
                Ast.Call(null, GetCompareMethod(reverse),
                    Ast.Call(null, typeof(PythonOps).GetMethod("CompareToZero"),
                        expr)));
        }

        private MethodInfo GetCompareMethod(bool reverse) {
            string name;

            switch (reverse ? CompilerHelpers.OperatorToReverseOperator(Action.Operation) : Action.Operation) {
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

        private Statement MakeFallbackCompare(StandardRule<T> block) {
            return block.MakeReturn(Binder, 
                Ast.Call(null, GetComparisonFallbackMethod(Action.Operation),
                    block.Parameters[0],
                    block.Parameters[1]));
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
            return binder.MakeBindingTarget(CallType.None, DynamicTypeOps.ConvertToTypes(types));
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
                    binder = MethodBinder.MakeBinder(Binder, op.ToString(),
                        yBf.Targets, MethodBinderType);
                }
            } else {
                if (yBf == null) {
                    binder = MethodBinder.MakeBinder(Binder, op.ToString(),
                        xBf.Targets, MethodBinderType);
                } else {
                    List<MethodBase> targets = new List<MethodBase>();
                    targets.AddRange(xBf.Targets);
                    foreach (MethodBase mb in yBf.Targets) {
                        if (!ContainsMethodSignature(targets, mb)) targets.Add(mb);
                    }
                    binder = MethodBinder.MakeBinder(Binder, op.ToString(),
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

        public static object DynamicInvokeBinaryOperation(CodeContext context, Operators op, object x, object y) {
            DynamicType xDType = DynamicHelpers.GetDynamicType(x);
            DynamicType yDType = DynamicHelpers.GetDynamicType(y);
            object ret;

            if (xDType == yDType || !yDType.IsSubclassOf(xDType)) {
                if (xDType.TryInvokeBinaryOperator(context, op, x, y, out ret) &&
                    ret != PythonOps.NotImplemented) {
                    return ret;
                }
            }

            if (xDType != yDType) {
                if (yDType.TryInvokeBinaryOperator(context, CompilerHelpers.OperatorToReverseOperator(op), y, x, out ret) &&
                    ret != PythonOps.NotImplemented) {
                    return ret;
                }
            }

            throw PythonOps.TypeError(MakeBinaryOpErrorMessage(op.ToString(), xDType.Name, yDType.Name));
        }

        internal static string MakeBinaryOpErrorMessage(string op, string xType, string yType) {
            return string.Format("unsupported operand type(s) for {2}: '{0}' and '{1}'",
                                xType, yType, op);
        }

        internal static string MakeUnaryOpErrorMessage(string op, string xType) {
            return string.Format("unsupported operand type for {1}: '{0}'",
                                xType, op);
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

        /// <summary>
        ///  Implements the -1, 0, -1 return value style sort dynamically. TODO: Better name?
        /// </summary>
        public static object DynamicInvokeSortCompareOperation(CodeContext context, Operators op, object x, object y) {
            object ret = PythonOps.NotImplemented;

            DynamicType xType = DynamicHelpers.GetDynamicType(x);
            DynamicType yType = DynamicHelpers.GetDynamicType(y);

            bool tryRich = true;
            if (xType == yType) {
                // use __cmp__ first if it's defined
                if (DynamicHelpers.GetDynamicType(x).TryInvokeBinaryOperator(context, Operators.Compare, x, y, out ret)) {
                    if (ret != PythonOps.NotImplemented) {
                        return ret;
                    }

                    if (xType != TypeCache.OldInstance) {
                        // try __cmp__ backwards for new-style classes and don't fallback to
                        // rich comparisons if available
                        ret = PythonOps.InternalCompare(context, Operators.Compare, y, x);
                        if (ret != PythonOps.NotImplemented) return -1 * Converter.ConvertToInt32(ret);
                        tryRich = false;
                    }
                }
            }

            // next try equals, return 0 if we match.
            if (tryRich) {
                ret = PythonOps.RichEqualsHelper(x, y);
                if (ret != PythonOps.NotImplemented) {
                    if (PythonOps.IsTrue(ret)) return 0;
                } else if (y != null) {
                    // try the reverse
                    ret = PythonOps.RichEqualsHelper(y, x);
                    if (ret != PythonOps.NotImplemented && PythonOps.IsTrue(ret)) return 0;
                }

                // next try less than
                ret = PythonOps.LessThanHelper(context, x, y);
                if (ret != PythonOps.NotImplemented) {
                    if (PythonOps.IsTrue(ret)) return -1;
                } else if (y != null) {
                    // try the reverse
                    ret = PythonOps.GreaterThanHelper(context, y, x);
                    if (ret != PythonOps.NotImplemented && PythonOps.IsTrue(ret)) return -1;
                }

                // finally try greater than
                ret = PythonOps.GreaterThanHelper(context, x, y);
                if (ret != PythonOps.NotImplemented) {
                    if (PythonOps.IsTrue(ret)) return 1;
                } else if (y != null) {
                    //and the reverse
                    ret = PythonOps.LessThanHelper(context, y, x);
                    if (ret != PythonOps.NotImplemented && PythonOps.IsTrue(ret)) return 1;
                }

                if (xType != yType) {
                    // finally try __cmp__ if our types are different
                    ret = PythonOps.InternalCompare(context, Operators.Compare, x, y);
                    if (ret != PythonOps.NotImplemented) return PythonOps.CompareToZero(ret);

                    ret = PythonOps.InternalCompare(context, Operators.Compare, y, x);
                    if (ret != PythonOps.NotImplemented) return -1 * PythonOps.CompareToZero(ret);
                }
            }

            return PythonOps.CompareTypes(x, y);
        }

        public static object DynamicInvokeCompareOperation(CodeContext context, Operators op, object x, object y) {
            object ret;

            DynamicType dt1 = DynamicHelpers.GetDynamicType(x);
            DynamicType dt2 = DynamicHelpers.GetDynamicType(y);

            if (dt1.TryInvokeBinaryOperator(context, op, x, y, out ret) &&
                ret != PythonOps.NotImplemented) {
                return ret;
            }
            if (dt2.TryInvokeBinaryOperator(context,
                CompilerHelpers.OperatorToReverseOperator(op), y, x, out ret) &&
                ret != PythonOps.NotImplemented) {
                return ret;
            }

            if (dt1.TryInvokeBinaryOperator(context, Operators.Compare, x, y, out ret) && ret != PythonOps.NotImplemented)
                return RuntimeHelpers.BooleanToObject(FinishCompareOperation(PythonOps.CompareToZero(ret), op));

            if (dt2.TryInvokeBinaryOperator(context, Operators.Compare, y, x, out ret) && ret != PythonOps.NotImplemented)
                return RuntimeHelpers.BooleanToObject(FinishCompareOperation(-1 * PythonOps.CompareToZero(ret), op));

            if (dt1 == dt2) {
                return RuntimeHelpers.BooleanToObject(FinishCompareOperation((int)(IdDispenser.GetId(x) - IdDispenser.GetId(y)), op));
            } else {
                if (dt1 == TypeCache.None) return FinishCompareOperation(-1, op);
                if (dt2 == TypeCache.None) return FinishCompareOperation(+1, op);
                return RuntimeHelpers.BooleanToObject(FinishCompareOperation(string.CompareOrdinal(dt1.Name, dt2.Name), op));
            }
        }

        public static object DynamicInvokeInplaceOperation(CodeContext context, Operators op, object x, object y) {
            object ret;
            DynamicType dt = DynamicHelpers.GetDynamicType(x);
            if (dt.TryInvokeBinaryOperator(context, op, x, y, out ret) && ret != PythonOps.NotImplemented)
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
                // we get the error message w/ {0} so that PythonBinderHelper.TypeError formats it correctly
                return PythonBinderHelper.TypeError<T>(MakeUnaryOpErrorMessage(Action.ToString(), "{0}"), types);
            }

            MethodBinder binder = MethodBinder.MakeBinder(Binder,
                op.ToString(),
                func.Targets,
                MethodBinderType);

            Debug.Assert(binder != null);

            MethodCandidate cand = binder.MakeBindingTarget(CallType.None, DynamicTypeOps.ConvertToTypes(types));
            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, types);
            rule.SetTarget(rule.MakeReturn(Binder, MakeCall(cand.Target, rule, false)));
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
            PythonBinderHelper.MakeTest(rule, types);

            if (nonzero == null && len == null) {
                // always False or True for None
                notExpr = types[0].IsNull ? Ast.True() : Ast.False();
            } else {
                MethodBinder binder = MethodBinder.MakeBinder(Binder,
                    "Not",
                    nonzero != null ? nonzero.Targets : len.Targets,
                    MethodBinderType);

                Debug.Assert(binder != null);
                MethodCandidate cand = binder.MakeBindingTarget(CallType.None, DynamicTypeOps.ConvertToTypes(types));
                notExpr = MakeCall(cand.Target, rule, false);

                if (nonzero != null) {
                    // call non-zero and negate it
                    if (notExpr.Type == typeof(bool)) {
                        notExpr = Ast.Equal(notExpr, Ast.False());
                    } else {
                        notExpr = Ast.Call(null, typeof(PythonOps).GetMethod("Not"), notExpr);
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
            rule.SetTarget(rule.MakeReturn(Binder, notExpr));
            return rule;
        }

        private StandardRule<T> MakeDynamicNotRule(DynamicType[] types) {
            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, types);
            rule.SetTarget(
                rule.MakeReturn(Binder,
                    Ast.Call(null,
                        typeof(PythonOps).GetMethod("Not"),
                        rule.Parameters[0])));
            return rule;
        }

        public static object DynamicInvokeUnaryOperation(CodeContext context, Operators op, object x) {
            object ret;
            DynamicType dt = DynamicHelpers.GetDynamicType(x);
            if (dt.TryInvokeUnaryOperator(context, op, x, out ret) && ret != PythonOps.NotImplemented)
                return ret;

            throw PythonOps.TypeError(MakeUnaryOpErrorMessage(op.ToString(), DynamicHelpers.GetDynamicType(x).Name));
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
    }
}
