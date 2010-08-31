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

    public class TypeIs {
        // Pass an expression with a value, use with the right type, and without the right type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeIs 1", new string[] { "positive", "typeis", "miscellaneous", "Pri1" })]
        public static Expr TypeIs1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.TypeIs(Expr.Constant(1), typeof(Int32)), Expr.Constant(true), "TypeIs 1"));
            Expressions.Add(EU.GenAreEqual(Expr.TypeIs(Expr.Constant(1), typeof(bool)), Expr.Constant(false), "TypeIs 1"));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
