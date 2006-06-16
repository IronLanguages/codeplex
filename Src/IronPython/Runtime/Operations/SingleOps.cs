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
    static partial class SingleOps {
        #region Unary operators

        [PythonName("__abs__")]
        public static object Abs(object value) {
            Debug.Assert(value is Single);
            Single valueSingle = (Single)value;
            if (valueSingle < 0) valueSingle = -valueSingle;
            return valueSingle;
        }

        [PythonName("__neg__")]
        public static object Negate(object value) {
            Debug.Assert(value is Single);
            return -(Single)value;
        }

        [PythonName("__pos__")]
        public static object Positive(object value) {
            return value;
        }

        #endregion

        internal static object DivideImpl(Single left, Single right) {
            if (right == 0.0) throw Ops.ZeroDivisionError();
            return left / right;
        }

        internal static object ReverseDivideImpl(Single left, Single right) {
            return DivideImpl(right, left);
        }

        internal static object FloorDivideImpl(Single left, Single right) {
            if (right == 0.0) throw Ops.ZeroDivisionError();
            return Math.Floor(left / right);
        }

        internal static object ReverseFloorDivideImpl(Single left, Single right) {
            return FloorDivideImpl(right, left);
        }

        internal static double ModImpl(Single left, Single right) {
            if (right == 0.0) throw Ops.ZeroDivisionError();

            double r = left % right;
            if (r > 0 && right < 0) {
                r = r + right;
            } else if (r < 0 && right > 0) {
                r = r + right;
            }
            return r;
        }

        internal static double ReverseModImpl(Single left, Single right) {
            return ModImpl(right, left);
        }

        internal static object DivModImpl(Single left, Single right) {
            object div = FloorDivideImpl(left, right);
            if (div == Ops.NotImplemented) return div;
            object mod = ModImpl(left, right);
            if (mod == Ops.NotImplemented) return mod;
            return Tuple.MakeTuple(div, mod);
        }
        internal static object ReverseDivModImpl(Single left, Single right) {
            return DivModImpl(right, left);
        }

        internal static object PowerImpl(Single left, Single right) {
            return FloatOps.Power((Double)left, (Double)right);
        }
        internal static object ReversePowerImpl(Single left, Single right) {
            return PowerImpl(right, left);
        }
    }
}
