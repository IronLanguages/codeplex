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
using System.Collections;
using System.Collections.Generic;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    // Scopes and variable binding tests
    public static partial class Scenarios {

        public static Expression Positive_VariablesSharingName(EU.IValidator V) {
            // 2 variables with the same name
            ParameterExpression v1 = Expression.Variable(typeof(int), "local");
            ParameterExpression v2 = Expression.Variable(typeof(int), "local");

            Expression body = Expression.Block(
                Expression.Assign(v1, Expression.Constant(7)),
                Expression.Assign(v2, Expression.Constant(11)),
                Expression.Multiply(v1, v2)
            );

            var e = Expression.Lambda<Func<int>>(Expression.Block(new[] { v1, v2 }, body));
            
            V.Validate(e, ri =>
            {
                EU.Equal(ri(), (7 * 11));
            });
            return e;
        }

        // Simple closure test
        public static Expression Positive_Closure(EU.IValidator V) {
            // Declare variable
            ParameterExpression local = Expression.Variable(typeof(int), "GenLocal1");

            // Assign to it on nested lambda
            LambdaExpression inner = Expression.Lambda(Expression.Assign(local, Expression.Constant(5)));

            // Read variable on the root lambda
            LambdaExpression outer = Expression.Lambda(
                Expression.Block(
                    new[] { local },
                    Expression.Invoke(inner),
                    MakeAreEqual(local, Expression.Constant(5)),
                    Expression.Assign(local, Expression.Constant(6)),
                    MakeAreEqual(local, Expression.Constant(6))
                )
            );
            V.Validate(outer);
            return outer;
        }

        public static Expression Positive_ReusingParameters(EU.IValidator V) {
            ParameterExpression i = Expression.Parameter(typeof(int), "i");

            Expression<Func<int, int>> e = Expression.Lambda<Func<int, int>>(
                Expression.Invoke(Expression.Lambda<Func<int, int>>(i, i), i),
                i
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(123), 123);
            });
            return e;
        }

        public static Expression Positive_ReusingParameters2(EU.IValidator V) {
            ParameterExpression i = Expression.Parameter(typeof(int), "i");

            Expression<Func<int, int>> e = Expression.Lambda<Func<int, int>>(
                Expression.Add(
                    Expression.Invoke(Expression.Lambda<Func<int, int>>(i, i), Expression.Constant(444)),
                    i
                ),
                i
            );

            V.Validate(e, f =>
            {
                // NOTE: in LinqV1 this returns 888, which is a bug in the inliner,
                // see the next test case, which works as expected in LinqV1
                EU.Equal(f(123), 567);
            });
            return e;
        }

        public static Expression Positive_ReusingParameters3(EU.IValidator V) {
            ParameterExpression i = Expression.Parameter(typeof(int), "i");
            ParameterExpression x = Expression.Parameter(typeof(Func<int, int>), "i");

            Expression<Func<int, int>> e = Expression.Lambda<Func<int, int>>(
                Expression.Add(
                    Expression.Invoke(
                        Expression.Lambda<Func<Func<int, int>, int>>(
                            Expression.Invoke(x, Expression.Constant(444)),
                            x
                        ),
                        Expression.Lambda<Func<int, int>>(i, i)
                    ),
                    i
                ),
                i
            );
            
            V.Validate(e, f =>
            {
                EU.Equal(f(123), 567);
            });

            return e;
        }

        public static Expression Positive_ReusingLambdas(EU.IValidator V) {
            ParameterExpression x = Expression.Parameter(typeof(int), "x");
            ParameterExpression y = Expression.Parameter(typeof(int), "y");
            ParameterExpression z = Expression.Parameter(typeof(int), "z");

            Expression<Func<int>> reuse = Expression.Lambda<Func<int>>(Expression.Add(x, y));

            var e = Expression.Lambda<Func<int, int, int, int>>(
                Expression.Add(
                    Expression.Invoke(reuse), // x + y
                    Expression.Invoke(
                        Expression.Lambda<Func<int, int>>(Expression.Invoke(reuse), y),
                        z
                    ) // x + z
                ),
                x,
                y,
                z
            );
            
            V.Validate(e, test =>
            {
                int actual = test(123, 444, 5000);
                int expected = (123 + 444) + (123 + 5000);
                EU.Equal(actual, expected);
            });
            return e;
        }

        public static void Positive_NestedScopes(EU.IValidator V) {
            var result = Expression.Parameter(typeof(List<Func<string>>), "$result");
            var str = Expression.Parameter(typeof(string), "$str");
            var e = Expression.Lambda<Func<List<Func<string>>>>(
                Expression.Block(
                    new[] { result },
                    Expression.Assign(result, Expression.New(typeof(List<Func<string>>).GetConstructor(Type.EmptyTypes))),
                    new TestReducibleNode(
                        Expression.NewArrayInit(typeof(string), Expression.Constant("foo"), Expression.Constant("bar"), Expression.Constant("red")),
                        str,
                        Expression.Call(
                            result,
                            typeof(List<Func<string>>).GetMethod("Add"),
                            MakeClosureOverVariable(str)
                        )
                    ),
                    result
                )
            );
            V.Validate(e, f =>
            {
                var list = f();
                EU.Equal(list.Count, 3);
                EU.Equal(list[0](), "foo");
                EU.Equal(list[1](), "bar");
                EU.Equal(list[2](), "red");
            });
        }

        //Verify that variables in ET is not reused.
        public static void Positive_NoLocalVariableReuse(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var e = Expression.Lambda<Func<int>>(
                Expression.Block(
                    Expression.Block(new[] { x }, Expression.Assign(x, Expression.Constant(123))),
                    Expression.Block(new[] { y }, y)
                )
            );

            V.Validate(e, z =>
            {
                EU.Equal(z(), default(int));
            });
        }

        // ET compiler should not merge shadowed variables
        public static void Positive_MergingShadowedVars1(EU.IValidator V) {
            // lambda with block
            var x = Expression.Parameter(typeof(int), "x");
            var e = Expression.Lambda<Func<int, int>>(
                Expression.Block(new[] { x }, x),
                x
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(123), 0);
            });
        }

        public static void Positive_MergingShadowedVars2(EU.IValidator V) {
            // block with block
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var e = Expression.Lambda<Func<int, int>>(
                Expression.Block(
                    new[] { y },
                    Expression.Block(new[] { y }, x)
                ),
                x
            );

            V.Validate(e, f =>
            {
                EU.Equal(f(456), 456);
            });
        }

        public static void Positive_MergingShadowedVars3(EU.IValidator V) {
            // inlined lambda with block
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var e = Expression.Lambda<Func<int, int>>(
                Expression.Invoke(
                    Expression.Lambda(
                        Expression.Block(new[] { y }, y),
                        y
                    ),
                    x
                ),
                x
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(789), 0);
            });
        }

        public static void Negative_DuplicateVariables1(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            EU.Throws<ArgumentException>(() => Expression.Lambda(x, x, x));
        }

        public static void Negative_DuplicateVariables2(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            EU.Throws<ArgumentException>(() => Expression.Block(new[] { x, x }, x));
        }

        public static void Positive_BlockWithVoidType(EU.IValidator V) {
            var l = Expression.Lambda<Action>(
                Expression.Block(
                    typeof(void),
                    Expression.Call(
                        Expression.Constant("Hello"),
                        typeof(string).GetMethod("ToUpperInvariant")
                    )
                )
            );
            V.Validate(l);
        }

        public static void Positive_BlockWithExplicitType(EU.IValidator V) {
            var l = Expression.Lambda<Func<IEnumerable>>(
                Expression.Block(
                    typeof(IEnumerable),
                    Expression.Call(
                        Expression.Constant("Hello"),
                        typeof(string).GetMethod("ToUpperInvariant")
                    )
                )
            );
            V.Validate(l);
        }
    }
}
#endif
