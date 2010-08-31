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

namespace ETScenarios.Operators {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Decrement {
        // Decrement of numeric types
        // Regression for Dev10 Bug 546775
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 1", new string[] { "positive", "decrement", "operators", "Pri1" })]
        public static Expr Decrement1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)(-1), typeof(short)), Expr.Decrement(Expr.Constant((short)(0), typeof(short))), "Decrement 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ushort)1, typeof(ushort)), Expr.Decrement(Expr.Constant((ushort)2, typeof(ushort))), "Decrement 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)0, typeof(Int16)), Expr.Decrement(Expr.Constant((Int16)1, typeof(Int16))), "Decrement 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt16)1, typeof(UInt16)), Expr.Decrement(Expr.Constant((UInt16)2, typeof(UInt16))), "Decrement 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)Int32.MaxValue, typeof(Int32)), Expr.Decrement(Expr.Constant((Int32)Int32.MinValue, typeof(Int32))), "Decrement 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt32)1, typeof(UInt32)), Expr.Decrement(Expr.Constant((UInt32)2, typeof(UInt32))), "Decrement 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)1.0, typeof(double)), Expr.Decrement(Expr.Constant((double)2.0, typeof(double))), "Decrement 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)1.1, typeof(long)), Expr.Decrement(Expr.Constant((long)2.1, typeof(long))), "Decrement 8"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ulong)1.001, typeof(ulong)), Expr.Decrement(Expr.Constant((ulong)2.001, typeof(ulong))), "Decrement 9"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((decimal)(0.001), typeof(decimal)), Expr.Decrement(Expr.Constant((decimal)1.001, typeof(decimal))), "Decrement 10"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Decrement of void
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 2", new string[] { "negative", "decrement", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Decrement2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var Result = EU.Throws<ArgumentException>(() => { Expr.Variable(typeof(void), "Result"); });

            Expr.Decrement(Result);

            return Expr.Empty();
        }

        // Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 3", new string[] { "negative", "decrement", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Decrement3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
			EU.Throws<System.ArgumentNullException>(() => {
	            Expressions.Add(Expr.Decrement(null)	);
				});
            return Expr.Empty();
        }

        public class TestClass {
            public int X { get; set; }
            public TestClass(int val) { X = val; }
            public static TestClass operator --(TestClass a) {
                a.X -= 1;
                return a;
            }
            public static int TestMethod(int x) {
                return x - 2;
            }
        }

        // Decrement of a user defined type that defines the decrement operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 4", new string[] { "positive", "decrement", "operators", "Pri2" })]
        public static Expr Decrement4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(TestClass), "Result");
            var ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(Result, Expr.New(ci, new Expression[] { Expr.Constant(0) })));

            PropertyInfo pi = typeof(TestClass).GetProperty("X");
            Expr newValue = Expr.Decrement(Result);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.Property(newValue, pi), "Decrement 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Decrement of string
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 5", new string[] { "negative", "decrement", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Decrement5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { Expr.Decrement(Expr.Constant("1")); }));
            return Expr.Empty();
        }

        // Decrement of date
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 6", new string[] { "negative", "decrement", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Decrement6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.Constant(DateTime.Parse("1/2/2009")), Expr.Decrement(Expr.Constant(DateTime.Parse("1/1/2009"))), "Decrement 1"); }));
            return Expr.Empty();
        }

        // Overflow an int with the decrement operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 7", new string[] { "positive", "decrement", "operators", "Pri2" })]
        public static Expr Decrement7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant(Int32.MaxValue), Expr.Decrement(Expr.Constant(Int32.MinValue)), "Decrement 1"));
            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 8", new string[] { "negative", "decrement", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Decrement8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");
            Expressions.Add(EU.Throws<ArgumentNullException>(() => { Expr.Decrement(null, mi); }));

            return Expr.Empty();
        }

        // With a null method, decrement of numeric types
        // Regression for Dev10 Bug 546775
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 9", new string[] { "positive", "decrement", "operators", "Pri2" })]
        public static Expr Decrement9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)(-1), typeof(short)), Expr.Decrement(Expr.Constant((short)(0), typeof(short)), null), "Decrement 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ushort)1, typeof(ushort)), Expr.Decrement(Expr.Constant((ushort)2, typeof(ushort)), null), "Decrement 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)0, typeof(Int16)), Expr.Decrement(Expr.Constant((Int16)1, typeof(Int16)), null), "Decrement 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt16)1, typeof(UInt16)), Expr.Decrement(Expr.Constant((UInt16)2, typeof(UInt16)), null), "Decrement 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)Int32.MaxValue, typeof(Int32)), Expr.Decrement(Expr.Constant((Int32)Int32.MinValue, typeof(Int32)), null), "Decrement 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt32)1, typeof(UInt32)), Expr.Decrement(Expr.Constant((UInt32)2, typeof(UInt32)), null), "Decrement 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)1.0, typeof(double)), Expr.Decrement(Expr.Constant((double)2.0, typeof(double)), null), "Decrement 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)1.1, typeof(long)), Expr.Decrement(Expr.Constant((long)2.1, typeof(long)), null), "Decrement 8"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ulong)1.001, typeof(ulong)), Expr.Decrement(Expr.Constant((ulong)2.001, typeof(ulong)), null), "Decrement 9"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((decimal)(0.001), typeof(decimal)), Expr.Decrement(Expr.Constant((decimal)1.001, typeof(decimal)), null), "Decrement 10"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a method that takes no arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 10", new string[] { "negative", "decrement", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Decrement10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Func<int> f = () => 1;
            MethodInfo mi = f.Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(0), Expr.Decrement(Expr.Constant(1), mi), "Decrement 1");
            }));

            return Expr.Block(Expressions);
        }

        public static int paramTestMethod(params int[] args) {
            return 3;
        }

        // Pass a method that takes a param array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 11", new string[] { "negative", "decrement", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Decrement11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Decrement).GetMethod("paramTestMethod");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Decrement(Expr.Constant(1), mi), "Decrement 1"));
            });

            return Expr.Empty();
        }

        // Pass a method that takes 2 arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 12", new string[] { "negative", "decrement", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Decrement12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Func<int, int, int> f = (x, y) => x + y;
            MethodInfo mi = f.Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(5), Expr.Decrement(Expr.Constant(4), mi), "Decrement 1");
            }));

            return Expr.Empty();
        }

        public static double typeTestMethod(int x) {
            return (double)x - 1.0;
        }

        // Pass a method that returns a different type from expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Decrement 13", new string[] { "positive", "decrement", "operators", "Pri2" })]
        public static Expr Decrement13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Decrement).GetMethod("typeTestMethod");

            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)2.0, typeof(double)), Expr.Decrement(Expr.Constant(3), mi), "Decrement 1"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
