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

    public class PowerAssign {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

        #region Helper functions and inner classes


        // Use to generate generic helper PowerAssign statements that take custome methods
        private static Expr TestPowerAssignExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, string testMsg) {
            List<Expression> Expressions = new List<Expression>();

            Expression expected = Expr.Constant(ExpectedVal, typeof(T));
            ParameterExpression Left = Expr.Parameter(typeof(T), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(LeftVal, typeof(T))));
            Expression Right = Expr.Constant(RightVal, typeof(T));
            Expression OpToTest = Expr.PowerAssign(Left, Right);

            Expression Ex = EU.GenAreEqual(expected, OpToTest, testMsg);
            Expressions.Add(Ex);

            //Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
            //                               Expr.PowerAssign(Expr.Constant(LeftVal, typeof(T)),
            //                                               Expr.Constant(RightVal, typeof(T))),
            //                               testMsg));

            return Expr.Block(new[] { Left }, Expressions);
        }


        #endregion


        // Test     : PowerAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_uint", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_unint(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestPowerAssignExprBlockForType<uint>(1, 1, 1, "Tests for uint");
            });

            return tree;
        }

        // Test     : PowerAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_Decimal", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Decimal(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestPowerAssignExprBlockForType<Decimal>(1, 1, 1, "Tests for Decimal");
            });

            return tree;
        }

        // Test     : PowerAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_ulong", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_ulong(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestPowerAssignExprBlockForType<ulong>(1, 1, 1, "Tests for ulong");
            });

            return tree;
        }

        // Test     : PowerAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_short", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_short(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestPowerAssignExprBlockForType<short>(1, 1, 1, "Test for short type");
            });

            return tree;
        }
        // Test     : PowerAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_ushort", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_ushort(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestPowerAssignExprBlockForType<ushort>(1, 1, 1, "Tests for ushort");
            });

            return tree;
        }

        // Test     : PowerAssign of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_long", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_long(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestPowerAssignExprBlockForType<long>(1, 1, 1, "Testing PowerAssign for long types");
            });

            return tree;
        }
        // Test     : PowerAssign of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_double", new string[] { "positive", "PowerAssign", "operators", "Pri1" })]
        public static Expr Test_double(EU.IValidator V) {
            var tree = TestPowerAssignExprBlockForType<double>(1, 1, 1, "Testing PowerAssign for double types");
            V.Validate(tree);
            return tree;
        }

        // Test     : PowerAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_byte", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_byte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestPowerAssignExprBlockForType<byte>(1, 1, 1, "Testing PowerAssign for byte types");
            });

            return tree;
        }

        // Test     : PowerAssign of byte, sbyte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_sbyte", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_sbyte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestPowerAssignExprBlockForType<sbyte>(1, 1, 1, "Testing PowerAssign for sbyte types");
            });

            return tree;
        }

        // Test     : PowerAssign of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_Single", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Single(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestPowerAssignExprBlockForType<Single>(1, 1, 1, "Testing PowerAssign for Single types");
            });

            return tree;

        }

        // PowerAssign of Boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign Boolean Constant Test", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.PowerAssign(Expr.Constant(1, typeof(Boolean)), Expr.Constant(1, typeof(Boolean)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // PowerAssign of String constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign String Constant Test", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_String(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(string), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant("1", typeof(string))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant("1", typeof(string)));
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        /// <summary>
        /// Used for : Test_Object
        /// </summary>
        public class UserDefinedObject {
            public int _value = 0;
            public UserDefinedObject() { }
            public UserDefinedObject(int n) {
                _value = n;
            }
        }

        // PowerAssign of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign Class Object Test", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Object(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = EU.Param<UserDefinedObject>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new UserDefinedObject(1))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(new UserDefinedObject(1)));
            }));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }

        // PowerAssign of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 8", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr PowerAssign8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = EU.Param<TestStruct>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new TestStruct(1))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.PowerAssign(Left, Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "PowerAssign 1"); }));

            return Expr.Empty();
        }

        // Test     : PowerAssigning across mixed types
        // Expected : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign Across Mixed Types", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MixedTypes(EU.IValidator V) {

            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() => { Expr.PowerAssign(Left, Expr.Constant(1, typeof(byte))); }));

            return Expr.Empty();
        }

        /// <summary>
        /// Test     : Pass null to method, normal arguments to left and right
        /// Expected : Same as PowerAssign(left, right)
        /// </summary>
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign Null MethodInfo", new string[] { "positive", "PowerAssign", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<double>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2.0)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(4.0),
                              Expr.PowerAssign(Left, Expr.Constant(2.0), (MethodInfo)null)));

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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign MethodInfo With No Args", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.PowerAssign(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), TakesNoArgs));
            });

            return Expr.Block(new[] { Left }, Expressions);
        }

        // Pass a methodinfo that takes no arguments 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign MethodInfo With No Args Ann", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args_Ann(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));

            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(1, typeof(int)), TakesNoArgs);
            }));

            return Expr.Block(new[] { Left }, Expressions);

        }

        // Pass a method info that takes a paramarray
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssignMethodInfo_That_Takes_Paramarray", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_That_Takes_Paramarray(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(0)));

            ParmsParameter par = new ParmsParameter(MethodWithParamsArgs);
            MethodInfo fn = par.Method;
            // The MethodInfo override the function.
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Left,
                    Expr.PowerAssign(Expr.Constant(2, typeof(int)),
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign With Values Widen To Args Method", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Values_Widen_To_Args_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            // Create a fn that's parameters are wider then the calling args.
            MethodInfo fn = new Func<long, long, int>((long a, long b) => { return 1; }).Method;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.PowerAssign(Left, Expr.Constant(1, typeof(int)), fn); }));

            return Expr.Empty();
        }


        /// <summary>
        /// Also use for other tests. Could reuse a lot of these.
        /// </summary>
        public static int NonNullableAndNullableMethod(int a, int? b) {
            return 1;
        }


        // With a valid methodinfo, pass a value of non nullable type, the other of nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign With Non Nullable and Nullable Args Types To Method", new string[] { "positive", "PowerAssign", "operators", "Pri1" })]
        public static Expr Test_With_Non_Nullable_Nullable_Args_To_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            MethodInfo fn = new Func<int, int?, int>(NonNullableAndNullableMethod).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int)),
                                           Expr.PowerAssign(Left,
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "PowerAssign With Non Nullable and Nullable Args Types To Method"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }


        // Test     : With a null methodinfo, pass a value of non nullable type, the other of nullable type.
        // Expected : Exception.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign Null MethodInfo Pass Non Nullable Type And Nullable Type", new string[] { "negative", "PowerAssign", "operations", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Non_Nullable_And_Nullable_Args_To_Null_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(null, typeof(int?)), null);
            }));


            var tree = EU.BlockVoid(new[] { Left }, Expressions);

            return tree;
        }


        // helper function for: "With a valid methodInfo that returns boolean, pass arguments of nullable type."
        public static int? PowerAssign_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate(int? a, int? b) {
            return 1;
        }

        // With a valid methodInfo that returns boolean, pass arguments of nullable type.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean", new string[] { "positive", "PowerAssign", "operators", "Pri1" })]
        public static Expr Test_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int?, int?, int?>(PowerAssign_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate).Method;
            ParameterExpression Left = EU.Param<int?>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(null, typeof(int?))));


            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int?)),
                                           Expr.PowerAssign(Left,
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "PowerAssign with a valid MethodInfo that returns boolean, passing arg of nullable type"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use to generate generic helper PowerAssign statements that take custome methods
        private static Expr TestPowerAssignExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, MethodInfo fn, string testMsg) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<T>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(LeftVal, typeof(T))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.PowerAssign(Left,
                                                           Expr.Constant(RightVal, typeof(T)), fn),
                                           testMsg));

            return Expr.Block(new[] { Left }, Expressions);
        }

        // Dynamically pick the a method with a type helper functions
        public static T TwoParamMethodRetFirstArg<T>(T a, T b) { return a; }
        public static MethodInfo GetExpressionMethodInfo<T>() {
            return new Func<T, T, T>(TwoParamMethodRetFirstArg<T>).Method;
        }

        // Test     : PowerAssign of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        // Note     : Didn't need to do this one.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_Across_Multiple_Types", new string[] { "positive", "PowerAssign", "operators", "Pri3" })]
        public static Expr Test_Across_All_Multiple_Types(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(TestPowerAssignExprBlockForType<int>(1, 1, 1, GetExpressionMethodInfo<int>(), "PowerAssign with MethodInfo test for int"));
            Expressions.Add(TestPowerAssignExprBlockForType<uint>(1, 1, 1, GetExpressionMethodInfo<uint>(), "PowerAssign with MethodInfo test for uint"));
            Expressions.Add(TestPowerAssignExprBlockForType<long>(1, 1, 1, GetExpressionMethodInfo<long>(), "PowerAssign with MethodInfo test for long"));
            Expressions.Add(TestPowerAssignExprBlockForType<ulong>(1, 1, 1, GetExpressionMethodInfo<ulong>(), "PowerAssign with MethodInfo test for ulong"));
            Expressions.Add(TestPowerAssignExprBlockForType<double>(1, 1, 1, GetExpressionMethodInfo<double>(), "PowerAssign with MethodInfo test for double"));
            Expressions.Add(TestPowerAssignExprBlockForType<float>(1, 1, 1, GetExpressionMethodInfo<float>(), "PowerAssign with MethodInfo test for float"));
            Expressions.Add(TestPowerAssignExprBlockForType<short>(1, 1, 1, GetExpressionMethodInfo<short>(), "PowerAssign with MethodInfo test for short"));
            Expressions.Add(TestPowerAssignExprBlockForType<ushort>(1, 1, 1, GetExpressionMethodInfo<ushort>(), "PowerAssign with MethodInfo test for ushort"));
            Expressions.Add(TestPowerAssignExprBlockForType<byte>(1, 1, 1, GetExpressionMethodInfo<byte>(), "PowerAssign with MethodInfo test for byte"));
            Expressions.Add(TestPowerAssignExprBlockForType<sbyte>(1, 1, 1, GetExpressionMethodInfo<sbyte>(), "PowerAssign with MethodInfo test for sbyte"));
            Expressions.Add(TestPowerAssignExprBlockForType<Single>(1, 1, 1, GetExpressionMethodInfo<Single>(), "PowerAssign with MethodInfo test for Single"));
            Expressions.Add(TestPowerAssignExprBlockForType<Decimal>(1, 1, 1, GetExpressionMethodInfo<Decimal>(), "PowerAssign with MethodInfo test for Decimal"));

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
            [System.Runtime.CompilerServices.SpecialName]
            public static OverloadedOperator op_Exponent(OverloadedOperator a, int b) {

                return new OverloadedOperator(b * a.Val);
            }
            public static OverloadedOperator PowerAssign(OverloadedOperator a, OverloadedOperator b) {
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_Overloaded_Operator_On_Left_Arg", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Overloaded_Operator_On_Left_Arg(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<OverloadedOperator>();

            OverloadedOperator LeftArg = new OverloadedOperator(2);
            Expressions.Add(Expr.Assign(Left, Expr.Constant(LeftArg, typeof(OverloadedOperator))));

            Expr Res =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(3, typeof(int)));
            });

            return Expr.Empty();
        }

        // With a valid method info, pass two values of a derived class of the methodinfo's arguments
        public class MyDerivedType : OverloadedOperator {
            public MyDerivedType(int a) : base(a) { }
            public int DerivedData { get; set; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 14", new string[] { "positive", "PowerAssign", "operators", "Pri1" })]
        public static Expr PowerAssign14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<OverloadedOperator>();


            MethodInfo mi = typeof(OverloadedOperator).GetMethod("PowerAssign");

            MyDerivedType right = new MyDerivedType(1);
            MyDerivedType left = new MyDerivedType(1);
            MyDerivedType expected = new MyDerivedType(2);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(left, typeof(OverloadedOperator))));

            Expr Res =
                Expr.PowerAssign(
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
            private double _Value;
            public double this[string append] {
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

        // Test     :  Verify order of evaluation of expressions on PowerAssign
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign_Order_Of_Operations_Verification", new string[] { "positive", "PowerAssign", "operators", "Pri1" })]
        public static Expr Test_Order_Of_Operations_Verification(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            TestOrder order = new TestOrder();

            ParameterExpression ov = Expr.Parameter(typeof(TestOrder), "order");

            Expressions.Add(Expr.Assign(ov, Expr.Constant(order)));
            Expressions.Add(Expr.Assign(Expr.Property(ov, order.pi, Expr.Constant("")), Expr.Constant(2.0)));


            Expr Res =
                Expr.PowerAssign(
                    Expr.Property(ov, order.pi, Expr.Constant("One")),
                    Expr.PowerAssign(
                        Expr.Property(ov, order.pi, Expr.Constant("Two")),
                        Expr.PowerAssign(
                            Expr.Property(ov, order.pi, Expr.Constant("Three")),
                            Expr.Constant(2.0)
                        )
                    ),
                    null
                );

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(65536.0)));
            Expressions.Add(EU.GenAreEqual(Expr.Field(ov, order.resi), Expr.Constant("OneTwoThreeThreeTwoOne")));

            var tree = Expr.Block(new[] { ov }, Expressions);
            V.Validate(tree);
            return tree;
        }





        // PowerAssign of values that overflow the type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 18", new string[] { "positive", "PowerAssign", "operators", "Pri1" })]
        public static Expr PowerAssign18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = EU.Param<int>();
            Expressions.Add(Expr.Assign(Left, Expr.Constant(2)));

            /*Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(-2),
                    Expr.PowerAssign(
                        Left,
                        Expr.Constant(Int32.MaxValue)
                    ),
                    "PowerAssign 1"
                )
            );
            Expressions.Add(Expr.Assign(Left, Expr.Constant(-2)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(0),
                    Expr.PowerAssign(
                        Left,
                        Expr.Constant(Int32.MinValue)
                    ),
                    
                    "PowerAssign 2"
                )
            );*/

            ParameterExpression Left2 = EU.Param<double>();
            Expressions.Add(Expr.Assign(Left2, Expr.Constant(double.MaxValue)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant((double)(Double.PositiveInfinity)),
                    Expr.PowerAssign(
                        Left2,
                        Expr.Constant((double)(Double.MaxValue))
                    ),

                    "PowerAssign 3"
                )
            );

            Expressions.Add(Expr.Assign(Left2, Expr.Constant((double)2)));

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant((double)(Double.PositiveInfinity)),
                    Expr.PowerAssign(
                        Left2,
                        Expr.Constant((double)(Double.MaxValue))
                    ),

                    "PowerAssign 4"
                )
            );

            var tree = Expr.Block(new[] { Left, Left2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // PowerAssign with an array index expression as the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 19", new string[] { "positive", "PowerAssign", "operators", "Pri1" })]
        public static Expr PowerAssign19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Variable(typeof(double[]), "Left");
            Expressions.Add(Expr.Assign(Left, Expr.NewArrayInit(typeof(double), new Expression[] { Expr.Constant(4.0), Expr.Constant(5.0), Expr.Constant(6.0), Expr.Constant(7.0), Expr.Constant(8.0) })));

            Expressions.Add(Expr.PowerAssign(Expr.ArrayAccess(Left, Expr.Constant(2)), Expr.Constant((double)4)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(Math.Pow(6.0, 4.0)), Expr.ArrayIndex(Left, Expr.Constant(2)), "SA 1"));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use constant for left operand
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 20", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.PowerAssign(Expr.Constant(1), Expr.Constant(4));
            }));

            return EU.BlockVoid(Expressions);
        }

        //Pass a non assignable expression to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 20_1", new string[] { "negative", "PowerAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign20_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(PowerAssign).GetMethod("PowerAssignCheckedInts");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.PowerAssign(Expr.Constant(1), Expr.Constant(2, typeof(Int32)), mi), Expr.Constant(3, typeof(Int32)));
            }));

            return Expr.Block(Expressions);
        }

        // Use comma for left operand
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 21", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.PowerAssign(Expr.Block(Expr.Constant(1)), Expr.Constant(4));
            }));

            return EU.BlockVoid(Expressions);
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 25", new string[] { "negative", "AddAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(AddAssign).GetMethod("BadReturn");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Block(new[] { Left }, Expressions);
        }


        //Pass null to left
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 26", new string[] { "negative", "PowerAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr PowerAssign26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.PowerAssign(null, Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        //Pass null to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 27", new string[] { "negative", "PowerAssign", "operators", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr PowerAssign27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.PowerAssign(Left, null);
            }));

            return Expr.Empty();
        }

        //Pass a non value returning expression to right
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 29", new string[] { "negative", "PowerAssign", "operators", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr PowerAssign29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Empty());
            }));

            var tree = Expr.Empty();

            return tree;
        }

        //Pass a Block to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 30", new string[] { "negative", "PowerAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Block(Expr.Constant(1));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.PowerAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        //Pass a Method Call to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 31", new string[] { "negative", "PowerAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Call(Expr.Constant("1"), typeof(string).GetMethod("ToString", new Type[] { }));
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.PowerAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }


        //Pass a constant to the left argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 33", new string[] { "negative", "PowerAssign", "operators", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign33(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Left = Expr.Constant(1);
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.PowerAssign(Left, Expr.Constant(1)));
            });

            return Expr.Empty();
        }

        // pass a MethodInfo that takes three arguments
        public static int PowerAssign3Args(int arg1, int arg2, int arg3) {
            return -1;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 34", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(PowerAssign).GetMethod("PowerAssign3Args");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }

        // pass a MethodInfo with two arguments, that returns void
        public static void PowerAssign2ArgsVoid(int arg1, int arg2) {
            return;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 35", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(PowerAssign).GetMethod("PowerAssign2ArgsVoid");

            ParameterExpression Left = Expr.Parameter(typeof(int), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((int)1)));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(2, typeof(Int32)), mi);
            }));

            return Expr.Empty();
        }


        //with a valid method info, pass two values of an enum class (methodinfo's arguments are of the base integer type)
        public static int PowerAssign2Args(int arg1, int arg2) {
            return arg1 + arg2;
        }
        enum e36 : int {
        }
        //enum -> int conversion not being accepted.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 36", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr PowerAssign36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(PowerAssign).GetMethod("PowerAssign2Args");

            ParameterExpression Left = Expr.Parameter(typeof(e36), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((e36)1)));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant((e36)2), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type
        public static int PowerAssignMethod37(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 37", new string[] { "positive", "PowerAssign", "operators", "Pri1" })]
        public static Expr PowerAssign37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(PowerAssign).GetMethod("PowerAssignMethod37");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(Expr.PowerAssign(Left, Expr.Constant(2, typeof(int?)), mi));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //with a valid method info, pass two values of the same nullable type
        public static int PowerAssignMethod38(int? arg1, int? arg2) {
            return (arg1 + arg2).GetValueOrDefault();
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 38", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(PowerAssign).GetMethod("PowerAssignMethod38");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(2, typeof(int?)), mi);
            }));

            return Expr.Empty();
        }

        //with a valid method info, pass two values of the same nullable type. return is nullable, arguments aren't
        public static int? PowerAssignMethod39(int arg1, int arg2) {
            return arg1 + arg2;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 39", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr PowerAssign39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(PowerAssign).GetMethod("PowerAssignMethod39");

            ParameterExpression Left = Expr.Parameter(typeof(int?), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(1, typeof(int?))));
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(2, typeof(int?)), mi);
            }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3, typeof(int?)), Left, ""));

            var tree = Expr.Empty();

            return tree;
        }






        //User defined operator on right argument, arguments are of the proper types
        public class C40 {
            public double Val = 2;

            [System.Runtime.CompilerServices.SpecialName()]
            public static double op_Exponent(double a, C40 b) {
                return b.Val + a;
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 40", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr PowerAssign40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(double), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant((double)1)));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(new C40()));
            }));

            Expressions.Add(EU.GenAreEqual(Left, Expr.Constant((double)3)));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }


        //User defined operator on left argument, right argument is convertible
        public class C41 {
            public string Val;

            public C41(string init) {
                Val = init;
            }
            [System.Runtime.CompilerServices.SpecialName]
            public static C41 op_Exponent(C41 b, Exception a) {
                return new C41(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 41", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr PowerAssign41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C41), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C41("1"))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(new ArgumentException("2")));
            }));

            Expressions.Add(EU.GenAreEqual(Expr.Field(Left, typeof(C41).GetField("Val")), Expr.Constant("12")));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }


        //User defined operator on right argument, left argument is convertible
        public class C42 {
            public string Val;

            public C42(string init) {
                Val = init;
            }
            [System.Runtime.CompilerServices.SpecialName]
            public static Exception op_Exponent(Exception a, C42 b) {
                return new Exception(b.Val + a.Message);
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 42", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr PowerAssign42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(Exception), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new Exception("1"))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(new C42("2")));
            }));

            Expressions.Add(EU.GenAreEqual(Expr.Property(Left, typeof(Exception).GetProperty("Message")), Expr.Constant("21")));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }


        //User defined operators exist for both arguments.
        public class C43 {
            public string Val;

            public C43(string init) {
                Val = init;
            }
            [System.Runtime.CompilerServices.SpecialName]
            public static C43 op_Exponent(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "A");
            }
        }
        public class C43_1 {
            public string Val;

            public C43_1(string init) {
                Val = init;
            }
            [System.Runtime.CompilerServices.SpecialName]
            public static C43 op_Exponent(C43 a, C43_1 b) {
                return new C43(a.Val + b.Val + "B");
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 43", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr PowerAssign43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(C43), "");
            Expressions.Add(Expr.Assign(Left, Expr.Constant(new C43("1"))));

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.PowerAssign(Left, Expr.Constant(new C43_1("2")));
            }));

            Expressions.Add(EU.GenAreEqual(Expr.Field(Left, typeof(C43).GetField("Val")), Expr.Constant("12A")));

            var tree = Expr.Block(new[] { Left }, Expressions);

            return tree;
        }







        //Add with an instance field as the left argument
        public class C44 {
            public double Val;
            public static FieldInfo Field = typeof(C44).GetField("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 44", new string[] { "positive", "PowerAssign", "operators", "Pri2" })]
        public static Expr PowerAssign44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C44), "instance");

            Expr Left = Expr.Field(Instance, C44.Field);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C44())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant((double)2)));

            Expressions.Add(Expr.PowerAssign(Left, Expr.Constant((double)5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)32), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with a static field as the left argument
        public class C45 {
            public static double Val;
            public static FieldInfo Field = typeof(C45).GetField("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 45", new string[] { "positive", "PowerAssign", "operators", "Pri2" })]
        public static Expr PowerAssign45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Field(null, C45.Field);

            Expressions.Add(Expr.Assign(Left, Expr.Constant((double)2)));

            Expressions.Add(Expr.PowerAssign(Left, Expr.Constant((double)5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant((double)32), Left));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        //Add with an instance Property as the left argument
        public class C46 {
            public double Val { get; set; }
            public static PropertyInfo Property = typeof(C46).GetProperty("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 46", new string[] { "positive", "PowerAssign", "operators", "Pri2" })]
        public static Expr PowerAssign46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C46), "instance");

            Expr Left = Expr.Property(Instance, C46.Property);

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C46())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant((double)2)));

            Expressions.Add(Expr.PowerAssign(Left, Expr.Constant((double)5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(32.0), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Add with a static Property as the left argument
        public class C47 {
            public static double Val { get; set; }
            public static PropertyInfo Property = typeof(C47).GetProperty("Val");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 47", new string[] { "positive", "PowerAssign", "operators", "Pri2" })]
        public static Expr PowerAssign47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Left = Expr.Property(null, C47.Property);

            Expressions.Add(Expr.Assign(Left, Expr.Constant((double)2)));

            Expressions.Add(Expr.PowerAssign(Left, Expr.Constant((double)5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(32.0), Left));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }




        //Add with a parameterized instance Property as the left argument
        public class C48 {
            private double m_this;
            public double this[double x] {
                get {
                    return m_this + x;
                }
                set {
                    m_this = value + x;
                }
            }
            public static PropertyInfo Property = typeof(C48).GetProperty("Item");
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 48", new string[] { "positive", "PowerAssign", "operators", "Pri2" })]
        public static Expr PowerAssign48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Instance = Expr.Parameter(typeof(C48), "instance");

            Expr Left = Expr.Property(Instance, C48.Property, new[] { Expr.Constant(0.0) });

            Expressions.Add(Expr.Assign(Instance, Expr.Constant(new C48())));

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2.0)));

            Expressions.Add(Expr.PowerAssign(Left, Expr.Constant(5.0)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(32.0), Left));

            var tree = Expr.Block(new[] { Instance }, Expressions);
            V.Validate(tree);
            return tree;
        }


        //Use same variable for both arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 50", new string[] { "positive", "PowerAssign", "operators", "Pri2" })]
        public static Expr PowerAssign50(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(double), "Left");

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2.0)));

            Expressions.Add(Expr.PowerAssign(Left, Left));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(4.0), Left));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Variable A, do A * A * A
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 51", new string[] { "positive", "PowerAssign", "operators", "Pri2" })]
        public static Expr PowerAssign51(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Parameter(typeof(double), "Left");

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2.0)));

            Expressions.Add(Expr.PowerAssign(Left, Expr.PowerAssign(Left, Left)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(16.0), Left));

            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }




        public static int PowerAssignConv(short arg1, short arg2) {
            return (int)(arg1 + arg2);
        }
        //PowerAssign with a lambda conversion for the return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 49_1", new string[] { "positive", "PowerAssign", "operators", "Pri1" })]
        public static Expr PowerAssign49_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(short)), arg1
                                                );
            MethodInfo mi = typeof(PowerAssign).GetMethod("PowerAssignConv");

            Expressions.Add(Expr.Assign(Left, Expr.Constant((short)1)));
            Expressions.Add(
                            EU.GenAreEqual(
                                            Expr.Constant((short)3),
                                            Expr.PowerAssign(Left, Expr.Constant((short)2), mi, Conv)
                                           )
                           );


            var tree = Expr.Block(new[] { Left }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //PowerAssign with a lambda conversion for the return type, to the wrong type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 50_1", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr PowerAssign50_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(byte)), arg1
                                                );
            MethodInfo mi = typeof(PowerAssign).GetMethod("PowerAssignConv");

            EU.Throws<System.InvalidOperationException>(() => { Expr.PowerAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }

        //Passing null to conversion lambda
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 51_1", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr PowerAssign51_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(short), "Left");

            LambdaExpression Conv = null;
            MethodInfo mi = typeof(PowerAssign).GetMethod("PowerAssignConv");

            EU.Throws<System.ArgumentException>(() => { Expr.PowerAssign(Left, Expr.Constant((short)2), mi, Conv); });

            return Expr.Empty();
        }


        //Passing a conversion lambda when it's not needed
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "PowerAssign 52_1", new string[] { "negative", "PowerAssign", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr PowerAssign52_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Left = Expr.Parameter(typeof(int), "Left");

            ParameterExpression arg1 = Expr.Parameter(typeof(int), "arg1");
            LambdaExpression Conv = Expr.Lambda(
                                                    Expr.Convert(arg1, typeof(int)), arg1
                                                );

            MethodInfo mi = null;// typeof(PowerAssign).GetMethod("PowerAssignInts");


            EU.Throws<System.InvalidOperationException>(() => { Expr.PowerAssign(Left, Expr.Constant(2), mi, Conv); });

            return Expr.Empty();
        }


    }
}
