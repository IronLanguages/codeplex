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

using Microsoft.Scripting.Internal;
using Microsoft.Scripting;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonExtensionType(typeof(ReflectedPackage), typeof(ReflectedPackageOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedPackageOps {
        [PropertyMethod, PythonName("__file__")]
        public static object GetFilename(ReflectedPackage self) {
            if (self._packageAssemblies.Count == 1) {
                return self._packageAssemblies[0].FullName;
            }

            List res = new List();
            for (int i = 0; i < self._packageAssemblies.Count; i++) {
                res.Add(self._packageAssemblies[i].FullName);
            }
            return res;                        
        }
    }
}
