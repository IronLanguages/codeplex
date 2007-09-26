/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class WithStatement : Statement {
        private SourceLocation _header;
        private readonly Expression _contextManager;
        private readonly Expression _var;
        private Statement _body;

        public WithStatement(Expression contextManager, Expression var, Statement body) {
            _contextManager = contextManager;
            _var = var;
            _body = body;
        }

        public SourceLocation Header {
            set { _header = value; }
        }

        public Expression Variable {
            get { return _var; }
        }

        /// <summary>
        /// WithStatement is translated to the DLR AST equivalent to
        /// the following Python code snippet (from with statement spec):
        /// 
        /// mgr = (EXPR)
        /// exit = mgr.__exit__  # Not calling it yet
        /// value = mgr.__enter__()
        /// exc = True
        /// try:
        ///     VAR = value  # Only if "as VAR" is present
        ///     BLOCK
        /// except:
        ///     # The exceptional case is handled here
        ///     exc = False
        ///     if not exit(*sys.exc_info()):
        ///         raise
        ///     # The exception is swallowed if exit() returns true
        /// finally:
        ///     # The normal and non-local-goto cases are handled here
        ///     if exc:
        ///         exit(None, None, None)
        /// 
        /// </summary>
        internal override MSAst.Statement Transform(AstGenerator ag) {
            // Five statements in the result...
            MSAst.Statement[] statements = new MSAst.Statement[5];

            //******************************************************************
            // 1. mgr = (EXPR)
            //******************************************************************
            MSAst.BoundExpression manager = ag.MakeTempExpression("with_manager", SourceSpan.None);
            statements[0] = AstGenerator.MakeAssignment(
                manager.Variable,
                ag.Transform(_contextManager),
                new SourceSpan(Start, _header)
            );

            //******************************************************************
            // 2. exit = mgr.__exit__  # Not calling it yet
            //******************************************************************
            MSAst.BoundExpression exit = ag.MakeGeneratorTempExpression("with_exit", SourceSpan.None);
            statements[1] = AstGenerator.MakeAssignment(
                exit.Variable,
                Ast.Action.GetMember(
                    SymbolTable.StringToId("__exit__"),
                    typeof(object),
                    manager
                )
            );

            //******************************************************************
            // 3. value = mgr.__enter__()
            //******************************************************************
            MSAst.BoundExpression value = ag.MakeTempExpression("with_value", SourceSpan.None);
            statements[2] = AstGenerator.MakeAssignment(
                value.Variable,
                Ast.Action.Call(
                    typeof(object),
                    Ast.Action.GetMember(
                        SymbolTable.StringToId("__enter__"),
                        typeof(object),
                        manager
                    )                
                )
            );

            //******************************************************************
            // 4. exc = True
            //******************************************************************
            MSAst.BoundExpression exc = ag.MakeGeneratorTempExpression("with_exc", typeof(bool), SourceSpan.None);
            statements[3] = AstGenerator.MakeAssignment(
                exc.Variable,
                Ast.True()
            );

            //******************************************************************
            //  5. The final try statement:
            //
            //  try:
            //      VAR = value  # Only if "as VAR" is present
            //      BLOCK
            //  except:
            //      # The exceptional case is handled here
            //      exc = False
            //      if not exit(*sys.exc_info()):
            //          raise
            //      # The exception is swallowed if exit() returns true
            //  finally:
            //      # The normal and non-local-goto cases are handled here
            //      if exc:
            //          exit(None, None, None)
            //******************************************************************

            MSAst.BoundExpression exception = ag.MakeTempExpression("exception", typeof(Exception), SourceSpan.None);

            statements[4] =
                // try:
                Ast.Try(
                    // try statement location
                    Span, _header,

                    // try statement body
                    _var != null ?
                        Ast.Block(
                            _body.Span,
                            // VAR = value
                            _var.TransformSet(ag, value, Operators.None),
                            // BLOCK
                            ag.Transform(_body)
                        ) :
                        // BLOCK
                        ag.Transform(_body)

                // except:
                ).Catch(typeof(Exception), exception.Variable,
                    Ast.Try(
                        // Python specific exception handling code
                        Ast.Statement(
                            Ast.Call(
                                null,
                                AstGenerator.GetHelperMethod("PushExceptionHandler"),
                                exception
                            )
                        ),
                        // Python specific exception handling code
                        Ast.Statement(
                            Ast.Call(
                                null,
                                AstGenerator.GetHelperMethod("ClearDynamicStackFrames")
                            )
                        ),
                        // exc = False
                        AstGenerator.MakeAssignment(
                            exc.Variable,
                            Ast.False()
                        ),
                        //  if not exit(*sys.exc_info()):
                        //      raise
                        Ast.IfThen(
                            Ast.Action.Operator(Operators.Not, typeof(bool), MakeExitCall(exit)),
                            Ast.Statement(Ast.Rethrow())
                        )
                    ).Finally(
                        // Python specific exception handling code
                        Ast.Statement(
                            Ast.Call(
                                null,
                                AstGenerator.GetHelperMethod("PopExceptionHandler")
                            )
                        )
                    )
                // finally:
                ).Finally(
                    //  if exc:
                    //      exit(None, None, None)
                    Ast.IfThen(
                        exc,
                        Ast.Statement(
                            Ast.Action.Call(
                                _contextManager.Span,
                                typeof(object),
                                exit,
                                Ast.Null(),
                                Ast.Null(),
                                Ast.Null()
                            )
                        )
                    )
                );

            return Ast.Block(_body.Span, statements);
        }

        private MSAst.Expression MakeExitCall(MSAst.BoundExpression exit) {
            // exit(*sys.exc_info())
            return Ast.Action.Call(
                CallAction.Make(new CallSignature(MSAst.ArgumentKind.List)),
                typeof(bool),
                exit,
                Ast.Call(
                    null,
                    AstGenerator.GetHelperMethod("GetExceptionInfo")
                )
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_contextManager != null) {
                    _contextManager.Walk(walker);
                }
                if (_var != null) {
                    _var.Walk(walker);
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
