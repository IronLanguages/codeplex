/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

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
        [SpecialName]
        public static bool Equals(char self, char other) {
            return self == other;
        }

        [SpecialName]
        public static bool NotEquals(char self, char other) {
            return self != other;
        }

        [SpecialName, PythonName("__hash__")]
        public static int GetHashCode(char self) {
            return new String(self, 1).GetHashCode();
        }

        [SpecialName, PythonName("__cmp__")]
        public static object Compare(char self, object other) {
            string strOther;

            if (other is char) {
                int diff = self - (char)other;
                return diff > 0 ? 1 : diff < 0 ? -1 : 0;
            } else if ((strOther = other as string) != null && strOther.Length == 1) {
                int diff = self - strOther[0];
                return diff > 0 ? 1 : diff < 0 ? -1 : 0;
            }

            return PythonOps.NotImplemented;
        }

        [ImplicitConversionMethod]
        public static string ConvertToString(char self) {
            return new string(self, 1);
        }

        [ExplicitConversionMethod]
        public static char ConvertToChar(int value) {
            if (value < 0 || value > Char.MaxValue) throw new OverflowException();

            return (char)value;
        }
    }
}
