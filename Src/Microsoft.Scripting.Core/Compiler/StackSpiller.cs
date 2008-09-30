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
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {

    /// <summary>
    /// Expression rewriting to spill the CLR stack into temporary variables
    /// in order to guarantee some properties of code generation, for
    /// example that we always enter try block on empty stack.
    /// </summary>
    internal partial class StackSpiller {

        // Is the evaluation stack empty?
        private enum Stack {
            Empty,
            NonEmpty
        };

        // Should the parent nodes be rewritten, and in what way?
        // Designed so bitwise-or produces the correct result when merging two
        // subtrees. In particular, SpillStack is preferred over Copy which is
        // preferred over None.
        //
        // Values:
        //   None -> no rewrite needed
        //   Copy -> copy into a new node
        //   SpillStack -> spill stack into temps
        [Flags]
        private enum RewriteAction {
            None = 0,
            Copy = 1,
            SpillStack = 3,
        }

        // Result of a rewrite operation. Always contains an action and a node.
        private struct Result {
            internal readonly RewriteAction Action;
            internal readonly Expression Node;

            internal Result(RewriteAction action, Expression node) {
                Action = action;
                Node = node;
            }
        }

        /// <summary>
        /// The source of temporary variables
        /// </summary>
        private readonly TempMaker _tm;

        #region StackSpiller entry points

        /// <summary>
        /// Analyzes a lambda, producing a new one that has correct invariants
        /// for codegen. In particular, it spills the IL stack to temps in
        /// places where it's invalid to have a non-empty stack (for example,
        /// entering a try statement).
        /// </summary>
        internal static LambdaExpression AnalyzeLambda(LambdaExpression lambda) {
            return new StackSpiller(new TempMaker()).Rewrite(lambda);
        }

        #endregion

        private StackSpiller(TempMaker tm) {
            _tm = tm;
        }

        private LambdaExpression Rewrite(LambdaExpression lambda) {
            VerifyTemps();

            // Lambda starts with an empty stack
            Result body = RewriteExpressionFreeTemps(lambda.Body, Stack.Empty);

            VerifyTemps();

            if (body.Action != RewriteAction.None) {
                // Create a new scope for temps
                // (none of these will be hoisted so there is no closure impact)
                Expression newBody = body.Node;
                if (_tm.Temps.Count > 0) {
                    newBody = Expression.Scope(newBody, "$lambda_temp_scope$" + lambda.Name, _tm.Temps);
                }

                // Clone the lambda, replacing the body & variables
                return Expression.Lambda(
                    lambda.Annotations,
                    lambda.NodeType,
                    lambda.Type,
                    lambda.Name,
                    newBody,
                    lambda.Parameters
                );
            }

            return lambda;
        }

        #region Expressions

        [Conditional("DEBUG")]
        private static void VerifyRewrite(Result result, Expression node) {
            Debug.Assert(result.Node != null);

            // (result.Action == RewriteAction.None) if and only if (node == result.Node)
            Debug.Assert((result.Action == RewriteAction.None) ^ (node != result.Node), "rewrite action does not match node object identity");

            // if the original node is an extension node, it should have been rewritten
            Debug.Assert(result.Node.NodeType != ExpressionType.Extension, "extension nodes must be rewritten");

            // if we have Copy, then node type must match
            Debug.Assert(
                result.Action != RewriteAction.Copy || node.NodeType == result.Node.NodeType || node.NodeType == ExpressionType.Extension,
                "rewrite action does not match node object kind"
            );

            // Type must always match.
            Debug.Assert(node.Type == result.Node.Type, "rewritten object must have same type as original");
        }

        private Result RewriteExpressionFreeTemps(Expression expression, Stack stack) {
            int mark = Mark();
            Result result = RewriteExpression(expression, stack);
            Free(mark);
            return result;
        }

        // DynamicExpression
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stack")]
        private Result RewriteDynamicExpression(Expression expr, Stack stack) {
            var node = (DynamicExpression)expr;

            // CallSite is on the stack
            ChildRewriter cr = new ChildRewriter(this, Stack.NonEmpty, node.Arguments.Count);
            cr.Add(node.Arguments);
            return cr.Finish(cr.Rewrite ? new DynamicExpression(node.Type, node.Annotations, node.DelegateType, node.Binder, cr[0, -1]) : expr);
        }

        private Result RewriteIndexAssignment(AssignmentExpression node, Stack stack) {
            IndexExpression index = (IndexExpression)node.Expression;

            ChildRewriter cr = new ChildRewriter(this, stack, 2 + index.Arguments.Count);

            cr.Add(index.Object);
            cr.Add(index.Arguments);
            cr.Add(node.Value);

            if (cr.Rewrite) {
                node = new AssignmentExpression(
                    node.Annotations,
                    new IndexExpression(
                        cr[0],                              // Object
                        index.Indexer,
                        index.Annotations,
                        cr[1, -2],                          // arguments
                        index.Type,
                        index.CanRead,
                        index.CanWrite
                    ),
                    cr[-1]                                  // value
                );
            }

            return cr.Finish(node);
        }

        // BinaryExpression: AndAlso, OrElse
        private Result RewriteLogicalBinaryExpression(Expression expr, Stack stack) {
            BinaryExpression node = (BinaryExpression)expr;
            // Left expression runs on a stack as left by parent
            Result left = RewriteExpression(node.Left, stack);
            // ... and so does the right one
            Result right = RewriteExpression(node.Right, stack);
            //conversion is a lambda. stack state will be ignored. 
            Result conversion = RewriteExpression(node.Conversion, stack);

            RewriteAction action = left.Action | right.Action | conversion.Action;
            if (action != RewriteAction.None) {
                expr = new BinaryExpression(
                    node.Annotations,
                    node.NodeType,
                    left.Node,
                    right.Node,
                    node.Type,
                    node.Method,
                    (LambdaExpression)conversion.Node
                );
            }
            return new Result(action, expr);
        }

        // BinaryExpression
        private Result RewriteBinaryExpression(Expression expr, Stack stack) {
            BinaryExpression node = (BinaryExpression)expr;

            ChildRewriter cr = new ChildRewriter(this, stack, 3);
            // Left expression executes on the stack as left by parent
            cr.Add(node.Left);
            // Right expression always has non-empty stack (left is on it)
            cr.Add(node.Right);
            // conversion is a lambda, stack state will be ignored
            cr.Add(node.Conversion);

            return cr.Finish(cr.Rewrite ?
                                    new BinaryExpression(node.Annotations,
                                            node.NodeType,
                                            cr[0],
                                            cr[1],
                                            node.Type,
                                            node.Method,
                                            (LambdaExpression)cr[2]) :
                                    expr);
        }

        // variable assignment
        private Result RewriteVariableAssignment(AssignmentExpression node, Stack stack) {
            // Expression is evaluated on a stack in current state
            Result value = RewriteExpression(node.Value, stack);
            if (value.Action != RewriteAction.None) {
                node = Expression.Assign(node.Expression, value.Node);
            }
            return new Result(value.Action, node);
        }

        // AssignmentExpression
        private Result RewriteAssignmentExpression(Expression expr, Stack stack) {
            AssignmentExpression node = (AssignmentExpression)expr;
            switch (node.Expression.NodeType) {
                case ExpressionType.Index:
                    return RewriteIndexAssignment(node, stack);
                case ExpressionType.MemberAccess:
                    return RewriteMemberAssignment(node, stack);
                case ExpressionType.Parameter:
                case ExpressionType.Variable:
                    return RewriteVariableAssignment(node, stack);
                default:
                    throw Error.InvalidLvalue(node.Expression.NodeType);
            }
        }

        // VariableExpression
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stack")]
        private static Result RewriteVariableExpression(Expression expr, Stack stack) {
            // No action necessary, regardless of the stack state
            return new Result(RewriteAction.None, expr);
        }

        // ParameterExpression
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stack")]
        private static Result RewriteParameterExpression(Expression expr, Stack stack) {
            // No action necessary, regardless of the stack state
            return new Result(RewriteAction.None, expr);
        }

        // LambdaExpression
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stack")]
        private static Result RewriteLambdaExpression(Expression expr, Stack stack) {
            LambdaExpression node = (LambdaExpression)expr;

            // Call back into the rewriter
            expr = AnalyzeLambda(node);

            // If the lambda gets rewritten, we don't need to spill the stack,
            // but we do need to rebuild the tree above us so it includes the new node.
            RewriteAction action = (expr == node) ? RewriteAction.None : RewriteAction.Copy;

            return new Result(action, expr);
        }

        // ConditionalExpression
        private Result RewriteConditionalExpression(Expression expr, Stack stack) {
            ConditionalExpression node = (ConditionalExpression)expr;
            // Test executes at the stack as left by parent
            Result test = RewriteExpression(node.Test, stack);
            // The test is popped by conditional jump so branches execute
            // at the stack as left by parent too.
            Result ifTrue = RewriteExpression(node.IfTrue, stack);
            Result ifFalse = RewriteExpression(node.IfFalse, stack);

            RewriteAction action = test.Action | ifTrue.Action | ifFalse.Action;
            if (action != RewriteAction.None) {
                expr = Expression.Condition(test.Node, ifTrue.Node, ifFalse.Node);
            }

            return new Result(action, expr);
        }

        // ConstantExpression
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stack")]
        private static Result RewriteConstantExpression(Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return new Result(RewriteAction.None, expr);
        }

        // LocalScopeExpression
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stack")]
        private static Result RewriteLocalScopeExpression(Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return new Result(RewriteAction.None, expr);
        }

        // member assignment
        private Result RewriteMemberAssignment(AssignmentExpression node, Stack stack) {
            MemberExpression lvalue = (MemberExpression)node.Expression;

            ChildRewriter cr = new ChildRewriter(this, stack, 2);

            // If there's an instance, it executes on the stack in current state
            // and rest is executed on non-empty stack.
            // Otherwise the stack is left unchaged.
            cr.Add(lvalue.Expression);

            cr.Add(node.Value);

            if (cr.Rewrite) {
                return cr.Finish(
                    new AssignmentExpression(
                        node.Annotations,
                        new MemberExpression(cr[0], lvalue.Member, lvalue.Annotations, lvalue.Type, lvalue.CanRead, lvalue.CanWrite),
                        cr[1]
                    )
                );
            }
            return new Result(RewriteAction.None, node);
        }

        // MemberExpression
        private Result RewriteMemberExpression(Expression expr, Stack stack) {
            MemberExpression node = (MemberExpression)expr;

            // Expression is emitted on top of the stack in current state
            Result expression = RewriteExpression(node.Expression, stack);
            if (expression.Action != RewriteAction.None) {
                expr = new MemberExpression(expression.Node, node.Member, node.Annotations, node.Type, node.CanRead, node.CanWrite);
            }
            return new Result(expression.Action, expr);
        }

        //RewriteIndexExpression
        private Result RewriteIndexExpression(Expression expr, Stack stack) {
            IndexExpression node = (IndexExpression)expr;

            ChildRewriter cr = new ChildRewriter(this, stack, node.Arguments.Count + 1);

            // For instance methods, the instance executes on the
            // stack as is, but stays on the stack, making it non-empty.
            cr.Add(node.Object);
            cr.Add(node.Arguments);

            if (cr.Rewrite) {
                expr = new IndexExpression(
                    cr[0],
                    node.Indexer,
                    node.Annotations,
                    cr[1, -1],
                    node.Type,
                    node.CanRead,
                    node.CanWrite
                );
            }

            return cr.Finish(expr);
        }

        // MethodCallExpression
        // TODO: ref parameters!!!
        private Result RewriteMethodCallExpression(Expression expr, Stack stack) {
            MethodCallExpression node = (MethodCallExpression)expr;

            ChildRewriter cr = new ChildRewriter(this, stack, node.Arguments.Count + 1);

            // For instance methods, the instance executes on the
            // stack as is, but stays on the stack, making it non-empty.
            cr.Add(node.Object);

            cr.Add(node.Arguments);

            return cr.Finish(cr.Rewrite ? new MethodCallExpression(node.Annotations, node.Type, node.Method, cr[0], cr[1, -1]) : expr);
        }

        // NewArrayExpression
        private Result RewriteNewArrayExpression(Expression expr, Stack stack) {
            NewArrayExpression node = (NewArrayExpression)expr;

            if (node.NodeType == ExpressionType.NewArrayInit) {
                // In a case of array construction with element initialization
                // the element expressions are never emitted on an empty stack because
                // the array reference and the index are on the stack.
                stack = Stack.NonEmpty;
            } else {
                // In a case of NewArrayBounds we make no modifications to the stack 
                // before emitting bounds expressions.
            }

            ChildRewriter cr = new ChildRewriter(this, stack, node.Expressions.Count);
            cr.Add(node.Expressions);

            return cr.Finish(cr.Rewrite ? Expression.NewArrayInit(node.Type.GetElementType(), cr[0, -1]) : expr);
        }

        // InvocationExpression
        private Result RewriteInvocationExpression(Expression expr, Stack stack) {
            InvocationExpression node = (InvocationExpression)expr;

            // first argument starts on stack as provided
            ChildRewriter cr = new ChildRewriter(this, stack, node.Arguments.Count + 1);
            cr.Add(node.Expression);

            // rest of arguments have non-empty stack (delegate instance on the stack)
            cr.Add(node.Arguments);

            return cr.Finish(cr.Rewrite ? new InvocationExpression(cr[0], node.Annotations, cr[1, -1], node.Type) : expr);
        }

        // NewExpression
        private Result RewriteNewExpression(Expression expr, Stack stack) {
            NewExpression node = (NewExpression)expr;

            // The first expression starts on a stack as provided by parent,
            // rest are definitely non-emtpy (which ChildRewriter guarantees)
            ChildRewriter cr = new ChildRewriter(this, stack, node.Arguments.Count);
            cr.Add(node.Arguments);

            return cr.Finish(cr.Rewrite ? new NewExpression(node.Annotations, node.Type, node.Constructor, cr[0, -1], node.Members) : expr);
        }

        // TypeBinaryExpression
        private Result RewriteTypeBinaryExpression(Expression expr, Stack stack) {
            TypeBinaryExpression node = (TypeBinaryExpression)expr;
            // The expression is emitted on top of current stack
            Result expression = RewriteExpression(node.Expression, stack);
            if (expression.Action != RewriteAction.None) {
                expr = Expression.TypeIs(expression.Node, node.TypeOperand);
            }
            return new Result(expression.Action, expr);
        }

        // UnaryExpression
        private Result RewriteUnaryExpression(Expression expr, Stack stack) {
            UnaryExpression node = (UnaryExpression)expr;

            // Do nothing for quoted sub-expressions
            // We can spill later if it gets compiled
            if (node.NodeType == ExpressionType.Quote) {
                return new Result(RewriteAction.None, expr);
            }

            // Operand is emitted on top of the stack as is
            Result expression = RewriteExpression(node.Operand, stack);
            if (expression.Action != RewriteAction.None) {
                expr = new UnaryExpression(node.Annotations, node.NodeType, expression.Node, node.Type, node.Method);
            }
            return new Result(expression.Action, expr);
        }

        // RewriteListInitExpression
        private Result RewriteListInitExpression(Expression expr, Stack stack) {
            ListInitExpression node = (ListInitExpression)expr;

            //ctor runs on initial stack
            Result newResult = RewriteExpression(node.NewExpression, stack);
            Expression rewrittenNew = newResult.Node;
            RewriteAction action = newResult.Action;

            ReadOnlyCollection<ElementInit> inits = node.Initializers;

            ChildRewriter[] cloneCrs = new ChildRewriter[inits.Count];

            for (int i = 0; i < inits.Count; i++) {
                ElementInit init = inits[i];

                //initializers all run on nonempty stack
                ChildRewriter cr = new ChildRewriter(this, Stack.NonEmpty, init.Arguments.Count);
                cr.Add(init.Arguments);

                action |= cr.Action;
                cloneCrs[i] = cr;
            }

            switch (action) {
                case RewriteAction.None:
                    break;
                case RewriteAction.Copy:
                    ElementInit[] newInits = new ElementInit[inits.Count];
                    for (int i = 0; i < inits.Count; i++) {
                        ChildRewriter cr = cloneCrs[i];
                        if (cr.Action == RewriteAction.None) {
                            newInits[i] = inits[i];
                        } else {
                            newInits[i] = Expression.ElementInit(inits[i].AddMethod, cr[0, -1]);
                        }
                    }
                    expr = Expression.ListInit((NewExpression)rewrittenNew, new ReadOnlyCollection<ElementInit>(newInits));
                    break;
                case RewriteAction.SpillStack:
                    VariableExpression tempNew = MakeTemp(rewrittenNew.Type);
                    Expression[] comma = new Expression[inits.Count + 2];
                    comma[0] = Expression.Assign(tempNew, rewrittenNew);

                    for (int i = 0; i < inits.Count; i++) {
                        ChildRewriter cr = cloneCrs[i];
                        Result add = cr.Finish(Expression.Call(tempNew, inits[i].AddMethod, cr[0, -1]));
                        comma[i + 1] = add.Node;
                    }
                    comma[inits.Count + 1] = tempNew;
                    expr = Expression.Comma(comma);
                    break;
                default:
                    throw Assert.Unreachable;
            }

            return new Result(action, expr);
        }

        // RewriteMemberInitExpression
        private Result RewriteMemberInitExpression(Expression expr, Stack stack) {
            MemberInitExpression node = (MemberInitExpression)expr;

            //ctor runs on original stack
            Result result = RewriteExpression(node.NewExpression, stack);
            Expression rewrittenNew = result.Node;
            RewriteAction action = result.Action;

            ReadOnlyCollection<MemberBinding> bindings = node.Bindings;
            BindingRewriter[] bindingRewriters = new BindingRewriter[bindings.Count];
            for (int i = 0; i < bindings.Count; i++) {
                MemberBinding binding = bindings[i];
                //bindings run on nonempty stack
                BindingRewriter rewriter = BindingRewriter.Create(binding, this, Stack.NonEmpty);
                bindingRewriters[i] = rewriter;
                action |= rewriter.Action;
            }

            switch (action) {
                case RewriteAction.None:
                    break;
                case RewriteAction.Copy:
                    MemberBinding[] newBindings = new MemberBinding[bindings.Count];
                    for (int i = 0; i < bindings.Count; i++) {
                        newBindings[i] = bindingRewriters[i].AsBinding();
                    }
                    expr = Expression.MemberInit((NewExpression)rewrittenNew, new ReadOnlyCollection<MemberBinding>(newBindings));
                    break;
                case RewriteAction.SpillStack:
                    VariableExpression tempNew = MakeTemp(rewrittenNew.Type);
                    Expression[] comma = new Expression[bindings.Count + 2];
                    comma[0] = Expression.Assign(tempNew, rewrittenNew);
                    for (int i = 0; i < bindings.Count; i++) {
                        BindingRewriter cr = bindingRewriters[i];
                        Expression initExpr = cr.AsExpression(tempNew);
                        comma[i + 1] = initExpr;
                    }
                    comma[bindings.Count + 1] = tempNew;
                    expr = Expression.Comma(comma);
                    break;
                default:
                    throw Assert.Unreachable;
            }
            return new Result(action, expr);
        }

        #endregion

        #region Statements

        // Block
        private Result RewriteBlock(Expression expr, Stack stack) {
            Block node = (Block)expr;

            ReadOnlyCollection<Expression> expressions = node.Expressions;
            RewriteAction action = RewriteAction.None;
            Expression[] clone = null;
            for (int i = 0; i < expressions.Count; i++) {
                Expression expression = expressions[i];
                // All statements within the block execute at the
                // same stack state.
                Result rewritten = RewriteExpression(expression, stack);
                action |= rewritten.Action;

                if (clone == null && rewritten.Action != RewriteAction.None) {
                    clone = Clone(node.Expressions, i);
                }

                if (clone != null) {
                    clone[i] = rewritten.Node;
                }
            }

            if (action != RewriteAction.None) {
                // okay to wrap since we know no one can mutate the clone array
                expr = new Block(node.Annotations, new ReadOnlyCollection<Expression>(clone), node.Type);
            }
            return new Result(action, expr);
        }

        // BreakStatement
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stack")]
        private static Result RewriteBreakStatement(Expression expr, Stack stack) {
            // No action necessary
            return new Result(RewriteAction.None, expr);
        }

        // ContinueStatement
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stack")]
        private static Result RewriteContinueStatement(Expression expr, Stack stack) {
            // No action necessary
            return new Result(RewriteAction.None, expr);
        }

        // DoStatement
        private Result RewriteDoStatement(Expression expr, Stack stack) {
            DoStatement node = (DoStatement)expr;

            // The "do" statement requires empty stack so it can
            // guarantee it for its child nodes.
            Result body = RewriteExpression(node.Body, Stack.Empty);
            Result test = RewriteExpressionFreeTemps(node.Test, Stack.Empty);

            RewriteAction action = body.Action | test.Action;

            // Loop needs empty stack to execute so if the stack is initially
            // not empty, we rewrite to get empty stack.
            if (stack != Stack.Empty) {
                action = RewriteAction.SpillStack;
            }

            if (action != RewriteAction.None) {
                expr = new DoStatement(node.Annotations, node.Label, test.Node, body.Node);
            }

            return new Result(action, expr);
        }

        // EmptyStatement
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stack")]
        private static Result RewriteEmptyStatement(Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return new Result(RewriteAction.None, expr);
        }

        // LabeledStatement
        private Result RewriteLabeledStatement(Expression expr, Stack stack) {
            LabeledStatement node = (LabeledStatement)expr;

            Result expression = RewriteExpression(node.Statement, stack);
            if (expression.Action != RewriteAction.None) {
                expr = Expression.Labeled(node.Label, expression.Node, node.Annotations);
            }
            return new Result(expression.Action, expr);
        }

        // LoopStatement
        private Result RewriteLoopStatement(Expression expr, Stack stack) {
            LoopStatement node = (LoopStatement)expr;

            // The loop statement requires empty stack for itself, so it
            // can guarantee it to the child nodes.
            Result test = RewriteExpressionFreeTemps(node.Test, Stack.Empty);
            Result incr = RewriteExpressionFreeTemps(node.Increment, Stack.Empty);
            Result body = RewriteExpression(node.Body, Stack.Empty);
            Result @else = RewriteExpression(node.ElseStatement, Stack.Empty);

            RewriteAction action = test.Action | incr.Action | body.Action | @else.Action;

            // However, the loop itself requires that it executes on an empty stack
            // so we need to rewrite if the stack is not empty.
            if (stack != Stack.Empty) {
                action = RewriteAction.SpillStack;
            }

            if (action != RewriteAction.None) {
                expr = Expression.Loop(test.Node, incr.Node, body.Node, @else.Node, node.Label, node.Annotations);
            }
            return new Result(action, expr);
        }

        // ReturnStatement
        private Result RewriteReturnStatement(Expression expr, Stack stack) {
            ReturnStatement node = (ReturnStatement)expr;

            // Return requires empty stack to execute so the expression is
            // going to execute on an empty stack.
            Result expression = RewriteExpressionFreeTemps(node.Expression, Stack.Empty);

            // However, the statement itself needs an empty stack for itself
            // so if stack is not empty, rewrite to empty the stack.
            RewriteAction action = expression.Action;
            if (stack != Stack.Empty) {
                action = RewriteAction.SpillStack;
            }

            if (action != RewriteAction.None) {
                expr = Expression.Return(expression.Node, node.Annotations);
            }
            return new Result(action, expr);
        }

        // ScopeExpression
        private Result RewriteScopeExpression(Expression expr, Stack stack) {
            ScopeExpression node = (ScopeExpression)expr;

            Result body = RewriteExpression(node.Body, stack);

            RewriteAction action = body.Action;
            if (action != RewriteAction.None) {
                expr = new ScopeExpression(body.Node, node.Name, node.Annotations, node.Variables);
            }
            return new Result(action, expr);
        }

        // SwitchStatement
        private Result RewriteSwitchStatement(Expression expr, Stack stack) {
            SwitchStatement node = (SwitchStatement)expr;

            // The switch statement test is emitted on the stack in current state
            Result test = RewriteExpressionFreeTemps(node.TestValue, stack);

            RewriteAction action = test.Action;
            ReadOnlyCollection<SwitchCase> cases = node.Cases;
            SwitchCase[] clone = null;
            for (int i = 0; i < cases.Count; i++) {
                SwitchCase @case = cases[i];

                // And all the cases also run on the same stack level.
                Result body = RewriteExpression(@case.Body, stack);
                action |= body.Action;

                if (body.Action != RewriteAction.None) {
                    @case = new SwitchCase(@case.IsDefault, @case.Value, body.Node);

                    if (clone == null) {
                        clone = Clone(cases, i);
                    }
                }

                if (clone != null) {
                    clone[i] = @case;
                }
            }

            if (action != RewriteAction.None) {
                if (clone != null) {
                    // okay to wrap because we aren't modifying the array
                    cases = new ReadOnlyCollection<SwitchCase>(clone);
                }

                expr = new SwitchStatement(test.Node, node.Label, node.Annotations, cases);
            }

            return new Result(action, expr);
        }

        // ThrowStatement
        private Result RewriteThrowStatement(Expression expr, Stack stack) {
            ThrowStatement node = (ThrowStatement)expr;

            Result value = RewriteExpressionFreeTemps(node.Value, stack);
            if (value.Action != RewriteAction.None) {
                expr = Expression.Throw(value.Node, node.Annotations);
            }
            return new Result(value.Action, expr);
        }

        // TryStatement
        private Result RewriteTryStatement(Expression expr, Stack stack) {
            TryStatement node = (TryStatement)expr;

            // Try statement definitely needs an empty stack so its
            // child nodes execute at empty stack.
            Result body = RewriteExpression(node.Body, Stack.Empty);
            ReadOnlyCollection<CatchBlock> handlers = node.Handlers;
            CatchBlock[] clone = null;

            RewriteAction action = body.Action;
            if (handlers != null) {
                for (int i = 0; i < handlers.Count; i++) {
                    RewriteAction curAction = body.Action;

                    CatchBlock handler = handlers[i];

                    Expression filter = handler.Filter;
                    if (handler.Filter != null) {
                        // our code gen saves the incoming filter value and provides it as a varaible so the stack is empty
                        Result rfault = RewriteExpression(handler.Filter, Stack.Empty);
                        action |= rfault.Action;
                        curAction |= rfault.Action;
                        filter = rfault.Node;
                    }

                    // Catch block starts with an empty stack (guaranteed by TryStatement)
                    Result rbody = RewriteExpression(handler.Body, Stack.Empty);
                    action |= rbody.Action;
                    curAction |= rbody.Action;

                    if (curAction != RewriteAction.None) {
                        handler = Expression.Catch(handler.Test, handler.Variable, rbody.Node, filter, handler.Annotations);

                        if (clone == null) {
                            clone = Clone(handlers, i);
                        }
                    }

                    if (clone != null) {
                        clone[i] = handler;
                    }
                }
            }

            Result fault = RewriteExpression(node.Fault, Stack.Empty);
            action |= fault.Action;

            Result @finally = RewriteExpression(node.Finally, Stack.Empty);
            action |= @finally.Action;

            // If the stack is initially not empty, rewrite to spill the stack
            if (stack != Stack.Empty) {
                action = RewriteAction.SpillStack;
            }

            if (action != RewriteAction.None) {
                if (clone != null) {
                    // okay to wrap because we aren't modifying the array
                    handlers = new ReadOnlyCollection<CatchBlock>(clone);
                }

                expr = new TryStatement(node.Annotations, body.Node, handlers, @finally.Node, fault.Node);
            }
            return new Result(action, expr);
        }

        // YieldStatement
        private Result RewriteYieldStatement(Expression expr, Stack stack) {
            YieldStatement node = (YieldStatement)expr;

            // Yield expression is always execute on an non-empty stack
            // given the nature of the codegen.
            Result expression = RewriteExpressionFreeTemps(node.Expression, Stack.NonEmpty);

            RewriteAction action = expression.Action;
            if (stack != Stack.Empty) {
                action = RewriteAction.SpillStack;
            }

            if (action == RewriteAction.SpillStack) {
                // Yield's argument was changed. We may need to hoist it.
                // This will flatten nested yields, which simplifies yield codegen. So:
                //   yield (yield x)
                // becomes:
                //   $t = yield x
                //   yield $t
                Expression saveArg, tempArg;
                tempArg = ToTemp(expression.Node, out saveArg);
                expr = Expression.Block(
                    saveArg,
                    Expression.Yield(tempArg, node.Annotations)
                );
            } else if (action == RewriteAction.Copy) {
                expr = Expression.Yield(expression.Node, node.Annotations);
            }
            return new Result(action, expr);
        }

        private Result RewriteExtensionExpression(Expression expr, Stack stack) {
            Result result = RewriteExpression(expr.ReduceToKnown(), stack);
            // it's at least Copy because we reduced the node
            return new Result(result.Action | RewriteAction.Copy, result.Node);
        }

        #endregion

        #region Cloning

        /// <summary>
        /// Will clone an IList into an array of the same size, and copy
        /// all vaues up to (and NOT including) the max index
        /// </summary>
        /// <returns>The cloned array.</returns>
        private static T[] Clone<T>(ReadOnlyCollection<T> original, int max) {
            Debug.Assert(original != null);
            Debug.Assert(max < original.Count);

            T[] clone = new T[original.Count];
            for (int j = 0; j < max; j++) {
                clone[j] = original[j];
            }
            return clone;
        }

        #endregion
    }

    internal partial class StackSpiller {

    }
}
