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
using System.Collections.ObjectModel;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// Base class for visiting and rewriting trees. Subclasses can override
    /// individual Visit methods from which they can return rewritten nodes.
    /// If a node is rewritten, all parent nodes will be rewritten.
    /// 
    /// TODO: rename back to ExpressionVisitor
    /// TODO: copy into Microsoft.Scripting, make this one internal
    /// </summary>
    public partial class ExpressionTreeVisitor {

        public Expression VisitNode(Expression node) {
            if (node == null) {
                return null;
            }

            return _Visitors[(int)node.NodeType](this, node);
        }

        protected ReadOnlyCollection<Expression> VisitNodes(ReadOnlyCollection<Expression> nodes) {
            Expression[] newNodes = null;
            for (int i = 0, n = nodes.Count; i < n; i++) {
                Expression node = nodes[i];
                if (node == null) {
                    continue;
                }
                // inlined to save stack space
                Expression e = _Visitors[(int)node.NodeType](this, node);
                if (newNodes != null) {
                    newNodes[i] = e;
                } else if (!object.ReferenceEquals(e, node)) {
                    newNodes = new Expression[n];
                    for (int j = 0; j < i; j++) {
                        newNodes[j] = nodes[j];
                    }
                    newNodes[i] = e;
                }
            }
            if (newNodes == null) {
                return nodes;
            }
            return new ReadOnlyCollection<Expression>(newNodes);
        }

        /// <summary>
        /// Visits all nodes in the collection using a specified element visitor.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="nodes">Input collection.</param>
        /// <param name="elementVisitor">Delegate that visits a single element.</param>
        /// <returns>Collection of visited nodes. Original collection is returned if no nodes were modified.</returns>
        protected static ReadOnlyCollection<T> VisitNodes<T>(ReadOnlyCollection<T> nodes, Func<T, T> elementVisitor) {
            T[] newNodes = null;
            for (int i = 0, n = nodes.Count; i < n; i++) {
                T newNode = elementVisitor(nodes[i]);
                if (newNodes != null) {
                    newNodes[i] = newNode;
                } else if (!object.ReferenceEquals(newNode, nodes[i])) {
                    newNodes = new T[n];
                    for (int j = 0; j < i; j++) {
                        newNodes[j] = nodes[j];
                    }
                    newNodes[i] = newNode;
                }
            }
            if (newNodes == null) {
                return nodes;
            }
            return new ReadOnlyCollection<T>(newNodes);
        }


        #region Individual Expression Visitors

        protected virtual Expression Visit(ActionExpression node) {
            ReadOnlyCollection<Expression> a = VisitNodes(node.Arguments);
            if (a == node.Arguments) {
                return node;
            }
            return new ActionExpression(node.Annotations, node.BindingInfo, a, node.Type);
        }

        protected virtual Expression Visit(AssignmentExpression node) {
            Expression e = VisitNode(node.Expression);
            Expression v = VisitNode(node.Value);
            if (e == node.Expression && v == node.Value) {
                return node;
            }
            return new AssignmentExpression(node.Annotations, e, v, node.Type, node.BindingInfo);
        }

        protected virtual Expression Visit(BinaryExpression node) {
            // Evaluation order: left, conversion, right
            Expression l = VisitNode(node.Left);
            LambdaExpression c = (LambdaExpression)VisitNode(node.Conversion);
            Expression r = VisitNode(node.Right);
            if (l == node.Left && r == node.Right) {
                return node;
            }
            return new BinaryExpression(node.Annotations, node.NodeType, l, r, node.Type, node.Method, node.Conversion, node.BindingInfo);
        }

        protected virtual Expression Visit(Block node) {
            ReadOnlyCollection<Expression> e = VisitNodes(node.Expressions);
            if (e == node.Expressions) {
                return node;
            }
            return new Block(node.Annotations, e, node.Type);
        }

        protected virtual Expression Visit(BreakStatement node) {
            return node;
        }

        protected virtual Expression Visit(ConditionalExpression node) {
            Expression t = VisitNode(node.Test);
            Expression l = VisitNode(node.IfTrue);
            Expression r = VisitNode(node.IfFalse);
            if (t == node.Test && l == node.IfTrue && r == node.IfFalse) {
                return node;
            }
            return new ConditionalExpression(node.Annotations, t, l, r, node.Type);
        }

        protected virtual Expression Visit(ConstantExpression node) {
            return node;
        }

        protected virtual Expression Visit(ContinueStatement node) {
            return node;
        }

        protected virtual Expression Visit(DeleteExpression node) {
            Expression e = VisitNode(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return new DeleteExpression(node.Annotations, e, node.BindingInfo);
        }

        protected virtual Expression Visit(DoStatement node) {
            Expression t = VisitNode(node.Test);
            Expression b = VisitNode(node.Body);
            if (t == node.Test && b == node.Body) {
                return node;
            }
            return new DoStatement(node.Annotations, node.Label, t, b);
        }

        protected virtual Expression Visit(EmptyStatement node) {
            return node;
        }

        protected virtual Expression VisitExtension(Expression node) {
            return node;
        }

        protected virtual Expression Visit(LocalScopeExpression node) {
            ReadOnlyCollection<Expression> v = VisitNodes(node.Variables);
            if (v == node.Variables) {
                return node;
            }
            // go through the factory for validation
            return Expression.AllVariables(node.Annotations, v);
        }

        protected virtual Expression Visit(InvocationExpression node) {
            Expression e = VisitNode(node.Expression);
            ReadOnlyCollection<Expression> a = VisitNodes(node.Arguments);
            if (e == node.Expression && a == node.Arguments) {
                return node;
            }
            return new InvocationExpression(node.Annotations, e, node.Type, node.BindingInfo, a);
        }

        protected virtual Expression Visit(LabeledStatement node) {
            Expression s = VisitNode(node.Statement);
            if (s == node.Statement) {
                return node;
            }
            return new LabeledStatement(node.Annotations, node.Label, s);
        }

        protected virtual Expression Visit(LambdaExpression node) {
            Expression b = VisitNode(node.Body);
            if (b == node.Body) {
                return node;
            }
            return Expression.Lambda(
                node.Annotations,
                node.NodeType,
                node.Type,
                node.Name,
                b,
                node.Parameters
            );
        }

        protected virtual Expression Visit(LoopStatement node) {
            Expression t = VisitNode(node.Test);
            Expression i = VisitNode(node.Increment);
            Expression b = VisitNode(node.Body);
            Expression e = VisitNode(node.ElseStatement);
            if (t == node.Test &&
                i == node.Increment &&
                b == node.Body &&
                e == node.ElseStatement) {
                return node;
            }
            return new LoopStatement(node.Annotations, node.Label, t, i, b, e);
        }

        protected virtual Expression Visit(MemberExpression node) {
            Expression e = VisitNode(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return new MemberExpression(node.Annotations, node.Member, e, node.Type, node.BindingInfo);
        }

        protected virtual Expression Visit(IndexedPropertyExpression node) {
            Expression o = VisitNode(node.Object);
            ReadOnlyCollection<Expression> a = VisitNodes(node.Arguments);
            if (o == node.Object && a == node.Arguments) {
                return node;
            }
            return new IndexedPropertyExpression(node.Annotations, o, node.GetMethod, node.SetMethod, a, node.Type, node.BindingInfo);
        }

        protected virtual Expression Visit(MethodCallExpression node) {
            Expression o = VisitNode(node.Object);
            ReadOnlyCollection<Expression> a = VisitNodes(node.Arguments);
            if (o == node.Object && a == node.Arguments) {
                return node;
            }
            return new MethodCallExpression(node.Annotations, node.Type, node.BindingInfo, node.Method, o, a);
        }

        protected virtual Expression Visit(NewArrayExpression node) {
            ReadOnlyCollection<Expression> e = VisitNodes(node.Expressions);
            if (e == node.Expressions) {
                return node;
            }
            return new NewArrayExpression(node.Annotations, node.NodeType, node.Type, e);
        }

        protected virtual Expression Visit(NewExpression node) {
            ReadOnlyCollection<Expression> a = VisitNodes(node.Arguments);
            if (a == node.Arguments) {
                return node;
            }
            return new NewExpression(node.Annotations, node.Type, node.Constructor, a, node.BindingInfo);
        }

        protected virtual Expression Visit(ParameterExpression node) {
            return node;
        }

        protected virtual Expression Visit(ReturnStatement node) {
            Expression e = VisitNode(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return new ReturnStatement(node.Annotations, e);
        }

        protected virtual Expression Visit(ScopeExpression node) {
            Expression b = VisitNode(node.Body);
            if (b == node.Body) {
                return node;
            }
            return new ScopeExpression(b, node.Name, node.Annotations, node.Variables);
        }

        protected virtual SwitchCase Visit(SwitchCase node) {
            Expression b = VisitNode(node.Body);
            if (b == node.Body) {
                return node;
            }
            return new SwitchCase(node.IsDefault, node.Value, b);
        }

        protected virtual Expression Visit(SwitchStatement node) {
            Expression t = VisitNode(node.TestValue);
            ReadOnlyCollection<SwitchCase> c = VisitNodes(node.Cases, Visit);

            if (t == node.TestValue && c == node.Cases) {
                return node;
            }

            return new SwitchStatement(node.Annotations, node.Label, t, c);
        }

        protected virtual Expression Visit(ThrowStatement node) {
            Expression v = VisitNode(node.Value);
            if (v == node.Value) {
                return node;
            }
            return new ThrowStatement(node.Annotations, v);
        }

        protected virtual CatchBlock Visit(CatchBlock node) {
            // TODO: change CatchBlock.Variable to any lvalue?
            VariableExpression v = (VariableExpression)VisitNode(node.Variable);
            Expression b = VisitNode(node.Body);
            Expression f = VisitNode(node.Filter);
            if (v == node.Variable && b == node.Body && f == node.Filter) {
                return node;
            }
            return new CatchBlock(node.Annotations, node.Test, v, b, f);
        }

        protected virtual Expression Visit(TryStatement node) {
            ReadOnlyCollection<CatchBlock> h = VisitNodes(node.Handlers, Visit);
            Expression b = VisitNode(node.Body);
            Expression y = VisitNode(node.Finally);
            Expression f = VisitNode(node.Fault);

            if (b == node.Body &&
                h == node.Handlers &&
                y == node.Finally &&
                f == node.Fault) {
                return node;
            }

            return new TryStatement(node.Annotations, b, h, y, f);
        }

        protected virtual Expression Visit(TypeBinaryExpression node) {
            Expression e = VisitNode(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return new TypeBinaryExpression(node.Annotations, node.NodeType, e, node.TypeOperand);
        }

        protected virtual Expression Visit(UnaryExpression node) {
            Expression o = VisitNode(node.Operand);
            if (o == node.Operand) {
                return node;
            }
            return new UnaryExpression(node.Annotations, node.NodeType, o, node.Type, node.Method, node.BindingInfo);
        }

        protected virtual Expression Visit(VariableExpression node) {
            return node;
        }

        protected virtual Expression Visit(YieldStatement node) {
            Expression e = VisitNode(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return new YieldStatement(node.Annotations, e);
        }

        protected virtual Expression Visit(MemberInitExpression node) {
            //inlined visit to New as it has to remain NewExpression
            NewExpression n = node.NewExpression;
            ReadOnlyCollection<Expression> a = VisitNodes(n.Arguments);
            if (a != n.Arguments) {
                n = new NewExpression(n.Annotations, n.Type, n.Constructor, a, n.BindingInfo);
            }

            ReadOnlyCollection<MemberBinding> bindings = VisitNodes(node.Bindings, Visit);
            if (n != node.NewExpression || bindings != node.Bindings) {
                return Expression.MemberInit(n, bindings);
            }
            return node;
        }

        protected virtual Expression Visit(ListInitExpression node) {
            //inlined visit to New as it has to remain NewExpression
            NewExpression n = node.NewExpression;
            ReadOnlyCollection<Expression> a = VisitNodes(n.Arguments);
            if (a != n.Arguments) {
                n = new NewExpression(n.Annotations, n.Type, n.Constructor, a, n.BindingInfo);
            }

            ReadOnlyCollection<ElementInit> initializers = VisitNodes(node.Initializers, Visit);
            if (n != node.NewExpression || initializers != node.Initializers) {
                return Expression.ListInit(n, initializers);
            }
            return node;
        }

        protected virtual ElementInit Visit(ElementInit initializer) {
            ReadOnlyCollection<Expression> arguments = VisitNodes(initializer.Arguments);
            if (arguments != initializer.Arguments) {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        protected virtual MemberBinding Visit(MemberBinding binding) {
            switch (binding.BindingType) {
                case MemberBindingType.Assignment:
                    return Visit((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return Visit((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return Visit((MemberListBinding)binding);
                default:
                    throw Error.UnhandledBindingType(binding.BindingType);
            }
        }

        protected virtual MemberAssignment Visit(MemberAssignment assignment) {
            Expression e = VisitNode(assignment.Expression);
            if (e != assignment.Expression) {
                return Expression.Bind(assignment.Member, e);
            }
            return assignment;
        }

        protected virtual MemberMemberBinding Visit(MemberMemberBinding binding) {
            ReadOnlyCollection<MemberBinding> bindings = VisitNodes(binding.Bindings, Visit);
            if (bindings != binding.Bindings) {
                return Expression.MemberBind(binding.Member, bindings);
            }
            return binding;
        }

        protected virtual MemberListBinding Visit(MemberListBinding binding) {
            ReadOnlyCollection<ElementInit> initializers = VisitNodes(binding.Initializers, Visit);
            if (initializers != binding.Initializers) {
                return Expression.ListBind(binding.Member, initializers);
            }
            return binding;
        }

        #endregion

        #region helpers

        // Helper to add a variable to a scope
        protected static Expression AddScopedVariable(Expression body, VariableExpression variable, Expression variableInit) {
            List<VariableExpression> vars = new List<VariableExpression>();
            string name = null;
            Annotations annotations = Annotations.Empty;
            while (body.NodeType == ExpressionType.Scope) {
                ScopeExpression scope = (ScopeExpression)body;
                vars.AddRange(scope.Variables);
                name = scope.Name;
                annotations = scope.Annotations;
                body = scope.Body;
            }

            vars.Add(variable);

            return Expression.Scope(
                Expression.Comma(
                    Expression.Assign(variable, variableInit),
                    body
                ),
                name,
                annotations,
                vars
            );
        }

        #endregion
    }
}
