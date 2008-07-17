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

using System.Scripting;
using System.Scripting.Runtime;
using ToyScript.Parser;
using ToyScript.Runtime;
using MSAst = System.Linq.Expressions;

namespace ToyScript {
    public class ToyLanguageContext : LanguageContext {
        public ToyLanguageContext(ScriptDomainManager manager) : base(manager) { 
            Binder = new ToyBinder(manager);
        }

        protected override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink) {
            ToyParser tp = new ToyParser(sourceUnit);
            MSAst.LambdaExpression ast;

            switch (sourceUnit.Kind) {
                case SourceCodeKind.InteractiveCode:
                    sourceUnit.CodeProperties = ScriptCodeParseResult.Complete;
                    ast = ToyGenerator.Generate(this, tp.ParseInteractiveStatement(), sourceUnit);
                    break;

                default:
                    sourceUnit.CodeProperties = ScriptCodeParseResult.Complete;
                    ast = ToyGenerator.Generate(this, tp.ParseFile(), sourceUnit);
                    break;
            }

            ast = new System.Scripting.Generation.GlobalLookupRewriter().RewriteLambda(ast);

            return new ScriptCode(ast, sourceUnit);
        }

        public override string DisplayName {
            get { return "ToyScript"; }
        }
    }
}
