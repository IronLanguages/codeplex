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
using System.Reflection.Emit;
using System.Diagnostics;

namespace Microsoft.Linq.Expressions.Compiler {
    partial class LambdaCompiler {
        private void EmitBlock(Expression expr) {
            // emit body
            Emit((BlockExpression)expr, EmitAs.Default);
        }

        private void Emit(BlockExpression node, EmitAs emitAs) {
            var expressions = node.Expressions;
            // Labels defined immediately in the block are valid for the whole block
            foreach (var e in expressions) {
                var label = e as LabelExpression;
                if (label != null) {
                    DefineLabel(label.Label);
                }
            }

            EnterScope(node);

            for (int index = 0; index < expressions.Count - 1; index++) {
                EmitExpressionAsVoid(expressions[index]);
            }
            if (expressions.Count > 0) {
                // if the type of Block it means this is not a Comma
                // so we will force the last expression to emit as void.
                if (emitAs == EmitAs.Void || node.Type == typeof(void)) {
                    EmitExpressionAsVoid(expressions[expressions.Count - 1]);
                } else {
                    EmitExpression(expressions[expressions.Count - 1]);
                }
            }

            ExitScope(node);
        }

        private void EnterScope(BlockExpression node) {
            if (node.Variables.Count > 0 && !_scope.MergedScopes.Contains(node)) {
                _scope = _tree.Scopes[node].Enter(this, _scope);
                Debug.Assert(_scope.Node == node);
            }
        }

        private void ExitScope(BlockExpression node) {
            if (_scope.Node == node) {
                _scope = _scope.Exit();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "expr")]
        private static void EmitEmptyStatement(Expression expr) {
        }

        private void EmitLoopStatement(Expression expr) {
            LoopExpression node = (LoopExpression)expr;

            PushLabelBlock(LabelBlockKind.Block);
            LabelInfo breakTarget = DefineLabel(node.BreakLabel);
            LabelInfo continueTarget = DefineLabel(node.ContinueLabel);

            continueTarget.Mark();

            EmitExpressionAsVoid(node.Body);

            _ilg.Emit(OpCodes.Br, continueTarget.Label);

            PopLabelBlock(LabelBlockKind.Block);

            breakTarget.Mark();
        }

        private void EmitReturnStatement(Expression expr) {
            var node = (ReturnStatement)expr;

            if (node.Expression != null) {
                // TODO: should be TypeUtils.AreReferenceAssignable
                // (but ReturnStatement is going away so it doesn't need to be fixed)
                if (!_lambda.ReturnType.IsAssignableFrom(node.Expression.Type)) {
                    throw Error.InvalidReturnTypeOfLambda(node.Expression.Type, _lambda.ReturnType, _lambda.Name);
                }
            } else if (_lambda.ReturnType != typeof(void)) {
                // return without expression can be only from lambda with void return type
                throw Error.MissingReturnForLambda(_lambda.Name, _lambda.ReturnType);
            }

            EmitReturn(node.Expression);
        }

        #region SwitchStatement

        private void EmitSwitchStatement(Expression expr) {
            SwitchExpression node = (SwitchExpression)expr;

            LabelInfo breakTarget = DefineLabel(node.BreakLabel);

            Label defaultTarget = breakTarget.Label;
            Label[] labels = new Label[node.SwitchCases.Count];

            // Create all labels
            for (int i = 0; i < node.SwitchCases.Count; i++) {
                labels[i] = _ilg.DefineLabel();

                // Default case.
                if (node.SwitchCases[i].IsDefault) {
                    // Set the default target
                    defaultTarget = labels[i];
                }
            }

            // Emit the test value
            EmitExpression(node.Test);

            // Check if jmp table can be emitted
            if (!TryEmitJumpTable(node, labels, defaultTarget)) {
                // There might be scenario(s) where the jmp table is not emitted
                // Emit the switch as conditional branches then
                EmitConditionalBranches(node, labels);
            }

            // If "default" present, execute default code, else exit the switch            
            _ilg.Emit(OpCodes.Br, defaultTarget);

            // Emit the bodies
            for (int i = 0; i < node.SwitchCases.Count; i++) {
                // First put the corresponding labels
                _ilg.MarkLabel(labels[i]);
                // And then emit the Body!!
                EmitExpressionAsVoid(node.SwitchCases[i].Body);
            }

            breakTarget.Mark();
        }

