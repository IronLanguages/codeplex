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
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.ComDispatch {

    public partial class ComObjectWithTypeInfo {

        private sealed class GetMemberBinder<T> : ComMemberBinderHelper<T, OldGetMemberAction> where T : class {

            private readonly Type _comType;

            internal GetMemberBinder(CodeContext context, Type comType, OldGetMemberAction action)
                : base(context, action) {

                _comType = comType;
            }

            internal RuleBuilder<T> MakeNewRule() {
                Rule.Test = ComObject.MakeComObjectTest(typeof(ComObjectWithTypeInfo), typeof(ComObjectWithTypeInfo).GetProperty("ComType"), _comType, Rule);
                Rule.Target = MakeGetMemberTarget();

                return Rule;
            }

            private Expression MakeGetMemberTarget() {
                // This method is a distillation of the GetMemberBinderHelper.MakeBodyHelper() method.
                Expression instance = Expression.Call(Rule.Parameters[0], typeof(ComObject).GetProperty("Obj").GetGetMethod());

                // We first look to see if the property to be set is a property on the inferred type.  If so, this
                // GetMember call or the subsequent WalkType call will return a non-empty MemberGroup.
                MemberGroup members = Binder.GetMember(Action, _comType, StringName);

                if (members.Count == 0) {
                    // IA class implementations often do not contain methods and properties, since
                    // COM implementations are centered around interface definitions.  Consequently,
                    // the standard binder fails to find methods/properties for class types.
                    MemberInfo[] foundMembers = ComObjectWithTypeInfo.WalkType(_comType, StringName);
                    if (!Context.LanguageContext.DomainManager.Configuration.PrivateBinding) {
                        members = new MemberGroup(CompilerHelpers.FilterNonVisibleMembers(_comType, foundMembers));
                    }
                }

                // If the MemberGroup is still empty, try to find the property as a language extension property.
                if (members.Count == 0) {
                    members = Binder.GetMember(Action, ComObject.ComObjectType, StringName);
                }

                Expression error = null;
                MemberTracker tracker = null;

                // If the MemberGroup is still empty, we have to give up.
                if (members.Count == 0) {
                    AddToBody(GetFailureStatement(_comType, StringName));
                    return Body;
                }

                TrackerTypes memberType = GetMemberType(members, out error);

                if (error != null) {
                    AddToBody(Rule.MakeError(error));
                    return Body;
                }

                switch (memberType) {
                    case TrackerTypes.Method:
                        tracker = ReflectionCache.GetMethodGroup(StringName, members);
                        break;
                    case TrackerTypes.Property:
                    case TrackerTypes.Event:
                        tracker = (MemberTracker)members[0];
                        break;
                    case TrackerTypes.All:
                        AddToBody(GetFailureStatement(_comType, StringName));
                        return Body;
                    default:
                        throw new InvalidOperationException(memberType.ToString());
                }

                tracker = tracker.BindToInstance(instance);
                Expression val = tracker.GetValue(Rule.Context, Binder, _comType);
                if (val != null) {
                    AddToBody(Rule.MakeReturn(Binder, val));
                } else {
                    AddToBody(tracker.GetError(Binder).MakeErrorForRule(Rule, Binder));
                }

                return Body;
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
