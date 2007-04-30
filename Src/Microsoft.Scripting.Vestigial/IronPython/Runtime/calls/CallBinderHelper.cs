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
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Internal.Generation;

// TODO: These dependencies should be moved to Microsoft.Scripting
using MethodCandidate = IronPython.Compiler.MethodCandidate;
using MethodBinder = IronPython.Compiler.MethodBinder;
using BinderType = IronPython.Compiler.BinderType;

namespace Microsoft.Scripting {
    /// <summary>
    /// Creates rules for performing method calls.  Currently supports calling built-in functions, built-in method descriptors (w/o 
    /// a bound value) and bound built-in method descriptors (w/ a bound value).
    /// 
    /// TODO: Support calling .NET DynamicType's system types, user defined functions and methods, and eventually user defined types.
    /// </summary>
    /// <typeparam name="T">The type of the dynamic site</typeparam>
    public class CallBinderHelper<T> : BinderHelper<T> {
        private ActionBinder _binder;
        
        public CallBinderHelper(ActionBinder binder) {
            _binder = binder;
        }

        public StandardRule<T> MakeRule(CodeContext context, CallAction action, object [] args) {
            IActionable ndo = args[0] as IActionable;
            if (ndo != null) {
                StandardRule<T> rule = ndo.GetRule<T>(action, context, args);
                if (rule != null) return rule;
            }

            return MakeNewRule(context, args[0], action, CompilerHelpers.ObjectTypes(args));
        }


        private StandardRule<T> MakeNewRule(CodeContext context, object target, CallAction action, DynamicType[] types) {
            if (action == CallAction.Simple) {
                BuiltinFunction bf = TryConvertToBuiltinFunction(target);
                if (target is BoundBuiltinFunction && ((BoundBuiltinFunction)target).Self is ISuperDynamicObject) {
                    // TODO: Only on virtual functions
                    return MakeDynamicCallRule(types, action);
                }

                if (bf != null && !bf.DeclaringType.UnderlyingSystemType.IsValueType) {
                    return MakeBuiltinFunctionRule(target, types, bf) ?? MakeDynamicCallRule(types, action);
                }

                //TODO delegates can be optimized in the next round
                //Delegate d = target as Delegate;
                //if (d != null) {
                //    return MakeDelegateRule(d, args);
                //}

                //TODO Issues with several different kinds of constructors to be resolved
                //DynamicType dt = target as DynamicType;
                DynamicType dt = target as DynamicType;
                if (dt != null) {
                    return MakeDynamicTypeRule(context, action, dt, types);
                }
            }
            return MakeDynamicCallRule(types, action);
        }

        private StandardRule<T> MakeDynamicCallRule(DynamicType[] types, CallAction action) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(types);
            rule.SetTarget(rule.MakeReturn(_binder, CallBinderHelper<T>.MakeDynamicTarget(rule, action)));
            return rule;
        }

        private StandardRule<T> MakeBuiltinFunctionRule(object target, DynamicType[] types, BuiltinFunction bf) {
            DynamicType[] argTypes = GetArgumentTypes(types);
            MethodCandidate cand = GetMethodCandidate(target, bf, argTypes);

            // currently we fall back to the fully dynamic case for all error messages
            if (cand != null &&
                cand.Target.Method.IsPublic &&
                CompilerHelpers.GetOutAndByRefParameterCount(cand.Target.Method) == 0) {

                StandardRule<T> rule = new StandardRule<T>();

                Expression test = MakeBuiltinFunctionTest(rule, target, bf.Id);
                if (argTypes.Length > 0) {
                    test = BinaryExpression.AndAlso(test, MakeTestForTypes(rule, argTypes, 0, bf));
                }
                rule.SetTest(test);

                Expression[] args = GetArgumentExpressions(rule, target);
                if (bf.IsReversedOperator) Swap(args);

                rule.SetTarget(rule.MakeReturn(
                    _binder,
                    cand.Target.MakeExpression(_binder, args)));

                return rule;
            }
            return null;
        }

