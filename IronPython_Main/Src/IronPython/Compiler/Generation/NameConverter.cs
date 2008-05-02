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
using System.Reflection;
using System.Text;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler {
    /// <summary>
    /// Contains helper methods for converting C# names into Python names.
    /// </summary>
    public static class NameConverter {
        public static NameType TryGetName(PythonType dt, MethodInfo mi, out string name) {
            name = mi.Name;

            return GetNameFromMethod(dt, mi, NameType.Method, ref name);
        }

        public static NameType TryGetName(PythonType dt, FieldInfo fi, out string name) {
            NameType nt = NameType.PythonField;
            name = fi.Name;

            // hide MinValue & MaxValue on int, Empty on string, Epsilon, Min/Max, etc.. on double
            if (fi.DeclaringType == typeof(string) ||
                fi.DeclaringType == typeof(int) ||
                fi.DeclaringType == typeof(double) ||
                fi.IsDefined(typeof(PythonHiddenAttribute), false)) nt = NameType.Field;

            string namePrefix = "";
            if (fi.IsPrivate || (fi.IsAssembly && !fi.IsFamilyOrAssembly)) {
                if (!ScriptDomainManager.Options.PrivateBinding) {
                    return NameType.None;
                } else {
                    // mangle protectes to private
                    namePrefix = "_" + dt.Name + "__";
                    nt = NameType.Field;
                }
            }

            name = namePrefix + name;
            return nt;
        }

        public static NameType TryGetName(PythonType dt, EventInfo ei, MethodInfo eventMethod, out string name) {
            name = ei.Name;
            NameType res = dt.IsPythonType ? NameType.PythonEvent : NameType.Event;

            return GetNameFromMethod(dt, eventMethod, res, ref name);
        }

        public static NameType TryGetName(PythonType dt, PropertyInfo pi, MethodInfo prop, out string name) {
            if (pi.IsDefined(typeof(PythonHiddenAttribute), false)) {
                name = null;
                return NameType.None;
            }

            name = pi.Name;

            return GetNameFromMethod(dt, prop, NameType.Property, ref name);
        }

        public static NameType TryGetName(PythonType dt, ExtensionPropertyInfo pi, MethodInfo prop, out string name) {
            name = pi.Name;

            return GetNameFromMethod(dt, prop, NameType.Property, ref name);
        }

        public static NameType TryGetName(Type t, out string name) {
            name = GetTypeName(t);

            string namePrefix = "";

            if ((ReflectionUtils.IsNested(t) && !t.IsNestedPublic) || (t.IsNestedAssembly && !t.IsNestedFamORAssem)) {
                if (!ScriptDomainManager.Options.PrivateBinding) {
                    return NameType.None;
                } else if (!t.IsGenericParameter) {
                    namePrefix = "_" + DynamicHelpers.GetPythonTypeFromType(t.DeclaringType).Name + "__";
                }
            }

            NameType res = NameType.Type;
            object[] attribute = t.GetCustomAttributes(typeof(PythonSystemTypeAttribute), false);
            if (attribute.Length > 0) {
                PythonSystemTypeAttribute attr = attribute[0] as PythonSystemTypeAttribute;
                if (attr.Name != null) {
                    name = attr.Name;
                }
            }

            name = namePrefix + name;
            return res;
        }

        public static string GetTypeName(Type t) {
            string name;
            name = t.Name;

            if (t.IsArray) {
                return "Array[" + PythonTypeOps.GetName(DynamicHelpers.GetPythonTypeFromType(t.GetElementType())) + "]";
            }

            int backtickIndex;
            if ((backtickIndex = name.IndexOf(ReflectionUtils.GenericArityDelimiter)) != -1) {
                name = name.Substring(0, backtickIndex);
                Type[] typeOf = t.GetGenericArguments();
                StringBuilder sb = new StringBuilder(name);
                sb.Append('[');
                bool first = true;
                foreach (Type tof in typeOf) {
                    if (first) first = false; else sb.Append(", ");
                    sb.Append(PythonTypeOps.GetName(DynamicHelpers.GetPythonTypeFromType(tof)));
                }
                sb.Append(']');
                name = sb.ToString();                
            }
            return name;
        }

        internal static NameType GetNameFromMethod(PythonType dt, MethodInfo mi, NameType res, ref string name) {
            string namePrefix = null;

            if (mi.IsDefined(typeof(PythonHiddenAttribute), false)) {
                name = null;
                return NameType.None;
            }

            if (mi.IsPrivate || (mi.IsAssembly && !mi.IsFamilyOrAssembly)) {
                // allow explicitly implemented interface
                if (!(mi.IsPrivate && mi.IsFinal && mi.IsHideBySig && mi.IsVirtual)) {
                    if (!ScriptDomainManager.Options.PrivateBinding) {
                        return NameType.None;
                    } else {
                        // mangle protectes to private
                        namePrefix = "_" + dt.Name + "__";
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

            if (mi.IsDefined(typeof(PythonClassMethodAttribute), false)) {
                res |= NameType.ClassMember;
            }

            if (namePrefix != null) name = namePrefix + name;

            if (mi.DeclaringType.IsDefined(typeof(PythonSystemTypeAttribute), false) ||
                !mi.DeclaringType.IsAssignableFrom(dt.UnderlyingSystemType)) {
                // extension types are all python names
                res |= NameType.Python;
            }

            if (mi.IsDefined(typeof(PropertyMethodAttribute), false)) {
                res = (res & ~NameType.BaseTypeMask) | NameType.Property;
            }

            return res;
        }
    }
}
