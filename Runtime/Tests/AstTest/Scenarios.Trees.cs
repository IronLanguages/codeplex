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
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Scripting.Utils;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    public class TestClass {
        private int _x = 111;
        public int X {
            get { return _x; }
            set { _x = value; }
        }

        private static int _y;
        public static int Y {
            get { return _y; }
            set { _y = value; }
        }

        private readonly Dictionary<string, int> _dict = new Dictionary<string, int>();
        public int this[string x, string y] {
            get {
                int value;
                _dict.TryGetValue(x + y, out value);
                return value;
            }
            set {
                _dict[x + y] = value;
            }
        }
    }

    public static partial class Scenarios {

        // helper method
        private static Expression MakeAreEqual(Expression left, Expression right) {
            return Expression.Condition(
                Expression.NotEqual(left, right),
                Expression.Throw(
                    Expression.New(
                        typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) }),
                        Expression.Constant("test failed.")
                    )
                ),
                Expression.Empty()
            );
        }

        public static void Positive_Constant(EU.IValidator V) {
            EU.Equal(Expression.Constant(10), 10);
        }

        public static decimal Decimal10 = 10;

        public static void Positive_StaticField(EU.IValidator V) {
            EU.Equal(
                Expression.Equal(
                    Expression.Field(null, Field("Decimal10")),
                    Expression.Field(null, Field("Decimal10"))
                ),
                true
            );
        }

        //372424
        delegate Object Del();
        public static void Negative_InvalidBreakContinue(EU.IValidator V) {
            int counter = 0;

            // break outside of a loop.
            try {
                Expression.Lambda(
                    Expression.Block(
                        Expression.Break(Expression.Label())
                    )
                ).Compile();
            } catch (InvalidOperationException) {
                counter += 1;
            }

            // continue outside of a loop.
            try {
                Expression.Lambda(
                    Expression.Block(
                        Expression.Continue(Expression.Label())
                    )
                ).Compile();
            } catch (InvalidOperationException) {
                counter += 1;
            }

            // continue with invalid target.
            try {
                Expression.Lambda(
                    Expression.Loop(Expression.Continue(Expression.Label()))
                ).Compile();
            } catch (InvalidOperationException) {
                counter += 1;
            }

            EU.Equals(counter, 3);
        }

        public static void Positive_ValueReturningLoop(EU.IValidator V) {
            var i = Expression.Variable(typeof(int), "i");
            var b = Expression.Label(typeof(int));
            var e = Expression.Lambda<Func<int>>(
                Expression.Block(
                    new[] { i },
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(i, Expression.Constant(123)),
                            Expression.PreIncrementAssign(i),
                            Expression.Break(b, i)
                        ),
                        b
                    )
                )
            );

            V.Validate(e, f =>
            {
                b = Expression.Label(typeof(Func<int>));
                var e2 = Expression.Lambda<Func<Func<int>>>(
                    Expression.Block(
                        new[] { i },
                        Expression.Loop(
                            Expression.IfThenElse(
                                Expression.LessThan(i, Expression.Constant(123)),
                                Expression.PreIncrementAssign(i),
                                Expression.Break(b, Expression.Constant(f))
                            ),
                            b
                        )
                    )
                );
                var f2 = e2.Compile();

                EU.Equal(f2(), f);
                EU.Equal(f(), 123);
            });
        }

        public delegate int TestIfStatement(int value);

        public static Expression Positive_Condition(EU.IValidator V) {
            ParameterExpression p = Expression.Parameter(typeof(int), "value");

            var r = Expression.Label(typeof(int));
            Expression body = Expression.Block(
                // if (p == 1) return 11;
                Expression.Condition(
                    Expression.Equal(p, Expression.Constant(1)),
                    Expression.Return(r, Expression.Constant(11)),
                    Expression.Empty()
                ),

                // if (p == 2) return 12;
                Expression.Condition(
                    Expression.Equal(p, Expression.Constant(2)),
                    Expression.Return(r, Expression.Constant(12)),
                    Expression.Empty()
                ),

                // if (p != 3) {
                //    ;
                // } else {
                //    return 13;
                //
                Expression.Condition(
                    Expression.NotEqual(p, Expression.Constant(3)),
                    Expression.Empty(),
                    Expression.Return(r, Expression.Constant(13))
                ),

                // if (p == 4) {
                //     return 14;
                // } else if (p == 5) {
                //     return 15;
                // }
                Expression.Condition(
                    Expression.Equal(p, Expression.Constant(4)),
                    Expression.Return(r, Expression.Constant(14)),
                    Expression.Condition(
                        Expression.Equal(p, Expression.Constant(5)),
                        Expression.Return(r, Expression.Constant(15)),
                        Expression.Empty()
                    )
                ),

                Expression.Constant(17)
            );

            var lambda = Expression.Lambda<TestIfStatement>(Expression.Label(r, body), p);

            V.Validate(lambda, s4 =>
            {
                EU.Equal(s4(1), 11);
                EU.Equal(s4(2), 12);
                EU.Equal(s4(3), 13);
                EU.Equal(s4(4), 14);
                EU.Equal(s4(5), 15);
                EU.Equal(s4(0), 17);
            });
            return lambda;
        }

        private static FieldInfo Field(string name) {
            return typeof(Scenarios).GetField(name);
        }

        public class OpAssignmentTest {
            public int Field;

            public OpAssignmentTest() {
                Field = 10;
                property = 20;
            }

            private int property;
            public int Property {
                get {
                    return property;
                }
                set {
                    property = value;
                }
            }
        }

        public static void Negative_MultiplyChecked_UInt16(EU.IValidator V) {
            MultiplyChecked<UInt16>(V);
        }
        public static void Negative_MultiplyChecked_Int16(EU.IValidator V) {
            MultiplyChecked<Int16>(V);
        }

        public static void Negative_MultiplyChecked_UInt32(EU.IValidator V) {
            MultiplyChecked<UInt32>(V);
        }
        public static void Negative_MultiplyChecked_Int32(EU.IValidator V) {
            MultiplyChecked<Int32>(V);
        }

        public static void Negative_MultiplyChecked_Int64(EU.IValidator V) {
            MultiplyChecked<Int64>(V);
        }
        public static void Negative_MultiplyChecked_UInt64(EU.IValidator V) {
            MultiplyChecked<UInt64>(V);
        }

        private static void MultiplyChecked<T>(EU.IValidator V) {
            Type type = typeof(T);
            object min = type.GetField("MinValue").GetValue(null);
            object max = type.GetField("MaxValue").GetValue(null);

            bool isUnsigned = default(T).Equals(min);

            object left, right;
            if (isUnsigned) {
                left = max;
#if SILVERLIGHT
                right = Convert.ChangeType(2, type, null);
#else
                right = Convert.ChangeType(2, type);
#endif
            } else {
                left = min;
#if SILVERLIGHT
                right = Convert.ChangeType(-1, type, null);
#else
                right = Convert.ChangeType(-1, type);
#endif
            }


            var x = Expression.Parameter(type, "x");
            var e = Expression.Lambda<Func<T, T>>(
                Expression.MultiplyChecked(
                    Expression.Constant(left, type),
                    x
                ),
                x
            );
            var f = e.Compile();
            EU.Throws<OverflowException>(() => f((T)right));

            var e2 = 
                Expression.MultiplyChecked(
                    Expression.Constant(left, type),
                    Expression.Constant(right, type)
                );

            V.ValidateException<OverflowException>(e2, false);
        }

        // Verify applying OpAssign operations on decimals.
        // See Dev10 bug 541511
        public static Expression Positive_OpAssignDecimal(EU.IValidator V) {
            ParameterExpression x = Expression.Variable(typeof(decimal), "x");
            var d = Expression.Constant((decimal)1);

            // x = 1
            // x += x
            // x += x
            // x -= 1
            // x -= 1
            // x *= x
            // x *= x
            // x /= 1
            // x %= 10
            // The result is 6

            Expression body = Expression.Block(
                new[] { x },
                Expression.Assign(x, d),
                Expression.AddAssign(x, x),
                Expression.AddAssignChecked(x, x),
                Expression.SubtractAssign(x, d),
                Expression.SubtractAssignChecked(x, d),
                Expression.MultiplyAssign(x, x),
                Expression.MultiplyAssignChecked(x, x),
                Expression.DivideAssign(x, d),
                Expression.ModuloAssign(x, Expression.Constant((decimal)10))
            );

            var e = Expression.Lambda<Func<decimal>>(body);
            V.Validate(e, f =>
            {
                EU.Equal(f(), (decimal)6);
            });
            return body;
        }

        // Test that boxing ints and bools behaves like it does in CLR.
        public static void Positive_BoxingIntBool(EU.IValidator V) {
            var e1 = Expression.Lambda<Func<object>>(Expression.Convert(Expression.Constant(1), typeof(object)));
            V.Validate(e1, boxInt =>
            {
                // not reference equal, but value equal
                EU.Equal(boxInt() != boxInt(), true);
                EU.Equal(boxInt(), boxInt());
            });

            var e2 = Expression.Lambda<Func<object>>(Expression.Convert(Expression.Constant(false), typeof(object)));
            V.Validate(e1, boxBool =>
            {
                // not reference equal, but value equal
                EU.Equal(boxBool() != boxBool(), true);
                EU.Equal(boxBool(), boxBool());
            });
        }

        #region OpAssign with conversion
        //Verify that OpAssign works with conversion lambda when the overload 
        //method returns different type than the oparands.

        //OpOverloads contains overload methods whose return type is different 
        //from operand type
        public static class OpOverloads {
            public static int Add(int? l, int? r) {
                return l.GetValueOrDefault() + r.GetValueOrDefault();
            }
            public static int Sub(int? l, int? r) {
                return l.GetValueOrDefault() - r.GetValueOrDefault();
            }
            public static int Mul(int? l, int? r) {
                return l.GetValueOrDefault() * r.GetValueOrDefault();
            }
            public static int Div(int? l, int? r) {
                return l.GetValueOrDefault() / r.GetValueOrDefault();
            }
            public static bool And(bool? l, bool? r) {
                return l.GetValueOrDefault() && r.GetValueOrDefault();
            }
            public static bool Or(bool? l, bool? r) {
                return l.GetValueOrDefault() || r.GetValueOrDefault();
            }
            public static bool ExOr(bool? l, bool? r) {
                return l.GetValueOrDefault() ^ r.GetValueOrDefault();
            }
            public static int ShL(int? l, int? r) {
                return l.GetValueOrDefault() << r.GetValueOrDefault();
            }
            public static int ShR(int? l, int? r) {
                return l.GetValueOrDefault() >> r.GetValueOrDefault();
            }
            public static int Mod(int? l, int? r) {
                return l.GetValueOrDefault() % r.GetValueOrDefault();
            }
            public static double Pow(double? l, double? r) {
                return Math.Pow(l.GetValueOrDefault(), r.GetValueOrDefault());
            }
        }

        public static void Positive_AddAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var add = typeof(OpOverloads).GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.AddAssign(x, one, add, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?, int?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(100), 101);
                EU.Equal(f(null), 1);
            });
        }

        public static void Positive_AddAssignCheckedWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var add = typeof(OpOverloads).GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.AddAssignChecked(x, one, add, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?, int?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(100), 101);
                EU.Equal(f(null), 1);
            });
        }

        public static void Positive_SubtractAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var sub = typeof(OpOverloads).GetMethod("Sub", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.SubtractAssign(x, one, sub, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?, int?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(100), 99);
                EU.Equal(f(null), -1);
            });
        }

        public static void Positive_SubtractAssignCheckedWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var sub = typeof(OpOverloads).GetMethod("Sub", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.SubtractAssignChecked(x, one, sub, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?, int?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(100), 99);
                EU.Equal(f(null), -1);
            });
        }

        public static void Positive_MultiplyAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var mul = typeof(OpOverloads).GetMethod("Mul", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.MultiplyAssign(x, one, mul, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?, int?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(100), 100);
                EU.Equal(f(null), 0);
            });
        }

        public static void Positive_MultiplyAssignCheckedWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var mul = typeof(OpOverloads).GetMethod("Mul", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.MultiplyAssignChecked(x, one, mul, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?, int?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(100), 100);
                EU.Equal(f(null), 0);
            });
        }

        public static void Positive_DivideAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var div = typeof(OpOverloads).GetMethod("Div", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.DivideAssign(x, one, div, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?, int?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(100), 100);
                EU.Equal(f(null), 0);
            });
        }

        public static void Positive_AndAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(bool?), "x");
            var c = Expression.Constant(true, typeof(bool?));
            var p = Expression.Parameter(typeof(bool), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(bool?)), p);

            //The overload method takes two nullables and return bool.
            var and = typeof(OpOverloads).GetMethod("And", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.AndAssign(x, c, and, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<bool?, bool?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(true), true);
                EU.Equal(f(false), false);
                EU.Equal(f(null), false);
            });
        }

        public static void Positive_OrAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(bool?), "x");
            var c = Expression.Constant(true, typeof(bool?));
            var p = Expression.Parameter(typeof(bool), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(bool?)), p);

            //The overload method takes two nullables and return bool.
            var or = typeof(OpOverloads).GetMethod("Or", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.OrAssign(x, c, or, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<bool?, bool?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(true), true);
                EU.Equal(f(false), true);
                EU.Equal(f(null), true);
            });
        }

        public static void Positive_ExclusiveOrAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(bool?), "x");
            var c = Expression.Constant(true, typeof(bool?));
            var p = Expression.Parameter(typeof(bool), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(bool?)), p);

            //The overload method takes two nullables and return bool.
            var exOr = typeof(OpOverloads).GetMethod("ExOr", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.ExclusiveOrAssign(x, c, exOr, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<bool?, bool?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(true), false);
                EU.Equal(f(false), true);
                EU.Equal(f(null), true);
            });
        }

        public static void Positive_LeftShiftAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var sh = typeof(OpOverloads).GetMethod("ShL", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.LeftShiftAssign(x, one, sh, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?, int?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(32), 64);
                EU.Equal(f(null), 0);
            });
        }

        public static void Positive_RightShiftAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var sh = typeof(OpOverloads).GetMethod("ShR", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.RightShiftAssign(x, one, sh, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?, int?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(32), 16);
                EU.Equal(f(null), 0);
            });
        }

        public static void Positive_ModuloAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(10, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var mod = typeof(OpOverloads).GetMethod("Mod", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.ModuloAssign(x, one, mod, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?, int?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(35), 5);
                EU.Equal(f(null), 0);
            });
        }

        public static void Positive_PowerAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(double?), "x");
            var one = Expression.Constant((double)2, typeof(double?));
            var p = Expression.Parameter(typeof(double), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(double?)), p);

            //The overload method takes two nullables and return int.
            var pow = typeof(OpOverloads).GetMethod("Pow", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.PowerAssign(x, one, pow, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<double?, double?>>(body, x);
            V.Validate(le, f =>
            {
                EU.Equal(f(3.0), 9.0);
                EU.Equal(f(null), 0.0);
            });
        }

        public class TestMemberOpAssign {
            private int? _x;
            public int? X {
                get { return _x; }
                set { _x = value; }
            }
        }
        //Verify when the LHS of opAssign is member access
        public static void Positive_MemberAddAssignWithConversionLambda(EU.IValidator V) {
            var obj = Expression.Parameter(typeof(TestMemberOpAssign), "obj");
            var x = Expression.Property(obj, "X");
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var add = typeof(OpOverloads).GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.AddAssign(x, one, add, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<TestMemberOpAssign, int?>>(body, obj);
            V.Validate(le, f =>
            {
                TestMemberOpAssign o = new TestMemberOpAssign();
                o.X = 100;
                EU.Equal(f(o), 101);
                o.X = null;
                EU.Equal(f(o), 1);
            });
        }

        //Verify when the LHS of opAssign is member access
        public static void Positive_IndexAddAssignWithConversionLambda(EU.IValidator V) {
            var a = Expression.Parameter(typeof(int?[]), "a");
            var i = Expression.Parameter(typeof(int), "i");
            var x = Expression.ArrayAccess(a, i);
            var one = Expression.Constant(1, typeof(int?));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //The overload method takes two nullables and return int.
            var add = typeof(OpOverloads).GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
            var e = Expression.AddAssign(x, one, add, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<int?[], int, int?>>(body, a, i);
            V.Validate(le, f =>
            {
                int?[] array = new int?[] { 1, 10, 100, null };
                EU.Equal(f(array, 0), 2);
                EU.Equal(f(array, 1), 11);
                EU.Equal(f(array, 2), 101);
                EU.Equal(f(array, 3), 1);
            });
        }

        public static void Negative_AddAssignWithConversionLambda(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var one = Expression.Constant(1, typeof(int?));
            //The overload method takes two nullables and return int.
            var add = typeof(OpOverloads).GetMethod("Add", BindingFlags.Public | BindingFlags.Static);

            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            //Conversion must takes one parameter.
            conversion = Expression.Lambda(Expression.Constant(0));
            EU.Throws<ArgumentException>(() => Expression.AddAssign(x, one, add, conversion));

            //The return type of conversion lambda must be the same as the type of left expression.
            p = Expression.Parameter(typeof(int), "p");
            conversion = Expression.Lambda(Expression.Constant(""), p);
            EU.Throws<InvalidOperationException>(() => Expression.AddAssign(x, one, add, conversion));

            //The parameter type of conversion lambda must be the same as the return type of overload method.
            p = Expression.Parameter(typeof(string), "p");
            conversion = Expression.Lambda(Expression.Constant(0, typeof(int?)), p);
            EU.Throws<InvalidOperationException>(() => Expression.AddAssign(x, one, add, conversion));
        }

        public struct BoxedInt {
            public int X;
            public BoxedInt(int x) {
                X = x;
            }
            //overload the + operator
            public static BoxedInt operator +(BoxedInt x1, BoxedInt x2) {
                return new BoxedInt(x1.X + x2.X);
            }

            public static BoxedInt operator +(BoxedInt? x1, BoxedInt? x2) {
                if (x1 == null || x2 == null) {
                    return new BoxedInt(0);
                }
                return new BoxedInt(x1.Value.X + x2.Value.X);
            }
        }

        public static void Negative_AddAssignWithoutConversionLambdaAndOperatorOverloading(EU.IValidator V) {
            var x = Expression.Parameter(typeof(BoxedInt?), "x");
            var one = Expression.Constant(new BoxedInt(1), typeof(BoxedInt?));
            EU.Throws<ArgumentException>(() => Expression.AddAssign(x, one));
        }

        public static void Positive_AddAssignWithConversionLambdaAndOperatorOverloading(EU.IValidator V) {
            var x = Expression.Parameter(typeof(BoxedInt?), "x");
            var one = Expression.Constant(new BoxedInt(1), typeof(BoxedInt?));
            var p = Expression.Parameter(typeof(BoxedInt), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(BoxedInt?)), p);

            var e = Expression.AddAssign(x, one, null, conversion);
            var body = Expression.Block(e, x);
            var le = Expression.Lambda<Func<BoxedInt?, BoxedInt?>>(body, x);
            V.Validate(le, f =>
            {
                BoxedInt? bi = new BoxedInt(100);
                EU.Equal(f(bi).Value.X, 101);
                EU.Equal(f(null).Value.X, 0);
            });
        }

        public static void Negative_AddAssignTwoPrimitiveWithConversion(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var one = Expression.Constant(1, typeof(int));
            var p = Expression.Parameter(typeof(int), "p");
            var conversion = Expression.Lambda(Expression.Convert(p, typeof(int?)), p);

            EU.Throws<InvalidOperationException>(() => Expression.AddAssign(x, one, null, conversion));
        }
        #endregion

        public class TestClassBug542728 {
            static public int x = 123;
        }

        // Verify applying OpAssign operations on static members.
        // See Dev10 bug 542728
        public static Expression Positive_OpAssignWithStaticFields(EU.IValidator V) {
            var left = Expression.Field(null, typeof(TestClassBug542728).GetField("x"));
            // left is 123
            // left += 1
            // left +=(checked) 1
            // left -= 1
            // left -=(checked) 1
            // left *= 1
            // left *=(checked) 1
            // left /= 1
            // left %= 124
            // result is 123
            var c = Expression.Constant(1);
            Expression body = Expression.Block(
                Expression.AddAssign(left, c),
                Expression.AddAssignChecked(left, c),
                Expression.SubtractAssign(left, c),
                Expression.SubtractAssignChecked(left, c),
                Expression.MultiplyAssign(left, c),
                Expression.MultiplyAssignChecked(left, c),
                Expression.DivideAssign(left, c),
                Expression.ModuloAssign(left, Expression.Constant(124))
            );

            var e = Expression.Lambda<Func<int>>(body);
            V.Validate(e, f =>
            {
                EU.Equal(f(), 123);
            });
            return body;
        }

        public static Expression Positive_OpAssign(EU.IValidator V) {
            // local = 100
            // local += 20
            // local
            ParameterExpression var = Expression.Variable(typeof(int), "local");
            Expression body = Expression.Block(
                Expression.Assign(var, Expression.Constant(100)),
                Expression.AddAssign(var, Expression.Constant(20)),
                var
            );

            var le = Expression.Lambda<Func<int>>(Expression.Block(new[] { var }, body));

            V.Validate(le, s =>
            {
                EU.Equal(s(), 120);
            });
            return le;
        }

        public static Expression Positive_OpAssign2(EU.IValidator V) {
            // oat = new OpAssignmentTest()
            // oat.field = *= 7
            // oat.property /= 4
            // oat.field + oat.Property
            ParameterExpression oat = Expression.Variable(typeof(OpAssignmentTest), "oat");

            Expression body = Expression.Block(
                Expression.Assign(oat, Expression.New(typeof(OpAssignmentTest))),
                Expression.MultiplyAssign(Expression.Field(oat, typeof(OpAssignmentTest).GetField("Field")), Expression.Constant(7)),
                Expression.DivideAssign(Expression.Property(oat, typeof(OpAssignmentTest).GetProperty("Property")), Expression.Constant(4)),
                Expression.Add(
                    Expression.Field(oat, typeof(OpAssignmentTest).GetField("Field")),
                    Expression.Property(oat, typeof(OpAssignmentTest).GetProperty("Property"))
                )
            );

            var le = Expression.Lambda<Func<int>>(Expression.Block(new[] { oat }, body));

            V.Validate(le, s =>
            {
                EU.Equal(s(), 10 * 7 + 20 / 4);
            });
            return le;
        }

        public static Expression Positive_OpAssign3(EU.IValidator V) {
            ParameterExpression array = Expression.Variable(typeof(int[]), "array");

            Expression body = Expression.Block(
                // array = new int[] { 7, 11 };
                Expression.Assign(array, Expression.NewArrayInit(typeof(int), Expression.Constant(7), Expression.Constant(11))),

                // array[0] *= array[1]
                Expression.MultiplyAssign(
                    Expression.ArrayAccess(array, Expression.Constant(0)),
                    Expression.ArrayAccess(array, Expression.Constant(1))
                ),

                // array[0] + array[1]
                Expression.Add(
                    Expression.ArrayAccess(array, Expression.Constant(0)),
                    Expression.ArrayAccess(array, Expression.Constant(1))
                )
            );

            var le = Expression.Lambda<Func<int>>(Expression.Block(new[] { array }, body));

            V.Validate(le, s =>
            {
                EU.Equal(s(), 88);
            });
            return le;
        }

        public static void Positive_OpAssignChecked(EU.IValidator V) {
            // local = 100
            // local checked(+= 20)
            // local
            ParameterExpression var = Expression.Variable(typeof(int), "local");
            Expression body = Expression.Block(
                Expression.Assign(var, Expression.Constant(100)),
                Expression.AddAssignChecked(var, Expression.Constant(20)),
                var
            );

            var le = Expression.Lambda<Func<int>>(Expression.Block(new[] { var }, body));
            
            V.Validate(le, s =>
            {
                EU.Equal(s(), 120);
            });
        }

        public static void Negative_OpAssignChecked(EU.IValidator V) {
            // overflow exception
            ParameterExpression var = Expression.Variable(typeof(int), "local");
            Expression body = Expression.Block(
                Expression.Assign(var, Expression.Constant(int.MaxValue)),
                Expression.MultiplyAssignChecked(var, Expression.Constant(20)),
                var
            );

            var le = Expression.Block(new[] { var }, body);
            V.ValidateException<OverflowException>(le, false);
        }

        //Dev10 bug #550086
        public static void Negative_OpMultiplyAssignCheckedLeftNoWritable(EU.IValidator V) {
            Expression left = Expression.Block(Expression.Constant(1));
            EU.Throws<ArgumentException>(
                () => Expression.MultiplyAssignChecked(left, Expression.Constant(1))
            );
        }

        #region Dev10 bug #550216
        public class TestBug550216 {
            public static int Foo(int? arg1, int? arg2) {
                return (arg1 + arg2).GetValueOrDefault();
            }
        }

        //Verify throw when return type of the operator method is not assignable to the left.
        //typeof(int) is not assignable to typeof(int?)
        public static void Negative_OpAssignReturnType(EU.IValidator V) {
            MethodInfo mi = typeof(TestBug550216).GetMethod("Foo");
            ParameterExpression left = Expression.Parameter(typeof(int?), "");
            EU.Throws<ArgumentException>(
                () => Expression.AddAssign(left, Expression.Constant(1, typeof(int?)), mi)
            );

            EU.Throws<ArgumentException>(
                () => Expression.AddAssignChecked(left, Expression.Constant(1, typeof(int?)), mi)
            );

            EU.Throws<ArgumentException>(
                () => Expression.SubtractAssign(left, Expression.Constant(1, typeof(int?)), mi)
            );

            EU.Throws<ArgumentException>(
                () => Expression.SubtractAssignChecked(left, Expression.Constant(1, typeof(int?)), mi)
            );

            EU.Throws<ArgumentException>(
                () => Expression.MultiplyAssign(left, Expression.Constant(1, typeof(int?)), mi)
            );

            EU.Throws<ArgumentException>(
                () => Expression.MultiplyAssignChecked(left, Expression.Constant(1, typeof(int?)), mi)
            );
        }
        #endregion

        //Assignment to ArrayIndex is not supported.
        public static void Positive_OpAssignArrayIndexExpression(EU.IValidator V) {
            var a = Expression.Parameter(typeof(int[]), "a");
            //a = new int[] {1, 2, 3, 4, 5}
            var expr1 = Expression.Assign(a, Expression.Constant(new int[] { 1, 2, 3, 4, 5 }));
            //a[1]
            var expr2 = Expression.ArrayIndex(a, Expression.Constant(1));

            //a[1] += 10
            EU.Throws<ArgumentException>(() => Expression.AddAssign(expr2, Expression.Constant(10)));

            //var le = Expression.Lambda<Func<int>>(Expression.Block(new[] { a }, expr1, expr3));
            //var s = le.Compile();
            //EU.Equal(s(), 12);
        }

        // Per ecma spec (Partition I 12.4.2)
        public static void Negative_TryFaultWithCatch(EU.IValidator V) {
            EU.Throws<ArgumentException>(
                () => Expression.MakeTry(null, Expression.Empty(), null, Expression.Empty(), new[] { Expression.Catch(typeof(Exception), Expression.Empty()) })
            );
        }

        //Assignment to ArrayIndex is not supported.
        public static void Negative_AssignArrayIndexExpression(EU.IValidator V) {
            var a = Expression.Parameter(typeof(int[]), "a");
            //a = new int[] {1, 2, 3, 4, 5}
            var expr1 = Expression.Assign(a, Expression.Constant(new int[] { 1, 2, 3, 4, 5 }));
            //a[1]
            var expr2 = Expression.ArrayIndex(a, Expression.Constant(1));
            //a[1] = 10
            EU.Throws<ArgumentException>(() => Expression.Assign(expr2, Expression.Constant(10)));

            //var le = Expression.Lambda<Func<int>>(Expression.Block(new[] { a }, expr1, expr3, expr2));
            //var s = le.Compile();
            //EU.Equal(s(), 10);
        }

        // test invoke where invoked thing returns a LambdaExpression instance
        public static Expression Positive_InvokeLambdaInstance(EU.IValidator V) {

            // Invoke((int x, int y) => x + y, 123, 444)
            Expression invoke = Expression.Invoke(
                Expression.Call(typeof(Scenarios).GetMethod("MakeRuntimeLambda")),
                Expression.Constant(123),
                Expression.Constant(444)
            );

            var e = Expression.Lambda<Action>(MakeAreEqual(invoke, Expression.Constant(567)));
            V.Validate(e);
            return e;
        }

        public static Expression<Func<int, int, int>> MakeRuntimeLambda() {
            // (int x, int y) => x + y
            ParameterExpression x = Expression.Parameter(typeof(int), "x");
            ParameterExpression y = Expression.Parameter(typeof(int), "y");
            return Expression.Lambda<Func<int, int, int>>(Expression.Add(x, y), x, y);
        }

        // Test NewExpression
        public static Expression Positive_NewExpression(EU.IValidator V) {
            var dt = Expression.Lambda<Func<DateTime>>(Expression.New(typeof(DateTime)));
            V.Validate(dt, f =>
            {
                EU.Equal(f(), new DateTime());
            });

            var e = Expression.Lambda<Func<List<int>>>(Expression.New(typeof(List<int>)));
            V.Validate(e, list =>
            {
                EU.Equal(((List<int>)list()).Count, 0);
            });
            return e;
        }

        // Test ConstantExpression on boxed constant
        public static void Positive_BoxedConstant(EU.IValidator V) {
            // test reference equality
            object x = 1.23, y = 1.23;

            var e1 = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Constant(x, typeof(object)),
                    Expression.Constant(x, typeof(object))
                )
            );

            V.Validate(e1, result =>
            {
                EU.Equal(result(), true); // same boxed object is equal
            });

            e1 = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Constant(x, typeof(object)),
                    Expression.Constant(y, typeof(object))
                )
            );
            V.Validate(e1, result =>
            {
                EU.Equal(result(), false); // different boxed objects aren't equal
            });

            e1 = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Constant(x),
                    Expression.Constant(y)
                )
            );
            V.Validate(e1, result =>
            {
                EU.Equal(result(), true); // same value is equal, even from different boxed objects
            });

            e1 = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Constant(x),
                    Expression.Constant(4.44)
                )
            );
            V.Validate(e1, result =>
            {
                EU.Equal(result(), false); // different value isn't equal
            });
        }

        // Test passing an element of a multidimensional array byref
        public static void Positive_ByRefMultidimensioArray(EU.IValidator V) {
            ParameterExpression v = Expression.Variable(typeof(int[,]), "array");

            var arrayIndex = Expression.ArrayIndex(v, Expression.Constant(0), Expression.Constant(0));

            var e1 = Expression.Lambda<Func<int[,]>>(
                Expression.Block(
                    new[] { v },
                    Expression.Assign(v, Expression.NewArrayBounds(typeof(int), Expression.Constant(1), Expression.Constant(1))),
                    Expression.Call(
                        typeof(Scenarios).GetMethod("TestRef"),
                        arrayIndex
                    ),
                    v
                )
            );

            V.Validate(e1, result =>
            {
                EU.Equal(result()[0, 0], 123);
            });

            // test ArrayAccess
            e1 = Expression.Lambda<Func<int[,]>>(
                Expression.Block(
                    new[] { v },
                    Expression.Assign(v, Expression.NewArrayBounds(typeof(int), Expression.Constant(1), Expression.Constant(1))),
                    Expression.Call(
                        typeof(Scenarios).GetMethod("TestRef"),
                        Expression.ArrayAccess(v, Expression.Constant(0), Expression.Constant(0))
                    ),
                    v
                )
            );
            V.Validate(e1, result =>
            {
                EU.Equal(result()[0, 0], 123);
            });
        }

        public static void TestRef(ref int x) {
            x += 123;
        }

        public class TestRefCtor {
            public TestRefCtor(ref int x) {
                x += 123;
            }
        }

        private static Expression TestWriteBack(Func<Expression, Expression> makeRefExpr, EU.IValidator V) {
            int getTestCalled = 0;
            var test = new TestClass();
            Func<TestClass> getTest = delegate() {
                getTestCalled++;
                return test;
            };

            int getArgCalled = 0;
            Func<string> getArg = delegate() {
                getArgCalled++;
                return "abc";
            };

            var getTestExpr = Expression.Invoke(Expression.Constant(getTest));
            var getArgExpr = Expression.Invoke(Expression.Constant(getArg));

            TestClass.Y = 555;

            var e = Expression.Lambda<Action>(
                Expression.Block(
                // instance property
                    makeRefExpr(Expression.Property(getTestExpr, "X")),

                    // static property
                    makeRefExpr(Expression.Property(null, typeof(TestClass), "Y")),

                    // indexed property
                    makeRefExpr(Expression.Property(getTestExpr, typeof(TestClass).GetProperty("Item"), getArgExpr, getArgExpr)),

                    Expression.Empty()
                )
            );

            V.Validate(e, f =>
            {
                f();
                EU.Equal(getTestCalled, 2);
                EU.Equal(getArgCalled, 2);
                EU.Equal(test.X, 234);
                EU.Equal(TestClass.Y, 678);
                EU.Equal(test["a", "bcabc"], 123);
            });
            return e;
        }

        // Test writeback into property
        public static void Positive_PropertyWriteback(EU.IValidator V) {
            // MethodCallExpression
            TestWriteBack(p => Expression.Call(typeof(Scenarios).GetMethod("TestRef"), p), V);

            // NewExpression
#if SILVERLIGHT
            TestWriteBack(p => Expression.New(typeof(Scenarios).GetNestedType("TestRefCtor", BindingFlags.Public).GetConstructors()[0], p), V);
#else
            TestWriteBack(p => Expression.New(typeof(Scenarios).GetNestedType("TestRefCtor").GetConstructors()[0], p), V);
#endif
        }

        public static void Positive_UnitializedLocal(EU.IValidator V) {
            // value type
            ParameterExpression v = Expression.Variable(typeof(TimeSpan), null);
            var e = Expression.Lambda<Func<TimeSpan>>(Expression.Block(new[] { v }, v));
            V.Validate(e, f =>
            {
                EU.Equal(new TimeSpan(), f());
            });

            // reference type
            v = Expression.Variable(typeof(Exception), null);
            var e2 = Expression.Lambda<Func<Exception>>(Expression.Block(new[] { v }, v));
            V.Validate(e2, f2 =>
            {
                EU.Equal(null, f2());
            });
        }

        public static void Positive_NullParameterVariableName(EU.IValidator V) {
            ParameterExpression p = Expression.Parameter(typeof(int));
            ParameterExpression v = Expression.Variable(typeof(int));

            var l = Expression.Lambda<Func<int, int>>(
                Expression.Block(
                    new[] { v },
                    Expression.Assign(v, Expression.Constant(7)),
                    Expression.Add(p, v)
                ),
                p
            );

            V.Validate(l, d =>
            {
                EU.Equal(d(13), 20);
            });
        }

        public static void Positive_UnitializedHoisted(EU.IValidator V) {
            // value type
            ParameterExpression v = Expression.Variable(typeof(TimeSpan), null);
            var e = Expression.Lambda<Func<TimeSpan>>(
                Expression.Block(new[] { v }, Expression.Invoke(Expression.Lambda<Func<TimeSpan>>(v)))
            );
            V.Validate(e, f =>
            {
                EU.Equal(new TimeSpan(), f());
            });

            // reference type

            v = Expression.Variable(typeof(Exception), null);
            var e2 = Expression.Lambda<Func<Exception>>(
                Expression.Block(new[] { v }, Expression.Invoke(Expression.Lambda<Func<Exception>>(v)))
            );
            V.Validate(e2, f2 =>
            {
                EU.Equal(null, f2());
            });
        }

        // Verify we can call a user-defined equals method which returns a
        // non-bool value. See Dev10 bug 414397
        public static Expression Positive_LiftedNonBoolEqual(EU.IValidator V) {
            Expression left = Expression.Constant(null, typeof(int?));
            Expression right = Expression.Constant(0, typeof(int?));
            Expression add = Expression.Equal(left, right, false, typeof(Scenarios).GetMethod("NonBoolEqual"));
            var e = Expression.Lambda<Func<int?>>(add);
            V.Validate(e, fint =>
            {
                EU.Equal(null, fint());
            });
            return e;
        }

        public static int NonBoolEqual(int a, int b) { return 0; }

        // Verify the passing an instance to a static call throws
        // See Dev10 bug 414439
        public static void Negative_StaticWithInstance(EU.IValidator V) {
            EU.Throws<ArgumentException>(
                () => Expression.Call(
                    Expression.Constant(123),
                    typeof(int).GetMethod("Parse", new Type[] { typeof(string) }),
                    Expression.Constant("456")
                )
            );
            EU.Throws<ArgumentException>(
                () => Expression.Field(
                    Expression.Constant("abc"),
                    typeof(string).GetField("Empty")
                )
            );
            EU.Throws<ArgumentException>(
                () => Expression.Property(
                    Expression.Constant(null),
                    typeof(Environment).GetProperty("CurrentDirectory")
                )
            );
        }

        // this needs to stay as a as non-visible type
        struct S : IEquatable<S> {
            public override bool Equals(object o) {
                return (o is S) && Equals((S)o);
            }

            public bool Equals(S other) {
                return true;
            }

            public override int GetHashCode() {
                return 0;
            }
        }
        /*
        public static Expression Positive_NonVisibleStructArray(EU.IValidator V) {
            S[] x = null, y = new S[0];
            var e = Expression.Lambda<Func<S[]>>(
                Expression.Condition(
                    Expression.Constant(false, typeof(bool)),
                    Expression.Constant(x, typeof(S[])),
                    Expression.Constant(y, typeof(S[]))
                )
            );
            
            V.Validate(e, z =>
            {
                EU.Equal(z(), y);
            });
            return e;
        }*/

        // make sure we don't emit string arrays directly into the IL stream
        public static Expression Positive_StringArrayIdentity(EU.IValidator V) {
            string[] foo = new string[] { "abc", "def" };
            var e = Expression.Lambda<Func<string[]>>(Expression.Constant(foo));
            V.Validate(e, bar =>
            {
                EU.Equal(foo, bar());
            });
            return e;
        }


        public static void TestRefString(ref string s) { s = "abc"; }

        // DevDiv bug 144293
        public static Expression Positive_ArrayElementAddress(EU.IValidator V) {
            string[] arr = new string[1];

            var arrayIndex = Expression.ArrayIndex(Expression.Constant(arr), Expression.Constant(0, typeof(int)));

            var e1 = Expression.Lambda<Action>(
                Expression.Call(
                    typeof(Scenarios).GetMethod("TestRefString"),
                    arrayIndex
                )
            );
            V.Validate<Action>(e1, f =>
            {
                f();
                EU.Equal(arr[0], "abc");
            });

            // test ArrayAccess
            arr = new string[1];

            var e = Expression.Lambda<Action>(
                Expression.Call(
                    typeof(Scenarios).GetMethod("TestRefString"),
                    Expression.ArrayAccess(Expression.Constant(arr), Expression.Constant(0, typeof(int)))
                )
            );
            V.Validate<Action>(e, f =>
            {
                f();
                EU.Equal(arr[0], "abc");
            });
            return e;
        }

        // Verify that we don't try to ldtoken on a DynamicMethod
        public static Expression Positive_DynamicMethodAsConstant(EU.IValidator V) {
            DynamicMethod d = new DynamicMethod("constant", typeof(int), new Type[] { typeof(int), typeof(int) });
            ILGenerator gen = d.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Add);
            gen.Emit(OpCodes.Ret);
            var e = Expression.Lambda<Func<Func<int, int, int>>>(
                Expression.Convert(
                    Expression.Call(
                        Expression.Constant(d),
                        typeof(DynamicMethod).GetMethod("CreateDelegate", new Type[] { typeof(Type) }),
                        Expression.Constant(typeof(Func<int, int, int>), typeof(Type))
                    ),
                    typeof(Func<int, int, int>)
                )
            );
            V.Validate(e, test =>
            {
                EU.Equal(test()(123, 444), 567);
            });
            return e;
        }

        private static string PrivateMethod() {
            return "secret";
        }

        public static Expression Positive_NonPublicMethodConstant(EU.IValidator V) {
            MethodInfo m = typeof(Scenarios).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Static);
            var e = Expression.Lambda<Func<MethodInfo>>(Expression.Constant(m, typeof(MethodInfo)));
            V.Validate(e, m2 =>
            {
                EU.Equal(m, m2());
            });
            return e;
        }

        public static Expression Positive_ConstructorConstant(EU.IValidator V) {
            ConstructorInfo c = typeof(TestClass).GetConstructor(Type.EmptyTypes);
            var e = Expression.Lambda<Func<ConstructorInfo>>(Expression.Constant(c, typeof(ConstructorInfo)));
            V.Validate(e, c2 =>
            {
                EU.Equal(c, c2());
            });
            return e;
        }

        public static Expression Positive_CtorMethodHandleConst(EU.IValidator V) {
            ConstructorInfo c = typeof(TestClass).GetConstructor(Type.EmptyTypes);
            var e = Expression.Lambda<Func<RuntimeMethodHandle>>(Expression.Constant(c.MethodHandle));
            V.Validate(e, f =>
            {
                RuntimeMethodHandle rmh = f();
                MethodBase c2 = MethodBase.GetMethodFromHandle(rmh);
                EU.Equal(c, c2);
            });
            return e;
        }

        #region op_True/op_False test classes

        public class T1 {
            internal static string _history = "";
            public static T1 operator |(T1 x, T1 y) {
                _history += "T1.op_BitwiseOr;";
                return new T1();
            }
            public static T1 operator &(T1 x, T1 y) {
                _history += "T1.op_BitwiseAnd;";
                return new T1();
            }
            public static bool operator true(T1 x) {
                _history += "T1.op_True;";
                return x != null;
            }
            public static bool operator false(T1 x) {
                _history += "T1.op_False;";
                return x == null;
            }
        }
        // Missing SpecialName bit
        public class T2 : T1 {
            public static T2 operator |(T2 x, T2 y) {
                _history += "T2.op_BitwiseOr;";
                return new T2();
            }
            public static T2 operator &(T2 x, T2 y) {
                _history += "T2.op_BitwiseAnd;";
                return new T2();
            }
            public static bool op_True(T2 x) {
                _history += "T2.op_True;";
                return x != null;
            }
            public static bool op_False(T2 x) {
                _history += "T2.op_False;";
                return x == null;
            }
        }
        // Generic parameters
        public class T3 : T1 {
            public static T3 operator |(T3 x, T3 y) {
                _history += "T3.op_BitwiseOr;";
                return new T3();
            }
            public static T3 operator &(T3 x, T3 y) {
                _history += "T3.op_BitwiseAnd;";
                return new T3();
            }
            [SpecialName]
            public static bool op_True<T>(T3 x) {
                _history += "T3.op_True;";
                return x != null;
            }
            [SpecialName]
            public static bool op_False<T>(T3 x) {
                _history += "T3.op_False;";
                return x == null;
            }
        }

        #endregion

        public static void Positive_BooleanOperatorResolution(EU.IValidator V) {
            TestBooleanOp<T1>(V);
            TestBooleanOp<T2>(V);
            TestBooleanOp<T3>(V);
        }

        // Dev10 bug 451369
        // We need to lookup op_True and op_False in the class heirarchy,
        // skipping over method that aren't marked with SpecialName or have
        // generic parameters
        private static void TestBooleanOp<T>(EU.IValidator V) where T : T1, new() {
            ParameterExpression x = Expression.Parameter(typeof(T), "x");

            var e2 = Expression.Lambda<Func<T, T1>>(Expression.OrElse(x, x), x);
            V.Validate(e2, f2 =>
            {
                f2(new T());
                EU.Equal(T1._history, "T1.op_True;"); T1._history = "";
                f2(null);
                EU.Equal(T1._history, "T1.op_True;" + typeof(T).Name + ".op_BitwiseOr;"); T1._history = "";
            });            

            e2 = Expression.Lambda<Func<T, T1>>(Expression.AndAlso(x, x), x);
            V.Validate(e2, f2 =>
            {
                f2(null);
                EU.Equal(T1._history, "T1.op_False;"); T1._history = "";
                f2(new T());
                EU.Equal(T1._history, "T1.op_False;" + typeof(T).Name + ".op_BitwiseAnd;"); T1._history = "";
            });
        }

        public interface ITestCalls {
            int X { get; }
            int SetXProp { get; }
            int SetXPropSetter { set; }
            int SetX();
        }

        public struct STestCalls : ITestCalls {
            public int X { get; private set; }
            public int SetXProp {
                get {
                    X += 11;
                    return 0;
                }
            }
            public int SetXPropSetter {
                set {
                    X += 200;
                }
            }
            public int SetX() {
                X = 7;
                return 0;
            }
        }

        // Dev10 bug 450176
        //
        // Method calls and property accesses on structs shouldn't box if
        // calling through an interface.
        public static Expression Positive_ValueTypeInterfaceCalls(EU.IValidator V) {
            ParameterExpression x = Expression.Parameter(typeof(STestCalls), "x");
            var e = Expression.Lambda<Func<STestCalls, int>>(
                Expression.AddChecked(
                    Expression.AddChecked(
                        Expression.AddChecked(
                            Expression.Call(x, typeof(ITestCalls).GetMethod("SetX")),
                            Expression.Property(x, typeof(ITestCalls).GetMethod("get_SetXProp"))
                        ),
                        Expression.Assign(
                            Expression.Property(x, typeof(ITestCalls).GetMethod("set_SetXPropSetter")),
                            Expression.Constant(0)
                        )
                    ),
                    Expression.Property(x, typeof(ITestCalls).GetMethod("get_X"))
                ),
                x
            );
            
            V.Validate(e, f =>
            {
                var s = new STestCalls();
                int result = f(s);
                EU.Equal(result, 218);
            });

            return e;
        }

        // Dev10 bug 445623
        public static Expression Positive_CoalesceUserDefinedConversion(EU.IValidator V) {
            ParameterExpression s = Expression.Parameter(typeof(string), "s");
            ParameterExpression s2 = Expression.Parameter(typeof(string), "s");

            var e = Expression.Lambda<Func<string, int>>(
                Expression.Coalesce(
                    s,
                    Expression.Constant(42),
                    Expression.Lambda<Func<string, int>>(
                        Expression.Call(typeof(int).GetMethod("Parse", new Type[] { typeof(string) }), s2),
                        s2
                    )
                ),
                s
            );

            V.Validate(e, coalesce =>
            {
                EU.Equal(12, coalesce("12"));
                EU.Equal(42, coalesce(null));
            });
            return e;
        }

        // Dev10 bug 409921: generates unverifiable code
        public static Expression Positive_LiftedRelational(EU.IValidator V) {
            var e = Expression.LessThan(Expression.Constant(null, typeof(int?)), Expression.Constant(123, typeof(int?)), true, null);
            var f = Expression.Lambda<Func<bool?>>(e).Compile();
            EU.Equal(f(), null);
            return e;
        }

        public static Expression Positive_LiftedRelational2(EU.IValidator V) {
            var e = Expression.LessThan(Expression.Constant(null, typeof(int?)), Expression.Constant(123, typeof(int?)), false, null);
            var f = Expression.Lambda<Func<bool>>(e).Compile();
            EU.Equal(f(), false);
            return e;
        }

        public static void Negative_CatchBlockClosure(EU.IValidator V) {
            var i = Expression.Parameter(typeof(int), null);
            var ex = Expression.Parameter(typeof(Exception), null);
            var x = new List<Func<string>>();
            var target = Expression.Label();

            //int i = 0;
            //var x = new List<Func<string>>();
            //target: {
            //    try {
            //        throw new Exception(i.ToString());
            //    } catch (Exception ex) {
            //        x.Add(() => ex.Message);
            //    }
            //    if (++i < 10) {
            //        goto target;
            //    }
            //}
            var e = Expression.Lambda<Action>(
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
                            Expression.Call(
                                Expression.Constant(x),
                                "Add",
                                null,
                                Expression.Lambda(Expression.Property(ex, "Message"))
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
                f();

                for (int j = 0; j < 10; j++) {
                    EU.Equal(x[j](), j.ToString());
                    EU.Equal(x[j](), j.ToString());
                }
                for (int j = 0; j < 10; j++) {
                    EU.Equal(x[j](), j.ToString());
                    EU.Equal(x[j](), j.ToString());
                }
            });
        }

        public class ClassImplicitULong {
            public ClassImplicitULong() { }

            public static implicit operator ulong?(ClassImplicitULong x) {
                return null;
            }
        }

        public static Expression Positive_CoalesceUserDefinedConversion2(EU.IValidator V) {
            var p = Expression.Parameter(typeof(ClassImplicitULong), "CoalesceLHS");
            var c1 = new ClassImplicitULong();
            var e = Expression.Lambda<Func<ulong?>>(
                Expression.Coalesce(
                    Expression.Constant(c1),
                    Expression.Constant(null, typeof(ulong?)),
                    Expression.Lambda<Func<ClassImplicitULong, ulong?>>(
                        Expression.ConvertChecked(
                            p,
                            typeof(ulong?),
                            typeof(ClassImplicitULong).GetMethod("op_Implicit")
                        ),
                        p
                    )
                )
            );
            V.Validate(e);
            return e;
        }

