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
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using System.Reflection;

namespace Microsoft.Scripting.Ast {
    partial class LambdaCompiler {
        private void Emit(Block node) {
            Emit(node, EmitAs.Default);
        }

        private void Emit(Block node, EmitAs emitAs) {
            ReadOnlyCollection<Expression> expressions = node.Expressions;
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
        }

        private void Emit(BreakStatement node) {
            CheckAndPushTargets(node.Target);

            EmitBreak();

            PopTargets();
        }

        private void Emit(ContinueStatement node) {
            CheckAndPushTargets(node.Target);

            EmitContinue();

            PopTargets();
        }

        private void Emit(DoStatement node) {
            Label startTarget = _ilg.DefineLabel();
            Label breakTarget = _ilg.DefineLabel();
            Label continueTarget = _ilg.DefineLabel();

            _ilg.MarkLabel(startTarget);
            if (node.Label != null) {
                PushTargets(breakTarget, continueTarget, node.Label);
            }

            EmitExpressionAsVoid(node.Body);

            _ilg.MarkLabel(continueTarget);

            EmitExpression(node.Test);

            _ilg.Emit(OpCodes.Brtrue, startTarget);

            if (node.Label != null) {
                PopTargets();
            }
            _ilg.MarkLabel(breakTarget);
        }

        private void Emit(LabeledStatement node) {
            Debug.Assert(node.Statement != null && node.Label != null);

            Label label = _ilg.DefineLabel();
            PushTargets(label, label, node.Label);

            EmitExpressionAsVoid(node.Statement);

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

            _ilg.MarkLabel(continueTarget);

            if (node.Increment != null) {
                EmitExpressionAsVoid(node.Increment);
                _ilg.MarkLabel(firstTime.Value);
            }

            if (node.Test != null) {
                EmitExpression(node.Test);
                _ilg.Emit(OpCodes.Brfalse, eol);
            }

            if (node.Label != null) {
                PushTargets(breakTarget, continueTarget, node.Label);
            }

            EmitExpressionAsVoid(node.Body);

            _ilg.Emit(OpCodes.Br, continueTarget);

            if (node.Label != null) {
                PopTargets();
            }

            _ilg.MarkLabel(eol);
            if (node.ElseStatement != null) {
                EmitExpressionAsVoid(node.ElseStatement);
            }
            _ilg.MarkLabel(breakTarget);
        }

        private void Emit(ReturnStatement node) {
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
            EmitExpressionAsVoid(node.Body);
            ContextSlot = tempContext;
        }

        #region SwitchStatement

