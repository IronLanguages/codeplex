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
    // Represents default(T) in the tree
    // TODO: rename to DefaultExpression
    public sealed class EmptyExpression : Expression {
        internal static readonly EmptyExpression VoidInstance = new EmptyExpression(typeof(void), Annotations.Empty);

        private readonly Type _type;

        internal EmptyExpression(Type type, Annotations annotations)
            : base(annotations) {
            _type = type;
        }

        protected override Type GetExpressionType() {
            return _type;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Default;
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitEmpty(this);
        }
    }

    public partial class Expression {
        public static EmptyExpression Empty() {
            return EmptyExpression.VoidInstance;
        }

        public static EmptyExpression Empty(Annotations annotations) {
            return new EmptyExpression(typeof(void), annotations);
        }

        public static EmptyExpression Empty(Type type) {
            if (type == typeof(void)) {
                return Empty();
            }
            return new EmptyExpression(type, null);
        }
    }
}
