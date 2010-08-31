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
using System.Runtime.CompilerServices;
using TreeUtils = Microsoft.Scripting.Ast.Utils;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    // Generator tests
    public static partial class Scenarios {

        public static Expression Positive_GeneratorBasic(EU.IValidator V) {
            var label = Expression.Label(typeof(int));
            var expr = TreeUtils.GeneratorLambda<Func<IEnumerator<int>>>(
                label,
                Expression.Block(
                    TreeUtils.YieldReturn(label, Expression.Constant(1)),
                    TreeUtils.YieldReturn(label, Expression.Constant(2)),
                    TreeUtils.YieldReturn(label, Expression.Constant(3))
                ),
                "test_generator"
            );
            
            V.Validate(expr, g =>
            {
                IEnumerator e = g();
                EU.Equal(e.Current, 0);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 1);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 2);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 3);
                EU.Equal(e.MoveNext(), false);
                EU.Equal(e.Current, 3);
                EU.Equal(e.MoveNext(), false);
            });
            return expr;
        }

        public static Expression Positive_GeneratorObject(EU.IValidator V) {
            var label = Expression.Label(typeof(object));
            var expr = TreeUtils.GeneratorLambda<Func<IEnumerator>>(
                label,
                Expression.Block(
                    TreeUtils.YieldReturn(label, Expression.Constant(1, typeof(object))),
                    TreeUtils.YieldReturn(label, Expression.Constant(2, typeof(object))),
                    TreeUtils.YieldReturn(label, Expression.Constant(3, typeof(object)))
                ),
                "test_generator"
            );
            
            V.Validate(expr, g =>
            {
                IEnumerator e = g();
                EU.Equal(e.Current, null);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 1);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 2);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 3);
                EU.Equal(e.MoveNext(), false);
                EU.Equal(e.Current, 3);
                EU.Equal(e.MoveNext(), false);
            });
            return expr;
        }

        private static Expression MakeClosureOverVariable(ParameterExpression foo) {
            string name = "closureOverVariable$" + foo.Name;
            ParameterExpression baz = Expression.Variable(foo.Type, name);

            return Expression.Block(
                        new [] { baz },
                        Expression.Assign(baz, foo),
                        Expression.Lambda(baz, name, new ParameterExpression[0])
            );
        }

        public static Expression Positive_GeneratorNestedScopes1(EU.IValidator V) {
            // Verify that scopes inside generators are lifted

            ParameterExpression foo = Expression.Variable(typeof(int), "$foo");
            ParameterExpression bar = Expression.Parameter(typeof(int), "$bar");

            // reuse the instance to make sure we can reuse scopes
            Expression closure = MakeClosureOverVariable(foo);
            var label = Expression.Label(typeof(object));
            var expr = TreeUtils.GeneratorLambda<Func<int, IEnumerator>>(
                label,
                Expression.Block(
                    new [] { foo },
                    TreeUtils.YieldReturn(label, closure),
                    Expression.Assign(foo, Expression.Constant(123)),
                    TreeUtils.YieldReturn(label, MakeClosureOverVariable(foo)),
                    Expression.Assign(foo, bar),
                    TreeUtils.YieldReturn(label, closure)
                ),
                "test_generator_nested_scopes1",
                bar
            );
            
            V.Validate(expr, g =>
            {
                IEnumerator e = g(444);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(((Func<int>)e.Current)(), 0);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(((Func<int>)e.Current)(), 123);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(((Func<int>)e.Current)(), 444);
                EU.Equal(e.MoveNext(), false);
                EU.Equal(((Func<int>)e.Current)(), 444);
                EU.Equal(e.MoveNext(), false);
            });
            return expr;
        }

        public static Expression Positive_GeneratorNestedScopes2(EU.IValidator V) {
            // Verify that scopes inside generators are lifted
            ParameterExpression gen = Expression.Variable(typeof(IEnumerator), "$test_generator");

            ParameterExpression foo = Expression.Variable(typeof(int), "$foo");
            ParameterExpression bar = Expression.Parameter(typeof(int), "$bar");

            // reuse the instance to make sure we can reuse lambdas
            Expression lambdaFoo = Expression.Lambda(foo);

            var label = Expression.Label(typeof(object));
            var expr = TreeUtils.GeneratorLambda<Func<int, IEnumerator>>(
                label,
                Expression.Block(
                    Expression.Block(
                        new[] { foo },
                        TreeUtils.YieldReturn(label, lambdaFoo),
                        Expression.Assign(foo, Expression.Constant(123)),
                        TreeUtils.YieldReturn(label, lambdaFoo),
                        Expression.Assign(foo, bar),
                        TreeUtils.YieldReturn(label, Expression.Lambda(foo, "$yield444", new ParameterExpression[0]))
                    ),
                    TreeUtils.YieldReturn(label, Expression.Lambda(Expression.Constant(888), "$yield888", new ParameterExpression[0]))
                ),
                "test_generator_nested_scopes2",
                bar
            );
            
            V.Validate(expr, g =>
            {
                IEnumerator e = g(444);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(((Func<int>)e.Current)(), 0);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(((Func<int>)e.Current)(), 123);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(((Func<int>)e.Current)(), 444);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(((Func<int>)e.Current)(), 888);
                EU.Equal(e.MoveNext(), false);
            });
            return expr;
        }

        // Regression for Dev10 bug 510554
        // Generator's should be able to close over variables, even if they
        // don't have a yield and variables themselves
        public static Expression Positive_GeneratorNoYieldClosure(EU.IValidator V) {
            var box = new string[1];

            var label = Expression.Label(typeof(object));
            var x = Expression.Variable(typeof(string), "x");
            var expr = Expression.Lambda<Func<IEnumerator>>(
                Expression.Invoke(
                    Expression.Block(
                        new[] { x },
                        TreeUtils.GeneratorLambda<Func<IEnumerator>>(
                            label,
                            Expression.Block(
                                Expression.Assign(x, Expression.Constant("Success")),
                                Expression.Assign(Expression.ArrayAccess(Expression.Constant(box), Expression.Constant(0)), x),
                                Expression.Empty()
                            )
                        )
                    )
                )
            );
            
            V.Validate(expr, f =>
            {
                var e = f();
                // Make sure we can execute the body of the method
                EU.Equal(e.MoveNext(), false);
                EU.Equal(box[0], "Success");
            });
            return expr;
        }

        public static Expression Positive_GeneratorWithYieldClosure(EU.IValidator V) {
            var box = new string[1];

            var label = Expression.Label(typeof(object));
            var x = Expression.Variable(typeof(string), "x");
            var y = Expression.Variable(typeof(string), "y");
            var expr = Expression.Lambda<Func<IEnumerator>>(
                Expression.Invoke(
                    Expression.Block(
                        new [] { x },
                        TreeUtils.GeneratorLambda<Func<IEnumerator>>(
                            label,
                            Expression.Block(
                                new [] { y },
                                Expression.Assign(x, Expression.Constant("Success")),
                                Expression.Assign(y, x),
                                Expression.Assign(Expression.ArrayAccess(Expression.Constant(box), Expression.Constant(0)), y),
                                Expression.Empty()
                            )
                        )
                    )
                )
            );
            
            V.Validate(expr, f =>
            {
                var e = f();
                // Make sure we can execute the body of the method
                EU.Equal(e.MoveNext(), false);
                EU.Equal(box[0], "Success");
            });
            return expr;
        }

        // Regression for Dev10 bug 496889
        public static Expression Positive_GeneratorNestedScopes3(EU.IValidator V) {
            var x = Expression.Variable(typeof(int), "x");
            var y = Expression.Variable(typeof(int), "y");
            var label = Expression.Label(typeof(int));
            var expr = TreeUtils.GeneratorLambda<Func<IEnumerator>>(
                label,
                TreeUtils.YieldReturn(
                    label,
                    Expression.Add(
                        Expression.Block(
                            new[] { x },
                            Expression.Add(
                                Expression.Block(
                                    new [] { y },
                                    TreeUtils.YieldReturn(label, y),
                                    Expression.Assign(x, Expression.Constant(111))
                                ),
                                x
                            )
                        ),
                        Expression.Constant(456)
                    )
                )
            );
            
            V.Validate(expr, g =>
            {
                IEnumerator e = g();
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 0);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 678);
                EU.Equal(e.MoveNext(), false);
            });
            return expr;
        }

        public interface IGeneratorMustReturnIEnumeratorTest : IEnumerator {
        }

        // Regression for Dev10 bug 510539
        public static void Negative_GeneratorMustReturnIEnumerator(EU.IValidator V) {
            var label = Expression.Label();
            EU.Throws<ArgumentException>(
                () => TreeUtils.GeneratorLambda(
                    typeof(Func<IGeneratorMustReturnIEnumeratorTest>),
                    label,
                    TreeUtils.YieldReturn(label, Expression.Constant(1))
                )
            );
        }

        public static Expression Positive_GeneratorTryFinally(EU.IValidator V) {
            var label = Expression.Label(typeof(int));
            var g = TreeUtils.GeneratorLambda<Func<IEnumerable>>(
                label,
                Expression.TryFinally(
                    TreeUtils.YieldReturn(label, Expression.Constant(1)),
                    TreeUtils.YieldReturn(label, Expression.Constant(2))
                )
            );
            
            V.Validate(g, f =>
            {
                var e = f().GetEnumerator();
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 1);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 2);
                EU.Equal(e.MoveNext(), false);
            });
            return g;
        }

        public static Expression Positive_GeneratorTryFinally2(EU.IValidator V) {
            var label = Expression.Label(typeof(int));
            var box = new StrongBox<int>(5);
            var boxVal = Expression.Field(Expression.Constant(box), "Value");
            var g = TreeUtils.GeneratorLambda<Func<IEnumerator>>(
                label,
                Expression.TryFinally(
                    Expression.Block(
                        TreeUtils.YieldReturn(label, Expression.Constant(1)),
                        TreeUtils.YieldReturn(label, Expression.Constant(2))
                    ),
                    Expression.Assign(boxVal, Expression.Add(boxVal, Expression.Constant(1)))
                )
            );
            
            V.Validate(g, f =>
            {
                var e = f();
                EU.Equal(box.Value, 5);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 1);
                EU.Equal(box.Value, 5);
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 2);
                EU.Equal(box.Value, 5);
                EU.Equal(e.MoveNext(), false);
                EU.Equal(box.Value, 6);
            });
            return g;
        }

        public static Expression Positive_GeneratorYieldBreak(EU.IValidator V) {
            // Yield break stops iteration, runs finally clauses
            var label = Expression.Label(typeof(string));
            var box = new StrongBox<int>(5);
            var boxVal = Expression.Field(Expression.Constant(box), "Value");
            var g = TreeUtils.GeneratorLambda<Func<IEnumerable<string>>>(
                label, 
                Expression.TryFinally(
                    Expression.Block(
                        TreeUtils.YieldReturn(label, Expression.Constant("hello")),
                        TreeUtils.YieldBreak(label),
                        TreeUtils.YieldReturn(label, Expression.Constant("bug"))
                    ),
                    Expression.Assign(boxVal, Expression.Add(boxVal, Expression.Constant(1)))
                )
            );
            
            V.Validate(g, f =>
            {
                var e = f().GetEnumerator();
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, "hello");
                EU.Equal(box.Value, 5);
                EU.Equal(e.MoveNext(), false);
                EU.Equal(box.Value, 6);
                EU.Equal(e.Current, "hello");
                EU.Equal(e.MoveNext(), false);
                EU.Equal(box.Value, 6);
                EU.Equal(e.Current, "hello");
            });
            return g;
        }

        // Marked "negative" because it throws an exception while running
        // (it is essentially a positive test)
        public static Expression Negative_GeneratorCatch(EU.IValidator V) {
            var label = Expression.Label(typeof(int));
            var g = TreeUtils.GeneratorLambda<Func<IEnumerable>>(
                label,
                Expression.TryCatch(
                    Expression.Block(
                        TreeUtils.YieldReturn(label, Expression.Constant(1)),
                        Expression.Divide(Expression.Constant(0), Expression.Constant(0)),
                        TreeUtils.YieldReturn(label, Expression.Constant(2))
                    ),
                    Expression.Catch(
                        typeof(DivideByZeroException), 
                        TreeUtils.YieldBreak(label)
                    )
                )
            );
            
            V.Validate(g, f =>
            {
                var e = f().GetEnumerator();
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 1);
                EU.Equal(e.MoveNext(), false);
                EU.Equal(e.Current, 1);
            });
            return g;
        }

        // Test that calling Dispose runs not-yet-run finally blocks. Also,
        // once dispose is called the generator can't be re-run
        // TODO: disabled. We don't support this yet. I had a partial
        // implementation, but it has the downside of making all generators
        // slower. And it's not needed yet so hard to justify...
        public static Expression Disabled_GeneratorDispose(EU.IValidator V) {
            var box = new StrongBox<string>("start");
            var boxVal = Expression.Field(Expression.Constant(box), "Value");
            Func<string, Expression> appendStr = s =>
                Expression.Assign(
                    boxVal,
                    Expression.Call(
                        new Func<string, string, string>(string.Concat).Method,
                        boxVal,
                        Expression.Constant("," + s)
                    )
                );

            var label = Expression.Label(typeof(int));
            var g = TreeUtils.GeneratorLambda<Func<IEnumerator<int>>>(
                label,
                Expression.TryFinally(
                    Expression.TryFinally(
                        Expression.Block(
                            TreeUtils.YieldReturn(label, Expression.Constant(1)),
                            Expression.TryFinally(
                                TreeUtils.YieldReturn(label, Expression.Constant(2)),
                                appendStr("finally2")
                            ),
                            TreeUtils.YieldReturn(label, Expression.Constant(3)),
                            TreeUtils.YieldReturn(label, Expression.Constant(4))
                        ),
                        appendStr("finally1")
                    ),
                    appendStr("finally0")
                )
            );

            
            V.Validate(g, f =>
            {
                var e = f();
                EU.Equal(e.Current, 0);
                EU.Equal(box.Value, "start");
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 1);
                EU.Equal(box.Value, "start");
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 2);
                EU.Equal(box.Value, "start");
                EU.Equal(e.MoveNext(), true);
                EU.Equal(e.Current, 3);
                EU.Equal(box.Value, "start,finally2");
                e.Dispose();
                EU.Equal(box.Value, "start,finally2,finally1,finally0");
                EU.Equal(e.Current, 3);
                EU.Equal(e.MoveNext(), false);
                EU.Equal(box.Value, "start,finally2,finally1,finally0");
                EU.Equal(e.Current, 3);
                EU.Equal(e.MoveNext(), false);
            });
            return g;
        }


        // Test that each call to GetEnumerator gives a new enumerator with its
        // own independant state
        public static Expression Positive_GeneratorEnumerable(EU.IValidator V) {
            var label = Expression.Label(typeof(int));
            var count = Expression.Parameter(typeof(int), "count");
            var g = TreeUtils.GeneratorLambda<Func<int, IEnumerable<int>>>(
                label,
                TreeUtils.Loop(
                    Expression.GreaterThan(count, Expression.Constant(0)),
                    Expression.Assign(count, Expression.Subtract(count, Expression.Constant(1))),
                    TreeUtils.YieldReturn(label, count),
                    null, null, null // else, break, continue
                ),
                count
            );
            
            V.Validate(g, f =>
            {
                var e = f(10);
                var e2 = f(5);

                // Verify all of these enumerators are independent of eachother
                int i = 10;
                foreach (int x in e) {
                    EU.Equal(i--, x);

                    int i2 = 10;
                    foreach (int x2 in e) {
                        EU.Equal(i2--, x2);

                        int j = 5;
                        foreach (int y in e2) {
                            EU.Equal(j--, y);

                            int j2 = 5;
                            foreach (int y2 in e2) {
                                EU.Equal(j2--, y2);
                            }
                        }
                    }
                }
            });
            return g;               
        }


        public static void Negative_GeneratorCatchBlock(EU.IValidator V) {
            var i = Expression.Parameter(typeof(int), "i");
            var ex = Expression.Parameter(typeof(Exception), "ex");
            var target = Expression.Label();
            var generatorTarget = Expression.Label(typeof(Func<string>));
            var msg = Expression.Parameter(typeof(string), "msg");

            // Test that closures work and that the value of the exception is
            // preserved across generator invocations
            var e = TreeUtils.GeneratorLambda<Func<IEnumerable<Func<string>>>>(
                generatorTarget,
                Expression.Block(
                    new[] { i },
                    Expression.Label(target),
                    Expression.TryCatch(
                        Expression.Throw(
                            Expression.New(
                                typeof(Exception).GetConstructor(new[] { typeof(string) }),
                                Expression.Call(i, "ToString", null)
                            )
                        ),
                        Expression.Catch(
                            ex,
                            Expression.Block(
                                new[] { msg },
                                TreeUtils.YieldReturn(
                                    generatorTarget,
                                    Expression.Lambda(Expression.Property(ex, "Message"))
                                ),
                                Expression.Assign(msg, Expression.Property(ex, "Message")),
                                TreeUtils.YieldReturn(generatorTarget, Expression.Lambda(msg))
                            )
                        )
                    ),
                    Expression.Condition(
                        Expression.LessThan(Expression.PreIncrementAssign(i), Expression.Constant(10)),
                        Expression.Goto(target),
                        Expression.Empty()
                    )
                )
            );
            
            V.Validate(e, f =>
            {
                var x = new List<string>();
                foreach (var g in f()) {
                    x.Add(g());
                }

                for (int j = 0; j < 10; j++) {
                    EU.Equal(x[2 * j], j.ToString());
                    EU.Equal(x[2 * j + 1], j.ToString());
                }
            });
        }

        public static void Negative_GeneratorRethrow(EU.IValidator V) {
            LabelTarget label = Expression.Label(typeof(int));

            // Simple rethrow:
            var e = TreeUtils.GeneratorLambda<Func<IEnumerable>>(
                label,
                Expression.Block(
                    Expression.TryCatch(
                        Expression.Throw(Expression.Constant(new DivideByZeroException())),
                        Expression.Catch(
                            typeof(DivideByZeroException),
                            Expression.Rethrow()
                        )
                    )
                )
            );
            var f = e.Compile();
            var g = f();
            EU.Throws<DivideByZeroException>(
                () => { foreach (int i in g); }
            );

            // Rethrow with yield
            e = TreeUtils.GeneratorLambda<Func<IEnumerable>>(
                label,
                Expression.Block(
                    Expression.TryCatch(
                        Expression.Throw(Expression.Constant(new DivideByZeroException())),
                        Expression.Catch(
                            typeof(DivideByZeroException),
                            Expression.Block(
                                TreeUtils.YieldReturn(label, Expression.Constant(0)),
                                Expression.Rethrow()
                            )
                        )
                    )
                )
            );
            f = e.Compile();
            g = f();

            EU.Throws<DivideByZeroException>(
                () => { foreach (int i in g) EU.Equal(0, i); }
            );
        }

