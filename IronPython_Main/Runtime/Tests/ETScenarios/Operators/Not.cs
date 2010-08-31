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
    
    public class Not {
        // Not of sbyte, short, int, long
        // Regression for Dev10 Bug 533490
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 1", new string[] { "positive", "not", "operators", "Pri1" })]
        public static Expr Not1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant((sbyte)(-3), typeof(sbyte)), Expr.Not(Expr.Constant((sbyte)2, typeof(sbyte))), "Not 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)(-11), typeof(short)), Expr.Not(Expr.Constant((short)10, typeof(short))), "Not 3"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant((ushort)(ushort.MaxValue - 3), typeof(ushort)), Expr.Not(Expr.Constant((ushort)3)), "Not 4"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)(-5), typeof(Int32)), Expr.Not(Expr.Constant((Int32)4, typeof(Int32))), "Not 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt32)4294966295), Expr.Not(Expr.Constant((UInt32)1000, typeof(UInt32))), "Not 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)9, typeof(long)), Expr.Not(Expr.Constant((long)(-10.5), typeof(long))), "Not 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ulong)18446744073709551604, typeof(ulong)), Expr.Not(Expr.Constant((ulong)11.11, typeof(ulong))), "Not 8"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Not of single constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 2", new string[] { "negative", "not", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Not2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Not(Expr.Constant((Single)1, typeof(Single)));
            }));

            var tree = Expr.Block(Expressions);

            return tree;
        }

        // Not of Double constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 3", new string[] { "negative", "not", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Not3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Not(Expr.Constant((Double)1, typeof(Double)));
            }));

            var tree = Expr.Block(Expressions);

            return tree;
        }

        // Not of Decimal constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 3_1", new string[] { "negative", "not", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Not3_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Not(Expr.Constant((Decimal)1, typeof(Decimal)));
            }));

            var tree = Expr.Block(Expressions);

            return tree;
        }

        // Not of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 4", new string[] { "positive", "not", "operators", "Pri1" })]
        public static Expr Not4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Not(Expr.Constant(false))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), Expr.Not(Expr.Constant(true))));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Not of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 5", new string[] { "negative", "not", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Not5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Not(Expr.Constant("Test")); }));

            return Expr.Empty();
        }

        // Not of class object, no user defined operator
        public class MyType {
            public int Data { get; set; }
            public MyType(int x) { Data = x; }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 6", new string[] { "negative", "not", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Not6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Not(Expr.Constant(new MyType(2))); }));
            return Expr.Empty();
        }

        // Not of structure object, no user defined operator
        public struct MyStruct {
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 7", new string[] { "negative", "not", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Not7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.Not(Expr.Constant(new MyStruct())); }));
            return Expr.Empty();
        }

        // Pass null to method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 8", new string[] { "positive", "not", "operators", "Pri1" })]
        public static Expr Not8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Expr.Not(Expr.Constant(-1), null)));
            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a methodinfo that takes no arguments
        public static int NotNoArgs() {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 9", new string[] { "negative", "not", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Not9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Not).GetMethod("NotNoArgs");
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.Not(Expr.Constant(1), mi)));
            });

            return Expr.Empty();
        }

        // Pass a methodinfo that takes a paramarray
        public static int NotParams(params int[] x) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 10", new string[] { "negative", "not", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Not10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Not).GetMethod("NotParams");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.Not(Expr.Constant(1), mi)));
            });

            var tree =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Block(Expressions);
            });

            return tree;
        }

        // With a valid method info, pass a value that widen to the argument of the method
        public static int validNot(double x) {
            return (Int32)(-(x * x));
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 11", new string[] { "negative", "not", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Not11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Not).GetMethod("validNot");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-4), Expr.Not(Expr.Constant((Int32)2, typeof(Int32)), mi)));
            });

            var tree =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Block(Expressions);
            });

            return tree;
        }

        // With a valid method info, pass a value of a derived class of the methodinfo's argument
        public class MyDerivedType : MyType {
            public MyDerivedType(int x) : base(x) { }
        }
        public static int NotBase(MyType x) {
            return -(x.Data * x.Data);
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 12", new string[] { "positive", "not", "operators", "Pri1" })]
        public static Expr Not12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Not).GetMethod("NotBase");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(-4),
                    Expr.Not(Expr.Constant(new MyDerivedType(2)), mi),
                    "Not 1"
                )
            );

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo that returns boolean, pass an argument of nullable type
        public static bool NotNullableTest(int x) {
            return (x > 0) ? true : false;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 13", new string[] { "positive", "not", "operators", "Pri1" })]
        public static Expr Not13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Not).GetMethod("NotNullableTest");
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(true, typeof(Nullable<bool>)),
                    Expr.Not(
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        mi
                    )
                )
            );

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // User defined overloaded operator on expression argument
        public class A {
            public bool Data { get; set; }
            public A(bool x) { Data = x; }
            public static A operator !(A a) {
                return new A(a.Data);
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Not 14", new string[] { "positive", "not", "operators", "Pri1" })]
        public static Expr Not14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(new A(false)),
                    Expr.Not(Expr.Constant(new A(false))),
                    "Not 1"
                )
            );

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
