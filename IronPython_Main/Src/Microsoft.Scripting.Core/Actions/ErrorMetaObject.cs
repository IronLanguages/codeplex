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

using System.Linq.Expressions;
using System.Scripting.Utils;

namespace System.Scripting.Actions {
    public class ErrorMetaObject : MetaObject {
        public ErrorMetaObject(Type exception, string message, Restrictions restrictions)
            : base(CreateThrow(exception, message), restrictions) {
        }

        private static Expression CreateThrow(Type exception, string message) {
            ContractUtils.RequiresNotNull(exception, "exception");
            ContractUtils.Requires(typeof(Exception).IsAssignableFrom(exception), "exception");
            return Expression.Throw(
                Expression.New(
                    exception.GetConstructor(new Type[] { typeof(string) }),
                    Expression.Constant(message)
                )
            );
        }

        public override MetaObject Call(CallAction action, MetaObject[] args) {
            return this;
        }

        public override MetaObject Convert(ConvertAction action, MetaObject[] args) {
            return this;
        }

        public override MetaObject Create(CreateAction action, MetaObject[] args) {
            return this;
        }

        public override MetaObject DeleteMember(DeleteMemberAction action, MetaObject[] args) {
            return this;
        }

        public override MetaObject Operation(OperationAction action, MetaObject[] args) {
            return this;
        }

        public override MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
            return this;
        }

        public override MetaObject SetMember(SetMemberAction action, MetaObject[] args) {
            return this;
        }
    }
}
