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

using Microsoft.Contracts;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    // TODO: rename to match InvocationExpression
    public class CallAction : DynamicAction, IEquatable<CallAction> {
        private readonly CallSignature _signature;

        protected CallAction(ActionBinder binder, CallSignature callSignature)
            : base(binder) {
            _signature = callSignature;
        }

        public static CallAction Make(ActionBinder binder, CallSignature signature) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return new CallAction(binder, signature);
        }

        public static CallAction Make(ActionBinder binder, int argumentCount) {
            ContractUtils.Requires(argumentCount >= 0, "argumentCount");
            ContractUtils.RequiresNotNull(binder, "binder");
            return new CallAction(binder, new CallSignature(argumentCount));
        }

        public CallSignature Signature {
            get { return _signature; }
        }

        public override DynamicActionKind Kind {
            get { return DynamicActionKind.Call; }
        }

        [StateIndependent]
        public bool Equals(CallAction other) {
            if (other == null || other.GetType() != GetType()) return false;
            return _signature.Equals(other._signature);
        }

        [Confined]
        public override bool Equals(object obj) {
            return Equals(obj as CallAction);
        }

        [Confined]
        public override int GetHashCode() {
            return (int)Kind << 28 ^ _signature.GetHashCode();
        }

        [Confined]
        public override string/*!*/ ToString() {
            return base.ToString() + _signature.ToString();
        }
    }
}


