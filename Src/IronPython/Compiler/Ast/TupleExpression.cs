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

using System; using Microsoft;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Binding;

using MSAst = Microsoft.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Linq.Expressions.Expression;
    using IronPython.Runtime.Operations;

    public class TupleExpression : SequenceExpression {
        private bool _expandable;

        public TupleExpression(bool expandable, params Expression[] items)
            : base(items) {
            _expandable = expandable;
        }

        internal override string CheckAssign() {
            if (Items.Length == 0) {
                return "can't assign to ()";
            }
            for (int i = 0; i < Items.Length; i++) {
                Expression e = Items[i];
                if (e.CheckAssign() != null) {
                    // we don't return the same message here as CPython doesn't seem to either, 
                    // for example ((yield a), 2,3) = (2,3,4) gives a different error than
                    // a = yield 3 = yield 4.
                    return "can't assign to " + e.NodeName;
                }
            }
            return null;
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            if (_expandable) {
                return Ast.NewArrayInit(
                    typeof(object),
                    ag.TransformAndConvert(Items, typeof(object))
                );
            }

            if (Items.Length == 0) {
                return Ast.Field(
                    null,
                    typeof(PythonOps).GetField("EmptyTuple")
                );
            }

            return Ast.Call(
                AstGenerator.GetHelperMethod("MakeTuple"),
                Ast.NewArrayInit(
                    typeof(object),
                    ag.TransformAndConvert(Items, typeof(object))
                )
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

        public bool IsExpandable {
            get {
                return _expandable;
            }
        }
    }
}
