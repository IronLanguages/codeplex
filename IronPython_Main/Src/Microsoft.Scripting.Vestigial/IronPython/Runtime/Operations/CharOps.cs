/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

[assembly: PythonExtensionType(typeof(char), typeof(CharOps))]
namespace IronPython.Runtime.Operations {
    /// <summary>
    /// We override the behavior of equals, compare and hashcode to make
    /// chars seem as much like strings as possible.  In Python there is no
    /// difference between these types.
    /// </summary>

    public static class CharOps {
        [OperatorMethod]
        public static bool Equal(char self, char other) {
            return self == other;
        }

        [OperatorMethod]
        public static bool NotEqual(char self, char other) {
            return self != other;
        }

        [OperatorMethod, PythonName("__hash__")]
        public static int GetHashCode(char self) {
            return new String(self, 1).GetHashCode();
        }

        public static object Compare(char self, object other) {
            string strOther;

            if (other is char) {
                int diff = self - (char)other;
                return diff > 0 ? 1 : diff < 0 ? -1 : 0;
            } else if ((strOther = other as string) != null && strOther.Length == 1) {
                int diff = self - strOther[0];
                return diff > 0 ? 1 : diff < 0 ? -1 : 0;
            }

            return Ops.NotImplemented;
        }
    }
}
