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
using System.Reflection.Emit;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {

    // The part of the LambdaCompiler dealing with low level control flow
    // break, contiue, return, exceptions, etc
    partial class LambdaCompiler {

        private LabelInfo EnsureLabel(LabelTarget node) {
            LabelInfo result;
            if (!_labelInfo.TryGetValue(node, out result)) {
                _labelInfo.Add(node, result = new LabelInfo(_ilg, node, false));
            }
            return result;
        }

        private LabelInfo ReferenceLabel(LabelTarget node) {
            LabelInfo result = EnsureLabel(node);
            result.Reference(_labelBlock);
            return result;
        }

        private LabelInfo DefineLabel(LabelTarget node) {
            if (node == null) {
                return new LabelInfo(_ilg, null, false);
            }
            LabelInfo result = EnsureLabel(node);
            result.Define(_ilg, _labelBlock);
            return result;
        }

        private void PushLabelBlock(LabelBlockKind type) {
            _labelBlock = new LabelBlockInfo(_labelBlock, type);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "kind")]
        private void PopLabelBlock(LabelBlockKind kind) {
            Debug.Assert(_labelBlock != null && _labelBlock.Kind == kind);
            _labelBlock = _labelBlock.Parent;
        }

        private void EmitLabelExpression(Expression expr) {
            var node = (LabelExpression)expr;
            Debug.Assert(node.Label != null);

            // If we're an immediate child of a block, our label will already
            // be defined. If not, we need to define our own block so this
            // label isn't exposed except to its own child expression.
            LabelInfo label;
            if (!_labelBlock.Labels.TryGetValue(node.Label, out label)) {
                label = DefineLabel(node.Label);
            }

            if (node.DefaultValue != null) {
                EmitExpression(node.DefaultValue);
            }

            label.Mark();
        }

        private void EmitGotoExpression(Expression expr) {
            var node = (GotoExpression)expr;
            if (node.Value != null) {
                EmitExpression(node.Value);
            }

            ReferenceLabel(node.Target).EmitJump();           
        }

        private void EmitReturn() {
            bool canReturn = true;
            for (LabelBlockInfo j = _labelBlock; j != null; j = j.Parent) {
                switch (j.Kind) {
                    case LabelBlockKind.Finally:
                        throw Error.ControlCannotLeaveFinally();
                    case LabelBlockKind.Filter:
                        throw Error.ControlCannotLeaveFilterTest();
                    case LabelBlockKind.Try:
                    case LabelBlockKind.Catch:
                        canReturn = false;
                        break;
                }
            }

            if (canReturn) {
                _ilg.Emit(OpCodes.Ret);
                return;
            }

            // We can't return directly, so store the return value into a local
            // and then jump to the end of the method
            EnsureReturnBlock();
            if (_returnBlock.Value != null) {
                _ilg.Emit(OpCodes.Stloc, _returnBlock.Value);
            }

            _ilg.Emit(OpCodes.Leave, _returnBlock.Label);
        }
                
        private void EmitReturn(Expression expr) {
            if (expr == null) {
                Debug.Assert(_method.GetReturnType() == typeof(void));
            } else {
                Type result = _method.GetReturnType();
                Debug.Assert(result.IsAssignableFrom(expr.Type));
                EmitExpression(expr);
                if (!TypeUtils.AreReferenceAssignable(result, expr.Type)) {
                    _ilg.EmitConvertToType(expr.Type, result, false/*unchecked*/);
                }
            }
            EmitReturn();
        }

        private void EnsureReturnBlock() {
            if (_returnBlock == null) {
                _returnBlock = new LabelInfo(_ilg, new LabelTarget(_method.GetReturnType(), null), false);
            }
        }

        private bool TryPushLabelBlock(Expression node) {
            // Anything that is "statement-like" -- e.g. has no associated
            // stack state can be jumped into, with the exception of try-blocks
            // We indicate this by a "Block"
            // 
            // Otherwise, we push an "Expression" to indicate that it can't be
            // jumped into
            switch (node.NodeType) {
                default:
                    if (_labelBlock.Kind != LabelBlockKind.Expression) {
                        PushLabelBlock(LabelBlockKind.Expression);
                        return true;
                    }
                    return false;
                case ExpressionType.Label:
                    // LabelExpression is a bit special, if it's directly in a block
                    // it becomes associate with the block's scope
                    if (_labelBlock.Kind != LabelBlockKind.Block ||
                        !_labelBlock.Labels.ContainsKey(((LabelExpression)node).Label)) {
                        PushLabelBlock(LabelBlockKind.Block);
                        return true;
                    }
                    return false;
                case ExpressionType.Assign:
                    // Assignment where left side is a variable/parameter is
                    // safe to jump into
                    var assign = (AssignmentExpression)node;
                    if (assign.Expression.NodeType == ExpressionType.Parameter) {
                        PushLabelBlock(LabelBlockKind.Block);
                        return true;
                    }
                    return false;
                case ExpressionType.DebugInfo:
                case ExpressionType.Conditional:
                case ExpressionType.Block:
                case ExpressionType.Switch:
                case ExpressionType.Loop:
                case ExpressionType.Goto:
                case ExpressionType.ReturnStatement:
                    PushLabelBlock(LabelBlockKind.Block);
                    return true;
            }
        }

        // See if this lambda has a return label
        // If so, we'll create it now and mark it as allowing the "ret" opcode
        // This allows us to generate better IL
        private void AddReturnLabel(Expression lambdaBody) {
            while (true) {
                switch (lambdaBody.NodeType) {
                    default:
                        // Didn't find return label
                        return;
                    case ExpressionType.Label:
                        // Found it!
                        var label = ((LabelExpression)lambdaBody).Label;
                        _labelInfo.Add(label, new LabelInfo(_ilg, label, true));
                        return;
                    case ExpressionType.DebugInfo:
                        // Look in the body
                        lambdaBody = ((DebugInfoExpression)lambdaBody).Expression;
                        continue;
                    case ExpressionType.Block:
                        // Look in the last expression of a block
                        var exprs = ((BlockExpression)lambdaBody).Expressions;

                        // TODO: shouldn't allow creating empty blocks
                        if (exprs.Count == 0) {
                            return;
                        }

                        lambdaBody = exprs[exprs.Count - 1];
                        continue;
                }
            }
        }
    }
}
