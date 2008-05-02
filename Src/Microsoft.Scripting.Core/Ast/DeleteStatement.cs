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
using System.Diagnostics;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// AST node representing deletion of the variable value.
    /// </summary>
    public sealed class DeleteStatement : Expression {
        private readonly Expression /*!*/ _expression;

        // TODO: remove type !!!
        internal DeleteStatement(Annotations annotations, Expression /*!*/ expression, Type result, DeleteMemberAction bindingInfo)
            : base(annotations, AstNodeType.DeleteStatement, result, bindingInfo) {
            if (IsBound) {
                RequiresBound(expression, "expression");
            }
            _expression = expression;
        }

        public Expression Variable {
            get { return _expression; }
        }
    }

    public partial class Expression {
        public static DeleteStatement Delete(SourceSpan span, Expression variable) {
            ContractUtils.RequiresNotNull(variable, "variable");
            ContractUtils.Requires(variable is VariableExpression || variable is ParameterExpression, "variable", "variable must be VariableExpression or ParameterExpression");
            return new DeleteStatement(Annotate(span), variable, typeof(void), null);
        }

        public static DeleteStatement DeleteMember(Expression expression, DeleteMemberAction bindingInfo) {
            return DeleteMember(Annotations.Empty, expression, bindingInfo);
        }

        public static DeleteStatement DeleteMember(Annotations annotations, Expression expression, DeleteMemberAction bindingInfo) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            return new DeleteStatement(annotations, expression, typeof(object), bindingInfo); // TODO: typeof(void)  !!!
        }
    }
}
