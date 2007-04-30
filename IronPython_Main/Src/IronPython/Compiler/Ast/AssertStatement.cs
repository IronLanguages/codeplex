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

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;

using MSAst = Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Actions;

namespace IronPython.Compiler.Ast {
    public class AssertStatement : Statement {
        private readonly Expression _test, _message;

        public AssertStatement(Expression test, Expression message) {
            _test = test;
            _message = message;
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Message {
            get { return _message; }
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            // If debugging is off, return empty statement
            if (!ScriptDomainManager.Options.DebugMode) {
                return new MSAst.EmptyStatement(Span);
            }

            // Transform into:
            // if (! _test) {
            //     RaiseAssertionError(_message);
            // }

            return new MSAst.IfStatement(               // if
                new MSAst.IfStatementTest[] {
                    new MSAst.IfStatementTest(          // !_test
                        MakeNotTest(ag),                        
                        new MSAst.ExpressionStatement(
                            new MSAst.MethodCallExpression(
                                AstGenerator.GetHelperMethod("RaiseAssertionError"),
                                null,
                                new MSAst.Expression[] {
                                    _message != null ?
                                        _message.Transform(ag) :
                                        new MSAst.ConstantExpression(null)
                                },
                                Span
                            ),
                            Span
                        ),
                        Span, _test.End
                    )
                },
                null,
                Span
                );
        }

        private MSAst.Expression MakeNotTest(AstGenerator ag) {
            return new MSAst.ActionExpression(
                DoOperationAction.Make(Operators.Not),
                new MSAst.Expression[] {
                    ag.Transform(_test),
                },
                _test.Span
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_test != null) {
                    _test.Walk(walker);
                }
                if (_message != null) {
                    _message.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
