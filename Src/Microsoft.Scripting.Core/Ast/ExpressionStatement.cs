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
    // TODO: remove?
    public sealed class ExpressionStatement : Expression {
        private readonly Expression /*!*/ _expression;

        internal ExpressionStatement(Annotations annotations, Expression /*!*/ expression)
            : base(annotations, AstNodeType.ExpressionStatement, expression.Type) {
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }
    }

    public static partial class Ast {
        public static Expression Statement(SourceSpan span, Expression expression) {
            return Statement(Annotations(span), expression);
        }
        public static Expression Statement(Annotations annotations, Expression expression) {
            Contract.RequiresNotNull(expression, "expression");
            return new ExpressionStatement(annotations, expression);
        }
    }
}
