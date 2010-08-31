#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Operators {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Negate {
        // Negate of sbyte, short, int, long
        // Negate of single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 1", new string[] { "positive", "negate", "operators", "Pri1" })]
        public static Expr Negate1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            //Expressions.Add(EU.GenAreEqual(Expr.Constant((sbyte)(-1), typeof(sbyte)), Expr.Negate(Expr.Constant((sbyte)1, typeof(sbyte))), "Negate 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)(-1), typeof(short)), Expr.Negate(Expr.Constant((short)1, typeof(short))), "Negate 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)(-1), typeof(Int16)), Expr.Negate(Expr.Constant((Int16)1, typeof(Int16))), "Negate 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)(-1), typeof(Int32)), Expr.Negate(Expr.Constant((Int32)1, typeof(Int32))), "Negate 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)(-1), typeof(long)), Expr.Negate(Expr.Constant((long)1, typeof(long))), "Negate 5"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant((Single)(-1), typeof(Single)), Expr.Negate(Expr.Constant((Single)1, typeof(Single))), "Negate 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Double)(-1), typeof(Double)), Expr.Negate(Expr.Constant((Double)1, typeof(Double))), "Negate 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Decimal)(-1), typeof(Decimal)), Expr.Negate(Expr.Constant((Decimal)1, typeof(Decimal))), "Negate 8"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Negate of byte
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 2", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Negate(Expr.Constant((byte)1, typeof(byte))); }));
            return Expr.Empty();
        }

        // Negate of sbyte
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 3", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.Constant((sbyte)(-1), typeof(sbyte)), Expr.Negate(Expr.Constant((sbyte)1, typeof(sbyte))), "Negate 1"); }));
            return Expr.Empty();
        }

        // Negate of ushort
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 4", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Negate(Expr.Constant((ushort)1, typeof(ushort))); }));
            return Expr.Empty();
        }

        // Negate of UInt16
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 5", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Negate(Expr.Constant((UInt16)1, typeof(UInt16))); }));
            return Expr.Empty();
        }

        // Negate of UInt32
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 6", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Negate(Expr.Constant((UInt32)1, typeof(UInt32))); }));
            return Expr.Empty();
        }

        // Negate of ulong
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 7", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Negate(Expr.Constant((ulong)1, typeof(ulong))); }));
            return Expr.Empty();
        }

        // Negate of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 8", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Negate(Expr.Constant(true)); }));
            return Expr.Empty();
        }

        // Negate of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 9", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Negate(Expr.Constant("Test")); }));
            return Expr.Empty();
        }

        // Negate of class object, no user defined operator
        public class MyType {
            public int Data { get; set; }
            public MyType(int x) { Data = x; }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 10", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Negate(Expr.Constant(new MyType(2))); }));
            return Expr.Empty();
        }

        // Negate of structure object, no user defined operator
        public struct MyStruct {
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 11", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Negate(Expr.Constant(new MyStruct())); }));
            return Expr.Empty();
        }

        // Pass null to method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 12", new string[] { "positive", "negate", "operators", "Pri1" })]
        public static Expr Negate12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.Negate(Expr.Constant(1), null)));
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a methodinfo that takes no arguments
        public static int negateNoArgs() {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 13", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Negate13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Negate).GetMethod("negateNoArgs");
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.Negate(Expr.Constant(1), mi)));
            });

            return Expr.Empty();
        }

        // Pass a methodinfo that takes a paramarray
        public static int negateParams(params int[] x) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 14", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Negate).GetMethod("negateParams");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.Negate(Expr.Constant(1), mi)));
            });

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass a value that widen to the argument of the method
        public static int validNegate(double x) {
            return (Int32)(-(x * x));
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 15", new string[] { "negative", "negate", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Negate15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Negate).GetMethod("validNegate");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-4), Expr.Negate(Expr.Constant((Int32)2, typeof(Int32)), mi)));
            });

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass a value of a derived class of the methodinfo's argument
        public class MyDerivedType : MyType {
            public MyDerivedType(int x) : base(x) { }
        }
        public static int negateBase(MyType x) {
            return -(x.Data * x.Data);
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 16", new string[] { "positive", "negate", "operators", "Pri1" })]
        public static Expr Negate16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Negate).GetMethod("negateBase");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(-4),
                    Expr.Negate(Expr.Constant(new MyDerivedType(2)), mi),
                    "Negate 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo that returns boolean, pass an argument of nullable type
        public static bool negateNullableTest(int x) {
            return (x > 0) ? true : false;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 17", new string[] { "positive", "negate", "operators", "Pri1" })]
        public static Expr Negate17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Negate).GetMethod("negateNullableTest");
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(true, typeof(Nullable<bool>)),
                    Expr.Negate(
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        mi
                    )
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // User defined overloaded operator on expression argument
        public class A {
            public int Data { get; set; }
            public A(int x) { Data = x; }
            public static A operator -(A a) {
                return new A(-(a.Data * a.Data));
            }
            public static bool operator ==(A a, A b) {
                return a.Data == b.Data;
            }
            public static bool operator !=(A a, A b) {
                return a.Data != b.Data;
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 18", new string[] { "positive", "negate", "operators", "Pri1" })]
        public static Expr Negate18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(new A(-16)),
                    Expr.Negate(Expr.Constant(new A(4))),
                    "Negate 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Overflow type (by negative type.minValue)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate 19", new string[] { "positive", "negate", "operators", "Pri1" })]
        public static Expr Negate19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(Int32.MinValue),
                    Expr.Negate(Expr.Constant(Int32.MinValue)),
                    "Negate 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

    }
}
