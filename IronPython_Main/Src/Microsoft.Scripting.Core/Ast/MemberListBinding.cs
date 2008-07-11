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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class MemberListBinding : MemberBinding {
        ReadOnlyCollection<ElementInit> _initializers;
        internal MemberListBinding(MemberInfo member, ReadOnlyCollection<ElementInit> initializers)
            : base(MemberBindingType.ListBinding, member) {
            _initializers = initializers;
        }
        public ReadOnlyCollection<ElementInit> Initializers {
            get { return _initializers; }
        }
        internal override void BuildString(StringBuilder builder) {
            builder.Append(Member.Name);
            builder.Append(" = {");
            for (int i = 0, n = _initializers.Count; i < n; i++) {
                if (i > 0) {
                    builder.Append(", ");
                }
                _initializers[i].BuildString(builder);
            }
            builder.Append("}");
        }
    }
    

    public partial class Expression {
        //CONFORMING
        public static MemberListBinding ListBind(MemberInfo member, params ElementInit[] initializers) {
            ContractUtils.RequiresNotNull(member, "member");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListBind(member, CollectionUtils.ToReadOnlyCollection(initializers));
        }
        //CONFORMING
        public static MemberListBinding ListBind(MemberInfo member, IEnumerable<ElementInit> initializers) {
            ContractUtils.RequiresNotNull(member, "member");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            Type memberType;
            ValidateGettableFieldOrPropertyMember(member, out memberType);
            ReadOnlyCollection<ElementInit> initList = CollectionUtils.ToReadOnlyCollection(initializers);
            ValidateListInitArgs(memberType, initList);
            return new MemberListBinding(member, initList);
        }
        //CONFORMING
        public static MemberListBinding ListBind(MethodInfo propertyAccessor, params ElementInit[] initializers) {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListBind(propertyAccessor, CollectionUtils.ToReadOnlyCollection(initializers));
        }
        //CONFORMING
        public static MemberListBinding ListBind(MethodInfo propertyAccessor, IEnumerable<ElementInit> initializers) {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListBind(GetProperty(propertyAccessor), initializers);
        }

        //CONFORMING
        private static void ValidateListInitArgs(Type listType, ReadOnlyCollection<ElementInit> initializers) {
            if (!TypeUtils.AreAssignable(typeof(IEnumerable), listType)) {
                throw Error.TypeNotIEnumerable(listType);
            }
            for (int i = 0, n = initializers.Count; i < n; i++) {
                ElementInit element = initializers[i];
                ContractUtils.RequiresNotNull(element, "initializers");
                ValidateCallInstanceType(listType, element.AddMethod);
            }
        }
    }
}