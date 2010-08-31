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

    public class Modulo {
        // Modulo of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 1", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Modulo1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.Modulo(Expr.Constant(true), Expr.Constant(false)), Expr.Constant(false)); }));
            return Expr.Empty();
        }

        // Modulo of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 2", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Modulo2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression String1 = Expr.Variable(typeof(string), "");
            ParameterExpression String2 = Expr.Variable(typeof(string), "");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.Modulo(Expr.Constant("Hello"), Expr.Constant("World")),
                Expr.Constant(false));
            }));

            var tree = EU.BlockVoid(new[] { String1, String2 }, Expressions);

            return tree;
        }

        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        // Modulo of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 3", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Modulo3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            Expr Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Modulo(Left, Right);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Modulo of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 4", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Modulo4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            Expr Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Modulo(Left, Right);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Modulo of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 5", new string[] { "positive", "modulo", "operators", "Pri1" })]
        public static Expr Modulo5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((Int16)32, typeof(Int16)), Expr.Constant((Int16)88, typeof(Int16))), Expr.Constant((Int16)32, typeof(Int16)), "Modulo 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((UInt16)3, typeof(UInt16)), Expr.Constant((UInt16)2, typeof(UInt16))), Expr.Constant((UInt16)1, typeof(UInt16)), "Modulo 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((Int32)(-8), typeof(Int32)), Expr.Constant((Int32)2, typeof(Int32))), Expr.Constant((Int32)0, typeof(Int32)), "Modulo 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((UInt32)32, typeof(UInt32)), Expr.Constant((UInt32)3, typeof(UInt32))), Expr.Constant((UInt32)2, typeof(UInt32)), "Modulo 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((short)1000, typeof(short)), Expr.Constant((short)(-101), typeof(short))), Expr.Constant((short)91, typeof(short)), "Modulo 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((ushort)2, typeof(ushort)), Expr.Constant((ushort)2, typeof(ushort))), Expr.Constant((ushort)0, typeof(ushort)), "Modulo 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((long)5, typeof(long)), Expr.Constant((long)1, typeof(long))), Expr.Constant((long)0, typeof(long)), "Modulo 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((ulong)4.0, typeof(ulong)), Expr.Constant((ulong)2.0, typeof(ulong))), Expr.Constant((ulong)0, typeof(ulong)), "Modulo 8"));

            double delta = 0.00001;
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.Modulo(Expr.Constant((Single)3.333), Expr.Constant((Single)1.111)),
                        Expr.Constant((Single)(0 + delta))
                    ),
                    Expr.Constant(true),
                    "Modulo 9"
                )
            );
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.Modulo(Expr.Constant((Single)3.333), Expr.Constant((Single)1.111)),
                        Expr.Constant((Single)(0 - delta))
                    ),
                    Expr.Constant(true),
                    "Modulo 9_1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.Modulo(Expr.Constant((Double)33.333), Expr.Constant((Double)11.1)),
                        Expr.Constant((Double)(.033 + delta))
                    ),
                    Expr.Constant(true),
                    "Modulo 10"
                )
            );
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.Modulo(Expr.Constant((Double)33.333), Expr.Constant((Double)11.1)),
                        Expr.Constant((Double)(.033 - delta))
                    ),
                    Expr.Constant(true),
                    "Modulo 10_1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.Modulo(Expr.Constant((decimal)9999.999), Expr.Constant((decimal)1111.111)),
                        Expr.Constant((decimal)(0 + delta))
                    ),
                    Expr.Constant(true),
                    "Modulo 11"
                )
            );
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.Modulo(Expr.Constant((decimal)9999.999), Expr.Constant((decimal)1111.111)),
                        Expr.Constant((decimal)(0 - delta))
                    ),
                    Expr.Constant(true),
                    "Modulo 11_1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 6", new string[] { "positive", "modulo", "operators", "Pri1" })]
        public static Expr Modulo6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant(4), Expr.Constant(4), null), Expr.Constant(0), "Modulo 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant(4), Expr.Constant(3), null), Expr.Constant(1), "Modulo 2"));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Modulo(
                        Expr.Constant(4, typeof(Nullable<int>)),
                        Expr.Constant(2, typeof(Nullable<int>)),
                        null
                    ),
                    Expr.Constant(0, typeof(Nullable<int>))
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Modulo(
                        Expr.Constant(4, typeof(Nullable<int>)),
                        Expr.Constant(3, typeof(Nullable<int>)),
                        null
                    ),
                    Expr.Constant(1, typeof(Nullable<int>))
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int ModuloNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 7", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Modulo7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Modulo).GetMethod("ModuloNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Modulo(Expr.Constant(2, typeof(Int32)), Expr.Constant(1, typeof(Int32)), mi), Expr.Constant(0, typeof(Int32)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // pass a MethodInfo that takes a paramarray
        public static int ModuloParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 8", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Modulo8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Modulo).GetMethod("ModuloParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Modulo(Expr.Constant(10, typeof(Int32)), Expr.Constant(3, typeof(Int32)), mi), Expr.Constant(1, typeof(Int32)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // with a valid MethodInfo, modulo two values of the same type
        public static int ModuloInts(int x, int y) {
            return x % y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 9", new string[] { "positive", "modulo", "operators", "Pri1" })]
        public static Expr Modulo9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Modulo).GetMethod("ModuloInts");

            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant(10, typeof(Int32)), Expr.Constant(3, typeof(Int32)), mi), Expr.Constant(1)));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant(10, typeof(Int32)), Expr.Constant(10, typeof(Int32)), mi), Expr.Constant(0)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 10", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Modulo10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Modulo).GetMethod("ModuloInts");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Modulo(Expr.Constant((Int16)3), Expr.Constant((Int16)2), mi), Expr.Constant((Int16)1));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static bool ModuloExceptionMsg(Exception e1, Exception e2) {
            return e1.Message == e2.Message;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 11", new string[] { "positive", "modulo", "operators", "Pri1" })]
        public static Expr Modulo11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Modulo).GetMethod("ModuloExceptionMsg");

            Expr Res =
                Expr.Modulo(
                    Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException)),
                    Expr.Constant(new RankException("One"), typeof(RankException)),
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true), "Modulo 1"));

            Expr Res2 =
                Expr.Modulo(
                    Expr.Constant(new IndexOutOfRangeException("Two"), typeof(IndexOutOfRangeException)),
                    Expr.Constant(new ArgumentNullException("Three"), typeof(ArgumentNullException)),
                    mi
                );

            Expressions.Add(EU.GenAreEqual(Res2, Expr.Constant(false), "Modulo 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type. 
        public static int ModuloNullableInt(int? x, int y) {
            return x.GetValueOrDefault() % y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 12", new string[] { "positive", "modulo", "operators", "Pri1" })]
        public static Expr Modulo12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Modulo).GetMethod("ModuloNullableInt");

            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(0), "Modulo 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((Nullable<int>)3, typeof(Nullable<int>)), Expr.Constant(2), mi), Expr.Constant(1), "Modulo 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant(2), mi), Expr.Constant(0), "Modulo 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 13", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Modulo13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.Modulo(Expr.Constant(1, typeof(Int32)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), mi), Expr.Constant(0)
                )
        ;
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Modulo of two values of different types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 14", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Modulo14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Modulo(Expr.Constant(true), Expr.Constant(1)), Expr.Constant(1), "Modulo 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Modulo across mixed types, no user defined operator
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Modulo 15", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Modulo15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Modulo(Expr.Constant(1), Expr.Constant(2.0), mi), Expr.Constant(3)
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

            public static int operator %(MyVal v1, int v2) {
                return (v1.Val % v2);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 16", new string[] { "positive", "modulo", "operators", "Pri1" })]
        public static Expr Modulo16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Modulo(Expr.Constant(new MyVal(8)), Expr.Constant(3), mi), Expr.Constant(2)
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Verify order of evaluation of expressions on modulo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 17", new string[] { "positive", "modulo", "operators", "Pri1" })]
        public static Expr Modulo17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr A = Expr.Constant(45);
            Expr B = Expr.Constant(12);
            Expr C = Expr.Constant(7);
            Expr Res =
                Expr.Modulo(Expr.Modulo(A, B), C);
            Expr Res2 =
                Expr.Modulo(A, Expr.Modulo(B, C));
            Expr Res3 =
                Expr.Modulo(Expr.Modulo(A, Expr.Constant(30)), B);

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(2), "Modulo 1"));
            Expressions.Add(EU.GenAreEqual(Res2, Expr.Constant(0), "Modulo 2"));
            Expressions.Add(EU.GenAreEqual(Res3, Expr.Constant(3), "Modulo 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        // With a valid methodinfo that returns boolean, pass arguments of nullable type. 
        public static bool ModLiftTest(int? x, int? y) {
            if (x.HasValue && y.HasValue)
                return x % y == 0;
            return false;
        }

        public static int ModLiftTestReturnInt(int? x, int? y) {
            if (x.HasValue && y.HasValue)
                return (x % y == 0) ? 1 : 0;
            else
                return -1;
        }

        public static int ModLiftTestWithoutNullables(int x, int y) {
            return (x % y == 0) ? 1 : 0;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 18", new string[] { "positive", "modulo", "operators", "Pri1" })]
        public static Expr Modulo18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Modulo).GetMethod("ModLiftTest");

            // With a valid methodinfo that returns boolean, pass arguments of nullable type. Set lift to null to false.
            Expr Res = Expr.Modulo(
                        Expr.Constant((Nullable<int>)4, typeof(Nullable<int>)),
                        Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true, typeof(bool)), "Modulo 1"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            // With a valid methodinfo that returns boolean, pass arguments of nullable type with null values. Set lift to null to false.
            Res = Expr.Modulo(
                        Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)),
                        Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(bool)), "Modulo 2"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            Res = Expr.Modulo(
                        Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)),
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(bool)), "Modulo 3"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(bool)));

            mi = typeof(Modulo).GetMethod("ModLiftTestReturnInt");

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to true.
            Res = Expr.Modulo(
                        Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)),
                        Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(-1, typeof(int)), "Modulo 4"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int)));

            Res = Expr.Modulo(
                        Expr.Constant((Nullable<int>)4, typeof(Nullable<int>)),
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        mi
                    );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(1, typeof(int)), "Modulo 5"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int)));

            // With a valid methodinfo that returns non boolean, pass arguments of nullable type. Set lift to null to false.
            Res = Expr.Modulo(
                        Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)),
                        Expr.Constant((Nullable<int>)3, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(0, typeof(int)), "Modulo 6"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(int)));

            mi = typeof(Modulo).GetMethod("ModLiftTestWithoutNullables");

            Res = Expr.Modulo(
                        Expr.Constant((Nullable<int>)3, typeof(Nullable<int>)),
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(1, typeof(Nullable<int>)), "Modulo 7"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            Res = Expr.Modulo(
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)),
                        mi
                    );
            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<int>)), "Modulo 8"));
            Expressions.Add(EU.ExprTypeCheck(Res, typeof(Nullable<int>)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Modulo by zero, with integer values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 19", new string[] { "negative", "modulo", "operators", "Pri1" }, Exception = typeof(DivideByZeroException))]
        public static Expr Modulo19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Modulo(Expr.Constant(1), Expr.Constant(0)), Expr.Constant(0), "Modulo 1"));

            var tree = EU.BlockVoid(Expressions);
            V.ValidateException<DivideByZeroException>(tree, false);
            return tree;
        }

        // Modulo zero by zero with single/double values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo 20", new string[] { "positive", "modulo", "operators", "Pri1" })]
        public static Expr Modulo20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo SingleMI = typeof(Single).GetMethod("IsNaN");
            Expr SingleValue = Expr.Modulo(Expr.Constant((Single)0.0, typeof(Single)), Expr.Constant((Single)0.0, typeof(Single)));
            Expr SingleResult = Expr.Call(SingleMI, new Expression[] { SingleValue });

            MethodInfo DoubleMI = typeof(Double).GetMethod("IsNaN");
            Expr DoubleValue = Expr.Modulo(Expr.Constant((double)0.0, typeof(double)), Expr.Constant((double)0.0, typeof(double)));
            Expr DoubleResult = Expr.Call(DoubleMI, new Expression[] { DoubleValue });

            Expressions.Add(EU.GenAreEqual(SingleResult, Expr.Constant(true), "Divide 1"));
            Expressions.Add(EU.GenAreEqual(DoubleResult, Expr.Constant(true), "Divide 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
