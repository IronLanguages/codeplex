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
    public static class DecimalOps {

        public static DynamicType MakeDynamicType() {
            return new OpsReflectedType("decimal", typeof(Decimal), typeof(DecimalOps), null);
        }

        [PythonName("__cmp__")]
        public static object Compare(decimal x, object other) {
            return FloatOps.Compare((double)x, other);
        }

        [PythonName("__gt__")]
        public static object GreaterThan(decimal x, object other) {
            object res = FloatOps.Compare((double)x, other);
            if (res != Ops.NotImplemented) return Ops.Bool2Object((int)res > 0);
            return res;
        }

        [PythonName("__lt__")]
        public static object LessThan(decimal x, object other) {
            object res = FloatOps.Compare((double)x, other);
            if (res != Ops.NotImplemented) return Ops.Bool2Object((int)res < 0);
            return res;
        }

        [PythonName("__ge__")]
        public static object GreaterThanEqual(decimal x, object other) {
            object res = FloatOps.Compare((double)x, other);
            if (res != Ops.NotImplemented) return Ops.Bool2Object((int)res >= 0);
            return res;
        }

        [PythonName("__le__")]
        public static object LessThanEqual(decimal x, object other) {
            object res = FloatOps.Compare((double)x, other);
            if (res != Ops.NotImplemented) return Ops.Bool2Object((int)res <= 0);
            return res;
        }

    }
}
