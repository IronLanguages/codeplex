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
    public abstract class CreateAction : StandardAction {
        private readonly CallSignature _signature;

        protected CreateAction(CallSignature signature)
            : base(StandardActionKind.Create) {
            _signature = signature;
        }

        public CallSignature Signature {
            get {
                return _signature;
            }
        }

        public sealed override MetaObject Bind(MetaObject[] args) {
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.Requires(args.Length > 0);
            return args[0].Create(this, args);
        }

        [Confined]
        public override bool Equals(object obj) {
            CreateAction ca = obj as CreateAction;
            return ca != null && ca._signature.Equals(_signature);
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28 ^ _signature.GetHashCode());
        }
    }
}
