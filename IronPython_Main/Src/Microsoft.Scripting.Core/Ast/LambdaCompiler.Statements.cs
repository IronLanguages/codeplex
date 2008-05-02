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
using System.Diagnostics;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using System.Reflection;

namespace Microsoft.Scripting.Ast {
    partial class LambdaCompiler {
        private void Emit(Block node) {
            EmitBlockPrefix(node, node.Expressions.Count);
        }

        private void EmitBlockPrefix(Block node, int count) {
            EmitPosition(node.Start, node.End);
            ReadOnlyCollection<Expression> expressions = node.Expressions;
            for (int index = 0; index < count; index++) {
                Expression current = expressions[index];

                // Emit the expression
                EmitExpression(current);

                // If we don't want the expression just emitted as the result,
                // pop it off of the stack, unless it is a void expression.
                if ((index != expressions.Count - 1 || node.Type == typeof(void)) && current.Type != typeof(void)) {
                    _ilg.Emit(OpCodes.Pop);
                }
            }
        }

        private void Emit(BreakStatement node) {
            EmitPosition(node.Start, node.End);

            CheckAndPushTargets(node.Target);

            EmitBreak();

            PopTargets();
        }

        private void Emit(ContinueStatement node) {
            EmitPosition(node.Start, node.End);

            CheckAndPushTargets(node.Target);

            EmitContinue();

            PopTargets();
        }

        private void Emit(DeleteStatement node) {
            EmitPosition(node.Start, node.End);

            if (node.IsDynamic) {
                EmitCallSite(node, node.Variable);
                return;
            }

            Slot slot = _info.ReferenceSlots[node.Variable];
            slot.EmitDelete(_ilg, VariableInfo.GetName(node.Variable));
        }

        private void Emit(DoStatement node) {
            Label startTarget = _ilg.DefineLabel();
            Label breakTarget = _ilg.DefineLabel();
            Label continueTarget = _ilg.DefineLabel();

            _ilg.MarkLabel(startTarget);
            if (node.Label != null) {
                PushTargets(breakTarget, continueTarget, node.Label);
            }

            EmitExpressionAndPop(node.Body);

            _ilg.MarkLabel(continueTarget);
            // TODO: Check if we need to emit position somewhere else also.
            EmitPosition(node.Start, node.Header);

            EmitExpression(node.Test);

            _ilg.Emit(OpCodes.Brtrue, startTarget);

            if (node.Label != null) {
                PopTargets();
            }
            _ilg.MarkLabel(breakTarget);
        }

        private void Emit(EmptyStatement node) {
            EmitPosition(node.Start, node.End);
        }

        private void Emit(LabeledStatement node) {
            Debug.Assert(node.Statement != null && node.Label != null);

            Label label = _ilg.DefineLabel();
            PushTargets(label, label, node.Label);

            EmitExpressionAndPop(node.Statement);

            _ilg.MarkLabel(label);
            PopTargets();
        }

        private void Emit(LoopStatement node) {
            Label? firstTime = null;
            Label eol = _ilg.DefineLabel();
            Label breakTarget = _ilg.DefineLabel();
            Label continueTarget = _ilg.DefineLabel();

            if (node.Increment != null) {
                firstTime = _ilg.DefineLabel();
                _ilg.Emit(OpCodes.Br, firstTime.Value);
            }

            if (node.Header.IsValid) {
                EmitPosition(node.Start, node.Header);
            }
            _ilg.MarkLabel(continueTarget);

            if (node.Increment != null) {
                EmitExpressionAndPop(node.Increment);
                _ilg.MarkLabel(firstTime.Value);
            }

            if (node.Test != null) {
                EmitExpression(node.Test);
                _ilg.Emit(OpCodes.Brfalse, eol);
            }

            if (node.Label != null) {
                PushTargets(breakTarget, continueTarget, node.Label);
            }

            EmitExpressionAndPop(node.Body);

            _ilg.Emit(OpCodes.Br, continueTarget);

            if (node.Label != null) {
                PopTargets();
            }

            _ilg.MarkLabel(eol);
            if (node.ElseStatement != null) {
                EmitExpressionAndPop(node.ElseStatement);
            }
            _ilg.MarkLabel(breakTarget);
        }

        private void Emit(ReturnStatement node) {
            EmitPosition(node.Start, node.End);
            EmitReturn(node.Expression);
        }