#if !SILVERLIGHT
        public static void Positive_NestedYields(EU.IValidator V) {
            var returnLabel = Expression.Label(typeof(object));
            var genLabel = Expression.Label(typeof(object));

            var obj = new object();
            var body = TreeUtils.YieldReturn(
                genLabel,
                Expression.Label(returnLabel,
                   TreeUtils.Convert(
                       Expression.Block(
                           TreeUtils.YieldReturn(genLabel, Expression.Constant(null), 1),
                           Expression.Return(returnLabel, Expression.Constant(obj))
                       ),
                       typeof(object)
                   )
                )
            );

            var l = TreeUtils.GeneratorLambda<Func<IEnumerator<object>>>(genLabel, body, "GeneratorFunc");

            var d = l.Compile();
            var e = d();

            EU.Equal(e.MoveNext(), true);
            EU.Equal(e.Current, null);
            EU.Equal(e.MoveNext(), true);
            EU.Equal(e.Current, obj);
            EU.Equal(e.MoveNext(), false);
        }
#endif

        // array[index] = { yield return value; expr }
        public static void Positive_Assignment_ArrayIndex_Yield(EU.IValidator V) {
            var genLabel = Expression.Label(typeof(object));
            var var_a = Expression.Parameter(typeof(object).MakeArrayType());
            var obj = new object();

            var body =
                Expression.Block(new[] { var_a },
                    Expression.Assign(
                        var_a,
                        Expression.NewArrayBounds(typeof(object), Expression.Constant(10))
                    ),

                    Expression.Assign(
                        Expression.ArrayAccess(var_a, Expression.Constant(0)),
                        Expression.Block(
                            TreeUtils.YieldReturn(genLabel, Expression.Constant(obj)),
                            Expression.Constant("const")
                        )
                    ),
                    Expression.Empty()
                );

            var l = TreeUtils.GeneratorLambda<Func<IEnumerator<object>>>(genLabel, body, "GeneratorFunc");

            var d = l.Compile();
            var e = d();

            EU.Equal(e.MoveNext(), true);
            EU.Equal(e.Current, obj);
            EU.Equal(e.MoveNext(), false);
        }
    }
}
#endif
