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
    public sealed class EmptyStatement : Expression {
        internal static readonly EmptyStatement Instance = new EmptyStatement(Annotations.Empty);

        internal EmptyStatement(Annotations annotations)
            : base(ExpressionType.EmptyStatement, typeof(void), annotations) {
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitEmpty(this);
        }
    }

    public partial class Expression {
        public static EmptyStatement Empty() {
            return EmptyStatement.Instance;
        }

        public static EmptyStatement Empty(Annotations annotations) {
            return new EmptyStatement(annotations);
        }
    }
}