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

using System; using Microsoft;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting;
using System.Text;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    static partial class PythonProtocol {
        private const string DisallowCoerce = "DisallowCoerce";

        public static MetaObject/*!*/ Operation(OperationBinder/*!*/ operation, params MetaObject/*!*/[]/*!*/ args) {
            foreach (MetaObject mo in args) {
                if (mo.NeedsDeferral()) {
                    return operation.Defer(args);
                }
            }

            ValidationInfo valInfo = BindingHelpers.GetValidationInfo(null, args);

            MetaObject res = MakeOperationRule(operation, args);

            return BindingHelpers.AddDynamicTestAndDefer(operation, res, args, valInfo);
        }

        private static MetaObject/*!*/ MakeOperationRule(OperationBinder/*!*/ operation, MetaObject/*!*/[]/*!*/ args) {
            switch (operation.Operation) {
                case StandardOperators.Documentation:
                    return MakeDocumentationOperation(operation, args);
                case StandardOperators.MemberNames:
                    return MakeMemberNamesOperation(operation, args);
                case StandardOperators.CallSignatures:
                    return MakeCallSignatureOperation(args[0], CompilerHelpers.GetMethodTargets(args[0].Value));
                case StandardOperators.IsCallable:
                    return MakeIscallableOperation(operation, args);

                case StandardOperators.GetItem:
                case StandardOperators.SetItem:
                case StandardOperators.GetSlice:
                case StandardOperators.SetSlice:
                case StandardOperators.DeleteItem:
                case StandardOperators.DeleteSlice:
                    // Indexers need to see if the index argument is an expandable tuple.  This will
                    // be captured in the AbstractValue in the future but today is captured in the
                    // real value.
                    return MakeIndexerOperation(operation, args);

                case StandardOperators.Not:
                    return MakeUnaryNotOperation(operation, args[0]);
                case OperatorStrings.Hash:
                    return MakeHashOperation(operation, args[0]);

                case StandardOperators.Contains:
                    return MakeContainsOperation(operation, args);

                default:
                    if (IsUnary(operation.Operation)) {
                        return MakeUnaryOperation(operation, args[0]);
                    } else if (IsComparision(operation.Operation)) {
                        return MakeComparisonOperation(args, operation);
                    }

                    return MakeSimpleOperation(args, operation);
            }
        }

        #region Unary Operations

        /// <summary>
        /// Creates a rule for the contains operator.  This is exposed via "x in y" in 
        /// IronPython.  It is implemented by calling the __contains__ method on x and
        /// passing in y.  
        /// 
        /// If a type doesn't define __contains__ but does define __getitem__ then __getitem__ is 
        /// called repeatedly in order to see if the object is there.
        /// 
        /// For normal .NET enumerables we'll walk the iterator and see if it's present.
        /// </summary>
        private static MetaObject/*!*/ MakeContainsOperation(OperationBinder/*!*/ operation, MetaObject/*!*/[]/*!*/ types) {
            MetaObject res;
            // the paramteres come in backwards from how we look up __contains__, flip them.
            Debug.Assert(types.Length == 2);
            ArrayUtils.SwapLastTwo(types);

            BinderState state = BinderState.GetBinderState(operation);
            SlotOrFunction sf = SlotOrFunction.GetSlotOrFunction(state, Symbols.Contains, types);

            if (sf.Success) {
                // just a call to __contains__
                res = sf.Target;
            } else {
                RestrictTypes(types);

                ParameterExpression curIndex = Ast.Variable(typeof(int), "count");
                sf = SlotOrFunction.GetSlotOrFunction(state, Symbols.GetItem, types[0], new MetaObject(curIndex, Restrictions.Empty));
                if (sf.Success) {
                    // defines __getitem__, need to loop over the indexes and see if we match

                    ParameterExpression getItemRes = Ast.Variable(sf.ReturnType, "getItemRes");
                    ParameterExpression containsRes = Ast.Variable(typeof(bool), "containsRes");

                    LabelTarget target = Ast.Label();
                    res = new MetaObject(
                        Ast.Block(
                            new ParameterExpression[] { curIndex, getItemRes, containsRes },
                            Utils.Loop(
                                null,                                                     // test
                                Ast.Assign(curIndex, Ast.Add(curIndex, Ast.Constant(1))), // increment
                                Ast.Block(                                            // body
                        // getItemRes = param0.__getitem__(curIndex)
                                    Utils.Try(
                                        Ast.Assign(
                                            getItemRes,
                                            sf.Target.Expression
                                        )
                                    ).Catch(
                        // end of indexes, return false
                                        typeof(IndexOutOfRangeException),
                                        Ast.Break(target)
                                    ),
                        // if(getItemRes == param1) return true
                                    Utils.If(
                                        Ast.Dynamic(
                                            new PythonOperationBinder(
                                                state,
                                                StandardOperators.Equal
                                            ),
                                            typeof(bool),
                                            types[1].Expression,
                                            getItemRes
                                        ),
                                        Ast.Assign(containsRes, Ast.Constant(true)),
                                        Ast.Break(target)
                                    ),
                                    Ast.Empty()
                                ),
                                null,                                               // loop else
                                target,                                             // break label target
                                null
                            ),
                            containsRes
                        ),
                        Restrictions.Combine(types)
                    );
                } else {
                    sf = SlotOrFunction.GetSlotOrFunction(state, Symbols.Iterator, types[0]);
                    if (sf.Success) {
                        // iterate using __iter__
                        res = new MetaObject(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("ContainsFromEnumerable"),
                                Ast.Constant(state.Context),
                                Ast.Dynamic(
                                    new ConversionBinder(
                                        state,
                                        typeof(IEnumerator),
                                        ConversionResultKind.ExplicitCast
                                    ),
                                    typeof(IEnumerator),
                                    sf.Target.Expression
                                ),
                                AstUtils.Convert(types[1].Expression, typeof(object))
                            ),
                            Restrictions.Combine(types)
                        );
                    } else {
                        // non-iterable object
                        res = new MetaObject(
                            Ast.Throw(
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("TypeErrorForNonIterableObject"),
                                    AstUtils.Convert(
                                        types[1].Expression,
                                        typeof(object)
                                    )
                                )
                            ),
                            Restrictions.Combine(types)
                        );
                    }
                }
            }

            return res;
        }

        private static void RestrictTypes(MetaObject/*!*/[] types) {
            for (int i = 0; i < types.Length; i++) {
                types[i] = types[i].Restrict(types[i].LimitType);
            }
        }

        private static MetaObject/*!*/ MakeHashOperation(OperationBinder/*!*/ operation, MetaObject/*!*/ self) {
            self = self.Restrict(self.LimitType);

            SlotOrFunction func = SlotOrFunction.GetSlotOrFunction(BinderState.GetBinderState(operation), Symbols.Hash, self);

            return func.Target;
        }

        private static MetaObject/*!*/ MakeUnaryOperation(OperationBinder/*!*/ operation, MetaObject/*!*/ self) {
            self = self.Restrict(self.LimitType);

            SlotOrFunction func = SlotOrFunction.GetSlotOrFunction(BinderState.GetBinderState(operation), Symbols.OperatorToSymbol(operation.Operation), self);

            if (!func.Success) {
                // we get the error message w/ {0} so that PythonBinderHelper.TypeError formats it correctly
                return TypeError(operation, MakeUnaryOpErrorMessage(operation.Operation.ToString(), "{0}"), self);
            }

            return func.Target;
        }

        private static MetaObject/*!*/ MakeUnaryNotOperation(OperationBinder/*!*/ operation, MetaObject/*!*/ self) {
            self = self.Restrict(self.LimitType);

            SlotOrFunction nonzero = SlotOrFunction.GetSlotOrFunction(BinderState.GetBinderState(operation), Symbols.NonZero, self);
            SlotOrFunction length = SlotOrFunction.GetSlotOrFunction(BinderState.GetBinderState(operation), Symbols.Length, self);

            Expression notExpr;

            if (!nonzero.Success && !length.Success) {
                // always False or True for None
                notExpr = self.LimitType == typeof(Null) ? Ast.Constant(true) : Ast.Constant(false);
            } else {
                SlotOrFunction target = nonzero.Success ? nonzero : length;

                notExpr = target.Target.Expression;

                if (nonzero.Success) {
                    // call non-zero and negate it
                    if (notExpr.Type == typeof(bool)) {
                        notExpr = Ast.Equal(notExpr, Ast.Constant(false));
                    } else {
                        notExpr = Ast.Call(
                            typeof(PythonOps).GetMethod("Not"),
                            AstUtils.Convert(notExpr, typeof(object))
                        );
                    }
                } else {
                    // call len, compare w/ zero
                    if (notExpr.Type == typeof(int)) {
                        notExpr = Ast.Equal(notExpr, Ast.Constant(0));
                    } else {
                        notExpr = Ast.Dynamic(
                            new PythonOperationBinder(
                                BinderState.GetBinderState(operation),
                                StandardOperators.Compare
                            ),
                            typeof(int),
                            notExpr,
                            Ast.Constant(0)
                        );
                    }
                }
            }

            return new MetaObject(
                notExpr,
                self.Restrictions.Merge(nonzero.Target.Restrictions.Merge(length.Target.Restrictions))
            );
        }


        #endregion

        #region Reflective Operations

        private static MetaObject/*!*/ MakeDocumentationOperation(OperationBinder/*!*/ operation, MetaObject/*!*/[]/*!*/ args) {
            BinderState state = BinderState.GetBinderState(operation);

            return new MetaObject(
                Binders.Get(
                    BinderState.GetCodeContext(operation),
                    state,
                    typeof(string),
                    "__doc__",
                    args[0].Expression
                ),
                args[0].Restrictions
            );
        }

        private static MetaObject/*!*/ MakeMemberNamesOperation(OperationBinder/*!*/ operation, MetaObject[] args) {
            MetaObject self = args[0];
            CodeContext context;
            if (args.Length > 1 && args[0].LimitType == typeof(CodeContext)) {
                self = args[1];
                context = (CodeContext)args[0].Value;
            } else {
                context = BinderState.GetBinderState(operation).Context;
            }

            if (typeof(IMembersList).IsAssignableFrom(self.LimitType)) {
                return BinderState.GetBinderState(operation).Binder.DoOperation(operation.Operation, BinderState.GetCodeContext(operation), args);
            }

            PythonType pt = DynamicHelpers.GetPythonType(self.Value);
            List<string> strNames = GetMemberNames(context, pt, self.Value);

            if (pt.IsSystemType) {
                return new MetaObject(
                    Ast.Constant(strNames),
                    Restrictions.GetInstanceRestriction(self.Expression, self.Value).Merge(self.Restrictions)
                );
            }

            return new MetaObject(
                Ast.Constant(strNames),
                Restrictions.GetInstanceRestriction(self.Expression, self.Value).Merge(self.Restrictions)
            );
        }

        internal static MetaObject/*!*/ MakeCallSignatureOperation(MetaObject/*!*/ self, IList<MethodBase/*!*/>/*!*/ targets) {
            List<string> arrres = new List<string>();
            foreach (MethodBase mb in targets) {
                StringBuilder res = new StringBuilder();
                string comma = "";

                Type retType = CompilerHelpers.GetReturnType(mb);
                if (retType != typeof(void)) {
                    res.Append(DynamicHelpers.GetPythonTypeFromType(retType).Name);
                    res.Append(" ");
                }

                MethodInfo mi = mb as MethodInfo;
                if (mi != null) {
                    string name;
                    NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mb.DeclaringType), mi, out name);
                    res.Append(name);
                } else {
                    res.Append(DynamicHelpers.GetPythonTypeFromType(mb.DeclaringType).Name);
                }

                res.Append("(");
                if (!CompilerHelpers.IsStatic(mb)) {
                    res.Append("self");
                    comma = ", ";
                }

                foreach (ParameterInfo pi in mb.GetParameters()) {
                    if (pi.ParameterType == typeof(CodeContext)) continue;

                    res.Append(comma);
                    res.Append(DynamicHelpers.GetPythonTypeFromType(pi.ParameterType).Name + " " + pi.Name);
                    comma = ", ";
                }
                res.Append(")");
                arrres.Add(res.ToString());
            }

            return new MetaObject(
                Ast.Constant(arrres.ToArray()),
                self.Restrictions.Merge(Restrictions.GetInstanceRestriction(self.Expression, self.Value))
            );
        }

        private static MetaObject/*!*/ MakeIscallableOperation(OperationBinder/*!*/ operation, MetaObject/*!*/[]/*!*/ args) {
            // Certain non-python types (encountered during interop) are callable, but don't have 
            // a __call__ attribute. The default base binder also checks these, but since we're overriding
            // the base binder, we check them here.
            MetaObject self = args[0];
            
            // only applies when called from a Python site
            if (typeof(Delegate).IsAssignableFrom(self.LimitType) ||
                typeof(MethodGroup).IsAssignableFrom(self.LimitType)) {
                return new MetaObject(
                    Ast.Constant(true),
                    self.Restrict(self.LimitType).Restrictions
                );
            }

            BinderState state = BinderState.GetBinderState(operation);
            Expression isCallable = Ast.NotEqual(
                Binders.TryGet(
                    BinderState.GetCodeContext(operation),
                    state,
                    typeof(object),
                    "__call__",
                    self.Expression
                ),
                Ast.Constant(OperationFailed.Value)
            );

            return new MetaObject(
                isCallable,
                self.Restrict(self.LimitType).Restrictions
            );
        }

        #endregion

        #region Common Binary Operations

        private static MetaObject/*!*/ MakeSimpleOperation(MetaObject/*!*/[]/*!*/ types, OperationBinder/*!*/ operation) {
            RestrictTypes(types);

            SlotOrFunction fbinder;
            SlotOrFunction rbinder;
            PythonTypeSlot fSlot;
            PythonTypeSlot rSlot;
            GetOpreatorMethods(types, operation.Operation, BinderState.GetBinderState(operation), out fbinder, out rbinder, out fSlot, out rSlot);

            return MakeBinaryOperatorResult(types, operation, fbinder, rbinder, fSlot, rSlot);
        }

        private static void GetOpreatorMethods(MetaObject/*!*/[]/*!*/ types, string oper, BinderState state, out SlotOrFunction fbinder, out SlotOrFunction rbinder, out PythonTypeSlot fSlot, out PythonTypeSlot rSlot) {
            oper = NormalizeOperator(oper);
            if (IsInPlace(oper)) {
                oper = DirectOperation(oper);
            }

            SymbolId op, rop;
            if (!TypeInfo.IsReverseOperator(oper)) {
                op = Symbols.OperatorToSymbol(oper);
                rop = Symbols.OperatorToReversedSymbol(oper);
            } else {
                // coming back after coercion, just try reverse operator.
                rop = Symbols.OperatorToSymbol(oper);
                op = Symbols.OperatorToReversedSymbol(oper);
            }

            fSlot = null;
            rSlot = null;
            PythonType fParent, rParent;

            if (oper == StandardOperators.Multiply && 
                IsSequence(types[0]) && 
                !PythonOps.IsNonExtensibleNumericType(types[1].LimitType)) {
                // class M:
                //      def __rmul__(self, other):
                //          print "CALLED"
                //          return 1
                //
                // print [1,2] * M()
                //
                // in CPython this results in a successful call to __rmul__ on the type ignoring the forward
                // multiplication.  But calling the __mul__ method directly does NOT return NotImplemented like
                // one might expect.  Therefore we explicitly convert the MetaObject argument into an Index
                // for binding purposes.  That allows this to work at multiplication time but not with
                // a direct call to __mul__.

                MetaObject[] newTypes = new MetaObject[2];
                newTypes[0] = types[0];
                newTypes[1] = new MetaObject(
                    Ast.New(
                        typeof(Index).GetConstructor(new Type[] { typeof(object) }),
                        types[1].Expression
                    ),
                    Restrictions.Empty
                );
                types = newTypes;
            }

            if (!SlotOrFunction.TryGetBinder(state, types, op, SymbolId.Empty, out fbinder, out fParent)) {
                foreach (PythonType pt in MetaPythonObject.GetPythonType(types[0]).ResolutionOrder) {
                    if (pt.TryLookupSlot(state.Context, op, out fSlot)) {
                        fParent = pt;
                        break;
                    }
                }
            }

            if (!SlotOrFunction.TryGetBinder(state, types, SymbolId.Empty, rop, out rbinder, out rParent)) {
                foreach (PythonType pt in MetaPythonObject.GetPythonType(types[1]).ResolutionOrder) {
                    if (pt.TryLookupSlot(state.Context, rop, out rSlot)) {
                        rParent = pt;
                        break;
                    }
                }
            }

            if (fParent != null && (rbinder.Success || rSlot != null) && rParent != fParent && rParent.IsSubclassOf(fParent)) {
                // Python says if x + subx and subx defines __r*__ we should call r*.
                fbinder = SlotOrFunction.Empty;
                fSlot = null;
            }

            if (!fbinder.Success && !rbinder.Success && fSlot == null && rSlot == null) {
                if (op == Symbols.OperatorTrueDivide || op == Symbols.OperatorReverseTrueDivide) {
                    // true div on a type which doesn't support it, go ahead and try normal divide
                    string newOp = op == Symbols.OperatorTrueDivide ? StandardOperators.Divide : OperatorStrings.ReverseDivide;

                    GetOpreatorMethods(types, newOp, state, out fbinder, out rbinder, out fSlot, out rSlot);
                }
            }
        }

        private static bool IsSequence(MetaObject/*!*/ metaObject) {
            if (typeof(List).IsAssignableFrom(metaObject.LimitType) ||
                typeof(PythonTuple).IsAssignableFrom(metaObject.LimitType) ||
                typeof(String).IsAssignableFrom(metaObject.LimitType)) {
                return true;
            }
            return false;
        }

        private static MetaObject/*!*/ MakeBinaryOperatorResult(MetaObject/*!*/[]/*!*/ types, OperationBinder/*!*/ operation, SlotOrFunction/*!*/ fCand, SlotOrFunction/*!*/ rCand, PythonTypeSlot fSlot, PythonTypeSlot rSlot) {
            Assert.NotNull(operation, fCand, rCand);

            string op = operation.Operation;
            SlotOrFunction fTarget, rTarget;

            // TODO: some Builder class for condition, body, vars
            ConditionalBuilder bodyBuilder = new ConditionalBuilder(operation);

            if (IsInPlace(op)) {
                // in place operator, see if there's a specific method that handles it.
                SlotOrFunction function = SlotOrFunction.GetSlotOrFunction(BinderState.GetBinderState(operation), Symbols.OperatorToSymbol(op), types);

                // we don't do a coerce for in place operators if the lhs implements __iop__
                if (!MakeOneCompareGeneric(function, false, types, MakeCompareReturn, bodyBuilder)) {
                    // the method handles it and always returns a useful value.
                    return bodyBuilder.GetMetaObject(types);
                }
            }

            if (!SlotOrFunction.GetCombinedTargets(fCand, rCand, out fTarget, out rTarget) &&
                fSlot == null &&
                rSlot == null &&
                !ShouldCoerce(operation, types[0], types[1], false) &&
                !ShouldCoerce(operation, types[1], types[0], false) &&
                bodyBuilder.NoConditions) {
                return MakeRuleForNoMatch(operation, op, types);
            }

            if (ShouldCoerce(operation, types[0], types[1], false) && 
                (op != StandardOperators.Mod || !MetaPythonObject.GetPythonType(types[0]).IsSubclassOf(TypeCache.String))) {
                // need to try __coerce__ first.
                DoCoerce(operation, bodyBuilder, op, types, false);
            }

            if (MakeOneTarget(BinderState.GetBinderState(operation), fTarget, fSlot, bodyBuilder, false, types)) {
                if (ShouldCoerce(operation, types[1], types[0], false)) {
                    // need to try __coerce__ on the reverse first                    
                    DoCoerce(operation, bodyBuilder, op, new MetaObject[] { types[1], types[0] }, true);
                }

                if (rSlot != null) {
                    MakeSlotCall(BinderState.GetBinderState(operation), types, bodyBuilder, rSlot, true);
                    bodyBuilder.FinishCondition(MakeBinaryThrow(operation, op, types).Expression);
                } else if (MakeOneTarget(BinderState.GetBinderState(operation), rTarget, rSlot, bodyBuilder, false, types)) {
                    // need to fallback to throwing or coercion
                    bodyBuilder.FinishCondition(MakeBinaryThrow(operation, op, types).Expression);
                }
            }

            return bodyBuilder.GetMetaObject(types);
        }

        private static void MakeCompareReturn(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ retValue, bool isReverse) {
            if (retCondition != null) {
                bodyBuilder.AddCondition(retCondition, retValue);
            } else {
                bodyBuilder.FinishCondition(retValue);
            }
        }

        /// <summary>
        /// Delegate for finishing the comparison.   This takes in a condition and a return value and needs to update the ConditionalBuilder
        /// with the appropriate resulting body.  The condition may be null.
        /// </summary>
        private delegate void ComparisonHelper(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ retValue, bool isReverse);

        /// <summary>
        /// Helper to handle a comparison operator call.  Checks to see if the call can
        /// return NotImplemented and allows the caller to modify the expression that
        /// is ultimately returned (e.g. to turn __cmp__ into a bool after a comparison)
        /// </summary>
        private static bool MakeOneCompareGeneric(SlotOrFunction/*!*/ target, bool reverse, MetaObject/*!*/[]/*!*/ types, ComparisonHelper returner, ConditionalBuilder/*!*/ bodyBuilder) {
            if (target == SlotOrFunction.Empty || !target.Success) return true;

            ParameterExpression tmp;

            if (target.ReturnType == typeof(bool)) {
                tmp = bodyBuilder.CompareRetBool;
            } else {
                tmp = Ast.Variable(target.ReturnType, "compareRetValue");
                bodyBuilder.AddVariable(tmp);
            }

            if (target.MaybeNotImplemented) {
                Expression call = target.Target.Expression;
                Expression assign = Ast.Assign(tmp, call);

                returner(
                    bodyBuilder,
                    Ast.NotEqual(
                        assign,
                        Ast.Constant(PythonOps.NotImplemented)
                    ),
                    tmp,
                    reverse);
                return true;
            } else {
                returner(
                    bodyBuilder,
                    null,
                    target.Target.Expression,
                    reverse
                );
                return false;
            }
        }

        private static bool MakeOneTarget(BinderState/*!*/ state, SlotOrFunction/*!*/ target, PythonTypeSlot slotTarget, ConditionalBuilder/*!*/ bodyBuilder, bool reverse, MetaObject/*!*/[]/*!*/ types) {
            if (target == SlotOrFunction.Empty && slotTarget == null) return true;

            if (slotTarget != null) {
                MakeSlotCall(state, types, bodyBuilder, slotTarget, reverse);
                return true;
            } else if (target.MaybeNotImplemented) {
                Debug.Assert(target.ReturnType == typeof(object));

                ParameterExpression tmp = Ast.Variable(typeof(object), "slot");
                bodyBuilder.AddVariable(tmp);

                bodyBuilder.AddCondition(
                    Ast.NotEqual(
                        Ast.Assign(
                            tmp,
                            target.Target.Expression
                        ),
                        Ast.Property(null, typeof(PythonOps).GetProperty("NotImplemented"))
                    ),
                    tmp
                );

                return true;
            } else {
                bodyBuilder.FinishCondition(target.Target.Expression);
                return false;
            }
        }

        private static void MakeSlotCall(BinderState/*!*/ state, MetaObject/*!*/[]/*!*/ types, ConditionalBuilder/*!*/ bodyBuilder, PythonTypeSlot/*!*/ slotTarget, bool reverse) {
            Debug.Assert(slotTarget != null);

            Expression self, other;
            if (reverse) {
                self = types[1].Expression;
                other = types[0].Expression;
            } else {
                self = types[0].Expression;
                other = types[1].Expression;
            }

            MakeSlotCallWorker(state, slotTarget, self, bodyBuilder, other);
        }

        private static void MakeSlotCallWorker(BinderState/*!*/ state, PythonTypeSlot/*!*/ slotTarget, Expression/*!*/ self, ConditionalBuilder/*!*/ bodyBuilder, params Expression/*!*/[]/*!*/ args) {
            // Generate:
            // 
            // SlotTryGetValue(context, slot, selfType, out callable) && (tmp=callable(args)) != NotImplemented) ?
            //      tmp :
            //      RestOfOperation
            //
            ParameterExpression callable = Ast.Variable(typeof(object), "slot");
            ParameterExpression tmp = Ast.Variable(typeof(object), "slot");

            bodyBuilder.AddCondition(
                Ast.AndAlso(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTryGetValue"),
                        Ast.Constant(state.Context),
                        AstUtils.Convert(Utils.WeakConstant(slotTarget), typeof(PythonTypeSlot)),
                        AstUtils.Convert(self, typeof(object)),
                        Ast.Call(
                            typeof(DynamicHelpers).GetMethod("GetPythonType"),
                            AstUtils.Convert(self, typeof(object))
                        ),
                        callable
                    ),
                    Ast.NotEqual(
                        Ast.Assign(
                            tmp,
                            Ast.Dynamic(
                                new PythonInvokeBinder(
                                    state,
                                    new CallSignature(args.Length)
                                ),
                                typeof(object),
                                ArrayUtils.Insert(Ast.Constant(state.Context), (Expression)callable, args)
                            )
                        ),
                        Ast.Property(null, typeof(PythonOps).GetProperty("NotImplemented"))
                    )
                ),
                tmp
            );
            bodyBuilder.AddVariable(callable);
            bodyBuilder.AddVariable(tmp);
        }

        private static void DoCoerce(OperationBinder/*!*/ operation, ConditionalBuilder/*!*/ bodyBuilder, string op, MetaObject/*!*/[]/*!*/ types, bool reverse) {
            DoCoerce(operation, bodyBuilder, op, types, reverse, delegate(Expression e) {
                return e;
            });
        }

        /// <summary>
        /// calls __coerce__ for old-style classes and performs the operation if the coercion is successful.
        /// </summary>
        private static void DoCoerce(OperationBinder/*!*/ operation, ConditionalBuilder/*!*/ bodyBuilder, string op, MetaObject/*!*/[]/*!*/ types, bool reverse, Func<Expression, Expression> returnTransform) {
            ParameterExpression coerceResult = Ast.Variable(typeof(object), "coerceResult");
            ParameterExpression coerceTuple = Ast.Variable(typeof(PythonTuple), "coerceTuple");

            if (!bodyBuilder.TestCoercionRecursionCheck) {
                // during coercion we need to enforce recursion limits if
                // they're enabled and the rule's test needs to reflect this.                
                bodyBuilder.Restrictions = bodyBuilder.Restrictions.Merge(
                    Restrictions.GetExpressionRestriction(
                        Ast.Equal(
                            Ast.Call(typeof(PythonOps).GetMethod("ShouldEnforceRecursion")),
                            Ast.Constant(PythonFunction.EnforceRecursion)
                        )
                    )
                );

                bodyBuilder.TestCoercionRecursionCheck = true;
            }

            // tmp = self.__coerce__(other)
            // if tmp != null && tmp != NotImplemented && (tuple = PythonOps.ValidateCoerceResult(tmp)) != null:
            //      return operation(tuple[0], tuple[1])                        
            SlotOrFunction slot = SlotOrFunction.GetSlotOrFunction(BinderState.GetBinderState(operation), Symbols.Coerce, types);

            if (slot.Success) {
                bodyBuilder.AddCondition(
                    Ast.AndAlso(
                        Ast.Not(
                            Ast.TypeIs(
                                Ast.Assign(
                                    coerceResult,
                                    slot.Target.Expression
                                ),
                                typeof(OldInstance)
                            )
                        ),
                        Ast.NotEqual(
                            Ast.Assign(
                                coerceTuple,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("ValidateCoerceResult"),
                                    coerceResult
                                )
                            ),
                            Ast.Constant(null)
                        )
                    ),
                    BindingHelpers.AddRecursionCheck(
                        returnTransform(
                            Ast.Dynamic(
                                new PythonOperationBinder(
                                    BinderState.GetBinderState(operation),
                                    DisallowCoerce + op
                                ),
                                typeof(object),
                                reverse ? CoerceTwo(coerceTuple) : CoerceOne(coerceTuple),
                                reverse ? CoerceOne(coerceTuple) : CoerceTwo(coerceTuple)
                            )
                        )
                    )
                );
                bodyBuilder.AddVariable(coerceResult);
                bodyBuilder.AddVariable(coerceTuple);
            }
        }

        private static MethodCallExpression/*!*/ CoerceTwo(ParameterExpression/*!*/ coerceTuple) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("GetCoerceResultTwo"),
                coerceTuple
            );
        }

        private static MethodCallExpression/*!*/ CoerceOne(ParameterExpression/*!*/ coerceTuple) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("GetCoerceResultOne"),
                coerceTuple
            );
        }


        #endregion

        #region Comparison Operations

        private static MetaObject/*!*/ MakeComparisonOperation(MetaObject/*!*/[]/*!*/ types, OperationBinder/*!*/ operation) {
            RestrictTypes(types);

            string op = NormalizeOperator(operation.Operation);
            if (op == StandardOperators.Compare) {
                return MakeSortComparisonRule(types, operation);
            }

            BinderState state = BinderState.GetBinderState(operation);
            Debug.Assert(types.Length == 2);
            MetaObject xType = types[0], yType = types[1];
            SymbolId opSym = Symbols.OperatorToSymbol(op);
            SymbolId ropSym = Symbols.OperatorToReversedSymbol(op);
            // reverse
            MetaObject[] rTypes = new MetaObject[] { types[1], types[0] };

            SlotOrFunction fop, rop, cmp, rcmp;
            fop = SlotOrFunction.GetSlotOrFunction(state, opSym, types);
            rop = SlotOrFunction.GetSlotOrFunction(state, ropSym, rTypes);
            cmp = SlotOrFunction.GetSlotOrFunction(state, Symbols.Cmp, types);
            rcmp = SlotOrFunction.GetSlotOrFunction(state, Symbols.Cmp, rTypes);

            ConditionalBuilder bodyBuilder = new ConditionalBuilder(operation);

            SlotOrFunction.GetCombinedTargets(fop, rop, out fop, out rop);
            SlotOrFunction.GetCombinedTargets(cmp, rcmp, out cmp, out rcmp);

            // first try __op__ or __rop__ and return the value
            if (MakeOneCompareGeneric(fop, false, types, MakeCompareReturn, bodyBuilder)) {
                if (MakeOneCompareGeneric(rop, true, types, MakeCompareReturn, bodyBuilder)) {

                    // then try __cmp__ or __rcmp__ and compare the resulting int appropriaetly
                    if (ShouldCoerce(operation, xType, yType, true)) {
                        DoCoerce(operation, bodyBuilder, StandardOperators.Compare, types, false, delegate(Expression e) {
                            return GetCompareTest(op, e, false);
                        });
                    }

                    if (MakeOneCompareGeneric(
                        cmp,
                        false,
                        types,
                        delegate(ConditionalBuilder builder, Expression retCond, Expression expr, bool reverse) {
                            MakeCompareTest(op, builder, retCond, expr, reverse);
                        },
                        bodyBuilder)) {

                        if (ShouldCoerce(operation, yType, xType, true)) {
                            DoCoerce(operation, bodyBuilder, StandardOperators.Compare, rTypes, true, delegate(Expression e) {
                                return GetCompareTest(op, e, true);
                            });
                        }

                        if (MakeOneCompareGeneric(
                            rcmp,
                            true,
                            types,
                            delegate(ConditionalBuilder builder, Expression retCond, Expression expr, bool reverse) {
                                MakeCompareTest(op, builder, retCond, expr, reverse);
                            },
                            bodyBuilder)) {
                            bodyBuilder.FinishCondition(MakeFallbackCompare(op, types));
                        }
                    }
                }
            }

            return bodyBuilder.GetMetaObject(types);
        }

        /// <summary>
        /// Makes the comparison rule which returns an int (-1, 0, 1).  TODO: Better name?
        /// </summary>
        private static MetaObject/*!*/ MakeSortComparisonRule(MetaObject/*!*/[]/*!*/ types, OperationBinder/*!*/ operation) {
            MetaObject fastPath = FastPathCompare(types);
            if (fastPath != null) {
                return fastPath;
            }

            string op = operation.Operation;

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
            MetaObject[] rTypes = new MetaObject[] { types[1], types[0] };
            SlotOrFunction cfunc, rcfunc, eqfunc, reqfunc, ltfunc, gtfunc, rltfunc, rgtfunc;

            BinderState state = BinderState.GetBinderState(operation);
            cfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.Cmp, types);
            rcfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.Cmp, rTypes);
            eqfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorEquals, types);
            reqfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorEquals, rTypes);
            ltfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorLessThan, types);
            gtfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorGreaterThan, types);
            rltfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorLessThan, rTypes);
            rgtfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorGreaterThan, rTypes);

            // inspect forward and reverse versions so we can pick one or both.
            SlotOrFunction cTarget, rcTarget, eqTarget, reqTarget, ltTarget, rgtTarget, gtTarget, rltTarget;
            SlotOrFunction.GetCombinedTargets(cfunc, rcfunc, out cTarget, out rcTarget);
            SlotOrFunction.GetCombinedTargets(eqfunc, reqfunc, out eqTarget, out reqTarget);
            SlotOrFunction.GetCombinedTargets(ltfunc, rgtfunc, out ltTarget, out rgtTarget);
            SlotOrFunction.GetCombinedTargets(gtfunc, rltfunc, out gtTarget, out rltTarget);

            PythonType xType = MetaPythonObject.GetPythonType(types[0]);
            PythonType yType = MetaPythonObject.GetPythonType(types[1]);

            // now build the rule from the targets.
            // bail if we're comparing to null and the rhs can't do anything special...
            if (xType.IsNull) {
                if (yType.IsNull) {
                    return new MetaObject(
                        Ast.Constant(0),
                        Restrictions.Combine(types)
                    );
                } else if (yType.UnderlyingSystemType.IsPrimitive || yType.UnderlyingSystemType == typeof(Microsoft.Scripting.Math.BigInteger)) {
                    return new MetaObject(
                        Ast.Constant(-1),
                        Restrictions.Combine(types)
                    );
                }
            }

            ConditionalBuilder bodyBuilder = new ConditionalBuilder(operation);

            bool tryRich = true, more = true;
            if (xType == yType && cTarget != SlotOrFunction.Empty) {
                // if the types are equal try __cmp__ first
                if (ShouldCoerce(operation, types[0], types[1], true)) {
                    // need to try __coerce__ first.
                    DoCoerce(operation, bodyBuilder, StandardOperators.Compare, types, false);
                }

                more = more && MakeOneCompareGeneric(cTarget, false, types, MakeCompareReverse, bodyBuilder);

                if (xType != TypeCache.OldInstance) {
                    // try __cmp__ backwards for new-style classes and don't fallback to
                    // rich comparisons if available
                    more = more && MakeOneCompareGeneric(rcTarget, true, types, MakeCompareReverse, bodyBuilder);
                    tryRich = false;
                }
            }

            if (tryRich && more) {
                // try the >, <, ==, !=, >=, <=.  These don't get short circuited using the more logic
                // because they don't give a definitive answer even if they return bool.  Only if they
                // return true do we know to return 0, -1, or 1.
                // try eq
                MakeOneCompareGeneric(eqTarget, false, types, MakeCompareToZero, bodyBuilder);
                MakeOneCompareGeneric(reqTarget, true, types, MakeCompareToZero, bodyBuilder);

                // try less than & reverse
                MakeOneCompareGeneric(ltTarget, false, types, MakeCompareToNegativeOne, bodyBuilder);
                MakeOneCompareGeneric(rgtTarget, true, types, MakeCompareToNegativeOne, bodyBuilder);

                // try greater than & reverse
                MakeOneCompareGeneric(gtTarget, false, types, MakeCompareToOne, bodyBuilder);
                MakeOneCompareGeneric(rltTarget, true, types, MakeCompareToOne, bodyBuilder);
            }

            if (xType != yType) {
                if (more && ShouldCoerce(operation, types[0], types[1], true)) {
                    // need to try __coerce__ first.
                    DoCoerce(operation, bodyBuilder, StandardOperators.Compare, types, false);
                }

                more = more && MakeOneCompareGeneric(cTarget, false, types, MakeCompareReverse, bodyBuilder);

                if (more && ShouldCoerce(operation, types[1], types[0], true)) {
                    // try __coerce__ first
                    DoCoerce(operation, bodyBuilder, StandardOperators.Compare, rTypes, true, delegate(Expression e) {
                        return ReverseCompareValue(e);
                    });
                }

                more = more && MakeOneCompareGeneric(rcTarget, true, types, MakeCompareReverse, bodyBuilder);
            }

            if (more) {
                // fall back to compare types
                bodyBuilder.FinishCondition(MakeFallbackCompare(op, types));
            }

            return bodyBuilder.GetMetaObject(types);
        }

        private static MetaObject FastPathCompare(MetaObject/*!*/[] types) {
            if (types[0].LimitType == types[1].LimitType) {
                // fast paths for comparing some types which don't define __cmp__
                if (types[0].LimitType == typeof(List)) {
                    types[0] = types[0].Restrict(typeof(List));
                    types[1] = types[1].Restrict(typeof(List));

                    return new MetaObject(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CompareLists"),
                            types[0].Expression,
                            types[1].Expression
                        ),
                        Restrictions.Combine(types)
                    );
                } else if (types[0].LimitType == typeof(PythonTuple)) {
                    types[0] = types[0].Restrict(typeof(PythonTuple));
                    types[1] = types[1].Restrict(typeof(PythonTuple));

                    return new MetaObject(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CompareTuples"),
                            types[0].Expression,
                            types[1].Expression
                        ),
                        Restrictions.Combine(types)
                    );
                } else if (types[0].LimitType == typeof(double)) {
                    types[0] = types[0].Restrict(typeof(double));
                    types[1] = types[1].Restrict(typeof(double));

                    return new MetaObject(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CompareFloats"),
                            types[0].Expression,
                            types[1].Expression
                        ),
                        Restrictions.Combine(types)
                    );
                }
            }
            return null;
        }

        private static void MakeCompareToZero(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ expr, bool reverse) {
            MakeValueCheck(0, expr, bodyBuilder, retCondition);
        }

        private static void MakeCompareToOne(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ expr, bool reverse) {
            MakeValueCheck(1, expr, bodyBuilder, retCondition);
        }

        private static void MakeCompareToNegativeOne(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ expr, bool reverse) {
            MakeValueCheck(-1, expr, bodyBuilder, retCondition);
        }

        private static void MakeValueCheck(int val, Expression retValue, ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition) {
            if (retValue.Type != typeof(bool)) {
                retValue = Ast.Dynamic(
                    new ConversionBinder(
                        BinderState.GetBinderState(bodyBuilder.Action),
                        typeof(bool),
                        ConversionResultKind.ExplicitCast
                    ),
                    typeof(bool),
                    retValue
                );
            }
            if (retCondition != null) {
                retValue = Ast.AndAlso(retCondition, retValue);
            }

            bodyBuilder.AddCondition(
                retValue,
                Ast.Constant(val)
            );
        }

        private static BinaryExpression/*!*/ ReverseCompareValue(Expression/*!*/ retVal) {
            return Ast.Multiply(
                AstUtils.Convert(
                    retVal,
                    typeof(int)
                ),
                Ast.Constant(-1)
            );
        }

        private static void MakeCompareReverse(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ expr, bool reverse) {
            Expression res = expr;
            if (reverse) {
                res = ReverseCompareValue(expr);
            }

            MakeCompareReturn(bodyBuilder, retCondition, res, reverse);
        }

        private static void MakeCompareTest(string op, ConditionalBuilder/*!*/ bodyBuilder, Expression retCond, Expression/*!*/ expr, bool reverse) {
            MakeCompareReturn(bodyBuilder, retCond, GetCompareTest(op, expr, reverse), reverse);
        }

        private static Expression/*!*/ MakeFallbackCompare(string op, MetaObject[] types) {
            return Ast.Call(
                GetComparisonFallbackMethod(op),
                AstUtils.Convert(types[0].Expression, typeof(object)),
                AstUtils.Convert(types[1].Expression, typeof(object))
            );
        }

        private static Expression GetCompareTest(string op, Expression expr, bool reverse) {
            if (expr.Type == typeof(int)) {
                // fast path, just do a compare in IL
                return GetCompareNode(op, reverse, expr);
            } else {
                return GetCompareExpression(
                    op,
                    reverse,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("CompareToZero"),
                        AstUtils.Convert(expr, typeof(object))
                    )
                );
            }
        }

        #endregion

        #region Index Operations

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
        private static MetaObject/*!*/ MakeIndexerOperation(OperationBinder/*!*/ operation, MetaObject/*!*/[]/*!*/ types) {
            SymbolId item, slice;
            MetaObject indexedType = types[0].Restrict(types[0].LimitType);
            BuiltinFunction itemFunc = null;
            PythonTypeSlot itemSlot = null;
            bool callSlice = false;
            int mandatoryArgs;
            string op = operation.Operation;

            GetIndexOperators(op, out item, out slice, out mandatoryArgs);

            if (types.Length == mandatoryArgs + 1 && IsSlice(op) && HasOnlyNumericTypes(operation, types, op == StandardOperators.SetSlice)) {
                // two slice indexes, all int arguments, need to call __*slice__ if it exists
                callSlice = BindingHelpers.TryGetStaticFunction(BinderState.GetBinderState(operation), slice, indexedType, out itemFunc);
                if (itemFunc == null || !callSlice) {
                    callSlice = MetaPythonObject.GetPythonType(indexedType).TryResolveSlot(BinderState.GetBinderState(operation).Context, slice, out itemSlot);
                }
            }

            if (!callSlice) {
                // 1 slice index (simple index) or multiple slice indexes or no __*slice__, call __*item__, 
                if (!BindingHelpers.TryGetStaticFunction(BinderState.GetBinderState(operation), item, indexedType, out itemFunc)) {
                    MetaPythonObject.GetPythonType(indexedType).TryResolveSlot(BinderState.GetBinderState(operation).Context, item, out itemSlot);
                }
            }

            // make the Callable object which does the actual call to the function or slot
            Callable callable = Callable.MakeCallable(BinderState.GetBinderState(operation), op, itemFunc, itemSlot);
            if (callable == null) {
                return TypeError(operation, "'{0}' object is unsubscriptable", indexedType);
            }

            // prepare the arguments and make the builder which will
            // call __*slice__ or __*item__
            MetaObject[] args;
            IndexBuilder builder;
            if (callSlice) {
                // we're going to call a __*slice__ method, we pass the args as is.
                Debug.Assert(IsSlice(op));

                builder = new SliceBuilder(types, callable);
                args = ConvertArgs(types);
            } else {
                // we're going to call a __*item__ method.
                builder = new ItemBuilder(types, callable);
                if (IsSlice(op)) {
                    // we need to create a new Slice object.
                    args = GetItemSliceArguments(op, types);
                } else {
                    // we just need to pass the arguments as they are
                    args = ConvertArgs(types);
                }
            }

            return builder.MakeRule(BinderState.GetBinderState(operation), args);
        }

        /// <summary>
        /// Helper to convert all of the arguments to their known types.
        /// </summary>
        private static MetaObject/*!*/[]/*!*/ ConvertArgs(MetaObject/*!*/[]/*!*/ types) {
            MetaObject[] res = new MetaObject[types.Length];
            for (int i = 0; i < types.Length; i++) {
                res[i] = types[i].Restrict(types[i].LimitType);
            }
            return res;
        }

        /// <summary>
        /// Gets the arguments that need to be provided to __*item__ when we need to pass a slice object.
        /// </summary>
        private static MetaObject/*!*/[]/*!*/ GetItemSliceArguments(string op, MetaObject/*!*/[]/*!*/ types) {
            MetaObject[] args;
            if (op == StandardOperators.SetSlice) {
                args = new MetaObject[] { 
                    types[0].Restrict(types[0].LimitType),
                    GetSetSlice(types), 
                    types[types.Length- 1].Restrict(types[types.Length - 1].LimitType)
                };
            } else {
                Debug.Assert(op == StandardOperators.GetSlice || op == StandardOperators.DeleteSlice);

                args = new MetaObject[] { 
                    types[0].Restrict(types[0].LimitType),
                    GetGetOrDeleteSlice(types)
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
            private readonly BinderState/*!*/ _binder;
            private readonly string _op;

            protected Callable(BinderState/*!*/ binder, string op) {
                Assert.NotNull(binder);

                _binder = binder;
                _op = op;
            }

            /// <summary>
            /// Creates a new CallableObject.  If BuiltinFunction is available we'll create a BuiltinCallable otherwise
            /// we create a SlotCallable.
            /// </summary>
            public static Callable MakeCallable(BinderState/*!*/ binder, string op, BuiltinFunction itemFunc, PythonTypeSlot itemSlot) {
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
            public virtual MetaObject[] GetTupleArguments(MetaObject[] arguments) {
                if (IsSetter) {
                    if (arguments.Length == 3) {
                        // simple setter, no extended slicing, no need to pack arguments into tuple
                        return arguments;
                    }

                    // we want self, (tuple, of, args, ...), value
                    Expression[] tupleArgs = new Expression[arguments.Length - 2];
                    for (int i = 1; i < arguments.Length - 1; i++) {
                        tupleArgs[i - 1] = AstUtils.Convert(arguments[i].Expression, typeof(object));
                    }
                    return new MetaObject[] {
                        arguments[0],
                        new MetaObject(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("MakeTuple"),
                                Ast.NewArrayInit(typeof(object), tupleArgs)
                            ),
                            Restrictions.Empty
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
                        tupleArgs[i - 1] = AstUtils.Convert(arguments[i].Expression, typeof(object));
                    }
                    return new MetaObject[] {
                        arguments[0],
                        new MetaObject(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("MakeTuple"),
                                Ast.NewArrayInit(typeof(object), tupleArgs)
                            ),
                            Restrictions.Empty
                        )
                    };
                }
            }

            /// <summary>
            /// Adds the target of the call to the rule.
            /// </summary>
            public abstract MetaObject/*!*/ CompleteRuleTarget(MetaObject[] args, Func<MetaObject> customFailure);

            protected PythonBinder Binder {
                get { return _binder.Binder; }
            }

            protected BinderState BinderState {
                get { return _binder; }
            }

            protected string Operator {
                get { return _op; }
            }

            protected bool IsSetter {
                get { return _op == StandardOperators.SetItem || _op == StandardOperators.SetSlice; }
            }
        }

        /// <summary>
        /// Subclass of Callable for a built-in function.  This calls a .NET method performing
        /// the appropriate bindings.
        /// </summary>
        class BuiltinCallable : Callable {
            private readonly BuiltinFunction/*!*/ _bf;

            public BuiltinCallable(BinderState/*!*/ binder, string op, BuiltinFunction/*!*/ func)
                : base(binder, op) {
                Assert.NotNull(func);

                _bf = func;
            }

            public override MetaObject[] GetTupleArguments(MetaObject[] arguments) {
                if (arguments[0].LimitType == typeof(OldInstance)) {
                    // old instances are special in that they take only a single parameter
                    // in their indexer but accept multiple parameters as tuples.
                    return base.GetTupleArguments(arguments);
                }
                return arguments;
            }

            public override MetaObject/*!*/ CompleteRuleTarget(MetaObject/*!*/[]/*!*/ args, Func<MetaObject> customFailure) {
                Assert.NotNull(args);
                Assert.NotNullItems(args);

                BindingTarget target;
                
                MetaObject res = Binder.CallInstanceMethod(
                    new ParameterBinderWithCodeContext(Binder, Ast.Constant(BinderState.Context)),
                    _bf.Targets,
                    args[0],
                    ArrayUtils.RemoveFirst(args),
                    new CallSignature(args.Length - 1),
                    Restrictions.Combine(args),
                    PythonNarrowing.None,
                    PythonNarrowing.IndexOperator,
                    _bf.Name,
                    out target
                );

                if (target.Success) {
                    if (IsSetter) {
                        res = new MetaObject(
                            Ast.Block(res.Expression, args[args.Length - 1].Expression),
                            res.Restrictions
                        );
                    }
                } else if (customFailure == null || (res = customFailure()) == null) {
                    res = DefaultBinder.MakeError(Binder.MakeInvalidParametersError(target), Restrictions.Combine(args));
                }

                return res;
            }
        }

        /// <summary>
        /// Callable to a user-defined callable object.  This could be a Python function,
        /// a class defining __call__, etc...
        /// </summary>
        class SlotCallable : Callable {
            private PythonTypeSlot _slot;

            public SlotCallable(BinderState/*!*/ binder, string op, PythonTypeSlot slot)
                : base(binder, op) {
                _slot = slot;
            }

            public override MetaObject/*!*/ CompleteRuleTarget(MetaObject/*!*/[]/*!*/ args, Func<MetaObject> customFailure) {
                Expression callable = _slot.MakeGetExpression(
                    Binder,
                    Ast.Constant(BinderState.Context),
                    args[0].Expression,
                    Ast.Call(
                        typeof(DynamicHelpers).GetMethod("GetPythonType"),
                        AstUtils.Convert(args[0].Expression, typeof(object))
                    ),
                    Ast.Throw(Ast.New(typeof(InvalidOperationException)))
                );

                Expression[] exprArgs = new Expression[args.Length - 1];
                for (int i = 1; i < args.Length; i++) {
                    exprArgs[i - 1] = args[i].Expression;
                }

                Expression retVal = Ast.Dynamic(
                    new PythonInvokeBinder(
                        BinderState,
                        new CallSignature(exprArgs.Length)
                    ),
                    typeof(object),
                    ArrayUtils.Insert(Ast.Constant(BinderState.Context), (Expression)callable, exprArgs)
                );

                if (IsSetter) {
                    retVal = Ast.Block(retVal, args[args.Length - 1].Expression);
                }

                return new MetaObject(
                    retVal,
                    Restrictions.Combine(args)
                );
            }
        }

        /// <summary>
        /// Base class for building a __*item__ or __*slice__ call.
        /// </summary>
        abstract class IndexBuilder {
            private readonly Callable/*!*/ _callable;
            private readonly MetaObject/*!*/[]/*!*/ _types;

            public IndexBuilder(MetaObject/*!*/[]/*!*/ types, Callable/*!*/ callable) {
                _callable = callable;
                _types = types;
            }

            public abstract MetaObject/*!*/ MakeRule(BinderState/*!*/ binder, MetaObject/*!*/[]/*!*/ args);

            protected Callable/*!*/ Callable {
                get { return _callable; }
            }

            protected MetaObject/*!*/[]/*!*/ Types {
                get { return _types; }
            }

            protected PythonType/*!*/ GetTypeAt(int index) {
                return MetaPythonObject.GetPythonType(_types[index]);
            }
        }

        /// <summary>
        /// Derived IndexBuilder for calling __*slice__ methods
        /// </summary>
        class SliceBuilder : IndexBuilder {
            private ParameterExpression _lengthVar;        // Nullable<int>, assigned if we need to calculate the length of the object during the call.

            public SliceBuilder(MetaObject/*!*/[]/*!*/ types, Callable/*!*/ callable)
                : base(types, callable) {
            }

            public override MetaObject/*!*/ MakeRule(BinderState/*!*/ binder, MetaObject/*!*/[]/*!*/ args) {
                // the semantics of simple slicing state that if the value
                // is less than 0 then the length is added to it.  The default
                // for unprovided parameters are 0 and maxint.  The callee
                // is responsible for ignoring out of range values but slicing
                // is responsible for doing this initial transformation.

                Debug.Assert(args.Length > 2);  // index 1 and 2 should be our slice indexes, we might have another arg if we're a setter
                args = ArrayUtils.Copy(args);
                for (int i = 1; i < 3; i++) {
                    args[i] = args[i].Restrict(args[i].LimitType);

                    if (args[i].LimitType == typeof(MissingParameter)) {
                        switch (i) {
                            case 1: args[i] = new MetaObject(Ast.Constant(0), args[i].Restrictions); break;
                            case 2: args[i] = new MetaObject(Ast.Constant(Int32.MaxValue), args[i].Restrictions); break;
                        }
                    } else if (args[i].LimitType == typeof(int)) {
                        args[i] = MakeIntTest(args[0], args[i]);
                    } else if (args[i].LimitType.IsSubclassOf(typeof(Extensible<int>))) {
                        args[i] = MakeIntTest(
                            args[0],
                            new MetaObject(
                                Ast.Property(
                                    args[i].Expression,
                                    args[i].LimitType.GetProperty("Value")
                                ),
                                args[i].Restrictions
                            )
                        );
                    } else if (args[i].LimitType == typeof(BigInteger)) {
                        args[i] = MakeBigIntTest(args[0], args[i]);
                    } else if (args[i].LimitType.IsSubclassOf(typeof(Extensible<BigInteger>))) {
                        args[i] = MakeBigIntTest(args[0], new MetaObject(Ast.Property(args[i].Expression, args[i].LimitType.GetProperty("Value")), args[i].Restrictions));
                    } else if (args[i].LimitType == typeof(bool)) {
                        args[i] = new MetaObject(
                            Ast.Condition(args[i].Expression, Ast.Constant(1), Ast.Constant(0)),
                            args[i].Restrictions
                        );
                    } else {
                        // this type defines __index__, otherwise we'd have an ItemBuilder constructing a slice
                        args[i] = MakeIntTest(args[0],
                            new MetaObject(
                                Binders.Convert(
                                    binder,
                                    typeof(int),
                                    ConversionResultKind.ExplicitCast,
                                    Ast.Dynamic(
                                        new PythonInvokeBinder(
                                            binder,
                                            new CallSignature(0)
                                        ),
                                        typeof(object),
                                        Ast.Constant(binder.Context),
                                        Binders.Get(
                                            Ast.Constant(binder.Context),
                                            binder,
                                            typeof(object),
                                            "__index__",
                                            args[i].Expression
                                        )
                                    )
                                ),
                                args[i].Restrictions
                            )
                        );
                    }
                }

                if (_lengthVar != null) {
                    // we need the length which we should only calculate once, calculate and
                    // store it in a temporary.  Note we only calculate the length if we'll
                    MetaObject res = Callable.CompleteRuleTarget(args, null);

                    return new MetaObject(
                        Ast.Block(
                            new ParameterExpression[] { _lengthVar },
                            Ast.Assign(_lengthVar, Ast.Constant(null, _lengthVar.Type)),
                            res.Expression
                        ),
                        res.Restrictions
                    );
                }

                return Callable.CompleteRuleTarget(args, null);
            }

            private MetaObject/*!*/ MakeBigIntTest(MetaObject/*!*/ self, MetaObject/*!*/ bigInt) {
                EnsureLengthVariable();

                return new MetaObject(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("NormalizeBigInteger"),
                        self.Expression,
                        bigInt.Expression,
                        _lengthVar
                    ),
                    self.Restrictions.Merge(bigInt.Restrictions)
                );
            }

            private MetaObject/*!*/ MakeIntTest(MetaObject/*!*/ self, MetaObject/*!*/ intVal) {
                return new MetaObject(
                    Ast.Condition(
                        Ast.LessThan(intVal.Expression, Ast.Constant(0)),
                        Ast.Add(intVal.Expression, MakeGetLength(self)),
                        intVal.Expression
                    ),
                    self.Restrictions.Merge(intVal.Restrictions)
                );
            }

            private Expression/*!*/ MakeGetLength(MetaObject /*!*/ self) {
                EnsureLengthVariable();

                return Ast.Call(
                    typeof(PythonOps).GetMethod("GetLengthOnce"),
                    self.Expression,
                    _lengthVar
                );
            }

            private void EnsureLengthVariable() {
                if (_lengthVar == null) {
                    _lengthVar = Ast.Variable(typeof(Nullable<int>), "objLength");
                }
            }
        }

        /// <summary>
        /// Derived IndexBuilder for calling __*item__ methods.
        /// </summary>
        class ItemBuilder : IndexBuilder {
            public ItemBuilder(MetaObject/*!*/[]/*!*/ types, Callable/*!*/ callable)
                : base(types, callable) {
            }

            public override MetaObject/*!*/ MakeRule(BinderState/*!*/ binder, MetaObject/*!*/[]/*!*/ args) {
                MetaObject[] tupleArgs = Callable.GetTupleArguments(args);
                return Callable.CompleteRuleTarget(tupleArgs, delegate() {
                    PythonTypeSlot indexSlot;
                    if (args[1].LimitType != typeof(Slice) && GetTypeAt(1).TryResolveSlot(binder.Context, Symbols.Index, out indexSlot)) {
                        args[1] = new MetaObject(
                            Ast.Dynamic(
                                new PythonInvokeBinder(
                                    binder,
                                    new CallSignature(0)
                                ),
                                typeof(int),
                                Ast.Constant(binder.Context),
                                Binders.Get(
                                    Ast.Constant(binder.Context),
                                    binder,
                                    typeof(object),
                                    "__index__",
                                    args[1].Expression
                                )
                            ),
                            Restrictions.Empty
                        );

                        return Callable.CompleteRuleTarget(tupleArgs, null);
                    }
                    return null;
                });
            }
        }

        private static bool HasOnlyNumericTypes(MetaObjectBinder/*!*/ action, MetaObject/*!*/[]/*!*/ types, bool skipLast) {
            bool onlyNumeric = true;
            BinderState state = BinderState.GetBinderState(action);

            for (int i = 1; i < (skipLast ? types.Length - 1 : types.Length); i++) {
                MetaObject obj = types[i];
                if (!IsIndexType(state, obj)) {
                    onlyNumeric = false;
                    break;
                }
            }
            return onlyNumeric;
        }

        private static bool IsIndexType(BinderState/*!*/ state, MetaObject/*!*/ obj) {
            bool numeric = true;
            if (obj.LimitType != typeof(MissingParameter) &&
                !PythonOps.IsNumericType(obj.LimitType)) {

                PythonType curType = MetaPythonObject.GetPythonType(obj);
                PythonTypeSlot dummy;

                if (!curType.TryResolveSlot(state.Context, Symbols.Index, out dummy)) {
                    numeric = false;
                }
            }
            return numeric;
        }

        private static bool IsSlice(string op) {
            return op == StandardOperators.GetSlice || op == StandardOperators.SetSlice || op == StandardOperators.DeleteSlice;
        }

        /// <summary>
        /// Helper to get the symbols for __*item__ and __*slice__ based upon if we're doing
        /// a get/set/delete and the minimum number of arguments required for each of those.
        /// </summary>
        private static void GetIndexOperators(string op, out SymbolId item, out SymbolId slice, out int mandatoryArgs) {
            switch (op) {
                case StandardOperators.GetItem:
                case StandardOperators.GetSlice:
                    item = Symbols.GetItem;
                    slice = Symbols.GetSlice;
                    mandatoryArgs = 2;
                    return;
                case StandardOperators.SetItem:
                case StandardOperators.SetSlice:
                    item = Symbols.SetItem;
                    slice = Symbols.SetSlice;
                    mandatoryArgs = 3;
                    return;
                case StandardOperators.DeleteItem:
                case StandardOperators.DeleteSlice:
                    item = Symbols.DelItem;
                    slice = Symbols.DeleteSlice;
                    mandatoryArgs = 2;
                    return;
            }

            throw new InvalidOperationException();
        }

        private static MetaObject/*!*/ GetSetSlice(MetaObject/*!*/[]/*!*/ args) {
            return new MetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod("MakeSlice"),
                    AstUtils.Convert(GetSetParameter(args, 1), typeof(object)),
                    AstUtils.Convert(GetSetParameter(args, 2), typeof(object)),
                    AstUtils.Convert(GetSetParameter(args, 3), typeof(object))
                ),
                Restrictions.Combine(args)
            );
        }

        private static MetaObject/*!*/ GetGetOrDeleteSlice(MetaObject/*!*/[]/*!*/ args) {
            return new MetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod("MakeSlice"),
                    AstUtils.Convert(GetGetOrDeleteParameter(args, 1), typeof(object)),
                    AstUtils.Convert(GetGetOrDeleteParameter(args, 2), typeof(object)),
                    AstUtils.Convert(GetGetOrDeleteParameter(args, 3), typeof(object))
                ),
                Restrictions.Combine(args)
            );
        }

        private static Expression/*!*/ GetGetOrDeleteParameter(MetaObject/*!*/[]/*!*/ args, int index) {
            if (args.Length > index) {
                return CheckMissing(args[index].Expression);
            }
            return Ast.Constant(null);
        }

        private static Expression GetSetParameter(MetaObject[] args, int index) {
            if (args.Length > (index + 1)) {
                return CheckMissing(args[index].Expression);
            }

            return Ast.Constant(null);
        }


        #endregion

        #region Helpers

        /// <summary>
        /// Checks if a coercion check should be performed.  We perform coercion under the following
        /// situations:
        ///     1. Old instances performing a binary operator (excluding rich comparisons)
        ///     2. User-defined new instances calling __cmp__ but only if we wouldn't dispatch to a built-in __coerce__ on the parent type
        ///     
        /// This matches the behavior of CPython.
        /// </summary>
        /// <returns></returns>
        private static bool ShouldCoerce(OperationBinder/*!*/ operation, MetaObject/*!*/ x, MetaObject/*!*/ y, bool isCompare) {
            if (operation.Operation.StartsWith(DisallowCoerce)) {
                return false;
            }

            PythonType xType = MetaPythonObject.GetPythonType(x), yType = MetaPythonObject.GetPythonType(y);

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
                    if (xType.TryResolveSlot(BinderState.GetBinderState(operation).Context, Symbols.Coerce, out pts)) {
                        // don't call __coerce__ if it's declared on the base type
                        BuiltinMethodDescriptor bmd = pts as BuiltinMethodDescriptor;
                        if (bmd == null) return true;

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

        public static string DirectOperation(string op) {
            string res = CompilerHelpers.InPlaceOperatorToOperator(op);
            if (res != StandardOperators.None) return res;

            throw new InvalidOperationException();
        }

        private static bool IsUnary(string op) {
            op = NormalizeOperator(op);

            switch (op) {
                case StandardOperators.OnesComplement:
                case StandardOperators.Negate:
                case StandardOperators.Positive:
                case StandardOperators.AbsoluteValue:
                case StandardOperators.Not:

                // Added for COM support...
                case StandardOperators.Documentation:
                    return true;
            }
            return false;
        }

        private static string NormalizeOperator(string op) {
            if (op.StartsWith(DisallowCoerce)) {
                op = op.Substring(DisallowCoerce.Length);
            }
            return op;
        }

        private static bool IsComparision(string op) {
            return CompilerHelpers.IsComparisonOperator(NormalizeOperator(op));
        }

        private static bool IsInPlace(string op) {
            return CompilerHelpers.InPlaceOperatorToOperator(op) != StandardOperators.None;
        }

        private static Expression/*!*/ GetCompareNode(string op, bool reverse, Expression expr) {
            op = NormalizeOperator(op);

            switch (reverse ? CompilerHelpers.OperatorToReverseOperator(op) : op) {
                case StandardOperators.Equal: return Ast.Equal(expr, Ast.Constant(0));
                case StandardOperators.NotEqual: return Ast.NotEqual(expr, Ast.Constant(0));
                case StandardOperators.GreaterThan: return Ast.GreaterThan(expr, Ast.Constant(0));
                case StandardOperators.GreaterThanOrEqual: return Ast.GreaterThanOrEqual(expr, Ast.Constant(0));
                case StandardOperators.LessThan: return Ast.LessThan(expr, Ast.Constant(0));
                case StandardOperators.LessThanOrEqual: return Ast.LessThanOrEqual(expr, Ast.Constant(0));
                default: throw new InvalidOperationException();
            }
        }

        private static Expression/*!*/ GetCompareExpression(string op, bool reverse, Expression/*!*/ value) {
            op = NormalizeOperator(op);

            Debug.Assert(value.Type == typeof(int));

            Expression zero = Ast.Constant(0);
            switch (reverse ? CompilerHelpers.OperatorToReverseOperator(op) : op) {
                case StandardOperators.Equal: return Ast.Equal(value, zero);
                case StandardOperators.NotEqual: return Ast.NotEqual(value, zero);
                case StandardOperators.GreaterThan: return Ast.GreaterThan(value, zero); ;
                case StandardOperators.GreaterThanOrEqual: return Ast.GreaterThanOrEqual(value, zero);
                case StandardOperators.LessThan: return Ast.LessThan(value, zero);
                case StandardOperators.LessThanOrEqual: return Ast.LessThanOrEqual(value, zero);
                default: throw new InvalidOperationException();
            }
        }

        private static MethodInfo/*!*/ GetComparisonFallbackMethod(string op) {
            op = NormalizeOperator(op);

            string name;
            switch (op) {
                case StandardOperators.Equal: name = "CompareTypesEqual"; break;
                case StandardOperators.NotEqual: name = "CompareTypesNotEqual"; break;
                case StandardOperators.GreaterThan: name = "CompareTypesGreaterThan"; break;
                case StandardOperators.LessThan: name = "CompareTypesLessThan"; break;
                case StandardOperators.GreaterThanOrEqual: name = "CompareTypesGreaterThanOrEqual"; break;
                case StandardOperators.LessThanOrEqual: name = "CompareTypesLessThanOrEqual"; break;
                case StandardOperators.Compare: name = "CompareTypes"; break;
                default: throw new InvalidOperationException();
            }
            return typeof(PythonOps).GetMethod(name);
        }

        internal static Expression/*!*/ CheckMissing(Expression/*!*/ toCheck) {
            if (toCheck.Type == typeof(MissingParameter)) {
                return Ast.Constant(null);
            }
            if (toCheck.Type != typeof(object)) {
                return toCheck;
            }

            return Ast.Condition(
                Ast.TypeIs(toCheck, typeof(MissingParameter)),
                Ast.Constant(null),
                toCheck
            );
        }
        
        private static MetaObject/*!*/ MakeRuleForNoMatch(OperationBinder/*!*/ operation, string/*!*/ op, params MetaObject/*!*/[]/*!*/ types) {
            // we get the error message w/ {0}, {1} so that TypeError formats it correctly
            return TypeError(
                   operation,
                   MakeBinaryOpErrorMessage(op, "{0}", "{1}"),
                   types);
        }

        internal static string/*!*/ MakeUnaryOpErrorMessage(string/*!*/ op, string/*!*/ xType) {
            return string.Format("unsupported operand type for {1}: '{0}'", xType, op);
        }


        internal static string/*!*/ MakeBinaryOpErrorMessage(string op, string/*!*/ xType, string/*!*/ yType) {
            return string.Format("unsupported operand type(s) for {2}: '{0}' and '{1}'",
                                xType, yType, GetOperatorDisplay(op));
        }

        private static string/*!*/ GetOperatorDisplay(string op) {
            op = NormalizeOperator(op);

            switch (op) {
                case StandardOperators.Add: return "+";
                case StandardOperators.Subtract: return "-";
                case StandardOperators.Power: return "**";
                case StandardOperators.Multiply: return "*";
                case StandardOperators.FloorDivide: return "/";
                case StandardOperators.Divide: return "/";
                case StandardOperators.TrueDivide: return "//";
                case StandardOperators.Mod: return "%";
                case StandardOperators.LeftShift: return "<<";
                case StandardOperators.RightShift: return ">>";
                case StandardOperators.BitwiseAnd: return "&";
                case StandardOperators.BitwiseOr: return "|";
                case StandardOperators.ExclusiveOr: return "^";
                case StandardOperators.LessThan: return "<";
                case StandardOperators.GreaterThan: return ">";
                case StandardOperators.LessThanOrEqual: return "<=";
                case StandardOperators.GreaterThanOrEqual: return ">=";
                case StandardOperators.Equal: return "==";
                case StandardOperators.NotEqual: return "!=";
                case StandardOperators.LessThanGreaterThan: return "<>";
                case StandardOperators.InPlaceAdd: return "+=";
                case StandardOperators.InPlaceSubtract: return "-=";
                case StandardOperators.InPlacePower: return "**=";
                case StandardOperators.InPlaceMultiply: return "*=";
                case StandardOperators.InPlaceFloorDivide: return "/=";
                case StandardOperators.InPlaceDivide: return "/=";
                case StandardOperators.InPlaceTrueDivide: return "//=";
                case StandardOperators.InPlaceMod: return "%=";
                case StandardOperators.InPlaceLeftShift: return "<<=";
                case StandardOperators.InPlaceRightShift: return ">>=";
                case StandardOperators.InPlaceBitwiseAnd: return "&=";
                case StandardOperators.InPlaceBitwiseOr: return "|=";
                case StandardOperators.InPlaceExclusiveOr: return "^=";
                case OperatorStrings.ReverseAdd: return "+";
                case OperatorStrings.ReverseSubtract: return "-";
                case OperatorStrings.ReversePower: return "**";
                case OperatorStrings.ReverseMultiply: return "*";
                case OperatorStrings.ReverseFloorDivide: return "/";
                case OperatorStrings.ReverseDivide: return "/";
                case OperatorStrings.ReverseTrueDivide: return "//";
                case OperatorStrings.ReverseMod: return "%";
                case OperatorStrings.ReverseLeftShift: return "<<";
                case OperatorStrings.ReverseRightShift: return ">>";
                case OperatorStrings.ReverseBitwiseAnd: return "&";
                case OperatorStrings.ReverseBitwiseOr: return "|";
                case OperatorStrings.ReverseExclusiveOr: return "^";
                default: return op.ToString();
            }
        }

        private static MetaObject/*!*/ MakeBinaryThrow(OperationBinder/*!*/action, string/*!*/ op, MetaObject/*!*/[]/*!*/ args) {
            if (action is IPythonSite) {
                // produce the custom Python error message
                return new MetaObject(
                    Ast.Throw(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("TypeErrorForBinaryOp"),
                            Ast.Constant(SymbolTable.IdToString(Symbols.OperatorToSymbol(NormalizeOperator(op)))),
                            AstUtils.Convert(args[0].Expression, typeof(object)),
                            AstUtils.Convert(args[1].Expression, typeof(object))
                        )
                    ),
                    Restrictions.Combine(args)
                );
            }

            // let the site produce its own error
            return action.FallbackOperation(args[0], new[] { args[1] });
        }

        private static List<string/*!*/>/*!*/ GetMemberNames(CodeContext/*!*/ context, PythonType/*!*/ pt, object value) {
            List names = pt.GetMemberNames(context, value);
            List<string> strNames = new List<string>();
            foreach (object o in names) {
                string s = o as string;
                if (s != null) {
                    strNames.Add(s);
                }
            }
            return strNames;
        }

        #endregion

        /// <summary>
        /// Produces an error message for the provided message and type names.  The error message should contain
        /// string formatting characters ({0}, {1}, etc...) for each of the type names.
        /// </summary>
        public static MetaObject/*!*/ TypeError(OperationBinder/*!*/ action, string message, params MetaObject[] types) {
            if (action is IPythonSite) {
                // produce our custom errors for Python...
                Expression[] formatArgs = new Expression[types.Length + 1];
                for (int i = 1; i < formatArgs.Length; i++) {
                    formatArgs[i] = Ast.Constant(MetaPythonObject.GetPythonType(types[i - 1]).Name);
                }
                formatArgs[0] = Ast.Constant(message);
                Type[] typeArgs = CompilerHelpers.MakeRepeatedArray<Type>(typeof(object), types.Length + 1);
                typeArgs[0] = typeof(string);

                Expression error = Ast.Throw(
                    Ast.Call(
                        typeof(ScriptingRuntimeHelpers).GetMethod("SimpleTypeError"),
                        AstUtils.ComplexCallHelper(
                            typeof(String).GetMethod("Format", typeArgs),
                            formatArgs
                        )
                    )
                );

                return new MetaObject(
                    error,
                    Restrictions.Combine(types)
                );
            }

            return action.FallbackOperation(types[0], ArrayUtils.RemoveFirst(types));
        }
    }
}
