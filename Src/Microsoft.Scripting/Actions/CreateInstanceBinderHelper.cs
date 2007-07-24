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
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class CreateInstanceBinderHelper<T> : BinderHelper<T, CreateInstanceAction> {
        public CreateInstanceBinderHelper(CodeContext context, CreateInstanceAction action)
            : base(context, action) {
        }

        public StandardRule<T> MakeRule(object[] args) {
            DynamicType[] types = CompilerHelpers.ObjectTypes(args);
            DynamicType dt = args[0] as DynamicType;

            return MakeTypeCallRule(dt, types, args);
        }

        private StandardRule<T> MakeTypeCallRule(DynamicType creating, DynamicType[] types, object[] args) {
            return MakeDotNetTypeCallRule(creating, args);
        }

        /// <summary>
        /// Creates a rule which calls a .NET constructor directly.
        /// </summary>
        private StandardRule<T> MakeDotNetTypeCallRule(DynamicType creating, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();

            if (creating.UnderlyingSystemType.IsPublic || creating.UnderlyingSystemType.IsNestedPublic) {
                MethodCandidate cand = GetTypeConstructor(creating, GetArgumentTypes(Action, args));
                if (cand != null) {
                    Expression[] parameters = GetArgumentExpressions(cand, Action, rule, args);
                    Expression target = cand.Target.MakeExpression(Binder, parameters);
                    rule.SetTest(MakeTestForTypeCall(creating, rule, args));
                    rule.SetTarget(rule.MakeReturn(Binder, target));
                    return rule;
                }
            }

            // parameters don't work or it's a private method/type, go dynamic.
            return null;
        }

        private Expression MakeTestForTypeCall(DynamicType creating, StandardRule<T> rule, object[] args) {
            return MakeTestForTypeCall(Action, creating, rule, args);
        }

        public static Expression MakeTestForTypeCall(CallAction action, DynamicType creating, StandardRule<T> rule, object[] args) {
            Expression test = Ast.AndAlso(
                rule.MakeTestForTypes(CompilerHelpers.ObjectTypes(args), 0),
                Ast.Equal(
                    Ast.ReadProperty(rule.Parameters[0], typeof(DynamicType).GetProperty("Version")),
                    Ast.Constant(creating.Version)
                )
            );

            if (action.IsParamsCall()) {
                test = Ast.AndAlso(test, MakeParamsTest(rule, args));
                IList<object> listArgs = args[args.Length - 1] as IList<object>;

                for (int i = 0; i < listArgs.Count; i++) {
                    test = Ast.AndAlso(test,
                        rule.MakeTypeTest(DynamicHelpers.GetDynamicType(listArgs[i]),
                            Ast.Call(
                                GetParamsList(rule),
                                typeof(IList<object>).GetMethod("get_Item"),
                                Ast.Constant(i)
                            )
                        )
                    );
                }
            }
            return test;
        }

        private MethodCandidate GetTypeConstructor(DynamicType creating, DynamicType[] argTypes) {
            return GetTypeConstructor(Binder, creating, argTypes);
        }

        /// <summary>
        /// Generates an expression which calls a .NET constructor directly.
        /// </summary>
        public static MethodCandidate GetTypeConstructor(ActionBinder binder, DynamicType creating, DynamicType[] argTypes) {
            // type has no __new__ override, call .NET ctor directly
            MethodBinder mb = MethodBinder.MakeBinder(binder,
                creating.Name,
                creating.UnderlyingSystemType.GetConstructors(),
                BinderType.Normal);

            MethodCandidate mc = mb.MakeBindingTarget(CallType.None, argTypes);
            if (mc != null && mc.Target.Method.IsPublic) {
                return mc;
            }
            return null;
        }     
    }
}