        private const int MaxJumpTableSize = 65536;
        private const double MaxJumpTableSparsity = 10;

        // Emits the switch as if stmts
        private void EmitConditionalBranches(SwitchExpression node, Label[] labels) {
            LocalBuilder testValueSlot = _ilg.GetLocal(typeof(int));
            _ilg.Emit(OpCodes.Stloc, testValueSlot);

            // For all the "cases" create their conditional branches
            for (int i = 0; i < node.SwitchCases.Count; i++) {
                // Not default case emit the condition
                if (!node.SwitchCases[i].IsDefault) {
                    // Test for equality of case value and the test expression
                    _ilg.EmitInt(node.SwitchCases[i].Value);
                    _ilg.Emit(OpCodes.Ldloc, testValueSlot);
                    _ilg.Emit(OpCodes.Beq, labels[i]);
                }
            }

            _ilg.FreeLocal(testValueSlot);
        }

        // Tries to emit switch as a jmp table
        private bool TryEmitJumpTable(SwitchExpression node, Label[] labels, Label defaultTarget) {
            if (node.SwitchCases.Count > MaxJumpTableSize) {
                return false;
            }

            int min = Int32.MaxValue;
            int max = Int32.MinValue;

            // Find the min and max of the values
            for (int i = 0; i < node.SwitchCases.Count; ++i) {
                // Not the default case.
                if (!node.SwitchCases[i].IsDefault) {
                    int val = node.SwitchCases[i].Value;
                    if (min > val) min = val;
                    if (max < val) max = val;
                }
            }

            long delta = (long)max - (long)min;
            if (delta > MaxJumpTableSize) {
                return false;
            }

            // Value distribution is too sparse, don't emit jump table.
            if (delta > node.SwitchCases.Count + MaxJumpTableSparsity) {
                return false;
            }

            // The actual jmp table of switch
            int len = (int)delta + 1;
            Label[] jmpLabels = new Label[len];

            // Initialize all labels to the default
            for (int i = 0; i < len; i++) {
                jmpLabels[i] = defaultTarget;
            }

            // Replace with the actual label target for all cases
            for (int i = 0; i < node.SwitchCases.Count; i++) {
                SwitchCase sc = node.SwitchCases[i];
                if (!sc.IsDefault) {
                    jmpLabels[sc.Value - min] = labels[i];
                }
            }

            // Emit the normalized index and then switch based on that
            if (min != 0) {
                _ilg.EmitInt(min);
                _ilg.Emit(OpCodes.Sub);
            }
            _ilg.Emit(OpCodes.Switch, jmpLabels);
            return true;
        }

        #endregion

        private void CheckRethrow() {
            // Rethrow is only valid inside a catch.
            for (LabelBlockInfo j = _labelBlock; j != null; j = j.Parent) {
                if (j.Kind == LabelBlockKind.Catch) {
                    return;
                } else if (j.Kind == LabelBlockKind.Finally) {
                    // Rethrow from inside finally is not verifiable
                    break;
                }
            }
            throw Error.RethrowRequiresCatch();
        }

        #region TryStatement

        private void EmitSaveExceptionOrPop(CatchBlock cb) {
            if (cb.Variable != null) {
                // If the variable is present, store the exception
                // in the variable.
                _scope.EmitSet(cb.Variable);
            } else {
                // Otherwise, pop it off the stack.
                _ilg.Emit(OpCodes.Pop);
            }
        }

