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

using System.Collections;
using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Runtime;

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

        internal override MSAst.Expression Transform(AstGenerator ag) {
            // Temporary variable for the IEnumerator object
            MSAst.VariableExpression enumerator = ag.MakeTemp(SymbolTable.StringToId("foreach_enumerator"), typeof(IEnumerator));

            // Only the body is "in the loop" for the purposes of break/continue
            // The "else" clause is outside
            MSAst.Expression body;
            MSAst.LabelTarget label = ag.EnterLoop();
            try {
                body = ag.Transform(_body);
            } finally {
                ag.ExitLoop();
            }
            return TransformForStatement(ag, enumerator, _list, _left, body, _else, Span, _header, label);
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

        internal static MSAst.Expression TransformForStatement(AstGenerator ag, MSAst.VariableExpression enumerator,
                                                    Expression list, Expression left, MSAst.Expression body,
                                                    Statement else_, SourceSpan span, SourceLocation header,
                                                    MSAst.LabelTarget loopLabel) {
            // enumerator = PythonOps.GetEnumeratorForIteration(list)
            MSAst.BoundAssignment init = Ast.Assign(
                enumerator,
                Ast.Call(
                    AstGenerator.GetHelperMethod("GetEnumeratorForIteration"),
                    ag.TransformAsObject(list)
                )
            );

            // while enumerator.MoveNext():
            //    left = enumerator.Current
            //    body
            // else:
            //    else
            MSAst.LoopStatement ls = Ast.Loop(
                new SourceSpan(left.Start, span.End),
                left.End,
                loopLabel,
                Ast.Call(
                    Ast.Read(enumerator),
                    typeof(IEnumerator).GetMethod("MoveNext")
                ),
                null,
                Ast.Block(
                    left.TransformSet(
                        ag,
                        SourceSpan.None,
                        Ast.Call(
                            Ast.Read(enumerator),
                            typeof(IEnumerator).GetProperty("Current").GetGetMethod()
                        ),
                        Operators.None
                    ),
                    body
                ),
                ag.Transform(else_)
            );

            return Ast.Block(
                Ast.Statement(list.Span, init),
                ls
            );
        }
    }
}
