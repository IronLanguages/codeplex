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
using MSAst = Microsoft.Scripting.Ast;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class TupleExpression : SequenceExpression {
        private bool _expandable;

        public TupleExpression(bool expandable, params Expression[] items)
            : base(items) {
            _expandable = expandable;
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            if (_expandable) {
                return Ast.NewArray(
                    Span,
                    typeof(object[]),
                    ag.Transform(Items)
                );
            }

            return Ast.Call(
                Span,
                null,
                AstGenerator.GetHelperMethod("MakeTuple"),
                ag.Transform(Items)
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (Items != null) {
                    foreach (Expression e in Items) {
                        e.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }

        internal bool IsExpandable {
            get {
                return _expandable;
            }
        }
    }
}
