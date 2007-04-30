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

using System.Collections;
using Microsoft.Scripting.Internal;
using MSAst = Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting;

namespace IronPython.Compiler.Ast {
    public class ForStatement : Statement {
        private SourceLocation _header;
        private readonly Expression _left;
        private Expression _list;
        private Statement _body;
        private readonly Statement _else;

        public ForStatement(Expression left, Expression list, Statement body, Statement else_) {
            _left = left;
            _list = list;
            _body = body;
            _else = else_;
        }

        public SourceLocation Header {
            set { _header = value; }
        }

        public Expression Left {
            get { return _left; }
        }

        public Statement Body {
            get { return _body; }
            set { _body = value; }
        }

        public Expression List {
            get { return _list; }
            set { _list = value; }
        }

        public Statement Else {
            get { return _else; }
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            // Temporary variable for the IEnumerator object
            MSAst.VariableReference enumerator = ag.MakeGeneratorTemp(
                SymbolTable.StringToId("foreach_enumerator"),
                typeof(IEnumerator)
            );

            return TransformForStatement(ag, enumerator, _list, _left, _body, _else, Span, _header);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_left != null) {
                    _left.Walk(walker);
                }
                if (_list != null) {
                    _list.Walk(walker);
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
                if (_else != null) {
                    _else.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        internal static MSAst.Statement TransformForStatement(AstGenerator ag, MSAst.VariableReference enumerator,
                                                    Expression list, Expression left, Statement body,
                                                    Statement else_, SourceSpan span, SourceLocation header) {
            return TransformForStatement(ag, enumerator, list, left, ag.Transform(body), else_, span, header);
        }

        internal static MSAst.Statement TransformForStatement(AstGenerator ag, MSAst.VariableReference enumerator,
                                                    Expression list, Expression left, MSAst.Statement body,
                                                    Statement else_, SourceSpan span, SourceLocation header) {
            // enumerator = PythonOps.GetEnumeratorForIteration(list)
            MSAst.BoundAssignment init = MSAst.BoundAssignment.Assign(
                enumerator,
                MSAst.MethodCallExpression.Call(
                    null,
                    AstGenerator.GetHelperMethod("GetEnumeratorForIteration"),
                    ag.Transform(list)
                ),
                list.Span
            );

            // while enumerator.MoveNext():
            //    left = enumerator.Current
            //    body
            // else:
            //    else
            MSAst.LoopStatement ls = new MSAst.LoopStatement(
                MSAst.MethodCallExpression.Call(
                    new MSAst.BoundExpression(enumerator),
                    typeof(IEnumerator).GetMethod("MoveNext")
                ),
                null,
                MSAst.BlockStatement.Block(
                    left.TransformSet(
                        ag,
                        MSAst.MethodCallExpression.Call(
                            new MSAst.BoundExpression(enumerator),
                            typeof(IEnumerator).GetProperty("Current").GetGetMethod()
                        ),
                        Operators.None
                    ),
                    body
                ),
                ag.Transform(else_),
                new SourceSpan(left.Start, span.End),
                left.End
            );

            return MSAst.BlockStatement.Block(
                new MSAst.ExpressionStatement(init, init.Span),
                ls
            );
        }
    }
}