        /*
        private StandardRule<T> MakeTypeError(BuiltinFunction bf, DynamicType[] types) {
            int min = Int32.MaxValue;
            int max = 0;
            for (int i = 0; i < bf.Targets.Length; i++) {
                int paramCnt = bf.Targets[i].GetParameters().Length;
                if (paramCnt < min) min = paramCnt;
                if (paramCnt > max) max = paramCnt;
            }
            if (min == max) {
                return StandardRule<T>.TypeError(String.Format("no overload matches (expected {0} args, target takes {1})", types.Length-1, min), types);
            } 
            return StandardRule<T>.TypeError(String.Format("no overload matches (expected {0} args, target takes at least {1} and at most {2})", types.Length-1, min, max), types);            
        }*/

        /// <summary>
        /// Gets the expressions which correspond to each parameter on the calling method.
        /// </summary>
        private Expression[] GetArgumentExpressions(StandardRule<T> rule, object target) {
            if (target is BuiltinFunction || target is BuiltinMethodDescriptor) {
                // static method call, for purposes of calling we only need the arguments provided
                VariableReference[] vars = new VariableReference[rule.Parameters.Length - 1];
                Array.Copy(rule.Parameters, 1, vars, 0, rule.Parameters.Length - 1);
                return VariableReference.ReferencesToExpressions(vars);
            } 

            // bound method call, the argument expressions include the instance from the bound function too
            Expression[] argParams = VariableReference.ReferencesToExpressions(rule.Parameters);
            argParams[0] = 
                MemberExpression.Property(
                    StaticUnaryExpression.Convert(rule.GetParameterExpression(0), typeof(BoundBuiltinFunction)),
                    typeof(BoundBuiltinFunction).GetProperty("Self"));

            return argParams;
        }

        private void Swap<ElemType>(ElemType[] array) {
            ElemType temp = array[array.Length - 1];
            array[array.Length - 1] = array[array.Length - 2];
            array[array.Length - 2] = temp;            
        }

        private MethodCandidate GetMethodCandidate(object target, BuiltinFunction bf, DynamicType[] argTypes) {
            MethodBinder binder = MethodBinder.MakeBinder(_binder, "__call__", bf.Targets, BinderType.Normal);
            BoundBuiltinFunction boundbf = target as BoundBuiltinFunction;
            if (boundbf == null) {
                return binder.MakeBindingTarget(CallType.None, argTypes);                
            }

            DynamicType[] types = new DynamicType[argTypes.Length + 1];
            types[0] = bf.DeclaringType;
            Array.Copy(argTypes, 0, types, 1, argTypes.Length);
            if (bf.IsReversedOperator) {
                Swap(types);
            }

            return binder.MakeBindingTarget(CallType.ImplicitInstance, types);            
        }

