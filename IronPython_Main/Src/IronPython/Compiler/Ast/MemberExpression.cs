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
using System.Scripting;
using System.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

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
            return AstUtils.GetMember(
                ag.Binder,
                SymbolTable.IdToString(_name),
                type,
                Ast.CodeContext(),
                ag.Transform(_target)
            );
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, Operators op) {
            if (op == Operators.None) {
                SourceSpan sspan = span.IsValid ? new SourceSpan(Span.Start, span.End) : SourceSpan.None;
                return AstUtils.Block(
                    span,
                    AstUtils.SetMember(
                        ag.Binder,
                        SymbolTable.IdToString(_name),
                        typeof(object),
                        Ast.CodeContext(),
                        ag.Transform(_target),
                        right
                    )
                );
            } else {
                MSAst.VariableExpression temp = ag.MakeTempExpression("inplace");
                return AstUtils.Block(
                    new SourceSpan(Span.Start, span.End),
                    Ast.Assign(temp, ag.Transform(_target)),
                    AstUtils.SetMember(
                        ag.Binder,
                        SymbolTable.IdToString(_name),
                        typeof(object),
                        Ast.CodeContext(),
                        temp,
                        AstUtils.Operator(
                            ag.Binder,
                            op,
                            typeof(object),
                            Ast.CodeContext(),
                            AstUtils.GetMember(ag.Binder, SymbolTable.IdToString(_name), typeof(object), Ast.CodeContext(), temp),
                            right
                        )
                    )
                );
            }
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            return AstUtils.DeleteMember(
                Span,
                ag.Binder,
                SymbolTable.IdToString(_name),
                Ast.CodeContext(),
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
