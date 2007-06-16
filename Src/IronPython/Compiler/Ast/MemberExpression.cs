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
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    public class MemberExpression : Expression {
        private readonly Expression _target;
        private readonly SymbolId _name;

        public MemberExpression(Expression target, SymbolId name) {
            _target = target;
            _name = name;
        }

        public Expression Target {
            get { return _target; }
        }

        public SymbolId Name {
            get { return _name; }
        }

        public override string ToString() {
            return base.ToString() + ":" + SymbolTable.IdToString(_name);
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return MSAst.ActionExpression.GetMember(
                Span,
                _name,
                type,
                ag.Transform(_target)
            );
        }

        internal override MSAst.Statement TransformSet(AstGenerator ag, MSAst.Expression right, Operators op) {
            if (op == Operators.None) {
                return new MSAst.ExpressionStatement(
                    MSAst.ActionExpression.SetMember(
                        _name,
                        typeof(object),
                        ag.Transform(_target),
                        right
                    ),
                    right.End.IsValid ? new SourceSpan(Span.Start, right.End) : SourceSpan.None);
            } else {
                MSAst.BoundExpression temp = ag.MakeTempExpression("inplace", _target.Span);
                return MSAst.BlockStatement.Block(
                    new SourceSpan(Span.Start, right.End),
                    new MSAst.ExpressionStatement(
                        MSAst.BoundAssignment.Assign(temp.Variable, ag.Transform(_target))),
                    new MSAst.ExpressionStatement(
                        MSAst.ActionExpression.SetMember(
                            _name,
                            typeof(object),
                            temp,
                            MSAst.ActionExpression.Operator(
                                op,
                                typeof(object),
                                MSAst.ActionExpression.GetMember(_name, typeof(object), temp),
                                right
                            )
                        )
                    )
                );
            }
        }

        internal override MSAst.Statement TransformDelete(AstGenerator ag) {
            return new MSAst.ExpressionStatement(
                new MSAst.DeleteDynamicMemberExpression(
                    ag.Transform(_target),
                    _name,
                    Span
                ),
                Span
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_target != null) {
                    _target.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
