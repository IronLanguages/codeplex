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

    public class LessThanOrEqual {
        // LessThanOrEqual of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 1", new string[] { "positive", "lessthanorequal", "operators", "Pri1" })]
        public static Expr LessThanOrEqual1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(Expr.Constant(1), Expr.Constant(2)),
                    Expr.Constant(true)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(Expr.Constant(2), Expr.Constant(1)),
                    Expr.Constant(false)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // LessThanOrEqual of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 2", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LessThanOrEqual2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression String1 = Expr.Variable(typeof(string), "");
            ParameterExpression String2 = Expr.Variable(typeof(string), "");

            Expressions.Add(Expr.Assign(String1, Expr.Constant("Hel")));
            Expressions.Add(Expr.Assign(String1, EU.ConcatEquals(String1, "lo")));
            Expressions.Add(Expr.Assign(String2, Expr.Constant("Hello")));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(String1, String2),
                    Expr.Constant(false));
            }));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(Expr.Constant("Hello"), Expr.Constant("World")),
                    Expr.Constant(true));
            }));

            var tree = EU.BlockVoid(new[] { String1, String2 }, Expressions);

            return tree;
        }

        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        // LessThanOrEqual of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 3", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LessThanOrEqual3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            Expr Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LessThanOrEqual(Left, Right);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        // LessThanOrEqual of struct object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 4", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LessThanOrEqual4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            Expr Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LessThanOrEqual(Left, Right);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // LessThanOrEqual of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 5", new string[] { "positive", "lessthanorequal", "operators", "Pri1" })]
        public static Expr LessThanOrEqual5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Int16)1), Expr.Constant((Int16)2)), Expr.Constant(true), "LessThanOrEqual 1"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Int16)(1)), Expr.Constant((Int16)(-1))), Expr.Constant(false), "LessThanOrEqual 2"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((UInt16)1), Expr.Constant((UInt16)2)), Expr.Constant(true), "LessThanOrEqual 3"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((UInt16)2), Expr.Constant((UInt16)1)), Expr.Constant(false), "LessThanOrEqual 4"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((short)1), Expr.Constant((short)2)), Expr.Constant(true), "LessThanOrEqual 5"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((short)200), Expr.Constant((short)0)), Expr.Constant(false), "LessThanOrEqual 6"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((ushort)1), Expr.Constant((ushort)2)), Expr.Constant(true), "LessThanOrEqual 7"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((ushort)4), Expr.Constant((ushort)2)), Expr.Constant(false), "LessThanOrEqual 8"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Int32)1), Expr.Constant((Int32)(2))), Expr.Constant(true), "LessThanOrEqual 9"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Int32)Int32.MaxValue), Expr.Constant((Int32)(-1))), Expr.Constant(false), "LessThanOrEqual 10"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((UInt32)1), Expr.Constant((UInt32)2)), Expr.Constant(true), "LessThanOrEqual 11"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((UInt32)1), Expr.Constant((UInt32)0)), Expr.Constant(false), "LessThanOrEqual 12"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((long)1.0), Expr.Constant((long)2.0)), Expr.Constant(true), "LessThanOrEqual 13"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((long)1.0), Expr.Constant((long)(-0.5))), Expr.Constant(false), "LessThanOrEqual 14"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((ulong)1.0), Expr.Constant((ulong)2.0)), Expr.Constant(true), "LessThanOrEqual 15"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((ulong)2.0), Expr.Constant((ulong)1.0)), Expr.Constant(false), "LessThanOrEqual 16"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Single)1.0), Expr.Constant((Single)2.0)), Expr.Constant(true), "LessThanOrEqual 17"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Single)(-2.0)), Expr.Constant((Single)(-3.0))), Expr.Constant(false), "LessThanOrEqual 18"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Double)1.0), Expr.Constant((Double)2.0)), Expr.Constant(true), "LessThanOrEqual 19"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Double)2.5), Expr.Constant((Double)2.4)), Expr.Constant(false), "LessThanOrEqual 20"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((decimal)1.0), Expr.Constant((decimal)2.0)), Expr.Constant(true), "LessThanOrEqual 21"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((decimal)1.333333), Expr.Constant((decimal)1.333332)), Expr.Constant(false), "LessThanOrEqual 22"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((SByte)1), Expr.Constant((SByte)2)), Expr.Constant(true), "LessThanOrEqual 23")); // Int16 is CLS compliant equivalent type
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((SByte)1), Expr.Constant((SByte)0)), Expr.Constant(false), "LessThanOrEqual 24"));

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Byte)1), Expr.Constant((Byte)2)), Expr.Constant(true), "LessThanOrEqual 25"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Byte)5), Expr.Constant((Byte)0)), Expr.Constant(false), "LessThanOrEqual 26"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 6", new string[] { "positive", "lessthanorequal", "operators", "Pri1" })]
        public static Expr LessThanOrEqual6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(4, typeof(Int32)), Expr.Constant(4, typeof(Int32)), false, null), Expr.Constant(true), "LessThanOrEqual 1"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(4, typeof(Int32)), Expr.Constant(5, typeof(Int32)), false, null), Expr.Constant(true), "LessThanOrEqual 2"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(4, typeof(Int32)), Expr.Constant(3, typeof(Int32)), false, null), Expr.Constant(false), "LessThanOrEqual 3"));

            Expr Res = Expr.LessThanOrEqual(
                        Expr.Constant(4, typeof(Nullable<int>)),
                        Expr.Constant(4, typeof(Nullable<int>)),
                        true,
                        null
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), "LessThanOrEqual 4"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            Res = Expr.LessThanOrEqual(
                        Expr.Constant(-1, typeof(Nullable<int>)),
                        Expr.Constant(-2, typeof(Nullable<int>)),
                        false,
                        null
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false), "LessThanOrEqual 5"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int LessThanOrEqualNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 7", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr LessThanOrEqual7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(LessThanOrEqual).GetMethod("LessThanOrEqualNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), false, mi), Expr.Constant(true));
            }));

            return EU.BlockVoid(Expressions);
        }

        // pass a MethodInfo that takes a paramarray
        public static int LessThanOrEqualParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 8", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr LessThanOrEqual8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(LessThanOrEqual).GetMethod("LessThanOrEqualParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), false, mi), Expr.Constant(true));
            }));

            return EU.BlockVoid(Expressions);
        }

        // with a valid MethodInfo, lessthanorequal two values of the same type
        public static bool LessThanOrEqualInts(int x, int y) {
            return x <= y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 9", new string[] { "positive", "lessthanorequal", "operators", "Pri1" })]
        public static Expr LessThanOrEqual9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(LessThanOrEqual).GetMethod("LessThanOrEqualInts");

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(10, typeof(Int32)), Expr.Constant(3, typeof(Int32)), false, mi), Expr.Constant(false)));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(-1, typeof(Int32)), Expr.Constant(-1, typeof(Int32)), false, mi), Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(1, typeof(Int32)), Expr.Constant(10, typeof(Int32)), false, mi), Expr.Constant(true)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 10", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LessThanOrEqual10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(LessThanOrEqual).GetMethod("LessThanOrEqualInts");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Int16)1), Expr.Constant((Int16)2), false, mi), Expr.Constant(true));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static bool LessThanOrEqualExceptionMsg(Exception e1, Exception e2) {
            return e1.Message == e2.Message;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 11", new string[] { "positive", "lessthanorequal", "operators", "Pri1" })]
        public static Expr LessThanOrEqual11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(LessThanOrEqual).GetMethod("LessThanOrEqualExceptionMsg");

            Expr Res =
                Expr.LessThanOrEqual(
                    Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException)),
                    Expr.Constant(new RankException("One"), typeof(RankException)),
                    false,
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true), "LessThanOrEqual 1"));

            Expr Res2 =
                Expr.LessThanOrEqual(
                    Expr.Constant(new IndexOutOfRangeException("Two"), typeof(IndexOutOfRangeException)),
                    Expr.Constant(new ArgumentNullException("Three"), typeof(ArgumentNullException)),
                    false,
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Res2, Expr.Constant(false), "LessThanOrEqual 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type. 
        public static bool LessThanOrEqualNullableInt(int? x, int y) {
            return x.GetValueOrDefault() <= y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 12", new string[] { "positive", "lessthanorequal", "operators", "Pri1" })]
        public static Expr LessThanOrEqual12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(LessThanOrEqual).GetMethod("LessThanOrEqualNullableInt");

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(1), false, mi), Expr.Constant(false), "LessThanOrEqual 1"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)0, typeof(Nullable<int>)), Expr.Constant(0), false, mi), Expr.Constant(true), "LessThanOrEqual 2"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant(2), false, mi), Expr.Constant(true), "LessThanOrEqual 3"));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), Expr.Constant(2), false, mi), Expr.Constant(true), "LessThanOrEqual 4"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 13", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LessThanOrEqual13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(Expr.Constant(1, typeof(Int32)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), false, mi), Expr.Constant(true)
                );
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // LessThanOrEqual of two values of different types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 14", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LessThanOrEqual14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(true), Expr.Constant(1)), Expr.Constant(1), "LessThanOrEqual 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // LessThanOrEqualing across mixed types, no user defined operator
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "LessThanOrEqual 15", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LessThanOrEqual15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(Expr.Constant(1), Expr.Constant(2.0), false, mi), Expr.Constant(3)
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.ValidateException<InvalidOperationException>(tree, false);
            return tree;
        }

        // User defined overloaded operator on left argument, arguments are the proper types
        public class MyVal {
            public int Val { get; set; }

            public MyVal(int x) { Val = x; }

            public static bool operator >=(MyVal v1, int v2) {
                return (v1.Val >= v2);
            }
            public static bool operator <=(MyVal v1, int v2) {
                return (v1.Val <= v2);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 16", new string[] { "positive", "lessthanorequal", "operators", "Pri1" })]
        public static Expr LessThanOrEqual16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(new MyVal(6)), Expr.Constant(4), false, mi), Expr.Constant(false)));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(new MyVal(-1)), Expr.Constant(6), false, mi), Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Expr.LessThanOrEqual(Expr.Constant(new MyVal(6)), Expr.Constant(6), false, mi), Expr.Constant(true)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Verify order of evaluation of expressions on lessthanorequal
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 17", new string[] { "positive", "lessthanorequal", "operators", "Pri1" })]
        public static Expr LessThanOrEqual17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.Block(
                    EU.ConcatEquals(Result, "One"),
                    Expr.LessThanOrEqual(
                        Expr.Add(
                            Expr.Block(EU.ConcatEquals(Result, "Two"), Expr.Constant(4)),
                            Expr.Block(EU.ConcatEquals(Result, "Three"), Expr.Constant(-1))
                        ),
                        Expr.Add(
                            Expr.Block(EU.ConcatEquals(Result, "Four"), Expr.Constant(1)),
                            Expr.Block(EU.ConcatEquals(Result, "Five"), Expr.Constant(6))
                        )
                    )
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("OneTwoThreeFourFive")));
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Res =
                Expr.Block(
                    EU.ConcatEquals(Result, "One"),
                    Expr.LessThanOrEqual(
                        Expr.Add(
                            Expr.Block(EU.ConcatEquals(Result, "Two"), Expr.Constant(4)),
                            Expr.Block(EU.ConcatEquals(Result, "Three"), Expr.Constant(-5))
                        ),
                        Expr.Add(
                            Expr.Block(EU.ConcatEquals(Result, "Four"), Expr.Constant(1)),
                            Expr.Block(EU.ConcatEquals(Result, "Five"), Expr.Constant(-6))
                        )
                    )
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("OneTwoThreeFourFive")));
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Res =
                Expr.Block(
                    EU.ConcatEquals(Result, "One"),
                    Expr.LessThanOrEqual(
                        Expr.Add(
                            Expr.Block(EU.ConcatEquals(Result, "Two"), Expr.Constant(-1)),
                            Expr.Block(EU.ConcatEquals(Result, "Three"), Expr.Constant(-1))
                        ),
                        Expr.Add(
                            Expr.Block(EU.ConcatEquals(Result, "Four"), Expr.Constant(-8)),
                            Expr.Block(EU.ConcatEquals(Result, "Five"), Expr.Constant(6))
                        )
                    )
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("OneTwoThreeFourFive")));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // For: With a valid methodinfo that returns boolean, pass arguments of nullable type.
        public static bool testLiftNullable(int x, int y) {
            return x <= y;
        }

        // For: With a valid methodinfo that returns non-boolean, pass arguments of nullable type.
        public static int testLiftNullableReturnInt(int x, int y) {
            return (x <= y) ? 1 : 0;
        }

        public static bool compareNullables(int? x, int? y) {
            if (x.HasValue && y.HasValue)
                return x <= y;
            return false;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 18", new string[] { "positive", "lessthanorequal", "operators", "Pri1" })]
        public static Expr LessThanOrEqual18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(LessThanOrEqual).GetMethod("testLiftNullable");

            // With a valid methodinfo that returns boolean, pass arguments of nullable type. Set lift to null to false.
            Expr Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true, typeof(bool)), "LessThanOrEqual 1"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type with null values. Set lift to null to false.
            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(bool)), "LessThanOrEqual 2"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true, typeof(bool)), "LessThanOrEqual 3"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(bool)), "LessThanOrEqual 4"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type with null values. Set lift to null to true.
            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)0, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<bool>)), "LessThanOrEqual 5"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<bool>)), "LessThanOrEqual 6"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(Nullable<bool>)), "LessThanOrEqual 7"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to true.
            mi = typeof(LessThanOrEqual).GetMethod("testLiftNullableReturnInt");

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), "LessThanOrEqual 8"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)0, typeof(Nullable<int>)), "LessThanOrEqual 9"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), "LessThanOrEqual 10"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), "LessThanOrEqual 11"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to false.
            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)null, typeof(Nullable<Int32>)), "LessThanOrEqual 12"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)0, typeof(Nullable<Int32>)), "LessThanOrEqual 13"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)1, typeof(Nullable<Int32>)), "LessThanOrEqual 14"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)null, typeof(Nullable<Int32>)), "LessThanOrEqual 15"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to true.
            mi = typeof(LessThanOrEqual).GetMethod("compareNullables");

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), true, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false), "LessThanOrEqual 16"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to false.
            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)3, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false), "LessThanOrEqual 17"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            Res = Expr.LessThanOrEqual(Expr.Constant((Nullable<int>)3, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)3, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true), "LessThanOrEqual 18"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // LessThanOrEquals on a type with IComparable definition, no user defined comparison operators
        public class MyGenericComparable : IComparable<MyGenericComparable> {
            int Val { get; set; }
            public MyGenericComparable(int x) { Val = x; }
            public int CompareTo(MyGenericComparable other) {
                return Val.CompareTo(other.Val);
            }
        }

        public class MyComparable : IComparable {
            int Val { get; set; }
            public MyComparable(int x) { Val = x; }
            public int CompareTo(object obj) {
                if (obj is MyComparable) {
                    MyComparable other = (MyComparable)obj;
                    return this.Val.CompareTo(other.Val);
                } else {
                    throw new ArgumentException("Object is not a MyComparable");
                }
            }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 19", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LessThanOrEqual19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression GenCompareValue1 = Expr.Variable(typeof(MyGenericComparable), "");
            ParameterExpression GenCompareValue2 = Expr.Variable(typeof(MyGenericComparable), "");

            Expressions.Add(Expr.Assign(GenCompareValue1, Expr.Constant(new MyGenericComparable(1))));
            Expressions.Add(Expr.Assign(GenCompareValue2, Expr.Constant(new MyGenericComparable(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.LessThanOrEqual(GenCompareValue1, GenCompareValue2), Expr.Constant(true), "LessThanOrEqual 1");
            }));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.LessThanOrEqual(GenCompareValue1, GenCompareValue1), Expr.Constant(false), "LessThanOrEqual 2");
            }));

            var tree = EU.BlockVoid(new[] { GenCompareValue1, GenCompareValue2 }, Expressions);

            return tree;
        }

        // LessThanOrEqual on a type with IComparable definition, no user defined comparison operators
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual 20", new string[] { "negative", "lessthanorequal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LessThanOrEqual20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression CompareValue1 = Expr.Variable(typeof(MyComparable), "");
            ParameterExpression CompareValue2 = Expr.Variable(typeof(MyComparable), "");

            Expressions.Add(Expr.Assign(CompareValue1, Expr.Constant(new MyComparable(1))));
            Expressions.Add(Expr.Assign(CompareValue2, Expr.Constant(new MyComparable(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.LessThanOrEqual(CompareValue1, CompareValue2), Expr.Constant(true), "LessThanOrEqual 1");
            }));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.LessThanOrEqual(CompareValue2, CompareValue2), Expr.Constant(false), "LessThanOrEqual 2");
            }));

            var tree = EU.BlockVoid(new[] { CompareValue1, CompareValue2 }, Expressions);

            return tree;
        }
    }
}