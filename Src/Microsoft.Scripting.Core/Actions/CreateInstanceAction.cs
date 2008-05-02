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
    public class CreateInstanceAction : CallAction, IEquatable<CreateInstanceAction> {
        protected CreateInstanceAction(ActionBinder binder, CallSignature callSignature)
            : base(binder, callSignature) {
        }

        public static new CreateInstanceAction Make(ActionBinder binder, CallSignature signature) {
            return new CreateInstanceAction(binder, signature);
        }

        public static new CreateInstanceAction Make(ActionBinder binder, int argumentCount) {
            ContractUtils.Requires(argumentCount >= 0, "argumentCount");
            return new CreateInstanceAction(binder, new CallSignature(argumentCount));
        }

        [Confined]
        public override bool Equals(object obj) {
            return Equals(obj as CreateInstanceAction);
        }

        [StateIndependent]
        public bool Equals(CreateInstanceAction other) {
            return base.Equals(other);
        }

        [Confined]
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override DynamicActionKind Kind {
            get {
                return DynamicActionKind.CreateInstance;
            }
        }
    }
}
