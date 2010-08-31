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
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class RightShift {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

        #region Helper functions and inner classes


        // Use to generate generic helper RightShift statements that take custome methods
        private static Expr TestRightShiftExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, string testMsg) {
            List<Expression> Expressions = new List<Expression>();


            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.RightShift(Expr.Constant(LeftVal, typeof(T)),
                                                           Expr.Constant(RightVal, typeof(T))),
                                           testMsg));

            return EU.BlockVoid(Expressions);
        }


        #endregion


        // Test     : RightShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_uint", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_unint(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestRightShiftExprBlockForType<uint>(1, 1, 0, "Tests for uint");
            });

            return tree;
        }

        // Test     : RightShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_Decimal", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Decimal(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestRightShiftExprBlockForType<Decimal>(1, 1, 0, "Tests for Decimal");
            });

            return tree;
        }

        // Test     : RightShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_ulong", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_ulong(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestRightShiftExprBlockForType<ulong>(1, 1, 0, "Tests for ulong");
            });

            return tree;
        }

        // Test     : RightShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_short", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_short(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestRightShiftExprBlockForType<short>(1, 1, 0, "Test for short type");
            });

            return tree;
        }
        // Test     : RightShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_ushort", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_ushort(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestRightShiftExprBlockForType<ushort>(1, 1, 0, "Tests for ushort");
            });

            return tree;
        }

        // Test     : RightShift of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_long", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_long(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestRightShiftExprBlockForType<long>(1, 1, 0, "Testing RightShift for long types");
            });

            return tree;
        }
        // Test     : RightShift of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_double", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_double(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestRightShiftExprBlockForType<double>(1, 1, 0, "Testing RightShift for double types");
            });

            return tree;
        }

        // Test     : RightShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_byte", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_byte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestRightShiftExprBlockForType<byte>(1, 1, 0, "Testing RightShift for byte types");
            });

            return tree;
        }

        // Test     : RightShift of byte, sbyte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_sbyte", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_sbyte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestRightShiftExprBlockForType<sbyte>(1, 1, 0, "Testing RightShift for sbyte types");
            });

            return tree;
        }

        // Test     : RightShift of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_Single", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Single(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestRightShiftExprBlockForType<Single>(1, 1, 0, "Testing RightShift for Single types");
            });

            return tree;

        }

        // RightShift of Boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift Boolean Constant Test", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() => { Expr.RightShift(Expr.Constant(1, typeof(Boolean)), Expr.Constant(1, typeof(Boolean))); }));

            return Expr.Empty();
        }

        // RightShift of String constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift String Constant Test", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_String(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.RightShift(Expr.Constant("1", typeof(string)), Expr.Constant("1", typeof(string))); }));

            return Expr.Empty();
        }
        // RightShifting with non integer right side.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift 20", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.RightShift(Expr.Constant(1), Expr.Constant('1', typeof(char)));
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

        // RightShift of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift Class Object Test", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Object(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.RightShift(Expr.Constant(new UserDefinedObject(1)), Expr.Constant(new UserDefinedObject(1))); }));

            return Expr.Empty();
        }

        // RightShift of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift 8", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr RightShift8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.RightShift(Expr.Constant(new TestStruct(1)), Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "RightShift 1"); }));

            return Expr.Empty();
        }


        // Test     : RightShifting across mixed types
        // Expected : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift Across Mixed Types", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MixedTypes(EU.IValidator V) {

            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() => { Expr.RightShift(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(byte))); }));

            return EU.BlockVoid(Expressions);
        }
        /// <summary>
        /// Test     : Pass null to method, normal arguments to left and right
        /// Expected : Same as RightShift(left, right)
        /// </summary>
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift Null MethodInfo", new string[] { "positive", "RightShift", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(0), Expr.RightShift(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), (MethodInfo)null));
            V.Validate(tree);
            return tree;
        }

        // Test     : Pass null to method, normal arguments to left and right - with annotations
        // Expected : Same as RightShift(left, right) 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift Null MethodInfo Annotations", new string[] { "positive", "RightShift", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo_Annotations(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(0), Expr.RightShift(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), (MethodInfo)null), "Test RightShift for Null Method Info Ann");
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift MethodInfo With No Args", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args(EU.IValidator V) {
            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;
            EU.Throws<System.ArgumentException>(() => { Expr.RightShift(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), TakesNoArgs); });
            return Expr.Empty();
        }

        // Pass a methodinfo that takes no arguments 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift MethodInfo With No Args Ann", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args_Ann(EU.IValidator V) {

            List<Expression> Expressions = new List<Expression>();
            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.RightShift(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), TakesNoArgs);
            }));

            return EU.BlockVoid(Expressions);

        }

        // Pass a method info that takes a paramarray
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShiftMethodInfo_That_Takes_Paramarray", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_That_Takes_Paramarray(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParmsParameter par = new ParmsParameter(MethodWithParamsArgs);
            MethodInfo fn = par.Method;
            // The MethodInfo override the function.
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(0, typeof(int)),
                    Expr.RightShift(Expr.Constant(2, typeof(int)),
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift With Values Widen To Args Method", new string[] { "negative", "RightShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Values_Widen_To_Args_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Create a fn that's parameters are wider then the calling args.
            MethodInfo fn = new Func<long, long, int>((long a, long b) => { return 1; }).Method;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.RightShift(Expr.Constant(2, typeof(int)), Expr.Constant(1, typeof(int)), fn);
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift With Non Nullable and Nullable Args Types To Method", new string[] { "positive", "RightShift", "operators", "Pri1" })]
        public static Expr Test_With_Non_Nullable_Nullable_Args_To_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int, int?, int>(NonNullableAndNullableMethod).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int)),
                                           Expr.RightShift(Expr.Constant(2, typeof(int)),
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "RightShift With Non Nullable and Nullable Args Types To Method"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        // Test     : With a null methodinfo, pass a value of non nullable type, the other of nullable type.
        // Expected : Exception.
        // Note     : Updated as positive test per update to bug 525563
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "RightShift Null MethodInfo Pass Non Nullable Type And Nullable Type", new string[] { "positive", "RightShift", "operations", "Pri1" })]
        public static Expr Test_With_Non_Nullable_And_Nullable_Args_To_Null_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expression Result = Expr.RightShift(Expr.Constant(2, typeof(int)), Expr.Constant(null, typeof(int?)), null);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(null), Result));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        // helper function for: "With a valid methodInfo that returns boolean, pass arguments of nullable type."
        public static Boolean RightShift_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate(int? a, int? b) {
            return true;
        }

        // With a valid methodInfo that returns boolean, pass arguments of nullable type.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean", new string[] { "positive", "RightShift", "operators", "Pri1" })]
        public static Expr Test_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int?, int?, Boolean>(RightShift_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant((Boolean)true, typeof(Boolean)),
                                           Expr.RightShift(Expr.Constant(null, typeof(int?)),
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "RightShift with a valid MethodInfo that returns boolean, passing arg of nullable type"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use to generate generic helper RightShift statements that take custome methods
        private static Expr TestRightShiftExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, MethodInfo fn, string testMsg) {
            List<Expression> Expressions = new List<Expression>();


            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.RightShift(Expr.Constant(LeftVal, typeof(T)),
                                                           Expr.Constant(RightVal, typeof(T)), fn),
                                           testMsg));

            return EU.BlockVoid(Expressions);
        }

        // Dynamically pick the a method with a type helper functions
        public static T TwoParamMethodRetFirstArg<T>(T a, T b) { return a; }
        public static MethodInfo GetExpressionMethodInfo<T>() {
            return new Func<T, T, T>(TwoParamMethodRetFirstArg<T>).Method;
        }

        // Test     : RightShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        // Note     : Didn't need to do this one.
        //            This covers Pri1 test "With a valid method info, LeftShift of normal values" Scenario
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_Across_Multiple_Types", new string[] { "positive", "RightShift", "operators", "Pri3" })]
        public static Expr Test_Across_All_Multiple_Types(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(TestRightShiftExprBlockForType<int>(1, 1, 1, GetExpressionMethodInfo<int>(), "RightShift with MethodInfo test for int"));
            Expressions.Add(TestRightShiftExprBlockForType<uint>(1, 1, 1, GetExpressionMethodInfo<uint>(), "RightShift with MethodInfo test for uint"));
            Expressions.Add(TestRightShiftExprBlockForType<long>(1, 1, 1, GetExpressionMethodInfo<long>(), "RightShift with MethodInfo test for long"));
            Expressions.Add(TestRightShiftExprBlockForType<ulong>(1, 1, 1, GetExpressionMethodInfo<ulong>(), "RightShift with MethodInfo test for ulong"));
            Expressions.Add(TestRightShiftExprBlockForType<double>(1, 1, 1, GetExpressionMethodInfo<double>(), "RightShift with MethodInfo test for double"));
            Expressions.Add(TestRightShiftExprBlockForType<float>(1, 1, 1, GetExpressionMethodInfo<float>(), "RightShift with MethodInfo test for float"));
            Expressions.Add(TestRightShiftExprBlockForType<short>(1, 1, 1, GetExpressionMethodInfo<short>(), "RightShift with MethodInfo test for short"));
            Expressions.Add(TestRightShiftExprBlockForType<ushort>(1, 1, 1, GetExpressionMethodInfo<ushort>(), "RightShift with MethodInfo test for ushort"));
            Expressions.Add(TestRightShiftExprBlockForType<byte>(1, 1, 1, GetExpressionMethodInfo<byte>(), "RightShift with MethodInfo test for byte"));
            Expressions.Add(TestRightShiftExprBlockForType<sbyte>(1, 1, 1, GetExpressionMethodInfo<sbyte>(), "RightShift with MethodInfo test for sbyte"));
            Expressions.Add(TestRightShiftExprBlockForType<Single>(1, 1, 1, GetExpressionMethodInfo<Single>(), "RightShift with MethodInfo test for Single"));
            Expressions.Add(TestRightShiftExprBlockForType<Decimal>(1, 1, 1, GetExpressionMethodInfo<Decimal>(), "RightShift with MethodInfo test for Decimal"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;

        }



        /// <summary>
        /// Used for test : Test_Overloaded_Operator_On_Left_Arg
        /// </summary>
        public class OverloadedOperator {
            public int Val { get; set; }
            public OverloadedOperator() { }
            public OverloadedOperator(int a) {
                Val = a;
            }
            public static bool operator ==(OverloadedOperator x, OverloadedOperator y) {
                return (x.Val == y.Val);
            }
            public static bool operator !=(OverloadedOperator x, OverloadedOperator y) {
                return (x.Val != y.Val);
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }
            public static int operator >>(OverloadedOperator a, int b) {

                return (b + a.Val);
            }
            public static OverloadedOperator RightShift(OverloadedOperator a, OverloadedOperator b) {
                return new OverloadedOperator(b.Val + a.Val);
            }
            //public static OverloadedOperator operator >>(OverloadedOperator a, OverloadedOperator b)
            //{
            //    return new OverloadedOperator(b.Val + a.Val);
            //}
        }

        // User defined overloaded operator on left argument, arguments are the proper types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_Overloaded_Operator_On_Left_Arg", new string[] { "positive", "RightShift", "operators", "Pri1" })]
        public static Expr Test_Overloaded_Operator_On_Left_Arg(EU.IValidator V) {
            OverloadedOperator LeftArg = new OverloadedOperator(1);

            var tree = EU.BlockVoid(EU.GenAreEqual(Expr.Constant(2, typeof(int)), Expr.RightShift(Expr.Constant(LeftArg, typeof(OverloadedOperator)), Expr.Constant(1, typeof(int))), "Test RightShift override opp"));
            V.Validate(tree);
            return tree;
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public class MyDerivedType : OverloadedOperator {
            public MyDerivedType() { }
            public MyDerivedType(int a) : base(a) { }
            public int DerivedData { get; set; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift 14", new string[] { "positive", "RightShift", "operators", "Pri1" })]
        public static Expr RightShift14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            //MethodInfo mi = typeof(MyDerivedType).GetMethod(">>");
            //RightShift
            MethodInfo mi = typeof(OverloadedOperator).GetMethod("RightShift");

            MyDerivedType right = new MyDerivedType(1);
            MyDerivedType left = new MyDerivedType(1);
            MyDerivedType expected = new MyDerivedType(2);

            Expr Res =
                Expr.RightShift(
                    Expr.Constant(left, typeof(OverloadedOperator)),
                    Expr.Constant(right, typeof(OverloadedOperator)),
                    mi
                );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(expected, typeof(OverloadedOperator)),
                    Res

                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        // Test     :  Verify order of evaluation of expressions on RightShift
        // Expected :  Verify res = "_first arg_second arg", and intresult = 3
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift_Order_Of_Operations_Verification", new string[] { "positive", "RightShift", "operators", "Pri1" })]
        public static Expr Test_Order_Of_Operations_Verification(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var res = Expr.Variable(typeof(string), "x");
            var intresult = Expr.Variable(typeof(int), "y");

            var LeftArg = Expr.Block(EU.ConcatEquals(res, "_first arg"), Expr.Constant(7));
            var RightArg = Expr.Block(EU.ConcatEquals(res, "_second arg"), Expr.Constant(1));
            var TmpExps = LeftArg.Expressions;


            Expressions.Add(Expr.Assign(intresult, Expr.RightShift(LeftArg, RightArg)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), intresult, "Order of operations validate intresult=3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("_first arg_second arg"), res, "Order of operations validate res='_first arg_second arg'"));

            var tree = EU.BlockVoid(new[] { res, intresult }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Test    : RightShift of values that overflow the type
        // Expected: Should just shifts the value out.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RightShift 18", new string[] { "positive", "RightShift", "operators", "Pri1" })]
        public static Expr RightShift18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Unsigned value overflow
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(uint.MaxValue / 2),
                    Expr.RightShift(
                        Expr.Constant(uint.MaxValue),
                        Expr.Constant(1)

                    ),
                    "RightShift Overflow the type 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}


