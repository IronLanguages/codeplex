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

    public class PostIncrementAssign {
        // PostIncrementAssign of numeric types
        // Regression for Dev10 Bug 546775
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 1", new string[] { "positive", "postincrementassign", "operators", "Pri1" })]
        public static Expr PostIncrementAssign1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression first = Expr.Variable(typeof(short), "first");
            Expressions.Add(Expr.Assign(first, Expr.Constant((short)(-1), typeof(short))));
            Expressions.Add(Expr.PostIncrementAssign(first));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)0, typeof(short)), first, "PostIncrementAssign 1"));

            ParameterExpression second = Expr.Variable(typeof(ushort), "second");
            Expressions.Add(Expr.Assign(second, Expr.Constant((ushort)1, typeof(ushort))));
            Expressions.Add(Expr.PostIncrementAssign(second));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ushort)2, typeof(ushort)), second, "PostIncrementAssign 2"));

            ParameterExpression third = Expr.Variable(typeof(Int16), "third");
            Expressions.Add(Expr.Assign(third, Expr.Constant((Int16)(-2), typeof(Int16))));
            Expressions.Add(Expr.PostIncrementAssign(third));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)(-1), typeof(Int16)), third, "PostIncrementAssign 3"));

            ParameterExpression fourth = Expr.Variable(typeof(UInt16), "fourth");
            Expressions.Add(Expr.Assign(fourth, Expr.Constant((UInt16)1, typeof(UInt16))));
            Expressions.Add(Expr.PostIncrementAssign(fourth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt16)2, typeof(UInt16)), fourth, "PostIncrementAssign 4"));

            ParameterExpression fifth = Expr.Variable(typeof(Int32), "fifth");
            Expressions.Add(Expr.Assign(fifth, Expr.Constant((Int32)Int32.MaxValue, typeof(Int32))));
            Expressions.Add(Expr.PostIncrementAssign(fifth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)Int32.MinValue, typeof(Int32)), fifth, "PostIncrementAssign 5"));

            ParameterExpression sixth = Expr.Variable(typeof(UInt32), "sixth");
            Expressions.Add(Expr.Assign(sixth, Expr.Constant((UInt32)1, typeof(UInt32))));
            Expressions.Add(Expr.PostIncrementAssign(sixth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt32)2, typeof(UInt32)), sixth, "PostIncrementAssign 6"));

            ParameterExpression seventh = Expr.Variable(typeof(double), "seventh");
            Expressions.Add(Expr.Assign(seventh, Expr.Constant((double)1.1, typeof(double))));
            Expressions.Add(Expr.PostIncrementAssign(seventh));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)2.1, typeof(double)), seventh, "PostIncrementAssign 7"));

            ParameterExpression eighth = Expr.Variable(typeof(long), "eighth");
            Expressions.Add(Expr.Assign(eighth, Expr.Constant((long)(-2.111), typeof(long))));
            Expressions.Add(Expr.PostIncrementAssign(eighth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)(-1.111), typeof(long)), eighth, "PostIncrementAssign 8"));

            ParameterExpression nineth = Expr.Variable(typeof(ulong), "nineth");
            Expressions.Add(Expr.Assign(nineth, Expr.Constant((ulong)1.999, typeof(ulong))));
            Expressions.Add(Expr.PostIncrementAssign(nineth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ulong)2.999, typeof(ulong)), nineth, "PostIncrementAssign 9"));

            ParameterExpression tenth = Expr.Variable(typeof(decimal), "tenth");
            Expressions.Add(Expr.Assign(tenth, Expr.Constant((decimal)1.001, typeof(decimal))));
            Expressions.Add(Expr.PostIncrementAssign(tenth));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((decimal)2.001, typeof(decimal)), tenth, "PostIncrementAssign 10"));

            var tree = Expr.Block(new[] { first, second, third, fourth, fifth, sixth, seventh, eighth, nineth, tenth }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a non assignable expression to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 3", new string[] { "negative", "postincrementassign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PostIncrementAssign3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.PostIncrementAssign(Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        public class TestClass {
            public int m_x;
            public int X { get { return m_x; } set { m_x = value; } }
            public TestClass(int val) { X = val; }
            public static int TestMethod(int x) {
                return x + 2;
            }
            public static TestClass operator ++(TestClass a) {
                a.X += 1;
                return a;
            }
        }

        // Pass a property to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 4", new string[] { "positive", "postincrementassign", "operators", "Pri1" })]
        public static Expr PostIncrementAssign4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Value = Expr.Variable(typeof(TestClass), "Value");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(Int32) });
            Expressions.Add(Expr.Assign(Value, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            PropertyInfo pi = typeof(TestClass).GetProperty("X");
            Expressions.Add(Expr.PostIncrementAssign(Expr.Property(Value, pi)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Property(Value, pi), "PreIncrement 1"));

            var tree = Expr.Block(new[] { Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a field to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 5", new string[] { "positive", "postincrementassign", "operators", "Pri1" })]
        public static Expr PostIncrementAssign5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Value = Expr.Variable(typeof(TestClass), "Value");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(Int32) });
            Expressions.Add(Expr.Assign(Value, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            FieldInfo fi = typeof(TestClass).GetField("m_x");
            Expressions.Add(Expr.PostIncrementAssign(Expr.Field(Value, fi)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Field(Value, fi), "PreIncrement 1"));

            var tree = Expr.Block(new[] { Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a methodinfo, pass a non assignable expression to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 6", new string[] { "negative", "postincrementassign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PostIncrementAssign6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Func<int, int> m = (x => (x + 2));
            MethodInfo mi = m.Method;
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.PostIncrementAssign(Expr.Constant(1), mi));
            });

            return Expr.Empty();
        }

        // With a methodinfo, pass a property to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 7", new string[] { "positive", "postincrementassign", "operators", "Pri1" })]
        public static Expr PostIncrementAssign7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Value = Expr.Variable(typeof(TestClass), "Value");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(Int32) });
            Expressions.Add(Expr.Assign(Value, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");

            PropertyInfo pi = typeof(TestClass).GetProperty("X");
            Expressions.Add(Expr.PostIncrementAssign(Expr.Property(Value, pi), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Property(Value, pi), "PreIncrement 1"));

            var tree = Expr.Block(new[] { Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a methodinfo, pass a field to Expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 8", new string[] { "positive", "postincrementassign", "operators", "Pri1" })]
        public static Expr PostIncrementAssign8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Value = Expr.Variable(typeof(TestClass), "Value");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(Int32) });
            Expressions.Add(Expr.Assign(Value, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");

            FieldInfo fi = typeof(TestClass).GetField("m_x");
            Expressions.Add(Expr.PostIncrementAssign(Expr.Field(Value, fi), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Field(Value, fi), "PreIncrement 1"));

            var tree = Expr.Block(new[] { Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a methodinfo, pass a variable to expression
        // With a methodinfo, check the return value of the expression and the value of the variable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 9", new string[] { "positive", "postincrementassign", "operators", "Pri1" })]
        public static Expr PostIncrementAssign9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.Constant(1)));

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");

            Expr op = Expr.PostIncrementAssign(Result, mi);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), op, "PostIncrementAssign 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Result, "PostIncrementAssign 2"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, check the return value of the expression and the value of the variable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 10", new string[] { "positive", "postincrementassign", "operators", "Pri1" })]
        public static Expr PostIncrementAssign10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.Constant(1)));

            Expr op = Expr.PostIncrementAssign(Result, (MethodInfo)null);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), op, "PostIncrementAssign 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Result, "PostIncrementAssign 2"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a variable to expression
        // Check the return value of the expression and the value of the variable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 11", new string[] { "positive", "postincrementassign", "operators", "Pri1" })]
        public static Expr PostIncrementAssign11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.Constant(-1)));

            Expr op = Expr.PostIncrementAssign(Result);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), op, "PostIncrementAssign 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Result, "PostIncrementAssign 2"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 12", new string[] { "negative", "postincrementassign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr PostIncrementAssign12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            EU.Throws<System.ArgumentNullException>(() =>
            {
                Expressions.Add(Expr.PostIncrementAssign(null));
            });
            return Expr.Empty();
        }

        // PostIncrementAssign of a user defined type that defines the postincrementassign operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 13", new string[] { "positive", "postincrementassign", "operators", "Pri2" })]
        public static Expr PostIncrementAssign13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(TestClass), "Result");
            var ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(Result, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            PropertyInfo pi = typeof(TestClass).GetProperty("X");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Property(Expr.PostIncrementAssign(Result), pi), "PostDecrementAssign 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // PostIncrementAssign of string
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 14", new string[] { "negative", "postincrementassign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr PostIncrementAssign14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.PostIncrementAssign(Result); }));
            return Expr.Empty();
        }

        // PostIncrementAssign of date
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 15", new string[] { "negative", "postincrementassign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr PostIncrementAssign15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(DateTime), "Result");

            Expressions.Add(Expr.Assign(Result, Expr.Constant(DateTime.Parse("1/1/2009"))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.Constant(DateTime.Parse("1/2/2009")), Expr.PostIncrementAssign(Result), "PostIncrementAssign 1"); }));

            return Expr.Empty();
        }

        // Overflow an int with the postincrementassign operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 16", new string[] { "positive", "postincrementassign", "operators", "Pri2" })]
        public static Expr PostIncrementAssign16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.Constant(Int32.MaxValue)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(Int32.MaxValue), Expr.PostIncrementAssign(Result), "PostIncrementAssign 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(Int32.MinValue), Result, "PostIncrementAssign 2"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 17", new string[] { "negative", "postincrementassign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr PostIncrementAssign17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");
            EU.Throws<System.ArgumentNullException>(() =>
            {
                Expressions.Add(Expr.PostIncrementAssign(null, mi));
            });

            return Expr.Empty();
        }

        // With a null method, postincrementassign of numeric types
        // Regression for Dev10 Bug 546775
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 18", new string[] { "positive", "postincrementassign", "operators", "Pri2" })]
        public static Expr PostIncrementAssign18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression first = Expr.Variable(typeof(short), "first");
            Expressions.Add(Expr.Assign(first, Expr.Constant((short)(-1), typeof(short))));
            Expressions.Add(Expr.PostIncrementAssign(first, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)0, typeof(short)), first, "PostIncrementAssign 1"));

            ParameterExpression second = Expr.Variable(typeof(ushort), "second");
            Expressions.Add(Expr.Assign(second, Expr.Constant((ushort)1, typeof(ushort))));
            Expressions.Add(Expr.PostIncrementAssign(second, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ushort)2, typeof(ushort)), second, "PostIncrementAssign 2"));

            ParameterExpression third = Expr.Variable(typeof(Int16), "third");
            Expressions.Add(Expr.Assign(third, Expr.Constant((Int16)(-2), typeof(Int16))));
            Expressions.Add(Expr.PostIncrementAssign(third, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)(-1), typeof(Int16)), third, "PostIncrementAssign 3"));

            ParameterExpression fourth = Expr.Variable(typeof(UInt16), "fourth");
            Expressions.Add(Expr.Assign(fourth, Expr.Constant((UInt16)1, typeof(UInt16))));
            Expressions.Add(Expr.PostIncrementAssign(fourth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt16)2, typeof(UInt16)), fourth, "PostIncrementAssign 4"));

            ParameterExpression fifth = Expr.Variable(typeof(Int32), "fifth");
            Expressions.Add(Expr.Assign(fifth, Expr.Constant((Int32)Int32.MaxValue, typeof(Int32))));
            Expressions.Add(Expr.PostIncrementAssign(fifth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)Int32.MinValue, typeof(Int32)), fifth, "PostIncrementAssign 5"));

            ParameterExpression sixth = Expr.Variable(typeof(UInt32), "sixth");
            Expressions.Add(Expr.Assign(sixth, Expr.Constant((UInt32)1, typeof(UInt32))));
            Expressions.Add(Expr.PostIncrementAssign(sixth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt32)2, typeof(UInt32)), sixth, "PostIncrementAssign 6"));

            ParameterExpression seventh = Expr.Variable(typeof(double), "seventh");
            Expressions.Add(Expr.Assign(seventh, Expr.Constant((double)1.1, typeof(double))));
            Expressions.Add(Expr.PostIncrementAssign(seventh, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)2.1, typeof(double)), seventh, "PostIncrementAssign 7"));

            ParameterExpression eighth = Expr.Variable(typeof(long), "eighth");
            Expressions.Add(Expr.Assign(eighth, Expr.Constant((long)(-2.111), typeof(long))));
            Expressions.Add(Expr.PostIncrementAssign(eighth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)(-1.111), typeof(long)), eighth, "PostIncrementAssign 8"));

            ParameterExpression nineth = Expr.Variable(typeof(ulong), "nineth");
            Expressions.Add(Expr.Assign(nineth, Expr.Constant((ulong)1.999, typeof(ulong))));
            Expressions.Add(Expr.PostIncrementAssign(nineth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ulong)2.999, typeof(ulong)), nineth, "PostIncrementAssign 9"));

            ParameterExpression tenth = Expr.Variable(typeof(decimal), "tenth");
            Expressions.Add(Expr.Assign(tenth, Expr.Constant((decimal)1.001, typeof(decimal))));
            Expressions.Add(Expr.PostIncrementAssign(tenth, null));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((decimal)2.001, typeof(decimal)), tenth, "PostIncrementAssign 10"));

            var tree = Expr.Block(new[] { first, second, third, fourth, fifth, sixth, seventh, eighth, nineth, tenth }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a method that takes no arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 19", new string[] { "negative", "postincrementassign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr PostIncrementAssign19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Func<int> f = () => 1;
            MethodInfo mi = f.Method;

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(1), Expr.PostIncrementAssign(Result, mi), "PostIncrementAssign 1");
            }));

            return Expr.Block(new[] { Result }, Expressions);
        }

        public static int paramTestMethod(params int[] args) {
            return 3;
        }

        // Pass a method that takes a param array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 20", new string[] { "negative", "postincrementassign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr PostIncrementAssign20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            MethodInfo mi = typeof(PostIncrementAssign).GetMethod("paramTestMethod");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.PostIncrementAssign(Result, mi), "PostIncrementAssign 1"));
            });

            var tree =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Block(new[] { Result }, Expressions);
            });

            return tree;
        }

        // Pass a method that takes 2 arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 21", new string[] { "negative", "postincrementassign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr PostIncrementAssign21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Func<int, int, int> f = (x, y) => x + y;
            MethodInfo mi = f.Method;

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(5), Expr.PostIncrementAssign(Result, mi), "PostIncrementAssign 1");
            }));

            return Expr.Empty();
        }

        public static double typeTestMethod(int x) {
            return (double)x + 1.0;
        }

        // Pass a method that returns a different type from expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PostIncrementAssign 22", new string[] { "negative", "postincrementassign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr PostIncrementAssign22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            MethodInfo mi = typeof(PostIncrementAssign).GetMethod("typeTestMethod");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant((double)1.0, typeof(double)), Expr.PostIncrementAssign(Result, mi), "PostIncrementAssign 1");
            }));

            return Expr.Empty();
        }

    }
}
