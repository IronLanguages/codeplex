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

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Utils;

    public class IndexExpression : Expression {
        private readonly Expression _target;
        private readonly Expression _index;

        public IndexExpression(Expression target, Expression index) {
            _target = target;
            _index = index;
        }

        public Expression Target {
            get { return _target; }
        }

        public Expression Index {
            get { return _index; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return Ast.Action.Operator(
                Span,
                Operators.GetItem,
                type,
                GetActionArgumentsForGetOrDelete(ag)
            );
            
        }

        private MSAst.Expression[] GetActionArgumentsForGetOrDelete(AstGenerator ag) {
            TupleExpression te = _index as TupleExpression;
            if (te != null && te.IsExpandable) {
                return ArrayUtils.Insert(ag.Transform(_target), ag.Transform(te.Items));
            }

            return new MSAst.Expression[] { ag.Transform(_target), ag.Transform(_index) };
        }

        private MSAst.Expression[] GetActionArgumentsForSet(AstGenerator ag, MSAst.Expression right) {
            TupleExpression te = _index as TupleExpression;
            if (te != null && te.IsExpandable) {
                MSAst.Expression[] res = new MSAst.Expression[te.Items.Length + 2];
                res[0] = ag.Transform(_target);
                for (int i = 0; i < te.Items.Length; i++) {
                    res[i + 1] = ag.Transform(te.Items[i]);
                }
                res[res.Length - 1] = right;
                return res;
            }

            return new MSAst.Expression[] { ag.Transform(_target), ag.Transform(_index), right };
        }

        internal override MSAst.Statement TransformSet(AstGenerator ag, MSAst.Expression right, Operators op) {
            if(op != Operators.None) {
                right = Ast.Action.Operator(op,
                            typeof(object),
                            Ast.Action.Operator(
                                Operators.GetItem,
                                typeof(object),
                                GetActionArgumentsForGetOrDelete(ag)
                            ),
                            right
                        );
            }

            return Ast.Statement(
                Ast.Action.Operator(
                    Span,
                    Operators.SetItem,
                    typeof(object),
                    GetActionArgumentsForSet(ag, right)
                )
            );
        }

        internal override MSAst.Statement TransformDelete(AstGenerator ag) {
            return Ast.Statement(
                Ast.Action.Operator(
                    Span,
                    Operators.DeleteItem,
                    typeof(object),
                    GetActionArgumentsForGetOrDelete(ag)
                )
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_target != null) {
                    _target.Walk(walker);
                }
                if (_index != null) {
                    _index.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
