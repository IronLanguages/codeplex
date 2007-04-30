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

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonExtensionType(typeof(BuiltinMethodDescriptor), typeof(BuiltinMethodDescriptorOps))]
namespace IronPython.Runtime.Operations {
    public static class BuiltinMethodDescriptorOps {
        [PropertyMethod, PythonName("__name__")]
        public static string GetName(BuiltinMethodDescriptor self) {
            return self.Name;
        }

        [PropertyMethod, PythonName("__doc__")]
        public static string GetDocumentation(BuiltinMethodDescriptor self) {
            return PythonBuiltinFunctionOps.GetDocumentation(self.template);
        }

        [OperatorMethod, PythonName("__repr__")]
        public static object ToCodeRepresentation(BuiltinMethodDescriptor self) {
            BuiltinFunction bf = self.template as BuiltinFunction;
            if (bf != null) {
                return String.Format("<method {0} of {1} objects>",
                    Ops.StringRepr(bf.Name),
                    Ops.StringRepr(DynamicTypeOps.GetName(bf.DeclaringType)));
            }

            return String.Format("<classmethod object at {0}>", IdDispenser.GetId(self));
        }
        /*
                [PythonName("__get__")]
                public static object GetAttribute(CodeContext context, BuiltinMethodDescriptor self, object instance) {
                    return GetAttribute(context, instance, owner);
                }

                [PythonName("__get__")]
                public static object GetAttribute(CodeContext context, BuiltinMethodDescriptor self, object instance, object owner) {
                }
         */
    }
}
