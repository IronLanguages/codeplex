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

using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    class YieldLabelBuilder : Walker {
        public abstract class ExceptionBlock {
            public enum State {
                Try,
                Handler,
                Finally,
                Else
            };
            public State state;

            protected ExceptionBlock(State state) {
                this.state = state;
            }

            public abstract void AddYieldTarget(YieldStatement ys, YieldTarget yt, CodeGen cg);
        }

        public sealed class TryBlock : ExceptionBlock {
            private TryStatement stmt;
            private bool isPython24TryFinallyStmt;

            public TryBlock(TryStatement stmt, bool isPython24TryFinallyStmt)
                : this(stmt, State.Try, isPython24TryFinallyStmt) {
            }

            public TryBlock(TryStatement stmt, State state, bool isPython24TryFinallyStmt)
                : base(state) {
                this.stmt = stmt;
                this.isPython24TryFinallyStmt = isPython24TryFinallyStmt;
            }

            public override void AddYieldTarget(YieldStatement ys, YieldTarget yt, CodeGen cg) {
                switch (state) {
                    case State.Try:
                        if (isPython24TryFinallyStmt)
                            cg.Context.AddError("cannot yield from try block with finally", ys);
                        else
                            stmt.AddTryYieldTarget(yt.FixForTryCatchFinally(cg));
                        break;
                    case State.Handler:
                        stmt.AddCatchYieldTarget(yt.FixForTryCatchFinally(cg));
                        break;
                    case State.Finally:
                        stmt.AddFinallyYieldTarget(yt.FixForTryCatchFinally(cg));
                        break;
                    case State.Else:
                        stmt.AddElseYieldTarget(yt.FixForTryCatchFinally(cg));
                        break;

                }
                ys.Label = yt.YieldContinuationTarget;

            }
        }

        private Stack<ExceptionBlock> _tryBlocks = new Stack<ExceptionBlock>();
        private List<YieldTarget> _topYields;
        private CodeGen _cg;

        private YieldLabelBuilder(CodeGen cg) {
            _cg = cg;
            _topYields = new List<YieldTarget>();
        }

        internal static List<YieldTarget> BuildYieldTargets(GeneratorCodeBlock code, CodeGen cg) {
            YieldLabelBuilder b = new YieldLabelBuilder(cg);
            code.Body.Walk(b);
            return b._topYields;
        }

        #region AstWalker method overloads

        public override bool Walk(CodeBlockExpression node) {
            // Do not recurse into nested functions
            return false;
        }

        public override bool Walk(TryStatement node) {
            TryBlock tb = null;

            if (!ScriptDomainManager.Options.Python25 && node.Handlers == null)
                tb = new TryBlock(node, true);
            else
                tb = new TryBlock(node, false);

            _tryBlocks.Push(tb);
            node.Body.Walk(this);

            if (node.Handlers != null) {
                tb.state = TryBlock.State.Handler;
                foreach (TryStatementHandler handler in node.Handlers) {
                    handler.Walk(this);
                }
            }

            if (node.ElseStatement != null) {
                tb.state = TryBlock.State.Else;
                node.ElseStatement.Walk(this);
            }

            if (node.FinallyStatement != null) {
                tb.state = TryBlock.State.Finally;
                node.FinallyStatement.Walk(this);
            }

            ExceptionBlock eb = _tryBlocks.Pop();
            Debug.Assert((object)tb == (object)eb);

            return false;
        }

        public override void PostWalk(YieldStatement node) {
            // Assign the yield statement index for codegen
            node.Index = _topYields.Count;

            _topYields.Add(new YieldTarget(_cg.DefineLabel()));

            if (_tryBlocks.Count == 0) {
                node.Label = _topYields[node.Index].TopBranchTarget;
            } else if (_tryBlocks.Count == 1) {
                ExceptionBlock eb = _tryBlocks.Peek();
                eb.AddYieldTarget(node, _topYields[node.Index], _cg);
            } else {
                _cg.Context.AddError("yield nested too deep in", node);
            }
        }

        #endregion
    }
}
