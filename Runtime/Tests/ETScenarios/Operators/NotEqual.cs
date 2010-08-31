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
    
    public class NotEqual {
        // NotEqual of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 1", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.NotEqual(Expr.Constant(true), Expr.Constant(false)),
                    Expr.Constant(true)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.NotEqual(Expr.Constant(true), Expr.Constant(true)),
                    Expr.Constant(false)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // NotEqual of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 2", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression String1 = Expr.Variable(typeof(string), "");
            ParameterExpression String2 = Expr.Variable(typeof(string), "");

            Expressions.Add(Expr.Assign(String1, Expr.Constant("Hel")));
            Expressions.Add(Expr.Assign(String1, EU.ConcatEquals(String1, "lo")));
            Expressions.Add(Expr.Assign(String2, Expr.Constant("Hello")));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.NotEqual(String1, String2),
                    Expr.Constant(false)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.NotEqual(Expr.Constant("Hello"), Expr.Constant("World")),
                    Expr.Constant(true)));

            Expressions.Add(Expr.Empty());
            var tree = Expr.Block(new ParameterExpression[] { String1, String2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class TestClass {
            int _x;
            public TestClass(int val) { _x = val; }
        }

        // NotEqual of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 3", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.NotEqual(Left, Right), "Equal 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), Expr.NotEqual(Left, Left), "Equal 2"));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public struct TestStruct {
            int _x;
            public TestStruct(int val) { _x = val; }
        }

        // NotEqual of struct object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 4", new string[] { "negative", "NotEqual", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NotEqual4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.NotEqual(Left, Right);
            }));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);

            return tree;
        }

        // NotEqual of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 5", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Int16)1), Expr.Constant((Int16)2)), Expr.Constant(true), "NotEqual 1"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Int16)(-1)), Expr.Constant((Int16)(-1))), Expr.Constant(false), "NotEqual 2"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((UInt16)1), Expr.Constant((UInt16)2)), Expr.Constant(true), "NotEqual 3"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((UInt16)2), Expr.Constant((UInt16)2)), Expr.Constant(false), "NotEqual 4"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((short)1), Expr.Constant((short)2)), Expr.Constant(true), "NotEqual 5"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((short)200), Expr.Constant((short)200)), Expr.Constant(false), "NotEqual 6"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((ushort)1), Expr.Constant((ushort)2)), Expr.Constant(true), "NotEqual 7"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((ushort)4), Expr.Constant((ushort)4)), Expr.Constant(false), "NotEqual 8"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Int32)1), Expr.Constant((Int32)(-2))), Expr.Constant(true), "NotEqual 9"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Int32)Int32.MaxValue), Expr.Constant((Int32)Int32.MaxValue)), Expr.Constant(false), "NotEqual 10"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((UInt32)1), Expr.Constant((UInt32)2)), Expr.Constant(true), "NotEqual 11"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((UInt32)1), Expr.Constant((UInt32)1)), Expr.Constant(false), "NotEqual 12"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((long)1.0), Expr.Constant((long)2.0)), Expr.Constant(true), "NotEqual 13"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((long)1.0), Expr.Constant((long)1.0)), Expr.Constant(false), "NotEqual 14"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((ulong)1.0), Expr.Constant((ulong)2.0)), Expr.Constant(true), "NotEqual 15"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((ulong)2.0), Expr.Constant((ulong)2.0)), Expr.Constant(false), "NotEqual 16"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Single)1.0), Expr.Constant((Single)2.0)), Expr.Constant(true), "NotEqual 17"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Single)(-2.0)), Expr.Constant((Single)(-2.0))), Expr.Constant(false), "NotEqual 18"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Double)1.0), Expr.Constant((Double)2.0)), Expr.Constant(true), "NotEqual 19"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Double)2.5), Expr.Constant((Double)2.5)), Expr.Constant(false), "NotEqual 20"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((decimal)1.0), Expr.Constant((decimal)2.0)), Expr.Constant(true), "NotEqual 21"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((decimal)1.33333), Expr.Constant((decimal)1.33333)), Expr.Constant(false), "NotEqual 22"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((SByte)1), Expr.Constant((SByte)2)), Expr.Constant(true), "NotEqual 23")); // Int16 is CLS compliant equivalent type
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((SByte)1), Expr.Constant((SByte)1)), Expr.Constant(false), "NotEqual 24"));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Byte)1), Expr.Constant((Byte)2)), Expr.Constant(true), "NotEqual 25"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Byte)0), Expr.Constant((Byte)0)), Expr.Constant(false), "NotEqual 26"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 6", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant(4), Expr.Constant(4), false, null), Expr.Constant(false), "NotEqual 1"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant(4), Expr.Constant(5), false, null), Expr.Constant(true), "NotEqual 2"));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.NotEqual(
                        Expr.Constant(4, typeof(Nullable<int>)),
                        Expr.Constant(5, typeof(Nullable<int>)),
                        true,
                        null
                    ),
                    Expr.Constant(true, typeof(Nullable<bool>))
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.NotEqual(
                        Expr.Constant(4, typeof(Nullable<int>)),
                        Expr.Constant(5, typeof(Nullable<int>)),
                        false,
                        null
                    ),
                    Expr.Constant(true, typeof(bool))
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int NotEqualNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 7", new string[] { "negative", "NotEqual", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr NotEqual7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NotEqual).GetMethod("NotEqualNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.NotEqual(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), false, mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Empty();
        }

        // pass a MethodInfo that takes a paramarray
        public static int NotEqualParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 8", new string[] { "negative", "NotEqual", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr NotEqual8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NotEqual).GetMethod("NotEqualParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.NotEqual(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), false, mi), Expr.Constant(true));
            }));

            return Expr.Empty();
        }

        // with a valid MethodInfo, NotEquals two values of the same type
        public static bool NotEqualInts(int x, int y) {
            return x != y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 9", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NotEqual).GetMethod("NotEqualInts");

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant(10, typeof(Int32)), Expr.Constant(3, typeof(Int32)), false, mi), Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant(10, typeof(Int32)), Expr.Constant(10, typeof(Int32)), false, mi), Expr.Constant(false)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 10", new string[] { "negative", "NotEqual", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NotEqual10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NotEqual).GetMethod("NotEqualInts");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Int16)1), Expr.Constant((Int16)2), false, mi), Expr.Constant(true));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static bool NotEqualExceptionMsg(Exception e1, Exception e2) {
            return e1.Message != e2.Message;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 11", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NotEqual).GetMethod("NotEqualExceptionMsg");

            Expr Res =
                Expr.NotEqual(
                    Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException)),
                    Expr.Constant(new RankException("One"), typeof(RankException)),
                    false,
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false), "NotEqual 1"));

            Expr Res2 =
                Expr.NotEqual(
                    Expr.Constant(new IndexOutOfRangeException("Two"), typeof(IndexOutOfRangeException)),
                    Expr.Constant(new ArgumentNullException("Three"), typeof(ArgumentNullException)),
                    false,
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Res2, Expr.Constant(true), "NotEqual 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type. 
        public static bool NotEqualNullableInt(int? x, int y) {
            return x.GetValueOrDefault() != y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 12", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NotEqual).GetMethod("NotEqualNullableInt");

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(1), false, mi), Expr.Constant(true), "NotEqual 1"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(2), false, mi), Expr.Constant(false), "NotEqual 2"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant(2), false, mi), Expr.Constant(true), "NotEqual 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 13", new string[] { "negative", "NotEqual", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NotEqual13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.NotEqual(Expr.Constant(1, typeof(Int32)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), false, mi), Expr.Constant(true)
                )
        ;
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // NotEqual of two values of different types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 14", new string[] { "negative", "NotEqual", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NotEqual14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.NotEqual(Expr.Constant(true), Expr.Constant(1)), Expr.Constant(1), "NotEqual 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // NotEqualing across mixed types, no user defined operator
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "NotEqual 15", new string[] { "negative", "NotEqual", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NotEqual15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.NotEqual(Expr.Constant(1), Expr.Constant(2.0), false, mi), Expr.Constant(3)
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 16", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.NotEqual(Expr.Constant(new MyVal(6)), Expr.Constant(6), false, mi), Expr.Constant(false)
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Verify order of evaluation of expressions on NotEquals
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 17", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr A = Expr.Constant(true);
            Expr B = Expr.Constant(false);
            Expr C = Expr.Constant(false);
            Expr Res =
                Expr.NotEqual(Expr.NotEqual(A, B), C);
            Expr Res2 =
                Expr.NotEqual(A, Expr.NotEqual(B, C));
            Expr Res3 =
                Expr.NotEqual(Expr.NotEqual(A, Expr.Constant(true)), C);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true), "NotEqual 1"));
            Expressions.Add(EU.GenAreEqual(Res2, Expr.Constant(true), "NotEqual 2"));
            Expressions.Add(EU.GenAreEqual(Res3, Expr.Constant(false), "NotEqual 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // For: With a valid methodinfo that returns boolean, pass arguments of nullable type.
        public static bool testLiftNullable(bool x, bool y) {
            return x != y;
        }

        // For: With a valid methodinfo that returns non-boolean, pass arguments of nullable type.
        public static int testLiftNullableReturnInt(bool x, bool y) {
            return (x != y) ? 1 : 0;
        }

        public static bool compareNullables(bool? x, bool? y) {
            if (x.HasValue && y.HasValue)
                return x != y;
            return false;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 18", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NotEqual).GetMethod("testLiftNullable");

            // With a valid methodinfo that returns boolean, pass arguments of nullable type. Set lift to null to false.
            Expr Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true, typeof(bool)), "NotEqual 1"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type with null values. Set lift to null to false.
            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true, typeof(bool)), "NotEqual 2"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), false, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(bool)), "NotEqual 3"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type with null values. Set lift to null to true.
            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<bool>)), "NotEqual 4"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<bool>)), "NotEqual 5"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<bool>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to true.
            mi = typeof(NotEqual).GetMethod("testLiftNullableReturnInt");

            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), "NotEqual 6"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)), "NotEqual 7"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)0, typeof(Nullable<int>)), "NotEqual 8"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), true, mi);
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), "NotEqual 9"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to false.
            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)1, typeof(Nullable<Int32>)), "NotEqual 10"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)0, typeof(Nullable<Int32>)), "NotEqual 11"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)null, typeof(Nullable<Int32>)), "NotEqual 12"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant((Nullable<Int32>)null, typeof(Nullable<Int32>)), "NotEqual 13"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<Int32>)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to true.
            mi = typeof(NotEqual).GetMethod("compareNullables");

            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)), true, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false), "NotEqual 14"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to false.
            Res = Expr.NotEqual(Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)), false, mi);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false), "NotEqual 15"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // NotEquals on a type with IComparable definition, no user defined comparison operators
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

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual 19", new string[] { "positive", "NotEqual", "operators", "Pri1" })]
        public static Expr NotEqual19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression GenCompareValue1 = Expr.Variable(typeof(MyGenericComparable), "");
            ParameterExpression GenCompareValue2 = Expr.Variable(typeof(MyGenericComparable), "");

            Expressions.Add(Expr.Assign(GenCompareValue1, Expr.Constant(new MyGenericComparable(1))));
            Expressions.Add(Expr.Assign(GenCompareValue2, Expr.Constant(new MyGenericComparable(2))));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(GenCompareValue1, GenCompareValue2), Expr.Constant(true), "NotEqual 1"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(GenCompareValue1, GenCompareValue1), Expr.Constant(false), "NotEqual 2"));

            // non-generic case
            ParameterExpression CompareValue1 = Expr.Variable(typeof(MyComparable), "");
            ParameterExpression CompareValue2 = Expr.Variable(typeof(MyComparable), "");

            Expressions.Add(Expr.Assign(CompareValue1, Expr.Constant(new MyComparable(1))));
            Expressions.Add(Expr.Assign(CompareValue2, Expr.Constant(new MyComparable(2))));

            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(CompareValue1, CompareValue2), Expr.Constant(true), "NotEqual 3"));
            Expressions.Add(EU.GenAreEqual(Expr.NotEqual(CompareValue2, CompareValue2), Expr.Constant(false), "NotEqual 4"));

            var tree = EU.BlockVoid(new ParameterExpression[] { GenCompareValue1, GenCompareValue2, CompareValue1, CompareValue2 }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
