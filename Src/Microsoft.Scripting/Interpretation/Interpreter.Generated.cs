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
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpretation {
    public partial class Interpreter {

        private delegate object InterpretDelegate(InterpreterState state, Expression expression);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static object Interpret(InterpreterState state, Expression expr) {
            switch (expr.NodeType) {
                #region Generated Ast Interpreter

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_interpreter from: generate_tree.py

                 case ExpressionType.Add: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.AddChecked: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.And: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.AndAlso: return InterpretAndAlsoBinaryExpression(state, expr);
                 case ExpressionType.ArrayLength: return InterpretUnaryExpression(state, expr);
                 case ExpressionType.ArrayIndex: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.Call: return InterpretMethodCallExpression(state, expr);
                 case ExpressionType.Coalesce: return InterpretCoalesceBinaryExpression(state, expr);
                 case ExpressionType.Conditional: return InterpretConditionalExpression(state, expr);
                 case ExpressionType.Constant: return InterpretConstantExpression(state, expr);
                 case ExpressionType.Convert: return InterpretConvertUnaryExpression(state, expr);
                 case ExpressionType.ConvertChecked: return InterpretConvertUnaryExpression(state, expr);
                 case ExpressionType.Divide: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.Equal: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.ExclusiveOr: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.GreaterThan: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.GreaterThanOrEqual: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.Invoke: return InterpretInvocationExpression(state, expr);
                 case ExpressionType.Lambda: return InterpretLambdaExpression(state, expr);
                 case ExpressionType.LeftShift: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.LessThan: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.LessThanOrEqual: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.ListInit: return InterpretListInitExpression(state, expr);
                 case ExpressionType.MemberAccess: return InterpretMemberExpression(state, expr);
                 case ExpressionType.MemberInit: return InterpretMemberInitExpression(state, expr);
                 case ExpressionType.Modulo: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.Multiply: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.MultiplyChecked: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.Negate: return InterpretUnaryExpression(state, expr);
                 case ExpressionType.UnaryPlus: return InterpretUnaryExpression(state, expr);
                 case ExpressionType.NegateChecked: return InterpretUnaryExpression(state, expr);
                 case ExpressionType.New: return InterpretNewExpression(state, expr);
                 case ExpressionType.NewArrayInit: return InterpretNewArrayExpression(state, expr);
                 case ExpressionType.NewArrayBounds: return InterpretNewArrayExpression(state, expr);
                 case ExpressionType.Not: return InterpretUnaryExpression(state, expr);
                 case ExpressionType.NotEqual: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.Or: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.OrElse: return InterpretOrElseBinaryExpression(state, expr);
                 case ExpressionType.Parameter: return InterpretParameterExpression(state, expr);
                 case ExpressionType.Power: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.Quote: return InterpretQuoteUnaryExpression(state, expr);
                 case ExpressionType.RightShift: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.Subtract: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.SubtractChecked: return InterpretBinaryExpression(state, expr);
                 case ExpressionType.TypeAs: return InterpretUnaryExpression(state, expr);
                 case ExpressionType.TypeIs: return InterpretTypeBinaryExpression(state, expr);
                 case ExpressionType.Assign: return InterpretAssignmentExpression(state, expr);
                 case ExpressionType.Block: return InterpretBlock(state, expr);
                 case ExpressionType.DoStatement: return InterpretDoStatement(state, expr);
                 case ExpressionType.Dynamic: return InterpretDynamicExpression(state, expr);
                 case ExpressionType.EmptyStatement: return InterpretEmptyStatement(state, expr);
                 case ExpressionType.Extension: return InterpretExtensionExpression(state, expr);
                 case ExpressionType.Goto: return InterpretGotoExpression(state, expr);
                 case ExpressionType.Index: return InterpretIndexExpression(state, expr);
                 case ExpressionType.Label: return InterpretLabelExpression(state, expr);
                 case ExpressionType.LocalScope: return InterpretLocalScopeExpression(state, expr);
                 case ExpressionType.LoopStatement: return InterpretLoopStatement(state, expr);
                 case ExpressionType.ReturnStatement: return InterpretReturnStatement(state, expr);
                 case ExpressionType.Scope: return InterpretScopeExpression(state, expr);
                 case ExpressionType.SwitchStatement: return InterpretSwitchStatement(state, expr);
                 case ExpressionType.ThrowStatement: return InterpretThrowStatement(state, expr);
                 case ExpressionType.TryStatement: return InterpretTryStatement(state, expr);
                 case ExpressionType.Unbox: return InterpretUnboxUnaryExpression(state, expr);

                // *** END GENERATED CODE ***

                #endregion
                default: throw Assert.Unreachable;
            };
        }
    }
}
