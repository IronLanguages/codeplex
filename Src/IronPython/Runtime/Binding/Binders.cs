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
using System.Linq.Expressions;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using System.Text;

using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    static class Binders {
        public static Expression/*!*/ Invoke(BinderState/*!*/ binder, Type/*!*/ resultType, CallSignature signature, params Expression/*!*/[]/*!*/ args) {
            return Ast.ActionExpression(
                new InvokeBinder(
                    binder,
                    signature
                ),
                resultType,
                ArrayUtils.Insert(Ast.CodeContext(), args)
            );
        }

        public static Expression/*!*/ Convert(BinderState/*!*/ binder, Type/*!*/ type, ConversionResultKind resultKind, Expression/*!*/ target) {
            return Ast.ActionExpression(
                new ConversionBinder(
                    binder,
                    type,
                    resultKind
                ),
                type,
                target
            );
        }

        /// <summary>
        /// Backwards compatible Convert for the old sites that need to flow CodeContext
        /// </summary>
        public static Expression/*!*/ Convert(Expression/*!*/ codeContext, BinderState/*!*/ binder, Type/*!*/ type, ConversionResultKind resultKind, Expression/*!*/ target) {
            return Ast.ActionExpression(
                new ConversionBinder(
                    binder,
                    type,
                    resultKind
                ),
                type,
                target
            );
        }

        public static Expression/*!*/ Operation(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ operation, params Expression[] args) {
            return Ast.ActionExpression(
                new OperationBinder(
                    binder,
                    operation
                ),
                resultType,
                args
            );
        }

        public static Expression/*!*/ Set(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target, Expression/*!*/ value) {
            return Ast.ActionExpression(
                new SetMemberBinder(
                    binder,
                    name
                ),
                resultType,
                target,
                value
            );
        }

        public static Expression/*!*/ Get(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            return Get(Ast.CodeContext(), binder, resultType, name, target);
        }

        public static Expression/*!*/ Get(Expression/*!*/ codeContext, BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            return Ast.ActionExpression(
                new GetMemberBinder(
                    binder,
                    name
                ),
                resultType,
                target,
                codeContext
            );
        }

        public static Expression/*!*/ TryGet(Expression/*!*/ codeContext, BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            return Ast.ActionExpression(
                new GetMemberBinder(
                    binder,
                    name,
                    true
                ),
                resultType,
                target,
                codeContext
            );
        }

        public static Expression/*!*/ TryGet(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            return TryGet(Ast.CodeContext(), binder, resultType, name, target);
        }

        public static Expression/*!*/ Delete(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {        
            return Ast.ActionExpression(
                new DeleteMemberBinder(
                    binder,
                    name
                ),
                resultType,
                target
            );
        }

        public static MetaAction/*!*/ BinaryOperationRetBool(BinderState/*!*/ state, string operatorName) {
            return BinaryOperationRetType(state, operatorName, typeof(bool));
        }

        public static MetaAction/*!*/ BinaryOperationRetType(BinderState/*!*/ state, string operatorName, Type retType) {
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
                    new ConversionBinder(state, retType, ConversionResultKind.ExplicitCast),
                    ParameterMappingInfo.Action(0)
                )
            );
        }

        public static MetaAction/*!*/ InvokeAndConvert(BinderState/*!*/ state, int argCount, Type retType) {
            // +2 for the target object and CodeContext which InvokeBinder recevies
            ParameterMappingInfo[] args = new ParameterMappingInfo[argCount + 2];   
            for (int i = 0; i < argCount + 2; i++) {
                args[i] = ParameterMappingInfo.Parameter(i);
            }

            return new ComboBinder(
                new BinderMappingInfo(
                    new InvokeBinder(
                        state,
                        new CallSignature(argCount)
                    ),
                    args
                ),
                new BinderMappingInfo(
                    new ConversionBinder(state, retType, ConversionResultKind.ExplicitCast),
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
