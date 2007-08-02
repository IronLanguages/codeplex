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
using System.Reflection;
using System.Text;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Types;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler {
    /// <summary>
    /// Contains helper methods for converting C# names into Python names.
    /// </summary>
    public static class NameConverter {
        public static NameType TryGetName(DynamicType dt, MethodInfo mi, out string name) {
            Debug.Assert(IsValidSubtype(dt, mi),
                String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0}.{1} is not declared on {2}",
                    mi.DeclaringType.FullName, mi.Name, dt.Name)
                );

            name = mi.Name;

            return GetNameFromMethod(dt, mi, NameType.Method, ref name);
        }

        public static NameType TryGetName(DynamicType dt, FieldInfo fi, out string name) {
            Debug.Assert(dt.IsSubclassOf(TypeHelpers.GetDeclaringType(fi)));

            NameType nt = NameType.PythonField;
            name = fi.Name;

            // hide MinValue & MaxValue on int, Empty on string, Epsilon, Min/Max, etc.. on double
            if (fi.DeclaringType == typeof(string) ||
                fi.DeclaringType == typeof(int) ||
                fi.DeclaringType == typeof(double) ||
                fi.IsDefined(typeof(PythonHiddenFieldAttribute), false)) nt = NameType.Field;

            object [] attrs = fi.GetCustomAttributes(typeof(ScriptNameAttribute), false);
            string namePrefix = "";
            if (attrs.Length > 0) {
                name = ((ScriptNameAttribute)attrs[0]).Name;
            } else if (fi.IsPrivate || (fi.IsAssembly && !fi.IsFamilyOrAssembly)) {
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

        public static NameType TryGetName(DynamicType dt, EventInfo ei, MethodInfo eventMethod, out string name) {
            Debug.Assert(IsValidSubtype(dt, ei));

            name = ei.Name;
            NameType res = dt.TypeContext == ContextId.Empty ? NameType.PythonEvent : NameType.Event;

            return GetNameFromMethod(dt, eventMethod, res, ref name);
        }

        public static NameType TryGetName(DynamicType dt, PropertyInfo pi, MethodInfo prop, out string name) {
            Debug.Assert(IsValidSubtype(dt, pi));

            name = pi.Name;

            return GetNameFromMethod(dt, prop, NameType.Property, ref name);
        }

        public static NameType TryGetName(DynamicType dt, ExtensionPropertyInfo pi, MethodInfo prop, out string name) {
            Debug.Assert(dt.IsSubclassOf(TypeHelpers.GetDeclaringType(pi.DeclaringType)));

            name = pi.Name;

            return GetNameFromMethod(dt, prop, NameType.Property, ref name);
        }

        public static NameType TryGetName(Type t, out string name) {
            name = GetTypeName(t);

            string namePrefix = "";

            if ((Utils.Reflection.IsNested(t) && !t.IsNestedPublic) || (t.IsNestedAssembly && !t.IsNestedFamORAssem)) {
                if (!ScriptDomainManager.Options.PrivateBinding) {
                    return NameType.None;
                } else {
                    namePrefix = "_" + DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(t.DeclaringType)) + "__";
                }
            }

            NameType res = NameType.Type;
            object[] attribute = t.GetCustomAttributes(typeof(PythonTypeAttribute), false);

            if (attribute.Length > 0) {
                PythonTypeAttribute attr = attribute[0] as PythonTypeAttribute;
                if (attr.Name != null && attr.Name.Length > 0) {
                    res = NameType.PythonType;
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
                return "Array[" + DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(t.GetElementType())) + "]";
            }

            int backtickIndex;
            if ((backtickIndex = name.IndexOf(Utils.Reflection.GenericArityDelimiter)) != -1) {
                name = name.Substring(0, backtickIndex);
                Type[] typeOf = t.GetGenericArguments();
                StringBuilder sb = new StringBuilder(name);
                sb.Append('[');
                bool first = true;
                foreach (Type tof in typeOf) {
                    if (first) first = false; else sb.Append(", ");
                    sb.Append(DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(tof)));
                }
                sb.Append(']');
                name = sb.ToString();                
            }
            return name;
        }

        internal static NameType GetNameFromMethod(DynamicType dt, MethodInfo mi, NameType res, ref string name) {
            string namePrefix = null;

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

            object[] attribute = mi.GetCustomAttributes(typeof(ScriptNameAttribute), false);

            if (namePrefix != null) name = namePrefix + name;
            if (attribute.Length > 0) {
                ScriptNameAttribute attr = attribute[0] as ScriptNameAttribute;
                if (attr.Name != null && attr.Name.Length > 0) {
                    if (attr is PythonClassMethodAttribute) res |= NameType.ClassMember;                    
                    res |= NameType.Python;
                    name = attr.Name;
                }
            } else if (!mi.DeclaringType.IsAssignableFrom(dt.UnderlyingSystemType)) {
                // extension types are all python names
                res |= NameType.Python;
            }

            if (mi.IsDefined(typeof(PropertyMethodAttribute), false)) {
                res = (res & ~NameType.BaseTypeMask) | NameType.Property;
            }

            return res;
        }

        private static bool IsValidSubtype(DynamicType dt, MemberInfo mi) {
            return dt.IsSubclassOf(TypeHelpers.GetDeclaringType(mi)) || mi.DeclaringType.IsInterface;
        }

    }
}
