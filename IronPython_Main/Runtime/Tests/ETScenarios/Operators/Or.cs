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

    public class Or {
        // Or of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 1", new string[] { "positive", "Or", "operators", "Pri1" })]
        public static Expr Or1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant((Int16)0x00), Expr.Constant((Int16)0x01)), Expr.Constant((Int16)0x01), "Or 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant((UInt16)0x01), Expr.Constant((UInt16)0x10)), Expr.Constant((UInt16)0x11), "Or 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant((short)0x10), Expr.Constant((short)0x10)), Expr.Constant((short)0x10), "Or 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant((ushort)0x01), Expr.Constant((ushort)0x01)), Expr.Constant((ushort)0x01), "Or 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant((Int32)0x0001), Expr.Constant((Int32)0x0010)), Expr.Constant((Int32)0x0011), "Or 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant((UInt32)0x0110), Expr.Constant((UInt32)0x0101)), Expr.Constant((UInt32)0x0111), "Or 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant((long)0x1111), Expr.Constant((long)0x0011)), Expr.Constant((long)0x1111), "Or 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant((ulong)0x1100), Expr.Constant((ulong)0x1010)), Expr.Constant((ulong)0x1110), "Or 8"));
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant((SByte)0x01), Expr.Constant((SByte)0x10)), Expr.Constant((SByte)0x11), "Or 9")); // Int16 is CLS compliant equivalent type
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant((Byte)0x10), Expr.Constant((Byte)0x11)), Expr.Constant((Byte)0x11), "Or 10"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Or of Single constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 2", new string[] { "negative", "Or", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Or2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Or(Expr.Constant((Single)0x1100), Expr.Constant((Single)0x1010)), Expr.Constant((Single)0x0110), "Or 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Or of Double constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 3", new string[] { "negative", "Or", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Or3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Or(Expr.Constant((double)0x1100), Expr.Constant((double)0x1010)), Expr.Constant((double)0x0110), "Or 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Or of Decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 4", new string[] { "negative", "Or", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Or4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Or(Expr.Constant((decimal)0x1100), Expr.Constant((decimal)0x1010)), Expr.Constant((decimal)0x0110), "Or 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Or of boolean constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 5", new string[] { "positive", "Or", "operators", "Pri1" })]
        public static Expr Or5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant(true), Expr.Constant(false)), Expr.Constant(true), "Or 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant(false), Expr.Constant(false)), Expr.Constant(false), "Or 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Or(Expr.Constant(true), Expr.Constant(true)), Expr.Constant(true), "Or 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Or of string constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 6", new string[] { "negative", "Or", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Or6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Or(Expr.Constant("Hello"), Expr.Constant("World")), Expr.Constant("HelloWorld"), "Or 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Or of class object, no user defined operator
        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 7", new string[] { "negative", "Or", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Or7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Or(Expr.Constant(new TestClass(1)), Expr.Constant(new TestClass(2))), Expr.Constant(new TestClass(-1)), "Or 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Or of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 8", new string[] { "negative", "Or", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Or8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Or(Expr.Constant(new TestStruct(1)), Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "Or 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 9", new string[] { "positive", "Or", "operators", "Pri1" })]
        public static Expr Or9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Or(Expr.Constant((Int32)0x1101), Expr.Constant((Int32)0x1011), null),
                    Expr.Constant((Int32)0x1111),
                    "Or 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a methodinfo that takes no arguments
        public static int OrNoArgs() {
            int x = 1;
            return x;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 10", new string[] { "negative", "Or", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Or10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Or).GetMethod("OrNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(
                    Expr.Or(Expr.Constant(0x1001), Expr.Constant(0x0001), mi),
                    Expr.Constant(0x1000),
                    "Or 1"
                );
            }));

            return EU.BlockVoid(Expressions);
        }

        // Pass a methodinfo that takes a paramarray
        public static int OrParamArray(params int[] args) {
            if (args == null)
                return -1;
            return 0;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 11", new string[] { "negative", "Or", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Or11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Or).GetMethod("OrParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(
                    Expr.Or(Expr.Constant(0x0001), Expr.Constant(0x0001), mi),
                    Expr.Constant(0x0001),
                    "Or 1"
                );
            }));

            return EU.BlockVoid(Expressions);
        }

        // TODO : Ask if this should replace existing test
        //        With a valid method info, and two values of the same type 
        public class MyType {
            public bool Val { get; set; }

            public static bool operator true(MyType x) {
                return x.Val;
            }
            public static bool operator false(MyType x) {
                return !(x.Val) ? true : false;
            }
            public static MyType Or(MyType x, MyType y) {
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

        // With a valid method info, Or two values of the same type
        public static int OrTwoArgs(int x, int y) {
            return x & y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 12", new string[] { "positive", "Or", "operators", "Pri1" })]
        public static Expr Or12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Or).GetMethod("OrTwoArgs");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Or(Expr.Constant(0x1111), Expr.Constant(0x0111), mi),
                    Expr.Constant(0x0111),
                    "Or 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 13", new string[] { "negative", "Or", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Or13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Or).GetMethod("OrTwoArgs");

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(
                    Expr.Or(Expr.Constant((Int16)(0x1111)), Expr.Constant((Int16)(0x0111)), mi),
                    Expr.Constant(0x1000),
                    "Or 1"
                );
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // TODO : Ask if this should replace existing test
        //        With a valid method info, pass two values of a derived class of the methodinfo's arguments
        public class MyDerivedType : MyType {
            public int DerivedData { get; set; }
        }


        // With a valid method info, pass two values of a derived class of the methodinfo's arguments
        public static string OrExceptionMsg(Exception e1, Exception e2) {
            return e1.Message + e2.Message;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 14", new string[] { "positive", "Or", "operators", "Pri1" })]
        public static Expr Or14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Or).GetMethod("OrExceptionMsg");

            Expr Res =
                Expr.Or(
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
        public static int OrNullableInt(int? x, int y) {
            return (x ?? 0) ^ y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 15", new string[] { "positive", "Or", "operators", "Pri1" })]
        public static Expr Or15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Or).GetMethod("OrNullableInt");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Or(Expr.Constant((Nullable<int>)2, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(3, typeof(Int32)), "Or 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Or(Expr.Constant((Nullable<int>)null, typeof(Nullable<int>)), Expr.Constant(1), mi), Expr.Constant(1), "Or 2"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 16", new string[] { "negative", "Or", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Or16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Or(Expr.Constant(0x0011, typeof(Int32)), Expr.Constant((Nullable<int>)0x1111, typeof(Nullable<int>)), mi), Expr.Constant(0x1100, typeof(Int32)));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid methodinfo that returns boolean, pass arguments of nullable type. 
        public static bool IsTrue(bool? x, bool? y) {
            return x.GetValueOrDefault() ^ y.GetValueOrDefault();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 17", new string[] { "positive", "Or", "operators", "Pri1" })]
        public static Expr Or17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Or).GetMethod("IsTrue");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Or(
                        Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)),
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(true),
                    "Or 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Or(
                        Expr.Constant((Nullable<bool>)true, typeof(Nullable<bool>)),
                        Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(true, typeof(bool)),
                    "Or 2"
                )
            );
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // User defined overloaded operator on left argument, arguments are the proper types
        public class MyVal {
            public int Val { get; set; }

            public MyVal(int x) { Val = x; }

            public static int operator |(MyVal v1, int v2) {
                return v1.Val | v2;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 18", new string[] { "positive", "Or", "operators", "Pri1" })]
        public static Expr Or18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(Expr.Constant(0x0111),
                    Expr.Or(Expr.Constant(new MyVal(0x0001)), Expr.Constant(0x0111), mi)
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Or of false and other expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or 19", new string[] { "positive", "Or", "operators", "Pri1" })]
        public static Expr Or19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.Or(
                    Expr.Block(EU.ConcatEquals(Result, "False"), Expr.Constant(false)),
                    Expr.Block(EU.ConcatEquals(Result, "Expression"), Expr.Constant(true))
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("FalseExpression")));

            var tree = EU.BlockVoid(new ParameterExpression[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
