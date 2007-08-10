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

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class SuiteStatement : Statement {
        private readonly Statement[] _statements;

        public SuiteStatement(Statement[] statements) {
            Assert.NotNull(statements);
            _statements = statements;
        }

        public Statement[] Statements {
            get { return _statements; }
        } 

        internal override MSAst.Statement Transform(AstGenerator ag) {
            return Ast.Block(ag.Transform(_statements));
        }
       
        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_statements != null) {
                    foreach (Statement s in _statements) {
                        s.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }

        public override string Documentation {
            get {
                if (_statements.Length > 0 && _statements[0] is ExpressionStatement) {
                    ExpressionStatement es = (ExpressionStatement)_statements[0];
                    if (es.Expression is ConstantExpression) {
                        object val = ((ConstantExpression)es.Expression).Value;
                        if (val is string && !ScriptDomainManager.Options.StripDocStrings) return (string)val;
                    }
                }
                return null;
            }
        }
    }
}
