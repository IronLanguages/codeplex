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

using MSAst = Microsoft.Scripting.Ast;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    public class ListComprehensionIf : ListComprehensionIterator {
        private readonly Expression _test;

        public ListComprehensionIf(Expression test) {
            _test = test;
        }

        public Expression Test {
            get { return _test; }
        }

        internal override MSAst.Statement Transform(AstGenerator ag, MSAst.Statement body) {
            return new MSAst.IfStatement(
                new MSAst.IfStatementTest[] {
                    new MSAst.IfStatementTest(
                        ag.Transform(_test),
                        body,
                        Span,
                        Span.End)
                    },
                null,
                Span);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_test != null) {
                    _test.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