        private void EmitTryStatement(Expression expr) {
            var node = (TryExpression)expr;

            //******************************************************************
            // 1. ENTERING TRY
            //******************************************************************

            PushLabelBlock(LabelBlockKind.Try);
            _ilg.BeginExceptionBlock();

            //******************************************************************
            // 2. Emit the try statement body
            //******************************************************************

            EmitExpressionAsVoid(node.Body);

            //******************************************************************
            // 3. Emit the catch blocks
            //******************************************************************

            if (node.Handlers.Count > 0) {
                PushLabelBlock(LabelBlockKind.Catch);

                foreach (CatchBlock cb in node.Handlers) {
                    // Begin the strongly typed exception block
                    EmitCatchStart(cb);

                    //
                    // Emit the catch block body
                    //
                    EmitExpressionAsVoid(cb.Body);
                }

                PopLabelBlock(LabelBlockKind.Catch);
            }

            //******************************************************************
            // 4. Emit the finally block
            //******************************************************************

            if (node.Finally != null || node.Fault != null) {
                PushLabelBlock(LabelBlockKind.Finally);

                if (node.Finally != null) {
                    _ilg.BeginFinallyBlock();
                } else if (IsDynamicMethod) {
                    // dynamic methods don't support fault blocks so we
                    // generate a catch/rethrow.
                    _ilg.BeginCatchBlock(typeof(Exception));
                } else {
                    _ilg.BeginFaultBlock();
                }

                // Emit the body
                EmitExpressionAsVoid(node.Finally ?? node.Fault);

                // rethrow the exception if we have a catch in a dynamic method.
                if (node.Fault != null && IsDynamicMethod) {
                    // rethrow when we generated a catch
                    _ilg.Emit(OpCodes.Rethrow);
                }

                _ilg.EndExceptionBlock();
                PopLabelBlock(LabelBlockKind.Finally);
            } else {
                _ilg.EndExceptionBlock();
            }

            PopLabelBlock(LabelBlockKind.Try);
        }

        /// <summary>
        /// Emits the start of a catch block.  The exception value that is provided by the
        /// CLR is stored in the variable specified by the catch block or popped if no
        /// variable is provided.
        /// </summary>
        private void EmitCatchStart(CatchBlock cb) {
            if (cb.Filter != null && !IsDynamicMethod) {
                // emit filter block as filter.  Filter blocks are 
                // untyped so we need to do the type check ourselves.  
                _ilg.BeginExceptFilterBlock();

                Label endFilter = _ilg.DefineLabel();
                Label rightType = _ilg.DefineLabel();

                // skip if it's not our exception type, but save
                // the exception if it is so it's available to the
                // filter
                _ilg.Emit(OpCodes.Isinst, cb.Test);
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Brtrue, rightType);
                _ilg.Emit(OpCodes.Pop);
                _ilg.Emit(OpCodes.Ldc_I4_0);
                _ilg.Emit(OpCodes.Br, endFilter);

                // it's our type, save it and emit the filter.
                _ilg.MarkLabel(rightType);
                EmitSaveExceptionOrPop(cb);
                PushLabelBlock(LabelBlockKind.Filter);
                EmitExpression(cb.Filter);
                PopLabelBlock(LabelBlockKind.Filter);

                // begin the catch, clear the exception, we've 
                // already saved it
                _ilg.MarkLabel(endFilter);
                _ilg.BeginCatchBlock(null);
                _ilg.Emit(OpCodes.Pop);
            } else {
                _ilg.BeginCatchBlock(cb.Test);

                EmitSaveExceptionOrPop(cb);

                if (cb.Filter != null) {
                    Label catchBlock = _ilg.DefineLabel();

                    // filters aren't supported in dynamic methods so instead
                    // emit the filter as if check, if (!expr) rethrow
                    PushLabelBlock(LabelBlockKind.Filter);
                    EmitExpressionAndBranch(true, cb.Filter, catchBlock);
                    PopLabelBlock(LabelBlockKind.Filter);

                    _ilg.Emit(OpCodes.Rethrow);
                    _ilg.MarkLabel(catchBlock);

                    // catch body continues
                }
            }
        }

        #endregion
    }
}
