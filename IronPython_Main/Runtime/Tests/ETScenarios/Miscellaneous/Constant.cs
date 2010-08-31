#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Constant {
        // Pass null to value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Constant 1", new string[] { "positive", "constant", "miscellaneous", "Pri1" })]
        public static Expr Constant1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(object), "");
            Expressions.Add(Expr.Assign(Result, Expr.Constant(null)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(null), Result));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Constant 2", new string[] { "negative", "constant", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Constant2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<ArgumentNullException>(() => { Expr.Constant(1, null); }));
            return Expr.Empty();
        }

        // Pass ref type to type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Constant 3", new string[] { "positive", "constant", "miscellaneous", "Pri1" })]
        public static Expr Constant3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(Expr.Constant(null, typeof(int).MakeByRefType()));
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        public struct s1 {
            public int i;
        }

        // struct with default value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Constant 4", new string[] { "positive", "constant", "miscellaneous", "Pri1" })]
        public static Expr Constant4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expr cs = Expr.New(typeof(s1));
            
            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Expr.Field(cs, typeof(s1).GetField("i"))));
            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        // struct with initialized value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Constant 5", new string[] { "positive", "constant", "miscellaneous", "Pri1" })]
        public static Expr Constant5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression cs = Expr.Variable(typeof(s1), "cs");
            Expressions.Add(Expr.Assign(cs, Expr.New(typeof(s1))));
            Expressions.Add(Expr.Assign(Expr.Field(cs, "i"), Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), Expr.Field(cs, typeof(s1).GetField("i"))));
            var tree = EU.BlockVoid(new [] { cs }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a non converting type as type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Constant 6", new string[] { "negative", "constant", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Constant6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Constant(5, typeof(short)); }));
            return Expr.Empty();
        }

        // pass a non converting type as type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Constant 7", new string[] { "positive", "constant", "miscellaneous", "Pri1" })]
        public static Expr Constant7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(Expr.Constant(new MemberAccessException(), typeof(Exception)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass an open generic type to type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Constant 8", new string[] { "negative", "constant", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Constant8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression var = Expr.Parameter(typeof(object), "");

            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Assign(var, Expr.Constant(null, typeof(List<int>).GetGenericTypeDefinition())); }));

            return Expr.Empty();
        }
    }
}
