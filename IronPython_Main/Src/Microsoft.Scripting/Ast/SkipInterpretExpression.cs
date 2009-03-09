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
using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// We don't need to insert code to track lines in adaptive mode as the
    /// interpreter does that for us. TODO: improve the adaptive compiler so we
    /// don't need to do this, and can just remove line tracking from languages
    /// </summary>
    public sealed class SkipInterpretExpression : Expression {
        private readonly Expression _body;

        internal SkipInterpretExpression(Expression body) {
            if (body.Type != typeof(void)) {
                body = Expression.Block(typeof(void), body);
            }
            _body = body;
        }

        public Expression Body {
            get { return _body; }
        }

        protected override Type TypeImpl() {
            return typeof(void);
        }

        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Extension;
        }

        public override bool CanReduce {
            get { return true;  }
        }

        public override Expression Reduce() {
            return _body;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            Expression body = visitor.Visit(_body);
            if (body == _body) {
                return this;
            }
            return new SkipInterpretExpression(body);
        }
    }

    public static partial class Utils {
        public static SkipInterpretExpression SkipInterpret(Expression body) {
            var skip = body as SkipInterpretExpression;
            if (skip != null) {
                return skip;
            }
            return new SkipInterpretExpression(body);
        }
    }
}
