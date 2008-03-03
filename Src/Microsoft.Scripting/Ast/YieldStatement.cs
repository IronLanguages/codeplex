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

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class YieldStatement : Expression, ISpan {
        private readonly Expression /*!*/ _expr;
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        internal YieldStatement(SourceLocation start, SourceLocation end, Expression /*!*/ expression)
            : base(AstNodeType.YieldStatement, typeof(void)) {
            _start = start;
            _end = end;
            _expr = expression;
        }

        public Expression Expression {
            get { return _expr; }
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }
    }

    /// <summary>
    /// Factory methods
    /// </summary>
    public static partial class Ast {
        public static YieldStatement Yield(Expression expression) {
            return Yield(SourceSpan.None, expression);
        }

        public static YieldStatement Yield(SourceSpan span, Expression expression) {
            Contract.Requires(expression != null, "expression");
            return new YieldStatement(span.Start, span.End, expression);
        }
    }
}
