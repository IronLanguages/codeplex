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
    public abstract partial class DynamicType {
        #region Generated DynamicType Binary Ops

        // *** BEGIN GENERATED CODE ***


        public virtual object Add(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseAdd(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceAdd(object self, object other) {
            return Ops.Add(self, other);
        }

        public virtual object Subtract(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseSubtract(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceSubtract(object self, object other) {
            return Ops.Subtract(self, other);
        }

        public virtual object Power(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReversePower(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlacePower(object self, object other) {
            return Ops.Power(self, other);
        }

        public virtual object Multiply(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseMultiply(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceMultiply(object self, object other) {
            return Ops.Multiply(self, other);
        }

        public virtual object FloorDivide(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseFloorDivide(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceFloorDivide(object self, object other) {
            return Ops.FloorDivide(self, other);
        }

        public virtual object Divide(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseDivide(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceDivide(object self, object other) {
            return Ops.Divide(self, other);
        }

        public virtual object TrueDivide(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseTrueDivide(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceTrueDivide(object self, object other) {
            return Ops.TrueDivide(self, other);
        }

        public virtual object Mod(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseMod(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceMod(object self, object other) {
            return Ops.Mod(self, other);
        }

        public virtual object LeftShift(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseLeftShift(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceLeftShift(object self, object other) {
            return Ops.LeftShift(self, other);
        }

        public virtual object RightShift(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseRightShift(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceRightShift(object self, object other) {
            return Ops.RightShift(self, other);
        }

        public virtual object BitwiseAnd(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseBitwiseAnd(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceBitwiseAnd(object self, object other) {
            return Ops.BitwiseAnd(self, other);
        }

        public virtual object BitwiseOr(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseBitwiseOr(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceBitwiseOr(object self, object other) {
            return Ops.BitwiseOr(self, other);
        }

        public virtual object Xor(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object ReverseXor(object self, object other) {
            return Ops.NotImplemented;
        }
        public virtual object InPlaceXor(object self, object other) {
            return Ops.Xor(self, other);
        }

        public virtual object LessThan(object self, object other) {
            return Ops.NotImplemented;
        }

        public virtual object GreaterThan(object self, object other) {
            return Ops.NotImplemented;
        }

        public virtual object LessThanOrEqual(object self, object other) {
            return Ops.NotImplemented;
        }

        public virtual object GreaterThanOrEqual(object self, object other) {
            return Ops.NotImplemented;
        }

        public virtual object Equal(object self, object other) {
            return Ops.NotImplemented;
        }

        public virtual object NotEqual(object self, object other) {
            return Ops.NotImplemented;
        }

        // *** END GENERATED CODE ***

        #endregion

    }
}
