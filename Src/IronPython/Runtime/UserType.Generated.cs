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
    //41 operators
    partial class UserType {
        #region Generated UserType Binary Operators

        // *** BEGIN GENERATED CODE ***


        public override object Add(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpAdd, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseAdd(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseAdd, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceAdd(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceAdd, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceAdd(self, other);
        }

        public override object Subtract(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpSubtract, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseSubtract(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseSubtract, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceSubtract(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceSubtract, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceSubtract(self, other);
        }

        public override object Power(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpPower, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReversePower(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReversePower, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlacePower(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlacePower, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlacePower(self, other);
        }

        public override object Multiply(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpMultiply, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseMultiply(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseMultiply, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceMultiply(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceMultiply, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceMultiply(self, other);
        }

        public override object FloorDivide(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpFloorDivide, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseFloorDivide(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseFloorDivide, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceFloorDivide(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceFloorDivide, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceFloorDivide(self, other);
        }

        public override object Divide(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpDivide, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseDivide(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseDivide, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceDivide(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceDivide, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceDivide(self, other);
        }

        public override object TrueDivide(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpTrueDivide, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseTrueDivide(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseTrueDivide, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceTrueDivide(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceTrueDivide, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceTrueDivide(self, other);
        }

        public override object Mod(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpMod, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseMod(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseMod, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceMod(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceMod, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceMod(self, other);
        }

        public override object LeftShift(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpLeftShift, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseLeftShift(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseLeftShift, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceLeftShift(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceLeftShift, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceLeftShift(self, other);
        }

        public override object RightShift(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpRightShift, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseRightShift(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseRightShift, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceRightShift(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceRightShift, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceRightShift(self, other);
        }

        public override object BitwiseAnd(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpBitwiseAnd, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseBitwiseAnd(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseBitwiseAnd, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceBitwiseAnd(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceBitwiseAnd, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceBitwiseAnd(self, other);
        }

        public override object BitwiseOr(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpBitwiseOr, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseBitwiseOr(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseBitwiseOr, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceBitwiseOr(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceBitwiseOr, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceBitwiseOr(self, other);
        }

        public override object Xor(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpXor, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }
        public override object ReverseXor(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpReverseXor, out func)){
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;

        }
        public override object InPlaceXor(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpInPlaceXor, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return base.InPlaceXor(self, other);
        }

        public override object LessThan(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpLessThan, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }

        public override object GreaterThan(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpGreaterThan, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }

        public override object LessThanOrEqual(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpLessThanOrEqual, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }

        public override object GreaterThanOrEqual(object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpGreaterThanOrEqual, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }
            return Ops.NotImplemented;
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
