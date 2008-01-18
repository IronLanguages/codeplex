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

namespace Microsoft.Scripting.Ast {
    // TODO: Make internal, don't allow direct callers
    public partial class Compiler {
        private void Emit(Block node) {
            EmitPosition(node.Start, node.End);
            EmitBlockPrefix(node, node.Expressions.Count);
        }

        private void EmitBlockPrefix(Block node, int count) {
            ReadOnlyCollection<Expression> expressions = node.Expressions;
            for (int index = 0; index < count; index++) {
                Expression current = expressions[index];

                // Emit the expression
                EmitExpression(current);

                // If we don't want the expression just emitted as the result,
                // pop it off of the stack, unless it is a void expression.
                if ((index != expressions.Count - 1 || node.Type == typeof(void)) && current.Type != typeof(void)) {
                    Emit(OpCodes.Pop);
                }
            }
        }

        private void Emit(BreakStatement node) {
            EmitPosition(node.Start, node.End);

            if (node.Statement != null) {
                CheckAndPushTargets(node.Statement);
            }

            EmitBreak();

            if (node.Statement != null) {
                PopTargets();
            }
        }

        private void Emit(ContinueStatement node) {
            EmitPosition(node.Start, node.End);

            if (node.Statement != null) {
                CheckAndPushTargets(node.Statement);
            }

            EmitContinue();

            if (node.Statement != null) {
                PopTargets();
            }
        }

        private void Emit(DeleteStatement node) {
            EmitPosition(node.Start, node.End);
            Slot slot = GetVariableSlot(node.Variable);
            slot.EmitDelete(this, node.Variable.Name, !node.IsDefined);
        }

        private void Emit(DoStatement node) {
            Label startTarget = DefineLabel();
            Label breakTarget = DefineLabel();
            Label continueTarget = DefineLabel();

            MarkLabel(startTarget);
            PushTargets(breakTarget, continueTarget, node);

            EmitExpressionAndPop(node.Body);

            MarkLabel(continueTarget);
            // TODO: Check if we need to emit position somewhere else also.
            EmitPosition(node.Start, node.Header);

            EmitExpression(node.Test);

            Emit(OpCodes.Brtrue, startTarget);

            PopTargets();
            MarkLabel(breakTarget);
        }

        private void Emit(EmptyStatement node) {
            EmitPosition(node.Start, node.End);
        }

        private void Emit(ExpressionStatement node) {
            EmitPosition(node.Start, node.End);
            EmitExpression(node.Expression);
        }

        private void Emit(LabeledStatement node) {
            // TODO: Validate in a pass before codegen!!!
            if (node.Statement == null) {
                throw new InvalidOperationException("Incomplete LabelStatement");
            }

            Label label = DefineLabel();
            PushTargets(label, label, node);

            EmitExpressionAndPop(node.Statement);

            MarkLabel(label);
            PopTargets();
        }

        private void Emit(LoopStatement node) {
            Nullable<Label> firstTime = null;
            Label eol = DefineLabel();
            Label breakTarget = DefineLabel();
            Label continueTarget = DefineLabel();

            if (node.Increment != null) {
                firstTime = DefineLabel();
                Emit(OpCodes.Br, firstTime.Value);
            }

            if (node.Header.IsValid) {
                EmitPosition(node.Start, node.Header);
            }
            MarkLabel(continueTarget);

            if (node.Increment != null) {
                EmitExpressionAndPop(node.Increment);
                MarkLabel(firstTime.Value);
            }

            if (node.Test != null) {
                EmitExpression(node.Test);
                Emit(OpCodes.Brfalse, eol);
            }

            PushTargets(breakTarget, continueTarget, node);

            EmitExpressionAndPop(node.Body);

            Emit(OpCodes.Br, continueTarget);

            PopTargets();

            MarkLabel(eol);
            if (node.ElseStatement != null) {
                EmitExpressionAndPop(node.ElseStatement);
            }
            MarkLabel(breakTarget);
        }

