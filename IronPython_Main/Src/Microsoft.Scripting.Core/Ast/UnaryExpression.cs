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
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class UnaryExpression : Expression {
        private readonly Expression /*!*/ _operand;

        internal UnaryExpression(AstNodeType nodeType, Expression/*!*/ expression, Type/*!*/ type)
            : this(nodeType, Annotations.Empty, expression, type, null) {
        }

        internal UnaryExpression(AstNodeType nodeType, Annotations annotations, Expression/*!*/ expression, Type type, DynamicAction bindingInfo)
            : base(annotations, nodeType, type, bindingInfo) {
            if (IsBound) {
                RequiresBound(expression, "expression");
            }
            _operand = expression;
        }

        public Expression Operand {
            get { return _operand; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        public static UnaryExpression Convert(Expression expression, Type type) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(type.IsVisible, "type", ResourceUtils.GetString(ResourceUtils.TypeMustBeVisible, type.FullName));

            return new UnaryExpression(AstNodeType.Convert, expression, type);
        }

        /// <summary>
        /// A dynamic or unbound conversion
        /// </summary>
        /// <param name="expression">the expression to convert</param>
        /// <param name="type">the type that the conversion returns, or null for an unbound node</param>
        /// <param name="bindingInfo">convert binding information</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static UnaryExpression Convert(Expression expression, Type type, ConvertToAction bindingInfo) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");

            return new UnaryExpression(AstNodeType.Convert, Annotations.Empty, expression, type, bindingInfo);
        }

        public static Expression ConvertHelper(Expression expression, Type type) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");

            if (expression.Type != type) {
                expression = Convert(expression, type);
            }
            return expression;
        }

        public static Expression Void(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");
            return ConvertHelper(expression, typeof(void));
        }

        public static UnaryExpression Negate(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.Requires(TypeUtils.IsArithmetic(expression.Type) && !TypeUtils.IsUnsigned(expression.Type), "expression", "Expression must be signed numeric type");

            return new UnaryExpression(AstNodeType.Negate, expression, expression.Type);
        }

        public static UnaryExpression Not(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.Requires(TypeUtils.IsIntegerOrBool(expression.Type), "expression", "Expression type must be integer or boolean.");

            return new UnaryExpression(AstNodeType.Not, expression, expression.Type);
        }

        public static UnaryExpression Not(Expression expression, Type result, DoOperationAction bindingInfo) {
            return Not(Annotations.Empty, expression, result, bindingInfo);
        }

        public static UnaryExpression Negate(Annotations annotations, Expression expression, Type result, DoOperationAction bindingInfo) {
            ContractUtils.RequiresNotNull(annotations, "annotations");
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            ContractUtils.Requires(bindingInfo.Operation == Operators.Negate, "bindingInfo", "operation kind must match node type");

            return new UnaryExpression(AstNodeType.Negate, annotations, expression, result, bindingInfo);
        }

        public static UnaryExpression Not(Annotations annotations, Expression expression, Type result, DoOperationAction bindingInfo) {
            ContractUtils.RequiresNotNull(annotations, "annotations");
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            ContractUtils.Requires(bindingInfo.Operation == Operators.Not, "bindingInfo", "operation kind must match node type");

            return new UnaryExpression(AstNodeType.Not, annotations, expression, result, bindingInfo);
        }

        public static UnaryExpression OnesComplement(Annotations annotations, Expression expression, Type result, DoOperationAction bindingInfo) {
            ContractUtils.RequiresNotNull(annotations, "annotations");
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            ContractUtils.Requires(bindingInfo.Operation == Operators.OnesComplement, "bindingInfo", "operation kind must match node type");

            return new UnaryExpression(AstNodeType.OnesComplement, annotations, expression, result, bindingInfo);
        }
    }
}
