#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.AssignRead {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class ArrayAccess {
        // Single dimension array, pass one valid index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 1", new string[] { "positive", "arrayaccess", "assignread", "Pri1" })]
        public static Expr ArrayAccess1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });

            Expr Result = Expr.ArrayAccess(arr, new List<Expression> { Expr.Constant(0) });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Result, "ArrayAccess 1"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Multi dimensional array, with more indexes than it takes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 2", new string[] { "negative", "arrayaccess", "assignread", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arg1 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            NewArrayExpression arg2 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6) });

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int[]), new Expression[] { arg1, arg2 });

            Expr Result =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ArrayAccess(arr, new List<Expression>() { Expr.Constant(1), Expr.Constant(2), Expr.Constant(1) });
            });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Result, "ArrayAccess 1"));

            return Expr.Empty();
        }

        // Single dimension array, pass one valid index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 3", new string[] { "positive", "arrayaccess", "assignread", "Pri1" })]
        public static Expr ArrayAccess3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });

            Expr Result = Expr.ArrayAccess(arr, new Expression[] { Expr.Constant(0) });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Result, "ArrayAccess 1"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Multi dimensional array, with more indexes than it takes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 4", new string[] { "negative", "arrayaccess", "assignread", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arg1 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            NewArrayExpression arg2 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6) });

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int[]), new Expression[] { arg1, arg2 });

            Expr Result =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ArrayAccess(arr, new List<Expression>() { Expr.Constant(0), Expr.Constant(1), Expr.Constant(2) });
            });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Result, "ArrayAccess 1"));

            return Expr.Empty();
        }

        // Indexing into null array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 5", new string[] { "negative", "arrayaccess", "assignread", "Pri1" }, Exception = typeof(IndexOutOfRangeException))]
        public static Expr ArrayAccess5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new Expression[] { });

            Expr Result = Expr.ArrayAccess(arr, new Expression[] { Expr.Constant(0) });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Result, "ArrayAccess 1"));

            var tree = Expr.Block(Expressions);
            V.ValidateException<IndexOutOfRangeException>(tree, false);
            return tree;
        }

        // Null for array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 6", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr ArrayAccess6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Result =
            EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.ArrayAccess(null, new List<Expression>() { Expr.Constant(0) });
            });
            return Expr.Empty();
        }

        // Null for indexes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 7", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new List<Expression>() { });
            var Result =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ArrayAccess(arr, null);
            });

            return Expr.Empty();
        }

        // Null elements for indexes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 8", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr ArrayAccess8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new Expression[] { });
            var Result = EU.Throws<ArgumentNullException>(() => { Expr.ArrayAccess(arr, new List<Expression>() { null }); });

            return Expr.Empty();
        }

        // Single dimension array, pass no indexes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 9", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new Expression[] { });
            var Result = EU.Throws<ArgumentException>(() => { Expr.ArrayAccess(arr, new List<Expression>() { }); });

            return Expr.Empty();
        }

        // Multi dimensional array, with corresponding valid indexes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 10", new string[] { "positive", "arrayaccess", "assignread", "Pri2" })]
        public static Expr ArrayAccess10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arg1 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            NewArrayExpression arg2 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6) });

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int[]), new Expression[] { arg1, arg2 });

            Expr Result = Expr.ArrayAccess(arr, new List<Expression>() { Expr.Constant(0) });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(Result, Expr.Constant(0)), "ArrayAccess 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.ArrayAccess(Result, Expr.Constant(1)), "ArrayAccess 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.ArrayAccess(Result, Expr.Constant(2)), "ArrayAccess 3"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Multi dimensional array, with fewer indexes than it takes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 11", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arg1 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            NewArrayExpression arg2 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6) });
            NewArrayExpression arg3 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(7), Expr.Constant(8), Expr.Constant(9) });

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int[]), new Expression[] { arg1, arg2, arg3 });

            Expr Result =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ArrayAccess(arr, new List<Expression>() { Expr.Constant(1), Expr.Constant(2) });
            });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Result, "ArrayAccess 1"));

            return Expr.Empty();
        }

        // IEnumerable with non integer types (convertible to integer)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 12", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });

            Expr Result =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ArrayAccess(arr, new List<Expression>() { Expr.Constant(1.0) });
            });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Result, "ArrayAccess 1"));

            return Expr.Empty();
        }

        // IEnumerable with non integer types (non convertible to integer)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 13", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });

            Expr Result =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ArrayAccess(arr, new List<Expression>() { Expr.Constant("1") });
            });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Result, "ArrayAccess 1"));

            return Expr.Empty();
        }

        // Null for array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 14", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr ArrayAccess14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr Result =
            EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.ArrayAccess(null, new Expression[] { Expr.Constant(0) });
            });
            return Expr.Empty();
        }

        // Null for indexes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 15", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var arr = Expr.NewArrayInit(typeof(int), new Expression[] { });
            var Result = EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ArrayAccess(arr, null);
            });

            return Expr.Empty();
        }

        // Null elements for indexes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 16", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr ArrayAccess16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var arr = Expr.NewArrayInit(typeof(int), new Expression[] { });
            Expr Result =
            EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.ArrayAccess(arr, new Expression[] { null });
            });

            return Expr.Empty();
        }

        // Single dimension array, pass no indexes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 17", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new Expression[] { });
            Expr Result = EU.Throws<ArgumentException>(() => { Expr.ArrayAccess(arr, new Expression[] { }); });

            return Expr.Empty();
        }

        // Multi dimensional array, with corresponding valid indexes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 18", new string[] { "positive", "arrayaccess", "assignread", "Pri2" })]
        public static Expr ArrayAccess18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arg1 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            NewArrayExpression arg2 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6) });

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int[]), new Expression[] { arg1, arg2 });

            Expr Result = Expr.ArrayAccess(arr, new Expression[] { Expr.Constant(0) });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(Result, Expr.Constant(0)), "ArrayAccess 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.ArrayAccess(Result, Expr.Constant(1)), "ArrayAccess 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.ArrayAccess(Result, Expr.Constant(2)), "ArrayAccess 3"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Multi dimensional array, with fewer indexes than it takes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 19", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arg1 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            NewArrayExpression arg2 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(4), Expr.Constant(5), Expr.Constant(6) });
            NewArrayExpression arg3 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(7), Expr.Constant(8), Expr.Constant(9) });

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int[]), new Expression[] { arg1, arg2, arg3 });

            Expr Result =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ArrayAccess(arr, new Expression[] { Expr.Constant(1), Expr.Constant(2) });
            });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Result, "ArrayAccess 1"));

            return Expr.Empty();
        }

        // IEnumerable with non integer types (convertible to integer)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 20", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });

            Expr Result =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ArrayAccess(arr, new Expression[] { Expr.Constant(1.0) });
            });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Result, "ArrayAccess 1"));

            return Expr.Empty();
        }

        // IEnumerable with non integer types (non convertible to integer)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 21", new string[] { "negative", "arrayaccess", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            NewArrayExpression arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });

            Expr Result =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ArrayAccess(arr, new Expression[] { Expr.Constant("1") });
            });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Result, "ArrayAccess 1"));

            return Expr.Empty();
        }

        public struct TestStruct {
            public TestStruct(int x, string y) { X = x; Y = y; }
            public int X;
            public string Y;
        }

        // Access an array of type structure, verify the structure members can be modified
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 22", new string[] { "positive", "arrayaccess", "readassign", "Pri2" })]
        public static Expr ArrayAccess22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(TestStruct), "Result");
            ParameterExpression Value1 = Expr.Variable(typeof(TestStruct), "Val1");
            ParameterExpression Value2 = Expr.Variable(typeof(TestStruct), "Val1");
            ParameterExpression arr = Expr.Variable(typeof(TestStruct[]), "arr");

            ConstructorInfo ci = typeof(TestStruct).GetConstructor(new Type[] { typeof(int), typeof(string) });

            Expressions.Add(Expr.Assign(Value1, Expr.New(ci, new Expression[] { Expr.Constant(2), Expr.Constant("One") })));
            Expressions.Add(Expr.Assign(Value2, Expr.New(ci, new Expression[] { Expr.Constant(4), Expr.Constant("Two") })));

            Expressions.Add(Expr.Assign(arr, Expr.NewArrayInit(typeof(TestStruct), new Expression[] { Value1, Value2 })));
            Expressions.Add(Expr.Assign(Result, Expr.ArrayAccess(arr, Expr.Constant(0))));

            FieldInfo fi = typeof(TestStruct).GetField("X");
            FieldInfo fi2 = typeof(TestStruct).GetField("Y");

            // make sure we pulled out the right element
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Field(Result, fi), "ArrayAccess 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("One"), Expr.Field(Result, fi2), "ArrayAccess 2"));

            // modify struct, works fine on local variable
            Expressions.Add(Expr.Assign(Expr.Field(Result, fi), Expr.Constant(3)));
            Expressions.Add(Expr.Assign(Expr.Field(Result, fi2), Expr.Constant("OneMore")));

            // local was modified correctly
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Field(Result, fi), "ArrayAccess 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("OneMore"), Expr.Field(Result, fi2), "ArrayAccess 4"));

            // modified correctly in the array too
            Expressions.Add(Expr.Assign(Expr.ArrayAccess(arr, Expr.Constant(0)), Result));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Field(Expr.ArrayAccess(arr, Expr.Constant(0)), fi), "ArrayAccess 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("OneMore"), Expr.Field(Expr.ArrayAccess(arr, Expr.Constant(0)), fi2), "ArrayAccess 4"));

            ParameterExpression DontVisit = Expr.Parameter(typeof(int), "Dont_Visit_Node");
            var tree = Expr.Block(new[] { Result, Value1, Value2, arr, DontVisit }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static void refArrayMethod(ref int x) {
            x += 1;
        }

        // Pass an indexed array to a ref argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 23", new string[] { "positive", "arrayaccess", "readassign", "Pri2" })]
        public static Expr ArrayAccess23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression arr = Expr.Variable(typeof(int[]), "arr");

            Expressions.Add(Expr.Assign(arr, Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) })));

            MethodInfo mi = typeof(ArrayAccess).GetMethod("refArrayMethod");

            Expressions.Add(
                Expr.Call(
                mi,
                new Expression[] { Expr.ArrayAccess(arr, Expr.Constant(0)) }
                )
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.ArrayAccess(arr, Expr.Constant(0)), "ArrayAccess 1"));

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "dont_visit_node");
            var tree = Expr.Block(new[] { arr, DoNotVisitTemp }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Try all the numeric types for index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 24", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.Throws<ArgumentException>(() => { EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(arr, Expr.Constant((short)0, typeof(short))), "ArrayAccess 1"); }));
            return Expr.Block(Expressions);
        }

        // Try all the numeric types for index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 24_1", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess24_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.Throws<ArgumentException>(() => { EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(arr, Expr.Constant((ushort)0, typeof(ushort))), "ArrayAccess 1"); }));
            return Expr.Block(Expressions);
        }

        // Try all the numeric types for index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 24_2", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess24_2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.Throws<ArgumentException>(() => { EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(arr, Expr.Constant((Int16)0, typeof(Int16))), "ArrayAccess 1"); }));
            return Expr.Block(Expressions);
        }

        // Try all the numeric types for index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 24_3", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess24_3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.Throws<ArgumentException>(() => { EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(arr, Expr.Constant((UInt16)0, typeof(UInt16))), "ArrayAccess 1"); }));
            return Expr.Block(Expressions);
        }

        // Try all the numeric types for index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 24_4", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess24_4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.Throws<ArgumentException>(() => { EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(arr, Expr.Constant((UInt32)0, typeof(UInt32))), "ArrayAccess 1"); }));
            return Expr.Block(Expressions);
        }

        // Try all the numeric types for index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 24_5", new string[] { "positive", "arrayaccess", "readassign", "Pri2" })]
        public static Expr ArrayAccess24_5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(arr, Expr.Constant((Int32)0, typeof(Int32))), "ArrayAccess 1"));
            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Try all the numeric types for index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 24_6", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess24_6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.Throws<ArgumentException>(() => { EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(arr, Expr.Constant((double)0, typeof(double))), "ArrayAccess 1"); }));
            return Expr.Block(Expressions);
        }

        // Try all the numeric types for index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 24_7", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess24_7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.Throws<ArgumentException>(() => { EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(arr, Expr.Constant((ulong)0, typeof(ulong))), "ArrayAccess 1"); }));
            return Expr.Block(Expressions);
        }

        // Try all the numeric types for index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 24_8", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess24_8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.Throws<ArgumentException>(() => { EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(arr, Expr.Constant((ulong)0, typeof(long))), "ArrayAccess 1"); }));
            return Expr.Block(Expressions);
        }

        // Try all the numeric types for index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 24_9", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess24_9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.Throws<ArgumentException>(() => { EU.GenAreEqual(Expr.Constant(1), Expr.ArrayAccess(arr, Expr.Constant((decimal)0, typeof(decimal))), "ArrayAccess 1"); }));
            return Expr.Block(Expressions);
        }

