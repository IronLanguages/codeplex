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

using System.Scripting;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

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

        internal override MSAst.Expression Transform(AstGenerator ag) {
            // Only the body is "in the loop" for the purposes of break/continue
            // The "else" clause is outside
            MSAst.Expression body;
            MSAst.LabelTarget label = ag.EnterLoop();
            try {
                body = ag.Transform(_body);
            } finally {
                ag.ExitLoop();
            }
            return AstUtils.While(
                ag.TransformAndDynamicConvert(_test, typeof(bool)), 
                body, 
                ag.Transform(_else), 
                label, 
                _header, 
                Span
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

        internal override bool CanThrow {
            get {
                return _test.CanThrow;
            }
        }
    }
}
