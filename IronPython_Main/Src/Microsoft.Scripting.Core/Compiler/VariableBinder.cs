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
using System.Collections.ObjectModel;
using System.Scripting;
using System.Scripting.Utils;

namespace System.Linq.Expressions.Compiler {
    /// <summary>
    /// Determines if variables are closed over in nested lambdas and need to
    /// be hoisted.
    /// 
    /// TODO: Here's a thought for improvement. We should have a seperate
    /// walker for binding inner lambdas verse the scope we're binding. That
    /// would elimininate the checks for "currentLambda" all over the place.
    /// Finally, we might want to have a derived class for binding generators
    /// since they need to do a bit of extra work.
    /// </summary>
    internal sealed class VariableBinder : ExpressionTreeVisitor {

        // The parent scope to the one we're binding, or null if we're binding
        // the outermost scope
        private readonly CompilerScope _parentScope;

        // The scope/lambda/generator we're binding
        private readonly Expression _expression;

        // A stack of variables that are defined in nested scopes. We search
        // this first when resolving a variable in case a nested scope shadows
        // one of our variable insteances.
        private readonly Stack<Set<Expression>> _hiddenVars = new Stack<Set<Expression>>();

        // For each variable in this scope: should it be hoisted to a closure?
        private readonly Dictionary<Expression, bool> _hoistMyVariables = new Dictionary<Expression, bool>();

        // For each variable referenced from this scope, this stores the
        // reference count. If the variable is referenced "enough", we'll cache
        // the closure StrongBox<T> into an IL local
        private readonly Dictionary<Expression, int> _referenceCount = new Dictionary<Expression, int>();

        // A list of variables in this scope, including merged ones.
        // Needed so we can preserve the order
        private readonly List<Expression> _myVariables = new List<Expression>();

        // The lambda that contains the scope we're binding
        // Or the lambda itself if we're binding a lambda
        private readonly LambdaExpression _topLambda;

        // The lambda that we're walking in
        private LambdaExpression _currentLambda;

        // Does this scope contain a yield?
        private bool _hasYield;

        // Is this scope a closure?
        // (references variables from an outer scope)
        private bool _isClosure;

        // A list of temps that need to be hoisted by the generator
        private readonly List<VariableExpression> _temps;

        // Stack of scopes. Only used if this is a generator
        private readonly Stack<ScopeExpression> _scopes;

        private VariableBinder(CompilerScope parent, Expression scope) {
            _parentScope = parent;
            _expression = scope;
            if (scope.NodeType == ExpressionType.Scope) {
                _topLambda = parent.Lambda;
            } else {
                _topLambda = (LambdaExpression)scope;
                if (scope.NodeType == ExpressionType.Generator) {
                    _temps = new List<VariableExpression>();
                    _scopes = new Stack<ScopeExpression>();
                }
            }
        }

        /// <summary>
        /// VariableBinder entry point. Binds the scope that is passed in.
        /// </summary>
        internal static CompilerScope Bind(CompilerScope parent, Expression scope) {
            return new VariableBinder(parent, scope).Bind();
        }

        private CompilerScope Bind() {
            // Define variables on this scope
            Expression body = DefineVariables();

            // Merge with child scopes
            Queue<Expression> mergedScopes = MergeScopes(ref body);

            // Walk the body to figure out which variables to hoist
            VisitNode(body);

            List<Expression> hoisted = new List<Expression>();
            List<Expression> locals = new List<Expression>();
            if (_hasYield) {
                hoisted = _myVariables;
            } else {
                foreach (Expression v in _myVariables) {
                    (_hoistMyVariables[v] ? hoisted : locals).Add(v);
                }
            }

            // Hoist generator temps
            KeyedQueue<Type, VariableExpression> temps = null;
            if (_temps != null) {
                temps = new KeyedQueue<Type, VariableExpression>();
                foreach (VariableExpression v in _temps) {
                    hoisted.Add(v);
                    temps.Enqueue(v.Type, v);
                }
            }
            
            // Dummy variable for hoisted locals
            VariableExpression hoistedSelfVar;
            if (_hasYield && _expression.NodeType == ExpressionType.Scope) {
                // store on generator's closure
                hoistedSelfVar = _parentScope.GetGeneratorTemp(typeof(object[]));
            } else {
                // store as an IL local
                hoistedSelfVar = Expression.Variable(typeof(object[]), "$hoistedLocals");
                locals.Add(hoistedSelfVar);
            }

            // Cache the StrongBox for hoisted variables into an IL local
            Set<Expression> cached = new Set<Expression>();
            foreach (var refCount in _referenceCount) {
                // Cache in local if refcount > 1
                // TODO: What's the optimal value? How cheap are locals?
                if (refCount.Value > 1) {
                    cached.Add(refCount.Key);
                }
            }

            return new CompilerScope(
                _parentScope,
                _expression,
                _isClosure,
                hoistedSelfVar,
                new ReadOnlyCollection<Expression>(hoisted),
                new ReadOnlyCollection<Expression>(locals),
                mergedScopes,
                temps,
                cached
            );
        }

