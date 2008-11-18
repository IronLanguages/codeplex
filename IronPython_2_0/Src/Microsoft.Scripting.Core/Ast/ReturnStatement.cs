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
namespace Microsoft.Linq.Expressions {
    public sealed class ReturnStatement : Expression {
        private readonly Expression _expr;

        internal ReturnStatement(Annotations annotations, Expression expression)
            : base(ExpressionType.ReturnStatement, typeof(void), annotations) {
            _expr = expression;
        }

        public Expression Expression {
            get { return _expr; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitReturn(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        [Obsolete("Use Expression.Return(LabelTarget) instead")]
        public static ReturnStatement Return() {
            return Return((Expression)null, (Annotations)null);
        }

        [Obsolete("Use Expression.Return(LabelTarget, Expression) instead")]
        public static ReturnStatement Return(Expression expression) {
            return Return(expression, (Annotations)null);
        }

        [Obsolete("Use Expression.Return(LabelTarget, Expression, Annotations) instead")]
        public static ReturnStatement Return(Expression expression, Annotations annotations) {
            if (expression != null) {
                RequiresCanRead(expression, "expression");
            }
            return new ReturnStatement(annotations, expression);
        }
    }
}