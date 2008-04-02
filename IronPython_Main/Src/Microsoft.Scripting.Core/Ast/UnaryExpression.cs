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

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public sealed class UnaryExpression : Expression {
        private readonly Expression /*!*/ _operand;

        internal UnaryExpression(AstNodeType nodeType, Expression /*!*/ expression, Type /*!*/ type)
            : base(nodeType, type) {
            _operand = expression;
        }

        public Expression Operand {
            get { return _operand; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static UnaryExpression Convert(Expression expression, Type type) {
            Contract.RequiresNotNull(expression, "expression");
            Contract.RequiresNotNull(type, "type");

            if (!type.IsVisible) {
                throw new ArgumentException(String.Format(Resources.TypeMustBeVisible, type.FullName));
            }

            return new UnaryExpression(AstNodeType.Convert, expression, type);
        }

        public static Expression ConvertHelper(Expression expression, Type type) {
            Contract.RequiresNotNull(expression, "expression");
            Contract.RequiresNotNull(type, "type");

            if (expression.Type != type) {
                expression = Convert(expression, type);
            }
            return expression;
        }

        public static Expression Void(Expression expression) {
            Contract.RequiresNotNull(expression, "expression");
            return ConvertHelper(expression, typeof(void));
        }

        public static UnaryExpression Negate(Expression expression) {
            Contract.RequiresNotNull(expression, "expression");
            Contract.Requires(TypeUtils.IsArithmetic(expression.Type) && !TypeUtils.IsUnsigned(expression.Type), "expression", "Expression must be signed numeric type");

            return new UnaryExpression(AstNodeType.Negate, expression, expression.Type);
        }

        public static UnaryExpression Not(Expression expression) {
            Contract.RequiresNotNull(expression, "expression");
            Contract.Requires(TypeUtils.IsIntegerOrBool(expression.Type), "expression", "Expression type must be integer or boolean.");

            return new UnaryExpression(AstNodeType.Not, expression, expression.Type);
        }
    }
}