#if !CLR2 // TODO: inline expression trees
        public static int NonVoidReturning() { return 123; }
        public static Expression Positive_NonVoidReturningBody(EU.IValidator V) {
            Expression<Action> compiled = () => new Action(() => NonVoidReturning())();
            V.Validate(compiled);
            return compiled;
        }
#endif

        public class Car<T, U> {
            public bool A(T x1, U x2) {
                return true;
            }

            public static bool B(T x1, U x2) {
                return true;
            }

            public bool C<V>(T x1, U x2, V x3) {
                return true;
            }

            public static bool D<V>(T x1, U x2, V x3) {
                return true;
            }

            public delegate bool ThreeDel<V>(T x1, U x2, V x3);
            public delegate bool TwoDel(T x1, U x2);
        }

        // We had a bug here--which was actually caused by emitting the
        // MethodInfo constant into IL. Turns out, you can't call
        // MethodBase.GetMethodFromHandle on a method that's on a generic type,
        // even if the method itself is non-generic and the type has no free
        // generic parameters.
        public static Expression Positive_DelegateGenericClass(EU.IValidator V) {
            var c = new Car<int, string>();

            var e = Expression.Lambda<Func<Car<int, string>.TwoDel>>(
                Expression.Convert(
                    Expression.Call(
                        typeof(Delegate).GetMethod("CreateDelegate", new[] { typeof(Type), typeof(object), typeof(MethodInfo), typeof(bool) }),
                        Expression.Constant(typeof(Car<int, string>.TwoDel), typeof(Type)),
                        Expression.Constant(c, typeof(object)),
                        Expression.Constant(typeof(Car<int, string>).GetMethod("A"), typeof(MethodInfo)),
                        Expression.Constant(false)
                    ),
                    typeof(Car<int, string>.TwoDel)
                )
            );

            V.Validate(e, del =>
            {
                EU.Equal(del()(1, "2"), true);
                EU.Equal(del().ToString(), "AstTest.Scenarios+Car`2+TwoDel[System.Int32,System.String]");
            });
            return e;
        }

        public class DecimalTestClosure {
            public decimal? X;
        }

        public static Expression Positive_BooleanWithValue(EU.IValidator V) {
            DecimalTestClosure c = new DecimalTestClosure();
            c.X = 10M;
            var e = Expression.Lambda<Func<bool?>>(
                Expression.NotEqual(
                    Expression.Field(Expression.Constant(c), typeof(DecimalTestClosure).GetField("X")),
                    Expression.Convert(Expression.Constant(null, typeof(object)), typeof(decimal?)),
                    true,
                    typeof(decimal).GetMethod("op_Inequality")
                )
            );
            V.Validate(e);
            return e;
        }

        // Can't create void variables/paramters
        public static void Negative_VoidVariables(EU.IValidator V) {
            EU.Throws<ArgumentException>(() => Expression.Variable(typeof(void), "x"));
            EU.Throws<ArgumentException>(() => Expression.Parameter(typeof(void), "x"));
        }

        // Regression for Dev10 bug 442512
        public static void Negative_NullMemberBinding(EU.IValidator V) {
            EU.Throws<ArgumentNullException>(
                () => Expression.MemberInit(
                    Expression.New(typeof(KeyValuePair<int, int>)),
                    new MemberBinding[] { null }
                )
            );
            EU.Throws<ArgumentNullException>(
                () => Expression.MemberBind(
                    typeof(KeyValuePair<int, int>).GetProperty("Key"),
                    new MemberBinding[] { null }
                )
            );
        }

        public static void Positive_NewExpressionMembers(EU.IValidator V) {
            var type = typeof(KeyValuePair<int, string>);
            var @new = Expression.New(
                type.GetConstructors()[0],
                new[] { Expression.Constant(123), Expression.Constant("OneTwoThree") },
                new[] { type.GetMethod("get_Key"), type.GetMethod("get_Value") }
            );
            EU.Equal(@new.Members[0], type.GetProperty("Key"));
            EU.Equal(@new.Members[1], type.GetProperty("Value"));
        }

        // Regression for Dev10 bug 489135
        public static void Positive_PropertyCreatedByRefEmit(EU.IValidator V)
        {
            //mark the assembly as transparent so it works in partial trust.
            var attributes = new[] { 
                new CustomAttributeBuilder(typeof(SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes), new object[0])
            };

#if SILVERLIGHT
            var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("foo"), AssemblyBuilderAccess.Run);
