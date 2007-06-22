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
using System.Text;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Creates rules for performing method calls.  Currently supports calling built-in functions, built-in method descriptors (w/o 
    /// a bound value) and bound built-in method descriptors (w/ a bound value).
    /// 
    /// TODO: Support calling .NET DynamicType's system types, user defined functions and methods, and eventually user defined types.
    /// </summary>
    /// <typeparam name="T">The type of the dynamic site</typeparam>
    public class CallBinderHelper<T> : BinderHelper<T, CallAction> {

        public CallBinderHelper(CodeContext context, CallAction action)
            : base(context, action) {
        }

        public StandardRule<T> MakeRule(object[] args) {
            DynamicType[] types = CompilerHelpers.ObjectTypes(args);
            object target = args[0];

            if (Action.IsSimple || IsParamsCall) {
                BuiltinFunction bf = TryConvertToBuiltinFunction(target);
                BoundBuiltinFunction bbf = target as BoundBuiltinFunction;
                if (bbf != null) {
                    ISuperDynamicObject sdo = bbf.Self as ISuperDynamicObject;
                    if (!CanOptimizeUserCall(sdo, bbf)) {
                        return MakeDynamicCallRule(bbf, types);
                    }
                }                

                if (bf != null && (!bf.DeclaringType.UnderlyingSystemType.IsValueType || bf.DeclaringType.UnderlyingSystemType.IsPrimitive)) {
                    return MakeBuiltinFunctionRule(bf, args) ?? MakeDynamicCallRule(types);
                }

                //TODO delegates can be optimized in the next round
                //Delegate d = target as Delegate;
                //if (d != null) {
                //    return MakeDelegateRule(d, args);
                //}
            }
            
            return MakeDynamicCallRule(types);
        }

        private bool IsParamsCall {
            get {
                return IsParamsCallWorker(Action);
            }
        }

        private static bool CanOptimizeUserCall(ISuperDynamicObject sdo, BoundBuiltinFunction bbf) {
            if (sdo == null) return true;

            foreach (MethodBase mb in bbf.Target.Targets) {
                if (mb.IsVirtual) return false;
            }

            return true;
        }

        private StandardRule<T> MakeDynamicCallRule(DynamicType[] types) {
            return MakeDynamicCallRule(Action, Binder, types);
        }

        public static StandardRule<T> MakeDynamicCallRule(CallAction action, ActionBinder binder, DynamicType[] types) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(types);
            rule.SetTarget(rule.MakeReturn(binder, CallBinderHelper<T>.MakeDynamicTarget(rule, action)));
            return rule;
        }

        private StandardRule<T> MakeDynamicCallRule(BoundBuiltinFunction bbf, DynamicType[] types) {
            return MakeDynamicCallRule(Action, Binder, bbf, types);
        }

        public static StandardRule<T> MakeDynamicCallRule(CallAction action, ActionBinder binder, BoundBuiltinFunction bbf, DynamicType[] types) {
            StandardRule<T> rule = new StandardRule<T>();
            
            rule.SetTest(
                BinaryExpression.AndAlso(
                    rule.MakeTestForTypes(types, 0),
                    BinaryExpression.Equal(
                        MemberExpression.Property(
                            MemberExpression.Property(
                                StaticUnaryExpression.Convert(
                                    rule.GetParameterExpression(0),
                                    typeof(BoundBuiltinFunction)),
                                typeof(BoundBuiltinFunction).GetProperty("Target")),
                            typeof(BuiltinFunction).GetProperty("Id")),
                        new ConstantExpression(bbf.Target.Id))
                )
            );
            rule.SetTarget(rule.MakeReturn(binder, CallBinderHelper<T>.MakeDynamicTarget(rule, action)));
            return rule;
        }

        private StandardRule<T> MakeBuiltinFunctionRule(BuiltinFunction bf, object []args) {
            DynamicType[] argTypes = GetArgumentTypes(args);
            if (argTypes == null) return null;

            MethodCandidate cand = GetMethodCandidate(args[0], bf, argTypes);

            // currently we fall back to the fully dynamic case for all error messages
            if (cand != null &&
                cand.Target.Method.IsPublic &&
                CompilerHelpers.GetOutAndByRefParameterCount(cand.Target.Method) == 0 &&
                (!bf.IsBinaryOperator || cand.NarrowingLevel == NarrowingLevel.None)) {  // narrowing level on binary operator may fail during call...

                StandardRule<T> rule;
                object []attrs = cand.Target.Method.GetCustomAttributes(typeof(ActionOnCallAttribute), false);
                if (attrs.Length > 0) {
                    rule = ((ActionOnCallAttribute)attrs[0]).GetRule<T>(args);
                    if (rule != null) return rule;
                }
                
                rule = new StandardRule<T>();

                Expression test = MakeBuiltinFunctionTest(rule, args[0], bf.Id);
                Expression[] exprargs = GetArgumentExpressions(Action, rule, args);

                if (IsParamsCall) {
                    test = BinaryExpression.AndAlso(test, MakeParamsTest(rule, args));
                }
                if (argTypes.Length > 0) {
                    test = BinaryExpression.AndAlso(test, MakeTestForTypes(rule, exprargs, argTypes, 0, args[0] is BoundBuiltinFunction, bf));
                } 

                rule.SetTest(test);
                
                if (bf.IsReversedOperator) CompilerHelpers.SwapLastTwo(exprargs);

                rule.SetTarget(rule.MakeReturn(
                    Binder,
                    cand.Target.MakeExpression(Binder, exprargs)));

                return rule;
            }
            return null;
        }

        private MethodCandidate GetMethodCandidate(object target, BuiltinFunction bf, DynamicType[] argTypes) {
            MethodBinder binder = MethodBinder.MakeBinder(Binder, "__call__", bf.Targets, bf.IsBinaryOperator ? BinderType.BinaryOperator : BinderType.Normal);
            BoundBuiltinFunction boundbf = target as BoundBuiltinFunction;
            if (boundbf == null) {
                return binder.MakeBindingTarget(CallType.None, argTypes);
            }

            DynamicType[] types = new DynamicType[argTypes.Length + 1];
            types[0] = bf.DeclaringType;
            Array.Copy(argTypes, 0, types, 1, argTypes.Length);
            if (bf.IsReversedOperator) {
                CompilerHelpers.SwapLastTwo(types);
            }

            return binder.MakeBindingTarget(CallType.ImplicitInstance, types);
        }

        /// <summary>
        /// Gets the test to see if the target is a built-in function.
        /// </summary>
        private Expression MakeBuiltinFunctionTest(StandardRule<T> rule, object target, int id) {
            Expression test = rule.MakeTypeTestExpression(target.GetType(), 0);

            Expression bfexpr = GetBuiltinFunctionFromTarget(rule, target);

            BoundBuiltinFunction bbf = target as BoundBuiltinFunction;
            if (bbf != null) {
                if (bbf.Self is ISuperDynamicObject) {
                    test = BinaryExpression.AndAlso(test,
                             TypeBinaryExpression.TypeIs(
                                 MemberExpression.Property(
                                      StaticUnaryExpression.Convert(rule.GetParameterExpression(0), typeof(BoundBuiltinFunction)),
                                      typeof(BoundBuiltinFunction).GetProperty("Self")),
                                 typeof(ISuperDynamicObject))
                          );

                } else {
                    test = BinaryExpression.AndAlso(test,
                               BinaryExpression.NotEqual(
                                   TypeBinaryExpression.TypeIs(
                                       MemberExpression.Property(
                                            StaticUnaryExpression.Convert(rule.GetParameterExpression(0), typeof(BoundBuiltinFunction)),
                                            typeof(BoundBuiltinFunction).GetProperty("Self")),
                                       typeof(ISuperDynamicObject)),
                                   new ConstantExpression(true)));
                }
            }

            return BinaryExpression.AndAlso(test,
                BinaryExpression.Equal(
                    MemberExpression.Property(bfexpr, typeof(BuiltinFunction).GetProperty("Id")),
                    ConstantExpression.Constant(id)));
        }

        /// <summary>
        /// Extracts a built-in function from the target.
        /// </summary>
        private static Expression GetBuiltinFunctionFromTarget(StandardRule<T> rule, object target) {
            if (target is BuiltinFunction) {
                return StaticUnaryExpression.Convert(rule.GetParameterExpression(0), typeof(BuiltinFunction));
            }

            if (target is BoundBuiltinFunction) {
                return MemberExpression.Property(
                    StaticUnaryExpression.Convert(rule.GetParameterExpression(0), typeof(BoundBuiltinFunction)),
                    typeof(BoundBuiltinFunction).GetProperty("Target"));
            }

            Debug.Assert(target is BuiltinMethodDescriptor);

            return MemberExpression.Property(
                StaticUnaryExpression.Convert(rule.GetParameterExpression(0), typeof(BuiltinMethodDescriptor)),
                typeof(BuiltinMethodDescriptor).GetProperty("Template"));
        }

        /// <summary>
        /// Makes the set of tests for the types w/ the parameter adjected by 1 to remove the target
        /// </summary>
        private Expression MakeTestForTypes(StandardRule<T> rule, Expression[] exprArgs, DynamicType[] types, int index, bool boundFunc, BuiltinFunction bf) {
            try {
                bool needTest = AreArgumentTypesOverloaded(types, index, boundFunc, bf);
                Expression test = needTest ? rule.MakeTypeTest(types[index], exprArgs[index + (boundFunc ? 1 : 0)]) : new ConstantExpression(true);

                if (index + 1 < types.Length) {
                    Expression nextTests = MakeTestForTypes(rule, exprArgs, types, index + 1, boundFunc, bf);
                    if (test.IsConstant(true)) {
                        return nextTests;
                    } else if (nextTests.IsConstant(true)) {
                        return test;
                    } else {
                        return BinaryExpression.AndAlso(test, nextTests);
                    }
                }

                return test;
            } catch {
                Debug.Assert(false);
                throw;
            }
        }

        private static bool AreArgumentTypesOverloaded(DynamicType[] types, int index, bool boundFunc, BuiltinFunction bf) {
            // always need to check for binary operators due to not supporting NotImplemented on calls.  If we don't
            // check here we can skip a type check which is needed to avoid a cast failure on a call.
            if (bf.IsBinaryOperator) return true;

            Type argType = null;
            for (int i = 0; i < bf.Targets.Length; i++) {                
                ParameterInfo[] pis = bf.Targets[i].GetParameters();

                if (pis.Length == 0) continue;

                int readIndex = index + 
                    ((boundFunc && CompilerHelpers.IsStatic(bf.Targets[i])) ? 1 : 0) + 
                    ((!boundFunc && !CompilerHelpers.IsStatic(bf.Targets[i])) ? -1 : 0);
                if (pis[0].ParameterType == typeof(CodeContext)) {
                    readIndex++;
                }
                
                Type curType;
                if (readIndex < pis.Length) {
                    if (readIndex == -1) {
                        curType = bf.Targets[i].DeclaringType;
                    } else if (CompilerHelpers.IsParamArray(pis[readIndex])) {
                        if (index == types.Length - 1) {
                            return true;    // TODO: Optimize this case
                        }
                        curType = pis[pis.Length - 1].ParameterType.GetElementType();
                    } else {
                        curType = pis[readIndex].ParameterType;
                    }
                } else if (CompilerHelpers.IsParamArray(pis[pis.Length - 1])) {
                    curType = pis[pis.Length - 1].ParameterType.GetElementType();
                } else {
                    continue;
                }
                
                if (argType == null) {
                    argType = curType;
                } else if (argType != curType) {
                    return true;
                }
            }
            return false;
        }

        private DynamicType[] GetArgumentTypes(object[] args) {
            return GetArgumentTypes(Action, args);
        }

        private static DynamicType[] GetReversedArgumentTypes(DynamicType[] types) {
            if (types.Length < 3) throw new InvalidOperationException("need 3 types or more for reversed operator");

            DynamicType[] argTypes = CompilerHelpers.RemoveFirst(types);
            CompilerHelpers.SwapLastTwo(argTypes);
            return argTypes;
        }

        /// <summary>
        /// Makes a dynamic call rule.  Currently we just embed a normal CallExpression in which does the
        /// fully dynamic call.  Supports all action types.
        /// </summary>
        public static Expression MakeDynamicTarget(StandardRule<T> rule, CallAction action) {
            //Console.Error.WriteLine("Going dynamic: {0}", action);
            List<Arg> args = new List<Arg>();
            Expression instance = null;

            bool argsTuple = false, keywordDict = false;
            int kwCnt = 0, extraArgs = 0;
            for (int i = 0; i < rule.Parameters.Length - 1; i++) {
                if (action.IsSimple) {
                    args.Add(Arg.Simple(rule.GetParameterExpression(i + 1)));
                } else if (action.ArgumentKinds[i].IsThis) {
                    instance = rule.GetParameterExpression(i + 1);
                } else if (action.ArgumentKinds[i].ExpandDictionary) {
                    args.Add(Arg.Dictionary(rule.GetParameterExpression(i + 1)));
                    keywordDict = true; extraArgs++;
                } else if (action.ArgumentKinds[i].ExpandList) {
                    args.Add(Arg.List(rule.GetParameterExpression(i + 1)));
                    argsTuple = true; extraArgs++;
                } else if (action.ArgumentKinds[i].Name != SymbolId.Empty) {
                    args.Add(Arg.Named(action.ArgumentKinds[i].Name, rule.GetParameterExpression(i + 1)));
                    kwCnt++;
                } else {
                    args.Add(Arg.Simple(rule.GetParameterExpression(i + 1)));
                }
            }

            if (instance != null) {
                return new CallWithThisExpression(
                    rule.GetParameterExpression(0),
                    instance,
                    args.ToArray());
            }

            return new CallExpression(
                rule.GetParameterExpression(0),
                args.ToArray(),
                argsTuple,
                keywordDict,
                kwCnt,
                extraArgs
                );
        }

        
#if FALSE

        private StandardRule MakeDelegateRule(Delegate d, object[] args) {
            Type dt = d.GetType();
            MethodInfo mi = dt.GetMethod("Invoke");

            MethodBinder binder = MethodBinder.MakeBinder(_binder, dt.Name, new MethodInfo[] { mi }, BinderType.Normal);
            DynamicType[] types = new DynamicType[args.Length];
            for (int i = 0; i < types.Length; i++) {
                types[i] = DynamicHelpers.GetDynamicType(args[i]);
            }
            StandardRule rule = binder.MakeBindingRule(CallType.ImplicitInstance, types);
            return rule;
        }

#endif
    }
}
