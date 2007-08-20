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
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class TryStatement : Statement {
        private SourceLocation _header;
        private Statement _body;
        private readonly TryStatementHandler[] _handlers;

        private Statement _else;
        private Statement _finally;

        private static bool GenerateNewTryStatement = false;

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
            if (GenerateNewTryStatement) {
                return TransformToTryStatement(ag);
            } else {
                return Ast.DynamicTry(
                    Span,
                    _header,
                    ag.Transform(_body),
                    ag.Transform(_handlers),
                    ag.Transform(_else),
                    ag.Transform(_finally)
                );
            }
        }

        private MSAst.Statement TransformToTryStatement(AstGenerator ag) {
            return Ast.TryCatchFinally(
                Span,
                _header,
                ag.Transform(_body),
                TransformCatch(ag),
                ag.Transform(_finally)
            );
        }

        private MSAst.CatchBlock[] TransformCatch(AstGenerator ag) {
            if (_handlers == null) {
                return null;
            } else {
                return new MSAst.CatchBlock[] {
                    TransformCatchBlocks(ag)
                };
            }
        }

        private MSAst.CatchBlock TransformCatchBlocks(AstGenerator ag) {
            Debug.Assert(_handlers != null);

            MSAst.BoundExpression exception = ag.MakeTempExpression("exception", typeof(Exception), SourceSpan.None);
            MSAst.BoundExpression extracted = ag.MakeTempExpression("extracted", SourceSpan.None);

            //
            // extracted = PushExceptionHandler(context, exception)
            //
            MSAst.Statement init = Ast.Statement(
                Ast.Assign(
                    extracted.Variable,
                    Ast.Call(
                        null,
                        AstGenerator.GetHelperMethod("PushExceptionHandler"),
                        Ast.CodeContext(),
                        exception
                    )
                )
            );

            //
            // PopExceptionHandler(context)
            //
            MSAst.Statement done = Ast.Statement(
                Ast.Call(
                    null,
                    AstGenerator.GetHelperMethod("PopExceptionHandler"),
                    Ast.CodeContext()
                )
            );

            List<MSAst.IfStatementTest> tests = new List<MSAst.IfStatementTest>(_handlers.Length);
            MSAst.BoundExpression converted = null;
            MSAst.Statement catchAll = null;

            for (int index = 0; index < _handlers.Length; index++) {
                TryStatementHandler tsh = _handlers[index];

                if (tsh.Test != null) {
                    MSAst.IfStatementTest ist;

                    //  translating:
                    //      except Test ...
                    //
                    //  generate following AST for the Test (common part):
                    //      CheckException(context, exception, Test)
                    MSAst.Expression test =
                        Ast.Call(
                            null,
                            AstGenerator.GetHelperMethod("CheckException"),
                            Ast.CodeContext(),
                            extracted,
                            ag.TransformAsObject(tsh.Test)
                        );

                    if (tsh.Target != null) {
                        //  translating:
                        //      except Test, Target:
                        //          <body>
                        //  into:
                        //      if ((converted = CheckException(context, exception, Test)) != null) {
                        //          Target = converted;
                        //          <body>
                        //      }

                        if (converted == null) {
                            converted = ag.MakeTempExpression("converted", SourceSpan.None);
                        }

                        ist = Ast.IfCondition(
                            Ast.NotEqual(
                                Ast.Assign(converted.Variable, test),
                                Ast.Null()
                            ),
                            Ast.Block(
                                tsh.Target.TransformSet(ag, converted, Operators.None),
                                ag.Transform(tsh.Body)
                            )
                        );
                    } else {
                        //  translating:
                        //      except Test:
                        //          <body>
                        //  into:
                        //      if (CheckException(context, exception, Test) != null) {
                        //          <body>
                        //      }
                        ist = Ast.IfCondition(
                            Ast.NotEqual(
                                test,
                                Ast.Null()
                            ),
                            ag.Transform(tsh.Body)
                        );
                    }

                    // Add the test to the if statement test cascade
                    tests.Add(ist);
                } else {
                    Debug.Assert(index == _handlers.Length - 1);
                    Debug.Assert(catchAll == null);

                    //  translating:
                    //      except:
                    //          <body>
                    //  into:
                    //      <body>
                    catchAll = ag.Transform(tsh.Body);
                }
            }

            MSAst.Statement body = null;

            if (tests.Count > 0) {
                // rethrow the exception if we have no catch-all block
                if (catchAll == null) {
                    catchAll = Ast.Statement(
                        Ast.Throw(exception)
                    );
                }

                body = Ast.If(
                    tests.ToArray(),
                    catchAll
                );
            } else {
                Debug.Assert(catchAll != null);
                body = catchAll;
            }

            MSAst.TryStatement handler = Ast.TryFinally(
                Span,
                _header,
                Ast.Block(
                    init,
                    body
                ),
                done
            );

            return Ast.Catch(
                typeof(Exception),
                exception.Variable,
                handler
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

        internal MSAst.DynamicTryStatementHandler Transform(AstGenerator ag) {
            if (_target != null) {
                MSAst.BoundExpression target = ag.MakeTempExpression("exception_target", _target.Span);
                return new MSAst.DynamicTryStatementHandler(
                    ag.Transform(_test),
                    target.Variable,
                    Ast.Block(
                        _body.Span,
                        _target.TransformSet(ag, target, Operators.None),
                        ag.Transform(_body)
                    ),
                    Span,
                    _header
                );
            } else {
                return new MSAst.DynamicTryStatementHandler(
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
