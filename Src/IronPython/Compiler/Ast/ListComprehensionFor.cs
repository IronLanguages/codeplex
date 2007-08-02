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

using System.Collections;
using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;
using Operators = Microsoft.Scripting.Operators;

namespace IronPython.Compiler.Ast {
    public class ListComprehensionFor : ListComprehensionIterator {
        private readonly Expression _lhs, _list;

        public ListComprehensionFor(Expression lhs, Expression list) {
            _lhs = lhs;
            _list = list;
        }

        public Expression Left {
            get { return _lhs; }
        }

        public Expression List {
            get { return _list; }
        }

        internal override MSAst.Statement Transform(AstGenerator ag, MSAst.Statement body) {
            MSAst.Variable temp = ag.MakeTemp(SymbolTable.StringToId("list_comprehension_for"), typeof(IEnumerator));
            return ForStatement.TransformForStatement(ag, temp, _list, _lhs, body, null, Span, _lhs.End);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_lhs != null) {
                    _lhs.Walk(walker);
                }
                if (_list != null) {
                    _list.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}