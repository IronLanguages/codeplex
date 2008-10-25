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
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// AST node representing deletion of the variable value.
    /// TODO: Python specific?
    /// </summary>
    public sealed class DeleteStatement : Expression {
        private readonly Expression _variable;

        internal DeleteStatement(Annotations annotations, Expression variable)
            : base(annotations) {
            _variable = variable;
        }

        public override bool CanReduce {
            get { return true; }
        }

        protected override Type GetExpressionType() {
            return typeof(void);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Extension;
        }

        public Expression Expression {
            get { return _variable; }
        }

        public override Expression Reduce() {
            return Expression.Void(
                Utils.Assign(
                    _variable,
                    Expression.Field(null, typeof(Uninitialized).GetField("Instance")),
                    Annotations
                )
            );
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor) {
            Expression v = visitor.Visit(_variable);
            if (v == _variable) {
                return this;
            }
            return Utils.Delete(v, Annotations);
        }
    }

    public static partial class Utils {
        public static DeleteStatement Delete(Expression variable) {
            return Delete(variable, Annotations.Empty);
        }

        [Obsolete("use Delete overload without SourceSpan")]
        public static DeleteStatement Delete(Expression variable, SourceSpan span) {
            return Delete(variable, Expression.Annotate(span));
        }

        public static DeleteStatement Delete(Expression variable, Annotations annotations) {
            ContractUtils.RequiresNotNull(variable, "variable");
            ContractUtils.Requires(
                variable is ParameterExpression || variable is GlobalVariableExpression,
                "variable",
                "variable must be ParameterExpression or GlobalVariableExpression");
            return new DeleteStatement(annotations, variable);
        }
    }
}
