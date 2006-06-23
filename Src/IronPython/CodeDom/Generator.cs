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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

using System.CodeDom;
using System.CodeDom.Compiler;

namespace IronPython.CodeDom {
    /* CodeGen Notes: 
     * 
     * return types are set using @returns(type) decorator syntax
     * argument types are set using @accepts(type) decorator syntax
     * 
     * we just need to then define these data descriptors somewhere (so
     * our code compiles)
     * 
     * Classes are defined as:
     * 
     * class foo(object):
     *      """type(x) == int, type(y) == System.EventArgs, type(z) == bar"""
     *      __slots__ = ['x', 'y', 'z']
     * 
     * @returns(str)
     * def bar():
     *     return "abc" 
     *      
     * @returns(str)
     * def baz():
     *     x = baz()
     *     return x
     * 
     * 
     */

    partial class PythonGenerator : CodeCompiler, ICodeCompiler, ICodeGenerator {
        CodeEntryPointMethod entryPoint = null;
        string entryPointNamespace = null;
        string lastNamespace;
        Stack<TypeDeclInfo> typeStack = new Stack<TypeDeclInfo>();
        Stack<CodeNamespace> namespaceStack = new Stack<CodeNamespace>();
        int col, row, lastCol, lastRow;
        CodeMerger merger;
        StringBuilder writeCache;   // when merging stores the output between output advancements
        bool suppressFlush = false;
        int lastIndent;
        internal const string ctorFieldInit = "_ConstructorFieldInitFunction";

        class TypeDeclInfo {
            public TypeDeclInfo(CodeTypeDeclaration decl) {
                Declaration = decl;
            }

            public CodeTypeDeclaration Declaration;
            public Nullable<bool> NeedsFieldInit;
        }

        static string[] keywords = new string[]{ "and", "assert", "break", "class", "continue", "def", "del", "elif", "else", "except", "exec", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "not", "or", "pass", "print", "raise", "return", "try", "while", "yield"};
        protected override void GenerateCompileUnit(CodeCompileUnit e) {
#if DEBUG
            try {
#endif
                if (Options != null) {
                    Options.BlankLinesBetweenMembers = false;
                }

                try {
                    // fetch the merger, if one is available.  When a merger
                    // is available our internal write functions will write
                    // to a cache.  When we advance output we'll commit the
                    // cache all at once.
                    merger = CodeMerger.GetCachedCode(e);
                    string oldNewline = Output.NewLine;
                    if (merger != null) {
                        writeCache = new StringBuilder();
                        lastRow = 1;
                        lastCol = 1;
                        col = 1;
                        row = 1;
                        Output.NewLine = "";
                    }


                    base.GenerateCompileUnit(e);

                    if (merger != null) {
                        // flush the writeCache for the last time, we need
                        // to un-supress flush incase the last thing we wrote
                        // suppressed it (this occurs when user code is at the
                        // end of the code compile unit).
                        suppressFlush = false;
                        DoFlush(-1, -1);  

                        Output.Write(merger.FinalizeMerge());
                        Output.NewLine = oldNewline;
                    }
                } finally {
                    merger = null;
                    writeCache = null;
                }
#if DEBUG
            } catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                Debug.Assert(false, String.Format("Unexpected exception: {0}", ex.Message), ex.StackTrace);
            }
#endif
        }
        protected override void GenerateNamespace(CodeNamespace e) {
            if (Options != null) {
                Options.BlankLinesBetweenMembers = false;
            }

            GenerateCommentStatements(e.Comments);
            GenerateNamespaceStart(e);

            GenerateNamespaceImports(e);
            WriteLine("");

            GenerateTypes(e);
            GenerateNamespaceEnd(e);
        }
        #region CodeGenerator abstract overrides
        protected override string CreateEscapedIdentifier(string value) {
            // Python has no identifier escaping...
            return CreateValidIdentifier(value);
        }

        protected override string CreateValidIdentifier(string value) {
            if(IsValidIdentifier(value)) return value;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++) {
                // mangle invalid identifier characters to _hexVal
                if ((value[i] >= 'a' && value[i] <= 'z') ||
                    (value[i] >= 'A' && value[i] <= 'Z') ||
                    (i != 0 && value[i] >= '0' && value[i] <= '9') ||
                    value[i] == '_') {
                    sb.Append(value[i]);
                    continue;
                }
                sb.AppendFormat("_{0:X}", (int)value[i]);
            }

            value = sb.ToString();
            if (!IsValidIdentifier(value)) {
                // keyword
                sb.Append("_");
                return sb.ToString();
            }

