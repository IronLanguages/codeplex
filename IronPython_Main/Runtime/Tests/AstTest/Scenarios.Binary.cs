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
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    public static partial class Scenarios {
        public class CoalesceTest {
            public string Left() {
                return "Left";
            }
            public string Right() {
                throw new InvalidOperationException("Test failed, this method should never be called.");
            }
        }

        /// <summary>
        /// The following test is to verify that we can pass T to Equal with a
        /// user provided method taking ByRef parameters. 
        /// </summary>
        public static void Positive_EqualRefParameters(EU.IValidator V) {
            var x = Expression.Parameter(typeof(ComparisonTest.Base), "x");
            var comparison = typeof(ComparisonTest).GetMethod("CompareRef");
            var e = Expression.Lambda<Func<ComparisonTest.Base, bool>>(
                Expression.Equal(
                    x,
                    Expression.Constant(new ComparisonTest.Base(0)),
                    false,
                    comparison
                ),
                x
            );

            V.Validate(e, f =>
            {
                EU.Equal(f(new ComparisonTest.Base(0)), true);
                EU.Equal(f(new ComparisonTest.Derived(0)), true);
                EU.Equal(f(new ComparisonTest.Base(1)), false);
                EU.Equal(f(new ComparisonTest.Derived(1)), false);
            });
        }

        /// <summary>
        /// test.Left() ?? { try { } catch { },  test.Right() }
        /// Right() should never be called as Left() returns non-null.
        /// </summary>
        public static void Positive_CoalesceWithTry(EU.IValidator V) {
            var p = Expression.Parameter(typeof(CoalesceTest), "arg");
            var f = Expression.Lambda<Func<CoalesceTest, string>>(
                Expression.Coalesce(
                    Expression.Call(p, typeof(CoalesceTest).GetMethod("Left")),
                    Expression.Block(
                        Expression.TryCatch(
                            Expression.Empty(),
                            Expression.Catch(typeof(Exception), Expression.Empty())
                        ),
                        Expression.Call(p, typeof(CoalesceTest).GetMethod("Right"))
                    )
                ),
                p
            );

            V.Validate(f, d =>
            {
                EU.Equal(d(new CoalesceTest()), "Left");
            });
        }

        public class RefType1 { }
        public class RefType2 { }
        public interface Interface1 { }
        public interface Interface2 { }
        public class DerivedType1 : RefType1, Interface1 { }
        public class DerivedType2 : RefType2, Interface2 { }

        public static void Positive_OptimizedEqual_UnrelatedClasses(EU.IValidator V) {
            var x = Expression.Parameter(typeof(RefType1), "x");
            var y = Expression.Parameter(typeof(RefType2), "y");

            // Equal on two arguments
            var e1 = Expression.Lambda<Func<RefType1, RefType2, bool>>(
                Expression.Equal(Expression.Convert(x, typeof(object)), Expression.Convert(y, typeof(object))),
                x, y
            );

            V.Validate(e1, f1 =>
            {
                EU.Equal(f1(new RefType1(), new RefType2()), false);
                EU.Equal(f1(null, null), true);
            });

            // NotEqual on two arguments
            e1 = Expression.Lambda<Func<RefType1, RefType2, bool>>(
                Expression.NotEqual(Expression.Convert(x, typeof(object)), Expression.Convert(y, typeof(object))),
                x, y
            );
            
            V.Validate(e1, f1 =>
            {
                EU.Equal(f1(new RefType1(), new RefType2()), true);
                EU.Equal(f1(null, null), false);
            });

            // Equal on two arguments with conditional
            var e2 = Expression.Lambda<Func<RefType1, RefType2, int>>(
                Expression.Condition(
                    Expression.Equal(Expression.Convert(x, typeof(object)), Expression.Convert(y, typeof(object))),
                    Expression.Constant(123),
                    Expression.Constant(444)
                ),
                x, y
            );
            
            V.Validate(e2, f2 =>
            {
                EU.Equal(f2(new RefType1(), new RefType2()), 444);
                EU.Equal(f2(null, null), 123);
            });

            // NotEqual on two arguments with conditional
            e2 = Expression.Lambda<Func<RefType1, RefType2, int>>(
                Expression.Condition(
                    Expression.NotEqual(Expression.Convert(x, typeof(object)), Expression.Convert(y, typeof(object))),
                    Expression.Constant(444),
                    Expression.Constant(123)
                ),
                x, y
            );
            
            V.Validate(e2, f2 =>
            {
                EU.Equal(f2(new RefType1(), new RefType2()), 444);
                EU.Equal(f2(null, null), 123);
            });
        }


        public static void Positive_OptimizedEqual_CompareToNull(EU.IValidator V) {
            var x = Expression.Parameter(typeof(RefType1), "x");

            // Equal to null
            var e1 = Expression.Lambda<Func<RefType1, bool>>(
                Expression.Equal(Expression.Convert(x, typeof(object)), Expression.Constant(null)),
                x
            );
            
            V.Validate(e1, f1 =>
            {
                EU.Equal(f1(new RefType1()), false);
                EU.Equal(f1(null), true);
            });

            // NotEqual to null
            e1 = Expression.Lambda<Func<RefType1, bool>>(
                Expression.NotEqual(Expression.Convert(x, typeof(object)), Expression.Constant(null)),
                x
            );
            
            V.Validate(e1, f1 =>
            {
                EU.Equal(f1(new RefType1()), true);
                EU.Equal(f1(null), false);
            });

            // Equal to null with conditional
            var e2 = Expression.Lambda<Func<RefType1, int>>(
                Expression.Condition(
                    Expression.Equal(Expression.Convert(x, typeof(object)), Expression.Constant(null)),
                    Expression.Constant(123),
                    Expression.Constant(444)
                ),
                x
            );
            
            V.Validate(e2, f2 =>
            {
                EU.Equal(f2(new RefType1()), 444);
                EU.Equal(f2(null), 123);
            });

            // NotEqual to null with conditional
            e2 = Expression.Lambda<Func<RefType1, int>>(
                Expression.Condition(
                    Expression.NotEqual(Expression.Convert(x, typeof(object)), Expression.Constant(null)),
                    Expression.Constant(444),
                    Expression.Constant(123)
                ),
                x
            );
            
            V.Validate(e2, f2 =>
            {
                EU.Equal(f2(new RefType1()), 444);
                EU.Equal(f2(null), 123);
            });
        }

        public static void Positive_ReferenceEqual(EU.IValidator V) {
            TestReferenceEqual(Expression.ReferenceEqual, V);
        }
        public static void Positive_ReferenceNotEqual(EU.IValidator V) {
            TestReferenceEqual(Expression.ReferenceNotEqual, V);
        }

        private static void TestReferenceEqual(Func<Expression, Expression, BinaryExpression> factory, EU.IValidator V) {
            var objs = new[] { null, new object(), new object(), new RefType1(), new RefType1(), new DerivedType1(), new DerivedType1() };
            var types = new[] { typeof(object), typeof(Interface1), typeof(RefType1), typeof(DerivedType1) };

            foreach (object x in objs) {
                foreach (Type u in types) {
                    if (x != null && !u.IsInstanceOfType(x)) {
                        continue;
                    }

                    foreach (object y in objs) {
                        foreach (Type v in types) {
                            if (y != null && !v.IsInstanceOfType(y)) {
                                continue;
                            }

                            bool expected = x == y;

                            BinaryExpression equality = factory(Expression.Constant(x, u), Expression.Constant(y, v));
                            var actual = Expression.Lambda<Func<bool>>(equality).Compile()();
                            if (equality.NodeType == ExpressionType.NotEqual) {
                                actual = !actual;
                            }
                            EU.Equal(expected, actual);
                        }
                    }
                }
            }
        }

        public static bool EqualityByRefNullable(ref int? x, ref double? y) {
            return x == y;
        }

        public static void Positive_EqualityByRefNullable(EU.IValidator V) {
            var e = Expression.Equal(
                Expression.Constant(1, typeof(int?)),
                Expression.Constant(1.0, typeof(double?)),
                false,
                typeof(Scenarios).GetMethod("EqualityByRefNullable")
            );
            
            EU.Equal(e.IsLifted, false);

            var e2 = Expression.Lambda<Func<bool>>(e);
            
            V.Validate(e2, f =>
            {
                EU.Equal(f(), true);
            });
        }

        public static bool BinaryWithRefMethodEqual(ref int x, ref int y) {
            EU.Equal(x, 1);
            EU.Equal(y, 3);
            return x == 1 && y == 3;
        }

        public static void Positive_BinaryWithRefMethod(EU.IValidator V) {
            var lambda = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Constant(1),
                    Expression.Constant(3),
                    false,
                    typeof(Scenarios).GetMethod("BinaryWithRefMethodEqual")
                )
            );
            
            V.Validate(lambda, d =>
            {
                EU.Equal(d(), true);
            });

            lambda = Expression.Lambda<Func<bool>>(
                Expression.Call(
                    typeof(Scenarios).GetMethod("BinaryWithRefMethodEqual"),
                    Expression.Constant(1),
                    Expression.Constant(3)
                )
            );
            
            V.Validate(lambda, d =>
            {
                EU.Equal(d(), true);
            });
        }

        public static void Positive_IntegerTypeAsNullable(EU.IValidator V) {
            var lambda = Expression.Lambda<Func<int?>>(
                Expression.TypeAs(
                    Expression.Constant(1),
                    typeof(int?)
                )
            );
            //var d = lambda.Compile();
            //d();
            V.Validate(lambda, d =>
            {
                EU.Equal(d(), 1);
            });
        }
    }
}
#endif
