/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
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
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorAdd, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseAdd, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator +(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseAdd, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__iadd__")]
        public object InPlaceAdd(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceAdd, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator -([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorSubtract, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseSubtract, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator -(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseSubtract, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__isub__")]
        public object InPlaceSubtract(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceSubtract, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object Power([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorPower, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReversePower, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object Power(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReversePower, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ipow__")]
        public object InPlacePower(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlacePower, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator *([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorMultiply, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseMultiply, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator *(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseMultiply, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__imul__")]
        public object InPlaceMultiply(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceMultiply, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object FloorDivide([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorFloorDivide, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseFloorDivide, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object FloorDivide(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseFloorDivide, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ifloordiv__")]
        public object InPlaceFloorDivide(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceFloorDivide, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator /([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorDivide, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseDivide, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator /(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseDivide, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__idiv__")]
        public object InPlaceDivide(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceDivide, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object TrueDivide([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorTrueDivide, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseTrueDivide, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object TrueDivide(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseTrueDivide, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__itruediv__")]
        public object InPlaceTrueDivide(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceTrueDivide, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator %([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorMod, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseMod, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator %(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseMod, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__imod__")]
        public object InPlaceMod(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceMod, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object LeftShift([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorLeftShift, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseLeftShift, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object LeftShift(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseLeftShift, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ilshift__")]
        public object InPlaceLeftShift(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceLeftShift, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object RightShift([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorRightShift, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseRightShift, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object RightShift(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseRightShift, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__irshift__")]
        public object InPlaceRightShift(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceRightShift, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator &([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorBitwiseAnd, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseBitwiseAnd, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator &(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseBitwiseAnd, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__iand__")]
        public object InPlaceBitwiseAnd(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceBitwiseAnd, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator |([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorBitwiseOr, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseBitwiseOr, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator |(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseBitwiseOr, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ior__")]
        public object InPlaceBitwiseOr(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceBitwiseOr, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator ^([NotNull]OldInstance self, object other) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorXor, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if(res != PythonOps.NotImplemented) return res;
            }

            OldInstance otherOc = other as OldInstance;
            if (otherOc != null && otherOc.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseXor, out value)) {
                return PythonOps.CallWithContext(DefaultContext.Default, value, self);
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator ^(object other, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorReverseXor, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ixor__")]
        public object InPlaceXor(object other) {
            object value;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorInPlaceXor, out value)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, value, other);
                if (res != PythonOps.NotImplemented) return res;
            }
            return PythonOps.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
