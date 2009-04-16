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
using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting.Ast {
    internal sealed class LambdaParameterRewriter : ExpressionVisitor {
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        internal LambdaParameterRewriter(Dictionary<ParameterExpression, ParameterExpression> map) {
            _map = map;
        }

        // We don't need to worry about parameter shadowing, because we're
        // replacing the instances consistently everywhere
        protected override Expression VisitParameter(ParameterExpression node) {
            ParameterExpression result;
            if (_map.TryGetValue(node, out result)) {
                return result;
            }
            return node;
        }
    }
}
