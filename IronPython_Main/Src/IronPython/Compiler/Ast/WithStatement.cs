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
                manager.Variable,
                ag.Transform(_contextManager),
                new SourceSpan(Start, _header)
            );

            // 2. exit = mgr.__exit__  # Not calling it yet
            MSAst.BoundExpression exit = ag.MakeGeneratorTempExpression("with_exit", SourceSpan.None);
            statements[1] = AstGenerator.MakeAssignment(
                exit.Variable,
                Ast.DynamicReadMember(
                    manager,
                    SymbolTable.StringToId("__exit__"),
                    MSAst.MemberBinding.Bound
                )
            );

            // 3. value = mgr.__enter__()
            MSAst.BoundExpression value = ag.MakeTempExpression("with_value", SourceSpan.None);
            statements[2] = AstGenerator.MakeAssignment(
                value.Variable,
                Ast.Action.Call(
                    CallAction.Simple,
                    typeof(object),
                    Ast.DynamicReadMember(
                        manager,
                        SymbolTable.StringToId("__enter__"),
                        MSAst.MemberBinding.Bound
                    )                
                )
            );

            // 4. exc = True
            MSAst.BoundExpression exc = ag.MakeGeneratorTempExpression("with_exc", SourceSpan.None);
            statements[3] = AstGenerator.MakeAssignment(
                exc.Variable,
                Ast.True()
            );

            // 5. if not exit(*sys.exc_info()):
            //        raise
            MSAst.Statement if_not_exit_raise = Ast.IfThen(
                Ast.Action.Operator(Operators.Not, typeof(bool), MakeExitCall(exit)),
                Ast.Statement(Ast.Rethrow())
            );

            // Create null argument for later use in the call to exit
            MSAst.Expression null_arg = Ast.Null();

            statements[4] = Ast.DynamicTry(
                // try statement location
                Span,
                _header,

                // try statement body
                _var != null ?
                    Ast.Block(
                        _body.Span,
                        _var.TransformSet(ag, value, Operators.None),
                        ag.Transform(_body)
                    ) :
                    ag.Transform(_body),

                // try statement handler
                new MSAst.DynamicTryStatementHandler[] {
                    new MSAst.DynamicTryStatementHandler(
                        null,               // no test
                        null,               // no target

                        Ast.Block(
                            // exc = False
                            AstGenerator.MakeAssignment(
                                exc.Variable,
                                Ast.False()
                            ),

                            // if not exit(*sys.exc_info()):
                            //    raise

                            if_not_exit_raise
                        )
                    ),
                },
                // try statement "else" statement
                null,

                // try statement "finally"
                Ast.IfThen(
                    exc,
                    Ast.Statement(
                        Ast.Action.Call(
                            _contextManager.Span,
                            CallAction.Simple,
                            typeof(object),
                            exit,
                            null_arg,
                            null_arg,
                            null_arg
                        )
                    )
                )
            );

            return Ast.Block(_body.Span, statements);
        }

        private MSAst.Expression MakeExitCall(MSAst.BoundExpression exit) {
            return Ast.Action.Call(
                CallAction.Make(new ArgumentInfo(MSAst.ArgumentKind.List)),
                typeof(bool),
                exit,
                Ast.Call(
                    null,
                    AstGenerator.GetHelperMethod("ExtractSysExcInfo"),
                    Ast.CodeContext()
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
