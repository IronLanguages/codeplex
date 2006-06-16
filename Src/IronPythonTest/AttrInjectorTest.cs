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
using System.Xml;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPythonTest {
    public class AttrInjectorTest {
        static AttrInjectorTest() {
            Ops.RegisterAttributesInjectorForType(typeof(XmlElement), new SimpleXmlAttrInjector(), true);
        }

        public static object LoadXml(string text) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);
            return doc.DocumentElement;
        }

        class SimpleXmlAttrInjector : IAttributesInjector {
            List IAttributesInjector.GetAttrNames(object obj) {
                List list = List.MakeEmptyList(0);
                XmlElement xml = obj as XmlElement;

                if (xml != null) {
                    for (XmlNode n = xml.FirstChild; n != null; n = n.NextSibling) {
                        if (n is XmlElement) {
                            list.Add(n.Name);
                        }
                    }
                }

                return list;
            }

            bool IAttributesInjector.TryGetAttr(object obj, SymbolId nameSymbol, out object value) {
                XmlElement xml = obj as XmlElement;

                if (xml == null) {
                    value = null;
                    return false;
                }

                string name = nameSymbol.ToString();

                for (XmlNode n = xml.FirstChild; n != null; n = n.NextSibling) {
                    if (n is XmlElement && string.CompareOrdinal(n.Name, name) == 0) {
                        if (n.HasChildNodes && n.FirstChild == n.LastChild && n.FirstChild is XmlText) {
                            value = n.InnerText;
                        }
                        else {
                            value = n;
                        }

                        return true;
                    }
                }

                value = null;
                return false;
            }
        }
    }
}
