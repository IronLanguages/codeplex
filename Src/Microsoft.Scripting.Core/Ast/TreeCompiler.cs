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

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {
    public static class TreeCompiler {
        public static T CompileExpression<T>(Expression expression) {
            ContractUtils.Requires(typeof(Delegate).IsAssignableFrom(typeof(T)), "T");
            ContractUtils.RequiresNotNull(expression, "expression");

            Expression body;
            if (expression.Type != typeof(void)) {
                body = Expression.Return(
                    expression
                );
            } else {
                body = Expression.Block(
                    expression,
                    Expression.Return()
                );
            }

            LambdaExpression lambda = Expression.Lambda(typeof(T),"<expression>", expression.Type, body, new ParameterExpression[0], new VariableExpression[0]);
            return CompileLambda<T>(lambda);
        }

        public static T CompileStatement<T>(Expression expression) {
            ContractUtils.Requires(typeof(Delegate).IsAssignableFrom(typeof(T)), "T");
            ContractUtils.RequiresNotNull(expression, "expression");

            Expression body = Expression.Block(
                expression,
                Expression.Return()
            );

            LambdaExpression lambda = Expression.Lambda(typeof(T), "<statement>", typeof(void), body, new ParameterExpression[0], new VariableExpression[0]);
            return CompileLambda<T>(lambda);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "lambda")]
        public static T CompileLambda<T>(LambdaExpression lambda) {
            ContractUtils.Requires(typeof(Delegate).IsAssignableFrom(typeof(T)), "T");
            ContractUtils.RequiresNotNull(lambda, "lambda");

            // Call compiler to create the delegate.
            return LambdaCompiler.CompileLambda<T>(lambda);
        }
    }
}
