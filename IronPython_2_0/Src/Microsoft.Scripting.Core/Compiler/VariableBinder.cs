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


using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {
    /// <summary>
    /// Determines if variables are closed over in nested lambdas and need to
    /// be hoisted.
    /// </summary>
    internal sealed class VariableBinder : ExpressionTreeVisitor {

        // The parent scope to the one we're binding, or null if we're binding
        // the outermost scope
        private readonly CompilerScope _parentScope;

        // The scope/lambda we're binding
        private readonly Expression _expression;

        // A stack of variables that are defined in nested scopes. We search
        // this first when resolving a variable in case a nested scope shadows
        // one of our variable instances.
        private readonly Stack<Set<ParameterExpression>> _hiddenVars = new Stack<Set<ParameterExpression>>();

        // For each variable in this scope: should it be hoisted to a closure?
        private readonly Dictionary<ParameterExpression, bool> _hoistVariable = new Dictionary<ParameterExpression, bool>();

        // For each variable referenced from this scope, this stores the
        // reference count. If the variable is referenced "enough", we'll cache
        // the closure StrongBox<T> into an IL local
        private readonly Dictionary<ParameterExpression, int> _referenceCount = new Dictionary<ParameterExpression, int>();

        // A list of variables in this scope, including merged ones.
        // Needed so we can preserve the order
        private readonly List<ParameterExpression> _myVariables = new List<ParameterExpression>();

        // The lambda that contains the scope we're binding
        // Or the lambda itself if we're binding a lambda
        private readonly LambdaExpression _topLambda;

        // The lambda that we're walking in
        private LambdaExpression _currentLambda;

        // Is this scope a closure?
        // (references variables from an outer scope)
        private bool _isClosure;

        private VariableBinder(CompilerScope parent, Expression scope) {
            _parentScope = parent;
            _expression = scope;
            if (scope.NodeType == ExpressionType.Scope) {
                _topLambda = parent.Lambda;
            } else {
                _topLambda = (LambdaExpression)scope;
            }
        }

        /// <summary>
        /// VariableBinder entry point. Binds the scope that is passed in.
        /// </summary>
        internal static CompilerScope Bind(CompilerScope parent, Expression scope) {
            return new VariableBinder(parent, scope).Bind();
        }

        private bool InTopLambda {
            get { return _currentLambda == _topLambda; }
        }

        private CompilerScope Bind() {
            // Define variables on this scope
            Expression body = DefineVariables();

            // Merge with child scopes
            Queue<Expression> mergedScopes = MergeScopes(ref body);

            // Walk the body to figure out which variables to hoist
            Visit(body);

            var hoisted = new List<ParameterExpression>();
            var locals = new List<ParameterExpression>();
            foreach (var v in _myVariables) {
                (_hoistVariable[v] ? hoisted : locals).Add(v);
            }
           
            // Dummy variable for hoisted locals
            ParameterExpression hoistedSelfVar = null;
            if (hoisted.Count > 0) {
                // store as an IL local
                hoistedSelfVar = Expression.Variable(typeof(object[]), "$hoistedLocals");
                locals.Add(hoistedSelfVar);
            }

            // Cache the StrongBox for hoisted variables into an IL local
            var cached = new Set<ParameterExpression>();
            foreach (var refCount in _referenceCount) {
                // Cache in local if refcount > 2
                // TODO: we need a smarter heuristic, especially with the new
                // generators; we don't want tons of stuff pulled into locals
                // each time we enter the generator
                if (refCount.Value > 2) {
                    cached.Add(refCount.Key);
                }
            }

            return new CompilerScope(
                _parentScope,
                _expression,
                _isClosure,
                hoistedSelfVar,
                new ReadOnlyCollection<ParameterExpression>(hoisted),
                new ReadOnlyCollection<ParameterExpression>(locals),
                mergedScopes,
                cached
            );
        }

        private Expression DefineVariables() {
            Expression body;
            if (_expression.NodeType == ExpressionType.Scope) {
                ScopeExpression scope = (ScopeExpression)_expression;
                foreach (var v in scope.Variables) {
                    _myVariables.Add(v);
                    _hoistVariable.Add(v, false);
                }
                body = scope.Body;
            } else {
                foreach (var v in _topLambda.Parameters) {
                    _myVariables.Add(v);
                    _hoistVariable.Add(v, false);
                }
                body = _topLambda.Body;
            }
            _currentLambda = _topLambda;
            return body;
        }

        // If the immediate child is another scope, merge it into this one
        // (This is an optimization to save environment allocations and
        // array accesses)
        private Queue<Expression> MergeScopes(ref Expression body) {
            Queue<Expression> mergedScopes = new Queue<Expression>();

            while (body.NodeType == ExpressionType.Scope) {
                ScopeExpression scope = (ScopeExpression)body;
                mergedScopes.Enqueue(scope);
                foreach (var v in scope.Variables) {
                    _myVariables.Add(v);
                    _hoistVariable.Add(v, false);
                }
                body = scope.Body;
            }

            return mergedScopes;
        }

        private void Reference(ParameterExpression variable, bool hoist) {
            // Skip variables that are shadowed by a nested scope/lambda
            foreach (Set<ParameterExpression> hidden in _hiddenVars) {
                if (hidden.Contains(variable)) {
                    return;
                }
            }

            // Increment the reference count
            int refCount;
            if (!_referenceCount.TryGetValue(variable, out refCount)) {
                refCount = 0;
            }
            _referenceCount[variable] = refCount + 1;

            // If it belongs to this scope, hoist it
            if (_hoistVariable.ContainsKey(variable)) {
                if (hoist) {
                    if (variable.IsByRef) {
                        throw Error.CannotCloseOverByRef(variable.Name, CompilerScope.GetName(_currentLambda) ?? "<unnamed>");
                    }
                    _hoistVariable[variable] = true;
                }
                return;
            }

            // If we don't have an outer scope, than it's an unbound reference
            if (_parentScope == null) {
                throw Error.UnboundVariable(variable.Name, CompilerScope.GetName(_currentLambda) ?? "<unnamed>");
            }

            // It belongs to an outer scope. Mark this scope as a closure
            _isClosure = true;
        }

        protected internal override Expression VisitParameter(ParameterExpression node) {
            Reference(node, !InTopLambda);
            return node;
        }

        protected internal override Expression VisitLambda(LambdaExpression node) {
            LambdaExpression saved = _currentLambda;
            _currentLambda = node;
            _hiddenVars.Push(new Set<ParameterExpression>(node.Parameters));
            Visit(node.Body);
            _hiddenVars.Pop();
            _currentLambda = saved;
            return node;
        }

        protected internal override Expression VisitScope(ScopeExpression node) {
            _hiddenVars.Push(new Set<ParameterExpression>(node.Variables));
            Visit(node.Body);
            _hiddenVars.Pop();
            return node;
        }

        protected internal override Expression VisitRuntimeVariables(LocalScopeExpression node) {
            // Force hoisting of these variables
            foreach (var v in node.Variables) {
                Reference(v, true);
            }
            return node;
        }
    }
}
