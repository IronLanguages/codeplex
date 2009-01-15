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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    partial class LambdaCompiler {

        private void EmitAddress(Expression node, Type type) {
            Debug.Assert(node != null);

            switch (node.NodeType) {
                default:
                    EmitExpressionAddress(node, type);
                    break;

                // TODO: remove
                case AstNodeType.Conditional:
                    AddresOf((ConditionalExpression)node, type);
                    break;

                // TODO: remove
                case AstNodeType.Assign:
                    AddresOf((AssignmentExpression)node);
                    break;

                case AstNodeType.GlobalVariable:
                case AstNodeType.LocalVariable:
                case AstNodeType.Parameter:
                case AstNodeType.TemporaryVariable:
                    AddressOfVariable(node, type);
                    break;

                // TODO: remove
                case AstNodeType.Block:
                    AddresOf((Block)node, type);
                    break;

                case AstNodeType.MemberExpression:
                    AddresOf((MemberExpression)node, type);
                    break;

                // TODO: remove
                case AstNodeType.Convert:
                    AddresOf((UnaryExpression)node, type);
                    break;
            }
        }

        private void AddresOf(AssignmentExpression node) {
            CodeContract.Requires(node.Expression is VariableExpression || node.Expression is ParameterExpression, "node");

            EmitExpression(node.Value);
            Slot slot = _info.GetVariableSlot(node.Expression);
            slot.EmitSet(_ilg);
            slot.EmitGetAddr(_ilg);
        }

        private void AddressOfVariable(Expression node, Type type) {
            Debug.Assert(node is VariableExpression || node is ParameterExpression);

            if (type == node.Type) {
                _info.GetVariableSlot(node).EmitGetAddr(_ilg);
            } else {
                EmitExpressionAddress(node, type);
            }
        }

        private void AddresOf(ConditionalExpression node, Type type) {
            Label eoi = _ilg.DefineLabel();
            Label next = _ilg.DefineLabel();
            EmitExpression(node.Test);
            _ilg.Emit(OpCodes.Brfalse, next);
            EmitAddress(node.IfTrue, type);
            _ilg.Emit(OpCodes.Br, eoi);
            _ilg.MarkLabel(next);
            EmitAddress(node.IfFalse, type);
            _ilg.MarkLabel(eoi);
        }

        private void AddresOf(Block node, Type type) {
            ReadOnlyCollection<Expression> expressions = node.Expressions;

            // TODO: Do something better
            if (node.Type == typeof(void)) {
                throw new NotSupportedException("Cannot emit address of void-typed block");
            }

            for (int index = 0; index < expressions.Count; index++) {
                Expression current = expressions[index];

                // Emit the expression
                if (index == expressions.Count - 1) {
                    EmitAddress(current, type);
                } else {
                    EmitExpression(current);
                    // If we don't want the expression just emitted as the result,
                    // pop it off of the stack, unless it is a void expression.
                    if (current.Type != typeof(void)) {
                        _ilg.Emit(OpCodes.Pop);
                    }
                }
            }
        }

        private void AddresOf(MemberExpression node, Type type) {
            if (type != node.Type || node.Member.MemberType != MemberTypes.Field) {
                EmitExpressionAddress(node, type);
            } else {
                EmitInstance(node.Expression, node.Member.DeclaringType);
                _ilg.EmitFieldAddress((FieldInfo)node.Member);
            }
        }

        private void AddresOf(UnaryExpression node, Type type) {
            Debug.Assert(node.NodeType == AstNodeType.Convert);

            if (node.Operand.Type == typeof(object) && type.IsValueType) {
                EmitExpression(node.Operand);
                _ilg.Emit(OpCodes.Unbox, type);
            } else {
                EmitExpressionAddress(node, type);
            }
        }

        private void EmitExpressionAddress(Expression node, Type type) {
            Debug.Assert(TypeUtils.CanAssign(type, node.Type));

            EmitExpression(node);
            Slot tmp = _ilg.GetLocalTmp(type);
            tmp.EmitSet(_ilg);
            tmp.EmitGetAddr(_ilg);
        }
    }
}
