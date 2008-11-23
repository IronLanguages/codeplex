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

using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    // TODO: rename to BlockExpression !!!
    public sealed class Block : Expression {
        private readonly ReadOnlyCollection<Expression> _expressions;

        public ReadOnlyCollection<Expression> Expressions {
            get { return _expressions; }
        }

        internal Block(Annotations annotations, ReadOnlyCollection<Expression> expressions, Type type)
            : base(ExpressionType.Block, type, annotations) {
            _expressions = expressions;
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitBlock(this);
        }
    }

    public partial class Expression {

        /// <summary>
        /// Creates a list of expressions whose value is void.
        /// </summary>
        public static Block Block(IEnumerable<Expression> expressions) {
            return Block(Annotations.Empty, expressions);
        }

        public static Block Block(Expression arg0, Expression arg1) {
            return MakeBlock(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1 }));
        }

        public static Block Block(Expression arg0, Expression arg1, Expression arg2) {
            return MakeBlock(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2 }));
        }

        public static Block Block(Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            return MakeBlock(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2, arg3 }));
        }

        public static Block Block(Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) {
            return MakeBlock(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2, arg3, arg4 }));
        }

        public static Block Block(params Expression[] expressions) {
            return Block(Annotations.Empty, expressions);
        }

        public static Block Block(Annotations annotations, params Expression[] expressions) {
            return Block(annotations, (IEnumerable<Expression>)expressions);
        }

        public static Block Block(Annotations annotations, IEnumerable<Expression> expressions) {
            RequiresCanRead(expressions, "expressions");

            // TODO: we shouldn't allow creating an empty block
            // When fixed, remove the check in LambdaCompiler.AddReturnLabel
            return new Block(annotations, expressions.ToReadOnly(), typeof(void));
        }

        private static Block MakeBlock(Annotations annotations, ReadOnlyCollection<Expression> expressions) {
            return new Block(annotations, expressions, typeof(void));
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

        public static Block Comma(Expression arg0, Expression arg1) {
            return Comma(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1 }));
        }

        public static Block Comma(Expression arg0, Expression arg1, Expression arg2) {
            return Comma(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2 }));
        }

        public static Block Comma(Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            return Comma(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2, arg3 }));
        }

        public static Block Comma(Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) {
            return Comma(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2, arg3, arg4 }));
        }

        public static Block Comma(Annotations annotations, params Expression[] expressions) {
            return Comma(annotations, (IEnumerable<Expression>)expressions);
        }

        public static Block Comma(Annotations annotations, IEnumerable<Expression> expressions) {
            RequiresCanRead(expressions, "expressions");
            var expressionList = expressions.ToReadOnly();
            ContractUtils.RequiresNotEmpty(expressionList, "expressions");

            //TODO: there could be some optimizations for blocks containing single nodes.
            return new Block(annotations, expressionList, expressionList[expressionList.Count - 1].Type);
        }
    }
}
