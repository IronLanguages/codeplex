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

using System.Diagnostics;
using System.Reflection.Emit;

namespace Microsoft.Scripting.Ast {

    public partial class Compiler {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void EmitBranchTrue(Expression node, Label label) {
            switch (node.NodeType) {
                case AstNodeType.AndAlso:
                    EmitBranchTrueAndAlso((BinaryExpression)node, label);
                    break;
                case AstNodeType.OrElse:
                    EmitBranchTrueOrElse((BinaryExpression)node, label);
                    break;
                case AstNodeType.CommaExpression:
                    EmitBranchTrue((CommaExpression)node, label);
                    break;
                case AstNodeType.Equal:
                    EmitBranchTrueEqual((BinaryExpression)node, label);
                    break;
                case AstNodeType.NotEqual:
                    EmitBranchTrueNotEqual((BinaryExpression)node, label);
                    break;
                default:
                    EmitExpressionBranchTrue(node, label);
                    break;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void EmitBranchFalse(Expression node, Label label) {
            switch (node.NodeType) {
                case AstNodeType.AndAlso:
                    EmitBranchFalseAndAlso((BinaryExpression)node, label);
                    break;
                case AstNodeType.OrElse:
                    EmitBranchFalseOrElse((BinaryExpression)node, label);
                    break;
                case AstNodeType.CommaExpression:
                    EmitBranchFalse((CommaExpression)node, label);
                    break;
                case AstNodeType.Equal:
                    EmitBranchTrueNotEqual((BinaryExpression)node, label);
                    break;
                case AstNodeType.NotEqual:
                    EmitBranchTrueEqual((BinaryExpression)node, label);
                    break;
                default:
                    EmitExpressionBranchFalse(node, label);
                    break;
            }
        }

        private void EmitBranchTrueEqual(BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == AstNodeType.Equal || node.NodeType == AstNodeType.NotEqual);

            if (node.Method != null) {
                EmitExpression(node.Left);
                EmitExpression(node.Right);
                _cg.EmitCall(node.Method);
                _cg.Emit(OpCodes.Brtrue, label);
            } else if (node.Left.IsConstant(null)) {
                if (TypeUtils.IsNullableType(node.Right.Type)) {
                    EmitNullableHasValue(node.Right);
                } else {
                    Debug.Assert(!node.Right.Type.IsValueType);
                    EmitExpression(node.Right);
                }
                _cg.Emit(OpCodes.Brfalse, label);
            } else if (node.Right.IsConstant(null)) {
                if (TypeUtils.IsNullableType(node.Left.Type)) {
                    EmitNullableHasValue(node.Left);
                } else {
                    Debug.Assert(!node.Left.Type.IsValueType);
                    EmitExpression(node.Left);
                }
                _cg.Emit(OpCodes.Brfalse, label);
            } else {
                EmitExpression(node.Left);
                EmitExpression(node.Right);
                _cg.Emit(OpCodes.Beq, label);
            }
        }

        private void EmitBranchTrueNotEqual(BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == AstNodeType.Equal || node.NodeType == AstNodeType.NotEqual);

            if (node.Method != null) {
                EmitExpression(node.Left);
                EmitExpression(node.Right);
                _cg.EmitCall(node.Method);
                _cg.Emit(OpCodes.Brfalse, label);
            } else if (node.Left.IsConstant(null)) {
                if (TypeUtils.IsNullableType(node.Right.Type)) {
                    EmitNullableHasValue(node.Right);
                } else {
                    Debug.Assert(!node.Right.Type.IsValueType);
                    EmitExpression(node.Right);
                }
                _cg.Emit(OpCodes.Brtrue, label);
            } else if (node.Right.IsConstant(null)) {
                if (TypeUtils.IsNullableType(node.Left.Type)) {
                    EmitNullableHasValue(node.Left);
                } else {
                    Debug.Assert(!node.Left.Type.IsValueType);
                    EmitExpression(node.Left);
                }
                _cg.Emit(OpCodes.Brtrue, label);
            } else {
                EmitExpression(node.Left);
                EmitExpression(node.Right);
                _cg.Emit(OpCodes.Ceq);
                _cg.Emit(OpCodes.Brfalse, label);
            }
        }

