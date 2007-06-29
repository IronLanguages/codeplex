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
using Microsoft.Scripting.Actions;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

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
                return Ast.Empty(Span);
            }

            // Transform into:
            // if (_test) {
            // } else {
            //     RaiseAssertionError(_message);
            // }
            return Ast.Unless(                                  // if
                Span,                                   
                ag.TransformAndConvert(_test, typeof(bool)),    // _test
                Ast.Statement(
                    Ast.Call(                                   // else branch
                        Span,
                        null,
                        AstGenerator.GetHelperMethod("RaiseAssertionError"),
                        _message != null ?
                            ag.TransformAsObject(_message) :
                            Ast.Null()
                    )
                )
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
