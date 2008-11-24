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
using System.Diagnostics;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = Microsoft.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Linq.Expressions.Expression;

    public class BinaryExpression : Expression {
        private readonly Expression _left, _right;
        private readonly PythonOperator _op;

        public BinaryExpression(PythonOperator op, Expression left, Expression right) {
            ContractUtils.RequiresNotNull(left, "left");
            ContractUtils.RequiresNotNull(right, "right");
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

        // This is a compound comparison operator like: a < b < c.
        // That's represented as binary operators, but it's not the same as (a<b) < c, so we do special transformations.
        // We need to:
        // - return true iff (a<b) && (b<c), but ensure that b is only evaluated once. 
        // - ensure evaluation order is correct (a,b,c)
        // - don't evaluate c if a<b is false.
        private MSAst.Expression FinishCompare(MSAst.Expression left, AstGenerator ag) {
            Debug.Assert(_right is BinaryExpression);

            BinaryExpression bright = (BinaryExpression)_right;

            // Transform the left child of my right child (the next node in sequence)
            MSAst.Expression rleft = ag.Transform(bright.Left);

            // Store it in the temp
            MSAst.ParameterExpression temp = ag.GetTemporary("chained_comparison");

            // Create binary operation: left <_op> (temp = rleft)
            MSAst.Expression comparison = MakeBinaryOperation(
                ag,
                _op,
                left,
                Ast.Assign(temp, Ast.Convert(rleft, temp.Type)),
                typeof(object),
                Span
            );

            MSAst.Expression rright;

            // Transform rright, comparing to temp
            if (IsComparison(bright._right)) {
                rright = bright.FinishCompare(temp, ag);
            } else {
                MSAst.Expression transformedRight = ag.Transform(bright.Right);
                rright = MakeBinaryOperation(
                    ag,
                    bright.Operator,
                    temp,
                    transformedRight,
                    typeof(object),
                    bright.Span
                );
            }

            ag.FreeTemp(temp);

            // return (left (op) (temp = rleft)) and (rright)
            return AstUtils.CoalesceTrue(
                ag.Block,
                comparison,
                rright,
                AstGenerator.GetHelperMethod("IsTrue")
            );
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            MSAst.Expression left = ag.Transform(_left);

            if (NeedComparisonTransformation()) {
                // This is a compound comparison like: (a < b < c)
                return FinishCompare(left, ag);
            } else {
                // Simple binary operator.
                return MakeBinaryOperation(ag, _op, left, ag.Transform(_right), type, Span);
            }
        }

        private static MSAst.Expression MakeBinaryOperation(AstGenerator ag, PythonOperator op, MSAst.Expression left, MSAst.Expression right, Type type, SourceSpan span) {
            if (op == PythonOperator.NotIn) {                
                return AstUtils.Convert(
                    Ast.Not(
                        Binders.Operation(
                            ag.BinderState,
                            typeof(bool),
                            StandardOperators.Contains,
                            left,
                            right
                        )                            
                    ),
                    type
                );
            }

            Operators action = PythonOperatorToAction(op);
            if (action != Operators.None) {
                // Create action expression
                if (op == PythonOperator.Divide &&
                    (ag.DivisionOptions == PythonDivisionOptions.Warn || ag.DivisionOptions == PythonDivisionOptions.WarnAll)) {
                    MSAst.ParameterExpression tempLeft = ag.GetTemporary("left", left.Type);
                    MSAst.ParameterExpression tempRight = ag.GetTemporary("right", right.Type);
                    return Ast.Block(
                        Ast.Call(
                            AstGenerator.GetHelperMethod("WarnDivision"),
                            AstUtils.CodeContext(),
                            Ast.Constant(ag.DivisionOptions),
                            AstUtils.Convert(
                                Ast.Assign(tempLeft, left),
                                typeof(object)
                            ),
                            AstUtils.Convert(
                                Ast.Assign(tempRight, right),
                                typeof(object)
                            )
                        ),
                        Binders.Operation(
                            ag.BinderState,
                            type,
                            StandardOperators.FromOperator(action),
                            tempLeft,
                            tempRight
                        )
                    );
                }

                return Binders.Operation(
                    ag.BinderState,
                    type,
                    StandardOperators.FromOperator(action),
                    left,
                    right
                );
            } else {
                // Call helper method
                return Ast.Call(
                    AstGenerator.GetHelperMethod(GetHelperName(op)),
                    AstGenerator.ConvertIfNeeded(left, typeof(object)),
                    AstGenerator.ConvertIfNeeded(right, typeof(object))
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
                    return Operators.ExclusiveOr;
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
                    return Operators.Equals;
                case PythonOperator.NotEqual:
                    return Operators.NotEquals;

                case PythonOperator.In:
                    return Operators.Contains;

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

        internal override bool CanThrow {
            get {
                if (_op == PythonOperator.Is || _op == PythonOperator.IsNot) {
                    return _left.CanThrow || _right.CanThrow;
                }
                return true;
            }
        }
    }
}
