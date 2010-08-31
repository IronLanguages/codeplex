#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MsSc = System.Dynamic;

namespace ETScenarios.ControlFlow {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Coalesce {
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 1", new string[] { "negative", "coalesce", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Coalesce1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            //lambda that takes an integer and returns integer.
            LambdaExpression LM = Expr.Lambda(Expr.Constant(5));

            //Left value integer type.
            ParameterExpression Left = Expr.Variable(typeof(int), "");

            //Right value. short type
            ParameterExpression Right = Expr.Variable(typeof(short), "");

            Expressions.Add(
                EU.Throws<InvalidOperationException>(() =>
                {
                    Expr.Coalesce(Left, Right, LM);
                })
            );

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 2", new string[] { "positive", "coalesce", "controlflow", "Pri1" })]
        public static Expr Coalesce2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            //Left value integer type.
            ParameterExpression Left = Expr.Variable(typeof(int?), "");

            //Right value. short type
            ParameterExpression Right = Expr.Variable(typeof(short), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant((short)3, typeof(short))));

            Expr Type = EU.GetExprType(Expr.Coalesce(Left, Right));

            Expr Value = Expr.Coalesce(Left, Right);

            Expressions.Add(EU.GenAreEqual(EU.GetExprType(Expr.Constant(3)), Type, "Coalesce 1"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Value, "Coalesce 2"));

            var tree = EU.BlockVoid(new[] { Result, Left, Right }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 3", new string[] { "positive", "coalesce", "controlflow", "Pri1" })]
        public static Expr Coalesce3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            //Left value integer type.
            ParameterExpression Left = Expr.Variable(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Convert(Expr.Constant(5), typeof(int?))));

