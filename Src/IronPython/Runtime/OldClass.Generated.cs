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

namespace IronPython.Runtime {
    partial class OldInstanceType {
        #region Generated OldClass Binary Ops

        // *** BEGIN GENERATED CODE ***


        public override object Add(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpAdd, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseAdd(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseAdd, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceAdd(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceAdd, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object Subtract(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpSubtract, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseSubtract(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseSubtract, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceSubtract(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceSubtract, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object Power(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpPower, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReversePower(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReversePower, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlacePower(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlacePower, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object Multiply(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpMultiply, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseMultiply(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseMultiply, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceMultiply(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceMultiply, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object FloorDivide(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpFloorDivide, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseFloorDivide(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseFloorDivide, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceFloorDivide(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceFloorDivide, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object Divide(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpDivide, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseDivide(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseDivide, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceDivide(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceDivide, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object TrueDivide(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpTrueDivide, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseTrueDivide(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseTrueDivide, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceTrueDivide(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceTrueDivide, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object Mod(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpMod, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseMod(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseMod, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceMod(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceMod, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object LeftShift(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpLeftShift, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseLeftShift(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseLeftShift, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceLeftShift(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceLeftShift, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object RightShift(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpRightShift, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseRightShift(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseRightShift, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceRightShift(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceRightShift, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object BitwiseAnd(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpBitwiseAnd, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseBitwiseAnd(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseBitwiseAnd, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceBitwiseAnd(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceBitwiseAnd, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object BitwiseOr(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpBitwiseOr, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseBitwiseOr(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseBitwiseOr, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceBitwiseOr(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceBitwiseOr, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object Xor(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpXor, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }
        public override object ReverseXor(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpReverseXor, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;

        }
        public override object InPlaceXor(object self, object other) {
            object func;
            OldInstance.DoCoerce(ref self, ref other);

            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceXor, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object LessThan(object self, object other) {
            object func;

            if (Ops.TryGetAttr(self, SymbolTable.OpLessThan, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object GreaterThan(object self, object other) {
            object func;

            if (Ops.TryGetAttr(self, SymbolTable.OpGreaterThan, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object LessThanOrEqual(object self, object other) {
            object func;

            if (Ops.TryGetAttr(self, SymbolTable.OpLessThanOrEqual, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object GreaterThanOrEqual(object self, object other) {
            object func;

            if (Ops.TryGetAttr(self, SymbolTable.OpGreaterThanOrEqual, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object Equal(object self, object other) {
            object func;

            if (Ops.TryGetAttr(self, SymbolTable.OpEqual, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        public override object NotEqual(object self, object other) {
            object func;

            if (Ops.TryGetAttr(self, SymbolTable.OpNotEqual, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        // *** END GENERATED CODE ***

        #endregion

    }
}
