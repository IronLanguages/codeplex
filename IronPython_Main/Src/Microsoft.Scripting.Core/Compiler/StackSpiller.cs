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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Scripting.Actions;
using System.Scripting.Utils;

namespace System.Linq.Expressions.Compiler {

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
            StackSpiller self = new StackSpiller(new TempMaker());
            return self.Rewrite(lambda);
        }

        #endregion

        private StackSpiller(TempMaker tm) {
            _tm = tm;
        }

        private LambdaExpression Rewrite(LambdaExpression lambda) {
            VerifyTemps();

            // Lambda starts with an empty stack
            Result body = RewriteExpressionFreeTemps(this, lambda.Body, Stack.Empty);

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

        /// <summary>
        /// Rewrite the expression
        /// </summary>
        /// <param name="self">Expression rewriter instance</param>
        /// <param name="node">Expression to rewrite</param>
        /// <param name="stack">State of the stack before the expression is emitted.</param>
        /// <returns>Rewritten expression.</returns>
        private static Result RewriteExpression(StackSpiller self, Expression node, Stack stack) {
            if (node == null) {
                return new Result(RewriteAction.None, null);
            }

            // dynamic nodes have already been removed
            Debug.Assert(!node.IsDynamic);

            ExpressionType ant = node.NodeType;
            Debug.Assert((int)ant < _Rewriters.Length);

            Result result = _Rewriters[(int)ant](self, node, stack);
            VerifyRewrite(result, node);

            return result;
        }

        [Conditional("DEBUG")]
        private static void VerifyRewrite(Result result, Expression node) {
            // (result.Action == RewriteAction.None) if and only if (node == result.Node)
            Debug.Assert((result.Action == RewriteAction.None) ^ (node != result.Node), "rewrite action does not match node object identity");
            // if we have Copy, then node type must match.
            Debug.Assert(((result.Action != RewriteAction.Copy) || (node.NodeType == result.Node.NodeType)), "rewrite action does not match node object kind");
            // Type must always match.
            Debug.Assert(node.Type == result.Node.Type, "rewritten object must have same type as original");
        }

        private static Result RewriteExpressionFreeTemps(StackSpiller self, Expression expression, Stack stack) {
            int mark = self.Mark();
            Result result = RewriteExpression(self, expression, stack);
            self.Free(mark);
            return result;
        }

        // ActionExpression
        private static Result RewriteActionExpression(StackSpiller self, Expression expr, Stack stack) {
            throw Error.DynamicNotReduced();
        }

        // array index assignment
        private static Result RewriteArrayIndexAssignment(StackSpiller self, AssignmentExpression node, Stack stack) {
            BinaryExpression arrayIndex = (BinaryExpression)node.Expression;

            ChildRewriter cr = new ChildRewriter(self, stack, 3);

            // Evaluation order is: array, index, value
            cr.Add(arrayIndex.Left);
            cr.Add(arrayIndex.Right);
            cr.Add(node.Value);

            return cr.Finish(cr.Rewrite ? Expression.AssignArrayIndex(cr[0], cr[1], cr[2]) : node);
        }

        private static Result RewriteIndexedPropertyAssignment(StackSpiller self, AssignmentExpression node, Stack stack) {
            IndexedPropertyExpression pExpression = (IndexedPropertyExpression)node.Expression;

            ChildRewriter cr = new ChildRewriter(self, stack, 2 + pExpression.Arguments.Count);

            cr.Add(pExpression.Object);
            cr.Add(pExpression.Arguments);
            cr.Add(node.Value);

            if (cr.Rewrite) {
                node = new AssignmentExpression(
                    node.Annotations,
                    new IndexedPropertyExpression(
                        node.Annotations,
                        cr[0],                              // Object
                        pExpression.GetMethod,
                        pExpression.SetMethod,
                        cr[1, -2],                          // arguments
                        node.Type,
                        node.BindingInfo
                    ),
                    cr[pExpression.Arguments.Count + 1],    // value
                    node.Type,
                    node.BindingInfo
                );
            }

            return cr.Finish(node);
        }

        // BinaryExpression: AndAlso, OrElse
        private static Result RewriteLogicalBinaryExpression(StackSpiller self, Expression expr, Stack stack) {
            BinaryExpression node = (BinaryExpression)expr;
            // Left expression runs on a stack as left by parent
            Result left = RewriteExpression(self, node.Left, stack);
            // ... and so does the right one
            Result right = RewriteExpression(self, node.Right, stack);
            //conversion is a lambda. stack state will be ignored. 
            Result conversion = RewriteExpression(self, node.Conversion, stack);

            RewriteAction action = left.Action | right.Action | conversion.Action;
            if (action != RewriteAction.None) {
                expr = new BinaryExpression(node.Annotations,
                            node.NodeType,
                            left.Node,
                            right.Node,
                            node.Type,
                            node.Method,
                            (LambdaExpression)conversion.Node,
                            node.BindingInfo);
            }
            return new Result(action, expr);
        }

        // BinaryExpression
        private static Result RewriteBinaryExpression(StackSpiller self, Expression expr, Stack stack) {
            BinaryExpression node = (BinaryExpression)expr;

            ChildRewriter cr = new ChildRewriter(self, stack, 3);
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
                                            (LambdaExpression)cr[2],
                                            node.BindingInfo) :
                                    expr);
        }

        // variable assignment
        private static Result RewriteVariableAssignment(StackSpiller self, AssignmentExpression node, Stack stack) {
            // Expression is evaluated on a stack in current state
            Result value = RewriteExpression(self, node.Value, stack);
            if (value.Action != RewriteAction.None) {
                node = Expression.Assign(node.Expression, value.Node);
            }
            return new Result(value.Action, node);
        }

        // AssignmentExpression
        private static Result RewriteAssignmentExpression(StackSpiller self, Expression expr, Stack stack) {
            AssignmentExpression node = (AssignmentExpression)expr;
            switch (node.Expression.NodeType) {
                case ExpressionType.ArrayIndex:
                    return RewriteArrayIndexAssignment(self, node, stack);
                case ExpressionType.IndexedProperty:
                    return RewriteIndexedPropertyAssignment(self, node, stack);
                case ExpressionType.MemberAccess:
                    return RewriteMemberAssignment(self, node, stack);
                case ExpressionType.Parameter:
                case ExpressionType.Variable:
                    return RewriteVariableAssignment(self, node, stack);
                default:
                    throw Error.InvalidLvalue(node.Expression.NodeType);
            }
        }

        // VariableExpression
        private static Result RewriteVariableExpression(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary, regardless of the stack state
            return new Result(RewriteAction.None, expr);
        }

        // ParameterExpression
        private static Result RewriteParameterExpression(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary, regardless of the stack state
            return new Result(RewriteAction.None, expr);
        }

        // LambdaExpression
        private static Result RewriteLambdaExpression(StackSpiller self, Expression expr, Stack stack) {
            LambdaExpression node = (LambdaExpression)expr;

            // Call back into the rewriter
            expr = AnalyzeLambda(node);

            // If the lambda gets rewritten, we don't need to spill the stack,
            // but we do need to rebuild the tree above us so it includes the new node.
            RewriteAction action = (expr == node) ? RewriteAction.None : RewriteAction.Copy;

            return new Result(action, expr);
        }

        // ConditionalExpression
        private static Result RewriteConditionalExpression(StackSpiller self, Expression expr, Stack stack) {
            ConditionalExpression node = (ConditionalExpression)expr;
            // Test executes at the stack as left by parent
            Result test = RewriteExpression(self, node.Test, stack);
            // The test is popped by conditional jump so branches execute
            // at the stack as left by parent too.
            Result ifTrue = RewriteExpression(self, node.IfTrue, stack);
            Result ifFalse = RewriteExpression(self, node.IfFalse, stack);

            RewriteAction action = test.Action | ifTrue.Action | ifFalse.Action;
            if (action != RewriteAction.None) {
                expr = Expression.Condition(test.Node, ifTrue.Node, ifFalse.Node);
            }

            return new Result(action, expr);
        }

        // ConstantExpression
        private static Result RewriteConstantExpression(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return new Result(RewriteAction.None, expr);
        }

        // LocalScopeExpression
        private static Result RewriteLocalScopeExpression(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return new Result(RewriteAction.None, expr);
        }

        // member assignment
        private static Result RewriteMemberAssignment(StackSpiller self, AssignmentExpression node, Stack stack) {
            MemberExpression lvalue = (MemberExpression)node.Expression;

            ChildRewriter cr = new ChildRewriter(self, stack, 2);

            // If there's an instance, it executes on the stack in current state
            // and rest is executed on non-empty stack.
            // Otherwise the stack is left unchaged.
            cr.Add(lvalue.Expression);

            cr.Add(node.Value);

            if (cr.Rewrite) {
                return cr.Finish(
                    new AssignmentExpression(
                        node.Annotations,
                        new MemberExpression(cr[0], lvalue.Member, lvalue.Annotations, lvalue.Type, lvalue.CanRead, lvalue.CanWrite, lvalue.BindingInfo),
                        cr[1],
                        node.Type,
                        node.BindingInfo
                    )
                );
            }
            return new Result(RewriteAction.None, node);
        }

        // MemberExpression
        private static Result RewriteMemberExpression(StackSpiller self, Expression expr, Stack stack) {
            MemberExpression node = (MemberExpression)expr;

            // Expression is emitted on top of the stack in current state
            Result expression = RewriteExpression(self, node.Expression, stack);
            if (expression.Action != RewriteAction.None) {
                expr = new MemberExpression(expression.Node, node.Member, node.Annotations, node.Type, node.CanRead, node.CanWrite, node.BindingInfo);
            }
            return new Result(expression.Action, expr);
        }

        //RewriteIndexedPropertyExpression
        private static Result RewriteIndexedPropertyExpression(StackSpiller self, Expression expr, Stack stack) {
            IndexedPropertyExpression node = (IndexedPropertyExpression)expr;

            ChildRewriter cr = new ChildRewriter(self, stack, node.Arguments.Count + 1);

            // For instance methods, the instance executes on the
            // stack as is, but stays on the stack, making it non-empty.
            cr.Add(node.Object);
            cr.Add(node.Arguments);

            if (cr.Rewrite) {
                expr = new IndexedPropertyExpression(
                    node.Annotations,
                    cr[0],
                    node.GetMethod,
                    node.SetMethod,
                    cr[1, -1],
                    node.Type,
                    node.BindingInfo
                );
            }

            return cr.Finish(expr);
        }


        // MethodCallExpression
        // TODO: ref parameters!!!
        private static Result RewriteMethodCallExpression(StackSpiller self, Expression expr, Stack stack) {
            MethodCallExpression node = (MethodCallExpression)expr;

            ChildRewriter cr = new ChildRewriter(self, stack, node.Arguments.Count + 1);

            // For instance methods, the instance executes on the
            // stack as is, but stays on the stack, making it non-empty.
            cr.Add(node.Object);

            cr.Add(node.Arguments);

            return cr.Finish(cr.Rewrite ? new MethodCallExpression(node.Annotations, node.Type, node.BindingInfo, node.Method, cr[0], cr[1, -1]) : expr);
        }

        // NewArrayExpression
        private static Result RewriteNewArrayExpression(StackSpiller self, Expression expr, Stack stack) {
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

            ChildRewriter cr = new ChildRewriter(self, stack, node.Expressions.Count);
            cr.Add(node.Expressions);

            return cr.Finish(cr.Rewrite ? Expression.NewArrayInit(node.Type.GetElementType(), cr[0, -1]) : expr);
        }

        // InvocationExpression
        private static Result RewriteInvocationExpression(StackSpiller self, Expression expr, Stack stack) {
            InvocationExpression node = (InvocationExpression)expr;

            // first argument starts on stack as provided
            ChildRewriter cr = new ChildRewriter(self, stack, node.Arguments.Count + 1);
            cr.Add(node.Expression);

            // rest of arguments have non-empty stack (delegate instance on the stack)
            cr.Add(node.Arguments);

            return cr.Finish(cr.Rewrite ? new InvocationExpression(node.Annotations, cr[0], node.Type, node.BindingInfo, cr[1, -1]) : expr);
        }

        // NewExpression
        private static Result RewriteNewExpression(StackSpiller self, Expression expr, Stack stack) {
            NewExpression node = (NewExpression)expr;

            // The first expression starts on a stack as provided by parent,
            // rest are definitely non-emtpy (which ChildRewriter guarantees)
            ChildRewriter cr = new ChildRewriter(self, stack, node.Arguments.Count);
            cr.Add(node.Arguments);

            return cr.Finish(cr.Rewrite ? new NewExpression(node.Annotations, node.Type, node.Constructor, cr[0, -1], node.BindingInfo) : expr);
        }

        // TypeBinaryExpression
        private static Result RewriteTypeBinaryExpression(StackSpiller self, Expression expr, Stack stack) {
            TypeBinaryExpression node = (TypeBinaryExpression)expr;
            // The expression is emitted on top of current stack
            Result expression = RewriteExpression(self, node.Expression, stack);
            if (expression.Action != RewriteAction.None) {
                expr = Expression.TypeIs(expression.Node, node.TypeOperand);
            }
            return new Result(expression.Action, expr);
        }

        // UnaryExpression
        private static Result RewriteUnaryExpression(StackSpiller self, Expression expr, Stack stack) {
            UnaryExpression node = (UnaryExpression)expr;

            // Do nothing for quoted sub-expressions
            // We can spill later if it gets compiled
            if (node.NodeType == ExpressionType.Quote) {
                return new Result(RewriteAction.None, expr);
            }

            // Operand is emitted on top of the stack as is
            Result expression = RewriteExpression(self, node.Operand, stack);
            if (expression.Action != RewriteAction.None) {
                expr = new UnaryExpression(node.Annotations, node.NodeType, expression.Node, node.Type, node.Method, node.BindingInfo);
            }
            return new Result(expression.Action, expr);
        }

        // RewriteListInitExpression
        private static Result RewriteListInitExpression(StackSpiller self, Expression expr, Stack stack) {
            ListInitExpression node = (ListInitExpression)expr;

            //ctor runs on initial stack
            Result newResult = RewriteExpression(self, node.NewExpression, stack);
            Expression rewrittenNew = newResult.Node;
            RewriteAction action = newResult.Action;

            ReadOnlyCollection<ElementInit> inits = node.Initializers;

            ChildRewriter[] cloneCrs = new ChildRewriter[inits.Count];

            for (int i = 0; i < inits.Count; i++) {
                ElementInit init = inits[i];

                //initializers all run on nonempty stack
                ChildRewriter cr = new ChildRewriter(self, Stack.NonEmpty, init.Arguments.Count);
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
                    VariableExpression tempNew = self.Temp(rewrittenNew.Type);
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
        private static Result RewriteMemberInitExpression(StackSpiller self, Expression expr, Stack stack) {
            MemberInitExpression node = (MemberInitExpression)expr;

            //ctor runs on original stack
            Result result = RewriteExpression(self, node.NewExpression, stack);
            Expression rewrittenNew = result.Node;
            RewriteAction action = result.Action;

            ReadOnlyCollection<MemberBinding> bindings = node.Bindings;
            BindingRewriter[] bindingRewriters = new BindingRewriter[bindings.Count];
            for (int i = 0; i < bindings.Count; i++) {
                MemberBinding binding = bindings[i];
                //bindings run on nonempty stack
                BindingRewriter rewriter = BindingRewriter.Create(binding, self, Stack.NonEmpty);
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
                    VariableExpression tempNew = self.Temp(rewrittenNew.Type);
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
        private static Result RewriteBlock(StackSpiller self, Expression expr, Stack stack) {
            Block node = (Block)expr;

            ReadOnlyCollection<Expression> expressions = node.Expressions;
            RewriteAction action = RewriteAction.None;
            Expression[] clone = null;
            for (int i = 0; i < expressions.Count; i++) {
                Expression expression = expressions[i];
                // All statements within the block execute at the
                // same stack state.
                Result rewritten = RewriteExpression(self, expression, stack);
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
        private static Result RewriteBreakStatement(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary
            return new Result(RewriteAction.None, expr);
        }

        // ContinueStatement
        private static Result RewriteContinueStatement(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary
            return new Result(RewriteAction.None, expr);
        }

        // DeleteExpression
        private static Result RewriteDeleteExpression(StackSpiller self, Expression expr, Stack stack) {
            DeleteExpression node = (DeleteExpression)expr;

            Debug.Assert(node.Expression.NodeType == ExpressionType.MemberAccess);
            MemberExpression lvalue = (MemberExpression)node.Expression;

            // Operand is emitted on top of the stack as is
            Result expression = RewriteExpression(self, lvalue.Expression, stack);
            if (expression.Action != RewriteAction.None) {
                expr = Expression.DeleteMember(expression.Node, (DeleteMemberAction)node.BindingInfo, node.Annotations);
            }
            return new Result(expression.Action, expr);
        }

        // DoStatement
        private static Result RewriteDoStatement(StackSpiller self, Expression expr, Stack stack) {
            DoStatement node = (DoStatement)expr;

            // The "do" statement requires empty stack so it can
            // guarantee it for its child nodes.
            Result body = RewriteExpression(self, node.Body, Stack.Empty);
            Result test = RewriteExpressionFreeTemps(self, node.Test, Stack.Empty);

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
        private static Result RewriteEmptyStatement(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return new Result(RewriteAction.None, expr);
        }

        // LabeledStatement
        private static Result RewriteLabeledStatement(StackSpiller self, Expression expr, Stack stack) {
            LabeledStatement node = (LabeledStatement)expr;

            Result expression = RewriteExpression(self, node.Statement, stack);
            if (expression.Action != RewriteAction.None) {
                expr = Expression.Labeled(node.Label, expression.Node, node.Annotations);
            }
            return new Result(expression.Action, expr);
        }

        // LoopStatement
        private static Result RewriteLoopStatement(StackSpiller self, Expression expr, Stack stack) {
            LoopStatement node = (LoopStatement)expr;

            // The loop statement requires empty stack for itself, so it
            // can guarantee it to the child nodes.
            Result test = RewriteExpressionFreeTemps(self, node.Test, Stack.Empty);
            Result incr = RewriteExpressionFreeTemps(self, node.Increment, Stack.Empty);
            Result body = RewriteExpression(self, node.Body, Stack.Empty);
            Result @else = RewriteExpression(self, node.ElseStatement, Stack.Empty);

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
        private static Result RewriteReturnStatement(StackSpiller self, Expression expr, Stack stack) {
            ReturnStatement node = (ReturnStatement)expr;

            // Return requires empty stack to execute so the expression is
            // going to execute on an empty stack.
            Result expression = RewriteExpressionFreeTemps(self, node.Expression, Stack.Empty);

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
        private static Result RewriteScopeExpression(StackSpiller self, Expression expr, Stack stack) {
            ScopeExpression node = (ScopeExpression)expr;

            Result body = RewriteExpression(self, node.Body, stack);

            RewriteAction action = body.Action;
            if (action != RewriteAction.None) {
                expr = new ScopeExpression(body.Node, node.Name, node.Annotations, node.Variables);
            }
            return new Result(action, expr);
        }

        // SwitchStatement
        private static Result RewriteSwitchStatement(StackSpiller self, Expression expr, Stack stack) {
            SwitchStatement node = (SwitchStatement)expr;

            // The switch statement test is emitted on the stack in current state
            Result test = RewriteExpressionFreeTemps(self, node.TestValue, stack);

            RewriteAction action = test.Action;
            ReadOnlyCollection<SwitchCase> cases = node.Cases;
            SwitchCase[] clone = null;
            for (int i = 0; i < cases.Count; i++) {
                SwitchCase @case = cases[i];

                // And all the cases also run on the same stack level.
                Result body = RewriteExpression(self, @case.Body, stack);
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
        private static Result RewriteThrowStatement(StackSpiller self, Expression expr, Stack stack) {
            ThrowStatement node = (ThrowStatement)expr;

            Result value = RewriteExpressionFreeTemps(self, node.Value, stack);
            if (value.Action != RewriteAction.None) {
                expr = Expression.Throw(value.Node, node.Annotations);
            }
            return new Result(value.Action, expr);
        }

        // TryStatement
        private static Result RewriteTryStatement(StackSpiller self, Expression expr, Stack stack) {
            TryStatement node = (TryStatement)expr;

            // Try statement definitely needs an empty stack so its
            // child nodes execute at empty stack.
            Result body = RewriteExpression(self, node.Body, Stack.Empty);
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
                        Result rfault = RewriteExpression(self, handler.Filter, Stack.Empty);
                        action |= rfault.Action;
                        curAction |= rfault.Action;
                        filter = rfault.Node;
                    }

                    // Catch block starts with an empty stack (guaranteed by TryStatement)
                    Result rbody = RewriteExpression(self, handler.Body, Stack.Empty);
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

            Result fault = RewriteExpression(self, node.Fault, Stack.Empty);
            action |= fault.Action;

            Result @finally = RewriteExpression(self, node.Finally, Stack.Empty);
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
        private static Result RewriteYieldStatement(StackSpiller self, Expression expr, Stack stack) {
            YieldStatement node = (YieldStatement)expr;

            // Yield expression is always execute on an non-empty stack
            // given the nature of the codegen.
            Result expression = RewriteExpressionFreeTemps(self, node.Expression, Stack.NonEmpty);

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
                tempArg = self.ToTemp(expression.Node, out saveArg);
                expr = Expression.Block(
                    saveArg,
                    Expression.Yield(tempArg, node.Annotations)
                );
            } else if (action == RewriteAction.Copy) {
                expr = Expression.Yield(expression.Node, node.Annotations);
            }
            return new Result(action, expr);
        }

        private static Result RewriteExtensionExpression(StackSpiller self, Expression expr, Stack stack) {
            // can't get here because DynamicNodeRewriter runs first and will take care of this
            throw Error.ExtensionNotReduced();
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
