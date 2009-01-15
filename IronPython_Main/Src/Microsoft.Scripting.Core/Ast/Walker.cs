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

using System.Collections.Generic;
using System; using Microsoft;

namespace Microsoft.Scripting.Ast {
    partial class Walker {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void WalkNode(Expression node) {
            if (node == null) {
                return;
            }

            switch (node.NodeType) {
                case AstNodeType.Add:
                case AstNodeType.And:
                case AstNodeType.AndAlso:
                case AstNodeType.ArrayIndex:
                case AstNodeType.Divide:
                case AstNodeType.Equal:
                case AstNodeType.ExclusiveOr:
                case AstNodeType.GreaterThan:
                case AstNodeType.GreaterThanOrEqual:
                case AstNodeType.LeftShift:
                case AstNodeType.LessThan:
                case AstNodeType.LessThanOrEqual:
                case AstNodeType.Modulo:
                case AstNodeType.Multiply:
                case AstNodeType.NotEqual:
                case AstNodeType.Or:
                case AstNodeType.OrElse:
                case AstNodeType.RightShift:
                case AstNodeType.Subtract:
                    DefaultWalk((BinaryExpression)node);
                    break;
                case AstNodeType.Call:
                    DefaultWalk((MethodCallExpression)node);
                    break;
                case AstNodeType.Conditional:
                    DefaultWalk((ConditionalExpression)node);
                    break;
                case AstNodeType.Constant:
                    DefaultWalk((ConstantExpression)node);
                    break;
                case AstNodeType.Convert:
                case AstNodeType.Negate:
                case AstNodeType.Not:
                case AstNodeType.OnesComplement:
                    DefaultWalk((UnaryExpression)node);
                    break;
                case AstNodeType.New:
                    DefaultWalk((NewExpression)node);
                    break;
                case AstNodeType.TypeIs:
                    DefaultWalk((TypeBinaryExpression)node);
                    break;
                case AstNodeType.ActionExpression:
                    DefaultWalk((ActionExpression)node);
                    break;
                case AstNodeType.Block:
                    DefaultWalk((Block)node);
                    break;
                case AstNodeType.Assign:
                    DefaultWalk((AssignmentExpression)node);
                    break;
                case AstNodeType.GlobalVariable:
                case AstNodeType.LocalVariable:
                case AstNodeType.TemporaryVariable:
                    DefaultWalk((VariableExpression)node);
                    break;
                case AstNodeType.Parameter:
                    DefaultWalk((ParameterExpression)node);
                    break;
                case AstNodeType.BreakStatement:
                    DefaultWalk((BreakStatement)node);
                    break;
                case AstNodeType.Lambda:
                    DefaultWalk((LambdaExpression)node);
                    break;
                case AstNodeType.Generator:
                    DefaultWalk((GeneratorLambdaExpression)node);
                    break;
                case AstNodeType.CodeContextExpression:
                case AstNodeType.GeneratorIntrinsic:
                    DefaultWalk((IntrinsicExpression)node);
                    break;
                case AstNodeType.ContinueStatement:
                    DefaultWalk((ContinueStatement)node);
                    break;
                case AstNodeType.Delete:
                    DefaultWalk((DeleteExpression)node);
                    break;
                case AstNodeType.DoStatement:
                    DefaultWalk((DoStatement)node);
                    break;
                case AstNodeType.EmptyStatement:
                    DefaultWalk((EmptyStatement)node);
                    break;
                case AstNodeType.LabeledStatement:
                    DefaultWalk((LabeledStatement)node);
                    break;
                case AstNodeType.LoopStatement:
                    DefaultWalk((LoopStatement)node);
                    break;
                case AstNodeType.MemberExpression:
                    DefaultWalk((MemberExpression)node);
                    break;
                case AstNodeType.NewArrayExpression:
                case AstNodeType.NewArrayBounds:
                    DefaultWalk((NewArrayExpression)node);
                    break;
                case AstNodeType.ReturnStatement:
                    DefaultWalk((ReturnStatement)node);
                    break;
                case AstNodeType.ScopeStatement:
                    DefaultWalk((ScopeStatement)node);
                    break;
                case AstNodeType.SwitchStatement:
                    DefaultWalk((SwitchStatement)node);
                    break;
                case AstNodeType.ThrowStatement:
                    DefaultWalk((ThrowStatement)node);
                    break;
                case AstNodeType.TryStatement:
                    DefaultWalk((TryStatement)node);
                    break;
                case AstNodeType.YieldStatement:
                    DefaultWalk((YieldStatement)node);
                    break;
                case AstNodeType.Invoke:
                    DefaultWalk((InvocationExpression)node);
                    break;
            }
        }

