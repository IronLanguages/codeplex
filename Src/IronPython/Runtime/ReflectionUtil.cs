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
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace IronPython.Runtime {
    static class ReflectionUtil {

        // Generic type names have the arity (number of generic type paramters) appended at the end. 
        // For eg. the mangled name of System.List<T> is "List`1". This mangling is done to enable multiple 
        // generic types to exist as long as they have different arities.
        internal const char GenericArityDelimiter = '`';

        internal static string GetDefaultDocumentation(string methodName) {
            switch (methodName) {
                case "__abs__":     return "x.__abs__() <==> abs(x)";
                case "__add__":     return "x.__add__(y) <==> x+y";
                case "__call__":    return "x.__call__(...) <==> x(...)";
                case "__cmp__":     return "x.__cmp__(y) <==> cmp(x,y)";
                case "__delitem__": return "x.__delitem__(y) <==> del x[y]";
                case "__div__":     return "x.__div__(y) <==> x/y";
                case "__eq__":      return "x.__eq__(y) <==> x==y";
                case "__floordiv__":return "x.__floordiv__(y) <==> x//y";
                case "__getitem__": return "x.__getitem__(y) <==> x[y]";
                case "__gt__":      return "x.__gt__(y) <==> x>y";
                case "__hash__":    return "x.__hash__() <==> hash(x)";
                case "__init__":    return "x.__init__(...) initializes x; see x.__class__.__doc__ for signature";
                case "__len__":     return "x.__len__() <==> len(x)";
                case "__lshift__":  return "x.__rshift__(y) <==> x<<y";
                case "__lt__":      return "x.__lt__(y) <==> x<y";
                case "__mod__":     return "x.__mod__(y) <==> x%y";
                case "__mul__":     return "x.__mul__(y) <==> x*y";
                case "__neg__":     return "x.__neg__() <==> -x";
                case "__new__":     return "T.__new__(S, ...) -> a new object with type S, a subtype of T";
                case "__pow__":     return "x.__pow__(y[, z]) <==> pow(x, y[, z])";
                case "__reduce__":  return "helper for pickle";
                case "__rshift__":  return "x.__rshift__(y) <==> x>>y";
                case "__setitem__": return "x.__setitem__(i, y) <==> x[i]=";
                case "__str__":     return "x.__str__() <==> str(x)";
                case "__sub__":     return "x.__sub__(y) <==> x-y";
                case "__truediv__": return "x.__truediv__(y) <==> x/y";
            }

            return null;
        }

        public static string DocOneInfo(MethodBase info) {
            // Look for methods tagged with [Documentation("doc string for foo")]
            object[] attrs = info.GetCustomAttributes(typeof(DocumentationAttribute), false);
            if (attrs.Length > 0) {
                Debug.Assert(attrs.Length == 1);
                DocumentationAttribute doc = attrs[0] as DocumentationAttribute;
                return doc.Value;
            }

            // Look for methods tagged with [PythonName("wellKnownMethodName")]
            attrs = info.GetCustomAttributes(typeof(PythonNameAttribute), false);
            if (attrs.Length > 0) {
                Debug.Assert(attrs.Length == 1);
                PythonNameAttribute pythonNameAttribute = attrs[0] as PythonNameAttribute;
                string defaultDoc = GetDefaultDocumentation(pythonNameAttribute.name);
                if (defaultDoc != null)
                    return defaultDoc;
            }

            return CreateAutoDoc(info);
        }

        public static string CreateAutoDoc(MethodBase info) {
            StringBuilder retType = new StringBuilder();
            StringBuilder ret = new StringBuilder();
            
            int returnCount = 0;
            MethodInfo mi = info as MethodInfo;
            if (mi != null) {
                if (mi.ReturnType != typeof(void)) {
                    retType.Append(GetPythonTypeName(mi.ReturnType));
                    returnCount++;
                }

                if (mi.Name.IndexOf('#') == -1) ret.Append(mi.Name);
                else ret.Append(mi.Name, 0, mi.Name.IndexOf('#'));
            } else {
                ret.Append("__new__");
            }
            
            // For generic methods display either type parameters (for unbound methods) or
            // type arguments (for bound ones).
            if (mi != null && mi.IsGenericMethod) {
                Type[] typePars = mi.GetGenericArguments();
                bool unbound = mi.ContainsGenericParameters;
                ret.Append("[");
                if (typePars.Length > 1)
                    ret.Append("(");

                bool insertComma = false;
                foreach (Type t in typePars) {
                    if (insertComma)
                        ret.Append(", ");
                    if (unbound)
                        ret.Append(t.Name);
                    else
                        ret.Append(GetPythonTypeName(t));
                    insertComma = true;
                }

                if (typePars.Length > 1)
                    ret.Append(")");
                ret.Append("]");
            }

            ret.Append("(");
            bool needComma = false;

            foreach (ParameterInfo pi in info.GetParameters()) {
                if (pi.IsOut || pi.ParameterType.IsByRef) {
                    if (returnCount == 1) {
                        retType.Insert(0, '(');
                    }

                    if(returnCount != 0) retType.Append(", ");

                    returnCount++;

                    retType.Append(GetPythonTypeName(pi.ParameterType));

                    if (pi.IsOut) continue;
                }

                if (needComma) ret.Append(", ");
                ret.Append(GetPythonTypeName(pi.ParameterType));
                ret.Append(" ");
                ret.Append(pi.Name);
                needComma = true;
            }
            ret.Append(")");

            if (returnCount > 1) {
                retType.Append(')');
            }

            if (mi != null && mi.IsStatic) 
                retType.Insert(0, "static ");

            if(retType.Length != 0) retType.Append(' ');

            retType.Append(ret.ToString());

            return retType.ToString();
        }

        public static string CreateEnumDoc(Type t) {
            Debug.Assert(t.IsEnum);

            string[] names = Enum.GetNames(t);
            Array values = Enum.GetValues(t);
            for (int i = 0; i < names.Length; i++) {
                names[i] = String.Concat(names[i],
                    " (",
                    Convert.ChangeType(values.GetValue(i), Enum.GetUnderlyingType(t)).ToString(),
                    ")");
            }

            Array.Sort<string>(names);
            string comment = "";

            if (t.IsDefined(typeof(FlagsAttribute), false))
                comment = " (flags) ";

            return String.Concat("enum ",
                comment,
                GetPythonTypeName(t),
                ", values: ",
                String.Join(", ", names));
        }

        public static bool IsParamArray(ParameterInfo pi) {
            return pi.IsDefined(typeof(ParamArrayAttribute), false);
        }

        public static bool IsParamDict(ParameterInfo pi) {
            return pi.IsDefined(typeof(ParamDictAttribute), false);
        }

        private static string GetPythonTypeName(Type type) {
            if (type.IsByRef) {
                type = type.GetElementType();
            }

            return (string)Ops.GetDynamicTypeFromType(type).__name__;
        }


    }
}
