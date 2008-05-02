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
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Expression;

    public class ListComprehensionIf : ListComprehensionIterator {
        private readonly Expression _test;

        public ListComprehensionIf(Expression test) {
            _test = test;
        }

        public Expression Test {
            get { return _test; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, MSAst.Expression body) {
            return Ast.If(
                Ast.IfConditions(
                    Ast.IfCondition(
                        Span, Span.End,
                        ag.TransformAndDynamicConvert(_test, typeof(bool)),
                        body
                    )
                ),
                null
            );
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
