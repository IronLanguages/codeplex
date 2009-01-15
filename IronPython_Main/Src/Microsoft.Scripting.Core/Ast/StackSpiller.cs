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
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {

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

        private class TempMaker {
            /// <summary>
            /// Current temporary variable
            /// </summary>
            private int _temp;

            /// <summary>
            /// List of free temporary variables. These can be recycled for new temps.
            /// </summary>
            private List<VariableExpression> _freeTemps;

            /// <summary>
            /// Stack of currently active temporary variables.
            /// </summary>
            private Stack<VariableExpression> _usedTemps;

            /// <summary>
            /// Temporary variables
            /// </summary>
            private List<VariableExpression> _temps = new List<VariableExpression>();

            internal List<VariableExpression> TemporaryVariables {
                get { return _temps; }
            }

            internal VariableExpression Temp(Type type) {
                VariableExpression temp;
                if (_freeTemps != null) {
                    // Recycle from the free-list if possible.
                    for (int i = _freeTemps.Count - 1; i >= 0; i--) {
                        temp = _freeTemps[i];
                        if (temp.Type == type) {
                            _freeTemps.RemoveAt(i);
                            return UseTemp(temp);
                        }
                    }
                }
                // Not on the free-list, create a brand new one. 
                temp = MakeTemp("$temp$" + _temp++, type);
                return UseTemp(temp);
            }

            private VariableExpression UseTemp(VariableExpression temp) {
                Debug.Assert(_freeTemps == null || !_freeTemps.Contains(temp));
                Debug.Assert(_usedTemps == null || !_usedTemps.Contains(temp));

                if (_usedTemps == null) {
                    _usedTemps = new Stack<VariableExpression>();
                }
                _usedTemps.Push(temp);
                return temp;
            }

            private void FreeTemp(VariableExpression temp) {
                Debug.Assert(_freeTemps == null || !_freeTemps.Contains(temp));
                if (_freeTemps == null) {
                    _freeTemps = new List<VariableExpression>();
                }
                _freeTemps.Add(temp);
            }

            internal int Mark() {
                return _usedTemps != null ? _usedTemps.Count : 0;
            }

            // Free temporaries created since the last marking. 
            // This is a performance optimization to lower the overall number of tempories needed.
            internal void Free(int mark) {
                // (_usedTemps != null) ==> (mark <= _usedTemps.Count)
                Debug.Assert(_usedTemps == null || mark <= _usedTemps.Count);
                // (_usedTemps == null) ==> (mark == 0)
                Debug.Assert(mark == 0 || _usedTemps != null);

                if (_usedTemps != null) {
                    while (mark < _usedTemps.Count) {
                        FreeTemp(_usedTemps.Pop());
                    }
                }
            }

            [Conditional("DEBUG")]
            internal void VerifyTemps() {
                Debug.Assert(_usedTemps == null || _usedTemps.Count == 0);
            }

            private VariableExpression MakeTemp(string name, Type type) {
                VariableExpression v = VariableExpression.Temporary(SymbolTable.StringToId(name), type);
                _temps.Add(v);
                return v;
            }
        }

        /// <summary>
        /// Rewrites child expressions, spilling them into temps if needed. The
        /// stack starts in the inital state, and after the first subexpression
        /// is added it is change to non-empty. This behavior can be overriden
        /// by setting the stack manually between adds.
        /// 
        /// When all children have been added, the caller should rewrite the 
        /// node if Rewrite is true. Then, it should call crFinish with etiher
        /// the orignal expression or the rewritten expression. Finish will call
        /// Expression.Comma if necessary and return a new Result.
        /// </summary>
        private class ChildRewriter {
            private readonly StackSpiller _self;
            private readonly Expression[] _expressions;
            private int _expressionsCount;
            private List<Expression> _comma;
            private RewriteAction _action;
            private Stack _stack;
            private bool _done;

            internal ChildRewriter(StackSpiller self, Stack stack, int count) {
                _self = self;
                _stack = stack;
                _expressions = new Expression[count];
            }

            internal Stack Stack {
                get { return _stack; }
                set { _stack = value; }
            }

            internal void Add(Expression node) {
                Debug.Assert(!_done);

                if (node == null) {
                    _expressions[_expressionsCount++] = null;
                    return;
                }

                Result exp = RewriteExpression(_self, node, _stack);
                _action |= exp.Action;
                _stack = Stack.NonEmpty;

                // track items in case we need to copy or spill stack
                _expressions[_expressionsCount++] = exp.Node;
            }

            internal void Add(params Expression[] expressions) {
                Add((IList<Expression>)expressions);
            }
            
            internal void Add(IList<Expression> expressions) {
                for (int i = 0, count = expressions.Count; i < count; i++) {
                    Add(expressions[i]);
                }
            }

            private void EnsureDone() {
                // done adding arguments, build the comma if necessary
                if (!_done) {
                    _done = true;

                    if (_action == RewriteAction.SpillStack) {
                        Expression[] clone = _expressions;
                        int count = clone.Length;
                        List<Expression> comma = new List<Expression>(count + 1);
                        for (int i = 0; i < count; i++) {
                            if (clone[i] != null) {
                                Expression temp;
                                clone[i] = _self.ToTemp(clone[i], out temp);
                                comma.Add(temp);
                            }
                        }
                        comma.Capacity = comma.Count + 1;
                        _comma = comma;
                    }
                }
            }

            internal bool Rewrite {
                get { return _action != RewriteAction.None; }
            }

            internal Result Finish(Expression expr) {
                EnsureDone();

                if (_action == RewriteAction.SpillStack) {
                    Debug.Assert(_comma.Capacity == _comma.Count + 1);
                    _comma.Add(expr);
                    expr = Expression.Comma(new ReadOnlyCollection<Expression>(_comma));
                }

                return new Result(_action, expr);
            }

            internal Expression this[int index] {
                get {
                    EnsureDone();
                    return _expressions[index];
                }
            }
            internal ReadOnlyCollection<Expression> this[int first, int last] {
                get {
                    EnsureDone();
                    if (last < 0) {
                        last += _expressions.Length;
                    }
                    int count = last - first + 1;
                    CodeContract.RequiresArrayRange(_expressions, first, count, "first", "last");

                    if (count == _expressions.Length) {
                        Debug.Assert(first == 0);
                        // if the entire array is requested just return it so we don't make a new array
                        return new ReadOnlyCollection<Expression>(_expressions);
                    }

                    Expression[] clone = new Expression[count];
                    Array.Copy(_expressions, first, clone, 0, count);
                    return new ReadOnlyCollection<Expression>(clone);
                }
            }
        }


        /// <summary>
        /// The source of temporary variables, either BlockTempMaker or RuleTempMaker
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

        /// <summary>
        /// Analyzes a rule, modifying it such that it has correct invariants
        /// for codegen. In particular, it spills the IL stack to temps in
        /// places where it's invalid to have a non-empty stack (for example,
        /// entering a try statement).
        /// 
        /// TODO: should return a new rule instead of mutating the existing one
        /// </summary>
        internal static Rule<T> AnalyzeRule<T>(Rule<T> rule) {
            StackSpiller self = new StackSpiller(new TempMaker());
            return self.Rewrite(rule);
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
                List<VariableExpression> temps = _tm.TemporaryVariables;
                ReadOnlyCollection<VariableExpression> vars = lambda.Variables;
                if (temps.Count > 0) {
                    VariableExpression[] newVars = new VariableExpression[lambda.Variables.Count + temps.Count];
                    vars.CopyTo(newVars, 0);
                    temps.CopyTo(newVars, vars.Count);
                    vars = new ReadOnlyCollection<VariableExpression>(newVars);
                }

                // Clone the lambda, replacing the body & variables
                if (lambda.NodeType == AstNodeType.Lambda) {
                    return new LambdaExpression(lambda.Annotations, AstNodeType.Lambda, lambda.Type, lambda.Name,
                        lambda.ReturnType, lambda.ScopeFactory, body.Node, lambda.Parameters, vars, lambda.IsGlobal,
                        lambda.IsVisible, lambda.EmitLocalDictionary, lambda.ParameterArray);
                } else {
                    GeneratorLambdaExpression g = (GeneratorLambdaExpression)lambda;
                    return new GeneratorLambdaExpression(
                        g.Annotations, g.Type, g.Name, g.GeneratorType, g.DelegateType, g.ScopeFactory, body.Node, g.Parameters, vars
                    );
                }
            }

            return lambda;
        }

        private Rule<T> Rewrite<T>(Rule<T> rule) {
            VerifyTemps();

            // The rule starts on empty stack.
            Result binding = RewriteExpressionFreeTemps(this, rule.Binding, Stack.Empty);

            VerifyTemps();

            if (binding.Action != RewriteAction.None) {
                List<VariableExpression> temps = _tm.TemporaryVariables;
                VariableExpression[] vars = rule.Variables;

                if (temps.Count > 0) {
                    VariableExpression[] newVars = new VariableExpression[vars.Length + temps.Count];
                    vars.CopyTo(newVars, 0);
                    temps.CopyTo(newVars, vars.Length);
                    vars = newVars;
                }
                rule = new Rule<T>(binding.Node, rule.Validators, rule.Template, rule.Parameters, vars);
            }

            return rule;
        }

        #region Temps

        private VariableExpression Temp(Type type) {
            return _tm.Temp(type);
        }

        private int Mark() {
            return _tm.Mark();
        }

        private void Free(int mark) {
            _tm.Free(mark);
        }

        [Conditional("DEBUG")]
        private void VerifyTemps() {
            _tm.VerifyTemps();
        }

        /// <summary>
        /// Will perform:
        ///     save: temp = expression
        ///     return value: temp
        /// </summary>
        private VariableExpression ToTemp(Expression expression, out Expression save) {
            VariableExpression temp = Temp(expression.Type);
            save = Expression.Assign(temp, expression);
            return temp;
        }

        #endregion

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

            if (node.IsDynamic) {
                // dynamic nodes get compiled into sites, and never have an
                // empty stack because the dynamic site is on the stack
                stack = Stack.NonEmpty;
            }

            AstNodeType ant = node.NodeType;
            Debug.Assert((int)ant < _Rewriters.Length);

            Result result = _Rewriters[(int)ant](self, node, stack);
            VerifyRewrite(result, node);

            return result;
        }

        [Conditional("DEBUG")]
        private static void VerifyRewrite(Result result, Expression node) {
            // (result.Action == RewriteAction.None) if and only if (node == result.Node)
            Debug.Assert((result.Action == RewriteAction.None) ^ (node != result.Node), "rewrite action does not match node object identity");
        }

        private static Result RewriteExpressionFreeTemps(StackSpiller self, Expression expression, Stack stack) {
            int mark = self.Mark();
            Result result = RewriteExpression(self, expression, stack);
            self.Free(mark);
            return result;
        }

        // ActionExpression
        private static Result RewriteActionExpression(StackSpiller self, Expression expr, Stack stack) {
            ActionExpression node = (ActionExpression)expr;

            // Stack is never empty when dynamic site arguments are being
            // executed because the dynamic site "this" is on the stack.
            ChildRewriter cr = new ChildRewriter(self, Stack.NonEmpty, node.Arguments.Count);
            cr.Add(node.Arguments);

            return cr.Finish(cr.Rewrite ? Expression.Action.ActionExpression(node.Action as DoOperationAction, cr[0, -1], node.Type) : expr);
        }

        // array index assignment
        private static Result RewriteArrayIndexAssignment(StackSpiller self, AssignmentExpression node, Stack stack) {
            BinaryExpression arrayIndex = (BinaryExpression)node.Expression;

            ChildRewriter cr = new ChildRewriter(self, stack, 3);

            // args are evaluated in a different order in the dynamic case
            // TODO: can we unify the order of argument evaluation
            if (node.IsDynamic) {
                cr.Add(arrayIndex.Left);
                cr.Add(arrayIndex.Right);
                cr.Add(node.Value);
                return cr.Finish(cr.Rewrite ? Expression.AssignArrayIndex(node.Annotations, cr[0], cr[1], cr[2], node.Type, node.BindingInfo as DoOperationAction) : node);                    
            }

            // Value is evaluated first, on a stack in current state
            cr.Add(node.Value);

            // Array is evaluated second, but value is saved into a temp
            // so the stack is still in the original state.
            cr.Stack = stack;
            cr.Add(arrayIndex.Left);

            // Index is emitted into definitely a non-empty stack
            cr.Add(arrayIndex.Right);

            // 1 = array, 2 = index, 0 = value
            return cr.Finish(cr.Rewrite ? Expression.AssignArrayIndex(cr[1], cr[2], cr[0]) : node);
        }

        // BinaryExpression: AndAlso, OrElse
        private static Result RewriteLogicalBinaryExpression(StackSpiller self, Expression expr, Stack stack) {
            BinaryExpression node = (BinaryExpression)expr;
            // Left expression runs on a stack as left by parent
            Result left = RewriteExpression(self, node.Left, stack);
            // ... and so does the right one
            Result right = RewriteExpression(self, node.Right, stack);

            RewriteAction action = left.Action | right.Action;
            if (action != RewriteAction.None) {
                expr = new BinaryExpression(node.NodeType, node.Annotations, left.Node, right.Node, node.Type, node.Method, node.BindingInfo);
            }
            return new Result(action, expr);
        }

        // BinaryExpression
        private static Result RewriteBinaryExpression(StackSpiller self, Expression expr, Stack stack) {
            BinaryExpression node = (BinaryExpression)expr;

            ChildRewriter cr = new ChildRewriter(self, stack, 2);
            // Left expression executes on the stack as left by parent
            cr.Add(node.Left);
            // Right expression always has non-empty stack (left is on it)
            cr.Add(node.Right);

            return cr.Finish(cr.Rewrite ? new BinaryExpression(node.NodeType, node.Annotations, cr[0], cr[1], node.Type, node.Method, node.BindingInfo) : expr);
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
                case AstNodeType.ArrayIndex:
                    return RewriteArrayIndexAssignment(self, node, stack);
                case AstNodeType.MemberExpression:
                    return RewriteMemberAssignment(self, node, stack);
                case AstNodeType.Parameter:
                case AstNodeType.LocalVariable:
                case AstNodeType.GlobalVariable:
                case AstNodeType.TemporaryVariable:
                    return RewriteVariableAssignment(self, node, stack);
                default:
                    throw new InvalidOperationException("Invalid lvalue for assignment: " + node.Expression.NodeType);
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

        // GeneratorLambdaExpression
        private static Result RewriteGeneratorLambdaExpression(StackSpiller self, Expression expr, Stack stack) {
            return RewriteLambdaExpression(self, expr, stack);
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

        // IntrinsicExpression
        private static Result RewriteIntrinsicExpression(StackSpiller self, Expression expr, Stack stack) {
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
                        new MemberExpression(lvalue.Member, cr[0], lvalue.Type, lvalue.BindingInfo as MemberAction),
                        cr[1],
                        node.Type,
                        node.BindingInfo as SetMemberAction
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
                expr = new MemberExpression(node.Member, expression.Node, node.Type, node.BindingInfo as GetMemberAction);
            }
            return new Result(expression.Action, expr);
        }

        // MethodCallExpression
        // TODO: ref parameters!!!
        private static Result RewriteMethodCallExpression(StackSpiller self, Expression expr, Stack stack) {
            MethodCallExpression node = (MethodCallExpression)expr;

            ChildRewriter cr = new ChildRewriter(self, stack, node.Arguments.Count + 1);
            
            // For instance methods, the instance executes on the
            // stack as is, but stays on the stack, making it non-empty.
            cr.Add(node.Instance);

            cr.Add(node.Arguments);

             return cr.Finish(cr.Rewrite ? new MethodCallExpression(node.Annotations, node.Type, node.BindingInfo as InvokeMemberAction, node.Method, cr[0], cr[1, -1]) : expr);
        }

        // NewArrayExpression
        private static Result RewriteNewArrayExpression(StackSpiller self, Expression expr, Stack stack) {
            NewArrayExpression node = (NewArrayExpression)expr;

            if (node.NodeType == AstNodeType.NewArrayExpression) {
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

            return cr.Finish(cr.Rewrite ? Expression.NewArray(node.Type, cr[0, -1]) : expr);
        }

        // InvocationExpression
        private static Result RewriteInvocationExpression(StackSpiller self, Expression expr, Stack stack) {
            InvocationExpression node = (InvocationExpression)expr;

            // first argument starts on stack as provided
            ChildRewriter cr = new ChildRewriter(self, stack, node.Arguments.Count + 1);
            cr.Add(node.Expression);

            // rest of arguments have non-empty stack (delegate instance on the stack)
            cr.Add(node.Arguments);

            return cr.Finish(cr.Rewrite ? new InvocationExpression(node.Annotations, cr[0], node.Type, node.BindingInfo as CallAction,  cr[1, -1]) : expr);
        }

        // NewExpression
        private static Result RewriteNewExpression(StackSpiller self, Expression expr, Stack stack) {
            NewExpression node = (NewExpression)expr;

            // The first expression starts on a stack as provided by parent,
            // rest are definitely non-emtpy (which ChildRewriter guarantees)
            ChildRewriter cr = new ChildRewriter(self, stack, node.Arguments.Count);
            cr.Add(node.Arguments);

            return cr.Finish(cr.Rewrite ? new NewExpression(node.Type, node.Constructor, cr[0, -1], node.BindingInfo as CreateInstanceAction) : expr);
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

            // Operand is emitted on top of the stack as is
            Result expression = RewriteExpression(self, node.Operand, stack);
            if (expression.Action != RewriteAction.None) {
                expr = new UnaryExpression(node.NodeType, node.Annotations, expression.Node, node.Type, node.BindingInfo);
            }
            return new Result(expression.Action, expr);
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

        // DeleteStatement
        private static Result RewriteDeleteStatement(StackSpiller self, Expression expr, Stack stack) {
            DeleteStatement node = (DeleteStatement)expr;

            // Operand is emitted on top of the stack as is
            Result expression = RewriteExpression(self, node.Variable, stack);
            if (expression.Action != RewriteAction.None) {
                expr = new DeleteStatement(node.Annotations, expression.Node, node.Type, node.BindingInfo as DeleteMemberAction);
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
                expr = Expression.Labeled(node.Annotations, node.Label, expression.Node);
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
                expr = Expression.Loop(node.Annotations, node.Label, test.Node, incr.Node, body.Node, @else.Node);
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
                expr = Expression.Return(node.Annotations, expression.Node);
            }
            return new Result(action, expr);
        }

        // ScopeStatement
        private static Result RewriteScopeStatement(StackSpiller self, Expression expr, Stack stack) {
            ScopeStatement node = (ScopeStatement)expr;

            Result body = RewriteExpression(self, node.Body, stack);

            RewriteAction action = body.Action;
            if (action != RewriteAction.None) {
                expr = Expression.Scope(node.Annotations, node.Factory, body.Node);
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
                    @case = new SwitchCase(@case.Header, @case.IsDefault, @case.Value, body.Node);

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

                expr = new SwitchStatement(node.Annotations, node.Label, test.Node, cases);
            }

            return new Result(action, expr);
        }

        // ThrowStatement
        private static Result RewriteThrowStatement(StackSpiller self, Expression expr, Stack stack) {
            ThrowStatement node = (ThrowStatement)expr;

            Result value = RewriteExpressionFreeTemps(self, node.Value, stack);
            if (value.Action != RewriteAction.None) {
                expr = Expression.Throw(node.Annotations, value.Node);
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
                    CatchBlock handler = handlers[i];

                    // Catch block starts with an empty stack (guaranteed by TryStatement)
                    Result rbody = RewriteExpression(self, handler.Body, Stack.Empty);
                    action |= rbody.Action;

                    if (rbody.Action != RewriteAction.None) {
                        handler = Expression.Catch(handler.Span, handler.Header, handler.Test, handler.Variable, rbody.Node);

                        if (clone == null) {
                            clone = Clone(handlers, i);
                        }
                    }

                    if (clone != null) {
                        clone[i] = handler;
                    }
                }
            }

            Result @finally = RewriteExpression(self, node.FinallyStatement, Stack.Empty);
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

                expr = new TryStatement(node.Annotations, body.Node, handlers, @finally.Node);
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
                    Expression.Yield(node.Annotations, tempArg)
                );
            } else if (action == RewriteAction.Copy) {
                expr = Expression.Yield(new SourceSpan(node.Start, node.End), expression.Node);
            }
            return new Result(action, expr);
        }

        #endregion

        #region Cloning

        /// <summary>
        /// Will clone an IList into an array of the same size, and copy
        /// all vaues up to (and NOT including) the max index
        /// </summary>
        /// <returns>The cloned array.</returns>
        private static T[] Clone<T>(ReadOnlyCollection<T>/*!*/ roc, int max) {
            Debug.Assert(roc != null);
            Debug.Assert(max < roc.Count);

            T[] clone = new T[roc.Count];
            for (int j = 0; j < max; j++) {
                clone[j] = roc[j];
            }
            return clone;
        }

        #endregion
    }
}
