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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public class ExpressionCollectionBuilder<TExpression> : IEnumerable<TExpression> {
        public TExpression Expression0 { get; private set; }
        public TExpression Expression1 { get; private set; }
        public TExpression Expression2 { get; private set; }
        public TExpression Expression3 { get; private set; }

        private int _count;
        private ReadOnlyCollectionBuilder<TExpression> _expressions;

        public ExpressionCollectionBuilder() {
        }

        public int Count {
            get { return _count; }
        }

        /// <summary>
        /// If the number of items added to the builder is greater than 4 returns a read-only collection builder containing all the items.
        /// Returns <c>null</c> otherwise.
        /// </summary>
        public ReadOnlyCollectionBuilder<TExpression> Expressions {
            get { return _expressions; }
        }

        public void Add(IEnumerable<TExpression> expressions) {
            if (expressions != null) {
                foreach (var expression in expressions) {
                    Add(expression);
                }
            }
        }

        public void Add(TExpression expression) {
            if (expression == null) {
                return;
            }

            switch (_count) {
                case 0: Expression0 = expression; break;
                case 1: Expression1 = expression; break;
                case 2: Expression2 = expression; break;
                case 3: Expression3 = expression; break;
                case 4:
                    _expressions = new ReadOnlyCollectionBuilder<TExpression> {
                        Expression0,
                        Expression1,
                        Expression2,
                        Expression3,
                        expression
                    };
                    break;

                default:
                    Debug.Assert(_expressions != null);
                    _expressions.Add(expression);
                    break;
            }

            _count++;
        }

        private IEnumerator<TExpression>/*!*/ GetItemEnumerator() {
            if (_count > 0) {
                yield return Expression0;
            }
            if (_count > 1) {
                yield return Expression1;
            }
            if (_count > 2) {
                yield return Expression2;
            }
            if (_count > 3) {
                yield return Expression3;
            }
        }

        public IEnumerator<TExpression>/*!*/ GetEnumerator() {
            if (_expressions != null) {
                return _expressions.GetEnumerator();
            } else {
                return GetItemEnumerator();
            }
        }

        IEnumerator/*!*/ IEnumerable.GetEnumerator() {
            return CollectionUtils.ToCovariant<TExpression, object>((IEnumerable<TExpression>)this).GetEnumerator();
        }
    }
}
