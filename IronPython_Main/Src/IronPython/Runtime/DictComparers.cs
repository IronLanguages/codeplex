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

using System.Diagnostics;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    class PythonObjectComparer : IEqualityComparer<object> {
        #region IEqualityComparer<object> Members

        bool IEqualityComparer<object>.Equals(object x, object y) {
            try {
                return Ops.EqualRetBool(x, y);
            } catch {
                // Dict isn't supposed to throw if cmp throws (test_operations.py)
                return false;
            }
        }

        public int GetHashCode(object obj) {
            return Ops.Hash(obj);
        }

        #endregion
    }
}
