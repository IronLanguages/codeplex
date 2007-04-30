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

using IronPython.Runtime;
#if !SILVERLIGHT    // System.Text.RegularExpressions
[assembly: PythonModule("_sre", typeof(IronPython.Modules.PythonSRegEx))]
namespace IronPython.Modules {
    [PythonType("_sre")]
    public static class PythonSRegEx {
        public static object MAGIC = 20031017;
        public static object CODESIZE = 2;

        [PythonName("getlower")]
        public static object GetLower(object val, object encoding) {
            int encInt = Converter.ConvertToInt32(val);
            int charVal = Converter.ConvertToInt32(val);

            if (encInt == (int)PythonRegex.UNICODE) {
                return (int)Char.ToLower((char)charVal);
            } else {
                return (int)Char.ToLowerInvariant((char)charVal);
            }
        }

        [PythonName("compile")]
        public static object Compile(object a, object b, object c, object d, object e, object f) {
            return null;
        }
    }
}
#endif