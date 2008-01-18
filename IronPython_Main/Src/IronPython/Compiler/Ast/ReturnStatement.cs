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

using MSAst = Microsoft.Scripting.Ast;
using Microsoft.Scripting;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;    

    public class ReturnStatement : Statement {
        private readonly Expression _expression;

        public ReturnStatement(Expression expression) {
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        } 

        internal override MSAst.Expression Transform(AstGenerator ag) {
            if ((_expression != null) && (ag.Block is MSAst.GeneratorCodeBlock)) {
                // Return statements in Generators can not have an expression.
                // Generators only return values via the yield keyword.
                ag.AddError("'return' with argument inside generator", this.Span);
                return null;
            }
            return Ast.Return(
                Span,
                ag.TransformOrConstantNull(_expression, typeof(object))
            );
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
