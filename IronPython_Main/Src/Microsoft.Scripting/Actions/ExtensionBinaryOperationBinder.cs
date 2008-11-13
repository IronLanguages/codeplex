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
using Microsoft.Scripting.Binders;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    public abstract class ExtensionBinaryOperationBinder : BinaryOperationBinder {
        private readonly string _operation;

        protected ExtensionBinaryOperationBinder(string operation)
            : base(ExpressionType.Extension) {
            ContractUtils.RequiresNotNull(operation, "operation");
            _operation = operation;
        }

        public string ExtensionOperation {
            get {
                return _operation;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _operation.GetHashCode();
        }

        public override bool Equals(object obj) {
            var ebob = obj as ExtensionBinaryOperationBinder;
            return ebob != null && base.Equals(obj) && _operation == ebob._operation;
        }
    }
}
