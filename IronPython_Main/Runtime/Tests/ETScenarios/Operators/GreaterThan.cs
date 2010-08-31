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

    public class GreaterThan {
        // GreaterThan of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 1", new string[] { "positive", "greaterthan", "operators", "Pri1" })]
        public static Expr GreaterThan1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThan(Expr.Constant(1), Expr.Constant(2)),
                    Expr.Constant(false)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThan(Expr.Constant(2), Expr.Constant(1)),
                    Expr.Constant(true)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // GreaterThan of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 2", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr GreaterThan2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression String1 = Expr.Variable(typeof(string), "");
            ParameterExpression String2 = Expr.Variable(typeof(string), "");

            Expressions.Add(Expr.Assign(String1, Expr.Constant("Hel")));
            Expressions.Add(Expr.Assign(String1, EU.ConcatEquals(String1, "lo")));
            Expressions.Add(Expr.Assign(String2, Expr.Constant("Hello")));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.GreaterThan(String1, String2),
                    Expr.Constant(true));
            }));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.GreaterThan(Expr.Constant("Hello"), Expr.Constant("World")),
                    Expr.Constant(false));
            }));

            return Expr.Empty();
        }

        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        // GreaterThan of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 3", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr GreaterThan3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            Expr Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.GreaterThan(Left, Right);
            }));

            return Expr.Empty();
        }

        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        // GreaterThan of struct object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 4", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr GreaterThan4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            Expr Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.GreaterThan(Left, Right);
            }));

            return Expr.Empty();
        }

        // GreaterThan of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 5", new string[] { "positive", "greaterthan", "operators", "Pri1" })]
        public static Expr GreaterThan5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Int16)1), Expr.Constant((Int16)2)), Expr.Constant(false), "GreaterThan 1"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Int16)(1)), Expr.Constant((Int16)(-1))), Expr.Constant(true), "GreaterThan 2"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((UInt16)1), Expr.Constant((UInt16)2)), Expr.Constant(false), "GreaterThan 3"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((UInt16)2), Expr.Constant((UInt16)1)), Expr.Constant(true), "GreaterThan 4"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((short)1), Expr.Constant((short)2)), Expr.Constant(false), "GreaterThan 5"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((short)200), Expr.Constant((short)0)), Expr.Constant(true), "GreaterThan 6"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((ushort)1), Expr.Constant((ushort)2)), Expr.Constant(false), "GreaterThan 7"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((ushort)4), Expr.Constant((ushort)2)), Expr.Constant(true), "GreaterThan 8"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Int32)1), Expr.Constant((Int32)(2))), Expr.Constant(false), "GreaterThan 9"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Int32)Int32.MaxValue), Expr.Constant((Int32)(-1))), Expr.Constant(true), "GreaterThan 10"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((UInt32)1), Expr.Constant((UInt32)2)), Expr.Constant(false), "GreaterThan 11"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((UInt32)1), Expr.Constant((UInt32)0)), Expr.Constant(true), "GreaterThan 12"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((long)1.0), Expr.Constant((long)2.0)), Expr.Constant(false), "GreaterThan 13"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((long)1.0), Expr.Constant((long)(-0.5))), Expr.Constant(true), "GreaterThan 14"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((ulong)1.0), Expr.Constant((ulong)2.0)), Expr.Constant(false), "GreaterThan 15"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((ulong)2.0), Expr.Constant((ulong)1.0)), Expr.Constant(true), "GreaterThan 16"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Single)1.0), Expr.Constant((Single)2.0)), Expr.Constant(false), "GreaterThan 17"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Single)(-2.0)), Expr.Constant((Single)(-3.0))), Expr.Constant(true), "GreaterThan 18"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Double)1.0), Expr.Constant((Double)2.0)), Expr.Constant(false), "GreaterThan 19"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Double)2.5), Expr.Constant((Double)2.4)), Expr.Constant(true), "GreaterThan 20"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((decimal)1.0), Expr.Constant((decimal)2.0)), Expr.Constant(false), "GreaterThan 21"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((decimal)1.333333), Expr.Constant((decimal)1.333332)), Expr.Constant(true), "GreaterThan 22"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((SByte)1), Expr.Constant((SByte)2)), Expr.Constant(false), "GreaterThan 23")); // Int16 is CLS compliant equivalent type
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((SByte)1), Expr.Constant((SByte)0)), Expr.Constant(true), "GreaterThan 24"));

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Byte)1), Expr.Constant((Byte)2)), Expr.Constant(false), "GreaterThan 25"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Byte)5), Expr.Constant((Byte)0)), Expr.Constant(true), "GreaterThan 26"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 6", new string[] { "positive", "greaterthan", "operators", "Pri1" })]
        public static Expr GreaterThan6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(4, typeof(Int32)), Expr.Constant(4, typeof(Int32)), false, null), Expr.Constant(false), "GreaterThan 1"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(4, typeof(Int32)), Expr.Constant(5, typeof(Int32)), false, null), Expr.Constant(false), "GreaterThan 2"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(4, typeof(Int32)), Expr.Constant(3, typeof(Int32)), false, null), Expr.Constant(true), "GreaterThan 3"));

            Expr Res = Expr.GreaterThan(
                        Expr.Constant(4, typeof(Nullable<int>)),
                        Expr.Constant(4, typeof(Nullable<int>)),
                        true,
                        null
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)), "GreaterThan 4"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            Res = Expr.GreaterThan(
                        Expr.Constant(5, typeof(Nullable<int>)),
                        Expr.Constant(4, typeof(Nullable<int>)),
                        false,
                        null
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true), "GreaterThan 5"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int GreaterThanNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 7", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr GreaterThan7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(GreaterThan).GetMethod("GreaterThanNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), false, mi), Expr.Constant(false));
            }));

            return Expr.Empty();
        }

        // pass a MethodInfo that takes a paramarray
        public static int GreaterThanParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 8", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr GreaterThan8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(GreaterThan).GetMethod("GreaterThanParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), false, mi), Expr.Constant(false));
            }));

            return Expr.Empty();
        }

        // with a valid MethodInfo, greaterthan two values of the same type
        public static bool GreaterThanInts(int x, int y) {
            return x > y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 9", new string[] { "positive", "greaterthan", "operators", "Pri1" })]
        public static Expr GreaterThan9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(GreaterThan).GetMethod("GreaterThanInts");

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(10, typeof(Int32)), Expr.Constant(3, typeof(Int32)), false, mi), Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(10, typeof(Int32)), Expr.Constant(10, typeof(Int32)), false, mi), Expr.Constant(false)));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(1, typeof(Int32)), Expr.Constant(10, typeof(Int32)), false, mi), Expr.Constant(false)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 10", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr GreaterThan10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(GreaterThan).GetMethod("GreaterThanInts");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Int16)1), Expr.Constant((Int16)2), false, mi), Expr.Constant(false));
            }));

            return Expr.Empty();
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static bool GreaterThanExceptionMsg(Exception e1, Exception e2) {
            return e1.Message == e2.Message;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 11", new string[] { "positive", "greaterthan", "operators", "Pri1" })]
        public static Expr GreaterThan11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(GreaterThan).GetMethod("GreaterThanExceptionMsg");

            Expr Res =
                Expr.GreaterThan(
                    Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException)),
                    Expr.Constant(new RankException("One"), typeof(RankException)),
                    false,
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true), "GreaterThan 1"));

            Expr Res2 =
                Expr.GreaterThan(
                    Expr.Constant(new IndexOutOfRangeException("Two"), typeof(IndexOutOfRangeException)),
                    Expr.Constant(new ArgumentNullException("Three"), typeof(ArgumentNullException)),
                    false,
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Res2, Expr.Constant(false), "GreaterThan 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type. 
        public static bool GreaterThanNullableInt(int? x, int y) {
            return x.GetValueOrDefault() > y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 12", new string[] { "positive", "greaterthan", "operators", "Pri1" })]
        public static Expr GreaterThan12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(GreaterThan).GetMethod("GreaterThanNullableInt");

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(1), false, mi), Expr.Constant(true), "GreaterThan 1"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(2), false, mi), Expr.Constant(false), "GreaterThan 2"));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant(2), false, mi), Expr.Constant(false), "GreaterThan 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 13", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr GreaterThan13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.GreaterThan(Expr.Constant(1, typeof(Int32)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), false, mi), Expr.Constant(false)
                );
            }));

            return Expr.Empty();
        }

        // GreaterThan of two values of different types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 14", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr GreaterThan14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(true), Expr.Constant(1)), Expr.Constant(1), "GreaterThan 1");
            }));

            return Expr.Empty();
        }

        // GreaterThaning across mixed types, no user defined operator
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "GreaterThan 15", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr GreaterThan15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThan(Expr.Constant(1), Expr.Constant(2.0), false, mi), Expr.Constant(3)
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

            public static bool operator >(MyVal v1, int v2) {
                return (v1.Val > v2);
            }
            public static bool operator <(MyVal v1, int v2) {
                return (v1.Val < v2);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 16", new string[] { "positive", "greaterthan", "operators", "Pri1" })]
        public static Expr GreaterThan16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(new MyVal(6)), Expr.Constant(4), false, mi), Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(new MyVal(-1)), Expr.Constant(6), false, mi), Expr.Constant(false)));
            Expressions.Add(EU.GenAreEqual(Expr.GreaterThan(Expr.Constant(new MyVal(6)), Expr.Constant(6), false, mi), Expr.Constant(false)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Verify order of evaluation of expressions on greaterthan
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 17", new string[] { "positive", "greaterthan", "operators", "Pri1" })]
        public static Expr GreaterThan17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.Block(
                    EU.ConcatEquals(Result, "One"),
                    Expr.GreaterThan(
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

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("OneTwoThreeFourFive")));
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Res =
                Expr.Block(
                    EU.ConcatEquals(Result, "One"),
                    Expr.GreaterThan(
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

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("OneTwoThreeFourFive")));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // For: With a valid methodinfo that returns boolean, pass arguments of nullable type.
        public static bool testLiftNullable(int x, int y) {
            return x > y;
        }

        // For: With a valid methodinfo that returns non-boolean, pass arguments of nullable type.
        public static int testLiftNullableReturnInt(int x, int y) {
            return (x > y) ? 1 : 0;
        }

        public static bool compareNullables(int? x, int? y) {
            if (x.HasValue && y.HasValue)
                return x > y;
            return false;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 18", new string[] { "positive", "greaterthan", "operators", "Pri1" })]
        public static Expr GreaterThan18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(GreaterThan).GetMethod("testLiftNullable");

            // With a valid methodinfo that returns boolean, pass arguments of nullable type. Set lift to null to false.
            Expr Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(bool)), "GreaterThan 1"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type with null values. Set lift to null to false.
            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true, typeof(bool)), "GreaterThan 2"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(bool)), "GreaterThan 3"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(bool)), "GreaterThan 4"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type with null values. Set lift to null to true.
            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)0, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<bool>)), "GreaterThan 5"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<bool>)), "GreaterThan 6"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true, typeof(Nullable<bool>)), "GreaterThan 7"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to true.
            mi = typeof(GreaterThan).GetMethod("testLiftNullableReturnInt");

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), "GreaterThan 8"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), "GreaterThan 9"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)0, typeof(Nullable<int>)), "GreaterThan 10"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), "GreaterThan 11"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to false.
            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)null, typeof(Nullable<Int32>)), "GreaterThan 12"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)1, typeof(Nullable<Int32>)), "GreaterThan 13"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)0, typeof(Nullable<Int32>)), "GreaterThan 14"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)null, typeof(Nullable<Int32>)), "GreaterThan 15"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to true.
            mi = typeof(GreaterThan).GetMethod("compareNullables");

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), true, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false), "GreaterThan 16"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to false.
            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)3, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true), "GreaterThan 17"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            Res = Expr.GreaterThan(Expr.Constant((Nullable<int>)3, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)3, typeof(Nullable<int>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false), "GreaterThan 18"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // GreaterThan on a type with IComparable definition, no user defined comparison operators
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

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 19", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr GreaterThan19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression GenCompareValue1 = Expr.Variable(typeof(MyGenericComparable), "");
            ParameterExpression GenCompareValue2 = Expr.Variable(typeof(MyGenericComparable), "");

            Expressions.Add(Expr.Assign(GenCompareValue1, Expr.Constant(new MyGenericComparable(1))));
            Expressions.Add(Expr.Assign(GenCompareValue2, Expr.Constant(new MyGenericComparable(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.GreaterThan(GenCompareValue1, GenCompareValue2), Expr.Constant(false), "GreaterThan 1");
            }));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.GreaterThan(GenCompareValue1, GenCompareValue1), Expr.Constant(true), "GreaterThan 2");
            }));

            return Expr.Empty();
        }

        // GreaterThan on a type with IComparable definition, no user defined comparison operators
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan 20", new string[] { "negative", "greaterthan", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr GreaterThan20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression CompareValue1 = Expr.Variable(typeof(MyComparable), "");
            ParameterExpression CompareValue2 = Expr.Variable(typeof(MyComparable), "");

            Expressions.Add(Expr.Assign(CompareValue1, Expr.Constant(new MyComparable(1))));
            Expressions.Add(Expr.Assign(CompareValue2, Expr.Constant(new MyComparable(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.GreaterThan(CompareValue1, CompareValue2), Expr.Constant(false), "GreaterThan 1");
            }));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.GreaterThan(CompareValue2, CompareValue2), Expr.Constant(true), "GreaterThan 2");
            }));

            return Expr.Empty();
        }
    }
}
