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
    using Ast = Microsoft.Scripting.Ast.Ast;

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
            return Ast.Action.GetMember(
                Span,
                _name,
                type,
                ag.Transform(_target)
            );
        }

        internal override MSAst.Statement TransformSet(AstGenerator ag, MSAst.Expression right, Operators op) {
            if (op == Operators.None) {
                return Ast.Statement(
                    right.End.IsValid ? new SourceSpan(Span.Start, right.End) : SourceSpan.None,
                    Ast.Action.SetMember(
                        _name,
                        typeof(object),
                        ag.Transform(_target),
                        right
                    )
                );
            } else {
                MSAst.BoundExpression temp = ag.MakeTempExpression("inplace", _target.Span);
                return Ast.Block(
                    new SourceSpan(Span.Start, right.End),
                    Ast.Statement(
                        Ast.Assign(temp.Variable, ag.Transform(_target))
                    ),
                    Ast.Statement(
                        Ast.Action.SetMember(
                            _name,
                            typeof(object),
                            temp,
                            Ast.Action.Operator(
                                op,
                                typeof(object),
                                Ast.Action.GetMember(_name, typeof(object), temp),
                                right
                            )
                        )
                    )
                );
            }
        }

        internal override MSAst.Statement TransformDelete(AstGenerator ag) {
            return Ast.Statement(
                Ast.Delete(
                    Span,
                    ag.Transform(_target),
                    _name
                )
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