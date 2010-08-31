#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Operators {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class IsFalse {


        //Pass null to expressions
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 1", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr IsFalse1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.ArgumentNullException>(() => { Expr.IsFalse(null); });

            return Expr.Empty();
        }

        //Pass an expression of bool type to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 2", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), Expr.IsFalse(Expr.Constant(true)), "IsFalse 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsFalse(Expr.Constant(false)), "IsFalse 2"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass an expression of int type to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 3", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsFalse3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsFalse(Expr.Constant(1)); });

            return Expr.Empty();
        }

        public class Test {
        }

        //Pass a class to expression, no user defined op_true/op_false
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 4", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsFalse4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsFalse(Expr.Constant(new Test())); });

            return Expr.Empty();
        }

#if VBCLASSLIB
#if !SILVERLIGHT
        //Pass a class to expression, with a user defined op_true/op_false
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 5", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsFalse(Expr.Constant(new VBClassLib.IsTrueIsFalse()))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), Expr.IsFalse(Expr.Constant(new VBClassLib.IsTrueIsFalse() { Value = true }))));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass a class to expression, without a user defined op_true/op_false but with a conversion to boolean
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 6", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsFalse6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.IsFalse(Expr.Constant((object)new VBClassLib.ToBool()));
            }));

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }

        //Pass a class to expression, without a user defined op_true/op_false but with a conversion to boolean
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 6_1", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsFalse6_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<InvalidOperationException>(() => { Expr.IsFalse(Expr.Constant((object)new VBClassLib.ToBool() { Value = true })); }));

            return Expr.Empty();
        }
#endif
#endif

        //Pass an expression of type void to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 7", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsFalse7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsFalse(Expr.Empty()); });

            return Expr.Empty();
        }

        //Pass an expression of bool? type, with value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 8", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(false, typeof(bool?)), Expr.IsFalse(Expr.Constant(true, typeof(bool?))), "IsFalse 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true, typeof(bool?)), Expr.IsFalse(Expr.Constant(false, typeof(bool?))), "IsFalse 2"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass an expression of bool? type, without value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 9", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(null, typeof(bool?)), Expr.IsFalse(Expr.Constant(null, typeof(bool?)))));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass a nullable of int type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 10", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsFalse10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsFalse(Expr.Constant(1, typeof(int?))); });

            return Expr.Empty();
        }

        //Pass a string with the value "True"
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 11", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsFalse11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsFalse(Expr.Constant("True")); });

            return Expr.Empty();
        }

        public static bool IsTrue(int x) {
            return x != 0;
        }

        //Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 12", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr IsFalse12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue");

            EU.Throws<System.ArgumentNullException>(() => { Expr.IsFalse(null, mi); });

            return Expr.Empty();
        }

        public static bool IsTrue2(bool x) {
            return x;
        }
        //Pass an expression of bool type to expression, methodinfo takes a bool
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 13", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue2");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), Expr.IsFalse(Expr.Constant(false), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass an expression of bool type to expression, methodinfo takes an int
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 14", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsFalse14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue");

            EU.Throws<System.InvalidOperationException>(() => { Expr.IsFalse(Expr.Constant(true), mi); });

            return Expr.Empty();
        }

        public static bool IsTrue3(DateTime x) {
            return true;
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 15", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue3");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsFalse(Expr.Constant(DateTime.Now), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }


        public static bool IsTrue4(double x) {
            return true;
        }
        //date, methodinfo takes double
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 16", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsFalse16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue4");

            EU.Throws<InvalidOperationException>(() => { Expr.IsFalse(Expr.Constant(DateTime.Now), mi); });
            
            return Expr.Empty();
        }

        public static bool IsTrue5(Test x) {
            return true;
        }
        //class, no op_true or false, methodinfo takes class
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 17", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue5");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsFalse(Expr.Constant(new Test()), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }


        public class Test2 : Test {
        }
        //Class, no op_true or false, methodinfo takes base class
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 18", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue5");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsFalse(Expr.Constant(new Test2()), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //void, methodinfo takes bool
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 19", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr IsFalse19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue2");

            EU.Throws<InvalidOperationException>(() => { Expr.IsFalse(Expr.Empty(), mi); });

            return Expr.Empty();
        }

        public static bool IsTrue6() {
            return true;
        }
        //void, methodinfo takes no arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 20", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr IsFalse20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue6");

            EU.Throws<ArgumentException>(() => { Expr.IsFalse(Expr.Empty(), mi); });

            return Expr.Empty();
        }

        public static bool IsTrue7(bool? arg) {
            return true;
        }
        //nullable of bool (with value), methodinfo takes nullable of bool
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 21", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue7");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.IsFalse(Expr.Constant(true, typeof(bool?)), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        //nullable of bool, methodinfo takes bool
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 22", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue2");
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true, typeof(bool?)), Expr.IsFalse(Expr.Constant(true, typeof(bool?)), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool IsTrue8(bool arg, bool arg2) {
            return true;
        }

        //MethodInfo takes two arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 23", new string[] { "negative", "increment", "operators", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr IsFalse23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue8");

            EU.Throws<ArgumentException>(() => { Expr.IsFalse(Expr.Constant(true), mi); });

            return Expr.Empty();
        }

        public static int IsTrue9(bool arg) {
            return 1;
        }
        //MethodInfo returns non bool
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 24", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue9");

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Expr.IsFalse(Expr.Constant(true), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool? IsTrue10(bool arg) {
            return true;
        }
        //MethodInfo returns bool?
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IsFalse 25", new string[] { "positive", "increment", "operators", "Pri1" })]
        public static Expr IsFalse25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(IsFalse).GetMethod("IsTrue10");

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true, typeof(bool?)), Expr.IsFalse(Expr.Constant(true), mi)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