#else
            var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("foo"), AssemblyBuilderAccess.Run, attributes);
#endif
            var mb = ab.DefineDynamicModule("foo");
            var tb = mb.DefineType("TestClass");
            var getter = tb.DefineMethod("get_TestProperty", MethodAttributes.Public);
            var il = getter.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4_S, 123);
            il.Emit(OpCodes.Ret);
            var pb = tb.DefineProperty("TestProperty", PropertyAttributes.None, typeof(int), Type.EmptyTypes);
            pb.SetGetMethod(getter);
            var type = tb.CreateType();

            // In the factory validation code, we can't use
            // PropertyInfo.GetAccessors because PropertyBuilder does not
            // support it
            Expression.Property(Expression.Constant(null, type), pb);
        }

        public static Expression Negative_Rethrow(EU.IValidator V) {
            // Rethrow from try inside a catch
            var e = 
                Expression.TryCatch(
                    Expression.Divide(Expression.Constant(0), Expression.Constant(0)),
                    Expression.Catch(
                        typeof(DivideByZeroException),
                        Expression.TryFinally(Expression.Rethrow(typeof(int)), Expression.Empty())
                    )
                );

            V.ValidateException<DivideByZeroException>(e, false);
            return e;
        }

        // Regression for Dev10 bug 493210
        public static void Negative_Rethrow2(EU.IValidator V) {
            Expression Body = Expression.Empty();

            CatchBlock C1 = Expression.Catch(typeof(Exception), Expression.Empty());

            TryExpression MyTry = Expression.TryCatch(Body, C1);
            Expression LambdaBody = Expression.Block(Expression.Rethrow(), MyTry);

            var Lambda = Expression.Lambda(LambdaBody);

            V.ValidateException<InvalidOperationException>(Lambda, true);


            // rethrowing from a lambda inside a catch
            Expression innerBody = Expression.Rethrow();
            Expression catchExpr = Expression.Lambda(LambdaBody);

            CatchBlock C2 = Expression.Catch(typeof(Exception), Expression.Block(catchExpr, Expression.Empty()));
            TryExpression MyTry2 = Expression.TryCatch(Body, C2);

            var Lambda2 = Expression.Lambda(MyTry2);

            V.ValidateException<InvalidOperationException>(Lambda2, true);

            // Rethrow from finally inside a catch
            var Lambda3 = Expression.Lambda<Action>(
                Expression.TryCatch(
                    Expression.Divide(Expression.Constant(0), Expression.Constant(0)),
                    Expression.Catch(
                        typeof(DivideByZeroException),
                        Expression.TryFinally(Expression.Default(typeof(int)), Expression.Rethrow())
                    )
                )
            );

            V.ValidateException<InvalidOperationException>(Lambda3, true);
        }

        // Regression for Dev10 bug 510361
        public static void Negative_ElementInitNoArgs(EU.IValidator V) {
            var mi = typeof(int).GetMethod("ToString", new Type[] { });
            EU.Throws<ArgumentNullException>(
                () => Expression.ElementInit(mi, null)
            );
        }

        // Verify that CatchBlock only accepts true variables as target
        public static void Positive_CatchBlockVariableTarget(EU.IValidator V) {
            ParameterExpression v = Expression.Variable(typeof(InvalidOperationException), "e");
            EU.Equal(v.IsByRef, false);
            Expression.Catch(
                v,
                Expression.Empty());
        }

        public static void Negative_CatchBlockVariableTarget(EU.IValidator V) {
            ParameterExpression v = Expression.Parameter(typeof(InvalidOperationException).MakeByRefType(), "e");
            EU.Equal(v.IsByRef, true);
            EU.Throws<ArgumentException>(
                () => Expression.Catch(
                        v,
                        Expression.Empty())
            );
        }

        //verify that ScopeExpression only accepts true variables
        public static Expression Positive_ScopeVariables(EU.IValidator V) {
            ParameterExpression v1 = Expression.Variable(typeof(int), "x");
            ParameterExpression v2 = Expression.Variable(typeof(string), "s");
            EU.Equal(v1.IsByRef, false);
            EU.Equal(v2.IsByRef, false);
            return Expression.Block(
                new ParameterExpression[] { v1, v2 },
                Expression.Empty()
            );
        }

        public static void Negative_ScopeVariables(EU.IValidator V) {
            ParameterExpression v1 = Expression.Parameter(typeof(int), "x");
            ParameterExpression v2 = Expression.Parameter(typeof(string).MakeByRefType(), "s");
            EU.Equal(v1.IsByRef, false);
            EU.Equal(v2.IsByRef, true);
            EU.Throws<ArgumentException>(
                () => Expression.Block(
                        new ParameterExpression[] { v1, v2 },
                        Expression.Empty()
                      )
            );
        }


        public static void TestLocalScope1(IRuntimeVariables scope) {
            EU.Equal(scope.Count, 3);
            EU.Equal((int)scope[0], 0);
            EU.Equal((int)scope[1], 0);
            EU.Equal((int)scope[2], 0);
        }

        public static void TestLocalScope2(IRuntimeVariables scope) {
            EU.Equal(scope.Count, 3);
            EU.Equal((int)scope[0], 123);
            EU.Equal((int)scope[1], 444);
            EU.Equal((int)scope[2], 0);

            scope[2] = 567;
            EU.Equal((int)scope[2], 567);
        }

        public static void Positive_RuntimeVariables(EU.IValidator V) {
            ParameterExpression a, b, x, y, z;

            x = Expression.Variable(typeof(int), "x");
            y = Expression.Variable(typeof(int), "y");
            z = Expression.Variable(typeof(int), "z");
            a = Expression.Variable(typeof(int), "a");
            b = Expression.Variable(typeof(int), "b");

            var e = Expression.Lambda<Func<int>>(
                Expression.Block(
                    new[] { x, y, z, a, b },
                    Expression.Assign(a, Expression.Constant(123)),
                    Expression.Assign(b, Expression.Constant(444)),
                    Expression.Call(typeof(Scenarios).GetMethod("TestLocalScope1"), Expression.RuntimeVariables(x, y, z)),
                    Expression.Assign(x, a),
                    Expression.Assign(y, b),
                    Expression.Call(typeof(Scenarios).GetMethod("TestLocalScope2"), Expression.RuntimeVariables(x, y, z)),
                    z
                )
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(), 567);
            });
        }

        public static void Positive_QuoteRuntimeVarsNone(EU.IValidator V) {
            var a = Expression.Parameter(typeof(int), "a");
            var b = Expression.Parameter(typeof(int), "b");

            var e = Expression.Lambda<Func<Expression<Func<int, int, IRuntimeVariables>>>>(
                Expression.Quote(
                    Expression.Lambda<Func<int, int, IRuntimeVariables>>(
                        Expression.RuntimeVariables(a, b),
                        a, b
                    )
                )
            );

            V.Validate(e, f =>
            {
                var r = f().Compile()(123, 444);
                EU.Equal(r[0], 123);
                EU.Equal(r[1], 444);
            });
        }

        public static void Positive_QuoteRuntimeVarsAll(EU.IValidator V) {
            var a = Expression.Parameter(typeof(int), "a");
            var b = Expression.Parameter(typeof(int), "b");

            var e = Expression.Lambda<Func<int, int, Expression<Func<IRuntimeVariables>>>>(
                Expression.Quote(
                    Expression.Lambda<Func<IRuntimeVariables>>(
                        Expression.RuntimeVariables(a, b)
                    )
                ),
                a, b
            );

            V.Validate(e, f =>
            {
                var r = f(123, 444).Compile()();
                EU.Equal(r[0], 123);
                EU.Equal(r[1], 444);

                var r2 = f(-1, -2).Compile()();
                EU.Equal(r2[0], -1);
                EU.Equal(r2[1], -2);

                EU.Equal(r[0], 123);
                EU.Equal(r[1], 444);
            });
        }

        public static void Positive_QuoteRuntimeVarsSome(EU.IValidator V) {
            var a = Expression.Parameter(typeof(int), "a");
            var b = Expression.Parameter(typeof(int), "b");
            var c = Expression.Parameter(typeof(int), "c");
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");

            var e = Expression.Lambda<Func<int, int, int, Expression<Func<int, int, IRuntimeVariables>>>>(
                Expression.Quote(
                    Expression.Lambda<Func<int, int, IRuntimeVariables>>(
                        Expression.RuntimeVariables(a, y, c, b, x),
                        x, y
                    )
                ),
                a, b, c
            );

            V.Validate(e, outer =>
            {
                var f1 = outer(111, -4, 42).Compile();
                var f2 = outer(222, -5, 24).Compile();

                EU.Equal(ToString(f1(789, 0)), "111,0,42,-4,789,");
                EU.Equal(ToString(f1(0, 789)), "111,789,42,-4,0,");
                EU.Equal(ToString(f2(777, 99)), "222,99,24,-5,777,");
                EU.Equal(ToString(f2(1, 2)), "222,2,24,-5,1,");
            });
        }

        // bug fix in v4.0: quote only supports lambdas
        public static void Negative_QuoteNonLambda(EU.IValidator V) {
            EU.Throws<ArgumentException>(() => Expression.Quote(Expression.Constant(123)));
        }

        //See Dev10 bug 526789
        public static void Negative_NullInitializerForListInit(EU.IValidator V) {
            NewExpression newExpr = Expression.New(typeof(List<int>));
            ElementInit[] einits = null;
            EU.Throws<ArgumentNullException>(
                () => Expression.ListInit(newExpr, einits)
            );
        }

        //See Dev10 bug 526801
        public static void Negative_NullInitializerForMemberInit(EU.IValidator V) {
            NewExpression newExpr = Expression.New(typeof(List<int>));
            MemberBinding[] inits = null;
            EU.Throws<ArgumentNullException>(
                () => Expression.MemberInit(newExpr, inits)
            );
        }

        public static void Negative_NullExpressionsForBlock(EU.IValidator V) {
            Expression[] expressions = null;
            EU.Throws<ArgumentNullException>(
                () => Expression.Block(new ParameterExpression[] { }, expressions)
            );
        }

        //Verify that Expressioin.TypeEqual(null, T) always gives false.
        public static Expression Positive_TypeEqualNull(EU.IValidator V) {
            Expression c = Expression.Constant(null, typeof(string));
            Expression typeEqual = Expression.TypeEqual(c, typeof(string));
            var le = Expression.Lambda<Func<bool>>(typeEqual);
            V.Validate(le, s =>
            {
                EU.Equal(s(), false);
            });

            typeEqual = Expression.TypeEqual(c, typeof(object));
            le = Expression.Lambda<Func<bool>>(typeEqual);
            V.Validate(le, s =>
            {
                EU.Equal(s(), false);
            });

            typeEqual = Expression.TypeEqual(c, typeof(Microsoft.Scripting.Runtime.DynamicNull));
            le = Expression.Lambda<Func<bool>>(typeEqual);
            V.Validate(le, s =>
            {
                EU.Equal(s(), false);
            });

            return typeEqual;
        }

        // Test that internally reducible TypeEqual node doesn't break closures
        public static void Positive_TypeEqualClosure(EU.IValidator V) {
            var w = Expression.Parameter(typeof(int), "w");
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var z = Expression.Parameter(typeof(Func<int, int>), "z");
            var e = Expression.Lambda<Func<int, Func<int, int>>>(
                Expression.Block(
                    new[] { y, z },
                    Expression.Assign(y, Expression.Constant(123)),
                    Expression.Condition(
                        Expression.TypeEqual(
                            Expression.Assign(
                                z,
                                Expression.Lambda(Expression.Add(w, Expression.Add(x, y)), x)
                            ),
                            typeof(Func<int, int>)
                        ),
                        z,
                        Expression.Constant(null, z.Type)
                    )
                ),
                w
            );
            V.Validate(e, f =>
            {
                var f2 = f(111);
                EU.Equal(f2(111), 345);
                var f3 = f(444);
                EU.Equal(f2(1000), 1234);
                EU.Equal(f3(0), 567);
            });
        }

        public static void Negative_TryExpressionWithUnmatchedType(EU.IValidator V) {
            //try and catch have different type
            CatchBlock cb = Expression.Catch(Expression.Parameter(typeof(Expression), "ex"), Expression.Constant(1));
            EU.Throws<ArgumentException>(
                () => Expression.TryCatch(Expression.Constant(true), cb)
            );
        }

        //Verify that values in Try and Catches are correctly handled.
        public static void Positive_TryCatchWithNonVoidType1(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var argEx = Expression.Constant(new ArgumentException());
            var notSupportedEx = Expression.Constant(new NotSupportedException());
            var invalidOpEx = Expression.Constant(new InvalidOperationException());

            SwitchCase case1 = Expression.SwitchCase(Expression.Empty(), Expression.Constant(1));
            SwitchCase case2 = Expression.SwitchCase(Expression.Throw(argEx), Expression.Constant(2));
            SwitchCase case3 = Expression.SwitchCase(Expression.Throw(notSupportedEx), Expression.Constant(3));
            SwitchCase case4 = Expression.SwitchCase(Expression.Throw(invalidOpEx), Expression.Constant(4));
            SwitchCase case5 = Expression.SwitchCase(Expression.Empty(), Expression.Constant(5));

            Expression expr = Expression.TryCatch(
                Expression.Block(
                    Expression.Switch(x, case1, case2, case3, case4, case5),
                    Expression.Constant("try")
                ),
                Expression.Catch(typeof(ArgumentException), Expression.Constant("ArgumentException")),
                Expression.Catch(typeof(NotSupportedException), Expression.Constant("NotSupportedException")),
                Expression.Catch(typeof(InvalidOperationException), Expression.Constant("InvalidOperationException"))
            );

            var le = Expression.Lambda<Func<int, string>>(expr, x);

            V.Validate(le, s =>
            {
                EU.Equal(s(1), "try");
                EU.Equal(s(2), "ArgumentException");
                EU.Equal(s(3), "NotSupportedException");
                EU.Equal(s(4), "InvalidOperationException");
                EU.Equal(s(5), "try");
            });
        }

        public static void Positive_TryCatchWithNonVoidType2(EU.IValidator V) {
            //try { 2 } catch { 4 }  +   try { 5 } catch { 6 }
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var invalidOpEx = Expression.Constant(new InvalidOperationException());

            SwitchCase case1 = Expression.SwitchCase(Expression.Throw(invalidOpEx), Expression.Constant(1));
            SwitchCase case2 = Expression.SwitchCase(Expression.Empty(), Expression.Constant(2));

            Expression try1 = Expression.TryCatch(
                Expression.Block(
                    Expression.Switch(x, case1, case2),
                    Expression.Constant(2)
                ),
                Expression.Catch(typeof(InvalidOperationException), Expression.Constant(4))
            );

            Expression try2 = Expression.TryCatch(
                Expression.Block(
                    Expression.Switch(y, case1, case2),
                    Expression.Constant(5)
                ),
                Expression.Catch(typeof(InvalidOperationException), Expression.Constant(6))
            );

            var le = Expression.Lambda<Func<int, int, int>>(Expression.Add(try1, try2), x, y);
            V.Validate(le, s =>
            {
                EU.Equal(s(2, 2), 7); //2+5
                EU.Equal(s(2, 1), 8); //2+6
                EU.Equal(s(1, 2), 9); //4+5
                EU.Equal(s(1, 1), 10); //4+6
            });
        }

        public static void Negative_TryInFilter(EU.IValidator V)
        {
            //mark the assembly as transparent so it works in partial trust.
            var attributes = new[] { 
                new CustomAttributeBuilder(typeof(SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes), new object[0])
            };

#if SILVERLIGHT
            var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Negative_TryInFilter"), AssemblyBuilderAccess.Run);
#else
            var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Negative_TryInFilter"), AssemblyBuilderAccess.Run, attributes);
#endif
            var type = asm.DefineDynamicModule("MyModule").DefineType("MyType");
            var method = type.DefineMethod("test", MethodAttributes.Public | MethodAttributes.Static);


            EU.Throws<InvalidOperationException>(
                () => Expression.Lambda(
                    Expression.TryCatch(
                        Expression.Empty(),
                        Expression.Catch(
                            typeof(Exception),
                            Expression.Empty(),
                            Expression.TryFinally(
                                Expression.Constant(true),
                                Expression.Empty()
                            )
                        )
                    )
                ).CompileToMethod(method, System.Runtime.CompilerServices.DebugInfoGenerator.CreatePdbGenerator())
            );
        }

        #region non-zero based arrays
