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
using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    internal static class ConstantCheck {

        /// <summary>
        /// Tests to see if the expression is a constant with the given value.
        /// </summary>
        /// <param name="e">The expression to examine</param>
        /// <param name="value">The constant value to check for.</param>
        /// <returns>true/false</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal static bool IsConstant(Expression e, bool value) {
            switch (e.NodeType) {
                case ExpressionType.AndAlso:
                    return CheckAndAlso((BinaryExpression)e, value);

                case ExpressionType.OrElse:
                    return CheckOrElse((BinaryExpression)e, value);

                case ExpressionType.Constant:
                    return value.Equals(((ConstantExpression)e).Value);

                case ExpressionType.TypeIs:
                    return IsInstanceOf((TypeBinaryExpression)e) == value;
            }
            return false;
        }

        internal static bool IsNull(Expression e) {
            switch (e.NodeType) {
                case ExpressionType.Constant:
                    return ((ConstantExpression)e).Value == null;

                case ExpressionType.TypeAs:
                    var typeAs = (UnaryExpression)e;
                    // if the TypeAs check is guarenteed to fail, then its result will be null
                    return (IsInstanceOf(typeAs) == false);
            }
            return false;
        }


        private static bool CheckAndAlso(BinaryExpression node, bool value) {
            Debug.Assert(node.NodeType == ExpressionType.AndAlso);

            if (node.Method != null || node.IsLifted) {
                return false;
            }
    
            if (value) {
                return IsConstant(node.Left, true) && IsConstant(node.Right, true);
            } else {
                // if left isn't a constant it has to be evaluated
                return IsConstant(node.Left, false);
            }
        }

        private static bool CheckOrElse(BinaryExpression node, bool value) {
            Debug.Assert(node.NodeType == ExpressionType.OrElse);

            if (node.Method != null || node.IsLifted) {
                return false;
            }

            if (value) {
                return IsConstant(node.Left, true);
            } else {
                return IsConstant(node.Left, false) && IsConstant(node.Right, false);
            }
        }

        /// <summary>
        /// If the result of a TypeBinaryExpression is known statically, this
        /// returns the result, otherwise it returns null, meaning we'll need
        /// to perform the IsInst instruction at runtime.
        /// 
        /// The result of this function must be equivalent to IsInst, or
        /// null.
        /// </summary>
        internal static bool? IsInstanceOf(TypeBinaryExpression typeIs) {
            return IsInstanceOf(typeIs.Expression.Type, typeIs.TypeOperand);
        }

        /// <summary>
        /// If the result of a unary TypeAs expression is known statically, this
        /// returns the result, otherwise it returns null, meaning we'll need
        /// to perform the IsInst instruction at runtime.
        /// 
        /// The result of this function must be equivalent to IsInst, or
        /// null.
        /// </summary>
        internal static bool? IsInstanceOf(UnaryExpression typeAs) {
            Debug.Assert(typeAs.NodeType == ExpressionType.TypeAs);
            return IsInstanceOf(typeAs.Operand.Type, typeAs.Type);
        }

        /// <summary>
        /// If the result of an isinst opcode is known statically, this
        /// returns the result, otherwise it returns null, meaning we'll need
        /// to perform the IsInst instruction at runtime.
        /// 
        /// The result of this function must be equivalent to IsInst, or
        /// null.
        /// </summary>
        internal static bool? IsInstanceOf(Type objectType, Type testType) {
            // Extensive testing showed that Type.IsAssignableFrom,
            // Type.IsInstanceOf, and the isinst instruction were all
            // equivalent when used against a live object

            // Oddly, we allow void operands
            // TODO: this is the LinqV1 behavior of TypeIs, seems bad
            if (objectType == typeof(void)) {
                return false;
            }

            // If we can statically assign, isinst will always succeed
            if (testType.IsAssignableFrom(objectType)) {
                return true;
            }

            // If we couldn't statically assign and the type is sealed, no
            // value at runtime can make isinst succeed
            if (objectType.IsSealed) {
                return false;
            }

            // Otherwise we need a runtime check
            return null;
        }

    }
}
