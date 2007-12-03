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
    public sealed class LoopStatement : Statement {
        private readonly SourceLocation _header;
        private readonly Expression _test;
        private readonly Expression _increment;
        private readonly Statement /*!*/ _body;
        private readonly Statement _else;

        /// <summary>
        /// Null test means infinite loop.
        /// </summary>
        internal LoopStatement(SourceSpan span, SourceLocation header, Expression test, Expression increment, Statement /*!*/ body, Statement @else)
            : base(AstNodeType.LoopStatement, span) {
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

        public Statement Body {
            get { return _body; }
        }

        public Statement ElseStatement {
            get { return _else; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static LoopStatement While(Expression test, Statement body, Statement @else) {
            return Loop(SourceSpan.None, SourceLocation.None, test, null, body, @else);
        }

        public static LoopStatement While(SourceSpan span, SourceLocation header, Expression test, Statement body, Statement @else) {
            return Loop(span, header, test, null, body, @else);
        }

        public static LoopStatement Loop(Statement body) {
            return Loop(SourceSpan.None, SourceLocation.None, null, null, body, null);
        }

        public static LoopStatement Loop(params Statement[] body) {
            return Loop(SourceSpan.None, SourceLocation.None, null, null, Block(body), null);
        }
        
        public static LoopStatement Loop(Expression test, Expression increment, Statement body, Statement @else) {
            return Loop(SourceSpan.None, SourceLocation.None, test, increment, body, @else);
        }

        public static LoopStatement Loop(SourceSpan span, SourceLocation header, Expression test, Expression increment, Statement body, Statement @else) {
            Contract.RequiresNotNull(body, "body");
            Contract.Requires(test == null || test.Type == typeof(bool), "test");
            return new LoopStatement(span, header, test, increment, body, @else);
        }
    }
}
