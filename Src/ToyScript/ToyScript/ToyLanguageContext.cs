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

using System;
using System.Collections.Generic;
using System.Text;

using ToyScript.Parser;
using ToyScript.Parser.Ast;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using MSAst = Microsoft.Scripting.Ast;

namespace ToyScript {
    public class ToyLanguageContext : LanguageContext {

        private ScriptEngine _engine;

        public ToyLanguageContext() { 
        }

        public override ScriptEngine Engine {
            get {
                return _engine;
            }
        }

        internal ToyEngine ToyEngine {
            get {
                return (ToyEngine)_engine;
            }
            set {
                _engine = value;
            }
        }

        public override MSAst.CodeBlock ParseSourceCode(CompilerContext context) {
            ToyParser tp = new ToyParser(context.SourceUnit.GetCode());

            switch (context.SourceUnit.Kind) {
                case SourceCodeKind.InteractiveCode:
                    context.SourceUnit.CodeProperties = SourceCodeProperties.None;
                    return ToyGenerator.Generate(tp.ParseInteractiveStatement(), context.SourceUnit.Id);

                default:
                    context.SourceUnit.CodeProperties = SourceCodeProperties.None;
                    return ToyGenerator.Generate(tp.ParseFile(), context.SourceUnit.Id);
            }
        }
    }
}
