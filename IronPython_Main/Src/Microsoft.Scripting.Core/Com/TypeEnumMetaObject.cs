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

namespace Microsoft.Scripting.ComInterop {
    internal class TypeEnumMetaObject : MetaObject {
        private readonly ComTypeEnumDesc _desc;

        internal TypeEnumMetaObject(ComTypeEnumDesc desc, Expression expression)
            : base(expression, Restrictions.Empty, desc) {
            _desc = desc;
        }

        public override MetaObject BindGetMember(GetMemberBinder binder) {
            if (_desc.HasMember(binder.Name)) {
                return new MetaObject(
                    // return (.bound $arg0).GetValue("<name>")
                    Expression.Call(
                        Expression.ConvertHelper(Expression, typeof(ComTypeEnumDesc)),
                        typeof(ComTypeEnumDesc).GetMethod("GetValue"),
                        Expression.Constant(binder.Name)
                    ),
                    EnumRestrictions()
                );
            }

            throw new NotImplementedException();
        }

        [Obsolete("Use UnaryOperation or BinaryOperation")]
        public override MetaObject BindOperation(OperationBinder binder, MetaObject[] args) {
            if (binder.Operation == "GetMemberNames" || binder.Operation == "MemberNames") {
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
            return Restrictions.GetTypeRestriction(
                Expression, typeof(ComTypeEnumDesc)
            ).Merge(
                // ((ComTypeEnumDesc)<arg>).TypeLib.Guid == <guid>
                Restrictions.GetExpressionRestriction(
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
                Restrictions.GetExpressionRestriction(
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