        private void Emit(ScopeStatement node) {
            Slot tempContext = ContextSlot;
            Slot newContext = _ilg.GetLocalTmp(typeof(CodeContext));

            // TODO:
            // $frame = CreateLocalScope<TTuple>(null, null, parent, true):
            _ilg.Emit(OpCodes.Ldnull);
            _ilg.Emit(OpCodes.Ldnull);
            EmitCodeContext();
            _ilg.EmitBoolean(true);
            _ilg.EmitCall(node.Factory.MakeGenericMethod(typeof(Tuple)));
            newContext.EmitSet(_ilg);

            ContextSlot = newContext;
            EmitExpressionAndPop(node.Body);
            ContextSlot = tempContext;
        }

        #region SwitchStatement

        private void Emit(SwitchStatement node) {
            EmitPosition(node.Start, node.Header);

            Label breakTarget = _ilg.DefineLabel();
            Label defaultTarget = breakTarget;
            Label[] labels = new Label[node.Cases.Count];

            // Create all labels
            for (int i = 0; i < node.Cases.Count; i++) {
                labels[i] = _ilg.DefineLabel();

                // Default case.
                if (node.Cases[i].IsDefault) {
                    // Set the default target
                    defaultTarget = labels[i];
                }
            }

            // Emit the test value
            EmitExpression(node.TestValue);

            // Check if jmp table can be emitted
            if (!TryEmitJumpTable(node, labels, defaultTarget)) {
                // There might be scenario(s) where the jmp table is not emitted
                // Emit the switch as conditional branches then
                EmitConditionalBranches(node, labels);
            }

            // If "default" present, execute default code, else exit the switch            
            _ilg.Emit(OpCodes.Br, defaultTarget);

            if (node.Label != null) {
                PushTargets(breakTarget, BlockContinueLabel, node.Label);
            }

            // Emit the bodies
            for (int i = 0; i < node.Cases.Count; i++) {
                // First put the corresponding labels
                _ilg.MarkLabel(labels[i]);
                // And then emit the Body!!
                EmitExpressionAndPop(node.Cases[i].Body);
            }

            if (node.Label != null) {
                PopTargets();
            }

            _ilg.MarkLabel(breakTarget);
        }

        private const int MaxJumpTableSize = 65536;
        private const double MaxJumpTableSparsity = 10;

        // Emits the switch as if stmts
        private void EmitConditionalBranches(SwitchStatement node, Label[] labels) {
            Slot testValueSlot = GetNamedLocal(typeof(int), "switchTestValue");
            testValueSlot.EmitSet(_ilg);

            // For all the "cases" create their conditional branches
            for (int i = 0; i < node.Cases.Count; i++) {
                // Not default case emit the condition
                if (!node.Cases[i].IsDefault) {
                    // Test for equality of case value and the test expression
                    _ilg.EmitInt(node.Cases[i].Value);
                    testValueSlot.EmitGet(_ilg);
                    _ilg.Emit(OpCodes.Beq, labels[i]);
                }
            }
        }

