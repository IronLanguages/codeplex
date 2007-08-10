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
using Microsoft.Scripting.Types;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonExtensionType(typeof(ConstructorFunction), typeof(ConstructionFunctionOps))]
namespace IronPython.Runtime.Operations {
    public static class ConstructionFunctionOps {
        [PropertyMethod, PythonName("__name__")]
        public static string GetName(BuiltinFunction self) {
            return "__new__";
        }

        [SpecialName, PythonName("__doc__")]
        public static string GetDocumentation(ConstructorFunction self) {
            StringBuilder sb = new StringBuilder();
            MethodBase[] targets = self.ConstructorTargets;

            for (int i = 0; i < targets.Length; i++) {
                if (targets[i] != null) sb.AppendLine(DocBuilder.DocOneInfo(targets[i], "__new__"));
            }
            return sb.ToString();
        }

    }
}
