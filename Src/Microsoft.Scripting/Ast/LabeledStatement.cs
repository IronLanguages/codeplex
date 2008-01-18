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
    public sealed class LabeledStatement : Expression, ISpan {
        private Expression _expression;

        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        internal LabeledStatement(SourceLocation start, SourceLocation end, Expression expression)
            : base(AstNodeType.LabeledStatement, typeof(void)) {
            _start = start;
            _end = end;
            _expression = expression;
        }

        public Expression Statement {
            get { return _expression; }
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }

        public LabeledStatement Mark(Expression expression) {
            Contract.RequiresNotNull(expression, "expression");
            _expression = expression;
            return this;
        }
    }

    public static partial class Ast {
        public static LabeledStatement Labeled(Expression expression) {
            return Labeled(SourceSpan.None, expression);
        }

        public static LabeledStatement Labeled(SourceSpan span, Expression expression) {
            return new LabeledStatement(span.Start, span.End, expression);
        }
    }
}
