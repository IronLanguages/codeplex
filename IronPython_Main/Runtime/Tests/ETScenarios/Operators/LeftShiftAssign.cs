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

    public class LeftShiftAssign {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

        #region Helper functions and inner classes

        // Use to generate generic helper LeftShiftAssign statements that take custome methods
        private static Expr TestLeftShiftAssignExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, string testMsg) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<T>("Left", LeftVal, Expressions);

            // Console.WriteLine("\n###__LeftShiftAssign__### Inside TestLeftShiftAssignForType of Type = {0}", typeof(T));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.LeftShiftAssign(Left,
                                                           Expr.Constant(RightVal, typeof(T))),
                                           testMsg));

            // Console.WriteLine("\n###__LeftShiftAssign__### Inside TestLeftShiftAssignForType of Type = {0}", typeof(T));
            return Expr.Block(new[] { Left }, Expressions);
        }
        #endregion

        // Test     : LeftShiftAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_uint", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_unint(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftAssignExprBlockForType<uint>(1, 1, 0, "Tests for uint");
            });

            return tree;
        }

        // Test     : LeftShiftAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_Decimal", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Decimal(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftAssignExprBlockForType<Decimal>(1, 1, 0, "Tests for Decimal");
            });

            return tree;
        }

        // Test     : LeftShiftAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_ulong", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_ulong(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftAssignExprBlockForType<ulong>(1, 1, 0, "Tests for ulong");
            });

            return tree;
        }

        // Test     : LeftShiftAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_short", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_short(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftAssignExprBlockForType<short>(1, 1, 0, "Test for short type");
            });

            return tree;
        }
        // Test     : LeftShiftAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_ushort", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_ushort(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftAssignExprBlockForType<ushort>(1, 1, 0, "Tests for ushort");
            });

            return tree;
        }

        // Test     : LeftShiftAssign of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_long", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_long(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftAssignExprBlockForType<long>(1, 1, 0, "Testing LeftShiftAssign for long types");
            });

            return tree;
        }
        // Test     : LeftShiftAssign of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_double", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_double(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftAssignExprBlockForType<double>(1, 1, 0, "Testing LeftShiftAssign for double types");
            });

            return tree;
        }

        // Test     : LeftShiftAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_byte", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_byte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftAssignExprBlockForType<byte>(1, 1, 0, "Testing LeftShiftAssign for byte types");
            });

            return tree;
        }

        // Test     : LeftShiftAssign of byte, sbyte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_sbyte", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_sbyte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftAssignExprBlockForType<sbyte>(1, 1, 0, "Testing LeftShiftAssign for sbyte types");
            });

            return tree;
        }

        // LeftShiftAssigning with non integer right side.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 20", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int>("Left", 1, Expressions);

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LeftShiftAssign(Left, Expr.Constant('1', typeof(char)));
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // Test     : LeftShiftAssign of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_Single", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Single(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestLeftShiftAssignExprBlockForType<Single>(1, 1, 0, "Testing LeftShiftAssign for Single types");
            });

            return tree;

        }

        // LeftShiftAssign of Boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign Boolean Constant Test", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<Boolean>("Left", true, Expressions);

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.LeftShiftAssign(Left, Expr.Constant(1, typeof(Boolean)));
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }

        // LeftShiftAssign of String constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign String Constant Test", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_String(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<string>("Left", "1", Expressions);

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LeftShiftAssign(Left, Expr.Constant("1", typeof(string)));
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
        // LeftShiftAssign of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign Class Object Test", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Object(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<UserDefinedObject>("Left", new UserDefinedObject(1), Expressions);

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LeftShiftAssign(Left, Expr.Constant(new UserDefinedObject(1)));
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // LeftShiftAssign of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 8", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LeftShiftAssign8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<TestStruct>("Left", new TestStruct(1), Expressions);

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                EU.GenAreEqual(Expr.LeftShiftAssign(Left, Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "LeftShiftAssign 1");
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }



        // Test     : LeftShiftAssigning across mixed types
        // Expected : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign Across Mixed Types", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MixedTypes(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int>("Left", 1, Expressions);

            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.LeftShiftAssign(Left, Expr.Constant(1, typeof(byte))); }));

            return Expr.Empty();
        }
        /// <summary>
        /// Test     : Pass null to method, normal arguments to left and right
        /// Expected : Same as LeftShiftAssign(left, right)
        /// </summary>
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign Null MethodInfo", new string[] { "positive", "LeftShiftAssign", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int>("Left", 1, Expressions);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2),
                              Expr.LeftShiftAssign(Left, Expr.Constant(1, typeof(int)), (MethodInfo)null)));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Test     : Pass null to method, normal arguments to left and right - with annotations
        // Expected : Same as LeftShiftAssign(left, right) 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign Null MethodInfo Annotations", new string[] { "positive", "LeftShiftAssign", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo_Annotations(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int>("Left", 1, Expressions);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2),
                                  Expr.LeftShiftAssign(Left, Expr.Constant(1, typeof(int)),
                                  (MethodInfo)null), "Test LeftShiftAssign for Null Method Info Ann"));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign MethodInfo With No Args", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int>("Left", 1, Expressions);

            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.LeftShiftAssign(Left, Expr.Constant(1, typeof(int)), TakesNoArgs));
            });

            return Expr.Block(new[] { Left }, Expressions);
        }

        // Pass a methodinfo that takes no arguments 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign MethodInfo With No Args Ann", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args_Ann(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int>("Left", 1, Expressions);

            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.LeftShiftAssign(Left, Expr.Constant(1, typeof(int)), TakesNoArgs);
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

        // Pass a method info that takes a paramarray
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssignMethodInfo_That_Takes_Paramarray", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_That_Takes_Paramarray(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int>("Left", 2, Expressions);

            ParmsParameter par = new ParmsParameter(MethodWithParamsArgs);
            MethodInfo fn = par.Method;
            // The MethodInfo override the function.
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(0, typeof(int)),
                    Expr.LeftShiftAssign(Left, Expr.Constant(1, typeof(int)), fn),
                    "Pass a method info that takes a paramarray");
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }


        // With a valid method info, pass values that widen to the arguments of the method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign With Values Widen To Args Method", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Values_Widen_To_Args_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int>("Left", 2, Expressions);

            // Create a fn that's parameters are wider then the calling args.
            MethodInfo fn = new Func<long, long, int>((long a, long b) => { return 1; }).Method;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LeftShiftAssign(Left, Expr.Constant(1, typeof(int)), fn);
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign With Non Nullable and Nullable Args Types To Method", new string[] { "positive", "LeftShiftAssign", "operators", "Pri1" })]
        public static Expr Test_With_Non_Nullable_Nullable_Args_To_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int>("Left", 2, Expressions);

            MethodInfo fn = new Func<int, int?, int>(NonNullableAndNullableMethod).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int)),
                                           Expr.LeftShiftAssign(Left,
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "LeftShiftAssign With Non Nullable and Nullable Args Types To Method"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // Test     : With a null methodinfo, pass a value of non nullable type, the other of nullable type.
        // Expected : Exception.
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "LeftShiftAssign Null MethodInfo Pass Non Nullable Type And Nullable Type", new string[] { "negative", "LeftShiftAssign", "operations", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Non_Nullable_And_Nullable_Args_To_Null_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int>("Left", 2, Expressions);

            Expressions.Add(Expr.LeftShiftAssign(Left, Expr.Constant(null, typeof(int?)), null));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, false);
            return tree;
        }


        // helper function for: "With a valid methodInfo that returns boolean, pass arguments of nullable type."
        public static int? LeftShiftAssign_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate(int? a, int? b) {
            return 1;
        }

        // With a valid methodInfo that returns boolean, pass arguments of nullable type.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean", new string[] { "positive", "LeftShiftAssign", "operators", "Pri1" })]
        public static Expr Test_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<int?>("Left", null, Expressions);

            MethodInfo fn = new Func<int?, int?, int?>(LeftShiftAssign_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int?)),
                                           Expr.LeftShiftAssign(Left,
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "LeftShiftAssign with a valid MethodInfo that returns boolean, passing arg of nullable type"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use to generate generic helper LeftShiftAssign statements that take custome methods
        private static Expr TestLeftShiftAssignExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, MethodInfo fn, string testMsg) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<T>("Left", LeftVal, Expressions);

            // Console.WriteLine("\n###__LeftShiftAssign__### Inside TestLeftShiftAssignForType of Type = {0}", typeof(T));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.LeftShiftAssign(Left,
                                                           Expr.Constant(RightVal, typeof(T)), fn),
                                           testMsg));

            // Console.WriteLine("\n###__LeftShiftAssign__### Inside TestLeftShiftAssignForType of Type = {0}", typeof(T));
            return Expr.Block(new[] { Left }, Expressions);
        }

        // Generically pick the a method with a type helper functions
        public static T TwoParamMethodRetFirstArg<T>(T a, T b) { return a; }
        public static MethodInfo GetExpressionMethodInfo<T>() {
            return new Func<T, T, T>(TwoParamMethodRetFirstArg<T>).Method;
        }

        // Test     : LeftShiftAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        // Note     : Didn't need to do this one.
        //            This covers Pri1 test "With a valid method info, LeftShiftAssign of normal values" Scenario
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_Across_Multiple_Types", new string[] { "positive", "LeftShiftAssign", "operators", "Pri3" })]
        public static Expr Test_Across_All_Multiple_Types(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(TestLeftShiftAssignExprBlockForType<int>(1, 1, 1, GetExpressionMethodInfo<int>(), "LeftShiftAssign with MethodInfo test for int"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<uint>(1, 1, 1, GetExpressionMethodInfo<uint>(), "LeftShiftAssign with MethodInfo test for uint"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<long>(1, 1, 1, GetExpressionMethodInfo<long>(), "LeftShiftAssign with MethodInfo test for long"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<ulong>(1, 1, 1, GetExpressionMethodInfo<ulong>(), "LeftShiftAssign with MethodInfo test for ulong"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<double>(1, 1, 1, GetExpressionMethodInfo<double>(), "LeftShiftAssign with MethodInfo test for double"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<float>(1, 1, 1, GetExpressionMethodInfo<float>(), "LeftShiftAssign with MethodInfo test for float"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<short>(1, 1, 1, GetExpressionMethodInfo<short>(), "LeftShiftAssign with MethodInfo test for short"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<ushort>(1, 1, 1, GetExpressionMethodInfo<ushort>(), "LeftShiftAssign with MethodInfo test for ushort"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<byte>(1, 1, 1, GetExpressionMethodInfo<byte>(), "LeftShiftAssign with MethodInfo test for byte"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<sbyte>(1, 1, 1, GetExpressionMethodInfo<sbyte>(), "LeftShiftAssign with MethodInfo test for sbyte"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<Single>(1, 1, 1, GetExpressionMethodInfo<Single>(), "LeftShiftAssign with MethodInfo test for Single"));
            Expressions.Add(TestLeftShiftAssignExprBlockForType<Decimal>(1, 1, 1, GetExpressionMethodInfo<Decimal>(), "LeftShiftAssign with MethodInfo test for Decimal"));

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
            //public static OverloadedOperator operator >>(OverloadedOperator a, int b)
            //{
            //    return new OverloadedOperator(b + a.Val);
            //}
            public static OverloadedOperator operator <<(OverloadedOperator a, int b) {

                return new OverloadedOperator(a.Val + b);
            }
            public static OverloadedOperator LeftShiftAssign(OverloadedOperator a, OverloadedOperator b) {
                return new OverloadedOperator(b.Val + a.Val);
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }

            public override int GetHashCode() {
                return base.GetHashCode();
            }
        }


        // User defined overloaded operator on left argument, arguments are the proper types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_Overloaded_Operator_On_Left_Arg", new string[] { "positive", "LeftShiftAssign", "operators", "Pri1" })]
        public static Expr Test_Overloaded_Operator_On_Left_Arg(EU.IValidator V) {
            OverloadedOperator LeftArg = new OverloadedOperator(1);
            OverloadedOperator Expected = new OverloadedOperator(2);

            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<OverloadedOperator>("Left", LeftArg, Expressions);

            Expr Res = Expr.LeftShiftAssign(Left,
                              Expr.Constant(1, typeof(int)));

            Expressions.Add(EU.GenAreEqual(
                //                 Expr.Constant(Expected, typeof(OverloadedOperator)), 
                              Expr.Constant(2, typeof(int)),
                              Expr.Property(Res, typeof(OverloadedOperator).GetProperty("Val")),
                              "Test LeftShiftAssign override opp"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // With a valid method info, pass two values of a derived class of the methodinfo's arguments
        public class MyDerivedType : OverloadedOperator {
            public MyDerivedType() { }
            public MyDerivedType(int a) : base(a) { }
            public int DerivedData { get; set; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 14", new string[] { "positive", "LeftShiftAssign", "operators", "Pri1" })]
        public static Expr LeftShiftAssign14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(OverloadedOperator).GetMethod("LeftShiftAssign");

            MyDerivedType right = new MyDerivedType(1);
            MyDerivedType left = new MyDerivedType(1);
            MyDerivedType expected = new MyDerivedType(2);

            ParameterExpression Left = EU.ParamInit<OverloadedOperator>("Left", left, Expressions);

            Expr Res =
                Expr.RightShift(
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

        // Test     :  Verify order of evaluation of expressions on LeftShiftAssign
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign_Order_Of_Operations_Verification", new string[] { "positive", "LeftShiftAssign", "operators", "Pri1" })]
        public static Expr Test_Order_Of_Operations_Verification(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            TestOrder order = new TestOrder();

            ParameterExpression ov = Expr.Parameter(typeof(TestOrder), "order");

            Expressions.Add(Expr.Assign(ov, Expr.Constant(order)));
            Expressions.Add(Expr.Assign(Expr.Property(ov, order.pi, Expr.Constant("")), Expr.Constant(1)));


            Expr Res =
                Expr.LeftShiftAssign(
                    Expr.Property(ov, order.pi, Expr.Constant("One")),
                    Expr.LeftShiftAssign(
                        Expr.Property(ov, order.pi, Expr.Constant("Two")),
                        Expr.LeftShiftAssign(
                            Expr.Property(ov, order.pi, Expr.Constant("Three")),
                            Expr.Constant(1)
                        )
                    ),
                    null
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(16)));
            Expressions.Add(EU.GenAreEqual(Expr.Field(ov, order.resi), Expr.Constant("OneTwoThreeThreeTwoOne")));

            var tree = Expr.Block(new[] { ov }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Test    : LeftShiftAssign of values that overflow the type
        // Expected: Should just shifts the value out.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 18", new string[] { "positive", "LeftShiftAssign", "operators", "Pri1" })]
        public static Expr LeftShiftAssign18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.ParamInit<uint>("Left", uint.MaxValue, Expressions);

            // Unsigned value overflow
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(uint.MaxValue - 1),
                    Expr.LeftShiftAssign(
                        Left,
                        Expr.Constant(1)

                    ),
                    "LeftShiftAssign Overflow the type 1"
                )
            );

            ParameterExpression Left2 = EU.ParamInit<int>("Left", int.MaxValue, Expressions);

            // Signed value overflow example
            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(-2),
                    Expr.LeftShiftAssign(
                        Left2,
                        Expr.Constant(1)

                    ),
                    "LeftShiftAssign Overflow the type 1"
                )
            );

            var tree = Expr.Block(new[] { Left, Left2 }, Expressions);
            V.Validate(tree);
            return tree;
        }


        //Pass a non assignable expression to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 20_1", new string[] { "negative", "LeftShiftAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr LeftShiftAssign20_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(LeftShiftAssign).GetMethod("LeftShiftAssignInts");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.LeftShiftAssign(Expr.Constant(1), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(Expressions);
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 25", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr LeftShiftAssign25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("BadReturn");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.LeftShiftAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }



        //Pass null to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 26", new string[] { "negative", "LeftShiftAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr LeftShiftAssign26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.LeftShiftAssign(null, Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        //Pass null to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 27", new string[] { "negative", "LeftShiftAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr LeftShiftAssign27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.LeftShiftAssign(Left, null);
            }));

            return Expr.Empty();
        }

        //Pass a non value returning expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 29", new string[] { "negative", "LeftShiftAssign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr LeftShiftAssign29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.LeftShiftAssign(Left, Expr.Empty());
            }));

            var tree = Expr.Empty();

            return tree;
        }

        //Pass a Block to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 30", new string[] { "negative", "LeftShiftAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr LeftShiftAssign30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Block(Expr.Constant(1));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.LeftShiftAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a Method Call to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 31", new string[] { "negative", "LeftShiftAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr LeftShiftAssign31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Call(Expr.Constant("1"), typeof(string).GetMethod("ToString", new Type[] { }));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.LeftShiftAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }


        //Pass a constant to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 33", new string[] { "negative", "LeftShiftAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr LeftShiftAssign33(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Constant(1);
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.LeftShiftAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        // pass a MethodInfo that takes three arguments
        public static int LeftShiftAssign3Args(int arg1, int arg2, int arg3) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 34", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr LeftShiftAssign34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(LeftShiftAssign).GetMethod("LeftShiftAssign3Args");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.LeftShiftAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }

        // pass a MethodInfo with two arguments, that returns void
        public static void LeftShiftAssign2ArgsVoid(int arg1, int arg2) {
            return;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 35", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr LeftShiftAssign35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(LeftShiftAssign).GetMethod("LeftShiftAssign2ArgsVoid");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.LeftShiftAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }

        //Add with an instance field as the left argument
        public class C44 {
            public int Val;
            public static FieldInfo Field = typeof(C44).GetField("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 44", new string[] { "positive", "LeftShiftAssign", "operators", "Pri2" })]
        public static Expr LeftShiftAssign44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C44), "instance");

            Expr Left = Expr.Field(Instance, C44.Field);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C44())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.LeftShiftAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(64), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with a static field as the left argument
        public class C45 {
            public static int Val;
            public static FieldInfo Field = typeof(C45).GetField("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 45", new string[] { "positive", "LeftShiftAssign", "operators", "Pri2" })]
        public static Expr LeftShiftAssign45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Field(null, C45.Field);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.LeftShiftAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(64), Left));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with an instance Property as the left argument
        public class C46 {
            public int Val { get; set; }
            public static PropertyInfo Property = typeof(C46).GetProperty("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 46", new string[] { "positive", "LeftShiftAssign", "operators", "Pri2" })]
        public static Expr LeftShiftAssign46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C46), "instance");

            Expr Left = Expr.Property(Instance, C46.Property);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C46())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.LeftShiftAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(64), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with a static Property as the left argument
        public class C47 {
            public static int Val { get; set; }
            public static PropertyInfo Property = typeof(C47).GetProperty("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 47", new string[] { "positive", "LeftShiftAssign", "operators", "Pri2" })]
        public static Expr LeftShiftAssign47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Property(null, C47.Property);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(Expr.LeftShiftAssign(Left, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(64), Left));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 48", new string[] { "positive", "LeftShiftAssign", "operators", "Pri2" })]
        public static Expr LeftShiftAssign48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C48), "instance");

            Expr Left = Expr.Property(Instance, C48.Property, new[] { Expr.Constant(2) });

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C48())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(3)));

            Expressions.Add(Expr.LeftShiftAssign(Left, Expr.Constant(2)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(32), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static int LeftShiftAssignConv(short arg1, short arg2) {
            return (int)(arg1 + arg2);
        }
        //LeftShiftAssign with a lambda conversion for the return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 49_1", new string[] { "positive", "LeftShiftAssign", "operators", "Pri1" })]
        public static Expr LeftShiftAssign49_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(short)), arg1
                                                );
            MethodInfo mi = typeof(LeftShiftAssign).GetMethod("LeftShiftAssignConv");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((short)1)));
            Expressions.Add(
                            EU.GenAreEqual(
                                            Expr.Constant((short)3),
                                            Expr.LeftShiftAssign(Left, Expr.Constant((short)2), mi, Conv)
                                           )
                           );

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //LeftShiftAssign with a lambda conversion for the return type, to the wrong type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 50", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LeftShiftAssign50(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(byte)), arg1
                                                );
            MethodInfo mi = typeof(LeftShiftAssign).GetMethod("LeftShiftAssignConv");

            EU.Throws<System.InvalidOperationException>(() => { Expr.LeftShiftAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }

        //Passing null to conversion lambda
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 51", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr LeftShiftAssign51(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            LambdaExpression Conv = null;
            MethodInfo mi = typeof(LeftShiftAssign).GetMethod("LeftShiftAssignConv");

            EU.Throws<System.ArgumentException>(() => { Expr.LeftShiftAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }


        //Passing a conversion lambda when it's not needed
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LeftShiftAssign 52", new string[] { "negative", "LeftShiftAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr LeftShiftAssign52(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(int)), arg1
                                                );

            MethodInfo mi = null;// typeof(LeftShiftAssign).GetMethod("LeftShiftAssignInts");

            EU.Throws<System.InvalidOperationException>(() => { Expr.LeftShiftAssign(Left, Expr.Constant(2), mi, Conv); });
            
            return Expr.Empty();
        }
    }
}


