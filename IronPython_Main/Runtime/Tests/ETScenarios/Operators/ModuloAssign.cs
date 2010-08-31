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

    public class ModuloAssign {
        // ModuloAssign of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 1", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(bool), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(true)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(false)), Expr.Constant(false)); }));
            return Expr.Empty();
        }

        // ModuloAssign of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 2", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(string), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant("Hello")));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.ModuloAssign(Left, Expr.Constant("World")),
                    Expr.Constant(false));
            }));

            var tree = Expr.Block(new ParameterExpression[] { Left }, Expressions);

            return tree;
        }

        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        // ModuloAssign of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 3", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.ModuloAssign(Left, Right);
            }));

            var tree = Expr.Block(new ParameterExpression[] { Left, Right }, Expressions);

            return tree;
        }

        // ModuloAssign of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 4", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.ModuloAssign(Left, Right);
            }));

            var tree = Expr.Block(new ParameterExpression[] { Left, Right }, Expressions);

            return tree;
        }

        // ModuloAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 5", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression v1 = Expr.Parameter(typeof(Int16), "");
            Expressions.Add(Expr.Assign(v1, Expr.Constant((Int16)32)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(v1, Expr.Constant((Int16)88, typeof(Int16))), Expr.Constant((Int16)32, typeof(Int16)), "ModuloAssign 1"));

            ParameterExpression v2 = Expr.Parameter(typeof(UInt16), "");
            Expressions.Add(Expr.Assign(v2, Expr.Constant((UInt16)3)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(v2, Expr.Constant((UInt16)2, typeof(UInt16))), Expr.Constant((UInt16)1, typeof(UInt16)), "ModuloAssign 2"));

            ParameterExpression v3 = Expr.Parameter(typeof(Int32), "");
            Expressions.Add(Expr.Assign(v3, Expr.Constant((Int32)4)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(v3, Expr.Constant((Int32)2, typeof(Int32))), Expr.Constant((Int32)0, typeof(Int32)), "ModuloAssign 3"));

            ParameterExpression v4 = Expr.Parameter(typeof(UInt32), "");
            Expressions.Add(Expr.Assign(v4, Expr.Constant((UInt32)32)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(v4, Expr.Constant((UInt32)3, typeof(UInt32))), Expr.Constant((UInt32)2, typeof(UInt32)), "ModuloAssign 4"));

            ParameterExpression v5 = Expr.Parameter(typeof(short), "");
            Expressions.Add(Expr.Assign(v5, Expr.Constant((short)1000)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(v5, Expr.Constant((short)(-101), typeof(short))), Expr.Constant((short)91, typeof(short)), "ModuloAssign 5"));

            ParameterExpression v6 = Expr.Parameter(typeof(ushort), "");
            Expressions.Add(Expr.Assign(v6, Expr.Constant((ushort)2)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(v6, Expr.Constant((ushort)2, typeof(ushort))), Expr.Constant((ushort)0, typeof(ushort)), "ModuloAssign 6"));

            ParameterExpression v7 = Expr.Parameter(typeof(long), "");
            Expressions.Add(Expr.Assign(v7, Expr.Constant((long)5)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(v7, Expr.Constant((long)1, typeof(long))), Expr.Constant((long)0, typeof(long)), "ModuloAssign 7"));

            ParameterExpression v8 = Expr.Parameter(typeof(ulong), "");
            Expressions.Add(Expr.Assign(v8, Expr.Constant((ulong)4)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(v8, Expr.Constant((ulong)2.0, typeof(ulong))), Expr.Constant((ulong)0, typeof(ulong)), "ModuloAssign 8"));

            double delta = 0.00001;


            ParameterExpression v9 = Expr.Parameter(typeof(Single), "");
            Expressions.Add(Expr.Assign(v9, Expr.Constant((Single)3.333)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.ModuloAssign(v9, Expr.Constant((Single)1.111)),
                        Expr.Constant((Single)(0 + delta))
                    ),
                    Expr.Constant(true),
                    "ModuloAssign 9"
                )
            );

            Expressions.Add(Expr.Assign(v9, Expr.Constant((Single)3.333)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.ModuloAssign(v9, Expr.Constant((Single)1.111)),
                        Expr.Constant((Single)(0 - delta))
                    ),
                    Expr.Constant(true),
                    "ModuloAssign 9_1"
                )
            );

            ParameterExpression v10 = Expr.Parameter(typeof(double), "");
            Expressions.Add(Expr.Assign(v10, Expr.Constant((double)33.333)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.ModuloAssign(v10, Expr.Constant((Double)11.1)),
                        Expr.Constant((Double)(.033 + delta))
                    ),
                    Expr.Constant(true),
                    "ModuloAssign 10"
                )
            );

            Expressions.Add(Expr.Assign(v10, Expr.Constant((double)33.333)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.ModuloAssign(v10, Expr.Constant((Double)11.1)),
                        Expr.Constant((Double)(.033 - delta))
                    ),
                    Expr.Constant(true),
                    "ModuloAssign 10_1"
                )
            );

            ParameterExpression v11 = Expr.Parameter(typeof(decimal), "");
            Expressions.Add(Expr.Assign(v11, Expr.Constant((decimal)9999.999)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.ModuloAssign(v11, Expr.Constant((decimal)1111.111)),
                        Expr.Constant((decimal)(0 + delta))
                    ),
                    Expr.Constant(true),
                    "ModuloAssign 11"
                )
            );

            Expressions.Add(Expr.Assign(v11, Expr.Constant((decimal)9999.999)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.ModuloAssign(v11, Expr.Constant((decimal)1111.111)),
                        Expr.Constant((decimal)(0 - delta))
                    ),
                    Expr.Constant(true),
                    "ModuloAssign 11_1"
                )
            );

            var tree = Expr.Block(new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 6", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)4)));

            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(4), null), Expr.Constant(0), "ModuloAssign 1"));

            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)4)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(3), null), Expr.Constant(1), "ModuloAssign 2"));

            ParameterExpression Left1 = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left1, Expr.Constant(4, typeof(int?))));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ModuloAssign(
                        Left1,
                        Expr.Constant(2, typeof(Nullable<int>)),
                        null
                    ),
                    Expr.Constant(0, typeof(Nullable<int>))
                )
            );

            Expressions.Add(Expr.Assign(Left1, Expr.Constant(4, typeof(int?))));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ModuloAssign(
                        Left1,
                        Expr.Constant(3, typeof(Nullable<int>)),
                        null
                    ),
                    Expr.Constant(1, typeof(Nullable<int>))
                )
            );

            var tree = Expr.Block(new[] { Left, Left1 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int ModuloAssignNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 7", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignNoArgs");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() => { EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(1, typeof(Int32)), mi), Expr.Constant(0, typeof(Int32))); }));

            return Expr.Empty();
        }

        // pass a MethodInfo that takes a paramarray
        public static int ModuloAssignParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 8", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignParamArray");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(10)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() => { EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(3, typeof(Int32)), mi), Expr.Constant(1, typeof(Int32))); }));

            return Expr.Block(new[] { Left }, Expressions);
        }

        // with a valid MethodInfo, ModuloAssign two values of the same type
        public static int ModuloAssignInts(int x, int y) {
            return x % y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 9", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignInts");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)10)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(3, typeof(Int32)), mi), Expr.Constant(1)));

            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)10)));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(10, typeof(Int32)), mi), Expr.Constant(0)));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 10", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignInts");

            ParameterExpression Left = Expr.Parameter(typeof(Int16), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Int16)3, typeof(Int16))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant((Int16)2), mi), Expr.Constant((Int16)1)); }));

            return Expr.Empty();
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static DivideByZeroException ModuloAssignExceptionMsg(Exception e1, Exception e2) {
            return new DivideByZeroException(e1.Message + e2.Message);
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 11", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignExceptionMsg");

            ParameterExpression Left = Expr.Parameter(typeof(DivideByZeroException), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException))));
            Expr Res =
                Expr.ModuloAssign(
                    Left,
                    Expr.Constant(new RankException("Two"), typeof(RankException)),
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Expr.Property(Res, typeof(DivideByZeroException).GetProperty("Message")), Expr.Constant("OneTwo"), "ModuloAssign 1"));


            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type. 
        public static int? ModuloAssignNullableInt(int? x, int y) {
            return x.GetValueOrDefault() % y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 12", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignNullableInt");

            ParameterExpression Left = Expr.Parameter(typeof(Nullable<int>), "12");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Nullable<int>)2, typeof(Nullable<int>))));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(1), mi), Expr.Constant(0, typeof(int?)), "ModuloAssign 1"));

            Expressions.Add(Expr.Assign(Left, Expr.Constant((Nullable<int>)3, typeof(Nullable<int>))));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(2), mi), Expr.Constant(1, typeof(int?)), "ModuloAssign 2"));

            Expressions.Add(Expr.Assign(Left, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>))));
            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(2), mi), Expr.Constant(0, typeof(int?)), "ModuloAssign 3"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 13", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Parameter(typeof(int), "13");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.ModuloAssign(Left, Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), mi), Expr.Constant(0)
                );
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // ModuloAssign of two values of different types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 14", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(bool), "14");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(true)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(1)), Expr.Constant(1), "ModuloAssign 1");
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // ModuloAssign across mixed types, no user defined operator
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "ModuloAssign 15", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ModuloAssign(Left, Expr.Constant(2.0), mi), Expr.Constant(3)
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

            public static MyVal operator %(MyVal v1, int v2) {
                return new MyVal(v1.Val % v2);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 16", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Parameter(typeof(MyVal), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new MyVal(8))));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(3), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Property(Left, typeof(MyVal).GetProperty("Val")), Expr.Constant(2)));

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

        // Verify order of evaluation of expressions on ModuloAssign
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 17", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            TestOrder order = new TestOrder();

            ParameterExpression ov = Expr.Parameter(typeof(TestOrder), "order");

            Expressions.Add(Expr.Assign(ov, Expr.Constant(order)));
            Expressions.Add(Expr.Assign(Expr.Property(ov, order.pi, Expr.Constant("")), Expr.Constant(9)));


            Expr Res =
                Expr.ModuloAssign(
                    Expr.Property(ov, order.pi, Expr.Constant("One")),
                    Expr.ModuloAssign(
                        Expr.Property(ov, order.pi, Expr.Constant("Two")),
                        Expr.ModuloAssign(
                            Expr.Property(ov, order.pi, Expr.Constant("Three")),
                            Expr.Constant(5)
                        )
                    ),
                    null
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(0)));
            Expressions.Add(EU.GenAreEqual(Expr.Field(ov, order.resi), Expr.Constant("OneTwoThreeThreeTwoOne")));

            var tree = Expr.Block(new[] { ov }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // With a valid methodinfo that returns boolean, pass arguments of nullable type. 
        public static int? ModLiftTest(int? x, int? y) {
            if (x.HasValue && y.HasValue)
                return x % y;
            return 0;
        }

        public static int? ModLiftTestReturnInt(int? x, int? y) {
            if (x.HasValue && y.HasValue)
                return (x % y == 0) ? 1 : 0;
            else
                return -1;
        }

        public static int ModLiftTestWithoutNullables(int x, int y) {
            return (x % y == 0) ? 1 : 0;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 18", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModLiftTest");

            ParameterExpression Left1 = Expr.Parameter(typeof(int?), "");

            Expressions.Add(Expr.Assign(Left1, Expr.Constant(5, typeof(int?))));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type. Set lift to null to false.
            Expr Res = Expr.ModuloAssign(
                        Left1,
                        Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(1, typeof(int?)), "ModuloAssign 1"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int?)));


            Expressions.Add(Expr.Assign(Left1, Expr.Constant(null, typeof(int?))));
            // With a valid methodinfo that returns boolean, pass arguments of nullable type with null values. Set lift to null to false.
            Res = Expr.ModuloAssign(
                        Left1,
                        Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(0, typeof(int?)), "ModuloAssign 2"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int?)));

            Expressions.Add(Expr.Assign(Left1, Expr.Constant(null, typeof(int?))));
            Res = Expr.ModuloAssign(
                        Left1,
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(0, typeof(int?)), "ModuloAssign 3"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int?)));

            mi = typeof(ModuloAssign).GetMethod("ModLiftTestReturnInt");

            Expressions.Add(Expr.Assign(Left1, Expr.Constant(null, typeof(int?))));
            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to true.
            Res = Expr.ModuloAssign(
                        Left1,
                        Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(-1, typeof(int?)), "ModuloAssign 4"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int?)));

            Expressions.Add(Expr.Assign(Left1, Expr.Constant(4, typeof(int?))));
            Res = Expr.ModuloAssign(
                        Left1,
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        mi
                    );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(1, typeof(int?)), "ModuloAssign 5"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int?)));

            Expressions.Add(Expr.Assign(Left1, Expr.Constant(2, typeof(int?))));
            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to false.
            Res = Expr.ModuloAssign(
                        Left1,
                        Expr.Constant((Nullable<int>)3, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(0, typeof(int?)), "ModuloAssign 6"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int?)));

            mi = typeof(ModuloAssign).GetMethod("ModLiftTestWithoutNullables");


            Expressions.Add(Expr.Assign(Left1, Expr.Constant(3, typeof(int?))));
            Res = Expr.ModuloAssign(
                        Left1,
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(1, typeof(Nullable<int>)), "ModuloAssign 7"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Expressions.Add(Expr.Assign(Left1, Expr.Constant(1, typeof(int?))));
            Res = Expr.ModuloAssign(
                        Left1,
                        Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<int>)), "ModuloAssign 8"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            var tree = Expr.Block(new[] { Left1 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // ModuloAssign by zero, with integer values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 19", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(DivideByZeroException))]
        public static Expr ModuloAssign19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.GenAreEqual(Expr.ModuloAssign(Left, Expr.Constant(0)), Expr.Constant(0), "ModuloAssign 1"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.ValidateException<DivideByZeroException>(tree, false);
            return tree;
        }

        // ModuloAssign zero by zero with single/double values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 20", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo SingleMI = typeof(Single).GetMethod("IsNaN");
            ParameterExpression Left = Expr.Parameter(typeof(Single), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Single)0.0)));
            Expr SingleValue = Expr.ModuloAssign(Left, Expr.Constant((Single)0.0, typeof(Single)));
            Expr SingleResult = Expr.Call(SingleMI, new Expression[] { SingleValue });

            MethodInfo DoubleMI = typeof(Double).GetMethod("IsNaN");
            ParameterExpression Left1 = Expr.Parameter(typeof(Double), "");
            Expressions.Add(Expr.Assign(Left1, Expr.Constant((Double)0.0)));
            Expr DoubleValue = Expr.ModuloAssign(Left1, Expr.Constant((double)0.0, typeof(double)));
            Expr DoubleResult = Expr.Call(DoubleMI, new Expression[] { DoubleValue });

            Expressions.Add(EU.GenAreEqual(SingleResult, Expr.Constant(true), "Divide 1"));
            Expressions.Add(EU.GenAreEqual(DoubleResult, Expr.Constant(true), "Divide 2"));

            var tree = Expr.Block(new[] { Left, Left1 }, Expressions);
            V.Validate(tree);
            return tree;
        }


        //Pass a non assignable expression to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 20_1", new string[] { "negative", "ModuloAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign20_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignInts");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.ModuloAssign(Expr.Constant(1), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(Expressions);
        }

        public class A20 {
            static public int x;
        }

        //pass a static field to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 21", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            var Left = Expr.Field(null, typeof(A20).GetField("x"));
            Expressions.Add(Expr.Assign(Left, Expr.Constant(5)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ModuloAssign(Left, Expr.Constant(0x02), mi), Expr.Constant(0x0001)
                )
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Left));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        public class A21 {
            public int x;
            static public FieldInfo fi = typeof(A21).GetField("x");
        }

        //pass an instance field to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 22", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            var c21 = Expr.Parameter(typeof(A21), "");
            Expressions.Add(Expr.Assign(c21, Expr.Constant(new A21())));

            var Left = Expr.Field(c21, A21.fi);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(5)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ModuloAssign(Left, Expr.Constant(0x02), mi), Expr.Constant(0x0001)
                )
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Left));

            var tree = Expr.Block(new[] { c21 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class A22 {
            private int _x;
            public int x {
                get {
                    return _x;
                }

                set {
                    _x = value;
                }
            }
            static public PropertyInfo pi = typeof(A22).GetProperty("x");
        }

        //pass an instance field to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 23", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            var c22 = Expr.Parameter(typeof(A22), "");
            Expressions.Add(Expr.Assign(c22, Expr.Constant(new A22())));

            var Left = Expr.Property(c22, A22.pi);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(5)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.ModuloAssign(Left, Expr.Constant(0x02), mi), Expr.Constant(0x0001)
                )
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Left));

            var tree = Expr.Block(new[] { c22 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // ModuloAssign with an array index expression as the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 24", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Variable(typeof(int[]), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6), Expr.Constant(7), Expr.Constant(8) })));

            Expressions.Add(Expr.ModuloAssign(Expr.ArrayAccess(Left, Expr.Constant(2)), Expr.Constant(4)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.ArrayIndex(Left, Expr.Constant(2)), "SA 1"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }


        public class A23 {
            public int X;
        }
        // Pass field to left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 25", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Field(Expression.Constant(new A23()), typeof(A23).GetField("X"));
            Expressions.Add(Expr.Assign(Left, Expr.Constant(6)));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(4)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Left, "1"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 26", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("BadReturn");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ModuloAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }


        //Pass null to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 27", new string[] { "negative", "ModuloAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr ModuloAssign27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.ModuloAssign(null, Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        //Pass null to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 28", new string[] { "negative", "ModuloAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr ModuloAssign28(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.ModuloAssign(Left, null);
            }));

            return Expr.Empty();
        }

        //Pass a non value returning expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 30", new string[] { "negative", "ModuloAssign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.ModuloAssign(Left, Expr.Empty());
            }));

            var tree = Expr.Empty();

            return tree;
        }

        //Pass a Block to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 31", new string[] { "negative", "ModuloAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Block(Expr.Constant(1));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a Method Call to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 32", new string[] { "negative", "ModuloAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign32(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Call(Expr.Constant("1"), typeof(string).GetMethod("ToString", new Type[] { }));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a constant to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 33", new string[] { "negative", "ModuloAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign33(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Constant(1);
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        // pass a MethodInfo that takes three arguments
        public static int ModuloAssign3Args(int arg1, int arg2, int arg3) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 34", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssign3Args");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ModuloAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }

        // pass a MethodInfo with two arguments, that returns void
        public static void ModuloAssign2ArgsVoid(int arg1, int arg2) {
            return;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 35", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssign2ArgsVoid");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ModuloAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }



        //with a valid method info, pass two values of an enum class (methodinfo's arguments are of the base integer type)
        public static int ModuloAssign2Args(int arg1, int arg2) {
            return arg1 + arg2;
        }
        enum e36 : int {
        }
        //enum -> int conversion not being accepted.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 36", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssign2Args");

            ParameterExpression Left = Expr.Parameter(typeof(e36), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((e36)1)));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.ModuloAssign(Left, Expr.Constant((e36)2), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type
        public static int ModuloAssignMethod37(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 37", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignMethod37");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(2, typeof(int?)), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //with a valid method info, pass two values of the same nullable type
        public static int ModuloAssignMethod38(int? arg1, int? arg2) {
            return (arg1 + arg2).GetValueOrDefault();
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 38", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignMethod38");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ModuloAssign(Left, Expr.Constant(2, typeof(int?)), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type. return is nullable, arguments aren't
        public static int? ModuloAssignMethod39(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 39", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignMethod39");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.ModuloAssign(Left, Expr.Constant(2, typeof(int?)), mi);
            }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Empty();

            return tree;
        }








        //User defined operator on right argument, arguments are of the proper types
        public class C40 {
            public int Val = 2;
            public static int operator %(int a, C40 b) {
                return b.Val + a;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 40", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(new C40())));

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
            public static C41 operator %(C41 b, Exception a) {
                return new C41(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 41", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C41), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C41("1"))));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(new ArgumentException("2"))));

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
            public static Exception operator %(Exception a, C42 b) {
                return new Exception(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 42", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(Exception), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new Exception("1"))));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(new C42("2"))));

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
            public static C43 operator %(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "A");
            }
        }
        public class C43_1 {
            public string Val;

            public C43_1(string init) {
                Val = init;
            }
            public static C43 operator %(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "B");
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 43", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C43), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C43("1"))));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(new C43_1("2"))));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 44", new string[] { "positive", "ModuloAssign", "operators", "Pri2" })]
        public static Expr ModuloAssign44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C44), "instance");

            Expr Left = Expr.Field(Instance, C44.Field);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C44())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with a static field as the left argument
        public class C45 {
            public static int Val;
            public static FieldInfo Field = typeof(C45).GetField("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 45", new string[] { "positive", "ModuloAssign", "operators", "Pri2" })]
        public static Expr ModuloAssign45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Field(null, C45.Field);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Left));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        //Add with an instance Property as the left argument
        public class C46 {
            public int Val { get; set; }
            public static PropertyInfo Property = typeof(C46).GetProperty("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 46", new string[] { "positive", "ModuloAssign", "operators", "Pri2" })]
        public static Expr ModuloAssign46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C46), "instance");

            Expr Left = Expr.Property(Instance, C46.Property);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C46())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with a static Property as the left argument
        public class C47 {
            public static int Val { get; set; }
            public static PropertyInfo Property = typeof(C47).GetProperty("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 47", new string[] { "positive", "ModuloAssign", "operators", "Pri2" })]
        public static Expr ModuloAssign47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Property(null, C47.Property);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Left));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 48", new string[] { "positive", "ModuloAssign", "operators", "Pri2" })]
        public static Expr ModuloAssign48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C48), "instance");

            Expr Left = Expr.Property(Instance, C48.Property, new[] { Expr.Constant(2) });

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C48())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(3)));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static int ModuloAssignConv(short arg1, short arg2) {
            return (int)(arg1 + arg2);
        }
        //ModuloAssign with a lambda conversion for the return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 49_1", new string[] { "positive", "ModuloAssign", "operators", "Pri1" })]
        public static Expr ModuloAssign49_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(short)), arg1
                                                );
            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignConv");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((short)1)));
            Expressions.Add(
                            EU.GenAreEqual(
                                            Expr.Constant((short)3),
                                            Expr.ModuloAssign(Left, Expr.Constant((short)2), mi, Conv)
                                           )
                           );


            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //ModuloAssign with a lambda conversion for the return type, to the wrong type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 50_1", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign50_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(byte)), arg1
                                                );
            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignConv");

            EU.Throws<System.InvalidOperationException>(() => { Expr.ModuloAssign(Left, Expr.Constant((short)2), mi, Conv); });
            
            return Expr.Empty();
        }

        //Passing null to conversion lambda
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 51_1", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ModuloAssign51_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            LambdaExpression Conv = null;
            MethodInfo mi = typeof(ModuloAssign).GetMethod("ModuloAssignConv");

            EU.Throws<System.ArgumentException>(() => { Expr.ModuloAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }

        //ModuloAssignByZero, with decimal values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 52", new string[] { "negative", "ModuloAssign", "operators", "Pri2" }, Exception = typeof(DivideByZeroException))]
        public static Expr ModuloAssign52(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(decimal), "Left");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((decimal)2)));

            Expressions.Add(Expr.ModuloAssign(Left, Expr.Constant((decimal)0)));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.ValidateException<DivideByZeroException>(tree, false);
            return tree;
        }

        //Passing a conversion lambda when it's not needed
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ModuloAssign 52_1", new string[] { "negative", "ModuloAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ModuloAssign52_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(int)), arg1
                                                );

            MethodInfo mi = null;// typeof(ModuloAssign).GetMethod("ModuloAssignInts");

            EU.Throws<System.InvalidOperationException>(() => { Expr.ModuloAssign(Left, Expr.Constant(2), mi, Conv); });

            return Expr.Empty();
        }
    }
}
