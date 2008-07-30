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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Scripting.Generation;

namespace System.Linq.Expressions {
    partial class LambdaCompiler {
        private static void EmitBlock(LambdaCompiler lc, Expression expr) {
            lc.Emit((Block)expr, EmitAs.Default);
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

        private static void EmitBreakStatement(LambdaCompiler lc, Expression expr) {
            lc.CheckAndPushTargets(((BreakStatement)expr).Target);
            lc.EmitBreak();
            lc.PopTargets();
        }

        private static void EmitContinueStatement(LambdaCompiler lc, Expression expr) {
            lc.CheckAndPushTargets(((ContinueStatement)expr).Target);
            lc.EmitContinue();
            lc.PopTargets();
        }

        private static void EmitDoStatement(LambdaCompiler lc, Expression expr) {
            DoStatement node = (DoStatement)expr;

            Label startTarget = lc._ilg.DefineLabel();
            Label breakTarget = lc._ilg.DefineLabel();
            Label continueTarget = lc._ilg.DefineLabel();

            lc._ilg.MarkLabel(startTarget);
            if (node.Label != null) {
                lc.PushTargets(breakTarget, continueTarget, node.Label);
            }

            lc.EmitExpressionAsVoid(node.Body);

            lc._ilg.MarkLabel(continueTarget);

            lc.EmitExpression(node.Test);

            lc._ilg.Emit(OpCodes.Brtrue, startTarget);

            if (node.Label != null) {
                lc.PopTargets();
            }
            lc._ilg.MarkLabel(breakTarget);
        }

        private static void EmitEmptyStatement(LambdaCompiler lc, Expression expr) {
        }

        private static void EmitLabeledStatement(LambdaCompiler lc, Expression expr) {
            LabeledStatement node = (LabeledStatement)expr;
            Debug.Assert(node.Statement != null && node.Label != null);

            Label label = lc._ilg.DefineLabel();
            lc.PushTargets(label, label, node.Label);

            lc.EmitExpressionAsVoid(node.Statement);

            lc._ilg.MarkLabel(label);
            lc.PopTargets();
        }

        private static void EmitLoopStatement (LambdaCompiler lc, Expression expr) {
            LoopStatement node = (LoopStatement)expr;
            Label? firstTime = null;
            Label eol = lc._ilg.DefineLabel();
            Label breakTarget = lc._ilg.DefineLabel();
            Label continueTarget = lc._ilg.DefineLabel();

            if (node.Increment != null) {
                firstTime = lc._ilg.DefineLabel();
                lc._ilg.Emit(OpCodes.Br, firstTime.Value);
            }

            lc._ilg.MarkLabel(continueTarget);

            if (node.Increment != null) {
                lc.EmitExpressionAsVoid(node.Increment);
                lc._ilg.MarkLabel(firstTime.Value);
            }

            if (node.Test != null) {
                lc.EmitExpression(node.Test);
                lc._ilg.Emit(OpCodes.Brfalse, eol);
            }

            if (node.Label != null) {
                lc.PushTargets(breakTarget, continueTarget, node.Label);
            }

            lc.EmitExpressionAsVoid(node.Body);

            lc._ilg.Emit(OpCodes.Br, continueTarget);

            if (node.Label != null) {
                lc.PopTargets();
            }

            lc._ilg.MarkLabel(eol);
            if (node.ElseStatement != null) {
                lc.EmitExpressionAsVoid(node.ElseStatement);
            }
            lc._ilg.MarkLabel(breakTarget);
        }

        private static void EmitReturnStatement(LambdaCompiler lc, Expression expr) {
            lc.EmitReturn(((ReturnStatement)expr).Expression);
        }

        #region SwitchStatement

        private static void EmitSwitchStatement(LambdaCompiler lc, Expression expr) {
            SwitchStatement node = (SwitchStatement)expr;

            Label breakTarget = lc._ilg.DefineLabel();
            Label defaultTarget = breakTarget;
            Label[] labels = new Label[node.Cases.Count];

            // Create all labels
            for (int i = 0; i < node.Cases.Count; i++) {
                labels[i] = lc._ilg.DefineLabel();

                // Default case.
                if (node.Cases[i].IsDefault) {
                    // Set the default target
                    defaultTarget = labels[i];
                }
            }

            // Emit the test value
            lc.EmitExpression(node.TestValue);

            // Check if jmp table can be emitted
            if (!lc.TryEmitJumpTable(node, labels, defaultTarget)) {
                // There might be scenario(s) where the jmp table is not emitted
                // Emit the switch as conditional branches then
                lc.EmitConditionalBranches(node, labels);
            }

            // If "default" present, execute default code, else exit the switch            
            lc._ilg.Emit(OpCodes.Br, defaultTarget);

            if (node.Label != null) {
                lc.PushTargets(breakTarget, lc.BlockContinueLabel, node.Label);
            }

            // Emit the bodies
            for (int i = 0; i < node.Cases.Count; i++) {
                // First put the corresponding labels
                lc._ilg.MarkLabel(labels[i]);
                // And then emit the Body!!
                lc.EmitExpressionAsVoid(node.Cases[i].Body);
            }

            if (node.Label != null) {
                lc.PopTargets();
            }

            lc._ilg.MarkLabel(breakTarget);
        }

        private const int MaxJumpTableSize = 65536;
        private const double MaxJumpTableSparsity = 10;

        // Emits the switch as if stmts
        private void EmitConditionalBranches(SwitchStatement node, Label[] labels) {
            LocalBuilder testValueSlot = GetNamedLocal(typeof(int), "$switchTestValue");
            _ilg.Emit(OpCodes.Stloc, testValueSlot);
            
            // For all the "cases" create their conditional branches
            for (int i = 0; i < node.Cases.Count; i++) {
                // Not default case emit the condition
                if (!node.Cases[i].IsDefault) {
                    // Test for equality of case value and the test expression
                    _ilg.EmitInt(node.Cases[i].Value);
                    _ilg.Emit(OpCodes.Ldloc, testValueSlot);
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

        private static void EmitThrowStatement(LambdaCompiler lc, Expression expr) {
            ThrowStatement node = (ThrowStatement)expr;
            if (node.Value == null) {
                lc._ilg.Emit(OpCodes.Rethrow);
            } else {
                lc.EmitExpression(node.Value);
                lc._ilg.Emit(OpCodes.Throw);
            }
        }

        #region TryStatement

        private static void EmitTryStatement(LambdaCompiler lc, Expression expr) {
            TryStatement node = (TryStatement)expr;

            // Codegen is affected by presence/absence of loop control statements
            // (break/continue) or return/yield statement in finally clause
            TryFlowResult flow = TryFlowAnalyzer.Analyze(node.Finally ?? node.Fault);

            // This will return null if we are not in a generator
            // or if the try statement is unaffected by yields
            TryStatementInfo tsi = lc.GetTsi(node);

            // If there's a yield anywhere, go for a complex codegen
            if (tsi != null && (YieldInBlock(tsi.TryYields) || tsi.YieldInCatch || YieldInBlock(tsi.FinallyYields))) {
                lc.EmitGeneratorTry(tsi, node, flow);
            } else {
                lc.EmitSimpleTry(node, flow);
            }
        }

        private static bool YieldInBlock(List<YieldTarget> block) {
            return block != null && block.Count > 0;
        }

        private void EmitGeneratorTry(TryStatementInfo tsi, TryStatement node, TryFlowResult flow) {
            //
            // Initialize the flow control flag
            //
            LocalBuilder flowControlFlag = null;

            if (flow.Any) {
                flowControlFlag = _ilg.GetLocal(typeof(int));
                _ilg.EmitInt(LambdaCompiler.FinallyExitsNormally);
                _ilg.Emit(OpCodes.Stloc, flowControlFlag);
            }

            VariableExpression exception = null;
            if (node.Finally != null || node.Fault != null) {
                exception = _scope.GetGeneratorTemp(typeof(Exception));
                _ilg.EmitNull();
                _scope.EmitSet(exception);
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
            if (node.Finally != null || node.Fault != null) {
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
                        _ilg.Emit(OpCodes.Ldloc, GotoRouter);
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
                        LocalBuilder local = _ilg.GetLocal(cb.Test);
                        catches.Add(new CatchRecord(local, cb));

                        EmitCatchStart(cb, local);
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
                    _ilg.Emit(OpCodes.Ldloc, cr.Local);
                    _ilg.EmitNull();
                    _ilg.Emit(OpCodes.Beq, next);

                    if (cr.Block.Variable != null) {
                        _ilg.Emit(OpCodes.Ldloc, cr.Local);
                        _scope.EmitSet(cr.Block.Variable);
                    }

                    _ilg.FreeLocal(cr.Local);
                    EmitExpressionAsVoid(cr.Block.Body);
                    _ilg.MarkLabel(next);
                }
            }

            //******************************************************************
            // Emit the finally body
            //******************************************************************

            if (node.Finally != null || node.Fault != null) {
                _ilg.MarkLabel(endFinallyBlock);
                PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);
                _ilg.BeginCatchBlock(typeof(Exception));
                _scope.EmitSet(exception);

                PopTargets(TargetBlockType.Catch);

                PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);

                if (node.Finally != null) {
                    _ilg.BeginFinallyBlock();
                } 
                // if we're a fault block we can just continue to run after the catch.

                Label exit = _ilg.DefineLabel();
                _ilg.Emit(OpCodes.Ldloc, GotoRouter);
                _ilg.EmitInt(LambdaCompiler.GotoRouterYielding);
                _ilg.Emit(OpCodes.Beq, exit);

                EmitYieldDispatch(tsi.FinallyYields);

                // Emit the finally body

                if (node.Finally != null) {
                    EmitExpressionAsVoid(node.Finally);
                } else {
                    EmitExpressionAsVoid(node.Fault);
                }

                // Rethrow the exception, if any

                Label noThrow = _ilg.DefineLabel();
                _scope.EmitGet(exception);
                _ilg.EmitNull();
                _ilg.Emit(OpCodes.Beq, noThrow);
                _scope.EmitGet(exception);
                _ilg.Emit(OpCodes.Throw);
                _ilg.MarkLabel(noThrow);

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
            return node.Handlers.Count > 0;
        }

        // TODO: 
        private static void ClearLabels(List<YieldTarget> targets) {
            if (targets != null) {
                foreach (YieldTarget yt in targets) {
                    yt.Clear();
                }
            }
        }

        private void EmitSaveExceptionOrPop(CatchBlock cb, LocalBuilder saveSlot) {
            if (saveSlot != null) {
                // overriding the default save location
                _ilg.Emit(OpCodes.Stloc, saveSlot);
            } else if (cb.Variable != null) {
                // If the variable is present, store the exception
                // in the variable.
                _scope.EmitSet(cb.Variable);
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
                    _ilg.Emit(OpCodes.Ldloc, GotoRouter);
                    _ilg.EmitInt(yt.Index);
                    _ilg.Emit(OpCodes.Beq, yt.EnsureLabel(this));
                }
            }
        }

        /// <summary>
        /// If the finally statement contains break, continue, return or yield, we need to
        /// handle the control flow statement after we exit out of finally via OpCodes.Endfinally.
        /// </summary>
        private void EmitFinallyFlowControl(TryFlowResult flow, LocalBuilder flag) {
            if (flow.Return || flow.Yield) {
                Debug.Assert(flag != null);

                Label noReturn = _ilg.DefineLabel();

                _ilg.Emit(OpCodes.Ldloc, flag);
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
                _ilg.Emit(OpCodes.Ldloc, flag);
                _ilg.EmitInt(LambdaCompiler.BranchForBreak);
                _ilg.Emit(OpCodes.Bne_Un, noReturn);
                EmitBreak();
                _ilg.MarkLabel(noReturn);
            }

            // Only emit continue handling if it if actually needed
            if (flow.Continue) {
                Debug.Assert(flag != null);

                Label noReturn = _ilg.DefineLabel();
                _ilg.Emit(OpCodes.Ldloc, flag);
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
            LocalBuilder flowControlFlag = null;
            if (flow.Any) {
                Debug.Assert(node.Finally != null);

                flowControlFlag = _ilg.GetLocal(typeof(int));
                _ilg.EmitInt(LambdaCompiler.FinallyExitsNormally);
                _ilg.Emit(OpCodes.Stloc, flowControlFlag);

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

            if (node.Finally != null || node.Fault != null) {
                LocalBuilder rethrow = null;
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

                    rethrow = _ilg.GetLocal(typeof(Exception));
                    _ilg.Emit(OpCodes.Stloc, rethrow);

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

                if (node.Finally != null) {
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
                EmitExpressionAsVoid(node.Finally ?? node.Fault);

                // rethrow the exception if we have flow control or a catch in
                // a dynamic method.
                if (flow.Any) {
                    Debug.Assert(rethrow != null);
                    Label noRethrow = _ilg.DefineLabel();

                    _ilg.Emit(OpCodes.Ldloc, rethrow);
                    _ilg.EmitNull();
                    _ilg.Emit(OpCodes.Beq, noRethrow);
                    _ilg.Emit(OpCodes.Ldloc, rethrow);
                    _ilg.Emit(OpCodes.Throw);
                    _ilg.MarkLabel(noRethrow);
                } else if (node.Fault != null && IsDynamicMethod) {
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

            _ilg.FreeLocal(flowControlFlag);
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
        private void EmitCatchStart(CatchBlock cb, LocalBuilder saveSlot) {
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

        private static void EmitYieldStatement(LambdaCompiler lc, Expression expr) {
            YieldStatement node = (YieldStatement)expr;
            lc.EmitYield(node.Expression, lc.GetYieldTarget(node));
        }
    }
}