        /// <summary>
        /// Gets the test to see if the target is a built-in function.
        /// </summary>
        private Expression MakeBuiltinFunctionTest(StandardRule<T> rule, object target, int id) {
            Expression test = rule.MakeTypeTestExpression(target.GetType(), 0);

            Expression bfexpr = GetBuiltinFunctionFromTarget(rule, target);

            if (target is BoundBuiltinFunction) {
                // avoid ISuperDynamicObject due to super() calls
                test = BinaryExpression.AndAlso(test,
                       BinaryExpression.NotEqual(
                           TypeBinaryExpression.TypeIs(
                               MemberExpression.Property(
                                    StaticUnaryExpression.Convert(rule.GetParameterExpression(0), typeof(BoundBuiltinFunction)),
                                    typeof(BoundBuiltinFunction).GetProperty("Target")),
                               typeof(ISuperDynamicObject)),
                           new ConstantExpression(true)));
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
        private Expression MakeTestForTypes(StandardRule<T> rule, DynamicType[] types, int index, BuiltinFunction bf) {
            try {
                bool needTest = AreArgumentTypesOverloaded(types, index, bf);
                Expression test = needTest ? rule.MakeTypeTest(types[index], index + 1) : new ConstantExpression(true);

                if (index + 1 < types.Length) {
                    Expression nextTests = MakeTestForTypes(rule, types, index + 1, bf);
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

        private static bool AreArgumentTypesOverloaded(DynamicType[] types, int index, BuiltinFunction bf) {
            Type argType = null;
            for (int i = 0; i < bf.Targets.Length; i++) {
                ParameterInfo[] pis = bf.Targets[i].GetParameters();

                if (pis.Length == 0) continue;

                int readIndex = index;
                if (pis[0].ParameterType == typeof(CodeContext)) {
                    readIndex++;
                }

                Type curType;
                if (readIndex < pis.Length) {
                    if (CompilerHelpers.IsParamArray(pis[readIndex])) {
                        if(index == types.Length-1) {
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

        private static DynamicType[] GetArgumentTypes(DynamicType[] types) {
            DynamicType []argTypes = new DynamicType[types.Length - 1];
            Array.Copy(types, 1, argTypes, 0, types.Length - 1);
            return argTypes;
        }

        private static DynamicType[] GetReversedArgumentTypes(DynamicType[] types) {
            if (types.Length < 3) throw new InvalidOperationException("need 3 types or more for reversed operator");

            DynamicType[] argTypes = new DynamicType[types.Length - 1];
            Array.Copy(types, 1, argTypes, 0, types.Length - 1);
            DynamicType temp = argTypes[argTypes.Length - 1];
            argTypes[argTypes.Length - 1] = argTypes[argTypes.Length-2];
            argTypes[argTypes.Length - 2] = temp;
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
                if (action == CallAction.Simple) {
                    args.Add(Arg.Simple(rule.GetParameterExpression(i + 1)));
                } else if (action.ArgumentKinds[i].IsThis) {
                    instance = rule.GetParameterExpression(i+1);
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
        private StandardRule<T> MakeDynamicTypeRule(CodeContext context, CallAction action, DynamicType dt, DynamicType[] types) {
            if(action != CallAction.Simple) {
                return MakeDynamicCallRule(types, action);
            }
            if (dt.GetType() != typeof(DynamicType)) {
                return MakeDynamicCallRule(types, action);
            }
                
            StandardRule<T> rule = new StandardRule<T>();
            Expression[] exprs = VariableReference.ReferencesToExpressions(rule.Parameters);
            Expression[] finalExprs = new Expression[exprs.Length - 1];
            Array.Copy(exprs, 1, finalExprs, 0, exprs.Length-1);

            /*if (!dt.IsImmutable) {
                rule.SetTest(rule.MakeTypeTest(dt, 0));
            } else*/ {
                rule.MakeTest(new DynamicType[] { types[0] });
            }
            
            rule.SetTarget(
                rule.MakeReturn(
                    _binder,
                    MethodCallExpression.Call( 
                        null,
                        typeof(IronPython.Runtime.Operations.DynamicTypeOps).GetMethod("CallWorker", new Type[] { typeof(CodeContext), typeof(DynamicType), typeof(object[]) }),
                        new CodeContextExpression(),
                        StaticUnaryExpression.Convert(
                            rule.GetParameterExpression(0),
                            typeof(DynamicType)
                        ),
                        NewArrayExpression.NewArrayInit(typeof(object[]), finalExprs)
                    )
                )
            );

            return rule;                        
                    

        }
#if FALSE

        private StandardRule MakeDelegateRule(Delegate d, object[] args) {
            Type dt = d.GetType();
            MethodInfo mi = dt.GetMethod("Invoke");

            MethodBinder binder = MethodBinder.MakeBinder(_binder, dt.Name, new MethodInfo[] { mi }, BinderType.Normal);
            DynamicType[] types = new DynamicType[args.Length];
            for (int i = 0; i < types.Length; i++) {
                types[i] = Ops.GetDynamicType(args[i]);
            }
            StandardRule rule = binder.MakeBindingRule(CallType.ImplicitInstance, types);
            return rule;
        }

#endif
    }
}
