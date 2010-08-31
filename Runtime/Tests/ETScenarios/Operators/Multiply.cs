#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MsSc = System.Dynamic;

namespace ETScenarios.Operators {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Multiply {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

        #region Helper functions and inner classes


        // Use to generate generic helper Multiply statements that take custome methods
        private static Expr TestMultiplyExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, string testMsg) {
            List<Expression> Expressions = new List<Expression>();

            Expression expected = Expr.Constant(ExpectedVal, typeof(T));
            Expression Left = Expr.Constant(LeftVal, typeof(T));
            Expression Right = Expr.Constant(RightVal, typeof(T));
            Expression OpToTest = Expr.Multiply(Left, Right);

            Expression Ex = EU.GenAreEqual(expected, OpToTest, testMsg);
            Expressions.Add(Ex);

            //Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
            //                               Expr.Multiply(Expr.Constant(LeftVal, typeof(T)),
            //                                               Expr.Constant(RightVal, typeof(T))),
            //                               testMsg));

            return EU.BlockVoid(Expressions);
        }
        #endregion


        // Test     : Multiply of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_uint", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_unint(EU.IValidator V) {
            var tree = TestMultiplyExprBlockForType<uint>(1, 1, 1, "Tests for uint");
            V.Validate(tree);
            return tree;
        }

        // Test     : Multiply of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_Decimal", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_Decimal(EU.IValidator V) {
            var tree = TestMultiplyExprBlockForType<Decimal>(1, 1, 1, "Tests for Decimal");
            V.Validate(tree);
            return tree;
        }

        // Test     : Multiply of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_ulong", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_ulong(EU.IValidator V) {
            var tree = TestMultiplyExprBlockForType<ulong>(1, 1, 1, "Tests for ulong");
            V.Validate(tree);
            return tree;
        }

        // Test     : Multiply of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_short", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_short(EU.IValidator V) {
            var tree = TestMultiplyExprBlockForType<short>(1, 1, 1, "Test for short type");
            V.Validate(tree);
            return tree;
        }
        // Test     : Multiply of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_ushort", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_ushort(EU.IValidator V) {
            var tree = TestMultiplyExprBlockForType<ushort>(1, 1, 1, "Tests for ushort");
            V.Validate(tree);
            return tree;
        }

        // Test     : Multiply of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_long", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_long(EU.IValidator V) {
            var tree = TestMultiplyExprBlockForType<long>(1, 1, 1, "Testing Multiply for long types");
            V.Validate(tree);
            return tree;
        }
        // Test     : Multiply of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_double", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_double(EU.IValidator V) {
            var tree = TestMultiplyExprBlockForType<double>(1, 1, 1, "Testing Multiply for double types");
            V.Validate(tree);
            return tree;
        }

        // Test     : Multiply of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_byte", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_byte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestMultiplyExprBlockForType<byte>(1, 1, 1, "Testing Multiply for byte types");
            });

            return tree;
        }

        // Test     : Multiply of byte, sbyte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_sbyte", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_sbyte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestMultiplyExprBlockForType<sbyte>(1, 1, 1, "Testing Multiply for sbyte types");
            });

            return tree;
        }

        // Test     : Multiply of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_Single", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_Single(EU.IValidator V) {
            var tree = TestMultiplyExprBlockForType<Single>(1, 1, 1, "Testing Multiply for Single types");
            V.Validate(tree);
            return tree;

        }

        // Multiply of Boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply Boolean Constant Test", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Multiply(Expr.Constant(1, typeof(Boolean)), Expr.Constant(1, typeof(Boolean)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // Multiply of String constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply String Constant Test", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_String(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Multiply(Expr.Constant("1", typeof(string)), Expr.Constant("1", typeof(string)));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        /// <summary>
        /// Used for : Test_Object
        /// </summary>
        class UserDefinedObject {
            public int _value = 0;
            public UserDefinedObject() { }
            public UserDefinedObject(int n) {
                _value = n;
            }
        }

        // Multiply of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply Class Object Test", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Object(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Multiply(Expr.Constant(new UserDefinedObject(1)), Expr.Constant(new UserDefinedObject(1)));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Multiply of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply 8", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Multiply8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.Multiply(Expr.Constant(new TestStruct(1)), Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "Multiply 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Test     : Multiplying across mixed types
        // Expected : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply Across Mixed Types", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MixedTypes(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() => { Expr.Multiply(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(byte))); }));

            return Expr.Empty();
        }
        /// <summary>
        /// Test     : Pass null to method, normal arguments to left and right
        /// Expected : Same as Multiply(left, right)
        /// </summary>
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply Null MethodInfo", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(1), Expr.Multiply(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), (MethodInfo)null));
            V.Validate(tree);
            return tree;
        }

        // Test     : Pass null to method, normal arguments to left and right - with annotations
        // Expected : Same as Multiply(left, right) 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply Null MethodInfo Annotations", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo_Annotations(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(1), Expr.Multiply(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), (MethodInfo)null), "Test Multiply for Null Method Info Ann");
            V.Validate(tree);
            return tree;
        }

        /// <summary>
        /// Method with no Args
        /// for tests : Test_MethodInfo_With_No_Args & Test_MethodInfo_With_No_Args_Ann
        /// </summary>
        public static int GetOne() {
            return 1;
        }

        // Pass a methodinfo that takes no arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply MethodInfo With No Args", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args(EU.IValidator V) {
            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;
            EU.Throws<System.ArgumentException>(() => { Expr.Multiply(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), TakesNoArgs); });
            return Expr.Empty();
        }

        // Pass a methodinfo that takes no arguments 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply MethodInfo With No Args Ann", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args_Ann(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Multiply(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), TakesNoArgs);
            }));

            return EU.BlockVoid(Expressions);

        }

        // Pass a method info that takes a paramarray
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyMethodInfo_That_Takes_Paramarray", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_That_Takes_Paramarray(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParmsParameter par = new ParmsParameter(MethodWithParamsArgs);
            MethodInfo fn = par.Method;
            // The MethodInfo override the function.
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(0, typeof(int)),
                    Expr.Multiply(Expr.Constant(2, typeof(int)),
                    Expr.Constant(1, typeof(int)), fn),
                    "Pass a method info that takes a paramarray");
            }));

            return EU.BlockVoid(Expressions);
        }


        /// <summary>
        /// Used for : Test_MethodInfo_That_Takes_Paramarray
        /// </summary>
        delegate int ParmsParameter(params int[] args);
        public static int MethodWithParamsArgs(params int[] args) {
            // accumulator
            int acc = 0;
            foreach (int i in args) {
                acc += i;
            }
            return acc;
        }

        // With a valid method info, pass values that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply With Values Widen To Args Method", new string[] { "negative", "Multiply", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Values_Widen_To_Args_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Create a fn that's parameters are wider then the calling args.
            MethodInfo fn = new Func<long, long, int>((long a, long b) => { return 1; }).Method;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Multiply(Expr.Constant(2, typeof(int)), Expr.Constant(1, typeof(int)), fn);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }


        /// <summary>
        /// Also use for other tests. Could reuse a lot of these.
        /// </summary>
        public static int NonNullableAndNullableMethod(int a, int? b) {
            return 1;
        }


        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply With Non Nullable and Nullable Args Types To Method", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_With_Non_Nullable_Nullable_Args_To_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int, int?, int>(NonNullableAndNullableMethod).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int)),
                                           Expr.Multiply(Expr.Constant(2, typeof(int)),
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "Multiply With Non Nullable and Nullable Args Types To Method"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        // Test     : With a null methodinfo, pass a value of non nullable type, the other of nullable type.
        // Expected : Exception.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply Null MethodInfo Pass Non Nullable Type And Nullable Type", new string[] { "negative", "Multiply", "operations", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Non_Nullable_And_Nullable_Args_To_Null_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Multiply(Expr.Constant(2, typeof(int)), Expr.Constant(null, typeof(int?)), null);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }


        // helper function for: "With a valid methodInfo that returns boolean, pass arguments of nullable type."
        public static Boolean Multiply_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate(int? a, int? b) {
            return true;
        }

        // With a valid methodInfo that returns boolean, pass arguments of nullable type.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int?, int?, Boolean>(Multiply_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant((Boolean)true, typeof(Boolean)),
                                           Expr.Multiply(Expr.Constant(null, typeof(int?)),
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "Multiply with a valid MethodInfo that returns boolean, passing arg of nullable type"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use to generate generic helper Multiply statements that take custome methods
        private static Expr TestMultiplyExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, MethodInfo fn, string testMsg) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.Multiply(Expr.Constant(LeftVal, typeof(T)),
                                                           Expr.Constant(RightVal, typeof(T)), fn),
                                           testMsg));

            return EU.BlockVoid(Expressions);
        }

        // Dynamically pick the a method with a type helper functions
        public static T TwoParamMethodRetFirstArg<T>(T a, T b) { return a; }
        public static MethodInfo GetExpressionMethodInfo<T>() {
            return new Func<T, T, T>(TwoParamMethodRetFirstArg<T>).Method;
        }

        // Test     : Multiply of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        // Note     : Didn't need to do this one.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_Across_Multiple_Types", new string[] { "positive", "Multiply", "operators", "Pri3" })]
        public static Expr Test_Across_All_Multiple_Types(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(TestMultiplyExprBlockForType<int>(1, 1, 1, GetExpressionMethodInfo<int>(), "Multiply with MethodInfo test for int"));
            Expressions.Add(TestMultiplyExprBlockForType<uint>(1, 1, 1, GetExpressionMethodInfo<uint>(), "Multiply with MethodInfo test for uint"));
            Expressions.Add(TestMultiplyExprBlockForType<long>(1, 1, 1, GetExpressionMethodInfo<long>(), "Multiply with MethodInfo test for long"));
            Expressions.Add(TestMultiplyExprBlockForType<ulong>(1, 1, 1, GetExpressionMethodInfo<ulong>(), "Multiply with MethodInfo test for ulong"));
            Expressions.Add(TestMultiplyExprBlockForType<double>(1, 1, 1, GetExpressionMethodInfo<double>(), "Multiply with MethodInfo test for double"));
            Expressions.Add(TestMultiplyExprBlockForType<float>(1, 1, 1, GetExpressionMethodInfo<float>(), "Multiply with MethodInfo test for float"));
            Expressions.Add(TestMultiplyExprBlockForType<short>(1, 1, 1, GetExpressionMethodInfo<short>(), "Multiply with MethodInfo test for short"));
            Expressions.Add(TestMultiplyExprBlockForType<ushort>(1, 1, 1, GetExpressionMethodInfo<ushort>(), "Multiply with MethodInfo test for ushort"));
            Expressions.Add(TestMultiplyExprBlockForType<byte>(1, 1, 1, GetExpressionMethodInfo<byte>(), "Multiply with MethodInfo test for byte"));
            Expressions.Add(TestMultiplyExprBlockForType<sbyte>(1, 1, 1, GetExpressionMethodInfo<sbyte>(), "Multiply with MethodInfo test for sbyte"));
            Expressions.Add(TestMultiplyExprBlockForType<Single>(1, 1, 1, GetExpressionMethodInfo<Single>(), "Multiply with MethodInfo test for Single"));
            Expressions.Add(TestMultiplyExprBlockForType<Decimal>(1, 1, 1, GetExpressionMethodInfo<Decimal>(), "Multiply with MethodInfo test for Decimal"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }



        /// <summary>
        /// Used for test : Test_Overloaded_Operator_On_Left_Arg
        /// </summary>
        public class OverloadedOperator {
            public int Val { get; set; }
            public OverloadedOperator(int a) {
                Val = a;
            }
            public static int operator *(OverloadedOperator a, int b) {

                return (b + a.Val);
            }
            public static OverloadedOperator Multiply(OverloadedOperator a, OverloadedOperator b) {
                return new OverloadedOperator(b.Val + a.Val);
            }
            public static bool operator ==(OverloadedOperator x, OverloadedOperator y) {
                return (x.Val == y.Val);
            }
            public static bool operator !=(OverloadedOperator x, OverloadedOperator y) {
                return ((x.Val != y.Val));
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }

        }

        // User defined overloaded operator on left argument, arguments are the proper types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_Overloaded_Operator_On_Left_Arg", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_Overloaded_Operator_On_Left_Arg(EU.IValidator V) {
            OverloadedOperator LeftArg = new OverloadedOperator(1);

            var tree = EU.BlockVoid(EU.GenAreEqual(Expr.Constant(2, typeof(int)), Expr.Multiply(Expr.Constant(LeftArg, typeof(OverloadedOperator)), Expr.Constant(1, typeof(int))), "Test Multiply override opp"));
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo's arguments
        public class MyDerivedType : OverloadedOperator {
            public MyDerivedType(int a) : base(a) { }
            public int DerivedData { get; set; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply 14", new string[] { "positive", "multiply", "operators", "Pri1" })]
        public static Expr Multiply14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OverloadedOperator).GetMethod("Multiply");

            MyDerivedType right = new MyDerivedType(1);
            MyDerivedType left = new MyDerivedType(1);
            MyDerivedType expected = new MyDerivedType(2);

            Expr Res =
                Expr.Multiply(
                    Expr.Constant(left, typeof(OverloadedOperator)),
                    Expr.Constant(right, typeof(OverloadedOperator)),
                    mi
                );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(expected, typeof(MyDerivedType)),
                    Res
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Test     :  Verify order of evaluation of expressions on Multiply
        // Expected :  Verify res = "_first arg_second arg", and intresult = 3
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply_Order_Of_Operations_Verification", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Test_Order_Of_Operations_Verification(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var res = Expr.Variable(typeof(string), "x");
            var intresult = Expr.Variable(typeof(int), "y");

            var LeftArg = Expr.Block(EU.ConcatEquals(res, "_first arg"), Expr.Constant(7));
            var RightArg = Expr.Block(EU.ConcatEquals(res, "_second arg"), Expr.Constant(1));
            var TmpExps = LeftArg.Expressions;


            Expressions.Add(Expr.Assign(intresult, Expr.Multiply(LeftArg, RightArg)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(7), intresult, "Order of operations validate intresult=3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("_first arg_second arg"), res, "Order of operations validate res='_first arg_second arg'"));

            var tree = EU.BlockVoid(new[] { res, intresult }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // Multiply of values that overflow the type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply 18", new string[] { "positive", "Multiply", "operators", "Pri1" })]
        public static Expr Multiply18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(-2),
                    Expr.Multiply(
                        Expr.Constant(2),
                        Expr.Constant(Int32.MaxValue)
                    ),
                    "Multiply 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(0),
                    Expr.Multiply(
                        Expr.Constant(-2),
                        Expr.Constant(Int32.MinValue)
                    ),

                    "Multiply 2"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant((double)(Double.PositiveInfinity)),
                    Expr.Multiply(
                        Expr.Constant((double)(Double.MaxValue)),
                        Expr.Constant((double)(Double.MaxValue))
                    ),

                    "Multiply 3"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant((double)(Double.PositiveInfinity)),
                    Expr.Multiply(
                        Expr.Constant((double)(Double.MinValue)),
                        Expr.Constant((double)(Double.MinValue))
                    ),

                    "Multiply 4"
                )
            );

            // comparing Infinity to Infinity can fail with rewriters
            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "dont_visit_node");
            var tree = Expr.Block(new[] { DoNotVisitTemp }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
