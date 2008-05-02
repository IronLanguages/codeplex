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

using System;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Expression;

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
                ag.Binder,
                _name,
                type,
                ag.Transform(_target)
            );
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, Operators op) {
            if (op == Operators.None) {
                SourceSpan sspan = span.IsValid ? new SourceSpan(Span.Start, span.End) : SourceSpan.None;
                return Ast.Block(
                    span,
                    Ast.Action.SetMember(
                        ag.Binder,
                        _name,
                        typeof(object),
                        ag.Transform(_target),
                        right
                    )
                );
            } else {
                MSAst.VariableExpression temp = ag.MakeTempExpression("inplace");
                return Ast.Block(
                    new SourceSpan(Span.Start, span.End),
                    Ast.Assign(temp, ag.Transform(_target)),
                    Ast.Action.SetMember(
                        ag.Binder,
                        _name,
                        typeof(object),
                        temp,
                        Ast.Action.Operator(
                            ag.Binder,
                            op,
                            typeof(object),
                            Ast.Action.GetMember(ag.Binder, _name, typeof(object), temp),
                            right
                        )
                    )
                );
            }
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            return Ast.Action.DeleteMember(
                Span,
                ag.Binder,
                _name,
                ag.Transform(_target)
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
