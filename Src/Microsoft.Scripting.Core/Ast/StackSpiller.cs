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

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// Ast rewriting to spill the CLR stack into temporary variables
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
                        lambda.ReturnType, body.Node, lambda.Parameters, vars, lambda.IsGlobal,
                        lambda.IsVisible, lambda.EmitLocalDictionary, lambda.ParameterArray);
                } else {
                    GeneratorLambdaExpression g = (GeneratorLambdaExpression)lambda;
                    return new GeneratorLambdaExpression(g.Annotations, g.Type, g.Name, g.GeneratorType, g.DelegateType, body.Node, g.Parameters, vars);
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

        #endregion

        /// <summary>
        /// Will perform:
        ///     save: temp = expression
        ///     return value: temp
        /// </summary>
        private VariableExpression ToTemp(Expression expression, out Expression save) {
            VariableExpression temp = Temp(expression.Type);
            save = Ast.Assign(temp, expression);
            return temp;
        }

        #region Expressions


        /// <summary>
        /// Rewrite the expression
        /// </summary>
        /// <param name="self">Ast rewriter instance</param>
        /// <param name="node">Expression to rewrite</param>
        /// <param name="stack">State of the stack before the expression is emitted.</param>
        /// <returns>Rewritten expression.</returns>
        private static Result RewriteExpression(StackSpiller self, Expression node, Stack stack) {
            if (node == null) {
                return new Result(RewriteAction.None, null);
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
            Expression[] clone, comma;

            // Stack is never empty when dynamic site arguments are being
            // executed because the dynamic site "this" is on the stack.
            RewriteAction action = RewriteExpressions(self, node.Arguments, Stack.NonEmpty, out clone, out comma);

            if (action != RewriteAction.None) {
                expr = Ast.Action.ActionExpression(node.Action, clone, node.Type);

                if (action == RewriteAction.SpillStack) {
                    comma[comma.Length - 1] = expr;
                    expr = Ast.Comma(comma);
                }
            }
            return new Result(action, expr);
        }

        // ArrayIndexAssignment
        private static Result RewriteArrayIndexAssignment(StackSpiller self, Expression expr, Stack stack) {
            ArrayIndexAssignment node = (ArrayIndexAssignment)expr;
            // Value is evaluated first, on a stack in current state
            Result value = RewriteExpression(self, node.Value, stack);

            // Array is evaluated second, but value is saved into a temp
            // so the stack is still in the original state.
            Result array = RewriteExpression(self, node.Array, stack);

            // Index is emitted into definitely a non-empty stack
            Result index = RewriteExpression(self, node.Index, Stack.NonEmpty);

            // Did any of them change?
            RewriteAction action = value.Action | array.Action | index.Action;
            if (action == RewriteAction.SpillStack) {
                Expression saveValue, saveArray, saveIndex;
                Expression tempValue, tempArray, tempIndex;

                tempValue = self.ToTemp(value.Node, out saveValue);
                tempArray = self.ToTemp(array.Node, out saveArray);
                tempIndex = self.ToTemp(index.Node, out saveIndex);

                expr = Ast.Comma(
                    saveValue,
                    saveArray,
                    saveIndex,
                    Ast.AssignArrayIndex(tempArray, tempIndex, tempValue)
                );
            } else if (action == RewriteAction.Copy) {
                expr = Ast.AssignArrayIndex(array.Node, index.Node, value.Node);
            }

            return new Result(action, expr);
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
                expr = new BinaryExpression(node.NodeType, left.Node, right.Node, node.Type, node.Method);
            }
            return new Result(action, expr);
        }

        // BinaryExpression
        private static Result RewriteBinaryExpression(StackSpiller self, Expression expr, Stack stack) {
            BinaryExpression node = (BinaryExpression)expr;
            // Left expression executes on the stack as left by parent
            Result left = RewriteExpression(self, node.Left, stack);
            // Right expression always has non-empty stack (left is on it)
            Result right = RewriteExpression(self, node.Right, Stack.NonEmpty);

            RewriteAction action = left.Action | right.Action;
            if (action == RewriteAction.SpillStack) {
                Expression saveLeft, saveRight;
                Expression tempLeft, tempRight;

                tempLeft = self.ToTemp(left.Node, out saveLeft);
                tempRight = self.ToTemp(right.Node, out saveRight);

                expr = Ast.Comma(
                    saveLeft,
                    saveRight,
                    new BinaryExpression(node.NodeType, tempLeft, tempRight, node.Type, node.Method)
                );
            } else if (action == RewriteAction.Copy) {
                expr = new BinaryExpression(node.NodeType, left.Node, right.Node, node.Type, node.Method);
            }
            return new Result(action, expr);
        }

        // BoundAssignment
        private static Result RewriteBoundAssignment(StackSpiller self, Expression expr, Stack stack) {
            BoundAssignment node = (BoundAssignment)expr;
            // Expression is evaluated on a stack in current state
            Result value = RewriteExpression(self, node.Value, stack);
            if (value.Action != RewriteAction.None) {
                expr = Ast.Assign(node.Variable, value.Node);
            }
            return new Result(value.Action, expr);
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
                expr = Ast.Condition(test.Node, ifTrue.Node, ifFalse.Node);
            }

            return new Result(action, expr);
        }

        // ConstantExpression
        private static Result RewriteConstantExpression(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return new Result(RewriteAction.None, expr);
        }

        // DeleteUnboundExpression
        private static Result RewriteDeleteUnboundExpression(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return new Result(RewriteAction.None, expr);
        }

        // IntrinsicExpression
        private static Result RewriteIntrinsicExpression(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return new Result(RewriteAction.None, expr);
        }

        // MemberAssignment
        private static Result RewriteMemberAssignment(StackSpiller self, Expression expr, Stack stack) {
            MemberAssignment node = (MemberAssignment)expr;

            Result expression = new Result(RewriteAction.None, null);
            if (node.Expression != null) {
                // If there's an instance, it executes on the stack in current state
                // and rest is executed on non-empty stack.
                // Otherwise the stack is left unchaged.
                expression = RewriteExpression(self, node.Expression, stack);
                stack = Stack.NonEmpty;
            }
            
            Result value = RewriteExpression(self, node.Value, stack);

            RewriteAction action = expression.Action | value.Action;

            if (action == RewriteAction.Copy) {
                expr = new MemberAssignment(node.Member, expression.Node, value.Node);
            } else if (action == RewriteAction.SpillStack) {

                if (expression.Node != null) {
                    Expression saveExpression, saveValue;
                    Expression tempExpression, tempValue;
                    tempExpression = self.ToTemp(expression.Node, out saveExpression);
                    tempValue = self.ToTemp(value.Node, out saveValue);

                    expr = Ast.Comma(
                        saveExpression,
                        saveValue,
                        new MemberAssignment(node.Member, tempExpression, tempValue)
                    );
                } else {
                    // Expression is null, value gets an empty stack
                    expr = new MemberAssignment(node.Member, expression.Node, value.Node);
                }
            }
            return new Result(action, expr);
        }

        // MemberExpression
        private static Result RewriteMemberExpression(StackSpiller self, Expression expr, Stack stack) {
            MemberExpression node = (MemberExpression)expr;

            // Expression is emitted on top of the stack in current state
            Result expression = RewriteExpression(self, node.Expression, stack);
            if (expression.Action != RewriteAction.None) {
                expr = new MemberExpression(node.Member, expression.Node, node.Type);
            }
            return new Result(expression.Action, expr);
        }

        // MethodCallExpression
        // TODO: ref parameters!!!
        private static Result RewriteMethodCallExpression(StackSpiller self, Expression expr, Stack stack) {
            MethodCallExpression node = (MethodCallExpression)expr;

            Expression instance = null;
            ReadOnlyCollection<Expression> args = node.Arguments;
            Expression[] clone = null;
            Expression[] comma = null;
            RewriteAction action = RewriteAction.None;

            if (node.Instance != null) {
                // For instance methods, the instance executes on the
                // stack as is, but stays on the stack, making it non-empty.
                Result rinstance = RewriteExpression(self, node.Instance, stack);
                instance = rinstance.Node;
                action = rinstance.Action;
                stack = Stack.NonEmpty;
            }

            if (args != null) {
                int ci = 0; // comma array fill index

                for (int i = 0; i < args.Count; i++) {
                    Expression arg = args[i];
                    Result rarg = RewriteExpression(self, arg, stack);
                    action |= rarg.Action;

                    // After the first argument, stack is definitely non-empty
                    stack = Stack.NonEmpty;

                    if (clone == null && rarg.Action != RewriteAction.None) {
                        clone = Clone(args, i);
                    }

                    if (clone != null) {
                        clone[i] = rarg.Node;
                    }

                    if (comma == null && rarg.Action == RewriteAction.SpillStack) {
                        if (instance != null) {
                            comma = new Expression[args.Count + 2]; // + instance + the call
                            instance = self.ToTemp(instance, out comma[ci++]);
                        } else {
                            comma = new Expression[args.Count + 1];
                        }

                        for (int j = 0; j < i; j++) {
                            clone[j] = self.ToTemp(clone[j], out comma[ci++]);
                        }
                    }

                    if (comma != null) {
                        clone[i] = self.ToTemp(clone[i], out comma[ci++]);
                    }
                }
            }

            if (action != RewriteAction.None) {
                if (clone != null) {
                    // okay to wrap because the array won't be mutated
                    args = new ReadOnlyCollection<Expression>(clone);
                }
                expr = Ast.Call(instance, node.Method, args);

                if (comma != null) {
                    comma[comma.Length - 1] = expr;
                    expr = Ast.Comma(comma);
                }
            }

            return new Result(action, expr);
        }

        // NewArrayExpression
        private static Result RewriteNewArrayExpression(StackSpiller self, Expression expr, Stack stack) {
            NewArrayExpression node = (NewArrayExpression)expr;
            Expression[] clone, comma;

            if (node.NodeType == AstNodeType.NewArrayExpression) {
                // In a case of array construction with element initialization
                // the element expressions are never emitted on an empty stack because
                // the array reference and the index are on the stack.
                stack = Stack.NonEmpty;
            } else {
                // In a case of NewArrayBounds we make no modifications to the stack 
                // before emitting bounds expressions.
            }

            RewriteAction action = RewriteExpressions(self, node.Expressions, stack, out clone, out comma);

            if (action != RewriteAction.None) {
                expr = Ast.NewArray(node.Type, clone);

                if (action == RewriteAction.SpillStack) {
                    comma[comma.Length - 1] = expr;
                    expr = Ast.Comma(comma);
                }
            }
            return new Result(action, expr);
        }

        // NewExpression
        private static Result RewriteNewExpression(StackSpiller self, Expression expr, Stack stack) {
            NewExpression node = (NewExpression)expr;
            Expression[] clone, comma;

            // The first expression starts on a stack as provided by
            // parent, rest are definitely non-emtpy (which RewriteExpressions
            // guarantees.
            RewriteAction action = RewriteExpressions(self, node.Arguments, stack, out clone, out comma);

            if (action != RewriteAction.None) {
                expr = Ast.New(node.Constructor, clone);

                if (action == RewriteAction.SpillStack) {
                    comma[comma.Length - 1] = expr;
                    expr = Ast.Comma(comma);
                }
            }
            return new Result(action, expr);
        }

        // TypeBinaryExpression
        private static Result RewriteTypeBinaryExpression(StackSpiller self, Expression expr, Stack stack) {
            TypeBinaryExpression node = (TypeBinaryExpression)expr;
            // The expression is emitted on top of current stack
            Result expression = RewriteExpression(self, node.Expression, stack);
            if (expression.Action != RewriteAction.None) {
                expr = Ast.TypeIs(expression.Node, node.TypeOperand);
            }
            return new Result(expression.Action, expr);
        }

        // UnaryExpression
        private static Result RewriteUnaryExpression(StackSpiller self, Expression expr, Stack stack) {
            UnaryExpression node = (UnaryExpression)expr;

            // Operand is emitted on top of the stack as is
            Result expression = RewriteExpression(self, node.Operand, stack);
            if (expression.Action != RewriteAction.None) {
                expr = new UnaryExpression(node.NodeType, expression.Node, node.Type);
            }
            return new Result(expression.Action, expr);
        }

        // UnboundAssignment
        private static Result RewriteUnboundAssignment(StackSpiller self, Expression expr, Stack stack) {
            UnboundAssignment node = (UnboundAssignment)expr;

            // Value is emitted on the stack in current state
            Result expression = RewriteExpression(self, node.Value, stack);
            if (expression.Action != RewriteAction.None) {
                expr = Ast.Assign(node.Name, expression.Node);
            }
            return new Result(expression.Action, expr);
        }

        // UnboundExpression
        private static Result RewriteUnboundExpression(StackSpiller self, Expression expr, Stack stack) {
            // No action necessary
            return new Result(RewriteAction.None, expr);
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
            // No action necessary
            return new Result(RewriteAction.None, expr);
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

        // ExpressionStatement
        private static Result RewriteExpressionStatement(StackSpiller self, Expression expr, Stack stack) {
            ExpressionStatement node = (ExpressionStatement)expr;

            // Expression executes on the stack in the current state
            Result expression = RewriteExpressionFreeTemps(self, node.Expression, stack);

            if (expression.Action != RewriteAction.None) {
                expr = Ast.Statement(node.Annotations, expression.Node);
            }
            return new Result(expression.Action, expr);
        }

        // LabeledStatement
        private static Result RewriteLabeledStatement(StackSpiller self, Expression expr, Stack stack) {
            LabeledStatement node = (LabeledStatement)expr;

            Result expression = RewriteExpression(self, node.Statement, stack);
            if (expression.Action != RewriteAction.None) {
                expr = Ast.Labeled(node.Annotations, node.Label, expression.Node);
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
                expr = Ast.Loop(node.Annotations, node.Label, test.Node, incr.Node, body.Node, @else.Node);
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
                expr = Ast.Return(node.Annotations, expression.Node);
            }
            return new Result(action, expr);
        }

        // ScopeStatement
        private static Result RewriteScopeStatement(StackSpiller self, Expression expr, Stack stack) {
            ScopeStatement node = (ScopeStatement)expr;

            Result scope = RewriteExpressionFreeTemps(self, node.Scope, stack);
            Result body = RewriteExpression(self, node.Body, stack);

            RewriteAction action = scope.Action | body.Action;

            if (action != RewriteAction.None) {
                expr = Ast.Scope(node.Annotations, scope.Node, body.Node);
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
                expr = Ast.Throw(node.Annotations, value.Node);
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
                        handler = Ast.Catch(handler.Span, handler.Header, handler.Test, handler.Variable, rbody.Node);

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
                expr = Ast.Block(
                    saveArg,
                    Ast.Yield(node.Annotations, tempArg)
                );
            } else if (action == RewriteAction.Copy) {
                expr = Ast.Yield(new SourceSpan(node.Start, node.End), expression.Node);
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

        /// <summary>
        /// Rewrites all rexpressions in the collecation. If any of them changes,
        /// will allocate the cloned array and an array of initialization expressions
        /// for the resulting comma.
        /// </summary>
        /// <returns>rewrite mode</returns>
        private static RewriteAction RewriteExpressions(StackSpiller self, ReadOnlyCollection<Expression>/*!*/ expressions, Stack stack, out Expression[] clone, out Expression[] comma) {
            Debug.Assert(expressions != null);

            clone = comma = null;

            for (int i = 0, count = expressions.Count; i < count; i++) {
                Expression arg = expressions[i];

                // Rewrite the expression. The first expression has stack
                // as parent guarantees it, others will set to non-empty.
                Result exp = RewriteExpression(self, arg, stack);
                stack = Stack.NonEmpty;

                // Create the cloned array if we're rewriting
                if (clone == null && exp.Action != RewriteAction.None) {
                    clone = Clone(expressions, i);
                }

                if (clone != null) {
                    clone[i] = exp.Node;
                }

                // Create the comma array if we're spilling
                if (comma == null && exp.Action == RewriteAction.SpillStack) {
                    comma = new Expression[count + 1];
                    for (int j = 0; j < i; j++) {
                        clone[j] = self.ToTemp(clone[j], out comma[j]);
                    }
                }

                if (comma != null) {
                    clone[i] = self.ToTemp(clone[i], out comma[i]);
                }
            }

            if (clone == null) {
                return RewriteAction.None;
            } else if (comma == null) {
                return RewriteAction.Copy;
            } else {
                return RewriteAction.SpillStack;
            }
        }

        #endregion
    }
}
