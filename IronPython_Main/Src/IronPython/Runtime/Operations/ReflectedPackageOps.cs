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

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonExtensionType(typeof(NamespaceTracker), typeof(ReflectedPackageOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedPackageOps {
        [PropertyMethod, PythonName("__file__")]
        public static object GetFilename(NamespaceTracker self) {
            if (self.PackageAssemblies.Count == 1) {
                return self.PackageAssemblies[0].FullName;
            }

            List res = new List();
            for (int i = 0; i < self.PackageAssemblies.Count; i++) {
                res.Add(self.PackageAssemblies[i].FullName);
            }
            return res;                        
        }

        [SpecialName, PythonName("__repr__")]
        public static string ToCodeString(NamespaceTracker self) {
            return ToString(self);
        }

        [SpecialName, PythonName("__str__")]
        public static string ToString(NamespaceTracker self) {
            if (self.PackageAssemblies.Count != 1) {
                return String.Format("<module '{0}' (CLS module, {1} assemblies loaded)>", GetName(self.Name), self.PackageAssemblies.Count);
            }
            return String.Format("<module '{0}' (CLS module from {1})>", GetName(self.Name), self.PackageAssemblies[0].FullName);
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection GetDictionary(CodeContext context, NamespaceTracker self) {
            PythonDictionary res = new PythonDictionary();
            foreach (KeyValuePair<object, object> kvp in self) {
                if (kvp.Value is TypeGroup || kvp.Value is NamespaceTracker) {
                    res[kvp.Key] = kvp.Value;
                } else {
                    res[kvp.Key] = DynamicHelpers.GetPythonTypeFromType(((TypeTracker)kvp.Value).Type);
                }
            }
            return res;
        }

        [PropertyMethod, PythonName("__name__")]
        public static string GetName(CodeContext context, NamespaceTracker self) {
            return GetName(self.Name);
        }

        private static string GetName(string name) {
            int lastDot = name.LastIndexOf('.');
            if (lastDot == -1) return name;

            return name.Substring(lastDot + 1);
        }
    }
}
