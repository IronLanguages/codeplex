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

    public class OrAssign {
        // OrAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 1", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression v1 = Expr.Parameter(typeof(Int16), "v1");
            Expressions.Add(Expr.Assign(v1, Expr.Constant((Int16)0x00)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(v1, Expr.Constant((Int16)0x01)), Expr.Constant((Int16)0x01), "OrAssign 1"));

            ParameterExpression v2 = Expr.Parameter(typeof(UInt16), "v2");
            Expressions.Add(Expr.Assign(v2, Expr.Constant((UInt16)0x01)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(v2, Expr.Constant((UInt16)0x10)), Expr.Constant((UInt16)0x11), "OrAssign 2"));

            ParameterExpression v3 = Expr.Parameter(typeof(short), "v3");
            Expressions.Add(Expr.Assign(v3, Expr.Constant((short)0x10)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(v3, Expr.Constant((short)0x10)), Expr.Constant((short)0x10), "OrAssign 3"));

            ParameterExpression v4 = Expr.Parameter(typeof(ushort), "v4");
            Expressions.Add(Expr.Assign(v4, Expr.Constant((ushort)0x01)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(v4, Expr.Constant((ushort)0x01)), Expr.Constant((ushort)0x01), "OrAssign 4"));

            ParameterExpression v5 = Expr.Parameter(typeof(int), "v5");
            Expressions.Add(Expr.Assign(v5, Expr.Constant((int)0x0001)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(v5, Expr.Constant((Int32)0x0010)), Expr.Constant((Int32)0x0011), "OrAssign 5"));

            ParameterExpression v6 = Expr.Parameter(typeof(uint), "v6");
            Expressions.Add(Expr.Assign(v6, Expr.Constant((uint)0x0110)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(v6, Expr.Constant((UInt32)0x0101)), Expr.Constant((UInt32)0x0111), "OrAssign 6"));

            ParameterExpression v7 = Expr.Parameter(typeof(long), "v7");
            Expressions.Add(Expr.Assign(v7, Expr.Constant((long)0x1111)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(v7, Expr.Constant((long)0x0011)), Expr.Constant((long)0x1111), "OrAssign 7"));

            ParameterExpression v8 = Expr.Parameter(typeof(ulong), "v8");
            Expressions.Add(Expr.Assign(v8, Expr.Constant((ulong)0x1100)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(v8, Expr.Constant((ulong)0x1010)), Expr.Constant((ulong)0x1110), "OrAssign 8"));

            ParameterExpression v9 = Expr.Parameter(typeof(SByte), "v9");
            Expressions.Add(Expr.Assign(v9, Expr.Constant((SByte)0x01)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(v9, Expr.Constant((SByte)0x10)), Expr.Constant((SByte)0x11), "OrAssign 9")); // Int16 is CLS compliant equivalent type

            ParameterExpression v10 = Expr.Parameter(typeof(Byte), "v10");
            Expressions.Add(Expr.Assign(v10, Expr.Constant((Byte)0x10)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(v10, Expr.Constant((Byte)0x11)), Expr.Constant((Byte)0x11), "OrAssign 10"));

            var tree = Expr.Block(new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // OrAssign of Single constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 2", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(Single), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Single)0x1100)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.OrAssign(Left, Expr.Constant((Single)0x1010)), Expr.Constant((Single)0x0110), "OrAssign 1");
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // OrAssign of Double constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 3", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(Double), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Double)0x1100)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.OrAssign(Left, Expr.Constant((double)0x1010)), Expr.Constant((double)0x0110), "OrAssign 1");
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // OrAssign of Decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 4", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(Decimal), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Decimal)0x1100)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.OrAssign(Left, Expr.Constant((decimal)0x1010)), Expr.Constant((decimal)0x0110), "OrAssign 1");
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // OrAssign of boolean constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 5", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left1 = Expr.Parameter(typeof(bool), "Left1");
            Expressions.Add(Expr.Assign(Left1, Expr.Constant((bool)true)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(Left1, Expr.Constant(false)), Expr.Constant(true), "OrAssign 1"));

            ParameterExpression Left2 = Expr.Parameter(typeof(bool), "Left2");
            Expressions.Add(Expr.Assign(Left2, Expr.Constant((bool)false)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(Left2, Expr.Constant(false)), Expr.Constant(false), "OrAssign 2"));

            ParameterExpression Left3 = Expr.Parameter(typeof(bool), "Left3");
            Expressions.Add(Expr.Assign(Left3, Expr.Constant((bool)true)));
            Expressions.Add(EU.GenAreEqual(Expr.OrAssign(Left3, Expr.Constant(true)), Expr.Constant(true), "OrAssign 3"));

            var tree = Expr.Block(new[] { Left1, Left2, Left3 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // OrAssign of string constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 6", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(string), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((string)"Hello")));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.OrAssign(Left, Expr.Constant("WOrAssignld")), Expr.Constant("HelloWOrAssignld"), "OrAssign 1");
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // OrAssign of class object, no user defined operator
        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 7", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(TestClass), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestClass(1), typeof(TestClass))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.OrAssign(Left, Expr.Constant(new TestClass(2))), Expr.Constant(new TestClass(-1)), "OrAssign 1");
            }));
            return Expr.Empty();
        }

        // OrAssign of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 8", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(TestStruct), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1), typeof(TestStruct))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.OrAssign(Left, Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "OrAssign 1");
            }));

            return Expr.Empty();
        }

        // Pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 9", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(0x1101)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.OrAssign(Left, Expr.Constant((Int32)0x1011), null),
                    Expr.Constant((Int32)0x1111),
                    "OrAssign 1"
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a methodinfo that takes no arguments
        public static int OrAssignNoArgs() {
            int x = 1;
            return x;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 10", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignNoArgs");

            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(0x0001)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {

                EU.GenAreEqual(
                    Expr.OrAssign(Left, Expr.Constant(0x0001), mi),
                    Expr.Constant(0x1000),
                    "OrAssign 1"
                )
        ;
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }

        // Pass a methodinfo that takes a paramarray
        public static int OrAssignParamArray(params int[] args) {
            if (args == null)
                return -1;
            return 0;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 11", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignParamArray");

            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(0x0001)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {

                EU.GenAreEqual(
                    Expr.OrAssign(Left, Expr.Constant(0x0001), mi),
                    Expr.Constant(0x0001),
                    "OrAssign 1"
                )
        ;
            }));

            return Expr.Block(new[] { Left }, Expressions);
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
            public static MyType OrAssign(MyType x, MyType y) {
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

        // With a valid method info, OrAssign two values of the same type
        public static int OrAssignTwoArgs(int x, int y) {
            return x & y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 12", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignTwoArgs");

            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(0x0111)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.OrAssign(Left, Expr.Constant(0x0111), mi),
                    Expr.Constant(0x0111),
                    "OrAssign 1"
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 13", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignTwoArgs");

            ParameterExpression Left = Expr.Parameter(typeof(Int16), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((Int16)0x1111)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.OrAssign(Left, Expr.Constant((Int16)(0x0111)), mi),
                    Expr.Constant(0x1000),
                    "OrAssign 1"
                )
        ;
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // TODO : Ask if this should replace existing test
        //        With a valid method info, pass two values of a derived class of the methodinfo's arguments
        public class MyDerivedType : MyType {
            public int DerivedData { get; set; }
        }


        // With a valid method info, pass two values of a derived class of the methodinfo's arguments
        public static DivideByZeroException ExclusiveOrAssignExceptionMsg(Exception e1, Exception e2) {
            return new DivideByZeroException(e1.Message + e2.Message);
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 14", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("ExclusiveOrAssignExceptionMsg");

            ParameterExpression Left = Expr.Parameter(typeof(DivideByZeroException), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new DivideByZeroException("One"), typeof(DivideByZeroException))));

            Expr Res =
                Expr.OrAssign(
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
        public static int? OrAssignNullableInt(int? x, int y) {
            return (x ?? 0) ^ y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 15", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignNullableInt");

            ParameterExpression Left = Expr.Parameter(typeof(Nullable<int>), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2, typeof(Nullable<int>))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.OrAssign(Left, Expr.Constant(1), mi), Expr.Constant(3, typeof(Int32?)), "OrAssign 1"
                )
            );

            Expressions.Add(Expr.Assign(Left, Expr.Constant(null, typeof(Nullable<int>))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.OrAssign(Left, Expr.Constant(1), mi), Expr.Constant(1, typeof(int?)), "OrAssign 2"
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 16", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            ParameterExpression Left = Expr.Parameter(typeof(Nullable<int>), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(0x0011, typeof(int?))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.OrAssign(Left, Expr.Constant((Nullable<int>)0x1111, typeof(Nullable<int>)), mi), Expr.Constant(0x1100, typeof(Int32))
                )
        ;
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // With a valid methodinfo that returns boolean, pass arguments of nullable type. 
        public static bool? IsTrue(bool? x, bool? y) {
            return x.GetValueOrDefault() ^ y.GetValueOrDefault();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 17", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("IsTrue");

            ParameterExpression Left = Expr.Parameter(typeof(Nullable<bool>), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(true, typeof(Nullable<bool>))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.OrAssign(
                        Left,
                        Expr.Constant((Nullable<bool>)false, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(true, typeof(bool?)),
                    "OrAssign 1"
                )
            );

            Expressions.Add(Expr.Assign(Left, Expr.Constant(true, typeof(Nullable<bool>))));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.OrAssign(
                        Left,
                        Expr.Constant((Nullable<bool>)null, typeof(Nullable<bool>)),
                        mi
                    ),
                    Expr.Constant(true, typeof(bool?)),
                    "OrAssign 2"
                )
            );
            var tree = Expr.Block(new[] { Left }, Expressions);
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
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "OrAssign 18", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(MyVal), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new MyVal(0x0001), typeof(MyVal))));

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(Expr.Constant(0x0111),
                    Expr.OrAssign(Left, Expr.Constant(0x0111), mi)
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class TestOrder {
            public string res;
            private bool _Value;
            public bool this[string append] {
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

        // OrAssign of false and other expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 19", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            TestOrder order = new TestOrder();

            ParameterExpression ov = Expr.Parameter(typeof(TestOrder), "order");

            Expressions.Add(Expr.Assign(ov, Expr.Constant(order)));
            Expressions.Add(Expr.Assign(Expr.Property(ov, order.pi, Expr.Constant("")), Expr.Constant(false)));

            Expr Res =
                Expr.OrAssign(
                    Expr.Property(ov, order.pi, Expr.Constant("One")),
                    Expr.Block(Expr.Assign(Expr.Property(ov, order.pi, Expr.Constant("Three")), Expr.Constant(true)), Expr.Property(ov, order.pi, Expr.Constant("Two")))
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(true)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("OneThreeTwoOne"), Expr.Field(ov, order.resi)));

            var tree = Expr.Block(new[] { ov }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class A20 {
            static public int x;
        }

        //pass a field to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 20", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            A20.x = 0x0101;

            var Left = Expr.Field(null, typeof(A20).GetField("x"));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.OrAssign(Left, Expr.Constant(0x0110), mi), Expr.Constant(0x0111)
                )
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(0x0111), Left));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass a non assignable expression to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 20_1", new string[] { "negative", "OrAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign20_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignInts");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.OrAssign(Expr.Constant(1), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(Expressions);
        }

        // OrAssign with an array index expression as the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 21", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Variable(typeof(int[]), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6), Expr.Constant(7), Expr.Constant(8) })));

            Expressions.Add(Expr.OrAssign(Expr.ArrayAccess(Left, Expr.Constant(2)), Expr.Constant(4)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Expr.ArrayIndex(Left, Expr.Constant(2)), "SA 1"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 25", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("BadReturn");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.OrAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }


        //Pass null to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 26", new string[] { "negative", "OrAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr OrAssign26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.OrAssign(null, Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        //Pass null to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 27", new string[] { "negative", "OrAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr OrAssign27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.OrAssign(Left, null);
            }));

            return Expr.Empty();
        }

        //Pass a non value returning expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 29", new string[] { "negative", "OrAssign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.OrAssign(Left, Expr.Empty());
            }));

            var tree = Expr.Empty();

            return tree;
        }

        //Pass a Block to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 30", new string[] { "negative", "OrAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Block(Expr.Constant(1));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.OrAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a Method Call to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 31", new string[] { "negative", "OrAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Call(Expr.Constant("1"), typeof(string).GetMethod("ToString", new Type[] { }));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.OrAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }


        //Pass a constant to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 33", new string[] { "negative", "OrAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign33(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Constant(1);
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.OrAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        // pass a MethodInfo that takes three arguments
        public static int OrAssign3Args(int arg1, int arg2, int arg3) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 34", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssign3Args");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.OrAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }

        // pass a MethodInfo with two arguments, that returns void
        public static void OrAssign2ArgsVoid(int arg1, int arg2) {
            return;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 35", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssign2ArgsVoid");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.OrAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }


        //with a valid method info, pass two values of an enum class (methodinfo's arguments are of the base integer type)
        public static int OrAssign2Args(int arg1, int arg2) {
            return arg1 + arg2;
        }
        enum e36 : int {
        }
        //enum -> int conversion not being accepted.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 36", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssign2Args");

            ParameterExpression Left = Expr.Parameter(typeof(e36), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((e36)1)));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.OrAssign(Left, Expr.Constant((e36)2), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type
        public static int OrAssignMethod37(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 37", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignMethod37");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(Expr.OrAssign(Left, Expr.Constant(2, typeof(int?)), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //with a valid method info, pass two values of the same nullable type
        public static int OrAssignMethod38(int? arg1, int? arg2) {
            return (arg1 + arg2).GetValueOrDefault();
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 38", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignMethod38");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.OrAssign(Left, Expr.Constant(2, typeof(int?)), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type. return is nullable, arguments aren't
        public static int? OrAssignMethod39(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 39", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignMethod39");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.OrAssign(Left, Expr.Constant(2, typeof(int?)), mi);
            }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Empty();

            return tree;
        }




        //User defined operator on right argument, arguments are of the proper types
        public class C40 {
            public int Val = 2;
            public static int operator |(int a, C40 b) {
                return b.Val + a;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 40", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(Expr.OrAssign(Left, Expr.Constant(new C40())));

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
            public static C41 operator |(C41 b, Exception a) {
                return new C41(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 41", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C41), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C41("1"))));

            Expressions.Add(Expr.OrAssign(Left, Expr.Constant(new ArgumentException("2"))));

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
            public static Exception operator |(Exception a, C42 b) {
                return new Exception(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 42", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(Exception), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new Exception("1"))));

            Expressions.Add(Expr.OrAssign(Left, Expr.Constant(new C42("2"))));

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
            public static C43 operator |(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "A");
            }
        }
        public class C43_1 {
            public string Val;

            public C43_1(string init) {
                Val = init;
            }
            public static C43 operator |(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "B");
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 43", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C43), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C43("1"))));

            Expressions.Add(Expr.OrAssign(Left, Expr.Constant(new C43_1("2"))));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 44", new string[] { "positive", "OrAssign", "operators", "Pri2" })]
        public static Expr OrAssign44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C44), "instance");

            Expr Left = Expr.Field(Instance, C44.Field);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C44())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.OrAssign(Left, Expr.Constant(5)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 45", new string[] { "positive", "OrAssign", "operators", "Pri2" })]
        public static Expr OrAssign45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Field(null, C45.Field);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.OrAssign(Left, Expr.Constant(5)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 46", new string[] { "positive", "OrAssign", "operators", "Pri2" })]
        public static Expr OrAssign46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C46), "instance");

            Expr Left = Expr.Property(Instance, C46.Property);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C46())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.OrAssign(Left, Expr.Constant(5)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 47", new string[] { "positive", "OrAssign", "operators", "Pri2" })]
        public static Expr OrAssign47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Property(null, C47.Property);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.OrAssign(Left, Expr.Constant(5)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 48", new string[] { "positive", "OrAssign", "operators", "Pri2" })]
        public static Expr OrAssign48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C48), "instance");

            Expr Left = Expr.Property(Instance, C48.Property, new[] { Expr.Constant(2) });

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C48())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(3)));

            Expressions.Add(Expr.OrAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(11), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }


        //OrAssign of Boolean and integer
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 49", new string[] { "negative", "OrAssign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign49(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(bool), "Left");

            Expressions.Add(Expr.Assign(Left, Expr.Constant(true)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.OrAssign(Left, Expr.Constant(5));
            }));

            var tree = Expr.Empty();

            return tree;
        }






        public static int OrAssignConv(short arg1, short arg2) {
            return (int)(arg1 + arg2);
        }
        //OrAssign with a lambda conversion for the return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 49_1", new string[] { "positive", "OrAssign", "operators", "Pri1" })]
        public static Expr OrAssign49_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(short)), arg1
                                                );
            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignConv");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((short)1)));
            Expressions.Add(
                            EU.GenAreEqual(
                                            Expr.Constant((short)3),
                                            Expr.OrAssign(Left, Expr.Constant((short)2), mi, Conv)
                                           )
                           );


            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //OrAssign with a lambda conversion for the return type, to the wrong type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 50_1", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign50_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(byte)), arg1
                                                );
            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignConv");

            EU.Throws<System.InvalidOperationException>(() => { Expr.OrAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }

        //Passing null to conversion lambda
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 51_1", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr OrAssign51_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            LambdaExpression Conv = null;
            MethodInfo mi = typeof(OrAssign).GetMethod("OrAssignConv");

            EU.Throws<System.ArgumentException>(() => { Expr.OrAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }


        //Passing a conversion lambda when it's not needed
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrAssign 52_1", new string[] { "negative", "OrAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr OrAssign52_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(int)), arg1
                                                );

            MethodInfo mi = null;// typeof(OrAssign).GetMethod("OrAssignInts");

            EU.Throws<System.InvalidOperationException>(() => { Expr.OrAssign(Left, Expr.Constant(2), mi, Conv); });

            return Expr.Empty();
        }

    }
}
