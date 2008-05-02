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


using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using MSAst = Microsoft.Scripting.Ast;

using ToyScript.Parser;
using ToyScript.Runtime;

namespace ToyScript {
    public class ToyLanguageContext : LanguageContext {
        public ToyLanguageContext(ScriptDomainManager manager) : base(manager) { 
            Binder = new ToyBinder(new CodeContext(new Scope(), this));
        }

        public override MSAst.LambdaExpression ParseSourceCode(CompilerContext context) {
            ToyParser tp = new ToyParser(context.SourceUnit.GetCode());

            switch (context.SourceUnit.Kind) {
                case SourceCodeKind.InteractiveCode:
                    context.SourceUnit.CodeProperties = SourceCodeProperties.None;
                    return ToyGenerator.Generate(this, tp.ParseInteractiveStatement(), context.SourceUnit.Path);

                default:
                    context.SourceUnit.CodeProperties = SourceCodeProperties.None;
                    return ToyGenerator.Generate(this, tp.ParseFile(), context.SourceUnit.Path);
            }
        }

        public override string DisplayName {
            get { return "ToyScript"; }
        }
    }
}