            //Right value. short type
            ParameterExpression Right = Expr.Variable(typeof(short), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant((short)3, typeof(short))));

            Expr Type = EU.GetExprType(Expr.Coalesce(Left, Right));

            Expr Value = Expr.Coalesce(Left, Right);

            Expressions.Add(EU.GenAreEqual(EU.GetExprType(Left), Type, "Coalesce 1"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant((int)5), Value, "Coalesce 2"));

            var tree = EU.BlockVoid(new[] { Result, Left, Right }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 4", new string[] { "negative", "coalesce", "controlfflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Coalesce4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Right = Expr.Variable(typeof(int?), "Right");
            Expressions.Add(Expr.Assign(Right, Expr.Constant((int?)1, typeof(int?))));

            var Value = EU.Throws<ArgumentNullException>(() => { Expr.Coalesce(null, Right); });

            return Expr.Block(new[] { Right }, Expressions);
        }

        // Pass null to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 5", new string[] { "negative", "coalesce", "controlfflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Coalesce5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(int?), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int?)1, typeof(int?))));

            var Value = EU.Throws<ArgumentNullException>(() => { Expr.Coalesce(Left, null); });

            return Expr.Block(new[] { Left }, Expressions);
        }

        // Pass a non value returning expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 6", new string[] { "negative", "coalesce", "controlfflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Coalesce6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(int?), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int?)1, typeof(int?))));

            var Value = EU.Throws<ArgumentException>(() => { Expr.Coalesce(Left, Expr.Block(Expr.Constant((int?)2, typeof(int?)), Expr.Empty())); });

            return Expr.Empty();
        }

        // Pass a non value returning expression to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 7", new string[] { "negative", "coalesce", "controlfflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Coalesce7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Right = Expr.Variable(typeof(int?), "Right");
            Expressions.Add(Expr.Assign(Right, Expr.Constant((int?)1, typeof(int?))));

            Expr Value = EU.Throws<InvalidOperationException>(() => { Expr.Coalesce(Expr.Block(Expr.Constant((int?)2, typeof(int?)), Expr.Empty()), Right); });

            return Expr.Block(new[] { Right }, Expressions);
        }

        // Pass an expression of Value type (not reference type) to Left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 8", new string[] { "negative", "coalesce", "controlflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Coalesce8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(int), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1, typeof(int))));

            Expr Value = EU.Throws<InvalidOperationException>(() => { Expr.Coalesce(Left, Expr.Constant((int?)2, typeof(int?))); });

            return Expr.Block(new[] { Left }, Expressions);
        }

        // Pass an expression of Value type (not reference type) to Right
        // Pass a constant expression to Left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 9", new string[] { "positive", "coalesce", "controlflow", "Pri2" })]
        public static Expr Coalesce9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Right = Expr.Variable(typeof(int), "Right");
            Expressions.Add(Expr.Assign(Right, Expr.Constant((int)1, typeof(int))));

            Expr Value = Expr.Coalesce(Expr.Constant((int?)2, typeof(int?)), Right);

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Value, "Coalesce 1"));
            Expressions.Add(EU.ExprTypeCheck(Value, typeof(int)));

            var tree = Expr.Block(new[] { Right }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // Pass a scope wrapped expression to left 
        // Pass a scope wrapped expression to right
        // Second case is regression for Dev10 Bug 556858 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 10", new string[] { "positive", "coalesce", "controlflow", "Pri2" })]
        public static Expr Coalesce10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Value =
                // works
                Expr.Coalesce(
                    Expr.Constant((double?)null, typeof(double?)),
                    Expr.Constant((double?)1.1, typeof(double?))
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant((double?)1.1, typeof(double?)), Value, "Coalesce 1"));
            Expressions.Add(EU.ExprTypeCheck(Value, typeof(double?)));

            Expr Value2 =
                Expr.Coalesce(
                    Expr.Constant((double?)null),
                    Expr.Constant((double?)1.1)
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)1.1, typeof(double)), Expr.Unbox(Value2, typeof(double)), "Coalesce 1"));
            Expressions.Add(EU.ExprTypeCheck(Value2, typeof(object)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 11", new string[] { "negative", "coalesce", "controlfflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Coalesce11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression val = Expr.Variable(typeof(string), "val");

            LambdaExpression converter =
                Expr.Lambda<Func<string, double?>>(
                    Expr.Convert(Expr.Call(typeof(double).GetMethod("Parse", new Type[] { typeof(string) }), val), typeof(double?)),
                    val
                );

            Expr Result =
                EU.Throws<System.ArgumentNullException>(() =>
                {
                    Expr.Coalesce(
                        null,
                        Expr.Constant(2.0, typeof(double?)),
                        converter
                    );
                });

            return Expr.Empty();
        }
        // Pass null to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 12", new string[] { "negative", "coalesce", "controlfflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Coalesce12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression val = Expr.Variable(typeof(string), "val");

            LambdaExpression converter =
                Expr.Lambda<Func<string, double?>>(
                    Expr.Convert(Expr.Call(typeof(double).GetMethod("Parse", new Type[] { typeof(string) }), val), typeof(double?)),
                    val
                );

            Expr Result =
                EU.Throws<ArgumentNullException>(() =>
                {
                    Expr.Coalesce(
                        Expr.Constant("1"),
                        null,
                        converter
                    );
                });

            return Expr.Empty();
        }

        // Pass a non value returning expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 13", new string[] { "negative", "coalesce", "controlfflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Coalesce13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression val = Expr.Variable(typeof(string), "val");

            LambdaExpression converter =
                Expr.Lambda<Func<string, double?>>(
                    Expr.Convert(Expr.Call(typeof(double).GetMethod("Parse", new Type[] { typeof(string) }), val), typeof(double?)),
                    val
                );

            Expr Result =
                EU.Throws<InvalidOperationException>(() =>
                {
                    Expr.Coalesce(
                        Expr.Constant("1"),
                        Expr.Block(Expr.Constant(2.0, typeof(double?)), Expr.Empty()),
                        converter
                    );
                });

            return Expr.Empty();
        }

        // Pass a non value returning expression to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 14", new string[] { "negative", "coalesce", "controlfflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Coalesce14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression val = Expr.Variable(typeof(string), "val");

            LambdaExpression converter =
                Expr.Lambda<Func<string, double?>>(
                    Expr.Convert(Expr.Call(typeof(double).GetMethod("Parse", new Type[] { typeof(string) }), val), typeof(double?)),
                    val
                );

            Expr Result =
                EU.Throws<InvalidOperationException>(() =>
                {
                    Expr.Coalesce(
                        Expr.Block(Expr.Constant("1"), Expr.Empty()),
                        Expr.Constant(2.0, typeof(double?)),
                        converter
                    );
                });

            return Expr.Empty();
        }

        // Pass an expression of Value type (not reference type) to Left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 15", new string[] { "negative", "coalesce", "controlflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Coalesce15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression val = Expr.Variable(typeof(int), "val");

            LambdaExpression converter =
                Expr.Lambda<Func<int, double>>(
                    Expr.Call(typeof(Convert).GetMethod("ToDouble", new Type[] { typeof(int) }), val),
                    val
                );

            Expr Result =
                EU.Throws<InvalidOperationException>(() =>
                {
                    Expr.Coalesce(
                        Expr.Constant(1, typeof(int)),
                        Expr.Constant(2.0, typeof(double)),
                        converter
                    );
                });

            return Expr.Empty();
        }

        // Pass an expression of Value type (not reference type) to Right
        // Pass a constant expression to Left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 16", new string[] { "positive", "coalesce", "controlflow", "Pri2" })]
        public static Expr Coalesce16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression val = Expr.Variable(typeof(string), "val");

            LambdaExpression converter =
                Expr.Lambda<Func<string, double>>(
                    Expr.Call(typeof(double).GetMethod("Parse", new Type[] { typeof(string) }), val),
                    val
                );

            Expr Result =
                Expr.Coalesce(
                    Expr.Constant("1"),
                    Expr.Constant(2.0, typeof(double)),
                    converter
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1.0, typeof(double)), Result, "Coalesce 1"));
            Expressions.Add(EU.ExprTypeCheck(Result, typeof(double)));

            var tree = Expr.Block(new[] { val }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // Pass a scope wrapped expression to left 
        // Pass a scope wrapped expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 17", new string[] { "positive", "coalesce", "controlflow", "Pri2" })]
        public static Expr Coalesce17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression val = Expr.Variable(typeof(string), "val");

            LambdaExpression converter =
                Expr.Lambda<Func<string, double?>>(
                    Expr.Convert(Expr.Call(typeof(double).GetMethod("Parse", new Type[] { typeof(string) }), val), typeof(double?)),
                    val
                );

            Expr Result =
                Expr.Coalesce(
                    Expr.Block(Expr.Constant("1")),
                    Expr.Block(Expr.Constant(2.0, typeof(double?))),
                    converter
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1.0, typeof(double?)), Result, "Coalesce 1"));
            Expressions.Add(EU.ExprTypeCheck(Result, typeof(double?)));

            var tree = Expr.Block(new[] { val }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to conversion
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 18", new string[] { "negative", "coalesce", "controlfflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Coalesce18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression val = Expr.Variable(typeof(string), "val");

            LambdaExpression converter =
                Expr.Lambda<Func<string, double?>>(
                    Expr.Convert(Expr.Call(typeof(double).GetMethod("Parse", new Type[] { typeof(string) }), val), typeof(double?)),
                    val
                );

            Expr Result =
                EU.Throws<ArgumentException>(() =>
                {
                    Expr.Coalesce(
                        Expr.Constant("1"),
                        Expr.Constant(2.0, typeof(double?)),
                        null
                    );
                });

            return Expr.Empty();
        }

        // Use a variable that isn't defined in the scope of the Coalesce expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 19", new string[] { "negative", "coalesce", "controlflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Coalesce19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression val = Expr.Variable(typeof(int?), "val");

            var Value =
                Expr.Lambda(
                    Expr.Coalesce(val, Expr.Constant(1, typeof(int?)))
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), EU.Throws<InvalidOperationException>(() => { Expr.Constant(Value.Compile().DynamicInvoke()); }), "Coalesce 1"));

            return Expr.Empty();
        }

        // Pass the same expression to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 20", new string[] { "positive", "coalesce", "controlfflow", "Pri2" })]
        public static Expr Coalesce20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Val = Expr.Variable(typeof(int?), "Left");

            Expressions.Add(Expr.Assign(Val, Expr.Constant((int?)null, typeof(int?))));
            Expr temp = Expr.Block(EU.ConcatEquals(Result, "Test"), Val);

            ParameterExpression CoalesceValue = Expr.Variable(typeof(int?), "Value");
            Expressions.Add(Expr.Assign(CoalesceValue, Expr.Coalesce(temp, temp)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestTest"), Result, "Coalesce 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(null, typeof(int?)), CoalesceValue, "Coalesce 2"));
            Expressions.Add(EU.ExprTypeCheck(CoalesceValue, typeof(int?)));

            var tree = Expr.Block(new[] { Result, Val, CoalesceValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Check that right argument isn't evaluated if left is non null
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 21", new string[] { "positive", "coalesce", "controlfflow", "Pri2" })]
        public static Expr Coalesce21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(int?), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int?)1, typeof(int?))));

            ParameterExpression Right = Expr.Variable(typeof(int?), "Right");
            Expressions.Add(Expr.Assign(Right, Expr.Constant((int?)2, typeof(int?))));

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression CoalesceValue = Expr.Variable(typeof(int?), "Value");
            Expressions.Add(
                Expr.Assign(
                    CoalesceValue,
                    Expr.Coalesce(
                        Expr.Block(EU.ConcatEquals(Result, "Left"), Left),
                        Expr.Block(EU.ConcatEquals(Result, "Right"), Right)
                    )
                )
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Left"), Result, "Coalesce 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int?)), CoalesceValue, "Coalesce 2"));
            Expressions.Add(EU.ExprTypeCheck(CoalesceValue, typeof(int?)));

            var tree = Expr.Block(new[] { Result, Left, Right, CoalesceValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass nullable type to left, non nullable type to right (non convertible to left)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 22", new string[] { "negative", "coalesce", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Coalesce22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Value =
                EU.Throws<ArgumentException>(() =>
                {
                    Expr.Coalesce(
                        Expr.Constant((int?)1, typeof(int?)),
                        Expr.Constant((DateTime)DateTime.Now, typeof(DateTime))
                    );
                });

            return Expr.Empty();
        }

        // Pass nullable type to left, non nullable type to right (convertible to left)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 23", new string[] { "positive", "coalesce", "controlflow", "Pri2" })]
        public static Expr Coalesce23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Value =
                Expr.Coalesce(
                    Expr.Constant((double?)1.1, typeof(double?)),
                    Expr.Constant((int)2.1, typeof(int))
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)1.1, typeof(double)), Value, "Coalesce 1"));
            Expressions.Add(EU.ExprTypeCheck(Value, typeof(double)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // CoalesceWithUserDefinedConversion
        // Regression test fro @bug 445623
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce 24", new string[] { "positive", "coalesce", "controlflow", "Pri1" }, Priority = 1)]
        public static Expr Coalesce24(EU.IValidator V) {

            var s = Expression.Parameter(typeof(string), "s");

            var coalesce = Expression.Lambda<Func<string, int>>(
                            Expression.Coalesce(s,
                                    Expression.Constant(42),
                                    Expression.Lambda<Func<string, int>>(
                                        Expression.Call(typeof(int).GetMethod("Parse",
                                                            new[] { typeof(string) }), s), s)), s).Compile();

            List<Expression> Expressions = new List<Expression>();

            ///var T1 = coalesce(null);

            Expressions.Add(EU.GenAreEqual(Expr.Constant(12, typeof(int)), Expr.Constant(coalesce("12"), typeof(int))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(42, typeof(int)), Expr.Constant(coalesce(null), typeof(int))));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
