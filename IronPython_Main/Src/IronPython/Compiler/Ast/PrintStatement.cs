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

using System.Collections.Generic;
using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Expression;

    public class PrintStatement : Statement {
        private readonly Expression _dest;
        private readonly Expression[] _expressions;
        private readonly bool _trailingComma;

        public PrintStatement(Expression destination, Expression[] expressions, bool trailingComma) {
            _dest = destination;
            _expressions = expressions;
            _trailingComma = trailingComma;
        }

        public Expression Destination {
            get { return _dest; }
        }

        public Expression[] Expressions {
            get { return _expressions; }
        }

        public bool TrailingComma {
            get { return _trailingComma; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            MSAst.Expression destination = ag.TransformAsObject(_dest);

            if (_expressions.Length == 0) {
                if (destination != null) {
                    return Ast.Call(
                        Span,
                        AstGenerator.GetHelperMethod("PrintNewlineWithDest"),
                        Ast.CodeContext(),
                        destination
                    );
                } else {
                    return Ast.Call(
                        Span,
                        AstGenerator.GetHelperMethod("PrintNewline"),
                        Ast.CodeContext()
                    );
                }
            } else {
                // Create list for the individual statements
                List<MSAst.Expression> statements = new List<MSAst.Expression>();

                // Store destination in a temp, if we have one
                if (destination != null) {
                    MSAst.VariableExpression temp = ag.MakeTempExpression("destination");

                    statements.Add(
                        AstGenerator.MakeAssignment(temp, destination)
                    );

                    destination = temp;
                }
                for (int i = 0; i < _expressions.Length; i++) {
                    string method = (i < _expressions.Length - 1 || _trailingComma) ? "PrintComma" : "Print";
                    Expression current = _expressions[i];
                    MSAst.MethodCallExpression mce;

                    if (destination != null) {
                        mce = Ast.Call(
                            AstGenerator.GetHelperMethod(method + "WithDest"),
                            Ast.CodeContext(),
                            destination,
                            ag.TransformAsObject(current)
                        );
                    } else {
                        mce = Ast.Call(
                            AstGenerator.GetHelperMethod(method),
                            Ast.CodeContext(),
                            ag.TransformAsObject(current)
                        );
                    }

                    statements.Add(mce);
                }

                return Ast.Block(Span, statements.ToArray());
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_dest != null) {
                    _dest.Walk(walker);
                }
                if (_expressions != null) {
                    foreach (Expression expression in _expressions) {
                        expression.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }
    }
}
