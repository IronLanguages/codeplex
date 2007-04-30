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

using MSAst = Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting;

namespace IronPython.Compiler.Ast {
    public class TryStatement : Statement {
        private SourceLocation _header;
        private Statement _body;
        private readonly TryStatementHandler[] _handlers;

        private Statement _else;
        private Statement _finally;

        public TryStatement(Statement body, TryStatementHandler[] handlers, Statement else_, Statement finally_) {
            _body = body;
            _handlers = handlers;
            _else = else_;
            _finally = finally_;
        }

        public SourceLocation Header {
            set { _header = value; }
        }

        public Statement Body {
            get { return _body; }
        }

        public Statement Else {
            get { return _else; }
        }

        public Statement Finally {
            get { return _finally; }
        }

        public TryStatementHandler[] Handlers {
            get { return _handlers; }
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            return new MSAst.TryStatement(
                ag.Transform(_body),
                ag.Transform(_handlers),
                ag.Transform(_else),
                ag.Transform(_finally),
                Span,
                _header
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_body != null) {
                    _body.Walk(walker);
                }
                if (_handlers != null) {
                    foreach (TryStatementHandler handler in _handlers) {
                        handler.Walk(walker);
                    }
                }
                if (_else != null) {
                    _else.Walk(walker);
                }
                if (_finally != null) {
                    _finally.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }

    public class TryStatementHandler : Node {
        private SourceLocation _header;
        private readonly Expression _test, _target;
        private readonly Statement _body;

        public TryStatementHandler(Expression test, Expression target, Statement body) {
            _test = test;
            _target = target;
            _body = body;
        }

        public SourceLocation Header {
            set { _header = value; }
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Target {
            get { return _target; }
        }

        public Statement Body {
            get { return _body; }
        }

        internal MSAst.TryStatementHandler Transform(AstGenerator ag) {
            if (_target != null) {
                MSAst.BoundExpression target = ag.MakeTempExpression("exception_target", _target.Span);
                return new MSAst.TryStatementHandler(
                    ag.Transform(_test),
                    target.Reference,
                    new MSAst.BlockStatement(
                        new MSAst.Statement[] {
                            _target.TransformSet(ag, target, Operators.None),
                            ag.Transform(_body),
                        },
                        _body.Span
                    ),
                    Span,
                    _header
                );
            } else {
                return new MSAst.TryStatementHandler(
                    ag.Transform(_test),
                    null,
                    ag.Transform(_body),
                    Span,
                    _header
                );
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_test != null) {
                    _test.Walk(walker);
                }
                if (_target != null) {
                    _target.Walk(walker);
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
