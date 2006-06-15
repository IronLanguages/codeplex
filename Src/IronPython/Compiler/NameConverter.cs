/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Diagnostics;

using IronPython.Runtime;

namespace IronPython.Compiler {
    /// <summary>
    /// Contains helper methods for converting C# names into Python names.
    /// </summary>
    static class NameConverter {
        public static NameType TryGetName(DynamicType dt, MethodInfo mi, out string name) {
            Debug.Assert(dt.IsSubclassOf(DynamicType.GetDeclaringType(mi)));

            string namePrefix = null;
            NameType res = NameType.Method;
            name = mi.Name;

            if (mi.IsPrivate || (mi.IsAssembly && !mi.IsFamilyOrAssembly)) {
                // allow explicitly implemented interface
                if (!(mi.IsPrivate && mi.IsFinal && mi.IsHideBySig && mi.IsVirtual)) {
                    if (!Options.PrivateBinding) {
                        return NameType.None;
                    } else {
                        // mangle protectes to private
                        namePrefix = "_" + dt.__name__ + "__";
                    }
                } else {
                    // explicitly implemented interface

                    // drop the namespace, leave the interface name, and replace 
                    // the dot with an underscore.  Eg System.IConvertible.ToBoolean
                    // becomes IConvertible_ToBoolean
                    int lastDot = name.LastIndexOf(Type.Delimiter);
                    if (lastDot != -1) {
                        name = name.Substring(lastDot + 1);
                    }
                }
            } 

            object[] attribute = mi.GetCustomAttributes(typeof(PythonNameAttribute), false);

            if (attribute.Length > 0) {
                PythonNameAttribute attr = attribute[0] as PythonNameAttribute;
                if (attr.name != null && attr.name.Length > 0) {
                    if (attr is PythonClassMethodAttribute) res = NameType.ClassMethod;
                    else res = NameType.PythonMethod;
                    name = attr.name;                    
                }
            }

            if (namePrefix != null) name = namePrefix + name;

            return res;
        }

        public static NameType TryGetName(DynamicType dt, FieldInfo fi, out string name) {
            Debug.Assert(dt.IsSubclassOf(DynamicType.GetDeclaringType(fi)));

            NameType nt = NameType.PythonField;
            name = null;

            if (fi.IsDefined(typeof(PythonHiddenFieldAttribute), false)) return NameType.None;

            string namePrefix = "";
            if (fi.IsPrivate || (fi.IsAssembly && !fi.IsFamilyOrAssembly)) {
                if (!Options.PrivateBinding) {
                    return NameType.None;
                } else {
                    // mangle protectes to private
                    namePrefix = "_" + dt.__name__ + "__";
                    nt = NameType.Field;
                }
            } 
           
            name = namePrefix + fi.Name;
            return nt;
        }

        public static NameType TryGetName(DynamicType dt, PropertyInfo pi, MethodInfo prop, out string name) {
            Debug.Assert(dt.IsSubclassOf(DynamicType.GetDeclaringType(pi)));

            name = pi.Name;
            string namePrefix = null;
            NameType res = NameType.Property; 

            if (prop.IsPrivate || (prop.IsAssembly && !prop.IsFamilyOrAssembly)) {
                // allow explicitly implemented interface
                if (!(prop.IsPrivate && prop.IsFinal && prop.IsHideBySig && prop.IsVirtual)) {
                    if (!Options.PrivateBinding) {
                        return NameType.None;
                    } else {
                        // mangle protectes to private
                        namePrefix = "_" + dt.__name__ + "__";
                    }
                } else {
                    // explicitly implemented interface

                    // drop the namespace, leave the interface name, and replace 
                    // the dot with an underscore.  Eg System.IConvertible.ToBoolean
                    // becomes IConvertible_ToBoolean
                    int lastDot = name.LastIndexOf(Type.Delimiter);
                    if (lastDot != -1) {
                        name = name.Substring(lastDot + 1);
                    }
                }
            } 

            object[] attribute = prop.GetCustomAttributes(typeof(PythonNameAttribute), false);

            if(namePrefix != null) name = namePrefix + pi.Name; 
            if (attribute.Length > 0) {
                PythonNameAttribute attr = attribute[0] as PythonNameAttribute;
                if (attr.name != null && attr.name.Length > 0) {
                    res = NameType.PythonProperty;
                    name = attr.name;
                }
            }
            
            return res;
        }

        public static NameType TryGetName(Type t, out string name) {
            name = t.Name;

            int backtickIndex;
            if ((backtickIndex = name.IndexOf(ReflectionUtil.GenericArityDelimiter)) != -1) {
                name = name.Substring(0, backtickIndex);
            }

            string namePrefix = "";
            if ((t.IsNested && !t.IsNestedPublic) || (t.IsNestedAssembly && !t.IsNestedFamORAssem)) {
                if (!Options.PrivateBinding) {
                    return NameType.None;
                } else {
                    namePrefix = "_" + Ops.GetDynamicTypeFromType(t.DeclaringType).__name__ + "__";
                }
            }

            NameType res = NameType.Type;
            object[] attribute = t.GetCustomAttributes(typeof(PythonTypeAttribute), false);

            if (attribute.Length > 0) {
                PythonTypeAttribute attr = attribute[0] as PythonTypeAttribute;
                if (attr.name != null && attr.name.Length > 0) {
                    res = NameType.PythonType;
                    name = attr.name;
                }
            }

            name = namePrefix + name;
            return res;
        }
    }
}
