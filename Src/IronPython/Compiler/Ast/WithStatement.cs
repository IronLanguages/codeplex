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
using Microsoft.Scripting.Actions;

namespace IronPython.Compiler.Ast {
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

/*
        mgr = (EXPR)
        exit = mgr.__exit__  # Not calling it yet
        value = mgr.__enter__()
        exc = True
        try:
            VAR = value  # Only if "as VAR" is present
            BLOCK
        except:
            # The exceptional case is handled here
            exc = False
            if not exit(*sys.exc_info()):
                raise
            # The exception is swallowed if exit() returns true
        finally:
            # The normal and non-local-goto cases are handled here
            if exc:
                exit(None, None, None)
*/

        internal override MSAst.Statement Transform(AstGenerator ag) {
            MSAst.Statement[] statements = new MSAst.Statement[5];
            
            // 1. mgr = (EXPR)
            MSAst.BoundExpression manager = ag.MakeTempExpression("with_manager", SourceSpan.None);
            statements[0] = AstGenerator.MakeAssignment(
                manager.Reference,
                ag.Transform(_contextManager),
                new SourceSpan(Start, _header)
            );

            // 2. exit = mgr.__exit__  # Not calling it yet
            MSAst.BoundExpression exit = ag.MakeGeneratorTempExpression("with_exit", SourceSpan.None);
            statements[1] = AstGenerator.MakeAssignment(
                exit.Reference,
                new MSAst.DynamicMemberExpression(
                    manager,
                    SymbolTable.StringToId("__exit__"),
                    MSAst.MemberBinding.Bound
                )
            );

            // 3. value = mgr.__enter__()
            MSAst.BoundExpression value = ag.MakeTempExpression("with_value", SourceSpan.None);
            statements[2] = AstGenerator.MakeAssignment(
                value.Reference,
                new MSAst.CallExpression(
                    new MSAst.DynamicMemberExpression(
                        manager,
                        SymbolTable.StringToId("__enter__"),
                        MSAst.MemberBinding.Bound
                    ),
                    new MSAst.Arg[0],
                    false, false, 0, 0
                )
            );

            // 4. exc = True
            MSAst.BoundExpression exc = ag.MakeGeneratorTempExpression("with_exc", SourceSpan.None);
            statements[3] = AstGenerator.MakeAssignment(
                exc.Reference,
                new MSAst.ConstantExpression(true)
            );

            // 5. if not exit(*sys.exc_info()):
            //        raise
            MSAst.Statement if_not_exit_raise = MSAst.IfStatement.IfThen(
                MakeNotExpression(MakeExitCall(exit)),
                new MSAst.ExpressionStatement(new MSAst.ThrowExpression(null)));

            // Create null argument for later use in the call to exit
            MSAst.Arg null_arg = MSAst.Arg.Simple(new MSAst.ConstantExpression(null));

            statements[4] = new MSAst.TryStatement(
                // try statement body
                _var != null ?
                    new MSAst.BlockStatement(
                        new MSAst.Statement[] {
                            _var.TransformSet(ag, value, Operators.None),
                            ag.Transform(_body)
                        },
                        _body.Span
                    ) :
                    ag.Transform(_body),

                // try statement handler
                new MSAst.TryStatementHandler[] {
                    new MSAst.TryStatementHandler(
                        null,               // no test
                        null,               // no target

                        new MSAst.BlockStatement(
                            new MSAst.Statement[] {
                                // exc = False
                                AstGenerator.MakeAssignment(
                                    exc.Reference,
                                    new MSAst.ConstantExpression(false)
                                ),

                                // if not exit(*sys.exc_info()):
                                //    raise

                                if_not_exit_raise,
                            }
                        )
                    ),
                },
                // try statement "else" statement
                null,

                // try statement "finally"
                MSAst.IfStatement.IfThen(
                    exc,
                    new MSAst.ExpressionStatement(
                        new MSAst.CallExpression(
                            exit,
                            new MSAst.Arg[] {
                                null_arg,
                                null_arg,
                                null_arg
                            },
                            false, false, 0, 0, _contextManager.Span
                        )
                    )
                ),

                // try statement location
                Span,
                _header
            );

            return new MSAst.BlockStatement(statements, _body.Span);
        }

        private static MSAst.Expression MakeNotExpression(MSAst.Expression e) {
            return new MSAst.ActionExpression(
                DoOperationAction.Make(Operators.Not),
                new MSAst.Expression[] { e, },
                SourceSpan.None);
        }

        private MSAst.Expression MakeExitCall(MSAst.BoundExpression exit) {
            return new MSAst.CallExpression(
                exit,
                new MSAst.Arg[] {
                    MSAst.Arg.List(
                            MSAst.MethodCallExpression.Call(
                                null,
                                AstGenerator.GetHelperMethod("ExtractSysExcInfo"),
                                new MSAst.CodeContextExpression()
                            )
                        )
                    },
                true,
                false,
                0,
                1);
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
