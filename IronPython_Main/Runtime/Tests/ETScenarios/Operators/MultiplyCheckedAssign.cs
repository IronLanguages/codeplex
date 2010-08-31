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

    public class MultiplyAssignChecked {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

        #region Helper functions and inner classes


        // Use to generate generic helper MultiplyAssignChecked statements that take custome methods
        private static Expr TestMultiplyAssignCheckedExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, string testMsg) {
            List<Expression> Expressions = new List<Expression>();

            Expression expected = Expr.Constant(ExpectedVal, typeof(T));
            ParameterExpression Left = EU.Param<T>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(LeftVal, typeof(T))));

            Expression Right = Expr.Constant(RightVal, typeof(T));
            Expression OpToTest = Expr.MultiplyAssignChecked(Left, Right);



            Expression Ex = EU.GenAreEqual(expected, OpToTest, testMsg);
            Expressions.Add(Ex);

            //Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
            //                               Expr.MultiplyAssignChecked(Expr.Constant(LeftVal, typeof(T)),
            //                                               Expr.Constant(RightVal, typeof(T))),
            //                               testMsg));

            return Expr.Block(new[] { Left }, Expressions);
        }

        #endregion


        // Test     : MultiplyAssignChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_uint", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_unint(EU.IValidator V) {
            var tree = TestMultiplyAssignCheckedExprBlockForType<uint>(1, 1, 1, "Tests for uint");
            V.Validate(tree);
            return tree;
        }

        // Test     : MultiplyAssignChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_Decimal", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_Decimal(EU.IValidator V) {
            var tree = TestMultiplyAssignCheckedExprBlockForType<Decimal>(1, 1, 1, "Tests for Decimal");
            V.Validate(tree);
            return tree;
        }

        // Test     : MultiplyAssignChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_ulong", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_ulong(EU.IValidator V) {
            var tree = TestMultiplyAssignCheckedExprBlockForType<ulong>(1, 1, 1, "Tests for ulong");
            V.Validate(tree);
            return tree;
        }

        // Test     : MultiplyAssignChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_short", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_short(EU.IValidator V) {
            var tree = TestMultiplyAssignCheckedExprBlockForType<short>(1, 1, 1, "Test for short type");
            V.Validate(tree);
            return tree;
        }
        // Test     : MultiplyAssignChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_ushort", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_ushort(EU.IValidator V) {
            var tree = TestMultiplyAssignCheckedExprBlockForType<ushort>(1, 1, 1, "Tests for ushort");
            V.Validate(tree);
            return tree;
        }

        // Test     : MultiplyAssignChecked of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_long", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_long(EU.IValidator V) {
            var tree = TestMultiplyAssignCheckedExprBlockForType<long>(1, 1, 1, "Testing MultiplyAssignChecked for long types");
            V.Validate(tree);
            return tree;
        }
        // Test     : MultiplyAssignChecked of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_double", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_double(EU.IValidator V) {
            var tree = TestMultiplyAssignCheckedExprBlockForType<double>(1, 1, 1, "Testing MultiplyAssignChecked for double types");
            V.Validate(tree);
            return tree;
        }

        // Test     : MultiplyAssignChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_byte", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_byte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestMultiplyAssignCheckedExprBlockForType<byte>(1, 1, 1, "Testing MultiplyAssignChecked for byte types");
            });

            return tree;
        }

        // Test     : MultiplyAssignChecked of byte, sbyte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_sbyte", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_sbyte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestMultiplyAssignCheckedExprBlockForType<sbyte>(1, 1, 1, "Testing MultiplyAssignChecked for sbyte types");
            });

            return tree;
        }

        // Test     : MultiplyAssignChecked of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_Single", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_Single(EU.IValidator V) {
            var tree = TestMultiplyAssignCheckedExprBlockForType<Single>(1, 1, 1, "Testing MultiplyAssignChecked for Single types");
            V.Validate(tree);
            return tree;

        }

        // MultiplyAssignChecked of Boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked Boolean Constant Test", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant(1, typeof(Boolean)));
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }

        // MultiplyAssignChecked of String constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked String Constant Test", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_String(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<string>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant("1", typeof(string))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant("1", typeof(string)));
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

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
        // MultiplyAssignChecked of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked Class Object Test", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Object(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<UserDefinedObject>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new UserDefinedObject(1))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant(new UserDefinedObject(1)));
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }


        // MultiplyAssignChecked of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 8", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr MultiplyAssignChecked8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<TestStruct>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.MultiplyAssignChecked(Left, Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "MultiplyAssignChecked 1");
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }


        // Test     : MultiplyAssignCheckeding across mixed types
        // Expected : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked Across Mixed Types", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MixedTypes(EU.IValidator V) {

            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() => { Expr.MultiplyAssignChecked(Left, Expr.Constant(1, typeof(byte))); }));

            return Expr.Empty();
        }
        /// <summary>
        /// Test     : Pass null to method, normal arguments to left and right
        /// Expected : Same as MultiplyAssignChecked(left, right)
        /// </summary>
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked Null MethodInfo", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1),
                              Expr.MultiplyAssignChecked(Left, Expr.Constant(1, typeof(int)), (MethodInfo)null)));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Test     : Pass null to method, normal arguments to left and right - with annotations
        // Expected : Same as MultiplyAssignChecked(left, right) 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked Null MethodInfo Annotations", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo_Annotations(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1),
                                  Expr.MultiplyAssignChecked(Left, Expr.Constant(1, typeof(int)),
                                  (MethodInfo)null), "Test MultiplyAssignChecked for Null Method Info Ann"));

            var tree = Expr.Block(new[] { Left }, Expressions);
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked MethodInfo With No Args", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(1, typeof(int)), TakesNoArgs));
            });

            return Expr.Block(new[] { Left }, Expressions);
        }

        // Pass a methodinfo that takes no arguments 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked MethodInfo With No Args Ann", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args_Ann(EU.IValidator V) {

            List<Expression> Expressions = new List<Expression>();
            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant(1, typeof(int)), TakesNoArgs);
            }));

            return Expr.Block(new[] { Left }, Expressions);

        }

        // Pass a method info that takes a paramarray
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignCheckedMethodInfo_That_Takes_Paramarray", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_That_Takes_Paramarray(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));


            ParmsParameter par = new ParmsParameter(MethodWithParamsArgs);
            MethodInfo fn = par.Method;
            // The MethodInfo override the function.
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(0, typeof(int)),
                    Expr.MultiplyAssignChecked(Left,
                    Expr.Constant(1, typeof(int)), fn),
                    "Pass a method info that takes a paramarray");
            }));

            return Expr.Block(new[] { Left }, Expressions);
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked With Values Widen To Args Method", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Values_Widen_To_Args_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            // Create a fn that's parameters are wider then the calling args.
            MethodInfo fn = new Func<long, long, int>((long a, long b) => { return 1; }).Method;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant(1, typeof(int)), fn);
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }


        /// <summary>
        /// Also use for other tests. Could reuse a lot of these.
        /// </summary>
        public static int NonNullableAndNullableMethod(int a, int? b) {
            return 1;
        }

        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked With Non Nullable and Nullable Args Types To Method", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_With_Non_Nullable_Nullable_Args_To_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            MethodInfo fn = new Func<int, int?, int>(NonNullableAndNullableMethod).Method;


            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int)),
                                           Expr.MultiplyAssignChecked(Left,
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "MultiplyAssignChecked With Non Nullable and Nullable Args Types To Method"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // Test     : With a null methodinfo, pass a value of non nullable type, the other of nullable type.
        // Expected : Exception.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked Null MethodInfo Pass Non Nullable Type And Nullable Type", new string[] { "negative", "MultiplyAssignChecked", "operations", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Non_Nullable_And_Nullable_Args_To_Null_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant(null, typeof(int?)), null);
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }


        // helper function for: "With a valid methodInfo that returns boolean, pass arguments of nullable type."
        public static int? MultiplyAssignChecked_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate(int? a, int? b) {
            return 1;
        }

        // With a valid methodInfo that returns boolean, pass arguments of nullable type.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int?>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(null, typeof(int?))));

            MethodInfo fn = new Func<int?, int?, int?>(MultiplyAssignChecked_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int?)),
                                           Expr.MultiplyAssignChecked(Left,
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "MultiplyAssignChecked with a valid MethodInfo that returns boolean, passing arg of nullable type"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use to generate generic helper MultiplyAssignChecked statements that take custome methods
        private static Expr TestMultiplyAssignCheckedExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, MethodInfo fn, string testMsg) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<T>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(LeftVal, typeof(T))));


            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.MultiplyAssignChecked(Left,
                                                           Expr.Constant(RightVal, typeof(T)), fn),
                                           testMsg));

            return Expr.Block(new[] { Left }, Expressions);
        }

        // Dynamically pick the a method with a type helper functions
        public static T TwoParamMethodRetFirstArg<T>(T a, T b) { return a; }
        public static MethodInfo GetExpressionMethodInfo<T>() {
            return new Func<T, T, T>(TwoParamMethodRetFirstArg<T>).Method;
        }

        // Test     : MultiplyAssignChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        // Note     : Didn't need to do this one.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_Across_Multiple_Types", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri3" })]
        public static Expr Test_Across_All_Multiple_Types(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<int>(1, 1, 1, GetExpressionMethodInfo<int>(), "MultiplyAssignChecked with MethodInfo test for int"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<uint>(1, 1, 1, GetExpressionMethodInfo<uint>(), "MultiplyAssignChecked with MethodInfo test for uint"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<long>(1, 1, 1, GetExpressionMethodInfo<long>(), "MultiplyAssignChecked with MethodInfo test for long"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<ulong>(1, 1, 1, GetExpressionMethodInfo<ulong>(), "MultiplyAssignChecked with MethodInfo test for ulong"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<double>(1, 1, 1, GetExpressionMethodInfo<double>(), "MultiplyAssignChecked with MethodInfo test for double"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<float>(1, 1, 1, GetExpressionMethodInfo<float>(), "MultiplyAssignChecked with MethodInfo test for float"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<short>(1, 1, 1, GetExpressionMethodInfo<short>(), "MultiplyAssignChecked with MethodInfo test for short"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<ushort>(1, 1, 1, GetExpressionMethodInfo<ushort>(), "MultiplyAssignChecked with MethodInfo test for ushort"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<byte>(1, 1, 1, GetExpressionMethodInfo<byte>(), "MultiplyAssignChecked with MethodInfo test for byte"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<sbyte>(1, 1, 1, GetExpressionMethodInfo<sbyte>(), "MultiplyAssignChecked with MethodInfo test for sbyte"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<Single>(1, 1, 1, GetExpressionMethodInfo<Single>(), "MultiplyAssignChecked with MethodInfo test for Single"));
            Expressions.Add(TestMultiplyAssignCheckedExprBlockForType<Decimal>(1, 1, 1, GetExpressionMethodInfo<Decimal>(), "MultiplyAssignChecked with MethodInfo test for Decimal"));

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
            public static OverloadedOperator operator *(OverloadedOperator a, int b) {

                return new OverloadedOperator(b + a.Val);
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

            public override int GetHashCode() {
                return base.GetHashCode();
            }

            public override bool Equals(object obj) {
                return base.Equals(obj);
            }

        }

        // User defined overloaded operator on left argument, arguments are the proper types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_Overloaded_Operator_On_Left_Arg", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_Overloaded_Operator_On_Left_Arg(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            OverloadedOperator LeftArg = new OverloadedOperator(1);

            ParameterExpression Left = EU.Param<OverloadedOperator>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(LeftArg, typeof(OverloadedOperator))));

            Expr Res = Expr.MultiplyAssignChecked(Left, Expr.Constant(1, typeof(int)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Property(Res, typeof(OverloadedOperator).GetProperty("Val")), "Test MultiplyAssignChecked override opp"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }



        // With a valid method info, pass two values of a derived class of the methodinfo's arguments
        public class MyDerivedType : OverloadedOperator {
            public MyDerivedType(int a) : base(a) { }
            public int DerivedData { get; set; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 14", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr MultiplyAssignChecked14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<OverloadedOperator>();

            MethodInfo mi = typeof(OverloadedOperator).GetMethod("Multiply");

            MyDerivedType right = new MyDerivedType(1);
            MyDerivedType left = new MyDerivedType(1);
            MyDerivedType expected = new MyDerivedType(2);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(left, typeof(OverloadedOperator))));

            Expr Res =
                Expr.MultiplyAssignChecked(
                    Left,
                    Expr.Constant(right, typeof(OverloadedOperator)),
                    mi
                );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(expected, typeof(MyDerivedType)),
                    Res
                )
            );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class TestOrder {
            public string res;
            private int _Value;
            public int this[string append] {
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

        // Test     :  Verify order of evaluation of expressions on MultiplyAssignChecked
        // Expected :  Verify res = "_first arg_second arg", and intresult = 3
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked_Order_Of_Operations_Verification", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr Test_Order_Of_Operations_Verification(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            TestOrder order = new TestOrder();

            ParameterExpression ov = Expr.Parameter(typeof(TestOrder), "order");

            Expressions.Add(Expr.Assign(ov, Expr.Constant(order)));
            Expressions.Add(Expr.Assign(Expr.Property(ov, order.pi, Expr.Constant("")), Expr.Constant(2)));


            Expr Res =
                Expr.MultiplyAssignChecked(
                    Expr.Property(ov, order.pi, Expr.Constant("One")),
                    Expr.MultiplyAssignChecked(
                        Expr.Property(ov, order.pi, Expr.Constant("Two")),
                        Expr.MultiplyAssignChecked(
                            Expr.Property(ov, order.pi, Expr.Constant("Three")),
                            Expr.Constant(4)
                        )
                    ),
                    null
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(32)));
            Expressions.Add(EU.GenAreEqual(Expr.Field(ov, order.resi), Expr.Constant("OneTwoThreeThreeTwoOne")));

            var tree = Expr.Block(new[] { ov }, Expressions);
            V.Validate(tree);
            return tree;

        }

        //Test    : MultiplyAssignChecked of values that overflow the type
        //Expected: Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 18", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr MultiplyAssignChecked18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(Int32.MaxValue)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(Int32.MinValue + 1),
                    Expr.MultiplyAssignChecked(
                        Left,
                        Expr.Constant(-2)

                    ),
                    "MultiplyAssignChecked Overflow the type 1"
                )
            );

            Expressions.Add(Expr.Assign(Left, Expr.Constant(-1)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(Int32.MaxValue),
                    Expr.MultiplyAssignChecked(
                        Left,
                        Expr.Constant(Int32.MinValue)
                    ),
                    "MultiplyAssignChecked Overflow the type 2"
                )
            );

            ParameterExpression Left2 = EU.Param<double>();
            Expressions.Add(Expr.Assign(Left2, Expr.Constant(double.MaxValue)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant((double)(Double.PositiveInfinity)),
                    Expr.MultiplyAssignChecked(
                        Left2,
                        Expr.Constant((double)(-Double.MaxValue))
                    ),
                    "MultiplyAssignChecked Overflow the type 3"
                )
            );

            Expressions.Add(Expr.Assign(Left2, Expr.Constant(double.MinValue)));
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant((double)(Double.NegativeInfinity)),
                    Expr.MultiplyAssignChecked(
                        Left2,
                        Expr.Constant((double)(-Double.MinValue))
                    ),
                    "MultiplyAssignChecked Overflow the type 4"
                )
            );

            var tree = Expr.Block(new[] { Left, Left2 }, Expressions);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }

        // MultiplyAssign with an array index expression as the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 19", new string[] { "positive", "MultiplyAssign", "operators", "Pri1" })]
        public static Expr MultiplyAssignChecked19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Variable(typeof(int[]), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6), Expr.Constant(7), Expr.Constant(8) })));

            Expressions.Add(Expr.MultiplyAssign(Expr.ArrayAccess(Left, Expr.Constant(2)), Expr.Constant(4)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(24), Expr.ArrayIndex(Left, Expr.Constant(2)), "SA 1"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass a non assignable expression to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 20_1", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr MultiplyAssignChecked20_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignCheckedInts");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.MultiplyAssignChecked(Expr.Constant(1), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(Expressions);
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 25", new string[] { "negative", "MultiplyAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr MultiplyAssignChecked25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("BadReturn");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }




        //Pass null to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 26", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr MultiplyAssignChecked26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.MultiplyAssignChecked(null, Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        //Pass null to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 27", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr MultiplyAssignChecked27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, null);
            }));

            return Expr.Empty();
        }

        //Pass a non value returning expression to left
        //Pass a non value returning expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 29", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr MultiplyAssignChecked29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Empty());
            }));

            var tree = Expr.Empty();

            return tree;
        }

        //Pass a Block to the left argument
        //bug 550086
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 30", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr MultiplyAssignChecked30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Block(Expr.Constant(1));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a Method Call to the left argument
        //bug 550086
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 31", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr MultiplyAssignChecked31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Call(Expr.Constant("1"), typeof(string).GetMethod("ToString", new Type[] { }));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a constant to the left argument
        //bug 550086
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 33", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr MultiplyAssignChecked33(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Constant(1);
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }


        // pass a MethodInfo that takes three arguments
        public static int MultiplyAssignChecked3Args(int arg1, int arg2, int arg3) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 34", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr MultiplyAssignChecked34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignChecked3Args");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }

        // pass a MethodInfo with two arguments, that returns void
        public static void MultiplyAssignChecked2ArgsVoid(int arg1, int arg2) {
            return;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 35", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr MultiplyAssignChecked35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignChecked2ArgsVoid");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }


        //with a valid method info, pass two values of an enum class (methodinfo's arguments are of the base integer type)
        public static int MultiplyAssignChecked2Args(int arg1, int arg2) {
            return arg1 + arg2;
        }
        enum e36 : int {
        }
        //enum -> int conversion not being accepted.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 36", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr MultiplyAssignChecked36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignChecked2Args");

            ParameterExpression Left = Expr.Parameter(typeof(e36), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((e36)1)));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant((e36)2), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type
        public static int MultiplyAssignCheckedMethod37(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 37", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr MultiplyAssignChecked37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignCheckedMethod37");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(2, typeof(int?)), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //with a valid method info, pass two values of the same nullable type
        public static int MultiplyAssignCheckedMethod38(int? arg1, int? arg2) {
            return (arg1 + arg2).GetValueOrDefault();
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 38", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr MultiplyAssignChecked38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignCheckedMethod38");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant(2, typeof(int?)), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type. return is nullable, arguments aren't
        public static int? MultiplyAssignCheckedMethod39(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 39", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr MultiplyAssignChecked39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignCheckedMethod39");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.MultiplyAssignChecked(Left, Expr.Constant(2, typeof(int?)), mi);
            }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Empty();

            return tree;
        }







        //User defined operator on right argument, arguments are of the proper types
        public class C40 {
            public int Val = 2;
            public static int operator *(int a, C40 b) {
                return b.Val + a;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 40", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr MultiplyAssignChecked40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(new C40())));

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
            public static C41 operator *(C41 b, Exception a) {
                return new C41(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 41", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr MultiplyAssignChecked41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C41), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C41("1"))));

            Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(new ArgumentException("2"))));

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
            public static Exception operator *(Exception a, C42 b) {
                return new Exception(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 42", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr MultiplyAssignChecked42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(Exception), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new Exception("1"))));

            Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(new C42("2"))));

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
            public static C43 operator *(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "A");
            }
        }
        public class C43_1 {
            public string Val;

            public C43_1(string init) {
                Val = init;
            }
            public static C43 operator *(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "B");
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 43", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr MultiplyAssignChecked43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C43), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C43("1"))));

            Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(new C43_1("2"))));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 44", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri2" })]
        public static Expr MultiplyAssignChecked44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C44), "instance");

            Expr Left = Expr.Field(Instance, C44.Field);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C44())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with a static field as the left argument
        public class C45 {
            public static int Val;
            public static FieldInfo Field = typeof(C45).GetField("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 45", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri2" })]
        public static Expr MultiplyAssignChecked45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Field(null, C45.Field);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10), Left));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        //Add with an instance Property as the left argument
        public class C46 {
            public int Val { get; set; }
            public static PropertyInfo Property = typeof(C46).GetProperty("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 46", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri2" })]
        public static Expr MultiplyAssignChecked46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C46), "instance");

            Expr Left = Expr.Property(Instance, C46.Property);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C46())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with a static Property as the left argument
        public class C47 {
            public static int Val { get; set; }
            public static PropertyInfo Property = typeof(C47).GetProperty("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 47", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri2" })]
        public static Expr MultiplyAssignChecked47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Property(null, C47.Property);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10), Left));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 48", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri2" })]
        public static Expr MultiplyAssignChecked48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C48), "instance");

            Expr Left = Expr.Property(Instance, C48.Property, new[] { Expr.Constant(2) });

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C48())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(3)));

            Expressions.Add(Expr.MultiplyAssignChecked(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(39), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }



        public static int MultiplyAssignCheckedConv(short arg1, short arg2) {
            return (int)(arg1 + arg2);
        }
        //MultiplyAssignChecked with a lambda conversion for the return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 49_1", new string[] { "positive", "MultiplyAssignChecked", "operators", "Pri1" })]
        public static Expr MultiplyAssignChecked49_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(short)), arg1
                                                );
            MethodInfo mi = typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignCheckedConv");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((short)1)));
            Expressions.Add(
                            EU.GenAreEqual(
                                            Expr.Constant((short)3),
                                            Expr.MultiplyAssignChecked(Left, Expr.Constant((short)2), mi, Conv)
                                           )
                           );


            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //MultiplyAssignChecked with a lambda conversion for the return type, to the wrong type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 50_1", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr MultiplyAssignChecked50_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(byte)), arg1
                                                );
            MethodInfo mi = typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignCheckedConv");

            EU.Throws<System.InvalidOperationException>(() => { Expr.MultiplyAssignChecked(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }

        //Passing null to conversion lambda
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 51_1", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr MultiplyAssignChecked51_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            LambdaExpression Conv = null;
            MethodInfo mi = typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignCheckedConv");

            EU.Throws<System.ArgumentException>(() => { Expr.MultiplyAssignChecked(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }


        //Passing a conversion lambda when it's not needed
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyAssignChecked 52_1", new string[] { "negative", "MultiplyAssignChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr MultiplyAssignChecked52_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(int)), arg1
                                                );

            MethodInfo mi = null;// typeof(MultiplyAssignChecked).GetMethod("MultiplyAssignCheckedInts");


            EU.Throws<System.InvalidOperationException>(() => { Expr.MultiplyAssignChecked(Left, Expr.Constant(2), mi, Conv); });

            return Expr.Empty();
        }
    }
}