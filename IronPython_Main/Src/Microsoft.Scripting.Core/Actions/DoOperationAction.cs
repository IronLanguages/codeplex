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

using Microsoft.Contracts;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {

    public class DoOperationAction : DynamicAction {
        private readonly ActionBinder _binder;
        private readonly Operators _operation;

        public static DoOperationAction Make(ActionBinder binder, Operators operation) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return new DoOperationAction(binder, operation);
        }

        private DoOperationAction(ActionBinder binder, Operators operation) {
            _binder = binder;
            _operation = operation;
        }

        public Operators Operation {
            get { return _operation; }
        }

        public override DynamicActionKind Kind {
            get { return DynamicActionKind.DoOperation; }
        }

        public override Rule<T> Bind<T>(object[] args) {
            return _binder.Bind<T>(this, args);
        }

        [Confined]
        public override bool Equals(object obj) {
            DoOperationAction other = obj as DoOperationAction;
            if (other == null) return false;
            return _operation == other._operation && (object)_binder == (object)other._binder;
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28 ^ ((int)_operation)) ^ System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_binder);
        }

        //??? Do these belong here or mone Operators enum
        public bool IsComparision {
            get {
                return CompilerHelpers.IsComparisonOperator(_operation);
            }
        }

        public bool IsUnary {
            get {
                switch(_operation){ 
                    case Operators.OnesComplement:
                    case Operators.Negate:
                    case Operators.Positive:
                    case Operators.AbsoluteValue:
                    case Operators.Not:

                    // Added for COM support...
                    case Operators.Documentation:
                        return true;                    
                }
                return false;
            }
        }

        public bool IsInPlace {
            get {
                return CompilerHelpers.InPlaceOperatorToOperator(_operation) != Operators.None;
            }
        }

        public Operators DirectOperation {
            get {
                Operators res = CompilerHelpers.InPlaceOperatorToOperator(_operation);
                if (res != Operators.None) return res;

                throw new InvalidOperationException();
            }
        }

        [Confined]
        public override string/*!*/ ToString() {
            return base.ToString() + " " + _operation.ToString();
        }
    }
}
