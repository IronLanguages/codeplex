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
using System.Diagnostics;

namespace IronPython.Runtime {
    static partial class UInt32Ops {
        #region Unary operators

        [PythonName("__abs__")]
        public static object Abs(object value) {
            Debug.Assert(value is UInt32);
            return value;
        }

        [PythonName("__neg__")]
        public static object Negate(object value) {
            Debug.Assert(value is UInt32);
            UInt32 valueUInt32 = (UInt32)value;
            if (valueUInt32 < Int32.MaxValue) {
                return -(Int32)valueUInt32;
            } else {
                return -(Int64)valueUInt32;
            }
        }

        [PythonName("__pos__")]
        public static object Positive(object value) {
            Debug.Assert(value is UInt32);
            return value;
        }

        [PythonName("__invert__")]
        public static object Invert(object value) {
            Debug.Assert(value is UInt32);
            UInt32 valueUInt32 = (UInt32)value;
            if (valueUInt32 < Int32.MaxValue) {
                return ~(Int32)valueUInt32;
            } else {
                return ~(Int64)value;
            }
        }

        #endregion
        
        internal static object LeftShiftImpl(UInt32 left, UInt32 right) {
            // UInt32 fits into Int64
            return Int64Ops.LeftShift(left, right);
        }
        internal static object PowerImpl(UInt32 left, UInt32 right) {
            // UInt32 fits into Int64
            return Int64Ops.Power(left, right);
        }
        internal static object RightShiftImpl(UInt32 left, UInt32 right) {
            // UInt32 fits into Int64
            return Int64Ops.RightShift(left, right);
        }
    }
}
