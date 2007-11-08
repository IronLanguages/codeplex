/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace ToyScript.Parser.Ast {
    using Ast = MSAst.Ast;

    class While : Statement {
        private readonly Expression _test;
        private readonly Statement _body;

        public While(SourceSpan span, Expression test, Statement body)
            : base(span) {
            _test = test;
            _body = body;
        }

        protected internal override MSAst.Statement Generate(ToyGenerator tg) {
            return Ast.While(
                Span,
                _test.End,
                Ast.DynamicConvert(
                    _test.Generate(tg),
                    typeof(bool)
                ),
                _body.Generate(tg),
                null
            );
        }
    }
}
