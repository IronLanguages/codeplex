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
            : base(typeof(void), true, annotations) {
            _variable = variable;
        }

        public Expression Expression {
            get { return _variable; }
        }

        public override Expression Reduce() {
            return Expression.Void(
                Expression.Assign(
                    _variable,
                    Expression.Field(null, typeof(Uninitialized).GetField("Instance")),
                    Annotations
                )
            );
        }
    }

    public static partial class Utils {
        public static DeleteStatement Delete(Expression variable) {
            return Delete(variable, Annotations.Empty);
        }

        public static DeleteStatement Delete(Expression variable, SourceSpan span) {
            return Delete(variable, Expression.Annotate(span));
        }

        public static DeleteStatement Delete(Expression variable, Annotations annotations) {
            ContractUtils.RequiresNotNull(variable, "variable");
            ContractUtils.Requires(
                variable is VariableExpression || variable is ParameterExpression || variable is GlobalVariableExpression,
                "variable",
                "variable must be VariableExpression, ParameterExpression, or GlobalVariableExpression");
            return new DeleteStatement(annotations, variable);
        }
    }
}
