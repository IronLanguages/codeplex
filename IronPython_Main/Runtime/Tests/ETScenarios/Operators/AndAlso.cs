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
        
    public class AndAlso {
        // AndAlso of sbyte, byte, short, ushort, int, uint,long,ulong constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 1", new string[] { "negative", "andalso", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((Int16)0x00), Expr.Constant((Int16)0x01)), Expr.Constant((Int16)0x00), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 1_1", new string[] { "negative", "andalso", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso1_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((UInt16)0x01), Expr.Constant((UInt16)0x10)), Expr.Constant((UInt16)0x00), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 1_2", new string[] { "negative", "andalso", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso1_2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((short)0x10), Expr.Constant((short)0x10)), Expr.Constant((short)0x10), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 1_3", new string[] { "negative", "andalso", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso1_3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((ushort)0x01), Expr.Constant((ushort)0x01)), Expr.Constant((ushort)0x01), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 1_4", new string[] { "negative", "andalso", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso1_4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((Int32)0x0001), Expr.Constant((Int32)0x0001)), Expr.Constant((Int32)0x0001), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 1_5", new string[] { "negative", "andalso", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso1_5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((UInt32)0x0110), Expr.Constant((UInt32)0x0101)), Expr.Constant((UInt32)0x0100), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 1_6", new string[] { "negative", "andalso", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso1_6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((long)0x1111), Expr.Constant((long)0x0011)), Expr.Constant((long)0x0011), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 1_7", new string[] { "negative", "andalso", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso1_7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((ulong)0x1100), Expr.Constant((ulong)0x1010)), Expr.Constant((ulong)0x1000), "And 1"); }));
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 1_8", new string[] { "negative", "andalso", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso1_8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((SByte)0x01), Expr.Constant((SByte)0x01)), Expr.Constant((SByte)0x01), "And 1"); })); // Int16 is CLS compliant equivalent type
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 1_9", new string[] { "negative", "andalso", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso1_9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((Byte)0x10), Expr.Constant((Byte)0x11)), Expr.Constant((Byte)0x10), "And 1"); }));
            return Expr.Empty();
        }

        // AndAlso of Single constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 2", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((Single)0x1100), Expr.Constant((Single)0x1010)), Expr.Constant((Single)0x1000), "AndAlso 1"); }));
            return Expr.Empty();
        }

        // AndAlso of Double constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 3", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((double)0x1100), Expr.Constant((double)0x1010)), Expr.Constant((double)0x1000), "AndAlso 1"); }));
            return Expr.Empty();
        }

        // AndAlso of Decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 4", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant((decimal)0x1100), Expr.Constant((decimal)0x1010)), Expr.Constant((decimal)0x1000), "AndAlso 1"); }));
            return Expr.Empty();
        }

        // AndAlso of boolean constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 5", new string[] { "positive", "andalso", "operators", "Pri1" })]
        public static Expr AndAlso5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.AndAlso(Expr.Constant(true), Expr.Constant(false)), Expr.Constant(false), "AndAlso 1"));
            Expressions.Add(EU.GenAreEqual(Expr.AndAlso(Expr.Constant(false), Expr.Constant(false)), Expr.Constant(false), "AndAlso 2"));
            Expressions.Add(EU.GenAreEqual(Expr.AndAlso(Expr.Constant(true), Expr.Constant(true)), Expr.Constant(true), "AndAlso 3"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // AndAlso of string constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 6", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<InvalidOperationException>(() => { EU.GenAreEqual(Expr.AndAlso(Expr.Constant("Hello"), Expr.Constant("World")), Expr.Constant("HelloWorld"), "AndAlso 1"); }));
            return Expr.Empty();
        }

        // AndAlso of class object, no user defined operator
        internal class TestClass {
            int _x;
            internal TestClass(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 7", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.AndAlso(Expr.Constant(new TestClass(1)), Expr.Constant(new TestClass(2))), Expr.Constant(new TestClass(-1)), "AndAlso 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // AndAlso of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 8", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.AndAlso(Expr.Constant(new TestStruct(1)), Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "AndAlso 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Pass null to method, same typed arguments to left and right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 9", new string[] { "positive", "andalso", "operators", "Pri1" })]
        public static Expr AndAlso9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AndAlso(Expr.Constant(true), Expr.Constant(false), null),
                    Expr.Constant(false),
                    "AndAlso 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a methodinfo that takes no arguments
        public static int AndAlsoNoArgs() {
            int x = 1;
            return x;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 10", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AndAlso10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AndAlso).GetMethod("AndAlsoNoArgs");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {

                EU.GenAreEqual(
                    Expr.AndAlso(Expr.Constant(0x0001), Expr.Constant(0x0001), mi),
                    Expr.Constant(0x0001),
                    "AndAlso 1"
                )
        ;
            }));

            return EU.BlockVoid(Expressions);
        }

        // Pass a methodinfo that takes a paramarray
        public static int AndAlsoParamArray(params int[] args) {
            if (args == null)
                return -1;
            return 0;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 11", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AndAlso11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AndAlso).GetMethod("AndAlsoParamArray");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {

                EU.GenAreEqual(
                    Expr.AndAlso(Expr.Constant(0x0001), Expr.Constant(0x0001), mi),
                    Expr.Constant(0x0001),
                    "AndAlso 1"
                )
        ;
            }));

            return EU.BlockVoid(Expressions);
        }

        // With a valid method info, and two values of the same type 
        public class MyType {
            public bool Val { get; set; }
            public int Data { get; set; } // for AndAlso17

            public static bool operator true(MyType x) {
                return x.Val;
            }
            public static bool operator false(MyType x) {
                return !(x.Val) ? true : false;
            }
            public static MyType operator &(MyType x, MyType y) {
                return new MyType { Val = x.Val & y.Val, Data = x.Data + y.Data };
            }
            public static bool operator ==(MyType x, MyType y) {
                return ((x.Val == y.Val) && (x.Data == y.Data));
            }
            public static bool operator !=(MyType x, MyType y) {
                return ((x.Val != y.Val) && (x.Data != y.Data));
            }
            // for AndAlso12/14
            public static MyType DerivedTest(MyType x, MyType y) {
                return new MyDerivedType { Val = x.Val | y.Val };
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 12", new string[] { "positive", "andalso", "operators", "Pri1" })]
        public static Expr AndAlso12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MyType).GetMethod("DerivedTest");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AndAlso(Expr.Constant(new MyType { Val = true }), Expr.Constant(new MyType { Val = false }), mi),
                    Expr.Constant(new MyType { Val = true }),
                    "AndAlso 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AndAlso(Expr.Constant(new MyType { Val = false }), Expr.Constant(new MyType { Val = false }), mi),
                    Expr.Constant(new MyType { Val = false }),
                    "AndAlso 2"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of the same value type that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 13", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = new Func<double, double, double>((double x, double y) => (x + y)).Method;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.AndAlso(Expr.Constant((Int16)(0x1111)), Expr.Constant((Int16)(0x0111)), mi),
                    Expr.Constant(0x0111),
                    "AndAlso 1"
                )
        ;
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public class MyDerivedType : MyType {
            public int DerivedData { get; set; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 14", new string[] { "positive", "andalso", "operators", "Pri1" })]
        public static Expr AndAlso14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MyType).GetMethod("DerivedTest");

            Expr Res =
                Expr.AndAlso(
                    Expr.Constant(new MyDerivedType { Val = true }, typeof(MyType)),
                    Expr.Constant(new MyDerivedType { Val = false }, typeof(MyType)),
                    mi
                );

            Expressions.Add(
                EU.GenAreEqual(
                    Res,
                    Expr.Constant(new MyDerivedType { Val = true })
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // AndAlso with types that define IsTrue and IsFalse operators
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 15", new string[] { "positive", "andalso", "operators", "Pri1" })]
        public static Expr AndAlso15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            PropertyInfo pi = typeof(MyType).GetProperty("Val");

            Expr Res =
                Expr.AndAlso(
                    Expr.Constant(new MyType { Val = true }),
                    Expr.Constant(new MyType { Val = false })
                );

            Expr Res2 =
                Expr.AndAlso(
                    Expr.Constant(new MyType { Val = true }),
                    Expr.Constant(new MyType { Val = true })
                );

            Expressions.Add(EU.GenAreEqual(Expr.Property(Res, pi), Expr.Property(Expr.Constant(new MyType { Val = false }), pi)));
            Expressions.Add(EU.GenAreEqual(Expr.Property(Res2, pi), Expr.Property(Expr.Constant(new MyType { Val = true }), pi)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a null methodinfo, pass a value of non nullable type, the other of nullable type. 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 16", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr AndAlso16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {

                EU.GenAreEqual(
                    Expr.AndAlso(Expr.Constant(0x0011, typeof(Int32)), Expr.Constant((Nullable<int>)0x1111, typeof(Nullable<int>)), mi), Expr.Constant(0x0011, typeof(Int32))
                )
        ;
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // User defined overloaded operator on left argument, arguments are the proper types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 17", new string[] { "positive", "andalso", "operators", "Pri1" })]
        public static Expr AndAlso17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = null;

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.AndAlso(
                        Expr.Constant(new MyType { Val = true, Data = 4 }),
                        Expr.Constant(new MyType { Val = false, Data = 5 }),
                        mi),
                    Expr.Constant(new MyType { Val = false, Data = 9 })
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // AndAlso of false and other expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 18", new string[] { "positive", "andalso", "operators", "Pri1" })]
        public static Expr AndAlso18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.AndAlso(
                    Expr.Block(EU.ConcatEquals(Result, "False"), Expr.Constant(false)),
                    Expr.Block(EU.ConcatEquals(Result, "Expression"), Expr.Constant(true))
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false)));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("False")));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // AndAlso of nullable Boolean, where left is false
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 19", new string[] { "positive", "andalso", "operators", "Pri1" })]
        public static Expr AndAlso19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.AndAlso(
                    Expr.Block(EU.ConcatEquals(Result, "False"), Expr.Constant(false, typeof(Nullable<bool>))),
                    Expr.Block(EU.ConcatEquals(Result, "Expression"), Expr.Constant(true, typeof(Nullable<bool>)))
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(false, typeof(Nullable<bool>))));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("False")));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // AndAlso of nullable Boolean, where left is null
        // Regression for Dev10 bug 511157
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 20", new string[] { "positive", "andalso", "operators", "Pri1" })]
        public static Expr AndAlso20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Res =
                Expr.AndAlso(
                    Expr.Block(EU.ConcatEquals(Result, "False"), Expr.Constant(null, typeof(Nullable<bool>))),
                    Expr.Block(EU.ConcatEquals(Result, "Expression"), Expr.Constant(true, typeof(Nullable<bool>)))
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(null, typeof(Nullable<bool>)), "AndAlso 1"));
            // if the right side is false, the result is false; if right is true, the result is null, so we need to evaluate both sides
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("FalseExpression"), "AndAlso 2"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }


        public static bool AndAlsoUD(bool arg1, bool arg2) {
            return true;
        }

        public static bool op_True(bool arg1) {
            return true;
        }

        public static bool op_False(bool arg1) {
            return true;
        }

        //Lifted user defined AndAlso
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 21", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AndAlso21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            MethodInfo mi = typeof(AndAlso).GetMethod("AndAlsoUD");

            Expr Res =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.AndAlso(
                        Expr.Block(EU.ConcatEquals(Result, "1_"), Expr.Constant(true, typeof(bool))),
                        Expr.Block(EU.ConcatEquals(Result, "2_"), Expr.Constant(true, typeof(bool))),
                        mi
                    );
                });

            var tree = Expr.Empty();

            return tree;
        }



        #region Regression for bug 446959
        //Regression for bug 446959

        public static String AndAlso21Res;
        public struct Slot {

            public int Value;

            public Slot(int val) {
                AndAlso21Res += val.ToString();

                this.Value = val;
            }

            public static Slot operator &(Slot a, Slot b) {
                return new Slot(a.Value & b.Value);
            }

            public static bool operator true(Slot a) {
                return a.Value != 0;
            }

            public static bool operator false(Slot a) {
                return a.Value == 0;
            }

            public override string ToString() {
                return Value.ToString();
            }
        }

        public static Expression CreateNotLiftedExpressionTree() {
            var l = Expression.Parameter(typeof(int), "l");
            var r = Expression.Parameter(typeof(int), "r");

            return Expression.Lambda<Func<int, int, Slot>>(
            Expression.AndAlso(
            Expression.New(typeof(Slot).GetConstructor(new[] { typeof(int) }), l),
            Expression.New(typeof(Slot).GetConstructor(new[] { typeof(int) }), r)),
            l, r);
        }

        public static Expression CreateLiftedExpressionTree() {
            var l = Expression.Parameter(typeof(int), "l");
            var r = Expression.Parameter(typeof(int), "r");

            return Expression.Lambda<Func<int, int, Slot?>>(
            Expression.AndAlso(
            Expression.Convert(
            Expression.New(typeof(Slot).GetConstructor(new[] { typeof(int) }), l),
            typeof(Slot?)),
            Expression.Convert(
            Expression.New(typeof(Slot).GetConstructor(new[] { typeof(int) }), r),
            typeof(Slot?))),
            l, r);
        }



        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 22", new string[] { "positive", "andalso", "operators", "Pri1" })]
        public static Expr AndAlso22(EU.IValidator V) {

            AndAlso21Res = "";

            List<Expression> Expressions = new List<Expression>();

            var notlifted = CreateNotLiftedExpressionTree();

            Expressions.Add(Expr.Invoke(notlifted, Expr.Constant(0), Expr.Constant(1)));

            var lifted = CreateLiftedExpressionTree();


            Expressions.Add(Expr.Invoke(lifted, Expr.Constant(0), Expr.Constant(1)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("00"), Expr.Field(null, typeof(AndAlso).GetField("AndAlso21Res"))));


            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        #endregion  //Regression for bug 446959

        //regression for bug 634624
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso 23", new string[] { "negative", "andalso", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr AndAlso23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var MyAnd =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.AndAlso(
                        Expression.Constant(false),
                        Expression.Constant(true),
                        ((Func<bool, bool, bool>)AndAlsoTest.AndAlso).Method
                    );
                });

            return Expr.Empty();
        }

        public class AndAlsoTest {
            public static bool AndAlso(bool arg1, bool arg2) {
                return arg1 && arg2;
            }
        }
    }
}


