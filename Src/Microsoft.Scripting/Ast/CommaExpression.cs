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
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Expression that represents list of expressions.
    /// The value of the CommaExpression is the expression identified by the index
    /// </summary>
    public sealed class CommaExpression : Expression {
        private readonly ReadOnlyCollection<Expression> /*!*/ _expressions;
        private int _valueIndex;

        internal CommaExpression(int valueIndex, ReadOnlyCollection<Expression> /*!*/ expressions)
            : base(AstNodeType.CommaExpression, expressions[valueIndex].Type) {
            _expressions = expressions;
            _valueIndex = valueIndex;
        }

        public ReadOnlyCollection<Expression> Expressions {
            get { return _expressions; }
        }

        public int ValueIndex {
            get { return _valueIndex; }
        }
    }

    public static partial class Ast {
        /// <summary>
        /// Creates a list of expressions whose value is the value of the last expression.
        /// </summary>
        public static CommaExpression Comma(params Expression[] expressions) {
            return Comma(-1, expressions);
        }

        /// <summary>
        /// Creates a list of expressions whose value is the value of the last expression.
        /// </summary>
        public static CommaExpression Comma(IList<Expression> expressions) {
            return Comma(-1, expressions);
        }

        /// <summary>
        /// Creates a list of expressions whose value is the value of the <paramref name="valueIndex"/>-th expression.
        /// A negative <paramref name="valueIndex"/> is equivalent to <paramref name="expressions"/>.Length + <paramref name="valueIndex"/> 
        /// (hence -1 designates the last expression specified).
        /// </summary>
        public static CommaExpression Comma(int valueIndex, params Expression[] expressions) {
            return Comma(valueIndex, (IList<Expression>)expressions);
        }

        /// <summary>
        /// Creates a list of expressions whose value is the value of the <paramref name="valueIndex"/>-th expression.
        /// A negative <paramref name="valueIndex"/> is equivalent to <paramref name="expressions"/>.Count + <paramref name="valueIndex"/> 
        /// (hence -1 designates the last expression specified).
        /// </summary>
        public static CommaExpression Comma(int valueIndex, IList<Expression> expressions) {
            Contract.RequiresNotEmpty(expressions, "expressions");
            Contract.RequiresNotNullItems(expressions, "expressions");

            if (valueIndex < 0) {
                valueIndex += expressions.Count;
            }

            Contract.RequiresArrayIndex(expressions, valueIndex, "valueIndex");

            return new CommaExpression(valueIndex, CollectionUtils.ToReadOnlyCollection(expressions));
        }
    }
}

