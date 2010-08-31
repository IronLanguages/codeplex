#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Goto {
        // Pass null to label
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 1", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Goto1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            LabelTarget target = Expr.Label(typeof(void));
            LabelExpression label = Expr.Label(target);

            var go = EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Goto(null);
            });

            Expressions.Add(go);

            return Expr.Empty();
        }

        // Goto a valid void label
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 2", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(void));
            LabelExpression label = Expr.Label(target);

            GotoExpression go = Expr.Goto(target);
            Expr test = Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Before")), label, EU.ConcatEquals(Result, "After"));

            Expressions.Add(go);
            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("After"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto a valid non-void label
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 3", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Goto3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(int));
            LabelExpression label = Expr.Label(target, Expr.Constant(1));

            var go = EU.Throws<ArgumentException>(() => { Expr.Goto(target); });

            return Expr.Empty();
        }

        // Pass null to value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 4", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(void));
            LabelExpression label = Expr.Label(target);

            GotoExpression go = Expr.Goto(target);
            Expr test = Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Before")), label, EU.ConcatEquals(Result, "After"));

            Expressions.Add(go);
            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("After"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 5", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Goto5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(string));
            LabelExpression label = Expr.Label(target, Expr.Constant("TestValue"));

            var go = EU.Throws<ArgumentException>(() => { Expr.Goto(target); });

            return Expr.Empty();
        }

        // Goto a void label passing a value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 6", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(void));
            LabelExpression label = Expr.Label(target);

            GotoExpression go = Expr.Goto(target, Expr.Block(EU.ConcatEquals(Result, Expression.Constant("Hello")), Expr.Constant("Hello")));
            Expr test = Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Before")), label, EU.ConcatEquals(Result, "After"));

            Expressions.Add(go);
            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("HelloAfter"), Result, "Goto 1"));

            var tree = Expression.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto a void label passing a void value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 6_1", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto6_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(void));
            LabelExpression label = Expr.Label(target);

            GotoExpression go = Expr.Goto(target, Expr.Empty());
            Expr test = Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Before")), label, EU.ConcatEquals(Result, "After"));

            Expressions.Add(go);
            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("After"), Result, "Goto 1"));

            var tree = Expression.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto a valid label passing a correctly typed block to value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 7", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test = Expr.Block(
                Expr.Assign(
                    Result2,
                    Expr.Label(
                        target,
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("Before")),
                            Expr.Goto(
                                target,
                                Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Comma")), Expr.Constant("Goto"))
                            ),
                            Expr.Constant("TestValue")
                        )
                    )
                ),
                EU.ConcatEquals(Result, "After")
            );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeforeCommaAfter"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Goto"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto a valid label passing a non-void block to value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 8", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(int), "Result2");

            LabelTarget target = Expr.Label(typeof(int));

            Expr test = Expr.Block(
                Expr.Assign(
                    Result2,
                    Expr.Label(
                        target,
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("Before")),
                            Expr.Goto(
                                target,
                                Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Block")), Expr.Constant(3))
                            ),
                            Expr.Constant(2)
                        )
                    )
                ),
                EU.ConcatEquals(Result, "After")
            );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeforeBlockAfter"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto a valid label passing a void block to value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 9", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(void));
            LabelExpression label = Expr.Label(target);

            GotoExpression go = Expr.Goto(target, Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Block")), Expr.Empty()));
            Expr test = Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Before")), label, EU.ConcatEquals(Result, "After"));

            Expressions.Add(go);
            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BlockAfter"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto a valid label passing a block to value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 10", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Goto10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(int), "Result2");

            LabelTarget target = Expr.Label(typeof(string));
            LabelExpression label = Expr.Label(target, Expr.Constant("Hello"));

            var go = Expr.Goto(target, Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Block")), Expr.Constant("World")));
            Expr test =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Before")), Expr.Assign(Result2, label), EU.ConcatEquals(Result, "After"));
                });

            return Expr.Empty();
        }

        // Goto a label defined in the same scope and below the Goto
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 11", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label();

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Before")),
                    EU.Goto(Result2, target, Expr.Constant("Go")),
                    EU.ConcatEquals(Result, Expr.Constant("After")),
                    EU.Label(Result2, target, Expr.Constant("Default")),
                    EU.ConcatEquals(Result, "End")
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeforeEnd"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto a label in a nested scope of the goto
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 12", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(double), "Result2");

            LabelTarget target = Expr.Label();

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Before")),
                    EU.Goto(Result2, target, Expr.Constant(3.0)),
                    EU.ConcatEquals(Result, Expr.Constant("After")),
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("InnerBegin")),
                        EU.Label(Result2, target, Expr.Constant(1.0)),
                        EU.ConcatEquals(Result, Expr.Constant("InnerEnd"))
                    ),
                    EU.ConcatEquals(Result, "OuterEnd")
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeforeInnerEndOuterEnd"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3.0), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto a label in an outer scope of the goto
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 13", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(double), "Result2");

            LabelTarget target = Expr.Label();

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    EU.Label(Result2, target, Expr.Constant(1.0)),
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("2")),
                        Expr.Condition(
                            Expr.Equal(Result2, Expr.Constant(1.0), false, null),
                            Expr.Block(
                                EU.Goto(Result2, target, Expr.Constant(3.0)),
                                EU.ConcatEquals(Result, Expr.Constant("3"))
                            ),
                            Expr.Block(EU.ConcatEquals(Result, Expr.Constant("4")))
                        )
                    ),
                    EU.ConcatEquals(Result, "5")
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12245"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3.0), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto a label which hides a label of the same name in an outer scope
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 14", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(double), "Result2");

            LabelTarget target = Expr.Label(typeof(void), "MyLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    EU.Label(Result2, target, Expr.Constant(1.0)),
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("2")),
                        Expr.Condition(
                            Expr.Equal(Result2, Expr.Constant(1.0), false, null),
                            Expr.Block(
                                EU.Goto(Result2, target, Expr.Constant(3.0)),
                                EU.ConcatEquals(Result, Expr.Constant("3")),
                                EU.Label(Result2, target, Expr.Constant(2.0)),
                                Expr.Empty()
                            ),
                            Expr.Block(EU.ConcatEquals(Result, Expr.Constant("4")), Expr.Empty())
                        )
                    ),
                    EU.ConcatEquals(Result, "5")
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1245"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3.0), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From an outer scope Goto a label that is reused in multiple expressions
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 15", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(double), "Result2");

            LabelTarget target = Expr.Label(typeof(double), "MyLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant(1.0))),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant(2.0))),
                    Expr.Goto(target, Expr.Constant(3.0)),
                    EU.ConcatEquals(Result, "5")
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1245"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3.0), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From an outer scope Goto a label that is reused in multiple expressions in a valid way
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 16", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(double), "Result2");

            LabelTarget target = Expr.Label(typeof(void), "MyLabel");
            Expression label = EU.Label(Result2, target, Expr.Constant(1.0));

            LambdaExpression lambda1 =
                Expr.Lambda(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("1")),
                        EU.Goto(Result2, target, Expr.Constant(2.0)),
                        EU.ConcatEquals(Result, Expr.Constant("2")),
                        label,
                        EU.ConcatEquals(Result, Expr.Constant("3"))
                    )
                );

            LambdaExpression lambda2 =
                Expr.Lambda(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("4")),
                        EU.Goto(Result2, target, Expr.Constant(3.0)),
                        EU.ConcatEquals(Result, Expr.Constant("5")),
                        label,
                        EU.ConcatEquals(Result, Expr.Constant("6"))
                    )
                );

            Expr test =
                Expr.Block(
                    Expr.Invoke(lambda1),
                    Expr.Invoke(lambda2)
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1346"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3.0), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto into assignment expression (i.e., left hand side is a ParameterExpression)
        // This is no longer legal.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 17", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(double), "Result2");

            LabelTarget target = Expr.Label(typeof(double), "MyLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant(2.0)),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant(1.0))),
                    EU.ConcatEquals(Result, Expr.Constant("3"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("13"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2.0), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto into assignment expression illegally (i.e., left hand side is not a ParameterExpression)
        // Regression for Dev10 Bug 539576
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 18", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(int), "Result2");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");

            ParameterExpression SB = Expr.Variable(typeof(System.Text.StringBuilder), "stringBuilder");
            Expressions.Add(Expr.Assign(SB, Expr.Constant(new System.Text.StringBuilder(5))));
            PropertyInfo pi = typeof(System.Text.StringBuilder).GetProperty("Capacity");

            var Arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.ArrayIndex(Arr, Expr.Constant(1)), "Arr"));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant(9)),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Assign(
                        Expr.Property(SB, pi),
                        Expr.Label(target, Expr.Constant(7))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("3"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("13"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(9), Result2, "Goto 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Expr.Property(SB, pi), "Goto 3"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(9), Expr.ArrayIndex(Arr, Expr.Constant(2)), "Arr 2"));

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "DoNotVisitTemp");
            var tree = Expr.Block(new[] { Result, Result2, SB, DoNotVisitTemp }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto into condition test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 19", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(double), "MyLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant(1.0)),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Condition(
                        Expr.Equal(Expr.Label(target, Expr.Constant(0.0)), Expr.Constant(1.0), false, null),
                        EU.ConcatEquals(Result, Expr.Constant("3")),
                        EU.ConcatEquals(Result, Expr.Constant("4"))
                    ),
                    EU.ConcatEquals(Result, "5")
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("135"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto into condition iftrue
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 20", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(double), "Result2");

            LabelTarget target = Expr.Label(typeof(void), "MyLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    EU.Goto(Result2, target, Expr.Constant(5.0)),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Condition(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            Expr.Equal(Expr.Constant(0.0), Expr.Constant(1.0), false, null)
                        ),
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            EU.Label(Result2, target, Expr.Constant(0.0)),
                            EU.ConcatEquals(Result, Expr.Constant("5")),
                            Expr.Empty()
                        ),
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("6")),
                            Expr.Empty()
                        )
                    ),
                    EU.ConcatEquals(Result, "7")
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("157"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(5.0), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto into condition iffalse
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 21", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(double), "Result2");

            LabelTarget target = Expr.Label(typeof(void), "MyLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    EU.Goto(Result2, target, Expr.Constant(5.0)),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Condition(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            Expr.Equal(Expr.Constant(1.0), Expr.Constant(1.0), false, null)
                        ),
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Empty()
                        ),
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("5")),
                            EU.Label(Result2, target, Expr.Constant(0.0)),
                            EU.ConcatEquals(Result, Expr.Constant("6")),
                            Expr.Empty()
                        )
                    ),
                    EU.ConcatEquals(Result, "7")
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("167"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(5.0), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto try block
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 22", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant("Go")),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                            EU.ConcatEquals(Result, Expr.Constant("4"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("5"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("145"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto a valid label in try from within try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 23", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(void));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            EU.Goto(Result2, target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            EU.Label(Result2, target, Expr.Constant("Default")),
                            EU.ConcatEquals(Result, Expr.Constant("4"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("5"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1245"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto try from catch
        // Regression for Dev10 Bug 527564.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 24", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                                Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Goto(target, Expr.Constant("Go")),
                                EU.ConcatEquals(Result, Expr.Constant("5"))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("6"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12436"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto try from finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 24_1", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto24_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatchFinally(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("5"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))

                    ),
                    EU.ConcatEquals(Result, Expr.Constant("6"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1234"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto catch from outside try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 25", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant("Go")),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("Caught")),
                                Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                EU.ConcatEquals(Result, Expr.Constant("4"))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("5"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1Caught4"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto catch from another catch
        // Regression for Dev10 Bug 540256
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 25_1", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto25_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException(), typeof(DivideByZeroException))),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        new CatchBlock[] {
                            Expr.Catch(
                                typeof(DivideByZeroException),
                                Expr.Block(
                                    EU.ConcatEquals(Result, Expr.Constant("Caught")),
                                    Expr.Goto(target, Expr.Constant("Go")),
                                    EU.ConcatEquals(Result, Expr.Constant("4"))
                                )
                            ),
                            Expr.Catch(
                                typeof(ArrayTypeMismatchException),
                                Expr.Block(
                                    EU.ConcatEquals(Result, Expr.Constant("CaughtAgain")),
                                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("OtherDefault"))),
                                    EU.ConcatEquals(Result, Expr.Constant("5"))
                                )
                            )
                        }
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("6"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12CaughtCaughtAgain56"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto catch from inside try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 26", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("Caught")),
                                Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                EU.ConcatEquals(Result, Expr.Constant("4"))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("5"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12Caught45"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto finally from inside try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 27", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatchFinally(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                            EU.ConcatEquals(Result, Expr.Constant("5"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("6"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1256"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto outside try block from inside finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 28", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto28(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(void));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatchFinally(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2"))
                        ),
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            EU.Goto(Result2, target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("4"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("5")),
                    EU.Label(Result2, target, Expr.Constant("Default")),
                    EU.ConcatEquals(Result, Expr.Constant("6"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1236"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto outside try block from inside finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 28_1", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto28_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label();

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    AstUtils.FinallyFlowControl(
                        Expr.TryCatchFinally(
                            Expr.Block(
                                EU.BlockVoid(EU.ConcatEquals(Result, Expr.Constant("2")))
                            ),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("3")),
                                EU.Goto(Result2, target, Expr.Constant("Go")),
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Empty()
                            ),
                            Expr.Catch(
                                typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, Expr.Constant("Caught")))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("5")),
                    EU.Label(Result2, target, Expr.Constant("Default")),
                    EU.ConcatEquals(Result, Expr.Constant("6"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1236"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto filter from inside try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 29", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");
            ParameterExpression Ex = Expr.Variable(typeof(DivideByZeroException), "Ex");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(
                            Ex,
                            Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Caught"))),
                            Expr.Block( // filter
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                EU.ConcatEquals(Result, Expr.Constant("5")),
                                Expr.Constant(true)
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("6"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("125Caught6"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }


        // Goto inside loop with valid local variables set
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 30", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Index = Expr.Variable(typeof(int), "Index");

            LabelTarget breakLabel = Expr.Label(typeof(void), "breakLabel");
            LabelTarget myLabel = Expr.Label(typeof(void), "myLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Begin")),
                    Expr.Assign(Index, Expr.Constant(1)),
                    Expr.Goto(myLabel),
                    Expr.Assign(Index, Expr.Constant(2)),
                    Expr.Loop(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("L")),
                            Expr.Label(myLabel),
                            EU.ConcatEquals(Result, Expr.Constant("1")),
                            Expr.Condition( // if true, loop ends
                                Expr.GreaterThanOrEqual(Index, Expr.Constant(3)),
                                Expr.Goto(breakLabel),
                                Expr.Empty()
                            ),
                            Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1)))
                        ),
                        breakLabel,
                        null
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("End"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Begin1L1L1End"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto out of loop
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 31", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Index = Expr.Variable(typeof(int), "Index");

            LabelTarget myLabel = Expr.Label(typeof(void), "myLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Begin")),
                    Expr.Assign(Index, Expr.Constant(0)),
                    Expr.Loop(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("L")),
                            Expr.Condition( // if true, loop ends
                                Expr.GreaterThanOrEqual(Index, Expr.Constant(3)),
                                Expr.Goto(myLabel),
                                Expr.Empty()
                            ),
                            Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1)))
                        ),
                        null,
                        null
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Label(myLabel),
                    EU.ConcatEquals(Result, Expr.Constant("End"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeginLLLLEnd"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a Goto to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 32", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto32(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            ParameterExpression GotoResult = Expr.Variable(typeof(string), "GotoResult");
            ParameterExpression GotoResult2 = Expr.Variable(typeof(string), "GotoResult2");

            Expressions.Add(Expr.Assign(GotoResult, Expr.Constant("None")));
            Expressions.Add(Expr.Assign(GotoResult2, Expr.Constant("None")));

            LabelTarget target = Expr.Label(typeof(void), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    EU.Goto(GotoResult, target, Expr.Block(EU.Goto(GotoResult2, target2, Expr.Constant("GotoAgain")), Expr.Constant("Goto"))),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    EU.Label(GotoResult, target, Expr.Constant("Default")),
                    EU.ConcatEquals(Result, Expr.Constant("3")),
                    EU.Label(GotoResult2, target2, Expr.Constant("Default2")),
                    EU.ConcatEquals(Result, Expr.Constant("4"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("14"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("None"), GotoResult, "Goto 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("GotoAgain"), GotoResult2, "Goto 3"));

            var tree = Expr.Block(new[] { Result, GotoResult, GotoResult2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a block to Expression which throws
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 33", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto33(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            Expressions.Add(Expr.Assign(Result2, Expr.Constant("None")));

            LabelTarget target = Expr.Label(typeof(void), "MyLabel");

            Expr test =
                Expr.Block(
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("1")),
                            EU.Goto(Result2, target, Expr.Block(Expr.Throw(Expr.Constant(new DivideByZeroException())), Expr.Constant("Goto"))),
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            EU.Label(Result2, target, Expr.Constant("Default")),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("4"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1Caught4"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("None"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use a label of a generic type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 34", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(List<int>), "Result2");

            LabelTarget target = Expr.Label(typeof(void), "MyLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    EU.Goto(Result2, target, Expr.Constant(new List<int> { 1, 2, 3 })),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    EU.Label(Result2, target, Expr.Constant(new List<int> { 0 })),
                    EU.ConcatEquals(Result, Expr.Constant("3"))
                );

            PropertyInfo pi = typeof(List<int>).GetProperty("Count");

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("13"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Property(Result2, pi), "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto within a try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 35", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(void));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            EU.Goto(Result2, target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            EU.Label(Result2, target, Expr.Constant("Default")),
                            EU.ConcatEquals(Result, Expr.Constant("4"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("5"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1245"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto within a catch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 36", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(void));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                EU.Goto(Result2, target, Expr.Constant("Go")),
                                EU.ConcatEquals(Result, Expr.Constant("5")),
                                EU.Label(Result2, target, Expr.Constant("Default")),
                                EU.ConcatEquals(Result, Expr.Constant("6"))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("7"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12467"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto within a finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 37", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(void));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatchFinally(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //finally
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            EU.Goto(Result2, target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("5")),
                            EU.Label(Result2, target, Expr.Constant("Default")),
                            EU.ConcatEquals(Result, Expr.Constant("6"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("7"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12Caught467"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto within a fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 38", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryFault(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //fault
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("5")),
                            Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                            EU.ConcatEquals(Result, Expr.Constant("6"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("7"))
                );

            Expressions.Add(Expr.TryCatch(test, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1246Caught"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // Goto a fault from a try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 39", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryFault(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Goto(target, Expr.Constant("Goto")),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //fault
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                EU.ConcatEquals(Result, Expr.Constant("5"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("6"))
                );

            Expressions.Add(Expr.TryCatch(test, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1246Caught"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // Goto a fault from a outside try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 40", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant("Goto")),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.TryFault(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //fault
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                            EU.ConcatEquals(Result, Expr.Constant("5"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("6"))
                );

            Expressions.Add(Expr.TryCatch(test, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("156"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // Goto a try from a fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 41", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryFault(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //fault
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("5"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("6"))
                );

            Expressions.Add(Expr.TryCatch(test, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12436"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // From inside a try, goto a nested try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 42", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            Expr.TryCatch(
                                Expr.Block(
                                    EU.ConcatEquals(Result, Expr.Constant("4")),
                                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                    EU.ConcatEquals(Result, Expr.Constant("5"))
                                ),
                                Expr.Catch(typeof(ArrayTypeMismatchException), EU.ConcatEquals(Result, Expr.Constant("CaughtNested")))
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("6"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("7"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12567"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From inside a try, goto a nested catch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 43", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            Expr.TryCatch(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Catch(
                                    typeof(ArrayTypeMismatchException),
                                    Expr.Block(
                                        EU.ConcatEquals(Result, Expr.Constant("5")),
                                        Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                        EU.ConcatEquals(Result, Expr.Constant("6"))
                                    )
                                )
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("7"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("8"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1678"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From inside a try, goto a nested scope
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 44", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label();

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            EU.Goto(Result2, target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                EU.Label(Result2, target, Expr.Constant("Default")),
                                EU.ConcatEquals(Result, Expr.Constant("5"))
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("6"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("7"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12567"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From inside a catch, goto a nested try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 45", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Goto(target, Expr.Constant("Goto")),
                                EU.ConcatEquals(Result, Expr.Constant("5")),
                                Expr.TryCatch(
                                    Expr.Block(
                                        EU.ConcatEquals(Result, Expr.Constant("6")),
                                        Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                        EU.ConcatEquals(Result, Expr.Constant("7"))
                                    ),
                                    Expr.Catch(typeof(ArrayTypeMismatchException), EU.ConcatEquals(Result, Expr.Constant("NestedCatch")))
                                ),
                                EU.ConcatEquals(Result, Expr.Constant("8"))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("9"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("124789"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From inside a catch, goto a nested catch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 46", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Goto(target, Expr.Constant("Goto")),
                                EU.ConcatEquals(Result, Expr.Constant("5")),
                                Expr.TryCatch(
                                    EU.ConcatEquals(Result, Expr.Constant("6")),
                                    Expr.Catch(
                                        typeof(ArrayTypeMismatchException),
                                        Expr.Block(
                                            EU.ConcatEquals(Result, Expr.Constant("7")),
                                            Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                            EU.ConcatEquals(Result, Expr.Constant("8"))
                                        )
                                    )
                                ),
                                EU.ConcatEquals(Result, Expr.Constant("9"))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("10"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1248910"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From inside a catch, goto a nested scope
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 47", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label();

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                EU.Goto(Result2, target, Expr.Constant("Go")),
                                EU.ConcatEquals(Result, Expr.Constant("5")),
                                Expr.Block(
                                    EU.ConcatEquals(Result, Expr.Constant("6")),
                                    EU.Label(Result2, target, Expr.Constant("Default")),
                                    EU.ConcatEquals(Result, Expr.Constant("7"))
                                ),
                                EU.ConcatEquals(Result, Expr.Constant("8"))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("9"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("124789"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From inside a finally, goto a nested finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 48", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatchFinally(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //finally
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("5")),
                            Expr.TryCatchFinally(
                                EU.ConcatEquals(Result, Expr.Constant("6")),
                                Expr.Block( // nested finally
                                    EU.ConcatEquals(Result, Expr.Constant("7")),
                                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                    EU.ConcatEquals(Result, Expr.Constant("8"))
                                ),
                                Expr.Catch(typeof(ArrayTypeMismatchException), EU.ConcatEquals(Result, Expr.Constant("NestedCatch")))
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("9"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("10"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12Caught48910"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From inside a finally, goto a nested scope
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 49", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto49(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label();

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatchFinally(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //finally
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                EU.Goto(Result2, target, Expr.Constant("Go")),
                                EU.ConcatEquals(Result, Expr.Constant("5")),
                                Expr.Block( // nested scope
                                    EU.ConcatEquals(Result, Expr.Constant("6")),
                                    EU.Label(Result2, target, Expr.Constant("Default")),
                                    EU.ConcatEquals(Result, Expr.Constant("7"))
                                ),
                                EU.ConcatEquals(Result, Expr.Constant("8"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("9"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12Caught4789"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From inside a fault, goto a nested try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 50", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto50(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryFault(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //fault
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("5")),
                            Expr.TryFault(
                                Expr.Block( // nested try
                                    EU.ConcatEquals(Result, Expr.Constant("6")),
                                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                    EU.ConcatEquals(Result, Expr.Constant("7"))
                                ),
                                EU.ConcatEquals(Result, Expr.Constant("NestedFault"))
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("8"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("9"))
                );

            Expressions.Add(Expr.TryCatch(test, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1247NestedFault89Caught"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // From inside a fault, goto a nested fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 51", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto51(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryFault(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //fault
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("5")),
                            Expr.TryFault(
                                EU.ConcatEquals(Result, Expr.Constant("6")),
                                Expr.Block( // nested finally
                                    EU.ConcatEquals(Result, Expr.Constant("7")),
                                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                    EU.ConcatEquals(Result, Expr.Constant("8"))
                                )
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("9"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("10"))
                );

            Expressions.Add(Expr.TryCatch(test, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1248910Caught"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // From inside a fault, goto a nested scope
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 52", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto52(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryFault(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //fault
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Goto(target, Expr.Constant("Go")),
                                EU.ConcatEquals(Result, Expr.Constant("5")),
                                Expr.Block( // nested scope
                                    EU.ConcatEquals(Result, Expr.Constant("6")),
                                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                    EU.ConcatEquals(Result, Expr.Constant("7"))
                                ),
                                EU.ConcatEquals(Result, Expr.Constant("8"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("9"))
                );

            Expressions.Add(Expr.TryCatch(test, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12478Caught"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // Goto into a lambda
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 53", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto53(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string), "MyLabel");

            var lambda =
                Expr.Lambda(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("InLambda")),
                        Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                        EU.ConcatEquals(Result, Expr.Constant("After"))
                    ),
                    new ParameterExpression[] { Result2 }
                );

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant("Go")),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Invoke(lambda, new Expression[] { Result2 }),
                    EU.ConcatEquals(Result, Expr.Constant("4"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1InLambdaAfter4"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto into a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 54", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto54(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(string), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("InGen")),
                        Expr.Assign(Result2, Expr.Label(target2, Expr.Constant("Default"))),
                        AstUtils.YieldReturn(target, Expr.Constant("G")),
                        EU.ConcatEquals(Result, Expr.Constant("After")),
                        Expr.Empty()
                    ),
                    new ParameterExpression[] { }
                );

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target2, Expr.Constant("Go")),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Invoke(Gen, new Expression[] { }),
                    EU.ConcatEquals(Result, Expr.Constant("4"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1InGenAfter4"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From within a generator, goto a try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 55", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto55(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.Goto(target2, Expr.Constant(10)),
                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(4))),
                                AstUtils.YieldReturn(target, Expr.Constant(5))
                            ),
                            Expr.Catch(typeof(DivideByZeroException), Expr.Empty())
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(6))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // From within a generator, goto a catch (which contains a yield)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 56", new string[] { "positive", "Goto", "miscellaneous", "Pri1" })]
        public static Expr Goto56(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");
            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        EU.Goto(target2Value, target2, Expr.Constant(10)),
                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                AstUtils.YieldReturn(target, Expr.Constant(4))
                            ),
                            Expr.Catch(
                                typeof(DivideByZeroException),
                                Expr.Block(
                                    AstUtils.YieldReturn(target, Expr.Constant(5)),
                                    AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(4))),
                                    AstUtils.YieldReturn(target, Expr.Constant(6))
                                )
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(7))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 10, 6, 7 }, 4, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From within a generator, goto a finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 57", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto57(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.Goto(target2, Expr.Constant(10)),
                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                        Expr.TryCatchFinally(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                AstUtils.YieldReturn(target, Expr.Constant(4))
                            ),
                            Expr.Block( //finally
                                    AstUtils.YieldReturn(target, Expr.Constant(6)),
                                    AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(4))),
                                    AstUtils.YieldReturn(target, Expr.Constant(7))
                            ),
                            Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(5)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(8))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // From within a generator, goto a fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 58", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(NotSupportedException))]
        public static Expr Goto58(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.Goto(target2, Expr.Constant(10)),
                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                        Expr.TryFault(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                AstUtils.YieldReturn(target, Expr.Constant(4))
                            ),
                            Expr.Block( //fault
                                AstUtils.YieldReturn(target, Expr.Constant(6)),
                                AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(4))),
                                AstUtils.YieldReturn(target, Expr.Constant(7))
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(8))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<NotSupportedException>(Gen, true);
            
            return Gen;
        }

        // Regression bug 541543
        // From within a generator, goto out of a try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 59", new string[] { "positive", "Goto", "miscellaneous", "Pri1" })]
        public static Expr Goto59(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                EU.Goto(target2Value, target2, Expr.Constant(10)),
                                AstUtils.YieldReturn(target, Expr.Constant(3))
                            ),
                            Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(4)))
                        ),
                        AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(5))),
                        AstUtils.YieldReturn(target, Expr.Constant(6))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 10, 6 }, 4, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From within a generator, goto out of a catch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 60", new string[] { "positive", "Goto", "miscellaneous", "Pri1" })]
        public static Expr Goto60(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                AstUtils.YieldReturn(target, Expr.Constant(3))
                            ),
                            Expr.Catch(
                                typeof(DivideByZeroException),
                                Expr.Block(
                                    AstUtils.YieldReturn(target, Expr.Constant(4)),
                                    EU.Goto(target2Value, target2, Expr.Constant(10)),
                                    AstUtils.YieldReturn(target, Expr.Constant(5))
                                )
                            )
                        ),
                        AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(6))),
                        AstUtils.YieldReturn(target, Expr.Constant(7))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 4, 10, 7 }, 5, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From within a generator, goto out of a finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 61", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto61(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatchFinally(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                AstUtils.YieldReturn(target, Expr.Constant(3))
                            ),
                            Expr.Block( // finally
                                AstUtils.YieldReturn(target, Expr.Constant(5)),
                                Expr.Goto(target2, Expr.Constant(10)),
                                AstUtils.YieldReturn(target, Expr.Constant(6))
                            ),
                            Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(4)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(7))),
                        AstUtils.YieldReturn(target, Expr.Constant(8))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // From within a generator, goto out of a fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 62", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(NotSupportedException))]
        public static Expr Goto62(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryFault(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                AstUtils.YieldReturn(target, Expr.Constant(3))
                            ),
                            Expr.Block( // fault
                                    AstUtils.YieldReturn(target, Expr.Constant(4)),
                                    Expr.Goto(target2, Expr.Constant(10)),
                                    AstUtils.YieldReturn(target, Expr.Constant(5))
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(6))),
                        AstUtils.YieldReturn(target, Expr.Constant(7))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<NotSupportedException>(Gen, true);

            return Gen;
        }

        // From within a generator, goto a try from a catch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 63", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto63(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(3))),
                                AstUtils.YieldReturn(target, Expr.Constant(4))
                            ),
                            Expr.Catch(
                                typeof(DivideByZeroException),
                                Expr.Block(
                                    AstUtils.YieldReturn(target, Expr.Constant(5)),
                                    Expr.Goto(target2, Expr.Constant(10)),
                                    AstUtils.YieldReturn(target, Expr.Constant(6))
                                )
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(7))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // Regression bug 541561
        // From within a generator, goto into a catch (which contains a yield) from a try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 64", new string[] { "positive", "Goto", "miscellaneous", "Pri1" })]
        public static Expr Goto64(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                EU.Goto(target2Value, target2, Expr.Constant(10)),
                                AstUtils.YieldReturn(target, Expr.Constant(3))
                            ),
                            Expr.Catch(
                                typeof(DivideByZeroException),
                                Expr.Block(
                                    AstUtils.YieldReturn(target, Expr.Constant(2)),
                                    AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(4))),
                                    AstUtils.YieldReturn(target, Expr.Constant(5))
                                )
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(6))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 10, 5, 6 }, 5, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From within a generator, goto into a catch (which doesn't contain a yield) from a try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 64_1", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto64_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Goto(target2, Expr.Constant(10)),
                                AstUtils.YieldReturn(target, Expr.Constant(3))
                            ),
                            Expr.Catch(
                                typeof(DivideByZeroException),
                                Expr.Block(
                                    Expr.Label(target2, Expr.Constant(4)),
                                    Expr.Empty()
                                )
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(6))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // Bug #541581
        // From within a generator, goto a catch from a catch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 65", new string[] { "positive", "Goto", "miscellaneous", "Pri1" })]
        public static Expr Goto65(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                AstUtils.YieldReturn(target, Expr.Constant(3))
                            ),
                            Expr.Catch(
                                typeof(DivideByZeroException),
                                Expr.Block(
                                    AstUtils.YieldReturn(target, Expr.Constant(4)),
                                    EU.Goto(target2Value, target2, Expr.Constant(10)),
                                    AstUtils.YieldReturn(target, Expr.Constant(5))
                                )
                            ),
                            Expr.Catch(
                                typeof(OverflowException),
                                Expr.Block(
                                    AstUtils.YieldReturn(target, Expr.Constant(6)),
                                    AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(0))),
                                    AstUtils.YieldReturn(target, Expr.Constant(7))
                                )
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(8))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 4, 10, 7, 8 }, 6, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From within a generator, goto a catch from a finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 66", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto66(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatchFinally(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2))
                            ),
                            Expr.Block( //finally
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                Expr.Goto(target2, Expr.Constant(10)),
                                AstUtils.YieldReturn(target, Expr.Constant(4))
                            ),
                            Expr.Catch(
                                typeof(OverflowException),
                                Expr.Block(
                                    AstUtils.YieldReturn(target, Expr.Constant(5)),
                                    AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(0))),
                                    AstUtils.YieldReturn(target, Expr.Constant(6))
                                )
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(7))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // Goto passing a yield to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 67", new string[] { "positive", "goto", "miscellaneous", "Pri2" })]
        public static Expr Goto67(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.Goto(target2, AstUtils.YieldReturn(target, Expr.Constant(10))),
                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                        Expr.Label(target2),
                        AstUtils.YieldReturn(target, Expr.Constant(4))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 10, 4 }, 3, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto outside try block from inside try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 68", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto68(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    Expr.Assign(
                        Result2,
                        Expr.Label(
                            target,
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("1")),
                                Expr.TryCatch(
                                    Expr.Block(
                                        EU.ConcatEquals(Result, Expr.Constant("2")),
                                        Expr.Goto(target, Expr.Constant("Go")),
                                        EU.ConcatEquals(Result, Expr.Constant("3"))
                                    ),
                                    Expr.Catch(
                                        typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught"))
                                    )
                                ),
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Constant("Default")
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("5"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("125"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // In a try nested inside a loop, break/continue
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 69", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto69(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Index = Expr.Variable(typeof(int), "Index");

            LabelTarget breakLabel = Expr.Label(typeof(void), "breakLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Begin")),
                    Expr.Assign(Index, Expr.Constant(0)),
                    Expr.Loop(
                        Expr.TryCatch(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("L")),
                                Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                                Expr.Condition( // if true, break out of try back to loop
                                    Expr.GreaterThanOrEqual(Index, Expr.Constant(3)),
                                    Expr.Block(Expr.Break(breakLabel), Expr.Throw(Expr.Constant(new DivideByZeroException()))),
                                    Expr.Empty()
                                )
                            ),
                            Expr.Catch(typeof(DivideByZeroException), Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Caught")), Expr.Break(breakLabel)))
                        ),
                        breakLabel,
                        null
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("End"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeginLLLEnd"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // In a catch nested inside a loop, break/continue
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 70", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto70(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Index = Expr.Variable(typeof(int), "Index");

            LabelTarget breakLabel = Expr.Label(typeof(void), "breakLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Begin")),
                    Expr.Assign(Index, Expr.Constant(0)),
                    Expr.Loop(
                        Expr.TryCatch(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("L")),
                                Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                                Expr.Condition( // if true, throw
                                    Expr.GreaterThanOrEqual(Index, Expr.Constant(3)),
                                    Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                    Expr.Empty()
                                )
                            ),
                            Expr.Catch(
                                typeof(DivideByZeroException),
                                Expr.Block(
                                    EU.ConcatEquals(Result, Expr.Constant("Caught")),
                                    Expr.Break(breakLabel),
                                    EU.ConcatEquals(Result, Expr.Constant("Bad")),
                                    Expr.Empty()
                                )
                            )
                        ),
                        breakLabel,
                        null
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("End"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeginLLLCaughtEnd"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // In a finally nested inside a loop, break/continue
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 71", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto71(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Index = Expr.Variable(typeof(int), "Index");

            LabelTarget breakLabel = Expr.Label(typeof(void), "breakLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Begin")),
                    Expr.Assign(Index, Expr.Constant(0)),
                    Expr.Loop(
                        Expr.TryCatchFinally(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("L")),
                                Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                                Expr.Condition( // if true, throw
                                    Expr.GreaterThanOrEqual(Index, Expr.Constant(3)),
                                    Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                    Expr.Empty()
                                )
                            ),
                            Expr.Block( //finally
                                EU.ConcatEquals(Result, Expr.Constant("Finally")),
                                Expr.Break(breakLabel),
                                EU.ConcatEquals(Result, Expr.Constant("Bad"))
                            ),
                            Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, Expr.Constant("Caught"))))
                        ),
                        breakLabel,
                        null
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("End"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeginLLLCaughtFinallyEnd"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result, Index }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto out of a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 72", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto72(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");
            ParameterExpression GotoResult = Expr.Variable(typeof(string), "GotoResult");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(string), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.Goto(target2, Expr.Constant("Go")),
                        AstUtils.YieldReturn(target, Expr.Constant(2))
                    ),
                    new ParameterExpression[] { }
                );

            Expr test =
                Expr.Block(
                    Gen,
                    Expr.Assign(GotoResult, Expr.Label(target2, Expr.Constant("Default")))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), GotoResult, "Goto 1"));

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // Goto a label that doesn't exist
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 73", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto73(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(int));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant(1)),
                    EU.ConcatEquals(Result, Expr.Constant("2"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto back to method caller
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 74", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto74(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string), "MyLabel");

            LambdaExpression lambda =
                Expr.Lambda(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("BeforeGo")),
                        Expr.Goto(target, Expr.Constant("Go")),
                        EU.ConcatEquals(Result, Expr.Constant("AfterGo"))
                    ),
                    new ParameterExpression[] { }
                );

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Invoke(lambda, new Expression[] { }),
                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                    EU.ConcatEquals(Result, Expr.Constant("2"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1BeforeGo2"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }


        // Goto into an expression (ex. Addition)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 75", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto75(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression AddResult = Expr.Variable(typeof(int), "AddResult");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant(10)),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Assign(AddResult, Expr.Add(Expr.Constant(2), Expr.Label(target, Expr.Constant(2)))),
                    EU.ConcatEquals(Result, Expr.Constant("3"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("13"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(12), AddResult, "Goto 2"));

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "DoNotVisitTemp");
            var tree = Expr.Block(new[] { Result, AddResult, DoNotVisitTemp }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto into nested scope which uses outer scope variables
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 76", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto76(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(int), "Result2");
            ParameterExpression Result3 = Expr.Variable(typeof(object), "Result3");

            LabelTarget target = Expr.Label(typeof(void), "MyLabel");

            Expressions.Add(Expr.Assign(Result, Expr.Constant("TestValue")));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Outer")),
                    Expr.Goto(target),
                    Expr.Assign(Result2, Expr.Constant(10)),
                    Expr.Block(
                        Expr.Label(target),
                        EU.ConcatEquals(Result, Expr.Constant("Inner")),
                        Expr.Assign(Result2, Expr.Add(Result2, Expr.Constant(2)))
                    )
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestValueOuterInner"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Result2, "Goto 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(null), Result3, "Goto 3"));

            var tree = Expr.Block(new[] { Result, Result2, Result3 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto into nested scope with variables which shadow outer scope variables
        // Goto outer scope whose variables were shadowed by inner scope variables
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 77", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto77(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(int), "Result2");
            ParameterExpression Result3 = Expr.Variable(typeof(object), "Result3");
            ParameterExpression Result4 = Expr.Variable(typeof(string), "Result4");

            LabelTarget target = Expr.Label(typeof(void), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");

            Expressions.Add(Expr.Assign(Result, Expr.Constant("TestValue")));

            MethodInfo mi = typeof(Int32).GetMethod("ToString", new Type[] { });
            MethodInfo mi2 = typeof(Object).GetMethod("ToString", new Type[] { });

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Outer")),
                    Expr.Assign(Result2, Expr.Constant(2)),
                    Expr.Goto(target),
                    Expr.Assign(Result2, Expr.Constant(10)),
                    Expr.Block(
                        new[] { Result2, Result3 }, // shadowed variables
                        Expr.Label(target),
                        EU.ConcatEquals(Result, Expr.Constant("Inner")),
                        Expr.Assign(Result2, Expr.Add(Result2, Expr.Constant(2))),
                        Expr.Assign(Result3, Expr.Convert(Expr.Constant(4.1), typeof(object))),
                        Expr.Assign(Result4, Expr.Call(Result2, mi)), // checking the nested changes did affect the shadowing variables
                        EU.ConcatEquals(Result4, Expr.Call(Result3, mi2)),
                        Expr.Goto(target2),
                        Expr.Assign(Result2, Expr.Add(Result2, Expr.Constant(2)))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("Bad")),
                    Expr.Label(target2),
                    Expr.Assign(Result2, Expr.Add(Result2, Expr.Constant(5))),
                    EU.ConcatEquals(Result, Expr.Constant("End"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestValueOuterInnerEnd"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(7), Result2, "Goto 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(null), Result3, "Goto 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("2" + (4.1).ToString()), Result4, "Goto 4"));

            var tree = Expr.Block(new[] { Result, Result2, Result3, Result4 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto into assignment expression where rhs is a value returning block 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 78", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto78(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");
            ParameterExpression Result3 = Expr.Variable(typeof(int), "Result3");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Begin")),
                    Expr.Goto(target, Expr.Constant("Go")),
                    EU.ConcatEquals(Result, Expr.Constant("Bad")),
                    Expr.Assign(
                        Result2,
                        Expr.Block( // value returning block
                            EU.ConcatEquals(Result, Expr.Constant("Inner")),
                            Expr.Assign(Result3, Expr.Constant(10)),
                            Expr.Label(target, Expr.Constant("Default"))
                        )
                    ),
                    Expr.Assign(Result3, Expr.Add(Result3, Expr.Constant(1))),
                    EU.ConcatEquals(Result, Expr.Constant("End"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeginEnd"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Result3, "Goto 3"));

            var tree = Expr.Block(new[] { Result, Result2, Result3 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From within a generator, Goto outside try block from inside finally (using Utils.FinallyFlowControl)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 79", new string[] { "positive", "Goto", "miscellaneous", "Pri1" })]
        public static Expr Goto79(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        AstUtils.FinallyFlowControl(
                            Expr.TryCatchFinally(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Block(
                                    AstUtils.YieldReturn(target, Expr.Constant(3)),
                                    Expr.Goto(target2),
                                    AstUtils.YieldReturn(target, Expr.Constant(4)),
                                    Expr.Empty()
                                ),
                                Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(5)))
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(6)),
                        Expr.Label(target2),
                        AstUtils.YieldReturn(target, Expr.Constant(7))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3, 7 }, 4, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From within a generator, Goto outside try block from inside finally 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 80", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto80(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatchFinally(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2))
                            ),
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                Expr.Goto(target2),
                                AstUtils.YieldReturn(target, Expr.Constant(4)),
                                Expr.Empty()
                            ),
                            Expr.Catch(
                                typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(5))
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(6)),
                        Expr.Label(target2),
                        AstUtils.YieldReturn(target, Expr.Constant(7))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // From within a generator, goto a nested try from inside a try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 81", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto81(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Goto(target2, Expr.Constant(10)),
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                Expr.TryCatch(
                                    Expr.Block(
                                        AstUtils.YieldReturn(target, Expr.Constant(4)),
                                        AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(5))),
                                        AstUtils.YieldReturn(target, Expr.Constant(6)),
                                        Expr.Throw(Expr.Constant(new DivideByZeroException()))
                                    ),
                                    Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(7)))
                                ),
                                AstUtils.YieldReturn(target, Expr.Constant(8)),
                                Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                            ),
                            Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(9)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(10))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // From within a generator, goto a nested catch (which contains a yield) from inside a try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 82", new string[] { "positive", "Goto", "miscellaneous", "Pri1" })]
        public static Expr Goto82(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                EU.Goto(target2Value, target2, Expr.Constant(20)),
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                Expr.TryCatch(
                                    Expr.Block(
                                        AstUtils.YieldReturn(target, Expr.Constant(4))
                                    ),
                                    Expr.Catch(
                                        typeof(DivideByZeroException),
                                        Expr.Block(
                                            AstUtils.YieldReturn(target, Expr.Constant(5)),
                                            AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(6))),
                                            AstUtils.YieldReturn(target, Expr.Constant(7))
                                        )
                                    )
                                ),
                                AstUtils.YieldReturn(target, Expr.Constant(8)),
                                Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                            ),
                            Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(9)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(10))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 20, 7, 8, 9, 10 }, 7, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From within a generator, goto a nested catch (which doesn't contain a yield) from inside a try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 83", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto83(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Goto(target2, Expr.Constant(10)),
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                Expr.TryCatch(
                                    AstUtils.YieldReturn(target, Expr.Constant(4)),
                                    Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(Expr.Label(target2, Expr.Constant(5))))
                                ),
                                AstUtils.YieldReturn(target, Expr.Constant(6)),
                                Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                            ),
                            Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(7)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(8))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // From within a generator, goto a nested finally from inside a try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 84", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto84(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Goto(target2, Expr.Constant(20)),
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                Expr.TryCatchFinally(
                                    AstUtils.YieldReturn(target, Expr.Constant(4)),
                                    Expr.Block( // nested finally
                                        AstUtils.YieldReturn(target, Expr.Constant(5)),
                                        AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(6))),
                                        AstUtils.YieldReturn(target, Expr.Constant(7))
                                    ),
                                    Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(8)))
                                ),
                                AstUtils.YieldReturn(target, Expr.Constant(9)),
                                Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                            ),
                            Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(10)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(11))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // From within a generator, goto a nested finally from inside a try using Utils.FinallyFlowControl
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 85", new string[] { "negative", "Goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto85(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                Expr.Goto(target2, Expr.Constant(20)),
                                AstUtils.YieldReturn(target, Expr.Constant(3)),
                                AstUtils.FinallyFlowControl(
                                    Expr.TryCatchFinally(
                                        AstUtils.YieldReturn(target, Expr.Constant(4)),
                                        Expr.Block( // nested finally
                                            AstUtils.YieldReturn(target, Expr.Constant(5)),
                                            AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(6))),
                                            AstUtils.YieldReturn(target, Expr.Constant(7))
                                        ),
                                        Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(8)))
                                    )
                                ),
                                AstUtils.YieldReturn(target, Expr.Constant(9)),
                                Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                            ),
                            Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(10)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(11))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // Using Utils.FinallyFlowControl from within a generator, jump from a finally nested in a try to inside the outer try body (which is in a FinallyControlFlow block)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 86", new string[] { "positive", "Goto", "miscellaneous", "Pri1" })]
        public static Expr Goto86(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                AstUtils.FinallyFlowControl(
                                    Expr.Block(
                                        Expr.TryCatchFinally(
                                            AstUtils.YieldReturn(target, Expr.Constant(3)),
                                            Expr.Block( // nested finally
                                                AstUtils.YieldReturn(target, Expr.Constant(4)),
                                                EU.Goto(target2Value, target2, Expr.Constant(20)),
                                                AstUtils.YieldReturn(target, Expr.Constant(5))),
                                            Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(6)))
                                        ),
                                        AstUtils.YieldReturn(target, Expr.Constant(7)),
                                        AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(8))), // outside nested try, inside outer try
                                        AstUtils.YieldReturn(target, Expr.Constant(9))
                                    )
                                ),
                                AstUtils.YieldReturn(target, Expr.Constant(10)),
                                Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                            ),
                            Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(11)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(12))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3, 4, 20, 9, 10, 11, 12 }, 9, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Using Utils.FinallyFlowControl from within a generator, jump from a finally nested in a try to outside both try blocks (which is still in a FinallyControlFlow block)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 87", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto87(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        AstUtils.FinallyFlowControl(
                            Expr.Block(
                                Expr.TryCatchFinally(
                                    Expr.Block(
                                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                                        Expr.TryCatchFinally(
                                            AstUtils.YieldReturn(target, Expr.Constant(3)),
                                            Expr.Block( // nested finally
                                                AstUtils.YieldReturn(target, Expr.Constant(4)),
                                                EU.Goto(target2Value, target2, Expr.Constant(20)),
                                                AstUtils.YieldReturn(target, Expr.Constant(5))
                                            ),
                                            Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(6)))
                                        ),
                                        AstUtils.YieldReturn(target, Expr.Constant(7)),
                                        Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                                    ),
                                    Expr.Block( // outer finally
                                        AstUtils.YieldReturn(target, Expr.Constant(8))
                                    ),
                                    Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(9)))
                                ),
                                AstUtils.YieldReturn(target, Expr.Constant(10)),
                                AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(11))), // outside both trys
                                AstUtils.YieldReturn(target, Expr.Constant(12))
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(13))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3, 4, 20, 12, 13 }, 7, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From within a generator, goto out of a finally nested in a try using Utils.FinallyFlowControl (goto outside FinallyFlowControl block)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 88", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto88(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatchFinally(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                AstUtils.FinallyFlowControl(
                                    Expr.TryCatchFinally(
                                        AstUtils.YieldReturn(target, Expr.Constant(3)),
                                        Expr.Block( // nested finally
                                            AstUtils.YieldReturn(target, Expr.Constant(4)),
                                            EU.Goto(target2Value, target2, Expr.Constant(20)),
                                            AstUtils.YieldReturn(target, Expr.Constant(5))
                                        ),
                                        Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(6)))
                                    )
                                ),
                                AstUtils.YieldReturn(target, Expr.Constant(7)),
                                Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                            ),
                            Expr.Block( // outer finally
                                AstUtils.YieldReturn(target, Expr.Constant(8))
                            ),
                            Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(9)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(10)),
                        AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(11))), // outside both trys
                        AstUtils.YieldReturn(target, Expr.Constant(12))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3, 4, 20, 12 }, 6, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto out of a finally nested in a try using Utils.FinallyFlowControl (goto outside FinallyFlowControl block)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 89", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto89(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(string), "MyLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatchFinally(
                        EU.BlockVoid(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            AstUtils.FinallyFlowControl( // so it's legal to goto out of nested finally
                                Expr.TryFinally(
                                    EU.BlockVoid(EU.ConcatEquals(Result, Expr.Constant("3"))),
                                    Expr.Block( // nested finally
                                        EU.ConcatEquals(Result, Expr.Constant("4")),
                                        Expr.Goto(target, Expr.Constant("20")),
                                        EU.ConcatEquals(Result, Expr.Constant("5")),
                                        Expr.Empty()
                                    )
                                )
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("6")),
                            Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                        ),
                        Expr.Block( // outer finally, gets skipped because of goto if inside a generator
                            EU.ConcatEquals(Result, Expr.Constant("7"))
                        ),
                        Expr.Catch(typeof(IndexOutOfRangeException), EU.BlockVoid(EU.ConcatEquals(Result, Expr.Constant("8"))))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("9")),
                    Expr.Label(target, Expr.Constant("10")), // outside both try blocks
                    EU.ConcatEquals(Result, Expr.Constant("11"))
              );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1234711"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Using Utils.FinallyFlowControl from within a generator, jump from a finally nested in a try to a CatchBlock associated with the same try
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 90", new string[] { "negative", "goto", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto90(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatchFinally(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                AstUtils.FinallyFlowControl(
                                    Expr.TryCatchFinally(
                                        AstUtils.YieldReturn(target, Expr.Constant(3)),
                                        Expr.Block( // nested finally
                                            AstUtils.YieldReturn(target, Expr.Constant(4)),
                                            Expr.Goto(target2, Expr.Constant(20)),
                                            AstUtils.YieldReturn(target, Expr.Constant(5))
                                        ),
                                        Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(6))))
                                    )
                                ),
                                AstUtils.YieldReturn(target, Expr.Constant(7)),
                                Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                            ),
                            Expr.Block( // outer finally
                                AstUtils.YieldReturn(target, Expr.Constant(8))
                            ),
                            Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(9)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(10)),
                        AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(11))), // outside both trys
                        AstUtils.YieldReturn(target, Expr.Constant(12))
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<InvalidOperationException>(Gen, true);

            return Gen;
        }

        // Goto with nullable values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 91", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto91(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Value = Expr.Variable(typeof(Nullable<int>), "Value");
            LabelTarget target = Expr.Label(typeof(Nullable<int>), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(Nullable<int>), "MySecondLabel");
            LabelTarget target3 = Expr.Label(typeof(Nullable<int>), "MyThirdLabel");

            Expr test =
                Expr.Block(
                    Expr.Assign(
                        Value,
                        Expr.Label(
                            target,
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("1")),
                                Expr.Goto(target, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>))),
                                EU.ConcatEquals(Result, Expr.Constant("2")),
                                Expr.Constant((Nullable<int>)1, typeof(Nullable<int>))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("3"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("13"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(null, typeof(Nullable<int>)), Value, "Goto 2"));
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expr test2 =
                Expr.Block(
                    Expr.Assign(
                        Value,
                        Expr.Label(
                            target2,
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("1")),
                                Expr.Goto(target2, Expr.Constant((Nullable<int>)1, typeof(Nullable<int>))),
                                EU.ConcatEquals(Result, Expr.Constant("2")),
                                Expr.Constant((Nullable<int>)null, typeof(Nullable<int>))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("3"))
                );

            Expressions.Add(test2);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("13"), Result, "Goto 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), Value, "Goto 4"));
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expr test3 =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Assign(Value, Expr.Label(target3, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)))),
                    EU.ConcatEquals(Result, Expr.Constant("2"))
                );

            Expressions.Add(test3);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12"), Result, "Goto 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Value, "Goto 6"));

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // From within a FinallyFlowControl goto out of a finally which doesnâ€™t return void
        // Blocked by Dev10 bug 548666
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Goto 92", new string[] { "positive", "Goto", "miscellaneous", "Pri1" })]
        public static Expr Goto92(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(int), "MyOtherLabel");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(
                                AstUtils.YieldReturn(target, Expr.Constant(2)),
                                AstUtils.FinallyFlowControl(
                                    Expr.Block(
                                        Expr.TryCatchFinally(
                                            AstUtils.YieldReturn(target, Expr.Constant(3)),
                                            Expr.Block( // nested finally
                                                AstUtils.YieldReturn(target, Expr.Constant(4)),
                                                Expr.Goto(target2, Expr.Constant(20)),
                                                AstUtils.YieldReturn(target, Expr.Constant(5)),
                                                Expr.Constant(1)
                                            ),
                                            Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(target, Expr.Constant(6)))
                                        ),
                                        AstUtils.YieldReturn(target, Expr.Constant(7)),
                                        AstUtils.YieldReturn(target, Expr.Label(target2, Expr.Constant(8))), // outside nested try, inside outer try
                                        AstUtils.YieldReturn(target, Expr.Constant(9))
                                    )
                                ),
                                AstUtils.YieldReturn(target, Expr.Constant(10)),
                                Expr.Throw(Expr.Constant(new IndexOutOfRangeException()))
                            ),
                            Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(11)))
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(12))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3, 4, 20, 9, 10, 11, 12 }, 9, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto a valid label of each primitive type passing the correct typed value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 93", new string[] { "positive", "goto", "miscellaneous", "Pri2" })]
        public static Expr Goto93(EU.IValidator V) {
            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            var variables = new List<ParameterExpression> { Result };

            var testValues = new object[] {
                sbyte.MinValue,
                byte.MaxValue,
                (short)-1,
                (ushort)1,
                int.MaxValue,
                (uint)1,
                long.MaxValue,
                (ulong)1,
                float.NegativeInfinity,
                double.MinValue,
                1.000001m
            };

            var expressions = new List<Expression>();

            // Add test for each value type
            foreach (object value in testValues) {
                Type type = value.GetType();
                var label = Expression.Label(type, type.Name + "Label");
                var result = Expression.Variable(type, type.Name + "Result");
                variables.Add(result);
                expressions.Add(
                    Expression.Assign(
                        result,
                        Expression.Label(
                            label,
                            Expression.Block(
                                EU.ConcatEquals(variables[0], Expression.Constant("1")),
                                Expression.Goto(label, Expression.Constant(value)),
                                EU.ConcatEquals(variables[0], Expression.Constant("2")),
                                Expression.Default(type)
                            )
                        )
                    )
                );
            }

            // Add result validation
            for (int i = 0; i < testValues.Length; i++) {
                expressions.Add(
                    EU.GenAreEqual(Expr.Constant(testValues[i]), variables[i + 1], "Goto " + (i + 1))
                );
            }

            expressions.Add(EU.GenAreEqual(Expr.Constant(new string('1', testValues.Length)), Result, "Verify Result"));

            var tree = Expr.Block(variables, expressions);
            V.Validate(tree);
            return tree;
        }

        public class TestClass {
            public int X { get; set; }
            public TestClass() { X = 0; }
            public TestClass(int val) { X = val; }
            public static bool operator ==(TestClass a, TestClass b) {
                return a.X == b.X;
            }
            public static bool operator !=(TestClass a, TestClass b) {
                return a.X != b.X;
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }
        }

        // Goto a valid label of a user defined type passing the correct typed value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 94", new string[] { "positive", "goto", "miscellaneous", "Pri2" })]
        public static Expr Goto94(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");

            LabelTarget target = Expr.Label(typeof(TestClass));

            Expr test = Expr.Assign(
                Obj,
                Expr.Label(
                    target,
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("1")),
                        Expr.Goto(target, Expr.Constant(new TestClass(5))),
                        EU.ConcatEquals(Result, Expr.Constant("2")),
                        Expr.Constant(new TestClass(0))
                    )
                )
            );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(new TestClass(5)), Obj, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class DerivedTestClass : TestClass {
            public DerivedTestClass(int val) { X = val; }
        }

        // Goto a valid label of a derived type passing the correct typed value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 95", new string[] { "positive", "goto", "miscellaneous", "Pri2" })]
        public static Expr Goto95(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Obj = Expr.Variable(typeof(DerivedTestClass), "Obj");

            LabelTarget target = Expr.Label(typeof(TestClass));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant(new DerivedTestClass(5))),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Label(target, Expr.Constant(new DerivedTestClass(0))),
                    EU.ConcatEquals(Result, Expr.Constant("3"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("13"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result, Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use label of an open generic type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 96", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Goto96(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Value = Expr.Variable(typeof(int), "Value");

            var target =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Label(typeof(List<>));
                });

            return Expr.Empty();
        }

        // Goto label defined in the same scope and above the Goto
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 97", new string[] { "positive", "goto", "miscellaneous", "Pri2" })]
        public static Expr Goto97(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Value = Expr.Variable(typeof(int), "Value");

            LabelTarget target = Expr.Label();

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Assign(Value, Expr.Constant(0)),
                    Expr.Label(target),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Condition(
                        Expr.Equal(Value, Expr.Constant(1)),
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Done")), Expr.Empty()),
                        Expr.Goto(target, Expr.Assign(Value, Expr.Constant(1)))
                    )
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("122Done"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Value, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Define two labels in the same scope each pointing to the same LabelTarget, Goto each
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 98", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto98(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target = Expr.Label(typeof(void));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Label(target),
                    EU.ConcatEquals(Result, Expr.Constant("3")),
                    Expr.Label(target),
                    EU.ConcatEquals(Result, Expr.Constant("4"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("123"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto switch cases
        // Goto out of switch cases
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 99", new string[] { "positive", "goto", "miscellaneous", "Pri2" })]
        public static Expr Goto99(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            LabelTarget target1 = Expr.Label(typeof(void));
            LabelTarget target2 = Expr.Label(typeof(void));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Switch(Expr.Constant(0), new SwitchCase[] {
                                    EU.SwitchCase(0, 
                                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("2")), Expr.Goto(target1), EU.ConcatEquals(Result, Expr.Constant("3")))
                                    ),
                                    EU.SwitchCase(1,
                                        Expr.Block(
                                            EU.ConcatEquals(Result, Expr.Constant("4")), 
                                            Expr.Label(target1),
                                            EU.ConcatEquals(Result, Expr.Constant("5")),
                                            Expr.Goto(target2), 
                                            EU.ConcatEquals(Result, Expr.Constant("6"))
                                        )
                                    ),
                                    EU.SwitchCase(2,
                                        Expr.Block(
                                            EU.ConcatEquals(Result, Expr.Constant("7"))
                                        )
                                    )        
                                }
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("8")),
                    Expr.Label(target2),
                    EU.ConcatEquals(Result, Expr.Constant("9"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1259"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto inside loop bypassing local variable assignments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 100", new string[] { "positive", "goto", "miscellaneous", "Pri2" })]
        public static Expr Goto100(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression X = Expr.Variable(typeof(int), "X");
            ParameterExpression Y = Expr.Variable(typeof(string), "Y");

            LabelTarget target = Expr.Label(typeof(void));
            LabelTarget breakTarget = Expr.Label(typeof(void));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target),
                    Expr.Assign(X, Expr.Constant(5)),
                    Expr.Assign(Y, Expr.Constant("Test")),
                    Expr.Loop(
                        Expr.Block(
                            Expr.Label(target),
                            Expr.Condition(
                                Expr.Equal(X, Expr.Constant(0)),
                                EU.ConcatEquals(Result, Expr.Constant("2")),
                                EU.ConcatEquals(Result, Expr.Constant("Bad"))
                            ),
                            Expr.Condition(
                                Expr.Equal(Y, Expr.Constant(null)),
                                EU.ConcatEquals(Result, Expr.Constant("3")),
                                EU.ConcatEquals(Result, Expr.Constant("AlsoBad"))
                            ),
                            Expr.Break(breakTarget)
                        ),
                        breakTarget
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("4"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1234"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result, X, Y }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto outside a try from a fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 101", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto101(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryFault(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //fault
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("5"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("6")),
                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                    EU.ConcatEquals(Result, Expr.Constant("7"))
                );

            Expressions.Add(Expr.TryCatch(test, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1247"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // From inside a try, goto a nested finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 102", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto102(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            Expr.TryFinally(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Block( // nested finally
                                    EU.ConcatEquals(Result, Expr.Constant("5")),
                                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                    EU.ConcatEquals(Result, Expr.Constant("6"))
                                )
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("7"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("8"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12678"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From inside a try, goto a nested fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 103", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto103(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("3")),
                            Expr.TryFault(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Block( // nested fault
                                    EU.ConcatEquals(Result, Expr.Constant("5")),
                                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                    EU.ConcatEquals(Result, Expr.Constant("6"))
                                )
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("7"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("8"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12678"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // From inside a catch, goto a nested finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 104", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto104(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Goto(target, Expr.Constant("Goto")),
                                EU.ConcatEquals(Result, Expr.Constant("5")),
                                Expr.TryFinally(
                                    EU.ConcatEquals(Result, Expr.Constant("6")),
                                    Expr.Block( // nested finally
                                        EU.ConcatEquals(Result, Expr.Constant("7")),
                                        Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                        EU.ConcatEquals(Result, Expr.Constant("8"))
                                    )
                                ),
                                EU.ConcatEquals(Result, Expr.Constant("9"))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("10"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12489"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // From inside a catch, goto a nested fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 105", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto105(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("4")),
                                Expr.Goto(target, Expr.Constant("Goto")),
                                EU.ConcatEquals(Result, Expr.Constant("5")),
                                Expr.TryFault(
                                    EU.ConcatEquals(Result, Expr.Constant("6")),
                                    Expr.Block( // nested fault
                                        EU.ConcatEquals(Result, Expr.Constant("7")),
                                        Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                        EU.ConcatEquals(Result, Expr.Constant("8"))
                                    )
                                ),
                                EU.ConcatEquals(Result, Expr.Constant("9"))
                            )
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("10"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12489"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // From inside a finally, goto a nested fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 106", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto106(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryCatchFinally(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //finally
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("5")),
                            Expr.TryFault(
                                EU.ConcatEquals(Result, Expr.Constant("6")),
                                Expr.Block( // nested fault
                                    EU.ConcatEquals(Result, Expr.Constant("7")),
                                    Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                    EU.ConcatEquals(Result, Expr.Constant("8"))
                                )
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("9"))
                        ),
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("10"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("12Caught48910"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // From inside a fault, goto a nested catch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 107", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto107(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string));

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.TryFault(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("2")),
                            Expr.Throw(Expr.Constant(new DivideByZeroException())),
                            EU.ConcatEquals(Result, Expr.Constant("3"))
                        ),
                        Expr.Block( //fault
                            EU.ConcatEquals(Result, Expr.Constant("4")),
                            Expr.Goto(target, Expr.Constant("Go")),
                            EU.ConcatEquals(Result, Expr.Constant("5")),
                            Expr.TryCatch(
                                EU.ConcatEquals(Result, Expr.Constant("6")),
                                Expr.Catch(typeof(ArgumentOutOfRangeException),
                                    Expr.Block( // nested try
                                        EU.ConcatEquals(Result, Expr.Constant("7")),
                                        Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                                        EU.ConcatEquals(Result, Expr.Constant("8"))
                                    )
                                )
                            ),
                            EU.ConcatEquals(Result, Expr.Constant("9"))
                        )
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("10"))
                );

            Expressions.Add(Expr.TryCatch(test, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Caught")))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1248910"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // In a fault nested inside a loop, break/continue
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 108", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto108(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Index = Expr.Variable(typeof(int), "Index");

            LabelTarget breakLabel = Expr.Label(typeof(void), "breakLabel");

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("Begin")),
                    Expr.Assign(Index, Expr.Constant(0)),
                    Expr.Loop(
                        Expr.TryFault(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("L")),
                                Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                                Expr.Condition( // if true, throw
                                    Expr.GreaterThanOrEqual(Index, Expr.Constant(3)),
                                    Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                    Expr.Empty()
                                )
                            ),
                            Expr.Block( //fault
                                EU.ConcatEquals(Result, Expr.Constant("Fault")),
                                Expr.Break(breakLabel),
                                EU.ConcatEquals(Result, Expr.Constant("Bad"))
                            )
                        ),
                        breakLabel,
                        null
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("End"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeginLLLFaultEnd"), Result, "Goto 1"));

            var tree = Expr.Block(new[] { Result, Index }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // Goto within a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 109", new string[] { "positive", "Goto", "miscellaneous", "Pri2" })]
        public static Expr Goto109(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        EU.Goto(target2Value, target2, Expr.Constant(10)),
                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                        AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(3))),
                        AstUtils.YieldReturn(target, Expr.Constant(4))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 10, 4 }, 3, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto within blocks in Utils.FinallyFlowControl
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 110", new string[] { "positive", "goto", "miscellaneous", "Pri2" })]
        public static Expr Goto110(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        AstUtils.FinallyFlowControl(
                            Expr.Block(
                                Expr.Block(
                                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                                        EU.Goto(target2Value, target2, Expr.Constant(10)),
                                        AstUtils.YieldReturn(target, Expr.Constant(3))
                                    ),
                                Expr.Block(
                                    Expr.TryCatchFinally(
                                        AstUtils.YieldReturn(target, Expr.Constant(4)),
                                        AstUtils.YieldReturn(target, Expr.Constant(5)),
                                        Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(-1)))
                                    )
                                ),
                                Expr.Block(
                                    AstUtils.YieldReturn(target, Expr.Constant(6)),
                                    AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(0))),
                                    AstUtils.YieldReturn(target, Expr.Constant(7))
                                )
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(8))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 10, 7, 8 }, 5, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto into a lambda
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 111", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Goto111(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Result2 = Expr.Variable(typeof(string), "Result2");

            LabelTarget target = Expr.Label(typeof(string), "MyLabel");

            LambdaExpression lambda =
                Expr.Lambda(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("BeforeLabel")),
                        Expr.Assign(Result2, Expr.Label(target, Expr.Constant("Default"))),
                        EU.ConcatEquals(Result, Expr.Constant("AfterLabel"))
                    ),
                    new ParameterExpression[] { }
                );

            Expr test =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Goto(target, Expr.Constant("Go")),
                    Expr.Invoke(lambda, new Expression[] { }),
                    EU.ConcatEquals(Result, Expr.Constant("2"))
                );

            Expressions.Add(test);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1GoAfterLabelBeforeLabelDefaultAfterLabel2"), Result, "Goto 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Go"), Result2, "Goto 2"));

            var tree = Expr.Block(new[] { Result, Result2 }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }


        // Goto with explicit type (type unrelated to the Value property)
        // Regression for 565112
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 112", new string[] { "positive", "goto", "miscellaneous", "Pri2" }, Priority = 2)]
        public static Expr Goto112(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            LabelTarget intTarget = Expr.Label(typeof(int), "intLabel");
            LabelTarget doubleTarget = Expr.Label(typeof(double), "doubleLabel");

            // type is the same as the type of the Goto Value
            var g1 = Expr.Goto(intTarget, Expr.Constant(2), typeof(int));

            Expr intTest =
                Expr.Block(
                    g1,
                    Expr.Label(intTarget, Expr.Constant(1))
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), intTest, "Goto 1"));
            Expressions.Add(EU.ExprTypeCheck(intTest, typeof(int)));
            Expressions.Add(EU.ExprTypeCheck(g1, typeof(int)));

            // Goto type is different from the type of the Goto Value
            var g2 = Expr.Goto(doubleTarget, Expr.Constant(2.1), typeof(int));

            Expr doubleTest =
                Expr.Block(
                    Expr.Goto(doubleTarget, Expr.Constant(2.1), typeof(int)),
                    Expr.Label(doubleTarget, Expr.Constant(1.1))
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2.1), doubleTest, "Goto 2"));
            Expressions.Add(EU.ExprTypeCheck(doubleTest, typeof(double)));
            Expressions.Add(EU.ExprTypeCheck(g2, typeof(int)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto with explicit type (type unrelated to the Value property)
        // Regression for 565112
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 113", new string[] { "negative", "goto", "miscellaneous", "Pri2" }, Priority = 2, Exception = typeof(ArgumentException))]
        public static Expr Goto113(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            LabelTarget intTarget = Expr.Label(typeof(int), "intLabel");

            // Goto type matches Goto Value but Label.Type is wrong
            Expr stringTest =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Block(
                        Expr.Goto(intTarget, Expr.Constant("A"), typeof(string)),
                        Expr.Label(intTarget, Expr.Constant("Default"))
                    );
                });

            return Expr.Empty();
        }

        // Pass null to value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Goto 114", new string[] { "positive", "goto", "miscellaneous", "Pri1" })]
        public static Expr Goto114(EU.IValidator V) {
            LabelTarget target = Expr.Label(typeof(string));
            AstUtils.Equals(target.ToString(), "UnamedLabel");
            target = Expr.Label(typeof(string), "foo");
            AstUtils.Equals(target.ToString(), "foo");

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }
    }
}
