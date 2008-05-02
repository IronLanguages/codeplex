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

    partial class LambdaCompiler {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void EmitBranchTrue(Expression node, Label label) {
            if (node.IsDynamic) {
                EmitExpressionBranchTrue(node, label);
                return;
            }

            switch (node.NodeType) {
                case AstNodeType.AndAlso:
                    EmitBranchTrueAndAlso((BinaryExpression)node, label);
                    break;
                case AstNodeType.OrElse:
                    EmitBranchTrueOrElse((BinaryExpression)node, label);
                    break;
                case AstNodeType.Block:
                    EmitBranchTrue((Block)node, label);
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
        internal void EmitBranchFalse(Expression node, Label label) {
            if (node.IsDynamic) {
                EmitExpressionBranchFalse(node, label);
                return;
            }

            switch (node.NodeType) {
                case AstNodeType.AndAlso:
                    EmitBranchFalseAndAlso((BinaryExpression)node, label);
                    break;
                case AstNodeType.OrElse:
                    EmitBranchFalseOrElse((BinaryExpression)node, label);
                    break;
                case AstNodeType.Block:
                    EmitBranchFalse((Block)node, label);
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
                _ilg.EmitCall(node.Method);
                _ilg.Emit(OpCodes.Brtrue, label);
            } else if (ConstantCheck.IsConstant(node.Left, null)) {
                if (TypeUtils.IsNullableType(node.Right.Type)) {
                    EmitNullableHasValue(node.Right);
                } else {
                    Debug.Assert(!node.Right.Type.IsValueType);
                    EmitExpression(node.Right);
                }
                _ilg.Emit(OpCodes.Brfalse, label);
            } else if (ConstantCheck.IsConstant(node.Right, null)) {
                if (TypeUtils.IsNullableType(node.Left.Type)) {
                    EmitNullableHasValue(node.Left);
                } else {
                    Debug.Assert(!node.Left.Type.IsValueType);
                    EmitExpression(node.Left);
                }
                _ilg.Emit(OpCodes.Brfalse, label);
            } else {
                EmitExpression(node.Left);
                EmitExpression(node.Right);
                _ilg.Emit(OpCodes.Beq, label);
            }
        }

        private void EmitBranchTrueNotEqual(BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == AstNodeType.Equal || node.NodeType == AstNodeType.NotEqual);

            if (node.Method != null) {
                EmitExpression(node.Left);
                EmitExpression(node.Right);
                _ilg.EmitCall(node.Method);
                _ilg.Emit(OpCodes.Brfalse, label);
            } else if (ConstantCheck.IsConstant(node.Left, null)) {
                if (TypeUtils.IsNullableType(node.Right.Type)) {
                    EmitNullableHasValue(node.Right);
                } else {
                    Debug.Assert(!node.Right.Type.IsValueType);
                    EmitExpression(node.Right);
                }
                _ilg.Emit(OpCodes.Brtrue, label);
            } else if (ConstantCheck.IsConstant(node.Right, null)) {
                if (TypeUtils.IsNullableType(node.Left.Type)) {
                    EmitNullableHasValue(node.Left);
                } else {
                    Debug.Assert(!node.Left.Type.IsValueType);
                    EmitExpression(node.Left);
                }
                _ilg.Emit(OpCodes.Brtrue, label);
            } else {
                EmitExpression(node.Left);
                EmitExpression(node.Right);
                _ilg.Emit(OpCodes.Ceq);
                _ilg.Emit(OpCodes.Brfalse, label);
            }
        }

        private void EmitBranchTrueAndAlso(BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == AstNodeType.AndAlso);

            // if (left AND right) branch label

            // if (left) then 
            //   if (right) branch label
            // endif
            Label endif = _ilg.DefineLabel();
            EmitBranchFalse(node.Left, endif);
            EmitBranchTrue(node.Right, label);
            _ilg.MarkLabel(endif);
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

            if (ConstantCheck.IsConstant(node.Left, false)) {
                _ilg.Emit(OpCodes.Br, label);
            } else {
                if (!ConstantCheck.IsConstant(node.Left, true)) {
                    EmitBranchFalse(node.Left, label);
                }

                if (ConstantCheck.IsConstant(node.Right, false)) {
                    _ilg.Emit(OpCodes.Br, label);
                } else if (!ConstantCheck.IsConstant(node.Right, true)) {
                    EmitBranchFalse(node.Right, label);
                }
            }
        }

        private void EmitBranchFalseOrElse(BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == AstNodeType.OrElse);
            // if NOT left AND NOT right branch label

            if (!ConstantCheck.IsConstant(node.Left, true) && !ConstantCheck.IsConstant(node.Right, true)) {
                if (ConstantCheck.IsConstant(node.Left, false)) {
                    EmitBranchFalse(node.Right, label);
                } else if (ConstantCheck.IsConstant(node.Right, false)) {
                    EmitBranchFalse(node.Left, label);
                } else {
                    // if (NOT left) then 
                    //   if (NOT right) branch label
                    // endif

                    Label endif = _ilg.DefineLabel();
                    EmitBranchTrue(node.Left, endif);
                    EmitBranchFalse(node.Right, label);
                    _ilg.MarkLabel(endif);
                }
            }
        }

        private void EmitBranchTrue(Block node, Label label) {
            EmitBlockPrefix(node, node.Expressions.Count - 1);
            EmitBranchTrue(node.Expressions[node.Expressions.Count - 1], label);
        }

        private void EmitBranchFalse(Block node, Label label) {
            EmitBlockPrefix(node, node.Expressions.Count - 1);
            EmitBranchFalse(node.Expressions[node.Expressions.Count - 1], label);
        }

        /// <summary>
        /// This is the dual of EmitBranchFalse.
        /// </summary>
        private void EmitExpressionBranchTrue(Expression node, Label label) {
            Debug.Assert(node.Type == typeof(bool));
            EmitExpression(node);
            _ilg.Emit(OpCodes.Brtrue, label);
        }

        /// <summary>
        /// Generates this expression as a bool and then branches to label
        /// if the resulting bool is false.
        /// </summary>
        private void EmitExpressionBranchFalse(Expression node, Label label) {
            Debug.Assert(node.Type == typeof(bool));
            EmitExpression(node);
            _ilg.Emit(OpCodes.Brfalse, label);
        }

        private void EmitNullableHasValue(Expression node) {
            Debug.Assert(TypeUtils.IsNullableType(node.Type));
            EmitAddress(node, node.Type);
            _ilg.EmitPropertyGet(node.Type, "HasValue");
        }
    }
}
