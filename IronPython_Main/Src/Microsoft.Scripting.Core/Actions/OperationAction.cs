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

using System.Scripting.Utils;
using Microsoft.Contracts;

namespace System.Scripting.Actions {
    public abstract class OperationAction : StandardAction {
        private string _operation;

        protected OperationAction(string operation)
            : base(StandardActionKind.Operation) {
            _operation = operation;
        }

        public string Operation {
            get {
                return _operation;
            }
        }

        public sealed override MetaObject Bind(MetaObject[] args) {
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.Requires(args.Length > 0);
            return args[0].Operation(this, args);
        }

        [Confined]
        public override bool Equals(object obj) {
            OperationAction oa = obj as OperationAction;
            return oa != null && oa._operation == _operation;
        }

        [Confined]
        public override int GetHashCode() {
            return (int)Kind << 28 ^ _operation.GetHashCode();
        }
    }
}
