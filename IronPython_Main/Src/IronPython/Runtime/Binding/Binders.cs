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
using System.Text;

using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    using Microsoft.Scripting.Actions;

    static class Binders {
        internal static bool UseNewSites = false;

        public static Expression/*!*/ Invoke(BinderState/*!*/ binder, Type/*!*/ resultType, CallSignature signature, params Expression/*!*/[]/*!*/ args) {
            if (UseNewSites) {
                return Ast.ActionExpression(
                    new InvokeBinder(
                        binder,
                        signature
                    ),
                    resultType,
                    args
                );
            } else {
                return Ast.ActionExpression(
                    OldCallAction.Make(
                        binder.Binder,
                        signature
                    ),
                    resultType,
                    ArrayUtils.Insert(Ast.CodeContext(), args)
                );
            }
        }

        public static Expression/*!*/ InvokeWithContext(BinderState/*!*/ binder, Type/*!*/ resultType, CallSignature signature, params Expression/*!*/[]/*!*/ args) {
            if (UseNewSites) {
                return Ast.ActionExpression(
                    new InvokeWithContextBinder(
                        binder,
                        signature
                    ),
                    resultType,
                    args
                );
            } else {
                return Ast.ActionExpression(
                    OldCallAction.Make(
                        binder.Binder,
                        signature
                    ),
                    resultType,
                    args            // context is already present
                );
            }
        }

        public static Expression/*!*/ Convert(BinderState/*!*/ binder, Type/*!*/ type, ConversionResultKind resultKind, Expression/*!*/ target) {
            if (UseNewSites) {
                return Ast.ActionExpression(
                    new ConversionBinder(
                        binder,
                        type,
                        resultKind
                    ),
                    type,
                    target
                );
            } else {
                return Ast.ActionExpression(
                    OldConvertToAction.Make(
                        binder.Binder,
                        type
                    ),
                    type,
                    Ast.CodeContext(),
                    target
                );
            }
        }

        /// <summary>
        /// Backwards compatible Convert for the old sites that need to flow CodeContext
        /// </summary>
        public static Expression/*!*/ Convert(Expression/*!*/ codeContext, BinderState/*!*/ binder, Type/*!*/ type, ConversionResultKind resultKind, Expression/*!*/ target) {
            if (UseNewSites) {
                return Ast.ActionExpression(
                    new ConversionBinder(
                        binder,
                        type,
                        resultKind
                    ),
                    type,
                    target
                );
            } else {
                return Ast.ActionExpression(
                    OldConvertToAction.Make(
                        binder.Binder,
                        type
                    ),
                    type,
                    codeContext,
                    target
                );
            }
        }

        public static Expression/*!*/ Operation(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ operation, params Expression[] args) {
            if (UseNewSites) {
                return Ast.ActionExpression(
                    new OperationBinder(
                        binder,
                        operation
                    ),
                    resultType,
                    args
                );
            } else {
                return Ast.ActionExpression(
                    OldDoOperationAction.Make(
                        binder.Binder,
                        StandardOperators.ToOperator(operation)
                    ),
                    resultType,
                    ArrayUtils.Insert(Ast.CodeContext(), args)
                );
            }
        }

        public static Expression/*!*/ Set(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target, Expression/*!*/ value) {
            if (UseNewSites) {
                return Ast.ActionExpression(
                    new SetMemberBinder(
                        binder,
                        name
                    ),
                    resultType,
                    target,
                    value
                );
            } else {
                return Ast.ActionExpression(
                    OldSetMemberAction.Make(
                        binder.Binder,
                        name
                    ),
                    resultType,
                    Ast.CodeContext(),
                    target,
                    value
                );
            }
        }

        public static Expression/*!*/ Get(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            if (UseNewSites) {
                return Ast.ActionExpression(
                    new GetMemberBinder(
                        binder,
                        name
                    ),
                    resultType,
                    target
                );
            } else {
                return Ast.ActionExpression(
                    OldGetMemberAction.Make(
                        binder.Binder,
                        name
                    ),
                    resultType,
                    Ast.CodeContext(),
                    target
                );
            }
        }

        public static Expression/*!*/ TryGet(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            if (UseNewSites) {
                return Ast.ActionExpression(
                    new GetMemberBinder(
                        binder,
                        name,
                        true
                    ),
                    resultType,
                    target
                );
            } else {
                return Ast.ActionExpression(
                    OldGetMemberAction.Make(
                        binder.Binder,
                        name,
                        GetMemberBindingFlags.NoThrow
                    ),
                    resultType,
                    Ast.CodeContext(),
                    target
                );
            }
        }

        public static Expression/*!*/ Delete(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            if (UseNewSites) {
                return Ast.ActionExpression(
                    new DeleteMemberBinder(
                        binder,
                        name
                    ),
                    resultType,
                    target
                );
            } else {
                return Ast.ActionExpression(
                    OldDeleteMemberAction.Make(
                        binder.Binder,
                        name
                    ),                    
                    resultType,
                    Ast.CodeContext(),
                    target
                );
            }
        }

        public static MetaAction/*!*/ BinaryOperationRetBool(BinderState/*!*/ state, string operatorName) {
            return new ComboBinder(
                new BinderMappingInfo(
                    new OperationBinder(
                        state,
                        operatorName
                    ),
                    ParameterMappingInfo.Parameter(0),
                    ParameterMappingInfo.Parameter(1)
                ),
                new BinderMappingInfo(
                    new ConversionBinder(state, typeof(bool), ConversionResultKind.ExplicitCast),
                    ParameterMappingInfo.Action(0)
                )
            );
        }

        /// <summary>
        /// Creates a new InvokeBinder which will call with positional splatting.
        /// 
        /// The signature of the target site should be object(function), object[], retType
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static InvokeBinder/*!*/ InvokeSplat(BinderState/*!*/ state) {
            return new InvokeBinder(
                state,
                new CallSignature(new ArgumentInfo(ArgumentKind.List))
            );
        }

        /// <summary>
        /// Creates a new InvokeBinder which will call with positional and keyword splatting.
        /// 
        /// The signature of the target site should be object(function), object[], dictionary, retType
        /// </summary>
        public static InvokeBinder/*!*/ InvokeKeywords(BinderState/*!*/ state) {
            return new InvokeBinder(
                state,
                new CallSignature(new ArgumentInfo(ArgumentKind.List), new ArgumentInfo(ArgumentKind.Dictionary))
            );
        }
    }
}
