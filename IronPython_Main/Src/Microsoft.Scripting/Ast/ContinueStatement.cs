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
    public sealed class ContinueStatement : Expression, ISpan {
        private Expression _expression;
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        internal ContinueStatement(SourceLocation start, SourceLocation end, Expression expression)
            : base(AstNodeType.ContinueStatement, typeof(void)) {
            _start = start;
            _end = end;
            _expression = expression;
        }

        public Expression Statement {
            get { return _expression; }
            set { _expression = value; }
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }
    }

    public static partial class Ast {
        public static ContinueStatement Continue() {
            return Continue(SourceSpan.None, null);
        }

        public static ContinueStatement Continue(SourceSpan span) {
            return Continue(span, null);
        }

        /// <param name="expression">The statement the label is pointing to (not the label itself).</param>
        public static ContinueStatement Continue(Expression expression) {
            return Continue(SourceSpan.None, expression);
        }

        public static ContinueStatement Continue(SourceSpan span, Expression expression) {
            return new ContinueStatement(span.Start, span.End, expression);
        }
    }
}
