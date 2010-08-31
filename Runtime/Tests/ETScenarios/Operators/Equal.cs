#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Operators {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Equal {
        // Equal of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 1", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Equal(Expr.Constant(true), Expr.Constant(false)),
                    Expr.Constant(false)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Equal(Expr.Constant(true), Expr.Constant(true)),
                    Expr.Constant(true)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Equal of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 2", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression String1 = Expr.Variable(typeof(string), "");
            ParameterExpression String2 = Expr.Variable(typeof(string), "");

            Expressions.Add(Expr.Assign(String1, Expr.Constant("Hel")));
            Expressions.Add(Expr.Assign(String1, EU.ConcatEquals(String1, "lo")));
            Expressions.Add(Expr.Assign(String2, Expr.Constant("Hello")));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Equal(String1, String2),
                    Expr.Constant(true)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Equal(Expr.Constant("Hello"), Expr.Constant("World")),
                    Expr.Constant(false)));

            var tree = EU.BlockVoid(new[] { String1, String2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class TestClass {
            int _x;
            public TestClass(int val) { _x = val; }
        }

        // Equal of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 3", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), Expr.Equal(Left, Right), "Equal 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Equal(Left, Left), "Equal 2"));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public struct TestStruct {
            int _x;
            public TestStruct(int val) { _x = val; }
        }

        // Equal of struct object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 4", new string[] { "negative", "equal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Equal4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Equal(Left, Right);
            }));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);

            return tree;
        }

        // Equal of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 5", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Int16)1), Expr.Constant((Int16)2)), Expr.Constant(false), "Equal 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Int16)(-1)), Expr.Constant((Int16)(-1))), Expr.Constant(true), "Equal 2"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((UInt16)1), Expr.Constant((UInt16)2)), Expr.Constant(false), "Equal 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((UInt16)2), Expr.Constant((UInt16)2)), Expr.Constant(true), "Equal 4"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((short)1), Expr.Constant((short)2)), Expr.Constant(false), "Equal 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((short)200), Expr.Constant((short)200)), Expr.Constant(true), "Equal 6"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((ushort)1), Expr.Constant((ushort)2)), Expr.Constant(false), "Equal 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((ushort)4), Expr.Constant((ushort)4)), Expr.Constant(true), "Equal 8"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Int32)1), Expr.Constant((Int32)(-2))), Expr.Constant(false), "Equal 9"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Int32)Int32.MaxValue), Expr.Constant((Int32)Int32.MaxValue)), Expr.Constant(true), "Equal 10"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((UInt32)1), Expr.Constant((UInt32)2)), Expr.Constant(false), "Equal 11"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((UInt32)1), Expr.Constant((UInt32)1)), Expr.Constant(true), "Equal 12"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((long)1.0), Expr.Constant((long)2.0)), Expr.Constant(false), "Equal 13"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((long)1.0), Expr.Constant((long)1.0)), Expr.Constant(true), "Equal 14"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((ulong)1.0), Expr.Constant((ulong)2.0)), Expr.Constant(false), "Equal 15"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((ulong)2.0), Expr.Constant((ulong)2.0)), Expr.Constant(true), "Equal 16"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Single)1.0), Expr.Constant((Single)2.0)), Expr.Constant(false), "Equal 17"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Single)(-2.0)), Expr.Constant((Single)(-2.0))), Expr.Constant(true), "Equal 18"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Double)1.0), Expr.Constant((Double)2.0)), Expr.Constant(false), "Equal 19"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Double)2.5), Expr.Constant((Double)2.5)), Expr.Constant(true), "Equal 20"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((decimal)1.0), Expr.Constant((decimal)2.0)), Expr.Constant(false), "Equal 21"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((decimal)1.33333), Expr.Constant((decimal)1.33333)), Expr.Constant(true), "Equal 22"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((SByte)1), Expr.Constant((SByte)2)), Expr.Constant(false), "Equal 23")); // Int16 is CLS compliant equivalent type
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((SByte)1), Expr.Constant((SByte)1)), Expr.Constant(true), "Equal 24"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Byte)1), Expr.Constant((Byte)2)), Expr.Constant(false), "Equal 25"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Byte)0), Expr.Constant((Byte)0)), Expr.Constant(true), "Equal 26"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 6", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(4), Expr.Constant(4), false, null), Expr.Constant(true), "Equal 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(4), Expr.Constant(5), false, null), Expr.Constant(false), "Equal 2"));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Equal(
                        Expr.Constant(4, typeof(Nullable<int>)),
                        Expr.Constant(5, typeof(Nullable<int>)),
                        true,
                        null
                    ),
                    Expr.Constant(false, typeof(Nullable<bool>))
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Equal(
                        Expr.Constant(4, typeof(Nullable<int>)),
                        Expr.Constant(5, typeof(Nullable<int>)),
                        false,
                        null
                    ),
                    Expr.Constant(false, typeof(bool))
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int EqualNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 7", new string[] { "negative", "equal", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Equal7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Equal).GetMethod("EqualNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Equal(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), false, mi), Expr.Constant(3, typeof(Int32)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // pass a MethodInfo that takes a paramarray
        public static int EqualParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 8", new string[] { "negative", "equal", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Equal8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Equal).GetMethod("EqualParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Equal(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), false, mi), Expr.Constant(3, typeof(Int32)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // with a valid MethodInfo, equals two values of the same type
        public static bool EqualInts(int x, int y) {
            return x == y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 9", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Equal).GetMethod("EqualInts");

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(10, typeof(Int32)), Expr.Constant(3, typeof(Int32)), false, mi), Expr.Constant(false)));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(10, typeof(Int32)), Expr.Constant(10, typeof(Int32)), false, mi), Expr.Constant(true)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 10", new string[] { "negative", "equal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Equal10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Equal).GetMethod("EqualInts");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Equal(Expr.Constant((Int16)1), Expr.Constant((Int16)2), false, mi), Expr.Constant((Int16)3));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfoâ€™s arguments
        public static bool EqualExceptionMsg(Exception e1, Exception e2) {
            return e1.Message == e2.Message;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 11", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Equal).GetMethod("EqualExceptionMsg");

            Expr Res =
                Expr.Equal(
                    Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException)),
                    Expr.Constant(new RankException("One"), typeof(RankException)),
                    false,
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true), "Equal 1"));

            Expr Res2 =
                Expr.Equal(
                    Expr.Constant(new IndexOutOfRangeException("Two"), typeof(IndexOutOfRangeException)),
                    Expr.Constant(new ArgumentNullException("Three"), typeof(ArgumentNullException)),
                    false,
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Res2, Expr.Constant(false), "Equal 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type. 
        public static bool EqualNullableInt(int? x, int y) {
            return x.GetValueOrDefault() == y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 12", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Equal).GetMethod("EqualNullableInt");

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(1), false, mi), Expr.Constant(false), "Equal 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(2), false, mi), Expr.Constant(true), "Equal 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant(2), false, mi), Expr.Constant(false), "Equal 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 13", new string[] { "negative", "equal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Equal13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.Equal(Expr.Constant(1, typeof(Int32)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), false, mi), Expr.Constant(false)
                )
        ;
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Equal of two values of different types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 14", new string[] { "negative", "equal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Equal14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Equal(Expr.Constant(true), Expr.Constant(1)), Expr.Constant(1), "Equal 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Equaling across mixed types, no user defined operator
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Equal 15", new string[] { "negative", "equal", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Equal15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Equal(Expr.Constant(1), Expr.Constant(2.0), false, mi), Expr.Constant(3)
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

            public static bool operator ==(MyVal v1, int v2) {
                return (v1.Val == v2);
            }
            public static bool operator !=(MyVal v1, int v2) {
                return (v1.Val != v2);
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 16", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Equal(Expr.Constant(new MyVal(6)), Expr.Constant(6), false, mi), Expr.Constant(true)
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Verify order of evaluation of expressions on equals
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 17", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr A = Expr.Constant(true);
            Expr B = Expr.Constant(false);
            Expr C = Expr.Constant(false);
            Expr Res =
                Expr.Equal(Expr.Equal(A, B), C);
            Expr Res2 =
                Expr.Equal(A, Expr.Equal(B, C));
            Expr Res3 =
                Expr.Equal(Expr.Equal(A, Expr.Constant(true)), B);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true), "Equal 1"));
            Expressions.Add(EU.GenAreEqual(Res2, Expr.Constant(true), "Equal 2"));
            Expressions.Add(EU.GenAreEqual(Res3, Expr.Constant(false), "Equal 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // For: With a valid methodinfo that returns boolean, pass arguments of nullable type.
        public static bool testLiftNullable(bool x, bool y) {
            return x == y;
        }

        // For: With a valid methodinfo that returns non-boolean, pass arguments of nullable type.
        public static int testLiftNullableReturnInt(bool x, bool y) {
            return (x == y) ? 1 : 0;
        }

        public static bool compareNullables(bool? x, bool? y) {
            if (x.HasValue && y.HasValue)
                return x == y;
            return false;
        }

        // Some regressions for Dev10 Bug 445635
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 18", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Equal).GetMethod("testLiftNullable");

            // With a valid methodinfo that returns boolean, pass arguments of nullable type. Set liftToNull to false.
            Expr Res = Expr.Equal(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(bool)), "Equal 1"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type with null values. Set liftToNull to false.
            Res = Expr.Equal(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(bool)), "Equal 2"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            Res = Expr.Equal(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true, typeof(bool)), "Equal 3"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type with null values. Set liftToNull to true.
            Res = Expr.Equal(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<bool>)), "Equal 4"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            Res = Expr.Equal(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<bool>)), "Equal 5"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set liftToNull to true (which is ignored for non-bool return types).
            // the following tests for non-bool returning methods also serve as a regression for Dev10 bug 445635
            mi = typeof(Equal).GetMethod("testLiftNullableReturnInt");

            Res = Expr.Equal(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), "Equal 6"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.Equal(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)0, typeof(Nullable<int>)), "Equal 7"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.Equal(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), "Equal 8"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.Equal(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), "Equal 9"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set liftToNull to false (which is ignored for non-bool return types).
            Res = Expr.Equal(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)0, typeof(Nullable<Int32>)), "Equal 10"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.Equal(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)1, typeof(Nullable<Int32>)), "Equal 11"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.Equal(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)null, typeof(Nullable<Int32>)), "Equal 12"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.Equal(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)null, typeof(Nullable<Int32>)), "Equal 13"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            // With a valid methodinfo that returns non boolean, pass arguments of non-nullable type. Set liftToNull to true (which is ignored for non-bool return types).
            Res = Expr.Equal(Expr.Constant(true), Expr.Constant(false), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(0), "Equal 14"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int)));

            // With a valid methodinfo that returns non boolean, pass arguments of non-nullable type. Set liftToNull to false (which is ignored for non-bool return types).
            Res = Expr.Equal(Expr.Constant(true), Expr.Constant(true), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(1), "Equal 15"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type. Set liftToNull to true.
            mi = typeof(Equal).GetMethod("compareNullables");

            Res = Expr.Equal(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), true, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false), "Equal 16"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type. Set liftToNull to false.
            Res = Expr.Equal(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true), "Equal 16"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Equals on a type with IComparable definition, no user defined comparison operators
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

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 19", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression GenCompareValue1 = Expr.Variable(typeof(MyGenericComparable), "");
            ParameterExpression GenCompareValue2 = Expr.Variable(typeof(MyGenericComparable), "");

            Expressions.Add(Expr.Assign(GenCompareValue1, Expr.Constant(new MyGenericComparable(1))));
            Expressions.Add(Expr.Assign(GenCompareValue2, Expr.Constant(new MyGenericComparable(2))));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(GenCompareValue1, GenCompareValue2), Expr.Constant(false), "Equal 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(GenCompareValue1, GenCompareValue1), Expr.Constant(true), "Equal 2"));

            // non-generic case
            ParameterExpression CompareValue1 = Expr.Variable(typeof(MyComparable), "");
            ParameterExpression CompareValue2 = Expr.Variable(typeof(MyComparable), "");

            Expressions.Add(Expr.Assign(CompareValue1, Expr.Constant(new MyComparable(1))));
            Expressions.Add(Expr.Assign(CompareValue2, Expr.Constant(new MyComparable(2))));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(CompareValue1, CompareValue2), Expr.Constant(false), "Equal 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(CompareValue2, CompareValue2), Expr.Constant(true), "Equal 4"));

            var tree = EU.BlockVoid(new[] { GenCompareValue1, GenCompareValue2, CompareValue1, CompareValue2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Methodinfo with byref nullable parameters
        public static bool EqualRef1(ref int? x, ref int? y) {
            return x.GetValueOrDefault() == y.GetValueOrDefault();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 20", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Equal).GetMethod("EqualRef1");

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(2, typeof(int?)), Expr.Constant(1, typeof(int?)), false, mi), Expr.Constant(false), "Equal 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(2, typeof(int?)), Expr.Constant(2, typeof(int?)), false, mi), Expr.Constant(true), "Equal 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(null, typeof(int?)), Expr.Constant(2, typeof(int?)), false, mi), Expr.Constant(false), "Equal 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(null, typeof(int?)), Expr.Constant(null, typeof(int?)), false, mi), Expr.Constant(true), "Equal 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(null, typeof(int?)), Expr.Constant(0, typeof(int?)), false, mi), Expr.Constant(true), "Equal 5"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool EqualRef2(ref int? x, ref int? y) {
            var ret = x.GetValueOrDefault() == y.GetValueOrDefault();
            x = (int?)5;
            y = (int?)6;
            return ret;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 21", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var Left = Expr.Parameter(typeof(int?), "");
            var Right = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(Expr.Assign(Right, Expr.Constant(2, typeof(int?))));

            MethodInfo mi = typeof(Equal).GetMethod("EqualRef2");


            Expressions.Add(EU.GenAreEqual(Expr.Equal(Left, Right, false, mi), Expr.Constant(false), "Equal 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(5, typeof(int?)), Left, "Equal 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6, typeof(int?)), Right, "Equal 3"));

            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(2, typeof(int?)), Expr.Constant(2, typeof(int?)), false, mi), Expr.Constant(true), "Equal 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(null, typeof(int?)), Expr.Constant(2, typeof(int?)), false, mi), Expr.Constant(false), "Equal 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(null, typeof(int?)), Expr.Constant(null, typeof(int?)), false, mi), Expr.Constant(true), "Equal 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Equal(Expr.Constant(null, typeof(int?)), Expr.Constant(0, typeof(int?)), false, mi), Expr.Constant(true), "Equal 7"));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool LiftedEquality(ref int? x, ref double? y) {
            return x == y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 22", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Equal).GetMethod("EqualRef1");


            var e = Expression.Equal(
                Expression.Constant(1, typeof(int?)),
                Expression.Constant(1.0, typeof(double?)),
                false,
                typeof(Equal).GetMethod("LiftedEquality")
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), Expr.Constant(e.IsLifted), "Equals 1"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Invoke(Expr.Lambda<Func<bool>>(e)), "Equals 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Equal of type objects
        // Regression for 661487
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 23", new string[] { "positive", "equal", "operators", "Pri1" })]
        public static Expr Equal23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Equal(Expr.Constant(typeof(string), typeof(Type)), Expr.Constant(typeof(string), typeof(Type))),
                    Expr.Constant(true)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Equal of type objects
        // Regression for 661487
        //[ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 24", new string[] { "negative", "equal", "operators", "Pri1", "PartialTrustOnly" }, Exception = typeof(TypeLoadException))]
        //public static Expr Equal24(EU.IValidator V) {
        //    List<Expression> Expressions = new List<Expression>();

        //    Expressions.Add(
        //            EU.GenAreEqual(
        //                Expr.Equal(Expr.Constant(typeof(string)), Expr.Constant(typeof(string))),
        //                Expr.Constant(true))
        //            );
        //    var tree = Expr.Block(Expressions);
        //    V.ValidateException<System.TypeLoadException>(tree, true);
        //    return tree;
        //}

        // Equal of type objects
        // Regression for 661487
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal 25", new string[] { "positive", "equal", "operators", "Pri1", "FullTrustOnly" })]
        public static Expr Equal25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Equal(Expr.Constant(typeof(string)), Expr.Constant(typeof(string))),
                    Expr.Constant(true)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