//Seems it's not possible to create such arrays in silverlight (it's missing the Array.CreateInstance overload)
#if !SILVERLIGHT
        // Access an array that takes negative indexes
        // Regression for Dev10 Bug 552957
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 25", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression arr = Expr.Variable(typeof(int[]), "arr");
            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(arr, Expr.Constant(System.Array.CreateInstance(typeof(int), new int[] { 3 }, new int[] { -2 })));
            }));

            Expressions.Add(Expr.Assign(Result, Expr.ArrayAccess(arr, new Expression[] { Expr.Constant(-1) })));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Result, "ArrayAccess 1"));

            return Expr.Empty();
        }

        // Access an array that takes negative indexes
        // Regression for Dev10 Bug 552957
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 25_1", new string[] { "positive", "arrayaccess", "readassign", "Pri2" })]
        public static Expr ArrayAccess25_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression arr = Expr.Variable(typeof(System.Array), "arr");
            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            Expressions.Add(Expr.Assign(arr, Expr.Constant(System.Array.CreateInstance(typeof(int), new int[] { 3 }, new int[] { -2 }))));

            MethodInfo mi = typeof(System.Array).GetMethod("GetValue", new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(Result, Expr.Unbox(Expr.Call(arr, mi, new Expr[] { Expr.Constant(-1) }), typeof(int))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Result, "ArrayAccess 1"));

            var tree = Expr.Block(new[] { arr, Result }, Expressions);
            V.Validate(tree);
            return tree;
        }
