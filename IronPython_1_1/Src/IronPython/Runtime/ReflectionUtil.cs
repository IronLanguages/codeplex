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
using System.Collections.Generic;
using System.IO;

using System.Xml;
using System.Xml.XPath;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    static class ReflectionUtil {

        // Generic type names have the arity (number of generic type paramters) appended at the end. 
        // For eg. the mangled name of System.List<T> is "List`1". This mangling is done to enable multiple 
        // generic types to exist as long as they have different arities.
        internal const char GenericArityDelimiter = '`';

        internal static Version pythonVersion25 = new Version(2, 5);
        private static XPathDocument cachedDoc;
        private static string cachedDocName;

        internal static string GetDefaultDocumentation(string methodName) {
            switch (methodName) {
                case "__abs__": return "x.__abs__() <==> abs(x)";
                case "__add__": return "x.__add__(y) <==> x+y";
                case "__call__": return "x.__call__(...) <==> x(...)";
                case "__cmp__": return "x.__cmp__(y) <==> cmp(x,y)";
                case "__delitem__": return "x.__delitem__(y) <==> del x[y]";
                case "__div__": return "x.__div__(y) <==> x/y";
                case "__eq__": return "x.__eq__(y) <==> x==y";
                case "__floordiv__": return "x.__floordiv__(y) <==> x//y";
                case "__getitem__": return "x.__getitem__(y) <==> x[y]";
                case "__gt__": return "x.__gt__(y) <==> x>y";
                case "__hash__": return "x.__hash__() <==> hash(x)";
                case "__init__": return "x.__init__(...) initializes x; see x.__class__.__doc__ for signature";
                case "__len__": return "x.__len__() <==> len(x)";
                case "__lshift__": return "x.__rshift__(y) <==> x<<y";
                case "__lt__": return "x.__lt__(y) <==> x<y";
                case "__mod__": return "x.__mod__(y) <==> x%y";
                case "__mul__": return "x.__mul__(y) <==> x*y";
                case "__neg__": return "x.__neg__() <==> -x";
                case "__new__": return "T.__new__(S, ...) -> a new object with type S, a subtype of T";
                case "__pow__": return "x.__pow__(y[, z]) <==> pow(x, y[, z])";
                case "__reduce__":
                case "__reduce_ex__": return "helper for pickle";
                case "__rshift__": return "x.__rshift__(y) <==> x>>y";
                case "__setitem__": return "x.__setitem__(i, y) <==> x[i]=";
                case "__str__": return "x.__str__() <==> str(x)";
                case "__sub__": return "x.__sub__(y) <==> x-y";
                case "__truediv__": return "x.__truediv__(y) <==> x/y";
            }

            return null;
        }

        public static string DocOneInfo(PropertyInfo info) {
            object[] attrs = info.GetCustomAttributes(typeof(DocumentationAttribute), false);
            if (attrs.Length == 0) {
                StringBuilder autoDoc = new StringBuilder();

                string summary;
                string returns;
                GetXmlDoc(info, out summary, out returns);

                if (summary != null) {
                    autoDoc.AppendLine(summary);
                    autoDoc.AppendLine();
                }
                
                MethodInfo getter = info.GetGetMethod();
                MethodInfo setter = info.GetSetMethod();
                if (getter != null) {
                    autoDoc.Append("Get: ");
                    autoDoc.AppendLine(CreateAutoDoc(getter, info.Name, 0));
                }

                if (setter != null) {
                    autoDoc.Append("Set: ");
                    autoDoc.Append(CreateAutoDoc(setter, info.Name, 1));
                    autoDoc.AppendLine(" = value");
                }
                return autoDoc.ToString();
            }

            StringBuilder docStr = new StringBuilder();
            for (int i = 0; i < attrs.Length; i++) {
                docStr.Append(((DocumentationAttribute)attrs[i]).Value);
                docStr.Append(Environment.NewLine);
            }
            return docStr.ToString();
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
            string name = null;
            if (attrs.Length > 0) {
                Debug.Assert(attrs.Length == 1);
                PythonNameAttribute pythonNameAttribute = attrs[0] as PythonNameAttribute;
                string defaultDoc = GetDefaultDocumentation(pythonNameAttribute.name);
                if (defaultDoc != null)
                    return defaultDoc;
                name = pythonNameAttribute.name;
            }

            return CreateAutoDoc(info, name, 0);
        }

        public static string CreateAutoDoc(MethodBase info) {
            return CreateAutoDoc(info, null, 0);
        }

        public static string CreateAutoDoc(FieldInfo info) {
            string summary;
            GetXmlDoc(info, out summary);

            return summary;
        }

        public static string CreateAutoDoc(PropertyInfo info) {
            string summary;
            string returns;
            GetXmlDoc(info, out summary, out returns);

            return summary;
        }

        public static string CreateAutoDoc(EventInfo info) {
            string summary;
            string returns;
            GetXmlDoc(info, out summary, out returns);

            return summary;
        }
        
        public static string CreateAutoDoc(Type t) {
            string summary;
            GetXmlDoc(t, out summary);

            if (summary != null)
                return summary;

            if (t.IsEnum) {

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
            return null;
        }

        private static string GetXmlName(Type type) {
            StringBuilder res = new StringBuilder();
            res.Append("T:");

            AppendTypeFormat(type, res);

            return res.ToString();
        }

        private static string GetXmlName(FieldInfo field) {
            StringBuilder res = new StringBuilder();
            res.Append("F:");

            AppendTypeFormat(field.DeclaringType, res);
            res.Append('.');
            res.Append(field.Name);            

            return res.ToString();
        }

        private static string GetXmlName(EventInfo field) {
            StringBuilder res = new StringBuilder();
            res.Append("E:");

            AppendTypeFormat(field.DeclaringType, res);
            res.Append('.');
            res.Append(field.Name);

            return res.ToString();
        }

        private static string GetXmlName(PropertyInfo property) {
            StringBuilder res = new StringBuilder();
            res.Append("P:");

            res.Append(property.DeclaringType.Namespace);
            res.Append('.');
            res.Append(property.DeclaringType.Name);
            res.Append('.');
            res.Append(property.Name);

            return res.ToString();
        }

        private static string GetXmlName(MethodBase info) {
            StringBuilder res = new StringBuilder();
            res.Append("M:");
            res.Append(info.DeclaringType.Namespace);
            res.Append('.');
            res.Append(info.DeclaringType.Name);
            res.Append('.');
            res.Append(info.Name);            
            ParameterInfo []pi = info.GetParameters();
            if (pi.Length > 0) {
                res.Append('(');
                for (int i = 0; i < pi.Length; i++) {
                    Type curType = pi[i].ParameterType;

                    if (i != 0) res.Append(',');
                    AppendTypeFormat(curType, res);
                }
                res.Append(')');
            }
            return res.ToString();
        }

        /// <summary>
        /// Converts a Type object into a string suitable for lookup in the help file.  All generic types are
        /// converted down to their generic type definition.
        /// </summary>
        private static void AppendTypeFormat(Type curType, StringBuilder res) {
            if (curType.IsGenericType) {
                curType = curType.GetGenericTypeDefinition();
            }

            if (curType.IsGenericParameter) {
                res.Append('`');
                res.Append(curType.GenericParameterPosition);
            } else if (curType.ContainsGenericParameters) {
                res.Append(curType.Namespace);
                res.Append('.');
                res.Append(curType.Name.Substring(0, curType.Name.Length - 2));
                res.Append('{');
                Type[] types = curType.GetGenericArguments();
                for (int j = 0; j < types.Length; j++) {
                    if (j != 0) res.Append(',');

                    if (types[j].IsGenericParameter) {
                        res.Append('`');
                        res.Append(types[j].GenericParameterPosition);
                    } else {
                        AppendTypeFormat(types[j], res);
                    }
                }
                res.Append('}');
            } else {
                res.Append(curType.FullName);
            }
        }

        private static string CreateAutoDoc(MethodBase info, string name, int endParamSkip) {
            string summary, returns;
            List<KeyValuePair<string, string>> parameters;

            GetXmlDoc(info, out summary, out returns, out parameters);
            
            StringBuilder retType = new StringBuilder();
            StringBuilder ret = new StringBuilder();

            int returnCount = 0;
            MethodInfo mi = info as MethodInfo;
            if (mi != null) {
                if (mi.ReturnType != typeof(void)) {
                    retType.Append(GetPythonTypeName(mi.ReturnType));
                    returnCount++;
                }

                if (name != null) ret.Append(name);
                else if (mi.Name.IndexOf('#') == -1) ret.Append(mi.Name);
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

            if (mi == null) {
                // constructor, auto-insert cls
                ret.Append("cls");
                needComma = true;
            } else if (!mi.IsStatic) {
                ret.Append("self");
                needComma = true;
            }

            ParameterInfo[] pis = info.GetParameters();
            for (int i = 0; i < pis.Length - endParamSkip; i++) {
                ParameterInfo pi = pis[i];
                if (pi.IsOut || pi.ParameterType.IsByRef) {
                    if (returnCount == 1) {
                        retType.Insert(0, '(');
                    }

                    if (returnCount != 0) retType.Append(", ");

                    returnCount++;

                    retType.Append(GetPythonTypeName(pi.ParameterType));

                    if (pi.IsOut) continue;
                }

                if (needComma) ret.Append(", ");

                if (pi.IsDefined(typeof(ParamArrayAttribute), false)) { 
                    ret.Append("*"); 
                } else if (pi.IsDefined(typeof(ParamDictAttribute), false)) { 
                    ret.Append("**"); 
                } else {
                    ret.Append(GetPythonTypeName(pi.ParameterType));
                    ret.Append(" ");
                }
                ret.Append(pi.Name);
                needComma = true;
            }
            ret.Append(")");

            if (returnCount > 1) {
                retType.Append(')');
            }

            if (retType.Length != 0) retType.Append(' ');

            retType.Append(ret.ToString());

            // Append XML Doc Info if available
            if (summary != null) {
                retType.AppendLine(); retType.AppendLine(); 
                retType.AppendLine(SplitWords(summary));
            }
            
            if (parameters != null && parameters.Count > 0) {
                // always output parameters in order
                for (int j = 0; j < pis.Length; j++) {  
                    for (int i = 0; i < parameters.Count; i++) {
                        if (pis[j].Name == parameters[i].Key) {
                            retType.Append("    ");
                            retType.Append(parameters[i].Key);
                            retType.Append(": ");
                            retType.AppendLine(SplitWords(parameters[i].Value, false));
                            break;
                        }
                    }
                }
            }

            if (returns != null) {
                retType.AppendLine();
                retType.Append("    Returns: ");
                retType.AppendLine(SplitWords(returns, false));
            }

            return retType.ToString();
        }

        private static string SplitWords(string text) {
            return SplitWords(text, true);
        }

        /// <summary>
        /// Splits text to fit into the console window - breaks along words, not characters.
        /// </summary>
        private static string SplitWords(string text, bool indentFirst) {
            const string indent = "    ";

            int splitLen;
            try {
                splitLen = Console.WindowWidth - 30;
            } catch {
                // console output has been redirected.
                splitLen = 80;
            }

            if (text.Length <= splitLen || splitLen <= 0) {
                if(indentFirst) return indent + text;
                return text;
            }
            
            StringBuilder res = new StringBuilder();
            int start = 0, len = splitLen;
            while (start != text.Length) {                
                if (len >= splitLen) {
                    // find last space to break on
                    while (len != 0 && !Char.IsWhiteSpace(text[start + len - 1]))
                        len--;
                }

                if (res.Length != 0) res.Append(' ');
                if(indentFirst || res.Length != 0) res.Append(indent);
                
                if (len == 0) {
                    int copying = Math.Min(splitLen, text.Length - start);
                    res.Append(text, start, copying);
                    start += copying;
                } else {
                    res.Append(text, start, len);
                    start += len;
                }
                res.AppendLine();
                len = Math.Min(splitLen, text.Length - start);
            }
            return res.ToString();
        }

        /// <summary>
        /// Gets the XPathDocument for the specified assembly, or null if one is not available.
        /// </summary>
        private static XPathDocument GetXPathDocument(Assembly asm) {
            System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            
            string location;

            try {
                location = asm.Location;
            } catch {
                return null;
            }

            string baseDir = Path.GetDirectoryName(location);
            string baseFile = Path.GetFileNameWithoutExtension(location) + ".xml";
            string xml = Path.Combine(Path.Combine(baseDir, ci.Name), baseFile);

            if (!System.IO.File.Exists(xml)) {
                int hyphen = ci.Name.IndexOf('-');
                if (hyphen != -1) {
                    xml = Path.Combine(Path.Combine(baseDir, ci.Name.Substring(0, hyphen)), baseFile);
                }
                if (!System.IO.File.Exists(xml)) {
                    xml = Path.Combine(baseDir, baseFile);
                    if (!System.IO.File.Exists(xml)) {
                        return null;
                    }
                }
            }

            XPathDocument xpd;
            
            if (cachedDocName == xml) xpd = cachedDoc;
            else xpd = new XPathDocument(xml);

            cachedDoc = xpd;
            cachedDocName = xml;
            return xpd;
        }

        /// <summary>
        /// Gets the Xml documentation for the specified MethodBase.
        /// </summary>
        private static void GetXmlDoc(MethodBase info, out string summary, out string returns, out List<KeyValuePair<string, string>> parameters) {
            summary = null;
            returns = null;
            parameters = null;

            XPathDocument xpd = GetXPathDocument(info.DeclaringType.Assembly);
            if (xpd == null) return;

            XPathNavigator xpn = xpd.CreateNavigator();
            string path = "/doc/members/member[@name='" + GetXmlName(info) + "']/*";
            XPathNodeIterator iter = xpn.Select(path);

            while (iter.MoveNext()) {
                switch (iter.Current.Name) {
                    case "summary": summary = XmlToString(iter); break;
                    case "returns": returns = XmlToString(iter); break;
                    case "param":
                        string name = null;
                        string paramText = XmlToString(iter);
                        if (iter.Current.MoveToFirstAttribute()) {                            
                            name = iter.Current.Value;
                        }

                        if (name != null) {
                            if (parameters == null) {
                                parameters = new List<KeyValuePair<string, string>>();
                            }

                            parameters.Add(new KeyValuePair<string, string>(name, paramText));
                        }
                        break;
                    case "exception":
                        break;
                }
            }            
        }

        /// <summary>
        /// Gets the Xml documentation for the specified Type.
        /// </summary>
        private static void GetXmlDoc(Type type, out string summary) {
            summary = null;

            XPathDocument xpd = GetXPathDocument(type.Assembly);
            if (xpd == null) return;

            XPathNavigator xpn = xpd.CreateNavigator();
            string path = "/doc/members/member[@name='" + GetXmlName(type) + "']/*";
            XPathNodeIterator iter = xpn.Select(path);

            while (iter.MoveNext()) {
                switch (iter.Current.Name) {
                    case "summary": summary = XmlToString(iter); break;
                }
            }
        }

        /// <summary>
        /// Gets the Xml documentation for the specified Field.
        /// </summary>
        private static void GetXmlDoc(FieldInfo field, out string summary) {
            summary = null;

            XPathDocument xpd = GetXPathDocument(field.DeclaringType.Assembly);
            if (xpd == null) return;

            XPathNavigator xpn = xpd.CreateNavigator();
            string path = "/doc/members/member[@name='" + GetXmlName(field) + "']/*";
            XPathNodeIterator iter = xpn.Select(path);

            while (iter.MoveNext()) {
                switch (iter.Current.Name) {
                    case "summary": summary = XmlToString(iter) + Environment.NewLine; break;
                }
            }
        }

        /// <summary>
        /// Gets the Xml documentation for the specified Field.
        /// </summary>
        private static void GetXmlDoc(PropertyInfo prop, out string summary, out string returns) {
            summary = null;
            returns = null;

            XPathDocument xpd = GetXPathDocument(prop.DeclaringType.Assembly);
            if (xpd == null) return;

            XPathNavigator xpn = xpd.CreateNavigator();
            string path = "/doc/members/member[@name='" + GetXmlName(prop) + "']/*";
            XPathNodeIterator iter = xpn.Select(path);

            while (iter.MoveNext()) {
                switch (iter.Current.Name) {
                    case "summary": summary = XmlToString(iter); break;
                    case "returns": returns = XmlToString(iter); break;
                }
            }
        }

        /// <summary>
        /// Gets the Xml documentation for the specified Field.
        /// </summary>
        private static void GetXmlDoc(EventInfo info, out string summary, out string returns) {
            summary = null;
            returns = null;

            XPathDocument xpd = GetXPathDocument(info.DeclaringType.Assembly);
            if (xpd == null) return;

            XPathNavigator xpn = xpd.CreateNavigator();
            string path = "/doc/members/member[@name='" + GetXmlName(info) + "']/*";
            XPathNodeIterator iter = xpn.Select(path);

            while (iter.MoveNext()) {
                switch (iter.Current.Name) {
                    case "summary": summary = XmlToString(iter) + Environment.NewLine; break;
                }
            }
        }
        /// <summary>
        /// Converts the XML as stored in the config file into a human readable string.
        /// </summary>
        private static string XmlToString(XPathNodeIterator iter) {
            XmlReader xr = iter.Current.ReadSubtree();
            StringBuilder text = new StringBuilder();
            if (xr.Read()) {
                for (; ; ) {
                    switch (xr.NodeType) {
                        case XmlNodeType.Text:
                            text.Append(xr.ReadString());
                            continue;
                        case XmlNodeType.Element:
                            switch (xr.Name) {
                                case "see":
                                    if (xr.MoveToFirstAttribute() && xr.ReadAttributeValue()) {
                                        int arity = xr.Value.IndexOf('`');
                                        if(arity != -1)
                                            text.Append(xr.Value, 2, arity - 2);
                                        else
                                            text.Append(xr.Value, 2, xr.Value.Length - 2);
                                    }
                                    break;
                            }
                            break;
                    }

                    if (!xr.Read()) break;
                }
            }
            return text.ToString();
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