//Silverlight does not have this overload of Array.Createinstance.  Not a valid scenario there
#if !SILVERLIGHT
        //Dev10 bug 552957
        //Verify that Expression.Assign throws when assigning a non-zero based array to a zero-based array.
        public static void Negative_NonZeroBasedArray1(EU.IValidator V) {
            ParameterExpression arr = Expression.Parameter(typeof(int[]), "arr");
            int length = 3;
            int lowerBound = -2;

            //lower bound is negative
            var a = System.Array.CreateInstance(typeof(int), new int[] { length }, new int[] { lowerBound });

            EU.Throws<ArgumentException>(
                () => Expression.Block(
                    new[] { arr },
                    Expression.Assign(arr, Expression.Constant(a))
                )
            );

            //lower bound is positive
            lowerBound = 3;
            a = System.Array.CreateInstance(typeof(int), new int[] { length }, new int[] { lowerBound });
            EU.Throws<ArgumentException>(
                () => Expression.Block(
                    new[] { arr },
                    Expression.Assign(arr, Expression.Constant(a))
                )
            );
        }

        //Verify that Expression.Constant throws when creating non-zero based array constant using a zero based array type.
        public static void Negative_NonZeroBasedArray2(EU.IValidator V) {
            int length = 3;
            int lowerBound = -2;

            //lower bound is negative
            var a = System.Array.CreateInstance(typeof(int), new int[] { length }, new int[] { lowerBound });
            EU.Throws<ArgumentException>(
                () => Expression.Constant(a, typeof(int[]))
            );

            //lower bound is positive
            lowerBound = 3;
            EU.Throws<ArgumentException>(
                () => Expression.Constant(a, typeof(int[]))
            );
        }

        //Verify assignment for non-zero based one-dimensional array
        public static void Positive_NonZeroBasedArray1(EU.IValidator V) {
            int length = 3;
            int lowerBound = -2;
            var a = System.Array.CreateInstance(typeof(int), new int[] { length }, new int[] { lowerBound });

            int length2 = 10;
            int lowerBound2 = 2;
            //t is non-zero based but has a differnt length and lower bound than a's type.
            Type t = System.Array.CreateInstance(typeof(int), new int[] { length2 }, new int[] { lowerBound2 }).GetType();

            ParameterExpression arr = Expression.Parameter(t, "arr");

            var le = Expression.Lambda(
                    Expression.Block(
                        new[] { arr },
                        Expression.Assign(arr, Expression.Constant(a))
                    )
            );
            V.Validate(le);
        }

        //Verify assignment for non-zero based one-dimensional array
        public static void Positive_NonZeroBasedArrayToZeroBasedArray(EU.IValidator V) {
            int length = 3;
            int lowerBound = -2;
            var a = System.Array.CreateInstance(typeof(int), new int[] { length }, new int[] { lowerBound });
            Type t = a.GetType();

            ParameterExpression arr = Expression.Parameter(t, "arr");

            int[] a1 = new int[] { 1, 2 };
            var le = Expression.Lambda(
                    Expression.Block(
                        new[] { arr },
                        Expression.Assign(arr, Expression.Constant(a1))
                    )
            );
            V.Validate(le);
        }

        //Verify assignment for non-zero based 2-dimensional array
        public static void Positive_NonZeroBasedArray2(EU.IValidator V) {
            int length1 = 3, length2 = 5; ;
            int lowerBound1 = -2, lowerBound2 = 2; ;
            var a = System.Array.CreateInstance(typeof(int), new int[] { length1, length2 }, new int[] { lowerBound1, lowerBound2 });

            int length3 = 10, length4 = 2; ;
            int lowerBound3 = -1, lowerBound4 = 0; ;
            //t is non-zero based but has a differnt lengths and lower bounds than a's type.
            Type t = System.Array.CreateInstance(typeof(int), new int[] { length3, length4 }, new int[] { lowerBound3, lowerBound4 }).GetType();

            ParameterExpression arr = Expression.Parameter(t, "arr");

            var le = Expression.Lambda(
                    Expression.Block(
                        new[] { arr },
                        Expression.Assign(arr, Expression.Constant(a))
                    )
            );
            V.Validate(le);
        }
