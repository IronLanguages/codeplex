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

    public class OnesComplement {
        // OnesComplement of sbyte, short, int, long
        // OnesComplement of single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 1", new string[] { "positive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            //Expressions.Add(EU.GenAreEqual(Expr.Constant((sbyte)(-1), typeof(sbyte)), Expr.OnesComplement(Expr.Constant((sbyte)1, typeof(sbyte))), "OnesComplement 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)(-2), typeof(short)), Expr.OnesComplement(Expr.Constant((short)1, typeof(short))), "OnesComplement 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)(-2), typeof(Int16)), Expr.OnesComplement(Expr.Constant((Int16)1, typeof(Int16))), "OnesComplement 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)(-2), typeof(Int32)), Expr.OnesComplement(Expr.Constant((Int32)1, typeof(Int32))), "OnesComplement 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)(-2), typeof(long)), Expr.OnesComplement(Expr.Constant((long)1, typeof(long))), "OnesComplement 5"));

            /*Expressions.Add(EU.GenAreEqual(Expr.Constant((Single)(-2), typeof(Single)), Expr.OnesComplement(Expr.Constant((Single)1, typeof(Single))), "OnesComplement 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Double)(-2), typeof(Double)), Expr.OnesComplement(Expr.Constant((Double)1, typeof(Double))), "OnesComplement 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Decimal)(-2), typeof(Decimal)), Expr.OnesComplement(Expr.Constant((Decimal)1, typeof(Decimal))), "OnesComplement 8"));*/

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // OnesComplement of byte
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 2", new string[] { "positive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant((byte)(byte.MaxValue - 1)), Expr.OnesComplement(Expr.Constant((byte)1, typeof(byte)))));
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // OnesComplement of sbyte
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 3", new string[] { "positive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant((sbyte)(-2), typeof(sbyte)), Expr.OnesComplement(Expr.Constant((sbyte)1, typeof(sbyte))), "OnesComplement 1"));
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // OnesComplement of ushort
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 4", new string[] { "positive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ushort)(ushort.MaxValue - 1)), Expr.OnesComplement(Expr.Constant((ushort)1, typeof(ushort)))));
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // OnesComplement of UInt16
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 5", new string[] { "positive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt16)(UInt16.MaxValue - 1)), Expr.OnesComplement(Expr.Constant((UInt16)1, typeof(UInt16)))));
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // OnesComplement of UInt32
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 6", new string[] { "ositive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant((UInt32)(UInt32.MaxValue - 1)), Expr.OnesComplement(Expr.Constant((UInt32)1, typeof(UInt32)))));
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // OnesComplement of ulong
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 7", new string[] { "positive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant((ulong)(ulong.MaxValue - 1)), Expr.OnesComplement(Expr.Constant((ulong)1, typeof(ulong)))));
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // OnesComplement of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 8", new string[] { "negative", "OnesComplement", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OnesComplement8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.OnesComplement(Expr.Constant(true)); }));
            return Expr.Empty();
        }

        // OnesComplement of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 9", new string[] { "negative", "OnesComplement", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OnesComplement9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.OnesComplement(Expr.Constant("Test")); }));
            return Expr.Empty();
        }

        // OnesComplement of class object, no user defined operator
        public class MyType {
            public int Data { get; set; }
            public MyType(int x) { Data = x; }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 10", new string[] { "negative", "OnesComplement", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OnesComplement10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.OnesComplement(Expr.Constant(new MyType(2))); }));
            return Expr.Empty();
        }

        // OnesComplement of structure object, no user defined operator
        public struct MyStruct {
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 11", new string[] { "negative", "OnesComplement", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OnesComplement11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.OnesComplement(Expr.Constant(new MyStruct())); }));
            return Expr.Empty();
        }

        // Pass null to method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 12", new string[] { "positive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-2), Expr.OnesComplement(Expr.Constant(1), null)));
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a methodinfo that takes no arguments
        public static int OnesComplementNoArgs() {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 13", new string[] { "negative", "OnesComplement", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OnesComplement13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OnesComplement).GetMethod("OnesComplementNoArgs");
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.OnesComplement(Expr.Constant(1), mi)));
            });

            return Expr.Empty();
        }

        // Pass a methodinfo that takes a paramarray
        public static int OnesComplementParams(params int[] x) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 14", new string[] { "negative", "OnesComplement", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OnesComplement14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OnesComplement).GetMethod("OnesComplementParams");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.OnesComplement(Expr.Constant(1), mi)));
            });

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass a value that widen to the argument of the method
        public static int validOnesComplement(double x) {
            return (Int32)(-(x * x));
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 15", new string[] { "negative", "OnesComplement", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OnesComplement15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OnesComplement).GetMethod("validOnesComplement");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-4), Expr.OnesComplement(Expr.Constant((Int32)2, typeof(Int32)), mi)));
            });

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass a value of a derived class of the methodinfo's argument
        public class MyDerivedType : MyType {
            public MyDerivedType(int x) : base(x) { }
        }
        public static int OnesComplementBase(MyType x) {
            return -(x.Data * x.Data);
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 16", new string[] { "positive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OnesComplement).GetMethod("OnesComplementBase");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(-4),
                    Expr.OnesComplement(Expr.Constant(new MyDerivedType(2)), mi),
                    "OnesComplement 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo that returns boolean, pass an argument of nullable type
        public static bool OnesComplementNullableTest(int x) {
            return (x > 0) ? true : false;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 17", new string[] { "positive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OnesComplement).GetMethod("OnesComplementNullableTest");
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(true, typeof(Nullable<bool>)),
                    Expr.OnesComplement(
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        mi
                    )
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Overflow type (by negative type.minValue)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OnesComplement 19", new string[] { "positive", "OnesComplement", "operators", "Pri1" })]
        public static Expr OnesComplement19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(Int32.MaxValue),
                    Expr.OnesComplement(Expr.Constant(Int32.MinValue)),
                    "OnesComplement 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

    }
}
