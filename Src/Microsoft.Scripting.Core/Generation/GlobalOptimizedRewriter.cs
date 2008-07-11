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

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Scripting.Runtime;

namespace System.Scripting.Generation {

    internal abstract class GlobalOptimizedRewriter : GlobalRewriter {
        private readonly Dictionary<GlobalVariableExpression, Expression> _mapToExpression = new Dictionary<GlobalVariableExpression, Expression>();
        private readonly Dictionary<string, GlobalVariableExpression> _globalNames = new Dictionary<string, GlobalVariableExpression>();

        protected abstract Expression MakeWrapper(GlobalVariableExpression variable);

        protected override Expression RewriteGet(GlobalVariableExpression node) {
            return Expression.ConvertHelper(MapToExpression(node), node.Type);
        }

        protected override Expression RewriteSet(AssignmentExpression node) {
            GlobalVariableExpression lvalue = (GlobalVariableExpression)node.Expression;

            return Expression.ConvertHelper(
                Expression.Assign(
                    MapToExpression(lvalue),
                    Expression.ConvertHelper(VisitNode(node.Value), typeof(object)),
                    node.Annotations
                ),
                node.Type
            );
        }

        protected Expression MapToExpression(GlobalVariableExpression variable) {
            Expression result;
            if (_mapToExpression.TryGetValue(variable, out result)) {
                return result;
            }

            EnsureUniqueName(_globalNames, variable);

            result = Expression.Property(
                MakeWrapper(variable),
                typeof(ModuleGlobalWrapper).GetProperty("CurrentValue"),
                variable.Annotations
            );

            return _mapToExpression[variable] = result;
        }
    }
}