#endif
        #endregion

        #region Verify that assignment from arrays to IEnumerable arrays works
        public static void Positive_AssignmentFromArrayToIEnumerable1(EU.IValidator V) {
            //int[] to IEnumerable<int>
            var a = System.Array.CreateInstance(typeof(int), 3);
            Type tIEnumerable = typeof(IEnumerable<int>);
            ParameterExpression p = Expression.Parameter(tIEnumerable, "p");
            var le = Expression.Lambda(
                Expression.Block(
                    new[] { p },
                    Expression.Assign(p, Expression.Constant(a))
                )
            );
            V.Validate(le);
        }

        public static void Positive_AssignmentFromArrayToIEnumerable2(EU.IValidator V) {
            //int[,][] to IEnumerable[,]
            var a = System.Array.CreateInstance(typeof(int).MakeArrayType(), new[] { 2, 3 });
            Type tIEnumerable = typeof(IEnumerable<int>).MakeArrayType(2);
            ParameterExpression p = Expression.Parameter(tIEnumerable, "p");
            var le = Expression.Lambda(
                Expression.Block(
                    new[] { p },
                    Expression.Assign(p, Expression.Constant(a))
                )
            );
            V.Validate(le);
        }

        public static void Positive_AssignmentFromArrayToIList(EU.IValidator V) {
            //int[,][] to IList<int>[,]
            var a = System.Array.CreateInstance(typeof(int).MakeArrayType(), new[] { 2, 3 });
            Type tIEnumerable = typeof(IList<int>).MakeArrayType(2);
            ParameterExpression p = Expression.Parameter(tIEnumerable, "p");
            var le = Expression.Lambda(
                Expression.Block(
                    new[] { p },
                    Expression.Assign(p, Expression.Constant(a))
                )
            );
            V.Validate(le);
        }

        public static void Positive_AssignmentFromArrayToICollection(EU.IValidator V) {
            //int[,][] to ICollection<int>[,]
            var a = System.Array.CreateInstance(typeof(int).MakeArrayType(), new[] { 2, 3 });
            Type tIEnumerable = typeof(ICollection<int>).MakeArrayType(2);
            ParameterExpression p = Expression.Parameter(tIEnumerable, "p");
            var le = Expression.Lambda(
                Expression.Block(
                    new[] { p },
                    Expression.Assign(p, Expression.Constant(a))
                )
            );
            V.Validate(le);
        }

