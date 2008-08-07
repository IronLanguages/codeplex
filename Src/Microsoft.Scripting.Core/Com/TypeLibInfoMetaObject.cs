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

using System.Linq.Expressions;
using System.Scripting.Actions;
using System.Scripting.Utils;

namespace System.Scripting.Com {
    internal sealed class TypeLibInfoMetaObject : MetaObject {
        private readonly ComTypeLibInfo _info;

        internal TypeLibInfoMetaObject(Expression expression, ComTypeLibInfo info)
            : base(expression, Restrictions.Empty, info) {
            _info = info;
        }

        public override MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
            string name = action.Name;

            if (name == _info.Name) {
                name = "TypeLibDesc";
            } else if (name != "Guid" &&
                name != "Name" &&
                name != "VersionMajor" &&
                name != "VersionMinor") {

                return action.Fallback(args);
            }

            return new MetaObject(
                Expression.Property(
                    Expression.ConvertHelper(Expression, typeof(ComTypeLibInfo)),
                    typeof(ComTypeLibInfo).GetProperty(name)
                ),
                ComTypeLibInfoRestrictions(args)
            );
        }

        public override MetaObject Operation(OperationAction action, MetaObject[] args) {
            if (action.Operation == "GetMemberNames" || action.Operation == "MemberNames") {
                return new MetaObject(
                    Expression.Call(
                        Expression.ConvertHelper(Expression, typeof(ComTypeLibInfo)),
                        typeof(ComTypeLibInfo).GetMethod("GetMemberNames")
                    ),
                    ComTypeLibInfoRestrictions(args)
                );
            }

            return action.Fallback(RestrictThisToType(args));
        }

        private Restrictions ComTypeLibInfoRestrictions(MetaObject[] args) {
            return Restrictions.Combine(args).Merge(Restrictions.TypeRestriction(Expression, typeof(ComTypeLibInfo)));
        }

        private static MetaObject[] RestrictThisToType(MetaObject[] args) {
            MetaObject[] copy = args.Copy();
            args[0] = args[0].Restrict(typeof(ComTypeLibInfo));
            return copy;
        }
    }
}

#endif
