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

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System;
using System.Dynamic;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    public class MemberExpression : Expression {
        private readonly Expression _target;
        private readonly string _name;

        public MemberExpression(Expression target, string name) {
            _target = target;
            _name = name;
        }

        public Expression Target {
            get { return _target; }
        }

        public string Name {
            get { return _name; }
        }

        public override string ToString() {
            return base.ToString() + ":" + _name;
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return ag.Get(
                type,
                _name,
                ag.Transform(_target)
            );
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, PythonOperationKind op) {
            if (op == PythonOperationKind.None) {
                return ag.AddDebugInfoAndVoid(
                    ag.Set(
                        typeof(object),
                        _name,
                        ag.Transform(_target),
                        right
                    ),
                    span
                );
            } else {
                MSAst.ParameterExpression temp = ag.GetTemporary("inplace");
                return ag.AddDebugInfo(
                    Ast.Block(
                        Ast.Assign(temp, ag.Transform(_target)),
                        SetMemberOperator(ag, right, op, temp),
                        AstUtils.Empty()
                    ),
                    Span.Start,
                    span.End
                );
            }
        }

        internal override string CheckAssign() {
            return null;
        }

        internal override string CheckDelete() {
            return null;
        }

        private MSAst.Expression SetMemberOperator(AstGenerator ag, MSAst.Expression right, PythonOperationKind op, MSAst.ParameterExpression temp) {
            return ag.Set(
                typeof(object),
                _name,
                temp,
                ag.Operation(
                    typeof(object),
                    op,
                    ag.Get(
                        typeof(object),
                        _name,
                        temp
                    ),
                    right
                )
            );
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            return ag.Delete(
                typeof(void),
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
