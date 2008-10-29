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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.ComInterop {
    internal sealed class TypeLibInfoMetaObject : MetaObject {
        private readonly ComTypeLibInfo _info;

        internal TypeLibInfoMetaObject(Expression expression, ComTypeLibInfo info)
            : base(expression, Restrictions.Empty, info) {
            _info = info;
        }

        public override MetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            string name = binder.Name;

            if (name == _info.Name) {
                name = "TypeLibDesc";
            } else if (name != "Guid" &&
                name != "Name" &&
                name != "VersionMajor" &&
                name != "VersionMinor") {

                return binder.FallbackGetMember(this);
            }

            return new MetaObject(
                Expression.Property(
                    Helpers.Convert(Expression, typeof(ComTypeLibInfo)),
                    typeof(ComTypeLibInfo).GetProperty(name)
                ),
                ComTypeLibInfoRestrictions(this)
            );
        }

        [Obsolete("Use UnaryOperation or BinaryOperation")]
        public override MetaObject BindOperation(OperationBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");

            if (binder.Operation == "GetMemberNames" || binder.Operation == "MemberNames") {
                return new MetaObject(
                    Expression.Call(
                        Helpers.Convert(Expression, typeof(ComTypeLibInfo)),
                        typeof(ComTypeLibInfo).GetMethod("GetMemberNames")
                    ),
                    ComTypeLibInfoRestrictions(args)
                );
            }

            return binder.FallbackOperation(RestrictThisToType(), args);
        }

        private Restrictions ComTypeLibInfoRestrictions(params MetaObject[] args) {
            return Restrictions.Combine(args).Merge(Restrictions.GetTypeRestriction(Expression, typeof(ComTypeLibInfo)));
        }

        private MetaObject RestrictThisToType() {
            return new MetaObject(
                Helpers.Convert(
                    Expression,
                    typeof(ComTypeLibInfo)
                ),
                Restrictions.GetTypeRestriction(Expression, typeof(ComTypeLibInfo))
            );
        }
    }
}

#endif
