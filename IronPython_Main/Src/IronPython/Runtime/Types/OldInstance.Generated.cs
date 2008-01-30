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
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;

namespace IronPython.Runtime.Types {
    public partial class OldInstance {
        #region Generated OldInstance Operators

        // *** BEGIN GENERATED CODE ***

        [return: MaybeNotImplemented]
        public static object operator +([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorAdd);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseAdd);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator +(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseAdd);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__iadd__")]
        public object InPlaceAdd(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceAdd);
        }

        [return: MaybeNotImplemented]
        public static object operator -([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorSubtract);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseSubtract);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator -(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseSubtract);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__isub__")]
        public object InPlaceSubtract(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceSubtract);
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object Power([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorPower);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReversePower);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object Power(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReversePower);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ipow__")]
        public object InPlacePower(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlacePower);
        }

        [return: MaybeNotImplemented]
        public static object operator *([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorMultiply);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseMultiply);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator *(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseMultiply);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__imul__")]
        public object InPlaceMultiply(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceMultiply);
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object FloorDivide([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorFloorDivide);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseFloorDivide);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object FloorDivide(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseFloorDivide);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ifloordiv__")]
        public object InPlaceFloorDivide(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceFloorDivide);
        }

        [return: MaybeNotImplemented]
        public static object operator /([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorDivide);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseDivide);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator /(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseDivide);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__idiv__")]
        public object InPlaceDivide(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceDivide);
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object TrueDivide([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorTrueDivide);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseTrueDivide);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object TrueDivide(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseTrueDivide);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__itruediv__")]
        public object InPlaceTrueDivide(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceTrueDivide);
        }

        [return: MaybeNotImplemented]
        public static object operator %([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorMod);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseMod);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator %(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseMod);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__imod__")]
        public object InPlaceMod(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceMod);
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object LeftShift([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorLeftShift);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseLeftShift);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object LeftShift(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseLeftShift);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ilshift__")]
        public object InPlaceLeftShift(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceLeftShift);
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object RightShift([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorRightShift);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseRightShift);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object RightShift(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseRightShift);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__irshift__")]
        public object InPlaceRightShift(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceRightShift);
        }

        [return: MaybeNotImplemented]
        public static object operator &([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorBitwiseAnd);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseBitwiseAnd);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator &(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseBitwiseAnd);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__iand__")]
        public object InPlaceBitwiseAnd(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceBitwiseAnd);
        }

        [return: MaybeNotImplemented]
        public static object operator |([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorBitwiseOr);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseBitwiseOr);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator |(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseBitwiseOr);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ior__")]
        public object InPlaceBitwiseOr(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceBitwiseOr);
        }

        [return: MaybeNotImplemented]
        public static object operator ^([NotNull]OldInstance self, object other) {
            object res = InvokeOne(self, other, Symbols.OperatorXor);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null) {
                return InvokeOne(other, self, Symbols.OperatorReverseXor);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator ^(object other, [NotNull]OldInstance self) {
            return InvokeOne(self, other, Symbols.OperatorReverseXor);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ixor__")]
        public object InPlaceXor(object other) {
            return InvokeOne(this, other, Symbols.OperatorInPlaceXor);
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