        private void Emit(ReturnStatement node) {
            EmitPosition(node.Start, node.End);
            EmitReturn(node.Expression);
        }

        private void Emit(ScopeStatement node) {
            Slot tempContext = ContextSlot;
            Slot newContext = GetLocalTmp(typeof(CodeContext));

            // TODO: should work with LocalScope
            if (node.Scope != null) {
                EmitExpression(node.Scope);     //Locals dictionary
                EmitCodeContext();              //CodeContext
                EmitBoolean(true);              //Visible = true
                EmitCall(typeof(RuntimeHelpers), "CreateNestedCodeContext");
            } else {
                EmitCodeContext();
                EmitCall(typeof(RuntimeHelpers), "CreateCodeContext");
            }

            newContext.EmitSet(this);

            ContextSlot = newContext;

            EmitExpressionAndPop(node.Body);

            ContextSlot = tempContext;
        }

        #region SwitchStatement

        private void Emit(SwitchStatement node) {
            EmitPosition(node.Start, node.Header);

            Label breakTarget = DefineLabel();
            Label defaultTarget = breakTarget;
            Label[] labels = new Label[node.Cases.Count];

            // Create all labels
            for (int i = 0; i < node.Cases.Count; i++) {
                labels[i] = DefineLabel();

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
            Emit(OpCodes.Br, defaultTarget);

            PushTargets(breakTarget, BlockContinueLabel, node);

            // Emit the bodies
            for (int i = 0; i < node.Cases.Count; i++) {
                // First put the corresponding labels
                MarkLabel(labels[i]);
                // And then emit the Body!!
                EmitExpressionAndPop(node.Cases[i].Body);
            }

            PopTargets();
            MarkLabel(breakTarget);
        }

        private const int MaxJumpTableSize = 65536;
        private const double MaxJumpTableSparsity = 10;

        // Emits the switch as if stmts
        private void EmitConditionalBranches(SwitchStatement node, Label[] labels) {
            Slot testValueSlot = GetNamedLocal(typeof(int), "switchTestValue");
            testValueSlot.EmitSet(this);

            // For all the "cases" create their conditional branches
            for (int i = 0; i < node.Cases.Count; i++) {
                // Not default case emit the condition
                if (!node.Cases[i].IsDefault) {
                    // Test for equality of case value and the test expression
                    EmitInt(node.Cases[i].Value);
                    testValueSlot.EmitGet(this);
                    Emit(OpCodes.Beq, labels[i]);
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
                EmitInt(min);
                Emit(OpCodes.Sub);
            }
            Emit(OpCodes.Switch, jmpLabels);
            return true;
        }

        #endregion

        private void Emit(ThrowStatement node) {
            EmitPosition(node.Start, node.End);
            if (node.Value == null) {
                Emit(OpCodes.Rethrow);
            } else {
                EmitExpression(node.Value);
                Emit(OpCodes.Throw);
            }
        }

        #region TryStatement

        private void Emit(TryStatement node) {
            // Codegen is affected by presence/absence of loop control statements
            // (break/continue) or return/yield statement in finally clause
            TryFlowResult flow = TryFlowAnalyzer.Analyze(node.FinallyStatement);

            EmitPosition(node.Start, node.Header);

            // If there's a yield anywhere, go for a complex codegen
            if (YieldInBlock(node.TryYields) || node.YieldInCatch || YieldInBlock(node.FinallyYields)) {
                EmitGeneratorTry(node, flow);
            } else {
                EmitSimpleTry(node, flow);
            }
        }

        private static bool YieldInBlock(List<YieldTarget> block) {
            return block != null && block.Count > 0;
        }

        private void EmitGeneratorTry(TryStatement node, TryFlowResult flow) {
            //
            // Initialize the flow control flag
            //
            Slot flowControlFlag = null;

            if (flow.Any) {
                flowControlFlag = GetLocalTmp(typeof(int));
                EmitInt(Compiler.FinallyExitsNormally);
                flowControlFlag.EmitSet(this);
            }

            Slot exception = null;
            if (node.FinallyStatement != null) {
                exception = GetTemporarySlot(typeof(Exception));
                EmitNull();
                exception.EmitSet(this);
            }

            //******************************************************************
            // Entering the try block
            //******************************************************************

            if (node.Target != null) {
                MarkLabel(node.Target.EnsureLabel(this));
            }

            //******************************************************************
            // If we have a 'finally', transform it into try..catch..finally
            // and rethrow
            //******************************************************************
            Label endFinallyBlock = new Label();
            if (node.FinallyStatement != null) {
                PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
                BeginExceptionBlock();
                endFinallyBlock = DefineLabel();

                //**************************************************************
                // If there is a yield in any catch, that catch will be hoisted
                // and we need to dispatch to it from here
                //**************************************************************
                if (node.YieldInCatch) {
                    EmitYieldDispatch(node.CatchYields);
                }

                if (YieldInBlock(node.FinallyYields)) {
                    foreach (YieldTarget yt in node.FinallyYields) {
                        GotoRouter.EmitGet(this);
                        EmitInt(yt.Index);
                        Emit(OpCodes.Beq, endFinallyBlock);
                    }
                }
            }

            //******************************************************************
            // If we have a 'catch', start a try block to handle all the catches
            //******************************************************************

            Label endCatchBlock = new Label();
            if (HaveHandlers(node)) {
                PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
                endCatchBlock = BeginExceptionBlock();
            }

            //******************************************************************
            // Emit the try block body
            //******************************************************************

            // First, emit the dispatch within the try block
            EmitYieldDispatch(node.TryYields);

            // Then, emit the actual body
            EmitExpressionAndPop(node.Body);
            EmitSequencePointNone();

            //******************************************************************
            // Emit the catch blocks
            //******************************************************************

            if (HaveHandlers(node)) {
                List<CatchRecord> catches = new List<CatchRecord>();
                PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);

                foreach (CatchBlock cb in node.Handlers) {
                    BeginCatchBlock(cb.Test);

                    if (cb.Yield) {
                        // The catch block body contains yield, therefore
                        // delay the body emit till after the try block.
                        Slot slot = GetLocalTmp(cb.Test);
                        slot.EmitSet(this);
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
                    Label next = DefineLabel();
                    cr.Slot.EmitGet(this);
                    EmitNull();
                    Emit(OpCodes.Beq, next);

                    if (cr.Block.Variable != null) {
                        Slot slot = GetVariableSlot(cr.Block.Variable);
                        slot.EmitSet(this, cr.Slot);
                    }

                    FreeLocalTmp(cr.Slot);
                    EmitExpressionAndPop(cr.Block.Body);
                    MarkLabel(next);
                    EmitSequencePointNone();
                }
            }

            //******************************************************************
            // Emit the finally body
            //******************************************************************

            if (node.FinallyStatement != null) {
                MarkLabel(endFinallyBlock);
                PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);
                BeginCatchBlock(typeof(Exception));
                exception.EmitSet(this);

                PopTargets(TargetBlockType.Catch);

                PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);
                BeginFinallyBlock();

                Label noExit = DefineLabel();
                GotoRouter.EmitGet(this);
                EmitInt(Compiler.GotoRouterYielding);
                Emit(OpCodes.Bne_Un_S, noExit);
                Emit(OpCodes.Endfinally);
                MarkLabel(noExit);

                EmitYieldDispatch(node.FinallyYields);

                // Emit the finally body

                EmitExpressionAndPop(node.FinallyStatement);

                // Rethrow the exception, if any

                Label noThrow = DefineLabel();
                exception.EmitGet(this);
                EmitNull();
                Emit(OpCodes.Beq, noThrow);
                exception.EmitGet(this);
                Emit(OpCodes.Throw);
                MarkLabel(noThrow);
                FreeLocalTmp(exception);

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

            ClearLabels(node.TryYields);
            ClearLabels(node.CatchYields);

            if (node.Target != null) {
                node.Target.Clear();
            }
        }

        private static bool HaveHandlers(TryStatement node) {
            return node.Handlers != null && node.Handlers.Count > 0;
        }

        // TODO: 
        internal static void ClearLabels(List<YieldTarget> targets) {
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
                Slot slot = GetVariableSlot(cb.Variable);
                slot.EmitSet(this);
            } else {
                // Otherwise, pop it off the stack.
                Emit(OpCodes.Pop);
            }
        }

