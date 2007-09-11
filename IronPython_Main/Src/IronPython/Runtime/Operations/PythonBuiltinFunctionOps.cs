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
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Runtime;

[assembly: PythonExtensionType(typeof(BuiltinFunction), typeof(PythonBuiltinFunctionOps))]
namespace IronPython.Runtime.Operations {
    public static class PythonBuiltinFunctionOps {
        [PropertyMethod, PythonName("__module__")]
        public static string GetModule(BuiltinFunction self) {
            if (self.Targets.Length > 0) {
                DynamicType declaringType = DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType);

                DynamicTypeSlot dts;
                if (!declaringType.TryLookupSlot(DefaultContext.Default, Symbols.Module, out dts))
                    return "__builtin__";   //!!! ???

                object val;
                if (dts.TryGetValue(DefaultContext.Default, null, declaringType, out val))
                    return (string)val;
            }
            return null;            
        }

        // Provides (for reflected methods) a mapping from a signature to the exact target
        // which takes this signature.
        // signature with syntax like the following:
        //    someClass.SomeMethod.Overloads[str, int]("Foo", 123)
        [PropertyMethod, PythonName("Overloads")] // TODO: Don't require attribute here
        public static BuiltinFunctionOverloadMapper GetOverloads(BuiltinFunction self) {
            ConstructorFunction cf = self as ConstructorFunction;
            if (cf != null) {
                return new ConstructorOverloadMapper(cf, null);
            } else {
                // The mapping is actually provided by a class rather than a dictionary
                // since it's hard to generate all the keys of the signature mapping when
                // two type systems are involved.  Creating the mapping object is quite
                // cheap so we don't cache a copy.
                return new BuiltinFunctionOverloadMapper(self, null);        
            }
        }

        [PropertyMethod, PythonName("func_name")]
        public static string GetFunctionName(BuiltinFunction self) {
            return self.Name;
        }

        [PropertyMethod, PythonName("__name__")]
        public static string GetName(BuiltinFunction self) {
            return self.Name;
        }

        [SpecialName, PythonName("__str__")]
        public static string ToString(BuiltinFunction self) {
            return string.Format("<built-in function {0}>", self.Name);
        }

        [SpecialName, PythonName("__repr__")]
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

        /// <summary>
        /// Use indexing on generic methods to provide a new reflected method with targets bound with
        /// the supplied type arguments.
        /// </summary>
        [SpecialName]
        public static BuiltinFunction GetItem(BuiltinFunction self, object key) {
            // Retrieve the list of type arguments from the index.
            Type[] types;
            Tuple typesTuple = key as Tuple;

            if (typesTuple != null) {
                types = new Type[typesTuple.Count];
                for (int i = 0; i < types.Length; i++) {
                    types[i] = Converter.ConvertToType(typesTuple[i]);
                }
            } else {
                types = new Type[] { Converter.ConvertToType(key) };
            }

            BuiltinFunction res = self.MakeGenericMethod(types);
            if (res == null) {
                throw PythonOps.TypeError(string.Format("bad type args to this generic method {0}", self));
            }

            return res;
        }

       
    }
}
