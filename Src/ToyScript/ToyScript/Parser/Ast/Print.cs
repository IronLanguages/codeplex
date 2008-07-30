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

using System.Scripting;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace ToyScript.Parser.Ast {
    using Ast = System.Linq.Expressions.Expression;

    class Print : Statement {
        private readonly Expression _expression;

        public Print(SourceSpan span, Expression expression)
            : base(span) {
            _expression = expression;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            return AstUtils.Call(
                typeof(ToyHelpers).GetMethod("Print"), 
                Span,
                AstUtils.CodeContext(), 
                Ast.ConvertHelper(
                    _expression.Generate(tg),
                    typeof(object)
                )
            );
        }
    }
}
