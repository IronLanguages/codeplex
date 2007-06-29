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
using IronPython.Runtime;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public abstract class ListComprehensionIterator : Node {
        internal abstract MSAst.Statement Transform(AstGenerator ag, MSAst.Statement body);
    }

    public class ListComprehension : Expression {
        private readonly Expression _item;
        private readonly ListComprehensionIterator[] _iterators;

        public ListComprehension(Expression item, ListComprehensionIterator[] iterators) {
            _item = item;
            _iterators = iterators;
        }

        public Expression Item {
            get { return _item; }
        }

        public ListComprehensionIterator[] Iterators {
            get { return _iterators; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            MSAst.BoundExpression list = ag.MakeTempExpression("list_comprehension_list", typeof(List), _item.Span);

            // 1. Initialization code - create list and store it in the temp variable
            MSAst.BoundAssignment initialize =
                Ast.Assign(
                    _item.Span,
                    list.Variable,
                    Ast.Call(
                        _item.Span,
                        null,                                                                  // instance
                        AstGenerator.GetHelperMethod("MakeList", Utils.Reflection.EmptyTypes), // method
                        new MSAst.Expression[0]                                                // arguments
                    )                    
                );

            // 2. Create body from _item:   list.Append(_item)
            MSAst.Statement body = Ast.Statement(
                _item.Span,
                Ast.Call(
                    list,
                    typeof(List).GetMethod("Append"),                    
                    ag.Transform(_item)
                )
            );

            // 3. Transform all iterators in reverse order, building the true body:
            int current = _iterators.Length;
            while (current-- > 0) {
                ListComprehensionIterator iterator = _iterators[current];
                body = iterator.Transform(ag, body);
            }

            return Ast.Comma(
                Span,
                2,                          // index of list (result)
                initialize,
                Ast.Void(body),
                list                        // result
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_item != null) {
                    _item.Walk(walker);
                }
                if (_iterators != null) {
                    foreach (ListComprehensionIterator lci in _iterators) {
                        lci.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }
    }
}
