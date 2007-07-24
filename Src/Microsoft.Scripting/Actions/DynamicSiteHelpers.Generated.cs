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
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions {
    public static partial class DynamicSiteHelpers {        
        #region Generated DynamicSiteHelpers

        // *** BEGIN GENERATED CODE ***

        public static readonly int MaximumArity = 7;

        public static Type MakeDynamicSiteType(params Type[] types) {
            Type genType;
            switch (types.Length) {
                case 2: genType = typeof(DynamicSite<,>); break;
                case 3: genType = typeof(DynamicSite<,,>); break;
                case 4: genType = typeof(DynamicSite<,,,>); break;
                case 5: genType = typeof(DynamicSite<,,,,>); break;
                case 6: genType = typeof(DynamicSite<,,,,,>); break;
                case 7: genType = typeof(DynamicSite<,,,,,,>); break;
                default:
                    return MakeBigDynamicSiteType(types);
            }

            return genType.MakeGenericType(types);
        }

        public static Type MakeFastDynamicSiteType(params Type[] types) {
            Type genType;
            switch (types.Length) {
                case 2: genType = typeof(FastDynamicSite<,>); break;
                case 3: genType = typeof(FastDynamicSite<,,>); break;
                case 4: genType = typeof(FastDynamicSite<,,,>); break;
                case 5: genType = typeof(FastDynamicSite<,,,,>); break;
                case 6: genType = typeof(FastDynamicSite<,,,,,>); break;
                case 7: genType = typeof(FastDynamicSite<,,,,,,>); break;
                default:
                    return MakeBigFastDynamicSiteType(types);
            }

            return genType.MakeGenericType(types);
        }

        public static object Execute(CodeContext context, ActionBinder binder, Action action, params object[] args) {
            bool result;
            switch (args.Length) {
                case 1:
                    StandardRule<DynamicSiteTarget<object, object>> rule1 = 
                        binder.GetRule<DynamicSiteTarget<object, object>>(context, action, args);

                    using (context.Scope.TemporaryVariableContext(rule1.TemporaryVariables, rule1.ParamVariables, args)) {
                        result = (bool)rule1.Test.Evaluate(context);
                        Debug.Assert(result);
                        return rule1.Target.Execute(context);
                    }
                case 2:
                    StandardRule<DynamicSiteTarget<object, object, object>> rule2 = 
                        binder.GetRule<DynamicSiteTarget<object, object, object>>(context, action, args);

                    using (context.Scope.TemporaryVariableContext(rule2.TemporaryVariables, rule2.ParamVariables, args)) {
                        result = (bool)rule2.Test.Evaluate(context);
                        Debug.Assert(result);
                        return rule2.Target.Execute(context);
                    }
                case 3:
                    StandardRule<DynamicSiteTarget<object, object, object, object>> rule3 = 
                        binder.GetRule<DynamicSiteTarget<object, object, object, object>>(context, action, args);

                    using (context.Scope.TemporaryVariableContext(rule3.TemporaryVariables, rule3.ParamVariables, args)) {
                        result = (bool)rule3.Test.Evaluate(context);
                        Debug.Assert(result);
                        return rule3.Target.Execute(context);
                    }
                case 4:
                    StandardRule<DynamicSiteTarget<object, object, object, object, object>> rule4 = 
                        binder.GetRule<DynamicSiteTarget<object, object, object, object, object>>(context, action, args);

                    using (context.Scope.TemporaryVariableContext(rule4.TemporaryVariables, rule4.ParamVariables, args)) {
                        result = (bool)rule4.Test.Evaluate(context);
                        Debug.Assert(result);
                        return rule4.Target.Execute(context);
                    }
                case 5:
                    StandardRule<DynamicSiteTarget<object, object, object, object, object, object>> rule5 = 
                        binder.GetRule<DynamicSiteTarget<object, object, object, object, object, object>>(context, action, args);

                    using (context.Scope.TemporaryVariableContext(rule5.TemporaryVariables, rule5.ParamVariables, args)) {
                        result = (bool)rule5.Test.Evaluate(context);
                        Debug.Assert(result);
                        return rule5.Target.Execute(context);
                    }
                case 6:
                    StandardRule<DynamicSiteTarget<object, object, object, object, object, object, object>> rule6 = 
                        binder.GetRule<DynamicSiteTarget<object, object, object, object, object, object, object>>(context, action, args);

                    using (context.Scope.TemporaryVariableContext(rule6.TemporaryVariables, rule6.ParamVariables, args)) {
                        result = (bool)rule6.Test.Evaluate(context);
                        Debug.Assert(result);
                        return rule6.Target.Execute(context);
                    }
                default:
                    //TODO: use CompilerHelpers.GetTypes(args) instead?
                    Type tupleType = NewTuple.MakeTupleType(CompilerHelpers.MakeRepeatedArray<Type>(typeof(object), args.Length));
                    Type targetType = typeof(BigDynamicSiteTarget<,>).MakeGenericType(tupleType, typeof(object));
                    Type ruleType = typeof(StandardRule<>).MakeGenericType(targetType);
                    MethodInfo getRule = typeof(ActionBinder).GetMethod("GetRule").MakeGenericMethod(targetType);
                    object ruleN = getRule.Invoke(binder, new object[] { context, action, args });
                    Ast.Expression test = (Ast.Expression)ruleType.GetProperty("Test").GetValue(ruleN, null);
                    Ast.Statement target = (Ast.Statement)ruleType.GetProperty("Target").GetValue(ruleN, null);
                    Ast.Variable[] paramVars = (Ast.Variable[]) ruleType.GetProperty("ParamVariables",
                        BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ruleN, null);
                    Ast.Variable[] tempVars = (Ast.Variable[])ruleType.GetProperty("TemporaryVariables",
                        BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ruleN, null);


                    NewTuple t = NewTuple.MakeTuple(tupleType, args);
                    object[] tupArg = new object[] {t};
                    using (context.Scope.TemporaryVariableContext(tempVars, paramVars, tupArg)) {
                        result = (bool)test.Evaluate(context);
                        Debug.Assert(result);
                        return target.Execute(context);
                    }
            }
        }

        private class UninitializedTargetHelper<T0, T1, T2, T3, T4, T5, Tret> {
            public Tret Invoke1(DynamicSite<T0, Tret> site, CodeContext context, T0 arg0) {
                return site.UpdateBindingAndInvoke(context, arg0);
            }
            public Tret FastInvoke1(FastDynamicSite<T0, Tret> site, T0 arg0) {
                return site.UpdateBindingAndInvoke(arg0);
            }
            public Tret Invoke2(DynamicSite<T0, T1, Tret> site, CodeContext context, T0 arg0, T1 arg1) {
                return site.UpdateBindingAndInvoke(context, arg0, arg1);
            }
            public Tret FastInvoke2(FastDynamicSite<T0, T1, Tret> site, T0 arg0, T1 arg1) {
                return site.UpdateBindingAndInvoke(arg0, arg1);
            }
            public Tret Invoke3(DynamicSite<T0, T1, T2, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2) {
                return site.UpdateBindingAndInvoke(context, arg0, arg1, arg2);
            }
            public Tret FastInvoke3(FastDynamicSite<T0, T1, T2, Tret> site, T0 arg0, T1 arg1, T2 arg2) {
                return site.UpdateBindingAndInvoke(arg0, arg1, arg2);
            }
            public Tret Invoke4(DynamicSite<T0, T1, T2, T3, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
                return site.UpdateBindingAndInvoke(context, arg0, arg1, arg2, arg3);
            }
            public Tret FastInvoke4(FastDynamicSite<T0, T1, T2, T3, Tret> site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
                return site.UpdateBindingAndInvoke(arg0, arg1, arg2, arg3);
            }
            public Tret Invoke5(DynamicSite<T0, T1, T2, T3, T4, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
                return site.UpdateBindingAndInvoke(context, arg0, arg1, arg2, arg3, arg4);
            }
            public Tret FastInvoke5(FastDynamicSite<T0, T1, T2, T3, T4, Tret> site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
                return site.UpdateBindingAndInvoke(arg0, arg1, arg2, arg3, arg4);
            }
            public Tret Invoke6(DynamicSite<T0, T1, T2, T3, T4, T5, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
                return site.UpdateBindingAndInvoke(context, arg0, arg1, arg2, arg3, arg4, arg5);
            }
            public Tret FastInvoke6(FastDynamicSite<T0, T1, T2, T3, T4, T5, Tret> site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
                return site.UpdateBindingAndInvoke(arg0, arg1, arg2, arg3, arg4, arg5);
            }
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
