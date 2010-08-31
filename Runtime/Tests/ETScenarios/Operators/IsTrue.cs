#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EU = ETUtils.ExpressionUtils;

namespace ETScenarios.Operators {
    using Expr = Expression;

    public class IsTrue {


        //Pass null to expressions
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 1", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr IsTrue1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.ArgumentNullException>(() => { Expr.IsTrue(null); });

            return Expr.Empty();
        }

        //Pass an expression of bool type to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 2", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsTrue(Expr.Constant(true)), "IsTrue 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), Expr.IsTrue(Expr.Constant(false)), "IsTrue 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass an expression of int type to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 3", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsTrue3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsTrue(Expr.Constant(1)); });

            return Expr.Empty();
        }

        public class Test {
        }

        //Pass a class to expression, no user defined op_true/op_false
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 4", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsTrue4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsTrue(Expr.Constant(new Test())); });

            return Expr.Empty();
        }

//Can't reference VBClassLib in Silverlight
#if VBCLASSLIB
#if !SILVERLIGHT
        //Pass a class to expression, with a user defined op_true/op_false
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 5", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), Expr.IsTrue(Expr.Constant(new VBClassLib.IsTrueIsFalse()))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsTrue(Expr.Constant(new VBClassLib.IsTrueIsFalse() { Value = true }))));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass a class to expression, without a user defined op_true/op_false but with a conversion to boolean
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 6", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsTrue6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.IsTrue(Expr.Constant((object)new VBClassLib.ToBool()));
            }));

            var tree = Expr.Empty();

            return tree;
        }

        //Pass a class to expression, without a user defined op_true/op_false but with a conversion to boolean
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 61", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsTrue6_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() => { Expr.IsTrue(Expr.Constant((object)new VBClassLib.ToBool() { Value = true })); }));

            return Expr.Empty();
        }
#endif
#endif

        //Pass an expression of type void to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 7", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsTrue7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsTrue(Expr.Empty()); });

            return Expr.Empty();
        }

        //Pass an expression of bool? type, with value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 8", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true, typeof(bool?)), Expr.IsTrue(Expr.Constant(true, typeof(bool?))), "IsTrue 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false, typeof(bool?)), Expr.IsTrue(Expr.Constant(false, typeof(bool?))), "IsTrue 2"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass an expression of bool? type, without value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 9", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(null, typeof(bool?)), Expr.IsTrue(Expr.Constant(null, typeof(bool?)))));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass a nullable of int type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 10", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsTrue10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsTrue(Expr.Constant(1, typeof(int?))); });

            return Expr.Empty();
        }

        //Pass a string with the value "True"
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 11", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsTrue11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsTrue(Expr.Constant("True")); });

            return Expr.Empty();
        }

        public static bool m_IsTrue1(int x) {
            return x != 0;
        }

        //Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 12", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr IsTrue12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue1");

            EU.Throws<System.ArgumentNullException>(() => { Expr.IsTrue(null, mi); });

            return Expr.Empty();
        }

        public static bool m_IsTrue2(bool x) {
            return x;
        }
        //Pass an expression of bool type to expression, methodinfo takes a bool
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 13", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue2");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), Expr.IsTrue(Expr.Constant(false), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass an expression of bool type to expression, methodinfo takes an int
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 14", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsTrue14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue1");

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsTrue(Expr.Constant(true), mi); });

            return Expr.Empty();
        }

        public static bool m_IsTrue3(DateTime x) {
            return true;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 15", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue3");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsTrue(Expr.Constant(DateTime.Now), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }


        public static bool m_IsTrue4(double x) {
            return true;
        }
        //date, methodinfo takes double
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 16", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsTrue16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue4");

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsTrue(Expr.Constant(DateTime.Now), mi); });

            return Expr.Empty();
        }

        public static bool m_IsTrue5(Test x) {
            return true;
        }
        //class, no op_true or false, methodinfo takes class
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 17", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue5");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsTrue(Expr.Constant(new Test()), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }


        public class Test2 : Test {
        }
        //Class, no op_true or false, methodinfo takes base class
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 18", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue5");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsTrue(Expr.Constant(new Test2()), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //void, methodinfo takes bool
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 19", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsTrue19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue2");

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsTrue(Expr.Empty(), mi); });

            return Expr.Empty();
        }

        public static bool m_IsTrue6() {
            return true;
        }
        //void, methodinfo takes no arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 20", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr IsTrue20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue6");

            EU.Throws<System.ArgumentException>(() => { Expr.IsTrue(Expr.Empty(), mi); });

            return Expr.Empty();
        }

        public static bool m_IsTrue7(bool? arg) {
            return true;
        }
        //nullable of bool (with value), methodinfo takes nullable of bool
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 21", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue7");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsTrue(Expr.Constant(true, typeof(bool?)), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //nullable of bool, methodinfo takes bool
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 22", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue2");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true, typeof(bool?)), Expr.IsTrue(Expr.Constant(true, typeof(bool?)), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool m_IsTrue8(bool arg, bool arg2) {
            return true;
        }

        //MethodInfo takes two arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 23", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr IsTrue23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue8");

            EU.Throws<System.ArgumentException>(() => { Expr.IsTrue(Expr.Constant(true), mi); });

            return Expr.Empty();
        }

        public static int m_IsTrue9(bool arg) {
            return 1;
        }
        //MethodInfo returns non bool
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 24", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue9");

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Expr.IsTrue(Expr.Constant(true), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool? m_IsTrue10(bool arg) {
            return true;
        }
        //MethodInfo returns bool?
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsTrue 25", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsTrue25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsTrue).GetMethod("m_IsTrue10");

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true, typeof(bool?)), Expr.IsTrue(Expr.Constant(true), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
