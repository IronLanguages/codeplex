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
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;

using IronPython.Compiler;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Calls {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class PythonDoOperationBinderHelper<T> : BinderHelper<T, DoOperationAction> {
        private object[] _args;
        public PythonDoOperationBinderHelper(CodeContext context, DoOperationAction action)
            : base(context, action) {
        }

        public StandardRule<T> MakeRule(object[] args) {
            _args = args;
            return MakeNewRule(PythonTypeOps.ObjectTypes(args));
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

        private StandardRule<T> MakeDynamicMatchRule(PythonType[] types) {
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
            } else {
                MakeDynamicTarget(DynamicInvokeBinaryOperation, ret);
            }

            return ret;
        }

        private delegate object DynamicOperationMethod(CodeContext context, Operators op, object x, object y);
        private delegate object DynamicUnaryOperationMethod(CodeContext context, Operators op, object x);
        private void MakeDynamicTarget(DynamicOperationMethod action, StandardRule<T> rule) {
            Expression expr =
                Ast.SimpleCallHelper(
                    null,
                    action.Method,
                    Ast.CodeContext(),
                    Ast.Constant(this.Action.Operation),
                    rule.Parameters[0],
                    rule.Parameters[1]
                );
            rule.Target = rule.MakeReturn(Binder, expr);
        }

        private void MakeDynamicTarget(DynamicUnaryOperationMethod action, StandardRule<T> rule) {
            Expression expr =
                Ast.SimpleCallHelper(
                    null,
                    action.Method,
                    Ast.CodeContext(),
                    Ast.Constant(Action.Operation),
                    rule.Parameters[0]
                );
            rule.Target = rule.MakeReturn(Binder, expr);
        }


        private StandardRule<T> MakeRuleForNoMatch(PythonType[] types) {
            if (IsComparision) {
                return MakeDynamicMatchRule(types);
            } else {
                // we get the error message w/ {0}, {1} so that TypeError formats it correctly
                return PythonBinderHelper.TypeError<T>(
                       MakeBinaryOpErrorMessage(Action.Operation, "{0}", "{1}"),
                       types);
            }
        }

        protected StandardRule<T> MakeNewRule(PythonType[] types) {
            if (Action.Operation == Operators.IsCallable) {
                // This will break in cross-language cases. Eg, if this rule applies to x,
                // then Python's callable(x) will invoke this rule, but Ruby's callable(x) 
                // will use the Ruby language binder instead and miss this rule, and thus 
                // may get a different result than python.
                return PythonBinderHelper.MakeIsCallableRule<T>(this.Context, _args[0]);
            }

            if (Action.Operation == Operators.GetItem || Action.Operation == Operators.SetItem ||
                Action.Operation == Operators.GetSlice || Action.Operation == Operators.SetSlice ||
                Action.Operation == Operators.DeleteItem || Action.Operation == Operators.DeleteSlice) {
                // Indexers need to see if the index argument is an expandable tuple.  This will
                // be captured in the AbstractValue in the future but today is captured in the
                // real value.
                return MakeIndexerRule(types);
            } 

            for (int i = 0; i < types.Length; i++) {
                if (types[i].Version == PythonType.DynamicVersion) {
                    return MakeDynamicMatchRule(types);
                }
            }

            Operators op = Action.Operation;
            if (op == Operators.MemberNames) {
                return MakeMemberNamesRule(types);
            } else if (op == Operators.CallSignatures) {
                return MakeCallSignatureRule(Binder, CompilerHelpers.GetMethodTargets(_args[0]), types);
            }

            if (Action.IsInPlace) {
                PythonType xType = types[0];
                PythonTypeSlot xSlot;
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

            

            return MakeSimpleRule(types, op);
        }

        internal static StandardRule<T> MakeCallSignatureRule(ActionBinder binder, IList<MethodBase> targets, params PythonType[] types) {
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
            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, types);
            rule.Target = rule.MakeReturn(binder, Ast.RuntimeConstant(arrres.ToArray()));
            return rule;
        }

        private StandardRule<T> MakeMemberNamesRule(PythonType[] types) {
            if (typeof(IMembersList).IsAssignableFrom(types[0].UnderlyingSystemType)) {
                return null;
            }

            IList<SymbolId> names = types[0].GetMemberNames(Context);
            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, types);

            rule.Target = rule.MakeReturn(
                Binder,
                Ast.RuntimeConstant(SymbolTable.IdsToStrings(names))
            );
            return rule;
        }

        private StandardRule<T> MakeSimpleRule(PythonType[] types, Operators oper) {
            SymbolId op = Symbols.OperatorToSymbol(oper);
            SymbolId rop = Symbols.OperatorToReversedSymbol(oper);

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

        private StandardRule<T> MakeComparisonRule(PythonType[] types, Operators op) {
            if (op == Operators.Compare) {
                return MakeSortComparisonRule(types, op);
            }

            PythonType xType = types[0];
            PythonType yType = types[1];
            SymbolId opSym = Symbols.OperatorToSymbol(op);
            SymbolId ropSym = Symbols.OperatorToReversedSymbol(op);

            MethodBinder fbind, rbind, cbind, rcbind;
            // forward
            if (!TryGetBinder(types, opSym, SymbolId.Empty, out fbind)) {
                return MakeDynamicMatchRule(types);
            }

            // reverse
            PythonType[] rTypes = new PythonType[] { types[1], types[0] };
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
            BindingTarget forward = ComparisonTargetFromBinder(fbind, types);
            BindingTarget reverse = ComparisonTargetFromBinder(rbind, rTypes);
            BindingTarget fcmp = ComparisonTargetFromBinder(cbind, types);
            BindingTarget rcmp = ComparisonTargetFromBinder(rcbind, rTypes);

            BindingTarget fTarget, rTarget, fCmpTarget, rCmpTarget;
            GetCombinedTargets(forward, reverse, out fTarget, out rTarget);
            GetCombinedTargets(fcmp, rcmp, out fCmpTarget, out rCmpTarget);

            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, types);

            List<Expression> stmts = new List<Expression>();
            if (MakeOneTarget(fTarget, rule, stmts, false, types)) {
                if (MakeOneTarget(rTarget, rule, stmts, true, types)) {
                    if (MakeOneCompare(fCmpTarget, rule, stmts, false, types)) {
                        if (MakeOneCompare(rCmpTarget, rule, stmts, true, types)) {
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
        private StandardRule<T> MakeSortComparisonRule(PythonType[] types, Operators op) {
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
            BindingTarget fcand = ComparisonTargetFromBinder(cbind, types);
            BindingTarget rcand = ComparisonTargetFromBinder(rcbind, rTypes);
            BindingTarget eqcand = ComparisonTargetFromBinder(eqbind, types);
            BindingTarget reqcand = ComparisonTargetFromBinder(reqbind, rTypes);
            BindingTarget ltcand = ComparisonTargetFromBinder(ltbind, types);
            BindingTarget rgtcand = ComparisonTargetFromBinder(rgtbind, rTypes);
            BindingTarget gtcand = ComparisonTargetFromBinder(gtbind, types);
            BindingTarget rltcand = ComparisonTargetFromBinder(rltbind, rTypes);

            // inspect forward and reverse versions so we can pick one or both.
            BindingTarget cTarget, rcTarget, eqTarget, reqTarget, ltTarget, rgtTarget, gtTarget, rltTarget;
            GetCombinedTargets(fcand, rcand, out cTarget, out rcTarget);
            GetCombinedTargets(eqcand, reqcand, out eqTarget, out reqTarget);
            GetCombinedTargets(ltcand, rgtcand, out ltTarget, out rgtTarget);
            GetCombinedTargets(gtcand, rltcand, out gtTarget, out rltTarget);

            PythonType xType = types[0];
            PythonType yType = types[1];

            // now build the rule from the targets.
            StandardRule<T> rule = new StandardRule<T>();
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
                more = more &&
                    MakeOneComparisonTarget(cTarget, rule, stmts, types, false);

                if (xType != TypeCache.OldInstance) {
                    // try __cmp__ backwards for new-style classes and don't fallback to
                    // rich comparisons if available
                    more = more && MakeOneComparisonTarget(rcTarget, rule, stmts, types, true);
                    tryRich = false;
                }
            }

            if (tryRich) {
                // try eq
                more = more &&
                    MakeOneComparisonTarget(eqTarget, rule, stmts, types, false, 0) &&
                    MakeOneComparisonTarget(reqTarget, rule, stmts, types, true, 0);

                // try less than & reverse
                more = more &&
                    MakeOneComparisonTarget(ltTarget, rule, stmts, types, false, -1) &&
                    MakeOneComparisonTarget(rgtTarget, rule, stmts, types, true, -1);

                // try greater than & reverse
                more = more &&
                    MakeOneComparisonTarget(gtTarget, rule, stmts, types, false, 1) &&
                    MakeOneComparisonTarget(rltTarget, rule, stmts, types, true, 1);
            }

            if (xType != yType) {
                more = more &&
                    MakeOneComparisonTarget(cTarget, rule, stmts, types, false) &&
                    MakeOneComparisonTarget(rcTarget, rule, stmts, types, true);
            }

            if (more) {
                // fall back to compare types
                stmts.Add(MakeFallbackCompare(rule));
            }

            rule.Target = Ast.Block(stmts);
            return rule;
        }

        private StandardRule<T> MakeRuleForBinaryOperator(PythonType[] types, Operators oper, BindingTarget fCand, BindingTarget rCand, PythonTypeSlot fSlot, PythonTypeSlot rSlot) {
            BindingTarget fTarget, rTarget;

            if (!GetCombinedTargets(fCand, rCand, out fTarget, out rTarget) &&
                fSlot == null &&
                rSlot == null) {
                return MakeRuleForNoMatch(types);
            }

            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, types);

            List<Expression> stmts = new List<Expression>();
            if (MakeOneTarget(fTarget, fSlot, rule, stmts, false, types)) {
                if (rSlot != null) {
                    stmts.Add(MakeSlotCall(rSlot, rule, true));
                    stmts.Add(MakeBinaryThrow(rule));
                } else if (MakeOneTarget(rTarget, rSlot, rule, stmts, false, types)) {
                    stmts.Add(MakeBinaryThrow(rule));
                }
            }
            rule.Target = Ast.Block(stmts);
            return rule;
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
        private StandardRule<T> MakeIndexerRule(PythonType[] types) {
            SymbolId item, slice;
            PythonType indexedType = types[0];
            BuiltinFunction itemFunc = null;
            PythonTypeSlot itemSlot = null;
            bool callSlice = false;
            int mandatoryArgs;

            GetIndexOperators(out item, out slice, out mandatoryArgs);

            if (types.Length == mandatoryArgs + 1 && IsSlice && HasOnlyNumericTypes(types, Action.Operation == Operators.SetSlice)) {
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
            Callable callable = Callable.MakeCallable(Binder, Action.Operation, itemFunc, itemSlot);
            if (callable == null) {
                return PythonBinderHelper.TypeError<T>("'{0}' object is unsubscriptable", types[0]);
            }
            
            // prepare the arguments and make the builder which will
            // call __*slice__ or __*item__
            Expression[] args;
            IndexBuilder builder;
            StandardRule<T> rule = new StandardRule<T>();
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
        private Expression[] GetItemSliceArguments(StandardRule<T> rule, PythonType[] types) {
            Expression[] args;
            if (Action.Operation == Operators.SetSlice) {
                args = new Expression[] { 
                            Ast.ConvertHelper(rule.Parameters[0], types[0]), 
                            GetSetSlice(rule), 
                            Ast.ConvertHelper(rule.Parameters[rule.Parameters.Count - 1], types[types.Length - 1])
                        };
            } else {
                Debug.Assert(Action.Operation == Operators.GetSlice || Action.Operation == Operators.DeleteSlice);

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
            public abstract void CompleteRuleTarget(StandardRule<T> rule, Expression[] args, Function<bool> customFailure);

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

            public override void CompleteRuleTarget(StandardRule<T>/*!*/ rule, Expression/*!*/[]/*!*/ args, Function<bool> customFailure) {
                Assert.NotNull(args);
                Assert.NotNullItems(args);
                Assert.NotNull(rule);

                MethodBinder binder = MethodBinder.MakeBinder(Binder,
                    Name,
                    _bf.Targets,
                    PythonNarrowing.None,
                    PythonNarrowing.IndexOperator);

                Type[] types = CompilerHelpers.GetExpressionTypes(args);
                BindingTarget target = binder.MakeBindingTarget(CallType.ImplicitInstance, types);

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

            public override void CompleteRuleTarget(StandardRule<T> rule, Expression[] args, Function<bool> customFailure) {
                Variable callable = rule.GetTemporary(typeof(object), "slot");

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
            private StandardRule<T> _rule;
            private Callable _callable;
            private PythonType[] _types;

            public IndexBuilder(StandardRule<T> rule, PythonType[] types, Callable callable) {
                _rule = rule;
                _callable = callable;
                _types = types;
                PythonBinderHelper.MakeTest(Rule, types);
            }
            
            public abstract StandardRule<T> MakeRule(Expression[] args);

            protected StandardRule<T> Rule {
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
            private Variable _lengthVar;        // Nullable<int>, assigned if we need to calculate the length of the object during the call.

            public SliceBuilder(StandardRule<T> rule, PythonType[] types, Callable callable)
                : base(rule, types, callable) {
            }

            public override StandardRule<T> MakeRule(Expression[] args) {
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
            public ItemBuilder(StandardRule<T> rule, PythonType[] types, Callable callable)
                : base(rule, types, callable) {
            }

            public override StandardRule<T> MakeRule(Expression[] args) {                
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
                return Action.Operation == Operators.GetSlice || Action.Operation == Operators.SetSlice || Action.Operation == Operators.DeleteSlice;
            }
        }

        private void FixArgsForIndex(PythonType[] types, Expression[] args, Type[] callTypes) {
            if (Action.Operation == Operators.GetItem || Action.Operation == Operators.SetItem || Action.Operation == Operators.DeleteItem) {
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
            switch (Action.Operation) {
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
        private bool ResolveOperatorIndex(PythonType[] types, StandardRule<T> ret, Expression[] indexArgs) {
            bool foundIndexable = false;

            for (int i = 1; i < types.Length; i++) {
                if (types[i].IsSubclassOf(TypeCache.Int32) || types[i].IsSubclassOf(TypeCache.BigInteger)) {
                    // these are inherently indexes...
                    continue;
                }

                PythonTypeSlot pts;
                if (types[i].TryResolveSlot(Context, Symbols.Index, out pts)) {
                    foundIndexable = true;
                    Variable tmp = ret.GetTemporary(typeof(object), "slotVal");
                    indexArgs[i] = Ast.Comma(
                        PythonBinderHelper.MakeTryGetTypeMember<T>(ret, pts, tmp, indexArgs[i], Ast.RuntimeConstant(types[i])),
                        Ast.Action.Call(typeof(int), Ast.Read(tmp))
                    );
                    types[i] = TypeCache.Int32;
                }
            }
            return foundIndexable;
        }                     

        private static Expression GetSetSlice(StandardRule<T> ret) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeSlice"),
                Ast.ConvertHelper(GetSetParameter(ret, 1), typeof(object)),
                Ast.ConvertHelper(GetSetParameter(ret, 2), typeof(object)),
                Ast.ConvertHelper(GetSetParameter(ret, 3), typeof(object))
            );
        }

        private static Expression GetGetOrDeleteSlice(StandardRule<T> ret) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeSlice"),
                Ast.ConvertHelper(GetGetOrDeleteParameter(ret, 1), typeof(object)),
                Ast.ConvertHelper(GetGetOrDeleteParameter(ret, 2), typeof(object)),
                Ast.ConvertHelper(GetGetOrDeleteParameter(ret, 3), typeof(object))
            );
        }

        private static Expression GetGetOrDeleteParameter(StandardRule<T> ret, int index) {
            if (ret.Parameters.Count > index) {
                return CheckMissing(ret.Parameters[index]);
            }
            return Ast.Null();
        }

        private static Expression GetSetParameter(StandardRule<T> ret, int index) {
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

        private Expression MakeCall(BindingTarget target, StandardRule<T> block, bool reverse) {
            return MakeCall(target, block, reverse, null);
        }

        private Expression MakeCall(BindingTarget target, StandardRule<T> block, bool reverse, PythonType[] types) {
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

            // add casts to the known types to avoid full conversions that MakeExpression will emit.
            if (types != null) {
                vars = ArrayUtils.MakeArray(vars);
                for (int i = 0; i < types.Length; i++) {
                    if (types[i] != null) {
                        vars[i] = Ast.ConvertHelper(vars[i], CompilerHelpers.GetVisibleType(types[i].UnderlyingSystemType));
                    }
                }
            }

            return target.MakeExpression(block, vars);
        }

        private bool MakeOneComparisonTarget(BindingTarget target, StandardRule<T> rule, List<Expression> stmts, PythonType[] types, bool reverse) {
            return MakeOneComparisonTarget(target, rule, stmts, types, reverse, null);
        }

        private bool MakeOneComparisonTarget(BindingTarget target, StandardRule<T> rule, List<Expression> stmts, PythonType[] types, bool reverse, int? val) {
            if (target == null) return true;

            if (ReturnsNotImplemented(target)) {
                Variable tmp = rule.GetTemporary(target.ReturnType, "tmp");
                Expression body;
                if (val != null) {
                    body = MakeValueCheck(rule, val, tmp);
                } else {
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

        private Expression MakeValueCheck(StandardRule<T> rule, int? val, Variable var) {
            Expression test = Ast.ReadDefined(var);
            if (test.Type != typeof(bool)) {
                test = Ast.Action.ConvertTo(typeof(bool), ConversionResultKind.ExplicitCast, test);
            }
            return Ast.IfThen(
                test,
                rule.MakeReturn(Binder, Ast.Constant(val))
            );
        }

        private bool MakeOneTarget(BindingTarget target, StandardRule<T> block, List<Expression> stmts, bool reverse, PythonType[] types) {
            return MakeOneTarget(target,
                null,
                block,
                stmts,
                reverse,
                types);
        }

        private bool MakeOneTarget(BindingTarget target, PythonTypeSlot slotTarget, StandardRule<T> block, List<Expression> stmts, bool reverse, PythonType[] types) {
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

        private Expression MakeSlotCall(PythonTypeSlot/*!*/ slotTarget, StandardRule<T>/*!*/ block, bool reverse) {
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

        private Expression MakeSlotCallWorker(PythonTypeSlot slotTarget, StandardRule<T> block, Expression self, params Expression[] args) {
            Variable callable = block.GetTemporary(typeof(object), "slot");
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

        private Expression CheckNotImplemented(StandardRule<T> block, Expression call) {
            Variable tmp = block.GetTemporary(call.Type, "tmp");

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

        private Expression MakeUnaryThrow(StandardRule<T> block) {
            return Ast.Throw(
                Ast.Call(
                    typeof(PythonOps).GetMethod("TypeErrorForUnaryOp"),
                    Ast.Constant(SymbolTable.IdToString(Symbols.OperatorToSymbol(Action.Operation))),
                    Ast.ConvertHelper(block.Parameters[0], typeof(object))
                )
            );
        }

        private Expression MakeBinaryThrow(StandardRule<T> block) {
            return Ast.Throw(
                Ast.Call(
                    typeof(PythonOps).GetMethod("TypeErrorForBinaryOp"),
                    Ast.Constant(SymbolTable.IdToString(Symbols.OperatorToSymbol(Action.Operation))),
                    Ast.ConvertHelper(block.Parameters[0], typeof(object)),
                    Ast.ConvertHelper(block.Parameters[1], typeof(object))
                )
            );
        }

        private bool MakeOneCompare(BindingTarget target, StandardRule<T> block, List<Expression> stmts, bool reverse, PythonType[] types) {
            if (target == null || !target.Success) return true;

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

        private Expression MakeCompareTest(Expression expr, StandardRule<T> block, bool reverse) {
            if (expr.Type == typeof(int)) {
                // fast path, just do a compare in IL
                return block.MakeReturn(Binder,
                    GetCompareNode(reverse, expr)
                );
            } else {
                return block.MakeReturn(Binder,
                    Ast.Call(
                        GetCompareMethod(reverse),
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CompareToZero"),
                            Ast.ConvertHelper(expr, typeof(object))
                        )
                    )
                );
            }
        }

        private Expression GetCompareNode(bool reverse, Expression expr) {
            switch (reverse ? CompilerHelpers.OperatorToReverseOperator(Action.Operation) : Action.Operation) {
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

        private Expression MakeFallbackCompare(StandardRule<T> block) {
            return block.MakeReturn(Binder,
                Ast.Call(
                    GetComparisonFallbackMethod(Action.Operation),
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
            return binder.MakeBindingTarget(CallType.None, PythonTypeOps.ConvertToTypes(types));
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
            PythonType yType = types[1];
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

        public static object DynamicInvokeBinaryOperation(CodeContext context, Operators op, object x, object y) {
            PythonType xDType = DynamicHelpers.GetPythonType(x);
            PythonType yDType = DynamicHelpers.GetPythonType(y);
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

            throw PythonOps.TypeError(MakeBinaryOpErrorMessage(op, xDType.Name, yDType.Name));
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
                case Operators.Xor: return "^";
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
                case Operators.InPlaceXor: return "^=";
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
                case Operators.ReverseXor: return "^";
                default: return op.ToString();
            }
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

            PythonType xType = DynamicHelpers.GetPythonType(x);
            PythonType yType = DynamicHelpers.GetPythonType(y);

            bool tryRich = true;
            if (xType == yType) {
                // use __cmp__ first if it's defined
                if (DynamicHelpers.GetPythonType(x).TryInvokeBinaryOperator(context, Operators.Compare, x, y, out ret)) {
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

            PythonType dt1 = DynamicHelpers.GetPythonType(x);
            PythonType dt2 = DynamicHelpers.GetPythonType(y);

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
            PythonType dt = DynamicHelpers.GetPythonType(x);
            if (dt.TryInvokeBinaryOperator(context, op, x, y, out ret) && ret != PythonOps.NotImplemented)
                return ret;

            //TODO something's backwards here - shouldn't need to make a new action
            return DynamicInvokeBinaryOperation(context, DoOperationAction.Make(op).DirectOperation, x, y);
        }

        #region Unary Operators

        private StandardRule<T> MakeUnaryRule(PythonType[] types, Operators op) {
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

            MethodBinder binder = MethodBinder.MakeBinder(Binder, op.ToString(), func.Targets, PythonNarrowing.None, PythonNarrowing.All);

            Debug.Assert(binder != null);

            BindingTarget target = binder.MakeBindingTarget(CallType.None, PythonTypeOps.ConvertToTypes(types));
            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, types);
            rule.Target = rule.MakeReturn(Binder, MakeCall(target, rule, false));
            return rule;
        }

        private StandardRule<T> MakeUnaryNotRule(PythonType[] types) {
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
                    PythonNarrowing.None,
                    PythonNarrowing.All);

                Debug.Assert(binder != null);
                BindingTarget target = binder.MakeBindingTarget(CallType.None, PythonTypeOps.ConvertToTypes(types));
                notExpr = MakeCall(target, rule, false);

                if (nonzero != null) {
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

        private StandardRule<T> MakeDynamicNotRule(PythonType[] types) {
            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, types);
            rule.Target = rule.MakeReturn(Binder,
                Ast.Call(
                    typeof(PythonOps).GetMethod("Not"),
                    Ast.ConvertHelper(rule.Parameters[0], typeof(object))
                )
            );
            return rule;
        }

        public static object DynamicInvokeUnaryOperation(CodeContext context, Operators op, object x) {
            object ret;
            PythonType dt = DynamicHelpers.GetPythonType(x);
            if (dt.TryInvokeUnaryOperator(context, op, x, out ret) && ret != PythonOps.NotImplemented)
                return ret;

            throw PythonOps.TypeError(MakeUnaryOpErrorMessage(op.ToString(), DynamicHelpers.GetPythonType(x).Name));
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
