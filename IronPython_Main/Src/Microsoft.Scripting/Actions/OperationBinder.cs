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

using Microsoft.Scripting;
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;
using System; using Microsoft;

namespace Microsoft.Scripting.Actions {
    [Obsolete("Use ExtensionBinaryOperationBinder or ExtensionUnaryOperationBinder")]
    public abstract class OperationBinder : MetaObjectBinder {
        private string _operation;

        protected OperationBinder(string operation) {
            _operation = operation;
        }

        public string Operation {
            get {
                return _operation;
            }
        }

        public MetaObject FallbackOperation(MetaObject target, MetaObject[] args) {
            return FallbackOperation(target, args, null);
        }

        public abstract MetaObject FallbackOperation(MetaObject target, MetaObject[] args, MetaObject errorSuggestion);

        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            // Try to call BindOperation
            var emo = target as OperationMetaObject;
            if (emo != null) {
                return emo.BindOperation(this, args);
            }

            // Otherwise just fall back (it's as if they didn't override BindOperation)
            return FallbackOperation(target, args);
        }

        [Confined]
        public override bool Equals(object obj) {
            OperationBinder oa = obj as OperationBinder;
            return oa != null && oa._operation == _operation;
        }

        [Confined]
        public override int GetHashCode() {
            return 0x10000000 ^ _operation.GetHashCode();
        }
    }
}
