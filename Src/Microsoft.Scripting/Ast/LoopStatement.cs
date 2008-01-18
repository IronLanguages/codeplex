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
    public sealed class LoopStatement : Expression, ISpan {
        private readonly SourceLocation _header;
        private readonly Expression _test;
        private readonly Expression _increment;
        private readonly Expression /*!*/ _body;
        private readonly Expression _else;
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        /// <summary>
        /// Null test means infinite loop.
        /// </summary>
        internal LoopStatement(SourceLocation start, SourceLocation end, SourceLocation header, Expression test, Expression increment, Expression /*!*/ body, Expression @else)
            : base(AstNodeType.LoopStatement, typeof(void)) {
            _start = start;
            _end = end;
            _test = test;
            _increment = increment;
            _body = body;
            _else = @else;
            _header = header;
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Increment {
            get { return _increment; }
        }

        public Expression Body {
            get { return _body; }
        }

        public Expression ElseStatement {
            get { return _else; }
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
        public static LoopStatement While(Expression test, Expression body, Expression @else) {
            return Loop(SourceSpan.None, SourceLocation.None, test, null, body, @else);
        }

        public static LoopStatement While(SourceSpan span, SourceLocation header, Expression test, Expression body, Expression @else) {
            return Loop(span, header, test, null, body, @else);
        }

        public static LoopStatement Infinite(Expression body) {
            return Loop(SourceSpan.None, SourceLocation.None, null, null, body, null);
        }

        public static LoopStatement Infinite(params Expression[] body) {
            return Loop(SourceSpan.None, SourceLocation.None, null, null, Block(body), null);
        }
        
        public static LoopStatement Loop(Expression test, Expression increment, Expression body, Expression @else) {
            return Loop(SourceSpan.None, SourceLocation.None, test, increment, body, @else);
        }

        public static LoopStatement Loop(SourceSpan span, SourceLocation header, Expression test, Expression increment, Expression body, Expression @else) {
            Contract.RequiresNotNull(body, "body");
            Contract.Requires(test == null || test.Type == typeof(bool), "test");
            return new LoopStatement(span.Start, span.End, header, test, increment, body, @else);
        }
    }
}
