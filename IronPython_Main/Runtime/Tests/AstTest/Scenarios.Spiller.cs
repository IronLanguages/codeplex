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
        private delegate int PrivateDelegate(int x);

        public static void Positive_SpillPrivateDelegate(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var e = Expression.Lambda<PrivateDelegate>(
                Expression.Add(
                    Expression.Constant(123),
                    Expression.Block(
                        Expression.TryFinally(
                            Expression.Assign(x, Expression.Add(x, Expression.Constant(111))),
                            Expression.Assign(x, Expression.Add(x, Expression.Constant(222)))
                        ),
                        x
                    )
                ),
                x
            );
            
            V.Validate(e, f =>
            {
                EU.Equal(f(0), 456);
                EU.Equal(f(321), 777);
            });
        }

        public static void TestByRef2(int x, ref int y) {
            y += x;
        }

        public static void Negative_SpillByRefArguments(EU.IValidator V) {
            var arg = Expression.Parameter(typeof(TestClass), "x");
            var x = Expression.Property(arg, "X");
            var lambda = Expression.Lambda<Action<TestClass>>(
                Expression.Call(
                    null,
                    typeof(Scenarios).GetMethod("TestByRef2"),
                    Expression.Add(
                        Expression.Constant(100),
                        Expression.TryFinally(
                            Expression.Constant(23),
                            Expression.AddAssign(x, Expression.Constant(432))
                        )
                    ),
                    x
                ),
                arg
            );
            V.ValidateException<NotSupportedException>(lambda, true);
            //var t = new TestClass { X = 222 };
            //f(t);
            //EU.Equal(t.X, 777);
        }

        public static void Negative_SpillMemberAssign(EU.IValidator V) {
            var x = Expression.Parameter(typeof(BoxedInt), "x");
            var e = Expression.Lambda<Func<BoxedInt, int>>(
                Expression.Block(                    
                    Expression.Assign(
                        Expression.Field(x, "X"),
                        Expression.Constant(123)
                    ),
                    Expression.Field(x, "X")
                ),
                x
            );

            V.Validate(e, f =>
            {
                EU.Equal(f(new BoxedInt { X = 444 }), 123);
            });

            e = Expression.Lambda<Func<BoxedInt, int>>(
                Expression.Block(                    
                    Expression.Assign(
                        Expression.Field(x, "X"),
                        Expression.TryFinally(
                            Expression.Constant(123),
                            Expression.Empty()
                        )
                    ),
                    Expression.Field(x, "X")
                ),
                x
            );
            V.ValidateException<NotSupportedException>(e, true);
        }

        public static void Negative_GotoSpilledExpression(EU.IValidator V) {
            var target = Expression.Label(typeof(int), "MyLabel");
            var sb = Expression.Variable(typeof(System.Text.StringBuilder), "stringBuilder");

            var e = Expression.Lambda<Func<int>>(
                Expression.Block(
                    new[] { sb },
                    Expression.Assign(sb, Expression.Constant(new System.Text.StringBuilder(5))),
                    Expression.Goto(target, Expression.Constant(9)),
                    Expression.Assign(
                        Expression.Property(sb, typeof(System.Text.StringBuilder).GetProperty("Capacity")),
                        Expression.Label(
                            target,
                            Expression.Block(
                                Expression.TryCatch(
                                    Expression.Empty(),
                                    Expression.Catch(
                                        typeof(Exception),
                                        Expression.Empty()
                                    )
                                ),
                                Expression.Constant(7)
                            )
                        )
                    )
                )
            );

            // control cannot enter an expression
            V.ValidateException<InvalidOperationException>(e, true);
        }

        public static void Positive_SpillNewArrayBounds(EU.IValidator V) {
            var e = Expression.Lambda<Func<int[,]>>(
                Expression.NewArrayBounds(
                    typeof(int),
                    Expression.Constant(3),
                    Expression.TryFinally(Expression.Constant(2), Expression.Empty())
                )
            );

            V.Validate(e, f =>
            {
                int[,] a = f();
                EU.Equal(a.GetLength(0), 3);
                EU.Equal(a.GetLength(1), 2);
            });
        }

        public static void Positive_SpillNegateChecked(EU.IValidator V) {
            var e = Expression.Lambda<Action>(
                Expression.Block(
                    Expression.NegateChecked(
                        Expression.Block(
                            Expression.TryCatch(
                                Expression.Empty(),
                                Expression.Catch(
                                    typeof(Exception),
                                    Expression.Empty()
                                )
                            ),
                            Expression.Constant((short)1, typeof(short))
                        )
                    ),

                    Expression.Empty()
                )
            );
            V.Validate(e);
        }

        public sealed class TransparentReducible : Expression {
            private readonly Expression _inner;
            public TransparentReducible(Expression inner) {
                _inner = inner;
            }

            public override bool CanReduce {
                get { return true; }
            }
            public override Expression Reduce() {
                return _inner;
            }
            public sealed override ExpressionType NodeType {
                get { return ExpressionType.Extension; }
            }
            public sealed override Type Type {
                get { return _inner.Type; }
            }
            protected override Expression VisitChildren(ExpressionVisitor visitor) {
                var i = visitor.Visit(_inner);
                if (i == _inner) {
                    return this;
                }
                return new TransparentReducible(i);
            }
        }

        public static void Positive_TypeEqual_Spill(EU.IValidator V) {
            var x = Expression.Parameter(typeof(Exception), "x");
            var e = Expression.Lambda<Func<Exception, bool>>(
                Expression.TypeEqual(new TransparentReducible(x), typeof(Exception)),
                x
            );
            
            V.Validate(e, f =>
            {
                EU.Equal(f(new Exception()), true);
                EU.Equal(f(new NotImplementedException()), false);
            });
        }
    }
}
#endif
