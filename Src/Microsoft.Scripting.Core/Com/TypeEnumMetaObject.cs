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
#if !SILVERLIGHT

using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Com {
    internal class TypeEnumMetaObject : MetaObject {
        private readonly ComTypeEnumDesc _desc;

        internal TypeEnumMetaObject(ComTypeEnumDesc desc, Expression expression)
            : base(expression, Restrictions.Empty, desc) {
            _desc = desc;
        }

        public override MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
            if (_desc.HasMember(action.Name)) {
                return new MetaObject(
                    // return (.bound $arg0).GetValue("<name>")
                    Expression.Call(
                        Expression.ConvertHelper(Expression, typeof(ComTypeEnumDesc)),
                        typeof(ComTypeEnumDesc).GetMethod("GetValue"),
                        Expression.Constant(action.Name)
                    ),
                    Restrictions.Combine(args).Merge(EnumRestrictions())
                );
            }

            throw new NotImplementedException();
        }

        public override MetaObject Operation(OperationAction action, MetaObject[] args) {
            if (action.Operation == "GetMemberNames" || action.Operation == "MemberNames") {
                // return (arg).GetMemberNames()
                return new MetaObject(
                    Expression.Call(
                        Expression.ConvertHelper(Expression, typeof(ComTypeEnumDesc)),
                        typeof(ComTypeEnumDesc).GetMethod("GetMemberNames")
                    ),
                    Restrictions.Combine(args).Merge(EnumRestrictions())
                );
            }

            throw new NotImplementedException();
        }

        private Restrictions EnumRestrictions() {
            return Restrictions.TypeRestriction(
                Expression, typeof(ComTypeEnumDesc)
            ).Merge(
                // ((ComTypeEnumDesc)<arg>).TypeLib.Guid == <guid>
                Restrictions.ExpressionRestriction(
                    Expression.Equal(
                        Expression.Property(
                            Expression.Property(
                                Expression.ConvertHelper(Expression, typeof(ComTypeEnumDesc)),
                                typeof(ComTypeDesc).GetProperty("TypeLib")),
                            typeof(ComTypeLibDesc).GetProperty("Guid")),
                        Expression.Constant(_desc.TypeLib.Guid)
                    )
                )
            ).Merge(
                Restrictions.ExpressionRestriction(
                    Expression.Equal(
                        Expression.Property(
                            Expression.ConvertHelper(Expression, typeof(ComTypeEnumDesc)),
                            typeof(ComTypeEnumDesc).GetProperty("TypeName")
                        ),
                        Expression.Constant(_desc.TypeName)
                    )
                )
            );
        }
    }
}

#endif
