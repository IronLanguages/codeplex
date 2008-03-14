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
    public sealed class DeleteStatement : Expression, ISpan {
        private readonly VariableExpression /*!*/ _var;
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        internal DeleteStatement(SourceLocation start, SourceLocation end, VariableExpression /*!*/ var)
            : base(AstNodeType.DeleteStatement, typeof(void)) {
            _start = start;
            _end = end;
            _var = var;
        }

        public VariableExpression Variable {
            get { return _var; }
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }
    }

    public static partial class Ast {
        public static DeleteStatement Delete(SourceSpan span, VariableExpression variable) {
            Contract.RequiresNotNull(variable, "variable");
            return new DeleteStatement(span.Start, span.End, variable);
        }
    }
}
