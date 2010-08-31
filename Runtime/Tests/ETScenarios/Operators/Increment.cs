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
    
    public class Increment {
        // Increment of numeric types
        // Regression for Dev10 Bug 546775
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 1", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr Increment1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)0, typeof(short)), Expr.Increment(Expr.Constant((short)(-1), typeof(short))), "Increment 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ushort)2, typeof(ushort)), Expr.Increment(Expr.Constant((ushort)1, typeof(ushort))), "Increment 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)1, typeof(Int16)), Expr.Increment(Expr.Constant((Int16)0, typeof(Int16))), "Increment 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt16)2, typeof(UInt16)), Expr.Increment(Expr.Constant((UInt16)1, typeof(UInt16))), "Increment 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)Int32.MinValue, typeof(Int32)), Expr.Increment(Expr.Constant((Int32)Int32.MaxValue, typeof(Int32))), "Increment 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt32)2, typeof(UInt32)), Expr.Increment(Expr.Constant((UInt32)1, typeof(UInt32))), "Increment 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)2.0, typeof(double)), Expr.Increment(Expr.Constant((double)1.0, typeof(double))), "Increment 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)2.1, typeof(long)), Expr.Increment(Expr.Constant((long)1.1, typeof(long))), "Increment 8"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ulong)2.001, typeof(ulong)), Expr.Increment(Expr.Constant((ulong)1.001, typeof(ulong))), "Increment 9"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((decimal).999, typeof(decimal)), Expr.Increment(Expr.Constant((decimal)(-0.001), typeof(decimal))), "Increment 10"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Increment of void
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 2", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Increment2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var Result = EU.Throws<ArgumentException>(() => { Expr.Variable(typeof(void), "Result"); });
            Expressions.Add(Expr.Increment(Result));

            return Expr.Empty();
        }

        // Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 3", new string[] { "negative", "increment", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Increment3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            EU.Throws<System.ArgumentNullException>(() =>
            {
                Expressions.Add(Expr.Increment(null));
            });
            return Expr.Empty();
        }

        public class TestClass {
            public int X { get; set; }
            public TestClass(int val) { X = val; }
            public static TestClass operator ++(TestClass a) {
                a.X += 1;
                return a;
            }
            public static int TestMethod(int x) {
                return x + 2;
            }
        }

        // Increment of a user defined type that defines the increment operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 4", new string[] { "positive", "increment", "operators", "Pri2" })]
        public static Expr Increment4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(TestClass), "Result");
            var ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(Result, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            PropertyInfo pi = typeof(TestClass).GetProperty("X");
            Expr newValue = Expr.Increment(Result);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Property(newValue, pi), "Increment 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Increment of string
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 5", new string[] { "negative", "increment", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Increment5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { Expr.Increment(Expr.Constant("1")); }));
            return Expr.Empty();
        }

        // Increment of date
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 6", new string[] { "negative", "increment", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Increment6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.Constant(DateTime.Parse("1/2/2009")), Expr.Increment(Expr.Constant(DateTime.Parse("1/1/2009"))), "Increment 1"); }));
            return Expr.Empty();
        }

        // Overflow an int with the increment operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 7", new string[] { "positive", "increment", "operators", "Pri2" })]
        public static Expr Increment7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant(Int32.MinValue), Expr.Increment(Expr.Constant(Int32.MaxValue)), "Increment 1"));
            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 8", new string[] { "negative", "increment", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Increment8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");
            Expressions.Add(EU.Throws<System.ArgumentNullException>(() => { Expr.Increment(null, mi); }));

            return Expr.Empty();
        }

        // With a null method, increment of numeric types
        // Regression for Dev10 Bug 546775
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 9", new string[] { "positive", "increment", "operators", "Pri2" })]
        public static Expr Increment9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)0, typeof(short)), Expr.Increment(Expr.Constant((short)(-1), typeof(short)), null), "Increment 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ushort)2, typeof(ushort)), Expr.Increment(Expr.Constant((ushort)1, typeof(ushort)), null), "Increment 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)1, typeof(Int16)), Expr.Increment(Expr.Constant((Int16)0, typeof(Int16)), null), "Increment 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt16)2, typeof(UInt16)), Expr.Increment(Expr.Constant((UInt16)1, typeof(UInt16)), null), "Increment 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)Int32.MinValue, typeof(Int32)), Expr.Increment(Expr.Constant((Int32)Int32.MaxValue, typeof(Int32)), null), "Increment 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt32)2, typeof(UInt32)), Expr.Increment(Expr.Constant((UInt32)1, typeof(UInt32)), null), "Increment 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)2.0, typeof(double)), Expr.Increment(Expr.Constant((double)1.0, typeof(double)), null), "Increment 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)2.1, typeof(long)), Expr.Increment(Expr.Constant((long)1.1, typeof(long)), null), "Increment 8"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ulong)2.001, typeof(ulong)), Expr.Increment(Expr.Constant((ulong)1.001, typeof(ulong)), null), "Increment 9"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((decimal).999, typeof(decimal)), Expr.Increment(Expr.Constant((decimal)(-0.001), typeof(decimal)), null), "Increment 10"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a method that takes no arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 10", new string[] { "negative", "increment", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Increment10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Func<int> f = () => 1;
            MethodInfo mi = f.Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(2), Expr.Increment(Expr.Constant(1), mi), "Increment 1");
            }));

            return Expr.Block(Expressions);
        }

        public static int paramTestMethod(params int[] args) {
            return 3;
        }

        // Pass a method that takes a param array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 11", new string[] { "negative", "increment", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Increment11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Increment).GetMethod("paramTestMethod");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Increment(Expr.Constant(1), mi), "Increment 1"));
            });

            return Expr.Empty();
        }

        // Pass a method that takes 2 arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 12", new string[] { "negative", "increment", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Increment12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Func<int, int, int> f = (x, y) => x + y;
            MethodInfo mi = f.Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(5), Expr.Increment(Expr.Constant(4), mi), "Increment 1");
            }));

            return Expr.Empty();
        }

        public static double typeTestMethod(int x) {
            return (double)x + 1.0;
        }

        // Pass a method that returns a different type from expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Increment 13", new string[] { "positive", "increment", "operators", "Pri2" })]
        public static Expr Increment13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Increment).GetMethod("typeTestMethod");

            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)4.0, typeof(double)), Expr.Increment(Expr.Constant(3), mi), "Increment 1"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
