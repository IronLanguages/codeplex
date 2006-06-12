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
    static partial class UInt16Ops {
        #region Unary operators

        [PythonName("__abs__")]
        public static object Abs(object value) {
            Debug.Assert(value is UInt16);
            return value;
        }

        [PythonName("__neg__")]
        public static object Negate(object value) {
            Debug.Assert(value is UInt16);
            return -(Int32)(UInt16)value;
        }

        [PythonName("__pos__")]
        public static object Positive(object value) {
            Debug.Assert(value is UInt16);
            return value;
        }

        [PythonName("__invert__")]
        public static object Invert(object value) {
            Debug.Assert(value is UInt16);
            return ~(UInt16)value;
        }

        #endregion

        internal static object LeftShiftImpl(UInt16 left, UInt16 right) {
            // UInt16 fits into Int32
            return IntOps.LeftShift((int)left, (int)right);
        }
        internal static object PowerImpl(UInt16 left, UInt16 right) {
            // UInt16 fits into Int32
            return IntOps.Power((int)left, (int)right);
        }
        internal static object RightShiftImpl(UInt16 left, UInt16 right) {
            // UInt16 fits into Int32
            return IntOps.RightShift((int)left, (int)right);
        }
    }
}
