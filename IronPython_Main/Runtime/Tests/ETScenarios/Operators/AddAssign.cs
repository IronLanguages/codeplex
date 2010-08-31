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

    public class AddAssign {
        // AddAssign of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 1", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(bool), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(true)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssign(Left, Expr.Constant(false), null);
            }));

            var tree = Expr.Block(new ParameterExpression[] { Left }, Expressions);

            return tree;
        }

        // AddAssign of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 2", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(string), "");

            Expressions.Add(Expr.Assign(Left, Expr.Constant("Hello")));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssign(Left, Expr.Constant("World"));
            }));

            var tree = Expr.Block(new ParameterExpression[] { Left }, Expressions);

            return tree;
        }

        public class TestClass {
            public int _x;
            internal TestClass(int val) { _x = val; }
        }

        // AddAssign of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 3", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssign(Left, Right, null);
            }));

            var tree = Expr.Block(new ParameterExpression[] { Left, Right }, Expressions);

            return tree;
        }

        public struct TestStruct {
            public int _x;
            public TestStruct(int val) { _x = val; }
        }

        // AddAssign of struct object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 4", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssign(Left, Right);
            }));

            var tree = Expr.Block(new ParameterExpression[] { Left, Right }, Expressions);

            return tree;
        }


        // AddAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // TODO: AddAssign tests for SByte and Byte once they support AddAssignChecked (Dev10 Bug 502521)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 5", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Binary Operator not defined for the following two:
            //Expressions.Add(EU.GenAreEqual(Expr.AddAssign(Expr.Constant((SByte)1), Expr.Constant((SByte)2)), Expr.Constant((SByte)3))); // Int16 is CLS compliant equivalent type
            //Expressions.Add(EU.GenAreEqual(Expr.AddAssign(Expr.Constant((Byte)1), Expr.Constant((Byte)2)), Expr.Constant((Byte)3)));

            ParameterExpression v1 = Expr.Parameter(typeof(Int16), "");
            Expressions.Add(Expr.Assign(v1, Expr.Constant((Int16)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v1, Expr.Constant((Int16)2)), Expr.Constant((Int16)3), "AddAssign 1"));
            ParameterExpression v2 = Expr.Parameter(typeof(UInt16), "");
            Expressions.Add(Expr.Assign(v2, Expr.Constant((UInt16)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v2, Expr.Constant((UInt16)2)), Expr.Constant((UInt16)3), "AddAssign 2"));
            ParameterExpression v3 = Expr.Parameter(typeof(short), "");
            Expressions.Add(Expr.Assign(v3, Expr.Constant((short)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v3, Expr.Constant((short)2)), Expr.Constant((short)3), "AddAssign 3"));
            ParameterExpression v4 = Expr.Parameter(typeof(ushort), "");
            Expressions.Add(Expr.Assign(v4, Expr.Constant((ushort)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v4, Expr.Constant((ushort)2)), Expr.Constant((ushort)3), "AddAssign 4"));
            ParameterExpression v5 = Expr.Parameter(typeof(Int32), "");
            Expressions.Add(Expr.Assign(v5, Expr.Constant((Int32)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v5, Expr.Constant((Int32)2)), Expr.Constant((Int32)3), "AddAssign 5"));
            ParameterExpression v6 = Expr.Parameter(typeof(UInt32), "");
            Expressions.Add(Expr.Assign(v6, Expr.Constant((UInt32)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v6, Expr.Constant((UInt32)2)), Expr.Constant((UInt32)3), "AddAssign 6"));
            ParameterExpression v7 = Expr.Parameter(typeof(long), "");
            Expressions.Add(Expr.Assign(v7, Expr.Constant((long)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v7, Expr.Constant((long)2.0)), Expr.Constant((long)3.0), "AddAssign 7"));
            ParameterExpression v8 = Expr.Parameter(typeof(ulong), "");
            Expressions.Add(Expr.Assign(v8, Expr.Constant((ulong)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v8, Expr.Constant((ulong)2.0)), Expr.Constant((ulong)3.0), "AddAssign 8"));
            ParameterExpression v9 = Expr.Parameter(typeof(Single), "");
            Expressions.Add(Expr.Assign(v9, Expr.Constant((Single)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v9, Expr.Constant((Single)2.0)), Expr.Constant((Single)3.0), "AddAssign 9"));
            ParameterExpression v10 = Expr.Parameter(typeof(double), "");
            Expressions.Add(Expr.Assign(v10, Expr.Constant((double)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v10, Expr.Constant((Double)2.0)), Expr.Constant((Double)3.0), "AddAssign 10"));
            ParameterExpression v11 = Expr.Parameter(typeof(decimal), "");
            Expressions.Add(Expr.Assign(v11, Expr.Constant((decimal)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(v11, Expr.Constant((decimal)2.0)), Expr.Constant((decimal)3.0), "AddAssign 11"));


            var tree = Expr.Block(new ParameterExpression[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 6", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(Left, Expr.Constant(2, typeof(Int32)), null), Expr.Constant(3, typeof(Int32))));

            var tree = Expr.Block(new ParameterExpression[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int AddAssignNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 7", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignNoArgs");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            EU.Throws<ArgumentException>(() => { Expr.AddAssign(Left, Expr.Constant(2, typeof(Int32)), mi); });

            return Expr.Empty();
        }

        // pass a MethodInfo that takes a paramarray
        public static int AddAssignParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 8", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignParamArray");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            EU.Throws<ArgumentException>(() => { Expr.AddAssign(Left, Expr.Constant(2, typeof(Int32)), mi); });

            return Expr.Empty();
        }

        // with a valid MethodInfo, AddAssign two values of the same type
        public static int AddAssignInts(int x, int y) {
            return x + y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 9", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignInts");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.GenAreEqual(Expr.AddAssign(Left, Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32))));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 10", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignInts");

            ParameterExpression Left = Expr.Parameter(typeof(Int16), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Int16)1, typeof(Int16))));
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AddAssign(Left, Expr.Constant((Int16)2), mi), Expr.Constant((Int16)3)); }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static DivideByZeroException AddAssignExceptionMsg(Exception e1, Exception e2) {
            return new DivideByZeroException(e1.Message + e2.Message);
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 11", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignExceptionMsg");

            ParameterExpression Left = Expr.Parameter(typeof(DivideByZeroException), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException))));

            Expr Res =
                Expr.AddAssign(
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
        public static int? AddAssignNullableInt(int? x, int y) {
            return (x ?? 0) + y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 12", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignNullableInt");


            ParameterExpression Left = Expr.Parameter(typeof(Nullable<int>), "12");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(Nullable<int>))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(Left, Expr.Constant(1), mi), Expr.Constant(2, typeof(Int32?)), "AddAssign 1"
                )
            );

            Expressions.Add(Expr.Assign(Left, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(Left, Expr.Constant(1), mi), Expr.Constant(1, typeof(Int32?)), "AddAssign 2"
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 13", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Parameter(typeof(int), "13");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.AddAssign(Left, Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), mi), Expr.Constant(3, typeof(Int32))
                )
        ;
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // With a valid methodinfo that returns boolean, pass arguments of nullable type. 
        public static bool? IsTrueNullable(bool? x, bool? y) {
            return x.GetValueOrDefault() && y.GetValueOrDefault();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 14", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("IsTrueNullable");

            ParameterExpression Left = Expr.Parameter(typeof(Nullable<bool>), "14");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(
                        Left,
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(false, typeof(bool?)),
                    "AddAssign 1"
                )
            );

            Expressions.Add(Expr.Assign(Left, Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(
                        Left,
                        Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(false, typeof(bool?)),
                    "AddAssign 2"
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

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 14_1", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign14_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("IsTrue");

            ParameterExpression Left = Expr.Parameter(typeof(Nullable<bool>), "14_1");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>))));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(
                        Left,
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                    "AddAssign 1"
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // AddAssigning across mixed types
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "AddAssign 15", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(Left, Expr.Constant(2.0), mi), Expr.Constant(3)
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

            public static int operator +(MyVal v1, int v2) {
                return v1.Val + v1.Val + v2 + v2;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "AddAssign 16", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Parameter(typeof(MyVal), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new MyVal(1))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(Left, Expr.Constant(2), mi), Expr.Constant(6)
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

        // Verify order of evaluation of expressions on AddAssign
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 17", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            TestOrder order = new TestOrder();

            ParameterExpression ov = Expr.Parameter(typeof(TestOrder), "order");

            Expressions.Add(Expr.Assign(ov, Expr.Constant(order)));
            Expressions.Add(Expr.Assign(Expr.Property(ov, order.pi, Expr.Constant("")), Expr.Constant(1)));


            Expr Res =
                Expr.AddAssign(
                    Expr.Property(ov, order.pi, Expr.Constant("One")),
                    Expr.AddAssign(
                        Expr.Property(ov, order.pi, Expr.Constant("Two")),
                        Expr.AddAssign(
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

        // AddAssign of values that overflow the type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 18", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left1 = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left1, Expr.Constant((int)2)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(
                        Left1,
                        Expr.Constant(Int32.MaxValue)
                    ),
                    Expr.Constant(Int32.MinValue + 1),
                    "AddAssign 1"
                )
            );

            ParameterExpression Left2 = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left2, Expr.Constant((int)-1)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(
                        Left2,
                        Expr.Constant(Int32.MinValue)
                    ),
                    Expr.Constant(Int32.MaxValue),
                    "AddAssign 2"
                )
            );

            ParameterExpression Left3 = Expr.Parameter(typeof(double), "");
            Expressions.Add(Expr.Assign(Left3, Expr.Constant(double.MaxValue)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(
                        Left3,
                        Expr.Constant((double)(Double.MaxValue))
                    ),
                    Expr.Constant((double)(Double.PositiveInfinity)),
                    "AddAssign 3"
                )
            );

            ParameterExpression Left4 = Expr.Parameter(typeof(double), "");
            Expressions.Add(Expr.Assign(Left4, Expr.Constant(double.MinValue)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AddAssign(
                        Left4,
                        Expr.Constant((double)(Double.MinValue))
                    ),
                    Expr.Constant((double)(Double.NegativeInfinity)),
                    "AddAssign 4"
                )
            );

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "dont_visit_node");
            var tree = Expr.Block(new ParameterExpression[] { Left1, Left2, Left3, Left4, DoNotVisitTemp }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass a constant to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 19", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AddAssign(Expr.Constant(1), Expr.Constant(2));
            }));

            var tree = Expr.Block(Expressions);

            return tree;
        }

        //Pass a non assignable expression to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 20", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignInts");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.AddAssign(Expr.Constant(1), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // AddAssign with an array index expression as the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 24", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Variable(typeof(int[]), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6), Expr.Constant(7), Expr.Constant(8) })));

            Expressions.Add(Expr.AddAssign(Expr.ArrayAccess(Left, Expr.Constant(2)), Expr.Constant(4)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10), Expr.ArrayIndex(Left, Expr.Constant(2)), "SA 1"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // with a valid MethodInfo, AddAssign two values of the same type
        public static long BadReturn(int x, int y) {
            return (long)x + y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 25", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("BadReturn");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AddAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass null to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 26", new string[] { "negative", "AddAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr AddAssign26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.AddAssign(null, Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        //Pass null to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 27", new string[] { "negative", "AddAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr AddAssign27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.AddAssign(Left, null);
            }));

            return Expr.Empty();
        }

        //Pass a non value returning expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 29", new string[] { "negative", "AddAssign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssign(Left, Expr.Empty());
            }));

            var tree = Expr.Empty();

            return tree;
        }

        //Pass a Block to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 30", new string[] { "negative", "AddAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Block(Expr.Constant(1));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.AddAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a Method Call to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 31", new string[] { "negative", "AddAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Call(Expr.Constant("1"), typeof(string).GetMethod("ToString", new Type[] { }));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.AddAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }


        // pass a MethodInfo that takes three arguments
        public static int AddAssign3Args(int arg1, int arg2, int arg3) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 34", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssign3Args");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AddAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }

        // pass a MethodInfo with two arguments, that returns void
        public static void AddAssign2ArgsVoid(int arg1, int arg2) {
            return;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 35", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssign2ArgsVoid");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AddAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }


        //with a valid method info, pass two values of an enum class (methodinfo's arguments are of the base integer type)
        public static int AddAssign2Args(int arg1, int arg2) {
            return arg1 + arg2;
        }
        enum e36 : int {
        }
        //enum -> int conversion not being accepted.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 36", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssign2Args");

            ParameterExpression Left = Expr.Parameter(typeof(e36), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((e36)1)));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssign(Left, Expr.Constant((e36)2), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type
        public static int AddAssignMethod37(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 37", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignMethod37");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(Expr.AddAssign(Left, Expr.Constant(2, typeof(int?)), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //with a valid method info, pass two values of the same nullable type
        public static int AddAssignMethod38(int? arg1, int? arg2) {
            return (arg1 + arg2).GetValueOrDefault();
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 38", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignMethod38");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AddAssign(Left, Expr.Constant(2, typeof(int?)), mi);
            }));

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }

        //with a valid method info, pass two values of the same nullable type. return is nullable, arguments aren't
        public static int? AddAssignMethod39(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 39", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignMethod39");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.AddAssign(Left, Expr.Constant(2, typeof(int?)), mi);
            }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }





        //User defined operator on right argument, arguments are of the proper types
        public class C40 {
            public int Val = 2;
            public static int operator +(int a, C40 b) {
                return b.Val + a;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 40", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(Expr.AddAssign(Left, Expr.Constant(new C40())));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 41", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C41), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C41("1"))));

            Expressions.Add(Expr.AddAssign(Left, Expr.Constant(new ArgumentException("2"))));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 42", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(Exception), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new Exception("1"))));

            Expressions.Add(Expr.AddAssign(Left, Expr.Constant(new C42("2"))));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 43", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C43), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C43("1"))));

            Expressions.Add(Expr.AddAssign(Left, Expr.Constant(new C43_1("2"))));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 44", new string[] { "positive", "AddAssign", "operators", "Pri2" })]
        public static Expr AddAssign44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C44), "instance");

            Expr Left = Expr.Field(Instance, C44.Field);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C44())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.AddAssign(Left, Expr.Constant(5)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 45", new string[] { "positive", "AddAssign", "operators", "Pri2" })]
        public static Expr AddAssign45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Field(null, C45.Field);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.AddAssign(Left, Expr.Constant(5)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 46", new string[] { "positive", "AddAssign", "operators", "Pri2" })]
        public static Expr AddAssign46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C46), "instance");

            Expr Left = Expr.Property(Instance, C46.Property);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C46())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.AddAssign(Left, Expr.Constant(5)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 47", new string[] { "positive", "AddAssign", "operators", "Pri2" })]
        public static Expr AddAssign47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Property(null, C47.Property);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.AddAssign(Left, Expr.Constant(5)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 48", new string[] { "positive", "AddAssign", "operators", "Pri2" })]
        public static Expr AddAssign48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C48), "instance");

            Expr Left = Expr.Property(Instance, C48.Property, new[] { Expr.Constant(2) });

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C48())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(3)));

            Expressions.Add(Expr.AddAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(16), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }


        public static int AddAssignConv(short arg1, short arg2) {
            return (int)(arg1 + arg2);
        }
        //AddAssign with a lambda conversion for the return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 49", new string[] { "positive", "AddAssign", "operators", "Pri1" })]
        public static Expr AddAssign49(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(short)), arg1
                                                );
            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignConv");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((short)1)));
            Expressions.Add(
                            EU.GenAreEqual(
                                            Expr.Constant((short)3),
                                            Expr.AddAssign(Left, Expr.Constant((short)2), mi, Conv)
                                           )
                           );


            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //AddAssign with a lambda conversion for the return type, to the wrong type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 50", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign50(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(byte)), arg1
                                                );
            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignConv");

            EU.Throws<InvalidOperationException>(() => { Expr.AddAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }

        //Passing null to conversion lambda
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 51", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AddAssign51(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            LambdaExpression Conv = null;
            MethodInfo mi = typeof(AddAssign).GetMethod("AddAssignConv");

            EU.Throws<ArgumentException>(() => { Expr.AddAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }


        //Passing a conversion lambda when it's not needed
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddAssign 52", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AddAssign52(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(int)), arg1
                                                );

            MethodInfo mi = null;// typeof(AddAssign).GetMethod("AddAssignInts");

            EU.Throws<InvalidOperationException>(() => { Expr.AddAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }
    }
}
