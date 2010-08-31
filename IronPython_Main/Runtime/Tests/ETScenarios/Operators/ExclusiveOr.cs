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

    public class ExclusiveOr {
        // ExclusiveOr of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 1", new string[] { "positive", "exclusiveor", "operators", "Pri1" })]
        public static Expr ExclusiveOr1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((Int16)0x00), Expr.Constant((Int16)0x01)), Expr.Constant((Int16)0x01), "ExclusiveOr 1"));
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((UInt16)0x01), Expr.Constant((UInt16)0x10)), Expr.Constant((UInt16)0x11), "ExclusiveOr 2"));
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((short)0x10), Expr.Constant((short)0x10)), Expr.Constant((short)0x00), "ExclusiveOr 3"));
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((ushort)0x01), Expr.Constant((ushort)0x01)), Expr.Constant((ushort)0x00), "ExclusiveOr 4"));
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((Int32)0x0001), Expr.Constant((Int32)0x0010)), Expr.Constant((Int32)0x0011), "ExclusiveOr 5"));
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((UInt32)0x0110), Expr.Constant((UInt32)0x0101)), Expr.Constant((UInt32)0x0011), "ExclusiveOr 6"));
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((long)0x1111), Expr.Constant((long)0x0011)), Expr.Constant((long)0x1100), "ExclusiveOr 7"));
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((ulong)0x1100), Expr.Constant((ulong)0x1010)), Expr.Constant((ulong)0x0110), "ExclusiveOr 8"));
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((SByte)0x01), Expr.Constant((SByte)0x10)), Expr.Constant((SByte)0x11), "ExclusiveOr 9")); // Int16 is CLS compliant equivalent type
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((Byte)0x10), Expr.Constant((Byte)0x11)), Expr.Constant((Byte)0x01), "ExclusiveOr 10"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // ExclusiveOr of Single constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 2", new string[] { "negative", "exclusiveor", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ExclusiveOr2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((Single)0x1100), Expr.Constant((Single)0x1010)), Expr.Constant((Single)0x0110), "ExclusiveOr 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // ExclusiveOr of Double constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 3", new string[] { "negative", "exclusiveor", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ExclusiveOr3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((double)0x1100), Expr.Constant((double)0x1010)), Expr.Constant((double)0x0110), "ExclusiveOr 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // ExclusiveOr of Decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 4", new string[] { "negative", "exclusiveor", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ExclusiveOr4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant((decimal)0x1100), Expr.Constant((decimal)0x1010)), Expr.Constant((decimal)0x0110), "ExclusiveOr 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // ExclusiveOr of boolean constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 5", new string[] { "positive", "exclusiveor", "operators", "Pri1" })]
        public static Expr ExclusiveOr5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant(true), Expr.Constant(false)), Expr.Constant(true), "ExclusiveOr 1"));
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant(false), Expr.Constant(false)), Expr.Constant(false), "ExclusiveOr 2"));
            Expressions.Add(EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant(true), Expr.Constant(true)), Expr.Constant(false), "ExclusiveOr 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // ExclusiveOr of string constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 6", new string[] { "negative", "exclusiveor", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ExclusiveOr6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant("Hello"), Expr.Constant("World")), Expr.Constant("HelloWorld"), "ExclusiveOr 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // ExclusiveOr of class object, no user defined operator
        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 7", new string[] { "negative", "exclusiveor", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ExclusiveOr7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant(new TestClass(1)), Expr.Constant(new TestClass(2))), Expr.Constant(new TestClass(-1)), "ExclusiveOr 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // ExclusiveOr of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 8", new string[] { "negative", "exclusiveor", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ExclusiveOr8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.ExclusiveOr(Expr.Constant(new TestStruct(1)), Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "ExclusiveOr 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 9", new string[] { "positive", "exclusiveor", "operators", "Pri1" })]
        public static Expr ExclusiveOr9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ExclusiveOr(Expr.Constant((Int32)0x1101), Expr.Constant((Int32)0x1011), null),
                    Expr.Constant((Int32)0x0110),
                    "ExclusiveOr 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a methodinfo that takes no arguments
        public static int ExclusiveOrNoArgs() {
            int x = 1;
            return x;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 10", new string[] { "negative", "exclusiveor", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ExclusiveOr10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ExclusiveOr).GetMethod("ExclusiveOrNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {

                EU.GenAreEqual(
                    Expr.ExclusiveOr(Expr.Constant(0x1001), Expr.Constant(0x0001), mi),
                    Expr.Constant(0x1000),
                    "ExclusiveOr 1"
                )
        ;
            }));

            return EU.BlockVoid(Expressions);
        }

        // Pass a methodinfo that takes a paramarray
        public static int ExclusiveOrParamArray(params int[] args) {
            if (args == null)
                return -1;
            return 0;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 11", new string[] { "negative", "exclusiveor", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ExclusiveOr11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ExclusiveOr).GetMethod("ExclusiveOrParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {

                EU.GenAreEqual(
                    Expr.ExclusiveOr(Expr.Constant(0x0001), Expr.Constant(0x0001), mi),
                    Expr.Constant(0x0001),
                    "ExclusiveOr 1"
                )
        ;
            }));

            return EU.BlockVoid(Expressions);
        }

        // With a valid method info, exclusiveor two values of the same type
        public static int ExclusiveOrTwoArgs(int x, int y) {
            return x & y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 12", new string[] { "positive", "exclusiveor", "operators", "Pri1" })]
        public static Expr ExclusiveOr12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ExclusiveOr).GetMethod("ExclusiveOrTwoArgs");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ExclusiveOr(Expr.Constant(0x1111), Expr.Constant(0x0111), mi),
                    Expr.Constant(0x0111),
                    "ExclusiveOr 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 13", new string[] { "negative", "exclusiveor", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ExclusiveOr13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ExclusiveOr).GetMethod("ExclusiveOrTwoArgs");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.ExclusiveOr(Expr.Constant((Int16)(0x1111)), Expr.Constant((Int16)(0x0111)), mi),
                    Expr.Constant(0x1000),
                    "ExclusiveOr 1"
                )
        ;
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static string ExclusiveOrExceptionMsg(Exception e1, Exception e2) {
            return e1.Message + e2.Message;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 14", new string[] { "positive", "exclusiveor", "operators", "Pri1" })]
        public static Expr ExclusiveOr14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ExclusiveOr).GetMethod("ExclusiveOrExceptionMsg");

            Expr Res =
                Expr.ExclusiveOr(
                    Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException)),
                    Expr.Constant(new RankException("Two"), typeof(RankException)),
                    mi
                );

            Expressions.Add(
                EU.GenAreEqual(
                    Res,
                    Expr.Constant("OneTwo")
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type. 
        public static int ExclusiveOrNullableInt(int? x, int y) {
            return (x ?? 0) ^ y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 15", new string[] { "positive", "exclusiveor", "operators", "Pri1" })]
        public static Expr ExclusiveOr15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ExclusiveOr).GetMethod("ExclusiveOrNullableInt");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ExclusiveOr(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(3, typeof(Int32)), "ExclusiveOr 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ExclusiveOr(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(1), "ExclusiveOr 2"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 16", new string[] { "negative", "exclusiveor", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ExclusiveOr16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.ExclusiveOr(Expr.Constant(0x0011, typeof(Int32)), Expr.Constant((Nullable<int>)0x1111, typeof(Nullable<int>)), mi), Expr.Constant(0x1100, typeof(Int32))
                )
        ;
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid methodinfo that returns boolean, pass arguments of nullable type. 
        public static bool IsTrue(bool? x, bool? y) {
            return x.GetValueOrDefault() ^ y.GetValueOrDefault();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 17", new string[] { "positive", "exclusiveor", "operators", "Pri1" })]
        public static Expr ExclusiveOr17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ExclusiveOr).GetMethod("IsTrue");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ExclusiveOr(
                        Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)),
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(true),
                    "ExclusiveOr 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ExclusiveOr(
                        Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)),
                        Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(true, typeof(bool)),
                    "ExclusiveOr 2"
                )
            );
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // User defined overloaded operator on left argument, arguments are the proper types
        public class MyVal {
            public int Val { get; set; }

            public MyVal(int x) { Val = x; }

            public static int operator ^(MyVal v1, int v2) {
                return v1.Val ^ v2;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 18", new string[] { "positive", "exclusiveor", "operators", "Pri1" })]
        public static Expr ExclusiveOr18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ExclusiveOr(Expr.Constant(new MyVal(0x0001)), Expr.Constant(0x0111), mi), Expr.Constant(0x0110)
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // ExclusiveOr of false and other expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr 19", new string[] { "positive", "exclusiveor", "operators", "Pri1" })]
        public static Expr ExclusiveOr19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.ExclusiveOr(
                    Expr.Block(EU.ConcatEquals(Result, "False"), Expr.Constant(false)),
                    Expr.Block(EU.ConcatEquals(Result, "Expression"), Expr.Constant(true))
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("FalseExpression")));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
