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
using System.Linq.Expressions;
using System.Scripting;
using System.Scripting.Actions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
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
            return Expression.ActionExpression(OldDoOperationAction.Make(binder, op), resultType, annotations, arguments);
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
        public static Expression SetMember(ActionBinder binder, string name, Type result, Expression expression, Expression value) {
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

        public static Expression SetMember(Annotations annotations, ActionBinder binder, string name, Type result, Expression expression, Expression value) {
            return Expression.ActionExpression(OldSetMemberAction.Make(binder, name), result, annotations, expression, value);
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

        public static Expression InvokeMember(CallSiteBinder action, Type result, params Expression[] arguments) {
            ContractUtils.RequiresNotNullItems(arguments, "arguments");
            ContractUtils.RequiresNotNull(arguments.Length > 0, "arguments");

            return Expression.ActionExpression(action, result, Annotations.Empty, arguments);
        }

        public static Expression InvokeMember(ActionBinder binder, string name, Type result, CallSignature signature,
            params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");

            return Expression.ActionExpression(
                OldInvokeMemberAction.Make(binder, name, signature),
                result,
                arguments
            );
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
