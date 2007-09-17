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

using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class DelStatement : Statement {
        private readonly Expression[] _expressions;

        public DelStatement(Expression[] expressions) {
            _expressions = expressions;
        }

        public Expression[] Expressions {
            get { return _expressions; }
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            // Transform to series of individual del statements.
            MSAst.Statement[] statements = new MSAst.Statement[_expressions.Length];
            for (int i = 0; i < statements.Length; i++) {
                statements[i] = _expressions[i].TransformDelete(ag);
            }
            return Ast.Block(
                Span,
                statements
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_expressions != null) {
                    foreach (Expression expression in _expressions) {
                        expression.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }
    }
}
