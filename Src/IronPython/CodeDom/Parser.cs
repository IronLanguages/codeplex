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
using System.Text;
using System.IO;
using System.Reflection;

using System.CodeDom;
using System.CodeDom.Compiler;

using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using System.Diagnostics;

namespace IronPython.CodeDom {
    class PythonParser : CodeParser  {
        private List<string> references = new List<string>();
        private SystemState state = new SystemState();

        public PythonParser(List<string> assemblyReferences) {
            if (assemblyReferences.Count == 0) {
                references.Add("System.Windows.Forms");
                references.Add("System");
                references.Add("System.Data");
                references.Add("System.Drawing");
                references.Add("System.Xml");
                references.Add("mscorlib");
            } else {
                references.AddRange(assemblyReferences);
            }
        }

        /// <summary>
        /// Parse an entire file - ideal path as we'll respect
        /// PEP-263 encodings and the such...
        /// </summary>
        public CodeCompileUnit ParseFile(string filename) {
            return Parse(Parser.FromFile(state, new CompilerContext(filename)), filename);
        }

        /// <summary>
        /// Parse an arbitrary stream
        /// </summary>
        public override CodeCompileUnit Parse(System.IO.TextReader codeStream) {
            // get a better filename if we can
            string name = "<unknown>";
            StreamReader sw = codeStream as StreamReader;
            if (sw != null) {                
                FileStream fs = sw.BaseStream as FileStream;
                if (fs != null) name = fs.Name;
            }
            //!!! it'd be nice to have Parser.FromStream to get proper decodings here            
            string codeText = codeStream.ReadToEnd();
            CodeCompileUnit tree = Parse(Parser.FromString(state, new CompilerContext(), codeText), name);

            CodeMerger.CacheCode(tree, codeText);
            return tree;
        }

        /// <summary>
        /// Parses an arbitrary string of Python code
        /// </summary>
        public CodeCompileUnit Parse(string text) {
            return Parse(Parser.FromString(state, new CompilerContext(), text), "<unknown>");
        }

        /// <summary>
        /// Private helper function for all parsing.  Takes in the IronPython Parser 
        /// object and a filename that's used for error reports
        /// </summary>
        /// <param name="p"></param>
        private CodeCompileUnit Parse(Parser p, string filename) {
            
            Statement s = p.ParseFileInput();
            CodeCompileUnit res = new CodeCompileUnit();
            CodeNamespace defaultNamespace = new CodeNamespace();

            //!!! enable AD usage when we're strong named.
            //AppDomainSetup ads = new AppDomainSetup();
            //ads.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //AppDomain ad = AppDomain.CreateDomain("ParserReferenceDomain",null,ads);
            try {
                RemoteReferences rc = new RemoteReferences();
                    /*(RemoteReferences)ad.CreateInstanceAndUnwrap(
                                    Assembly.GetExecutingAssembly().FullName,
                                    "IronPython.CodeDom.RemoteReferences");*/
                rc.Initialize(references);


                CodeWalker cw = new CodeWalker(filename, rc);
                s.Walk(cw);

                CodeObject co = cw.LastObject;
                CodeObjectSuite cos = co as CodeObjectSuite;
                if (cos == null) cos = new CodeObjectSuite(new CodeObject[] { co });

                // walk the top-level and see if we need to create a fake top-type
                // or if we can just stick everything directly into the namespace.
                CodeTypeDeclaration topType = null;
                for (int i = 0; i < cos.Count; i++) {
                    topType = CheckTopLevel(cos[i], res, defaultNamespace, topType);
                }

                // if no namespaces we're added then everything's in our default namespace.
                if (res.Namespaces.Count == 0) {
                    res.Namespaces.Add(defaultNamespace);
                }

                UpdateTopType(topType, defaultNamespace);
            } finally {
                //AppDomain.Unload(ad);
            }
            
            return res;
        }

