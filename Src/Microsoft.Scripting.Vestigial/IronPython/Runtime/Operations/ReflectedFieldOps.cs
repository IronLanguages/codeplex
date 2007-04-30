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

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;

[assembly: PythonExtensionType(typeof(ReflectedField), typeof(ReflectedFieldOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedFieldOps {
        [OperatorMethod, PythonName("__str__")]
        public static string ToString(ReflectedField field) {
            return CodeRepresentation(field);
        }

        [OperatorMethod, PythonName("__repr__")]
        public static string CodeRepresentation(ReflectedField field) {
            return string.Format("<field# {0} on {1}>", field.info.Name, field.info.DeclaringType.Name);
        }
    }
}
