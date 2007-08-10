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
using Microsoft.Scripting.Types;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Utils;

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
            Type[] argTypes = GetArgumentTypes(args);
            if (argTypes == null) return null;

            SymbolId[] argNames = Action.GetArgumentNames();

            Type[] testTypes;
            MethodCandidate cand = GetMethodCandidate(args[0], bf, argTypes, argNames, out testTypes);

            if (cand == null) {
            } else if (cand != null &&
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
                    // MakeTestForTypes(rule, exprargs, argTypes, 0, args[0] is BoundBuiltinFunction, bf)
                    test = Ast.AndAlso(test, MakeNecessaryTests(rule, new Type[][]{ testTypes }, exprargs));
                } 

                rule.SetTest(test);
                
                if (bf.IsReversedOperator) ArrayUtils.SwapLastTwo(exprargs);

                rule.SetTarget(rule.MakeReturn(
                    Binder,
                    cand.Target.MakeExpression(Binder, exprargs)));
                
                return rule;
            }
            return null;
        }


        private MethodCandidate GetMethodCandidate(object target, BuiltinFunction bf, Type[] argTypes, SymbolId []argNames, out Type[] testTypes) {
            MethodBinder binder = MethodBinder.MakeBinder(Binder, "__call__", bf.Targets, bf.IsBinaryOperator ? BinderType.BinaryOperator : BinderType.Normal);
            BoundBuiltinFunction boundbf = target as BoundBuiltinFunction;
            if (boundbf == null) {                
                return binder.MakeBindingTarget(CallType.None, argTypes, argNames, out testTypes);
            }

            Type[] types = ArrayUtils.Insert(CompilerHelpers.GetType(boundbf.Self), argTypes);
            if (bf.IsReversedOperator) {
                ArrayUtils.SwapLastTwo(types);
                if (argNames.Length >= 2) {
                    ArrayUtils.SwapLastTwo(argNames);
                }
            }


            MethodCandidate res = binder.MakeBindingTarget(CallType.ImplicitInstance, types, argNames, out testTypes);
            if (bf.IsReversedOperator && testTypes != null) {
                ArrayUtils.SwapLastTwo(testTypes);
            }
            return res;
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

        private Type[] GetArgumentTypes(object[] args) {
            return GetArgumentTypes(Action, args);
        }

        private static DynamicType[] GetReversedArgumentTypes(DynamicType[] types) {
            if (types.Length < 3) throw new InvalidOperationException("need 3 types or more for reversed operator");

            DynamicType[] argTypes = ArrayUtils.RemoveFirst(types);
            ArrayUtils.SwapLastTwo(argTypes);
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
