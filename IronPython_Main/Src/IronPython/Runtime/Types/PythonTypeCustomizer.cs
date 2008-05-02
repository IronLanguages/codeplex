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
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;

using IronPython.Compiler;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

#if !SILVERLIGHT

using Microsoft.Scripting.Actions.ComDispatch;

#endif

namespace IronPython.Runtime.Types {
    class PythonTypeCustomizer //: CoreReflectedTypeBuilder 
    {
        private static readonly Dictionary<Type/*!*/, string/*!*/>/*!*/ _sysTypes = MakeSystemTypes();

        /// <summary>
        /// Creates a table of standard .NET types which are also standard Python types
        /// </summary>
        private static Dictionary<Type/*!*/, string/*!*/>/*!*/ MakeSystemTypes() {
            Dictionary<Type/*!*/, string/*!*/> res = new Dictionary<Type, string>();

            res[typeof(object)] = "object";
            res[typeof(string)] = "str";
            res[typeof(int)] = "int";
            res[typeof(bool)] = "bool";
            res[typeof(double)] = "float";
            res[typeof(decimal)] = "decimal";
            res[typeof(BigInteger)] = "long";
            res[typeof(Complex64)] = "complex";
            res[typeof(Scope)] = "module";
            res[typeof(ClassMethodDescriptor)] = "method_descriptor";
            res[typeof(ValueType)] = "ValueType";   // just hiding it's methods in the inheritance hierarchy
            res[typeof(TypeGroup)] = "type-collision";
            res[typeof(None)] = "NoneType";
            res[typeof(Byte)] = "Byte";
            res[typeof(SByte)] = "SByte";
            res[typeof(Int16)] = "Int16";
            res[typeof(UInt16)] = "UInt16";
            res[typeof(UInt32)] = "UInt32";
            res[typeof(Int64)] = "Int64";
            res[typeof(UInt64)] = "UInt64";
            res[typeof(Single)] = "Single";
            res[typeof(NamespaceTracker)] = "namespace#";
            res[typeof(Assembly)] = "Assembly";
            res[typeof(IAttributesCollection)] = "dict";
#if !SILVERLIGHT
            res[ComObject.ComObjectType] = ComObject.ComObjectType.Name;
#endif

            return res;
        }

        public static Dictionary<Type, string> SystemTypes {
            get {
                return _sysTypes;
            }
        }

        public static bool IsPythonType(Type t) {
            return _sysTypes.ContainsKey(t) || t.IsDefined(typeof(PythonSystemTypeAttribute), false);
        }

        internal static MethodInfo[] GetMethodSet(string name, int expected) {
            MethodInfo[] methods = typeof(InstanceOps).GetMethods();
            MethodInfo[] filtered = new MethodInfo[expected];
            int j = 0;
            for (int i = 0; i < methods.Length; i++) {
                if (methods[i].Name == name) {
                    filtered[j++] = methods[i];
#if !DEBUG
                    if (j == expected) break;
#endif
                }
            }
            Debug.Assert(j == expected);
            return filtered;
        }

    }
}