        // Tries to emit switch as a jmp table
        private bool TryEmitJumpTable(SwitchStatement node, Label[] labels, Label defaultTarget) {
            if (node.Cases.Count > MaxJumpTableSize) {
                return false;
            }

            int min = Int32.MaxValue;
            int max = Int32.MinValue;

            // Find the min and max of the values
            for (int i = 0; i < node.Cases.Count; ++i) {
                // Not the default case.
                if (!node.Cases[i].IsDefault) {
                    int val = node.Cases[i].Value;
                    if (min > val) min = val;
                    if (max < val) max = val;
                }
            }

            long delta = (long)max - (long)min;
            if (delta > MaxJumpTableSize) {
                return false;
            }

            // Value distribution is too sparse, don't emit jump table.
            if (delta > node.Cases.Count + MaxJumpTableSparsity) {
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
            for (int i = 0; i < node.Cases.Count; i++) {
                SwitchCase sc = node.Cases[i];
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

        private void Emit(ThrowStatement node) {
            EmitPosition(node.Start, node.End);
            if (node.Value == null) {
                _ilg.Emit(OpCodes.Rethrow);
            } else {
                EmitExpression(node.Value);
                _ilg.Emit(OpCodes.Throw);
            }
        }

        #region TryStatement

        private void Emit(TryStatement node) {
            // Codegen is affected by presence/absence of loop control statements
            // (break/continue) or return/yield statement in finally clause
            TryFlowResult flow = TryFlowAnalyzer.Analyze(node.FinallyStatement);
            // This will return null if we are not in a generator
            // or if the try statement is unaffected by yields
            TryStatementInfo tsi = GetTsi(node);

            EmitPosition(node.Start, node.Header);

            // If there's a yield anywhere, go for a complex codegen
            if (tsi != null && (YieldInBlock(tsi.TryYields) || tsi.YieldInCatch || YieldInBlock(tsi.FinallyYields))) {
                EmitGeneratorTry(tsi, node, flow);
            } else {
                EmitSimpleTry(node, flow);
            }
        }

        private static bool YieldInBlock(List<YieldTarget> block) {
            return block != null && block.Count > 0;
        }

        private void EmitGeneratorTry(TryStatementInfo tsi, TryStatement node, TryFlowResult flow) {
            //
            // Initialize the flow control flag
            //
            Slot flowControlFlag = null;

            if (flow.Any) {
                flowControlFlag = _ilg.GetLocalTmp(typeof(int));
                _ilg.EmitInt(LambdaCompiler.FinallyExitsNormally);
                flowControlFlag.EmitSet(_ilg);
            }

            Slot exception = null;
            if (node.FinallyStatement != null) {
                exception = GetTemporarySlot(typeof(Exception));
                _ilg.EmitNull();
                exception.EmitSet(_ilg);
            }

            //******************************************************************
            // Entering the try block
            //******************************************************************

            if (tsi.Target != null) {
                _ilg.MarkLabel(tsi.Target.EnsureLabel(this));
            }

            //******************************************************************
            // If we have a 'finally', transform it into try..catch..finally
            // and rethrow
            //******************************************************************
            Label endFinallyBlock = new Label();
            if (node.FinallyStatement != null) {
                PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
                _ilg.BeginExceptionBlock();
                endFinallyBlock = _ilg.DefineLabel();

                //**************************************************************
                // If there is a yield in any catch, that catch will be hoisted
                // and we need to dispatch to it from here
                //**************************************************************
                if (tsi.YieldInCatch) {
                    EmitYieldDispatch(tsi.CatchYields);
                }

                if (YieldInBlock(tsi.FinallyYields)) {
                    foreach (YieldTarget yt in tsi.FinallyYields) {
                        GotoRouter.EmitGet(_ilg);
                        _ilg.EmitInt(yt.Index);
                        _ilg.Emit(OpCodes.Beq, endFinallyBlock);
                    }
                }
            }

            //******************************************************************
            // If we have a 'catch', start a try block to handle all the catches
            //******************************************************************

            Label endCatchBlock = new Label();
            if (HaveHandlers(node)) {
                PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
                endCatchBlock = _ilg.BeginExceptionBlock();
            }

            //******************************************************************
            // Emit the try block body
            //******************************************************************

            // First, emit the dispatch within the try block
            EmitYieldDispatch(tsi.TryYields);

            // Then, emit the actual body
            EmitExpressionAndPop(node.Body);
            EmitSequencePointNone();

            //******************************************************************
            // Emit the catch blocks
            //******************************************************************

            if (HaveHandlers(node)) {
                List<CatchRecord> catches = new List<CatchRecord>();
                PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);

                ReadOnlyCollection<CatchBlock> handlers = node.Handlers;
                for (int index = 0; index < handlers.Count; index++) {
                    CatchBlock cb = handlers[index];
                    _ilg.BeginCatchBlock(cb.Test);

                    if (tsi.CatchBlockYields(index)) {
                        // The catch block body contains yield, therefore
                        // delay the body emit till after the try block.
                        Slot slot = _ilg.GetLocalTmp(cb.Test);
                        slot.EmitSet(_ilg);
                        catches.Add(new CatchRecord(slot, cb));
                    } else {
                        // Save the exception (if the catch block asked for it) or pop
                        EmitSaveExceptionOrPop(cb);
                        // Emit the body right now, since it doesn't contain yield
                        EmitExpressionAndPop(cb.Body);
                    }
                }

                PopTargets(TargetBlockType.Catch);
                EndExceptionBlock();
                PopTargets(TargetBlockType.Try);

                //******************************************************************
                // Emit the postponed catch block bodies (with yield in them)
                //******************************************************************
                foreach (CatchRecord cr in catches) {
                    Label next = _ilg.DefineLabel();
                    cr.Slot.EmitGet(_ilg);
                    _ilg.EmitNull();
                    _ilg.Emit(OpCodes.Beq, next);

                    if (cr.Block.Variable != null) {
                        Slot slot = _info.ReferenceSlots[cr.Block.Variable];
                        slot.EmitSet(_ilg, cr.Slot);
                    }

                    _ilg.FreeLocalTmp(cr.Slot);
                    EmitExpressionAndPop(cr.Block.Body);
                    _ilg.MarkLabel(next);
                    EmitSequencePointNone();
                }
            }

            //******************************************************************
            // Emit the finally body
            //******************************************************************

            if (node.FinallyStatement != null) {
                _ilg.MarkLabel(endFinallyBlock);
                PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);
                _ilg.BeginCatchBlock(typeof(Exception));
                exception.EmitSet(_ilg);

                PopTargets(TargetBlockType.Catch);

                PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);
                _ilg.BeginFinallyBlock();

                Label noExit = _ilg.DefineLabel();
                GotoRouter.EmitGet(_ilg);
                _ilg.EmitInt(LambdaCompiler.GotoRouterYielding);
                _ilg.Emit(OpCodes.Bne_Un_S, noExit);
                _ilg.Emit(OpCodes.Endfinally);
                _ilg.MarkLabel(noExit);

                EmitYieldDispatch(tsi.FinallyYields);

                // Emit the finally body

                EmitExpressionAndPop(node.FinallyStatement);

                // Rethrow the exception, if any

                Label noThrow = _ilg.DefineLabel();
                exception.EmitGet(_ilg);
                _ilg.EmitNull();
                _ilg.Emit(OpCodes.Beq, noThrow);
                exception.EmitGet(_ilg);
                _ilg.Emit(OpCodes.Throw);
                _ilg.MarkLabel(noThrow);
                _ilg.FreeLocalTmp(exception);

                EndExceptionBlock();
                PopTargets(TargetBlockType.Finally);
                PopTargets(TargetBlockType.Try);

                //
                // Emit the flow control for finally, if there was any.
                //
                EmitFinallyFlowControl(flow, flowControlFlag);

                EmitSequencePointNone();
            }

            // Clear the target labels

            ClearLabels(tsi.TryYields);
            ClearLabels(tsi.CatchYields);

            if (tsi.Target != null) {
                tsi.Target.Clear();
            }
        }

