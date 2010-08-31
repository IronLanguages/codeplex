#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.ObjectInstantiation {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Arrays {

        public static Expr GetLowerBound(Expr arr, int index) {
            var mi = typeof(System.Array).GetMethod("GetLowerBound");
            return Expr.Call(arr, mi, Expr.Constant(index));
        }

        public static Expr GetUpperBound(Expr arr, int index) {
            var mi = typeof(System.Array).GetMethod("GetUpperBound");
            return Expr.Call(arr, mi, Expr.Constant(index));
        }

        // Pass -2 to element bounds
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Arrays 1", new string[] { "negative", "array", "new", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr Arrays1(EU.IValidator V) {
            List<Expression> Exprs = new List<Expression>();

            ParameterExpression Arr = Expr.Parameter(typeof(int[]), "Arr");

            Exprs.Add(Expr.Assign(Arr, Expr.NewArrayBounds(typeof(int), new[] { Expr.Constant(-2) })));
            MethodInfo Mi = typeof(object).GetMethod("ToString", new Type[] { });
            Exprs.Add(Expr.Call(Arr, Mi));

            var tree = Expr.Block(new[] { Arr }, Exprs);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }

        // Pass -1 to element bounds
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Arrays 2", new string[] { "negative", "array", "new", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr Arrays2(EU.IValidator V) {
            List<Expression> Exprs = new List<Expression>();

            Exprs.Add(Expr.NewArrayBounds(typeof(int), new[] { Expr.Constant(-1) }));

            var tree = EU.BlockVoid(Exprs);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }

        // create a single dimension array
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Arrays 3", new string[] { "positive", "array", "new", "Pri1" })]
        public static Expr Arrays3(EU.IValidator V) {
            List<Expression> Exprs = new List<Expression>();

            ParameterExpression Arr = Expr.Parameter(typeof(int[]), "Arr");

            Exprs.Add(Expr.Assign(Arr, Expr.NewArrayBounds(typeof(int), new[] { Expr.Constant(5) })));
            Exprs.Add(EU.GenAreEqual(Expr.Constant(0), GetLowerBound(Arr, 0)));
            Exprs.Add(EU.GenAreEqual(Expr.Constant(4), GetUpperBound(Arr, 0)));

            var tree = Expr.Block(new[] { Arr }, Exprs);
            V.Validate(tree);
            return tree;
        }


        // create a multi dimensions array, 2 dimensions
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Arrays 4", new string[] { "positive", "array", "new", "Pri1" })]
        public static Expr Arrays4(EU.IValidator V) {
            List<Expression> Exprs = new List<Expression>();

            ParameterExpression Arr = Expr.Parameter(typeof(int[,]), "Arr");

            Exprs.Add(Expr.Assign(Arr, Expr.NewArrayBounds(typeof(int), new[] { Expr.Constant(3), Expr.Constant(2) })));
            Exprs.Add(EU.GenAreEqual(Expr.Constant(0), GetLowerBound(Arr, 0)));
            Exprs.Add(EU.GenAreEqual(Expr.Constant(2), GetUpperBound(Arr, 0)));
            Exprs.Add(EU.GenAreEqual(Expr.Constant(0), GetLowerBound(Arr, 1)));
            Exprs.Add(EU.GenAreEqual(Expr.Constant(1), GetUpperBound(Arr, 1)));

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "DoNotVisitTemp");
            var tree = Expr.Block(new[] { Arr, DoNotVisitTemp }, Exprs);
            V.Validate(tree);
            return tree;
        }


        // Pass -2 to element bounds
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Arrays 5", new string[] { "negative", "array", "new", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr Arrays5(EU.IValidator V) {
            List<Expression> Exprs = new List<Expression>();

            ParameterExpression Arr = Expr.Parameter(typeof(int[]), "Arr");

            Exprs.Add(Expr.Assign(Arr, Expr.NewArrayBounds(typeof(int), (IEnumerable<Expr>)new[] { Expr.Constant(-2) })));
            MethodInfo Mi = typeof(object).GetMethod("ToString", new Type[] { });
            Exprs.Add(Expr.Call(Arr, Mi));

            var tree = Expr.Block(new[] { Arr }, Exprs);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }

        // Pass -1 to element bounds
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Arrays 6", new string[] { "negative", "array", "new", "Pri1" }, Exception = typeof(OverflowException))]
        public static Expr Arrays6(EU.IValidator V) {
            List<Expression> Exprs = new List<Expression>();

            Exprs.Add(Expr.NewArrayBounds(typeof(int), (IEnumerable<Expr>)new[] { Expr.Constant(-1) }));

            var tree = EU.BlockVoid(Exprs);
            V.ValidateException<OverflowException>(tree, false);
            return tree;
        }
    }
}
