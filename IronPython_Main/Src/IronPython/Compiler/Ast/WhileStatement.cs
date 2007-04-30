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

using Microsoft.Scripting.Internal;
using MSAst = Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting;

namespace IronPython.Compiler.Ast {
    public class WhileStatement : Statement {
        private SourceLocation _header;
        private readonly Expression _test;
        private readonly Statement _body;
        private readonly Statement _else;

        public WhileStatement(Expression test, Statement body, Statement else_) {
            _test = test;
            _body = body;
            _else = else_;
        }

        public SourceLocation Header {
            set { _header = value; }
        }

        public Expression Test {
            get { return _test;}
        }

        public Statement Body {
            get { return _body; }
        }

        public Statement ElseStatement {
            get { return _else; }
        }

        public void SetLoc(SourceLocation start, SourceLocation header, SourceLocation end) {
            Start = start;
            _header = header;
            End = end;
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            return new MSAst.LoopStatement(
                ag.Transform(_test),
                null,                // increment
                ag.Transform(_body),
                ag.Transform(_else),
                Span,
                _header
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_test != null) {
                    _test.Walk(walker);
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
                if (_else != null) {
                    _else.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
