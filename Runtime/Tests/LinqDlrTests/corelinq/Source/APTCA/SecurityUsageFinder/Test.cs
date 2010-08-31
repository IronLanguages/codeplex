extern alias Core;
using AltCore=Core.System.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using System.Reflection;
using System.IO;

using System.Security;
using System.Security.Permissions;
using System.Xml;

class Test
{
    static XmlElement GetTypeAttributes(Type[] ts, XmlDocument dom)
    {
        XmlElement types = dom.CreateElement("Types");
        foreach (Type t in ts)
        {
            if (t.FullName.IndexOf("Linq") >= 0)
            {
                XmlElement atts = GetSecurityAttributes(t.GetCustomAttributes(true), dom);
                XmlElement members = VisitMembers(t, dom);
                XmlElement subTypes = GetTypeAttributes(t.GetNestedTypes(), dom);

                if (atts != null || members != null || subTypes != null)
                {
                    XmlElement type = dom.CreateElement("Type");
                    XmlAttribute name = dom.CreateAttribute("name");
                    name.Value = t.ToString();
                    type.Attributes.Append(name);
                    if (atts != null)
                    {
                        type.AppendChild(atts);
                    }
                    if (members != null)
                    {
                        type.AppendChild(members);
                    }
                    if (subTypes != null)
                    {
                        type.AppendChild(subTypes);
                    }
                    types.AppendChild(type);
                }
            }
        }
        return types.ChildNodes.Count == 0 ? null : types;
    }

    static XmlElement GetSecurityAttributes(object[] atts, XmlDocument dom)
    {
        XmlElement attributes = dom.CreateElement("Attributes");
        foreach (Attribute a in atts)
        {
            if (a.ToString().StartsWith("System.Security"))
            {
                XmlNode attribute = dom.CreateElement("Attribute");
                attribute.InnerText = a.ToString();
                attributes.AppendChild(attribute);
            }
        }
        return attributes.ChildNodes.Count == 0 ? null : attributes;
    }

    static XmlElement VisitMembers(Type t, XmlDocument dom)
    {
        //PermissionSet perms = new PermissionSet(PermissionState.None);
        //perms.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
        //perms.Assert();

        XmlElement members = dom.CreateElement("Members");

        foreach (MemberInfo member in t.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            XmlElement attributes = GetSecurityAttributes(member.GetCustomAttributes(true), dom);
            string methodBodyCalls = "";
            if (member is MethodInfo)
            {
                MethodInfo method = member as MethodInfo;
                MethodBodyReader mr = new MethodBodyReader();
                methodBodyCalls = MethodBodyReader.GetSecurityCalls(method);
            }
            if (attributes != null || methodBodyCalls.IndexOf("System.Security") >= 0)
            {
                XmlElement xmember = dom.CreateElement("Member");
                XmlAttribute name = dom.CreateAttribute("name");
                XmlAttribute type = dom.CreateAttribute("type");
                name.Value = member.ToString();
                type.Value = member.MemberType.ToString();
                xmember.Attributes.Append(name);
                xmember.Attributes.Append(type);
                if (attributes != null)
                {
                    xmember.AppendChild(attributes);
                }
                if (methodBodyCalls.IndexOf("System.Security") >= 0)
                {
                    XmlElement code = dom.CreateElement("Code");
                    code.AppendChild(dom.CreateCDataSection(methodBodyCalls));
                    xmember.AppendChild(code);
                }
                members.AppendChild(xmember);
            }
        }
        return members.ChildNodes.Count == 0 ? null : members;
    }

    static bool Equal(XmlNode doc1, XmlNode doc2)
    {
        if (doc1.Name != doc2.Name) return false;
        if (doc1.ChildNodes.Count != doc2.ChildNodes.Count) return false;
        if (doc1.ChildNodes.Count == 0) return (doc1.InnerText == doc2.InnerText);
        foreach (XmlNode node1 in doc1.ChildNodes)
        {
            foreach (XmlNode node2 in doc2.ChildNodes)
            {
                if (Equal(node1, node2))
                {
                    doc2.RemoveChild(node2);
                }
            }
        }
        return doc2.ChildNodes.Count == 0;
    }

    static int Main(string[] args)
    {
        MethodBodyReader.LoadOpCodes();

        XmlDocument dom = new XmlDocument();

        StringBuilder result = new StringBuilder();

        //Assembly a = Assembly.GetExecutingAssembly();
        Assembly a = Assembly.Load(@"System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

        XmlElement root = dom.CreateElement("Assembly");
        XmlAttribute name = dom.CreateAttribute("name");
        name.Value = a.FullName;
        root.Attributes.Append(name);

        XmlElement atts = GetSecurityAttributes(a.GetCustomAttributes(true), dom);
        if (atts != null)
        {
            root.AppendChild(atts);
        }
        XmlElement types = GetTypeAttributes(a.GetTypes(), dom);
        if (types != null)
        {
            root.AppendChild(types);
        }
        dom.AppendChild(root);

        XmlNode docEleme = dom.DocumentElement;
        XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "UTF-8", "yes");
        decl.Encoding = "UTF-8";
        decl.Standalone = "yes";
        dom.InsertBefore(decl, docEleme);

        dom.Save(@"Log.xml");

        XmlDocument doc2 = new XmlDocument();
        doc2.Load(@"Baseline.xml");

        Console.WriteLine("Expected:");
        Console.WriteLine(doc2.OuterXml);
        Console.WriteLine("Result:");
        Console.WriteLine(dom.OuterXml);

        bool equal = Equal(dom.DocumentElement, doc2.DocumentElement);

        Console.WriteLine("Test: " + (equal ? "PASSED" : "FAILED"));

        return equal ? 0 : 1;
    }
}
