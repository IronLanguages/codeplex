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
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Ast {
    public sealed class ActionExpression : Expression {
        private readonly ReadOnlyCollection<Expression>/*!*/ _arguments;
        private readonly DynamicAction/*!*/ _action;

        internal ActionExpression(DynamicAction/*!*/ action, ReadOnlyCollection<Expression>/*!*/ arguments, Type/*!*/ result)
            : base(AstNodeType.ActionExpression, result) {
            _action = action;
            _arguments = arguments;
        }

        public DynamicAction Action {
            get { return _action; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }
    }

    public static partial class Ast {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public static class Action {
            /// <summary>
            /// Creates ActionExpression representing DoOperationAction.
            /// </summary>
            /// <param name="op">The operation to perform</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression Operator(Operators op, Type result, params Expression[] arguments) {
                return ActionExpression(DoOperationAction.Make(op), arguments, result);
            }

            /// <summary>
            /// Creates ActionExpression representing a GetMember action.
            /// </summary>
            /// <param name="name">The qualifier.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression GetMember(SymbolId name, Type result, params Expression[] arguments) {
                return ActionExpression(GetMemberAction.Make(name), arguments, result);
            }

            /// <summary>
            /// Creates ActionExpression representing a GetMember action.
            /// </summary>
            /// <param name="name">The qualifier.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <param name="getMemberFlags">The binding flags for the get operation</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression GetMember(SymbolId name, GetMemberBindingFlags getMemberFlags, Type result, params Expression[] arguments) {
                return ActionExpression(GetMemberAction.Make(name, getMemberFlags), arguments, result);
            }

            /// <summary>
            /// Creates ActionExpression representing a SetMember action.
            /// </summary>
            /// <param name="name">The qualifier.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression SetMember(SymbolId name, Type result, params Expression[] arguments) {
                return ActionExpression(SetMemberAction.Make(name), arguments, result);
            }

            public static Expression DeleteMember(SymbolId name, params Expression[] arguments) {
                return ActionExpression(DeleteMemberAction.Make(name), arguments, typeof(object));
            }

            // TODO:
            public static ActionExpression InvokeMember(SymbolId name, Type result, InvokeMemberActionFlags flags, CallSignature signature, 
                params Expression[] arguments) {

                return ActionExpression(InvokeMemberAction.Make(name, flags, signature), arguments, result);
            }
       
            public static ActionExpression Call(Type result, params Expression[] arguments) {
                return Call(CallAction.Make(arguments.Length - 1), result, arguments);
            }

            /// <summary>
            /// Creates ActionExpression representing a Call action.
            /// </summary>
            /// <param name="action">The call action to perform.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
            public static ActionExpression Call(CallAction action, Type result, params Expression[] arguments) {
                return ActionExpression(action, arguments, result);
            }

            /// <summary>
            /// Creates ActionExpression representing a CreateInstance action.
            /// </summary>
            /// <param name="action">The create instance action to perform.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
            public static ActionExpression Create(CreateInstanceAction action, Type result, params Expression[] arguments) {
                return ActionExpression(action, arguments, result);
            }

            public static ActionExpression Create(Type result, params Expression[] arguments) {
                return ActionExpression(CreateInstanceAction.Make(arguments.Length - 1), arguments, result);
            }

            /// <summary>
            /// Creates a new ActionExpression which performs a conversion.
            /// </summary>
            public static ActionExpression ConvertTo(ConvertToAction action, Expression argument, Type type) {
                Utils.Contract.RequiresNotNull(action, "action");
                Utils.Contract.RequiresNotNull(argument, "argument");
                Utils.Contract.RequiresNotNull(type, "type");

                return ActionExpression(action, new Expression[] { argument }, type);
            }

            /// <summary>
            /// Creates a new ActionExpression which performs the specified conversion and returns the strongly typed result.
            /// </summary>
            public static ActionExpression ConvertTo(ConvertToAction action, Expression argument) {
                return ConvertTo(action, argument, action.ToType);
            }

            /// <summary>
            /// Creates a new ActionExpression which performs the specified conversion to the type.  The ActionExpression
            /// is strongly typed to the provided type.
            /// </summary>
            public static ActionExpression ConvertTo(Type toType, Expression argument) {
                Utils.Contract.RequiresNotNull(toType, "toType");

                return ConvertTo(ConvertToAction.Make(toType), argument);
            }

            /// <summary>
            /// Creates a new ActionExpressoin which performs the specified conversion to the type.  The ActionExpress
            /// is strongly typed to the converted type.
            /// </summary>
            public static ActionExpression ConvertTo(Type toType, ConversionResultKind kind, Expression argument) {
                Utils.Contract.RequiresNotNull(toType, "toType");

                return ConvertTo(ConvertToAction.Make(toType, kind), argument);
            }

            /// <summary>
            /// Creates a new ActionExpression which performs the conversion to the specified type with the 
            /// specified conversion kind.  The ActionExpression is strongly typed to the provided
            /// actionExpressionType.
            /// </summary>
            /// <param name="toType">The type to convert to</param>
            /// <param name="actionExpressionType">The return type for the ActionExpression (should be assignable from the toType)</param>
            /// <param name="kind">The kind of conversion to preform</param>
            /// <param name="argument">The argument to be converted</param>
            public static ActionExpression ConvertTo(Type toType, ConversionResultKind kind, Expression argument, Type actionExpressionType) {
                Utils.Contract.RequiresNotNull(toType, "toType");

                return ConvertTo(ConvertToAction.Make(toType, kind), argument, actionExpressionType);
            }


            internal static ActionExpression ActionExpression(DynamicAction action, IList<Expression> arguments, Type result) {
                Contract.RequiresNotNull(action, "action");
                Contract.RequiresNotNullItems(arguments, "arguments");
                Contract.RequiresNotNull(result, "result");
                Contract.Requires(result.IsVisible, "result", String.Format(Resources.TypeMustBeVisible, result.FullName));

                ValidateAction(action, arguments);

                return new ActionExpression(action, CollectionUtils.ToReadOnlyCollection(arguments), result);
            }

            private static void ValidateAction(DynamicAction action, IList<Expression> arguments) {
                switch (action.Kind) {
                    case DynamicActionKind.DoOperation:
                        // TODO: ValidateDoOperationAction((DoOperationAction)action, arguments, result);
                        break;
                    case DynamicActionKind.ConvertTo:
                        Contract.Requires(arguments.Count == 1, "arguments", "One argument required for convert action");
                        break;
                    case DynamicActionKind.GetMember:
                        Contract.Requires(arguments.Count == 1, "arguments", "One argument required for get member action");
                        break;
                    case DynamicActionKind.SetMember:
                        Contract.Requires(arguments.Count == 2, "arguments", "Two arguments required for set member action");
                        break;
                    case DynamicActionKind.DeleteMember:
                        Contract.Requires(arguments.Count == 1, "arguments", "One argument required for delete member action");
                        break;
                    case DynamicActionKind.InvokeMember:
                    case DynamicActionKind.Call:
                    case DynamicActionKind.CreateInstance:
                        break;
                    default:
                        throw new ArgumentException("Invalid action kind", "action");
                }
            }
        }
    }
}
