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

using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// AST node representing deletion of the variable value.
    /// </summary>
    public sealed class DeleteStatement : Expression {
        private readonly Expression /*!*/ _var;

        internal DeleteStatement(Annotations annotations, Expression /*!*/ var)
            : base(annotations, AstNodeType.DeleteStatement, typeof(void)) {
            _var = var;
        }

        public Expression Variable {
            get { return _var; }
        }
    }

    public static partial class Ast {
        public static DeleteStatement Delete(SourceSpan span, Expression variable) {
            Contract.RequiresNotNull(variable, "variable");
            Contract.Requires(variable is VariableExpression || variable is ParameterExpression, "variable", "variable must be VariableExpression or ParameterExpression");
            return new DeleteStatement(Annotations(span), variable);
        }
    }
}
