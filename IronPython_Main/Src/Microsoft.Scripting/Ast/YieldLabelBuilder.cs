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

using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.Scripting.Ast {
    sealed class YieldLabelBuilder : LambdaWalker {
        sealed class ExceptionBlock {
            public enum TryStatementState {
                Try,
                Handler,
                Finally
            };

            private readonly TryStatementInfo _tsi;
            private TryStatementState _state;
            private int _handler;

            public ExceptionBlock(TryStatementInfo tsi) {
                Debug.Assert(tsi != null);

                _state = TryStatementState.Try;
                _tsi = tsi;
            }

            internal TryStatementState State {
                get { return _state; }
                set { _state = value; }
            }

            internal int Handler {
                get { return _handler; }
                set { _handler = value; }
            }

            /// <summary>
            /// Adds yield target to the current try statement and returns the label
            /// to which the outer code must jump to to route properly to this label.
            /// </summary>
            internal TargetLabel AddYieldTarget(TargetLabel label, int index) {
                switch (State) {
                    case TryStatementState.Try:
                        return _tsi.AddTryYieldTarget(label, index);
                    case TryStatementState.Handler:
                        return _tsi.AddCatchYieldTarget(label, index, _handler);
                    case TryStatementState.Finally:
                        return _tsi.AddFinallyYieldTarget(label, index);

                    default:
                        Debug.Assert(false, "Invalid try statement state " + State.ToString());
                        throw new System.InvalidOperationException();
                }
            }
        }

        private Dictionary<TryStatement, TryStatementInfo> _tryInfos;
        private Dictionary<YieldStatement, YieldTarget> _yieldTargets;
        private readonly Stack<ExceptionBlock> _tryBlocks = new Stack<ExceptionBlock>();
        private readonly List<YieldTarget> _topTargets = new List<YieldTarget>();
        private int _temps;

        private YieldLabelBuilder() {
        }

        internal static void BuildYieldTargets(GeneratorCodeBlock gcb, LambdaInfo lambdaInfo) {
            YieldLabelBuilder ylb = new YieldLabelBuilder();
            ylb.WalkNode(gcb.Body);

            // Populate results into the CodeBlockInfo
            lambdaInfo.PopulateGeneratorInfo(ylb._tryInfos, ylb._yieldTargets, ylb._topTargets, ylb._temps);
        }

        #region AstWalker method overloads

        protected internal override bool Walk(TryStatement node) {
            TryStatementInfo tsi = new TryStatementInfo(node);
            ExceptionBlock block = new ExceptionBlock(tsi);

            _tryBlocks.Push(block);
            WalkNode(node.Body);

            IList<CatchBlock> handlers = node.Handlers;
            if (handlers != null) {
                tsi.CreateCatchFlags(handlers.Count);

                block.State = ExceptionBlock.TryStatementState.Handler;
                for (int handler = 0; handler < handlers.Count; handler++) {
                    block.Handler = handler;
                    WalkNode(handlers[handler].Body);
                }
            }

            if (node.FinallyStatement != null) {
                block.State = ExceptionBlock.TryStatementState.Finally;
                WalkNode(node.FinallyStatement);
            }

            Debug.Assert((object)block == (object)_tryBlocks.Peek());
            _tryBlocks.Pop();

            // Remember the TryStatementInfo for code generation.
            if (_tryInfos == null) {
                _tryInfos = new Dictionary<TryStatement, TryStatementInfo>();
            }
            _tryInfos[node] = tsi;

            return false;
        }

        protected internal override void PostWalk(TryStatement node) {
            _temps += node.GetGeneratorTempCount();
        }

        protected internal override void PostWalk(YieldStatement node) {
            // Assign the yield statement index for codegen
            int index = _topTargets.Count;
            TargetLabel label = new TargetLabel();

            // Remember the YieldTarget for code generation.
            if (_yieldTargets == null) {
                _yieldTargets = new Dictionary<YieldStatement, YieldTarget>();
            }
            _yieldTargets[node] = new YieldTarget(index, label);

            foreach (ExceptionBlock eb in _tryBlocks) {
                // The exception statement must determine
                // the label for the enclosing code to jump to
                // to return to the given yield target

                label = eb.AddYieldTarget(label, index);
            }

            // Insert the top target to the top yields
            Debug.Assert(_topTargets.Count == index);
            _topTargets.Add(new YieldTarget(index, label));
        }

        #endregion
    }
}
