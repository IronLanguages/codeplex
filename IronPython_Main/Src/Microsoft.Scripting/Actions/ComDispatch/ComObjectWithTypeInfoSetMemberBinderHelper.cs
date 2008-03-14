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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.ComDispatch {

    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Runtime;

    internal class ComObjectWithTypeInfoSetMemberBinderHelper<T> : MemberBinderHelper<T, SetMemberAction> {
        private Type _comType;

        internal ComObjectWithTypeInfoSetMemberBinderHelper(CodeContext context, Type comType, SetMemberAction action, object[] args)
            : base(context, action, args) {
            _comType = comType;
        }

        internal StandardRule<T> MakeNewRule() {
            // Since the only way to get this rule in the first place is to pass through the statically defined pre-binders,
            // the test here essentially redundant.  We need one though, so we'll use the basic type test.
            Rule.MakeTest(typeof(ComObjectWithTypeInfo));
            Rule.Target = MakeSetMemberTarget();

            return Rule;
        }

        private Expression MakeSetMemberTarget() {
            // This method is a distillation of the SetMemberBinderHelper.MakeSetMemberRule() method.

            // We first look to see if the property to be set is a property on the inferred type.  If so, this
            // GetMember call or the subsequent WalkType call will return a non-empty MemberGroup.
            MemberGroup members = Binder.GetMember(Action, _comType, StringName);

            if (members.Count == 0) {
                // IA class implementations often do not contain methods and properties, since
                // COM implementations are centered around interface definitions.  Consequently,
                // the standard binder fails to find methods/properties for class types.
                MemberInfo[] foundMembers = ComObjectWithTypeInfo.WalkType(_comType, StringName);
                members = new MemberGroup(ActionBinder.FilterNonVisibleMembers(_comType, foundMembers));
            }

            // If the MemberGroup is still empty, try to find the property as a language extension property.
            if (members.Count == 0) {
                members = Binder.GetMember(Action, ComObject.ComObjectType, StringName);
            }

            // If the MemberGroup is still empty, we have to give up.
            if (members.Count == 0) {
                MakeMissingMemberError(ComObject.ComObjectType);
                return Body;
            }

            Expression error = null;
            TrackerTypes memberType = GetMemberType(members, out error);

            if (error != null) {
                AddToBody(Rule.MakeError(error));
                return Body;
            }

            switch (memberType) {
                case TrackerTypes.Method:
                    MakeReadOnlyMemberError(ComObject.ComObjectType);
                    break;
                case TrackerTypes.Event:
                    AddToBody(Binder.MakeEventValidation(Rule, members).MakeErrorForRule(Rule, Binder));
                    break;
                case TrackerTypes.Property:
                    MakePropertyRule(ComObject.ComObjectType, members);
                    break;
                case TrackerTypes.All:
                    MakeMissingMemberError(ComObject.ComObjectType);
                    break;
                default:
                    throw new InvalidOperationException(memberType.ToString());
            }

            return Body;
        }

        private void MakePropertyRule(Type targetType, MemberGroup properties) {
            PropertyTracker info = (PropertyTracker)properties[0];

            Expression comObject = Rule.Parameters[0];
            Rule.Parameters[0] = Ast.Call(Rule.Parameters[0], typeof(ComObject).GetProperty("Obj").GetGetMethod());

            MethodInfo setter = info.GetSetMethod(true);

            if (setter != null) {
                setter = CompilerHelpers.GetCallableMethod(setter);

                if (setter.IsPublic) {
                    AddToBody(Rule.MakeReturn(Binder, MakeReturnValue(Binder.MakeCallExpression(setter, Rule.Parameters))));
                } else {
                    AddToBody(Binder.MakeMissingMemberError(targetType, StringName).MakeErrorForRule(Rule, Binder));
                }
            } else {
                AddToBody(Binder.MakeMissingMemberError(targetType, StringName).MakeErrorForRule(Rule, Binder));
            }
        }

        private Expression MakeReturnValue(Expression expression) {
            return Ast.Comma(Rule.Parameters[1], expression);
        }
    }
}

#endif