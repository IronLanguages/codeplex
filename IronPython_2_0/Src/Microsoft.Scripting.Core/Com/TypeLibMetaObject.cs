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
    internal class TypeLibMetaObject : MetaObject {
        private readonly ComTypeLibDesc _lib;

        internal TypeLibMetaObject(Expression expression, ComTypeLibDesc lib)
            : base(expression, Restrictions.Empty, lib) {
            _lib = lib;
        }

        public override MetaObject GetMember(GetMemberAction action) {
            if (_lib.HasMember(action.Name)) {
                Restrictions restrictions =
                    Restrictions.TypeRestriction(
                        Expression, typeof(ComTypeLibDesc)
                    ).Merge(
                        Restrictions.ExpressionRestriction(
                            Expression.Equal(
                                Expression.Property(
                                    Expression.ConvertHelper(
                                        Expression, typeof(ComTypeLibDesc)
                                    ),
                                    typeof(ComTypeLibDesc).GetProperty("Guid")
                                ),
                                Expression.Constant(_lib.Guid.ToString())
                            )
                        )
                    );

                return new MetaObject(
                    Expression.Call(
                        Expression.ConvertHelper(Expression, typeof(ComTypeLibDesc)),
                        typeof(ComTypeLibDesc).GetMethod("GetTypeLibObjectDesc"),
                        Expression.Constant(action.Name.ToString())
                    ),
                    restrictions
                );
            }

            return base.GetMember(action);
        }

        public override MetaObject Operation(OperationAction action, MetaObject[] args) {
            switch (action.Operation) {
                case "GetMemberNames":
                case "MemberNames":
                    return new MetaObject(
                        Expression.Call(
                            Expression.ConvertHelper(Expression, typeof(ComTypeLibDesc)),
                            typeof(ComTypeLibDesc).GetMethod("GetMemberNames")
                        ),
                        Restrictions.Combine(args).Merge(Restrictions.TypeRestriction(Expression, typeof(ComTypeLibDesc)))
                    );

                default:
                    return base.Operation(action, args);
            }
        }
    }
}

#endif
