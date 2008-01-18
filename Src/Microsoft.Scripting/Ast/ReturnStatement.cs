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

namespace Microsoft.Scripting.Ast {
    public sealed class ReturnStatement : Expression, ISpan {
        private readonly Expression _expr;
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        internal ReturnStatement(SourceLocation start, SourceLocation end, Expression expression)
            : base(AstNodeType.ReturnStatement, typeof(void)) {
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
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static ReturnStatement Return() {
            return Return(SourceSpan.None, null);
        }

        public static ReturnStatement Return(Expression expression) {
            return Return(SourceSpan.None, expression);
        }

        public static ReturnStatement Return(SourceSpan span, Expression expression) {
            return new ReturnStatement(span.Start, span.End, expression);
        }
    }
}
