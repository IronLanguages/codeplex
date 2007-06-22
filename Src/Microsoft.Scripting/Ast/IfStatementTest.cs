/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
namespace Microsoft.Scripting.Ast {
    public class IfStatementTest : Node {
        private readonly SourceLocation _header;
        private readonly Expression _test;
        private readonly Statement _body;

        public IfStatementTest(Expression test, Statement body)
            : this(test, body, SourceSpan.None, SourceLocation.None) {
        }

        public IfStatementTest(Expression test, Statement body, SourceSpan span, SourceLocation header)
            : base(span) {
            if (test == null) throw new ArgumentNullException("test");
            if (body == null) throw new ArgumentNullException("body");

            _test = test;
            _body = body;
            _header = header;
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public Expression Test {
            get { return _test; }
        }

        public Statement Body {
            get { return _body; }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _test.Walk(walker);
                _body.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
