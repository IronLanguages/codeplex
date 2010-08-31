#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.ConvertCallInvoke {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Convert {
        // Pass an expression that is of the same type as type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 1", new string[] { "positive", "convert", "convertcallinvoke", "Pri1" })]
        public static Expr Convert1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(Int32), "");

            Expressions.Add(Expr.Assign(Result, Expr.Constant(1)));
            Expressions.Add(Expr.Convert(Result, typeof(Int32)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant(1, typeof(Int32))));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass an expression that is of the same type as type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 2", new string[] { "positive", "convert", "convertcallinvoke", "Pri1" })]
        public static Expr Convert2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(float), "");

            Expressions.Add(Expr.Assign(Result, Expr.Convert(Expr.Constant(1), typeof(float))));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant((float)1.0, typeof(float))));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass an expression with a null value, convert to a convertible type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 3", new string[] { "positive", "convert", "convertcallinvoke", "Pri1" })]
        public static Expr Convert3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expressions.Add(Expr.Assign(Result, Expr.Convert(Expr.Constant(null), typeof(string))));
            Expressions.Add(EU.ConcatEquals(Result, Expr.Constant("Test")));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("Test")));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass an expression of a nullable type without a value, convert to a base type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 4", new string[] { "negative", "convert", "convertcallinvoke", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Convert4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(Int32), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Nullable<Int32>), "");

            Expressions.Add(Expr.Assign(Result, Expr.Convert(TestValue, typeof(Int32))));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, false);
            return tree;
        }

        // Pass an expression with a type that has a user defined conversion to type
        public class MyType {
            public int Val { get; set; }
            public MyType(int x) { Val = x; }
            public static explicit operator int(MyType src) {
                return src.Val;
            }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 5", new string[] { "positive", "convert", "convercallinvoke", "Pri1" })]
        public static Expr Convert5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(int), "");

            Expressions.Add(Expr.Assign(Result, Expr.Convert(Expr.Constant(new MyType(1)), typeof(Int32))));
            Expressions.Add(EU.GenAreEqual(Expr.Add(Result, Expr.Constant(2)), Expr.Constant(3)));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass an expression that is of a CLR convertible type that isn't convertible to the type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 6", new string[] { "negative", "convert", "convertcallinvoke", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Convert6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(DateTime), "");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Assign(Result, Expr.Convert(Expr.Constant((float)(1.0), typeof(float)), typeof(DateTime))); }));
            return Expr.Empty();
        }

        // Pass a user defined type to expression with that type's base class as type arguments
        public class MyExceptionType : Exception {
            public int Val { get; set; }
            public MyExceptionType() { Val = -1; }
            public MyExceptionType(int x) { Val = x; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 7", new string[] { "positive", "convert", "convertcallinvoke", "Pri1" })]
        public static Expr Convert7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(Exception), "");
            Expressions.Add(Expr.Assign(Result, Expr.Convert(Expr.Constant(new MyExceptionType(1)), typeof(Exception))));
            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Multiple nested casts
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 8", new string[] { "positive", "convert", "convertcallinvoke", "Pri1" })]
        public static Expr Convert8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(Exception), "");

            Expressions.Add(
                Expr.Assign(
                    Result,
                    Expr.Convert(
                        Expr.Convert(
                            Expr.Constant(new ArgumentOutOfRangeException()),
                            typeof(ArgumentException)),
                        typeof(Exception)
                    )
                )
            );
            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // Helper method for Convert9 and 10
        // For arrays of BaseType with elements of DerivedType (after a conversion) we need to do this stuff
        // to get an element out as the proper DerivedType
        public static Expr getArrayElement(Expression array, int index) {
            Expr value = Expr.ArrayAccess(array, Expr.Constant(index));
            LambdaExpression L = Expr.Lambda(value, new ParameterExpression[] { });
            Object o = L.Compile().DynamicInvoke();
            Expr newValue = Expr.Constant(o);
            return newValue;
        }

        // Convert array of derived type to array of base type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 9", new string[] { "positive", "convert", "convertcallinvoke", "Pri1" })]
        public static Expr Convert9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr ArrayOfArgEx = Expr.NewArrayInit(typeof(ArgumentException), new Expression[] { Expr.Constant(new ArgumentException("one")), Expr.Constant(new ArgumentException("two")) });
            Expr ArrayOfEx = Expr.Convert(ArrayOfArgEx, typeof(Exception[]));

            PropertyInfo pi = typeof(ArgumentException).GetProperty("Message");
            MethodInfo mi = typeof(ArgumentException).GetMethod("GetBaseException");

            Expressions.Add(EU.ExprTypeCheck(ArrayOfEx, typeof(Exception[])));

            // check first element
            Expr value = Expr.ArrayAccess(ArrayOfEx, Expr.Constant(0));
            LambdaExpression L = Expr.Lambda(value, new ParameterExpression[] { });
            Object o = L.Compile().DynamicInvoke();
            Expr newValue = Expr.Constant(o);

            Expressions.Add(EU.GenAreEqual(Expr.Property(newValue, pi), Expr.Constant("one")));
            Expressions.Add(EU.ExprTypeCheck(newValue, typeof(ArgumentException)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(Expr.Call(newValue, mi).Type, typeof(Type)), Expr.Constant(typeof(Exception), typeof(Type))));

            // check second element
            Expr value2 = Expr.ArrayAccess(ArrayOfEx, Expr.Constant(1));
            L = Expr.Lambda(value2, new ParameterExpression[] { });
            o = L.Compile().DynamicInvoke();
            Expr newValue2 = Expr.Constant(o);

            Expressions.Add(EU.GenAreEqual(Expr.Property(newValue2, pi), Expr.Constant("two")));
            Expressions.Add(EU.ExprTypeCheck(newValue2, typeof(ArgumentException)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(Expr.Call(newValue2, mi).Type, typeof(Type)), Expr.Constant(typeof(Exception), typeof(Type))));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Convert array of derived type to array of base type and add elements of derived type to converted array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 10", new string[] { "positive", "convert", "convertcallinvoke", "Pri1" })]
        public static Expr Convert10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression ArrayOfEx = Expr.Variable(typeof(Exception[]), "ArrayOfEx");
            ParameterExpression thirdElement = Expr.Variable(typeof(ArgumentException), "thirdElement");

            Expr ArrayOfArgEx =
                Expr.NewArrayInit(
                    typeof(ArgumentException),
                    new Expression[] { 
                        Expr.Constant(new ArgumentException("one")),
                        Expr.Constant(new ArgumentException("two")),
                        Expr.Constant(new ArgumentException("notSupposedToExist")) 
                    }
                );

            PropertyInfo pi = typeof(ArgumentException).GetProperty("Message");
            MethodInfo mi = typeof(ArgumentException).GetMethod("GetBaseException");

            // assign new element to ArrayOfEx[2]
            var innerLM =
                Expr.Lambda(
                    Expr.Block(
                        new ParameterExpression[] { thirdElement, ArrayOfEx },
                        Expr.Assign(thirdElement, Expr.Constant(new ArgumentException("three"))),
                        Expr.Assign(ArrayOfEx, Expr.Convert(ArrayOfArgEx, typeof(Exception[]))),
                        Expr.Assign(
                            Expr.ArrayAccess(ArrayOfEx, Expr.Constant(2)),
                            thirdElement
                        ),
                        Expr.ArrayAccess(ArrayOfEx, Expr.Constant(2))
                    )
                );

            Expressions.Add(EU.ExprTypeCheck(ArrayOfEx, typeof(Exception[])));

            // check new inserted element
            Object res = innerLM.Compile().DynamicInvoke();
            Expr newValue3 = Expr.Constant((ArgumentException)res);

            Expressions.Add(EU.GenAreEqual(Expr.Property(newValue3, pi), Expr.Constant("three")));
            Expressions.Add(EU.ExprTypeCheck(newValue3, typeof(ArgumentException)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(Expr.Call(newValue3, mi).Type, typeof(Type)), Expr.Constant(typeof(Exception), typeof(Type))));

            var tree = EU.BlockVoid(new[] { thirdElement, ArrayOfEx }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Convert array of derived type to array of base type and add elements of derived type to converted array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 11", new string[] { "negative", "convert", "convertcallinvoke", "Pri1" }, Exception = typeof(ArrayTypeMismatchException))]
        public static Expr Convert11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr ArrayOfArgEx = Expr.NewArrayInit(typeof(ArgumentException), new Expression[] { Expr.Constant(new ArgumentException("one")), Expr.Constant(new ArgumentException("two")), Expr.Constant(null, typeof(ArgumentException)) });
            Expr ArrayOfEx = Expr.Convert(ArrayOfArgEx, typeof(Exception[]));

            Expressions.Add(Expr.Assign(Expr.ArrayAccess(ArrayOfEx, Expr.Constant(2)), Expr.Constant(new InvalidCastException("three"))));

            var tree = EU.BlockVoid(Expressions);
            V.ValidateException<ArrayTypeMismatchException>(tree, false);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Hand Convert 12", new string[] { "positive", "convert", "convertcallinvoke", "Pri1" })]
        public static Expr Convert12(EU.IValidator V) {
            if (System.Environment.Version.Major >= 4) {
                List<Expression> Expressions = new List<Expression>();

                var p = Expression.Parameter(typeof(object));
                var lm =
                    Expression.Lambda<Func<object, uint>>(
                        Expression.Convert(Expression.Unbox(p, typeof(double)), typeof(uint)),
                        p
                    );

                Func<object, uint> lm2 = foo => (uint)(double)foo;
                double x = uint.MaxValue;

                var res = Expr.Constant(lm.Compile()(x));
                var res2 = Expr.Constant(lm2(x));
                Expression tree = Expr.Block(EU.GenAreEqual(res, res2));
                V.Validate(tree);
                return tree;
            } else {
                return Expr.Empty();
            }
        }

        enum Regress817484_Enum {
            One,
            Two,
            Three
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Regression for Dev10 bug 817484", new string[] { "positive", "convert", "regression", "817484", "Pri1" })]
        public static Expr Regress817484(EU.IValidator V) {
            //This test is only valid on .NET 4.5, which internally is still
            //versioned 4.0 with just a higher build number.  4.0 RTM was
            //build 30319.
            if (System.Environment.Version >= new Version(4, 0, 30322, 0)) {
                List<Expression> Expressions = new List<Expression>();

                //null Enum -> Regress817484_Enum?
                Regress817484_Helper<Enum, Regress817484_Enum?>(null, null);

                //null ValueType -> int?
                Regress817484_Helper<ValueType, int?>(null, null);

                //Regress817484_Enum -> Regress817484_Enum?
                Regress817484_Helper<Enum, Regress817484_Enum?>(Regress817484_Enum.One, Regress817484_Enum.One);
                Regress817484_Helper<Enum, Regress817484_Enum?>(Regress817484_Enum.Two, Regress817484_Enum.Two);
                Regress817484_Helper<Enum, Regress817484_Enum?>(Regress817484_Enum.Three, Regress817484_Enum.Three);

                //ConsoleColor -> Regress817484_Enum?
                EU.Throws<InvalidCastException>(() => Regress817484_Helper<Enum, Regress817484_Enum?>(ConsoleColor.DarkBlue, Regress817484_Enum.One));

                //3 ValueType -> int?
                Regress817484_Helper<ValueType, int?>(3, 3);

                //ValueType -> string
                EU.Throws<InvalidOperationException>(() => Regress817484_Helper<ValueType, String>(null, null));
                EU.Throws<InvalidOperationException>(() => Regress817484_Helper<ValueType, String>(3, null));
            }
            return Expr.Empty();
        }

        /// <summary>
        /// Creates, compiles, and executes a Lambda that converts 'argument' from
        /// type T to U and checks that the result matches 'expected'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="argument"></param>
        /// <param name="expected"></param>
        private static void Regress817484_Helper<T, U>(T argument, U expected) {
            ParameterExpression Argument = Expr.Variable(typeof(T), "x");

            var LM = Expr.Lambda<Func<T, U>>(
                Expr.Convert(Argument, typeof(U)),
                Argument
            );

            var func = LM.Compile();
            EU.Equal((U)expected, func(argument));
        }
    }
}