            return value;
        }

        protected override void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e) {
            Write(e.ParameterName);
        }

        protected override void GenerateArrayCreateExpression(CodeArrayCreateExpression e) {
            CodeTypeReference elementType = e.CreateType.ArrayElementType;
            if (elementType == null) {
                // This is necessary to support clients which incorrectly pass a non-array
                // type.  CSharpCodeProvider has similar logic.
                elementType = e.CreateType;
            }

            if (e.Initializers.Count > 0) {
                Write("System.Array[");
                OutputType(elementType);
                Write("]");

                Write("((");
                for (int i = 0; i < e.Initializers.Count; i++) {
                    GenerateExpression(e.Initializers[i]);
                    Write(", "); // we can always end Tuple w/ an extra , and need to if the count == 1
                }
                Write("))");
            } else {
                Write("System.Array.CreateInstance(");
                OutputType(elementType);
                Write(",");

                if (e.SizeExpression != null) {
                    GenerateExpression(e.SizeExpression);
                } else {
                    Write(e.Size);
                }
                Write(")");
            }
        }

        protected override void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e) {
            GenerateExpression(e.TargetObject);
            Write("[");
            string comma = "";
            for (int i = 0; i < e.Indices.Count; i++) {
                Write(comma);
                GenerateExpression(e.Indices[i]);
                comma = ", ";
            }
            Write("]");
        }

        protected override void GenerateAssignStatement(CodeAssignStatement e) {
            AdvanceOutput(e);

            GenerateExpression(e.Left);
            Write(" = ");
            GenerateExpression(e.Right);
            WriteLine();
        }

        protected override void GenerateAttachEventStatement(CodeAttachEventStatement e) {
            AdvanceOutput(e);

            GenerateEventReferenceExpression(e.Event);

            Write(" += ");

            CodeObjectCreateExpression coce = e.Listener as CodeObjectCreateExpression;
            if (coce != null && coce.Parameters.Count == 1) {
                // += new Foo(methodname)
                // we want to transform it to:
                // += methodname
                GenerateExpression(coce.Parameters[0]);
            } else {
                GenerateExpression(e.Listener);
            }
            WriteLine();
        }

        protected override void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e) {
            Write("super(type(self), self)");
        }

        protected override void GenerateCastExpression(CodeCastExpression e) {
            GenerateExpression(e.Expression);
        }

        protected override void GenerateComment(CodeComment e) {
            string[] lines = e.Text.Split('\n');
            foreach(string line in lines){
                Write("# ");
                WriteLine(line);
            }
        }

        protected override void GenerateConditionStatement(CodeConditionStatement e) {
            AdvanceOutput(e);

            Write("if ");
            GenerateExpression(e.Condition);
            if (e.TrueStatements.Count != 0) {
                WriteLine(":"); //!!! Consult UserData["NoNewLine"]

                Indent++;
                GenerateStatements(e.TrueStatements);
                Indent--;
            } else {
                WriteLine(": pass"); //!!! Consult UserData["NoNewLine"]
            }
            if (e.FalseStatements != null && e.FalseStatements.Count > 0) {
                WriteLine("else:"); //!!! Consult UserData["NoNewLine"]
                Indent++;
                GenerateStatements(e.FalseStatements);
                Indent--;
            }
        }

        private bool NeedFieldInit() {
            if (typeStack.Peek().NeedsFieldInit == null) {
                bool needsInit = false;
                for (int i = 0; i < CurrentClass.Members.Count; i++) {
                    CodeMemberField field = CurrentClass.Members[i] as CodeMemberField;
                    if (field != null && field.InitExpression != null) {
                        needsInit = true;
                        break;
                    }
                }

                typeStack.Peek().NeedsFieldInit = needsInit;
            }

            return (bool)typeStack.Peek().NeedsFieldInit;
        }

        protected override void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c) {
            FlushOutput(e);
            
            Write("def __init__(self");
            if (e.Parameters.Count > 0) {
                Write(", ");
                OutputParameters(e.Parameters);
            }
            WriteLine("):"); //!!! Consult UserData["NoNewLine"]
            Indent++;

            GenerateStatements(e.Statements);

            if (NeedFieldInit()) {
                bool needsCall = true;
                for (int i = 0; i < e.Statements.Count; i++) {
                    CodeExpressionStatement ces = e.Statements[i] as CodeExpressionStatement;
                    if (ces != null) {
                        CodeMethodInvokeExpression cmie = ces.Expression as CodeMethodInvokeExpression;
                        if (cmie != null) {
                            if (cmie.Method.TargetObject is CodeThisReferenceExpression &&
                                ("_" + cmie.Method.MethodName) == ctorFieldInit) {
                                needsCall = false;
                                break;
                            }
                        }
                    }
                }
                if (needsCall) {
                    WriteLine("self." + ctorFieldInit + "()");
                } else if (e.Statements.Count == 0) {
                    Write("pass");
                }
            } else if (e.Statements.Count == 0) {
                Write("pass");
            }

            Indent--;
            WriteLine();            
        }

        protected override void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e) {
            if (e.TargetObject != null) {
                GenerateExpression(e.TargetObject);
                Write(".");
            }
            if(e.TargetObject is CodeThisReferenceExpression) WritePrivatePrefix(e.MethodName);
            Write(e.MethodName);
        }

        protected override void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e) {
            GenerateExpression(e.TargetObject);

            string comma = "";
            foreach(CodeExpression ce in e.Parameters){
                Write(comma);
                GenerateExpression(ce);
                comma = ", ";
            }
        }

        protected override void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c) {
            FlushOutput(e);

            entryPointNamespace = lastNamespace;
            entryPoint = e;
        }

        protected override void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c) {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void GenerateEventReferenceExpression(CodeEventReferenceExpression e) {
            if (e.TargetObject != null) {
                GenerateExpression(e.TargetObject);
                if (!String.IsNullOrEmpty(e.EventName)) Write(".");
            }
            
            if (!String.IsNullOrEmpty(e.EventName)) {
                if (e.TargetObject is CodeThisReferenceExpression)  WritePrivatePrefix(e.EventName);
                Write(e.EventName);
            }
        }

        private void WritePrivatePrefix(string name) {
            for (int i = 0; i < CurrentClass.Members.Count; i++) {
                if (CurrentClass.Members[i].Name == name) {
                    if ((CurrentClass.Members[i].Attributes & MemberAttributes.AccessMask) == MemberAttributes.Private &&
                        (CurrentClass.Members[i].Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Static)
                        Write("_");
                    break;
                }
            }
        }

        protected override void GenerateExpressionStatement(CodeExpressionStatement e) {
            AdvanceOutput(e);

            if (e.Expression is CodeMethodInvokeExpression) {
                GenerateMethodInvokeExpression(e.Expression as CodeMethodInvokeExpression);
                WriteLine();
            } else {
                GenerateExpression(e.Expression);
            }
        }

        protected override void GenerateField(CodeMemberField e) {
            // init expressions are generated in ctorFieldInit (const string)
            // and calls in the constructors are generated to the function.
        }

        protected override void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e) {
            if (e.TargetObject != null) {
                GenerateExpression(e.TargetObject);
                Write(".");
            }

            if (e.TargetObject is CodeThisReferenceExpression) WritePrivatePrefix(e.FieldName);
            else if (e.TargetObject is CodeTypeReferenceExpression) WritePrivatePrefix(e.FieldName); 

            Write(e.FieldName);
        }

        protected override void GenerateIndexerExpression(CodeIndexerExpression e) {
            GenerateExpression(e.TargetObject);
            Write("[");
            string comma = "";
            for (int i = 0; i < e.Indices.Count; i++) {
                Write(comma);
                GenerateExpression(e.Indices[i]);
                comma = ", ";
            }
            Write("]");
        }

        protected override void GenerateIterationStatement(CodeIterationStatement e) {
            AdvanceOutput(e);

            if(e.InitStatement!=null)
                GenerateStatement(e.InitStatement);
            Write("while ");
            GenerateExpression(e.TestExpression);
            WriteLine(":"); //!!! Consult UserData["NoNewLine"]
            Indent++;
            if (e.Statements.Count == 0) {
                if (e.IncrementStatement == null)
                    WriteLine("pass");
            } else
                GenerateStatements(e.Statements);
            if(e.IncrementStatement!=null)
                GenerateStatement(e.IncrementStatement);
            Indent--;
        }

        
        protected override void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c) {            
            FlushOutput(e);
            if (merger != null && e.UserData["MergeOnly"] != null) {
                MarkMergeOnly(e);
                return;
            }

            if((e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Static) 
                WriteLine("@staticmethod");
            
            string thisName = null;
            if((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Static)
                thisName= UserDataString(e.UserData, "ThisArg", "self");

            string name = e.Name;
            if ((e.Attributes & MemberAttributes.AccessMask) == MemberAttributes.Private) name = "_" + e.Name;

            GenerateMethodWorker(thisName, 
                UserDataString(e.UserData, "ThisType", null),
                name, 
                e.Parameters, 
                e.Statements,
                e.ReturnType, 
                e.UserData);

        }

        protected override void GeneratePrimitiveExpression(CodePrimitiveExpression e) {
            if (e.Value is char) {
                char chVal = (char)e.Value;
                if (chVal > 0xFF || chVal < 32) {
                    Write("System.Convert.ToChar(");
                    Write(((int)chVal).ToString());
                    Write(")");
                } else if (chVal == '\'') {
                    Write("'\\''");
                } else if(chVal == '\\'){
                    Write("'\\\\'");                
                } else {
                    Write("System.Convert.ToChar('");
                    Write(chVal.ToString());
                    Write("')");
                }
                return;
            }

            string strVal = e.Value as string;
            if(strVal != null) {
                for(int i = 0; i<strVal.Length;i++){
                    if (strVal[i] > 0xFF) {
                        // possibly un-encodable unicode characters,
                        // write unicode characters specially...
                        Write("u'");
                        for (i = 0; i < strVal.Length; i++) {
                            if (strVal[i] > 0xFF) {
                                Write(String.Format("\\u{0:X}", (int)strVal[i]));
                            } else if(strVal[i] < 32) {
                                Write(String.Format("\\x{0:X}", (int)strVal[i]));
                            } else if(strVal[i] == '\'') {
                                Write("\\'");
                            } else if (strVal[i] == '\\') {
                                Write("\\");
                            } else {
                                Write(strVal[i]);
                            }
                        }
                        Write("'");
                        return;
                    }
                }
            } 

            Write(Ops.Repr(e.Value));           
        }
        protected override void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e) {
            if (e.Method.TargetObject != null) {
                GenerateExpression(e.Method.TargetObject);
                if(!String.IsNullOrEmpty(e.Method.MethodName)) Write(".");
            }

            if (e.Method.MethodName != null) {
                if (e.Method.TargetObject is CodeThisReferenceExpression)
                    //If the code is invoking the special method ctorFieldInit, then WritePrivatePrefix
                    //won't be able to detect it as a private member of the current class, which it will
                    //eventually become.
                    if ("_" + e.Method.MethodName == ctorFieldInit)
                        Write("_");
                    else
                        WritePrivatePrefix(e.Method.MethodName);
                Write(e.Method.MethodName);
            }
            EmitGenericTypeArgs(e.Method.TypeArguments);

            Write("(");
            OutputExpressionList(e.Parameters);
            Write(")");
        }

        private void EmitGenericTypeArgs(CodeTypeReferenceCollection typeArgs) {
            if (typeArgs != null && typeArgs.Count > 0) {
                Write("[");
                for (int i = 0; i < typeArgs.Count; i++) {
                    if (i != 0) Write(", ");
                    Write(typeArgs[i].BaseType);
                }
                Write("]");
            }
        }

        protected override void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e) {
            GenerateExpression(e.TargetObject);
            Write(".");

            if (e.TargetObject is CodeThisReferenceExpression) WritePrivatePrefix(e.MethodName);

            Write(e.MethodName);

            EmitGenericTypeArgs(e.TypeArguments);
        }

        protected override void GenerateMethodReturnStatement(CodeMethodReturnStatement e) {
            AdvanceOutput(e);

            Write("return ");
            GenerateExpression(e.Expression);
            WriteLine();
        }

        protected override void GenerateNamespaceEnd(CodeNamespace e) {
            if (!String.IsNullOrEmpty(e.Name)) {
                Indent--;
                WriteLine();

                if (typeStack.Count == 0 && entryPointNamespace != null) {
                    // end of the outer most scope, generate the real call
                    // to the entry point if we have one.
                    WriteLine("");
                    WriteLine(String.Format("{0}.RealEntryPoint()", entryPointNamespace));
                    entryPointNamespace = null;
                }

                namespaceStack.Pop();
            }
        }

        protected override void GenerateNamespaceImport(CodeNamespaceImport e) {
            if (namespaceStack.Count == 0) {
                RealGenerateNamespaceImport(e);
            }
        }

        protected override void GenerateNamespaceStart(CodeNamespace e) {
            if (!UserDataFalse(e.UserData, "PreImport")) {
                // loigcally part of the namespace declaration, so
                // we generate these before flushing output (as flushing will advance
                // our cursor past the start of these).
                GenerateNamespaceImportsWorker(e);
            }

            FlushOutput(e);

            if (!String.IsNullOrEmpty(e.Name)) {
                namespaceStack.Push(e);

                lastNamespace = e.Name;

                Write("class ");
                Write(e.Name);
                WriteLine(": # namespace");
                Indent++;

                if (UserDataFalse(e.UserData, "PreImport")) {
                    GenerateNamespaceImportsWorker(e);
                }
            }            
        }

        private void GenerateNamespaceImportsWorker(CodeNamespace e){
            bool fHasClr = false;
            foreach (CodeNamespaceImport cni in e.Imports) {
                RealGenerateNamespaceImport(cni);
                if (cni.Namespace == "clr" && cni.UserData["FromImport"] != null) {
                    fHasClr = true;
                }
            }

            if (!fHasClr) {
                WriteLine("from clr import *");  // import CLR for @returns and @accepts
            }
        }

        private void RealGenerateNamespaceImport(CodeNamespaceImport e) {
            AdvanceOutput(e);

            string fromImport = e.UserData["FromImport"] as string;
            if (fromImport != null) {
                WriteLine(String.Format("from {0} import {1}", e.Namespace, fromImport));
            } else {
                WriteLine(String.Format("import {0}", e.Namespace));
            }
        }

        protected override void GenerateObjectCreateExpression(CodeObjectCreateExpression e) {
            OutputType(e.CreateType);
            EmitGenericTypeArgs(e.CreateType.TypeArguments);
            Write("(");
            OutputExpressionList(e.Parameters);
            Write(")");
        }

        protected override void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c) {
            FlushOutput(e);
            
            string priv = String.Empty;
            if ((e.Attributes & MemberAttributes.AccessMask) == MemberAttributes.Private) priv = "_";

            string thisName = null;
            if ((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Static)
                thisName = UserDataString(e.UserData, "ThisArg", "self");

            if (e.HasGet) {
                //WriteLine(String.Format("#this name {0} {1}", thisName,e.Attributes));
                string getterName = UserDataString(e.UserData, "GetName", priv + "get_" + e.Name);
                
                GenerateMethodWorker(
                    thisName,
                    UserDataString(e.UserData, "ThisType", null),
                    getterName, 
                    new CodeParameterDeclarationExpressionCollection(), 
                    e.GetStatements,
                    e.Type,
                    e.UserData);
            }

            if (e.HasSet) {
                string setterName = UserDataString(e.UserData, "SetName", priv + "set_" + e.Name);

                GenerateMethodWorker(
                    thisName,
                    UserDataString(e.UserData, "ThisType", null),
                    setterName, 
                    new CodeParameterDeclarationExpressionCollection(
                        new CodeParameterDeclarationExpression[] { 
                            new CodeParameterDeclarationExpression(e.Type, "value") }), 
                    e.SetStatements,
                    null,
                    e.UserData);
            }

            string name = priv + e.Name;

            Write(name);
            Write(" = property(");
            string comma = "";
            if (e.HasGet) {
                Write("fget=" + priv + "get_");
                Write(e.Name);
                comma = ",";
            }
            if (e.HasSet) {
                Write(comma);
                Write("fset=" + priv + "set_");
                Write(e.Name);
            }
            if (e.Comments != null && e.Comments.Count > 0) {
                Write(",fdoc=\"\"\"");
                foreach (CodeCommentStatement comment in e.Comments) {
                    if (!comment.Comment.DocComment) continue;

                    WriteLine(comment.Comment.Text);
                }
                Write("\"\"\"");
            }
            WriteLine(")");

        }

        protected override void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e) {
            if (e.TargetObject != null) {
                GenerateExpression(e.TargetObject);
                Write(".");
            }

            if (e.TargetObject is CodeThisReferenceExpression) WritePrivatePrefix(e.PropertyName);

            Write(e.PropertyName);
        }

        protected override void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e) {            
            Write("value");
        }
        
        protected override void GenerateRemoveEventStatement(CodeRemoveEventStatement e) {
            AdvanceOutput(e);

            GenerateEventReferenceExpression(e.Event);

            Write(" -= ");

            GenerateExpression(e.Listener);
            WriteLine();
        }

        protected override void GenerateSnippetExpression(CodeSnippetExpression e) {
            Write(e.Value);
        }

        protected override void GenerateSnippetMember(CodeSnippetTypeMember e) {
            // the codedom base trys to generate w/o indentation, but
            // we need to generate with indentation due to the signficigance
            // of white space.
            
            int oldIndent = Indent;
            Indent = lastIndent;

            FlushOutput(e);
            
            WriteLine("# begin snippet member "+Indent.ToString()+CurrentTypeName);

            string[] lines = e.Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach(string line in lines) {
                WriteLine(line);
            }
            WriteLine("# end snippet member");

            Indent = oldIndent;
        }

        protected override void GenerateSnippetStatement(CodeSnippetStatement e) {
            AdvanceOutput(e);
            int oldIndent = Indent;
            Indent = lastIndent;


            WriteLine("# Snippet Statement");

            string[] lines = e.Value.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in lines) {
                WriteLine(line);
            }
            WriteLine("# End Snippet Statement");


            Indent = oldIndent;
        }
        


        protected override void GenerateThisReferenceExpression(CodeThisReferenceExpression e) {
            Write("self");
        }

        protected override void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e) {
            AdvanceOutput(e);

            Write("raise ");
            GenerateExpression(e.ToThrow);
            WriteLine();
        }

        protected override void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e) {
            AdvanceOutput(e);

            WriteLine("try:"); //!!! Consult UserData["NoNewLine"]
            if (e.CatchClauses.Count != 0 && e.FinallyStatements != null && e.FinallyStatements.Count > 0) {
                Indent++;
                WriteLine("try:"); //!!! Consult UserData["NoNewLine"]
            }

            Indent++;
            if (e.TryStatements != null && e.TryStatements.Count > 0)
                GenerateStatements(e.TryStatements);
            else
                WriteLine("pass");
            Indent--;


            if (e.CatchClauses.Count != 0) {
                for (int i = 0; i < e.CatchClauses.Count; i++) {
                    Write("except ");
                    OutputType(e.CatchClauses[i].CatchExceptionType);
                    if (!String.IsNullOrEmpty(e.CatchClauses[i].LocalName)) {
                        Write(", ");
                        Write(e.CatchClauses[i].LocalName);
                    }
                    if (e.CatchClauses[i].Statements != null && e.CatchClauses[i].Statements.Count > 0) {
                        WriteLine(":"); //!!! Consult UserData["NoNewLine"]
                        Indent++;
                        GenerateStatements(e.CatchClauses[i].Statements);
                        Indent--;
                    } else {
                        WriteLine(": pass"); //!!! Consult UserData["NoNewLine"]
                    }
                }
            }

            if (e.CatchClauses.Count != 0 && e.FinallyStatements != null && e.FinallyStatements.Count > 0) {
                Indent--;
            }

            if (e.FinallyStatements != null && e.FinallyStatements.Count > 0) {
                WriteLine("finally:"); //!!! Consult UserData["NoNewLine"]
                Indent++;
                GenerateStatements(e.FinallyStatements);
                Indent--;
            }
        }

        protected override void GenerateTypeConstructor(CodeTypeConstructor e) {
            FlushOutput(e);

            GenerateStatements(e.Statements);
        }

        private void GenerateFieldInit() {
            WriteLine("def " + ctorFieldInit + "(self):");
            Indent++;

            for (int i = 0; i < CurrentClass.Members.Count; i++) {
                CodeMemberField e = CurrentClass.Members[i] as CodeMemberField;
                if(e == null) continue;

                if (e.InitExpression != null) {
                    //!!! non-static init expression should be moved to constructor
                    FlushOutput(e);

                    Write("self.");
                    if ((e.Attributes & MemberAttributes.AccessMask) == MemberAttributes.Private) Write("_");
                    Write(e.Name);
                    Write(" = ");
                    GenerateExpression(e.InitExpression);
                    WriteLine();
                } else if ((e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Static) {
                    FlushOutput(e);

                    if ((e.Attributes & MemberAttributes.AccessMask) == MemberAttributes.Private) Write("_");
                    Write(e.Name);
                    Write(" = ");
                    switch(e.Type.BaseType){
                        case "bool":
                        case "System.Boolean":
                            Write("False"); break;
                        case "int":
                        case "System.Int32":
                            Write("0"); break;
                        default:
                            Write("None"); break;
                    }
                
                    WriteLine();                
                }
            }
            
            Indent--;
        }

        protected override void GenerateTypeEnd(CodeTypeDeclaration e) {
            if (e.Name != "__top__" || !UserDataFalse(e.UserData, "IsTopType")) {
                
                if (NeedFieldInit()) {
                    GenerateFieldInit();
                }

                TypeDeclInfo popped = typeStack.Pop();
                System.Diagnostics.Debug.Assert(popped.Declaration == e);

                if (UserDataFalse(e.UserData, "NoEmit")) {
                    Indent--;
                    WriteLine();
                }

                if (entryPoint != null) {
                    WriteLine("@staticmethod");
                    WriteLine("def RealEntryPoint():");
                    Indent++;

                    if (entryPoint.Parameters.Count == 1) {
                        // should be args
                        WriteLine("import sys");

                        WriteLine(String.Format("{0} = sys.argv", entryPoint.Parameters[0].Name));
                    }

                    if (entryPoint.Statements != null && entryPoint.Statements.Count > 0)
                        GenerateStatements(entryPoint.Statements);
                    else
                        WriteLine("pass");

                    // return type: either the user has a return statement
                    // or they don't, it's not our problem here.
                    Indent--;

                    entryPoint = null;
                }
            }

        }

        protected override void GenerateTypeStart(CodeTypeDeclaration e) {
            FlushOutput(e);
            if (e.UserData["MergeOnly"] != null) {
                MarkMergeOnly(e);
                return; 
            }

            if (e.Name != "__top__" || !UserDataFalse(e.UserData, "IsTopType")) {

                typeStack.Push(new TypeDeclInfo(e));

                if (!UserDataFalse(e.UserData, "NoEmit")) return;

                Write("class ");
                Write(e.Name);
                Write("(");
                if (e.BaseTypes.Count > 0) {
                    string comma = "";
                    for (int i = 0; i < e.BaseTypes.Count; i++) {
                        Write(comma);
                        OutputType(e.BaseTypes[i]);
                        comma = ",";
                    }
                } else {
                    Write("object");
                }
                if (e.Members.Count == 0) {
                    WriteLine("): pass"); //!!! Consult UserData["NoNewLine"]
                    Indent++;
                } else {
                    WriteLine("):"); //!!! Consult UserData["NoNewLine"]
                    Indent++;

                    bool fHasSlots = UserDataTrue(e.UserData, "HasSlots");

                    if (fHasSlots) {
                        // generate type & slot information, eg:
                        // """type(x) == int, type(y) == System.EventArgs, type(z) == bar"""
                        // __slots__ = ['x', 'y', 'z']

                        bool fOpenedQuote = false;

                        List<string> slots = new List<string>();
                        string comma = "";
                        for (int i = 0; i < e.Members.Count; i++) {                            
                            CodeMemberField cmf = e.Members[i] as CodeMemberField;
                            if (cmf != null && (cmf.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Static) {                                
                                if (!fOpenedQuote) { Write("\"\"\""); fOpenedQuote = true; }

                                string name;
                                
                                if ((cmf.Attributes & MemberAttributes.AccessMask) == MemberAttributes.Private) {
                                    name = "_" + cmf.Name;
                                } else {
                                    name = cmf.Name;
                                }
                                
                                Write(comma);
                                Write("type(");
                                Write(name);
                                Write(") == ");
                                GenerateTypeReferenceExpression(new CodeTypeReferenceExpression(cmf.Type));
                                comma = ", ";

                                slots.Add(name);
                            }

                            CodeMemberEvent cme = e.Members[i] as CodeMemberEvent;
                            if (cme != null) {
                                if (!fOpenedQuote) { Write("\"\"\""); fOpenedQuote = true; }

                                string name;
                                if ((cme.Attributes & MemberAttributes.AccessMask) == MemberAttributes.Private) {
                                    name = "_" + cme.Name;
                                } else {
                                    name = cme.Name;
                                }

                                Write(comma);
                                Write("type(");
                                Write(name);
                                Write(") == ");
                                GenerateTypeReferenceExpression(new CodeTypeReferenceExpression(cme.Type));
                                comma = ", ";

                                slots.Add(name);
                            }
                        }
                        if (fOpenedQuote) WriteLine("\"\"\"");

                        Write("__slots__ = [");
                        comma = "";
                        foreach (string slot in slots) {
                            Write(comma);
                            Write("'");
                            Write(slot);
                            Write("'");
                            comma = ", ";
                        }
                        WriteLine("]");

                        for (int i = 0; i < e.Members.Count; i++) {
                            CodeMemberField cmf = e.Members[i] as CodeMemberField;
                            if (cmf != null && (cmf.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Static) {
                                if (cmf.Type.BaseType == "System.Boolean" || cmf.Type.BaseType == "bool")
                                    WriteLine(String.Format("{0} = False", cmf.Name));
                                else
                                    WriteLine(String.Format("{0} = None", cmf.Name));
                            }
                        }
                    }
                }
            }
            lastIndent = Indent;
        }

        protected override void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e) {
            // if we have no init expression then we don't
            // need to declare the variable yet.  Once we
            // have the value we will infer it's type via
            // the parser on the re-parse and generate a
            // VariableDeclarationStatement.
            if (e.InitExpression != null) {
                AdvanceOutput(e);

                Write(e.Name);
                Write(" = ");
                GenerateExpression(e.InitExpression);
                WriteLine();
            }
        }

        protected override void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e) {
            Write(e.VariableName);
        }

        protected override string GetTypeOutput(CodeTypeReference value) {
            if (value.ArrayRank > 0) {
                return "System.Array[" + GetTypeOutput(value.ArrayElementType) + "]";
            }

            if (value.TypeArguments != null && value.TypeArguments.Count > 0) {
                // generate generic type reference
                string nonGenericName = value.BaseType.Substring(0, value.BaseType.LastIndexOf('`'));
                StringBuilder baseName = new StringBuilder(PythonizeType(nonGenericName));

                baseName.Append('[');
                string comma = "";
                for (int i = 0; i < value.TypeArguments.Count; i++) {
                    baseName.Append(comma);
                    baseName.Append(GetTypeOutput(value.TypeArguments[i]));
                    comma = ", ";
                }
                baseName.Append(']');
                return baseName.ToString();                
            }

            return PythonizeType(value.BaseType);
        }

        private static string PythonizeType(string baseType) {
            if (baseType == "Boolean" || baseType == "System.Boolean") {
                return "bool";
            /*} else if (baseType == "System.Int32") {
                return "int";
            } else if (baseType == "System.String") {
                return "str";*/
            } else if (baseType == "Void" || baseType == "System.Void" || baseType == "void") {
                return "None";
            }
            return baseType;
        }

        protected override bool IsValidIdentifier(string value) {
            for (int i = 0; i < keywords.Length; i++) {
                if (keywords[i] == value) return false;
            }
            for (int i = 0; i < value.Length; i++) {
                if ((value[i] >= 'a' && value[i] <= 'z') ||
                    (value[i] >= 'A' && value[i] <= 'Z') ||
                    (i != 0 && value[i] >= '0' && value[i] <= '9') ||
                    value[i] == '_') {
                    continue;
                }
                return false;
            }
            return true;
        }

        protected override string NullToken {
            get { return "None"; }
        }

        protected override void OutputType(CodeTypeReference typeRef) {
            Write(GetTypeOutput(typeRef));
        }

        protected override string QuoteSnippetString(string value) {
            return (string)Ops.Repr(value);
        }

        protected override bool Supports(GeneratorSupport support) {
            switch (support) {
                case GeneratorSupport.ArraysOfArrays: return true;
                case GeneratorSupport.AssemblyAttributes: return false;
                case GeneratorSupport.ChainedConstructorArguments: return false;
                case GeneratorSupport.ComplexExpressions: return true;
                case GeneratorSupport.DeclareDelegates: return false;
                case GeneratorSupport.DeclareEnums: return false;
                case GeneratorSupport.DeclareEvents: return false;
                case GeneratorSupport.DeclareIndexerProperties: return true;
                case GeneratorSupport.DeclareInterfaces: return false;
                case GeneratorSupport.DeclareValueTypes: return false;
                case GeneratorSupport.EntryPointMethod: return true;
                case GeneratorSupport.GenericTypeDeclaration: return false;
                case GeneratorSupport.GenericTypeReference: return true;
                case GeneratorSupport.GotoStatements: return false;
                case GeneratorSupport.MultidimensionalArrays: return true;
                case GeneratorSupport.MultipleInterfaceMembers: return false;
                case GeneratorSupport.NestedTypes: return true;
                case GeneratorSupport.ParameterAttributes: return false;
                case GeneratorSupport.PartialTypes: return false;
                case GeneratorSupport.PublicStaticMembers: return true;
                case GeneratorSupport.ReferenceParameters: return false;
                case GeneratorSupport.Resources: return false;
                case GeneratorSupport.ReturnTypeAttributes: return false;
                case GeneratorSupport.StaticConstructors: return false;
                case GeneratorSupport.TryCatchStatements: return true;
                case GeneratorSupport.Win32Resources: return false;
            }
            return false;
        }

        #region Not supported overrides
        protected override void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes) {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes) {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void GenerateGotoStatement(CodeGotoStatement e) {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void GenerateLabeledStatement(CodeLabeledStatement e) {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void GenerateLinePragmaEnd(CodeLinePragma e) {
        }

        protected override void GenerateLinePragmaStart(CodeLinePragma e) {
        }
        #endregion

        #endregion

        #region Non-required overrides
        protected override void OutputParameters(CodeParameterDeclarationExpressionCollection parameters) {
            string comma = "";
            for (int i = 0; i < parameters.Count; i++) {
                Write(comma);
                Write(parameters[i].Name);
                comma = ",";
            }
        }
        protected override void GenerateTypeOfExpression(CodeTypeOfExpression e) {
            if (e.Type.BaseType == this.typeStack.Peek().Declaration.Name) {
                CodeMemberMethod curMeth = CurrentMember as CodeMemberMethod;
                if (curMeth != null) {
                    if((curMeth.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Static)
                        throw new InvalidOperationException("can't access current type in static scope");                    
                }
                Write("self.__class__");
            } else {
                OutputType(e.Type);
            }
        }

        protected override void ContinueOnNewLine(string st) {
            WriteLine("\\");
        }

        protected override void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e) {
            GenerateExpression(e.Left);
            switch (e.Operator) {
                case CodeBinaryOperatorType.Add: Write(" + "); break;
                case CodeBinaryOperatorType.Assign: Write(" = "); break;
                case CodeBinaryOperatorType.BitwiseAnd: Write(" & "); break;
                case CodeBinaryOperatorType.BitwiseOr: Write(" | "); break;
                case CodeBinaryOperatorType.BooleanAnd: Write(" and "); break;
                case CodeBinaryOperatorType.BooleanOr: Write(" or "); break;
                case CodeBinaryOperatorType.Divide: Write(" / "); break;
                case CodeBinaryOperatorType.GreaterThan: Write(" > "); break;
                case CodeBinaryOperatorType.GreaterThanOrEqual: Write(" >= "); break;
                case CodeBinaryOperatorType.IdentityEquality: Write(" is "); break;
                case CodeBinaryOperatorType.IdentityInequality: Write(" != "); break;
                case CodeBinaryOperatorType.LessThan: Write(" < "); break;
                case CodeBinaryOperatorType.LessThanOrEqual: Write(" <= "); break;
                case CodeBinaryOperatorType.Modulus: Write(" % "); break;
                case CodeBinaryOperatorType.Multiply: Write(" * "); break;
                case CodeBinaryOperatorType.Subtract: Write(" - "); break;
                case CodeBinaryOperatorType.ValueEquality: Write(" == "); break;
            }
            if (e.Right is CodeBinaryOperatorExpression) Write("(");
            GenerateExpression(e.Right);
            if (e.Right is CodeBinaryOperatorExpression) Write(")");
        }

        #endregion

        #region Overrides to ensure only we call Write
        public override void GenerateCodeFromMember(CodeTypeMember member, System.IO.TextWriter writer, CodeGeneratorOptions options) {
            CodeGeneratorOptions opts = (options == null) ? new CodeGeneratorOptions() : options;
            opts.BlankLinesBetweenMembers = false;
            
            base.GenerateCodeFromMember(member, writer, opts);
        }
        protected override void OutputAttributeDeclarations(CodeAttributeDeclarationCollection attributes) {
            //!!! Implement me
        }

        protected override void OutputAttributeArgument(CodeAttributeArgument arg) {
            //!!! Implement me
        }

        protected override void OutputTypeAttributes(System.Reflection.TypeAttributes attributes, bool isStruct, bool isEnum) {
            //!!! implement me
        }

        protected override void OutputDirection(FieldDirection dir) {
            // not supported in Python
        }

        protected override void OutputFieldScopeModifier(MemberAttributes attributes) {
            // not supported in Python
        }

        protected override void OutputMemberAccessModifier(MemberAttributes attributes) {
            // not supported in Python
        }

        protected override void OutputMemberScopeModifier(MemberAttributes attributes) {
            // not supported in Python
        }

        protected override void OutputTypeNamePair(CodeTypeReference typeRef, string name) {
            OutputType(typeRef);
            Write(" ");
            OutputIdentifier(name);
        }

        protected override void OutputIdentifier(string ident) {
            Write(ident);
        }

        protected override void OutputExpressionList(CodeExpressionCollection expressions, bool newlineBetweenItems) {
            bool first = true;
            IEnumerator en = expressions.GetEnumerator();
            Indent++;
            while (en.MoveNext()) {
                if (first) {
                    first = false;
                } else {
                    if (newlineBetweenItems) ContinueOnNewLine(",");
                    else Write(", ");
                }
                GenerateExpression((CodeExpression)en.Current);
            }
            Indent--;
        }

        protected override void OutputOperator(CodeBinaryOperatorType op) {
            switch (op) {
                case CodeBinaryOperatorType.Add:  Write("+"); break;
                case CodeBinaryOperatorType.Subtract: Write("-"); break;
                case CodeBinaryOperatorType.Multiply: Write("*"); break;
                case CodeBinaryOperatorType.Divide: Write("/"); break;
                case CodeBinaryOperatorType.Modulus: Write("%"); break;
                case CodeBinaryOperatorType.Assign: Write("="); break;
                case CodeBinaryOperatorType.IdentityInequality: Write("!="); break;
                case CodeBinaryOperatorType.IdentityEquality: Write("=="); break;
                case CodeBinaryOperatorType.ValueEquality: Write("=="); break;
                case CodeBinaryOperatorType.BitwiseOr: Write("|"); break;
                case CodeBinaryOperatorType.BitwiseAnd: Write("&"); break;
                case CodeBinaryOperatorType.BooleanOr: Write("||"); break;
                case CodeBinaryOperatorType.BooleanAnd: Write("&&"); break;
                case CodeBinaryOperatorType.LessThan: Write("<"); break;
                case CodeBinaryOperatorType.LessThanOrEqual: Write("<="); break;
                case CodeBinaryOperatorType.GreaterThan: Write(">"); break;
                case CodeBinaryOperatorType.GreaterThanOrEqual: Write(">="); break;
            }
        }

        protected override void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e) {
            if (e.CustomAttributes.Count > 0) {
                OutputAttributeDeclarations(e.CustomAttributes);
                Write(" ");
            }

            OutputDirection(e.Direction);
            OutputTypeNamePair(e.Type, e.Name);
        }

        protected override void GenerateSingleFloatValue(float s) {
            Write(Ops.Repr(s));
        }

        protected override void GenerateDoubleValue(double d) {
            Write(Ops.Repr(d));
        }

        protected override void GenerateDecimalValue(decimal d) {
            Write(Ops.Repr(d));
        }

        #endregion

        protected void OutputParameters(string instanceName, CodeParameterDeclarationExpressionCollection parameters) {
            string comma = "";
            if (instanceName != null) {
                Write(instanceName);
                comma = ", ";
            }
            for (int i = 0; i < parameters.Count; i++) {
                Write(comma);
                Write(parameters[i].Name);
                comma = ", ";
            }
        }

        protected override void GenerateTypeReferenceExpression(CodeTypeReferenceExpression e) {
            if(e.Type.BaseType == "void"){
                Write("System.Void");
            }else{
                base.GenerateTypeReferenceExpression(e);
            }

        }
        private void GenerateMethodWorker(string instanceName, string instanceType, string name, CodeParameterDeclarationExpressionCollection parameters, CodeStatementCollection stmts, CodeTypeReference retType, IDictionary userData) {
            // generate decorators w/ type info
            if (UserDataTrue(userData, "HasAccepts")) {
                Write("@accepts(");
                string comma = "";
                // Self() is defined in clr, returns None.
                if (instanceType != null) {
                    Write("Self()");
                    comma = ", ";
                } else if(instanceName != null) {
                    Write("Self()");
                    comma = ", ";
                }
                foreach (CodeParameterDeclarationExpression param in parameters) {
                    Write(comma);
                    GenerateTypeReferenceExpression(new CodeTypeReferenceExpression(param.Type));
                    comma = ", ";
                }
                WriteLine(")");
            }

            if (retType != null) {
                if (UserDataTrue(userData, "HasReturns")) {
                    Write("@returns(");
                    GenerateTypeReferenceExpression(new CodeTypeReferenceExpression(retType));
                    WriteLine(")");
                }
            }


            // generate raw method body
            Write("def ");
            Write(name);
            Write("(");
            OutputParameters(instanceName, parameters);

            int cursorCol, cursorRow;
            if (stmts.Count != 0) {
                WriteLine("):"); //!!! Consult UserData["NoNewLine"]
                Indent++;
                lastIndent = Indent;
                GenerateStatements(stmts);
                Indent--;
                cursorCol = col;
                cursorRow = row;
            } else {
                WriteLine("):"); //!!! Consult UserData["NoNewLine"]
                Indent++;
                Write("pass");
                cursorCol = col - 3;
                cursorRow = row;
                WriteLine();
                Indent--;
            }

            WriteLine("");

            // store the location the cursor shold goto for this function.
            userData[typeof(System.Drawing.Point)] = new System.Drawing.Point(cursorCol, cursorRow);
        }

        private string UserDataString(IDictionary userData, string name, string defaultValue) {
            if (userData == null) return defaultValue;
            string res = userData[name] as string;
            if (res == null) return defaultValue;
            return res;
        }

        private bool UserDataFalse(IDictionary userData, string name) {
            return userData == null ||
                userData[name] == null ||
                ((bool)userData[name]) == false;
        }
        
        private bool UserDataTrue(IDictionary userData, string name) {
            return userData == null || 
                userData[name] == null || 
                ((bool)userData[name]) == true;
        }

        private void MarkMergeOnly(CodeTypeMember ctm) {
            suppressFlush = true;

            ctm.UserData["IPCreated"] = true;
            ctm.UserData["Column"] = col;
            ctm.LinePragma.LineNumber = row;
        }

        private void AdvanceOutput(CodeStatement co) {
            if (merger != null && co.UserData["IPCreated"] != null && co.LinePragma != null) {
                while (row < co.LinePragma.LineNumber) {
                    WriteLine();
                }
            }
        }

        private void AdvanceOutput(CodeObject co) {
            if (merger != null && co.UserData["IPCreated"] != null) {
                int line = (int)co.UserData["Line"];
                while (row < line) {
                    WriteLine();
                }
            }
        }

        private void FlushOutput(CodeTypeMember ctm) {
            if (ctm.UserData["IPCreated"] != null && ctm.LinePragma != null) {
                int line = ctm.LinePragma.LineNumber;
                int column = (int)ctm.UserData["Column"];

                DoFlush(line, column);
            }

            if (merger != null) {
                // update line information for round-tripping
                if (ctm.LinePragma == null) ctm.LinePragma = new CodeLinePragma();

                ctm.UserData["IPCreated"] = true;
                ctm.LinePragma.LineNumber = row;
                ctm.UserData["Column"] = col;
            }
        }

        /// <summary>
        /// Flushes output to the merger based upon the location
        /// of this object.
        /// </summary>
        private void FlushOutput(CodeObject co) {
            int line = row;
            if (co.UserData["IPCreated"] != null) {
                line = (int)co.UserData["Line"];
                int column = (int)co.UserData["Column"];

                DoFlush(line, column);
            }

            if (merger != null) {
                AdvanceOutput(co);

                // update line information for round-tripping
                co.UserData["IPCreated"] = true;
                co.UserData["Line"] = row;
                co.UserData["Column"] = col;
            }
        }

        /// <summary> Performs the actual work of flushing output </summary>
        private void DoFlush(int line, int column) {
            if (merger != null) {
                if (!suppressFlush) {
                    // last write was a merge only item, therefore we
                    // don't merge now, we just update lastRow/lastCol.
                    merger.DoMerge(lastRow, lastCol, line, col, writeCache.ToString());
                    writeCache.Length = 0;
                } else {
                    // we've skipped the merged output, and now we're 
                    // writing where this new element starts, update
                    // our output position.
                    row = line + merger.LineDelta;
                    col = 0;
                }
                Debug.Assert(writeCache.Length == 0);

                suppressFlush = false;
            }

            lastRow = line;
            lastCol = column;
        }        

        private void Write(object val) {
            Write(val.ToString());
        }
        
        private void Write(int val) {            
            Write(val.ToString());
        }

        private void WriteLine() {
            WriteLine("");
        }

        private void Write(string txt) {
            if(merger != null){
                string[] lines = txt.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                
                for (int i = 0; i < lines.Length; i++) {
                    if (i != 0) {
                        writeCache.AppendLine();
                        for (int j = 0; j < Indent; j++) writeCache.Append("    ");
                        col = Indent * 4;
                        row++;
                    } else if (col == 0) {
                        for (int j = 0; j < Indent; j++) writeCache.Append("    ");
                        col = Indent * 4;
                    }

                    writeCache.Append(lines[i]);
                    col += lines[i].Length;
                }                
            }else{
                Output.Write(txt);
            }
        }

        private void WriteLine(string txt) {
            if (merger != null) {
                string[] lines = txt.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length; i++) {
                    if (col == 0) {
                        for (int j = 0; j < Indent; j++) writeCache.Append("    ");
                    }

                    writeCache.AppendLine(lines[i]);
                    col = 0;
                    row++;
                }
            } else {
                Output.WriteLine(txt);
            }
        }

    }
}