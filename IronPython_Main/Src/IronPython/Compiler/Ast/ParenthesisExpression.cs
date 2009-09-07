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
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Binding;

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif


namespace IronPython.Compiler.Ast {

    public class ParenthesisExpression : Expression {
        private readonly Expression _expression;

        public ParenthesisExpression(Expression expression) {
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return ag.Transform(_expression, type);
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, PythonOperationKind op) {
            return _expression.TransformSet(ag, span, right, op);
        }

        internal override string CheckAssign() {
            return _expression.CheckAssign();
        }

        internal override string CheckDelete() {
            return _expression.CheckDelete();
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            return _expression.TransformDelete(ag);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_expression != null) {
                    _expression.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
