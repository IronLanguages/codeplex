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

    public class AddChecked {
        // AddChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // TODO: Add tests for SByte and Byte once they support AddChecked (Dev10 Bug 502521)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 1", new string[] { "positive", "addchecked", "operators", "Pri1" })]
        public static Expr AddChecked1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Binary Operator not defined for the following two:
            //Expressions.Add(EU.GenAreEqual(Expr.Add(Expr.Constant((SByte)1), Expr.Constant((SByte)2)), Expr.Constant((SByte)3))); // Int16 is CLS compliant equivalent type
            //Expressions.Add(EU.GenAreEqual(Expr.Add(Expr.Constant((Byte)1), Expr.Constant((Byte)2)), Expr.Constant((Byte)3)));

            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((Int16)1), Expr.Constant((Int16)2)), Expr.Constant((Int16)3), "AddChecked 1"));
            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((UInt16)1), Expr.Constant((UInt16)2)), Expr.Constant((UInt16)3), "AddChecked 2"));
            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((short)1), Expr.Constant((short)2)), Expr.Constant((short)3), "AddChecked 3"));
            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((ushort)1), Expr.Constant((ushort)2)), Expr.Constant((ushort)3), "AddChecked 4"));
            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((Int32)1), Expr.Constant((Int32)2)), Expr.Constant((Int32)3), "AddChecked 5"));
            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((UInt32)1), Expr.Constant((UInt32)2)), Expr.Constant((UInt32)3), "AddChecked 6"));
            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((long)1.0), Expr.Constant((long)2.0)), Expr.Constant((long)3.0), "AddChecked 7"));
            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((ulong)1.0), Expr.Constant((ulong)2.0)), Expr.Constant((ulong)3.0), "AddChecked 8"));
            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((Single)1.0), Expr.Constant((Single)2.0)), Expr.Constant((Single)3.0), "AddChecked 9"));
            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((Double)1.0), Expr.Constant((Double)2.0)), Expr.Constant((Double)3.0), "AddChecked 10"));
            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant((decimal)1.0), Expr.Constant((decimal)2.0)), Expr.Constant((decimal)3.0), "AddChecked 11"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // AddChecked of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 2", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddChecked2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddChecked(Expr.Constant(true), Expr.Constant(false), null);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // AddChecked of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 3", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddChecked3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddChecked(Expr.Constant("Hello"), Expr.Constant("World"));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        // AddChecked of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 4", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddChecked4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddChecked(Left, Right, null);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        // AddChecked of struct object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 5", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddChecked5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            Expr Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddChecked(Left, Right);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 6", new string[] { "positive", "addchecked", "operators", "Pri1" })]
        public static Expr AddChecked6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant(-1, typeof(Int32)), Expr.Constant(1, typeof(Int32)), null), Expr.Constant(0, typeof(Int32))));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int AddNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 7", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddChecked7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddChecked).GetMethod("AddNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.AddChecked(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // pass a MethodInfo that takes a paramarray
        public static int AddParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 8", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddChecked8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddChecked).GetMethod("AddParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.AddChecked(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // with a valid MethodInfo, add two values of the same type
        public static int AddInts(int x, int y) {
            return x + y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 9", new string[] { "positive", "addchecked", "operators", "Pri1" })]
        public static Expr AddChecked9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddChecked).GetMethod("AddInts");

            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant(1000, typeof(Int32)), Expr.Constant(10000, typeof(Int32)), mi), Expr.Constant(11000, typeof(Int32))));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 10", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddChecked10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddChecked).GetMethod("AddInts");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.AddChecked(Expr.Constant((Int16)1), Expr.Constant((Int16)2), mi), Expr.Constant((Int16)3));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static string AddExceptionMsg(Exception e1, Exception e2) {
            return e1.Message + e2.Message;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 11", new string[] { "positive", "addchecked", "operators", "Pri1" })]
        public static Expr AddChecked11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddChecked).GetMethod("AddExceptionMsg");

            Expr Res =
                Expr.AddChecked(
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
        public static int AddNullableInt(int? x, int y) {
            return x.GetValueOrDefault() + y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 12", new string[] { "positive", "addchecked", "operators", "Pri1" })]
        public static Expr AddChecked12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddChecked).GetMethod("AddNullableInt");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddChecked(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(3, typeof(Int32)), "Add 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddChecked(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(1, typeof(Int32)), "Add 2"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 13", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddChecked13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.AddChecked(Expr.Constant(1, typeof(Int32)), Expr.Constant((Nullable<int>)(-10), typeof(Nullable<int>)), mi), Expr.Constant(-9, typeof(Int32))
                )
        ;
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid methodinfo that returns boolean, pass arguments of nullable type. 
        public static bool IsTrueNullable(bool? x, bool? y) {
            return x.GetValueOrDefault() && y.GetValueOrDefault();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 14", new string[] { "positive", "addchecked", "operators", "Pri1" })]
        public static Expr AddChecked14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Add).GetMethod("IsTrueNullable");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddChecked(
                        Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)),
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(false, typeof(bool)),
                    "AddChecked 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddChecked(
                        Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)),
                        Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(false, typeof(bool)),
                    "AddChecked 2"
                )
            );
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo that returns boolean, pass arguments of nullable type
        public static bool IsTrue(bool x, bool y) {
            return x && y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 14_1", new string[] { "positive", "addchecked", "operators", "Pri1" })]
        public static Expr AddChecked14_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Add).GetMethod("IsTrue");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddChecked(
                        Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)),
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                    "AddChecked 1"
                )
            );
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Adding across mixed types
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "AddChecked 15", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddChecked15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddChecked(Expr.Constant(1), Expr.Constant(2.0), mi), Expr.Constant(3)
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

            public static int operator +(MyVal v1, int v2) {
                return v1.Val + v1.Val + v2 + v2;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 16", new string[] { "positive", "addchecked", "operators", "Pri1" })]
        public static Expr AddChecked16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddChecked(Expr.Constant(new MyVal(1)), Expr.Constant(2), mi), Expr.Constant(6)
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Verify order of evaluation of expressions on add
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 17", new string[] { "positive", "addchecked", "operators", "Pri1" })]
        public static Expr AddChecked17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.AddChecked(
                    Expr.Block(EU.ConcatEquals(Result, "One"), Expr.Constant(1)),
                    Expr.Multiply(
                        Expr.Block(EU.ConcatEquals(Result, "Two"), Expr.Constant(2)),
                        Expr.AddChecked(
                            Expr.Block(EU.ConcatEquals(Result, "Three"), Expr.Constant(3)),
                            Expr.Constant(4)
                        )
                    ),
                    null
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(15)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("OneTwoThree")));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Add of values that overflow the type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 18", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr AddChecked18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant(2), Expr.Constant(Int32.MaxValue)), Expr.Constant(Int32.MinValue + 1)));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }

        // Add of values that overflow the type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 19", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr AddChecked19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expressions.Add(EU.GenAreEqual(Expr.AddChecked(Expr.Constant(-1), Expr.Constant(Int32.MinValue)), Expr.Constant(Int32.MaxValue)));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }

        // Add of values that overflow the type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked 20", new string[] { "negative", "addchecked", "operators", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr AddChecked20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddChecked(
                        Expr.Constant(2),
                        Expr.Constant(Int32.MaxValue)
                    ),
                    Expr.Constant(Int32.MinValue + 1),
                    "Add 1"
                )
            );

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }
    }
}
