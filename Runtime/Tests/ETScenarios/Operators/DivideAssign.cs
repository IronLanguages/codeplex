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

    public class DivideAssign {
        // DivideAssign of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 1", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(bool), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(true)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.DivideAssign(Left, Expr.Constant(false), null);
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // DivideAssign of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 2", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(string), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant("Hello")));


            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.DivideAssign(Left, Expr.Constant("World"));
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        // DivideAssign of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 3", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.DivideAssign(Left, Right, null);
            }));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);

            return tree;
        }

        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        // DivideAssign of struct object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 4", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.DivideAssign(Left, Right);
            }));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);

            return tree;
        }

        // DivideAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // TODO: DivideAssign tests for SByte and Byte once they support DivideAssign (Dev10 Bug 502521)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 5", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Binary Operator not defined for the following two:
            //Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(Expr.Constant((SByte)1), Expr.Constant((SByte)2)), Expr.Constant((SByte)3))); // Int16 is CLS compliant equivalent type
            //Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(Expr.Constant((Byte)1), Expr.Constant((Byte)2)), Expr.Constant((Byte)3)));


            ParameterExpression v1 = Expr.Variable(typeof(Int16), "");
            Expressions.Add(Expr.Assign(v1, Expr.Constant((Int16)(-4))));
            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(v1, Expr.Constant((Int16)(-2))), Expr.Constant((Int16)2), "DivideAssign 1"));

            ParameterExpression v2 = Expr.Variable(typeof(UInt16), "");
            Expressions.Add(Expr.Assign(v2, Expr.Constant((UInt16)3)));
            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(v2, Expr.Constant((UInt16)2)), Expr.Constant((UInt16)1), "DivideAssign 2"));

            ParameterExpression v3 = Expr.Variable(typeof(short), "");
            Expressions.Add(Expr.Assign(v3, Expr.Constant((short)1)));
            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(v3, Expr.Constant((short)2)), Expr.Constant((short)0), "DivideAssign 3"));

            ParameterExpression v4 = Expr.Variable(typeof(ushort), "");
            Expressions.Add(Expr.Assign(v4, Expr.Constant((ushort)10)));
            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(v4, Expr.Constant((ushort)3)), Expr.Constant((ushort)3), "DivideAssign 4"));

            ParameterExpression v5 = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(v5, Expr.Constant((Int32)(-6))));
            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(v5, Expr.Constant((Int32)2)), Expr.Constant((Int32)(-3)), "DivideAssign 5"));

            ParameterExpression v6 = Expr.Variable(typeof(uint), "");
            Expressions.Add(Expr.Assign(v6, Expr.Constant((uint)12)));
            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(v6, Expr.Constant((UInt32)4)), Expr.Constant((UInt32)3), "DivideAssign 6"));

            ParameterExpression v7 = Expr.Variable(typeof(long), "");
            Expressions.Add(Expr.Assign(v7, Expr.Constant((long)4.0)));
            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(v7, Expr.Constant((long)2.0)), Expr.Constant((long)2), "DivideAssign 7"));

            ParameterExpression v8 = Expr.Variable(typeof(ulong), "");
            Expressions.Add(Expr.Assign(v8, Expr.Constant((ulong)2)));
            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(v8, Expr.Constant((ulong)2.0)), Expr.Constant((ulong)1.0), "DivideAssign 8"));

            double delta = .00010;

            ParameterExpression v9 = Expr.Variable(typeof(Single), "");
            Expressions.Add(Expr.Assign(v9, Expr.Constant((Single)8.0)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.DivideAssign(v9, Expr.Constant((Single)3.0)),
                        Expr.Constant((Single)(2.66667 + delta))
                    ),
                    Expr.Constant(true),
                    "DivideAssign 9"
                )
            );

            ParameterExpression v10 = Expr.Variable(typeof(Single), "");
            Expressions.Add(Expr.Assign(v10, Expr.Constant((Single)8.0)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.DivideAssign(v10, Expr.Constant((Single)3.0)),
                        Expr.Constant((Single)(2.66667 - delta))
                    ),
                    Expr.Constant(true),
                    "DivideAssign 9_1"
                )
            );

            ParameterExpression v11 = Expr.Variable(typeof(double), "");
            Expressions.Add(Expr.Assign(v11, Expr.Constant((double)-11)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.DivideAssign(v11, Expr.Constant((Double)2.0)),
                        Expr.Constant((Double)(-5.5 + delta))
                    ),
                    Expr.Constant(true),
                    "DivideAssign 10"
                )
            );

            ParameterExpression v12 = Expr.Variable(typeof(double), "");
            Expressions.Add(Expr.Assign(v12, Expr.Constant((double)-11)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.DivideAssign(v12, Expr.Constant((Double)2.0)),
                        Expr.Constant((Double)(-5.5 - delta))
                    ),
                    Expr.Constant(true),
                    "DivideAssign 10_1"
                )
            );


            ParameterExpression v13 = Expr.Variable(typeof(decimal), "");
            Expressions.Add(Expr.Assign(v13, Expr.Constant((decimal)3.3333)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.DivideAssign(v13, Expr.Constant((decimal)2.0)),
                        Expr.Constant((decimal)(1.66665 + delta))
                    ),
                    Expr.Constant(true),
                    "DivideAssign 11"
                )
            );
            ParameterExpression v14 = Expr.Variable(typeof(decimal), "");
            Expressions.Add(Expr.Assign(v14, Expr.Constant((decimal)3.3333)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.DivideAssign(v14, Expr.Constant((decimal)2.0)),
                        Expr.Constant((decimal)(1.66665 - delta))
                    ),
                    Expr.Constant(true),
                    "DivideAssign 11_1"
                )
            );

            var tree = Expr.Block(new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 6", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(4)));

            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(Left, Expr.Constant(3, typeof(Int32)), null), Expr.Constant(1, typeof(Int32))));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int DivideAssignNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 7", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssign7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignNoArgs");

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.DivideAssign(Left, Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }

        // pass a MethodInfo that takes a paramarray
        public static int DivideAssignParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 8", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssign8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignParamArray");

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.DivideAssign(Left, Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }

        // with a valid MethodInfo, DivideAssign two values of the same type
        public static int DivideAssignInts(int x, int y) {
            return x / y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 9", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignInts");

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(10)));

            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(Left, Expr.Constant(3, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32))));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 10", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignInts");

            ParameterExpression Left = Expr.Variable(typeof(Int16), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Int16)1)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.DivideAssign(Left, Expr.Constant((Int16)2), mi), Expr.Constant((Int16)3));
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static DivideByZeroException DivideAssignExceptionMsg(Exception e1, Exception e2) {
            return new DivideByZeroException(e1.Message + e2.Message);
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 11", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignExceptionMsg");

            ParameterExpression Left = Expr.Variable(typeof(DivideByZeroException), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException))));

            Expr Res =
                Expr.DivideAssign(
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
        public static int DivideAssignInt(int x, int y) {
            return x / y;
        }

        public static int? DivideAssignNullableInt(int? x, int y) {
            return x.GetValueOrDefault() / y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 12", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignInt");

            ParameterExpression Left = Expr.Variable(typeof(Nullable<int>), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2, typeof(Nullable<int>))));

            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.DivideAssign(Left, Expr.Constant(1), mi), Expr.Constant(2, typeof(Int32)), "DivideAssign 1"); }));

            return Expr.Empty();
        }

        // With a valid methodinfo, pass a value of a non nullable type, the other of nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 12_1", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign12_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignInt");

            ParameterExpression Left = Expr.Variable(typeof(Nullable<int>), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(null, typeof(Nullable<int>))));

            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.DivideAssign(Left, Expr.Constant(1), mi), Expr.Constant(0, typeof(Int32)), "DivideAssign 1"); }));
            return Expr.Empty();
        }

        // With a valid methodinfo, pass a value of a non nullable type, the other of nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 12_2", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign12_2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(Nullable<int>), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2, typeof(Nullable<int>))));

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignNullableInt");
            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(Left, Expr.Constant(1), mi), Expr.Constant(2, typeof(Int32?)), "DivideAssign 1"));
            ParameterExpression Left1 = Expr.Variable(typeof(Nullable<int>), "");
            Expressions.Add(Expr.Assign(Left1, Expr.Constant(null, typeof(Nullable<int>))));
            Expressions.Add(EU.GenAreEqual(Expr.DivideAssign(Left1, Expr.Constant(1), mi), Expr.Constant(0, typeof(Int32?)), "DivideAssign 2"));

            var tree = Expr.Block(new[] { Left1, Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 13", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.DivideAssign(Left, Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), mi), Expr.Constant(0, typeof(Int32))
                )
        ;
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // with a valid methodinfo that returns boolean, pass arguments of nullable type
        public static bool? IsTrueNullable(bool? x, bool? y) {
            if (x.HasValue && y.HasValue)
                return true;
            return false;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 14", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("IsTrueNullable");


            ParameterExpression Left = Expr.Variable(typeof(Nullable<bool>), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(true, typeof(Nullable<bool>))));


            Expressions.Add(
                EU.GenAreEqual(
                    Expr.DivideAssign(
                        Left,
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(true, typeof(bool?)),
                    "DivideAssign 1"
                )
            );

            Expressions.Add(Expr.Assign(Left, Expr.Constant(true, typeof(Nullable<bool>))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.DivideAssign(
                        Left,
                        Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(false, typeof(bool?)),
                    "DivideAssign 2"
                )
            );
            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo that returns boolean, pass arguments of nullable type. 
        public static bool IsTrue(bool x, bool y) {
            return x && y;
        }

        // with a valid methodinfo that returns boolean, pass arguments of nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 14_1", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign14_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("IsTrue");

            ParameterExpression Left = Expr.Variable(typeof(Nullable<bool>), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(true, typeof(Nullable<bool>))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.DivideAssign(
                        Left,
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(true, typeof(bool)),
                    "DivideAssign 1"
                )
        ;
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // DivideAssigning across mixed types
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "DivideAssign 15", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Variable(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.DivideAssign(Left, Expr.Constant(2.0), mi), Expr.Constant(3)
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

            public static MyVal operator /(MyVal v1, int v2) {
                return new MyVal((v1.Val + v1.Val) / (v2 + v2));
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 16", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Variable(typeof(MyVal), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new MyVal(6), typeof(MyVal))));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(2), mi));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Property(Left, typeof(MyVal).GetProperty("Val")), Expr.Constant(3)
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

        // Verify order of evaluation of expressions on DivideAssign
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 17", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            TestOrder order = new TestOrder();

            ParameterExpression ov = Expr.Parameter(typeof(TestOrder), "order");

            Expressions.Add(Expr.Assign(ov, Expr.Constant(order)));
            Expressions.Add(Expr.Assign(Expr.Property(ov, order.pi, Expr.Constant("")), Expr.Constant(2)));

            Expr Res =
                Expr.DivideAssign(
                    Expr.Property(ov, order.pi, Expr.Constant("One")),
                    Expr.DivideAssign(
                        Expr.Property(ov, order.pi, Expr.Constant("Two")),
                        Expr.DivideAssign(
                            Expr.Property(ov, order.pi, Expr.Constant("Three")),
                            Expr.Constant(1)
                        )
                    ),
                    null
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Res));
            Expressions.Add(EU.GenAreEqual(Expr.Field(ov, order.resi), Expr.Constant("OneTwoThreeThreeTwoOne")));

            var tree = Expr.Block(new[] { ov }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // DivideAssign zero by zero with single/double values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 18", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo SingleMI = typeof(Single).GetMethod("IsNaN");
            ParameterExpression Left1 = Expr.Variable(typeof(Single), "");
            Expressions.Add(Expr.Assign(Left1, Expr.Constant((Single)0)));

            Expr SingleValue = Expr.DivideAssign(Left1, Expr.Constant((Single)0.0, typeof(Single)));
            Expr SingleResult = Expr.Call(SingleMI, new Expression[] { SingleValue });

            MethodInfo DoubleMI = typeof(Double).GetMethod("IsNaN");
            ParameterExpression Left2 = Expr.Variable(typeof(Double), "");
            Expressions.Add(Expr.Assign(Left2, Expr.Constant((Double)0)));
            Expr DoubleValue = Expr.DivideAssign(Left2, Expr.Constant((double)0.0, typeof(double)));
            Expr DoubleResult = Expr.Call(DoubleMI, new Expression[] { DoubleValue });

            Expressions.Add(EU.GenAreEqual(SingleResult, Expr.Constant(true), "DivideAssign 1"));
            Expressions.Add(EU.GenAreEqual(DoubleResult, Expr.Constant(true), "DivideAssign 2"));

            var tree = Expr.Block(new[] { Left1, Left2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class A20 {
            static public int x;
        }

        //pass a field to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 19", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            var Left = Expr.Field(null, typeof(A20).GetField("x"));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.DivideAssign(Left, Expr.Constant(0x01), mi), Expr.Constant(0x0)
                )
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Left));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass a non assignable expression to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssignChecked 20_1", new string[] { "negative", "DivideAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssignChecked20_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignCheckedInts");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.DivideAssign(Expr.Constant(1), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(Expressions);
        }

        // DivideAssign with an array index expression as the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 24", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Variable(typeof(int[]), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6), Expr.Constant(7), Expr.Constant(8) })));

            Expressions.Add(Expr.DivideAssign(Expr.ArrayAccess(Left, Expr.Constant(2)), Expr.Constant(4)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Expr.ArrayIndex(Left, Expr.Constant(2)), "SA 1"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 25", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssign25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("BadReturn");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.DivideAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }

        //Pass null to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 26", new string[] { "negative", "DivideAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr DivideAssign26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.DivideAssign(null, Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        //Pass null to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 27", new string[] { "negative", "DivideAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr DivideAssign27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.DivideAssign(Left, null);
            }));

            return Expr.Empty();
        }

        //Pass a non value returning expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 29", new string[] { "negative", "DivideAssign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.DivideAssign(Left, Expr.Empty());
            }));

            var tree = Expr.Empty();

            return tree;
        }

        //Pass a Block to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 30", new string[] { "negative", "DivideAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssign30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Block(Expr.Constant(1));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a Method Call to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 31", new string[] { "negative", "DivideAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssign31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Call(Expr.Constant("1"), typeof(string).GetMethod("ToString", new Type[] { }));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a constant to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 33", new string[] { "negative", "DivideAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssign33(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Constant(1);
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        // pass a MethodInfo that takes three arguments
        public static int DivideAssign3Args(int arg1, int arg2, int arg3) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 34", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssign34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssign3Args");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.DivideAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }

        // pass a MethodInfo with two arguments, that returns void
        public static void DivideAssign2ArgsVoid(int arg1, int arg2) {
            return;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 35", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssign35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssign2ArgsVoid");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.DivideAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }


        //with a valid method info, pass two values of an enum class (methodinfo's arguments are of the base integer type)
        public static int DivideAssign2Args(int arg1, int arg2) {
            return arg1 + arg2;
        }
        enum e36 : int {
        }
        //enum -> int conversion not being accepted.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 36", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssign2Args");

            ParameterExpression Left = Expr.Parameter(typeof(e36), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((e36)1)));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.DivideAssign(Left, Expr.Constant((e36)2), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type
        public static int DivideAssignMethod37(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 37", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignMethod37");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(2, typeof(int?)), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //with a valid method info, pass two values of the same nullable type
        public static int DivideAssignMethod38(int? arg1, int? arg2) {
            return (arg1 + arg2).GetValueOrDefault();
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 38", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssign38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignMethod38");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.DivideAssign(Left, Expr.Constant(2, typeof(int?)), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type. return is nullable, arguments aren't
        public static int? DivideAssignMethod39(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 39", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignMethod39");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.DivideAssign(Left, Expr.Constant(2, typeof(int?)), mi);
            }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Empty();

            return tree;
        }






        //User defined operator on right argument, arguments are of the proper types
        public class C40 {
            public int Val = 2;
            public static int operator /(int a, C40 b) {
                return b.Val + a;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 40", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(new C40())));

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
            public static C41 operator /(C41 b, Exception a) {
                return new C41(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 41", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C41), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C41("1"))));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(new ArgumentException("2"))));

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
            public static Exception operator /(Exception a, C42 b) {
                return new Exception(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 42", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(Exception), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new Exception("1"))));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(new C42("2"))));

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
            public static C43 operator /(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "A");
            }
        }
        public class C43_1 {
            public string Val;

            public C43_1(string init) {
                Val = init;
            }
            public static C43 operator /(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "B");
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 43", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C43), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C43("1"))));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(new C43_1("2"))));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 44", new string[] { "positive", "DivideAssign", "operators", "Pri2" })]
        public static Expr DivideAssign44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C44), "instance");

            Expr Left = Expr.Field(Instance, C44.Field);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C44())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(5)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(2)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 45", new string[] { "positive", "DivideAssign", "operators", "Pri2" })]
        public static Expr DivideAssign45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Field(null, C45.Field);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(5)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(2)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 46", new string[] { "positive", "DivideAssign", "operators", "Pri2" })]
        public static Expr DivideAssign46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C46), "instance");

            Expr Left = Expr.Property(Instance, C46.Property);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C46())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(5)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(2)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 47", new string[] { "positive", "DivideAssign", "operators", "Pri2" })]
        public static Expr DivideAssign47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Property(null, C47.Property);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(5)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(2)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 48", new string[] { "positive", "DivideAssign", "operators", "Pri2" })]
        public static Expr DivideAssign48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C48), "instance");

            Expr Left = Expr.Property(Instance, C48.Property, new[] { Expr.Constant(2) });

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C48())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(3)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }




        //DivideAssignByZero, with decimal values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 52", new string[] { "negative", "DivideAssign", "operators", "Pri2" }, Exception = typeof(DivideByZeroException))]
        public static Expr DivideAssign52(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)2)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant((int)0)));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.ValidateException<DivideByZeroException>(tree, false);
            return tree;
        }

        //DivideAssignByZero, with decimal values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 53", new string[] { "positive", "DivideAssign", "operators", "Pri2" })]
        public static Expr DivideAssign53(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(float), "Left");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((float)2)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant((float)0)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(float.PositiveInfinity), Left));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //DivideAssignByZero, with decimal values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 54", new string[] { "positive", "DivideAssign", "operators", "Pri2" })]
        public static Expr DivideAssign54(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(double), "Left");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((double)2)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant((double)0)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(double.PositiveInfinity), Left));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //DivideAssignByZero, with decimal values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 55", new string[] { "negative", "DivideAssign", "operators", "Pri2" }, Exception = typeof(DivideByZeroException))]
        public static Expr DivideAssign55(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(decimal), "Left");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((decimal)2)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant((decimal)0)));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.ValidateException<DivideByZeroException>(tree, false);
            return tree;
        }

        //DivideAssignByZero, with decimal values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 56", new string[] { "negative", "DivideAssign", "operators", "Pri2" }, Exception = typeof(DivideByZeroException))]
        public static Expr DivideAssign56(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(decimal), "Left");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((decimal)0)));

            Expressions.Add(Expr.DivideAssign(Left, Expr.Constant((decimal)0)));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.ValidateException<DivideByZeroException>(tree, false);
            return tree;
        }







        public static int DivideAssignConv(short arg1, short arg2) {
            return (int)(arg1 + arg2);
        }
        //DivideAssign with a lambda conversion for the return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 49_1", new string[] { "positive", "DivideAssign", "operators", "Pri1" })]
        public static Expr DivideAssign49_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(short)), arg1
                                                );
            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignConv");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((short)1)));
            Expressions.Add(
                            EU.GenAreEqual(
                                            Expr.Constant((short)3),
                                            Expr.DivideAssign(Left, Expr.Constant((short)2), mi, Conv)
                                           )
                           );


            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //DivideAssign with a lambda conversion for the return type, to the wrong type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 50_1", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign50_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(byte)), arg1
                                                );
            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignConv");

            EU.Throws<InvalidOperationException>(() => { Expr.DivideAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }

        //Passing null to conversion lambda
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 51_1", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr DivideAssign51_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            LambdaExpression Conv = null;
            MethodInfo mi = typeof(DivideAssign).GetMethod("DivideAssignConv");

            EU.Throws<ArgumentException>(() => { Expr.DivideAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }


        //Passing a conversion lambda when it's not needed
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DivideAssign 52_1", new string[] { "negative", "DivideAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr DivideAssign52_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(int)), arg1
                                                );

            MethodInfo mi = null;// typeof(DivideAssign).GetMethod("DivideAssignInts");

            EU.Throws<InvalidOperationException>(() => { Expr.DivideAssign(Left, Expr.Constant(2), mi, Conv); });

            return Expr.Empty();
        }
    }
}