        private static bool HaveHandlers(TryStatement node) {
            return node.Handlers != null && node.Handlers.Count > 0;
        }

        // TODO: 
        private static void ClearLabels(List<YieldTarget> targets) {
            if (targets != null) {
                foreach (YieldTarget yt in targets) {
                    yt.Clear();
                }
            }
        }

        private void EmitSaveExceptionOrPop(CatchBlock cb) {
            if (cb.Variable != null) {
                // If the variable is present, store the exception
                // in the variable.
                Slot slot = _info.ReferenceSlots[cb.Variable];
                slot.EmitSet(_ilg);
            } else {
                // Otherwise, pop it off the stack.
                _ilg.Emit(OpCodes.Pop);
            }
        }

        private void EmitYieldDispatch(List<YieldTarget> targets) {
            if (YieldInBlock(targets)) {
                Debug.Assert(GotoRouter != null);

                // TODO: Emit as switch!
                foreach (YieldTarget yt in targets) {
                    GotoRouter.EmitGet(_ilg);
                    _ilg.EmitInt(yt.Index);
                    _ilg.Emit(OpCodes.Beq, yt.EnsureLabel(this));
                }
            }
        }

        /// <summary>
        /// If the finally statement contains break, continue, return or yield, we need to
        /// handle the control flow statement after we exit out of finally via OpCodes.Endfinally.
        /// </summary>
        private void EmitFinallyFlowControl(TryFlowResult flow, Slot flag) {
            if (flow.Return || flow.Yield) {
                Debug.Assert(flag != null);

                Label noReturn = _ilg.DefineLabel();

                flag.EmitGet(_ilg);
                _ilg.EmitInt(LambdaCompiler.BranchForReturn);
                _ilg.Emit(OpCodes.Bne_Un, noReturn);

                if (IsGeneratorBody) {
                    // return true from the generator method
                    _ilg.Emit(OpCodes.Ldc_I4_1);
                    EmitReturn();
                } else if (flow.Any) {
                    // return the actual value
                    EmitReturnValue();
                    EmitReturn();
                }
                _ilg.MarkLabel(noReturn);
            }

            // Only emit break handling if it is actually needed
            if (flow.Break) {
                Debug.Assert(flag != null);

                Label noReturn = _ilg.DefineLabel();
                flag.EmitGet(_ilg);
                _ilg.EmitInt(LambdaCompiler.BranchForBreak);
                _ilg.Emit(OpCodes.Bne_Un, noReturn);
                EmitBreak();
                _ilg.MarkLabel(noReturn);
            }

            // Only emit continue handling if it if actually needed
            if (flow.Continue) {
                Debug.Assert(flag != null);

                Label noReturn = _ilg.DefineLabel();
                flag.EmitGet(_ilg);
                _ilg.EmitInt(LambdaCompiler.BranchForContinue);
                _ilg.Emit(OpCodes.Bne_Un, noReturn);
                EmitContinue();
                _ilg.MarkLabel(noReturn);
            }
        }

