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
using IronPython.Runtime.Calls;
using IronPython.Runtime;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using System.Reflection;

[assembly: PythonExtensionType(typeof(BuiltinFunction), typeof(PythonBuiltinFunctionOps))]
namespace IronPython.Runtime.Operations {
    public static class PythonBuiltinFunctionOps {
        [PropertyMethod, PythonName("__module__")]
        public static string GetModule(BuiltinFunction self) {
            if (self.Targets.Length > 0) {
                DynamicType declaringType = self.DeclaringType;

                DynamicTypeSlot dts;
                if (!self.DeclaringType.TryLookupSlot(DefaultContext.Default, Symbols.Module, out dts))
                    return "__builtin__";   //!!! ???

                object val;
                if (dts.TryGetValue(DefaultContext.Default, null, declaringType, out val))
                    return (string)val;
            }
            return null;            
        }

        [PropertyMethod, PythonName("func_name")]
        public static string GetFunctionName(BuiltinFunction self) {
            return self.Name;
        }

        [PropertyMethod, PythonName("__name__")]
        public static string GetName(BuiltinFunction self) {
            return self.Name;
        }

        [OperatorMethod, PythonName("__str__")]
        public static string ToString(BuiltinFunction self) {
            return string.Format("<built-in function {0}>", self.Name);
        }

        [OperatorMethod, PythonName("__repr__")]
        public static string ToCodeRepresentation(BuiltinFunction self) {
            return ToString(self);
        }

        [PropertyMethod, PythonName("__doc__")]
        public static string GetDocumentation(BuiltinFunction self) {
            StringBuilder sb = new StringBuilder();
            MethodBase[] targets = self.Targets;
            bool needNewLine = false;
            for (int i = 0; i < targets.Length; i++) {
                if (targets[i] != null) AddDocumentation(self, sb, ref needNewLine, targets[i]);
            }
            return sb.ToString();
        }

        [PropertyMethod, PythonName("__self__")]
        public static object GetSelf(BuiltinFunction self) {
            return null;
        }

        private static void AddDocumentation(BuiltinFunction self, StringBuilder sb, ref bool nl, MethodBase mb) {
            if (nl) {
                sb.Append(System.Environment.NewLine);
            }
            sb.Append(DocBuilder.DocOneInfo(mb, GetName(self)));
            nl = true;
        }

    }
}
