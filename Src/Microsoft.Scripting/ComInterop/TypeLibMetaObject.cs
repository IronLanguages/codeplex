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

using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.ComInterop {

    internal class TypeLibMetaObject : DynamicMetaObject {
        private readonly ComTypeLibDesc _lib;

        internal TypeLibMetaObject(Expression expression, ComTypeLibDesc lib)
            : base(expression, BindingRestrictions.Empty, lib) {
            _lib = lib;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            if (_lib.HasMember(binder.Name)) {
                BindingRestrictions restrictions =
                    BindingRestrictions.GetTypeRestriction(
                        Expression, typeof(ComTypeLibDesc)
                    ).Merge(
                        BindingRestrictions.GetExpressionRestriction(
                            Expression.Equal(
                                Expression.Property(
                                    AstUtils.Convert(
                                        Expression, typeof(ComTypeLibDesc)
                                    ),
                                    typeof(ComTypeLibDesc).GetProperty("Guid")
                                ),
                                AstUtils.Constant(_lib.Guid)
                            )
                        )
                    );

                return new DynamicMetaObject(
                    AstUtils.Constant(
                        ((ComTypeLibDesc)Value).GetTypeLibObjectDesc(binder.Name)
                    ),
                    restrictions
                );
            }

            return base.BindGetMember(binder);
        }

        public override IEnumerable<string> GetDynamicMemberNames() {
            return _lib.GetMemberNames();
        }
    }
}

#endif