#endif

        // Index an array out of bounds
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 26", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(IndexOutOfRangeException))]
        public static Expr ArrayAccess26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.ArrayAccess(arr, Expr.Constant(4)), "ArrayAccess 1"));
            var tree = Expr.Block(Expressions);
            V.ValidateException<IndexOutOfRangeException>(tree, false);
            return tree;
        }

        // Use a negative index on a zero based array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 27", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(IndexOutOfRangeException))]
        public static Expr ArrayAccess27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.ArrayAccess(arr, Expr.Constant(-1)), "ArrayAccess 1"));
            var tree = Expr.Block(Expressions);
            V.ValidateException<IndexOutOfRangeException>(tree, false);
            return tree;
        }

        // Use a nullable integer for an index
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 28", new string[] { "negative", "arrayaccess", "readassign", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr ArrayAccess28(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr arr = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.ArrayAccess(arr, Expr.Constant((Nullable<Int32>)1, typeof(Nullable<Int32>))), "ArrayAccess 1"));
            });
            return Expr.Empty();
        }

        // Assigning two array elements with two same typed nullable values
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ArrayAccess 29", new string[] { "positive", "arrayaccess", "readassign", "Pri2" })]
        public static Expr ArrayAccess29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var Arr = Expr.Parameter(typeof(int?[]), "");
            Expr arr1 = Expr.NewArrayInit(typeof(int?), new Expression[] { Expr.Constant(-1, typeof(int?)), Expr.Constant(-2, typeof(int?)), Expr.Constant(-3, typeof(int?)) });
            Expressions.Add(Expr.Assign(Arr, arr1));

            Expressions.Add(Expr.Assign(Expr.ArrayAccess(Arr, Expr.Constant(1)), Expr.Constant(1, typeof(int?))));
            Expressions.Add(Expr.Assign(Expr.ArrayAccess(Arr, Expr.Constant(2)), Expr.Constant(2, typeof(int?))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1, typeof(int?)), Expr.ArrayAccess(Arr, Expr.Constant(1)), "ArrayAccess 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2, typeof(int?)), Expr.ArrayAccess(Arr, Expr.Constant(2)), "ArrayAccess 2"));


            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }
    }
}