//Silverlight does not have this overload of Array.Createinstance.  Not a valid scenario there
#if !SILVERLIGHT
        public static void Negative_AssignmentFromNonZeroBasedArrayToIEnumerable(EU.IValidator V) {
            //int[-2..2] to IEnumerable<int>
            var a = System.Array.CreateInstance(typeof(int), new[] { 5 }, new[] { -2 });
            Type tIEnumerable = typeof(IEnumerable<int>);
            ParameterExpression p = Expression.Parameter(tIEnumerable, "p");
            EU.Throws<ArgumentException>(
                () => Expression.Lambda(
                    Expression.Block(
                        new[] { p },
                        Expression.Assign(p, Expression.Constant(a))
                    )
                )
            );
        }
#endif

        public static void Negative_AssignmentFromArrayToIEnumerable1(EU.IValidator V) {
            //int[,] array to IEnumerable
            var a = System.Array.CreateInstance(typeof(int), new[] { 2, 3 });
            Type tIEnumerable = typeof(IEnumerable<int>);
            ParameterExpression p = Expression.Parameter(tIEnumerable, "p");
            EU.Throws<ArgumentException>(
                () => Expression.Lambda(
                    Expression.Block(
                        new[] { p },
                        Expression.Assign(p, Expression.Constant(a))
                    )
                )
            );
        }

        public static void Negative_AssignmentFromArrayToIEnumerable2(EU.IValidator V) {
            //int[][,] to IEnumerable[]
            var a = System.Array.CreateInstance(typeof(int).MakeArrayType(2), 3);
            Type tIEnumerable = typeof(IEnumerable<int>).MakeArrayType();
            ParameterExpression p = Expression.Parameter(tIEnumerable, "p");
            EU.Throws<ArgumentException>(
                () => Expression.Lambda(
                    Expression.Block(
                        new[] { p },
                        Expression.Assign(p, Expression.Constant(a))
                    )
                )
            );
        }
        #endregion

        #region Verify Expression.NewArrayBounds() works after replacing TypeEU.AreAssignable() with Type.IsAssignableFrom().
#if !CLR2 // TODO: inline expression trees
        public static void NewArrayInvoke(EU.IValidator V) {
            Expression<Func<int, string[]>> linq1 = (a => new string[a]);
            InvocationExpression linq1a = Expression.Invoke(linq1, new Expression[] { Expression.Constant(3) });
            Expression<Func<string[]>> linq1b = Expression.Lambda<Func<string[]>>(linq1a, new ParameterExpression[] { });
            //Func<string[]> f = linq1b.Compile();
            V.Validate(linq1b);
        }

        public static void Arrays() {
            Expression<Func<int, int[]>> exp1 = i => new int[i];
            NewArrayExpression aex1 = exp1.Body as NewArrayExpression;
            Debug.Assert(aex1 != null);
            Debug.Assert(aex1.NodeType == ExpressionType.NewArrayBounds);

            Expression<Func<int[], int>> exp2 = (i) => i.Length;
            UnaryExpression uex2 = exp2.Body as UnaryExpression;
            Debug.Assert(uex2 != null);
            Debug.Assert(uex2.NodeType == ExpressionType.ArrayLength);
        }
#endif
        #endregion

#if !CLR2 // TODO: inline expression trees
        public static void Positive_ShorCircuiting(EU.IValidator V) {
            Expression<Func<int, string, bool>> f = (i, s) => s != "" && i > 1;
            V.Validate(f, fc =>
            {
                var result = ((Func<int, string, bool>)fc)(3, "foo");
            });
        }
