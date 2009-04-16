/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System; using Microsoft;
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {

        // Helper to add a variable to a block
        internal static Expression AddScopedVariable(Expression body, ParameterExpression variable, Expression variableInit) {
            List<ParameterExpression> vars = new List<ParameterExpression>();
            List<Expression> newBody = new List<Expression>();

            var exprs = new ReadOnlyCollection<Expression>(new [] { body });
            var parent = body;
            //Merge blocks if the current block has only one child that is another block, 
            //the blocks to merge must have the same type.
            while (exprs.Count == 1 && exprs[0].NodeType == ExpressionType.Block && parent.Type == exprs[0].Type) {
                BlockExpression scope = (BlockExpression)(exprs[0]);
                vars.AddRange(scope.Variables);
                parent = scope;
                exprs = scope.Expressions;
            }

            newBody.Add(Expression.Assign(variable, variableInit));
            newBody.AddRange(exprs);
            vars.Add(variable);
            return Expression.Block(
                vars,
                newBody.ToArray()
            );
        }

        internal static BlockExpression BlockVoid(Expression[] expressions) {
            if (expressions.Length == 0 || expressions[expressions.Length - 1].Type != typeof(void)) {
                expressions = expressions.AddLast(Utils.Empty());
            }
            return Expression.Block(expressions);
        }

        internal static BlockExpression Block(Expression[] expressions) {
            if (expressions.Length == 0) {
                expressions = expressions.AddLast(Utils.Empty());
            }
            return Expression.Block(expressions);
        }
    }
}
