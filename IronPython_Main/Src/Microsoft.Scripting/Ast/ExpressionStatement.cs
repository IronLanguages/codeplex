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
    // TODO: Rename or remove?
    // Essentially only useful to add spans to an expression.
    public sealed class ExpressionStatement : Expression, ISpan {
        private readonly Expression /*!*/ _expression;
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        internal ExpressionStatement(SourceLocation start, SourceLocation end, Expression /*!*/ expression)
            : base(AstNodeType.ExpressionStatement, expression.Type) {
            _start = start;
            _end = end;
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }
    }

    public static partial class Ast {
        public static Expression Statement(SourceSpan span, Expression expression) {
            Contract.RequiresNotNull(expression, "expression");
            return new ExpressionStatement(span.Start, span.End, expression);
        }
    }
}
