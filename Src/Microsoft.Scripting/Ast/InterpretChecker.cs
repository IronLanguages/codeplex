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
    /// <summary>
    /// Determine if we should evaluate a given tree in interpreted mode.
    /// Currently, we can evaluate everything that does not contain as-yet-unsupported nodes.
    /// In the future, we may consider using heuristics based on size, presence of loops, etc.
    /// </summary>
    class InterpretChecker : Walker {
        private bool _hasUnsupportedNodes;
        private bool _hasLoops;
        private int _expressionDepth;

        public bool EvaluationOK(bool useHeuristics) {
            if (useHeuristics) {
                return !_hasUnsupportedNodes && !_hasLoops;
            } else {
                return !_hasUnsupportedNodes;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static bool CanEvaluate(CodeBlock node, bool useHeuristics) {
            InterpretChecker walker = new InterpretChecker();
            walker.WalkNode(node);
            Debug.Assert(walker._expressionDepth == 0);
            return walker.EvaluationOK(useHeuristics);
        }

        #region Walker Overrides

        // ActionExpression
        protected internal override bool Walk(ActionExpression node) { Push();  return true; }
        protected internal override void PostWalk(ActionExpression node) { Pop(); }

        // ArrayIndexAssignment
        protected internal override bool Walk(ArrayIndexAssignment node) { Push(); return true; }
        protected internal override void PostWalk(ArrayIndexAssignment node) { Pop(); }

        // BinaryExpression
        protected internal override bool Walk(BinaryExpression node) { Push(); return true; }
        protected internal override void PostWalk(BinaryExpression node) { Pop(); }

        // BoundAssignment
        protected internal override bool Walk(BoundAssignment node) { Push(); return true; }
        protected internal override void PostWalk(BoundAssignment node) { Pop(); }

        // BoundExpression
        protected internal override bool Walk(BoundExpression node) { Push(); return true; }
        protected internal override void PostWalk(BoundExpression node) { Pop(); }

        // BreakStatement
        protected internal override bool Walk(BreakStatement node) {
            return DisallowControlFlowInExpression();
        }

        // CodeBlockExpression
        protected internal override bool Walk(CodeBlockExpression node) {
            // Do not recurse into nested code blocks
            return false;
        }
        protected internal override void PostWalk(CodeBlockExpression node) {
            // We use PostWalk here since Walk is only called for IsDeclarative=true

            // Currently, CodeBlockExpression ignores the DelegateType. This will result in the generation of 
            // an incorrectly type delegate. For eg, for event hookup with a source code snippet, a language might 
            // generate a rule that creates CodeBlockExpressions with the source code snippet typed as EventHandler.
            // The interpreter will ignore that a create a CallWithContextN delegate, which will cause a problem 
            // with the event hookup
            if (node.DelegateType != null) {
                _hasUnsupportedNodes = true;
            }
        }

        // ConditionalExpression
        protected internal override bool Walk(ConditionalExpression node) { Push(); return true; }
        protected internal override void PostWalk(ConditionalExpression node) { Pop(); }

        // ConstantExpression
        protected internal override bool Walk(ConstantExpression node) { Push(); return true; }
        protected internal override void PostWalk(ConstantExpression node) { Pop(); }

        // ContinueStatement
        protected internal override bool Walk(ContinueStatement node) {
            return DisallowControlFlowInExpression();
        }

        // DeleteUnboundExpression
        protected internal override bool Walk(DeleteUnboundExpression node) { Push(); return true; }
        protected internal override void PostWalk(DeleteUnboundExpression node) { Pop(); }

        // DoStatement
        protected internal override bool Walk(DoStatement node) {
            _hasLoops = true;
            return false;
        }

        // IntrinsicExpression
        protected internal override bool Walk(IntrinsicExpression node) {
            // No need to push/pop sinte this is a leaf node
            switch (node.NodeType) {
                case AstNodeType.ParamsExpression:
                case AstNodeType.EnvironmentExpression:
                    _hasUnsupportedNodes = true;
                    break;
            }
            return false;
        }

        // LoopStatement
        protected internal override bool Walk(LoopStatement node) {
            _hasLoops = true;
            return false;
        }

        // MemberAssignment
        protected internal override bool Walk(MemberAssignment node) { Push(); return true; }
        protected internal override void PostWalk(MemberAssignment node) { Pop(); }

        // MemberExpression
        protected internal override bool Walk(MemberExpression node) { Push(); return true; }
        protected internal override void PostWalk(MemberExpression node) { Pop(); }

        // MethodCallExpression
        protected internal override bool Walk(MethodCallExpression node) { Push(); return true; }
        protected internal override void PostWalk(MethodCallExpression node) { Pop(); }

        // NewArrayExpression
        protected internal override bool Walk(NewArrayExpression node) { Push(); return true; }
        protected internal override void PostWalk(NewArrayExpression node) { Pop(); }

        // NewExpression
        protected internal override bool Walk(NewExpression node) { Push(); return true; }
        protected internal override void PostWalk(NewExpression node) { Pop(); }

        // ReturnStatement
        protected internal override bool Walk(ReturnStatement node) {
            return DisallowControlFlowInExpression();
        }

        // TypeBinaryExpression
        protected internal override bool Walk(TypeBinaryExpression node) { Push(); return true; }
        protected internal override void PostWalk(TypeBinaryExpression node) { Pop(); }

        // UnaryExpression
        protected internal override bool Walk(UnaryExpression node) { Push(); return true; }
        protected internal override void PostWalk(UnaryExpression node) { Pop(); }

        // UnboundAssignment
        protected internal override bool Walk(UnboundAssignment node) { Push(); return true; }
        protected internal override void PostWalk(UnboundAssignment node) { Pop(); }

        // UnboundExpression
        protected internal override bool Walk(UnboundExpression node) {
            Push();
            // Right now, locals() fails for nested functions.
            // This is a crude test, but at least it errs on the side of disabling evaluation.
            if (SymbolTable.IdToString(node.Name) == "locals") {
                _hasUnsupportedNodes = true;
                return false;
            } else {
                return true;
            }
        }
        protected internal override void PostWalk(UnboundExpression node) { Pop(); }

        #endregion

        private void Push() { _expressionDepth++; }
        private void Pop() { _expressionDepth--; }

        private bool DisallowControlFlowInExpression() { 
            if (_expressionDepth > 0) {
                _hasUnsupportedNodes = true;
                return false;
            }
            return true;
        }
    }
}
