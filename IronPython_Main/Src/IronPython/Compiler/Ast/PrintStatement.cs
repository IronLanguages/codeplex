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

using System.Collections.Generic;
using MSAst = Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting;

namespace IronPython.Compiler.Ast {
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

        internal override MSAst.Statement Transform(AstGenerator ag) {
            MSAst.Expression destination = ag.Transform(_dest);

            if (_expressions.Length == 0) {
                if (destination != null) {
                    return new MSAst.ExpressionStatement(
                        MSAst.MethodCallExpression.Call(
                            null,
                            AstGenerator.GetHelperMethod("PrintNewlineWithDest"),
                            destination
                        ),
                        Span
                    );
                } else {
                    return new MSAst.ExpressionStatement(
                        MSAst.MethodCallExpression.Call(
                            null,
                            AstGenerator.GetHelperMethod("PrintNewline")
                        ),
                        Span
                    );
                }
            } else {
                // Create list for the individual statements
                List<MSAst.Statement> statements = new List<MSAst.Statement>();

                // Store destination in a temp, if we have one
                if (destination != null) {
                    MSAst.BoundExpression temp = ag.MakeTempExpression("destination", destination.Span);

                    statements.Add(
                        AstGenerator.MakeAssignment(temp.Reference, destination, destination.Span)
                    );

                    destination = temp;
                }
                for (int i = 0; i < _expressions.Length; i++) {
                    string method = (i < _expressions.Length - 1 || _trailingComma) ? "PrintComma" : "Print";
                    Expression current = _expressions[i];
                    MSAst.MethodCallExpression mce;

                    if (destination != null) {
                        mce = MSAst.MethodCallExpression.Call(
                            null,
                            AstGenerator.GetHelperMethod(method + "WithDest"),
                            destination,
                            current.Transform(ag)
                        );
                    } else {
                        mce = MSAst.MethodCallExpression.Call(
                            null,
                            AstGenerator.GetHelperMethod(method),
                            current.Transform(ag)
                        );
                    }

                    statements.Add(new MSAst.ExpressionStatement(mce));
                }

                return new MSAst.BlockStatement(statements.ToArray(), Span);
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
