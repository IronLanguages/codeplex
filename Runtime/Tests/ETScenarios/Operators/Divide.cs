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
    
    public class Divide {
        // Divide of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 1", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Divide1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Divide(Expr.Constant(true), Expr.Constant(false), null);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Divide of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 2", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Divide2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Divide(Expr.Constant("Hello"), Expr.Constant("World"));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        // Divide of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 3", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Divide3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1))));

            ParameterExpression Right = Expr.Variable(typeof(TestClass), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestClass(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Divide(Left, Right, null);
            }));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);

            return tree;
        }

        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        // Divide of struct object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 4", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Divide4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            Expr Right = Expr.Variable(typeof(TestStruct), "");
            Expressions.Add(Expr.Assign(Right, Expr.Constant(new TestStruct(2))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Divide(Left, Right);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Divide of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // TODO: Divide tests for SByte and Byte once they support Divide (Dev10 Bug 502521)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 5", new string[] { "positive", "divide", "operators", "Pri1" })]
        public static Expr Divide5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Binary Operator not defined for the following two:
            //Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((SByte)1), Expr.Constant((SByte)2)), Expr.Constant((SByte)3))); // Int16 is CLS compliant equivalent type
            //Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((Byte)1), Expr.Constant((Byte)2)), Expr.Constant((Byte)3)));

            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((Int16)(-4)), Expr.Constant((Int16)(-2))), Expr.Constant((Int16)2), "Divide 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((UInt16)3), Expr.Constant((UInt16)2)), Expr.Constant((UInt16)1), "Divide 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((short)1), Expr.Constant((short)2)), Expr.Constant((short)0), "Divide 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((ushort)10), Expr.Constant((ushort)3)), Expr.Constant((ushort)3), "Divide 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((Int32)(-6)), Expr.Constant((Int32)2)), Expr.Constant((Int32)(-3)), "Divide 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((UInt32)12), Expr.Constant((UInt32)4)), Expr.Constant((UInt32)3), "Divide 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((long)4.0), Expr.Constant((long)2.0)), Expr.Constant((long)2), "Divide 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((ulong)2.0), Expr.Constant((ulong)2.0)), Expr.Constant((ulong)1.0), "Divide 8"));

            double delta = .00010;
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.Divide(Expr.Constant((Single)8.0), Expr.Constant((Single)3.0)),
                        Expr.Constant((Single)(2.66667 + delta))
                    ),
                    Expr.Constant(true),
                    "Divide 9"
                )
            );
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.Divide(Expr.Constant((Single)8.0), Expr.Constant((Single)3.0)),
                        Expr.Constant((Single)(2.66667 - delta))
                    ),
                    Expr.Constant(true),
                    "Divide 9_1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.Divide(Expr.Constant((Double)(-11.0)), Expr.Constant((Double)2.0)),
                        Expr.Constant((Double)(-5.5 + delta))
                    ),
                    Expr.Constant(true),
                    "Divide 10"
                )
            );
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.Divide(Expr.Constant((Double)(-11.0)), Expr.Constant((Double)2.0)),
                        Expr.Constant((Double)(-5.5 - delta))
                    ),
                    Expr.Constant(true),
                    "Divide 10_1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.LessThanOrEqual(
                        Expr.Divide(Expr.Constant((decimal)3.3333), Expr.Constant((decimal)2.0)),
                        Expr.Constant((decimal)(1.66665 + delta))
                    ),
                    Expr.Constant(true),
                    "Divide 11"
                )
            );
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.GreaterThanOrEqual(
                        Expr.Divide(Expr.Constant((decimal)3.3333), Expr.Constant((decimal)2.0)),
                        Expr.Constant((decimal)(1.66665 - delta))
                    ),
                    Expr.Constant(true),
                    "Divide 11_1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 6", new string[] { "positive", "divide", "operators", "Pri1" })]
        public static Expr Divide6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant(4, typeof(Int32)), Expr.Constant(3, typeof(Int32)), null), Expr.Constant(1, typeof(Int32))));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a MethodInfo that takes no arguments
        public static int DivideNoArgs() {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 7", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Divide7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Divide).GetMethod("DivideNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Divide(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // pass a MethodInfo that takes a paramarray
        public static int DivideParamArray(params int[] args) {
            return -1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 8", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Divide8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Divide).GetMethod("DivideParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Divide(Expr.Constant(1, typeof(Int32)), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // with a valid MethodInfo, divide two values of the same type
        public static int DivideInts(int x, int y) {
            return x / y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 9", new string[] { "positive", "divide", "operators", "Pri1" })]
        public static Expr Divide9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Divide).GetMethod("DivideInts");

            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant(10, typeof(Int32)), Expr.Constant(3, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32))));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 10", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Divide10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Divide).GetMethod("DivideInts");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Divide(Expr.Constant((Int16)1), Expr.Constant((Int16)2), mi), Expr.Constant((Int16)3));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public static string DivideExceptionMsg(Exception e1, Exception e2) {
            return e1.Message + e2.Message;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 11", new string[] { "positive", "divide", "operators", "Pri1" })]
        public static Expr Divide11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Divide).GetMethod("DivideExceptionMsg");

            Expr Res =
                Expr.Divide(
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
        public static int DivideInt(int x, int y) {
            return x / y;
        }

        public static int DivideNullableInt(int? x, int y) {
            return x.GetValueOrDefault() / y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 12", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Divide12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo mi = typeof(Divide).GetMethod("DivideInt");
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.Divide(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(2, typeof(Int32)), "Divide 1"); }));
            return Expr.Empty();
        }

        // With a valid methodinfo, pass a value of a non nullable type, the other of nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 12_1", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Divide12_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo mi = typeof(Divide).GetMethod("DivideInt");
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.Divide(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(0, typeof(Int32)), "Divide 1"); }));
            return Expr.Empty();
        }

        // With a valid methodinfo, pass a value of a non nullable type, the other of nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 12_2", new string[] { "positive", "divide", "operators", "Pri1" })]
        public static Expr Divide12_2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Divide).GetMethod("DivideNullableInt");
            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(2, typeof(Int32)), "Divide 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Divide(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(0, typeof(Int32)), "Divide 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 13", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Divide13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.Divide(Expr.Constant(1, typeof(Int32)), Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), mi), Expr.Constant(0, typeof(Int32))
                )
        ;
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // with a valid methodinfo that returns boolean, pass arguments of nullable type
        public static bool IsTrueNullable(bool? x, bool? y) {
            if (x.HasValue && y.HasValue)
                return true;
            return false;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 14", new string[] { "positive", "divide", "operators", "Pri1" })]
        public static Expr Divide14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Divide).GetMethod("IsTrueNullable");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Divide(
                        Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)),
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(true, typeof(bool)),
                    "Divide 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Divide(
                        Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)),
                        Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(false, typeof(bool)),
                    "Divide 2"
                )
            );
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo that returns boolean, pass arguments of nullable type. 
        public static bool IsTrue(bool x, bool y) {
            return x && y;
        }

        // with a valid methodinfo that returns boolean, pass arguments of nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 14_1", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Divide14_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Divide).GetMethod("IsTrue");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.Divide(
                        Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)),
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(true, typeof(bool)),
                    "Divide 1"
                )
        ;
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Divideing across mixed types
        // TODO: more types, automated generation?
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Divide 15", new string[] { "negative", "divide", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Divide15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Divide(Expr.Constant(1), Expr.Constant(2.0), mi), Expr.Constant(3)
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

            public static int operator /(MyVal v1, int v2) {
                return (v1.Val + v1.Val) / (v2 + v2);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 16", new string[] { "positive", "divide", "operators", "Pri1" })]
        public static Expr Divide16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Divide(Expr.Constant(new MyVal(6)), Expr.Constant(2), mi), Expr.Constant(3)
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Verify order of evaluation of expressions on divide
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 17", new string[] { "positive", "divide", "operators", "Pri1" })]
        public static Expr Divide17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.Divide(
                    Expr.Block(EU.ConcatEquals(Result, "One"), Expr.Constant(24)),
                    Expr.Add(
                        Expr.Block(EU.ConcatEquals(Result, "Two"), Expr.Constant(1)),
                        Expr.Divide(
                            Expr.Block(EU.ConcatEquals(Result, "Three"), Expr.Constant(6)),
                            Expr.Constant(2)
                        )
                    ),
                    null
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(6)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("OneTwoThree")));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Divide zero by zero with single/double values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide 18", new string[] { "positive", "divide", "operators", "Pri1" })]
        public static Expr Divide18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo SingleMI = typeof(Single).GetMethod("IsNaN");
            Expr SingleValue = Expr.Divide(Expr.Constant((Single)0.0, typeof(Single)), Expr.Constant((Single)0.0, typeof(Single)));
            Expr SingleResult = Expr.Call(SingleMI, new Expression[] { SingleValue });

            MethodInfo DoubleMI = typeof(Double).GetMethod("IsNaN");
            Expr DoubleValue = Expr.Divide(Expr.Constant((double)0.0, typeof(double)), Expr.Constant((double)0.0, typeof(double)));
            Expr DoubleResult = Expr.Call(DoubleMI, new Expression[] { DoubleValue });

            Expressions.Add(EU.GenAreEqual(SingleResult, Expr.Constant(true), "Divide 1"));
            Expressions.Add(EU.GenAreEqual(DoubleResult, Expr.Constant(true), "Divide 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
