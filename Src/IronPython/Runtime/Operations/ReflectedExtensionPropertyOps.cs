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

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Microsoft.Scripting;

[assembly: PythonExtensionType(typeof(ReflectedExtensionProperty), typeof(ReflectedExtensionPropertyOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedExtensionPropertyOps {
        [PythonName("__doc__")]
        public static string GetDocumentation(ReflectedExtensionProperty self) {
            return DocBuilder.DocOneInfo(self.ExtInfo);
        }
    }
}