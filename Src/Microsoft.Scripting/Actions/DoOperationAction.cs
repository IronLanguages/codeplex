/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Actions {

    public class DoOperationAction : Action {
        private Operators _operation;

        public static DoOperationAction Make(string operation) {
            Operators op = (Operators)typeof(Operators).GetField(operation).GetValue(null);
            return Make(op);
        }
        
        public static DoOperationAction Make(Operators operation) {
            return new DoOperationAction(operation);
        }

        private DoOperationAction(Operators operation) {
            this._operation = operation;
        }

        public Operators Operation {
            get { return _operation; }
        }

        public override ActionKind Kind {
            get { return ActionKind.DoOperation; }
        }

        public override string ParameterString {
            get { return _operation.ToString(); }
        }

        public override bool Equals(object obj) {
            DoOperationAction other = obj as DoOperationAction;
            if (other == null) return false;
            return _operation == other._operation;
        }

        public override int GetHashCode() {
            return (int)Kind << 28 ^ ((int)_operation) ;
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
                    case Operators.ConvertToBigInteger:
                    case Operators.ConvertToBoolean:
                    case Operators.ConvertToComplex:
                    case Operators.ConvertToDouble:
                    case Operators.ConvertToHex:
                    case Operators.ConvertToInt32:
                    case Operators.ConvertToOctal:
                    case Operators.ConvertToString:
                    case Operators.Not:
                        return true;                    
                }
                return false;
            }
        }

        public bool IsInPlace {
            get {
                switch (_operation) {
                    case Operators.InPlaceAdd: return true;
                    case Operators.InPlaceBitwiseAnd: return true;
                    case Operators.InPlaceBitwiseOr: return true;
                    case Operators.InPlaceDivide: return true;
                    case Operators.InPlaceFloorDivide: return true;
                    case Operators.InPlaceLeftShift: return true;
                    case Operators.InPlaceMod: return true;
                    case Operators.InPlaceMultiply: return true;
                    case Operators.InPlacePower: return true;
                    case Operators.InPlaceRightShift: return true;
                    case Operators.InPlaceSubtract: return true;
                    case Operators.InPlaceTrueDivide: return true;
                    case Operators.InPlaceXor: return true;
                    case Operators.InPlaceRightShiftUnsigned: return true;
                }
                return false;
            }
        }

        public Operators DirectOperation {
            get {
                switch (_operation) {
                    case Operators.InPlaceAdd: return Operators.Add;
                    case Operators.InPlaceBitwiseAnd: return Operators.BitwiseAnd;
                    case Operators.InPlaceBitwiseOr: return Operators.BitwiseOr;
                    case Operators.InPlaceDivide: return Operators.Divide;
                    case Operators.InPlaceFloorDivide: return Operators.FloorDivide;
                    case Operators.InPlaceLeftShift: return Operators.LeftShift;
                    case Operators.InPlaceMod: return Operators.Mod;
                    case Operators.InPlaceMultiply: return Operators.Multiply;
                    case Operators.InPlacePower: return Operators.Power;
                    case Operators.InPlaceRightShift: return Operators.RightShift;
                    case Operators.InPlaceSubtract: return Operators.Subtract;
                    case Operators.InPlaceTrueDivide: return Operators.TrueDivide;
                    case Operators.InPlaceXor: return Operators.Xor;
                    case Operators.InPlaceRightShiftUnsigned: return Operators.RightShiftUnsigned;
                    default: throw new NotImplementedException();
                }
            }
        }
    }
}
