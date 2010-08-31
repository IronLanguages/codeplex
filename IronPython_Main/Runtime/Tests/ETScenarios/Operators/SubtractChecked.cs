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

    public class SubtractChecked {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

        #region Helper functions and inner classes


        // Use to generate generic helper SubtractChecked statements that take custome methods
        private static Expr TestSubtractCheckedExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, string testMsg) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.SubtractChecked(Expr.Constant(LeftVal, typeof(T)),
                                                           Expr.Constant(RightVal, typeof(T))),
                                           testMsg));

            return EU.BlockVoid(Expressions);
        }


        #endregion


        // Test     : SubtractChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_uint", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_unint(EU.IValidator V) {
            var tree = TestSubtractCheckedExprBlockForType<uint>(1, 1, 0, "Tests for uint");
            V.Validate(tree);
            return tree;
        }

        // Test     : SubtractChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_Decimal", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_Decimal(EU.IValidator V) {
            var tree = TestSubtractCheckedExprBlockForType<Decimal>(1, 1, 0, "Tests for Decimal");
            V.Validate(tree);
            return tree;
        }

        // Test     : SubtractChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_ulong", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_ulong(EU.IValidator V) {
            var tree = TestSubtractCheckedExprBlockForType<ulong>(1, 1, 0, "Tests for ulong");
            V.Validate(tree);
            return tree;
        }

        // Test     : SubtractChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_short", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_short(EU.IValidator V) {
            var tree = TestSubtractCheckedExprBlockForType<short>(1, 1, 0, "Test for short type");
            V.Validate(tree);
            return tree;
        }
        // Test     : SubtractChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_ushort", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_ushort(EU.IValidator V) {
            var tree = TestSubtractCheckedExprBlockForType<ushort>(1, 1, 0, "Tests for ushort");
            V.Validate(tree);
            return tree;
        }

        // Test     : SubtractChecked of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_long", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_long(EU.IValidator V) {
            var tree = TestSubtractCheckedExprBlockForType<long>(1, 1, 0, "Testing SubtractChecked for long types");
            V.Validate(tree);
            return tree;
        }
        // Test     : SubtractChecked of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_double", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_double(EU.IValidator V) {
            var tree = TestSubtractCheckedExprBlockForType<double>(1, 1, 0, "Testing SubtractChecked for double types");
            V.Validate(tree);
            return tree;
        }

        // Test     : SubtractChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_byte", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_byte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestSubtractCheckedExprBlockForType<byte>(1, 1, 0, "Testing SubtractChecked for byte types");
            });

            return tree;
        }

        // Test     : SubtractChecked of byte, sbyte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_sbyte", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_sbyte(EU.IValidator V) {
            var tree =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                TestSubtractCheckedExprBlockForType<sbyte>(1, 1, 0, "Testing SubtractChecked for sbyte types");
            });

            return tree;
        }

        // Test     : SubtractChecked of byte, sbyte, Single, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_Single", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_Single(EU.IValidator V) {
            var tree = TestSubtractCheckedExprBlockForType<Single>(1, 1, 0, "Testing SubtractChecked for Single types");
            V.Validate(tree);
            return tree;

        }

        // SubtractChecked of Boolean constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked Boolean Constant Test", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.SubtractChecked(Expr.Constant(1, typeof(Boolean)), Expr.Constant(1, typeof(Boolean)));
            }));

            return EU.BlockVoid(Expressions);
        }

        // SubtractChecked of String constant
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked String Constant Test", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_String(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.SubtractChecked(Expr.Constant("1", typeof(string)), Expr.Constant("1", typeof(string)));
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

        // SubtractChecked of class object, no user defined operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked Class Object Test", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_Object(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.SubtractChecked(Expr.Constant(new UserDefinedObject(1)), Expr.Constant(new UserDefinedObject(1))); }));

            return Expr.Empty();
        }

        // SubtractChecked of struct object, no user defined operator
        internal struct TestStruct {
            int _x;
            internal TestStruct(int val) { _x = val; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked 8", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr SubtractChecked8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { EU.GenAreEqual(Expr.SubtractChecked(Expr.Constant(new TestStruct(1)), Expr.Constant(new TestStruct(2))), Expr.Constant(new TestStruct(-1)), "SubtractChecked 1"); }));

            return Expr.Empty();
        }

        // Test     : SubtractCheckeding across mixed types
        // Expected : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked Across Mixed Types", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MixedTypes(EU.IValidator V) {

            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() => { Expr.SubtractChecked(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(byte))); }));

            return Expr.Empty();
        }
        /// <summary>
        /// Test     : Pass null to method, normal arguments to left and right
        /// Expected : Same as SubtractChecked(left, right)
        /// </summary>
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked Null MethodInfo", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(0), Expr.SubtractChecked(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), (MethodInfo)null));
            V.Validate(tree);
            return tree;
        }

        // Test     : Pass null to method, normal arguments to left and right - with annotations
        // Expected : Same as SubtractChecked(left, right) 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked Null MethodInfo Annotations", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_Null_MethodInfo_Annotations(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(0), Expr.SubtractChecked(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), (MethodInfo)null), "Test SubtractChecked for Null Method Info Ann");
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked MethodInfo With No Args", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args(EU.IValidator V) {
            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;
            return EU.Throws<System.ArgumentException>(() => { Expr.SubtractChecked(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), TakesNoArgs); });
        }

        // Pass a methodinfo that takes no arguments 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked MethodInfo With No Args Ann", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_With_No_Args_Ann(EU.IValidator V) {

            List<Expression> Expressions = new List<Expression>();
            MethodInfo TakesNoArgs = new Func<int>(GetOne).Method;

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.SubtractChecked(Expr.Constant(1, typeof(int)), Expr.Constant(1, typeof(int)), TakesNoArgs);
            }));

            return EU.BlockVoid(Expressions);

        }

        // Pass a method info that takes a paramarray
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractCheckedMethodInfo_That_Takes_Paramarray", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Test_MethodInfo_That_Takes_Paramarray(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParmsParameter par = new ParmsParameter(MethodWithParamsArgs);
            MethodInfo fn = par.Method;
            // The MethodInfo override the function.
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(0, typeof(int)),
                    Expr.SubtractChecked(Expr.Constant(2, typeof(int)),
                    Expr.Constant(1, typeof(int)), fn),
                "Pass a method info that takes a paramarray");
            }));

            return Expr.Empty();
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked With Values Widen To Args Method", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Values_Widen_To_Args_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Create a fn that's parameters are wider then the calling args.
            MethodInfo fn = new Func<long, long, int>((long a, long b) => { return 1; }).Method;

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.SubtractChecked(Expr.Constant(2, typeof(int)), Expr.Constant(1, typeof(int)), fn);
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
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked With Non Nullable and Nullable Args Types To Method", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_With_Non_Nullable_Nullable_Args_To_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int, int?, int>(NonNullableAndNullableMethod).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int)),
                                           Expr.SubtractChecked(Expr.Constant(2, typeof(int)),
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "SubtractChecked With Non Nullable and Nullable Args Types To Method"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        // Test     : With a null methodinfo, pass a value of non nullable type, the other of nullable type.
        // Expected : Exception.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked Null MethodInfo Pass Non Nullable Type And Nullable Type", new string[] { "negative", "SubtractChecked", "operations", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Test_With_Non_Nullable_And_Nullable_Args_To_Null_Method(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.SubtractChecked(Expr.Constant(2, typeof(int)), Expr.Constant(null, typeof(int?)), null);
            }));

            var tree = EU.BlockVoid(Expressions);

            return tree;
        }


        // helper function for: "With a valid methodInfo that returns boolean, pass arguments of nullable type."
        public static Boolean SubtractChecked_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate(int? a, int? b) {
            return true;
        }

        // With a valid methodInfo that returns boolean, pass arguments of nullable type.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            MethodInfo fn = new Func<int?, int?, Boolean>(SubtractChecked_Valid_MethodInfo_Pass_Nullable_Args_Return_Boolean_Delegate).Method;

            Expressions.Add(EU.GenAreEqual(Expr.Constant((Boolean)true, typeof(Boolean)),
                                           Expr.SubtractChecked(Expr.Constant(null, typeof(int?)),
                                                           Expr.Constant(null, typeof(int?)), fn),
                                           "SubtractChecked with a valid MethodInfo that returns boolean, passing arg of nullable type"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use to generate generic helper SubtractChecked statements that take custome methods
        private static Expr TestSubtractCheckedExprBlockForType<T>(T LeftVal, T RightVal, T ExpectedVal, MethodInfo fn, string testMsg) {
            List<Expression> Expressions = new List<Expression>();


            Expressions.Add(EU.GenAreEqual(Expr.Constant(ExpectedVal, typeof(T)),
                                           Expr.SubtractChecked(Expr.Constant(LeftVal, typeof(T)),
                                                           Expr.Constant(RightVal, typeof(T)), fn),
                                           testMsg));

            return EU.BlockVoid(Expressions);
        }

        // Dynamically pick the a method with a type helper functions
        public static T TwoParamMethodRetFirstArg<T>(T a, T b) { return a; }
        public static MethodInfo GetExpressionMethodInfo<T>() {
            return new Func<T, T, T>(TwoParamMethodRetFirstArg<T>).Method;
        }

        // Test     : SubtractChecked of sbyte, byte, short, ushort, int, uint,long,ulong, single, double, decimal constants
        // Expected : Appropriate values
        // Note     : Didn't need to do this one.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_Across_Multiple_Types", new string[] { "positive", "SubtractChecked", "operators", "Pri3" })]
        public static Expr Test_Across_All_Multiple_Types(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(TestSubtractCheckedExprBlockForType<int>(1, 1, 1, GetExpressionMethodInfo<int>(), "SubtractChecked with MethodInfo test for int"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<uint>(1, 1, 1, GetExpressionMethodInfo<uint>(), "SubtractChecked with MethodInfo test for uint"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<long>(1, 1, 1, GetExpressionMethodInfo<long>(), "SubtractChecked with MethodInfo test for long"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<ulong>(1, 1, 1, GetExpressionMethodInfo<ulong>(), "SubtractChecked with MethodInfo test for ulong"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<double>(1, 1, 1, GetExpressionMethodInfo<double>(), "SubtractChecked with MethodInfo test for double"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<float>(1, 1, 1, GetExpressionMethodInfo<float>(), "SubtractChecked with MethodInfo test for float"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<short>(1, 1, 1, GetExpressionMethodInfo<short>(), "SubtractChecked with MethodInfo test for short"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<ushort>(1, 1, 1, GetExpressionMethodInfo<ushort>(), "SubtractChecked with MethodInfo test for ushort"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<byte>(1, 1, 1, GetExpressionMethodInfo<byte>(), "SubtractChecked with MethodInfo test for byte"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<sbyte>(1, 1, 1, GetExpressionMethodInfo<sbyte>(), "SubtractChecked with MethodInfo test for sbyte"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<Single>(1, 1, 1, GetExpressionMethodInfo<Single>(), "SubtractChecked with MethodInfo test for Single"));
            Expressions.Add(TestSubtractCheckedExprBlockForType<Decimal>(1, 1, 1, GetExpressionMethodInfo<Decimal>(), "SubtractChecked with MethodInfo test for Decimal"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;

        }



        /// <summary>
        /// Used for test : Test_Overloaded_Operator
        /// Note          : Make subtraction return the wrong value in order to valide operator is called
        /// </summary>
        public class OverloadedOperator {
            public int Val { get; set; }
            public OverloadedOperator(int a) {
                Val = a;
            }
            public static bool operator ==(OverloadedOperator x, OverloadedOperator y) {
                return (x.Val == y.Val);
            }
            public static bool operator !=(OverloadedOperator x, OverloadedOperator y) {
                return ((x.Val != y.Val));
            }
            public static bool operator ==(OverloadedOperator x, int y) {
                return (x.Val == y);
            }
            public static bool operator !=(OverloadedOperator x, int y) {
                return ((x.Val != y));
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }

            public static OverloadedOperator operator -(OverloadedOperator a, OverloadedOperator b) {

                return new OverloadedOperator(b.Val + a.Val);
            }
            public static int operator -(OverloadedOperator a, int b) {

                return (b + a.Val);
            }

            public static OverloadedOperator Sub(OverloadedOperator a, OverloadedOperator b) {

                return new OverloadedOperator(b.Val + a.Val);
            }


        }

        // User defined overloaded operator on left argument, arguments are the proper types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_Overloaded_Operator_On_Left_Arg", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_Overloaded_Operator_On_Left_Arg(EU.IValidator V) {
            OverloadedOperator Left = new OverloadedOperator(1);
            OverloadedOperator Right = new OverloadedOperator(1);
            OverloadedOperator Res = new OverloadedOperator(2);

            return EU.BlockVoid(EU.GenAreEqual(Expr.Constant(Res, typeof(OverloadedOperator)),
                                             Expr.SubtractChecked(Expr.Constant(Left, typeof(OverloadedOperator)),
                                             Expr.Constant(Right, typeof(OverloadedOperator))), "Test SubtractChecked override opp"));
        }

        // With a valid method info, pass two values of a derived class of the methodinfo’s arguments
        public class MyDerivedType : OverloadedOperator {
            public MyDerivedType(int a) : base(a) { }
            public int DerivedData { get; set; }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked 14", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr SubtractChecked14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(MyDerivedType).GetMethod("Sub");

            MyDerivedType right = new MyDerivedType(1);
            MyDerivedType left = new MyDerivedType(1);
            MyDerivedType expected = new MyDerivedType(2);

            Expr Res =
                Expr.SubtractChecked(
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



        // Test     :  Verify order of evaluation of expressions on SubtractChecked
        // Expected :  Verify res = "_first arg_second arg", and intresult = 3
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked_Order_Of_Operations_Verification", new string[] { "positive", "SubtractChecked", "operators", "Pri1" })]
        public static Expr Test_Order_Of_Operations_Verification(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var res = Expr.Variable(typeof(string), "x");
            var intresult = Expr.Variable(typeof(int), "y");

            var LeftArg = Expr.Block(EU.ConcatEquals(res, "_first arg"), Expr.Constant(7));
            var RightArg = Expr.Block(EU.ConcatEquals(res, "_second arg"), Expr.Constant(1));
            var TmpExps = LeftArg.Expressions;


            Expressions.Add(Expr.Assign(intresult, Expr.SubtractChecked(LeftArg, RightArg)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), intresult, "Order of operations validate intresult=6"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("_first arg_second arg"), res, "Order of operations validate res='_first arg_second arg'"));

            var tree = EU.BlockVoid(new[] { res, intresult }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Test    : SubtractChecked of values that overflow the type
        //Expected: Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked 18", new string[] { "negative", "SubtractChecked", "operators", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr SubtractChecked18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(Int32.MinValue + 1),
                    Expr.SubtractChecked(
                        Expr.Constant(Int32.MaxValue),
                        Expr.Constant(-2)

                    ),
                    "SubtractChecked Overflow the type 1"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant(Int32.MaxValue),
                    Expr.SubtractChecked(
                        Expr.Constant(-1),
                        Expr.Constant(Int32.MinValue)
                    ),
                    "SubtractChecked Overflow the type 2"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant((double)(Double.PositiveInfinity)),
                    Expr.SubtractChecked(
                        Expr.Constant((double)(Double.MaxValue)),
                        Expr.Constant((double)(-Double.MaxValue))
                    ),
                    "SubtractChecked Overflow the type 3"
                )
            );

            Expressions.Add(
                EU.GenAreEqual(
                    Expr.Constant((double)(Double.NegativeInfinity)),
                    Expr.SubtractChecked(
                        Expr.Constant((double)(Double.MinValue)),
                        Expr.Constant((double)(-Double.MinValue))
                    ),
                    "SubtractChecked Overflow the type 4"
                )
            );

            var tree = EU.BlockVoid(Expressions);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }
    }
}


