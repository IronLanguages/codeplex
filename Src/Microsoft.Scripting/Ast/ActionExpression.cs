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
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {

        public static Expression Operator(SourceSpan span, ActionBinder binder, Operators op, Type result, params Expression[] arguments) {
            return Operator(Expression.Annotate(span), binder, op, result, arguments);
        }

        public static Expression DeleteMember(SourceSpan span, ActionBinder binder, string name, params Expression[] arguments) {
            return DeleteMember(Expression.Annotate(span), binder, name, arguments);
        }

        /// <summary>
        /// Creates ActionExpression representing OldDoOperationAction.
        /// </summary>
        /// <param name="binder">The binder responsible for binding the dynamic operation.</param>
        /// <param name="op">The operation to perform</param>
        /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
        /// <param name="arguments">Array of arguments for the action expression</param>
        /// <returns>New instance of the ActionExpression</returns>
        public static Expression Operator(ActionBinder binder, Operators op, Type result, params Expression[] arguments) {
            return Operator(Annotations.Empty, binder, op, result, arguments);
        }

        public static Expression Operator(Annotations annotations, ActionBinder binder, Operators op, Type resultType, params Expression[] arguments) {
            OldDoOperationAction bindingInfo = OldDoOperationAction.Make(binder, op);

            if (arguments.Length > 0 && !TypeUtils.CanAssign(typeof(CodeContext), arguments[0].Type)) {
                Expression result = null;

                // map operator onto UnaryExpression/BinaryExpression where possible
                if (arguments.Length == 1) {
                    result = GetUnaryOperator(annotations, resultType, bindingInfo, arguments[0]);
                } else if (arguments.Length == 2) {
                    result = GetBinaryOperator(annotations, resultType, bindingInfo, arguments[0], arguments[1]);
                } else if (arguments.Length == 3 && op == Operators.SetItem) {
                    result = Expression.AssignArrayIndex(arguments[0], arguments[1], arguments[2], resultType, bindingInfo, annotations);
                }

                if (result != null) {
                    return result;
                }
            }

            // otherwise, return a custom action
            return Expression.ActionExpression(bindingInfo, resultType, annotations, arguments);
        }

        private static Expression GetBinaryOperator(Annotations annotations, Type result, OldDoOperationAction bindingInfo, Expression left, Expression right) {
            switch (bindingInfo.Operation) {
                case Operators.GetItem:
                    return Expression.ArrayIndex(annotations, left, right, result, bindingInfo);
                case Operators.Equals:
                    return Expression.Equal(annotations, left, right, result, bindingInfo);
                case Operators.NotEquals:
                    return Expression.NotEqual(annotations, left, right, result, bindingInfo);
                case Operators.GreaterThan:
                    return Expression.GreaterThan(annotations, left, right, result, bindingInfo);
                case Operators.LessThan:
                    return Expression.LessThan(annotations, left, right, result, bindingInfo);
                case Operators.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(annotations, left, right, result, bindingInfo);
                case Operators.LessThanOrEqual:
                    return Expression.LessThanOrEqual(annotations, left, right, result, bindingInfo);
                case Operators.Add:
                    return Expression.Add(annotations, left, right, result, bindingInfo);
                case Operators.Subtract:
                    return Expression.Subtract(annotations, left, right, result, bindingInfo);
                case Operators.Multiply:
                    return Expression.Multiply(annotations, left, right, result, bindingInfo);
                case Operators.Divide:
                    return Expression.Divide(annotations, left, right, result, bindingInfo);
                case Operators.Mod:
                    return Expression.Modulo(annotations, left, right, result, bindingInfo);
                case Operators.LeftShift:
                    return Expression.LeftShift(annotations, left, right, result, bindingInfo);
                case Operators.RightShift:
                    return Expression.RightShift(annotations, left, right, result, bindingInfo);
                case Operators.BitwiseAnd:
                    return Expression.And(annotations, left, right, result, bindingInfo);
                case Operators.BitwiseOr:
                    return Expression.Or(annotations, left, right, result, bindingInfo);
                case Operators.ExclusiveOr:
                    return Expression.ExclusiveOr(annotations, left, right, result, bindingInfo);
                // TODO: case Operators.Power
                default:
                    return null;
            }
        }

        private static Expression GetUnaryOperator(Annotations annotations, Type result, OldDoOperationAction bindingInfo, Expression operand) {
            switch (bindingInfo.Operation) {
                case Operators.Not:
                    return Expression.Not(annotations, operand, result, bindingInfo);
                case Operators.OnesComplement:
                    return Expression.OnesComplement(annotations, operand, result, bindingInfo);
                case Operators.Negate:
                    return Expression.Negate(annotations, operand, result, bindingInfo);
                // TODO: case Operators.Positive:
                default:
                    return null;
            }
        }


        /// <summary>
        /// Creates ActionExpression representing a GetMember action.
        /// </summary>
        /// <param name="binder">The binder responsible for binding the dynamic operation.</param>
        /// <param name="name">The qualifier.</param>
        /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
        /// <param name="expression">the instance expression</param>
        /// <returns>New instance of the ActionExpression</returns>
        public static MemberExpression GetMember(ActionBinder binder, string name, Type result, Expression expression) {
            return Expression.GetMember(expression, result, OldGetMemberAction.Make(binder, name));
        }

        public static Expression GetMember(ActionBinder binder, string name, Type result, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            if (arguments.Length == 1) {
                return GetMember(binder, name, result, arguments[0]);
            }

            return Expression.ActionExpression(OldGetMemberAction.Make(binder, name), result, arguments);
        }

        /// <summary>
        /// Creates ActionExpression representing a GetMember action.
        /// </summary>
        /// <param name="binder">The binder responsible for binding the dynamic operation.</param>
        /// <param name="name">The qualifier.</param>
        /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
        /// <param name="expression">the instance expression</param>
        /// <param name="getMemberFlags">The binding flags for the get operation</param>
        /// <returns>New instance of the ActionExpression</returns>
        public static MemberExpression GetMember(ActionBinder binder, string name, GetMemberBindingFlags getMemberFlags, Type result, Expression expression) {
            return Expression.GetMember(expression, result, OldGetMemberAction.Make(binder, name, getMemberFlags));
        }

        public static Expression GetMember(ActionBinder binder, string name, GetMemberBindingFlags getMemberFlags, Type result, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            if (arguments.Length == 1) {
                return GetMember(binder, name, getMemberFlags, result, arguments[0]);
            }

            return Expression.ActionExpression(OldGetMemberAction.Make(binder, name, getMemberFlags), result, arguments);
        }

        /// <summary>
        /// Creates ActionExpression representing a SetMember action.
        /// </summary>
        /// <param name="binder">The binder responsible for binding the dynamic operation.</param>
        /// <param name="name">The qualifier.</param>
        /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
        /// <param name="expression">Target of the set member expression</param>
        /// <param name="value">The value to set</param>
        /// <returns>New instance of the ActionExpression</returns>
        public static AssignmentExpression SetMember(ActionBinder binder, string name, Type result, Expression expression, Expression value) {
            return SetMember(Annotations.Empty, binder, name, result, expression, value);
        }

        public static Expression SetMember(ActionBinder binder, string name, Type result, params Expression[] arguments) {
            return SetMember(Annotations.Empty, binder, name, result, arguments);
        }

        private static Expression SetMember(Annotations annotations, ActionBinder binder, string name, Type result, Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length >= 2, "arguments");

            if (arguments.Length == 2) {
                return SetMember(annotations, binder, name, result, arguments[0], arguments[1]);
            }

            return Expression.ActionExpression(OldSetMemberAction.Make(binder, name), result, arguments);
        }

        public static AssignmentExpression SetMember(Annotations annotations, ActionBinder binder, string name, Type result, Expression expression, Expression value) {
            return Expression.SetMember(expression, result, OldSetMemberAction.Make(binder, name), value, annotations);
        }

        public static Expression DeleteMember(ActionBinder binder, string name, params Expression[] arguments) {
            return DeleteMember(Annotations.Empty, binder, name, arguments);
        }

        public static DeleteExpression DeleteMember(ActionBinder binder, string name, Expression expression) {
            return DeleteMember(Annotations.Empty, binder, name, expression);
        }

        public static DeleteExpression DeleteMember(Annotations annotations, ActionBinder binder, string name, Expression expression) {
            return Expression.DeleteMember(expression, OldDeleteMemberAction.Make(binder, name), annotations);
        }

        public static Expression DeleteMember(Annotations annotations, ActionBinder binder, string name, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length >= 1, "arguments");

            if (arguments.Length == 1) {
                return DeleteMember(annotations, binder, name, arguments[0]);
            }

            return Expression.ActionExpression(OldDeleteMemberAction.Make(binder, name), typeof(object), annotations, arguments);
        }

        public static Expression InvokeMember(OldInvokeMemberAction action, Type result, params Expression[] arguments) {
            ContractUtils.RequiresNotNullItems(arguments, "arguments");
            ContractUtils.RequiresNotNull(arguments.Length > 0, "arguments");

            if (TypeUtils.CanAssign(typeof(CodeContext), arguments[0].Type)) {
                return Expression.ActionExpression(action, result, Annotations.Empty, arguments);
            } else {
                return Expression.Call(result, arguments[0], action, ArrayUtils.RemoveFirst(arguments));
            }
        }

        public static Expression InvokeMember(ActionBinder binder, string name, Type result, CallSignature signature,
            params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");

            if (arguments.Length > 0 && arguments[0] != null && TypeUtils.CanAssign(typeof(CodeContext), arguments[0].Type)) {
                return Expression.ActionExpression(
                    OldInvokeMemberAction.Make(binder, name, signature),
                    result,
                    arguments
                );
            } else {
                return Expression.Call(
                    result,
                    arguments[0],
                    OldInvokeMemberAction.Make(binder, name, signature),
                    ArrayUtils.RemoveFirst(arguments)
                );
            }
        }

        // TODO: This helper should go. It does too much number magic.
        public static Expression Call(ActionBinder binder, Type result, params Expression[] arguments) {
            return Call(OldCallAction.Make(binder, arguments.Length - 2), result, arguments);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static Expression Call(Annotations annotations, OldCallAction action, Type result, params Expression[] arguments) {
            return Expression.ActionExpression(action, result, annotations, arguments);
        }

        /// <summary>
        /// Creates ActionExpression representing a Call action.
        /// </summary>
        /// <param name="action">The call action to perform.</param>
        /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
        /// <param name="arguments">Array of arguments for the action expression</param>
        /// <returns>New instance of the ActionExpression</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static Expression Call(OldCallAction action, Type result, params Expression[] arguments) {
            return Call(Annotations.Empty, action, result, arguments);
        }


        /// <summary>
        /// Creates ActionExpression representing a CreateInstance action.
        /// </summary>
        /// <param name="action">The create instance action to perform.</param>
        /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
        /// <param name="arguments">Array of arguments for the action expression</param>
        /// <returns>New instance of the ActionExpression</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static NewExpression Create(OldCreateInstanceAction action, Type result, params Expression[] arguments) {
            return Expression.New(result, action, arguments);
        }

        // TODO: This helper should go. It does too much number magic.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static NewExpression Create(ActionBinder binder, Type result, params Expression[] arguments) {
            return Expression.New(result, OldCreateInstanceAction.Make(binder, arguments.Length - 2), arguments);
        }

        /// <summary>
        /// Creates a new ActionExpression which performs a conversion.
        /// </summary>
        public static UnaryExpression ConvertTo(CallSiteBinder action, Expression argument, Type type) {
            ContractUtils.RequiresNotNull(action, "action");
            ContractUtils.RequiresNotNull(argument, "argument");
            ContractUtils.RequiresNotNull(type, "type");

            return Expression.Convert(argument, type, action, Annotations.Empty);
        }

        /// <summary>
        /// Creates a new ActionExpression which performs the specified conversion and returns the strongly typed result.
        /// </summary>
        public static UnaryExpression ConvertTo(OldConvertToAction action, Expression argument) {
            return ConvertTo(action, argument, action.ToType);
        }

        public static Expression ConvertTo(OldConvertToAction action, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            if (arguments.Length == 1) {
                return ConvertTo(action, arguments[0]);
            }

            return Expression.ActionExpression(action, action.ToType, arguments);
        }

        /// <summary>
        /// Creates a new ActionExpression which performs the specified conversion to the type.  The ActionExpression
        /// is strongly typed to the provided type.
        /// </summary>
        public static UnaryExpression ConvertTo(ActionBinder binder, Type toType, Expression argument) {
            ContractUtils.RequiresNotNull(toType, "toType");

            return ConvertTo(OldConvertToAction.Make(binder, toType), argument);
        }

        public static Expression ConvertTo(ActionBinder binder, Type toType, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(toType, "toType");
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            if (arguments.Length == 1) {
                return ConvertTo(binder, toType, arguments[0]);
            }

            return Expression.ActionExpression(OldConvertToAction.Make(binder, toType), toType, arguments);
        }

        /// <summary>
        /// Creates a new ActionExpressoin which performs the specified conversion to the type.  The ActionExpress
        /// is strongly typed to the converted type.
        /// </summary>
        public static UnaryExpression ConvertTo(ActionBinder binder, Type toType, ConversionResultKind kind, Expression argument) {
            ContractUtils.RequiresNotNull(toType, "toType");

            return ConvertTo(OldConvertToAction.Make(binder, toType, kind), argument);
        }

        /// <summary>
        /// Creates a new ActionExpressoin which performs the specified conversion to the type.  The ActionExpress
        /// is strongly typed to the converted type.
        /// </summary>
        public static Expression ConvertTo(ActionBinder binder, Type toType, ConversionResultKind kind, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(toType, "toType");
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            if (arguments.Length == 1) {
                return ConvertTo(binder, toType, kind, arguments[0]);
            }

            return Expression.ActionExpression(OldConvertToAction.Make(binder, toType, kind), toType, arguments);
        }

        /// <summary>
        /// Creates a new ActionExpression which performs the conversion to the specified type with the 
        /// specified conversion kind.  The ActionExpression is strongly typed to the provided
        /// actionExpressionType.
        /// </summary>
        /// <param name="binder">The binder responsible for binding the dynamic operation.</param>
        /// <param name="toType">The type to convert to</param>
        /// <param name="actionExpressionType">The return type for the ActionExpression (should be assignable from the toType)</param>
        /// <param name="kind">The kind of conversion to preform</param>
        /// <param name="argument">The argument to be converted</param>
        public static UnaryExpression ConvertTo(ActionBinder binder, Type toType, ConversionResultKind kind, Expression argument, Type actionExpressionType) {
            ContractUtils.RequiresNotNull(toType, "toType");

            return ConvertTo(OldConvertToAction.Make(binder, toType, kind), argument, actionExpressionType);
        }

        public static Expression ConvertTo(ActionBinder binder, Type toType, ConversionResultKind kind, Type actionExpressionType, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(toType, "toType");
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            if (arguments.Length == 1) {
                return ConvertTo(binder, toType, kind, arguments[0], actionExpressionType);
            }

            return Expression.ActionExpression(OldConvertToAction.Make(binder, toType, kind), actionExpressionType, arguments);
        }
    }
}
