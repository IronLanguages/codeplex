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

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// AST walker to analyze control flow for the DynamicTryStatement code generation.
    /// </summary>
    class TryFlowAnalyzer : CodeBlockWalker {
        /// <summary>
        /// Any control flow (return, break, continue)
        /// </summary>
        private bool _flow;

        /// <summary>
        /// Any loop control flow directly contained within
        /// the try block (not in nested loops which don't
        /// affect the try block)
        /// </summary>
        private bool _loop;

        /// <summary>
        /// Nested loops counter. We are only interested in
        /// control flow statements outside on the top level.
        /// </summary>
        private int _nesting;

        public static void Analyze(Statement statement, out bool flow, out bool loop) {
            if (statement == null) {
                // No return in null statement
                flow = loop = false;
            } else {
                // find it now.
                TryFlowAnalyzer tfa = new TryFlowAnalyzer();
                statement.Walk(tfa);

                Debug.Assert(tfa._nesting == 0);

                flow = tfa._flow;
                loop = tfa._loop;
            }
        }

        public override bool Walk(BreakStatement node) {
            if (_nesting == 0) {
                _flow = true;
                _loop = true;
            }
            return true;
        }

        public override bool Walk(ContinueStatement node) {
            if (_nesting == 0) {
                _flow = true;
                _loop = true;
            }
            return true;
        }

        public override bool Walk(ReturnStatement node) {
            _flow = true;
            return true;
        }

        // Keep track of nested loops, only loop flow control
        // statements outside of nested loops concern us

        public override bool Walk(LoopStatement node) {
            _nesting++;
            return true;
        }

        public override void PostWalk(LoopStatement node) {
            _nesting--;
        }

        public override bool Walk(DoStatement node) {
            _nesting++;
            return true;
        }

        public override void PostWalk(DoStatement node) {
            _nesting--;
        }
    }
}