        private void EmitYieldDispatch(List<YieldTarget> targets) {
            if (YieldInBlock(targets)) {
                Debug.Assert(GotoRouter != null);

                // TODO: Emit as switch!
                foreach (YieldTarget yt in targets) {
                    GotoRouter.EmitGet(this);
                    EmitInt(yt.Index);
                    Emit(OpCodes.Beq, yt.EnsureLabel(this));
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

                Label noReturn = DefineLabel();

                flag.EmitGet(this);
                EmitInt(Compiler.BranchForReturn);
                Emit(OpCodes.Bne_Un, noReturn);

                if (IsGenerator) {
                    // return true from the generator method
                    Emit(OpCodes.Ldc_I4_1);
                    EmitReturn();
                } else if (flow.Any) {
                    // return the actual value
                    EmitReturnValue();
                    EmitReturn();
                }
                MarkLabel(noReturn);
            }

            // Only emit break handling if it is actually needed
            if (flow.Break) {
                Debug.Assert(flag != null);

                Label noReturn = DefineLabel();
                flag.EmitGet(this);
                EmitInt(Compiler.BranchForBreak);
                Emit(OpCodes.Bne_Un, noReturn);
                EmitBreak();
                MarkLabel(noReturn);
            }

            // Only emit continue handling if it if actually needed
            if (flow.Continue) {
                Debug.Assert(flag != null);

                Label noReturn = DefineLabel();
                flag.EmitGet(this);
                EmitInt(Compiler.BranchForContinue);
                Emit(OpCodes.Bne_Un, noReturn);
                EmitContinue();
                MarkLabel(noReturn);
            }
        }

        private void EmitSimpleTry(TryStatement node, TryFlowResult flow) {
            //
            // Initialize the flow control flag
            //
            Slot flowControlFlag = null;
            if (flow.Any) {
                Debug.Assert(node.FinallyStatement != null);

                flowControlFlag = GetLocalTmp(typeof(int));
                EmitInt(Compiler.FinallyExitsNormally);
                flowControlFlag.EmitSet(this);

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
                    BeginExceptionBlock();
                }
            }

            //******************************************************************
            // 1. ENTERING TRY
            //******************************************************************

            PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
            BeginExceptionBlock();

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
                    BeginCatchBlock(cb.Test);

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
                    BeginCatchBlock(typeof(Exception));

                    rethrow = GetLocalTmp(typeof(Exception));
                    rethrow.EmitSet(this);

                    PopTargets(TargetBlockType.Catch);
                }

                PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);
                BeginFinallyBlock();

                //
                // Emit the finally block body
                //
                EmitExpressionAndPop(node.FinallyStatement);

                if (flow.Any) {
                    Debug.Assert(rethrow != null);
                    Label noRethrow = DefineLabel();

                    rethrow.EmitGet(this);
                    EmitNull();
                    Emit(OpCodes.Beq, noRethrow);
                    rethrow.EmitGet(this);
                    Emit(OpCodes.Throw);
                    MarkLabel(noRethrow);
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

            FreeLocalTmp(flowControlFlag);
        }

        #endregion

        private void Emit(YieldStatement node) {
            EmitPosition(node.Start, node.End);
            EmitYield(node.Expression, node.Target);
        }
    }
}
