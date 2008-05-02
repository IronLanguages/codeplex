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
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    // TODO: remove
    public sealed class ActionExpression : Expression {
        private readonly ReadOnlyCollection<Expression>/*!*/ _arguments;

        internal ActionExpression(Annotations annotations, DoOperationAction/*!*/ action, ReadOnlyCollection<Expression>/*!*/ arguments, Type/*!*/ result)
            : base(annotations, AstNodeType.ActionExpression, result, action) {
            _arguments = arguments;
        }

        new public DynamicAction Action {
            get { return BindingInfo; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }
    }

    public partial class Expression {
        // TODO: move these factories to Microsoft.Scripting, except for one
        // factory for custom actions which should move to Expression
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public static class Action {
            /// <summary>
            /// Creates ActionExpression representing DoOperationAction.
            /// </summary>
            /// <param name="binder">The binder responsible for binding the dynamic operation.</param>
            /// <param name="op">The operation to perform</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static Expression Operator(ActionBinder binder, Operators op, Type result, params Expression[] arguments) {
                return Operator(Annotations.Empty, binder, op, result, arguments);
            }

            public static Expression Operator(SourceSpan span, ActionBinder binder, Operators op, Type result, params Expression[] arguments) {
                return Operator(Annotate(span), binder, op, result, arguments);
            }
           
            public static Expression Operator(Annotations annotations, ActionBinder binder, Operators op, Type resultType, params Expression[] arguments) {
                DoOperationAction bindingInfo = DoOperationAction.Make(binder, op);
                Expression result = null;

                // map operator onto UnaryExpression/BinaryExpression where possible
                if (arguments.Length == 1) {
                    result = GetUnaryOperator(annotations, resultType, bindingInfo, arguments[0]);                    
                } else if (arguments.Length == 2) {
                    result = GetBinaryOperator(annotations, resultType, bindingInfo, arguments[0], arguments[1]);
                } else if (arguments.Length == 3 && op == Operators.SetItem) {
                    result = Expression.AssignArrayIndex(annotations, arguments[0], arguments[1], arguments[2], resultType, bindingInfo);
                }

                if (result != null) {
                    return result;
                }

                // otherwise, return a custom action
                return ActionExpression(annotations, bindingInfo, arguments, resultType);
            }

            private static Expression GetBinaryOperator(Annotations annotations, Type result, DoOperationAction bindingInfo, Expression left, Expression right) {
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
                        return Expression.GreaterThanEquals(annotations, left, right, result, bindingInfo);
                    case Operators.LessThanOrEqual:
                        return Expression.LessThanEquals(annotations, left, right, result, bindingInfo);
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

            private static Expression GetUnaryOperator(Annotations annotations, Type result, DoOperationAction bindingInfo, Expression operand) {
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
            public static MemberExpression GetMember(ActionBinder binder, SymbolId name, Type result, Expression expression) {
                return Expression.GetMember(expression, result, GetMemberAction.Make(binder, name));
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
            public static MemberExpression GetMember(ActionBinder binder, SymbolId name, GetMemberBindingFlags getMemberFlags, Type result, Expression expression) {
                return Expression.GetMember(expression, result, GetMemberAction.Make(binder, name, getMemberFlags));
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
            public static AssignmentExpression SetMember(ActionBinder binder, SymbolId name, Type result, Expression expression, Expression value) {
                return SetMember(Annotations.Empty, binder, name, result, expression, value);
            }

            public static AssignmentExpression SetMember(SourceSpan span, ActionBinder binder, SymbolId name, Type result, Expression expression, Expression value) {
                return SetMember(Annotate(span), binder, name, result, expression, value);
            }

            public static AssignmentExpression SetMember(Annotations annotations, ActionBinder binder, SymbolId name, Type result, Expression expression, Expression value) {
                return Expression.SetMember(annotations, expression, result, SetMemberAction.Make(binder, name), value);
            }


            public static DeleteStatement DeleteMember(ActionBinder binder, SymbolId name, Expression expression) {
                return DeleteMember(Annotations.Empty, binder, name, expression);
            }

            public static DeleteStatement DeleteMember(SourceSpan span, ActionBinder binder, SymbolId name, Expression expression) {
                return DeleteMember(Annotate(span), binder, name, expression);
            }

            public static DeleteStatement DeleteMember(Annotations annotations, ActionBinder binder, SymbolId name, Expression expression) {
                return Expression.DeleteMember(annotations, expression, DeleteMemberAction.Make(binder, name));
            }

            public static MethodCallExpression InvokeMember(ActionBinder binder, SymbolId name, Type result, InvokeMemberActionFlags flags, CallSignature signature,
                params Expression[] arguments) {

                return Expression.Call(
                    result,
                    arguments[0],
                    InvokeMemberAction.Make(binder, name, flags, signature),
                    ArrayUtils.RemoveFirst(arguments)
                );
            }

            public static InvocationExpression Call(ActionBinder binder, Type result, params Expression[] arguments) {
                return Call(CallAction.Make(binder, arguments.Length - 1), result, arguments);
            }

            public static InvocationExpression Call(SourceSpan span, ActionBinder binder, Type result, params Expression[] arguments) {
                return Call(Annotate(span), CallAction.Make(binder, arguments.Length - 1), result, arguments);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
            public static InvocationExpression Call(Annotations annotations, CallAction action, Type result, params Expression[] arguments) {
                return Expression.Invoke(annotations, result, arguments[0], action, ArrayUtils.RemoveFirst(arguments));
            }

            /// <summary>
            /// Creates ActionExpression representing a Call action.
            /// </summary>
            /// <param name="action">The call action to perform.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
            public static InvocationExpression Call(CallAction action, Type result, params Expression[] arguments) {
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
            public static NewExpression Create(CreateInstanceAction action, Type result, params Expression[] arguments) {
                return Expression.New(result, action, arguments);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
            public static NewExpression Create(ActionBinder binder, Type result, params Expression[] arguments) {
                return Expression.New(result, CreateInstanceAction.Make(binder, arguments.Length - 1), arguments);
            }

            /// <summary>
            /// Creates a new ActionExpression which performs a conversion.
            /// </summary>
            public static UnaryExpression ConvertTo(ConvertToAction action, Expression argument, Type type) {
                Utils.ContractUtils.RequiresNotNull(action, "action");
                Utils.ContractUtils.RequiresNotNull(argument, "argument");
                Utils.ContractUtils.RequiresNotNull(type, "type");

                return Expression.Convert(argument, type, action);
            }

            /// <summary>
            /// Creates a new ActionExpression which performs the specified conversion and returns the strongly typed result.
            /// </summary>
            public static UnaryExpression ConvertTo(ConvertToAction action, Expression argument) {
                return ConvertTo(action, argument, action.ToType);
            }

            /// <summary>
            /// Creates a new ActionExpression which performs the specified conversion to the type.  The ActionExpression
            /// is strongly typed to the provided type.
            /// </summary>
            public static UnaryExpression ConvertTo(ActionBinder binder, Type toType, Expression argument) {
                Utils.ContractUtils.RequiresNotNull(toType, "toType");

                return ConvertTo(ConvertToAction.Make(binder, toType), argument);
            }

            /// <summary>
            /// Creates a new ActionExpressoin which performs the specified conversion to the type.  The ActionExpress
            /// is strongly typed to the converted type.
            /// </summary>
            public static UnaryExpression ConvertTo(ActionBinder binder, Type toType, ConversionResultKind kind, Expression argument) {
                Utils.ContractUtils.RequiresNotNull(toType, "toType");

                return ConvertTo(ConvertToAction.Make(binder, toType, kind), argument);
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
                Utils.ContractUtils.RequiresNotNull(toType, "toType");

                return ConvertTo(ConvertToAction.Make(binder, toType, kind), argument, actionExpressionType);
            }


            internal static ActionExpression ActionExpression(DoOperationAction action, IList<Expression> arguments, Type result) {
                return ActionExpression(Annotations.Empty, action, arguments, result);
            }

            internal static ActionExpression ActionExpression(Annotations annotations, DoOperationAction action, IList<Expression> arguments, Type result) {
                ContractUtils.RequiresNotNull(action, "action");
                ContractUtils.RequiresNotNullItems(arguments, "arguments");
                ContractUtils.RequiresNotNull(result, "result");
                ContractUtils.Requires(result.IsVisible, "result", ResourceUtils.GetString(ResourceUtils.TypeMustBeVisible, result.FullName));

                return new ActionExpression(annotations, action, CollectionUtils.ToReadOnlyCollection(arguments), result);
            }
        }
    }
}