#endif

        // Unary/binary ops should leave on the IL stack the value they claim
        // to leave.
        public static void Positive_SmallIntOps(EU.IValidator V) {
            var e = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Constant(unchecked((ushort)(ushort.MaxValue + 3)), typeof(ushort)),
                    Expression.Add(Expression.Constant(ushort.MaxValue), Expression.Constant((ushort)3))
                )
            );

            V.Validate(e, result =>
            {
                EU.Equal(result(), true);
            });

            e = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Constant(unchecked((short)(short.MaxValue + 3)), typeof(short)),
                    Expression.Add(Expression.Constant(short.MaxValue), Expression.Constant((short)3))
                )
            );

            V.Validate(e, result =>
            {
                EU.Equal(result(), true);
            });

            e = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Constant(unchecked((ushort)(ushort.MaxValue - 3)), typeof(ushort)),
                    Expression.Not(Expression.Constant((ushort)3, typeof(ushort)))
                )
            );

            V.Validate(e, result =>
            {
                EU.Equal(result(), true);
            });


            e = Expression.Lambda<Func<bool>>(
                Expression.Equal(
                    Expression.Constant(unchecked((byte)(byte.MaxValue - 3)), typeof(byte)),
                    Expression.Not(Expression.Constant((byte)3, typeof(byte)))
                )
            );

            V.Validate(e, result =>
            {
                EU.Equal(result(), true);
            });

        }

        private static void Negative_SmallIntOps(EU.IValidator V) {
            var e = Expression.Lambda<Func<short>>(
                  Expression.AddChecked(Expression.Constant(short.MaxValue), Expression.Constant((short)3))
                );
            V.ValidateException<OverflowException>(e, false);

            e = Expression.Lambda<Func<short>>(
                  Expression.SubtractChecked(Expression.Constant(short.MinValue), Expression.Constant((short)3))
                );
            V.ValidateException<OverflowException>(e, false);
        }

        //Verify that Void cannot be converted to interface.
        //See Dev10 bug 512821
        public static void Negative_ConvertVoidToInterface(EU.IValidator V) {
            var eVoid = Expression.Empty();
            EU.Throws<InvalidOperationException>(() => Expression.Convert(eVoid, typeof(IDisposable)));
        }

        public static void Negative_ConvertVoidToObject(EU.IValidator V) {
            var eVoid = Expression.Empty();
            EU.Throws<InvalidOperationException>(() => Expression.Convert(eVoid, typeof(System.Object)));
        }

        private static string ToString(IRuntimeVariables boxes) {
            var result = new StringBuilder();
            for (int i = 0, count = boxes.Count; i < count; i++) {
                result.Append(boxes[i]).Append(',');
            }
            return result.ToString();
        }

        #region Verify that Expression.New works when using constructor with argument's type is subtype of member's
        public static void Positive_NewUsingConstructorWithDerivedTypeArgument(EU.IValidator V) {
            var ctor = typeof(Anything).GetConstructor(new Type[] { typeof(object) });
            //The argument is a string constant while the member has the type Object
            var arguments = new Expression[] { Expression.Constant("object") };
            var members = typeof(Anything).GetMember("Object");
            var e = Expression.New(
                ctor,
                arguments,
                members
            );
        }

        class Anything {
            public object Object { get; set; }
            public Anything(object obj) {
            }
        }
        #endregion

        //Verify that RightShift works when operands have nullable types.
        public static void Positive_RightShiftOfNullables(EU.IValidator V) {
            //16 >> 1 = 8
            var x1 = Expression.Lambda<Func<long?>>(Expression.RightShift(Expression.Constant((long)16, typeof(long?)), Expression.Constant(1)));
            V.Validate(x1, result1 =>
            {
                EU.Equal(result1(), (long)8);
            });

            //null >> 1 = null
            var x2 = Expression.Lambda<Func<long?>>(Expression.RightShift(Expression.Constant(null, typeof(long?)), Expression.Constant(1)));
            V.Validate(x2, result2 =>
            {
                EU.Equal(result2(), null);
            });

            //16 >> 1 = 8
            var x3 = Expression.Lambda<Func<int?>>(Expression.RightShift(Expression.Constant(16), Expression.Constant(1, typeof(int?))));
            V.Validate(x3, result3 =>
            {
                EU.Equal(result3(), 8);
            });

            //16 >> null = null
            var x4 = Expression.Lambda<Func<int?>>(Expression.RightShift(Expression.Constant(1), Expression.Constant(null, typeof(int?))));
            V.Validate(x4, result4 =>
            {
                EU.Equal(result4(), null);
            });
        }

        //Verify that LeftShift works when operands have nullable types.
        public static void Positive_LeftShiftOfNullables(EU.IValidator V) {
            //16 << 1 = 32
            var x1 = Expression.Lambda<Func<long?>>(Expression.LeftShift(Expression.Constant((long)16, typeof(long?)), Expression.Constant(1)));
            V.Validate(x1, result1 =>
            {
                EU.Equal(result1(), (long)32);
            });

            //null << 1 = null
            var x2 = Expression.Lambda<Func<long?>>(Expression.LeftShift(Expression.Constant(null, typeof(long?)), Expression.Constant(1)));
            V.Validate(x2, result2 =>
            {
                EU.Equal(result2(), null);
            });

            //16 << 1 = 32
            var x3 = Expression.Lambda<Func<int?>>(Expression.LeftShift(Expression.Constant(16), Expression.Constant(1, typeof(int?))));
            V.Validate(x3, result3 =>
            {
                EU.Equal(result3(), 32);
            });

            //16 << null = null
            var x4 = Expression.Lambda<Func<int?>>(Expression.LeftShift(Expression.Constant(1), Expression.Constant(null, typeof(int?))));
            V.Validate(x4, result4 =>
            {
                EU.Equal(result4(), null);
            });
        }

        //Verfiy that Coalesce works when the left operand is null.
        public static void Positive_CoalesceLeftTypeUnspecified(EU.IValidator V) {
            Expression x = Expression.Coalesce(Expression.Constant(null), Expression.Constant(2));
            var result = Expression.Lambda(x).Compile().DynamicInvoke();
            EU.Equal(result, 2);
        }

        #region lambda inliner tests

        public static void Positive_InlineInvokeSimple(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var z = Expression.Parameter(typeof(int), "z");
            var e = Expression.Lambda<Func<int, int, int, int>>(
                Expression.Invoke(
                    Expression.Lambda(Expression.Subtract(x, y), x, y),
                    z,
                    Expression.Subtract(x, y)
                ),
                x, y, z
            );

            V.Validate(e, f =>
            {
                EU.Equal(f(100, 20, 3), -77);
            });
        }

        // nested with some closures
        public static void Positive_InlineInvokeNested(EU.IValidator V) {
            var w = Expression.Parameter(typeof(int), "w");
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var z = Expression.Parameter(typeof(int), "z");

            var e = Expression.Lambda<Func<int, int, int, int, int>>(
                Expression.Invoke(
                    Expression.Lambda(
                        Expression.Invoke(
                            Expression.Lambda(
                                Expression.Invoke(
                                    Expression.Lambda(
                                        Expression.Subtract(x, Expression.Subtract(y, z)),
                                        y
                                    ),
                                    Expression.Constant(123)
                                ),
                                x
                            ),
                            Expression.Add(x, Expression.Subtract(w, y))
                        ),
                        w
                    ),
                    Expression.Add(w, Expression.Constant(444))
                ),
                w, x, y, z
            );
            
            // (w, x, y, z) => Invoke(w => Invoke(x => Invoke(y => (x - (y - z)), 123), (x + (w - y))), (w + 444))

            // ((x + ((w + 444) - y)) - (123 - z))
            // w = 1000; x = 200; y = 30; z = 4;
            V.Validate(e, f =>
            {
                EU.Equal(f(1000, 200, 30, 4), 1495);
            });
        }

        private delegate int TestInlineInvokeRef3(ref int x, ref int y, ref int z);
        private delegate int TestInlineInvokeRef2(ref int x, ref int y);

        public static void Positive_InlineInvokeRef(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int).MakeByRefType(), "x");
            var y = Expression.Parameter(typeof(int).MakeByRefType(), "y");
            var z = Expression.Parameter(typeof(int).MakeByRefType(), "z");
            var e = Expression.Lambda<TestInlineInvokeRef3>(
                Expression.Invoke(
                    Expression.Lambda<TestInlineInvokeRef2>(
                        Expression.Assign(x, Expression.Subtract(x, y)),
                        x, y
                    ),
                    z,
                    Expression.Subtract(x, y)
                ),
                x, y, z
            );

            V.Validate(e, f =>
            {
                int i = 100;
                int j = 20;
                int k = 3;
                EU.Equal(f(ref i, ref j, ref k), -77);
                EU.ArrayEqual(new[] { i, j, k }, new[] { 100, 20, -77 });
            });
        }

        // test spilling inside body, and spilling inside args
        public static void Positive_InlineInvokeSpill(EU.IValidator V) {

            Func<object, Expression> c = i => Expression.Constant(i);
            Func<Expression, Expression, Expression> append = (i, j) => Expression.Call(
                typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }),
                i, j
            );

            var x = Expression.Parameter(typeof(string), "x");
            var y = Expression.Parameter(typeof(string), "y");

            var e = Expression.Lambda<Func<string, string, string>>(
                append(
                    c("S_"),
                    append(
                        Expression.Invoke(
                            Expression.Lambda(
                                Expression.TryFinally(
                                    append(x, c("T1_")),
                                    Expression.Assign(y, append(y, c("F1_")))
                                ),
                                x
                            ),
                            c("A1_")
                        ),
                        Expression.Invoke(
                            Expression.Lambda(append(x, y), x),
                            Expression.TryFinally(
                                append(x, c("T2_")),
                                Expression.Assign(y, append(y, c("F2_")))
                            )
                        )
                    )
                ),
                x, y
            );
            
            V.Validate(e, f =>
            {
                EU.Equal(f("X_", "Y_"), "S_A1_T1_X_T2_Y_F1_F2_");
            });
        }

        // Test arbitrary goto can reuse the same label in nested lambda
        // also has nested live-object constants
        public static void Positive_InlineInvokeGoto(EU.IValidator V) {
            var label = Expression.Label("foo");
            var e = Expression.Lambda<Func<string>>(
                Expression.Block(
                    Expression.Block(
                        Expression.Goto(label),
                        Expression.Throw(Expression.Constant(new Exception("should jump over this")))
                    ),
                    Expression.Block(
                        Expression.Label(label)
                    ),
                    Expression.Invoke(
                        Expression.Lambda(
                                Expression.Block(
                                Expression.Block(
                                    Expression.Goto(label),
                                    Expression.Throw(Expression.Constant(new Exception("should jump over this too")))
                                ),
                                Expression.Block(
                                    Expression.Label(label)
                                ),
                                Expression.Constant("hello")
                            )
                        )
                    )
                )
            );
            
            V.Validate(e, f =>
            {
                EU.Equal(f(), "hello");
            });
        }

        #endregion

        #region OnesComplement
        //Verify taht OnesComplement works on interger type
        public static void Positive_OnesComplementInteger(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var le = Expression.Lambda<Func<int, int>>(Expression.OnesComplement(x), x);
            
            V.Validate(le, f =>
            {
                EU.Equal(f(1), -2);
                EU.Equal(f(-1), 0);
            });
        }

        public static void Positive_OnesComplementNullableInteger(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int?), "x");
            var le = Expression.Lambda<Func<int?, int?>>(Expression.OnesComplement(x), x);
            V.Validate(le, f =>
            {
                EU.Equal(f(-1), 0);
                EU.Equal(f(null), null);
            });
        }

        public struct OnesComplementTest {
            public int X;
            public OnesComplementTest(int x) {
                X = x;
            }
            //overload the ~ operator
            public static OnesComplementTest operator ~(OnesComplementTest x1) {
                return new OnesComplementTest(~(x1.X));
            }

            public static OnesComplementTest? operator ~(OnesComplementTest? x1) {
                if (x1 == null) {
                    return null;
                }
                return new OnesComplementTest(~(x1.Value.X));
            }

            public static OnesComplementTest OnesComplement(OnesComplementTest x1) {
                return new OnesComplementTest(~(x1.X));
            }
        }

        public static void Positive_OnesComplementOverload(EU.IValidator V) {
            var x = Expression.Parameter(typeof(OnesComplementTest), "x");
            var le = Expression.Lambda<Func<OnesComplementTest, OnesComplementTest>>(Expression.OnesComplement(x), x);
            V.Validate(le, f =>
            {
                var v = new OnesComplementTest(0);
                EU.Equal(f(v).X, -1);
                v.X = -1;
                EU.Equal(f(v).X, 0);
            });
        }

        public static void Positive_OnesComplementMethodInfo(EU.IValidator V) {
            var x = Expression.Parameter(typeof(OnesComplementTest), "x");
            var mi = typeof(OnesComplementTest).GetMethod("OnesComplement");
            var le = Expression.Lambda<Func<OnesComplementTest, OnesComplementTest>>(Expression.OnesComplement(x, mi), x);
            V.Validate(le, f =>
            {
                var v = new OnesComplementTest(0);
                EU.Equal(f(v).X, -1);
                v.X = -1;
                EU.Equal(f(v).X, 0);
            });
        }

        public static void Positive_OnesComplementOverloadNullable(EU.IValidator V) {
            var x = Expression.Parameter(typeof(OnesComplementTest?), "x");
            var le = Expression.Lambda<Func<OnesComplementTest?, OnesComplementTest?>>(Expression.OnesComplement(x), x);
            V.Validate(le, f =>
            {
                OnesComplementTest? v = new OnesComplementTest(0);
                EU.Equal(f(v).Value.X, -1);
                EU.Equal(f(null), null);
            });
        }
        #endregion

        #region IsFalse and IsTrue

        //Verify that IsFalse works on boolean expressions
        public static Expression Positive_IsFalseBoolean(EU.IValidator V) {
            var x = Expression.Parameter(typeof(bool), "x");
            var le = Expression.Lambda<Func<bool, bool>>(Expression.IsFalse(x), x);
            V.Validate(le, f =>
            {
                EU.Equal(f(true), false);
                EU.Equal(f(false), true);
            });

            return le;
        }

        //Verify that IsFalse works on nullable boolean expressions
        public static Expression Positive_IsFalseNullableBoolean(EU.IValidator V) {
            var x = Expression.Parameter(typeof(bool?), "x");
            var le = Expression.Lambda<Func<bool?, bool?>>(Expression.IsFalse(x), x);
            V.Validate(le, f =>
            {
                EU.Equal(f(true), false);
                EU.Equal(f(false), true);
                EU.Equal(f(null), null);
            });

            return le;
        }

        //Verify that IsTrue works on boolean expressions
        public static Expression Positive_IsTrueBoolean(EU.IValidator V) {
            var x = Expression.Parameter(typeof(bool), "x");
            var le = Expression.Lambda<Func<bool, bool>>(Expression.IsTrue(x), x);
            V.Validate(le, f =>
            {
                EU.Equal(f(true), true);
                EU.Equal(f(false), false);
            });
            return le;
        }

        //Verify that IsTrue works on nullable boolean expressions
        public static Expression Positive_IsTrueNullableBoolean(EU.IValidator V) {
            var x = Expression.Parameter(typeof(bool?), "x");
            var le = Expression.Lambda<Func<bool?, bool?>>(Expression.IsTrue(x), x);
            V.Validate(le, f =>
            {
                EU.Equal(f(true), true);
                EU.Equal(f(false), false);
                EU.Equal(f(null), null);
            });
            return le;
        }

        public struct TrueFalseTest {
            public int X;
            public TrueFalseTest(int x) {
                X = x;
            }
            //overload the true and false operators
            public static bool operator false(TrueFalseTest x1) {
                return x1.X == 0;
            }

            public static bool operator true(TrueFalseTest x1) {
                return x1.X != 0;
            }

            public static bool IsFalse(TrueFalseTest x1) {
                return x1.X == 0;
            }

            public static bool IsTrue(TrueFalseTest x1) {
                return x1.X != 0;
            }
        }

        //Verify that IsFalse works on overload operator
        public static Expression Positive_IsFalseOverload(EU.IValidator V) {
            var x = Expression.Parameter(typeof(TrueFalseTest), "x");
            var le = Expression.Lambda<Func<TrueFalseTest, bool>>(Expression.IsFalse(x), x);
            V.Validate(le, f =>
            {
                EU.Equal(f(new TrueFalseTest(100)), false);
                EU.Equal(f(new TrueFalseTest(0)), true);
            });
            return le;
        }

        //Verify that IsTrue works on overload operator
        public static Expression Positive_IsTrueOverload(EU.IValidator V) {
            var x = Expression.Parameter(typeof(TrueFalseTest), "x");
            var le = Expression.Lambda<Func<TrueFalseTest, bool>>(Expression.IsTrue(x), x);
            V.Validate(le, f =>
            {
                EU.Equal(f(new TrueFalseTest(100)), true);
                EU.Equal(f(new TrueFalseTest(0)), false);
            });
            return le;
        }

        //Verify that IsFalse works on user-provided MethodInfo
        public static Expression Positive_IsFalseMethodInfo(EU.IValidator V) {
            var mi = typeof(TrueFalseTest).GetMethod("IsFalse");
            Assert.NotNull(mi);
            var x = Expression.Parameter(typeof(TrueFalseTest), "x");
            var le = Expression.Lambda<Func<TrueFalseTest, bool>>(Expression.IsFalse(x, mi), x);
            V.Validate(le, f =>
            {
                EU.Equal(f(new TrueFalseTest(100)), false);
                EU.Equal(f(new TrueFalseTest(0)), true);
            });
            return le;
        }

        //Verify that IsTrue works on user-provided MethodInfo
        public static Expression Positive_IsTrueMethodInfo(EU.IValidator V) {
            var mi = typeof(TrueFalseTest).GetMethod("IsTrue");
            Assert.NotNull(mi);
            var x = Expression.Parameter(typeof(TrueFalseTest), "x");
            var le = Expression.Lambda<Func<TrueFalseTest, bool>>(Expression.IsTrue(x, mi), x);
            V.Validate(le, f =>
            {
                EU.Equal(f(new TrueFalseTest(100)), true);
                EU.Equal(f(new TrueFalseTest(0)), false);
            });
            return le;
        }

        //Verify that IsFalse throws exception if IsFalse op is not defined
        public static void Negative_IsFalseString(EU.IValidator V) {
            var x = Expression.Parameter(typeof(string), "x");
            EU.Throws<InvalidOperationException>(() => Expression.Lambda<Func<string, bool>>(Expression.IsFalse(x), x));
        }

        #endregion

        public static void Positive_DelegateWithByRefArgs(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int).MakeByRefType(), "x");
            var y = Expression.Parameter(typeof(int).MakeByRefType(), "y");

            int a1 = 100;
            int a2 = 10;
            var le = Expression.Lambda(Expression.Assign(x, Expression.Subtract(x, y)), x, y);
            var f = le.Compile();
            object[] objs = new object[] { a1, a2 };
            EU.Equal(objs[0], 100);
            f.DynamicInvoke(objs);
            //a1 is passed as ByRef so its value should be changed to 90
            EU.Equal(objs[0], 90);
        }


        #region Validate that ET doesn't allow creating an operator expression with unmatched type.

        public static void Negative_OpOnDoubleAndInt(EU.IValidator V) {
            var c = Expression.Constant(1, typeof(int));
            var d = Expression.Constant(1.0, typeof(double));

            EU.Throws<InvalidOperationException>(() =>
                Expression.Lambda<Func<bool>>(
                    Expression.Equal(c, d))
            );
        }

        public class OpMethodValidation {
            public OpMethodValidation(double x) {
                value = x;
            }
            private double value;
            public static double operator +(OpMethodValidation x, double y) {
                return x.Value + y;
            }

            public double Value {
                get { return value; }
            }
        }

        public static void Negative_OpMethodValidation(EU.IValidator V) {
            var x = Expression.Constant(new OpMethodValidation(1.0));
            var c = Expression.Constant(1, typeof(int));

            //The op method takes double but we pass in int here.
            EU.Throws<InvalidOperationException>(() =>
                Expression.Lambda<Func<bool>>(
                    Expression.Add(x, c))
            );
        }
        #endregion

        //test senario for BlockExpression.Result
        public static void Positive_BlockResult(EU.IValidator V) {
            var e1 = Expression.Constant(1);
            var e2 = Expression.Constant(2);
            var e3 = Expression.Constant(3);
            var e4 = Expression.Constant(4);
            var e5 = Expression.Constant(5);
            var e6 = Expression.Constant(6);

            var b1 = Expression.Block(e1);
            var b2 = Expression.Block(e1, e2);
            var b3 = Expression.Block(e1, e2, e3);
            var b4 = Expression.Block(e1, e2, e3, e4);
            var b5 = Expression.Block(e1, e2, e3, e4, e5);
            var b6 = Expression.Block(e1, e2, e3, e4, e5, e6);

            EU.Equal(b1.Result, e1);
            EU.Equal(b2.Result, e2);
            EU.Equal(b3.Result, e3);
            EU.Equal(b4.Result, e4);
            EU.Equal(b5.Result, e5);
            EU.Equal(b6.Result, e6);

            var x = Expression.Parameter(typeof(int), "x");
            var scope1 = Expression.Block(new[] { x }, e1);
            var scopeN = Expression.Block(new[] { x }, e1, e2);

            EU.Equal(scope1.Result, e1);
            EU.Equal(scopeN.Result, e2);
        }
        
        //Verify that assignment to ArrayIndex is not supported.
        //ArrayIndex is an expression being replaced by IndexExpression 
        //which has support for assignment. Since we are slowly 
        //deprecating this node we shouldn't add new functionality to this node.
        public static void Negative_AssignToArrayIndex(EU.IValidator V) {
            var a = Expression.Parameter(typeof(int[]), "a");
            var i = Expression.Parameter(typeof(int), "i");
            var ai = Expression.ArrayIndex(a, i);
            EU.Throws<ArgumentException>(() => Expression.Assign(ai, Expression.Constant(0)));
            EU.Throws<ArgumentException>(() => Expression.AddAssign(ai, Expression.Constant(1)));
        }

        public static void Positive_DebugInfoProperties(EU.IValidator V) {
            var document = Expression.SymbolDocument("Foo.cs");

            //A DebugInfo containing a real source code span
            var debug1 = Expression.DebugInfo(document, 22, 1, 23, 100);
            EU.Equal(debug1.IsClear, false);
            EU.Equal(debug1.Document.FileName, "Foo.cs");
            EU.Equal(debug1.StartLine, 22);
            EU.Equal(debug1.StartColumn, 1);
            EU.Equal(debug1.EndLine, 23);
            EU.Equal(debug1.EndColumn, 100);
            EU.Equal(debug1.Type, typeof(void));

            //A DebugInfo for clearance
            var debug2 = Expression.ClearDebugInfo(document);
            EU.Equal(debug2.IsClear, true);
            EU.Equal(debug2.Document.FileName, "Foo.cs");
            EU.Equal(debug2.StartLine, 0xfeefee);
            EU.Equal(debug2.StartColumn, 0);
            EU.Equal(debug2.EndLine, 0xfeefee);
            EU.Equal(debug2.EndColumn, 0);
            EU.Equal(debug2.Type, typeof(void));

            //A DebugInfo for clearance by explicitly giving the particular offsets
            var debug3 = Expression.DebugInfo(document, 0xfeefee, 0, 0xfeefee, 0);
            EU.Equal(debug3.IsClear, true);
        }

        #region Test creating IndexExpression for accessing indexers using name
        public class TestIndexersClass {
            string[] _stringValues;
            int[,] _intValues;
            Dictionary<string, int> _dict;

            public Dictionary<string, int> Dict {
                get { return _dict; }
            }

            public TestIndexersClass() {
                _stringValues = new string[10];
                _intValues = new int[10,10];
                _dict = new Dictionary<string,int>();
            }

            public string this[int index] {
                get {
                    return _stringValues[index];
                }
                set {
                    _stringValues[index] = value;
                }
            }

            public int this[int i, int j] {
                get {
                    return _intValues[i,j];
                }
                set {
                    _intValues[i,j] = value;
                }
            }

            public int this[string name] {
                get {
                    return _dict[name];
                }
                set {
                    _dict[name] = value;
                }
            }

            //an overload that can cause ambiguous when using a string argument type
            public int this[object obj] {
                get {
                    if (obj is string) {
                        return _dict[obj as string];
                    } else {
                        return 0;
                    }
                }
                set {
                    if (obj is string) {
                        _dict[obj as string] = value;
                    }
                }
            }

            public int this[bool b] {
                //a writeonly indexer
                set {
                    _dict[b.ToString()] = value;
                }
            }
        }

        public static void Positive_IndexExpressionWithIndexerName(EU.IValidator V) {
            var obj = Expression.Constant(new TestIndexersClass());
            var i = Expression.Parameter(typeof(int), "i");
            var j = Expression.Parameter(typeof(int), "j");
            var s = Expression.Parameter(typeof(string), "s");

            var indexer1 = Expression.Property(obj, "Item", i); //indexer to this[int]
            var leSet1 = Expression.Lambda<Action<int, string>>(
                //this[i] = "Hello"
                Expression.Block(
                    Expression.Assign(indexer1, s),
                    Expression.Empty()
                ),
                i, s
            );

            V.Validate(leSet1, set1 =>
            {
                var leGet1 = Expression.Lambda<Func<int, string>>(
                    //get this[i]
                    indexer1,
                    i
                );
                var get1 = leGet1.Compile();
                //set this[3] = "Hello"
                set1(3, "Hello");
                EU.Equal(get1(3), "Hello");
            });

            var indexer2 = Expression.Property(obj, "Item", i, j); //indexer to this[int, int]
            var leSet2 = Expression.Lambda<Action<int, int>>(
                //this[i, j] = i * j
                Expression.Block(
                    Expression.Assign(indexer2, Expression.Multiply(i, j)),
                    Expression.Empty()
                ),
                i, j
            );

            V.Validate(leSet2, set2 =>
            {
                var leGet2 = Expression.Lambda<Func<int, int, int>>(
                    //get this[i, j]
                    indexer2,
                    i, j
                );
                var get2 = leGet2.Compile();

                //set this[3, 2] = 6
                set2(3, 2);
                EU.Equal(get2(3, 2), 6);
            });
        }

        public static void Positive_IndexExpressionWriteonlyIndexer(EU.IValidator V) {
            var o = new TestIndexersClass();
            var obj = Expression.Constant(o);
            var b = Expression.Parameter(typeof(bool), "b");
            var x = Expression.Parameter(typeof(int), "x");
            var s = Expression.Parameter(typeof(string), "s");
            var indexer = Expression.Property(obj, "Item", b); //indexer is writeonly

            var leSet = Expression.Lambda<Action<bool, int>>(
                //this[b] = x
                Expression.Block(
                    Expression.Assign(indexer, x),
                    Expression.Empty()
                ),
                b, x
            );
            V.Validate(leSet, set =>
            {
                set(true, 100);

                //Verify that the value is really set
                EU.Equal(o.Dict[true.ToString()], 100);
            });
        }

        public static void Negative_IndexExpressionAmbiguousMatch(EU.IValidator V) {
            var obj = Expression.Constant(new TestIndexersClass());
            var s = Expression.Parameter(typeof(string), "s");
            //indexer can match this[string] or this[object]
            EU.Throws<InvalidOperationException>(() => Expression.Property(obj, "Item", s)); 
        }

        public static void Negative_PropertyExpressionForIndex(EU.IValidator V) {
            //When using Expression.Property(name) without any argument, it is supposed to find
            //a property, not the index
            var obj = Expression.Constant(new TestIndexersClass());
            //it would throw unhandled AmbiguousMatchException from reflection
            EU.Throws<AmbiguousMatchException>(() => Expression.Property(obj, "Item"));
        }

        public class ClassWithItemProperty {
            public int Item {
                get { return 0; }
            }
        }

        public static void Positive_PropertyExpressionItemProperty(EU.IValidator V) {
            //verify that it doesn't throw exception since there is no indexer in the class.
            var obj = Expression.Constant(new ClassWithItemProperty());
            var p = Expression.Property(obj, "Item");
        }

        #endregion

        internal interface IPrivateInterface { }
        internal class TestPrivateInterface : IPrivateInterface {}

        //TODO: NEEDS FULL TRUST

        //public static void Positive_PrivateInterfaceConstant() {
        //    // We need to run this against real System.Core for skip
        //    // visibility to work on DynamicMethods.
        //    if (Environment.Version.Major < 4) {
        //        return;
        //    }
        //
        //    // Make sure we don't blow up if the constant is a private
        //    // interface
        //    var test = new TestPrivateInterface();
        //    var e = Expression.Lambda<Func<IPrivateInterface>>(
        //        Expression.Constant(test, typeof(IPrivateInterface))
        //    );
        //    var f = e.Compile();
        //    EU.Equal(f(), test);
        //}


        #region Verify that Expression.TypeEqual does not call the overloaded NotEqual operator
        public class ClassOverloadEqualOperator {
            public static bool operator ==(ClassOverloadEqualOperator x, object d) {
                throw new InvalidOperationException();
            }

            public static bool operator !=(ClassOverloadEqualOperator x, object d) {
                throw new InvalidOperationException();
            }

            public override bool Equals(object o) {
                return this == o;
            }

            public override int GetHashCode() {
                return 1;
            }
        }

        public static void Positive_TypeEqualNotCallOverloadedNotEqual(EU.IValidator V) {
            var p = Expression.Parameter(typeof(ClassOverloadEqualOperator), "p");
            var e = Expression.TypeEqual(p, typeof(ClassOverloadEqualOperator));
            var le = Expression.Lambda<Func<ClassOverloadEqualOperator, bool>>(e, p);
            var f = le.Compile();
            //The overloaded != operator should not be invoked so there is no exception.
            EU.Equal(f(new ClassOverloadEqualOperator()), true);
        }
        #endregion

        #region Test Expression.Throw on a non-exception object
        public static void Negative_ThrowNonExceptionValue(EU.IValidator V) {
            EU.Throws<ArgumentException>(() => Expression.Throw(Expression.Constant(100)));
        }

        public static void Positive_ThrowNonExceptionBoxedValue(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var e = Expression.Parameter(typeof(object), "e");
            var @try = Expression.TryCatch(
                Expression.Throw(Expression.Convert(x, typeof(object)), typeof(int)),
                Expression.Catch(
                    e, //the caught object 
                    Expression.Add(Expression.Convert(e, typeof(int)), Expression.Constant(1))
                )
            );
            var le = Expression.Lambda<Func<int, int>>(
                @try,
                x
            );
            
            V.Validate(le, f =>
            {
                EU.Equal(f(100), 101);
            });
        }

        public static void Positive_ThrowNonExceptionObject(EU.IValidator V) {
            var x = Expression.Parameter(typeof(string), "x");
            var @try = Expression.TryCatch(
                Expression.Throw(x, typeof(string)),
                Expression.Catch(
                    typeof(string),
                    x
                )
            );
            var le = Expression.Lambda<Func<string, string>>(
                @try,
                x
            );
            
            V.Validate(le, f =>
            {
                EU.Equal(f("hello"), "hello");
            });
        }

