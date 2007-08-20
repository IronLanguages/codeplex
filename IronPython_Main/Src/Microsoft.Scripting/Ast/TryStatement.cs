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
    public class TryStatement : Statement {
        private readonly SourceLocation _header;
        private readonly Statement _body;
        private readonly CatchBlock[] _handlers;
        private readonly Statement _finally;

        /// <summary>
        /// Called by <see cref="TryStatementBuilder"/>.
        /// Creates a try/catch/finally/else block.
        /// 
        /// The body is protected by the try block.
        /// The handlers consist of a set of language-dependent tests which call into the LanguageContext.
        /// The elseSuite runs if no exception is thrown.
        /// The finallySuite runs regardless of how control exits the body.
        /// </summary>
        internal TryStatement(SourceSpan span, SourceLocation header, Statement body, CatchBlock[] handlers, Statement @finally)
            : base(span) {

            if (handlers == null && @finally == null) {
                throw new ArgumentException("TryStatement requires at least one catch block or a finally");
            }

            _body = body;
            _handlers = handlers;
            _finally = @finally;
            _header = header;
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public Statement Body {
            get { return _body; }
        }

        public IList<CatchBlock> Handlers {
            get { return _handlers; }
        }

        public Statement FinallyStatement {
            get { return _finally; }
        }
        
        protected override object DoExecute(CodeContext context) {
            bool rethrow = false;
            Exception savedExc = null;
            object ret = Statement.NextStatement;
            try {
                ret = _body.Execute(context);
            } catch (Exception exc) {
                rethrow = true;
                savedExc = exc;
                if (_handlers != null) {
                    foreach (CatchBlock handler in _handlers) {
                        if (handler.Test.IsInstanceOfType(exc)) {
                            rethrow = false;
                            if (handler.Variable != null) {
                                RuntimeHelpers.SetName(context, handler.Variable.Name, exc);
                            }
                            ret = handler.Body.Execute(context);
                            break;
                        }
                    }
                }
            } finally {
                if (_finally != null) {
                    object finallyRet = _finally.Execute(context);
                    if (finallyRet != Statement.NextStatement) {
                        ret = finallyRet;
                        rethrow = false;
                    }
                }
                if (rethrow) {
                    throw ExceptionHelpers.UpdateForRethrow(savedExc);
                }
            }

            return ret;
        }
        
        public override void Emit(CodeGen cg) {
            // Codegen is affected by presence/absence of loop control statements
            // (break/continue) or return statement in finally clause
            TryFlowResult flow = TryFlowAnalyzer.Analyze(FinallyStatement);

            //
            // Initialize the flow control flag
            //
            Slot flowControlFlag = null;

            if (flow.Any) {
                flowControlFlag = cg.GetLocalTmp(typeof(int));
                cg.EmitInt(CodeGen.FinallyExitsNormally);
                flowControlFlag.EmitSet(cg);
            }

            /******************************************************************/
            // 1. ENTERING TRY
            /******************************************************************/

            cg.PushExceptionBlock(TargetBlockType.Try, flowControlFlag);
            Label exceptionEnd = cg.BeginExceptionBlock();

            /******************************************************************/
            // 2. Emit the try statement body
            /******************************************************************/

            _body.Emit(cg);
            cg.EmitSequencePointNone();

            /******************************************************************/
            // 3. Emit the catch blocks
            /******************************************************************/

            if (_handlers != null) {
                cg.PushExceptionBlock(TargetBlockType.Catch, flowControlFlag);

                foreach (CatchBlock cb in _handlers) {
                    // Begin the strongly typed exception block
                    cg.BeginCatchBlock(cb.Test);

                    if (cb.Variable != null) {
                        // If the variable is present, store the exception
                        // in the variable.
                        cb.Ref.Slot.EmitSet(cg);
                    } else {
                        // Otherwise, pop it off the stack.
                        cg.Emit(OpCodes.Pop);
                    }

                    //
                    // Emit the catch block body
                    //
                    cb.Body.Emit(cg);
                }

                cg.PopTargets(TargetBlockType.Catch);
            }

            /******************************************************************/
            // 4. Emit the finally block
            /******************************************************************/

            if (_finally != null) {
                cg.PushExceptionBlock(TargetBlockType.Finally, flowControlFlag);
                cg.BeginFinallyBlock();

                //
                // Emit the finally block body
                //
                _finally.Emit(cg);

                cg.EndExceptionBlock();
                cg.PopTargets(TargetBlockType.Finally);
            } else {
                cg.EndExceptionBlock();
            }

            cg.PopTargets(TargetBlockType.Try);

            if (flow.Return || flow.Yield) {
                Debug.Assert(flowControlFlag != null);

                Label noReturn = cg.DefineLabel();

                flowControlFlag.EmitGet(cg);
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

            if (flow.Loop) {
                Debug.Assert(flowControlFlag != null);

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

            cg.FreeLocalTmp(flowControlFlag);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _body.Walk(walker);

                if (_handlers != null) {
                    foreach (CatchBlock handler in _handlers) {
                        handler.Walk(walker);
                    }
                }

                if (_finally != null) {
                    _finally.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
