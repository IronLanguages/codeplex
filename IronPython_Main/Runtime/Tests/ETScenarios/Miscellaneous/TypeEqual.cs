#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class TypeEqual {
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeEqual 1", new string[] { "negative", "typedbinaryexpression", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr TypeEqual1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentNullException>(() => { Expr.TypeEqual(null, typeof(int)); });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeEqual 2", new string[] { "negative", "typedbinaryexpression", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr TypeEqual2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentNullException>(() => { Expr.TypeEqual(Expr.Constant(1), null); });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeEqual 3", new string[] { "positive", "typedbinaryexpression", "miscellaneous", "Pri1" })]
        public static Expr TypeEqual3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var tree = EU.GenAreEqual(Expr.Constant(true), Expr.TypeEqual(Expr.Constant(1), typeof(int)), "TypeEqual 1");
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeEqual 4", new string[] { "positive", "typedbinaryexpression", "miscellaneous", "Pri1" })]
        public static Expr TypeEqual4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var tree = EU.GenAreEqual(Expr.Constant(false), Expr.TypeEqual(Expr.Constant(1), typeof(short)), "TypeEqual 1");
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeEqual 5", new string[] { "positive", "typedbinaryexpression", "miscellaneous", "Pri1" })]
        public static Expr TypeEqual5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var tree = EU.GenAreEqual(Expr.Constant(true), Expr.TypeEqual(Expr.Constant(new ArgumentException()), typeof(ArgumentException)), "TypeEqual 1");
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeEqual 6", new string[] { "positive", "typedbinaryexpression", "miscellaneous", "Pri1" })]
        public static Expr TypeEqual6(EU.IValidator V) {
            var Except = Expr.Parameter(typeof(ArgumentException), "");
            var tree = Expr.Block(
                new[] { Except },
                Expr.Assign(Except, Expr.Constant(new ArgumentException())),
                EU.GenAreEqual(Expr.Constant(false), Expr.TypeEqual(Except, typeof(Exception)), "TypeEqual 1")
             );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeEqual 7", new string[] { "positive", "typedbinaryexpression", "miscellaneous", "Pri1" })]
        public static Expr TypeEqual7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var tree = EU.GenAreEqual(Expr.Constant(false), Expr.TypeEqual(Expr.Constant(new Exception()), typeof(ArgumentException)), "TypeEqual 1");
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeEqual 8", new string[] { "positive", "typedbinaryexpression", "miscellaneous", "Pri1" })]
        public static Expr TypeEqual8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var tree = EU.GenAreEqual(Expr.Constant(true), Expr.TypeEqual(Expr.Constant(1, typeof(int?)), typeof(int?)), "TypeEqual 1");
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeEqual 9", new string[] { "positive", "typedbinaryexpression", "miscellaneous", "Pri1" })]
        public static Expr TypeEqual9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var tree = EU.GenAreEqual(Expr.Constant(true), Expr.TypeEqual(Expr.Constant(1, typeof(int?)), typeof(int)), "TypeEqual 1");
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeEqual 10", new string[] { "positive", "typedbinaryexpression", "miscellaneous", "Pri1" })]
        public static Expr TypeEqual10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var tree = EU.GenAreEqual(Expr.Constant(true), Expr.TypeEqual(Expr.Constant(1, typeof(int)), typeof(int?)), "TypeEqual 1");
            V.Validate(tree);
            return tree;
        }
    }
}
