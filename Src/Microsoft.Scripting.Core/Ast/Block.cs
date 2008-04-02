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
    public sealed class Block : Expression {
        private readonly ReadOnlyCollection<Expression> /*!*/ _expressions;

        public ReadOnlyCollection<Expression> /*!*/ Expressions {
            get { return _expressions; }
        }

        internal Block(Annotations annotations, ReadOnlyCollection<Expression> /*!*/ expressions, Type /*!*/ type)
            : base(annotations, AstNodeType.Block, type) {
            _expressions = expressions;
        }
    }

    public static partial class Ast {
        public static Expression Block(List<Expression> expressions) {
            Contract.RequiresNotNullItems(expressions, "expressions");

            if (expressions.Count == 1) {
                return expressions[0];
            } else {
                return Block(expressions.ToArray());
            }
        }

        public static Block Block(params Expression[] expressions) {
            return Block(SourceSpan.None, expressions);
        }

        public static Block Block(SourceSpan span, params Expression[] expressions) {
            Contract.RequiresNotNullItems(expressions, "expressions");
            return new Block(Annotations(span), CollectionUtils.ToReadOnlyCollection(expressions), typeof(void));
        }

        /// <summary>
        /// Creates a list of expressions whose value is the value of the last expression.
        /// </summary>
        public static Block Comma(params Expression[] expressions) {
            return Comma((IList<Expression>)expressions);
        }

        /// <summary>
        /// Creates a list of expressions whose value is the value of the last expression.
        /// </summary>
        public static Block Comma(IList<Expression> expressions) {
            Contract.RequiresNotEmpty(expressions, "expressions");
            Contract.RequiresNotNullItems(expressions, "expressions");

            return new Block(Annotations(SourceSpan.None), CollectionUtils.ToReadOnlyCollection(expressions), expressions[expressions.Count - 1].Type);
        }
    }
}