        public void WalkNode(CatchBlock node) {
            DefaultWalk(node);
        }

        public void WalkNode(LambdaExpression node) {
            DefaultWalk(node);
        }

        public void WalkNode(GeneratorLambdaExpression node) {
            DefaultWalk(node);
        }

        public void WalkNode(IfStatementTest node) {
            DefaultWalk(node);
        }

        public void WalkNode(SwitchCase node) {
            DefaultWalk(node);
        }

        // ActionExpression
        private void DefaultWalk(ActionExpression node) {
            if (Walk(node)) {
                foreach (Expression ex in node.Arguments) {
                    WalkNode(ex);
                }
            }
            PostWalk(node);
        }

        // BinaryExpression
        private void DefaultWalk(BinaryExpression node) {
            if (Walk(node)) {
                WalkNode(node.Left);
                WalkNode(node.Right);
            }
            PostWalk(node);
        }

        // AssignmentExpression
        private void DefaultWalk(AssignmentExpression node) {
            if (Walk(node)) {
                WalkNode(node.Expression);
                WalkNode(node.Value);
            }
            PostWalk(node);
        }

        // VariableExpression
        private void DefaultWalk(VariableExpression node) {
            Walk(node);
            PostWalk(node);
        }

        // ParameterExpression 
        private void DefaultWalk(ParameterExpression node) {
            Walk(node);
            PostWalk(node);
        }

        // ConditionalExpression
        private void DefaultWalk(ConditionalExpression node) {
            if (Walk(node)) {
                WalkNode(node.Test);
                WalkNode(node.IfTrue);
                WalkNode(node.IfFalse);
            }
            PostWalk(node);
        }

        // ConstantExpression
        private void DefaultWalk(ConstantExpression node) {
            Walk(node);
            PostWalk(node);
        }

        // IntrinsicExpression
        private void DefaultWalk(IntrinsicExpression node) {
            Walk(node);
            PostWalk(node);
        }

        // MemberExpression
        private void DefaultWalk(MemberExpression node) {
            if (Walk(node)) {
                WalkNode(node.Expression);
            }
            PostWalk(node);
        }

        // MethodCallExpression
        private void DefaultWalk(MethodCallExpression node) {
            if (Walk(node)) {
                WalkNode(node.Instance);
                IList<Expression> args = node.Arguments;
                if (args != null) {
                    foreach (Expression e in args) {
                        WalkNode(e);
                    }
                }
            }
            PostWalk(node);
        }

        // InvocationExpression
        private void DefaultWalk(InvocationExpression node) {
            if (Walk(node)) {
                WalkNode(node.Expression);
                IList<Expression> args = node.Arguments;
                if (args != null) {
                    foreach (Expression e in args) {
                        WalkNode(e);
                    }
                }
            }
            PostWalk(node);
        }

        // NewArrayExpression
        private void DefaultWalk(NewArrayExpression node) {
            if (Walk(node)) {
                foreach (Expression expr in node.Expressions) {
                    WalkNode(expr);
                }
            }
            PostWalk(node);
        }

        // NewExpression
        private void DefaultWalk(NewExpression node) {
            if (Walk(node)) {
                IList<Expression> args = node.Arguments;
                if (args != null) {
                    foreach (Expression e in args) {
                        WalkNode(e);
                    }
                }
            }
            PostWalk(node);
        }

        // TypeBinaryExpression
        private void DefaultWalk(TypeBinaryExpression node) {
            if (Walk(node)) {
                WalkNode(node.Expression);
            }
            PostWalk(node);
        }

