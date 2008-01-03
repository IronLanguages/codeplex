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

using System;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class TryStatement : Statement {
        private SourceLocation _header;

        /// <summary>
        /// The statements under the try-block.
        /// </summary>
        private Statement _body;

        /// <summary>
        /// Array of except (catch) blocks associated with this try. NULL if there are no except blocks.
        /// </summary>
        private readonly TryStatementHandler[] _handlers;

        /// <summary>
        /// The body of the optional Else block for this try. NULL if there is no Else block.
        /// </summary>
        private Statement _else;

        /// <summary>
        /// The body of the optional finally associated with this try. NULL if there is no finally block.
        /// </summary>
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
            MSAst.Statement body = ag.Transform(_body);
            MSAst.Statement @else = ag.Transform(_else);
            MSAst.Statement @finally = ag.Transform(_finally);

            MSAst.Variable exception;
            MSAst.Statement @catch = TransformHandlers(ag, out exception);

            // We have else clause, must generate guard around it
            if (@else != null) {
                Debug.Assert(@catch != null);

                MSAst.BoundExpression runElse = ag.MakeTempExpression("run_else", typeof(bool));

                //  run_else = true;
                //  try {
                //      try_body
                //  } catch ( ... ) {
                //      run_else = false;
                //      catch_body
                //  }
                //  if (run_else) {
                //      else_body
                //  }
                MSAst.Statement result =
                    Ast.Block(
                        Ast.Write(runElse.Variable, Ast.True()),
                        Ast.Try(
                            Span, _header, body
                        ).Catch(exception.Type, exception,
                            Ast.Write(runElse.Variable, Ast.False()),
                            @catch
                        ),
                        Ast.IfThen(runElse,
                            @else
                        )
                    );

                // If we have both "else" and "finally", wrap the whole result in
                // another try .. finally
                if (@finally != null) {
                    result = Ast.Try(
                         result
                    ).Finally(
                        @finally
                    );
                }
                return result;
            } else {        // no "else" clause
                //  try {
                //      <try body>
                //  } catch (Exception e) {
                //      ... catch handling ...
                //  } finally {
                //      ... finally body ...
                //  }
                //
                //  Either catch or finally may be absent, but not both.
                //
                return Ast.TryCatchFinally(
                    Span, _header,
                    body,
                    @catch != null ? new MSAst.CatchBlock[] { Ast.Catch(exception.Type, exception, @catch) } : null,
                    @finally
                );
            }
        }

        /// <summary>
        /// Transform multiple python except handlers for a try block into a single catch body.
        /// </summary>
        /// <param name="ag"></param>
        /// <param name="variable">The variable for the exception in the catch block.</param>
        /// <returns>Null if there are no except handlers. Else the statement to go inside the catch handler</returns>
        private MSAst.Statement TransformHandlers(AstGenerator ag, out MSAst.Variable variable) {
            if (_handlers == null || _handlers.Length == 0) {
                variable = null;
                return null;
            }

            MSAst.BoundExpression exception = ag.MakeTempExpression("exception", typeof(Exception));
            MSAst.BoundExpression extracted = ag.MakeTempExpression("extracted", typeof(object));

            // The variable where the runtime will store the exception.
            variable = exception.Variable;

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
                    //      CheckException(exception, Test)
                    MSAst.Expression test =
                        Ast.Call(
                            AstGenerator.GetHelperMethod("CheckException"),
                            extracted,
                            ag.TransformAsObject(tsh.Test)
                        );

                    if (tsh.Target != null) {
                        //  translating:
                        //      except Test, Target:
                        //          <body>
                        //  into:
                        //      if ((converted = CheckException(exception, Test)) != null) {
                        //          ClearDynamicStackFrames();
                        //          Target = converted;
                        //          <body>
                        //      }

                        if (converted == null) {
                            converted = ag.MakeTempExpression("converted");
                        }

                        ist = Ast.IfCondition(
                            tsh.Span, tsh.Header,
                            Ast.NotEqual(
                                Ast.Assign(converted.Variable, test),
                                Ast.Null()
                            ),
                            Ast.Block(
                                ClearDynamicStackFramesAst(SourceSpan.None),
                                tsh.Target.TransformSet(ag, SourceSpan.None, converted, Operators.None),
                                ag.Transform(tsh.Body)
                            )
                        );
                    } else {
                        //  translating:
                        //      except Test:
                        //          <body>
                        //  into:
                        //      if (CheckException(exception, Test) != null) {
                        //          ClearDynamicStackFrames();
                        //          <body>
                        //      }
                        ist = Ast.IfCondition(
                            tsh.Span, tsh.Header,
                            Ast.NotEqual(
                                test,
                                Ast.Null()
                            ),
                            Ast.Block(
                                ClearDynamicStackFramesAst(SourceSpan.None),
                                ag.Transform(tsh.Body)
                            )
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
                    //  {
                    //      ClearDynamicStackFrames();
                    //      <body>
                    //  }

                    catchAll = Ast.Block(
                        ClearDynamicStackFramesAst(new SourceSpan(tsh.Start, tsh.Header)),
                        ag.Transform(tsh.Body)
                    );
                }
            }

            MSAst.Statement body = null;

            if (tests.Count > 0) {
                // rethrow the exception if we have no catch-all block
                if (catchAll == null) {
                    catchAll = Ast.Throw(exception);
                }

                body = Ast.If(
                    tests.ToArray(),
                    catchAll
                );
            } else {
                Debug.Assert(catchAll != null);
                body = catchAll;
            }

            // Codegen becomes:
            //   try {
            //     extracted = PythonOps.SetCurrentException(exception)
            //      < dynamic exception analysis >
            //  } finally {
            //     PythonOps.CheckThreadAbort(); 
            //  }
            return Ast.Try(
                Ast.Statement(
                    Ast.Assign(
                        extracted.Variable,
                        Ast.Call(
                            AstGenerator.GetHelperMethod("SetCurrentException"),
                            exception
                        )
                    )
                ),
                body
            ).Finally(
                Ast.Statement(
                    Ast.Call(
                        AstGenerator.GetHelperMethod("CheckThreadAbort")
                    )
                )
            );
        }

        private static MSAst.Statement ClearDynamicStackFramesAst(SourceSpan span) {
            return Ast.Statement(
                span,
                Ast.Call(AstGenerator.GetHelperMethod("ClearDynamicStackFrames"))
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

    // A handler corresponds to the except block.
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
            get { return _header; }
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
