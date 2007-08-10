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
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;

[assembly: PythonExtensionType(typeof(BoundBuiltinFunction), typeof(BoundBuiltinFunctionOps))]
namespace IronPython.Runtime.Operations {
    public static class BoundBuiltinFunctionOps {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cls"), StaticExtensionMethod("__new__")]
        public static object Make(object cls, object newFunction, object inst) {
            return new Method(newFunction, inst, null);
        }

        [PropertyMethod, PythonName("__self__")]
        public static object GetSelf(BoundBuiltinFunction self) {
            return self.Self;
        }

        [PropertyMethod, PythonName("__name__")]
        public static string GetName(BoundBuiltinFunction self) {
            return self.Name;
        }

        [PropertyMethod, PythonName("__doc__")]
        public static string GetDocumentation(BoundBuiltinFunction self) {            
            return PythonBuiltinFunctionOps.GetDocumentation(self.Target);
        }

        [SpecialName, PythonName("__str__")]
        public static string ToString(BoundBuiltinFunction self) {
            return ToCodeRepresentation(self);
        }

        [SpecialName, PythonName("__repr__")]
        public static string ToCodeRepresentation(BoundBuiltinFunction self) {
            return string.Format("<built-in method {0} of {1} object at {2}>",
                    self.Name,
                    PythonOps.GetPythonTypeName(self.Self),
                    PythonOps.HexId(self.Self));
        }

        [SpecialName]
        public static object GetItem(BoundBuiltinFunction self, object key) {
            return new BoundBuiltinFunction(PythonBuiltinFunctionOps.GetItem(self.Target, key), self.Self);
        }

        [PropertyMethod, PythonName("Overloads")]
        public static BuiltinFunctionOverloadMapper GetOverloads(BoundBuiltinFunction self) {
            return new BuiltinFunctionOverloadMapper(self.Target, self.Self);
        }


    }
}
