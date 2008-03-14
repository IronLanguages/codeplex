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
    partial class AstRewriter {

        // Is the evaluation stack empty?
        private enum Stack {
            Empty,
            NonEmpty
        };

        private abstract class TempMaker {
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

            protected internal abstract VariableExpression MakeTemp(string name, Type type);
        }

        private class BlockTempMaker : TempMaker {
            /// <summary>
            /// LambdaExpression the body of which is being rewritten
            /// </summary>
            private readonly LambdaExpression _lambda;

            internal BlockTempMaker(LambdaExpression lambda) {
                Debug.Assert(lambda != null);
                _lambda = lambda;
            }
            protected internal override VariableExpression MakeTemp(string name, Type type) {
                return _lambda.CreateTemporaryVariable(SymbolTable.StringToId(name), type);
            }
        }

        private class RuleTempMaker : TempMaker {
            /// <summary>
            /// Rule which is being rewritten
            /// </summary>
            private readonly StandardRule _rule;

            internal RuleTempMaker(StandardRule rule) {
                Debug.Assert(rule != null);
                _rule = rule;
            }

            protected internal override VariableExpression MakeTemp(string name, Type type) {
                return _rule.GetTemporary(type, name);
            }
        }

        /// <summary>
        /// The source of temporary variables, either BlockTempMaker or RuleTempMaker
        /// </summary>
        private readonly TempMaker _tm;

        /// <summary>
        /// Mapping of statements before and after rewrite.
        /// This is to handle break and continue statements
        /// which must be redirected to the new statements.
        /// </summary>
        private Dictionary<Expression, Expression> _map;

        /// <summary>
        /// List of break statements in the AST
        /// </summary>
        private List<BreakStatement> _break;

        /// <summary>
        /// List of continue statements in the AST
        /// </summary>
        private List<ContinueStatement> _continue;

        #region Rewriter entry points

        public static void RewriteBlock(LambdaExpression lambda) {
            AstRewriter ar = new AstRewriter(new BlockTempMaker(lambda));
            ar.Rewrite(lambda);
        }

        public static void RewriteRule(StandardRule rule) {
            AstRewriter ar = new AstRewriter(new RuleTempMaker(rule));
            ar.Rewrite(rule);
        }

        #endregion

        private AstRewriter(TempMaker tm) {
            _tm = tm;
        }

        private void Rewrite(LambdaExpression lambda) {
            VerifyTemps();

            // Lambda starts with an empty stack
            Expression body = RewriteExpressionFreeTemps(this, lambda.Body, Stack.Empty);

            VerifyTemps();

            if ((object)body != (object)lambda.Body) {
                FixBreakAndContinue();
                lambda.Body = body;
            }
        }

        private void Rewrite(StandardRule rule) {
            VerifyTemps();

            // The rule test starts on empty stack.
            Expression test = RewriteExpressionFreeTemps(this, rule.Test, Stack.Empty);
            // So does the target
            Expression target = RewriteExpressionFreeTemps(this, rule.Target, Stack.Empty);

            VerifyTemps();

            if (((object)test != (object)rule.Test) ||
                ((object)target != (object)rule.Target)) {
                FixBreakAndContinue();

                if ((object)test != (object)rule.Test) {
                    rule.RewriteTest(test);
                }
                if ((object)target != (object)rule.Target) {
                    rule.RewriteTarget(target);
                }
            }
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
        private Expression ToTemp(Expression expression, out Expression save) {
            VariableExpression temp = Temp(expression.Type);
            save = Ast.Assign(temp, expression);
            return Ast.Read(temp);
        }

        #region Rewritten statement mapping

        private Expression Map(Expression original, Expression rewritten) {
            if ((object)original != (object)rewritten) {
                if (_map == null) {
                    _map = new Dictionary<Expression, Expression>();
                }
                _map.Add(original, rewritten);
            }
            return rewritten;
        }

        private bool TryFindMapping(Expression original, out Expression rewritten) {
            if (_map != null) {
                return _map.TryGetValue(original, out rewritten);
            } else {
                rewritten = null;
                return false;
            }
        }

        private void FixBreakAndContinue() {
            Expression mapped;

            if (_break != null) {
                foreach (BreakStatement b in _break) {
                    if (TryFindMapping(b.Statement, out mapped)) {
                        b.Statement = mapped;
                    }
                }
            }
            if (_continue != null) {
                foreach (ContinueStatement c in _continue) {
                    if (TryFindMapping(c.Statement, out mapped)) {
                        c.Statement = mapped;
                    }
                }
            }
        }

        #endregion

        #region Expressions


        /// <summary>
        /// Rewrite the expression
        /// </summary>
        /// <param name="ar">Ast rewriter instance</param>
        /// <param name="node">Expression to rewrite</param>
        /// <param name="stack">State of the stack before the expression is emitted.</param>
        /// <returns>Rewritten expression.</returns>
        private static Expression RewriteExpression(AstRewriter ar, Expression node, Stack stack) {
            if (node == null) {
                return null;
            }

            AstNodeType ant = node.NodeType;
            Debug.Assert((int)ant < _Rewriters.Length);

            Expression result = _Rewriters[(int)ant](ar, node, stack);

            // Store the mapping to resolve the break and continue later
            return ar.Map(node, result);
        }

        private static Expression RewriteExpressionFreeTemps(AstRewriter ar, Expression expression, Stack stack) {
            int mark = ar.Mark();
            expression = RewriteExpression(ar, expression, stack);
            ar.Free(mark);
            return expression;
        }

        // ActionExpression
        private static Expression RewriteActionExpression(AstRewriter ar, Expression expr, Stack stack) {
            ActionExpression node = (ActionExpression)expr;
            Expression[] clone, comma;

            // Stack is never empty when dynamic site arguments are being
            // executed because the dynamic site "this" is on the stack.
            if (RewriteExpressions(ar, node.Arguments, Stack.NonEmpty, out clone, out comma)) {
                comma[comma.Length - 1] =
                    Ast.Action.ActionExpression(node.Action, clone, node.Type);
                return Ast.Comma(comma);
            } else {
                return node;
            }
        }

        // ArrayIndexAssignment
        private static Expression RewriteArrayIndexAssignment(AstRewriter ar, Expression expr, Stack stack) {
            ArrayIndexAssignment node = (ArrayIndexAssignment)expr;
            // Value is evaluated first, on a stack in current state
            Expression value = RewriteExpression(ar, node.Value, stack);

            // Array is evaluated second, but value is saved into a temp
            // so the stack is still in the original state.
            Expression array = RewriteExpression(ar, node.Array, stack);

            // Index is emitted into definitely a non-empty stack
            Expression index = RewriteExpression(ar, node.Index, Stack.NonEmpty);

            // Did any of them change?
            if (((object)value != (object)node.Value) ||
                ((object)array != (object)node.Array) ||
                ((object)index != (object)node.Index)) {

                Expression saveValue, saveArray, saveIndex;

                value = ar.ToTemp(value, out saveValue);
                array = ar.ToTemp(array, out saveArray);
                index = ar.ToTemp(index, out saveIndex);

                return Ast.Comma(
                    saveValue,
                    saveArray,
                    saveIndex,
                    Ast.AssignArrayIndex(array, index, value)
                );
            } else {
                return node;
            }
        }

        // BinaryExpression: AndAlso, OrElse
        private static Expression RewriteLogicalBinaryExpression(AstRewriter ar, Expression expr, Stack stack) {
            BinaryExpression node = (BinaryExpression)expr;
            // Left expression runs on a stack as left by parent
            Expression left = RewriteExpression(ar, node.Left, stack);
            // ... and so does the right one
            Expression right = RewriteExpression(ar, node.Right, stack);

            if (((object)left != (object)node.Left) ||
                ((object)right != (object)node.Right)) {
                return new BinaryExpression(node.NodeType, left, right, node.Type, node.Method);
            } else {
                return node;
            }
        }

        // BinaryExpression
        private static Expression RewriteBinaryExpression(AstRewriter ar, Expression expr, Stack stack) {
            BinaryExpression node = (BinaryExpression)expr;
            // Left expression executes on the stack as left by parent
            Expression left = RewriteExpression(ar, node.Left, stack);
            // Right expression always has non-empty stack (left is on it)
            Expression right = RewriteExpression(ar, node.Right, Stack.NonEmpty);

            if (((object)left != (object)node.Left) ||
                ((object)right != (object)node.Right)) {
                Expression saveLeft, saveRight;

                left = ar.ToTemp(left, out saveLeft);
                right = ar.ToTemp(right, out saveRight);

                return Ast.Comma(
                    saveLeft,
                    saveRight,
                    new BinaryExpression(node.NodeType, left, right, node.Type, node.Method)
                );
            } else {
                return node;
            }
        }

        // BoundAssignment
        private static Expression RewriteBoundAssignment(AstRewriter ar, Expression expr, Stack stack) {
            BoundAssignment node = (BoundAssignment)expr;
            // Expression is evaluated on a stack in current state
            Expression value = RewriteExpression(ar, node.Value, stack);
            if ((object)value != (object)node.Value) {
                return Ast.Assign(node.Variable, value);
            } else {
                return node;
            }
        }

        // Variable
        private static Expression RewriteVariableExpression(AstRewriter ar, Expression expr, Stack stack) {
            // No action necessary, regardless of the stack state
            return expr;
        }

        // LambdaExpression
        private static Expression RewriteLambdaExpression(AstRewriter ar, Expression expr, Stack stack) {
            // No action necessary, regardless of the stack state.
            return expr;
        }

        // LambdaExpression
        private static Expression RewriteGeneratorLambdaExpression(AstRewriter ar, Expression expr, Stack stack) {
            // No action necessary, regardless of the stack state.
            return expr;
        }

        // ConditionalExpression
        private static Expression RewriteConditionalExpression(AstRewriter ar, Expression expr, Stack stack) {
            ConditionalExpression node = (ConditionalExpression)expr;
            // Test executes at the stack as left by parent
            Expression test = RewriteExpression(ar, node.Test, stack);
            // The test is popped by conditional jump so branches execute
            // at the stack as left by parent too.
            Expression ifTrue = RewriteExpression(ar, node.IfTrue, stack);
            Expression ifFalse = RewriteExpression(ar, node.IfFalse, stack);

            if (((object)test != (object)node.Test) ||
                ((object)ifTrue != (object)node.IfTrue) ||
                ((object)ifFalse != (object)node.IfFalse)) {
                return Ast.Condition(test, ifTrue, ifFalse);
            } else {
                return node;
            }
        }

        // ConstantExpression
        private static Expression RewriteConstantExpression(AstRewriter ar, Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return expr;
        }

        // DeleteUnboundExpression
        private static Expression RewriteDeleteUnboundExpression(AstRewriter ar, Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return expr;
        }

        // IntrinsicExpression
        private static Expression RewriteIntrinsicExpression(AstRewriter ar, Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return expr;
        }

        // MemberAssignment
        private static Expression RewriteMemberAssignment(AstRewriter ar, Expression expr, Stack stack) {
            MemberAssignment node = (MemberAssignment)expr;

            Expression expression = null;
            if (node.Expression != null) {
                // If there's an instance, it executes on the stack in current state
                // and rest is executed on non-empty stack.
                // Otherwise the stack is left unchaged.
                expression = RewriteExpression(ar, node.Expression, stack);
                stack = Stack.NonEmpty;
            }
            
            Expression value = RewriteExpression(ar, node.Value, stack);

            if (((object)expression != (object)node.Expression) ||
                ((object)value != (object)node.Value)) {

                if (expression != null) {
                    Expression saveExpression, saveValue;
                    expression = ar.ToTemp(expression, out saveExpression);
                    value = ar.ToTemp(value, out saveValue);

                    return Ast.Comma(
                        saveExpression,
                        saveValue,
                        new MemberAssignment(node.Member, expression, value)
                    );
                } else {
                    // Expression is null, value gets an empty stack
                    return new MemberAssignment(node.Member, expression, value);
                }
            } else {
                return node;
            }
        }

        // MemberExpression
        private static Expression RewriteMemberExpression(AstRewriter ar, Expression expr, Stack stack) {
            MemberExpression node = (MemberExpression)expr;

            // Expression is emitted on top of the stack in current state
            Expression expression = RewriteExpression(ar, node.Expression, stack);
            if ((object)expression != (object)node.Expression) {
                return new MemberExpression(node.Member, expression, node.Type);
            } else {
                return node;
            }
        }

        // MethodCallExpression
        // TODO: ref parameters!!!
        private static Expression RewriteMethodCallExpression(AstRewriter ar, Expression expr, Stack stack) {
            MethodCallExpression node = (MethodCallExpression)expr;

            Expression instance = null;
            ReadOnlyCollection<Expression> args = node.Arguments;
            Expression[] clone = null;
            Expression[] comma = null;
            int ci = 0; // comma array fill index

            if (node.Instance != null) {
                // For instance methods, the instance executes on the
                // stack as is, but stays on the stack, making it non-empty.
                instance = RewriteExpression(ar, node.Instance, stack);
                stack = Stack.NonEmpty;
            }

            if (args != null) {
                for (int i = 0; i < args.Count; i++) {
                    Expression arg = args[i];
                    Expression rarg = RewriteExpression(ar, arg, stack);

                    // After the first argument, stack is definitely non-empty
                    stack = Stack.NonEmpty;

                    if ((object)arg != (object)rarg) {
                        if (clone == null) {
                            clone = new Expression[args.Count];
                            if (instance != null) {
                                comma = new Expression[args.Count + 2]; // + instance + the call
                                instance = ar.ToTemp(instance, out comma[ci++]);
                            } else {
                                comma = new Expression[args.Count + 1];
                            }

                            for (int j = 0; j < i; j++) {
                                clone[j] = ar.ToTemp(args[j], out comma[ci++]);
                            }
                        }
                    }

                    if (clone != null) {
                        clone[i] = ar.ToTemp(rarg, out comma[ci++]);
                    }
                }
            }

            if (clone != null) {
                comma[ci] = Ast.Call(instance, node.Method, clone);
                return Ast.Comma(comma);
            } else if ((object)instance != (object)node.Instance) {
                return Ast.Call(
                    instance,
                    node.Method,
                    node.Arguments
                );
            } else {
                return node;
            }
        }

        // NewArrayExpression
        private static Expression RewriteNewArrayExpression(AstRewriter ar, Expression expr, Stack stack) {
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
            if (RewriteExpressions(ar, node.Expressions, stack, out clone, out comma)) {
                comma[comma.Length - 1] = Ast.NewArray(node.Type, clone);
                return Ast.Comma(comma);
            } else {
                return node;
            }
        }

        // NewExpression
        private static Expression RewriteNewExpression(AstRewriter ar, Expression expr, Stack stack) {
            NewExpression node = (NewExpression)expr;
            Expression[] clone, comma;

            // The first expression starts on a stack as provided by
            // parent, rest are definitely non-emtpy (which RewriteExpressions
            // guarantees.
            if (RewriteExpressions(ar, node.Arguments, stack, out clone, out comma)) {
                comma[comma.Length - 1] =
                    Ast.New(node.Constructor, clone);
                return Ast.Comma(comma);
            } else {
                return node;
            }
        }

        // TypeBinaryExpression
        private static Expression RewriteTypeBinaryExpression(AstRewriter ar, Expression expr, Stack stack) {
            TypeBinaryExpression node = (TypeBinaryExpression)expr;
            // The expression is emitted on top of current stack
            Expression expression = RewriteExpression(ar, node.Expression, stack);
            if ((object)expression != (object)node.Expression) {
                return Ast.TypeIs(expression, node.TypeOperand);
            } else {
                return node;
            }
        }

        // UnaryExpression
        private static Expression RewriteUnaryExpression(AstRewriter ar, Expression expr, Stack stack) {
            UnaryExpression node = (UnaryExpression)expr;
            // Operand is emitted on top of the stack as is
            Expression expression = RewriteExpression(ar, node.Operand, stack);
            if ((object)expression != (object)node.Operand) {
                return new UnaryExpression(node.NodeType, expression, node.Type);
            } else {
                return node;
            }
        }

        // UnboundAssignment
        private static Expression RewriteUnboundAssignment(AstRewriter ar, Expression expr, Stack stack) {
            UnboundAssignment node = (UnboundAssignment)expr;

            // Value is emitted on the stack in current state
            Expression expression = RewriteExpression(ar, node.Value, stack);
            if ((object)expression != (object)node.Value) {
                return Ast.Assign(node.Name, expression);
            } else {
                return node;
            }
        }

        // UnboundExpression
        private static Expression RewriteUnboundExpression(AstRewriter ar, Expression expr, Stack stack) {
            // No action necessary
            return expr;
        }

        #endregion

        #region Statements

        // Block
        private static Expression RewriteBlock(AstRewriter ar, Expression expr, Stack stack) {
            Block node = (Block)expr;
            ReadOnlyCollection<Expression> expressions = node.Expressions;
            Expression[] clone = null;

            for (int i = 0; i < expressions.Count; i++) {
                Expression expression = expressions[i];
                // All statements within the block execute at the
                // same stack state.
                Expression rewritten = RewriteExpression(ar, expression, stack);

                if (((object)rewritten != (object)expression) && (clone == null)) {
                    clone = Clone(expressions, i);
                }

                if (clone != null) {
                    clone[i] = rewritten;
                }
            }

            if (clone != null) {
                return new Block(node.Start, node.End, CollectionUtils.ToReadOnlyCollection(clone), node.Type);
            } else {
                return node;
            }
        }

        // BreakStatement
        private static Expression RewriteBreakStatement(AstRewriter ar, Expression expr, Stack stack) {
            BreakStatement node = (BreakStatement)expr;

            if (node.Statement != null) {
                if (ar._break == null) {
                    ar._break = new List<BreakStatement>();
                }
                ar._break.Add(node);
            }

            // No further action necessary
            return node;
        }

        // ContinueStatement
        private static Expression RewriteContinueStatement(AstRewriter ar, Expression expr, Stack stack) {
            ContinueStatement node = (ContinueStatement)expr;

            if (node.Statement != null) {
                if (ar._continue == null) {
                    ar._continue = new List<ContinueStatement>();
                }
                ar._continue.Add(node);
            }

            // No further action necessary
            return node;
        }

        // DeleteStatement
        private static Expression RewriteDeleteStatement(AstRewriter ar, Expression expr, Stack stack) {
            // No action necessary
            return expr;
        }

        // DoStatement
        private static Expression RewriteDoStatement(AstRewriter ar, Expression expr, Stack stack) {
            DoStatement node = (DoStatement)expr;

            // The "do" statement requires empty stack so it can
            // guarantee it for its child nodes.
            Expression body = RewriteExpression(ar, node.Body, Stack.Empty);
            Expression test = RewriteExpressionFreeTemps(ar, node.Test, Stack.Empty);

            // Loop needs empty stack to execute so if the stack is initially
            // not empty, we rewrite to get empty stack.
            if (((object)body != (object)node.Body) ||
                ((object)test != (object)node.Test) ||
                stack != Stack.Empty) {
                return new DoStatement(node.Start, node.End, node.Header, test, body);
            } else {
                return node;
            }
        }

        // EmptyStatement
        private static Expression RewriteEmptyStatement(AstRewriter ar, Expression expr, Stack stack) {
            // No action necessary, regardless of stack
            return expr;
        }

        // ExpressionStatement
        private static Expression RewriteExpressionStatement(AstRewriter ar, Expression expr, Stack stack) {
            ExpressionStatement node = (ExpressionStatement)expr;

            // Expression executes on the stack in the current state
            Expression expression = RewriteExpressionFreeTemps(ar, node.Expression, stack);

            if ((object)expression != (object)node.Expression) {
                return Ast.Statement(new SourceSpan(node.Start, node.End), expression);
            } else {
                return node;
            }
        }

        // LabeledStatement
        private static Expression RewriteLabeledStatement(AstRewriter ar, Expression expr, Stack stack) {
            LabeledStatement node = (LabeledStatement)expr;

            Expression expression = RewriteExpression(ar, node.Statement, stack);
            if ((object)expression != (object)node.Statement) {
                return Ast.Labeled(new SourceSpan(node.Start, node.End), expression);
            } else {
                return node;
            }
        }

        // LoopStatement
        private static Expression RewriteLoopStatement(AstRewriter ar, Expression expr, Stack stack) {
            LoopStatement node = (LoopStatement)expr;

            // The loop statement requires empty stack for itself, so it
            // can guarantee it to the child nodes.
            Expression test = RewriteExpressionFreeTemps(ar, node.Test, Stack.Empty);
            Expression incr = RewriteExpressionFreeTemps(ar, node.Increment, Stack.Empty);
            Expression body = RewriteExpression(ar, node.Body, Stack.Empty);
            Expression @else = RewriteExpression(ar, node.ElseStatement, Stack.Empty);

            // However, the loop itself requires that it executes on an empty stack
            // so we need to rewrite if the stack is not empty.
            if (((object)test != (object)node.Test) ||
                ((object)incr != (object)node.Increment) ||
                ((object)body != (object)node.Body) ||
                ((object)@else != (object)node.ElseStatement) ||
                stack != Stack.Empty) {
                return Ast.Loop(new SourceSpan(node.Start, node.End), node.Header, test, incr, body, @else);
            } else {
                return node;
            }
        }

        // ReturnStatement
        private static Expression RewriteReturnStatement(AstRewriter ar, Expression expr, Stack stack) {
            ReturnStatement node = (ReturnStatement)expr;

            // Return requires empty stack to execute so the expression is
            // going to execute on an empty stack.
            Expression expression = RewriteExpressionFreeTemps(ar, node.Expression, Stack.Empty);

            // However, the statement itself needs an empty stack for itself
            // so if stack is not empty, rewrite to empty the stack.
            if ((object)expression != (object)node.Expression ||
                stack != Stack.Empty) {
                return Ast.Return(new SourceSpan(node.Start, node.End), expression);
            } else {
                return node;
            }
        }

        // ScopeStatement
        private static Expression RewriteScopeStatement(AstRewriter ar, Expression expr, Stack stack) {
            ScopeStatement node = (ScopeStatement)expr;

            Expression scope = RewriteExpressionFreeTemps(ar, node.Scope, stack);
            Expression body = RewriteExpression(ar, node.Body, stack);

            if (((object)scope != (object)node.Scope) ||
                ((object)body != (object)node.Body)) {
                return Ast.Scope(new SourceSpan(node.Start, node.End), scope, body);
            } else {
                return node;
            }
        }

        // SwitchStatement
        private static Expression RewriteSwitchStatement(AstRewriter ar, Expression expr, Stack stack) {
            SwitchStatement node = (SwitchStatement)expr;

            // The switch statement test is emitted on the stack in current state
            Expression test = RewriteExpressionFreeTemps(ar, node.TestValue, stack);
            ReadOnlyCollection<SwitchCase> cases = node.Cases;
            SwitchCase[] clone = null;

            for (int i = 0; i < cases.Count; i++) {
                SwitchCase @case = cases[i];
                // And all the cases also run on the same stack level.
                SwitchCase rcase = Rewrite(ar, @case, stack);

                if (((object)rcase != (object)@case) && (clone == null)) {
                    clone = Clone(cases, i);
                }

                if (clone != null) {
                    clone[i] = rcase;
                }
            }

            if (clone != null) {
                return Ast.Switch(new SourceSpan(node.Start, node.End), node.Header, test, clone);
            } else if ((object)test != (object)node.TestValue) {
                return Ast.Switch(new SourceSpan(node.Start, node.End), node.Header, test, ArrayUtils.ToArray(node.Cases));
            } else {
                return node;
            }
        }

        // ThrowStatement
        private static Expression RewriteThrowStatement(AstRewriter ar, Expression expr, Stack stack) {
            ThrowStatement node = (ThrowStatement)expr;

            Expression value = RewriteExpressionFreeTemps(ar, node.Value, stack);
            if ((object)value != (object)node.Value) {
                return Ast.Throw(new SourceSpan(node.Start, node.End), value);
            } else {
                return node;
            }
        }

        // TryStatement
        private static Expression RewriteTryStatement(AstRewriter ar, Expression expr, Stack stack) {
            TryStatement node = (TryStatement)expr;

            // Try statement definitely needs an empty stack so its
            // child nodes execute at empty stack.
            Expression body = RewriteExpression(ar, node.Body, Stack.Empty);
            ReadOnlyCollection<CatchBlock> handlers = node.Handlers;
            CatchBlock[] clone = null;

            if (handlers != null) {
                for (int i = 0; i < handlers.Count; i++) {
                    CatchBlock handler = handlers[i];
                    CatchBlock rhandler = Rewrite(ar, handler);

                    if (((object)rhandler != (object)handler) && (clone == null)) {
                        clone = Clone(handlers, i);
                    }

                    if (clone != null) {
                        clone[i] = rhandler;
                    }
                }
            }

            Expression @finally = RewriteExpression(ar, node.FinallyStatement, Stack.Empty);

            // If the stack is initially not empty, rewrite to spill the stack
            if ((clone != null) ||
                ((object)body != (object)node.Body) ||
                ((object)@finally != (object)node.FinallyStatement) ||
                stack != Stack.Empty) {

                if (clone != null) {
                    handlers = CollectionUtils.ToReadOnlyCollection(clone);
                }

                return new TryStatement(node.Start, node.End, node.Header, body, handlers, @finally);
            } else {
                return node;
            }
        }

        // YieldStatement
        private static Expression RewriteYieldStatement(AstRewriter ar, Expression expr, Stack stack) {
            YieldStatement node = (YieldStatement)expr;

            // Yield expression is always execute on an non-empty stack
            // given the nature of the codegen.
            Expression expression = RewriteExpressionFreeTemps(ar, node.Expression, Stack.NonEmpty);

            if ((object)expression != (object)node.Expression || stack != Stack.Empty) {
                // Yield's argument was changed. We may need to hoist it.
                // This will flatten nested yields, which simplifies yield codegen. So:
                //   yield (yield x)
                // becomes:
                //   $t = yield x
                //   yield $t
                Expression saveArg;
                expression = ar.ToTemp(expression, out saveArg);
                return Ast.Block(
                    saveArg,
                    Ast.Yield(new SourceSpan(node.Start, node.End), expression)
                );
            } else {
                return node;
            }
        }

        #endregion

        #region Nodes

        // CatchBlock
        private static CatchBlock Rewrite(AstRewriter ar, CatchBlock node) {
            // Catch block starts with an empty stack (guaranteed by TryStatement)
            Expression body = RewriteExpression(ar, node.Body, Stack.Empty);

            if ((object)body != (object)node.Body) {
                return Ast.Catch(node.Span, node.Header, node.Test, node.Variable, body);
            } else {
                return node;
            }
        }

        // IfStatementTest
        private static IfStatementTest Rewrite(AstRewriter ar, IfStatementTest node, Stack stack) {
            Expression test = RewriteExpressionFreeTemps(ar, node.Test, stack);
            Expression body = RewriteExpression(ar, node.Body, stack);

            if (((object)test != (object)node.Test) ||
                ((object)body != (object)node.Body)) {
                return new IfStatementTest(node.Start, node.End, node.Header, test, body);
            } else {
                return node;
            }
        }

        // SwitchCase
        private static SwitchCase Rewrite(AstRewriter ar, SwitchCase node, Stack stack) {
            Expression body = RewriteExpression(ar, node.Body, stack);

            if ((object)body != (object)node.Body) {
                return new SwitchCase(node.Header, node.IsDefault, node.Value, body);
            } else {
                return node;
            }
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
        /// <returns>Cloned array or null, if no change encountered.</returns>
        private static bool RewriteExpressions(AstRewriter ar, ReadOnlyCollection<Expression>/*!*/ expressions, Stack stack, out Expression[] clone, out Expression[] comma) {
            Debug.Assert(expressions != null);

            clone = comma = null;

            for (int i = 0, count = expressions.Count; i < count; i++) {
                Expression arg = expressions[i];

                // Rewrite the expression. The first expression has stack
                // as parent guarantees it, others will set to non-empty.
                Expression exp = RewriteExpression(ar, arg, stack);
                stack = Stack.NonEmpty;

                // The expression has been rewritten, rewrite this too.
                if (((object)arg != (object)exp) && (clone == null)) {
                    clone = new Expression[count];
                    comma = new Expression[count + 1];
                    for (int j = 0; j < i; j++) {
                        clone[j] = ar.ToTemp(expressions[j], out comma[j]);
                    }
                }

                if (clone != null) {
                    clone[i] = ar.ToTemp(exp, out comma[i]);
                }
            }

            return clone != null;
        }

        #endregion
    }
}
