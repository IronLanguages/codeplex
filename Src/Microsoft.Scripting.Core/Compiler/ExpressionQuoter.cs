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
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using Microsoft.Linq.Expressions.Compiler;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

namespace Microsoft.Linq.Expressions.Compiler {

    // Modifies a quoted Expression instance by changing hoisted variables and
    // parameters into hoisted local references. The variable's StrongBox is
    // burned as a constant, and all hoisted variables/parameters are rewritten
    // as indexing expressions.
    //
    // The behavior of Quote is indended to be like C# and VB expression quoting
    internal sealed class ExpressionQuoter : ExpressionTreeVisitor {
        private readonly HoistedLocals _scope;
        private readonly object[] _locals;

        // A stack of variables that are defined in nested scopes. We search
        // this first when resolving a variable in case a nested scope shadows
        // one of our variable instances.
        //
        // TODO: should HoistedLocals track shadowing so we don't need to worry
        // about it here?
        private readonly Stack<Set<ParameterExpression>> _hiddenVars = new Stack<Set<ParameterExpression>>();

        internal ExpressionQuoter(HoistedLocals scope, object[] locals) {
            _scope = scope;
            _locals = locals;
        }

        protected internal override Expression VisitLambda(LambdaExpression node) {
            _hiddenVars.Push(new Set<ParameterExpression>(node.Parameters));
            Expression b = Visit(node.Body);
            _hiddenVars.Pop();
            if (b == node.Body) {
                return node;
            }
            return node.CloneWith(node.Name, b, node.Annotations, node.Parameters);
        }

        protected internal override Expression VisitBlock(BlockExpression node) {
            _hiddenVars.Push(new Set<ParameterExpression>(node.Variables));
            var b = Visit(node.Expressions);
            _hiddenVars.Pop();
            if (b == node.Expressions) {
                return node;
            }
            if (node.Type == typeof(void)) {
                return Expression.Block(node.Annotations, node.Variables, b);
            } else {
                return Expression.Comma(node.Annotations, node.Variables, b);
            }
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node) {
            _hiddenVars.Push(new Set<ParameterExpression>(new[] { node.Variable }));
            Expression b = Visit(node.Body);
            Expression f = Visit(node.Filter);
            _hiddenVars.Pop();
            if (b == node.Body && f == node.Filter) {
                return node;
            }
            return Expression.Catch(node.Test, node.Variable, b, f, node.Annotations);
        }

        protected internal override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
            try {
                return base.VisitRuntimeVariables(node);
            } catch (InvalidOperationException) {
                // TODO: this is not a critical feature, but we can add support for
                // this in a later release (by rewriting the expression to a new
                // one that combines the boxes from the closed over variables and
                // the non-closed over variables)
                throw Error.RuntimeVariablesNotSupportedInQuote(node);
            }
        }

        protected internal override Expression VisitParameter(ParameterExpression node) {
            IStrongBox box = GetBox(node);
            if (box == null) {
                return node;
            }
            return Expression.Field(Expression.Constant(box), "Value", node.Annotations);
        }

        private IStrongBox GetBox(ParameterExpression variable) {
            // Skip variables that are shadowed by a nested scope/lambda
            foreach (Set<ParameterExpression> hidden in _hiddenVars) {
                if (hidden.Contains(variable)) {
                    return null;
                }
            }

            HoistedLocals scope = _scope;
            object[] locals = _locals;
            while (true) {
                int hoistIndex;
                if (scope.Indexes.TryGetValue(variable, out hoistIndex)) {
                    return (IStrongBox)locals[hoistIndex];
                }
                scope = scope.Parent;
                if (scope == null) {
                    break;
                }
                locals = HoistedLocals.GetParent(locals);
            }

            // TODO: this should be unreachable because it's an unbound
            // variable, so we should throw here. It's a breaking change,
            // however
            return null;
        }
    }
}

namespace Microsoft.Runtime.CompilerServices {
    public partial class RuntimeOps {
        [Obsolete("used by generated code", true)]
        public static Expression Quote(Expression expression, object hoistedLocals, object[] locals) {
            Debug.Assert(hoistedLocals != null && locals != null);
            ExpressionQuoter quoter = new ExpressionQuoter((HoistedLocals)hoistedLocals, locals);
            return quoter.Visit(expression);
        }
    }
}
