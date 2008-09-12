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
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting.Ast {
    internal class LambdaParameterRewriter : ExpressionTreeVisitor {
        private Dictionary<ParameterExpression, Expression> _paramMapping;

        internal LambdaParameterRewriter(Dictionary<ParameterExpression, Expression> paramMapping) {
            _paramMapping = paramMapping;
        }

        protected override Expression Visit(ParameterExpression node) {
            if (_paramMapping.ContainsKey(node)) {
                //parameter may be mapped to itself or to a variable.
                Debug.Assert(_paramMapping[node].NodeType == ExpressionType.Variable ||
                             (_paramMapping[node].NodeType == ExpressionType.Parameter &&
                               _paramMapping[node] == node));

                return _paramMapping[node];
            } else {
                return node;
            }
        }

        protected override Expression VisitExtension(Expression node) {
            if (node.IsReducible) {
                return VisitNode(node.ReduceToKnown());
            } else {
                CodeContextScopeExpression ccse = node as CodeContextScopeExpression;
                if (ccse != null) {
                    return VisitCodeContextScope(ccse);
                }
                // we will ignore other extension nodes that are not redicible
                // non-reducible extension nodes normally do not have subexpressions
                return node;
            }
        }

        private Expression VisitCodeContextScope(CodeContextScopeExpression node) {
            Expression newcontext = VisitNode(node.NewContext);
            Expression body = VisitNode(node.Body);

            if (newcontext != node.NewContext || body != node.Body) {
                node = Utils.CodeContextScope(
                    body,
                    newcontext,
                    node.Annotations
                );
            }
            return node;
        }
    }
}
