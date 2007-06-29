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

using System.Diagnostics;
using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class ExecStatement : Statement {
        private readonly Expression _code, _locals, _globals;

        public ExecStatement(Expression code, Expression locals, Expression globals) {
            _code = code;
            _locals = locals;
            _globals = globals;
        }

        public Expression Code {
            get { return _code; }
        }

        public Expression Locals {
            get { return _locals; }
        }

        public Expression Globals {
            get { return _globals; }
        }

        public bool NeedsLocalsDictionary() {
            return _globals == null && _locals == null;
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            MSAst.MethodCallExpression call;

            if (_locals == null && _globals == null) {
                // exec code
                call = Ast.Call(
                    Span,
                    null,
                    AstGenerator.GetHelperMethod("UnqualifiedExec"),
                    Ast.CodeContext(),
                    ag.TransformAsObject(_code)
                );
            } else {
                // exec code in globals [ , locals ]
                // We must have globals now (locals is last and may be absent)
                Debug.Assert(_globals != null);
                call = Ast.Call(
                    Span,
                    null,
                    AstGenerator.GetHelperMethod("QualifiedExec"),
                    Ast.CodeContext(),
                    ag.Transform(_code),
                    ag.TransformOrConstantNull(_globals, typeof(IAttributesCollection)),
                    ag.TransformOrConstantNull(_locals, typeof(object))
                );
            }

            return Ast.Statement(Span, call);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_code != null) {
                    _code.Walk(walker);
                }
                if (_locals != null) {
                    _locals.Walk(walker);
                }
                if (_globals != null) {
                    _globals.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
