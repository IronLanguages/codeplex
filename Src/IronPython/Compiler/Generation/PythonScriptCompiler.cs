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
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Hosting;
using System.Reflection;
using System.Reflection.Emit;

using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Compiler;

using PythonAst = IronPython.Compiler.Ast;
using IronPython.Runtime.Types;

namespace IronPython.Compiler.Generation {
    public class PythonScriptCompiler : ScriptCompiler {

        public new PythonEngine Engine {
            get { return (PythonEngine)base.Engine; }
        }

        internal PythonScriptCompiler(PythonEngine engine)
            : base(engine) {            
        }

        public PythonScriptCompiler()
            : base(PythonEngine.CurrentEngine) {

        }

        public override CodeBlock ParseInteractiveCode(CompilerContext cc, bool allowIncomplete, out InteractiveCodeProperties properties) {

            PythonAst.Statement ps;
            using (Parser parser = Parser.CreateParser(cc, Engine.Options)) {
                ps = parser.ParseInteractiveInput(allowIncomplete, true, out properties);
            }

            return ps != null ? BindAndTransform(cc, ps, false, true) : null;
        }

        public override CodeBlock ParseExpressionCode(CompilerContext cc) {
            PythonAst.Expression e;
            using (Parser parser = Parser.CreateParser(cc, Engine.Options)) {
                e = parser.ParseTestListAsExpression();
            }
            
            PythonAst.ReturnStatement rs = new IronPython.Compiler.Ast.ReturnStatement(e);
            return BindAndTransform(cc, rs, false);
        }

        public override CodeBlock ParseStatementCode(CompilerContext cc) {
            Ast.Statement ast;
            using (Parser parser = Parser.CreateParser(cc, Engine.Options)) {
                ast = parser.ParseSingleStatement();
            }

            return BindAndTransform(cc, ast, false);
        }

        public override CodeBlock ParseFile(CompilerContext cc) {
            Ast.Statement ast;
            using (Parser parser = Parser.CreateParser(cc, Engine.Options)) {
                ast = parser.ParseFileInput();
            }

            return BindAndTransform(cc, ast, true);
        }

#if !SILVERLIGHT
        public override SourceUnit ParseCodeDom(System.CodeDom.CodeObject codeDom) {
            return new PythonCodeDomCodeGen().GenerateCode(Engine, codeDom);
        }
#endif

        #region Implementation

        public static CodeBlock BindAndTransform(CompilerContext context, PythonAst.Statement statement, bool module) {
            return BindAndTransform(context, statement, module, false);
        }

        public static CodeBlock BindAndTransform(CompilerContext context, PythonAst.Statement statement, bool module, bool print) {
            PythonAst.PythonAst past = PythonAst.PythonNameBinder.Bind(statement, context, module);
            return PythonAst.AstGenerator.TransformAst(context, past, print);
        }
        
        #endregion
    }
}
