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
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class TryStatement : Statement {
        private readonly SourceLocation _header;
        private readonly Statement _body;
        private readonly TryStatementHandler[] _handlers;
        private readonly Statement _else;
        private readonly Statement _finally;

        private List<YieldTarget> _tryTargets = null;
        private List<YieldTarget> _catchTargets = null;
        private List<YieldTarget> _finallyTargets = null;
        private List<YieldTarget> _elseTargets = null;

        public const int GeneratorTemps = 6;

        /// <summary>
        /// Creates a try/catch/finally/else block.
        /// 
        /// The body is protected by the try block.
        /// The handlers consist of a set of language-dependent tests which call into the LanguageContext.
        /// The elseSuite runs if no exception is thrown.
        /// The finallySuite runs regardless of how control exits the body.
        /// </summary>
        public TryStatement(Statement body, TryStatementHandler[] handlers, Statement elseSuite, Statement finallySuite, SourceSpan span, SourceLocation header)
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

        public IList<TryStatementHandler> Handlers {
            get { return _handlers; }
        }

        public Statement ElseStatement {
            get { return _else; }
        }

        public Statement FinallyStatement {
            get { return _finally; }
        }

        internal IList<YieldTarget> TryYieldTargets {
            get { return _tryTargets; }
        }

        internal void AddTryYieldTarget(YieldTarget target) {
            if (_tryTargets == null)
                _tryTargets = new List<YieldTarget>();
            _tryTargets.Add(target);
        }

        internal IList<YieldTarget> ElseYieldTargets {
            get { return _elseTargets; }
        }

        internal void AddElseYieldTarget(YieldTarget target) {
            if (_elseTargets == null)
                _elseTargets = new List<YieldTarget>();

            _elseTargets.Add(target);
        }

        internal IList<YieldTarget> CatchYieldTargets {
            get { return _catchTargets; }
        }

        internal void AddCatchYieldTarget(YieldTarget target) {
            if (_catchTargets == null)
                _catchTargets = new List<YieldTarget>();

            _catchTargets.Add(target);
        }

        internal IList<YieldTarget> FinallyYieldTargets {
            get { return _finallyTargets; }
        }

        internal void AddFinallyYieldTarget(YieldTarget target) {
            if (_finallyTargets == null)
                _finallyTargets = new List<YieldTarget>();

            _finallyTargets.Add(target);
        }

        private bool IsBlockYieldable(List<YieldTarget> block) {
            if (block != null && block.Count > 0)
                return true;
            return false;
        }

        private void EmitTopYieldTargetLabels(List<YieldTarget> yieldTargets, Slot choiceVar, CodeGen cg) {
            if (IsBlockYieldable(yieldTargets)) {
                Label label = cg.DefineLabel();
                cg.EmitInt(-1);
                choiceVar.EmitSet(cg);
                cg.Emit(OpCodes.Br, label);

                int index = 0;
                foreach (YieldTarget yt in yieldTargets) {
                    cg.MarkLabel(yt.TopBranchTarget);
                    cg.EmitInt(index++);
                    choiceVar.EmitSet(cg);
                    cg.Emit(OpCodes.Br, label);
                }
                cg.MarkLabel(label);
            }
        }

        private void EmitYieldDispatch(List<YieldTarget> yieldTargets, Slot isYielded, Slot choiceVar, CodeGen cg) {
            if (IsBlockYieldable(yieldTargets)) {
                cg.Emit(OpCodes.Ldc_I4_0);
                isYielded.EmitSet(cg);

                int index = 0;
                foreach (YieldTarget yt in yieldTargets) {
                    choiceVar.EmitGet(cg);
                    cg.EmitInt(index);
                    cg.Emit(OpCodes.Beq, yt.YieldContinuationTarget);
                    index++;
                }
            }
        }

        private void EmitOnYieldBranchToLabel(List<YieldTarget> yieldTargets, Slot isYielded, Label label, CodeGen cg) {
            if (IsBlockYieldable(yieldTargets)) {
                isYielded.EmitGet(cg);
                cg.Emit(OpCodes.Brtrue, label);
            }
        }

        // codegen algorithm for unified try-catch-else-finally

        //    isTryYielded = false
        //    isCatchYielded = false
        //    isFinallyYielded = false
        //    isElseYielded = false
        //    Set up the labels for Try Yield Targets
        //    Set up the labels for Catch Yield Targets
        //    Set up the labels for Else Yield Targets
        //    Set up the labels for Finally Yield Targets
        //    returnVar = false
        //    isElseBlock = false

        //TRY:
        //    if isCatchYielded :
        //        rethow  storedException
        //    if finallyYielded :
        //        goto endOfTry
        //    if isElseYielded :
        //        goto beginElseBlock
        //    if isTryYielded:
        //        isTryYielded = false
        //        goto desired_label_in_TRY-BODY
        //    TRY-BODY
        //  beginElseBlock: # Note we are still under TRY
        //    isElseBlock = true
        //    if isElseYielded :
        //        isElseYielded  = false
        //        goto desired_label_in_ELSE-BODY
        //    ELSE-BODY
        //  endOfTry:
        //EXCEPT Exception, $e: # catches any exception
        //    if isElseBlock:
        //        rethrow
        //    try:
        //        pyExc = RuntimeHelpers.PushExceptionHandler(context$, $e)
        //        exObj = RuntimeHelpers.CheckException(context$, pyExc, handler[0].Test) # exObj exposed to user
        //        if exObj != None:
        //            if isCatchYielded :
        //                isCatchYielded  = false
        //                goto desired_label_in_HANDLER-BODY
        //            HANDLER-BODY
        //            Leave afterFinally
        //        exObj = RuntimeHelpers.CheckException(context$, pyExc, handler[1].Test)    # exObj exposed to user
        //        if exObj != None:
        //            if isCatchYielded :
        //                isCatchYielded  = false
        //                goto desired_label_in_HANDLER-BODY
        //            HANDLER-BODY
        //            Leave afterFinally
        //    finally:
        //        RuntimeHelpers.PopExceptionHandler(context$)
        //    .
        //    .
        //    .
        //    Rethrow
        //FINALLY:
        //    if (isTryYielded  or isCatchYielded  or isElseYielded ):
        //        goto endOfFinally
        //    if isFinallyYielded :
        //        isFinallyYielded  = false
        //        goto desired_label_in_FINALLY-BODY
        //    FINALLY-BODY
        //  endOfFinally:
        // #try-cathch-finally ends here
        // afterFinally:
        //    if not returnVar :
        //      goto noReturn
        //    if (finally may yield ):
        //          return 1
        //    else
        //          return appropriate_return_value
        //  noReturn:


        public override void Emit(CodeGen cg) {
            // Add locals
            Slot tryYieldSlot = cg.GetTemporarySlot("try_yield", typeof(int));
            Slot catchYieldSlot = cg.GetTemporarySlot("catch_yield", typeof(int));
            Slot finallyYieldSlot = cg.GetTemporarySlot("finally_yield", typeof(int));
            Slot elseYieldSlot = cg.GetTemporarySlot("else_yield", typeof(int));
            Slot extracted = cg.GetTemporarySlot("extracted", typeof(object));
            Slot exceptionSlot = cg.GetTemporarySlot("exc", typeof(Exception));

            // local slots
            Slot tryChoiceVar = null;
            Slot catchChoiceVar = null;
            Slot elseChoiceVar = null;
            Slot finallyChoiceVar = null;

            Slot flowControlVar = cg.GetLocalTmp(typeof(int));
            Slot isElseBlock = null;

            cg.EmitPosition(Start, _header);

            if (IsBlockYieldable(_tryTargets)) {
                tryChoiceVar = cg.GetLocalTmp(typeof(int));
                cg.Emit(OpCodes.Ldc_I4_0);
                tryYieldSlot.EmitSet(cg);
            }

            if (IsBlockYieldable(_catchTargets)) {
                catchChoiceVar = cg.GetLocalTmp(typeof(int));
                cg.Emit(OpCodes.Ldc_I4_0);
                catchYieldSlot.EmitSet(cg);
            }

            if (IsBlockYieldable(_finallyTargets)) {
                finallyChoiceVar = cg.GetLocalTmp(typeof(int));
                cg.Emit(OpCodes.Ldc_I4_0);
                finallyYieldSlot.EmitSet(cg);
            }

            if (IsBlockYieldable(_elseTargets)) {
                elseChoiceVar = cg.GetLocalTmp(typeof(int));
                cg.Emit(OpCodes.Ldc_I4_0);
                elseYieldSlot.EmitSet(cg);
            }

            if (_else != null) {
                isElseBlock = cg.GetLocalTmp(typeof(bool));
                cg.EmitInt(0);
                isElseBlock.EmitSet(cg);
            }

            bool foundLoopControl;
            bool returnInFinally = ControlFlowFinder.FindControlFlow(FinallyStatement, out foundLoopControl);

            if (IsBlockYieldable(_finallyTargets)) {
                cg.Emit(OpCodes.Ldnull);
                exceptionSlot.EmitSet(cg);
            } else if (returnInFinally) {
                cg.Emit(OpCodes.Ldnull);
                exceptionSlot.EmitSet(cg);
            }

            EmitTopYieldTargetLabels(_tryTargets, tryChoiceVar, cg);
            EmitTopYieldTargetLabels(_catchTargets, catchChoiceVar, cg);
            EmitTopYieldTargetLabels(_elseTargets, elseChoiceVar, cg);
            EmitTopYieldTargetLabels(_finallyTargets, finallyChoiceVar, cg);


            cg.EmitInt(CodeGen.FinallyExitsNormally);
            flowControlVar.EmitSet(cg);

            Label afterFinally = cg.DefineLabel();

            cg.PushExceptionBlock(TargetBlockType.Try, flowControlVar, tryYieldSlot);
            cg.BeginExceptionBlock();

            // if catch yielded, rethow the storedException to be handled by Catch block
            if (IsBlockYieldable(_catchTargets)) {
                Label testFinally = cg.DefineLabel();
                catchYieldSlot.EmitGet(cg);
                cg.Emit(OpCodes.Brfalse, testFinally);

                exceptionSlot.EmitGet(cg);
                cg.Emit(OpCodes.Throw);
                
                cg.MarkLabel(testFinally);
            }

            // if Finally yielded, Branch to the end of Try block
            Label endOfTry = cg.DefineLabel();
            EmitOnYieldBranchToLabel(_finallyTargets, finallyYieldSlot, endOfTry, cg);

            Label beginElseBlock = cg.DefineLabel();
            if (IsBlockYieldable(_elseTargets)) {
                // isElseYielded ?
                Debug.Assert(elseYieldSlot != null);
                elseYieldSlot.EmitGet(cg);
                cg.Emit(OpCodes.Brtrue, beginElseBlock);
            }


            EmitYieldDispatch(_tryTargets, tryYieldSlot, tryChoiceVar, cg);

            _body.Emit(cg);

            cg.EmitSequencePointNone();

            if (_else != null) {
                if (IsBlockYieldable(_elseTargets)) {
                    cg.MarkLabel(beginElseBlock);
                }

                cg.PopTargets(TargetBlockType.Try);
                cg.PushExceptionBlock(TargetBlockType.Else, flowControlVar, elseYieldSlot);

                cg.EmitInt(1);
                isElseBlock.EmitSet(cg);

                EmitYieldDispatch(_elseTargets, elseYieldSlot, elseChoiceVar, cg);

                _else.Emit(cg);
                cg.PopTargets(TargetBlockType.Else);
                cg.PushExceptionBlock(TargetBlockType.Try, flowControlVar, tryYieldSlot);

                cg.EmitSequencePointNone();
            }

            cg.MarkLabel(endOfTry);            

            // get the exception if there is a yield / return  in finally
            if (IsBlockYieldable(_finallyTargets) || returnInFinally) {
                cg.BeginCatchBlock(typeof(Exception));
                exceptionSlot.EmitSet(cg);
            }

            if (_handlers != null) {
                cg.PushExceptionBlock(TargetBlockType.Catch, flowControlVar, catchYieldSlot);
                if (IsBlockYieldable(_finallyTargets) || returnInFinally) {
                    exceptionSlot.EmitGet(cg);
                } else {
                    cg.BeginCatchBlock(typeof(Exception));
                }


                // if in Catch block due to exception in else block -> just rethrow
                if (_else != null) {
                    Label beginCatchBlock = cg.DefineLabel();
                    isElseBlock.EmitGet(cg);
                    cg.Emit(OpCodes.Brfalse, beginCatchBlock);
                    cg.Emit(OpCodes.Rethrow);
                    cg.MarkLabel(beginCatchBlock);
                }

                // Extract state from the carrier exception
                exceptionSlot.EmitSet(cg);

                cg.PushTryBlock(null);
                cg.BeginExceptionBlock();

                cg.EmitCodeContext();
                exceptionSlot.EmitGet(cg);
                cg.EmitCall(typeof(RuntimeHelpers), "PushExceptionHandler");

                Slot tmpExc = cg.GetLocalTmp(typeof(object));
                cg.Emit(OpCodes.Dup);
                tmpExc.EmitSet(cg);
                extracted.EmitSet(cg);

                foreach (TryStatementHandler handler in _handlers) {
                    cg.EmitPosition(handler.Start, handler.Header);
                    Label next = cg.DefineLabel();
                    if (handler.Test != null) {
                        
                        cg.EmitCodeContext();
                        extracted.EmitGet(cg);
                        handler.Test.Emit(cg);
                        cg.EmitCall(typeof(RuntimeHelpers), "CheckException");

                        if (handler.Target != null) {
                            tmpExc.EmitSet(cg);
                            tmpExc.EmitGet(cg);
                        }
                        cg.EmitNull();
                        cg.Emit(OpCodes.Ceq);
                        cg.Emit(OpCodes.Brtrue, next);
                    }

                    if (handler.Target != null) {
                        tmpExc.EmitGet(cg);
                        handler.Target.Slot.EmitSet(cg);
                    }

                    if (IsBlockYieldable(_finallyTargets) || returnInFinally) {
                        cg.Emit(OpCodes.Ldnull);
                        exceptionSlot.EmitSet(cg);
                    }

                    EmitYieldDispatch(_catchTargets, catchYieldSlot, catchChoiceVar, cg);
                    handler.Body.Emit(cg);

                    cg.Emit(OpCodes.Leave, afterFinally);
                    cg.MarkLabel(next);

                }

                cg.PopTargets();
                cg.BeginFinallyBlock();

                cg.EmitCodeContext();
                cg.EmitCall(typeof(RuntimeHelpers), "PopExceptionHandler");

                cg.EndExceptionBlock();

                cg.FreeLocalTmp(tmpExc);

                cg.Emit(OpCodes.Rethrow);
                cg.PopTargets(TargetBlockType.Catch);
            }

            if (_finally != null) {
                cg.PushExceptionBlock(TargetBlockType.Finally, flowControlVar, finallyYieldSlot);
                cg.BeginFinallyBlock();

                Label endOfFinally = cg.DefineLabel();

                // if try yielded
                EmitOnYieldBranchToLabel(_tryTargets, tryYieldSlot, endOfFinally, cg);
                // if catch yielded
                EmitOnYieldBranchToLabel(_catchTargets, catchYieldSlot, endOfFinally, cg);
                //if else yielded
                EmitOnYieldBranchToLabel(_elseTargets, elseYieldSlot, endOfFinally, cg);

                EmitYieldDispatch(_finallyTargets, finallyYieldSlot, finallyChoiceVar, cg);
                _finally.Emit(cg);

                if (IsBlockYieldable(_finallyTargets) || returnInFinally) {
                    Label nothrow = cg.DefineLabel();
                    exceptionSlot.EmitGet(cg);
                    cg.Emit(OpCodes.Dup);
                    cg.Emit(OpCodes.Brfalse_S, nothrow);
                    cg.Emit(OpCodes.Throw);
                    cg.MarkLabel(nothrow);
                    cg.Emit(OpCodes.Pop);
                }

                cg.MarkLabel(endOfFinally);
                cg.EndExceptionBlock();
                cg.PopTargets(TargetBlockType.Finally);
            } else {
                cg.EndExceptionBlock();
            }
            cg.PopTargets(TargetBlockType.Try);

            cg.MarkLabel(afterFinally);

            if (cg.IsGenerator || returnInFinally) {
                Label noReturn = cg.DefineLabel();
                
                flowControlVar.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForReturn);
                cg.Emit(OpCodes.Bne_Un, noReturn);

                if (cg.IsGenerator) {
                    // return true from the generator method
                    cg.Emit(OpCodes.Ldc_I4_1);
                    cg.EmitReturn();
                } else if (returnInFinally) {
                    // return the actual value
                    cg.EmitReturnValue();
                    cg.EmitReturn();
                }
                cg.MarkLabel(noReturn);
            }

            if (foundLoopControl) {
                Label noReturn = cg.DefineLabel();

                noReturn = cg.DefineLabel();
                flowControlVar.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForBreak);
                cg.Emit(OpCodes.Bne_Un, noReturn);
                cg.EmitBreak();
                cg.MarkLabel(noReturn);

                noReturn = cg.DefineLabel();
                flowControlVar.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForContinue);
                cg.Emit(OpCodes.Bne_Un, noReturn);
                cg.EmitContinue();
                cg.MarkLabel(noReturn);
            }

            // clean up 
            if (IsBlockYieldable(_tryTargets)) {
                cg.FreeLocalTmp(tryChoiceVar);
                _tryTargets.Clear();
            }
            if (IsBlockYieldable(_catchTargets)) {
                cg.FreeLocalTmp(catchChoiceVar);
                _catchTargets.Clear();
            }
            if (IsBlockYieldable(_finallyTargets)) {
                cg.FreeLocalTmp(finallyChoiceVar);
                _finallyTargets.Clear();
            }
            if (IsBlockYieldable(_elseTargets)) {
                cg.FreeLocalTmp(elseChoiceVar);
                _elseTargets.Clear();
            }
            if (_else != null) {
                cg.FreeLocalTmp(isElseBlock);
            }

            cg.FreeLocalTmp(flowControlVar);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _body.Walk(walker);
                if (_handlers != null)
                    foreach (TryStatementHandler handler in _handlers)
                        handler.Walk(walker);

                if (_else != null)
                    _else.Walk(walker);

                if (_finally != null)
                    _finally.Walk(walker);

            }
            walker.PostWalk(this);
        }

        /// <summary>
        /// Control flow finder walker for TryStatement codegen.
        /// </summary>
        class ControlFlowFinder : Walker {
            bool found;

            bool foundLoopControl;
            int loopCount = 0;

            public static bool FindControlFlow(Statement statement, out bool foundLoopControl) {
                // No return in null statement
                if (statement == null) {
                    foundLoopControl = false;
                    return false;
                }

                // find it now.
                ControlFlowFinder rf = new ControlFlowFinder();
                statement.Walk(rf);
                foundLoopControl = rf.foundLoopControl;
                return rf.found;
            }

            public override bool Walk(BreakStatement node) {
                if (loopCount == 0) {
                    found = true;
                    foundLoopControl = true;
                }
                return true;
            }

            public override bool Walk(ContinueStatement node) {
                if (loopCount == 0) {
                    found = true;
                    foundLoopControl = true;
                }
                return true;
            }

            public override bool Walk(ReturnStatement node) {
                found = true;
                return true;
            }

            public override bool Walk(LoopStatement node) {
                loopCount++;
                return true;
            }

            public override void PostWalk(LoopStatement node) {
                loopCount--;
            }
        }
    }
}
