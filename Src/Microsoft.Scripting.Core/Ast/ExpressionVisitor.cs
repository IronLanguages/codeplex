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


using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// Base class for visiting and rewriting trees. Subclasses can override
    /// individual Visit methods from which they can return rewritten nodes.
    /// If a node is rewritten, all parent nodes will be rewritten
    /// automatically.
    /// </summary>
    public abstract class ExpressionVisitor {

        public Expression Visit(Expression node) {
            return (node == null) ? null : node.Accept(this);
        }

        public ReadOnlyCollection<Expression> Visit(ReadOnlyCollection<Expression> nodes) {
            Expression[] newNodes = null;
            for (int i = 0, n = nodes.Count; i < n; i++) {
                Expression node = nodes[i].Accept(this);

                if (newNodes != null) {
                    newNodes[i] = node;
                } else if (!object.ReferenceEquals(node, nodes[i])) {
                    newNodes = new Expression[n];
                    for (int j = 0; j < i; j++) {
                        newNodes[j] = nodes[j];
                    }
                    newNodes[i] = node;
                }
            }
            if (newNodes == null) {
                return nodes;
            }
            return new ReadOnlyCollection<Expression>(newNodes);
        }

        internal Expression[] VisitArguments(IArgumentProvider nodes) {
            Expression[] newNodes = null;
            for (int i = 0, n = nodes.ArgumentCount; i < n; i++) {
                Expression curNode = nodes.GetArgument(i);
                Expression node = curNode.Accept(this);

                if (newNodes != null) {
                    newNodes[i] = node;
                } else if (!object.ReferenceEquals(node, curNode)) {
                    newNodes = new Expression[n];
                    for (int j = 0; j < i; j++) {
                        newNodes[j] = nodes.GetArgument(j);
                    }
                    newNodes[i] = node;
                }
            }
            return newNodes;
        }

        /// <summary>
        /// Visits all nodes in the collection using a specified element visitor.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="nodes">Input collection.</param>
        /// <param name="elementVisitor">Delegate that visits a single element.</param>
        /// <returns>Collection of visited nodes. Original collection is returned if no nodes were modified.</returns>
        protected static ReadOnlyCollection<T> Visit<T>(ReadOnlyCollection<T> nodes, Func<T, T> elementVisitor) {
            T[] newNodes = null;
            for (int i = 0, n = nodes.Count; i < n; i++) {
                T node = elementVisitor(nodes[i]);
                if (newNodes != null) {
                    newNodes[i] = node;
                } else if (!object.ReferenceEquals(node, nodes[i])) {
                    newNodes = new T[n];
                    for (int j = 0; j < i; j++) {
                        newNodes[j] = nodes[j];
                    }
                    newNodes[i] = node;
                }
            }
            if (newNodes == null) {
                return nodes;
            }
            return new ReadOnlyCollection<T>(newNodes);
        }

        protected T VisitAndConvert<T>(T node, string callerName) where T : Expression {
            if (node == null) {
                return null;
            }
            node = node.Accept(this) as T;
            if (node == null) {
                throw Error.MustRewriteToSameType(callerName, typeof(T), callerName);
            }
            return node;
        }

        /// <summary>
        /// Visits all of the nodes in the collection, and tries to convert each
        /// result back to the original type. If any conversion fails, it
        /// throws an error
        /// </summary>
        protected ReadOnlyCollection<T> VisitAndConvert<T>(ReadOnlyCollection<T> nodes, string callerName) where T : Expression {
            T[] newNodes = null;
            for (int i = 0, n = nodes.Count; i < n; i++) {
                T node = nodes[i].Accept(this) as T;
                if (node == null) {
                    throw Error.MustRewriteToSameType(callerName, typeof(T), callerName);
                }

                if (newNodes != null) {
                    newNodes[i] = node;
                } else if (!object.ReferenceEquals(node, nodes[i])) {
                    newNodes = new T[n];
                    for (int j = 0; j < i; j++) {
                        newNodes[j] = nodes[j];
                    }
                    newNodes[i] = node;
                }
            }
            if (newNodes == null) {
                return nodes;
            }
            return new ReadOnlyCollection<T>(newNodes);
        }

        protected internal virtual Expression VisitBinary(BinaryExpression node) {
            // Walk children in evaluation order: left, conversion, right
            Expression l = Visit(node.Left);
            LambdaExpression c = VisitAndConvert(node.Conversion, "VisitBinary");
            Expression r = Visit(node.Right);
            if (l == node.Left && r == node.Right && c == node.Conversion) {
                return node;
            }
            return Expression.MakeBinary(node.NodeType, l, r, node.IsLiftedToNull, node.Method, c);
        }

        protected internal virtual Expression VisitBlock(BlockExpression node) {
            int count = node.ExpressionCount;
            Expression[] nodes = null;
            for (int i = 0; i < count; i++) {
                Expression oldNode = node.GetExpression(i);
                Expression newNode = Visit(oldNode);

                if (oldNode != newNode) {
                    if (nodes == null) {
                        nodes = new Expression[count];
                    }
                    nodes[i] = newNode;
                }
            }
            var v = VisitAndConvert(node.Variables, "VisitBlock");

            if (v == node.Variables && nodes == null) {
                return node;
            } else {
                for (int i = 0; i < count; i++) {
                    if (nodes[i] == null) {
                        nodes[i] = node.GetExpression(i);
                    }
                }
            }

            return node.Rewrite(v, nodes);
        }

        protected internal virtual Expression VisitConditional(ConditionalExpression node) {
            Expression t = Visit(node.Test);
            Expression l = Visit(node.IfTrue);
            Expression r = Visit(node.IfFalse);
            if (t == node.Test && l == node.IfTrue && r == node.IfFalse) {
                return node;
            }
            return Expression.Condition(t, l, r);
        }

        protected internal virtual Expression VisitConstant(ConstantExpression node) {
            return node;
        }

        protected internal virtual Expression VisitDebugInfo(DebugInfoExpression node) {
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return Expression.DebugInfo(e, node.Document, node.StartLine, node.StartColumn, node.EndLine, node.EndColumn);
        }

        protected internal virtual Expression VisitDynamic(DynamicExpression node) {
            Expression[] a = VisitArguments((IArgumentProvider)node);
            if (a == null) {
                return node;
            }

            return node.Rewrite(a);
        }

        protected internal virtual Expression VisitDefault(DefaultExpression node) {
            return node;
        }

        /// <summary>
        /// Override called for Extension nodes. This can be overriden to
        /// rewrite certain extension nodes. If it's not overriden, this method
        /// will call into Expression.Visit, which gives the node a chance to
        /// walk its children
        /// </summary>
        protected internal virtual Expression VisitExtension(Expression node) {
            return node.VisitChildren(this);
        }

        protected internal virtual Expression VisitGoto(GotoExpression node) {
            LabelTarget t = VisitLabelTarget(node.Target);
            Expression v = Visit(node.Value);
            if (t == node.Target && v == node.Value) {
                return node;
            }
            return Expression.MakeGoto(node.Kind, t, v);
        }

        protected internal virtual Expression VisitInvocation(InvocationExpression node) {
            Expression e = Visit(node.Expression);
            Expression[] a = VisitArguments(node);
            if (e == node.Expression && a == null) {
                return node;
            }
            return Expression.Invoke(e, a);
        }

        protected virtual LabelTarget VisitLabelTarget(LabelTarget node) {
            return node;
        }

        protected internal virtual Expression VisitLabel(LabelExpression node) {
            LabelTarget l = VisitLabelTarget(node.Target);
            Expression d = Visit(node.DefaultValue);
            if (l == node.Target && d == node.DefaultValue) {
                return node;
            }
            return Expression.Label(l, d);
        }

        protected internal virtual Expression VisitLambda<T>(Expression<T> node) {
            Expression b = Visit(node.Body);
            var p = VisitAndConvert(node.Parameters, "VisitLambda");
            if (b == node.Body && p == node.Parameters) {
                return node;
            }
            return Expression.Lambda<T>(b, node.Name, p);
        }

        protected internal virtual Expression VisitLoop(LoopExpression node) {
            LabelTarget @break = VisitLabelTarget(node.BreakLabel);
            LabelTarget @continue = VisitLabelTarget(node.ContinueLabel);
            Expression b = Visit(node.Body);
            if (@break == node.BreakLabel &&
                @continue == node.ContinueLabel &&
                b == node.Body) {
                return node;
            }
            return Expression.Loop(b, @break, @continue);
        }

        protected internal virtual Expression VisitMember(MemberExpression node) {
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return Expression.MakeMemberAccess(e, node.Member);
        }

        protected internal virtual Expression VisitIndex(IndexExpression node) {
            Expression o = Visit(node.Object);
            IList<Expression> a = VisitArguments(node);
            if (o == node.Object && a == null) {
                return node;
            }
            return Expression.MakeIndex(o, node.Indexer, a);
        }

        protected internal virtual Expression VisitMethodCall(MethodCallExpression node) {
            Expression o = Visit(node.Object);
            Expression[] a = VisitArguments((IArgumentProvider)node);
            if (o == node.Object && a == null) {
                return node;
            }

            return node.Rewrite(o, a);
        }

        protected internal virtual Expression VisitNewArray(NewArrayExpression node) {
            ReadOnlyCollection<Expression> e = Visit(node.Expressions);
            if (e == node.Expressions) {
                return node;
            }
            if (node.NodeType == ExpressionType.NewArrayInit) {
                return Expression.NewArrayInit(node.Type.GetElementType(), e);
            }
            return Expression.NewArrayBounds(node.Type.GetElementType(), e);
        }

        protected internal virtual Expression VisitNew(NewExpression node) {
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (a == node.Arguments) {
                return node;
            }
            if (node.Members != null) {
                return Expression.New(node.Constructor, a, node.Members);
            }
            return Expression.New(node.Constructor, a);
        }

        protected internal virtual Expression VisitParameter(ParameterExpression node) {
            return node;
        }

        protected internal virtual Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
            var v = VisitAndConvert(node.Variables, "VisitRuntimeVariables");
            if (v == node.Variables) {
                return node;
            }
            return Expression.RuntimeVariables(v);
        }

        protected virtual SwitchCase VisitSwitchCase(SwitchCase node) {
            Expression b = Visit(node.Body);
            if (b == node.Body) {
                return node;
            }
            if (node.IsDefault) {
                return Expression.DefaultCase(b);
            }
            return Expression.SwitchCase(node.Value, b);
        }

        protected internal virtual Expression VisitSwitch(SwitchExpression node) {
            LabelTarget l = VisitLabelTarget(node.BreakLabel);
            Expression t = Visit(node.Test);
            ReadOnlyCollection<SwitchCase> c = Visit(node.SwitchCases, VisitSwitchCase);

            if (l == node.BreakLabel && t == node.Test && c == node.SwitchCases) {
                return node;
            }
            return Expression.Switch(t, l, c);
        }

        protected virtual CatchBlock VisitCatchBlock(CatchBlock node) {
            ParameterExpression v = VisitAndConvert(node.Variable, "VisitCatchBlock");
            Expression f = Visit(node.Filter);
            Expression b = Visit(node.Body);
            if (v == node.Variable && b == node.Body && f == node.Filter) {
                return node;
            }
            return Expression.MakeCatchBlock(node.Test, v, b, f);
        }

        protected internal virtual Expression VisitTry(TryExpression node) {
            Expression b = Visit(node.Body);
            ReadOnlyCollection<CatchBlock> h = Visit(node.Handlers, VisitCatchBlock);
            Expression y = Visit(node.Finally);
            Expression f = Visit(node.Fault);

            if (b == node.Body &&
                h == node.Handlers &&
                y == node.Finally &&
                f == node.Fault) {
                return node;
            }
            return Expression.MakeTry(b, y, f, h);
        }

        protected internal virtual Expression VisitTypeBinary(TypeBinaryExpression node) {
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return Expression.TypeIs(e, node.TypeOperand);
        }

        protected internal virtual Expression VisitUnary(UnaryExpression node) {
            Expression o = Visit(node.Operand);
            if (o == node.Operand) {
                return node;
            }
            return Expression.MakeUnary(node.NodeType, o, node.Type, node.Method);
        }

        protected internal virtual Expression VisitMemberInit(MemberInitExpression node) {
            NewExpression n = VisitAndConvert(node.NewExpression, "VisitMemberInit");
            ReadOnlyCollection<MemberBinding> bindings = Visit(node.Bindings, VisitMemberBinding);
            if (n == node.NewExpression && bindings == node.Bindings) {
                return node;
            }
            return Expression.MemberInit(n, bindings);
        }

        protected internal virtual Expression VisitListInit(ListInitExpression node) {
            NewExpression n = VisitAndConvert(node.NewExpression, "VisitListInit");
            ReadOnlyCollection<ElementInit> initializers = Visit(node.Initializers, VisitElementInit);
            if (n == node.NewExpression && initializers == node.Initializers) {
                return node;
            }
            return Expression.ListInit(n, initializers);
        }

        protected virtual ElementInit VisitElementInit(ElementInit initializer) {
            ReadOnlyCollection<Expression> arguments = Visit(initializer.Arguments);
            if (arguments == initializer.Arguments) {
                return initializer;
            }
            return Expression.ElementInit(initializer.AddMethod, arguments);
        }

        protected virtual MemberBinding VisitMemberBinding(MemberBinding binding) {
            switch (binding.BindingType) {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw Error.UnhandledBindingType(binding.BindingType);
            }
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment) {
            Expression e = Visit(assignment.Expression);
            if (e == assignment.Expression) {
                return assignment;
            }
            return Expression.Bind(assignment.Member, e);
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding) {
            ReadOnlyCollection<MemberBinding> bindings = Visit(binding.Bindings, VisitMemberBinding);
            if (bindings == binding.Bindings) {
                return binding;
            }
            return Expression.MemberBind(binding.Member, bindings);
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding) {
            ReadOnlyCollection<ElementInit> initializers = Visit(binding.Initializers, VisitElementInit);
            if (initializers == binding.Initializers) {
                return binding;
            }
            return Expression.ListBind(binding.Member, initializers);
        }
    }
}
