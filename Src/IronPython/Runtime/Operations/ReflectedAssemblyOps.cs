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
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

[assembly: PythonExtensionType(typeof(Assembly), typeof(ReflectedAssemblyOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedAssemblyOps {
        [SpecialName, PythonName("__repr__")]
        public static object Repr(Assembly self) {
            Assembly asmSelf = self as Assembly;

            return "<Assembly " + asmSelf.FullName + ">";
        }
    }
}
