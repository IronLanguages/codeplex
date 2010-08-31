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

    public class NegateChecked {
        // NegateChecked of sbyte, short, int, long
        // NegateChecked of single, double, decimal constants
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 1", new string[] { "positive", "negatechecked", "operators", "Pri1" })]
        public static Expr NegateChecked1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            //Expressions.Add(EU.GenAreEqual(Expr.Constant((sbyte)(-1), typeof(sbyte)), Expr.NegateChecked(Expr.Constant((sbyte)1, typeof(sbyte))), "NegateChecked 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((short)(-1), typeof(short)), Expr.NegateChecked(Expr.Constant((short)1, typeof(short))), "NegateChecked 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int16)(-1), typeof(Int16)), Expr.NegateChecked(Expr.Constant((Int16)1, typeof(Int16))), "NegateChecked 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Int32)(-1), typeof(Int32)), Expr.NegateChecked(Expr.Constant((Int32)1, typeof(Int32))), "NegateChecked 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((long)(-1), typeof(long)), Expr.NegateChecked(Expr.Constant((long)1, typeof(long))), "NegateChecked 5"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant((Single)(-1), typeof(Single)), Expr.NegateChecked(Expr.Constant((Single)1, typeof(Single))), "NegateChecked 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Double)(-1), typeof(Double)), Expr.NegateChecked(Expr.Constant((Double)1, typeof(Double))), "NegateChecked 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant((Decimal)(-1), typeof(Decimal)), Expr.NegateChecked(Expr.Constant((Decimal)1, typeof(Decimal))), "NegateChecked 8"));

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "DoNotVisitTemp");
            var tree = Expr.Block(new[] { DoNotVisitTemp }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // NegateChecked of byte
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 2", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.Constant((byte)(1), typeof(byte)), Expr.NegateChecked(Expr.Constant((byte)1, typeof(byte))), "Negate 1"); }));
            return Expr.Empty();
        }

        // NegateChecked of sbyte
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 3", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.Constant((sbyte)(-1), typeof(sbyte)), Expr.NegateChecked(Expr.Constant((sbyte)1, typeof(sbyte))), "Negate 1"); }));
            return Expr.Empty();
        }

        // NegateChecked of ushort
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 4", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.NegateChecked(Expr.Constant((ushort)1, typeof(ushort))); }));
            return Expr.Empty();
        }

        // NegateChecked of UInt16
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 5", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.NegateChecked(Expr.Constant((UInt16)1, typeof(UInt16))); }));
            return Expr.Empty();
        }

        // NegateChecked of UInt32
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 6", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.NegateChecked(Expr.Constant((UInt32)1, typeof(UInt32))); }));
            return Expr.Empty();
        }

        // NegateChecked of ulong
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 7", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.NegateChecked(Expr.Constant((ulong)1, typeof(ulong))); }));
            return Expr.Empty();
        }

        // NegateChecked of boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 8", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.NegateChecked(Expr.Constant(true)); }));
            return Expr.Empty();
        }

        // NegateChecked of string constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 9", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.NegateChecked(Expr.Constant("Test")); }));
            return Expr.Empty();
        }

        // NegateChecked of class object, no user defined operator
        public class MyType {
            public int Data { get; set; }
            public MyType(int x) { Data = x; }
            public static MyType operator >>(MyType a, int x) {
                return new MyType(a.Data >> x);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 10", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.NegateChecked(Expr.Constant(new MyType(2))); }));
            return Expr.Empty();
        }

        // NegateChecked of structure object, no user defined operator
        public struct MyStruct {
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 11", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.NegateChecked(Expr.Constant(new MyStruct())); }));
            return Expr.Empty();
        }

        // Pass null to method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 12", new string[] { "positive", "negatechecked", "operators", "Pri1" })]
        public static Expr NegateChecked12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.NegateChecked(Expr.Constant(1), null)));

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "DoNotVisitTemp");
            var tree = Expr.Block(new[] { DoNotVisitTemp }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a methodinfo that takes no arguments
        public static int NegateCheckedNoArgs() {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 13", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr NegateChecked13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NegateChecked).GetMethod("NegateCheckedNoArgs");
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.NegateChecked(Expr.Constant(1), mi)));
            });

            return Expr.Empty();
        }

        // Pass a methodinfo that takes a paramarray
        public static int NegateCheckedParams(params int[] x) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 14", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NegateChecked).GetMethod("NegateCheckedParams");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), Expr.NegateChecked(Expr.Constant(1), mi)));
            });

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass a value that widen to the argument of the method
        public static int validNegateChecked(double x) {
            return (Int32)(-(x * x));
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 15", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr NegateChecked15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NegateChecked).GetMethod("validNegateChecked");
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(-4), Expr.NegateChecked(Expr.Constant((Int32)2, typeof(Int32)), mi)));
            });

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // With a valid method info, pass a value of a derived class of the methodinfo's argument
        public class MyDerivedType : MyType {
            public MyDerivedType(int x) : base(x) { }
        }
        public static int NegateCheckedBase(MyType x) {
            return -(x.Data * x.Data);
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 16", new string[] { "positive", "negatechecked", "operators", "Pri1" })]
        public static Expr NegateChecked16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NegateChecked).GetMethod("NegateCheckedBase");

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(-4),
                    Expr.NegateChecked(Expr.Constant(new MyDerivedType(2)), mi),
                    "NegateChecked 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // With a valid methodinfo that returns boolean, pass an argument of nullable type
        public static bool NegateCheckedNullableTest(int x) {
            return (x > 0) ? true : false;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 17", new string[] { "positive", "negatechecked", "operators", "Pri1" })]
        public static Expr NegateChecked17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(NegateChecked).GetMethod("NegateCheckedNullableTest");
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(true, typeof(Nullable<bool>)),
                    Expr.NegateChecked(
                        Expr.Constant((Nullable<int>)1, typeof(Nullable<int>)),
                        mi
                    )
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // User defined overloaded operator on expression argument
        public class A {
            public int Data { get; set; }
            public A(int x) { Data = x; }
            public static A operator -(A a) {
                return new A(-(a.Data * a.Data));
            }
            public static bool operator ==(A a, A b) {
                return a.Data == b.Data;
            }
            public static bool operator !=(A a, A b) {
                return a.Data != b.Data;
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 18", new string[] { "positive", "negatechecked", "operators", "Pri1" })]
        public static Expr NegateChecked18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(new A(-16)),
                    Expr.NegateChecked(Expr.Constant(new A(4))),
                    "NegateChecked 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Overflow type (by negative type.minValue)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NegateChecked 19", new string[] { "negative", "negatechecked", "operators", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr NegateChecked19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(Int32.MinValue),
                    Expr.NegateChecked(Expr.Constant(Int32.MinValue)),
                    "NegateChecked 1"
                )
            );

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "DoNotVisitTemp");
            var tree = Expr.Block(new[] { DoNotVisitTemp }, Expressions);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }

    }
}
