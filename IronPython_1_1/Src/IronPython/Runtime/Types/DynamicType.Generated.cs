/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    public abstract partial class DynamicType {
        #region Generated DynamicType Binary Ops

        // *** BEGIN GENERATED CODE ***


        public object Add(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpAdd, self, other);
        }
        public object ReverseAdd(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseAdd, self, other);
        }
        public object InPlaceAdd(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceAdd, self, other);
        }


        public object Subtract(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpSubtract, self, other);
        }
        public object ReverseSubtract(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseSubtract, self, other);
        }
        public object InPlaceSubtract(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceSubtract, self, other);
        }


        public object Power(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpPower, self, other);
        }
        public object ReversePower(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReversePower, self, other);
        }
        public object InPlacePower(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlacePower, self, other);
        }


        public object Multiply(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpMultiply, self, other);
        }
        public object ReverseMultiply(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseMultiply, self, other);
        }
        public object InPlaceMultiply(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceMultiply, self, other);
        }


        public object FloorDivide(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpFloorDivide, self, other);
        }
        public object ReverseFloorDivide(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseFloorDivide, self, other);
        }
        public object InPlaceFloorDivide(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceFloorDivide, self, other);
        }


        public object Divide(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpDivide, self, other);
        }
        public object ReverseDivide(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseDivide, self, other);
        }
        public object InPlaceDivide(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceDivide, self, other);
        }


        public object TrueDivide(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpTrueDivide, self, other);
        }
        public object ReverseTrueDivide(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseTrueDivide, self, other);
        }
        public object InPlaceTrueDivide(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceTrueDivide, self, other);
        }


        public object Mod(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpMod, self, other);
        }
        public object ReverseMod(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseMod, self, other);
        }
        public object InPlaceMod(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceMod, self, other);
        }


        public object LeftShift(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpLeftShift, self, other);
        }
        public object ReverseLeftShift(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseLeftShift, self, other);
        }
        public object InPlaceLeftShift(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceLeftShift, self, other);
        }


        public object RightShift(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpRightShift, self, other);
        }
        public object ReverseRightShift(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseRightShift, self, other);
        }
        public object InPlaceRightShift(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceRightShift, self, other);
        }


        public object BitwiseAnd(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpBitwiseAnd, self, other);
        }
        public object ReverseBitwiseAnd(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseBitwiseAnd, self, other);
        }
        public object InPlaceBitwiseAnd(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceBitwiseAnd, self, other);
        }


        public object BitwiseOr(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpBitwiseOr, self, other);
        }
        public object ReverseBitwiseOr(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseBitwiseOr, self, other);
        }
        public object InPlaceBitwiseOr(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceBitwiseOr, self, other);
        }


        public object Xor(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpXor, self, other);
        }
        public object ReverseXor(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpReverseXor, self, other);
        }
        public object InPlaceXor(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpInPlaceXor, self, other);
        }


        public object LessThan(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpLessThan, self, other);
        }

        public object GreaterThan(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpGreaterThan, self, other);
        }

        public object LessThanOrEqual(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpLessThanOrEqual, self, other);
        }

        public object GreaterThanOrEqual(object self, object other) {
            return InvokeBinaryOperator(SymbolTable.OpGreaterThanOrEqual, self, other);
        }

        // *** END GENERATED CODE ***

        #endregion

    }
}
