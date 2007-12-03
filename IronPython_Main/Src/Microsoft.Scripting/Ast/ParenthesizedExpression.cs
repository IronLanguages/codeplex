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

using System;
using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class ParenthesizedExpression : Expression {
        private readonly Expression /*!*/ _expression;

        internal ParenthesizedExpression(Expression /*!*/ expression)
            : base(AstNodeType.ParenthesizedExpression, expression.Type) {
            Debug.Assert(expression != null);
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static ParenthesizedExpression Parenthesize(Expression expression) {
            Contract.RequiresNotNull(expression, "expression");
            return new ParenthesizedExpression(expression);
        }
    }
}
