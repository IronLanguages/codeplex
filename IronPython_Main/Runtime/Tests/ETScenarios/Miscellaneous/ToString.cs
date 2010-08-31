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

    public class ToString {

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "StrPositionalArg 1", new string[] { "positive", "arguments", "miscellaneous", "Pri1" })]
        public static Expr PositionalArg1(EU.IValidator V) {
            var test = Expr.Block(Expr.Empty());
            var tree = Expr.Constant(test.ToString());

            V.Validate(tree);
            return tree;
        }
    }
}
