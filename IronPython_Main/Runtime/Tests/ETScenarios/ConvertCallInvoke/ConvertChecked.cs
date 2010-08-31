#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace ETScenarios.ConvertCallInvoke {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class ConvertChecked {
        // ConvertChecked with source and destination type == object
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ConvertChecked 1", new string[] { "positive", "convertchecked", "convertcallinvoke", "Pri2" }, Priority = 2)]
        public static Expr ConvertChecked1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(object), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.ConvertChecked(Expr.Constant(1, typeof(object)), typeof(object))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Expr.Unbox(Result, typeof(int)), "ConvertChecked 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // ConvertChecked with source and destination having reference conversion
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ConvertChecked 2", new string[] { "positive", "convertchecked", "convertcallinvoke", "Pri2" }, Priority = 2)]
        public static Expr ConvertChecked2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(Exception), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.ConvertChecked(Expr.Constant(new DivideByZeroException()), typeof(Exception))));
            Expressions.Add(EU.ExprTypeCheck(Result, typeof(Exception)));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // ConvertChecked with source != void and destination == void
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ConvertChecked 3", new string[] { "negative", "convertchecked", "convertcallinvoke", "Pri2" }, Exception = typeof(InvalidOperationException), Priority = 2)]
        public static Expr ConvertChecked3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(Exception), "Result");
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Assign(Result, Expr.ConvertChecked(Expr.Constant(new DivideByZeroException()), typeof(void)));
            }));
            Expressions.Add(EU.ExprTypeCheck(Result, typeof(Exception)));

            var tree = Expr.Empty();

            return tree;
        }

        public static DateTime ConvertMethod(int x) {
            return DateTime.Now;
        }

        // ConvertChecked with MethodInfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ConvertChecked 4", new string[] { "positive", "convertchecked", "convertcallinvoke", "Pri2" }, Priority = 2)]
        public static Expr ConvertChecked4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ConvertChecked).GetMethod("ConvertMethod");
            ParameterExpression Result = Expr.Variable(typeof(DateTime), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.ConvertChecked(Expr.Constant(1), typeof(DateTime), mi)));
            Expressions.Add(EU.ExprTypeCheck(Result, typeof(DateTime)));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static object ConvertMethod2(object x) {
            return new object();
        }

        // ConvertChecked with source and destination type == object and MethodInfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ConvertChecked 5", new string[] { "positive", "convertchecked", "convertcallinvoke", "Pri2" }, Priority = 2)]
        public static Expr ConvertChecked5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ConvertChecked).GetMethod("ConvertMethod2");
            ParameterExpression Result = Expr.Variable(typeof(object), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.ConvertChecked(Expr.Constant(new object()), typeof(object))));
            Expressions.Add(EU.ExprTypeCheck(Result, typeof(object)));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // ConvertChecked with source != void and destination == void and MethodInfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ConvertChecked 6", new string[] { "negative", "convertchecked", "convertcallinvoke", "Pri2" }, Exception = typeof(ArgumentException), Priority = 2)]
        public static Expr ConvertChecked6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(ConvertChecked).GetMethod("ConvertMethod2");
            var Result = EU.Throws<System.ArgumentException>(() => { Expr.Variable(typeof(void), "Result"); });
            //Expressions.Add(EU.Throws<ArgumentException>(() => { Expr.Assign(Result, Expr.ConvertChecked(Expr.Constant(new object()), typeof(void))); }));

            return Expr.Empty();
        }
    }
}
