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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;

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
            Debug.Assert(args != null && args.Length > 0);
            // args[0]: target
            // args[1..n]: arguments

            DynamicType[] types = CompilerHelpers.ObjectTypes(args);
            object target = args[0];

            if (CanGenerateRule) {
                BuiltinFunction bf = TryConvertToBuiltinFunction(target);

                if (bf != null) {
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

        private bool CanGenerateRule {
            get {
                return !Action.HasDictionaryArgument();
            }
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
                Ast.AndAlso(
                    rule.MakeTestForTypes(types, 0),
                    Ast.Equal(
                        Ast.ReadProperty(
                            Ast.ReadProperty(
                                Ast.Cast(
                                    rule.Parameters[0],
                                    typeof(BoundBuiltinFunction)),
                                typeof(BoundBuiltinFunction).GetProperty("Target")),
                            typeof(BuiltinFunction).GetProperty("Id")),
                        Ast.Constant(bbf.Target.Id))
                )
            );
            rule.SetTarget(rule.MakeReturn(binder, CallBinderHelper<T>.MakeDynamicTarget(rule, action)));
            return rule;
        }

        private StandardRule<T> MakeBuiltinFunctionRule(BuiltinFunction bf, object []args) {
            DynamicType[] argTypes = GetArgumentTypes(args);
            if (argTypes == null) return null;

            SymbolId[] argNames = Action.GetArgumentNames();

            MethodCandidate cand = GetMethodCandidate(args[0], bf, argTypes, argNames);

            // currently we fall back to the fully dynamic case for all error messages
            if (cand != null &&
                cand.Target.Method.IsPublic &&
                CompilerHelpers.GetOutAndByRefParameterCount(cand.Target.Method) == 0 &&
                (!bf.IsBinaryOperator || cand.NarrowingLevel == NarrowingLevel.None)) {  // narrowing level on binary operator may fail during call...

                StandardRule<T> rule;
                object []attrs = cand.Target.Method.GetCustomAttributes(typeof(ActionOnCallAttribute), false);
                if (attrs.Length > 0) {
                    rule = ((ActionOnCallAttribute)attrs[0]).GetRule<T>(Context, args);
                    if (rule != null) return rule;
                }
                
                rule = new StandardRule<T>();

                Expression test = MakeBuiltinFunctionTest(rule, args[0], bf.Id);
                Expression[] exprargs = GetArgumentExpressions(cand, Action, rule, args);

                if (Action.IsParamsCall()) {
                    test = Ast.AndAlso(test, MakeParamsTest(rule, args));
                }
                if (argTypes.Length > 0) {
                    test = Ast.AndAlso(test, MakeTestForTypes(rule, exprargs, argTypes, 0, args[0] is BoundBuiltinFunction, bf));
                } 

                rule.SetTest(test);
                
                if (bf.IsReversedOperator) Utils.Array.SwapLastTwo(exprargs);

                rule.SetTarget(rule.MakeReturn(
                    Binder,
                    cand.Target.MakeExpression(Binder, exprargs)));
                
                return rule;
            }
            return null;
        }


        private MethodCandidate GetMethodCandidate(object target, BuiltinFunction bf, DynamicType[] argTypes, SymbolId []argNames) {
            MethodBinder binder = MethodBinder.MakeBinder(Binder, "__call__", bf.Targets, bf.IsBinaryOperator ? BinderType.BinaryOperator : BinderType.Normal);
            BoundBuiltinFunction boundbf = target as BoundBuiltinFunction;
            if (boundbf == null) {
                return binder.MakeBindingTarget(CallType.None, CompilerHelpers.ConvertToTypes(argTypes), argNames);
            }

            DynamicType[] types = Utils.Array.Insert(DynamicHelpers.GetDynamicType(boundbf.Self), argTypes);
            if (bf.IsReversedOperator) {
                Utils.Array.SwapLastTwo(types);
                if (argNames.Length >= 2) {
                    Utils.Array.SwapLastTwo(argNames);
                }
            }

            return binder.MakeBindingTarget(CallType.ImplicitInstance, CompilerHelpers.ConvertToTypes(types), argNames);
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
                    test = Ast.AndAlso(test,
                             Ast.TypeIs(
                                 Ast.ReadProperty(
                                      Ast.Cast(rule.Parameters[0], typeof(BoundBuiltinFunction)),
                                      typeof(BoundBuiltinFunction).GetProperty("Self")),
                                 typeof(ISuperDynamicObject))
                          );

                } else {
                    test = Ast.AndAlso(test,
                               Ast.Equal(
                                   Ast.TypeIs(
                                       Ast.ReadProperty(
                                            Ast.Cast(rule.Parameters[0], typeof(BoundBuiltinFunction)),
                                            typeof(BoundBuiltinFunction).GetProperty("Self")),
                                       CompilerHelpers.GetVisibleType(bbf.Self)),
                                   Ast.True()));
                }
            }

            return Ast.AndAlso(test,
                Ast.Equal(
                    Ast.ReadProperty(bfexpr, typeof(BuiltinFunction).GetProperty("Id")),
                    Ast.Constant(id)));
        }

        /// <summary>
        /// Extracts a built-in function from the target.
        /// </summary>
        private static Expression GetBuiltinFunctionFromTarget(StandardRule<T> rule, object target) {
            if (target is BuiltinFunction) {
                return Ast.Cast(rule.Parameters[0], typeof(BuiltinFunction));
            }

            if (target is BoundBuiltinFunction) {
                return Ast.ReadProperty(
                    Ast.Cast(rule.Parameters[0], typeof(BoundBuiltinFunction)),
                    typeof(BoundBuiltinFunction).GetProperty("Target"));
            }

            Debug.Assert(target is BuiltinMethodDescriptor);

            return Ast.ReadProperty(
                Ast.Cast(rule.Parameters[0], typeof(BuiltinMethodDescriptor)),
                typeof(BuiltinMethodDescriptor).GetProperty("Template"));
        }

        /// <summary>
        /// Makes the set of tests for the types w/ the parameter adjected by 1 to remove the target
        /// </summary>
        private Expression MakeTestForTypes(StandardRule<T> rule, Expression[] exprArgs, DynamicType[] types, int index, bool boundFunc, BuiltinFunction bf) {
            try {
                bool needTest = AreArgumentTypesOverloaded(types, index, boundFunc, bf);
                Expression test = needTest ? rule.MakeTypeTest(types[index], exprArgs[index + (boundFunc ? 1 : 0)]) : Ast.True();

                if (index + 1 < types.Length) {
                    Expression nextTests = MakeTestForTypes(rule, exprArgs, types, index + 1, boundFunc, bf);
                    if (test.IsConstant(true)) {
                        return nextTests;
                    } else if (nextTests.IsConstant(true)) {
                        return test;
                    } else {
                        return Ast.AndAlso(test, nextTests);
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

            DynamicType[] argTypes = Utils.Array.RemoveFirst(types);
            Utils.Array.SwapLastTwo(argTypes);
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
            for (int i = 0; i < rule.ParameterCount - 1; i++) {
                switch (action.GetArgumentKind(i)) {
                    case ArgumentKind.Instance:
                        instance = rule.Parameters[i + 1];
                        break;

                    case ArgumentKind.Dictionary:
                        args.Add(Arg.Dictionary(rule.Parameters[i + 1]));
                        keywordDict = true; 
                        extraArgs++;
                        break;
                    
                    case ArgumentKind.List:
                        args.Add(Arg.List(rule.Parameters[i + 1]));
                        argsTuple = true; 
                        extraArgs++;
                        break;

                    case ArgumentKind.Named:
                        args.Add(Arg.Named(action.GetArgumentName(i), rule.Parameters[i + 1]));
                        kwCnt++;
                        break;

                    case ArgumentKind.Simple:
                    default:
                        args.Add(Arg.Simple(rule.Parameters[i + 1]));
                        break;

                }
            }

            if (instance != null) {
                return Ast.CallWithThis(
                    rule.Parameters[0],
                    instance,
                    args.ToArray()
                );
            }

            return Ast.DynamicCall(
                rule.Parameters[0],
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