        private void EmitBranchTrueAndAlso(BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == AstNodeType.AndAlso);

            // if (left AND right) branch label

            // if (left) then 
            //   if (right) branch label
            // endif
            Label endif = _cg.DefineLabel();
            EmitBranchFalse(node.Left, endif);
            EmitBranchTrue(node.Right, label);
            _cg.MarkLabel(endif);
        }

        private void EmitBranchTrueOrElse(BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == AstNodeType.OrElse);
            // if (left OR right) branch label

            // if (left) then branch label endif
            // if (right) then branch label endif
            EmitBranchTrue(node.Left, label);
            EmitBranchTrue(node.Right, label);
        }

        private void EmitBranchFalseAndAlso(BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == AstNodeType.AndAlso);
            // if NOT (left AND right) branch label

            if (node.Left.IsConstant(false)) {
                _cg.Emit(OpCodes.Br, label);
            } else {
                if (!node.Left.IsConstant(true)) {
                    EmitBranchFalse(node.Left, label);
                }

                if (node.Right.IsConstant(false)) {
                    _cg.Emit(OpCodes.Br, label);
                } else if (!node.Right.IsConstant(true)) {
                    EmitBranchFalse(node.Right, label);
                }
            }
        }

        private void EmitBranchFalseOrElse(BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == AstNodeType.OrElse);
            // if NOT left AND NOT right branch label

            if (!node.Left.IsConstant(true) && !node.Right.IsConstant(true)) {
                if (node.Left.IsConstant(false)) {
                    EmitBranchFalse(node.Right, label);
                } else if (node.Right.IsConstant(false)) {
                    EmitBranchFalse(node.Left, label);
                } else {
                    // if (NOT left) then 
                    //   if (NOT right) branch label
                    // endif

                    Label endif = _cg.DefineLabel();
                    EmitBranchTrue(node.Left, endif);
                    EmitBranchFalse(node.Right, label);
                    _cg.MarkLabel(endif);
                }
            }
        }

        private void EmitBranchTrue(CommaExpression node, Label label) {
            if (node.ValueIndex == node.Expressions.Count - 1) {
                EmitCommaPrefix(node, node.ValueIndex);
                EmitBranchTrue(node.Expressions[node.ValueIndex], label);
            } else {
                // Default behavior
                EmitExpressionBranchTrue(node, label);
            }
        }

        private void EmitBranchFalse(CommaExpression node, Label label) {
            if (node.ValueIndex== node.Expressions.Count - 1) {
                EmitCommaPrefix(node, node.ValueIndex);
                EmitBranchFalse(node.Expressions[node.ValueIndex], label);
            } else {
                // Default behavior
                EmitExpressionBranchFalse(node, label);
            }
        }

        /// <summary>
        /// This is the dual of EmitBranchFalse.
        /// </summary>
        private void EmitExpressionBranchTrue(Expression node, Label label) {
            Debug.Assert(node.Type == typeof(bool));
            EmitExpression(node);
            _cg.Emit(OpCodes.Brtrue, label);
        }

        /// <summary>
        /// Generates this expression as a bool and then branches to label
        /// if the resulting bool is false.
        /// </summary>
        private void EmitExpressionBranchFalse(Expression node, Label label) {
            Debug.Assert(node.Type == typeof(bool));
            EmitExpression(node);
            _cg.Emit(OpCodes.Brfalse, label);
        }

        private void EmitNullableHasValue(Expression node) {
            Debug.Assert(TypeUtils.IsNullableType(node.Type));
            EmitAddress(node, node.Type);
            _cg.EmitPropertyGet(node.Type, "HasValue");
        }
    }
}
