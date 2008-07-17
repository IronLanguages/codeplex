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

#if !SILVERLIGHT // ComObject

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.ComDispatch {

    public partial class IDispatchComObject {

        private sealed class SetMemberBinder<T> : ComMemberBinderHelper<T, OldSetMemberAction> where T : class {

            private readonly ComTypeDesc _wrapperType;

            internal SetMemberBinder(CodeContext context, ComTypeDesc wrapperType, OldSetMemberAction action)
                : base(context, action) {

                _wrapperType = wrapperType;
            }

            internal RuleBuilder<T> MakeNewRule() {

                Rule.Test = ComObject.MakeComObjectTest(typeof(IDispatchComObject), typeof(IDispatchComObject).GetProperty("ComTypeDesc"), _wrapperType, Rule);
                Rule.Target = MakeSetMemberTarget();

                return Rule;
            }

            private Expression MakeSetMemberTarget() {

                Expression fallback = MakeSetMemberTargetForLanguageExtension();
                VariableExpression exception = Rule.GetTemporary(typeof(Exception), "exception");

                AddToBody(
                    Expression.Block(
                        Expression.If(
                            Expression.Not(
                                Expression.SimpleCallHelper(
                                    Rule.Parameters[0],
                                    typeof(IDispatchComObject).GetMethod("TrySetAttr"),
                                    Rule.Context,
                                    Expression.Constant(SymbolTable.IdToString(Action.Name)),
                                    Rule.Parameters[1],
                                    exception)),
                            fallback),
                        Rule.MakeReturn(
                            Binder,
                            Expression.Null())));

                return Body;
            }

            private Expression MakeSetMemberTargetForLanguageExtension() {

                // This method is a distillation of the SetMemberBinderHelper.MakeSetMemberRule() method.

                // We first look to see if the property to be set is a property on the inferred type.  If so, this
                // GetMember call or the subsequent WalkType call will return a non-empty MemberGroup.
                MemberGroup members = Binder.GetMember(Action, ComObject.ComObjectType, StringName);

                // If the MemberGroup is still empty, we have to give up.
                if (members.Count == 0) {
                    return Binder.MakeMissingMemberError(ComObject.ComObjectType, StringName).MakeErrorForRule(Rule, Binder);
                }

                Expression error = null;
                TrackerTypes memberType = GetMemberType(members, out error);

                if (error != null) {
                    return Rule.MakeError(error);
                }

                Expression expression = null;

                switch (memberType) {
                    case TrackerTypes.Method:
                        expression = Binder.MakeReadOnlyMemberError<T>(Rule, ComObject.ComObjectType, StringName);
                        break;
                    case TrackerTypes.Event:
                        expression = Binder.MakeEventValidation(Rule, members).MakeErrorForRule(Rule, Binder);
                        break;
                    case TrackerTypes.Property:
                        expression = MakePropertyRule(ComObject.ComObjectType, members);
                        break;
                    case TrackerTypes.All:
                        expression = Binder.MakeMissingMemberError(ComObject.ComObjectType, StringName).MakeErrorForRule(Rule, Binder);
                        break;
                    default:
                        throw new InvalidOperationException(memberType.ToString());
                }

                return expression;
            }

            private Expression MakePropertyRule(Type targetType, MemberGroup properties) {

                PropertyTracker info = (PropertyTracker)properties[0];
                MethodInfo setter = info.GetSetMethod(true);
                Expression expression = null;

                Rule.Parameters[0] = Expression.Call(Rule.Parameters[0], typeof(ComObject).GetProperty("Obj").GetGetMethod());

                if (setter != null) {
                    setter = CompilerHelpers.GetCallableMethod(setter);

                    if (setter.IsPublic) {
                        expression = Rule.MakeReturn(Binder, MakeReturnValue(Binder.MakeCallExpression(Rule.Context, setter, Rule.Parameters)));
                    } else {
                        expression = Binder.MakeMissingMemberError(targetType, StringName).MakeErrorForRule(Rule, Binder);
                    }
                } else {
                    expression = Binder.MakeMissingMemberError(targetType, StringName).MakeErrorForRule(Rule, Binder);
                }

                return expression;
            }

            private Expression MakeReturnValue(Expression expression) {

                return Expression.Comma(Rule.Parameters[1], expression);
            }
        }
    }
}

#endif
