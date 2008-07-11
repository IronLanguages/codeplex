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

using System;
using System.Scripting;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using IronPython.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

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
            return AstUtils.Operator(
                ag.Binder,
                GetOperator,
                type,
                GetActionArgumentsForGetOrDelete(ag)
            );

        }

        private MSAst.Expression[] GetActionArgumentsForGetOrDelete(AstGenerator ag) {
            TupleExpression te = _index as TupleExpression;
            if (te != null && te.IsExpandable) {
                return ArrayUtils.Insert(Ast.CodeContext(), ag.Transform(_target), ag.Transform(te.Items));
            }

            SliceExpression se = _index as SliceExpression;
            if (se != null) {
                if (se.StepProvided) {
                    return new MSAst.Expression[] { 
                        Ast.CodeContext(),
                        ag.Transform(_target),
                        GetSliceValue(ag, se.SliceStart),
                        GetSliceValue(ag, se.SliceStop),
                        GetSliceValue(ag, se.SliceStep) 
                    };
                }

                return new MSAst.Expression[] { 
                    Ast.CodeContext(),
                    ag.Transform(_target),
                    GetSliceValue(ag, se.SliceStart),
                    GetSliceValue(ag, se.SliceStop)
                };
            }

            return new MSAst.Expression[] { Ast.CodeContext(), ag.Transform(_target), ag.Transform(_index) };
        }

        private static MSAst.Expression GetSliceValue(AstGenerator ag, Expression expr) {
            if (expr != null) {
                return ag.Transform(expr);
            }

            return Ast.Field(null, typeof(MissingParameter).GetField("Value"));
        }

        private MSAst.Expression[] GetActionArgumentsForSet(AstGenerator ag, MSAst.Expression right) {
            return ArrayUtils.Append(GetActionArgumentsForGetOrDelete(ag), right);
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, Operators op) {
            if (op != Operators.None) {
                right = AstUtils.Operator(
                    ag.Binder,
                    op,
                    typeof(object),
                    Ast.CodeContext(),
                    AstUtils.Operator(
                        ag.Binder,
                        GetOperator,
                        typeof(object),
                        GetActionArgumentsForGetOrDelete(ag)
                    ),
                    right
                );
            }

            return AstUtils.Block(
                Span,
                AstUtils.Operator(
                    ag.Binder,
                    SetOperator,
                    typeof(object),
                    GetActionArgumentsForSet(ag, right)
                )
            );
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            return AstUtils.Operator(
                Span,
                ag.Binder,
                DeleteOperator,
                typeof(object),
                GetActionArgumentsForGetOrDelete(ag)
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

        private Operators GetOperator {
            get {
                if (_index is SliceExpression) {
                    return Operators.GetSlice;
                }

                return Operators.GetItem;
            }
        }

        private Operators SetOperator {
            get {
                if (_index is SliceExpression) {
                    return Operators.SetSlice;
                }

                return Operators.SetItem;
            }
        }

        private Operators DeleteOperator {
            get {
                if (_index is SliceExpression) {
                    return Operators.DeleteSlice;
                }

                return Operators.DeleteItem;
            }
        }
    }
}
