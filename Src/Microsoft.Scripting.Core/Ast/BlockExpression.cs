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
    /// <summary>
    /// Defines a block where variables are defined. The compiler will
    /// automatically close over these variables if they're referenced in a
    /// nested LambdaExpession
    /// </summary>
    public class BlockExpression : Expression {
        private readonly ReadOnlyCollection<Expression> _expressions;
        private readonly ReadOnlyCollection<ParameterExpression> _variables;

        public ReadOnlyCollection<Expression> Expressions {
            get { return _expressions; }
        }

        /// <summary>
        /// The variables in this block.
        /// </summary>
        public ReadOnlyCollection<ParameterExpression> Variables {
            get {
                return _variables;
            }
        }

        internal BlockExpression(Annotations annotations, ReadOnlyCollection<ParameterExpression> variables, ReadOnlyCollection<Expression> expressions)
            : base(annotations) {
            _expressions = expressions;
            _variables = variables;
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitBlock(this);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Block;
        }

        protected override Type GetExpressionType() {
            return typeof(void);
        }
    }

    internal sealed class CommaExpression : BlockExpression {
        internal CommaExpression(Annotations annotations, ReadOnlyCollection<ParameterExpression> variables, ReadOnlyCollection<Expression> expressions)
            : base(annotations, variables, expressions) {
            
        }

        protected override Type GetExpressionType() {
            return Expressions[Expressions.Count - 1].Type;
        }
    }

    public partial class Expression {

        /// <summary>
        /// Creates a list of expressions whose value is void.
        /// </summary>

        public static BlockExpression Block(IEnumerable<Expression> expressions) {
            return Block(Annotations.Empty, expressions);
        }

        public static BlockExpression Block(Expression arg0, Expression arg1) {
            return MakeBlock(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1 }));
        }

        public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2) {
            return MakeBlock(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2 }));
        }

        public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            return MakeBlock(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2, arg3 }));
        }

        public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) {
            return MakeBlock(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2, arg3, arg4 }));
        }

        public static BlockExpression Block(params Expression[] expressions) {
            return Block(Annotations.Empty, expressions);
        }

        public static BlockExpression Block(Annotations annotations, params Expression[] expressions) {
            return Block(annotations, (IEnumerable<Expression>)expressions);
        }

        public static BlockExpression Block(Annotations annotations, IEnumerable<Expression> expressions) {
            return Block(annotations, EmptyReadOnlyCollection<ParameterExpression>.Instance, expressions);
        }

        public static BlockExpression Block(IEnumerable<ParameterExpression> variables, params Expression[] expressions) {
            return Block(Annotations.Empty, variables, expressions);
        }

        public static BlockExpression Block(IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) {
            return Block(Annotations.Empty, variables, expressions);
        }

        public static BlockExpression Block(Annotations annotations, IEnumerable<ParameterExpression> variables, params Expression[] expressions) {
            return Block(annotations, variables, (IEnumerable<Expression>)expressions);
        }

        public static BlockExpression Block(Annotations annotations, IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) {
            RequiresCanRead(expressions, "expressions");
            var varList = variables.ToReadOnly();
            ContractUtils.RequiresNotNullItems(varList, "variables");
            Expression.RequireVariablesNotByRef(varList, "variables");

            // TODO: we shouldn't allow creating an empty block
            // When fixed, remove the check in LambdaCompiler.AddReturnLabel
            return new BlockExpression(annotations, variables.ToReadOnly(), expressions.ToReadOnly());
        }

        private static BlockExpression MakeBlock(Annotations annotations, ReadOnlyCollection<Expression> expressions) {
            return new BlockExpression(annotations, EmptyReadOnlyCollection<ParameterExpression>.Instance, expressions);
        }

        /// <summary>
        /// Creates a list of expressions whose value is the value of the last expression.
        /// </summary>
        public static BlockExpression Comma(IEnumerable<Expression> expressions) {
            return Comma(Annotations.Empty, expressions);
        }

        public static BlockExpression Comma(params Expression[] expressions) {
            return Comma(Annotations.Empty, (IEnumerable<Expression>)expressions);
        }

        public static BlockExpression Comma(Expression arg0, Expression arg1) {
            return Comma(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1 }));
        }

        public static BlockExpression Comma(Expression arg0, Expression arg1, Expression arg2) {
            return Comma(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2 }));
        }

        public static BlockExpression Comma(Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            return Comma(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2, arg3 }));
        }

        public static BlockExpression Comma(Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) {
            return Comma(Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2, arg3, arg4 }));
        }

        public static BlockExpression Comma(Annotations annotations, params Expression[] expressions) {
            return Comma(annotations, (IEnumerable<Expression>)expressions);
        }

        public static BlockExpression Comma(Annotations annotations, IEnumerable<Expression> expressions) {
            return Comma(annotations, EmptyReadOnlyCollection<ParameterExpression>.Instance, expressions);
        }

        public static BlockExpression Comma(IEnumerable<ParameterExpression> variables, params Expression[] expressions) {
            return Comma(Annotations.Empty, variables, (IEnumerable<Expression>)expressions);
        }

        public static BlockExpression Comma(IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) {
            return Comma(Annotations.Empty, variables, expressions);
        }

        public static BlockExpression Comma(Annotations annotations, IEnumerable<ParameterExpression> variables, params Expression[] expressions) {
            return Comma(annotations, variables, (IEnumerable<Expression>)expressions);
        }

        public static BlockExpression Comma(Annotations annotations, IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) {
            RequiresCanRead(expressions, "expressions");
            var expressionList = expressions.ToReadOnly();
            ContractUtils.RequiresNotEmpty(expressionList, "expressions");
            var varList = variables.ToReadOnly();
            ContractUtils.RequiresNotNullItems(varList, "variables");
            Expression.RequireVariablesNotByRef(varList, "variables");

            //TODO: there could be some optimizations for blocks containing single nodes.
            return new CommaExpression(annotations, varList, expressionList);
        }


        [Obsolete("Do not use. Use Block instead.")]
        public static BlockExpression Scope(Expression body, params ParameterExpression[] variables) {
            return Scope(body, Annotations.Empty, (IEnumerable<ParameterExpression>)variables);
        }

        [Obsolete("Do not use. Use Block instead.")]
        public static BlockExpression Scope(Expression body, IEnumerable<ParameterExpression> variables) {
            return Scope(body, Annotations.Empty, variables);
        }

        [Obsolete("Do not use. Use Block instead.")]
        public static BlockExpression Scope(Expression body, Annotations annotations, params ParameterExpression[] variables) {
            return Scope(body, annotations, (IEnumerable<ParameterExpression>)variables);
        }

        [Obsolete("Do not use. Use Block instead.")]
        public static BlockExpression Scope(Expression body, Annotations annotations, IEnumerable<ParameterExpression> variables) {
            RequiresCanRead(body, "body");

            var varList = variables.ToReadOnly();
            ContractUtils.RequiresNotNullItems(varList, "variables");
            Expression.RequireVariablesNotByRef(varList, "variables");

            var exprList = new Expression[] { body }.ToReadOnly();

            return new CommaExpression(annotations, varList, exprList);
        }

    }
}
