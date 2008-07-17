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

using System.Scripting;
using System.Scripting.Utils;
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class TypeBinaryExpression : Expression {
        private readonly Expression _expression;
        private readonly Type _typeOperand;

        internal TypeBinaryExpression(Annotations annotations, ExpressionType nodeType, Expression expression, Type typeOperand)
            : base(annotations, nodeType, typeof(bool)) {
            _expression = expression;
            _typeOperand = typeOperand;
        }

        public Expression Expression {
            get { return _expression; }
        }

        public Type TypeOperand {
            get { return _typeOperand; }
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");

            System.Diagnostics.Debug.Assert(this.NodeType == ExpressionType.TypeIs);
            builder.Append("(");
            _expression.BuildString(builder);
            builder.Append(" Is ");
            builder.Append(_typeOperand.Name);
            builder.Append(")");
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
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);

            return new TypeBinaryExpression(annotations, ExpressionType.TypeIs, expression, type);
        }
    }
}
