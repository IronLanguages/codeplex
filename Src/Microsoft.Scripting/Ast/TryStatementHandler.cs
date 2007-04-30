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

namespace Microsoft.Scripting.Internal.Ast {
    public class TryStatementHandler : Node {
        private readonly SourceLocation _header;
        private readonly Expression _test;
        private readonly VariableReference _target;
        private readonly Statement _body;

        public TryStatementHandler(Expression test, VariableReference target, Statement body)
            : this(test, target, body, SourceSpan.None, SourceLocation.None) {
        }

        public TryStatementHandler(Expression test, VariableReference target, Statement body, SourceSpan span, SourceLocation header)
            : base(span) {
            _test = test;
            _target = target;
            _body = body;
            _header = header;
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public VariableReference Target {
            get { return _target; }
        }

        public Expression Test {
            get { return _test; }
        }

        public Statement Body {
            get { return _body; }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                if (_test != null) _test.Walk(walker);
                _body.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
