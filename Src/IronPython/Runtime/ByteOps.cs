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

using IronPython.Runtime;

namespace IronPython.Runtime.Operations {
    static partial class ByteOps {
        #region Unary operators
        [PythonName("__abs__")]
        public static object Abs(object value) {
            Debug.Assert(value is Byte);
            return value;
        }

        [PythonName("__neg__")]
        public static object Negate(object value) {
            Debug.Assert(value is Byte);
            return -(Int16)(Byte)value;
        }

        [PythonName("__pos__")]
        public static object Positive(object value) {
            Debug.Assert(value is Byte);
            return value;
        }

        [PythonName("__invert__")]
        public static object Invert(object value) {
            Debug.Assert(value is Byte);
            return ~(Byte)value;
        }

        #endregion

        internal static object LeftShiftImpl(Byte left, Byte right) {
            return IntOps.LeftShift((int)left, (int)right);
        }
        internal static object PowerImpl(Byte left, Byte right) {
            return IntOps.Power((int)left, (int)right);
        }
        internal static object RightShiftImpl(Byte left, Byte right) {
            return IntOps.RightShift((int)left, (int)right);
        }
    }
}
