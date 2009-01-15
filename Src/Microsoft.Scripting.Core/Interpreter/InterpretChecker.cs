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
using System; using Microsoft;


using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Interpreter {
    /// <summary>
    /// Determine if we should evaluate a given tree in interpreted mode.
    /// Currently, we can evaluate everything that does not contain as-yet-unsupported nodes.
    /// In the future, we may consider using heuristics based on size, presence of loops, etc.
    /// </summary>
    internal sealed class InterpretChecker : Walker {
        private bool _ok = true;                // We start off assuming that the tree can be interpreted
        private int _expressionDepth;
        private Queue<LambdaExpression> _lambdas;

        public static bool CanEvaluate(LambdaExpression node) {
            InterpretChecker walker = new InterpretChecker();
            return walker.Check(node);
        }

        private bool Check(LambdaExpression lambda) {
            // explicitly walk the lambda body.
            // walker will not recurse into lambdas and will put them in the queue instead.
            WalkNode(lambda.Body);

            Debug.Assert(_expressionDepth == 0);

            while (_ok && _lambdas != null && _lambdas.Count > 0) {
                lambda = _lambdas.Dequeue();
                WalkNode(lambda.Body);
                Debug.Assert(_expressionDepth == 0);
            }

            return _ok;
        }

        #region Walker Overrides

        // ActionExpression
        protected override bool Walk(ActionExpression node) { return Push(); }
        protected override void PostWalk(ActionExpression node) { Pop(); }

        // BinaryExpression
        protected override bool Walk(BinaryExpression node) { return Push(); }
        protected override void PostWalk(BinaryExpression node) { Pop(); }

        // AssignmentExpression
        protected override bool Walk(AssignmentExpression node) { return Push(); }
        protected override void PostWalk(AssignmentExpression node) { Pop(); }

        // VariableExpression
        protected override bool Walk(VariableExpression node) { return Push(); }
        protected override void PostWalk(VariableExpression node) { Pop(); }

        // ParameterExpression
        protected override bool Walk(ParameterExpression node) { return Push(); }
        protected override void PostWalk(ParameterExpression node) { Pop(); }

        // BreakStatement
        protected override bool Walk(BreakStatement node) {
            return DisallowControlFlowInExpression();
        }

        // LambdaExpression
        protected override bool Walk(LambdaExpression node) {
            // Do not recurse into nested lambdas, but process them later
            if (_lambdas == null) {
                _lambdas = new Queue<LambdaExpression>();
            }
            _lambdas.Enqueue(node);
            return false;
        }

        // ConditionalExpression
        protected override bool Walk(ConditionalExpression node) { return Push(); }
        protected override void PostWalk(ConditionalExpression node) { Pop(); }

        // ConstantExpression
        protected override bool Walk(ConstantExpression node) { return Push(); }
        protected override void PostWalk(ConstantExpression node) { Pop(); }

        // ContinueStatement
        protected override bool Walk(ContinueStatement node) {
            return DisallowControlFlowInExpression();
        }

        // IntrinsicExpression
        protected override bool Walk(IntrinsicExpression node) {
            // No need to push/pop sinte this is a leaf node
            switch (node.NodeType) {
                case AstNodeType.GeneratorIntrinsic:
                    _ok = false;
                    break;
            }
            return false;
        }

        // LabeledStatement
        protected override bool Walk(LabeledStatement node) {
            return _ok = false;    // Cannot interpret labeled statement yet
        }

        // MemberExpression
        protected override bool Walk(MemberExpression node) { return Push(); }
        protected override void PostWalk(MemberExpression node) { Pop(); }

        // MethodCallExpression
        protected override bool Walk(MethodCallExpression node) { return Push(); }
        protected override void PostWalk(MethodCallExpression node) { Pop(); }

        // NewArrayExpression
        protected override bool Walk(NewArrayExpression node) { return Push(); }
        protected override void PostWalk(NewArrayExpression node) { Pop(); }

        // NewExpression
        protected override bool Walk(NewExpression node) { return Push(); }
        protected override void PostWalk(NewExpression node) { Pop(); }

        // ReturnStatement
        protected override bool Walk(ReturnStatement node) {
            return DisallowControlFlowInExpression();
        }

        // TypeBinaryExpression
        protected override bool Walk(TypeBinaryExpression node) { return Push(); }
        protected override void PostWalk(TypeBinaryExpression node) { Pop(); }

        // UnaryExpression
        protected override bool Walk(UnaryExpression node) { return Push(); }
        protected override void PostWalk(UnaryExpression node) { Pop(); }

        // YieldStatement
        protected override bool Walk(YieldStatement node) {
            return _ok = false;    // Cannot interpret yield
        }

        #endregion

        private bool Push() {
            _expressionDepth++;
            return _ok;
        }
        private void Pop() {
            _expressionDepth--;
        }

        private bool DisallowControlFlowInExpression() {
            if (_expressionDepth > 0) {
                _ok = false;
            }
            return _ok;
        }
    }
}
