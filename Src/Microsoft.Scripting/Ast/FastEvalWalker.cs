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

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Determine if we should evaluate a given tree in interpreted mode.
    /// Currently, we can evaluate everything that does not contain as-yet-unsupported nodes.
    /// In the future, we may consider using heuristics based on size, presence of loops, etc.
    /// </summary>
    public class FastEvalWalker : Walker {
        private bool _hasUnsupportedNodes = false;
        private bool _hasLoops = false;

        public bool EvaluationOK(bool useHeuristics) {
            if (useHeuristics) {
                return !_hasUnsupportedNodes && !_hasLoops;
            } else {
                return !_hasUnsupportedNodes;
            }
        }

        public static bool CanEvaluate(CodeBlock node, bool useHeuristics) {
            FastEvalWalker walker = new FastEvalWalker();
            node.Walk(walker);
            return walker.EvaluationOK(useHeuristics);
        }

        //
        // Nodes we don't support.
        // Rather than adding to this category, consider implementing Evaluate() on the node!
        //

        public override bool Walk(ParamsExpression node) {
            _hasUnsupportedNodes = true;
            return false;
        }

        public override bool Walk(EnvironmentExpression node) {
            _hasUnsupportedNodes = true;
            return false;
        }

        public override bool Walk(SwitchStatement node) {
            _hasUnsupportedNodes = true;
            return false;
        }

        public override bool Walk(UnboundExpression node) {
            // Right now, locals() fails for nested functions.
            // This is a crude test, but at least it errs on the side of disabling evaluation.
            if (SymbolTable.IdToString(node.Name) == "locals") {
                _hasUnsupportedNodes = true;
                return false;
            } else {
                return true;
            }
        }

        //
        // Nodes that may take a long time to execute
        //
        public override bool Walk(LoopStatement node) {
            _hasLoops = true;
            return false;
        }

        public override bool Walk(DoStatement node) {
            _hasLoops = true;
            return false;
        }
    }
}
