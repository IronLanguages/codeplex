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
    /// Determine if we should evaluate a given tree in FastEval mode.
    /// Currently, we can evaluate everything that does not contain as-yet-unsupported nodes.
    /// In the future, we may consider using heuristics based on size, presence of loops, etc.
    /// </summary>
    public class FastEvalWalker : Walker {
        private bool _hasUnsupportedNodes = false;
#if USE_HEURISTICS
        private bool _hasLoops = false;
#endif
        public bool EvaluationOK {
            get {
#if !USE_HEURISTICS
                return !_hasUnsupportedNodes;
#else
                return !_hasUnsupportedNodes && !_hasLoops;
#endif
            }
        }

        public static bool CanEvaluate(CodeBlock node) {
            // Evaluation is ALWAYS DISABLED for now.
            // It will be enabled when it no longer breaks user code.
            return false;

#if USE_HEURISTICS
            // If the tree is sufficiently large, don't even bother walking it: we want to emit code here.
            // TODO: use a better heuristic, or at least determine the appropriate magic number.
            if (node.Span.Length > 1000) {
                return false;
            }
#endif

#if FALSE
            FastEvalWalker walker = new FastEvalWalker();
            node.Walk(walker);
            return walker.EvaluationOK;
#endif
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

        // Only partially supported (missing param arrays and wrapper methods)
        public override bool Walk(GeneratorCodeBlock node) {
            _hasUnsupportedNodes = true;
            return false;
        }

        //
        // Nodes that may take a long time to execute
        //
#if USE_HEURISTICS
        public override bool Walk(LoopStatement node) {
            _hasLoops = true;
            return false;
        }

        public override bool Walk(DoStatement node) {
            _hasLoops = true;
            return false;
        }
#endif
    }
}
