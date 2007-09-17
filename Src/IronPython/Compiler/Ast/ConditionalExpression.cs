/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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

    public class ConditionalExpression : Expression {
        private readonly Expression _testExpr;
        private readonly Expression _trueExpr;
        private readonly Expression _falseExpr;

        public ConditionalExpression(Expression testExpression, Expression trueExpression, Expression falseExpression) {
            this._testExpr = testExpression;
            this._trueExpr = trueExpression;
            this._falseExpr = falseExpression;
        }

        public Expression FalseExpression {
            get { return _falseExpr; }
        }

        public Expression Test {
            get { return _testExpr; }
        }

        public Expression TrueExpression {
            get { return _trueExpr; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return Ast.Condition(
                Span,
                ag.TransformAndConvert(_testExpr, typeof(bool)),
                ag.TransformAndConvert(_trueExpr, type),
                ag.TransformAndConvert(_falseExpr, type)
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_testExpr != null) {
                    _testExpr.Walk(walker);
                }
                if (_trueExpr != null) {
                    _trueExpr.Walk(walker);
                }
                if (_falseExpr != null) {
                    _falseExpr.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
