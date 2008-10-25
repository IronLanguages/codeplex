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
    // TODO: It seems overkill to have a full expression just for this.
    // Can we just reuse ConstantExpression?
    public sealed class EmptyExpression : Expression {
        internal static readonly EmptyExpression Instance = new EmptyExpression(Annotations.Empty);

        internal EmptyExpression(Annotations annotations)
            : base(annotations) {
        }

        protected override Type GetExpressionType() {
            return typeof(void);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.EmptyStatement;
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitEmpty(this);
        }
    }

    public partial class Expression {
        public static EmptyExpression Empty() {
            return EmptyExpression.Instance;
        }

        public static EmptyExpression Empty(Annotations annotations) {
            return new EmptyExpression(annotations);
        }
    }
}