        private Expression DefineVariables() {
            Expression body;
            if (_expression.NodeType == ExpressionType.Scope) {
                ScopeExpression scope = (ScopeExpression)_expression;
                foreach (Expression v in scope.Variables) {
                    _myVariables.Add(v);
                    _hoistMyVariables.Add(v, false);
                }
                body = scope.Body;
            } else {
                foreach (Expression v in _topLambda.Parameters) {
                    _myVariables.Add(v);
                    _hoistMyVariables.Add(v, false);
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
                foreach (Expression v in scope.Variables) {
                    _myVariables.Add(v);
                    _hoistMyVariables.Add(v, false);
                }
                body = scope.Body;
            }

            return mergedScopes;
        }

        private void Reference(Expression variable, bool hoist) {
            // Skip variables that belong to another scope/lambda
            foreach (Set<Expression> hidden in _hiddenVars) {
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
            if (_hoistMyVariables.ContainsKey(variable)) {
                if (hoist) {
                    EnsureNotByRef(variable);
                    _hoistMyVariables[variable] = true;
                }
                return;
            }

            // If we don't have an outer scope, than it's an unbound reference
            if (_parentScope == null) {
                throw Error.UnboundVariable(CompilerScope.GetName(variable), CompilerScope.GetName(_currentLambda) ?? "<unnamed>");
            }

            // It belongs to an outer scope. Mark this scope as a closure
            _isClosure = true;
        }

        #region ExpressionVisitor overrides

        protected override Expression Visit(VariableExpression node) {
            Reference(node, ShouldHoist);
            return node;
        }

        protected override Expression Visit(ParameterExpression node) {
            Reference(node, ShouldHoist);
            return node;
        }

        protected override Expression Visit(LambdaExpression node) {
            LambdaExpression saved = _currentLambda;
            _currentLambda = node;
            _hiddenVars.Push(new Set<Expression>(node.Parameters));
            VisitNode(node.Body);
            _hiddenVars.Pop();
            _currentLambda = saved;
            return node;
        }

        protected override Expression Visit(ScopeExpression node) {
            if (_currentLambda == _topLambda && _scopes != null) {
                _scopes.Push(node);
            }
            _hiddenVars.Push(new Set<Expression>(node.Variables));
            VisitNode(node.Body);
            _hiddenVars.Pop();
            // may have been popped already
            if (_currentLambda == _topLambda &&
                _scopes != null && _scopes.Count > 0 && _scopes.Peek() == node) {
                _scopes.Pop();
            }
            return node;
        }

        protected override Expression Visit(LocalScopeExpression node) {
            // Force hoisting of these variables
            foreach (Expression v in node.Variables) {
                Reference(v, true);
            }
            return node;
        }

        protected override Expression Visit(YieldStatement node) {
            // If we're directly in the lambda/scope
            if (_hiddenVars.Count == 0) {
                _hasYield = true;
                // If this is a scope, it needs access to its generator temp,
                // so mark it as a closure
                if (_expression.NodeType == ExpressionType.Scope) {
                    _isClosure = true;
                }
            }

            // if we're directly inside the lambda we're binding
            if (_currentLambda == _expression) {

                // Validation: yield cannot appear outside of a generator
                if (_expression.NodeType == ExpressionType.Lambda) {
                    throw Error.YieldOutsideOfGenerator(CompilerScope.GetName(_expression) ?? "<unnamed>");
                }

                // Create a generator temp for storing each scope's environment
                // TODO: only scopes that actually have environments need slots
                for (int i = 0; i < _scopes.Count; i++) {
                    _temps.Add(Expression.Variable(typeof(object[]), "tempScope$" + _temps.Count));
                }
                // empty the stack so we don't create temps for these scopes again
                _scopes.Clear();
            }

            return base.Visit(node);
        }

        protected override Expression Visit(TryStatement node) {
            if (_temps != null && (node.Finally ?? node.Fault) != null) {
                _temps.Add(Expression.Variable(typeof(Exception), "tempException$" + _temps.Count));
            }
            return base.Visit(node);
        }

        // This may not belong here because it is checking for the
        // tree type consistency. However, since it is the only check
        // it seems unwarranted to make an extra walk of the tree just
        // to verify this condition.
        protected override Expression Visit(ReturnStatement node) {
            // If we're binding the lambda, and we're in it or in ones of its
            // scopes, check return
            if (_currentLambda == _expression) {
                Type returnType = _currentLambda.ReturnType;

                if (node.Expression != null) {
                    // BUG: should be TypeUtils.AreReferenceAssignable !!!
                    if (_currentLambda.NodeType == ExpressionType.Lambda &&
                        !returnType.IsAssignableFrom(node.Expression.Type)) {

                        throw Error.InvalidReturnTypeOfLambda(node.Expression.Type, returnType, _currentLambda.Name);
                    }
                } else if (returnType != typeof(void)) {
                    // return without expression can be only from lambda with void return type
                    throw Error.MissingReturnForLambda(_currentLambda.Name, returnType);
                }
            }

            return base.Visit(node);
        }

        #endregion

        private bool ShouldHoist {
            // Hoist variables referenced from an inner lambda, or variables in
            // a generator (which are effectively referenced from an inner
            // method thanks to how we compile it)
            get { return _topLambda != _currentLambda || _topLambda.NodeType == ExpressionType.Generator; }
        }

        // Cannot close over ref parameters
        private void EnsureNotByRef(Expression variable) {
            ParameterExpression pe = variable as ParameterExpression;
            if (pe != null && pe.IsByRef) {
                throw Error.CannotCloseOverByRef(CompilerScope.GetName(variable), CompilerScope.GetName(_currentLambda) ?? "<unnamed>");
            }
        }
    }
}
