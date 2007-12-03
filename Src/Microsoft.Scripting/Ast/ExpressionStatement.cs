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

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class ExpressionStatement : Statement {
        private readonly Expression /*!*/ _expression;

        internal ExpressionStatement(SourceSpan span, Expression /*!*/ expression)
            : base(AstNodeType.ExpressionStatement, span) {
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }
    }

    public static partial class Ast {
        public static Statement Statement(Expression expression) {
            return Statement(SourceSpan.None, expression);
        }

        public static Statement Statement(SourceSpan span, Expression expression) {
            Contract.RequiresNotNull(expression, "expression");
            return new ExpressionStatement(span, expression);
        }
    }
}
