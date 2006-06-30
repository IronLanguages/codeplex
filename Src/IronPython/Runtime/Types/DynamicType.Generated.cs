/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    public abstract partial class DynamicType {
        #region Generated DynamicType Binary Ops

        // *** BEGIN GENERATED CODE ***


        public virtual object Add(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpAdd, self, other);
        }
        public virtual object ReverseAdd(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseAdd, self, other);
        }
        public virtual object InPlaceAdd(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceAdd, self, other);
        }


        public virtual object Subtract(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpSubtract, self, other);
        }
        public virtual object ReverseSubtract(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseSubtract, self, other);
        }
        public virtual object InPlaceSubtract(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceSubtract, self, other);
        }


        public virtual object Power(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpPower, self, other);
        }
        public virtual object ReversePower(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReversePower, self, other);
        }
        public virtual object InPlacePower(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlacePower, self, other);
        }


        public virtual object Multiply(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpMultiply, self, other);
        }
        public virtual object ReverseMultiply(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseMultiply, self, other);
        }
        public virtual object InPlaceMultiply(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceMultiply, self, other);
        }


        public virtual object FloorDivide(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpFloorDivide, self, other);
        }
        public virtual object ReverseFloorDivide(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseFloorDivide, self, other);
        }
        public virtual object InPlaceFloorDivide(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceFloorDivide, self, other);
        }


        public virtual object Divide(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpDivide, self, other);
        }
        public virtual object ReverseDivide(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseDivide, self, other);
        }
        public virtual object InPlaceDivide(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceDivide, self, other);
        }


        public virtual object TrueDivide(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpTrueDivide, self, other);
        }
        public virtual object ReverseTrueDivide(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseTrueDivide, self, other);
        }
        public virtual object InPlaceTrueDivide(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceTrueDivide, self, other);
        }


        public virtual object Mod(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpMod, self, other);
        }
        public virtual object ReverseMod(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseMod, self, other);
        }
        public virtual object InPlaceMod(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceMod, self, other);
        }


        public virtual object LeftShift(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpLeftShift, self, other);
        }
        public virtual object ReverseLeftShift(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseLeftShift, self, other);
        }
        public virtual object InPlaceLeftShift(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceLeftShift, self, other);
        }


        public virtual object RightShift(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpRightShift, self, other);
        }
        public virtual object ReverseRightShift(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseRightShift, self, other);
        }
        public virtual object InPlaceRightShift(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceRightShift, self, other);
        }


        public virtual object BitwiseAnd(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpBitwiseAnd, self, other);
        }
        public virtual object ReverseBitwiseAnd(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseBitwiseAnd, self, other);
        }
        public virtual object InPlaceBitwiseAnd(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceBitwiseAnd, self, other);
        }


        public virtual object BitwiseOr(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpBitwiseOr, self, other);
        }
        public virtual object ReverseBitwiseOr(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseBitwiseOr, self, other);
        }
        public virtual object InPlaceBitwiseOr(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceBitwiseOr, self, other);
        }


        public virtual object Xor(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpXor, self, other);
        }
        public virtual object ReverseXor(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpReverseXor, self, other);
        }
        public virtual object InPlaceXor(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpInPlaceXor, self, other);
        }


        public object LessThan(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpLessThan, self, other);
        }

        public object GreaterThan(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpGreaterThan, self, other);
        }

        public object LessThanOrEqual(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpLessThanOrEqual, self, other);
        }

        public object GreaterThanOrEqual(object self, object other) {
            return CallBinaryOperator(SymbolTable.OpGreaterThanOrEqual, self, other);
        }

        // *** END GENERATED CODE ***

        #endregion

    }
}
