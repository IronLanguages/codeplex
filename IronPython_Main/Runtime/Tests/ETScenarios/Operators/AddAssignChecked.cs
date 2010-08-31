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

    public class AddAssignChecked {
        // AddAssignChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // TODO: Add tests for SByte and Byte once they support AddAssignChecked (Dev10 Bug 502521)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 1", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Binary Operator not defined for the following two:
            //Expressions.Add(EU.GenAreEqual(Expr.Add(Expr.Constant((SByte)1), Expr.Constant((SByte)2)), Expr.Constant((SByte)3))); // Int16 is CLS compliant equivalent type
            //Expressions.Add(EU.GenAreEqual(Expr.Add(Expr.Constant((Byte)1), Expr.Constant((Byte)2)), Expr.Constant((Byte)3)));

            ParameterExpression v1 = Expr.Parameter(typeof(Int16), "");
            Expressions.Add(Expr.Assign(v1, Expr.Constant((Int16)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v1, Expr.Constant((Int16)2)), Expr.Constant((Int16)3), "AddAssignChecked 1"));
            ParameterExpression v2 = Expr.Parameter(typeof(UInt16), "");
            Expressions.Add(Expr.Assign(v2, Expr.Constant((UInt16)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v2, Expr.Constant((UInt16)2)), Expr.Constant((UInt16)3), "AddAssignChecked 2"));
            ParameterExpression v3 = Expr.Parameter(typeof(short), "");
            Expressions.Add(Expr.Assign(v3, Expr.Constant((short)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v3, Expr.Constant((short)2)), Expr.Constant((short)3), "AddAssignChecked 3"));
            ParameterExpression v4 = Expr.Parameter(typeof(ushort), "");
            Expressions.Add(Expr.Assign(v4, Expr.Constant((ushort)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v4, Expr.Constant((ushort)2)), Expr.Constant((ushort)3), "AddAssignChecked 4"));
            ParameterExpression v5 = Expr.Parameter(typeof(Int32), "");
            Expressions.Add(Expr.Assign(v5, Expr.Constant((Int32)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v5, Expr.Constant((Int32)2)), Expr.Constant((Int32)3), "AddAssignChecked 5"));
            ParameterExpression v6 = Expr.Parameter(typeof(UInt32), "");
            Expressions.Add(Expr.Assign(v6, Expr.Constant((UInt32)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v6, Expr.Constant((UInt32)2)), Expr.Constant((UInt32)3), "AddAssignChecked 6"));
            ParameterExpression v7 = Expr.Parameter(typeof(long), "");
            Expressions.Add(Expr.Assign(v7, Expr.Constant((long)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v7, Expr.Constant((long)2.0)), Expr.Constant((long)3.0), "AddAssignChecked 7"));
            ParameterExpression v8 = Expr.Parameter(typeof(ulong), "");
            Expressions.Add(Expr.Assign(v8, Expr.Constant((ulong)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v8, Expr.Constant((ulong)2.0)), Expr.Constant((ulong)3.0), "AddAssignChecked 8"));
            ParameterExpression v9 = Expr.Parameter(typeof(Single), "");
            Expressions.Add(Expr.Assign(v9, Expr.Constant((Single)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v9, Expr.Constant((Single)2.0)), Expr.Constant((Single)3.0), "AddAssignChecked 9"));
            ParameterExpression v10 = Expr.Parameter(typeof(double), "");
            Expressions.Add(Expr.Assign(v10, Expr.Constant((double)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v10, Expr.Constant((Double)2.0)), Expr.Constant((Double)3.0), "AddAssignChecked 10"));
            ParameterExpression v11 = Expr.Parameter(typeof(decimal), "");
            Expressions.Add(Expr.Assign(v11, Expr.Constant((decimal)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(v11, Expr.Constant((decimal)2.0)), Expr.Constant((decimal)3.0), "AddAssignChecked 11"));

            var tree = Expr.Block(new ParameterExpression[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // AddAssignChecked of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 2", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(bool), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(true)));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssignChecked(Left, Expr.Constant(false), null);
            }));

            var tree = Expr.Block(new ParameterExpression[] { Left }, Expressions);

            return tree;
        }

        // AddAssignChecked of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 3", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(string), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant("Hello")));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssignChecked(Left, Expr.Constant("World"));
            }));

            var tree = Expr.Block(new ParameterExpression[] { Left }, Expressions);

            return tree;
        }

        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        // AddAssignChecked of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 4", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssignChecked(Left, Right, null);
            }));

            var tree = Expr.Block(new ParameterExpression[] { Left }, Expressions);

            return tree;
        }

        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        // AddAssignChecked of struct object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 5", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssignChecked(Left, Right);
            }));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);

            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 6", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(-1)));

            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(Left, Expr.Constant(1, typeof(Int32)), null), Expr.Constant(0, typeof(Int32))));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int AddNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 7", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssignChecked7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddNoArgs");

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.AddAssignChecked(Left, Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }

        // pass a MethodInfo that takes a paramarray
        public static int AddParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 8", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssignChecked8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddParamArray");

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.AddAssignChecked(Left, Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }

        // with a valid MethodInfo, add two values of the same type
        public static int AddInts(int x, int y) {
            return x + y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 9", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddInts");

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1000)));

            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(Left, Expr.Constant(10000, typeof(Int32)), mi), Expr.Constant(11000, typeof(Int32))));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 10", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddInts");

            ParameterExpression Left = Expr.Variable(typeof(Int16), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Int16)1)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.AddAssignChecked(Left, Expr.Constant((Int16)2), mi), Expr.Constant((Int16)3));
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static DivideByZeroException AddExceptionMsg(Exception e1, Exception e2) {
            return new DivideByZeroException(e1.Message + e2.Message);
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 11", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddExceptionMsg");

            ParameterExpression Left = Expr.Variable(typeof(DivideByZeroException), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException))));

            Expr Res =
                Expr.AddAssignChecked(
                    Left,
                    Expr.Constant(new RankException("Two"), typeof(RankException)),
                    mi
                );

            Expressions.Add(
                EU.GenAreEqual(
                    Expression.Property(Res, typeof(DivideByZeroException).GetProperty("Message")),
                    Expr.Constant("OneTwo")
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type. 
        public static int? AddNullableInt(int? x, int y) {
            return x.GetValueOrDefault() + y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 12", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddNullableInt");

            ParameterExpression Left = Expr.Variable(typeof(Nullable<int>), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2, typeof(int?))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssignChecked(Left, Expr.Constant(1), mi), Expr.Constant(3, typeof(Int32?)), "Add 1"
                )
            );

            Expressions.Add(Expr.Assign(Left, Expr.Constant(null, typeof(int?))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssignChecked(Left, Expr.Constant(1), mi), Expr.Constant(1, typeof(Int32?)), "Add 2"
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 13", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.AddAssignChecked(Left, Expr.Constant((Nullable<int>)(-10), typeof(Nullable<int>)), mi), Expr.Constant(-9, typeof(Int32))
                );
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // With a valid methodinfo that returns boolean, pass arguments of nullable type. 
        public static bool? IsTrueNullable(bool? x, bool? y) {
            return x.GetValueOrDefault() && y.GetValueOrDefault();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 14", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("IsTrueNullable");

            ParameterExpression Left = Expr.Variable(typeof(Nullable<bool>), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(true, typeof(bool?))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssignChecked(
                        Left,
                        Expr.Constant(false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(false, typeof(bool?)),
                    "AddAssignChecked 1"
                )
            );

            Expressions.Add(Expr.Assign(Left, Expr.Constant(true, typeof(bool?))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssignChecked(
                        Left,
                        Expr.Constant(null, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(false, typeof(bool?)),
                    "AddAssignChecked 2"
                )
            );
            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo that returns boolean, pass arguments of nullable type
        public static bool IsTrue(bool x, bool y) {
            return x && y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 14_1", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked14_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Add).GetMethod("IsTrue");

            ParameterExpression Left = Expr.Variable(typeof(Nullable<bool>), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(true, typeof(Nullable<bool>))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.AddAssignChecked(
                        Left,
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(false, typeof(bool)),
                    "AddAssignChecked 1"
                );
            }));
            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // Adding across mixed types
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "AddAssignChecked 15", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Variable(typeof(Nullable<int>), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssignChecked(Left, Expr.Constant(2.0), mi), Expr.Constant(3)
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, false);
            return tree;
        }

        // User defined overloaded operator on left argument, arguments are the proper types
        public class MyVal {
            public int Val { get; set; }

            public MyVal(int x) { Val = x; }

            public static MyVal operator +(MyVal v1, int v2) {
                return new MyVal(v1.Val + v1.Val + v2 + v2);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 16", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Variable(typeof(MyVal), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new MyVal(1))));

            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(2), mi));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Property(Left, typeof(MyVal).GetProperty("Val")), Expr.Constant(6)
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class TestOrder {
            public string res;
            private int _Value;
            public int this[string append] {
                get {
                    res += append;
                    return _Value;
                }
                set {
                    res += append;
                    _Value = value;
                }
            }

            public PropertyInfo pi = typeof(TestOrder).GetProperty("Item");
            public FieldInfo resi = typeof(TestOrder).GetField("res");
        }

        // Verify order of evaluation of expressions on AddAssignChecked
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 17", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            TestOrder order = new TestOrder();

            ParameterExpression ov = Expr.Parameter(typeof(TestOrder), "order");

            Expressions.Add(Expr.Assign(ov, Expr.Constant(order)));
            Expressions.Add(Expr.Assign(Expr.Property(ov, order.pi, Expr.Constant("")), Expr.Constant(1)));


            Expr Res =
                Expr.AddAssignChecked(
                    Expr.Property(ov, order.pi, Expr.Constant("One")),
                    Expr.AddAssignChecked(
                        Expr.Property(ov, order.pi, Expr.Constant("Two")),
                        Expr.AddAssignChecked(
                            Expr.Property(ov, order.pi, Expr.Constant("Three")),
                            Expr.Constant(4)
                        )
                    ),
                    null
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(7)));
            Expressions.Add(EU.GenAreEqual(Expr.Field(ov, order.resi), Expr.Constant("OneTwoThreeThreeTwoOne")));

            var tree = Expr.Block(new[] { ov }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Add of values that overflow the type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 18", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr AddAssignChecked18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(Left, Expr.Constant(Int32.MaxValue)), Expr.Constant(Int32.MinValue + 1)));

            var tree = Expr.Block(new[] { Result, Left }, Expressions);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }

        // Add of values that overflow the type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 19", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr AddAssignChecked19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(-1)));

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expressions.Add(EU.GenAreEqual(Expr.AddAssignChecked(Left, Expr.Constant(Int32.MinValue)), Expr.Constant(Int32.MaxValue)));

            var tree = Expr.Block(new[] { Result, Left }, Expressions);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }

        // Add of values that overflow the type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 20", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr AddAssignChecked20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssignChecked(
                        Left,
                        Expr.Constant(Int32.MaxValue)
                    ),
                    Expr.Constant(Int32.MinValue + 1),
                    "Add 1"
                )
            );

            var tree = Expr.Block(new[] { Result, Left }, Expressions);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }

        //Pass a non assignable expression to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 20_1", new string[] { "negative", "AddAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssignChecked20_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddAssignCheckedInts");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.AddAssignChecked(Expr.Constant(1), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(Expressions);
        }


        // AddAssignChecked with an array index expression as the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 24", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Variable(typeof(int[]), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6), Expr.Constant(7), Expr.Constant(8) })));

            Expressions.Add(Expr.AddAssignChecked(Expr.ArrayAccess(Left, Expr.Constant(2)), Expr.Constant(4)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10), Expr.ArrayIndex(Left, Expr.Constant(2)), "SA 1"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 25", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("BadReturn");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AddAssignChecked(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }

        //Pass null to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 26", new string[] { "negative", "AddAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr AddAssignChecked26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.AddAssignChecked(null, Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        //Pass null to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 27", new string[] { "negative", "AddAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr AddAssignChecked27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.AddAssignChecked(Left, null);
            }));

            return Expr.Empty();
        }

        //Pass a non value returning expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 29", new string[] { "negative", "AddAssignChecked", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssignChecked(Left, Expr.Empty());
            }));

            var tree = Expr.Empty();

            return tree;
        }

        //Pass a Block to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 30", new string[] { "negative", "AddAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssignChecked30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Block(Expr.Constant(1));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a Method Call to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 31", new string[] { "negative", "AddAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssignChecked31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Call(Expr.Constant("1"), typeof(string).GetMethod("ToString", new Type[] { }));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a constant to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 33", new string[] { "negative", "AddAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssignChecked33(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Constant(1);
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        // pass a MethodInfo that takes three arguments
        public static int AddAssignChecked3Args(int arg1, int arg2, int arg3) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 34", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssignChecked34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddAssignChecked3Args");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AddAssignChecked(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }

        // pass a MethodInfo with two arguments, that returns void
        public static void AddAssignChecked2ArgsVoid(int arg1, int arg2) {
            return;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 35", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssignChecked35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddAssignChecked2ArgsVoid");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AddAssignChecked(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }



        //with a valid method info, pass two values of an enum class (methodinfo's arguments are of the base integer type)
        public static int AddAssignChecked2Args(int arg1, int arg2) {
            return arg1 + arg2;
        }
        enum e36 : int {
        }
        //enum -> int conversion not being accepted.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 36", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddAssignChecked2Args");

            ParameterExpression Left = Expr.Parameter(typeof(e36), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((e36)1)));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssignChecked(Left, Expr.Constant((e36)2), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type
        public static int AddAssignCheckedMethod37(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 37", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddAssignCheckedMethod37");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(2, typeof(int?)), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //with a valid method info, pass two values of the same nullable type
        public static int AddAssignCheckedMethod38(int? arg1, int? arg2) {
            return (arg1 + arg2).GetValueOrDefault();
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 38", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssignChecked38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddAssignCheckedMethod38");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AddAssignChecked(Left, Expr.Constant(2, typeof(int?)), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type. return is nullable, arguments aren't
        public static int? AddAssignCheckedMethod39(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 39", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddAssignCheckedMethod39");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssignChecked(Left, Expr.Constant(2, typeof(int?)), mi);
            }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Empty();

            return tree;
        }

        //User defined operator on right argument, arguments are of the proper types
        public class C40 {
            public int Val = 2;
            public static int operator +(int a, C40 b) {
                return b.Val + a;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 40", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(new C40())));

            Expressions.Add(EU.GenAreEqual(Left, Expr.Constant(3)));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }


        //User defined operator on left argument, right argument is convertible
        public class C41 {
            public string Val;

            public C41(string init) {
                Val = init;
            }
            public static C41 operator +(C41 b, Exception a) {
                return new C41(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 41", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C41), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C41("1"))));

            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(new ArgumentException("2"))));

            Expressions.Add(EU.GenAreEqual(Expr.Field(Left, typeof(C41).GetField("Val")), Expr.Constant("12")));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }


        //User defined operator on right argument, left argument is convertible
        public class C42 {
            public string Val;

            public C42(string init) {
                Val = init;
            }
            public static Exception operator +(Exception a, C42 b) {
                return new Exception(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 42", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(Exception), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new Exception("1"))));

            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(new C42("2"))));

            Expressions.Add(EU.GenAreEqual(Expr.Property(Left, typeof(Exception).GetProperty("Message")), Expr.Constant("21")));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }


        //User defined operators exist for both arguments.
        public class C43 {
            public string Val;

            public C43(string init) {
                Val = init;
            }
            public static C43 operator +(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "A");
            }
        }
        public class C43_1 {
            public string Val;

            public C43_1(string init) {
                Val = init;
            }
            public static C43 operator +(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "B");
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 43", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C43), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C43("1"))));

            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(new C43_1("2"))));

            Expressions.Add(EU.GenAreEqual(Expr.Field(Left, typeof(C43).GetField("Val")), Expr.Constant("12A")));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with an instance field as the left argument
        public class C44 {
            public int Val;
            public static FieldInfo Field = typeof(C44).GetField("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 44", new string[] { "positive", "AddAssignChecked", "operators", "Pri2" })]
        public static Expr AddAssignChecked44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C44), "instance");

            Expr Left = Expr.Field(Instance, C44.Field);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C44())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(7), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with a static field as the left argument
        public class C45 {
            public static int Val;
            public static FieldInfo Field = typeof(C45).GetField("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 45", new string[] { "positive", "AddAssignChecked", "operators", "Pri2" })]
        public static Expr AddAssignChecked45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Field(null, C45.Field);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(7), Left));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        //Add with an instance Property as the left argument
        public class C46 {
            public int Val { get; set; }
            public static PropertyInfo Property = typeof(C46).GetProperty("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 46", new string[] { "positive", "AddAssignChecked", "operators", "Pri2" })]
        public static Expr AddAssignChecked46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C46), "instance");

            Expr Left = Expr.Property(Instance, C46.Property);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C46())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(7), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with a static Property as the left argument
        public class C47 {
            public static int Val { get; set; }
            public static PropertyInfo Property = typeof(C47).GetProperty("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 47", new string[] { "positive", "AddAssignChecked", "operators", "Pri2" })]
        public static Expr AddAssignChecked47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Property(null, C47.Property);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(7), Left));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }



        //Add with a parameterized instance Property as the left argument
        public class C48 {
            private int m_this;
            public int this[int x] {
                get {
                    return m_this + x;
                }
                set {
                    m_this = value + x;
                }
            }
            public static PropertyInfo Property = typeof(C48).GetProperty("Item");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 48", new string[] { "positive", "AddAssignChecked", "operators", "Pri2" })]
        public static Expr AddAssignChecked48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C48), "instance");

            Expr Left = Expr.Property(Instance, C48.Property, new[] { Expr.Constant(2) });

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C48())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(3)));

            Expressions.Add(Expr.AddAssignChecked(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(16), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }


        public static int AddAssignCheckedConv(short arg1, short arg2) {
            return (int)(arg1 + arg2);
        }
        //AddAssignChecked with a lambda conversion for the return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 49", new string[] { "positive", "AddAssignChecked", "operators", "Pri1" })]
        public static Expr AddAssignChecked49(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(short)), arg1
                                                );
            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddAssignCheckedConv");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((short)1)));
            Expressions.Add(
                            EU.GenAreEqual(
                                            Expr.Constant((short)3),
                                            Expr.AddAssignChecked(Left, Expr.Constant((short)2), mi, Conv)
                                           )
                           );


            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //AddAssignChecked with a lambda conversion for the return type, to the wrong type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 50", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked50(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(byte)), arg1
                                                );
            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddAssignCheckedConv");

            EU.Throws<InvalidOperationException>(() => { Expr.AddAssignChecked(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }

        //Passing null to conversion lambda
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 51", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssignChecked51(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            LambdaExpression Conv = null;
            MethodInfo mi = typeof(AddAssignChecked).GetMethod("AddAssignCheckedConv");

            EU.Throws<ArgumentException>(() => { Expr.AddAssignChecked(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }

        //Passing a conversion lambda when it's not needed
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssignChecked 52", new string[] { "negative", "AddAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssignChecked52(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(int)), arg1
                                                );

            MethodInfo mi = null;// typeof(AddAssignChecked).GetMethod("AddAssignCheckedInts");

            EU.Throws<InvalidOperationException>(() => { Expr.AddAssignChecked(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }
    }
}
