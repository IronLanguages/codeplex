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
using Microsoft.Scripting.Actions;
using MSAst = Microsoft.Scripting.Ast;

namespace ToyScript.Parser.Ast {
    using Ast = MSAst.Expression;

    class If : Statement {
        private readonly Expression _test;
        private readonly Statement _then;
        private readonly Statement _else;

        public If(SourceSpan span, Expression test, Statement then, Statement @else)
            : base(span) {
            _test = test;
            _then = then;
            _else = @else;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            return Ast.If(
                Ast.IfConditions(
                    Ast.IfCondition(
                        Span,
                        _test.End,
                        Ast.Action.ConvertTo(
                            tg.Binder,
                            typeof(bool),
                            ConversionResultKind.ExplicitCast,
                            _test.Generate(tg)
                        ),
                        _then.Generate(tg)
                    )
                ),
                _else != null ? _else.Generate(tg) : null
            );
        }
    }
}
