#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.ConvertCallInvoke {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Call {
        // Pass non null to instance, null to methodinfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 1", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Call1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<ArgumentNullException>(() => { Expr.Call(Expr.Constant("TestString"), null); }));
            return Expr.Empty();
        }

        // Pass methodinfo to method that doesn't belong to the object passed to instance
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 2", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Call2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Array).GetMethod("GetLength");
            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Call(Expr.Constant("TestString"), mi); }));

            return Expr.Empty();
        }

        // Pass a non static method MethodInfo with one argument to method, one argument (the class) to arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 3", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Call3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(System.Text.StringBuilder).GetMethod("EnsureCapacity");
            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Call(mi, new Expression[] { Expr.Constant(new System.Text.StringBuilder()) }); }));

            return Expr.Empty();
        }

        // Pass a non static method MethodInfo with zero arguments to method, one argument (the class) to arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 3_1", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Call3_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(string).GetMethod("ToUpperInvariant");
            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Call(mi, new Expression[] { Expr.Constant("Test") }); }));

            return Expr.Empty();
        }

        // Pass a non static method MethodInfo with one argument to method, the class and the argument to arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 4", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Call4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(System.Text.StringBuilder).GetMethod("EnsureCapacity");
            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Call(mi, new Expression[] { Expr.Constant(new System.Text.StringBuilder()), Expr.Constant(1) }); }));

            return Expr.Empty();
        }

        // Pass a non static method MethodInfo with one argument to method, one null element and a valid argument to arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 5", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Call5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(System.Text.StringBuilder).GetMethod("EnsureCapacity");
            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Call(mi, new Expression[] { Expr.Constant(null), Expr.Constant(1) }); }));

            return Expr.Empty();
        }

        // Pass null to instance, null to MethodInfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 6", new string[] { "negative", "call", "callconvertinvoke", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Call6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<ArgumentNullException>(() => { Expr.Call(null, null, new Expression[] { Expr.Constant(1) }); }));
            return Expr.Empty();
        }

        // Pass null to instance, null to MethodInfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 7", new string[] { "negative", "call", "callconvertinvoke", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Call7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            List<Expression> args = new List<Expression>();
            args.Add(Expr.Constant(1));
            MethodInfo method = null;
            Expressions.Add(EU.Throws<ArgumentNullException>(() => { Expr.Call(method, args.AsReadOnly()); }));
            return Expr.Empty();
        }


        // Pass null to type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 11", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Call11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<ArgumentNullException>(() => { Expr.Call((Type)null, "GetType", new Type[] { typeof(string) }, new Expr[] { Expr.Constant("") }); }));
            return Expr.Empty();
        }

        // Pass more arguments than there are arguments in the method. (but pass the right number of type arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 12", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Call12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(
                EU.Throws<InvalidOperationException>(() =>
                {
                    Expr.Call(typeof(string), "Compare", new Type[] { typeof(string), typeof(string) }, new Expression[] { Expr.Constant("one"), Expr.Constant("two"), Expr.Constant("three") });
                }));

            return EU.BlockVoid(Expressions);
        }

        // Pass arguments that have conversions to the type they are supposed to
        public class A {
            public int Data { get; set; }
            public static int Test(A a) {
                return a.Data * a.Data;
            }
        }

        public class B : A {
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 13", new string[] { "positive", "call", "convertcallinvoke", "Pri1" })]
        public static Expr Call13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(int), "");

            Expressions.Add(Expr.Assign(Result, Expr.Call(typeof(A), "Test", new Type[] { }, new Expr[] { Expr.Constant(new B() { Data = 2 }) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(4), Result));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 13_1", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Call13_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expressions.Add(EU.Throws<InvalidOperationException>(() => { Expr.Assign(Result, Expr.Call(typeof(double), "IsNaN", new Type[] { }, new Expr[] { Expr.Constant(1) })); }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("False"), Result));

            return EU.BlockVoid(new[] { Result }, Expressions);
        }

        // Pass an array with null elements to arguments
        public static string TestMethod(string a, string b, string c) {
            if (a == null || b == null || c == null)
                return "NullArg";
            else
                return "NoNull";
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 14", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Call14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expressions.Add(EU.Throws<InvalidOperationException>(() =>
            {
                Expr.Assign(Result, Expr.Call(typeof(Call), "TestMethod", new Type[] { }, new Expr[] { Expr.Constant("1"), Expr.Constant(null), Expr.Constant("3") }));
            }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("NullArg"), Result));

            return EU.BlockVoid(new[] { Result }, Expressions);
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 14_1", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Call14_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Assign(Result, Expr.Call(typeof(Call), "TestMethod", new Type[] { }, new Expr[] { Expr.Constant("1"), null, Expr.Constant("3") }));
            }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("NullArg"), Result));

            return EU.BlockVoid(new[] { Result }, Expressions);
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 14_2", new string[] { "positive", "call", "convertcallinvoke", "Pri1" })]
        public static Expr Call14_2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expressions.Add(Expr.Assign(Result, Expr.Call(typeof(Call), "TestMethod", new Type[] { }, new Expr[] { Expr.Constant("1"), Expr.Constant(null, typeof(string)), Expr.Constant("3") })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("NullArg"), Result));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass methodname for an instance method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 15", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Call15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() =>
            {
                Expr.Call(typeof(string), "CompareTo", new Type[] { typeof(string), typeof(string) }, new Expr[] { Expr.Constant("one"), Expr.Constant("two") });
            }));

            return EU.BlockVoid(Expressions);
        }

        // Call extension methods of built in and user defined types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 16", new string[] { "positive", "call", "convertcallinvoke", "Pri2" })]
        public static Expr Call16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Result2 = Expr.Variable(typeof(MyType), "");

            Expressions.Add(Expr.Assign(Result, Expr.Constant("One")));
            Expressions.Add(Expr.Assign(Result, Expr.Call(typeof(TestUtil), "ExtensionTest", new Type[] { }, new Expression[] { Result })));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("OneTest"), "Call 1"));

            // try extension method on user defined type
            PropertyInfo pi = typeof(MyType).GetProperty("Data");
            Expressions.Add(Expr.Assign(Result2, Expr.Constant(new MyType("One"))));
            Expressions.Add(Expr.Assign(Result2, Expr.Call(typeof(TestUtil), "UserExtensionTest", new Type[] { }, new Expression[] { Result2 })));
            Expressions.Add(EU.GenAreEqual(Expr.Property(Result2, pi), Expr.Constant("OneTest"), "Call 2"));

            var tree = EU.BlockVoid(new[] { Result, Result2 }, Expressions); ;
            V.Validate(tree);
            return tree;
        }

        // Call base class method on derived types which override it
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 17", new string[] { "positive", "call", "convertcallinvoke", "Pri1" })]
        public static Expr Call17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex = Expr.Variable(typeof(MyException), "");

            MethodInfo mi = typeof(MyException).GetMethod("TestMethod");

            // call virtual method on base class reference which points to an instance of the base class
            Expressions.Add(Expr.Assign(Ex, Expr.Constant(new MyException("Test"))));
            Expressions.Add(Expr.Assign(Result, Expr.Call(Ex, mi)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("BaseTestMethod"), "Call 1"));

            // call overridden virtual method on base class reference which points to an instance of the derived class
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Ex, Expr.Constant(new MyDerivedException())));
            Expressions.Add(Expr.Assign(Result, Expr.Call(Ex, mi)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("DerivedTestMethod"), "Call 2"));

            var tree = EU.BlockVoid(new[] { Result, Ex }, Expressions); ;
            V.Validate(tree);
            return tree;
        }

        // Call hidden methods of derived types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 18", new string[] { "positive", "call", "convertcallinvoke", "Pri1" })]
        public static Expr Call18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex = Expr.Variable(typeof(MyException), "");
            ParameterExpression Ex2 = Expr.Variable(typeof(MyDerivedException), "");

            MethodInfo mi = typeof(MyException).GetMethod("HiddenMethod");

            // call hidden method on reference to base class object that points to an instance of derived class
            Expressions.Add(Expr.Assign(Ex, Expr.Constant(new MyDerivedException())));
            Expressions.Add(Expr.Assign(Result, Expr.Call(Ex, mi)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("BaseClass"), "Call 1"));

            // call hidden method on reference to base class object that points to an instance of the base class
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Ex, Expr.Constant(new MyException(""))));
            Expressions.Add(Expr.Assign(Result, Expr.Call(Ex, mi)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("BaseClass"), "Call 2"));

            // call hidden method on reference to derived class object that points to an instance of the derived class
            // in C# this would call the hidden method but because we're using an explicit MethodInfo of MyException it calls the base class version
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Ex2, Expr.Constant(new MyDerivedException())));
            Expressions.Add(Expr.Assign(Result, Expr.Call(Ex2, mi)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("BaseClass"), "Call 3"));

            // call hidden method on reference to derived class object that points to an instance of the derived class using explicit MethodInfo
            mi = typeof(MyDerivedException).GetMethod("HiddenMethod");

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Ex2, Expr.Constant(new MyDerivedException())));
            Expressions.Add(Expr.Assign(Result, Expr.Call(Ex2, mi)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("DerivedClass"), "Call 4"));

            var tree = EU.BlockVoid(new[] { Result, Ex, Ex2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass an instance, a valid MethodInfo and mismatched arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 19", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Call19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MyType).GetMethod("TestMethod");
            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Call(Expr.Constant(new MyType("Instance")), mi, new Expression[] { Expr.Constant(1) }); }));

            return Expr.Empty();
        }

        // Call methods with ref and out parameters
        public static int RefAndOutTest(ref int x, out string y) {
            x = x * x;
            y = x.ToString();
            return x;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 20", new string[] { "positive", "call", "convertcallinvoke", "Pri1" })]
        public static Expr Call20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "");
            ParameterExpression Arg1 = Expr.Variable(typeof(int), "");
            ParameterExpression Arg2 = Expr.Variable(typeof(string), "");

            Expressions.Add(Expr.Assign(Arg1, Expr.Constant(2)));

            Expressions.Add(
                Expr.Assign(
                    Result,
                    Expr.Call(typeof(Call), "RefAndOutTest", new Type[] { }, new Expr[] { Expr.Constant(2), Arg2 })
                )
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(4), Result, "Call 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("4"), Arg2));

            var tree = EU.BlockVoid(new[] { Result, Arg1, Arg2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Call methods of interface, derived interface
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 21", new string[] { "positive", "call", "convertcallinvoke", "Pri1" })]
        public static Expr Call21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression t1 = Expr.Variable(typeof(MyType), "");
            ParameterExpression t2 = Expr.Variable(typeof(MyOtherType), "");
            ParameterExpression Result = Expr.Variable(typeof(int), "");

            MethodInfo mi = typeof(IMyInterface).GetMethod("MyInterfaceMethod");
            MethodInfo mi2 = typeof(IMyDerivedInterface).GetMethod("MyDerivedInterfaceMethod");

            Expressions.Add(Expr.Assign(t1, Expr.Constant(new MyType("t1"))));
            Expressions.Add(Expr.Assign(t2, Expr.Constant(new MyOtherType("t2"))));

            // call interface method on type that implements it
            Expressions.Add(Expr.Assign(Result, Expr.Call(t1, mi, new Expression[] { Expr.Constant(3) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(9), Result, "Call 1"));

            // call interface method on type that implements a derived interface of that type
            Expressions.Add(Expr.Assign(Result, Expr.Call(t2, mi, new Expression[] { Expr.Constant(3) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(27), Result, "Call 2"));

            // call derived interface method on type that implements that interface
            Expressions.Add(Expr.Assign(Result, Expr.Call(t2, mi2, new Expression[] { Expr.Constant(3) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(81), Result, "Call 3"));

            var tree = EU.BlockVoid(new[] { Result, t1, t2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Call methods of interface, derived interface (negative cases)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 21_1", new string[] { "negative", "call", "convertcallinvoke", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Call21_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression t1 = Expr.Variable(typeof(MyType), "");
            ParameterExpression t2 = Expr.Variable(typeof(MyOtherType), "");
            ParameterExpression Result = Expr.Variable(typeof(int), "");

            MethodInfo mi = typeof(IMyInterface).GetMethod("MyInterfaceMethod");
            MethodInfo mi2 = typeof(IMyDerivedInterface).GetMethod("MyDerivedInterfaceMethod");

            Expressions.Add(Expr.Assign(t1, Expr.Constant(new MyType("t1"))));
            Expressions.Add(Expr.Assign(t2, Expr.Constant(new MyOtherType("t2"))));

            // call derived interface method on type which only implements the base interface
            Expressions.Add(Expr.Assign(Result, EU.Throws<ArgumentException>(() => { Expr.Call(t1, mi2, new Expression[] { Expr.Constant(3) }); })));

            return Expr.Empty();
        }


        // methods with two ref arguments of the same type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 22", new string[] { "positive", "call", "convertcallinvoke", "Pri1" })]
        public static Expr Call22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var Left = Expr.Parameter(typeof(int), "");
            var Right = Expr.Parameter(typeof(int), "");

            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));
            Expressions.Add(Expr.Assign(Right, Expr.Constant(2)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Call(null, typeof(Call).GetMethod("Call22_method"), Left, Right), "Call 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), Left, "Call 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Right, "Call 3"));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static int Call22_method(ref int arg1, ref int arg2) {
            var ret = arg1 + arg2;
            arg1 = 5;
            arg2 = 6;
            return ret;
        }

        // Test     : Expression.Call factory error message lacks detail
        // Expected : ArgumentException with better exception feedback.
        // Notes    : Regession for bug 522953 see bug for comments on expanded comments for this neg test.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Call 23", new string[] { "negative", "call", "convertcallinvoke", "Pri1", "regression" }, Exception = typeof(ArgumentException))]
        public static Expr Call23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(System.Text.StringBuilder).GetMethod("EnsureCapacity");

            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Call(mi, new Expression[] { Expr.Constant(new System.Text.StringBuilder()), Expr.Constant(1) }); }));

            LambdaExpression LM = Expr.Lambda(Expr.Block(Expr.Block(Expressions)));

            LM.Compile().DynamicInvoke();

            return Expr.Empty();
        }

    }

    // for testing user defined extension method in Call16()
    public class MyException : Exception {
        public MyException(string s) { }
        public virtual string TestMethod() {
            return "BaseTestMethod";
        }
        public virtual string HiddenMethod() {
            return "BaseClass";
        }
    }

    public class MyDerivedException : MyException {
        public MyDerivedException() : base("Test") { }
        public override string TestMethod() {
            return "DerivedTestMethod";
        }
        public new string HiddenMethod() {
            return "DerivedClass";
        }
    }

    // for testing extension methods on user defined type and calls from interfaces
    public interface IMyInterface {
        int MyInterfaceMethod(int x);
    }
    public interface IMyDerivedInterface : IMyInterface {
        int MyDerivedInterfaceMethod(int x);
    }

    public class MyType : IMyInterface {
        public string Data { get; set; }
        public MyType(string s) {
            Data = s;
        }
        public void TestMethod(string s) {
            Data = s;
            return;
        }
        public int MyInterfaceMethod(int x) {
            return x * x;
        }
    }

    public class MyOtherType : IMyDerivedInterface {
        public string Data { get; set; }
        public MyOtherType(string s) {
            Data = s;
        }
        public int MyInterfaceMethod(int x) {
            return x * x * x;
        }
        public int MyDerivedInterfaceMethod(int x) {
            return x * x * x * x;
        }
    }

    // for testing extension methods in Call16()
    public static class TestUtil {
        public static string ExtensionTest(this string x) {
            return x + "Test";
        }

        public static MyType UserExtensionTest(this MyType x) {
            return new MyType(x.Data + "Test") { };
        }
    }
}
