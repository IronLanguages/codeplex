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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class Compiler {

        private void EmitAddress(Expression node, Type type) {
            Debug.Assert(node != null);

            switch (node.NodeType) {
                default:
                    EmitExpressionAddress(node, type);
                    break;

                case AstNodeType.Conditional:
                    AddresOf((ConditionalExpression)node, type);
                    break;

                // TODO: Remove !!!
                case AstNodeType.Convert:
                    AddresOf((UnaryExpression)node, type);
                    break;

                case AstNodeType.BoundAssignment:
                    AddresOf((BoundAssignment)node);
                    break;

                case AstNodeType.BoundExpression:
                    AddresOf((BoundExpression)node, type);
                    break;

                case AstNodeType.CommaExpression:
                    AddresOf((CommaExpression)node, type);
                    break;

                case AstNodeType.MemberExpression:
                    AddresOf((MemberExpression)node, type);
                    break;
            }
        }

        private void AddresOf(BoundAssignment node) {
            EmitExpression(node.Value);
            node.Ref.Slot.EmitSet(_cg);
            node.Ref.Slot.EmitGetAddr(_cg);
        }

        private void AddresOf(BoundExpression node, Type type) {
            if (type == node.Type) {
                node.Ref.Slot.EmitGetAddr(_cg);
            } else {
                EmitExpressionAddress(node, type);
            }
        }

        private void AddresOf(ConditionalExpression node, Type type) {
            Label eoi = _cg.DefineLabel();
            Label next = _cg.DefineLabel();
            EmitExpression(node.Test);
            _cg.Emit(OpCodes.Brfalse, next);
            EmitAddress(node.IfTrue, type);
            _cg.Emit(OpCodes.Br, eoi);
            _cg.MarkLabel(next);
            EmitAddress(node.IfFalse, type);
            _cg.MarkLabel(eoi);
        }

        private void AddresOf(CommaExpression node, Type type) {
            ReadOnlyCollection<Expression> expressions = node.Expressions;

            for (int index = 0; index < expressions.Count; index++) {
                Expression current = expressions[index];

                // Emit the expression
                if (index == node.ValueIndex) {
                    EmitAddress(current, type);
                } else {
                    EmitExpression(current);
                    // If we don't want the expression just emitted as the result,
                    // pop it off of the stack, unless it is a void expression.
                    if (current.Type != typeof(void)) {
                        _cg.Emit(OpCodes.Pop);
                    }
                }
            }
        }

        private void AddresOf(MemberExpression node, Type type) {
            if (type != node.Type || node.Member.MemberType != MemberTypes.Field) {
                EmitExpressionAddress(node, type);
            } else {
                EmitInstance(node.Expression, node.Member.DeclaringType);
                _cg.EmitFieldAddress((FieldInfo)node.Member);
            }
        }

        private void AddresOf(UnaryExpression node, Type type) {
            Debug.Assert(node.NodeType == AstNodeType.Convert);
            if (node.Type == type) {
                EmitAddress(node.Operand, type);
            } else {
                EmitExpressionAddress(node, type);
            }
        }

        private void EmitExpressionAddress(Expression node, Type type) {
            // TODO: Enable !!!
            //Debug.Assert(TypeUtils.CanAssign(type, node.Type));

            EmitAs(node, type);
            Slot tmp = _cg.GetLocalTmp(type);
            tmp.EmitSet(_cg);
            tmp.EmitGetAddr(_cg);
        }
    }
}
