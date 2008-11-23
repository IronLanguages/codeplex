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
    /// 
    /// TODO: rename back to ExpressionVisitor (fix the Linq test that has a copy)
    /// TODO: needs public API vetting
    /// </summary>
    public abstract class ExpressionTreeVisitor {

        public Expression Visit(Expression node) {
            return (node == null) ?  null : node.Accept(this);
        }

        protected ReadOnlyCollection<Expression> Visit(ReadOnlyCollection<Expression> nodes) {
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


        #region Individual Expression Visitors

        protected internal virtual Expression VisitAssignment(AssignmentExpression node) {
            Expression e = Visit(node.Expression);
            Expression v = Visit(node.Value);
            if (e == node.Expression && v == node.Value) {
                return node;
            }
            return Expression.Assign(e, v, node.Annotations);
        }

        protected internal virtual Expression VisitBinary(BinaryExpression node) {
            // Walk children in evaluation order: left, conversion, right
            Expression l = Visit(node.Left);
            LambdaExpression c = VisitAndConvert(node.Conversion, "VisitBinary");
            Expression r = Visit(node.Right);
            if (l == node.Left && r == node.Right && c == node.Conversion) {
                return node;
            }
            return Expression.MakeBinary(node.NodeType, l, r, node.IsLiftedToNull, node.Method, c, node.Annotations);
        }

        protected internal virtual Expression VisitBlock(Block node) {
            ReadOnlyCollection<Expression> e = Visit(node.Expressions);
            if (e == node.Expressions) {
                return node;
            }
            if (node.Type == typeof(void)) {
                return Expression.Block(node.Annotations, e);
            } else {
                return Expression.Comma(node.Annotations, e);
            }
        }

        protected internal virtual Expression VisitConditional(ConditionalExpression node) {
            Expression t = Visit(node.Test);
            Expression l = Visit(node.IfTrue);
            Expression r = Visit(node.IfFalse);
            if (t == node.Test && l == node.IfTrue && r == node.IfFalse) {
                return node;
            }
            return Expression.Condition(t, l, r, node.Annotations);
        }

        protected internal virtual Expression VisitConstant(ConstantExpression node) {
            return node;
        }

        protected internal virtual Expression VisitDoWhile(DoStatement node) {
            LabelTarget @break = VisitLabelTarget(node.BreakLabel);
            LabelTarget @continue = VisitLabelTarget(node.ContinueLabel);
            Expression t = Visit(node.Test);
            Expression e = Visit(node.Body);
            if (@break == node.BreakLabel && @continue == node.ContinueLabel && t == node.Test && e == node.Body) {
                return node;
            }
            return Expression.DoWhile(e, t, @break, @continue, node.Annotations);
        }

        protected internal virtual Expression VisitDynamic(DynamicExpression node) {
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (a == node.Arguments) {
                return node;
            }
            return Expression.MakeDynamic(node.DelegateType, node.Binder, node.Annotations, a);
        }

        protected internal virtual Expression VisitEmpty(EmptyStatement node) {
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
            return Expression.MakeGoto(node.Kind, t, v, node.Annotations);
        }

        protected internal virtual Expression VisitInvocation(InvocationExpression node) {
            Expression e = Visit(node.Expression);
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (e == node.Expression && a == node.Arguments) {
                return node;
            }
            return Expression.Invoke(e, node.Annotations, a);
        }

        protected virtual LabelTarget VisitLabelTarget(LabelTarget node) {
            return node;
        }

        protected internal virtual Expression VisitLabel(LabelExpression node) {
            LabelTarget l = VisitLabelTarget(node.Label);
            Expression d = Visit(node.DefaultValue);
            if (l == node.Label && d == node.DefaultValue) {
                return node;
            }
            return Expression.Label(l, d, node.Annotations);
        }

        protected internal virtual Expression VisitLambda(LambdaExpression node) {
            Expression b = Visit(node.Body);
            var p = VisitAndConvert(node.Parameters, "VisitLambda");
            if (b == node.Body && p == node.Parameters) {
                return node;
            }
            return Expression.Lambda(node.NodeType, node.Type, node.Name, b, node.Annotations, p);
        }

        protected internal virtual Expression VisitLoop(LoopStatement node) {
            LabelTarget @break = VisitLabelTarget(node.BreakLabel);
            LabelTarget @continue = VisitLabelTarget(node.ContinueLabel);
            Expression t = Visit(node.Test);
            Expression i = Visit(node.Increment);
            Expression b = Visit(node.Body);
            Expression e = Visit(node.ElseStatement);
            if (@break == node.BreakLabel &&
                @continue == node.ContinueLabel &&
                t == node.Test &&
                i == node.Increment &&
                b == node.Body &&
                e == node.ElseStatement) {
                return node;
            }
            return Expression.Loop(t, i, b, e, @break, @continue, node.Annotations);
        }

        protected internal virtual Expression VisitMemberAccess(MemberExpression node) {
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return Expression.MakeMemberAccess(e, node.Member, node.Annotations);
        }

        protected internal virtual Expression VisitIndex(IndexExpression node) {
            Expression o = Visit(node.Object);
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (o == node.Object && a == node.Arguments) {
                return node;
            }
            return Expression.MakeIndex(o, node.Indexer, node.Annotations, a);
        }

        protected internal virtual Expression VisitMethodCall(MethodCallExpression node) {
            Expression o = Visit(node.Object);
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (o == node.Object && a == node.Arguments) {
                return node;
            }
            return Expression.Call(o, node.Method, node.Annotations, a);
        }

        protected internal virtual Expression VisitNewArray(NewArrayExpression node) {
            ReadOnlyCollection<Expression> e = Visit(node.Expressions);
            if (e == node.Expressions) {
                return node;
            }
            if (node.NodeType == ExpressionType.NewArrayInit) {
                return Expression.NewArrayInit(node.Type.GetElementType(), node.Annotations, e);
            }
            return Expression.NewArrayBounds(node.Type.GetElementType(), node.Annotations, e);
        }

        protected internal virtual Expression VisitNew(NewExpression node) {
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (a == node.Arguments) {
                return node;
            }
            if (node.Members != null) {
                return Expression.New(node.Constructor, a, node.Annotations, node.Members);
            }
            return Expression.New(node.Constructor, node.Annotations, a);
        }

        protected internal virtual Expression VisitParameter(ParameterExpression node) {
            return node;
        }

        protected internal virtual Expression VisitReturn(ReturnStatement node) {
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
#pragma warning disable 618
            return Expression.Return(e, node.Annotations);
#pragma warning restore 618
        }

        protected internal virtual Expression VisitRuntimeVariables(LocalScopeExpression node) {
            var v = VisitAndConvert(node.Variables, "VisitRuntimeVariables");
            if (v == node.Variables) {
                return node;
            }
            return Expression.AllVariables(node.Annotations, v);
        }

        protected internal virtual Expression VisitScope(ScopeExpression node) {
            Expression b = Visit(node.Body);
            var v = VisitAndConvert(node.Variables, "VisitScope");
            if (b == node.Body) {
                return node;
            }
            return Expression.Scope(b, node.Name, node.Annotations, v);
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

        protected internal virtual Expression VisitSwitch(SwitchStatement node) {
            LabelTarget l = VisitLabelTarget(node.Label);
            Expression t = Visit(node.TestValue);
            ReadOnlyCollection<SwitchCase> c = Visit(node.Cases, VisitSwitchCase);

            if (l == node.Label && t == node.TestValue && c == node.Cases) {
                return node;
            }
            return Expression.Switch(t, l, node.Annotations, c);
        }

        protected internal virtual Expression VisitThrow(ThrowStatement node) {
            Expression v = Visit(node.Value);
            if (v == node.Value) {
                return node;
            }
            return Expression.Throw(v, node.Annotations);
        }

        protected virtual CatchBlock VisitCatchBlock(CatchBlock node) {
            ParameterExpression v = VisitAndConvert(node.Variable, "VisitCatchBlock");
            Expression f = Visit(node.Filter);
            Expression b = Visit(node.Body);
            if (v == node.Variable && b == node.Body && f == node.Filter) {
                return node;
            }
            return Expression.Catch(node.Test, v, b, f, node.Annotations);
        }

        protected internal virtual Expression VisitTry(TryStatement node) {
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
            return Expression.MakeTry(b, y, f, node.Annotations, h);
        }

        protected internal virtual Expression VisitTypeBinary(TypeBinaryExpression node) {
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return Expression.TypeIs(e, node.TypeOperand, node.Annotations);
        }

        protected internal virtual Expression VisitUnary(UnaryExpression node) {
            Expression o = Visit(node.Operand);
            if (o == node.Operand) {
                return node;
            }
            return Expression.MakeUnary(node.NodeType, o, node.Type, node.Method, node.Annotations);
        }

        protected internal virtual Expression VisitMemberInit(MemberInitExpression node) {
            NewExpression n = VisitAndConvert(node.NewExpression, "VisitMemberInit");
            ReadOnlyCollection<MemberBinding> bindings = Visit(node.Bindings, VisitMemberBinding);
            if (n == node.NewExpression && bindings == node.Bindings) {
                return node;
            }
            return Expression.MemberInit(n, node.Annotations, bindings);
        }

        protected internal virtual Expression VisitListInit(ListInitExpression node) {
            NewExpression n = VisitAndConvert(node.NewExpression, "VisitListInit");
            ReadOnlyCollection<ElementInit> initializers = Visit(node.Initializers, VisitElementInit);
            if (n == node.NewExpression && initializers == node.Initializers) {
                return node;
            }
            return Expression.ListInit(n, node.Annotations, initializers);
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

        #endregion        
    }
}
