#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.ConvertCallInvoke {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Invoke {
        // Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Invoke 1", new string[] { "negative", "invoke", "convertcallinvoke", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Invoke1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(EU.Throws<ArgumentNullException>(() => { Expr.Invoke(null, new Expression[] { Expr.Constant(1) }); }));
            return Expr.Empty();
        }


        // methods with two ref arguments of the same type
        public delegate int Inv2(ref int arg1, ref int arg2);
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Invoke 2", new string[] { "positive", "call", "convertcallinvoke", "Pri1" })]
        public static Expr Invoke2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var Left = Expr.Parameter(typeof(int), "");
            var Right = Expr.Parameter(typeof(int), "");

            var arg1 = Expr.Parameter(typeof(int).MakeByRefType(), "");
            var arg2 = Expr.Parameter(typeof(int).MakeByRefType(), "");
            var tmp = Expr.Parameter(typeof(int), "");

            var lmd = Expr.Lambda(typeof(Inv2),
                Expr.Block(new[] { tmp },
                    Expr.Assign(tmp, Expr.Add(arg1, arg2)),
                    Expr.Assign(arg1, Expr.Constant(5)),
                    Expr.Assign(arg2, Expr.Constant(6)),
                    tmp
                ),
                arg1, arg2);

            Expressions.Add(Expr.Assign(Left, Expr.Constant(1)));
            Expressions.Add(Expr.Assign(Right, Expr.Constant(2)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Invoke(lmd, Left, Right), "Invoke 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), Left, "Invoke 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Right, "Invoke 3"));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);
            V.Validate(tree);
            return tree;
        }

    }
}
