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
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
        /// <summary>
        /// Creates a list of expressions whose value is the value of the last expression.
        /// </summary>
        [Obsolete("use Expression.Block instead (make sure the last argument type is Void)")]
        public static BlockExpression Block(SourceSpan span, IEnumerable<Expression> expressions) {
            return Expression.BlockVoid(Expression.Annotate(span), expressions);
        }

        [Obsolete("use Expression.Block instead (make sure the last argument type is Void)")]
        public static BlockExpression Block(SourceSpan span, params Expression[] expressions) {
            return Expression.BlockVoid(Expression.Annotate(span), (IList<Expression>)expressions);
        }

        [Obsolete("use Expression.Block instead (make sure the last argument type is Void)")]
        public static BlockExpression Block(SourceSpan span, Expression arg0) {
            return Expression.BlockVoid(Expression.Annotate(span), new ReadOnlyCollection<Expression>(new[] { arg0 }));
        }

        [Obsolete("use Expression.Block instead (make sure the last argument type is Void)")]
        public static BlockExpression Block(SourceSpan span, Expression arg0, Expression arg1) {
            return Expression.BlockVoid(Expression.Annotate(span), new ReadOnlyCollection<Expression>(new[] { arg0, arg1 }));
        }

        [Obsolete("use Expression.Block instead (make sure the last argument type is Void)")]
        public static BlockExpression Block(SourceSpan span, Expression arg0, Expression arg1, Expression arg2) {
            return Expression.BlockVoid(Expression.Annotate(span), new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2 }));
        }

        /// <summary>
        /// Creates a list of expressions whose value is the value of the last expression.
        /// </summary>
        [Obsolete("use Expression.Block instead")]
        public static BlockExpression Comma(SourceSpan span, IEnumerable<Expression> expressions) {
            return Expression.Block(Expression.Annotate(span), expressions);
        }

        [Obsolete("use Expression.Block instead")]
        public static BlockExpression Comma(SourceSpan span, params Expression[] expressions) {
            return Expression.Block(Expression.Annotate(span), (IList<Expression>)expressions);
        }

        [Obsolete("use Expression.Block instead")]
        public static BlockExpression Comma(SourceSpan span, Expression arg0) {
            return Expression.Block(Expression.Annotate(span), new ReadOnlyCollection<Expression>(new[] { arg0 }));
        }

        // Helper to add a variable to a block
        internal static Expression AddScopedVariable(Expression body, ParameterExpression variable, Expression variableInit) {
            List<ParameterExpression> vars = new List<ParameterExpression>();
            Annotations annotations = Annotations.Empty;
            List<Expression> newBody = new List<Expression>();

            var exprs = new ReadOnlyCollection<Expression>(new [] { body });
            var parent = body;
            //Merge blocks if the current block has only one child that is another block, 
            //the blocks to merge must have the same type.
            while (exprs.Count == 1 && exprs[0].NodeType == ExpressionType.Block && parent.Type == exprs[0].Type) {
                BlockExpression scope = (BlockExpression)(exprs[0]);
                vars.AddRange(scope.Variables);
                annotations = scope.Annotations;
                parent = scope;
                exprs = scope.Expressions;
            }

            newBody.Add(Expression.Assign(variable, variableInit));
            newBody.AddRange(exprs);
            vars.Add(variable);
            return Expression.Block(
                annotations,
                vars,
                newBody.ToArray()
            );
        }

        internal static BlockExpression BlockVoid(Expression[] expressions) {
            if (expressions.Length == 0 || expressions[expressions.Length - 1].Type != typeof(void)) {
                expressions = expressions.AddLast(Expression.Empty());
            }
            return Expression.Block(expressions);
        }
    }
}
