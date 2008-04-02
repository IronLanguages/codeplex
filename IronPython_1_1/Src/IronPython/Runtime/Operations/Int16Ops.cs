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
using System.Diagnostics;

using IronPython.Runtime;

namespace IronPython.Runtime.Operations {
    static partial class Int16Ops {
        #region Unary operators
        [PythonName("__abs__")]
        public static object Abs(object value) {
            Debug.Assert(value is Int16);
            Int16 valueInt16 = (Int16)value;
            if (valueInt16 < 0) {
                if (valueInt16 == Int16.MinValue) {
                    // would overflow
                    return -(Int32)Int16.MinValue;
                } else return -valueInt16;
            }
            return valueInt16;
        }

        [PythonName("__neg__")]
        public static object Negate(object value) {
            Debug.Assert(value is Int16);
            Int16 valueInt16 = (Int16)value;
            if (valueInt16 == Int16.MinValue) {
                // would overflow
                return -(Int32)Int16.MinValue;
            } else return -valueInt16;
        }

        [PythonName("__pos__")]
        public static object Positive(object value) {
            Debug.Assert(value is Int16);
            return value;
        }

        [PythonName("__invert__")]
        public static object Invert(object value) {
            Debug.Assert(value is Int16);
            return ~(Int16)value;
        }

        #endregion

        internal static object LeftShiftImpl(Int16 left, Int16 right) {
            return IntOps.LeftShift((int)left, (int)right);
        }
        internal static object PowerImpl(Int16 left, Int16 right) {
            return IntOps.Power((int)left, (int)right);
        }
        internal static object RightShiftImpl(Int16 left, Int16 right) {
            return IntOps.RightShift((int)left, (int)right);
        }
    }
}
