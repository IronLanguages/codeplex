/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !SILVERLIGHT3
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Reflection;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    public static partial class Scenarios {
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Compare type constants", new string[] { "FullTrustOnly" })]
        public static void Positive_CompareTypeConstants(EU.IValidator V) {
            var l = Expression.Lambda<Func<bool>>(
#if SILVERLIGHT
                Expression.Equal(Expression.Constant(typeof(string), typeof(Type)), Expression.Constant(typeof(string), typeof(Type)))
#else
                Expression.Equal(Expression.Constant(typeof(string)), Expression.Constant(typeof(string)))
#endif
            );
            
            V.Validate(l, d =>
            {
                EU.Equal(d(), true);
            });
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Use constant of the RuntimeMetdhodInfo type", new string[] { "FullTrustOnly" })]
        public static void Positive_RuntimeMethodInfoConstant(EU.IValidator V) {
            Type rmi = typeof(object).Assembly.GetType("System.Reflection.RuntimeMethodInfo");
            var method = rmi.GetMethod("get_BindingFlags", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) {
                throw new InvalidOperationException("internal BindingFlags on RuntimeMethodInfo no longer exists, change the test");
            }

            var l = Expression.Lambda<Func<BindingFlags>>(
                Expression.Call(Expression.Constant(typeof(object).GetMethod("ToString")), method)
            );
            
            V.Validate(l, d =>
            {
                EU.Equal(d(), BindingFlags.Public | BindingFlags.Instance);
            });
        }

        public static void Negative_CompileToMethodConstantsAsVoid(EU.IValidator V) {
            // Regression test: live constants that were emitted as void
            // weren't being correctly detected by the compiler, leading to bad
            // generated code.

            // It is fine if this works or throws a compile error, but it
            // shouldn't lead to bad code gen.

            var lambda = Expression.Lambda<Action>(
                Expression.Call(
                    Expression.Lambda<Action>(
                        Expression.Constant(new object())
                    ),
                    "Invoke",
                    null
                )
            );

            EU.Throws<InvalidOperationException>(() => ETUtils.CompileAsMethodUtils.CompileAsMethod(lambda));
        }

        public static void Positive_LambdaReuseWithCachedConstants(EU.IValidator V) {
            var v1 = Expression.Variable(typeof(object));
            var v2 = Expression.Variable(typeof(object));
            var v3 = Expression.Variable(typeof(object));
            object constObj = new object();

            var lambda3 = Expression.Lambda(
                Expression.Block(
                    new[] { v1, v2, v3 },
                    Expression.Assign(v1, Expression.Constant(constObj)),
                    Expression.Assign(v2, Expression.Constant(constObj)),
                    Expression.Assign(v3, Expression.Constant(constObj)),
                    Expression.Default(typeof(void))
                )
            );

            var lambda2 = Expression.Lambda(
                Expression.Call(lambda3, typeof(Action).GetMethod("Invoke"))
            );

            var lambda1 = Expression.Lambda(
                Expression.Block(
                    Expression.Call(lambda2, typeof(Action).GetMethod("Invoke")),
                    Expression.Call(lambda3, typeof(Action).GetMethod("Invoke"))
                )
            );

            V.Validate(lambda1);
        }
    }
}
#endif
