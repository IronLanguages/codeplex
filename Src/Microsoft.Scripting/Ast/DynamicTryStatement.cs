/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
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
    public class DynamicTryStatement : Statement {
        private readonly SourceLocation _header;
        private readonly Statement _body;
        private readonly DynamicTryStatementHandler[] _handlers;
        private readonly Statement _else;
        private readonly Statement _finally;

        private TargetLabel _target;        // entry to the try statement

        private List<YieldTarget> _tryTargets = null;
        private List<YieldTarget> _catchTargets = null;
        private List<YieldTarget> _finallyTargets = null;
        private List<YieldTarget> _elseTargets = null;

        /// <summary>
        /// Creates a try/catch/finally/else block.
        /// 
        /// The body is protected by the try block.
        /// The handlers consist of a set of language-dependent tests which call into the LanguageContext.
        /// The elseSuite runs if no exception is thrown.
        /// The finallySuite runs regardless of how control exits the body.
        /// </summary>
        public DynamicTryStatement(Statement body, DynamicTryStatementHandler[] handlers, Statement elseSuite, Statement finallySuite, SourceSpan span, SourceLocation header)
            : base(span) {
            _body = body;
            _handlers = handlers;
            _else = elseSuite;
            _finally = finallySuite;
            _header = header;
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public Statement Body {
            get { return _body; }
        }

        public IList<DynamicTryStatementHandler> Handlers {
            get { return _handlers; }
        }

        public Statement ElseStatement {
            get { return _else; }
        }

        public Statement FinallyStatement {
            get { return _finally; }
        }

        internal void AddTryYieldTarget(TargetLabel target, int index) {
            AddYieldTarget(ref _tryTargets, target, index);
        }

        internal void AddElseYieldTarget(TargetLabel target, int index) {
            AddYieldTarget(ref _elseTargets, target, index);
        }

        internal void AddCatchYieldTarget(TargetLabel target, int index) {
            AddYieldTarget(ref _catchTargets, target, index);
        }

        internal void AddFinallyYieldTarget(TargetLabel target, int index) {
            AddYieldTarget(ref _finallyTargets, target, index);
        }

        private void AddYieldTarget(ref List<YieldTarget> list, TargetLabel target, int index) {
            if (list == null) {
                list = new List<YieldTarget>();
            }
            list.Add(new YieldTarget(index, target));
        }

        internal TargetLabel EnsureTopTarget() {
            if (_target == null) {
                _target = new TargetLabel();
            }
            return _target;
        }

        private bool YieldInBlock(List<YieldTarget> block) {
            return block != null && block.Count > 0;
        }

        internal int GetGeneratorTempCount() {
            // cachedException is always needed
            int temps = 1;

            // Codegen is affected by presence/absence of loop control statements
            // (break/continue) or return statement in finally clause
            bool loopControl;
            bool flowControl;

            TryFlowAnalyzer.Analyze(FinallyStatement, out flowControl, out loopControl);

            if (YieldInBlock(_finallyTargets) || flowControl) {
                temps += 1;
            }

            return temps;
        }

        private void EmitYieldDispatch(List<YieldTarget> targets, CodeGen cg) {
            if (YieldInBlock(targets)) {
                Debug.Assert(cg.GotoRouter != null);

                // TODO: Emit as switch!
                foreach (YieldTarget yt in targets) {
                    cg.GotoRouter.EmitGet(cg);
                    cg.EmitInt(yt.Index);
                    cg.Emit(OpCodes.Beq, yt.EnsureLabel(cg));
                }
            }
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, _header);

            Slot cachedException = cg.GetTemporarySlot(typeof(Exception)); ;
            Slot rethrow = null;

            // If there's an else block, initialize the "entered else" flag to false
            Slot enteredElse = null;
            if (_else != null) {
                enteredElse = cg.GetLocalTmp(typeof(bool));
                cg.EmitBoolean(false);
                enteredElse.EmitSet(cg);
            }

            // Codegen is affected by presence/absence of loop control statements
            // (break/continue) or return statement in finally clause
            bool loopControl;
            bool flowControl;

            TryFlowAnalyzer.Analyze(FinallyStatement, out flowControl, out loopControl);

            // Initialize exception rethrow logic, if needed
            if (YieldInBlock(_finallyTargets) || flowControl) {
                rethrow = cg.GetTemporarySlot(typeof(bool));
                cg.EmitNull();
                cachedException.EmitSet(cg);
                cg.EmitBoolean(true);
                rethrow.EmitSet(cg);
            }

            /******************************************************************/
            // All of the above happens on DEFAULT approach to the try block
            // The yield dispatch will route the code to the label below, so
            // during the yield dispatch all of the above code will be skipped.
            /******************************************************************/

            if (_target != null) {
                cg.MarkLabel(_target.EnsureLabel(cg));
            }

            // Initialize the flow control flag
            Slot flowControlFlag = cg.GetLocalTmp(typeof(int));
            cg.EmitInt(CodeGen.FinallyExitsNormally);
            flowControlFlag.EmitSet(cg);

            // Begin the exception block - "try"
            cg.PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
            Label exceptionEnd = cg.BeginExceptionBlock();

            /******************************************************************/
            // ENTERING TRY
            /******************************************************************/

            /******************************************************************/
            // 1. The dispatch within try
            //    - can happen directly via gotos
            /******************************************************************/

            EmitYieldDispatch(_tryTargets, cg);

            /******************************************************************/
            // 2. The Dispatch to else clause
            //    - turn the "enteredElse on to start, but
            //      if we fall through, turn the flag off
            //  (alternatively we can customize EmitYieldDispatch)
            /******************************************************************/
            if (_else != null && YieldInBlock(_elseTargets)) {
                cg.EmitBoolean(true);
                enteredElse.EmitSet(cg);

                EmitYieldDispatch(_elseTargets, cg);

                cg.EmitBoolean(false);
                enteredElse.EmitSet(cg);
            }

            /******************************************************************/
            // 3. The Dispatch into labels within catch
            //    - this happen via rethrowing the earlier exception
            /******************************************************************/

            if (YieldInBlock(_catchTargets)) {
                Label throwLabel = cg.DefineLabel();

                foreach (YieldTarget yt in _catchTargets) {
                    cg.GotoRouter.EmitGet(cg);
                    cg.EmitInt(yt.Index);
                    cg.Emit(OpCodes.Beq, throwLabel);
                }

                Label noMatch = cg.DefineLabel();
                cg.Emit(OpCodes.Br_S, noMatch);

                // Throw the exception to enter _catch block again
                cg.MarkLabel(throwLabel);
                cachedException.EmitGet(cg);
                cg.Emit(OpCodes.Throw);

                cg.MarkLabel(noMatch);
            }

            Label endOfTry = cg.DefineLabel();

            /******************************************************************/
            // 4. Dispatch into finally
            //    - this happens via skipping try and letting code run
            /******************************************************************/
            if (YieldInBlock(_finallyTargets)) {
                foreach (YieldTarget yt in _finallyTargets) {
                    cg.GotoRouter.EmitGet(cg);
                    cg.EmitInt(yt.Index);
                    cg.Emit(OpCodes.Beq, endOfTry);
                }
            }

            /******************************************************************/
            // 5. Emit the try statement body
            /******************************************************************/

            _body.Emit(cg);
            cg.EmitSequencePointNone();

            /******************************************************************/
            // 6. Emit the else clause
            /******************************************************************/

            if (_else != null) {
                cg.PopTargets(TargetBlockType.Try);
                cg.PushExceptionBlock(TargetBlockType.Else, flowControlFlag);

                cg.EmitBoolean(true);
                enteredElse.EmitSet(cg);

                _else.Emit(cg);

                cg.PopTargets(TargetBlockType.Else);
                cg.PushExceptionBlock(TargetBlockType.Try, flowControlFlag);

                cg.EmitSequencePointNone();
            }

            /******************************************************************/
            // 7. This is the end of try
            /******************************************************************/

            cg.MarkLabel(endOfTry);

            /******************************************************************/
            // 8. Start the catch block
            /******************************************************************/

            if (_handlers != null || YieldInBlock(_finallyTargets) || flowControl) {
                cg.BeginCatchBlock(typeof(Exception));

                // If entering catch due to an exception in else block, rethrow
                // TODO: This is not quite correct, the exception needs to be re-thrown
                //       at the end of the finally
                if (_else != null) {
                    Label end = cg.DefineLabel();
                    enteredElse.EmitGet(cg);
                    cg.Emit(OpCodes.Brfalse, end);
                    cg.Emit(OpCodes.Rethrow);
                    cg.MarkLabel(end);
                }

                // Save the exception for future use
                cachedException.EmitSet(cg);

                if (_handlers != null) {
                    cg.PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);

                    cg.PushTryBlock();
                    cg.BeginExceptionBlock();

                    cg.EmitCodeContext();
                    cachedException.EmitGet(cg);
                    cg.EmitCall(typeof(RuntimeHelpers), "PushExceptionHandler");

                    Slot extracted = cg.GetLocalTmp(typeof(object));
                    extracted.EmitSet(cg);

                    // Dispatch to the labels within the catch block
                    EmitYieldDispatch(_catchTargets, cg);

                    /******************************************************************/
                    // Emit the actual exception handling
                    /******************************************************************/

                    foreach (DynamicTryStatementHandler handler in _handlers) {
                        cg.EmitPosition(handler.Start, handler.Header);
                        Label next = cg.DefineLabel();
                        if (handler.Test != null) {
                            cg.EmitCodeContext();
                            extracted.EmitGet(cg);
                            handler.Test.EmitAsObject(cg);
                            cg.EmitCall(typeof(RuntimeHelpers), "CheckException");

                            Slot tmpExc = null;
                            if (handler.Variable != null) {
                                tmpExc = cg.GetLocalTmp(typeof(object));
                                tmpExc.EmitSet(cg);
                                tmpExc.EmitGet(cg);
                            }

                            cg.EmitNull();
                            cg.Emit(OpCodes.Ceq);
                            cg.Emit(OpCodes.Brtrue, next);

                            if (handler.Variable != null) {
                                Debug.Assert(tmpExc != null);
                                handler.Slot.EmitSet(cg, tmpExc);
                            }
                            cg.FreeLocalTmp(tmpExc);
                        } else {
                            if (handler.Variable != null) {
                                handler.Slot.EmitSet(cg, extracted);
                            }
                        }

                        // This handler handles the exception, therefore
                        // don't rethrow it at the end of finally
                        if (rethrow != null) {
                            cg.EmitBoolean(false);
                            rethrow.EmitSet(cg);
                        }

                        handler.Body.Emit(cg);

                        cg.Emit(OpCodes.Leave, exceptionEnd);
                        cg.MarkLabel(next);
                    }

                    cg.PopTargets();
                    cg.BeginFinallyBlock();

                    cg.EmitCodeContext();
                    cg.EmitCall(typeof(RuntimeHelpers), "PopExceptionHandler");

                    cg.EndExceptionBlock();

                    cg.FreeLocalTmp(extracted);

                    cg.Emit(OpCodes.Rethrow);
                    cg.PopTargets(TargetBlockType.Catch);
                }
            }

            if (_finally != null) {
                cg.PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);
                cg.BeginFinallyBlock();

                Label endOfFinally = cg.DefineLabel();

                // If yielding in the generator, skip finally body
                if (cg.IsGenerator) {
                    cg.GotoRouter.EmitGet(cg);
                    cg.EmitInt(CodeGen.GotoRouterYielding);
                    cg.Emit(OpCodes.Beq, endOfFinally);
                }

                // Dispatch to the labels within the finally block
                EmitYieldDispatch(_finallyTargets, cg);

                /******************************************************************/
                // Emit the finally clause body
                /******************************************************************/

                _finally.Emit(cg);

                /******************************************************************/
                // Rethrow the cached exception (if any)
                //  - unless the exception was handled (rethrow == false)
                //    (the actual cachedException is stored beyond this point
                //     in order to do rethrows as we route execution to yield labels)
                /******************************************************************/

                if (YieldInBlock(_finallyTargets) || flowControl) {
                    Label nothrow1 = cg.DefineLabel();
                    Label nothrow2 = cg.DefineLabel();

                    Debug.Assert(rethrow != null);

                    rethrow.EmitGet(cg);
                    cg.Emit(OpCodes.Brfalse_S, nothrow1);
                    cachedException.EmitGet(cg);
                    cg.Emit(OpCodes.Dup);
                    cg.Emit(OpCodes.Brfalse_S, nothrow2);
                    cg.Emit(OpCodes.Throw);
                    cg.MarkLabel(nothrow2);
                    cg.Emit(OpCodes.Pop);
                    cg.MarkLabel(nothrow1);
                }

                cg.MarkLabel(endOfFinally);
                cg.EndExceptionBlock();
                cg.PopTargets(TargetBlockType.Finally);
            } else {
                cg.EndExceptionBlock();
            }

            cg.PopTargets(TargetBlockType.Try);

            if (cg.IsGenerator || flowControl) {
                Label noReturn = cg.DefineLabel();

                flowControlFlag.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForReturn);
                cg.Emit(OpCodes.Bne_Un, noReturn);

                if (cg.IsGenerator) {
                    // return true from the generator method
                    cg.Emit(OpCodes.Ldc_I4_1);
                    cg.EmitReturn();
                } else if (flowControl) {
                    // return the actual value
                    cg.EmitReturnValue();
                    cg.EmitReturn();
                }
                cg.MarkLabel(noReturn);
            }

            if (loopControl) {
                Label noReturn = cg.DefineLabel();

                noReturn = cg.DefineLabel();
                flowControlFlag.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForBreak);
                cg.Emit(OpCodes.Bne_Un, noReturn);
                cg.EmitBreak();
                cg.MarkLabel(noReturn);

                noReturn = cg.DefineLabel();
                flowControlFlag.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForContinue);
                cg.Emit(OpCodes.Bne_Un, noReturn);
                cg.EmitContinue();
                cg.MarkLabel(noReturn);
            }

            // Clean up the codegen labels
            ClearLabels(_tryTargets);
            ClearLabels(_catchTargets);
            ClearLabels(_finallyTargets);
            ClearLabels(_elseTargets);

            if (_target != null) {
                _target.Clear();
            }

            if (enteredElse != null) {
                cg.FreeLocalTmp(enteredElse);
            }

            // Free the temporaries. If in generator, they will actually
            // not get freed, but in non-generator case, they are safe to go.
            cg.FreeTemporarySlot(cachedException);
            cg.FreeTemporarySlot(rethrow);

            cg.FreeLocalTmp(flowControlFlag);
        }

        private static void ClearLabels(List<YieldTarget> targets) {
            if (targets != null) {
                foreach (YieldTarget yt in targets) {
                    yt.Clear();
                }
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _body.Walk(walker);
                if (_handlers != null)
                    foreach (DynamicTryStatementHandler handler in _handlers)
                        handler.Walk(walker);

                if (_else != null)
                    _else.Walk(walker);

                if (_finally != null)
                    _finally.Walk(walker);

            }
            walker.PostWalk(this);
        }
    }
}
