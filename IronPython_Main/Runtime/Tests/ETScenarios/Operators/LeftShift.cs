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

    public class LeftShift {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

        #region Helper functions and inner classes

        // Use to generate generic helper LeftShift statements that take custome methods
        private static Expr TestLeftShiftExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, string testMsg) {
            List<Expression> Expressions = new List<Expression>();

            // Console.WriteLine("\n###__LeftShift__### Inside TestLeftShiftForType of Type = {0}", typeof(T));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.LeftShift(Expr.Constant(LeftVal, typeof(T)),
                                                           Expr.Constant(RightVal, typeof(T))),
                                           testMsg));

            // Console.WriteLine("\n###__LeftShift__### Inside TestLeftShiftForType of Type = {0}", typeof(T));
            return EU.BlockVoid(Expressions);
        }
        #endregion

        // Test     : LeftShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_uint", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_unint(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftExprBlockForType<uint>(1, 1, 0, "Tests for uint");
            });

            return tree;
        }

        // Test     : LeftShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_Decimal", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Decimal(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftExprBlockForType<Decimal>(1, 1, 0, "Tests for Decimal");
            });

            return tree;
        }

        // Test     : LeftShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_ulong", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_ulong(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftExprBlockForType<ulong>(1, 1, 0, "Tests for ulong");
            });

            return tree;
        }

        // Test     : LeftShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_short", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_short(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftExprBlockForType<short>(1, 1, 0, "Test for short type");
            });

            return tree;
        }
        // Test     : LeftShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_ushort", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_ushort(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftExprBlockForType<ushort>(1, 1, 0, "Tests for ushort");
            });

            return tree;
        }

        // Test     : LeftShift of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_long", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_long(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftExprBlockForType<long>(1, 1, 0, "Testing LeftShift for long types");
            });

            return tree;
        }
        // Test     : LeftShift of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_double", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_double(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftExprBlockForType<double>(1, 1, 0, "Testing LeftShift for double types");
            });

            return tree;
        }

        // Test     : LeftShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_byte", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_byte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftExprBlockForType<byte>(1, 1, 0, "Testing LeftShift for byte types");
            });

            return tree;
        }

        // Test     : LeftShift of byte, sbyte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_sbyte", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_sbyte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftExprBlockForType<sbyte>(1, 1, 0, "Testing LeftShift for sbyte types");
            });

            return tree;
        }

        // LeftShifting with non integer right side.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift 20", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LeftShift(Expr.Constant(1), Expr.Constant('1', typeof(char)));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // Test     : LeftShift of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_Single", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Single(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftExprBlockForType<Single>(1, 1, 0, "Testing LeftShift for Single types");
            });

            return tree;

        }

        // LeftShift of Boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift Boolean Constant Test", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.LeftShift(Expr.Constant(1, typeof(Boolean)), Expr.Constant(1, typeof(Boolean)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // LeftShift of String constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift String Constant Test", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_String(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LeftShift(Expr.Constant("1", typeof(string)), Expr.Constant("1", typeof(string)));
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
        // LeftShift of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift Class Object Test", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Object(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LeftShift(Expr.Constant(new UserDefinedObject(1)), Expr.Constant(new UserDefinedObject(1)));
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }

        // LeftShift of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift 8", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LeftShift8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.LeftShift(Expr.Constant(new TestStruct(1)), Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "LeftShift 1");
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }



        // Test     : LeftShifting across mixed types
        // Expected : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift Across Mixed Types", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MixedTypes(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() => { Expr.LeftShift(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(byte))); }));

            return Expr.Empty();
        }
        /// <summary>
        /// Test     : Pass null to method, normal arguments to left and right
        /// Expected : Same as LeftShift(left, right)
        /// </summary>
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift Null MethodInfo", new string[] { "positive", "LeftShift", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(2), Expr.LeftShift(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), (MethodInfo)null));
            V.Validate(tree);
            return tree;
        }

        // Test     : Pass null to method, normal arguments to left and right - with annotations

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift Null MethodInfo Annotations", new string[] { "positive", "LeftShift", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo_Annotations(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(2), Expr.LeftShift(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), (MethodInfo)null), "Test LeftShift for Null Method Info Ann");
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift MethodInfo With No Args Ann", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args_Ann(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.LeftShift(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), TakesNoArgs);
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

        // Pass a method info that takes a paramarray
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftMethodInfo_That_Takes_Paramarray", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_That_Takes_Paramarray(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParmsParameter par = new ParmsParameter(MethodWithParamsArgs);
            MethodInfo fn = par.Method;
            // The MethodInfo override the function.
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(0, typeof(int)),
                    Expr.LeftShift(Expr.Constant(2, typeof(int)),
                    Expr.Constant(1, typeof(int)), fn),
                    "Pass a method info that takes a paramarray");
            }));

            return EU.BlockVoid(Expressions);
        }


        // With a valid method info, pass values that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift With Values Widen To Args Method", new string[] { "negative", "LeftShift", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Values_Widen_To_Args_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Create a fn that's parameters are wider then the calling args.
            MethodInfo fn = new Func<long, long, int>((long a, long b) => { return 1; }).Method;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LeftShift(Expr.Constant(2, typeof(int)), Expr.Constant(1, typeof(int)), fn);
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift With Non Nullable and Nullable Args Types To Method", new string[] { "positive", "LeftShift", "operators", "Pri1" })]
        public static Expr Test_With_Non_Nullable_Nullable_Args_To_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int, int?, int>(NonNullableAndNullableMethod).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int)),
                                           Expr.LeftShift(Expr.Constant(2, typeof(int)),
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "LeftShift With Non Nullable and Nullable Args Types To Method"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        // Test     : With a null methodinfo, pass a value of non nullable type, the other of nullable type.
        // Expected : Exception.
        // Note     : Updated as positive test per update to bug 525563
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "LeftShift Null MethodInfo Pass Non Nullable Type And Nullable Type", new string[] { "positive", "LeftShift", "operations", "Pri1" })]
        public static Expr Test_With_Non_Nullable_And_Nullable_Args_To_Null_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expression Result = Expr.LeftShift(Expr.Constant(2, typeof(int)), Expr.Constant(null, typeof(int?)), null);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(null), Result));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        // helper function for: "With a valid methodInfo that returns boolean, pass arguments of nullable type."
        public static Boolean LeftShift_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate(int? a, int? b) {
            return true;
        }

        // With a valid methodInfo that returns boolean, pass arguments of nullable type.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean", new string[] { "positive", "LeftShift", "operators", "Pri1" })]
        public static Expr Test_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int?, int?, Boolean>(LeftShift_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant((Boolean)true, typeof(Boolean)),
                                           Expr.LeftShift(Expr.Constant(null, typeof(int?)),
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "LeftShift with a valid MethodInfo that returns boolean, passing arg of nullable type"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use to generate generic helper LeftShift statements that take custome methods
        private static Expr TestLeftShiftExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, MethodInfo fn, string testMsg) {
            List<Expression> Expressions = new List<Expression>();

            // Console.WriteLine("\n###__LeftShift__### Inside TestLeftShiftForType of Type = {0}", typeof(T));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.LeftShift(Expr.Constant(LeftVal, typeof(T)),
                                                           Expr.Constant(RightVal, typeof(T)), fn),
                                           testMsg));

            // Console.WriteLine("\n###__LeftShift__### Inside TestLeftShiftForType of Type = {0}", typeof(T));
            return EU.BlockVoid(Expressions);
        }

        // Generically pick the a method with a type helper functions
        public static T TwoParamMethodRetFirstArg<T>(T a, T b) { return a; }
        public static MethodInfo GetExpressionMethodInfo<T>() {
            return new Func<T, T, T>(TwoParamMethodRetFirstArg<T>).Method;
        }

        // Test     : LeftShift of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        // Note     : Didn't need to do this one.
        //            This covers Pri1 test "With a valid method info, LeftShift of normal values" Scenario
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_Across_Multiple_Types", new string[] { "positive", "LeftShift", "operators", "Pri3" })]
        public static Expr Test_Across_All_Multiple_Types(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(TestLeftShiftExprBlockForType<int>(1, 1, 1, GetExpressionMethodInfo<int>(), "LeftShift with MethodInfo test for int"));
            Expressions.Add(TestLeftShiftExprBlockForType<uint>(1, 1, 1, GetExpressionMethodInfo<uint>(), "LeftShift with MethodInfo test for uint"));
            Expressions.Add(TestLeftShiftExprBlockForType<long>(1, 1, 1, GetExpressionMethodInfo<long>(), "LeftShift with MethodInfo test for long"));
            Expressions.Add(TestLeftShiftExprBlockForType<ulong>(1, 1, 1, GetExpressionMethodInfo<ulong>(), "LeftShift with MethodInfo test for ulong"));
            Expressions.Add(TestLeftShiftExprBlockForType<double>(1, 1, 1, GetExpressionMethodInfo<double>(), "LeftShift with MethodInfo test for double"));
            Expressions.Add(TestLeftShiftExprBlockForType<float>(1, 1, 1, GetExpressionMethodInfo<float>(), "LeftShift with MethodInfo test for float"));
            Expressions.Add(TestLeftShiftExprBlockForType<short>(1, 1, 1, GetExpressionMethodInfo<short>(), "LeftShift with MethodInfo test for short"));
            Expressions.Add(TestLeftShiftExprBlockForType<ushort>(1, 1, 1, GetExpressionMethodInfo<ushort>(), "LeftShift with MethodInfo test for ushort"));
            Expressions.Add(TestLeftShiftExprBlockForType<byte>(1, 1, 1, GetExpressionMethodInfo<byte>(), "LeftShift with MethodInfo test for byte"));
            Expressions.Add(TestLeftShiftExprBlockForType<sbyte>(1, 1, 1, GetExpressionMethodInfo<sbyte>(), "LeftShift with MethodInfo test for sbyte"));
            Expressions.Add(TestLeftShiftExprBlockForType<Single>(1, 1, 1, GetExpressionMethodInfo<Single>(), "LeftShift with MethodInfo test for Single"));
            Expressions.Add(TestLeftShiftExprBlockForType<Decimal>(1, 1, 1, GetExpressionMethodInfo<Decimal>(), "LeftShift with MethodInfo test for Decimal"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        /// <summary>
        /// Used for test : Test_Overloaded_Operators
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
                return ((x.Val != y.Val));
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }
            //public static OverloadedOperator operator >>(OverloadedOperator a, int b)
            //{
            //    return new OverloadedOperator(b + a.Val);
            //}
            public static int operator <<(OverloadedOperator a, int b) {

                return (a.Val + b);
            }
            public static OverloadedOperator LeftShift(OverloadedOperator a, OverloadedOperator b) {
                return new OverloadedOperator(b.Val + a.Val);
            }

        }


        // User defined overloaded operator on left argument, arguments are the proper types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_Overloaded_Operator_On_Left_Arg", new string[] { "positive", "LeftShift", "operators", "Pri1" })]
        public static Expr Test_Overloaded_Operator_On_Left_Arg(EU.IValidator V) {
            OverloadedOperator LeftArg = new OverloadedOperator(1);
            OverloadedOperator Expected = new OverloadedOperator(2);

            var tree = EU.BlockVoid(
                EU.GenAreEqual(
                    Expr.Constant(2, typeof(int)),
                    Expr.LeftShift(Expr.Constant(LeftArg, typeof(OverloadedOperator)),
                    Expr.Constant(1, typeof(int))),
                  "Test LeftShift override opp"));

            V.Validate(tree);
            return tree;
        }



        // With a valid method info, pass two values of a derived class of the methodinfo's arguments
        public class MyDerivedType : OverloadedOperator {
            public MyDerivedType() { }
            public MyDerivedType(int a) : base(a) { }
            public int DerivedData { get; set; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift 14", new string[] { "positive", "LeftShift", "operators", "Pri1" })]
        public static Expr LeftShift14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OverloadedOperator).GetMethod("LeftShift");

            MyDerivedType right = new MyDerivedType(1);
            MyDerivedType left = new MyDerivedType(1);
            MyDerivedType expected = new MyDerivedType(2);

            Expr Res =
                Expr.LeftShift(
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



        // Test     :  Verify order of evaluation of expressions on LeftShift
        // Expected :  Verify res = "_first arg_second arg", and intresult = 3
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift_Order_Of_Operations_Verification", new string[] { "positive", "LeftShift", "operators", "Pri1" })]
        public static Expr Test_Order_Of_Operations_Verification(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var res = Expr.Variable(typeof(string), "x");
            var intresult = Expr.Variable(typeof(int), "y");

            var LeftArg = Expr.Block(EU.ConcatEquals(res, "_first arg"), Expr.Constant(7));
            var RightArg = Expr.Block(EU.ConcatEquals(res, "_second arg"), Expr.Constant(1));
            var TmpExps = LeftArg.Expressions;


            Expressions.Add(Expr.Assign(intresult, Expr.LeftShift(LeftArg, RightArg)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(14), intresult, "Order of operations validate intresult=14"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("_first arg_second arg"), res, "Order of operations validate res='_first arg_second arg'"));

            var tree = EU.BlockVoid(new[] { res, intresult }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Test    : LeftShift of values that overflow the type
        // Expected: Should just shifts the value out.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShift 18", new string[] { "positive", "LeftShift", "operators", "Pri1" })]
        public static Expr LeftShift18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Unsigned value overflow
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(uint.MaxValue - 1),
                    Expr.LeftShift(
                        Expr.Constant(uint.MaxValue),
                        Expr.Constant(1)

                    ),
                    "LeftShift Overflow the type 1"
                )
            );

            // Signed value overflow example
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(-2),
                    Expr.LeftShift(
                        Expr.Constant(int.MaxValue),
                        Expr.Constant(1)

                    ),
                    "LeftShift Overflow the type 1"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

    }
}


