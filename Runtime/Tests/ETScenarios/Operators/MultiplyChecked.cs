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

namespace ETScenarios.Operators {
    using Expr = Expression;
    using MsSc = System.Dynamic;
    using EU = ETUtils.ExpressionUtils;

    public class MultiplyChecked {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

        #region Helper functions and inner classes


        // Use to generate generic helper MultiplyChecked statements that take custome methods
        private static Expr TestMultiplyCheckedExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, string testMsg) {
            List<Expression> Expressions = new List<Expression>();

            Expression expected = Expr.Constant(ExpectedVal, typeof(T));
            Expression Left = Expr.Constant(LeftVal, typeof(T));
            Expression Right = Expr.Constant(RightVal, typeof(T));
            Expression OpToTest = Expr.MultiplyChecked(Left, Right);

            Expression Ex = EU.GenAreEqual(expected, OpToTest, testMsg);
            Expressions.Add(Ex);

            //Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
            //                               Expr.MultiplyChecked(Expr.Constant(LeftVal, typeof(T)),
            //                                               Expr.Constant(RightVal, typeof(T))),
            //                               testMsg));

            return EU.BlockVoid(Expressions);
        }


        #endregion


        // Test     : MultiplyChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_uint", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_unint(EU.IValidator V) {
            var tree = TestMultiplyCheckedExprBlockForType<uint>(1, 1, 1, "Tests for uint");
            V.Validate(tree);
            return tree;
        }

        // Test     : MultiplyChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_Decimal", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_Decimal(EU.IValidator V) {
            var tree = TestMultiplyCheckedExprBlockForType<Decimal>(1, 1, 1, "Tests for Decimal");
            V.Validate(tree);
            return tree;
        }

        // Test     : MultiplyChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_ulong", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_ulong(EU.IValidator V) {
            var tree = TestMultiplyCheckedExprBlockForType<ulong>(1, 1, 1, "Tests for ulong");
            V.Validate(tree);
            return tree;
        }

        // Test     : MultiplyChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_short", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_short(EU.IValidator V) {
            var tree = TestMultiplyCheckedExprBlockForType<short>(1, 1, 1, "Test for short type");
            V.Validate(tree);
            return tree;
        }
        // Test     : MultiplyChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_ushort", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_ushort(EU.IValidator V) {
            var tree = TestMultiplyCheckedExprBlockForType<ushort>(1, 1, 1, "Tests for ushort");
            V.Validate(tree);
            return tree;
        }

        // Test     : MultiplyChecked of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_long", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_long(EU.IValidator V) {
            var tree = TestMultiplyCheckedExprBlockForType<long>(1, 1, 1, "Testing MultiplyChecked for long types");
            V.Validate(tree);
            return tree;
        }
        // Test     : MultiplyChecked of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_double", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_double(EU.IValidator V) {
            var tree = TestMultiplyCheckedExprBlockForType<double>(1, 1, 1, "Testing MultiplyChecked for double types");
            V.Validate(tree);
            return tree;
        }

        // Test     : MultiplyChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_byte", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_byte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestMultiplyCheckedExprBlockForType<byte>(1, 1, 1, "Testing MultiplyChecked for byte types");
            });

            return tree;
        }

        // Test     : MultiplyChecked of byte, sbyte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_sbyte", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_sbyte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestMultiplyCheckedExprBlockForType<sbyte>(1, 1, 1, "Testing MultiplyChecked for sbyte types");
            });

            return tree;
        }

        // Test     : MultiplyChecked of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_Single", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_Single(EU.IValidator V) {
            var tree = TestMultiplyCheckedExprBlockForType<Single>(1, 1, 1, "Testing MultiplyChecked for Single types");
            V.Validate(tree);
            return tree;

        }

        // MultiplyChecked of Boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked Boolean Constant Test", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.MultiplyChecked(Expr.Constant(1, typeof(Boolean)), Expr.Constant(1, typeof(Boolean)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // MultiplyChecked of String constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked String Constant Test", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_String(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.MultiplyChecked(Expr.Constant("1", typeof(string)), Expr.Constant("1", typeof(string)));
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
        // MultiplyChecked of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked Class Object Test", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Object(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.MultiplyChecked(Expr.Constant(new UserDefinedObject(1)), Expr.Constant(new UserDefinedObject(1)));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }


        // MultiplyChecked of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked 8", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr MultiplyChecked8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.MultiplyChecked(Expr.Constant(new TestStruct(1)), Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "MultiplyChecked 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }


        // Test     : MultiplyCheckeding across mixed types
        // Expected : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked Across Mixed Types", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MixedTypes(EU.IValidator V) {

            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() => { Expr.MultiplyChecked(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(byte))); }));

            return Expr.Empty();
        }
        /// <summary>
        /// Test     : Pass null to method, normal arguments to left and right
        /// Expected : Same as MultiplyChecked(left, right)
        /// </summary>
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked Null MethodInfo", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(1), Expr.MultiplyChecked(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), (MethodInfo)null));
            V.Validate(tree);
            return tree;
        }

        // Test     : Pass null to method, normal arguments to left and right - with annotations
        // Expected : Same as MultiplyChecked(left, right) 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked Null MethodInfo Annotations", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo_Annotations(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(1), Expr.MultiplyChecked(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), (MethodInfo)null), "Test MultiplyChecked for Null Method Info Ann");
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked MethodInfo With No Args", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args(EU.IValidator V) {
            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;
            EU.Throws<System.ArgumentException>(() => { Expr.MultiplyChecked(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), TakesNoArgs); });
            return Expr.Empty();
        }

        // Pass a methodinfo that takes no arguments 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked MethodInfo With No Args Ann", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args_Ann(EU.IValidator V) {

            List<Expression> Expressions = new List<Expression>();
            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.MultiplyChecked(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), TakesNoArgs);
            }));

            return EU.BlockVoid(Expressions);
        }

        // Pass a method info that takes a paramarray
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyCheckedMethodInfo_That_Takes_Paramarray", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_That_Takes_Paramarray(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParmsParameter par = new ParmsParameter(MethodWithParamsArgs);
            MethodInfo fn = par.Method;
            // The MethodInfo override the function.
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(0, typeof(int)),
                    Expr.MultiplyChecked(Expr.Constant(2, typeof(int)),
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked With Values Widen To Args Method", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Values_Widen_To_Args_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Create a fn that's parameters are wider then the calling args.
            MethodInfo fn = new Func<long, long, int>((long a, long b) => { return 1; }).Method;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.MultiplyChecked(Expr.Constant(2, typeof(int)), Expr.Constant(1, typeof(int)), fn); }));

            return Expr.Empty();
        }


        /// <summary>
        /// Also use for other tests. Could reuse a lot of these.
        /// </summary>
        public static int NonNullableAndNullableMethod(int a, int? b) {
            return 1;
        }

        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked With Non Nullable and Nullable Args Types To Method", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_With_Non_Nullable_Nullable_Args_To_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int, int?, int>(NonNullableAndNullableMethod).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int)),
                                           Expr.MultiplyChecked(Expr.Constant(2, typeof(int)),
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "MultiplyChecked With Non Nullable and Nullable Args Types To Method"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        // Test     : With a null methodinfo, pass a value of non nullable type, the other of nullable type.
        // Expected : Exception.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked Null MethodInfo Pass Non Nullable Type And Nullable Type", new string[] { "negative", "MultiplyChecked", "operations", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Non_Nullable_And_Nullable_Args_To_Null_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.MultiplyChecked(Expr.Constant(2, typeof(int)), Expr.Constant(null, typeof(int?)), null);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }


        // helper function for: "With a valid methodInfo that returns boolean, pass arguments of nullable type."
        public static Boolean MultiplyChecked_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate(int? a, int? b) {
            return true;
        }

        // With a valid methodInfo that returns boolean, pass arguments of nullable type.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int?, int?, Boolean>(MultiplyChecked_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant((Boolean)true, typeof(Boolean)),
                                           Expr.MultiplyChecked(Expr.Constant(null, typeof(int?)),
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "MultiplyChecked with a valid MethodInfo that returns boolean, passing arg of nullable type"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use to generate generic helper MultiplyChecked statements that take custome methods
        private static Expr TestMultiplyCheckedExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, MethodInfo fn, string testMsg) {
            List<Expression> Expressions = new List<Expression>();


            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.MultiplyChecked(Expr.Constant(LeftVal, typeof(T)),
                                                           Expr.Constant(RightVal, typeof(T)), fn),
                                           testMsg));

            return EU.BlockVoid(Expressions);
        }

        // Dynamically pick the a method with a type helper functions
        public static T TwoParamMethodRetFirstArg<T>(T a, T b) { return a; }
        public static MethodInfo GetExpressionMethodInfo<T>() {
            return new Func<T, T, T>(TwoParamMethodRetFirstArg<T>).Method;
        }

        // Test     : MultiplyChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        // Note     : Didn't need to do this one.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_Across_Multiple_Types", new string[] { "positive", "MultiplyChecked", "operators", "Pri3" })]
        public static Expr Test_Across_All_Multiple_Types(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(TestMultiplyCheckedExprBlockForType<int>(1, 1, 1, GetExpressionMethodInfo<int>(), "MultiplyChecked with MethodInfo test for int"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<uint>(1, 1, 1, GetExpressionMethodInfo<uint>(), "MultiplyChecked with MethodInfo test for uint"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<long>(1, 1, 1, GetExpressionMethodInfo<long>(), "MultiplyChecked with MethodInfo test for long"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<ulong>(1, 1, 1, GetExpressionMethodInfo<ulong>(), "MultiplyChecked with MethodInfo test for ulong"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<double>(1, 1, 1, GetExpressionMethodInfo<double>(), "MultiplyChecked with MethodInfo test for double"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<float>(1, 1, 1, GetExpressionMethodInfo<float>(), "MultiplyChecked with MethodInfo test for float"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<short>(1, 1, 1, GetExpressionMethodInfo<short>(), "MultiplyChecked with MethodInfo test for short"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<ushort>(1, 1, 1, GetExpressionMethodInfo<ushort>(), "MultiplyChecked with MethodInfo test for ushort"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<byte>(1, 1, 1, GetExpressionMethodInfo<byte>(), "MultiplyChecked with MethodInfo test for byte"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<sbyte>(1, 1, 1, GetExpressionMethodInfo<sbyte>(), "MultiplyChecked with MethodInfo test for sbyte"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<Single>(1, 1, 1, GetExpressionMethodInfo<Single>(), "MultiplyChecked with MethodInfo test for Single"));
            Expressions.Add(TestMultiplyCheckedExprBlockForType<Decimal>(1, 1, 1, GetExpressionMethodInfo<Decimal>(), "MultiplyChecked with MethodInfo test for Decimal"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;

        }


        /// <summary>
        /// Used for test : Test_Overloaded_Operator
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_Overloaded_Operator_On_Left_Arg", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_Overloaded_Operator_On_Left_Arg(EU.IValidator V) {
            OverloadedOperator LeftArg = new OverloadedOperator(1);

            var tree = EU.BlockVoid(EU.GenAreEqual(Expr.Constant(2, typeof(int)), Expr.MultiplyChecked(Expr.Constant(LeftArg, typeof(OverloadedOperator)), Expr.Constant(1, typeof(int))), "Test MultiplyChecked override opp"));
            V.Validate(tree);
            return tree;
        }



        // With a valid method info, pass two values of a derived class of the methodinfo's arguments
        public class MyDerivedType : OverloadedOperator {
            public MyDerivedType(int a) : base(a) { }
            public int DerivedData { get; set; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked 14", new string[] { "positive", "multiplychecked", "operators", "Pri1" })]
        public static Expr MultiplyChecked14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OverloadedOperator).GetMethod("Multiply");

            MyDerivedType right = new MyDerivedType(1);
            MyDerivedType left = new MyDerivedType(1);
            MyDerivedType expected = new MyDerivedType(2);

            Expr Res =
                Expr.MultiplyChecked(
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


        // Test     :  Verify order of evaluation of expressions on MultiplyChecked
        // Expected :  Verify res = "_first arg_second arg", and intresult = 3
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked_Order_Of_Operations_Verification", new string[] { "positive", "MultiplyChecked", "operators", "Pri1" })]
        public static Expr Test_Order_Of_Operations_Verification(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var res = Expr.Variable(typeof(string), "x");
            var intresult = Expr.Variable(typeof(int), "y");

            var LeftArg = Expr.Block(EU.ConcatEquals(res, "_first arg"), Expr.Constant(7));
            var RightArg = Expr.Block(EU.ConcatEquals(res, "_second arg"), Expr.Constant(1));
            var TmpExps = LeftArg.Expressions;


            Expressions.Add(Expr.Assign(intresult, Expr.MultiplyChecked(LeftArg, RightArg)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(7), intresult, "Order of operations validate intresult=3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("_first arg_second arg"), res, "Order of operations validate res='_first arg_second arg'"));

            var tree = EU.BlockVoid(new[] { res, intresult }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Test    : MultiplyChecked of values that overflow the type
        //Expected: Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked 18", new string[] { "negative", "MultiplyChecked", "operators", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr MultiplyChecked18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(Int32.MinValue + 1),
                    Expr.MultiplyChecked(
                        Expr.Constant(Int32.MaxValue),
                        Expr.Constant(-2)

                    ),
                    "MultiplyChecked Overflow the type 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(Int32.MaxValue),
                    Expr.MultiplyChecked(
                        Expr.Constant(-1),
                        Expr.Constant(Int32.MinValue)
                    ),
                    "MultiplyChecked Overflow the type 2"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant((double)(Double.PositiveInfinity)),
                    Expr.MultiplyChecked(
                        Expr.Constant((double)(Double.MaxValue)),
                        Expr.Constant((double)(-Double.MaxValue))
                    ),
                    "MultiplyChecked Overflow the type 3"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant((double)(Double.NegativeInfinity)),
                    Expr.MultiplyChecked(
                        Expr.Constant((double)(Double.MinValue)),
                        Expr.Constant((double)(-Double.MinValue))
                    ),
                    "MultiplyChecked Overflow the type 4"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }
    }
}
