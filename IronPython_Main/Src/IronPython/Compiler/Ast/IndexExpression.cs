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
using System.Diagnostics;

using IronPython.Runtime;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

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
                GetOperator,
                type,
                GetActionArgumentsForGetOrDelete(ag)
            );
            
        }

        private MSAst.Expression[] GetActionArgumentsForGetOrDelete(AstGenerator ag) {
            TupleExpression te = _index as TupleExpression;
            if (te != null && te.IsExpandable) {
                return ArrayUtils.Insert(ag.Transform(_target), ag.Transform(te.Items));
            }

            SliceExpression se = _index as SliceExpression;
            if (se != null) {
                if (se.StepProvided) {
                    return new MSAst.Expression[] { 
                        ag.Transform(_target),
                        GetSliceValue(ag, se.SliceStart),
                        GetSliceValue(ag, se.SliceStop),
                        GetSliceValue(ag, se.SliceStep) 
                    };
                }

                return new MSAst.Expression[] { 
                    ag.Transform(_target),
                    GetSliceValue(ag, se.SliceStart),
                    GetSliceValue(ag, se.SliceStop)
                };
            }

            return new MSAst.Expression[] { ag.Transform(_target), ag.Transform(_index) };
        }

        private static MSAst.Expression GetSliceValue(AstGenerator ag, Expression expr) {
            if (expr != null) {
                return ag.Transform(expr);
            } 

            return Ast.ReadField(null, typeof(MissingParameter).GetField("Value"));            
        }

        private MSAst.Expression[] GetActionArgumentsForSet(AstGenerator ag, MSAst.Expression right) {
            return ArrayUtils.Append(GetActionArgumentsForGetOrDelete(ag), right);
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, Operators op) {
            if(op != Operators.None) {
                right = Ast.Action.Operator(op,
                            typeof(object),
                            Ast.Action.Operator(
                                GetOperator,
                                typeof(object),
                                GetActionArgumentsForGetOrDelete(ag)
                            ),
                            right
                        );
            }

            return Ast.Statement(
                Span,
                Ast.Void(
                    Ast.Action.Operator(
                        SetOperator,
                        typeof(object),
                        GetActionArgumentsForSet(ag, right)
                    )
                )
            );
        }
        
        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            return Ast.Statement(
                Span,
                Ast.Action.Operator(
                    DeleteOperator,
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
