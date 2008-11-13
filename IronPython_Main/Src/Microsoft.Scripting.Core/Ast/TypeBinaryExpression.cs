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
using System.Collections.Generic;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public sealed class TypeBinaryExpression : Expression {
        private readonly Expression _expression;
        private readonly Type _typeOperand;
        private readonly ExpressionType _nodeKind;

        internal TypeBinaryExpression(Expression expression, Type typeOperand, ExpressionType nodeKind){
            _expression = expression;
            _typeOperand = typeOperand;
            _nodeKind = nodeKind;
        }

        protected override Type GetExpressionType() {
            return typeof(bool);
        }

        protected override ExpressionType GetNodeKind() {
            return _nodeKind;
        }

        public Expression Expression {
            get { return _expression; }
        }

        public Type TypeOperand {
            get { return _typeOperand; }
        }

        public override bool CanReduce {
            get {
                return _nodeKind == ExpressionType.TypeEqual;
            }
        }

        public override Expression Reduce() {
            if (CanReduce) {
                return ReduceTypeEqual();
            } else {
                return this;
            }
        }

        #region "Reduce TypeEqual"

        private Expression ReduceTypeEqual() {
            Type cType = Expression.Type;
            
            // value types (including Void). Can perform test right now.
            if (cType.IsValueType) {
                return EvalAndReturnConst(cType == _typeOperand);
            }

            // Null is special. True if expression produces null.
            if (_typeOperand == typeof(Null)) {
                return Expression.Equal(Expression, Expression.Constant(null));
            }

            // Can check the value right now for constants.
            if (Expression.NodeType == ExpressionType.Constant) {
                return ReduceConstantTypeEqual();
            }

            // if LHS type is sealed and of the same type it will match if value is not null
            if (cType.IsSealed && (cType == _typeOperand)) {
                return Expression.NotEqual(Expression, Expression.Constant(null));
            }

            // expression is a ByVal parameter. Can safely reevaluate.
            if (Expression.NodeType == ExpressionType.Parameter) {
                ParameterExpression pe = Expression as ParameterExpression;
                if (!pe.IsByRef) {
                    return ByValParameterTypeEqual(pe);
                }
            }
            
            // ==== general case
            // need a temp to avoid LHS reevaluation.
            ParameterExpression temp = Variable(
                typeof(Object),
                "TypeEqualLHS"
            );

            return Expression.Block(
                new ParameterExpression[] { temp },
                Expression.Assign(temp, Expression),
                ByValParameterTypeEqual(temp)
            );
        }

        // evaluates the expression and returns a const 
        // this is used where we already know the result.
        private Expression EvalAndReturnConst(bool value) {
            return Block(
                Expression,
                Expression.Constant(value)
            );
        }

        // helper that is used when re-eval of LHS is safe.
        private Expression ByValParameterTypeEqual(ParameterExpression value) {
            return Expression.AndAlso(
                Expression.NotEqual(value, Expression.Constant(null)),
                Expression.Equal(
                    Expression.Call(
                        value,
                        typeof(object).GetMethod("GetType")
                    ),
                    Expression.Constant(_typeOperand)
                ),
                null
            );
        }

        private Expression ReduceConstantTypeEqual() {
            ConstantExpression ce = Expression as ConstantExpression;
            if (ce.Value == null) {
                return Expression.Constant(
                    _typeOperand == typeof(Null)
                );
            } else {
                return Expression.Constant(
                    _typeOperand == ce.Value.GetType()
                );
            }
        }

        #endregion

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitTypeBinary(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        //CONFORMING
        public static TypeBinaryExpression TypeIs(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);

            return new TypeBinaryExpression(expression, type, ExpressionType.TypeIs);
        }

        public static TypeBinaryExpression TypeEqual(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);

            return new TypeBinaryExpression(expression, type, ExpressionType.TypeEqual);
        }

    }
}
