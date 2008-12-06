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

using System; using Microsoft;
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Linq.Expressions.Expression;

    public partial class DefaultBinder : ActionBinder {
        /// <summary>
        /// Builds a MetaObject for performing a member delete.  Supports all built-in .NET members, the OperatorMethod 
        /// DeleteMember, and StrongBox instances.
        /// </summary>
        public DynamicMetaObject DeleteMember(string name, DynamicMetaObject target) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(target, "target");

            return DeleteMember(
                name,
                target,
                Ast.Constant(null, typeof(CodeContext))
            );
        }

        public DynamicMetaObject DeleteMember(string name, DynamicMetaObject target, Expression codeContext) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(target, "target");

            return MakeDeleteMemberTarget(
                new SetOrDeleteMemberInfo(
                    name,
                    codeContext
                ),
                target.Restrict(target.LimitType)
            );
        }

        private DynamicMetaObject MakeDeleteMemberTarget(SetOrDeleteMemberInfo delInfo, DynamicMetaObject target) {
            Type type = target.LimitType;
            BindingRestrictions restrictions = target.Restrictions;
            Expression self = target.Expression;

            // needed for DeleteMember call until DynamicAction goes away
            OldDynamicAction act = OldDeleteMemberAction.Make(
                this,
                delInfo.Name
            );

            if (typeof(TypeTracker).IsAssignableFrom(type)) {
                restrictions = restrictions.Merge(
                    BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value)
                );

                type = ((TypeTracker)target.Value).Type;
                self = null;
            }

            delInfo.Body.Restrictions = restrictions;

            if (self == null || !MakeOperatorDeleteMemberBody(delInfo, self, type, "DeleteMember")) {
                MemberGroup group = GetMember(act, type, delInfo.Name);
                if (group.Count != 0) {
                    if (group[0].MemberType == TrackerTypes.Property) {
                        MethodInfo del = ((PropertyTracker)group[0]).GetDeleteMethod(PrivateBinding);
                        if (del != null) {
                            MakePropertyDeleteStatement(delInfo, self, del);
                            return delInfo.Body.GetMetaObject(target);
                        }
                    }

                    delInfo.Body.FinishCondition(MakeError(MakeUndeletableMemberError(GetDeclaringMemberType(group), delInfo.Name)));
                } else {
                    delInfo.Body.FinishCondition(MakeError(MakeMissingMemberError(type, delInfo.Name)));
                }
            }

            return delInfo.Body.GetMetaObject(target);
        }

        private static Type GetDeclaringMemberType(MemberGroup group) {
            Type t = typeof(object);
            foreach (MemberTracker mt in group) {
                if (t.IsAssignableFrom(mt.DeclaringType)) {
                    t = mt.DeclaringType;
                }
            }
            return t;
        }

        private void MakePropertyDeleteStatement(SetOrDeleteMemberInfo delInfo, Expression instance, MethodInfo delete) {
            delInfo.Body.FinishCondition(
                MakeCallExpression(delInfo.CodeContext, delete, instance)
            );
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        private bool MakeOperatorDeleteMemberBody(SetOrDeleteMemberInfo delInfo, Expression instance, Type type, string name) {
            MethodInfo delMem = GetMethod(type, name);

            if (delMem != null && delMem.IsSpecialName) {
                Expression call = MakeCallExpression(delInfo.CodeContext, delMem, instance, Ast.Constant(delInfo.Name));

                if (delMem.ReturnType == typeof(bool)) {
                    delInfo.Body.AddCondition(
                        call,
                        Ast.Constant(null)
                    );
                } else {
                    delInfo.Body.FinishCondition(call);
                }

                return delMem.ReturnType != typeof(bool);
            }
            return false;
        }

        /// <summary>
        /// Helper class for flowing information about the GetMember request.
        /// </summary>
        private sealed class SetOrDeleteMemberInfo {
            public readonly string Name;
            public readonly Expression CodeContext;
            public readonly ConditionalBuilder Body = new ConditionalBuilder();

            public SetOrDeleteMemberInfo(string name, Expression codeContext) {
                Name = name;
                CodeContext = codeContext;
            }
        }
    }
}
