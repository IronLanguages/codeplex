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
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using System.Text;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public sealed class TypeBinaryExpression : Expression {
        private readonly Expression _expression;
        private readonly Type _typeOperand;

        internal TypeBinaryExpression(Expression expression, Type typeOperand, Annotations annotations)
            : base(annotations) {
            _expression = expression;
            _typeOperand = typeOperand;
        }

        protected override Type GetExpressionType() {
            return typeof(bool);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.TypeIs;
        }

        public Expression Expression {
            get { return _expression; }
        }

        public Type TypeOperand {
            get { return _typeOperand; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitTypeBinary(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        public static TypeBinaryExpression TypeIs(Expression expression, Type type) {
            return TypeIs(expression, type, Annotations.Empty);
        }

        //CONFORMING
        public static TypeBinaryExpression TypeIs(Expression expression, Type type, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);

            return new TypeBinaryExpression(expression, type, annotations);
        }
    }
}
