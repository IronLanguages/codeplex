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
using System.Reflection;
using System.Linq.Expressions;
using System.Scripting.Actions;
using System.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.ComDispatch {

    public partial class IDispatchComObject {

        internal class GetMemberBinder<T> : ComMemberBinderHelper<T, OldGetMemberAction> where T : class {

            private readonly ComTypeDesc _wrapperType;

            internal GetMemberBinder(CodeContext context, ComTypeDesc wrapperType, OldGetMemberAction action, object[] args)
                : base(context, action, args) {

                _wrapperType = wrapperType;
            }

            internal RuleBuilder<T> MakeNewRule() {

                Rule.Test = ComObject.MakeComObjectTest(typeof(IDispatchComObject), typeof(IDispatchComObject).GetProperty("ComTypeDesc"), _wrapperType, Rule);
                Rule.Target = MakeGetMemberTarget();

                return Rule;
            }

            private Expression MakeGetMemberTarget() {

                VariableExpression dispCallable = Rule.GetTemporary(typeof(object), "dispCallable");

                // The fallback expression permits us to get at language-specific extensions for
                // COM objects.  For example, in Python, every class supports the "__repr__" method.
                // This method must be implemented in Python as a type extension.  The fallback
                // expression gives us access to that type extension...
                Expression fallback = MakeExpressionForLanguageExtensionGetMember();

                AddToBody(
                    Expression.Block(
                        Expression.If(
                            Expression.SimpleCallHelper(
                                Rule.Parameters[0],
                                typeof(IDispatchComObject).GetMethod("TryGetAttr"),
                                Rule.Context,
                                Expression.Constant(Action.Name),
                                dispCallable),
                            Rule.MakeReturn(
                                Binder,
                                dispCallable)),
                        fallback));

                return Body;
            }

            Expression MakeExpressionForLanguageExtensionGetMember() {

                MemberGroup members = Binder.GetMember(Action, ComObject.ComObjectType, StringName);

                if (members.Count == 0) {
                    return GetFailureStatement(ComObject.ComObjectType, StringName);
                }

                Expression error = null;

                TrackerTypes memberType = GetMemberType(members, out error);

                if (error != null) {
                    return Rule.MakeError(error);
                }

                MemberTracker tracker = null;

                switch (memberType) {
                    case TrackerTypes.Method:
                        tracker = ReflectionCache.GetMethodGroup(StringName, members);
                        break;
                    case TrackerTypes.Property:
                    case TrackerTypes.Event:
                        tracker = (MemberTracker)members[0];
                        break;
                    case TrackerTypes.All:
                        return GetFailureStatement(ComObject.ComObjectType, StringName);
                    default:
                        throw new InvalidOperationException(memberType.ToString());
                }

                tracker = tracker.BindToInstance(Rule.Parameters[0]);

                Expression expression = tracker.GetValue(Rule.Context, Binder, ComObject.ComObjectType);

                if (expression != null) {
                    expression = Rule.MakeReturn(Binder, expression);
                } else {
                    expression = tracker.GetError(Binder).MakeErrorForRule(Rule, Binder);
                }

                return expression;
            }

            /// <summary>
            /// Generate the failure Statement
            ///     if Action.IsNoThrow = True
            ///         OperationFailed.Value 
            ///     else
            ///         MakeMissingMemberError -> returns Undefined.Value
            /// </summary>
            /// <returns></returns>
            private Expression GetFailureStatement(Type type, string memberName) {

                return Action.IsNoThrow ?
                           Rule.MakeReturn(
                               Context.LanguageContext.Binder,
                               Expression.Field(
                                   null,
                                   typeof(OperationFailed).GetField("Value")
                               )
                           ) :
                           Binder.MakeMissingMemberError(
                                type,
                                memberName
                           ).MakeErrorForRule(Rule, Binder);
            }
        }
    }
}

#endif