        private static CodeTypeDeclaration CheckTopLevel(CodeObject cur, CodeCompileUnit res, CodeNamespace defaultNamespace, CodeTypeDeclaration topType) {
            if (cur is CodeNamespaceImport) {
                defaultNamespace.Imports.Add(cur as CodeNamespaceImport);
            } else if (cur is CodeTypeDeclaration) {
                CodeTypeDeclaration ctd = cur as CodeTypeDeclaration;
                bool fRealClass = false;
                foreach(CodeTypeMember mem in ctd.Members) {
                    if (mem is CodeTypeDeclaration) 
                        continue;

                    fRealClass = true;
                    break;
                }

                if (fRealClass) {
                    defaultNamespace.Types.Add(ctd);
                } else {
                    // class w/ nested types, transfer the
                    // type's content to a namespace.
                    TypeToNamespace(res, defaultNamespace, ctd);
                }
            } else if (cur is CodeNamespace) {
                res.Namespaces.Add(cur as CodeNamespace);
            } else if (cur is CodeExpressionStatement) {
                ValidateCodeExpression(cur as CodeExpressionStatement);
            } else {
                if (topType == null) topType = new CodeTypeDeclaration("__top__");
                topType.Members.Add((CodeTypeMember)cur);
            }
            return topType;
        }

        private static void TypeToNamespace(CodeCompileUnit res, CodeNamespace defaultNamespace, CodeTypeDeclaration ctd) {
            CodeNamespace nsRes = new CodeNamespace(ctd.Name);
            res.Namespaces.Add(nsRes);

            foreach (CodeTypeMember mem in ctd.Members) {
                nsRes.Types.Add((CodeTypeDeclaration)mem);
            }

            CopyLineInfo(ctd, nsRes);

            foreach (CodeNamespaceImport cn in defaultNamespace.Imports) {
                nsRes.Imports.Add(cn);
            }
        }

        private static void CopyLineInfo(CodeTypeDeclaration ctd, CodeNamespace nsRes) {
            nsRes.UserData["PreImport"] = true;
            if (ctd.LinePragma != null) {
                nsRes.UserData["IPCreated"] = true;
                nsRes.UserData["Line"] = ctd.LinePragma.LineNumber;
                nsRes.UserData["Column"] = 0;
            } else {
                nsRes.UserData["IPCreated"] = ctd.UserData["IPCreated"];
                nsRes.UserData["Line"] = ctd.UserData["Line"];
                nsRes.UserData["Column"] = ctd.UserData["Column"];
            }
        }

        private static void UpdateTopType(CodeTypeDeclaration topType, CodeNamespace defaultNamespace) {
            if (topType != null) {
                // we had a non-type at the top-level, we need to transfer
                // all our types from the default name space to the top-type
                for (int i = 0; i < defaultNamespace.Types.Count; i++) {
                    topType.Members.Insert(i, defaultNamespace.Types[i]);
                }

                defaultNamespace.Types.Clear();

                topType.UserData["IsTopType"] = true;
            }
        }

        private static void ValidateCodeExpression(CodeExpressionStatement ces) {
            CodeMethodInvokeExpression invoke = ces.Expression as CodeMethodInvokeExpression;
            if (invoke != null) {
                CodeFieldReferenceExpression target = invoke.Method.TargetObject as CodeFieldReferenceExpression;
                if (target != null) {
                    if (target.FieldName != "RealEntryPoint") throw new NotImplementedException("arbitrary calls at the top-level");
                } else if (invoke.Method.MethodName != "RealEntryPoint") throw new NotImplementedException("arbitrary calls at the top-level");

                // otherwise we want to ignore the statement.
            } else {
                throw new NotImplementedException("arbitrary expressions at the top-level");
            }
        }
    }

    
    class CodeObjectSuite : CodeObject {
        List<CodeObject> objects = new List<CodeObject>();

        public CodeObjectSuite() {
        }

        public CodeObjectSuite(CodeObject[] objs) {
            objects.AddRange(objs);
        }

        public int Count {
            get {
                return objects.Count;
            }
        }

        public CodeObject this[int index] {
            get {
                return objects[index];
            }
            set {
                objects[index] = value;
            }
        }

        public void Add(CodeObject co) {
            objects.Add(co);
        }

        public void RemoveAt(int index) {
            objects.RemoveAt(index);
        }
    }
}
