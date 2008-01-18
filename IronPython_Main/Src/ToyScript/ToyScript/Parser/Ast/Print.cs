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

    class Print : Statement {
        private readonly Expression _expression;

        public Print(SourceSpan span, Expression expression)
            : base(span) {
            _expression = expression;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            return Ast.Statement(
                Span,
                Ast.Call(
                    typeof(ToyHelpers).GetMethod("Print"),
                    Ast.ConvertHelper(
                        _expression.Generate(tg),
                        typeof(object)
                    )
                )
            );
        }
    }
}
