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

    public class Comma {
        // Pass no elements to the Comma
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Comma 1", new string[] { "negative", "comma", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Comma1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Block(); }));

            return Expr.Empty();
        }

        // Using commas in multiple expressions that expect a value.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Comma 2", new string[] { "positive", "comma", "miscellaneous", "Pri1" })]
        public static Expr Comma2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expressions.Add(Expr.Assign(TestValue, Expr.Block(EU.ConcatEquals(Result, "C1"), Expr.Constant(1))));

            Expr Res =
                Expr.Add(
                    Expr.Block(EU.ConcatEquals(Result, "C2"), Expr.Constant(1)),
                    Expr.Block(EU.ConcatEquals(Result, "C3"), Expr.Constant(3)));

            Expressions.Add(
                Expr.Condition(
                    Expr.LessThan(TestValue, Expr.Block(Expr.Constant(5))),
                    Expr.Block(EU.ConcatEquals(Result, "C4"), Expr.Assign(TestValue, Expr.Constant(5)), Expr.Constant(true)),
                    Expr.Block(EU.ConcatEquals(Result, "C5"), Expr.Constant(false))));

            Expressions.Add(EU.GenAreEqual(Res, Expr.Constant(4), "Comma 1"));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("C1C4C2C3"), "Comma 2"));
            Expressions.Add(EU.GenAreEqual(TestValue, Expr.Constant(5), "Comma 3"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
