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

using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    // TODO: Make internal, don't allow direct callers
    public partial class Compiler {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void EmitStatement(Statement node) {
            Debug.Assert(node != null);

            switch (node.NodeType) {
                case AstNodeType.BlockStatement:
                    Emit((BlockStatement)node);
                    break;

                case AstNodeType.BreakStatement:
                    Emit((BreakStatement)node);
                    break;

                case AstNodeType.ContinueStatement:
                    Emit((ContinueStatement)node);
                    break;

                case AstNodeType.DebugStatement:
                    Emit((DebugStatement)node);
                    break;

                case AstNodeType.DeleteStatement:
                    Emit((DeleteStatement)node);
                    break;

                case AstNodeType.DoStatement:
                    Emit((DoStatement)node);
                    break;

                case AstNodeType.EmptyStatement:
                    Emit((EmptyStatement)node);
                    break;

                case AstNodeType.ExpressionStatement:
                    Emit((ExpressionStatement)node);
                    break;

                case AstNodeType.IfStatement:
                    Emit((IfStatement)node);
                    break;

                case AstNodeType.LabeledStatement:
                    Emit((LabeledStatement)node);
                    break;

                case AstNodeType.LoopStatement:
                    Emit((LoopStatement)node);
                    break;

                case AstNodeType.ReturnStatement:
                    Emit((ReturnStatement)node);
                    break;

                case AstNodeType.ScopeStatement:
                    Emit((ScopeStatement)node);
                    break;

                case AstNodeType.SwitchStatement:
                    Emit((SwitchStatement)node);
                    break;

                case AstNodeType.ThrowStatement:
                    Emit((ThrowStatement)node);
                    break;

                case AstNodeType.TryStatement:
                    Emit((TryStatement)node);
                    break;

                case AstNodeType.YieldStatement:
                    Emit((YieldStatement)node);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void Emit(BlockStatement node) {
            _cg.EmitPosition(node.Start, node.End);

            foreach (Statement stmt in node.Statements) {
                EmitStatement(stmt);
            }
        }

        private void Emit(BreakStatement node) {
            _cg.EmitPosition(node.Start, node.End);

            if (node.Statement != null) {
                _cg.CheckAndPushTargets(node.Statement);
            }

            _cg.EmitBreak();

            if (node.Statement != null) {
                _cg.PopTargets();
            }
        }

        private void Emit(ContinueStatement node) {
            _cg.EmitPosition(node.Start, node.End);

            if (node.Statement != null) {
                _cg.CheckAndPushTargets(node.Statement);
            }

            _cg.EmitContinue();

            if (node.Statement != null) {
                _cg.PopTargets();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "node")]
        private void Emit(DebugStatement node) {
            _cg.EmitDebugMarker(node.Marker);
        }

        private void Emit(DeleteStatement node) {
            _cg.EmitPosition(node.Start, node.End);
            node.Ref.Slot.EmitDelete(_cg, node.Variable.Name, !node.IsDefined);
        }

        private void Emit(DoStatement node) {
            Label startTarget = _cg.DefineLabel();
            Label breakTarget = _cg.DefineLabel();
            Label continueTarget = _cg.DefineLabel();

            _cg.MarkLabel(startTarget);
            _cg.PushTargets(breakTarget, continueTarget, node);

            EmitStatement(node.Body);

            _cg.MarkLabel(continueTarget);
            // TODO: Check if we need to emit position somewhere else also.
            _cg.EmitPosition(node.Start, node.Header);

            EmitExpression(node.Test);

            _cg.Emit(OpCodes.Brtrue, startTarget);

            _cg.PopTargets();
            _cg.MarkLabel(breakTarget);
        }

        private void Emit(EmptyStatement node) {
            _cg.EmitPosition(node.Start, node.End);
        }

        private void Emit(ExpressionStatement node) {
            _cg.EmitPosition(node.Start, node.End);
            EmitExpressionAndPop(node.Expression);
        }

        private void Emit(IfStatement node) {
            Label eoi = _cg.DefineLabel();
            foreach (IfStatementTest t in node.Tests) {
                Label next = _cg.DefineLabel();
                _cg.EmitPosition(t.Start, t.Header);

                EmitBranchFalse(t.Test, next);

                EmitStatement(t.Body);

                // optimize no else case                
                _cg.EmitSequencePointNone();     // hide compiler generated branch.
                _cg.Emit(OpCodes.Br, eoi);
                _cg.MarkLabel(next);
            }
            if (node.ElseStatement != null) {
                EmitStatement(node.ElseStatement);
            }
            _cg.MarkLabel(eoi);
        }

        private void Emit(LabeledStatement node) {
            // TODO: Validate in a pass before codegen!!!
            if (node.Statement == null) {
                throw new InvalidOperationException("Incomplete LabelStatement");
            }

            Label label = _cg.DefineLabel();
            _cg.PushTargets(label, label, node);

            EmitStatement(node.Statement);

            _cg.MarkLabel(label);
            _cg.PopTargets();
        }

        private void Emit(LoopStatement node) {
            Nullable<Label> firstTime = null;
            Label eol = _cg.DefineLabel();
            Label breakTarget = _cg.DefineLabel();
            Label continueTarget = _cg.DefineLabel();

            if (node.Increment != null) {
                firstTime = _cg.DefineLabel();
                _cg.Emit(OpCodes.Br, firstTime.Value);
            }

            if (node.Header.IsValid) {
                _cg.EmitPosition(node.Start, node.Header);
            }
            _cg.MarkLabel(continueTarget);

            if (node.Increment != null) {
                EmitExpressionAndPop(node.Increment);
                _cg.MarkLabel(firstTime.Value);
            }

            if (node.Test != null) {
                EmitExpression(node.Test);
                _cg.Emit(OpCodes.Brfalse, eol);
            }

            _cg.PushTargets(breakTarget, continueTarget, node);

            EmitStatement(node.Body);

            _cg.Emit(OpCodes.Br, continueTarget);

            _cg.PopTargets();

            _cg.MarkLabel(eol);
            if (node.ElseStatement != null) {
                EmitStatement(node.ElseStatement);
            }
            _cg.MarkLabel(breakTarget);
        }

        private void Emit(ReturnStatement node) {
            _cg.EmitPosition(node.Start, node.End);
            _cg.EmitReturn(node.Expression);
        }

        private void Emit(ScopeStatement node) {
            Slot tempContext = _cg.ContextSlot;
            Slot newContext = _cg.GetLocalTmp(typeof(CodeContext));

            // TODO: should work with LocalScope
            if (node.Scope != null) {
                EmitExpression(node.Scope);  //Locals dictionary
                _cg.EmitCodeContext();       //CodeContext
                _cg.EmitBoolean(true);       //Visible = true
                _cg.EmitCall(typeof(RuntimeHelpers), "CreateNestedCodeContext");
            } else {
                _cg.EmitCodeContext();
                _cg.EmitCall(typeof(RuntimeHelpers), "CreateCodeContext");
            }

            newContext.EmitSet(_cg);

            _cg.ContextSlot = newContext;

            EmitStatement(node.Body);

            _cg.ContextSlot = tempContext;
        }

        #region SwitchStatement

        private void Emit(SwitchStatement node) {
            _cg.EmitPosition(node.Start, node.Header);

            Label breakTarget = _cg.DefineLabel();
            Label defaultTarget = breakTarget;
            Label[] labels = new Label[node.Cases.Count];

            // Create all labels
            for (int i = 0; i < node.Cases.Count; i++) {
                labels[i] = _cg.DefineLabel();

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
            _cg.Emit(OpCodes.Br, defaultTarget);

            _cg.PushTargets(breakTarget, _cg.BlockContinueLabel, node);

            // Emit the bodies
            for (int i = 0; i < node.Cases.Count; i++) {
                // First put the corresponding labels
                _cg.MarkLabel(labels[i]);
                // And then emit the Body!!
                EmitStatement(node.Cases[i].Body);
            }

            _cg.PopTargets();
            _cg.MarkLabel(breakTarget);
        }

        private const int MaxJumpTableSize = 65536;
        private const double MaxJumpTableSparsity = 10;

        // Emits the switch as if stmts
        private void EmitConditionalBranches(SwitchStatement node, Label[] labels) {
            Slot testValueSlot = _cg.GetNamedLocal(typeof(int), "switchTestValue");
            testValueSlot.EmitSet(_cg);

            // For all the "cases" create their conditional branches
            for (int i = 0; i < node.Cases.Count; i++) {
                // Not default case emit the condition
                if (!node.Cases[i].IsDefault) {
                    // Test for equality of case value and the test expression
                    _cg.EmitInt(node.Cases[i].Value);
                    testValueSlot.EmitGet(_cg);
                    _cg.Emit(OpCodes.Beq, labels[i]);
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
                _cg.EmitInt(min);
                _cg.Emit(OpCodes.Sub);
            }
            _cg.Emit(OpCodes.Switch, jmpLabels);
            return true;
        }

        #endregion

        private void Emit(ThrowStatement node) {
            _cg.EmitPosition(node.Start, node.End);
            if (node.Value == null) {
                _cg.Emit(OpCodes.Rethrow);
            } else {
                EmitExpression(node.Value);
                _cg.Emit(OpCodes.Throw);
            }
        }

        #region TryStatement

        private void Emit(TryStatement node) {
            // Codegen is affected by presence/absence of loop control statements
            // (break/continue) or return/yield statement in finally clause
            TryFlowResult flow = TryFlowAnalyzer.Analyze(node.FinallyStatement);

            _cg.EmitPosition(node.Start, node.Header);

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
                flowControlFlag = _cg.GetLocalTmp(typeof(int));
                _cg.EmitInt(CodeGen.FinallyExitsNormally);
                flowControlFlag.EmitSet(_cg);
            }

            Slot exception = null;
            if (node.FinallyStatement != null) {
                exception = _cg.GetTemporarySlot(typeof(Exception));
                _cg.EmitNull();
                exception.EmitSet(_cg);
            }

            //******************************************************************
            // Entering the try block
            //******************************************************************

            if (node.Target != null) {
                _cg.MarkLabel(node.Target.EnsureLabel(_cg));
            }

            //******************************************************************
            // If we have a 'finally', transform it into try..catch..finally
            // and rethrow
            //******************************************************************
            Label endFinallyBlock = new Label();
            if (node.FinallyStatement != null) {
                _cg.PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
                _cg.BeginExceptionBlock();
                endFinallyBlock = _cg.DefineLabel();

                //**************************************************************
                // If there is a yield in any catch, that catch will be hoisted
                // and we need to dispatch to it from here
                //**************************************************************
                if (node.YieldInCatch) {
                    EmitYieldDispatch(node.CatchYields, _cg);
                }

                if (YieldInBlock(node.FinallyYields)) {
                    foreach (YieldTarget yt in node.FinallyYields) {
                        _cg.GotoRouter.EmitGet(_cg);
                        _cg.EmitInt(yt.Index);
                        _cg.Emit(OpCodes.Beq, endFinallyBlock);
                    }
                }
            }

            //******************************************************************
            // If we have a 'catch', start a try block to handle all the catches
            //******************************************************************

            Label endCatchBlock = new Label();
            if (HaveHandlers(node)) {
                _cg.PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
                endCatchBlock = _cg.BeginExceptionBlock();
            }

            //******************************************************************
            // Emit the try block body
            //******************************************************************

            // First, emit the dispatch within the try block
            EmitYieldDispatch(node.TryYields, _cg);

            // Then, emit the actual body
            EmitStatement(node.Body);
            _cg.EmitSequencePointNone();

            //******************************************************************
            // Emit the catch blocks
            //******************************************************************

            if (HaveHandlers(node)) {
                List<CatchRecord> catches = new List<CatchRecord>();
                _cg.PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);

                foreach (CatchBlock cb in node.Handlers) {
                    _cg.BeginCatchBlock(cb.Test);

                    if (cb.Yield) {
                        // The catch block body contains yield, therefore
                        // delay the body emit till after the try block.
                        Slot slot = _cg.GetLocalTmp(cb.Test);
                        slot.EmitSet(_cg);
                        catches.Add(new CatchRecord(slot, cb));
                    } else {
                        // Save the exception (if the catch block asked for it) or pop
                        EmitSaveExceptionOrPop(_cg, cb);
                        // Emit the body right now, since it doesn't contain yield
                        EmitStatement(cb.Body);
                    }
                }

                _cg.PopTargets(TargetBlockType.Catch);
                _cg.EndExceptionBlock();
                _cg.PopTargets(TargetBlockType.Try);

                //******************************************************************
                // Emit the postponed catch block bodies (with yield in them)
                //******************************************************************
                foreach (CatchRecord cr in catches) {
                    Label next = _cg.DefineLabel();
                    cr.Slot.EmitGet(_cg);
                    _cg.EmitNull();
                    _cg.Emit(OpCodes.Beq, next);

                    if (cr.Block.Slot != null) {
                        cr.Block.Slot.EmitSet(_cg, cr.Slot);
                    }

                    _cg.FreeLocalTmp(cr.Slot);
                    EmitStatement(cr.Block.Body);
                    _cg.MarkLabel(next);
                    _cg.EmitSequencePointNone();
                }
            }

            //******************************************************************
            // Emit the finally body
            //******************************************************************

            if (node.FinallyStatement != null) {
                _cg.MarkLabel(endFinallyBlock);
                _cg.PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);
                _cg.BeginCatchBlock(typeof(Exception));
                exception.EmitSet(_cg);

                _cg.PopTargets(TargetBlockType.Catch);

                _cg.PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);
                _cg.BeginFinallyBlock();

                Label noExit = _cg.DefineLabel();
                _cg.GotoRouter.EmitGet(_cg);
                _cg.EmitInt(CodeGen.GotoRouterYielding);
                _cg.Emit(OpCodes.Bne_Un_S, noExit);
                _cg.Emit(OpCodes.Endfinally);
                _cg.MarkLabel(noExit);

                EmitYieldDispatch(node.FinallyYields, _cg);

                // Emit the finally body

                EmitStatement(node.FinallyStatement);

                // Rethrow the exception, if any

                Label noThrow = _cg.DefineLabel();
                exception.EmitGet(_cg);
                _cg.EmitNull();
                _cg.Emit(OpCodes.Beq, noThrow);
                exception.EmitGet(_cg);
                _cg.Emit(OpCodes.Throw);
                _cg.MarkLabel(noThrow);
                _cg.FreeLocalTmp(exception);

                _cg.EndExceptionBlock();
                _cg.PopTargets(TargetBlockType.Finally);
                _cg.PopTargets(TargetBlockType.Try);

                //
                // Emit the flow control for finally, if there was any.
                //
                EmitFinallyFlowControl(_cg, flow, flowControlFlag);

                _cg.EmitSequencePointNone();
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

        private static void EmitSaveExceptionOrPop(CodeGen cg, CatchBlock cb) {
            if (cb.Variable != null) {
                Debug.Assert(cb.Slot != null);
                // If the variable is present, store the exception
                // in the variable.
                cb.Slot.EmitSet(cg);
            } else {
                // Otherwise, pop it off the stack.
                cg.Emit(OpCodes.Pop);
            }
        }

        private static void EmitYieldDispatch(List<YieldTarget> targets, CodeGen _cg) {
            if (YieldInBlock(targets)) {
                Debug.Assert(_cg.GotoRouter != null);

                // TODO: Emit as switch!
                foreach (YieldTarget yt in targets) {
                    _cg.GotoRouter.EmitGet(_cg);
                    _cg.EmitInt(yt.Index);
                    _cg.Emit(OpCodes.Beq, yt.EnsureLabel(_cg));
                }
            }
        }

        /// <summary>
        /// If the finally statement contains break, continue, return or yield, we need to
        /// handle the control flow statement after we exit out of finally via OpCodes.Endfinally.
        /// </summary>
        private static void EmitFinallyFlowControl(CodeGen cg, TryFlowResult flow, Slot flag) {
            if (flow.Return || flow.Yield) {
                Debug.Assert(flag != null);

                Label noReturn = cg.DefineLabel();

                flag.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForReturn);
                cg.Emit(OpCodes.Bne_Un, noReturn);

                if (cg.IsGenerator) {
                    // return true from the generator method
                    cg.Emit(OpCodes.Ldc_I4_1);
                    cg.EmitReturn();
                } else if (flow.Any) {
                    // return the actual value
                    cg.EmitReturnValue();
                    cg.EmitReturn();
                }
                cg.MarkLabel(noReturn);
            }

            // Only emit break handling if it is actually needed
            if (flow.Break) {
                Debug.Assert(flag != null);

                Label noReturn = cg.DefineLabel();
                flag.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForBreak);
                cg.Emit(OpCodes.Bne_Un, noReturn);
                cg.EmitBreak();
                cg.MarkLabel(noReturn);
            }

            // Only emit continue handling if it if actually needed
            if (flow.Continue) {
                Debug.Assert(flag != null);

                Label noReturn = cg.DefineLabel();
                flag.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForContinue);
                cg.Emit(OpCodes.Bne_Un, noReturn);
                cg.EmitContinue();
                cg.MarkLabel(noReturn);
            }
        }

        private void EmitSimpleTry(TryStatement node, TryFlowResult flow) {
            //
            // Initialize the flow control flag
            //
            Slot flowControlFlag = null;
            if (flow.Any) {
                Debug.Assert(node.FinallyStatement != null);

                flowControlFlag = _cg.GetLocalTmp(typeof(int));
                _cg.EmitInt(CodeGen.FinallyExitsNormally);
                flowControlFlag.EmitSet(_cg);

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
                    _cg.PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
                    _cg.BeginExceptionBlock();
                }
            }

            //******************************************************************
            // 1. ENTERING TRY
            //******************************************************************

            _cg.PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
            _cg.BeginExceptionBlock();

            //******************************************************************
            // 2. Emit the try statement body
            //******************************************************************

            EmitStatement(node.Body);
            _cg.EmitSequencePointNone();

            //******************************************************************
            // 3. Emit the catch blocks
            //******************************************************************

            if (HaveHandlers(node)) {
                _cg.PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);

                foreach (CatchBlock cb in node.Handlers) {
                    // Begin the strongly typed exception block
                    _cg.BeginCatchBlock(cb.Test);

                    // Save the exception (if the catch block asked for it) or pop
                    EmitSaveExceptionOrPop(_cg, cb);

                    //
                    // Emit the catch block body
                    //
                    EmitStatement(cb.Body);
                }

                _cg.PopTargets(TargetBlockType.Catch);
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
                        _cg.EndExceptionBlock();
                        _cg.PopTargets(TargetBlockType.Try);
                    }

                    _cg.PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);
                    _cg.BeginCatchBlock(typeof(Exception));

                    rethrow = _cg.GetLocalTmp(typeof(Exception));
                    rethrow.EmitSet(_cg);

                    _cg.PopTargets(TargetBlockType.Catch);
                }

                _cg.PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);
                _cg.BeginFinallyBlock();

                //
                // Emit the finally block body
                //
                EmitStatement(node.FinallyStatement);

                if (flow.Any) {
                    Debug.Assert(rethrow != null);
                    Label noRethrow = _cg.DefineLabel();

                    rethrow.EmitGet(_cg);
                    _cg.EmitNull();
                    _cg.Emit(OpCodes.Beq, noRethrow);
                    rethrow.EmitGet(_cg);
                    _cg.Emit(OpCodes.Throw);
                    _cg.MarkLabel(noRethrow);
                }

                _cg.EndExceptionBlock();
                _cg.PopTargets(TargetBlockType.Finally);
            } else {
                _cg.EndExceptionBlock();
            }

            _cg.PopTargets(TargetBlockType.Try);

            //
            // Emit the flow control for finally, if there was any.
            //
            EmitFinallyFlowControl(_cg, flow, flowControlFlag);

            _cg.FreeLocalTmp(flowControlFlag);
        }

        #endregion

        private void Emit(YieldStatement node) {
            _cg.EmitPosition(node.Start, node.End);
            _cg.EmitYield(node.Expression, node.Target);
        }
    }
}
