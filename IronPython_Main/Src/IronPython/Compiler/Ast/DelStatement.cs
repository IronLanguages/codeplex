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

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using IronPython.Runtime.Operations;

    public class DelStatement : Statement {
        private readonly Expression[] _expressions;

        public DelStatement(Expression[] expressions) {
            _expressions = expressions;
        }

        public Expression[] Expressions {
            get { return _expressions; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            // Transform to series of individual del statements.
            MSAst.Expression[] statements = new MSAst.Expression[_expressions.Length];
            for (int i = 0; i < statements.Length; i++) {
                statements[i] = _expressions[i].TransformDelete(ag);
                if (statements[i] == null) {     
                    throw PythonOps.SyntaxError(string.Format("can't delete {0}", _expressions[i].NodeName), ag.Context.SourceUnit, _expressions[i].Span, 1);
                }
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
