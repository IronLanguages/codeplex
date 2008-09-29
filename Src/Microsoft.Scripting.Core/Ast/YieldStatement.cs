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
using System; using Microsoft;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    public sealed class YieldStatement : Expression {
        private readonly Expression _expr;

        internal YieldStatement(Annotations annotations, Expression expression)
            : base(ExpressionType.YieldStatement, typeof(void), annotations) {
            _expr = expression;
        }

        public Expression Expression {
            get { return _expr; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitYield(this);
        }
    }

    /// <summary>
    /// Factory methods
    /// </summary>
    public partial class Expression {
        public static YieldStatement Yield(Expression expression) {
            return Yield(expression, Annotations.Empty);
        }
        public static YieldStatement Yield(Expression expression, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            return new YieldStatement(annotations, expression);
        }
    }
}
