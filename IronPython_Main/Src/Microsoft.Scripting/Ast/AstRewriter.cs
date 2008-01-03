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
    class AstRewriter {

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
            private List<Variable> _freeTemps;

            /// <summary>
            /// Stack of currently active temporary variables.
            /// </summary>
            private Stack<Variable> _usedTemps;

            internal Variable Temp(Type type) {
                Variable temp;
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

            private Variable UseTemp(Variable temp) {
                Debug.Assert(_freeTemps == null || !_freeTemps.Contains(temp));
                Debug.Assert(_usedTemps == null || !_usedTemps.Contains(temp));

                if (_usedTemps == null) {
                    _usedTemps = new Stack<Variable>();
                }
                _usedTemps.Push(temp);
                return temp;
            }

            private void FreeTemp(Variable temp) {
                Debug.Assert(_freeTemps == null || !_freeTemps.Contains(temp));
                if (_freeTemps == null) {
                    _freeTemps = new List<Variable>();
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

            protected internal abstract Variable MakeTemp(string name, Type type);
        }

        private class BlockTempMaker : TempMaker {
            /// <summary>
            /// CodeBlock the body of which is being rewritten
            /// </summary>
            private readonly CodeBlock _block;

            internal BlockTempMaker(CodeBlock block) {
                Debug.Assert(block != null);
                _block = block;
            }
            protected internal override Variable MakeTemp(string name, Type type) {
                return _block.CreateTemporaryVariable(SymbolTable.StringToId(name), type);
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

            protected internal override Variable MakeTemp(string name, Type type) {
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
        private Dictionary<Statement, Statement> _map;

        /// <summary>
        /// List of break statements in the AST
        /// </summary>
        private List<BreakStatement> _break;

        /// <summary>
        /// List of continue statements in the AST
        /// </summary>
        private List<ContinueStatement> _continue;

        #region Rewriter entry points

        public static void RewriteBlock(CodeBlock block) {
            AstRewriter ar = new AstRewriter(new BlockTempMaker(block));
            ar.Rewrite(block);
        }

        public static void RewriteRule(StandardRule rule) {
            AstRewriter ar = new AstRewriter(new RuleTempMaker(rule));
            ar.Rewrite(rule);
        }

        #endregion

        private AstRewriter(TempMaker tm) {
            _tm = tm;
        }

        private void Rewrite(CodeBlock block) {
            VerifyTemps();

            // Block starts with an empty stack
            Statement body = RewriteStatement(block.Body, Stack.Empty);

            VerifyTemps();

            if ((object)body != (object)block.Body) {
                FixBreakAndContinue();
                block.Body = body;
            }
        }

        private void Rewrite(StandardRule rule) {
            VerifyTemps();

            // The rule test starts on empty stack.
            Expression test = RewriteExpressionFreeTemps(rule.Test, Stack.Empty);
            // So does the target
            Statement target = RewriteStatement(rule.Target, Stack.Empty);

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

        private Variable Temp(Type type) {
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
            Variable temp = Temp(expression.Type);
            save = Ast.Assign(temp, expression);
            return Ast.Read(temp);
        }

        #region Rewritten statement mapping

        private Statement Map(Statement original, Statement rewritten) {
            if ((object)original != (object)rewritten) {
                if (_map == null) {
                    _map = new Dictionary<Statement, Statement>();
                }
                _map.Add(original, rewritten);
            }
            return rewritten;
        }

        private bool TryFindMapping(Statement original, out Statement rewritten) {
            if (_map != null) {
                return _map.TryGetValue(original, out rewritten);
            } else {
                rewritten = null;
                return false;
            }
        }

        private void FixBreakAndContinue() {
            Statement mapped;

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
        /// <param name="node">Expression to rewrite</param>
        /// <param name="stack">State of the stack before the expression is emitted.</param>
        /// <returns>Rewritten expression.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private Expression RewriteExpression(Expression node, Stack stack) {
            if (node == null) {
                return null;
            }

            switch (node.NodeType) {
                case AstNodeType.AndAlso:
                case AstNodeType.OrElse:
                    return RewriteLogical((BinaryExpression)node, stack);
                case AstNodeType.Add:
                case AstNodeType.And:
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
                case AstNodeType.RightShift:
                case AstNodeType.Subtract:
                    return Rewrite((BinaryExpression)node, stack);

                case AstNodeType.Call:
                    return Rewrite((MethodCallExpression)node, stack);

                case AstNodeType.Conditional:
                    return Rewrite((ConditionalExpression)node, stack);

                case AstNodeType.Constant:
                    return Rewrite((ConstantExpression)node);

                case AstNodeType.Convert:
                case AstNodeType.Negate:
                case AstNodeType.Not:
                case AstNodeType.OnesComplement:
                    return Rewrite((UnaryExpression)node, stack);

                case AstNodeType.New:
                    return Rewrite((NewExpression)node, stack);

                case AstNodeType.TypeIs:
                    return Rewrite((TypeBinaryExpression)node, stack);

                case AstNodeType.ActionExpression:
                    return Rewrite((ActionExpression)node);

                case AstNodeType.ArrayIndexAssignment:
                    return Rewrite((ArrayIndexAssignment)node, stack);

                case AstNodeType.BoundAssignment:
                    return Rewrite((BoundAssignment)node, stack);

                case AstNodeType.BoundExpression:
                    return Rewrite((BoundExpression)node);

                case AstNodeType.CodeBlockExpression:
                    return Rewrite((CodeBlockExpression)node);

                case AstNodeType.CodeContextExpression:
                    return Rewrite((IntrinsicExpression)node);

                case AstNodeType.CommaExpression:
                    return Rewrite((CommaExpression)node, stack);

                case AstNodeType.DeleteUnboundExpression:
                    return Rewrite((DeleteUnboundExpression)node);

                case AstNodeType.DynamicConversionExpression:
                    return Rewrite((DynamicConversionExpression)node, stack);

                case AstNodeType.EnvironmentExpression:
                case AstNodeType.GeneratorIntrinsic:
                    return Rewrite((IntrinsicExpression)node);

                case AstNodeType.MemberAssignment:
                    return Rewrite((MemberAssignment)node, stack);

                case AstNodeType.MemberExpression:
                    return Rewrite((MemberExpression)node, stack);

                case AstNodeType.NewArrayExpression:
                    // NewArrayExpression doesn't need stack state because
                    // its children always execute on non-empty stack
                    return Rewrite((NewArrayExpression)node);

                case AstNodeType.ParamsExpression:
                    return Rewrite((IntrinsicExpression)node);

                case AstNodeType.UnboundAssignment:
                    return Rewrite((UnboundAssignment)node, stack);

                case AstNodeType.UnboundExpression:
                    return Rewrite((UnboundExpression)node);

                case AstNodeType.VoidExpression:
                    return Rewrite((VoidExpression)node, stack);

                default:
                    throw new InvalidOperationException();
            }
        }

        private Expression RewriteExpressionFreeTemps(Expression expression, Stack stack) {
            int mark = Mark();
            expression = RewriteExpression(expression, stack);
            Free(mark);
            return expression;
        }

        // ActionExpression
        private Expression Rewrite(ActionExpression node) {
            Expression[] clone, comma;

            // Stack is never empty when dynamic site arguments are being
            // executed because the dynamic site "this" is on the stack.
            if (RewriteExpressions(node.Arguments, Stack.NonEmpty, out clone, out comma)) {
                comma[comma.Length - 1] =
                    Ast.Action.ActionExpression(node.Action, clone, node.Type);
                return Ast.Comma(comma);
            } else {
                return node;
            }
        }

        // ArrayIndexAssignment
        private Expression Rewrite(ArrayIndexAssignment node, Stack stack) {
            // Value is evaluated first, on a stack in current state
            Expression value = RewriteExpression(node.Value, stack);

            // Array is evaluated second, but value is saved into a temp
            // so the stack is still in the original state.
            Expression array = RewriteExpression(node.Array, stack);

            // Index is emitted into definitely a non-empty stack
            Expression index = RewriteExpression(node.Index, Stack.NonEmpty);

            // Did any of them change?
            if (((object)value != (object)node.Value) ||
                ((object)array != (object)node.Array) ||
                ((object)index != (object)node.Index)) {

                Expression saveValue, saveArray, saveIndex;

                value = ToTemp(value, out saveValue);
                array = ToTemp(array, out saveArray);
                index = ToTemp(index, out saveIndex);

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
        private Expression RewriteLogical(BinaryExpression node, Stack stack) {
            // Left expression runs on a stack as left by parent
            Expression left = RewriteExpression(node.Left, stack);
            // ... and so does the right one
            Expression right = RewriteExpression(node.Right, stack);

            if (((object)left != (object)node.Left) ||
                ((object)right != (object)node.Right)) {
                return new BinaryExpression(node.NodeType, left, right, node.Type, node.Method);
            } else {
                return node;
            }
        }

        // BinaryExpression
        private Expression Rewrite(BinaryExpression node, Stack stack) {
            // Left expression executes on the stack as left by parent
            Expression left = RewriteExpression(node.Left, stack);
            // Right expression always has non-empty stack (left is on it)
            Expression right = RewriteExpression(node.Right, Stack.NonEmpty);

            if (((object)left != (object)node.Left) ||
                ((object)right != (object)node.Right)) {
                Expression saveLeft, saveRight;

                left = ToTemp(left, out saveLeft);
                right = ToTemp(right, out saveRight);

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
        private Expression Rewrite(BoundAssignment node, Stack stack) {
            // Expression is evaluated on a stack in current state
            Expression value = RewriteExpression(node.Value, stack);
            if ((object)value != (object)node.Value) {
                return Ast.Assign(node.Variable, value);
            } else {
                return node;
            }
        }

        // BoundExpression
        private Expression Rewrite(BoundExpression node) {
            // No action necessary, regardless of the stack state
            return node;
        }

        // CodeBlockExpression
        private Expression Rewrite(CodeBlockExpression node) {
            // No action necessary, regardless of the stack state.
            return node;
        }

        // CommaExpression
        private Expression Rewrite(CommaExpression node, Stack stack) {
            ReadOnlyCollection<Expression> expressions = node.Expressions;
            int index = node.ValueIndex;
            Expression[] clone = null;
            Expression result = null;

            for (int i = 0; i < expressions.Count; i++) {
                Expression expression = expressions[i];

                // The expressions up and till the "ValueIndex" are
                // evaluated on the stack as left by parent. After that
                // the expression that forms the value of the comma
                // stays on the stack so rest is evaluated with non-empty stack.
                Expression rewritten = RewriteExpression(expression, stack);

                if (i == index) {
                    stack = Stack.NonEmpty;
                }

                if ((object)expression != (object)rewritten) {
                    if (clone == null) {
                        int size = expressions.Count;
                        // If the result is not at the end of the comma,
                        // we need an extra element for the result temp.
                        if (index < expressions.Count - 1) {
                            size++;
                        }
                        clone = new Expression[size];
                        for (int j = 0; j < i; j++) {
                            Expression expr = expressions[j];
                            if (j == index) {
                                // This expression is not the last (j < i < expressions.Count)
                                Debug.Assert(j < expressions.Count - 1);
                                result = ToTemp(expr, out expr);
                            }
                            clone[j] = expr;
                        }
                    }
                }

                if (clone != null) {
                    if (i == index && index < expressions.Count - 1) {
                        result = ToTemp(rewritten, out rewritten);
                    }
                    clone[i] = rewritten;
                }
            }

            if (clone != null) {
                if (result != null) {
                    Debug.Assert(index < expressions.Count - 1);
                    Debug.Assert(clone[clone.Length - 1] == null);
                    clone[clone.Length - 1] = result;
                }
                return Ast.Comma(clone);
            } else {
                return node;
            }
        }

        // ConditionalExpression
        private Expression Rewrite(ConditionalExpression node, Stack stack) {
            // Test executes at the stack as left by parent
            Expression test = RewriteExpression(node.Test, stack);
            // The test is popped by conditional jump so branches execute
            // at the stack as left by parent too.
            Expression ifTrue = RewriteExpression(node.IfTrue, stack);
            Expression ifFalse = RewriteExpression(node.IfFalse, stack);

            if (((object)test != (object)node.Test) ||
                ((object)ifTrue != (object)node.IfTrue) ||
                ((object)ifFalse != (object)node.IfFalse)) {
                return Ast.Condition(test, ifTrue, ifFalse);
            } else {
                return node;
            }
        }

        // ConstantExpression
        private Expression Rewrite(ConstantExpression node) {
            // No action necessary, regardless of stack
            return node;
        }

        // DeleteUnboundExpression
        private Expression Rewrite(DeleteUnboundExpression node) {
            // No action necessary, regardless of stack
            return node;
        }

        // DynamicConversionExpression
        private Expression Rewrite(DynamicConversionExpression node, Stack stack) {
            // The expression is evaluated on the current stack
            Expression expression = RewriteExpression(node.Expression, stack);
            if ((object)expression != (object)node.Expression) {
                return Ast.DynamicConvert(expression, node.Type);
            } else {
                return node;
            }
        }

        // IntrinsicExpression
        private Expression Rewrite(IntrinsicExpression node) {
            // No action necessary, regardless of stack
            return node;
        }

        // MemberAssignment
        private Expression Rewrite(MemberAssignment node, Stack stack) {
            Expression expression = null;
            if (node.Expression != null) {
                // If there's an instance, it executes on the stack in current state
                // and rest is executed on non-empty stack.
                // Otherwise the stack is left unchaged.
                expression = RewriteExpression(node.Expression, stack);
                stack = Stack.NonEmpty;
            }
            
            Expression value = RewriteExpression(node.Value, stack);

            if (((object)expression != (object)node.Expression) ||
                ((object)value != (object)node.Value)) {

                if (expression != null) {
                    Expression saveExpression, saveValue;
                    expression = ToTemp(expression, out saveExpression);
                    value = ToTemp(value, out saveValue);

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
        private Expression Rewrite(MemberExpression node, Stack stack) {
            // Expression is emitted on top of the stack in current state
            Expression expression = RewriteExpression(node.Expression, stack);
            if ((object)expression != (object)node.Expression) {
                return new MemberExpression(node.Member, expression, node.Type);
            } else {
                return node;
            }
        }

        // MethodCallExpression
        // TODO: ref parameters!!!
        private Expression Rewrite(MethodCallExpression node, Stack stack) {
            Expression instance = null;
            ReadOnlyCollection<Expression> args = node.Arguments;
            Expression[] clone = null;
            Expression[] comma = null;
            int ci = 0; // comma array fill index

            if (node.Instance != null) {
                // For instance methods, the instance executes on the
                // stack as is, but stays on the stack, making it non-empty.
                instance = RewriteExpression(node.Instance, stack);
                stack = Stack.NonEmpty;
            }

            if (args != null) {
                for (int i = 0; i < args.Count; i++) {
                    Expression arg = args[i];
                    Expression rarg = RewriteExpression(arg, stack);

                    // After the first argument, stack is definitely non-empty
                    stack = Stack.NonEmpty;

                    if ((object)arg != (object)rarg) {
                        if (clone == null) {
                            clone = new Expression[args.Count];
                            if (instance != null) {
                                comma = new Expression[args.Count + 2]; // + instance + the call
                                instance = ToTemp(instance, out comma[ci++]);
                            } else {
                                comma = new Expression[args.Count + 1];
                            }

                            for (int j = 0; j < i; j++) {
                                clone[j] = ToTemp(args[j], out comma[ci++]);
                            }
                        }
                    }

                    if (clone != null) {
                        clone[i] = ToTemp(rarg, out comma[ci++]);
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
        private Expression Rewrite(NewArrayExpression node) {
            Expression[] clone, comma;

            // Array elements are never emitted on an empty stack because
            // the array reference and the index are on the stack.
            if (RewriteExpressions(node.Expressions, Stack.NonEmpty, out clone, out comma)) {
                comma[comma.Length - 1] = Ast.NewArray(node.Type, clone);
                return Ast.Comma(comma);
            } else {
                return node;
            }
        }

        // NewExpression
        private Expression Rewrite(NewExpression node, Stack stack) {
            Expression[] clone, comma;

            // The first expression starts on a stack as provided by
            // parent, rest are definitely non-emtpy (which RewriteExpressions
            // guarantees.
            if (RewriteExpressions(node.Arguments, stack, out clone, out comma)) {
                comma[comma.Length - 1] =
                    Ast.New(node.Constructor, clone);
                return Ast.Comma(comma);
            } else {
                return node;
            }
        }

        // TypeBinaryExpression
        private Expression Rewrite(TypeBinaryExpression node, Stack stack) {
            // The expression is emitted on top of current stack
            Expression expression = RewriteExpression(node.Expression, stack);
            if ((object)expression != (object)node.Expression) {
                return Ast.TypeIs(expression, node.TypeOperand);
            } else {
                return node;
            }
        }

        // UnaryExpression
        private Expression Rewrite(UnaryExpression node, Stack stack) {
            // Operand is emitted on top of the stack as is
            Expression expression = RewriteExpression(node.Operand, stack);
            if ((object)expression != (object)node.Operand) {
                return new UnaryExpression(node.NodeType, expression, node.Type);
            } else {
                return node;
            }
        }

        // UnboundAssignment
        private Expression Rewrite(UnboundAssignment node, Stack stack) {
            // Value is emitted on the stack in current state
            Expression expression = RewriteExpression(node.Value, stack);
            if ((object)expression != (object)node.Value) {
                return Ast.Assign(node.Name, expression);
            } else {
                return node;
            }
        }

        // UnboundExpression
        private Expression Rewrite(UnboundExpression node) {
            // No action necessary
            return node;
        }

        // VoidExpression
        private Expression Rewrite(VoidExpression node, Stack stack) {
            Statement statement = RewriteStatement(node.Statement, stack);
            if ((object)statement != (object)node.Statement) {
                return Ast.Void(statement);
            } else {
                return node;
            }
        }

        #endregion

        #region Statements

        /// <summary>
        /// Rewrites the statement to spill stack if needed.
        /// </summary>
        /// <param name="node">The node to rewrite</param>
        /// <param name="stack">The state of the stack before the statement codegen.</param>
        /// <returns>Rewritten statement</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private Statement RewriteStatement(Statement node, Stack stack) {
            if (node == null) {
                return null;
            }

            Statement result;

            switch (node.NodeType) {
                case AstNodeType.BlockStatement:
                    result = Rewrite((BlockStatement)node, stack);
                    break;

                case AstNodeType.BreakStatement:
                    result = Rewrite((BreakStatement)node);
                    break;

                case AstNodeType.ContinueStatement:
                    result = Rewrite((ContinueStatement)node);
                    break;

                case AstNodeType.DeleteStatement:
                    result = Rewrite((DeleteStatement)node);
                    break;

                case AstNodeType.DoStatement:
                    result = Rewrite((DoStatement)node, stack);
                    break;

                case AstNodeType.EmptyStatement:
                    result = Rewrite((EmptyStatement)node);
                    break;

                case AstNodeType.ExpressionStatement:
                    result = Rewrite((ExpressionStatement)node, stack);
                    break;

                case AstNodeType.IfStatement:
                    result = Rewrite((IfStatement)node, stack);
                    break;

                case AstNodeType.LabeledStatement:
                    result = Rewrite((LabeledStatement)node, stack);
                    break;

                case AstNodeType.LoopStatement:
                    result = Rewrite((LoopStatement)node, stack);
                    break;

                case AstNodeType.ReturnStatement:
                    result = Rewrite((ReturnStatement)node, stack);
                    break;

                case AstNodeType.ScopeStatement:
                    result = Rewrite((ScopeStatement)node, stack);
                    break;

                case AstNodeType.SwitchStatement:
                    result = Rewrite((SwitchStatement)node, stack);
                    break;

                case AstNodeType.ThrowStatement:
                    result = Rewrite((ThrowStatement)node, stack);
                    break;

                case AstNodeType.TryStatement:
                    result = Rewrite((TryStatement)node, stack);
                    break;

                case AstNodeType.YieldStatement:
                    result = Rewrite((YieldStatement)node, stack);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            // Store the mapping to resolve the break and continue later
            return Map(node, result);
        }

        // BlockStatement
        private Statement Rewrite(BlockStatement node, Stack stack) {
            ReadOnlyCollection<Statement> statements = node.Statements;
            Statement[] clone = null;

            for (int i = 0; i < statements.Count; i++) {
                Statement statement = statements[i];
                // All statements within the block execute at the
                // same stack state.
                Statement rewritten = RewriteStatement(statement, stack);

                if (((object)rewritten != (object)statement) && (clone == null)) {
                    clone = Clone(statements, i);
                }

                if (clone != null) {
                    clone[i] = rewritten;
                }
            }

            if (clone != null) {
                return Ast.Block(node.Span, clone);
            } else {
                return node;
            }
        }

        // BreakStatement
        private Statement Rewrite(BreakStatement node) {
            if (node.Statement != null) {
                if (_break == null) {
                    _break = new List<BreakStatement>();
                }
                _break.Add(node);
            }

            // No further action necessary
            return node;
        }

        // ContinueStatement
        private Statement Rewrite(ContinueStatement node) {
            if (node.Statement != null) {
                if (_continue == null) {
                    _continue = new List<ContinueStatement>();
                }
                _continue.Add(node);
            }

            // No further action necessary
            return node;
        }

        // DeleteStatement
        private Statement Rewrite(DeleteStatement node) {
            // No action necessary
            return node;
        }

        // DoStatement
        private Statement Rewrite(DoStatement node, Stack stack) {
            // The "do" statement requires empty stack so it can
            // guarantee it for its child nodes.
            Statement body = RewriteStatement(node.Body, Stack.Empty);
            Expression test = RewriteExpressionFreeTemps(node.Test, Stack.Empty);

            // Loop needs empty stack to execute so if the stack is initially
            // not empty, we rewrite to get empty stack.
            if (((object)body != (object)node.Body) ||
                ((object)test != (object)node.Test) ||
                stack != Stack.Empty) {
                return new DoStatement(node.Span, node.Header, test, body);
            } else {
                return node;
            }
        }

        // EmptyStatement
        private Statement Rewrite(EmptyStatement node) {
            // No action necessary, regardless of stack
            return node;
        }

        // ExpressionStatement
        private Statement Rewrite(ExpressionStatement node, Stack stack) {
            // Expression executes on the stack in the current state
            Expression expression = RewriteExpressionFreeTemps(node.Expression, stack);

            if ((object)expression != (object)node.Expression) {
                return Ast.Statement(node.Span, expression);
            } else {
                return node;
            }
        }

        // IfStatement
        private Statement Rewrite(IfStatement node, Stack stack) {
            ReadOnlyCollection<IfStatementTest> tests = node.Tests;
            IfStatementTest[] clone = null;

            for (int i = 0; i < tests.Count; i++) {
                IfStatementTest test = tests[i];
                IfStatementTest rtest = Rewrite(test, stack);

                if (((object)test != (object)rtest) && (clone == null)) {
                    clone = Clone(tests, i);
                }

                if (clone != null) {
                    clone[i] = rtest;
                }
            }

            Statement @else = RewriteStatement(node.ElseStatement, stack);

            // Did we rewrite anything?
            if (clone != null) {
                return new IfStatement(node.Span, CollectionUtils.ToReadOnlyCollection(clone), @else);
            } else if ((object)@else != (object)node.ElseStatement) {
                return new IfStatement(node.Span, CollectionUtils.ToReadOnlyCollection(tests), @else);
            } else {
                return node;
            }
        }

        // LabeledStatement
        private Statement Rewrite(LabeledStatement node, Stack stack) {
            Statement statement = RewriteStatement(node.Statement, stack);
            if ((object)statement != (object)node.Statement) {
                return Ast.Labeled(node.Span, statement);
            } else {
                return node;
            }
        }

        // LoopStatement
        private Statement Rewrite(LoopStatement node, Stack stack) {
            // The loop statement requires empty stack for itself, so it
            // can guarantee it to the child nodes.
            Expression test = RewriteExpressionFreeTemps(node.Test, Stack.Empty);
            Expression incr = RewriteExpressionFreeTemps(node.Increment, Stack.Empty);
            Statement body = RewriteStatement(node.Body, Stack.Empty);
            Statement @else = RewriteStatement(node.ElseStatement, Stack.Empty);

            // However, the loop itself requires that it executes on an empty stack
            // so we need to rewrite if the stack is not empty.
            if (((object)test != (object)node.Test) ||
                ((object)incr != (object)node.Increment) ||
                ((object)body != (object)node.Body) ||
                ((object)@else != (object)node.ElseStatement) ||
                stack != Stack.Empty) {
                return Ast.Loop(node.Span, node.Header, test, incr, body, @else);
            } else {
                return node;
            }
        }

        // ReturnStatement
        private Statement Rewrite(ReturnStatement node, Stack stack) {
            // Return requires empty stack to execute so the expression is
            // going to execute on an empty stack.
            Expression expression = RewriteExpressionFreeTemps(node.Expression, Stack.Empty);

            // However, the statement itself needs an empty stack for itself
            // so if stack is not empty, rewrite to empty the stack.
            if ((object)expression != (object)node.Expression ||
                stack != Stack.Empty) {
                return Ast.Return(node.Span, expression);
            } else {
                return node;
            }
        }

        // ScopeStatement
        private Statement Rewrite(ScopeStatement node, Stack stack) {
            Expression scope = RewriteExpressionFreeTemps(node.Scope, stack);
            Statement body = RewriteStatement(node.Body, stack);

            if (((object)scope != (object)node.Scope) ||
                ((object)body != (object)node.Body)) {
                return Ast.Scope(node.Span, scope, body);
            } else {
                return node;
            }
        }

        // SwitchStatement
        private Statement Rewrite(SwitchStatement node, Stack stack) {
            // The switch statement test is emitted on the stack in current state
            Expression test = RewriteExpressionFreeTemps(node.TestValue, stack);
            ReadOnlyCollection<SwitchCase> cases = node.Cases;
            SwitchCase[] clone = null;

            for (int i = 0; i < cases.Count; i++) {
                SwitchCase @case = cases[i];
                // And all the cases also run on the same stack level.
                SwitchCase rcase = Rewrite(@case, stack);

                if (((object)rcase != (object)@case) && (clone == null)) {
                    clone = Clone(cases, i);
                }

                if (clone != null) {
                    clone[i] = rcase;
                }
            }

            if (clone != null) {
                return Ast.Switch(node.Span, node.Header, test, clone);
            } else if ((object)test != (object)node.TestValue) {
                return Ast.Switch(node.Span, node.Header, test, ArrayUtils.ToArray(node.Cases));
            } else {
                return node;
            }
        }

        // ThrowStatement
        private Statement Rewrite(ThrowStatement node, Stack stack) {
            Expression value = RewriteExpressionFreeTemps(node.Value, stack);
            if ((object)value != (object)node.Value) {
                return Ast.Throw(node.Span, value);
            } else {
                return node;
            }
        }

        // TryStatement
        private Statement Rewrite(TryStatement node, Stack stack) {
            // Try statement definitely needs an empty stack so its
            // child nodes execute at empty stack.
            Statement body = RewriteStatement(node.Body, Stack.Empty);
            ReadOnlyCollection<CatchBlock> handlers = node.Handlers;
            CatchBlock[] clone = null;

            if (handlers != null) {
                for (int i = 0; i < handlers.Count; i++) {
                    CatchBlock handler = handlers[i];
                    CatchBlock rhandler = Rewrite(handler);

                    if (((object)rhandler != (object)handler) && (clone == null)) {
                        clone = Clone(handlers, i);
                    }

                    if (clone != null) {
                        clone[i] = rhandler;
                    }
                }
            }

            Statement @finally = RewriteStatement(node.FinallyStatement, Stack.Empty);

            // If the stack is initially not empty, rewrite to spill the stack
            if ((clone != null) ||
                ((object)body != (object)node.Body) ||
                ((object)@finally != (object)node.FinallyStatement) ||
                stack != Stack.Empty) {

                if (clone != null) {
                    handlers = CollectionUtils.ToReadOnlyCollection(clone);
                }

                return new TryStatement(node.Span, node.Header, body, handlers, @finally);
            } else {
                return node;
            }
        }

        // YieldStatement
        private Statement Rewrite(YieldStatement node, Stack stack) {
            // Yield expression is always execute on an non-empty stack
            // given the nature of the codegen.
            Expression expression = RewriteExpressionFreeTemps(node.Expression, Stack.NonEmpty);

            if ((object)expression != (object)node.Expression || stack != Stack.Empty) {
                // Yield's argument was changed. We may need to hoist it.
                // This will flatten nested yields, which simplifies yield codegen. So:
                //   yield (yield x)
                // becomes:
                //   $t = yield x
                //   yield $t
                Expression saveArg;
                expression = ToTemp(expression, out saveArg);
                return Ast.Block(Ast.Statement(saveArg), Ast.Yield(node.Span, expression));
            } else {
                return node;
            }
        }

        #endregion

        #region Nodes

        // CatchBlock
        private CatchBlock Rewrite(CatchBlock node) {
            // Catch block starts with an empty stack (guaranteed by TryStatement)
            Statement body = RewriteStatement(node.Body, Stack.Empty);

            if ((object)body != (object)node.Body) {
                return Ast.Catch(node.Span, node.Header, node.Test, node.Variable, body);
            } else {
                return node;
            }
        }

        // IfStatementTest
        private IfStatementTest Rewrite(IfStatementTest node, Stack stack) {
            Expression test = RewriteExpressionFreeTemps(node.Test, stack);
            Statement body = RewriteStatement(node.Body, stack);

            if (((object)test != (object)node.Test) ||
                ((object)body != (object)node.Body)) {
                return new IfStatementTest(node.Span, node.Header, test, body);
            } else {
                return node;
            }
        }

        // SwitchCase
        private SwitchCase Rewrite(SwitchCase node, Stack stack) {
            Statement body = RewriteStatement(node.Body, stack);

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
        private bool RewriteExpressions(ReadOnlyCollection<Expression>/*!*/ expressions, Stack stack, out Expression[] clone, out Expression[] comma) {
            Debug.Assert(expressions != null);

            clone = comma = null;

            for (int i = 0, count = expressions.Count; i < count; i++) {
                Expression arg = expressions[i];

                // Rewrite the expression. The first expression has stack
                // as parent guarantees it, others will set to non-empty.
                Expression exp = RewriteExpression(arg, stack);
                stack = Stack.NonEmpty;

                // The expression has been rewritten, rewrite this too.
                if (((object)arg != (object)exp) && (clone == null)) {
                    clone = new Expression[count];
                    comma = new Expression[count + 1];
                    for (int j = 0; j < i; j++) {
                        clone[j] = ToTemp(expressions[j], out comma[j]);
                    }
                }

                if (clone != null) {
                    clone[i] = ToTemp(exp, out comma[i]);
                }
            }

            return clone != null;
        }

        #endregion
    }
}
