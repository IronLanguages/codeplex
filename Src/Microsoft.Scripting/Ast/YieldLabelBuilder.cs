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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    class YieldLabelBuilder : CodeBlockWalker {
        sealed class ExceptionBlock {
            public enum TryStatementState {
                Try,
                Handler,
                Finally,
                Else
            };

            private readonly DynamicTryStatement _statement;
            private TryStatementState _state;

            public ExceptionBlock(DynamicTryStatement stmt)
                : this(TryStatementState.Try, stmt) {
            }

            public ExceptionBlock(TryStatementState state, DynamicTryStatement stmt) {
                Debug.Assert(stmt != null);

                _state = state;
                _statement = stmt;
            }

            internal TryStatementState State {
                get { return _state; }
                set { _state = value; }
            }

            internal TargetLabel TopTarget {
                get {
                    return _statement.EnsureTopTarget();
                }
            }

            public void AddYieldTarget(TargetLabel tl, int index) {
                switch (_state) {
                    case TryStatementState.Try:
                        _statement.AddTryYieldTarget(tl, index);
                        break;
                    case TryStatementState.Handler:
                        _statement.AddCatchYieldTarget(tl, index);
                        break;
                    case TryStatementState.Finally:
                        _statement.AddFinallyYieldTarget(tl, index);
                        break;
                    case TryStatementState.Else:
                        _statement.AddElseYieldTarget(tl, index);
                        break;
                }
            }
        }

        private readonly Stack<ExceptionBlock> _tryBlocks = new Stack<ExceptionBlock>();
        private readonly List<YieldTarget> _topTargets = new List<YieldTarget>();
        private int _temps;

        private YieldLabelBuilder() {
        }

        internal static void BuildYieldTargets(GeneratorCodeBlock g, out List<YieldTarget> topTargets, out int temps) {
            YieldLabelBuilder b = new YieldLabelBuilder();
            g.Body.Walk(b);
            topTargets = b._topTargets;
            temps = b._temps;
        }

        #region AstWalker method overloads

        public override bool Walk(DynamicTryStatement node) {
            ExceptionBlock tb = new ExceptionBlock(node);

            _tryBlocks.Push(tb);
            node.Body.Walk(this);

            if (node.Handlers != null) {
                tb.State = ExceptionBlock.TryStatementState.Handler;
                foreach (DynamicTryStatementHandler handler in node.Handlers) {
                    handler.Walk(this);
                }
            }

            if (node.ElseStatement != null) {
                tb.State = ExceptionBlock.TryStatementState.Else;
                node.ElseStatement.Walk(this);
            }

            if (node.FinallyStatement != null) {
                tb.State = ExceptionBlock.TryStatementState.Finally;
                node.FinallyStatement.Walk(this);
            }

            ExceptionBlock eb = _tryBlocks.Pop();
            Debug.Assert((object)tb == (object)eb);

            return false;
        }

        public override void PostWalk(DynamicTryStatement node) {
            _temps += node.GetGeneratorTempCount();
        }

        public override void PostWalk(YieldStatement node) {
            // Assign the yield statement index for codegen
            int index = _topTargets.Count;
            TargetLabel label = new TargetLabel();
            node.Target = new YieldTarget(index, label);

            foreach (ExceptionBlock eb in _tryBlocks) {
                eb.AddYieldTarget(label, index);

                // From enclosing lexical scopes, one must jump
                // to the try block label in order to jump to any
                // of the enclosed yield targets.
                label = eb.TopTarget;
            }

            // Insert the top target to the top yields
            Debug.Assert(_topTargets.Count == index);
            _topTargets.Add(new YieldTarget(index, label));
        }

        #endregion
    }
}