        private void Emit(SwitchStatement node) {
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
                EmitExpressionAsVoid(node.Cases[i].Body);
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
            TryFlowResult flow = TryFlowAnalyzer.Analyze(node.FinallyStatement ?? node.FaultStatement);
            // This will return null if we are not in a generator
            // or if the try statement is unaffected by yields
            TryStatementInfo tsi = GetTsi(node);

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
            if (node.FinallyStatement != null || node.FaultStatement != null) {
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
            if (node.FinallyStatement != null || node.FaultStatement != null) {
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
            EmitExpressionAsVoid(node.Body);

            //******************************************************************
            // Emit the catch blocks
            //******************************************************************

            if (HaveHandlers(node)) {
                List<CatchRecord> catches = new List<CatchRecord>();
                PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);

                ReadOnlyCollection<CatchBlock> handlers = node.Handlers;
                for (int index = 0; index < handlers.Count; index++) {
                    CatchBlock cb = handlers[index];

                    if (tsi.CatchBlockYields(index)) {
                        // The catch block body contains yield, therefore
                        // delay the body emit till after the try block.
                        Slot slot = _ilg.GetLocalTmp(cb.Test);
                        catches.Add(new CatchRecord(slot, cb));
                        
                        EmitCatchStart(cb, slot);
                    } else {
                        EmitCatchStart(cb);
                        // Emit the body right now, since it doesn't contain yield
                        EmitExpressionAsVoid(cb.Body);
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
                    EmitExpressionAsVoid(cr.Block.Body);
                    _ilg.MarkLabel(next);
                }
            }

            //******************************************************************
            // Emit the finally body
            //******************************************************************

            if (node.FinallyStatement != null || node.FaultStatement != null) {
                _ilg.MarkLabel(endFinallyBlock);
                PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);
                _ilg.BeginCatchBlock(typeof(Exception));
                exception.EmitSet(_ilg);

                PopTargets(TargetBlockType.Catch);

                PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);

                if (node.FinallyStatement != null) {
                    _ilg.BeginFinallyBlock();
                } 
                // if we're a fault block we can just continue to run after the catch.

                Label exit = _ilg.DefineLabel();
                GotoRouter.EmitGet(_ilg);
                _ilg.EmitInt(LambdaCompiler.GotoRouterYielding);
                _ilg.Emit(OpCodes.Beq, exit);

                EmitYieldDispatch(tsi.FinallyYields);

                // Emit the finally body

                if (node.FinallyStatement != null) {
                    EmitExpressionAsVoid(node.FinallyStatement);
                } else {
                    EmitExpressionAsVoid(node.FaultStatement);
                }

                // Rethrow the exception, if any

                Label noThrow = _ilg.DefineLabel();
                exception.EmitGet(_ilg);
                _ilg.EmitNull();
                _ilg.Emit(OpCodes.Beq, noThrow);
                exception.EmitGet(_ilg);
                _ilg.Emit(OpCodes.Throw);
                _ilg.MarkLabel(noThrow);
                _ilg.FreeLocalTmp(exception);

                _ilg.MarkLabel(exit);

                EndExceptionBlock();
                PopTargets(TargetBlockType.Finally);
                PopTargets(TargetBlockType.Try);

                //
                // Emit the flow control for finally, if there was any.
                //
                EmitFinallyFlowControl(flow, flowControlFlag);
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

        private void EmitSaveExceptionOrPop(CatchBlock cb, Slot saveSlot) {
            if (saveSlot != null) {
                // overriding the default save location
                saveSlot.EmitSet(_ilg);
            } else if (cb.Variable != null) {
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
                //  
                // if we have a fault handler we turn this into the better:
                //  try {
                //      // try block body and all catch handling
                //  } catch (Exception all) {
                //      saved = all;
                //      fault_body
                //      throw saved
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

            EmitExpressionAsVoid(node.Body);

            //******************************************************************
            // 3. Emit the catch blocks
            //******************************************************************

            if (HaveHandlers(node)) {
                PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);

                foreach (CatchBlock cb in node.Handlers) {
                    // Begin the strongly typed exception block
                    EmitCatchStart(cb);

                    //
                    // Emit the catch block body
                    //
                    EmitExpressionAsVoid(cb.Body);
                }

                PopTargets(TargetBlockType.Catch);
            }

            //******************************************************************
            // 4. Emit the finally block
            //******************************************************************

            if (node.FinallyStatement != null || node.FaultStatement != null) {
                Slot rethrow = null;
                if (flow.Any) {
                    // If there is a control flow in finally/fault, end the catch
                    // statement and emit the catch-all and finally/fault clause
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

                // emit the beginning of the finally/fault start.  If we're
                // in a fault block and we have flow then we just continue
                // with the catch block we have above.  Whenever this is 
                // implemented as a exception handler our code gen isn't ideal
                // because we say we're in a finally, but it is legal code gen.
                // The un-optimial part of this is that we'll generate extra 
                // branches out of the catch block when we could just leave 
                // directly.

                PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);

                if (node.FinallyStatement != null) {
                    _ilg.BeginFinallyBlock();
                } else if (!flow.Any) {
                    // dynamic methods don't support fault blocks so we
                    // generate a catch/rethrow.
                    if (IsDynamicMethod) {
                        _ilg.BeginCatchBlock(typeof(Exception));
                    } else {
                        _ilg.BeginFaultBlock();
                    }
                }

                // Emit the body
                EmitExpressionAsVoid(node.FinallyStatement ?? node.FaultStatement);

                // rethrow the exception if we have flow control or a catch in
                // a dynamic method.
                if (flow.Any) {
                    Debug.Assert(rethrow != null);
                    Label noRethrow = _ilg.DefineLabel();

                    rethrow.EmitGet(_ilg);
                    _ilg.EmitNull();
                    _ilg.Emit(OpCodes.Beq, noRethrow);
                    rethrow.EmitGet(_ilg);
                    _ilg.Emit(OpCodes.Throw);
                    _ilg.MarkLabel(noRethrow);
                } else if (node.FaultStatement != null && IsDynamicMethod) {
                    // rethrow when we generated a catch
                    _ilg.Emit(OpCodes.Rethrow);
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

        /// <summary>
        /// Emits the start of a catch block.  The exception value that is provided by the
        /// CLR is stored in the variable specified by the catch block or popped if no
        /// variable is provided.
        /// </summary>
        private void EmitCatchStart(CatchBlock cb) {
            EmitCatchStart(cb, null);
        }

        /// <summary>
        /// Emits the start of the catch block.  The exception value is stored in the slot
        /// if not null or otherwise provided in the variable of the catch block.
        /// </summary>
        private void EmitCatchStart(CatchBlock cb, Slot saveSlot) {
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
                EmitSaveExceptionOrPop(cb, saveSlot);                
                EmitExpression(cb.Filter);

                // begin the catch, clear the exception, we've 
                // already saved it
                _ilg.MarkLabel(endFilter);
                _ilg.BeginCatchBlock(null);
                _ilg.Emit(OpCodes.Pop);
            } else {
                _ilg.BeginCatchBlock(cb.Test);
                
                EmitSaveExceptionOrPop(cb, saveSlot);

                if (cb.Filter != null) {
                    Label catchBlock = _ilg.DefineLabel();

                    // filters aren't supported in dynamic methods so instead
                    // emit the filter as if check, if (!expr) rethrow
                    EmitExpression(cb.Filter);
                    _ilg.Emit(OpCodes.Brtrue, catchBlock);

                    _ilg.Emit(OpCodes.Rethrow);
                    _ilg.MarkLabel(catchBlock);

                    // catch body continues
                }
            }
        }

        #endregion

        private void Emit(YieldStatement node) {
            EmitYield(node.Expression, GetYieldTarget(node));
        }
    }
}
