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

namespace Microsoft.Scripting.Ast {
    public sealed class BreakStatement : Expression, ISpan {
        private Expression _expression;
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        internal BreakStatement(SourceLocation start, SourceLocation end, Expression expression)
            : base(AstNodeType.BreakStatement, typeof(void)) {
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
        public static BreakStatement Break() {
            return Break(SourceSpan.None, null);
        }
        public static BreakStatement Break(SourceSpan span) {
            return Break(span, null);
        }
        public static BreakStatement Break(Expression expression) {
            return Break(SourceSpan.None, expression);
        }
        public static BreakStatement Break(SourceSpan span, Expression expression) {
            return new BreakStatement(span.Start, span.End, expression);
        }
    }
}