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
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    internal class DeferMetaObject : MetaObject {
        internal DeferMetaObject(Expression expression, Restrictions restrictions)
            : base(expression, restrictions) {
        }

        public override MetaObject Call(CallAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(args.AddFirst(this));
        }

        public override MetaObject Convert(ConvertAction action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(this);
        }

        public override MetaObject Create(CreateAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(args.AddFirst(this));
        }

        public override MetaObject DeleteMember(DeleteMemberAction action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(this);
        }

        public override MetaObject Operation(OperationAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(args);
        }

        public override MetaObject GetMember(GetMemberAction action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(this);
        }

        public override MetaObject SetMember(SetMemberAction action, MetaObject value) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(this, value);
        }
    }
}
