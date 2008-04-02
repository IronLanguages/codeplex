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
    static partial class SByteOps {
        #region Unary operators
        [PythonName("__abs__")]
        public static object Abs(object value) {
            Debug.Assert(value is SByte);
            SByte valueSByte = (SByte)value;
            if (valueSByte < 0) {
                if (valueSByte == SByte.MinValue) {
                    // would overflow
                    return -(Int16)SByte.MinValue;
                } else return -valueSByte;
            }
            return valueSByte;
        }

        [PythonName("__neg__")]
        public static object Negate(object value) {
            Debug.Assert(value is SByte);
            SByte valueSByte = (SByte)value;
            if (valueSByte == SByte.MinValue) {
                // would overflow
                return -(Int16)SByte.MinValue;
            } else return -valueSByte;
        }

        [PythonName("__pos__")]
        public static object Positive(object value) {
            Debug.Assert(value is SByte);
            return value;
        }

        [PythonName("__invert__")]
        public static object Invert(object value) {
            Debug.Assert(value is SByte);
            return ~(SByte)value;
        }

        #endregion

        internal static object LeftShiftImpl(SByte left, SByte right) {
            return IntOps.LeftShift((int)left, (int)right);
        }
        internal static object PowerImpl(SByte left, SByte right) {
            return IntOps.Power((int)left, (int)right);
        }
        internal static object RightShiftImpl(SByte left, SByte right) {
            return IntOps.RightShift((int)left, (int)right);
        }
    }
}
