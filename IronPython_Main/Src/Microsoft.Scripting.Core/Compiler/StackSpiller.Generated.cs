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

using System.Diagnostics;
using System.Scripting.Utils;

namespace System.Linq.Expressions.Compiler {
    internal partial class StackSpiller {

        /// <summary>
        /// Rewrite the expression
        /// </summary>
        /// <param name="self">Expression rewriter instance</param>
        /// <param name="node">Expression to rewrite</param>
        /// <param name="stack">State of the stack before the expression is emitted.</param>
        /// <returns>Rewritten expression.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static Result RewriteExpression(StackSpiller self, Expression node, Stack stack) {
            if (node == null) {
                return new Result(RewriteAction.None, null);
            }

            Result result;
            switch (node.NodeType) {
                #region Generated StackSpiller Switch

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_stackspiller_switch from: generate_tree.py

                // Add
                case ExpressionType.Add:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // AddChecked
                case ExpressionType.AddChecked:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // And
                case ExpressionType.And:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // AndAlso
                case ExpressionType.AndAlso:
                    result = RewriteLogicalBinaryExpression(self, node, stack);
                    break;
                // ArrayLength
                case ExpressionType.ArrayLength:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // ArrayIndex
                case ExpressionType.ArrayIndex:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // Call
                case ExpressionType.Call:
                    result = RewriteMethodCallExpression(self, node, stack);
                    break;
                // Coalesce
                case ExpressionType.Coalesce:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // Conditional
                case ExpressionType.Conditional:
                    result = RewriteConditionalExpression(self, node, stack);
                    break;
                // Constant
                case ExpressionType.Constant:
                    result = RewriteConstantExpression(self, node, stack);
                    break;
                // Convert
                case ExpressionType.Convert:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // ConvertChecked
                case ExpressionType.ConvertChecked:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // Divide
                case ExpressionType.Divide:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // Equal
                case ExpressionType.Equal:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // ExclusiveOr
                case ExpressionType.ExclusiveOr:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // GreaterThan
                case ExpressionType.GreaterThan:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // GreaterThanOrEqual
                case ExpressionType.GreaterThanOrEqual:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // Invoke
                case ExpressionType.Invoke:
                    result = RewriteInvocationExpression(self, node, stack);
                    break;
                // Lambda
                case ExpressionType.Lambda:
                    result = RewriteLambdaExpression(self, node, stack);
                    break;
                // LeftShift
                case ExpressionType.LeftShift:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // LessThan
                case ExpressionType.LessThan:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // LessThanOrEqual
                case ExpressionType.LessThanOrEqual:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // ListInit
                case ExpressionType.ListInit:
                    result = RewriteListInitExpression(self, node, stack);
                    break;
                // MemberAccess
                case ExpressionType.MemberAccess:
                    result = RewriteMemberExpression(self, node, stack);
                    break;
                // MemberInit
                case ExpressionType.MemberInit:
                    result = RewriteMemberInitExpression(self, node, stack);
                    break;
                // Modulo
                case ExpressionType.Modulo:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // Multiply
                case ExpressionType.Multiply:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // MultiplyChecked
                case ExpressionType.MultiplyChecked:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // Negate
                case ExpressionType.Negate:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // UnaryPlus
                case ExpressionType.UnaryPlus:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // NegateChecked
                case ExpressionType.NegateChecked:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // New
                case ExpressionType.New:
                    result = RewriteNewExpression(self, node, stack);
                    break;
                // NewArrayInit
                case ExpressionType.NewArrayInit:
                    result = RewriteNewArrayExpression(self, node, stack);
                    break;
                // NewArrayBounds
                case ExpressionType.NewArrayBounds:
                    result = RewriteNewArrayExpression(self, node, stack);
                    break;
                // Not
                case ExpressionType.Not:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // NotEqual
                case ExpressionType.NotEqual:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // Or
                case ExpressionType.Or:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // OrElse
                case ExpressionType.OrElse:
                    result = RewriteLogicalBinaryExpression(self, node, stack);
                    break;
                // Parameter
                case ExpressionType.Parameter:
                    result = RewriteParameterExpression(self, node, stack);
                    break;
                // Power
                case ExpressionType.Power:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // Quote
                case ExpressionType.Quote:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // RightShift
                case ExpressionType.RightShift:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // Subtract
                case ExpressionType.Subtract:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // SubtractChecked
                case ExpressionType.SubtractChecked:
                    result = RewriteBinaryExpression(self, node, stack);
                    break;
                // TypeAs
                case ExpressionType.TypeAs:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // TypeIs
                case ExpressionType.TypeIs:
                    result = RewriteTypeBinaryExpression(self, node, stack);
                    break;
                // Assign
                case ExpressionType.Assign:
                    result = RewriteAssignmentExpression(self, node, stack);
                    break;
                // Block
                case ExpressionType.Block:
                    result = RewriteBlock(self, node, stack);
                    break;
                // BreakStatement
                case ExpressionType.BreakStatement:
                    result = RewriteBreakStatement(self, node, stack);
                    break;
                // Generator
                case ExpressionType.Generator:
                    result = RewriteLambdaExpression(self, node, stack);
                    break;
                // ContinueStatement
                case ExpressionType.ContinueStatement:
                    result = RewriteContinueStatement(self, node, stack);
                    break;
                // DoStatement
                case ExpressionType.DoStatement:
                    result = RewriteDoStatement(self, node, stack);
                    break;
                // Dynamic
                case ExpressionType.Dynamic:
                    result = RewriteDynamicExpression(self, node, stack);
                    break;
                // EmptyStatement
                case ExpressionType.EmptyStatement:
                    result = RewriteEmptyStatement(self, node, stack);
                    break;
                // Extension
                case ExpressionType.Extension:
                    result = RewriteExtensionExpression(self, node, stack);
                    break;
                // Index
                case ExpressionType.Index:
                    result = RewriteIndexExpression(self, node, stack);
                    break;
                // LabeledStatement
                case ExpressionType.LabeledStatement:
                    result = RewriteLabeledStatement(self, node, stack);
                    break;
                // LocalScope
                case ExpressionType.LocalScope:
                    result = RewriteLocalScopeExpression(self, node, stack);
                    break;
                // LoopStatement
                case ExpressionType.LoopStatement:
                    result = RewriteLoopStatement(self, node, stack);
                    break;
                // OnesComplement
                case ExpressionType.OnesComplement:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // ReturnStatement
                case ExpressionType.ReturnStatement:
                    result = RewriteReturnStatement(self, node, stack);
                    break;
                // Scope
                case ExpressionType.Scope:
                    result = RewriteScopeExpression(self, node, stack);
                    break;
                // SwitchStatement
                case ExpressionType.SwitchStatement:
                    result = RewriteSwitchStatement(self, node, stack);
                    break;
                // ThrowStatement
                case ExpressionType.ThrowStatement:
                    result = RewriteThrowStatement(self, node, stack);
                    break;
                // TryStatement
                case ExpressionType.TryStatement:
                    result = RewriteTryStatement(self, node, stack);
                    break;
                // Unbox
                case ExpressionType.Unbox:
                    result = RewriteUnaryExpression(self, node, stack);
                    break;
                // Variable
                case ExpressionType.Variable:
                    result = RewriteVariableExpression(self, node, stack);
                    break;
                // YieldStatement
                case ExpressionType.YieldStatement:
                    result = RewriteYieldStatement(self, node, stack);
                    break;

                // *** END GENERATED CODE ***

                #endregion

                default:
                    throw Assert.Unreachable;
            }

            VerifyRewrite(result, node);

            return result;
        }
    }
}

