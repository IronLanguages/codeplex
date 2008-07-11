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
using System.Diagnostics;
using System.Scripting.Utils;

namespace System.Linq.Expressions {

    internal partial class StackSpiller {

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
            /// List of all temps created by stackspiller for this rule/lambda
            /// </summary>
            private List<VariableExpression> _temps = new List<VariableExpression>();

            internal List<VariableExpression> Temps {
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
                temp = Expression.Variable(type, "$temp$" + _temp++);
                _temps.Add(temp);
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
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            internal void VerifyTemps() {
                Debug.Assert(_usedTemps == null || _usedTemps.Count == 0);
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

            internal RewriteAction Action {
                get { return _action; }
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
                    ContractUtils.RequiresArrayRange(_expressions, first, count, "first", "last");

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
    }
}