        // UnaryExpression
        private void DefaultWalk(UnaryExpression node) {
            if (Walk(node)) {
                WalkNode(node.Operand);
            }
            PostWalk(node);
        }

        // Block
        private void DefaultWalk(Block node) {
            if (Walk(node)) {
                foreach (Expression expr in node.Expressions) {
                    WalkNode(expr);
                }
            }
            PostWalk(node);
        }

        // BreakStatement
        private void DefaultWalk(BreakStatement node) {
            Walk(node);
            PostWalk(node);
        }

        // ContinueStatement
        private void DefaultWalk(ContinueStatement node) {
            Walk(node);
            PostWalk(node);
        }

        // DeleteExpression
        private void DefaultWalk(DeleteExpression node) {
            if (Walk(node)) {
                WalkNode(node.Expression);
            }
            PostWalk(node);
        }

        // DoStatement
        private void DefaultWalk(DoStatement node) {
            if (Walk(node)) {
                WalkNode(node.Body);
                WalkNode(node.Test);
            }
            PostWalk(node);
        }

        // EmptyStatement
        private void DefaultWalk(EmptyStatement node) {
            Walk(node);
            PostWalk(node);
        }

        // LabeledStatement
        private void DefaultWalk(LabeledStatement node) {
            if (Walk(node)) {
                WalkNode(node.Statement);
            }
            PostWalk(node);
        }

        // LoopStatement
        private void DefaultWalk(LoopStatement node) {
            if (Walk(node)) {
                WalkNode(node.Test);
                WalkNode(node.Increment);
                WalkNode(node.Body);
                WalkNode(node.ElseStatement);
            }
            PostWalk(node);
        }

        // ReturnStatement
        private void DefaultWalk(ReturnStatement node) {
            if (Walk(node)) {
                WalkNode(node.Expression);
            }
            PostWalk(node);
        }

        // ScopeStatement
        private void DefaultWalk(ScopeStatement node) {
            if (Walk(node)) {
                WalkNode(node.Body);
            }
            PostWalk(node);
        }

        // SwitchStatement
        private void DefaultWalk(SwitchStatement node) {
            if (Walk(node)) {
                WalkNode(node.TestValue);
                foreach (SwitchCase sc in node.Cases) {
                    WalkNode(sc.Body);
                }
            }
            PostWalk(node);
        }

        // ThrowStatement
        private void DefaultWalk(ThrowStatement node) {
            if (Walk(node)) {
                WalkNode(node.Value);
            }
            PostWalk(node);
        }

        // TryStatement
        private void DefaultWalk(TryStatement node) {
            if (Walk(node)) {
                WalkNode(node.Body);
                if (node.Handlers != null) {
                    foreach (CatchBlock handler in node.Handlers) {
                        if (handler.Filter != null) {
                            WalkNode(handler.Filter);
                        }
                        WalkNode(handler);
                    }
                }
                WalkNode(node.FinallyStatement);
                WalkNode(node.FaultStatement);
            }
            PostWalk(node);
        }

        // YieldStatement
        private void DefaultWalk(YieldStatement node) {
            if (Walk(node)) {
                WalkNode(node.Expression);
            }
            PostWalk(node);
        }

        // CatchBlock
        private void DefaultWalk(CatchBlock node) {
            if (Walk(node)) {
                WalkNode(node.Body);
            }
            PostWalk(node);
        }

        // LambdaExpression
        private void DefaultWalk(LambdaExpression node) {
            if (Walk(node)) {
                WalkNode(node.Body);
            }
            PostWalk(node);
        }

        // GeneratorLambdaExpression
        private void DefaultWalk(GeneratorLambdaExpression node) {
            if (Walk(node)) {
                WalkNode(node.Body);
            }
            PostWalk(node);
        }

        // IfStatementTest
        private void DefaultWalk(IfStatementTest node) {
            if (Walk(node)) {
                WalkNode(node.Test);
                WalkNode(node.Body);
            }
            PostWalk(node);
        }

        // SwitchCase
        private void DefaultWalk(SwitchCase node) {
            if (Walk(node)) {
                WalkNode(node.Body);
            }
            PostWalk(node);
        }
    }
}
