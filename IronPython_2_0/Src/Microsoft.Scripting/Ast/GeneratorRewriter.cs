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
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// When finding a yield return or yield break, this rewriter flattens out
    /// containing blocks, scopes, and expressions with stack state. All
    /// scopes encountered have their variables promoted to the generator's
    /// closure, so they survive yields.
    /// </summary>
    internal sealed class GeneratorRewriter : ExpressionTreeVisitor {
        // These two constants are used internally. They should not conflict
        // with valid yield states.
        private const int GotoRouterYielding = 0;
        private const int GotoRouterNone = -1;
        // The state of the generator before it starts and when it's done
        internal const int NotStarted = -1;
        internal const int Finished = 0;

        private sealed class YieldMarker {
            // Note: Label can be mutated as we generate try blocks
            internal LabelTarget Label = Expression.Label();
            internal readonly int State;

            internal YieldMarker(int state) {
                State = state;
            }
        }

        private readonly GeneratorExpression _generator;
        private readonly ParameterExpression _current;
        private readonly ParameterExpression _state;

        // The one return label, or more than one if we're in a finally
        private readonly Stack<LabelTarget> _returnLabels = new Stack<LabelTarget>();
        private ParameterExpression _gotoRouter;
        private bool _inTryWithFinally;

        private readonly List<YieldMarker> _yields = new List<YieldMarker>();

        private List<int> _debugCookies;

        private readonly List<ParameterExpression> _vars = new List<ParameterExpression>();
        
        // Possible optimization: reuse temps. Requires scoping them correctly,
        // and then storing them back in a free list
        private readonly List<ParameterExpression> _temps = new List<ParameterExpression>();

        internal GeneratorRewriter(GeneratorExpression generator) {
            _generator = generator;
            _state = Expression.Parameter(typeof(int).MakeByRefType(), "state");
            _current = Expression.Parameter(_generator.Label.Type.MakeByRefType(), "current");
            _returnLabels.Push(Expression.Label());
            _gotoRouter = Expression.Variable(typeof(int), "$gotoRouter");
        }

        internal Expression Reduce() {
            // Visit body
            Expression body = Visit(_generator.Body);
            Debug.Assert(_returnLabels.Count == 1);

            // Add the switch statement to the body
            int count = _yields.Count;
            var cases = new SwitchCase[count + 1];
            for (int i = 0; i < count; i++) {
                cases[i] = Expression.SwitchCase(_yields[i].State, Expression.Goto(_yields[i].Label));
            }
            cases[count] = Expression.SwitchCase(Finished, Expression.Goto(_returnLabels.Peek()));

            Type generatorNextOfT = typeof(GeneratorNext<>).MakeGenericType(_generator.Label.Type);

            // Create the lambda for the GeneratorNext<T>, hoisting variables
            // into a scope outside the lambda
            var allVars = new List<ParameterExpression>(_vars);
            allVars.AddRange(_temps);

            body = Expression.Scope(
                Expression.Lambda(
                    generatorNextOfT,
                    Expression.Scope(
                        Expression.Block(
                            Expression.Switch(Expression.Assign(_gotoRouter, _state), cases),
                            body,
                            Expression.Assign(_state, Expression.Constant(Finished)),
                            Expression.Label(_returnLabels.Peek())
                        ),
                        _gotoRouter
                    ),
                    _state,
                    _current
                ),
                allVars
            );

            // Enumerable factory takes Func<GeneratorNext<T>> instead of GeneratorNext<T>
            if (_generator.IsEnumerable) {
                body = Expression.Lambda(body);
            }

            // Generate a call to RuntimeHelpers.MakeGenerator<T>(args)
            return Expression.Call(
                typeof(RuntimeHelpers),
                "MakeGenerator",
                new[] { _generator.Label.Type },
                (_debugCookies != null)
                    ? new[] { body, Expression.Constant(_debugCookies) }
                    : new[] { body }
            );
        }

        private YieldMarker GetYieldMarker(YieldExpression node) {
            YieldMarker result = new YieldMarker(_yields.Count + 1);
            _yields.Add(result);
            YieldAnnotation debugCookie;
            if (node.Annotations.TryGet(out debugCookie)) {
                if (_debugCookies == null) {
                    _debugCookies = new List<int>();
                }
                _debugCookies.Insert(result.State, debugCookie.YieldMarker);
            }
            return result;
        }

        private AssignmentExpression ToTemp(ref Expression e) {
            Debug.Assert(e != null);
            var temp = Expression.Variable(e.Type, "$temp$" + _temps.Count);
            _temps.Add(temp);
            var result = Expression.Assign(temp, e);
            e = temp;
            return result;
        }

        private Block ToTemp(ref ReadOnlyCollection<Expression> args) {
            int count = args.Count;
            var block = new Expression[count];
            var newArgs = new Expression[count];
            args.CopyTo(newArgs, 0);
            for (int i = 0; i < count; i++) {
                block[i] = ToTemp(ref newArgs[i]);
            }
            args = new ReadOnlyCollection<Expression>(newArgs);
            return Expression.Block(block);
        }

        #region VisitTry

        protected override Expression VisitTry(TryStatement node) {
            int startYields = _yields.Count;

            bool savedInTryWithFinally = _inTryWithFinally;
            if (node.Finally != null || node.Fault != null) {
                _inTryWithFinally = true;
            }
            Expression @try = Visit(node.Body);
            int tryYields = _yields.Count;

            IList<CatchBlock> handlers = Visit(node.Handlers, VisitCatchBlock);
            int catchYields = _yields.Count;

            // push a new return label in case the finally block yields
            _returnLabels.Push(Expression.Label());
            // only one of these can be non-null
            Expression @finally = Visit(node.Finally);
            Expression fault = Visit(node.Fault);
            LabelTarget finallyReturn = _returnLabels.Pop();
            int finallyYields = _yields.Count;

            _inTryWithFinally = savedInTryWithFinally;

            if (@try == node.Body &&
                handlers == node.Handlers &&
                @finally == node.Finally &&
                fault == node.Fault) {
                return node;
            }

            // No yields, just return
            if (startYields == _yields.Count) {
                return Expression.MakeTry(@try, @finally, fault, node.Annotations, handlers);
            }

            if (fault != null && finallyYields != catchYields) {
                // No one needs this yet, and it's not clear how we should get back to
                // the fault
                throw new NotSupportedException("yield in fault block is not supported");
            }

            // If try has yields, we need to build a new try body that
            // dispatches to the yield labels
            var tryStart = Expression.Label();
            if (tryYields != startYields) {
                @try = Expression.Block(MakeYieldRouter(startYields, tryYields, tryStart), @try);
            }

            // Transform catches with yield to deferred handlers
            if (catchYields != tryYields) {
                // Temps which are only needed temporarily, so they can go into
                // a transient scope (contents will be lost each yield)
                var temps = new List<ParameterExpression>();
                var block = new List<Expression>();

                block.Add(MakeYieldRouter(tryYields, catchYields, tryStart));
                block.Add(null); // empty slot to fill in later

                for (int i = 0, n = handlers.Count; i < n; i++) {
                    CatchBlock c = handlers[i];

                    if (c == node.Handlers[i]) {
                        continue;
                    }
                    if (c.Filter != handlers[i].Filter) {
                        // No one needs this yet, and it's not clear what it should even do
                        throw new NotSupportedException("yield in filter test is not supported");
                    }

                    if (handlers.IsReadOnly) {
                        handlers = handlers.ToArray();
                    }

                    // TODO: when CatchBlock's variable is scoped properly, this
                    // implementation will need to be different
                    var deferredVar = Expression.Variable(c.Test, null);
                    temps.Add(deferredVar);
                    handlers[i] = Expression.Catch(c.Test, deferredVar, Expression.Empty(), c.Filter);

                    var catchBody = c.Body;
                    if (c.Variable != null) {
                        catchBody = Expression.Block(c.Annotations, Expression.Assign(c.Variable, deferredVar), catchBody);
                    } else {
                        catchBody = Expression.Block(c.Annotations, catchBody);
                    }

                    block.Add(
                        Expression.Condition(
                            Expression.NotEqual(deferredVar, Expression.Null(deferredVar.Type)),
                            catchBody,
                            Expression.Empty()
                        )
                    );
                }

                block[1] = Expression.MakeTry(@try, null, null, null, new ReadOnlyCollection<CatchBlock>(handlers));
                @try = Expression.Scope(Expression.Block(block), temps);
                handlers = new CatchBlock[0]; // so we don't reuse these
            }

            if (finallyYields != catchYields) {
                // We need to add a catch block to save the exception, so we
                // can rethrow in case there is a yield in the finally. Also,
                // add logic for returning. It looks like this:
                // try { ... } catch (Exception e) {}
                // finally {
                //  if (_finallyReturnVar) goto finallyReturn;
                //   ...
                //   if (e != null) throw e;
                //   finallyReturn:
                // }
                // if (_finallyReturnVar) goto _return;

                // We need to add a catch(Exception), so if we have catches,
                // wrap them in a try
                if (handlers.Count > 0) {
                    @try = Expression.MakeTry(@try, null, null, null, handlers);
                    handlers = new CatchBlock[0];
                }

                // NOTE: the order of these routers is important
                // The first call changes the labels to all point at "tryEnd",
                // so the second router will jump to "tryEnd"
                var tryEnd = Expression.Label();
                Expression inFinallyRouter = MakeYieldRouter(catchYields, finallyYields, tryEnd);
                Expression inTryRouter = MakeYieldRouter(catchYields, finallyYields, tryStart);

                var exception = Expression.Variable(typeof(Exception), "$temp$" + _temps.Count);
                _temps.Add(exception);
                @try = Expression.Block(
                    Expression.TryCatchFinally(
                        Expression.Block(
                            inTryRouter,
                            @try,
                            Expression.Assign(exception, Expression.Null(exception.Type)),
                            Expression.Label(tryEnd)
                        ),
                        Expression.Block(
                            MakeSkipFinallyBlock(finallyReturn),
                            inFinallyRouter,
                            @finally,
                            Expression.Condition(
                                Expression.NotEqual(exception, Expression.Null(exception.Type)),
                                Expression.Throw(exception),
                                Expression.Empty()
                            ),
                            Expression.Label(finallyReturn)
                        ),
                        Expression.Catch(exception.Type, exception, Expression.Empty())
                    ),
                    Expression.Condition(
                        Expression.Equal(_gotoRouter, Expression.Constant(GotoRouterYielding)),
                        Expression.Goto(_returnLabels.Peek()),
                        Expression.Empty()
                    )
                );

                @finally = null;
            } else if (@finally != null) {
                // try or catch had a yield, modify finally so we can skip over it
                @finally = Expression.Block(
                    MakeSkipFinallyBlock(finallyReturn),
                    @finally,
                    Expression.Label(finallyReturn)
                );
            }

            // Make the outer try, if needed
            if (handlers.Count > 0 || @finally != null || fault != null) {
                @try = Expression.MakeTry(@try, @finally, fault, null, handlers);
            }

            return Expression.Block(node.Annotations, Expression.Label(tryStart), @try);
        }

        // Skip the finally block if we are yielding, but not if we're doing a
        // yield break
        private Expression MakeSkipFinallyBlock(LabelTarget target) {
            return Expression.Condition(
                Expression.AndAlso(
                    Expression.Equal(_gotoRouter, Expression.Constant(GotoRouterYielding)),
                    Expression.NotEqual(_state, Expression.Constant(Finished))
                ),
                Expression.Goto(target),
                Expression.Empty()
            );
        }

        #endregion

        private SwitchStatement MakeYieldRouter(int start, int end, LabelTarget newTarget) {
            Debug.Assert(end > start);
            var cases = new SwitchCase[end - start];
            for (int i = start; i < end; i++) {
                YieldMarker y = _yields[i];
                cases[i - start] = Expression.SwitchCase(y.State, Expression.Goto(y.Label));
                // Any jumps from outer switch statements should go to the this
                // router, not the original label (which they cannot legally jump to)
                y.Label = newTarget;
            }
            return Expression.Switch(_gotoRouter, cases);
        }

        protected override Expression VisitExtension(Expression node) {
            var yield = node as YieldExpression;
            if (yield != null) {
                return VisitYield(yield);
            }

            // We need to reduce here, otherwise we can't guarentee proper
            // stack spilling of the resulting expression.
            // In effect, generators are one of the last rewrites that should
            // happen
            return Visit(node.ReduceExtensions());
        }

        // TODO: this can go away when ReturnStatement goes away
        protected override Expression VisitReturn(ReturnStatement node) {
            throw new InvalidOperationException("Cannot return value from generator using ReturnStatement, use YieldExpression instead");
        }

        private Expression VisitYield(YieldExpression node) {
            if (node.Target != _generator.Label) {
                throw new InvalidOperationException("yield and generator must have the same LabelTarget object");
            }

            var value = Visit(node.Value);

            var block = new List<Expression>();
            if (value == null) {
                // Yield break
                block.Add(Expression.Assign(_state, Expression.Constant(Finished)));
                if (_inTryWithFinally) {
                    block.Add(Expression.Assign(_gotoRouter, Expression.Constant(GotoRouterYielding)));
                }
                block.Add(Expression.Goto(_returnLabels.Peek()));
                return Expression.Block(node.Annotations, block);
            }

            // Yield return
            block.Add(Expression.Assign(_current, value));
            YieldMarker marker = GetYieldMarker(node);
            block.Add(Expression.Assign(_state, Expression.Constant(marker.State)));
            if (_inTryWithFinally) {
                block.Add(Expression.Assign(_gotoRouter, Expression.Constant(GotoRouterYielding)));
            }
            block.Add(Expression.Goto(_returnLabels.Peek()));
            block.Add(Expression.Label(marker.Label));
            block.Add(Expression.Assign(_gotoRouter, Expression.Constant(GotoRouterNone)));
            return Expression.Block(node.Annotations, block);
        }

        protected override Expression VisitScope(ScopeExpression node) {
            int yields = _yields.Count;
            var b = Visit(node.Body);
            if (b == node.Body) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.Scope(b, node.Name, node.Annotations, node.Variables);
            }

            // Remove scopes, save the variables for later
            // (they'll be hoisted outside of the lambda)
            _vars.AddRange(node.Variables);
            return b;
        }

        protected override Expression VisitLambda(LambdaExpression node) {
            // don't recurse into nested lambdas
            return node;
        }

        #region stack spilling (to permit yield in the middle of an expression)

        protected override Expression VisitAssignment(AssignmentExpression node) {
            int yields = _yields.Count;
            Expression left = Visit(node.Expression);
            Expression value = Visit(node.Value);
            if (left == node.Expression && value == node.Value) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.Assign(left, value, node.Annotations);
            }
            
            var block = new List<Expression>();

            // If the left hand side did not rewrite itself, we may still need
            // to rewrite to ensure proper evaluation order. Essentially, we
            // want all of the left side evaluated first, then the value, then
            // the assignment
            if (left == node.Expression) {
                switch (left.NodeType) {
                    case ExpressionType.MemberAccess:
                        var member = (MemberExpression)node.Expression;
                        Expression e = Visit(member.Expression);
                        block.Add(ToTemp(ref e));
                        left = Expression.MakeMemberAccess(e, member.Member, member.Annotations);
                        break;
                    case ExpressionType.Index:
                        var index = (IndexExpression)node.Expression;
                        Expression o = Visit(index.Object);
                        ReadOnlyCollection<Expression> a = Visit(index.Arguments);
                        if (o == index.Object && a == index.Arguments) {
                            return index;
                        }
                        block.Add(ToTemp(ref o));
                        block.Add(ToTemp(ref a));
                        left = Expression.MakeIndex(o, index.Indexer, index.Annotations, a);
                        break;
                    case ExpressionType.Parameter:
                        // no action needed
                        break;
                    default:
                        // Extension should've been reduced by Visit above,
                        // and returned a different node
                        throw Assert.Unreachable;
                }
            } else {
                // Get the last expression of the rewritten left side
                var leftBlock = (Block)left;
                left = leftBlock.Expressions[leftBlock.Expressions.Count - 1];
                block.AddRange(leftBlock.Expressions);
                block.RemoveAt(block.Count - 1);
            }
            Debug.Assert(left.CanWrite);

            if (value != node.Value) {
                block.Add(ToTemp(ref value));
            }

            block.Add(Expression.Assign(left, value, null));
            return Expression.Comma(node.Annotations, block);
        }

        protected override Expression VisitDynamic(DynamicExpression node) {
            int yields = _yields.Count;
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (a == node.Arguments) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.MakeDynamic(node.DelegateType, node.Binder, node.Annotations, a);
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref a),
                Expression.MakeDynamic(node.DelegateType, node.Binder, null, a)
            );
        }

        protected override Expression VisitIndex(IndexExpression node) {
            int yields = _yields.Count;
            Expression o = Visit(node.Object);
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (o == node.Object && a == node.Arguments) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.MakeIndex(o, node.Indexer, node.Annotations, a);
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref o),
                ToTemp(ref a),
                Expression.MakeIndex(o, node.Indexer, null, a)
            );
        }

        protected override Expression VisitInvocation(InvocationExpression node) {
            int yields = _yields.Count;
            Expression e = Visit(node.Expression);
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (e == node.Expression && a == node.Arguments) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.Invoke(e, node.Annotations, a);
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref e),
                ToTemp(ref a),
                Expression.Invoke(e, null, a)
            );
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            int yields = _yields.Count;
            Expression o = Visit(node.Object);
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (o == node.Object && a == node.Arguments) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.Call(o, node.Method, node.Annotations, a);
            }
            if (o == null) {
                return Expression.Comma(
                    node.Annotations,
                    ToTemp(ref a),
                    Expression.Call(null, node.Method, null, a)
                );
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref o),
                ToTemp(ref a),
                Expression.Call(o, node.Method, null, a)
            );
        }

        protected override Expression VisitNew(NewExpression node) {
            int yields = _yields.Count;
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (a == node.Arguments) {
                return node;
            }
            if (yields == _yields.Count) {
                return (node.Members != null)
                    ? Expression.New(node.Constructor, a, node.Annotations, node.Members)
                    : Expression.New(node.Constructor, node.Annotations, a);
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref a),
                (node.Members != null)
                    ? Expression.New(node.Constructor, a, null, node.Members)
                    : Expression.New(node.Constructor, null, a)
            );
        }

        protected override Expression VisitNewArray(NewArrayExpression node) {
            int yields = _yields.Count;
            ReadOnlyCollection<Expression> e = Visit(node.Expressions);
            if (e == node.Expressions) {
                return node;
            }
            if (yields == _yields.Count) {
                return (node.NodeType == ExpressionType.NewArrayInit)
                    ? Expression.NewArrayInit(node.Type.GetElementType(), node.Annotations, e)
                    : Expression.NewArrayBounds(node.Type.GetElementType(), node.Annotations, e);
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref e),
                (node.NodeType == ExpressionType.NewArrayInit)
                    ? Expression.NewArrayInit(node.Type.GetElementType(), null, e)
                    : Expression.NewArrayBounds(node.Type.GetElementType(), null, e)
            );
        }

        protected override Expression VisitMemberAccess(MemberExpression node) {
            int yields = _yields.Count;
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.MakeMemberAccess(e, node.Member, node.Annotations);
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref e),
                Expression.MakeMemberAccess(e, node.Member, null)
            );
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            int yields = _yields.Count;
            Expression left = Visit(node.Left);
            Expression right = Visit(node.Right);
            if (left == node.Left && right == node.Right) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method, node.Conversion, node.Annotations);
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref left),
                ToTemp(ref right),
                Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method, node.Conversion, null)
            );
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node) {
            int yields = _yields.Count;
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.TypeIs(e, node.TypeOperand, node.Annotations);
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref e),
                Expression.TypeIs(e, node.TypeOperand, null)
            );
        }

        protected override Expression VisitUnary(UnaryExpression node) {
            int yields = _yields.Count;
            Expression o = Visit(node.Operand);
            if (o == node.Operand) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.MakeUnary(node.NodeType, o, node.Type, node.Method, node.Annotations);
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref o),
                Expression.MakeUnary(node.NodeType, o, node.Type, node.Method, null)
            );
        }

        protected override Expression VisitThrow(ThrowStatement node) {
            int yields = _yields.Count;
            Expression v = Visit(node.Value);
            if (v == node.Value) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.Throw(v, node.Annotations);
            }
            return Expression.Comma(
                node.Annotations,
                ToTemp(ref v),
                Expression.Throw(v, null)
            );
        }

        protected override Expression VisitMemberInit(MemberInitExpression node) {
            // See if anything changed
            int yields = _yields.Count;
            Expression e = base.Visit(node);
            if (yields == _yields.Count) {
                return e;
            }
            // It has a yield. Reduce to basic nodes so we can jump in
            return e.Reduce();
        }

        protected override Expression VisitListInit(ListInitExpression node) {
            // See if anything changed
            int yields = _yields.Count;
            Expression e = base.Visit(node);
            if (yields == _yields.Count) {
                return e;
            }
            // It has a yield. Reduce to basic nodes so we can jump in
            return e.Reduce();
        }

        #endregion
    }
}