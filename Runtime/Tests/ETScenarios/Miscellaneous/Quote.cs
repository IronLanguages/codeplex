#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Quote {
        //Validate quote only takes Expression<T> where T is a delegate.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Quote 1", new string[] { "negative", "quote", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Quote1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentException>(() => { Expr.Quote(Expr.Block(Expr.Empty())); });

            return Expr.Empty();

        }

        //Validate quote only takes Expression<T> where T is a delegate.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Quote 2", new string[] { "negative", "quote", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Quote2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentException>(() => { Expr.Quote(Expr.Parameter(typeof(int), "")); });

            return Expr.Empty();

        }

        //Validate quote only takes Expression<T> where T is a delegate.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Quote 3", new string[] { "positive", "quote", "miscellaneous", "Pri1" })]
        public static Expr Quote3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var tree = Expr.Quote(Expr.Lambda(Expr.Constant(1)));
            V.Validate(tree);
            return tree;            
        }

        //Validate quote only takes Expression<T> where T is a delegate.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Quote 4", new string[] { "positive", "quote", "miscellaneous", "Pri1" })]
        public static Expr Quote4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var tree = Expr.Quote(Expr.Lambda<Action>(Expr.Constant(1)));
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Quote 5", new string[] { "positive", "quote", "miscellaneous", "Pri1" })]
        public static Expr Quote5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Param1 = Expr.Variable(typeof(int), "Param1");

            var tree =
                Expr.Lambda(
                    Expr.Block(
                        Expr.RuntimeVariables(Param1),
                        Expr.Assign(Param1, Expr.Constant(5)),
                        Expr.Quote(
                            Expr.Lambda(
                                Expr.Assign(Param1, Expr.Constant(3)),
                                new ParameterExpression[] { Param1 }
                            )
                        )
                    ),
                    new ParameterExpression[] { Param1 }
                );

            Expressions.Add(Expr.Invoke(tree, Expr.Constant(0)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), Param1, "Quote 1"));

            var FinalTree = Expr.Block(new[] { Param1 }, Expressions);
            V.Validate(FinalTree);
            return FinalTree;
        }
    }
}