//RuntimeWrappedException is inaccessible on Silverlight
#if !SILVERLIGHT
        //Throw a non-exception results in a RuntimeWrappedException,
        //but that exception cannot be caught in the lambda. For catching
        //it, needs to use the thrown object's type.
        public static void Negative_ThrowNonExceptionCatchException(EU.IValidator V) {
            var x = Expression.Parameter(typeof(string), "x");
            var @try = Expression.TryCatch(
                Expression.Throw(x, typeof(string)),
                Expression.Catch(
                    typeof(Exception),
                    x
                )
            );
            var le = Expression.Lambda<Func<string, string>>(
                @try,
                x
            );
            var f = le.Compile();
            EU.Throws<RuntimeWrappedException>(() => f("hello"));
        }
#endif
        #endregion

        // Verify that Constant and Default expression APIs create fresh objects.
        public static void Positive_ExpressionFactoryAPICreatesFreshObject(EU.IValidator V) {
            //DefaultExpression
            var empty1 = Expression.Empty();
            var empty2 = Expression.Empty();
            EU.Equal(empty1 != empty2, true);

            //ContantExpression
            var true1 = Expression.Constant(true);
            var true2 = Expression.Constant(true);
            EU.Equal(true1 != true2, true);

            var false1 = Expression.Constant(false);
            var false2 = Expression.Constant(false);
            EU.Equal(false1 != false2, true);

            var null1 = Expression.Constant(null);
            var null2 = Expression.Constant(null);
            EU.Equal(null1 != null2, true);

            string emptyStr = "";
            var emptyStr1 = Expression.Constant(emptyStr);
            var emptyStr2 = Expression.Constant(emptyStr);
            EU.Equal(emptyStr1 != emptyStr2, true);

            string nullStr = null;
            var nullStr1 = Expression.Constant(nullStr);
            var nullStr2 = Expression.Constant(nullStr);
            EU.Equal(nullStr1 != nullStr2, true);

            var integer1 = Expression.Constant(1);
            var integer2 = Expression.Constant(1);
            EU.Equal(integer1 != integer2, true);
        }

        public static void Positive_SymbolDocumentInfoDocumentType(EU.IValidator V) {
            var sd = Expression.SymbolDocument("no_document_type");
            EU.Equal(sd.DocumentType.ToString(), "5a869d0b-6611-11d3-bd2a-0000f80849bd");
            sd = Expression.SymbolDocument("no_document_type_either", Guid.NewGuid());
            EU.Equal(sd.DocumentType.ToString(), "5a869d0b-6611-11d3-bd2a-0000f80849bd");
        }

        public static object TestGetByRefDelegate(ref object x, object y) {
            object temp = x;
            x = y;
            return temp;
        }

        public static void Positive_TryFuncByRef(EU.IValidator V) {
            var t = new[] { typeof(object).MakeByRefType(), typeof(object), typeof(object) };

            Type funcType;
            EU.Equal(Expression.TryGetFuncType(t, out funcType), false);
            EU.Equal(Expression.TryGetActionType(t, out funcType), false);
            funcType = Expression.GetDelegateType(t);
            var d = Delegate.CreateDelegate(funcType, typeof(Scenarios).GetMethod("TestGetByRefDelegate"));
            var args = new object[] { "hello", "world" };
            var result = d.DynamicInvoke(args);
            EU.Equal(args[0], "world");
            EU.Equal(result, "hello");
        }

        public static void Positive_ConvertUnsigned(EU.IValidator V)
        {
            double x = uint.MaxValue;
            uint expected = (uint)x;
            var lambda = Expression.Lambda<Func<uint>>(Expression.Convert(Expression.Constant(x), typeof(uint)));
            var f = lambda.Compile();
            EU.Equal(f(), expected);
        }

    }
}
#endif
