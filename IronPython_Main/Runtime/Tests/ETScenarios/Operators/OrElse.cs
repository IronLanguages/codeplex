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

    public class OrElse {
        // OrElse of sbyte, byte, short, ushort, int, uint,long,ulong constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 1", new string[] { "negative", "orelse", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((Int16)0x00), Expr.Constant((Int16)0x01)), Expr.Constant((Int16)0x00), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 1_1", new string[] { "negative", "orelse", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse1_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((UInt16)0x01), Expr.Constant((UInt16)0x10)), Expr.Constant((UInt16)0x00), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 1_2", new string[] { "negative", "orelse", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse1_2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((short)0x10), Expr.Constant((short)0x10)), Expr.Constant((short)0x10), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 1_3", new string[] { "negative", "orelse", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse1_3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((ushort)0x01), Expr.Constant((ushort)0x01)), Expr.Constant((ushort)0x01), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 1_4", new string[] { "negative", "orelse", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse1_4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((Int32)0x0001), Expr.Constant((Int32)0x0001)), Expr.Constant((Int32)0x0001), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 1_5", new string[] { "negative", "orelse", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse1_5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((UInt32)0x0110), Expr.Constant((UInt32)0x0101)), Expr.Constant((UInt32)0x0100), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 1_6", new string[] { "negative", "orelse", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse1_6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((long)0x1111), Expr.Constant((long)0x0011)), Expr.Constant((long)0x0011), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 1_7", new string[] { "negative", "orelse", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse1_7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((ulong)0x1100), Expr.Constant((ulong)0x1010)), Expr.Constant((ulong)0x1000), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 1_8", new string[] { "negative", "orelse", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse1_8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((SByte)0x01), Expr.Constant((SByte)0x01)), Expr.Constant((SByte)0x01), "And 1"); })); // Int16 is CLS compliant equivalent type
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 1_9", new string[] { "negative", "orelse", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse1_9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((Byte)0x10), Expr.Constant((Byte)0x11)), Expr.Constant((Byte)0x10), "And 1"); }));
            return Expr.Empty();
        }

        // OrElse of Single constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 2", new string[] { "negative", "orelse", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((Single)0x1100), Expr.Constant((Single)0x1010)), Expr.Constant((Single)0x1000), "OrElse 1"); }));
            return Expr.Empty();
        }

        // OrElse of Double constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 3", new string[] { "negative", "orelse", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((double)0x1100), Expr.Constant((double)0x1010)), Expr.Constant((double)0x1000), "OrElse 1"); }));
            return Expr.Empty();
        }

        // OrElse of Decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 4", new string[] { "negative", "orelse", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant((decimal)0x1100), Expr.Constant((decimal)0x1010)), Expr.Constant((decimal)0x1000), "OrElse 1"); }));
            return Expr.Empty();
        }

        // OrElse of boolean constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 5", new string[] { "positive", "orelse", "operators", "Pri1" })]
        public static Expr OrElse5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.OrElse(Expr.Constant(true), Expr.Constant(false)), Expr.Constant(true), "OrElse 1"));
            Expressions.Add(EU.GenAreEqual(Expr.OrElse(Expr.Constant(false), Expr.Constant(false)), Expr.Constant(false), "OrElse 2"));
            Expressions.Add(EU.GenAreEqual(Expr.OrElse(Expr.Constant(true), Expr.Constant(true)), Expr.Constant(true), "OrElse 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // OrElse of string constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 6", new string[] { "negative", "orelse", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant("Hello"), Expr.Constant("World")), Expr.Constant("HelloWorld"), "OrElse 1"); }));
            return Expr.Empty();
        }

        // OrElse of class object, no user defined operator
        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 7", new string[] { "negative", "orelse", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant(new TestClass(1)), Expr.Constant(new TestClass(2))), Expr.Constant(new TestClass(-1)), "OrElse 1"); }));

            return Expr.Empty();
        }

        // OrElse of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 8", new string[] { "negative", "orelse", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.OrElse(Expr.Constant(new TestStruct(1)), Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "OrElse 1"); }));

            return Expr.Empty();
        }

        // Pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 9", new string[] { "positive", "orelse", "operators", "Pri1" })]
        public static Expr OrElse9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.OrElse(Expr.Constant(true), Expr.Constant(false), null),
                    Expr.Constant(true),
                    "OrElse 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a methodinfo that takes no arguments
        public static int OrElseNoArgs() {
            int x = 1;
            return x;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 10", new string[] { "negative", "orelse", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OrElse10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrElse).GetMethod("OrElseNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(
                    Expr.OrElse(Expr.Constant(0x0001), Expr.Constant(0x0001), mi),
                    Expr.Constant(0x0001),
                    "OrElse 1"
                );
            }));

            return EU.BlockVoid(Expressions);
        }

        // Pass a methodinfo that takes a paramarray
        public static int OrElseParamArray(params int[] args) {
            if (args == null)
                return -1;
            return 0;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 11", new string[] { "negative", "orelse", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OrElse11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrElse).GetMethod("OrElseParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(
                    Expr.OrElse(Expr.Constant(0x0001), Expr.Constant(0x0001), mi),
                    Expr.Constant(0x0001),
                    "OrElse 1"
                );
            }));

            return EU.BlockVoid(Expressions);
        }

        // With a valid method info, and two values of the same type 
        public class MyType {
            public bool Val { get; set; }

            public static bool operator true(MyType x) {
                return x.Val;
            }
            public static bool operator false(MyType x) {
                return !(x.Val) ? true : false;
            }
            public static MyType OrElse(MyType x, MyType y) {
                return new MyType { Val = x.Val || y.Val };
            }
            public static MyType operator |(MyType x, MyType y) {
                return new MyType { Val = x.Val || y.Val };
            }
            public static bool operator ==(MyType x, MyType y) {
                return (x.Val == y.Val);
            }
            public static bool operator !=(MyType x, MyType y) {
                return (x.Val != y.Val);
            }
            public static bool operator ==(MyType x, bool y) {
                return (x.Val == y);
            }
            public static bool operator !=(MyType x, bool y) {
                return (x.Val != y);
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 12", new string[] { "positive", "orelse", "operators", "Pri1" })]
        public static Expr OrElse12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MyType).GetMethod("OrElse");

            MyType left = new MyType { Val = true };
            MyType right = new MyType { Val = false };
            MyType expected = new MyType { Val = true };
            Expression Opp = Expr.OrElse(Expr.Constant(left, typeof(MyType)), Expr.Constant(right, typeof(MyType)), mi);


            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(expected, typeof(MyType)), Opp, "OrElse 12"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 13", new string[] { "negative", "orelse", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Create a fn that's parameters are wider then the calling args.
            MethodInfo mi = new Func<long, long, int>((long a, long b) => { return 1; }).Method;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.OrElse(Expr.Constant((Int16)(0x1111)), Expr.Constant((Int16)(0x0111)), mi),
                    Expr.Constant(0x0111),
                    "OrElse 13"
                );
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public class MyDerivedType : MyType {
            public int DerivedData { get; set; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 14", new string[] { "positive", "orelse", "operators", "Pri1" })]
        public static Expr OrElse14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MyDerivedType).GetMethod("OrElse");

            Expr Res =
                Expr.OrElse(
                    Expr.Constant(new MyDerivedType { Val = true }, typeof(MyType)),
                    Expr.Constant(new MyDerivedType { Val = false }, typeof(MyType)),
                    mi
                );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(new MyDerivedType { Val = true }),
                    Res

                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // OrElse with types that define IsTrue and IsFalse operators
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 15", new string[] { "positive", "orelse", "operators", "Pri1" })]
        public static Expr OrElse15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            PropertyInfo pi = typeof(MyType).GetProperty("Val");

            Expr Res =
                Expr.OrElse(
                    Expr.Constant(new MyType { Val = true }),
                    Expr.Constant(new MyType { Val = false })
                );

            Expr Res2 =
                Expr.OrElse(
                    Expr.Constant(new MyType { Val = true }),
                    Expr.Constant(new MyType { Val = true })
                );

            Expressions.Add(EU.GenAreEqual(Expr.Property(Expr.Constant(new MyType { Val = true }), pi), Expr.Property(Res, pi)));
            Expressions.Add(EU.GenAreEqual(Expr.Property(Expr.Constant(new MyType { Val = true }), pi), Expr.Property(Res2, pi)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 16", new string[] { "negative", "orelse", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrElse16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.OrElse(Expr.Constant(0x0011, typeof(Int32)), Expr.Constant((Nullable<int>)0x1111, typeof(Nullable<int>)), mi), Expr.Constant(0x0011, typeof(Int32))
                );
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // User defined overloaded operator on left argument, arguments are the proper types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 17", new string[] { "positive", "orelse", "operators", "Pri1" })]
        public static Expr OrElse17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.OrElse(
                        Expr.Constant(new MyType { Val = true }),
                        Expr.Constant(new MyType { Val = false }),
                        mi),
                    Expr.Constant(new MyType { Val = true })
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Test     : OrElse of false and other expression
        // Expected : Verify both expressions are evaluated, in the right order
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 18", new string[] { "positive", "orelse", "operators", "Pri1" })]
        public static Expr OrElse18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.OrElse(
                    Expr.Block(EU.ConcatEquals(Result, "False"), Expr.Constant(false)),
                    Expr.Block(EU.ConcatEquals(Result, "Expression"), Expr.Constant(true))
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Res));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("FalseExpression"), Result));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Test     : OrElse of false and other expression
        // Expected : verify second expression is not evaluated
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 18.1", new string[] { "positive", "orelse", "operators", "Pri1" })]
        public static Expr OrElse18_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.OrElse(
                    Expr.Block(EU.ConcatEquals(Result, "False"), Expr.Constant(true)),
                    Expr.Block(EU.ConcatEquals(Result, "Expression"), Expr.Constant(false))
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Res));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("False"), Result));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // OrElse of nullable Boolean, where left is false
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 19", new string[] { "positive", "orelse", "operators", "Pri1" })]
        public static Expr OrElse19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.OrElse(
                    Expr.Block(EU.ConcatEquals(Result, "False"), Expr.Constant(false, typeof(Nullable<bool>))),
                    Expr.Block(EU.ConcatEquals(Result, "Expression"), Expr.Constant(true, typeof(Nullable<bool>)))
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true, typeof(Nullable<bool>)), Res));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("FalseExpression"), Result));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // OrElse of nullable Boolean, where left is null
        // TODO: Dev10 bug 511157
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 20", new string[] { "positive", "orelse", "operators", "Pri1" })]
        public static Expr OrElse20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.OrElse(
                    Expr.Block(EU.ConcatEquals(Result, "False"), Expr.Constant(null, typeof(Nullable<bool>))),
                // if true - result is null but both parts evaluated("FalseExpression")
                // if false - result is false but both parts evaluated ("FalseExpression")
                    Expr.Block(EU.ConcatEquals(Result, "Expression"), Expr.Constant(true, typeof(Nullable<bool>)))
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true, typeof(Nullable<bool>)), "OrElse 1"));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("FalseExpression"), "OrElse 2"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }



        //regression for bug 634624
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse 23", new string[] { "negative", "OrElse", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OrElse23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var MyOr =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.OrElse(
                        Expression.Constant(false),
                        Expression.Constant(true),
                        ((Func<bool, bool, bool>)OrElseTest.OrElse).Method
                    );
                });


            return Expr.Empty();
        }

        public class OrElseTest {
            public static bool OrElse(bool arg1, bool arg2) {
                return arg1 || arg2;
            }
        }

    }
}


