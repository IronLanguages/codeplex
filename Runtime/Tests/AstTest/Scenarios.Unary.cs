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
using System.Runtime.CompilerServices;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {

    public static partial class Scenarios {

        // Test quote (closing over a parameter)
        public static void Positive_Quote(EU.IValidator V) {
            ParameterExpression x = Expression.Parameter(typeof(int), "x");
            ParameterExpression y = Expression.Parameter(typeof(int), "y");
            var e = Expression.Lambda<Func<int, Expression>>(
                Expression.Quote(
                    Expression.Lambda<Func<int, int>>(Expression.Add(x, y), y)
                ),
                x
            );

            V.Validate(e, f =>
            {
                Func<int, int> z = ((Expression<Func<int, int>>)f(123)).Compile();
                int a = z(111);
                int b = z(222);
                int c = z(333);
                EU.Equal(a, 234);
                EU.Equal(b, 345);
                EU.Equal(c, 456);
            });
        }

        // Test quote (closing over a parameter and mutating it)
        public static void Positive_QuoteMutation(EU.IValidator V) {
            ParameterExpression x = Expression.Parameter(typeof(int), "x");
            ParameterExpression y = Expression.Parameter(typeof(int), "y");
            var e = Expression.Lambda<Func<int, Expression<Func<int, int>>>>(
                Expression.Quote(
                    Expression.Lambda<Func<int, int>>(
                        Expression.Assign(x, Expression.Add(x, y)),
                        y
                    )
                ),
                x
            );

            V.Validate(e, f =>
            {
                Expression<Func<int, int>> w = f(123);

                Func<int, int> z = f(123).Compile();
                int a = z(111);
                int b = z(222);
                int c = z(333);
                EU.Equal(a, 234);
                EU.Equal(b, 456);
                EU.Equal(c, 789);
            });
        }

        // Test quote (closing over a parameter and mutating it using ref)
        public static void Positive_QuoteMutationRef(EU.IValidator V) {
            ParameterExpression x = Expression.Parameter(typeof(int), "x");
            ParameterExpression y = Expression.Parameter(typeof(int), "y");
            var e = Expression.Lambda<Func<int, Expression>>(
                Expression.Quote(
                    Expression.Lambda<Func<int, int>>(
                        Expression.Call(typeof(Scenarios).GetMethod("AddAssign"), x, y),
                        y
                    )
                ),
                x
            );

            V.Validate(e, f =>
            {
                Func<int, int> z = ((Expression<Func<int, int>>)f(123)).Compile();
                int a = z(111);
                int b = z(222);
                int c = z(333);
                EU.Equal(a, 234);
                EU.Equal(b, 456);
                EU.Equal(c, 789);
            });
        }

        public static int AddAssign(ref int x, int y) {
            return x += y;
        }

        // Verify that the expression tree mutates the boxed value
        public static void Positive_Unbox1(EU.IValidator V) {
            object s = new STestCalls();
            var e = Expression.Lambda<Action>(
                Expression.Block(
                    Expression.Call(
                        Expression.Unbox(Expression.Constant(s, typeof(object)), typeof(STestCalls)),
                        typeof(STestCalls).GetMethod("SetX")
                    ),
                    Expression.Property(
                        Expression.Unbox(Expression.Constant(s, typeof(object)), typeof(STestCalls)),
                        typeof(STestCalls).GetProperty("SetXProp")
                    ),
                    Expression.Empty()
                )
            );

            V.Validate(e, f =>
            {
                f();
                EU.Equal(((STestCalls)s).X, 18);
            });
        }

        // Inserts a call frame to ensure the object is of the correct type
        // on both ends (in this case, an interface)
        public static T Pass<T>(T t) {
            return t;
        }

        private static Expression MakePass(Expression e) {
            return Expression.Call(typeof(Scenarios).GetMethod("Pass").MakeGenericMethod(e.Type), e);
        }

        public static void Positive_Unbox2(EU.IValidator V) {
            // Test unbox on interfaces
            object s = new STestCalls();
            var e = Expression.Lambda<Action>(
                Expression.Block(
                    Expression.Call(
                        Expression.Unbox(MakePass(Expression.Constant(s, typeof(ITestCalls))), typeof(STestCalls)),
                        typeof(STestCalls).GetMethod("SetX")
                    ),
                    Expression.Property(
                        Expression.Unbox(MakePass(Expression.Constant(s, typeof(ITestCalls))), typeof(STestCalls)),
                        typeof(STestCalls).GetProperty("SetXProp")
                    ),
                    Expression.Empty()
                )
            );
            V.Validate(e, f =>
            {
                f();
                // Verify that the boxed value was mutated
                EU.Equal(((STestCalls)s).X, 18);
            });
        }


        public static void Positive_Unbox3(EU.IValidator V) {
            // Unbox to value
            object s = new STestCalls();
            var e = Expression.Lambda<Action>(
                Expression.Block(
                    Expression.Call(
                        MakePass(Expression.Unbox(Expression.Constant(s, typeof(object)), typeof(STestCalls))),
                        typeof(STestCalls).GetMethod("SetX")
                    ),
                    Expression.Property(
                        MakePass(Expression.Unbox(Expression.Constant(s, typeof(object)), typeof(STestCalls))),
                        typeof(STestCalls).GetProperty("SetXProp")
                    ),
                    Expression.Empty()
                )
            );

            V.Validate(e, f =>
            {
                f();
                // Verify that the value was not mutated
                EU.Equal(((STestCalls)s).X, 0);
            });
        }

        // Verify that convert doesn't mutate value types
        // See Dev10 bug 471736
        public static void Positive_ConvertMutation(EU.IValidator V) {
            object s = new STestCalls();
            var e = Expression.Lambda<Action>(
                Expression.Block(
                    Expression.Call(
                        Expression.Convert(Expression.Constant(s, typeof(object)), typeof(STestCalls)),
                        typeof(STestCalls).GetMethod("SetX")
                    ),
                    Expression.Property(
                        Expression.Convert(Expression.Constant(s, typeof(object)), typeof(STestCalls)),
                        typeof(STestCalls).GetProperty("SetXProp")
                    ),
                    Expression.Empty()
                )
            );
            V.Validate(e, f =>
            {
                f();
                // Verify that the boxed value was not mutated
                EU.Equal(((STestCalls)s).X, 0);
            });
        }

        public delegate TRet FuncRef<T0, TRet>(ref T0 arg0);

        public static void Positive_IncrementVariable(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int).MakeByRefType(), "x");
            var e = Expression.Lambda<FuncRef<int, int>>(Expression.Increment(x), x);
            int y = 123;
            V.Validate(e, f =>
            {
                EU.Equal(f(ref y), 124);
                EU.Equal(y, 123);
            });

            e = Expression.Lambda<FuncRef<int, int>>(Expression.PreIncrementAssign(x), x);
            V.Validate(e, f =>
            {
                EU.Equal(f(ref y), 124);
                EU.Equal(y, 124);
            });

            e = Expression.Lambda<FuncRef<int, int>>(Expression.PostIncrementAssign(x), x);
            V.Validate(e, f =>
            {
                EU.Equal(f(ref y), 124);
                EU.Equal(y, 125);
            });
        }

        public static void Positive_DecrementVariable(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int).MakeByRefType(), "x");
            var e = Expression.Lambda<FuncRef<int, int>>(Expression.Decrement(x), x);
            int y = 123;
            V.Validate(e, f =>
            {
                EU.Equal(f(ref y), 122);
                EU.Equal(y, 123);
            });

            e = Expression.Lambda<FuncRef<int, int>>(Expression.PreDecrementAssign(x), x);
            V.Validate(e, f =>
            {
                EU.Equal(f(ref y), 122);
                EU.Equal(y, 122);
            });

            e = Expression.Lambda<FuncRef<int, int>>(Expression.PostDecrementAssign(x), x);
            V.Validate(e, f =>
            {
                EU.Equal(f(ref y), 122);
                EU.Equal(y, 121);
            });
        }

        public static void Positive_IncrementMember(EU.IValidator V) {
            var box = new StrongBox<int>();
            int evalCount = 0;
            Func<StrongBox<int>> getBox = () => {
                evalCount++;
                return box;
            };

            var member = Expression.Field(Expression.Invoke(Expression.Constant(getBox)), "Value");

            var x = Expression.Parameter(typeof(int), "x");
            var e = Expression.Lambda<Func<int>>(Expression.Increment(member));
            box.Value = 123;
            V.Validate(e, f =>
            {
                EU.Equal(f(), 124);
                EU.Equal(box.Value, 123);
                EU.Equal(evalCount, 1);
            });

            e = Expression.Lambda<Func<int>>(Expression.PreIncrementAssign(member));
            V.Validate(e, f =>
            {
                EU.Equal(f(), 124);
                EU.Equal(box.Value, 124);
                EU.Equal(evalCount, 2);
            });

            e = Expression.Lambda<Func<int>>(Expression.PostIncrementAssign(member));
            V.Validate(e, f =>
            {
                EU.Equal(f(), 124);
                EU.Equal(box.Value, 125);
                EU.Equal(evalCount, 3);
            });
        }

        public class TestUnaryOpWithStaticField {
            static public int x = 123;
        }

        public static void Positive_IncrementStaticMember(EU.IValidator V) {
            //TestUnaryOpWithStaticField.x++
            var member = Expression.Field(null, typeof(TestUnaryOpWithStaticField).GetField("x"));
            var e = Expression.Lambda<Func<int>>(Expression.Increment(member));
            V.Validate(e, f =>
            {
                EU.Equal(f(), 124);
            });
        }

        public static void Positive_DecrementStaticMember(EU.IValidator V) {
            //TestUnaryOpWithStaticField.x--
            var member = Expression.Field(null, typeof(TestUnaryOpWithStaticField).GetField("x"));
            var e = Expression.Lambda<Func<int>>(Expression.Decrement(member));
            V.Validate(e, f =>
            {
                EU.Equal(f(), 122);
            });
        }

        public static void Positive_DecrementMember(EU.IValidator V) {
            var box = new StrongBox<int>();
            int evalCount = 0;
            Func<StrongBox<int>> getBox = () => {
                evalCount++;
                return box;
            };

            var member = Expression.Field(Expression.Invoke(Expression.Constant(getBox)), "Value");

            var x = Expression.Parameter(typeof(int), "x");
            var e = Expression.Lambda<Func<int>>(Expression.Decrement(member));
            box.Value = 123;
            V.Validate(e, f =>
            {
                EU.Equal(f(), 122);
                EU.Equal(box.Value, 123);
                EU.Equal(evalCount, 1);
            });

            e = Expression.Lambda<Func<int>>(Expression.PreDecrementAssign(member));
            V.Validate(e, f =>
            {
                EU.Equal(f(), 122);
                EU.Equal(box.Value, 122);
                EU.Equal(evalCount, 2);
            });

            e = Expression.Lambda<Func<int>>(Expression.PostDecrementAssign(member));
            V.Validate(e, f =>
            {
                EU.Equal(f(), 122);
                EU.Equal(box.Value, 121);
                EU.Equal(evalCount, 3);
            });
        }

        public static void Positive_IncrementIndex(EU.IValidator V) {
            var array = new int[5, 5];
            int evalCount = 0;
            Func<int[,]> getBox = () => {
                evalCount++;
                return array;
            };

            int arg1Count = 0;
            Func<int> getArg1 = () => {
                arg1Count++;
                return 1;
            };

            int arg2Count = 0;
            Func<int> getArg2 = () => {
                arg2Count++;
                return 2;
            };

            var index = Expression.ArrayAccess(
                Expression.Invoke(Expression.Constant(getBox)),
                Expression.Invoke(Expression.Constant(getArg1)),
                Expression.Invoke(Expression.Constant(getArg2))
            );

            var x = Expression.Parameter(typeof(int), "x");
            var e = Expression.Lambda<Func<int>>(Expression.Increment(index));
            array[1, 2] = 123;
            V.Validate(e, f =>
            {
                EU.Equal(f(), 124);
                EU.Equal(array[1, 2], 123);
                EU.Equal(evalCount, 1);
                EU.Equal(arg1Count, 1);
                EU.Equal(arg2Count, 1);
            });

            e = Expression.Lambda<Func<int>>(Expression.PreIncrementAssign(index));
            V.Validate(e, f =>
            {
                EU.Equal(f(), 124);
                EU.Equal(array[1, 2], 124);
                EU.Equal(evalCount, 2);
                EU.Equal(arg1Count, 2);
                EU.Equal(arg2Count, 2);
            });

            e = Expression.Lambda<Func<int>>(Expression.PostIncrementAssign(index));
            V.Validate(e, f =>
            {
                EU.Equal(f(), 124);
                EU.Equal(array[1, 2], 125);
                EU.Equal(evalCount, 3);
                EU.Equal(arg1Count, 3);
                EU.Equal(arg2Count, 3);
            });
        }

        public static void Positive_DecrementIndex(EU.IValidator V) {
            var array = new int[5, 5];
            int evalCount = 0;
            Func<int[,]> getBox = () => {
                evalCount++;
                return array;
            };

            int arg1Count = 0;
            Func<int> getArg1 = () => {
                arg1Count++;
                return 1;
            };

            int arg2Count = 0;
            Func<int> getArg2 = () => {
                arg2Count++;
                return 2;
            };

            var index = Expression.ArrayAccess(
                Expression.Invoke(Expression.Constant(getBox)),
                Expression.Invoke(Expression.Constant(getArg1)),
                Expression.Invoke(Expression.Constant(getArg2))
            );

            var x = Expression.Parameter(typeof(int), "x");
            var e = Expression.Lambda<Func<int>>(Expression.Decrement(index));
            array[1, 2] = 123;
            V.Validate(e, f =>
            {
                EU.Equal(f(), 122);
                EU.Equal(array[1, 2], 123);
                EU.Equal(evalCount, 1);
                EU.Equal(arg1Count, 1);
                EU.Equal(arg2Count, 1);
            });

            e = Expression.Lambda<Func<int>>(Expression.PreDecrementAssign(index));
            V.Validate(e, f =>
            {
                EU.Equal(f(), 122);
                EU.Equal(array[1, 2], 122);
                EU.Equal(evalCount, 2);
                EU.Equal(arg1Count, 2);
                EU.Equal(arg2Count, 2);
            });

            e = Expression.Lambda<Func<int>>(Expression.PostDecrementAssign(index));
            V.Validate(e, f =>
            {
                EU.Equal(f(), 122);
                EU.Equal(array[1, 2], 121);
                EU.Equal(evalCount, 3);
                EU.Equal(arg1Count, 3);
                EU.Equal(arg2Count, 3);
            });
        }

        public class Adjustable {
            public readonly int Value;

            public Adjustable(int value) {
                Value = value;
            }

            public static Adjustable operator ++(Adjustable a) {
                return new Adjustable(a.Value + 1);
            }

            public static Adjustable operator --(Adjustable a) {
                return new Adjustable(a.Value - 1);
            }

            public override bool Equals(object obj) {
                return (obj is int) && Value == (int)obj;
            }

            public override int GetHashCode() {
                return Value.GetHashCode();
            }
        }

        public static void Positive_IncrementUserDefined(EU.IValidator V) {
            var x = Expression.Parameter(typeof(Adjustable).MakeByRefType(), "x");
            var e = Expression.Increment(x);
            EU.Equal(e.Method, typeof(Adjustable).GetMethod("op_Increment"));
            var e1 = Expression.Lambda<FuncRef<Adjustable, Adjustable>>(e, x);
            var y = new Adjustable(123);
            V.Validate(e1, f =>
            {
                var z = f(ref y);
                EU.Equal(z.Value, 124);
                EU.Equal(y.Value, 123);
            });

            var e2 = Expression.Lambda<FuncRef<Adjustable, Adjustable>>(Expression.PreIncrementAssign(x), x);
            V.Validate(e2, f =>
            {
                var z = f(ref y);
                EU.Equal(z.Value, 124);
                EU.Equal(y.Value, 124);
            });

            var e3 = Expression.Lambda<FuncRef<Adjustable, Adjustable>>(Expression.PostIncrementAssign(x), x);
            V.Validate(e3, f =>
            {
                var z = f(ref y);
                EU.Equal(z.Value, 124);
                EU.Equal(y.Value, 125);
            });
        }

        public static void Positive_DecrementUserDefined(EU.IValidator V) {
            var x = Expression.Parameter(typeof(Adjustable).MakeByRefType(), "x");
            var e = Expression.Decrement(x);
            EU.Equal(e.Method, typeof(Adjustable).GetMethod("op_Decrement"));
            var e1 = Expression.Lambda<FuncRef<Adjustable, Adjustable>>(e, x);
            var y = new Adjustable(123);
            V.Validate(e1, f =>
            {
                var z = f(ref y);
                EU.Equal(z.Value, 122);
                EU.Equal(y.Value, 123);
            });

            var e2 = Expression.Lambda<FuncRef<Adjustable, Adjustable>>(Expression.PreDecrementAssign(x), x);
            V.Validate(e2, f =>
            {
                var z = f(ref y);
                EU.Equal(z.Value, 122);
                EU.Equal(y.Value, 122);
            });

            var e3 = Expression.Lambda<FuncRef<Adjustable, Adjustable>>(Expression.PostDecrementAssign(x), x);
            V.Validate(e3, f =>
            {
                var z = f(ref y);
                EU.Equal(z.Value, 122);
                EU.Equal(y.Value, 121);
            });
        }

        public static void Positive_IncrementBigTypes(EU.IValidator V) {
            var e1 = Expression.Lambda<Func<long>>(Expression.Increment(Expression.Constant((long)UInt32.MaxValue)));
            V.Validate(e1, f => {
                EU.Equal(1 + (long)UInt32.MaxValue, f());
            });

            var e2 = Expression.Lambda<Func<ulong>>(Expression.Increment(Expression.Constant((ulong)UInt32.MaxValue)));
            V.Validate(e2, f => {
                EU.Equal(1 + (ulong)UInt32.MaxValue, f());
            });

            var e3 = Expression.Lambda<Func<float>>(Expression.Increment(Expression.Constant((float)UInt32.MaxValue)));
            V.Validate(e3, f => {
                EU.Equal(1 + (float)UInt32.MaxValue, f());
            });

            var e4 = Expression.Lambda<Func<double>>(Expression.Increment(Expression.Constant((double)UInt32.MaxValue)));
            V.Validate(e4, f => {
                EU.Equal(1 + (double)UInt32.MaxValue, f());
            });
        }

        public static void Positive_DecrementBigTypes(EU.IValidator V) {
            EU.Equal(
                (long)-1,
                Expression.Lambda<Func<long>>(
                    Expression.Decrement(Expression.Constant((long)0))
                ).Compile()()
            );

            EU.Equal(
                (ulong)0,
                Expression.Lambda<Func<ulong>>(
                    Expression.Decrement(Expression.Constant((ulong)1))
                ).Compile()()
            );

            EU.Equal(
                (float)-1,
                Expression.Lambda<Func<float>>(
                    Expression.Decrement(Expression.Constant((float)0))
                ).Compile()()
            );

            EU.Equal(
                (double)-1,
                Expression.Lambda<Func<double>>(
                    Expression.Decrement(Expression.Constant((double)0))
                ).Compile()()
            );

        }

        public struct GenericStruct<T> {
            T t;

            public T Value {
                get { return t; }
                set { t = value; }
            }
        };

        public static void Positive_UsingGenericTypesIllegaly(EU.IValidator V) {
            EU.Throws<ArgumentException>(
                delegate() {
                    ParameterExpression param = Expression.Parameter(typeof(object));
                    Expression.Lambda<Action<object>>(
                        Expression.Convert(param, typeof(Action<>)),
                        param
                    ).Compile();
                }
            );

            EU.Throws<ArgumentException>(
                delegate() {
                    ParameterExpression param = Expression.Parameter(typeof(object));
                    Expression.Lambda<Action<object>>(
                        Expression.ConvertChecked(param, typeof(Action<>)),
                        param
                    ).Compile();
                }
            );

            EU.Throws<ArgumentException>(
                delegate() {
                    ParameterExpression param = Expression.Parameter(typeof(object));
                    Expression.Lambda<Action<object>>(
                        Expression.Throw(param, typeof(Action<>)),
                        param
                    ).Compile();
                }
            );

            EU.Throws<ArgumentException>(
                delegate() {
                    ParameterExpression param = Expression.Parameter(typeof(object));
                    Expression.Lambda<Action<object>>(
                        Expression.TypeAs(param, typeof(Action<>)),
                        param
                    ).Compile();
                }
            );

            EU.Throws<ArgumentException>(
                delegate() {
                    ParameterExpression param = Expression.Parameter(typeof(object));
                    Expression.Lambda<Action<object>>(
                        Expression.Unbox(param, typeof(GenericStruct<>)),
                        param
                    ).Compile();
                }
            );
        }
    }
}
#endif
