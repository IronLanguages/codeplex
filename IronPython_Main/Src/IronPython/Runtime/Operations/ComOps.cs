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

#if !SILVERLIGHT

using System; using Microsoft;
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Types;
using System.Runtime.InteropServices;

namespace IronPython.Runtime.Operations {
    public static class ComOps {
        private static readonly Type ComObjectType = typeof(object).Assembly.GetType("System.__ComObject");

        internal static bool IsComObject(object obj) {
            // we can't use System.Runtime.InteropServices.Marshal.IsComObject(obj) since it doesn't work in partial trust
            return obj != null && ComObjectType.IsAssignableFrom(obj.GetType());
        }

        public static string __str__(object/*!*/ self) {
            return self.ToString();
        }

        public static string/*!*/ __repr__(object/*!*/ self) {
            return String.Format("<{0} object at {1}>",
                self.ToString(),
                PythonOps.HexId(self)
            );
        }
    }
}

#endif
