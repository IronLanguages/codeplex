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
using System.Collections.Generic;
using TreeUtils = Microsoft.Scripting.Ast.Utils;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {

    // Tests of goto and similar branching constructs
    public static partial class Scenarios {

        // Break/continue not allowed from finally
        public static Expression Negative_BreakFromFinally(EU.IValidator V) {
            var label = Expression.Label();
            var label2 = Expression.Label();
            var e = Expression.Lambda<Action>(
                Expression.Loop(
                    Expression.TryFinally(
                        Expression.Empty(),
                        Expression.Loop(
                            Expression.Break(label),
                            label2
                        )
                    ),
                    label
                )
            );
            V.ValidateException<InvalidOperationException>(e, true);
            return e;
        }

        // Return from finally not allowed
        public static Expression Negative_ReturnFromFinally(EU.IValidator V) {
            var r = Expression.Label();
            var e = Expression.Lambda<Action>(
                Expression.Label(
                    r,
                    Expression.TryFinally(
                        Expression.Empty(),
                        Expression.Return(r)
                    )
                )
            );
            V.ValidateException<InvalidOperationException>(e, true);
            return e;
        }

        // Can't jump somewhere with non-empty stack
        public static Expression Negative_JumpIntoExpression(EU.IValidator V) {
            var label = Expression.Label(typeof(int));
            var e = Expression.Lambda<Action>(
                Expression.Block(
                    Expression.Add(Expression.Constant(123), Expression.Label(label, Expression.Constant(0))),
                    Expression.Goto(label, Expression.Constant(234))
                )
            );
            V.ValidateException<InvalidOperationException>(e, true);
            return e;
        }

        // Can't jump into any part of a TryStatement
        public static Expression Negative_JumpIntoTry(EU.IValidator V) {
            var label = Expression.Label();
            var e = Expression.Lambda(
                Expression.Block(
                    Expression.TryFinally(Expression.Label(label), Expression.Empty()),
                    Expression.Goto(label)
                )
            );
            V.ValidateException<InvalidOperationException>(e, true);
            return e;
        }

        public static Expression Negative_AmbiguousJump(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var a = Expression.Label();
            var r = Expression.Label(typeof(int));
            var e = Expression.Lambda<Func<int, int>>(
                Expression.Label(
                    r,
                    Expression.Block(
                        Expression.Condition(
                            Expression.LessThan(x, Expression.Constant(0)),
                            Expression.Goto(a),
                            Expression.Goto(a)
                        ),
                        Expression.Block(
                            Expression.Label(a),
                            Expression.Return(r, Expression.Constant(-1))
                        ),
                        Expression.Block(
                            Expression.Label(a),
                            Expression.Return(r, Expression.Constant(1))
                        ),
                        Expression.Constant(0)
                    )
                ),
                x
            );
            V.ValidateException<InvalidOperationException>(e, true);

            // Now, make the label occur first
            e = Expression.Lambda<Func<int, int>>(
                Expression.Label(
                    r,
                    Expression.Block(
                        Expression.Block(
                            Expression.Label(a),
                            Expression.Return(r, Expression.Constant(-1))
                        ),
                        Expression.Block(
                            Expression.Label(a),
                            Expression.Return(r, Expression.Constant(1))
                        ),
                        Expression.Condition(
                            Expression.LessThan(x, Expression.Constant(0)),
                            Expression.Goto(a),
                            Expression.Goto(a)
                        ),
                        Expression.Constant(0)
                    )
                ),
                x
            );

            V.ValidateException<InvalidOperationException>(e, true);
            return e;
        }

        // Jump to a label from inside and outside its DefaultValue
        public static Expression Positive_Goto(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var a = Expression.Label();
            var b = Expression.Label(typeof(int));
            var expr = Expression.Lambda<Func<int, int, int>>(
                Expression.Label(
                    b,
                    Expression.Block(
                        Expression.Label(a),
                        Expression.Assign(x, Expression.Add(x, Expression.Constant(1))),
                        Expression.Condition(
                            Expression.LessThan(x, y),
                            Expression.Goto(a),
                            Expression.Empty()
                        ),
                        Expression.Goto(b, x),
                        Expression.Constant(0)
                    )
                ),
                x, y
            );
            
            V.Validate(expr, f =>
            {
                EU.Equal(f(0, 123), 123);
                EU.Equal(f(123, 444), 444);
            });
            return expr;
        }

        public static Expression Positive_JumpIntoBlock(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var a = Expression.Label();
            var b = Expression.Label();
            var r = Expression.Label(typeof(int));
            var expr = Expression.Lambda<Func<int, int>>(
                Expression.Label(
                    r,
                    Expression.Block(
                        Expression.Condition(
                            Expression.LessThan(x, Expression.Constant(0)),
                            Expression.Goto(a),
                            Expression.Goto(b)
                        ),
                        Expression.Block(
                            Expression.Label(a),
                            Expression.Return(r, Expression.Constant(-1))
                        ),
                        Expression.Block(
                            Expression.Label(b),
                            Expression.Return(r, Expression.Constant(1))
                        ),
                        Expression.Constant(0)
                    )
                ),
                x
            );
            
            V.Validate(expr, f =>
            {
                EU.Equal(f(123), 1);
                EU.Equal(f(-42), -1);
                EU.Equal(f(456), 1);
            });
            return expr;
        }

        // Can reuse labels as long as jumps are inside-out
        public static Expression Positive_JumpReusingLabel(EU.IValidator V) {
            var label = Expression.Label(typeof(int));
            var expr = Expression.Lambda<Func<int>>(
                Expression.Add(
                    Expression.Label(
                        label,
                        Expression.Goto(label, Expression.Constant(123), typeof(int))
                    ),
                    Expression.Block(
                        Expression.Goto(label, Expression.Constant(444)),
                        Expression.Label(label, Expression.Constant(0))
                    )
                )
            );
            V.Validate(expr, f =>
            {
                EU.Equal(f(), 567);
            });
            return expr;
        }

        // also test EmitBranchBlock
        public static Expression Positive_JumpReusingLabel2(EU.IValidator V) {
            var label = Expression.Label(typeof(int));
            var temp = Expression.Variable(typeof(bool), null);
            var expr = Expression.Lambda<Func<int>>(
                Expression.Add(
                    Expression.Label(
                        label,
                        Expression.Goto(label, Expression.Constant(123), typeof(int))
                    ),
                    Expression.Condition(
                        Expression.Block(
                            new[] { temp },
                            Expression.Goto(label, Expression.Constant(555)),
                            Expression.Assign(temp, Expression.Constant(true)),
                            Expression.Label(label, Expression.Constant(0)),
                            temp
                        ),
                        Expression.Constant(8888),
                        Expression.Constant(444)
                    )
                )
            );
            V.Validate(expr, f =>
            {
                EU.Equal(f(), 567);
            });
            return expr;
        }

        //TODO: fixme

        // Possible JIT verifier bug, or dynamic methods don't allow this
        // See bug 527564, should work against .NET 4.0 but not 3.5
        public static Expression DisabledPositive_JumpToTryFromCatch(EU.IValidator V) {
            if (System.Environment.Version.Major >= 4) {
                var label = Expression.Label(typeof(string));
                var @return = Expression.Label(typeof(string));
                var expr = Expression.Lambda<Func<string>>(
                    Expression.Block(
                        Expression.TryCatch(
                            Expression.Block(
                                Expression.Divide(Expression.Constant(0), Expression.Constant(0)),
                                Expression.Return(
                                    @return,
                                    Expression.Label(label, Expression.Constant(""))
                                )
                            ),
                            Expression.Catch(
                                typeof(DivideByZeroException),
                                Expression.Goto(label, Expression.Constant("hello"))
                            )
                        ),
                        Expression.Label(@return, Expression.Constant(null, typeof(string)))
                    )
                );
                V.Validate(expr, f =>
                {
                    EU.Equal(f(), "hello");
                });
                return expr;
            } else {
                return Expression.Constant(true);
            }
        }

        public static Expression Positive_ReturnFromFinally(EU.IValidator V) {
            var r = Expression.Label(typeof(int));
            var e = Expression.Lambda<Func<int>>(
                Expression.Label(
                    r,
                    Expression.Block(
                        TreeUtils.FinallyFlowControl(
                            Expression.TryFinally(
                                Expression.Return(r, Expression.Constant(234)),
                                Expression.Return(r, Expression.Constant(123))
                            )
                        ),
                        Expression.Constant(0)
                    )
                )
            );
            V.Validate(e, f =>
            {
                EU.Equal(f(), 123);
            });
            return e;
        }

        public delegate int TestReturnFromFinally(out string result);

        public static Expression Positive_ReturnFromFinally2(EU.IValidator V) {
            var a = Expression.Label(typeof(string));
            var b = Expression.Label(typeof(int));
            var x = Expression.Parameter(typeof(string).MakeByRefType(), "x");
            var e = Expression.Lambda<TestReturnFromFinally>(
                Expression.Label(
                    b,
                    Expression.Block(
                        TreeUtils.FinallyFlowControl(
                            Expression.TryFinally(
                                Expression.Block(
                                    Expression.Assign(
                                        x,
                                        Expression.Label(
                                            a,
                                            Expression.Block(
                                                Expression.TryFinally(
                                                    Expression.Goto(a, Expression.Constant("foo")),
                                                    Expression.Goto(a, Expression.Constant("bar"))
                                                ),
                                                Expression.Constant(null, a.Type)
                                            )
                                        )
                                    ),
                                    Expression.Empty()
                                ),
                                Expression.Return(b, Expression.Constant(123))
                            )
                        ),
                        Expression.Constant(0)
                    )
                ),
                x
            );
            
            V.Validate(e, f =>
            {
                string test = "test";
                EU.Equal(f(out test), 123);
                EU.Equal(test, "bar");
            });
            return e;
        }

        // can't jump into assignment if left side is index/member
        public static void Negative_GotoAssignment(EU.IValidator V) {
            var target = Expression.Label(typeof(int));
            var lambda = Expression.Lambda<Action>(
                Expression.Block(
                    Expression.Goto(target, Expression.Constant(234)),
                    Expression.Assign(
                        Expression.ArrayAccess(Expression.Constant(new int[1]), Expression.Constant(0)),
                        Expression.Label(target, Expression.Constant(123))
                    )
                )
            );

            V.ValidateException<InvalidOperationException>(lambda, true);

            lambda = Expression.Lambda<Action>(
                Expression.Block(
                    Expression.Goto(target, Expression.Constant(234)),
                    Expression.Assign(
                        Expression.Property(
                            Expression.Constant(new List<int> { 123 }),
                            typeof(List<int>).GetProperty("Item"),
                            Expression.Constant(0)
                        ),
                        Expression.Label(target, Expression.Constant(123))
                    )
                )
            );

            V.ValidateException<InvalidOperationException>(lambda, true);
        }

        // can't jump from one catch to another
        public static void Negative_GotoAnotherCatch(EU.IValidator V) {
            var target = Expression.Label(typeof(void));
            var lambda = Expression.Lambda<Action>(
                Expression.TryCatch(
                    Expression.Empty(),
                    Expression.Catch(typeof(DivideByZeroException), Expression.Goto(target)),
                    Expression.Catch(typeof(ArrayTypeMismatchException), Expression.Label(target))
                )
            );

            V.ValidateException<InvalidOperationException>(lambda, true);
        }

        [ETUtils.Test(ETUtils.TestState.Enabled, "Positive_VerifyTailCallConditional", new string[] { "PartialTrust" })]
        public static void DisablePositive_VerifyTailCallConditional(EU.IValidator V) {
            var x = Expression.Parameter(typeof(bool), "x");
            var r = Expression.Label(typeof(int));
            var e = Expression.Lambda<Func<bool, int>>(
                Expression.Label(
                    r,
                    Expression.Condition(
                        x,
                        Expression.Return(r, Expression.Constant(1), typeof(int)),
                        Expression.Constant(2)
                    )
                ),
                true,
                x
            );
            var f = e.CompileAndVerify();
            EU.Equal(f(true), 1);
            EU.Equal(f(false), 2);
        }
        [ETUtils.Test(ETUtils.TestState.Enabled,"Positive_VerifyNonVoidReturn", new string [] {"PartialTrust"})]
        public static void Disable_Positive_VerifyNonVoidReturn(EU.IValidator V) {
            var r = Expression.Label();
            var e = Expression.Lambda<Action>(Expression.Label(r, Expression.Return(r, typeof(int))));
            var f = e.CompileAndVerify();
            f();
        }

        public static void Disable_Positive_GotoToIntLabelWithVoidReturn(EU.IValidator V) {
            LabelTarget returnTarget = Expression.Label(typeof(int));

            var lambda = Expression.Lambda<Action>(
                Expression.Block(
                    Expression.Condition(
                        Expression.Equal(Expression.Constant(1), Expression.Constant(1)),
                        Expression.Return(returnTarget, Expression.Constant(10)),
                        Expression.Return(returnTarget, Expression.Constant(-10))
                    ),
                    Expression.Label(returnTarget, Expression.Constant(0))
                )
            );

            var f = lambda.CompileAndVerify();
            f();
        }
    }
}
#endif
