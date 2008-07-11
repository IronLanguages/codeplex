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

using System.Linq.Expressions;
using System.Diagnostics;
using System.Scripting.Utils;

namespace System.Linq.Expressions {

    // Modifies a quoted Expression instance by changing hoisted variables and
    // parameters into hoisted local references. The variable's StrongBox is
    // burned as a constant, and all hoisted variables/parameters are rewritten
    // as indexing expressions.
    //
    // TODO: this behavior seems really bizarre for what Quote generally means
    // in programming languages, but it's needed for backwards compatibility.
    internal sealed class ExpressionQuoter : ExpressionTreeVisitor {
        private readonly HoistedLocals _scope;
        private readonly object[] _locals;

        internal ExpressionQuoter(HoistedLocals scope, object[] locals) {
            _scope = scope;
            _locals = locals;
        }

        protected override Expression Visit(ParameterExpression node) {
            return VisitVariable(node);
        }

        protected override Expression Visit(VariableExpression node) {
            return VisitVariable(node);
        }

        private Expression VisitVariable(Expression node) {
            HoistedLocals scope = _scope;
            object[] locals = _locals;
            while (true) {
                int hoistIndex;
                if (scope.Indexes.TryGetValue(node, out hoistIndex)) {
                    return Expression.Field(Expression.Constant(locals[hoistIndex]), "Value", node.Annotations);
                }
                scope = scope.Parent;
                if (scope == null) {
                    break;
                }
                locals = HoistedLocals.GetParent(locals);
            }
            return node;            
        }
    }
}

namespace System.Scripting.Runtime {
    public partial class RuntimeHelpers {
        [Obsolete("used by generated code", true)]
        public static Expression Quote(Expression expression, object hoistedLocals, object[] locals) {
            Debug.Assert(hoistedLocals != null && locals != null);
            ExpressionQuoter quoter = new ExpressionQuoter((HoistedLocals)hoistedLocals, locals);
            return quoter.VisitNode(expression);
        }
    }
}