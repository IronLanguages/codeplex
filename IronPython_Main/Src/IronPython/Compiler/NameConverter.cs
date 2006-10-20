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
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler {
    /// <summary>
    /// Contains helper methods for converting C# names into Python names.
    /// </summary>
    static class NameConverter {
        public static NameType TryGetName(ReflectedType dt, MethodInfo mi, out string name) {
            Debug.Assert(dt.IsSubclassOf(DynamicType.GetDeclaringType(mi)));

            NameType res = dt.IsClsType ? NameType.PythonMethod : NameType.Method;
            name = mi.Name;

            return GetNameFromMethod(dt, mi, res, ref name);
        }

        public static NameType TryGetName(ReflectedType dt, FieldInfo fi, out string name) {
            Debug.Assert(dt.IsSubclassOf(DynamicType.GetDeclaringType(fi)));

            NameType nt = NameType.PythonField;
            name = null;

            // hide MinValue & MaxValue on int, Empty on string, Epsilon, Min/Max, etc.. on double
            if (fi.DeclaringType == typeof(string) ||
                fi.DeclaringType == typeof(int) ||
                fi.DeclaringType == typeof(double) ||
                fi.IsDefined(typeof(PythonHiddenFieldAttribute), false)) nt = NameType.Field;

            string namePrefix = "";
            if (fi.IsPrivate || (fi.IsAssembly && !fi.IsFamilyOrAssembly)) {
                if (!Options.PrivateBinding) {
                    return NameType.None;
                } else {
                    // mangle protectes to private
                    namePrefix = "_" + dt.Name + "__";
                    nt = NameType.Field;
                }
            }

            name = namePrefix + fi.Name;
            return nt;
        }

        public static NameType TryGetName(ReflectedType dt, EventInfo ei, MethodInfo eventMethod, out string name) {
            Debug.Assert(dt.IsSubclassOf(DynamicType.GetDeclaringType(ei)));

            name = ei.Name;
            NameType res = dt.IsClsType ? NameType.PythonEvent : NameType.Event;

            return GetNameFromMethod(dt, eventMethod, res, ref name);
        }

        public static NameType TryGetName(ReflectedType dt, PropertyInfo pi, MethodInfo prop, out string name) {
            Debug.Assert(dt.IsSubclassOf(DynamicType.GetDeclaringType(pi)));

            name = pi.Name;
            NameType res = dt.IsClsType ? NameType.PythonProperty : NameType.Property;

            return GetNameFromMethod(dt, prop, res, ref name);
        }

        public static NameType TryGetName(ReflectedType outerType, Type t, out string name) {
            name = t.Name;

            int backtickIndex;
            if ((backtickIndex = name.IndexOf(ReflectionUtil.GenericArityDelimiter)) != -1) {
                name = name.Substring(0, backtickIndex);
                if (!t.ContainsGenericParameters) {
                    Type[] typeOf = t.GetGenericArguments();
                    StringBuilder sb = new StringBuilder(name);
                    sb.Append('[');
                    bool first = true;
                    foreach (Type tof in typeOf) {
                        if (first) first = false; else sb.Append(", ");
                        sb.Append(Ops.GetDynamicTypeFromType(tof).Name);
                    }
                    sb.Append(']');
                    name = sb.ToString();
                }
            }

            string namePrefix = "";
            if ((t.IsNested && !t.IsNestedPublic) || (t.IsNestedAssembly && !t.IsNestedFamORAssem)) {
                if (!Options.PrivateBinding) {
                    return NameType.None;
                } else {
                    namePrefix = "_" + Ops.GetDynamicTypeFromType(t.DeclaringType).Name + "__";
                }
            }

            NameType res = NameType.Type;
            if (outerType.IsPythonType) {
                object[] attribute = t.GetCustomAttributes(typeof(PythonTypeAttribute), false);

                if (attribute.Length > 0) {
                    PythonTypeAttribute attr = attribute[0] as PythonTypeAttribute;
                    if (attr.name != null && attr.name.Length > 0) {
                        res = NameType.PythonType;
                        name = attr.name;
                    }
                }
            } else {
                res = NameType.PythonType;
            }

            name = namePrefix + name;
            return res;
        }

        private static NameType GetNameFromMethod(ReflectedType dt, MethodInfo mi, NameType res, ref string name) {
            string namePrefix = null;

            if (mi.IsPrivate || (mi.IsAssembly && !mi.IsFamilyOrAssembly)) {
                // allow explicitly implemented interface
                if (!(mi.IsPrivate && mi.IsFinal && mi.IsHideBySig && mi.IsVirtual)) {
                    if (!Options.PrivateBinding) {
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

            object[] attribute = mi.GetCustomAttributes(typeof(PythonNameAttribute), false);

            if (namePrefix != null) name = namePrefix + name;
            if (attribute.Length > 0) {
                PythonNameAttribute attr = attribute[0] as PythonNameAttribute;
                if (attr.name != null && attr.name.Length > 0) {
                    if (attr is PythonClassMethodAttribute) res |= NameType.ClassMember;

                    res |= NameType.Python;
                    name = attr.name;
                }
            }

            return res;
        }

    }
}
