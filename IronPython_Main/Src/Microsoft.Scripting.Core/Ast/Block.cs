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

using System.Scripting.Utils;

namespace System.Linq.Expressions {
    public sealed class Block : Expression {
        private readonly ReadOnlyCollection<Expression> /*!*/ _expressions;

        public ReadOnlyCollection<Expression> /*!*/ Expressions {
            get { return _expressions; }
        }

        internal Block(Annotations annotations, ReadOnlyCollection<Expression> /*!*/ expressions, Type /*!*/ type)
            : base(annotations, ExpressionType.Block, type) {
            _expressions = expressions;
        }
    }

    public partial class Expression {

        /// <summary>
        /// Creates a list of expressions whose value is void.
        /// </summary>
        public static Block Block(IEnumerable<Expression> expressions) {
            return Block(Annotations.Empty, expressions);
        }

        public static Block Block(params Expression[] expressions) {
            return Block(Annotations.Empty, expressions);
        }

        public static Block Block(Annotations annotations, params Expression[] expressions) {
            return Block(annotations, (IEnumerable<Expression>)expressions);
        }

        public static Block Block(Annotations annotations, IEnumerable<Expression> expressions) {
            ReadOnlyCollection<Expression> expressionList = expressions.ToReadOnly();
            ContractUtils.RequiresNotNullItems(expressionList, "expressions");

            return new Block(annotations, expressionList, typeof(void));
        }

        /// <summary>
        /// Creates a list of expressions whose value is the value of the last expression.
        /// </summary>
        public static Block Comma(IEnumerable<Expression> expressions) {
            return Comma(Annotations.Empty, expressions);
        }

        public static Block Comma(params Expression[] expressions) {
            return Comma(Annotations.Empty, (IEnumerable<Expression>)expressions);
        }

        public static Block Comma(Annotations annotations, params Expression[] expressions) {
            return Comma(annotations, (IEnumerable<Expression>)expressions);
        }

        public static Block Comma(Annotations annotations, IEnumerable<Expression> expressions) {
            ReadOnlyCollection<Expression> expressionList = expressions.ToReadOnly();

            ContractUtils.RequiresNotEmpty(expressionList, "expressions");
            ContractUtils.RequiresNotNullItems(expressionList, "expressions");

            //TODO: there could be some optimizations for blocks containing single nodes.
            return new Block(annotations, expressionList, expressionList[expressionList.Count - 1].Type);
        }
    }
}