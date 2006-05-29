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

namespace IronPython.Runtime {
    static partial class SingleOps {
        internal static object DivideImpl(Single left, Single right) {
            return left / right;
        }

        internal static object ReverseDivideImpl(Single left, Single right) {
            return right / left;
        }

        internal static object FloorDivideImpl(Single left, Single right) {
            return Math.Floor(left / right);
        }

        internal static object ReverseFloorDivideImpl(Single left, Single right) {
            return FloorDivideImpl(right, left);
        }

        internal static double ModImpl(Single left, Single right) {
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
    }
}
