/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !SILVERLIGHT // XML

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

[assembly: ExtensionType(typeof(XmlElement), typeof(IronPythonTest.AttrInjectorTest.SimpleXmlAttrInjector))]
namespace IronPythonTest {
    public class AttrInjectorTest {
        public static object LoadXml(string text) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);
            return doc.DocumentElement;
        }

        public static class SimpleXmlAttrInjector {
            [SpecialName]
            public static IList<SymbolId> GetMemberNames(object obj) {
                List<SymbolId> list = new List<SymbolId>();
                XmlElement xml = obj as XmlElement;

                if (xml != null) {
                    for (XmlNode n = xml.FirstChild; n != null; n = n.NextSibling) {
                        if (n is XmlElement) {
                            list.Add(SymbolTable.StringToId(n.Name));
                        }
                    }
                }

                return list;
            }

            [SpecialName]
            public static object GetBoundMember(object obj, string name) {
                XmlElement xml = obj as XmlElement;

                if (xml != null) {
                    for (XmlNode n = xml.FirstChild; n != null; n = n.NextSibling) {
                        if (n is XmlElement && string.CompareOrdinal(n.Name, name) == 0) {
                            if (n.HasChildNodes && n.FirstChild == n.LastChild && n.FirstChild is XmlText) {
                                return n.InnerText;
                            } else {
                                return n;
                            }

                        }
                    }
                }

                return NotImplementedType.Value;
            }

        }
    }
}

#endif
