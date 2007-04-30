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
using System.Diagnostics;
using MSAst = Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

namespace IronPython.Compiler.Ast {
    public class BinaryExpression : Expression {
        private readonly Expression _left, _right;
        private readonly PythonOperator _op;

        public BinaryExpression(PythonOperator op, Expression left, Expression right) {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            if (op == PythonOperator.None) throw new ArgumentException("op");

            _op = op;
            _left = left;
            _right = right;
            Start = left.Start;
            End = right.End;
        }

        public Expression Left {
            get { return _left; }
        }

        public Expression Right {
            get { return _right; }
        }

        public PythonOperator Operator {
            get { return _op; }
        }

        private bool IsComparison() {
            switch (_op) {
                case PythonOperator.LessThan:
                case PythonOperator.LessThanOrEqual:
                case PythonOperator.GreaterThan:
                case PythonOperator.GreaterThanOrEqual:
                case PythonOperator.Equal:
                case PythonOperator.NotEqual:
                case PythonOperator.In:
                case PythonOperator.NotIn:
                case PythonOperator.IsNot:
                case PythonOperator.Is:
                    return true;
            }
            return false;
        }

        private bool NeedComparisonTransformation() {
            return IsComparison() && IsComparison(_right);
        }

        public static bool IsComparison(Expression expression) {
            BinaryExpression be = expression as BinaryExpression;
            return be != null && be.IsComparison();
        }

        private MSAst.Expression FinishCompare(MSAst.Expression left, AstGenerator ag) {
            Debug.Assert(_right is BinaryExpression);

            BinaryExpression bright = (BinaryExpression)_right;

            // Transform the left child of my right child (the next node in sequence)
            MSAst.Expression rleft = bright.Left.Transform(ag);

            // Store it in the temp
            MSAst.BoundExpression temp = ag.MakeTempExpression("chained_comparison", rleft.Span);

            // Create binary operation: left <_op> (temp = rleft)
            MSAst.Expression comparison = MakeBinaryOperation(
                _op,
                left,
                new MSAst.BoundAssignment(temp.Reference, rleft,  Operators.None),
                Span
            );

            MSAst.Expression rright;

            // Transform rright, comparing to temp
            if (IsComparison(bright._right)) {
                rright = bright.FinishCompare(temp, ag);
            } else {
                rright = MakeBinaryOperation(
                    bright.Operator,
                    temp,
                    bright.Right.Transform(ag),
                    bright.Span
                );
            }

            ag.FreeTemp(temp);

            // return (left (op) (temp = rleft)) and (rright)
            return new MSAst.AndExpression(comparison, rright, Span);
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            MSAst.Expression left = ag.Transform(_left);

            if (NeedComparisonTransformation()) {
                return FinishCompare(left, ag);
            } else {
                return MakeBinaryOperation(_op, left, ag.Transform(_right), Span);
            }
        }

        private static MSAst.Expression MakeBinaryOperation(PythonOperator op, MSAst.Expression left, MSAst.Expression right, SourceSpan span) {
            Operators action = PythonOperatorToAction(op);
            if (action != Operators.None) {
                // Create action expression
                return new MSAst.ActionExpression(
                    DoOperationAction.Make(action),
                    new MSAst.Expression[] {
                            left,
                            right,
                        },
                    span
                );
            } else {
                // Call helper method
                return new MSAst.MethodCallExpression(
                    AstGenerator.GetHelperMethod(GetHelperName(op)),
                    null,
                    new MSAst.Expression[] {
                        left,
                        right
                    },
                    span
                );
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                _left.Walk(walker);
                _right.Walk(walker);
            }
            walker.PostWalk(this);
        }

        private static Operators PythonOperatorToAction(PythonOperator op) {
            switch (op) {
                // Binary
                case PythonOperator.Add:
                    return Operators.Add;
                case PythonOperator.Subtract:
                    return Operators.Subtract;
                case PythonOperator.Multiply:
                    return Operators.Multiply;
                case PythonOperator.Divide:
                    return Operators.Divide;
                case PythonOperator.TrueDivide:
                    return Operators.TrueDivide;
                case PythonOperator.Mod:
                    return Operators.Mod;
                case PythonOperator.BitwiseAnd:
                    return Operators.BitwiseAnd;
                case PythonOperator.BitwiseOr:
                    return Operators.BitwiseOr;
                case PythonOperator.Xor:
                    return Operators.Xor;
                case PythonOperator.LeftShift:
                    return Operators.LeftShift;
                case PythonOperator.RightShift:
                    return Operators.RightShift;
                case PythonOperator.Power:
                    return Operators.Power;
                case PythonOperator.FloorDivide:
                    return Operators.FloorDivide;

                // Comparisons
                case PythonOperator.LessThan:
                    return Operators.LessThan;
                case PythonOperator.LessThanOrEqual:
                    return Operators.LessThanOrEqual;
                case PythonOperator.GreaterThan:
                    return Operators.GreaterThan;
                case PythonOperator.GreaterThanOrEqual:
                    return Operators.GreaterThanOrEqual;
                case PythonOperator.Equal:
                    return Operators.Equal;
                case PythonOperator.NotEqual:
                    return Operators.NotEqual;

                case PythonOperator.In:
                case PythonOperator.NotIn:
                case PythonOperator.IsNot:
                case PythonOperator.Is:
                    return Operators.None;

                default:
                    Debug.Assert(false, "Unexpected PythonOperator: " + op.ToString());
                    return Operators.None;
            }
        }

        private static string GetHelperName(PythonOperator op) {
            switch (op) {
                case PythonOperator.In:
                    return "In";
                case PythonOperator.NotIn:
                    return "NotIn";
                case PythonOperator.IsNot:
                    return "IsNot";
                case PythonOperator.Is:
                    return "Is";

                default:
                    Debug.Assert(false, "Invalid PythonOperator: " + op.ToString());
                    return null;
            }
        }
    }
}
