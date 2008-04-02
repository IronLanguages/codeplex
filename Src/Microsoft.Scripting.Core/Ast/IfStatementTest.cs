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

    // TODO: Remove???
    public sealed class IfStatementTest {
        private readonly SourceLocation _start;
        private readonly SourceLocation _header;
        private readonly SourceLocation _end;

        private readonly Expression /*!*/ _test;
        private readonly Expression /*!*/ _body;

        internal IfStatementTest(SourceLocation start, SourceLocation end, SourceLocation header, Expression /*!*/ test, Expression /*!*/ body) {
            _test = test;
            _body = body;
            _header = header;
            _start = start;
            _end = end;
        }

        internal SourceLocation Start {
            get { return _start; }
        }

        internal SourceLocation Header {
            get { return _header; }
        }

        internal SourceLocation End {
            get { return _end; }
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Body {
            get { return _body; }
        }
    }

    public static partial class Ast {
        public static IfStatementTest IfCondition(Expression test, Expression body) {
            return IfCondition(SourceSpan.None, SourceLocation.None, test, body);
        }

        public static IfStatementTest IfCondition(SourceSpan span, SourceLocation header, Expression test, Expression body) {
            Contract.RequiresNotNull(test, "test");
            Contract.RequiresNotNull(body, "body");
            Contract.Requires(test.Type == typeof(bool), "test", "Test must be boolean");

            return new IfStatementTest(span.Start, span.End, header, test, body);
        }

        public static IfStatementTest[] IfConditions(params IfStatementTest[] tests) {
            Contract.RequiresNotNullItems(tests, "tests");
            return tests;
        }
    }
}
