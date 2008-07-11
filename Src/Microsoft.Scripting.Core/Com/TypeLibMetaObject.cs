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

#if !SILVERLIGHT

using System.Scripting.Actions;
using System.Linq.Expressions;

namespace System.Scripting.Com {
    internal class TypeLibMetaObject : MetaObject {
        private readonly ComTypeLibDesc _lib;

        internal TypeLibMetaObject(Expression expression, ComTypeLibDesc lib)
            : base(expression, Restrictions.Empty, lib) {
            _lib = lib;
        }

        public override MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
            Restrictions restrictions =
                Restrictions.Combine(
                    args
                ).Merge(
                    Restrictions.TypeRestriction(
                        Expression, typeof(ComTypeLibDesc)
                    )
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

            if (_lib.HasMember(action.Name)) {
                return new MetaObject(
                    Expression.Call(
                        Expression.ConvertHelper(Expression, typeof(ComTypeLibDesc)),
                        typeof(ComTypeLibDesc).GetMethod("GetTypeLibObjectDesc"),
                        Expression.Constant(action.Name.ToString())
                    ),
                    restrictions
                );
            } else {
                return new ErrorMetaObject(
                    typeof(MissingMemberException),
                    "No member " + action.Name,
                    restrictions
                );
            }
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
                    return new ErrorMetaObject(
                        typeof(NotSupportedException),
                        "Not supported: " + action.Operation,
                        Restrictions.Combine(args)
                    );
            }
        }
    }
}

#endif
