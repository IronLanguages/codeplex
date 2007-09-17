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

using System.Diagnostics;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    class PythonObjectComparer : IEqualityComparer<object> {
        #region IEqualityComparer<object> Members

        bool IEqualityComparer<object>.Equals(object x, object y) {
            try {
                return PythonOps.EqualRetBool(x, y);
            } catch {
                // Dict isn't supposed to throw if cmp throws (test_operations.py)
                return false;
            }
        }

        public int GetHashCode(object obj) {
            return PythonOps.Hash(obj);
        }

        #endregion
    }
}