        private void EmitSimpleTry(TryStatement node, TryFlowResult flow) {
            //
            // Initialize the flow control flag
            //
            Slot flowControlFlag = null;
            if (flow.Any) {
                Debug.Assert(node.FinallyStatement != null);

                flowControlFlag = _ilg.GetLocalTmp(typeof(int));
                _ilg.EmitInt(LambdaCompiler.FinallyExitsNormally);
                flowControlFlag.EmitSet(_ilg);

                //  If there is a control flow in finally, emit outer:
                //  try {
                //      // try block body and all catch handling
                //  } catch (Exception all) {
                //      saved = all;
                //  } finally {
                //      finally_body
                //      if (saved != null) {
                //          throw saved;
                //      }
                //  }

                if (HaveHandlers(node)) {
                    PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
                    _ilg.BeginExceptionBlock();
                }
            }

            //******************************************************************
            // 1. ENTERING TRY
            //******************************************************************

            PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
            _ilg.BeginExceptionBlock();

            //******************************************************************
            // 2. Emit the try statement body
            //******************************************************************

            EmitExpressionAndPop(node.Body);
            EmitSequencePointNone();

            //******************************************************************
            // 3. Emit the catch blocks
            //******************************************************************

            if (HaveHandlers(node)) {
                PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);

                foreach (CatchBlock cb in node.Handlers) {
                    // Begin the strongly typed exception block
                    _ilg.BeginCatchBlock(cb.Test);

                    // Save the exception (if the catch block asked for it) or pop
                    EmitSaveExceptionOrPop(cb);

                    //
                    // Emit the catch block body
                    //
                    EmitExpressionAndPop(cb.Body);
                }

                PopTargets(TargetBlockType.Catch);
            }

            //******************************************************************
            // 4. Emit the finally block
            //******************************************************************

            if (node.FinallyStatement != null) {
                Slot rethrow = null;
                if (flow.Any) {
                    // If there is a control flow in finally, end the catch
                    // statement and emit the catch-all and finally clause
                    // with rethrow at the end.

                    if (HaveHandlers(node)) {
                        EndExceptionBlock();
                        PopTargets(TargetBlockType.Try);
                    }

                    PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);
                    _ilg.BeginCatchBlock(typeof(Exception));

                    rethrow = _ilg.GetLocalTmp(typeof(Exception));
                    rethrow.EmitSet(_ilg);

                    PopTargets(TargetBlockType.Catch);
                }

                PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);
                _ilg.BeginFinallyBlock();
                
                //
                // Emit the finally block body
                //
                EmitExpressionAndPop(node.FinallyStatement);

                if (flow.Any) {
                    Debug.Assert(rethrow != null);
                    Label noRethrow = _ilg.DefineLabel();

                    rethrow.EmitGet(_ilg);
                    _ilg.EmitNull();
                    _ilg.Emit(OpCodes.Beq, noRethrow);
                    rethrow.EmitGet(_ilg);
                    _ilg.Emit(OpCodes.Throw);
                    _ilg.MarkLabel(noRethrow);
                }                

                EndExceptionBlock();
                PopTargets(TargetBlockType.Finally);
            } else {
                EndExceptionBlock();
            }

            PopTargets(TargetBlockType.Try);

            //
            // Emit the flow control for finally, if there was any.
            //
            EmitFinallyFlowControl(flow, flowControlFlag);

            _ilg.FreeLocalTmp(flowControlFlag);
        }

        #endregion

        private void Emit(YieldStatement node) {
            EmitPosition(node.Start, node.End);
            EmitYield(node.Expression, GetYieldTarget(node));
        }
    }
}
