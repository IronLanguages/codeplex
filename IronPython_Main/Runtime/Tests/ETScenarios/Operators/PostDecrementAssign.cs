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

    public class PostDecrementAssign {
        // PostDecrementAssign of numeric types
        // Regression for Dev10 Bug 546775
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 1", new string[] { "positive", "postdecrementassign", "operators", "Pri1" })]
        public static Expr PostDecrementAssign1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression first = Expr.Variable(typeof(short), "first");
            Expressions.Add(Expr.Assign(first, Expr.Constant((short)(0), typeof(short))));
            Expressions.Add(Expr.PostDecrementAssign(first));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)(-1), typeof(short)), first, "PostDecrementAssign 1"));

            ParameterExpression second = Expr.Variable(typeof(ushort), "second");
            Expressions.Add(Expr.Assign(second, Expr.Constant((ushort)2, typeof(ushort))));
            Expressions.Add(Expr.PostDecrementAssign(second));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ushort)1, typeof(ushort)), second, "PostDecrementAssign 2"));

            ParameterExpression third = Expr.Variable(typeof(Int16), "third");
            Expressions.Add(Expr.Assign(third, Expr.Constant((Int16)(-1), typeof(Int16))));
            Expressions.Add(Expr.PostDecrementAssign(third));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)(-2), typeof(Int16)), third, "PostDecrementAssign 3"));

            ParameterExpression fourth = Expr.Variable(typeof(UInt16), "fourth");
            Expressions.Add(Expr.Assign(fourth, Expr.Constant((UInt16)2, typeof(UInt16))));
            Expressions.Add(Expr.PostDecrementAssign(fourth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt16)1, typeof(UInt16)), fourth, "PostDecrementAssign 4"));

            ParameterExpression fifth = Expr.Variable(typeof(Int32), "fifth");
            Expressions.Add(Expr.Assign(fifth, Expr.Constant((Int32)Int32.MinValue, typeof(Int32))));
            Expressions.Add(Expr.PostDecrementAssign(fifth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)Int32.MaxValue, typeof(Int32)), fifth, "PostDecrementAssign 5"));

            ParameterExpression sixth = Expr.Variable(typeof(UInt32), "sixth");
            Expressions.Add(Expr.Assign(sixth, Expr.Constant((UInt32)2, typeof(UInt32))));
            Expressions.Add(Expr.PostDecrementAssign(sixth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt32)1, typeof(UInt32)), sixth, "PostDecrementAssign 6"));

            ParameterExpression seventh = Expr.Variable(typeof(double), "seventh");
            Expressions.Add(Expr.Assign(seventh, Expr.Constant((double)2.1, typeof(double))));
            Expressions.Add(Expr.PostDecrementAssign(seventh));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)1.1, typeof(double)), seventh, "PostDecrementAssign 7"));

            ParameterExpression eighth = Expr.Variable(typeof(long), "eighth");
            Expressions.Add(Expr.Assign(eighth, Expr.Constant((long)(-1.111), typeof(long))));
            Expressions.Add(Expr.PostDecrementAssign(eighth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)(-2.111), typeof(long)), eighth, "PostDecrementAssign 8"));

            ParameterExpression nineth = Expr.Variable(typeof(ulong), "nineth");
            Expressions.Add(Expr.Assign(nineth, Expr.Constant((ulong)2.999, typeof(ulong))));
            Expressions.Add(Expr.PostDecrementAssign(nineth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ulong)1.999, typeof(ulong)), nineth, "PostDecrementAssign 9"));

            ParameterExpression tenth = Expr.Variable(typeof(decimal), "tenth");
            Expressions.Add(Expr.Assign(tenth, Expr.Constant((decimal)2.001, typeof(decimal))));
            Expressions.Add(Expr.PostDecrementAssign(tenth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((decimal)1.001, typeof(decimal)), tenth, "PostDecrementAssign 10"));

            var tree = Expr.Block(new[] { first, second, third, fourth, fifth, sixth, seventh, eighth, nineth, tenth }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a non assignable expression to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 3", new string[] { "negative", "postdecrementassign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PostDecrementAssign3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.PostDecrementAssign(Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        public class TestClass {
            public int m_x;
            public int X { get { return m_x; } set { m_x = value; } }
            public TestClass(int val) { X = val; }
            public static int TestMethod(int x) {
                return x - 2;
            }
            public static TestClass operator --(TestClass a) {
                a.X -= 1;
                return a;
            }
        }

        // Pass a property to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 4", new string[] { "positive", "postdecrementassign", "operators", "Pri1" })]
        public static Expr PostDecrementAssign4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Value = Expr.Variable(typeof(TestClass), "Value");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(Int32) });
            Expressions.Add(Expr.Assign(Value, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            PropertyInfo pi = typeof(TestClass).GetProperty("X");
            Expressions.Add(Expr.PostDecrementAssign(Expr.Property(Value, pi)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Expr.Property(Value, pi), "PreIncrement 1"));

            var tree = Expr.Block(new[] { Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a field to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 5", new string[] { "positive", "postdecrementassign", "operators", "Pri1" })]
        public static Expr PostDecrementAssign5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Value = Expr.Variable(typeof(TestClass), "Value");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(Int32) });
            Expressions.Add(Expr.Assign(Value, Expr.New(ci, new Expression[] { Expr.Constant(0) })));

            FieldInfo fi = typeof(TestClass).GetField("m_x");
            Expressions.Add(Expr.PostDecrementAssign(Expr.Field(Value, fi)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.Field(Value, fi), "PreIncrement 1"));

            var tree = Expr.Block(new[] { Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a methodinfo, pass a non assignable expression to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 6", new string[] { "negative", "postdecrementassign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PostDecrementAssign6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Func<int, int> m = (x => (x - 2));
            MethodInfo mi = m.Method;
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.PostDecrementAssign(Expr.Constant(1), mi));
            });

            return Expr.Empty();
        }

        // With a methodinfo, pass a property to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 7", new string[] { "positive", "postdecrementassign", "operators", "Pri1" })]
        public static Expr PostDecrementAssign7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Value = Expr.Variable(typeof(TestClass), "Value");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(Int32) });
            Expressions.Add(Expr.Assign(Value, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");

            PropertyInfo pi = typeof(TestClass).GetProperty("X");
            Expressions.Add(Expr.PostDecrementAssign(Expr.Property(Value, pi), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.Property(Value, pi), "PreIncrement 1"));

            var tree = Expr.Block(new[] { Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a methodinfo, pass a field to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 8", new string[] { "positive", "postdecrementassign", "operators", "Pri1" })]
        public static Expr PostDecrementAssign8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Value = Expr.Variable(typeof(TestClass), "Value");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(Int32) });
            Expressions.Add(Expr.Assign(Value, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");

            FieldInfo fi = typeof(TestClass).GetField("m_x");
            Expressions.Add(Expr.PostDecrementAssign(Expr.Field(Value, fi), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.Field(Value, fi), "PreIncrement 1"));

            var tree = Expr.Block(new[] { Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a methodinfo, pass a variable to expression
        // With a methodinfo, check the return value of the expression and the value of the variable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 9", new string[] { "positive", "postdecrementassign", "operators", "Pri1" })]
        public static Expr PostDecrementAssign9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.Constant(1)));

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");

            Expr op = Expr.PostDecrementAssign(Result, mi);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), op, "PostDecrementAssign 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Result, "PostDecrementAssign 2"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, check the return value of the expression and the value of the variable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 10", new string[] { "positive", "postdecrementassign", "operators", "Pri1" })]
        public static Expr PostDecrementAssign10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.Constant(1)));

            Expr op = Expr.PostDecrementAssign(Result, (MethodInfo)null);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), op, "PostDecrementAssign 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Result, "PostDecrementAssign 2"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a variable to expression
        // Check the return value of the expression and the value of the variable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 11", new string[] { "positive", "postdecrementassign", "operators", "Pri1" })]
        public static Expr PostDecrementAssign11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.Constant(-1)));

            Expr op = Expr.PostDecrementAssign(Result);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), op, "PostDecrementAssign 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-2), Result, "PostDecrementAssign 2"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 12", new string[] { "negative", "postdecrementassign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr PostDecrementAssign12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            EU.Throws<System.ArgumentNullException>(() =>
            {
                Expressions.Add(Expr.PostDecrementAssign(null));
            });
            return Expr.Empty();
        }

        // PostDecrementAssign of a user defined type that defines the postdecrementassign operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 13", new string[] { "positive", "postdecrementassign", "operators", "Pri2" })]
        public static Expr PostDecrementAssign13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(TestClass), "Result");
            var ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(Result, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            PropertyInfo pi = typeof(TestClass).GetProperty("X");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Expr.Property(Expr.PostDecrementAssign(Result), pi), "PostDecrementAssign 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // PostDecrementAssign of string
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 14", new string[] { "negative", "postdecrementassign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr PostDecrementAssign14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.PostDecrementAssign(Result); }));
            return Expr.Empty();
        }

        // PostDecrementAssign of date
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 15", new string[] { "negative", "postdecrementassign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr PostDecrementAssign15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(DateTime), "Result");

            Expressions.Add(Expr.Assign(Result, Expr.Constant(DateTime.Parse("1/2/2009"))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.Constant(DateTime.Parse("1/1/2009")), Expr.PostDecrementAssign(Result), "PostDecrementAssign 1"); }));

            return Expr.Empty();
        }

        // Overflow an int with the postdecrementassign operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 16", new string[] { "positive", "postdecrementassign", "operators", "Pri2" })]
        public static Expr PostDecrementAssign16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.Constant(Int32.MinValue)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(Int32.MinValue), Expr.PostDecrementAssign(Result), "PostDecrementAssign 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(Int32.MaxValue), Result, "PostDecrementAssign 2"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 17", new string[] { "negative", "postdecrementassign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr PostDecrementAssign17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");
            EU.Throws<System.ArgumentNullException>(() =>
            {
                Expressions.Add(Expr.PostDecrementAssign(null, mi));
            });

            return Expr.Empty();
        }

        // With a null method, postdecrementassign of numeric types
        // Regression for Dev10 Bug 546775
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 18", new string[] { "positive", "postdecrementassign", "operators", "Pri2" })]
        public static Expr PostDecrementAssign18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression first = Expr.Variable(typeof(short), "first");
            Expressions.Add(Expr.Assign(first, Expr.Constant((short)(0), typeof(short))));
            Expressions.Add(Expr.PostDecrementAssign(first, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)(-1), typeof(short)), first, "PostDecrementAssign 1"));

            ParameterExpression second = Expr.Variable(typeof(ushort), "second");
            Expressions.Add(Expr.Assign(second, Expr.Constant((ushort)2, typeof(ushort))));
            Expressions.Add(Expr.PostDecrementAssign(second, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ushort)1, typeof(ushort)), second, "PostDecrementAssign 2"));

            ParameterExpression third = Expr.Variable(typeof(Int16), "third");
            Expressions.Add(Expr.Assign(third, Expr.Constant((Int16)(-1), typeof(Int16))));
            Expressions.Add(Expr.PostDecrementAssign(third, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)(-2), typeof(Int16)), third, "PostDecrementAssign 3"));

            ParameterExpression fourth = Expr.Variable(typeof(UInt16), "fourth");
            Expressions.Add(Expr.Assign(fourth, Expr.Constant((UInt16)2, typeof(UInt16))));
            Expressions.Add(Expr.PostDecrementAssign(fourth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt16)1, typeof(UInt16)), fourth, "PostDecrementAssign 4"));

            ParameterExpression fifth = Expr.Variable(typeof(Int32), "fifth");
            Expressions.Add(Expr.Assign(fifth, Expr.Constant((Int32)Int32.MinValue, typeof(Int32))));
            Expressions.Add(Expr.PostDecrementAssign(fifth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)Int32.MaxValue, typeof(Int32)), fifth, "PostDecrementAssign 5"));

            ParameterExpression sixth = Expr.Variable(typeof(UInt32), "sixth");
            Expressions.Add(Expr.Assign(sixth, Expr.Constant((UInt32)2, typeof(UInt32))));
            Expressions.Add(Expr.PostDecrementAssign(sixth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt32)1, typeof(UInt32)), sixth, "PostDecrementAssign 6"));

            ParameterExpression seventh = Expr.Variable(typeof(double), "seventh");
            Expressions.Add(Expr.Assign(seventh, Expr.Constant((double)2.1, typeof(double))));
            Expressions.Add(Expr.PostDecrementAssign(seventh, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)1.1, typeof(double)), seventh, "PostDecrementAssign 7"));

            ParameterExpression eighth = Expr.Variable(typeof(long), "eighth");
            Expressions.Add(Expr.Assign(eighth, Expr.Constant((long)(-1.111), typeof(long))));
            Expressions.Add(Expr.PostDecrementAssign(eighth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)(-2.111), typeof(long)), eighth, "PostDecrementAssign 8"));

            ParameterExpression nineth = Expr.Variable(typeof(ulong), "nineth");
            Expressions.Add(Expr.Assign(nineth, Expr.Constant((ulong)2.999, typeof(ulong))));
            Expressions.Add(Expr.PostDecrementAssign(nineth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ulong)1.999, typeof(ulong)), nineth, "PostDecrementAssign 9"));

            ParameterExpression tenth = Expr.Variable(typeof(decimal), "tenth");
            Expressions.Add(Expr.Assign(tenth, Expr.Constant((decimal)2.001, typeof(decimal))));
            Expressions.Add(Expr.PostDecrementAssign(tenth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((decimal)1.001, typeof(decimal)), tenth, "PostDecrementAssign 10"));

            var tree = Expr.Block(new[] { first, second, third, fourth, fifth, sixth, seventh, eighth, nineth, tenth }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a method that takes no arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 19", new string[] { "negative", "postdecrementassign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr PostDecrementAssign19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Func<int> f = () => 1;
            MethodInfo mi = f.Method;

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(1), Expr.PostDecrementAssign(Result, mi), "PostDecrementAssign 1");
            }));

            return Expr.Block(new[] { Result }, Expressions);
        }

        public static int paramTestMethod(params int[] args) {
            return 3;
        }

        // Pass a method that takes a param array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 20", new string[] { "negative", "postdecrementassign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr PostDecrementAssign20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            MethodInfo mi = typeof(PostDecrementAssign).GetMethod("paramTestMethod");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.PostDecrementAssign(Result, mi), "PostDecrementAssign 1"));
            });

            var tree =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Block(new[] { Result }, Expressions);
            });

            return tree;
        }

        // Pass a method that takes 2 arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 21", new string[] { "negative", "postdecrementassign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr PostDecrementAssign21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Func<int, int, int> f = (x, y) => x + y;
            MethodInfo mi = f.Method;

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(5), Expr.PostDecrementAssign(Result, mi), "PostDecrementAssign 1");
            }));

            return Expr.Empty();
        }

        public static double typeTestMethod(int x) {
            return (double)x - 1.0;
        }

        // Pass a method that returns a different type from expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostDecrementAssign 22", new string[] { "negative", "postdecrementassign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr PostDecrementAssign22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            MethodInfo mi = typeof(PostDecrementAssign).GetMethod("typeTestMethod");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant((double)(-1.0), typeof(double)), Expr.PostDecrementAssign(Result, mi), "PostDecrementAssign 1");
            }));

            return Expr.Empty();
        }
    }
}
