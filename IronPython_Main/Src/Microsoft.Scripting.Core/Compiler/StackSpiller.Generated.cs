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
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {
    internal partial class StackSpiller {

        /// <summary>
        /// Rewrite the expression
        /// </summary>
        /// 
        /// <param name="node">Expression to rewrite</param>
        /// <param name="stack">State of the stack before the expression is emitted.</param>
        /// <returns>Rewritten expression.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private Result RewriteExpression(Expression node, Stack stack) {
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
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // AddChecked
                case ExpressionType.AddChecked:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // And
                case ExpressionType.And:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // AndAlso
                case ExpressionType.AndAlso:
                    result = RewriteLogicalBinaryExpression(node, stack);
                    break;
                // ArrayLength
                case ExpressionType.ArrayLength:
                    result = RewriteUnaryExpression(node, stack);
                    break;
                // ArrayIndex
                case ExpressionType.ArrayIndex:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // Call
                case ExpressionType.Call:
                    result = RewriteMethodCallExpression(node, stack);
                    break;
                // Coalesce
                case ExpressionType.Coalesce:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // Conditional
                case ExpressionType.Conditional:
                    result = RewriteConditionalExpression(node, stack);
                    break;
                // Constant
                case ExpressionType.Constant:
                    result = RewriteConstantExpression(node, stack);
                    break;
                // Convert
                case ExpressionType.Convert:
                    result = RewriteUnaryExpression(node, stack);
                    break;
                // ConvertChecked
                case ExpressionType.ConvertChecked:
                    result = RewriteUnaryExpression(node, stack);
                    break;
                // Divide
                case ExpressionType.Divide:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // Equal
                case ExpressionType.Equal:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // ExclusiveOr
                case ExpressionType.ExclusiveOr:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // GreaterThan
                case ExpressionType.GreaterThan:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // GreaterThanOrEqual
                case ExpressionType.GreaterThanOrEqual:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // Invoke
                case ExpressionType.Invoke:
                    result = RewriteInvocationExpression(node, stack);
                    break;
                // Lambda
                case ExpressionType.Lambda:
                    result = RewriteLambdaExpression(node, stack);
                    break;
                // LeftShift
                case ExpressionType.LeftShift:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // LessThan
                case ExpressionType.LessThan:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // LessThanOrEqual
                case ExpressionType.LessThanOrEqual:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // ListInit
                case ExpressionType.ListInit:
                    result = RewriteListInitExpression(node, stack);
                    break;
                // MemberAccess
                case ExpressionType.MemberAccess:
                    result = RewriteMemberExpression(node, stack);
                    break;
                // MemberInit
                case ExpressionType.MemberInit:
                    result = RewriteMemberInitExpression(node, stack);
                    break;
                // Modulo
                case ExpressionType.Modulo:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // Multiply
                case ExpressionType.Multiply:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // MultiplyChecked
                case ExpressionType.MultiplyChecked:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // Negate
                case ExpressionType.Negate:
                    result = RewriteUnaryExpression(node, stack);
                    break;
                // UnaryPlus
                case ExpressionType.UnaryPlus:
                    result = RewriteUnaryExpression(node, stack);
                    break;
                // NegateChecked
                case ExpressionType.NegateChecked:
                    result = RewriteUnaryExpression(node, stack);
                    break;
                // New
                case ExpressionType.New:
                    result = RewriteNewExpression(node, stack);
                    break;
                // NewArrayInit
                case ExpressionType.NewArrayInit:
                    result = RewriteNewArrayExpression(node, stack);
                    break;
                // NewArrayBounds
                case ExpressionType.NewArrayBounds:
                    result = RewriteNewArrayExpression(node, stack);
                    break;
                // Not
                case ExpressionType.Not:
                    result = RewriteUnaryExpression(node, stack);
                    break;
                // NotEqual
                case ExpressionType.NotEqual:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // Or
                case ExpressionType.Or:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // OrElse
                case ExpressionType.OrElse:
                    result = RewriteLogicalBinaryExpression(node, stack);
                    break;
                // Parameter
                case ExpressionType.Parameter:
                    result = RewriteParameterExpression(node, stack);
                    break;
                // Power
                case ExpressionType.Power:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // Quote
                case ExpressionType.Quote:
                    result = RewriteUnaryExpression(node, stack);
                    break;
                // RightShift
                case ExpressionType.RightShift:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // Subtract
                case ExpressionType.Subtract:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // SubtractChecked
                case ExpressionType.SubtractChecked:
                    result = RewriteBinaryExpression(node, stack);
                    break;
                // TypeAs
                case ExpressionType.TypeAs:
                    result = RewriteUnaryExpression(node, stack);
                    break;
                // TypeIs
                case ExpressionType.TypeIs:
                    result = RewriteTypeBinaryExpression(node, stack);
                    break;
                // Assign
                case ExpressionType.Assign:
                    result = RewriteAssignmentExpression(node, stack);
                    break;
                // Block
                case ExpressionType.Block:
                    result = RewriteBlock(node, stack);
                    break;
                // DebugInfo
                case ExpressionType.DebugInfo:
                    result = RewriteDebugInfoExpression(node, stack);
                    break;
                // DoStatement
                case ExpressionType.DoStatement:
                    result = RewriteDoStatement(node, stack);
                    break;
                // Dynamic
                case ExpressionType.Dynamic:
                    result = RewriteDynamicExpression(node, stack);
                    break;
                // EmptyStatement
                case ExpressionType.EmptyStatement:
                    result = RewriteEmptyStatement(node, stack);
                    break;
                // Extension
                case ExpressionType.Extension:
                    result = RewriteExtensionExpression(node, stack);
                    break;
                // Goto
                case ExpressionType.Goto:
                    result = RewriteGotoExpression(node, stack);
                    break;
                // Index
                case ExpressionType.Index:
                    result = RewriteIndexExpression(node, stack);
                    break;
                // Label
                case ExpressionType.Label:
                    result = RewriteLabelExpression(node, stack);
                    break;
                // LocalScope
                case ExpressionType.LocalScope:
                    result = RewriteLocalScopeExpression(node, stack);
                    break;
                // LoopStatement
                case ExpressionType.LoopStatement:
                    result = RewriteLoopStatement(node, stack);
                    break;
                // ReturnStatement
                case ExpressionType.ReturnStatement:
                    result = RewriteReturnStatement(node, stack);
                    break;
                // SwitchStatement
                case ExpressionType.SwitchStatement:
                    result = RewriteSwitchStatement(node, stack);
                    break;
                // ThrowStatement
                case ExpressionType.ThrowStatement:
                    result = RewriteThrowStatement(node, stack);
                    break;
                // TryStatement
                case ExpressionType.TryStatement:
                    result = RewriteTryStatement(node, stack);
                    break;
                // Unbox
                case ExpressionType.Unbox:
                    result = RewriteUnaryExpression(node, stack);
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

