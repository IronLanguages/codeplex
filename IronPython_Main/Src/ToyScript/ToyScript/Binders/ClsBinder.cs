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

using System;
using System.Reflection;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Generation;

namespace ToyScript.Binders {
    public abstract class ClsBinder {
        internal static MetaObject GetMemberOnType(Type type, string name, Expression expression, Restrictions restrictions) {
            MemberInfo[] members = type.GetMember(name);
            if (members == null || members.Length != 1) {
                return new ErrorMetaObject(typeof(MissingMemberException), "No or ambiguous member " + name, restrictions);
            }

            MemberInfo member = members[0];
            switch (member.MemberType) {
                case MemberTypes.Field:
                    return new ClsMetaObject(Expression.Field(expression, (FieldInfo)member), restrictions);
                case MemberTypes.Property:
                    return new ClsMetaObject(Expression.Property(expression, (PropertyInfo)member), restrictions);
                default:
                    return new ErrorMetaObject(typeof(MissingMemberException), "Wrong member " + name, restrictions);
            }
        }

        internal static MetaObject BindObjectGetMember(GetMemberBinder action, MetaObject []args) {
            MetaObject arg = args[0];
            Type type = arg.RuntimeType;
            if (type == null) {
                return action.Defer(args);
            }

            MetaObject restricted = arg.Restrict(type);

            if (CompilerHelpers.IsStrongBox(arg.Value)) {
                MetaObject box = new ClsMetaObject(
                    Expression.Field(restricted.Expression, "Value"),
                    restricted.Restrictions
                );
                return box.GetMember(action, new MetaObject[] { box });
            } else {
                return GetMemberOnType(restricted.LimitType, action.Name, restricted.Expression, restricted.Restrictions);
            }
        }

        internal static MetaObject BindTypeGetMember(GetMemberBinder action, MetaObject[] args) {
            MetaObject arg = args[0];
            Type type = arg.LimitType;
            if (!type.IsSealed) {
                return action.Defer(args);
            }

            return GetMemberOnType(type, action.Name, arg.Expression, arg.Restrictions);
        }
    }
}
