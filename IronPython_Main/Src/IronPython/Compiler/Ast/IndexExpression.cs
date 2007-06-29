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
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class IndexExpression : Expression {
        private readonly Expression _target;
        private readonly Expression _index;

        public IndexExpression(Expression target, Expression index) {
            _target = target;
            _index = index;
        }

        public Expression Target {
            get { return _target; }
        }

        public Expression Index {
            get { return _index; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return Ast.DynamicReadItem(
                Span,
                ag.Transform(_target),
                ag.Transform(_index)
            );
        }

        internal override MSAst.Statement TransformSet(AstGenerator ag, MSAst.Expression right, Operators op) {
            return Ast.Statement(
                Ast.DynamicAssignItem(
                    Span,
                    ag.Transform(_target),
                    ag.Transform(_index),
                    right,
                    op
                )
            );
        }


        internal override MSAst.Statement TransformDelete(AstGenerator ag) {
            return Ast.Statement(
                Span,
                Ast.Delete(
                    Span,
                    ag.Transform(_target),
                    ag.Transform(_index)
                )
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_target != null) {
                    _target.Walk(walker);
                }
                if (_index != null) {
                    _index.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
