/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Collections.Generic;

using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    /// <summary>
    /// Expression that represents list of expressions.
    /// The value of the CommaExpression is the expression identified by the index
    /// </summary>
    public class CommaExpression : Expression {
        private IList<Expression> _expressions;
        private int _valueIndex;

        public CommaExpression(IList<Expression> expressions, int valueIndex) {
            if (expressions == null) {
                throw new ArgumentNullException("expressions");
            }
            // This check also validates that expression.Count > 0
            if (valueIndex < 0 || valueIndex >= expressions.Count) {
                throw new ArgumentOutOfRangeException("valueIndex");
            }
            for (int i = 0; i < expressions.Count; i++) {
                if (expressions[i] == null) {
                    Debug.Assert(false);
                    throw new ArgumentNullException("expressions[" + i.ToString() + "]");
                }
            }

            _expressions = expressions;
            _valueIndex = valueIndex;
        }

        public IList<Expression> Expressions {
            get { return _expressions; }
        }

        public int ValueIndex {
            get { return _valueIndex; }
        }

        public CommaExpression(IList<Expression> expressions, int valueIndex, SourceSpan span)
            : base(span) {
            if (expressions == null) {
                throw new ArgumentNullException("expressions");
            }
            if (valueIndex < 0 || valueIndex >= expressions.Count) {
                throw new ArgumentOutOfRangeException("valueIndex");
            }

            // At this point we know that there is at least 1 expression
            Debug.Assert(expressions.Count > 0);

            _expressions = expressions;
            _valueIndex = valueIndex;
        }

        public override void Emit(CodeGen cg) {
            EmitAs(cg, typeof(object));
        }

        public override void EmitAs(CodeGen cg, Type asType) {
            for (int index = 0; index < _expressions.Count; index++) {
                Expression current = _expressions[index];
                // Emit the expression

                if (index == _valueIndex) {
                    if (current != null) {
                        // This is the expression that is our value
                        current.EmitAs(cg, asType);
                    } else if (asType != typeof(void)) {
                        cg.EmitMissingValue(asType);
                    }
                } else if (current != null) {
                    // We do not need the value of this expression
                    // Hopefully constant expressions can optimize themselves away
                    current.EmitAs(cg, typeof(void));
                }
            }
        }

        public override object Evaluate(CodeContext context) {
            object result = null;
            for (int index = 0; index < _expressions.Count; index++) {
                Expression current = _expressions[index];

                // Evaluate the expression
                if (current != null) {
                    object val = current.Evaluate(context);

                    // And remember the value of the expression should be returned.
                    if (index == _valueIndex) {
                        result = val;
                    }
                }
            }
            return result;
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                foreach (Expression e in _expressions) {
                    e.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        #region Factories

        public static CommaExpression Comma(int valueIndex, params Expression[] expressions) {
            return new CommaExpression(expressions, valueIndex);
        }

        public static CommaExpression Comma(int valueIndex, IList<Expression> expressions) {
            return new CommaExpression(expressions, valueIndex);
        }

        #endregion
    }
}
