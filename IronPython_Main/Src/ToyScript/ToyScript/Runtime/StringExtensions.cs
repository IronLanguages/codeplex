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
using System.Scripting.Runtime;
using System.Text;
using ToyScript.Runtime;

[assembly:ExtensionType(typeof(string), typeof(StringExtensions))]

namespace ToyScript.Runtime {

    public static class StringExtensions {
        public static string Invert(string self) {
            if (self == null) {
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder(self.Length);
            for (int i = 0; i < self.Length; i ++) {
                char c = self[i];
                sb.Append(
                    Char.IsLower(c) ? Char.ToUpper(c) :
                    Char.IsUpper(c) ? Char.ToLower(c) :
                    '?'
                );
            }
            return sb.ToString();
        }

        [PropertyMethod]
        public static string GetInversion(string self) {
            return Invert(self);
        }
    }
}
