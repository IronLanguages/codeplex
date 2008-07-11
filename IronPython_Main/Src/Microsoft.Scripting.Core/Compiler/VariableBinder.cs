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
using System.Diagnostics;
using System.Scripting;
using System.Scripting.Generation;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// The base class for LambdaBinder and RuleBinder.
    /// </summary>
    internal sealed class VariableBinder : ExpressionTreeVisitor {
        // Information collected about the scopes while doing variable binding
        private sealed class ScopeBindingInfo {
            // created during post processing
            internal CompilerScope CompilerScope;

            // true if this scope closes over variables in a parent lambda
            internal bool IsClosure;

            // true if this scope contains a yield statement
            internal bool HasYield;

            internal readonly LambdaExpression Lambda;
            internal readonly Expression Scope;
            internal readonly ScopeBindingInfo Parent;
            internal readonly List<VariableBindingInfo> Variables = new List<VariableBindingInfo>();
            internal readonly List<VariableExpression> GeneratorTemps = new List<VariableExpression>();

            internal ScopeBindingInfo(Expression scope, LambdaExpression lambda, ScopeBindingInfo parent) {
                Scope = scope;
                Lambda = lambda;
                Parent = parent;
            }

            internal bool HoistAll {
                get {
                    return Lambda.NodeType == ExpressionType.Generator && (HasYield || Scope == Lambda);
                }
            }

            internal VariableBindingInfo AddVariable(Expression variable) {
                VariableBindingInfo result = new VariableBindingInfo(variable, this);
                Variables.Add(result);
                return result;
            }

            internal void AddGeneratorTemp(Type type) {
                VariableExpression temp = Expression.Variable(type, "temp$" + GeneratorTemps.Count);
                AddVariable(temp);
                GeneratorTemps.Add(temp);
            }
        }

        // Information collected about the variables while doing variable binding
        private sealed class VariableBindingInfo {
            // true if this variable is hoisted to an array
            internal bool IsHoisted;
            internal readonly Expression Variable;
            internal readonly ScopeBindingInfo Scope;

            internal VariableBindingInfo(Expression variable, ScopeBindingInfo scope) {
                Variable = variable;
                Scope = scope;
            }
        }

        // The list of all scopes, in prefix order, for post processing
        private readonly List<ScopeBindingInfo> _scopeList = new List<ScopeBindingInfo>();

        // The dictionary of all scopes and their infos in the tree
        private Dictionary<Expression, ScopeBindingInfo> _scopes = new Dictionary<Expression, ScopeBindingInfo>();

        // The dictionary of all variables and their infos in the tree
        private Dictionary<Expression, VariableBindingInfo> _variables = new Dictionary<Expression, VariableBindingInfo>();

        // The dictionary of all generators and their infos in the tree.
        private Dictionary<LambdaExpression, GeneratorInfo> _generators;

        // The CompilerScopes that are the result of the variable binder
        private Dictionary<Expression, CompilerScope> _resultScopes;

        // Stack to keep track of scope nesting.
        private Stack<ScopeBindingInfo> _stack = new Stack<ScopeBindingInfo>();

        private VariableBinder() {
        }

        #region entry point

        /// <summary>
        /// LambdaBinder entry point.
        /// </summary>
        internal static AnalyzedTree Bind(LambdaExpression ast) {
            return new VariableBinder().BindLambda(ast);
        }

        #endregion

        private AnalyzedTree BindLambda(LambdaExpression lambda) {
            // Collect the lambdas
            Visit(lambda);
            BindTheScopes();

            return new AnalyzedTree(_resultScopes, _generators);
        }

        private LambdaExpression GetTopLambda() {
            return _stack.Peek().Lambda;
        }

        #region ExpressionVisitor overrides

        protected override Expression Visit(VariableExpression node) {
            Reference(node);
            return node;
        }

        protected override Expression Visit(ParameterExpression node) {
            Reference(node);
            return node;
        }

        protected override Expression Visit(LambdaExpression node) {
            if (Push(node)) {
                base.Visit(node);
            }

            if (node.NodeType == ExpressionType.Generator) {
                MakeGeneratorInfo(node);
            }

            ScopeBindingInfo sbi = Pop();
            Debug.Assert(sbi.Scope == node);
            return node;
        }

        private void MakeGeneratorInfo(LambdaExpression node) {
            if (_generators == null) {
                _generators = new Dictionary<LambdaExpression, GeneratorInfo>();
            }

            // the info may already exist if we've already processed this lambda
            // (i.e. it appears in multiple places in the tree)
            if (!_generators.ContainsKey(node)) {

                int tempsNeeded;
                GeneratorInfo gi = YieldLabelBuilder.BuildYieldTargets(node, out tempsNeeded);
                _generators.Add(node, gi);

                for (int i = 0; i < tempsNeeded; i++) {
                    _stack.Peek().AddGeneratorTemp(typeof(Exception));
                }
                    
                // For scopes inside a generator, we need to hoist the scope
                // access slots. Otherwise, the scope would be null when
                // calling back into the generator.
                for (int i = _scopeList.Count - 1; i >= 0; i--) {
                    ScopeBindingInfo s = _scopeList[i];

                    // stop when we get to the generator itself
                    if (s.Scope == node) {
                        break;
                    }

                    // If this scope exists immediately in the generator and
                    // yields, we want to hoist its access slot and use a
                    // generator temp instead of an IL local
                    if (s.Lambda == node && s.HasYield) {
                        _stack.Peek().AddGeneratorTemp(typeof(object[]));

                        // mark closures up to the generator
                        for (ScopeBindingInfo t = s; t.Scope != node; t = t.Parent) {
                            t.IsClosure = true;
                        }
                    }
                }
            }
        }

        // This may not belong here because it is checking for the
        // AST type consistency. However, since it is the only check
        // it seems unwarranted to make an extra walk of the AST just
        // to verify this condition.
        protected override Expression Visit(ReturnStatement node) {
            LambdaExpression lambda = GetTopLambda();
            Type returnType = CompilerHelpers.GetReturnType(lambda);

            if (node.Expression != null) {
                if (lambda.NodeType == ExpressionType.Lambda) {
                    if (!returnType.IsAssignableFrom(node.Expression.Type)) {
                        throw InvalidReturnStatement("Invalid type of return expression value", node);
                    }
                }
            } else {
                // return without expression can be only from lambda with void return type
                if (returnType != typeof(void)) {
                    throw InvalidReturnStatement("Missing return expression", node);
                }
            }

            return base.Visit(node);
        }

        private static ArgumentException InvalidReturnStatement(string message, ReturnStatement node) {
            SourceSpan span = node.Annotations.Get<SourceSpan>();
            return new ArgumentException(
                String.Format(
                    "{0} at {1}:{2}-{3}:{4}", message,
                    span.Start.Line, span.Start.Column, span.End.Line, span.End.Column
                )
            );
        }

        protected override Expression Visit(ScopeExpression node) {
            if (Push(node)) {
                base.Visit(node);
            }

            ScopeBindingInfo sbi = Pop();
            Debug.Assert(sbi.Scope == node);
            return node;
        }

        protected override CatchBlock Visit(CatchBlock node) {
            if (node.Variable != null) {
                Reference(node.Variable);
            }
            return base.Visit(node);
        }

        protected override Expression Visit(YieldStatement node) {
            // Mark the whole scope chain in this generator as having a yield
            ScopeBindingInfo sbi = _stack.Peek();

            ScopeBindingInfo current = sbi;
            while (current.Scope.NodeType != ExpressionType.Generator) {
                Debug.Assert(current.Lambda == sbi.Lambda);

                current.HasYield = true;
                current = current.Parent;
            }

            return base.Visit(node);
        }

        protected override Expression Visit(LocalScopeExpression node) {
            // Ensure that all variables are defined in an outer scope
            // Force hoisting of variables if needed

            ScopeBindingInfo current = _stack.Peek();
            foreach (Expression v in node.Variables) {
                // Make sure the variable is defined
                VariableBindingInfo vbi;
                if (!_variables.TryGetValue(v, out vbi)) {
                    throw InvalidVariableReference(v);
                }

                // Cannot access ref parameters via local scope expression
                if (IsByRefParameter(v)) {
                    throw InvalidVariableReference(v);
                }

                // Close over the variable if necessary
                if (node.IsClosure) {
                    // reference it from this scope
                    Reference(v);

                    // force hoisting even if it's in the same lambda
                    vbi.IsHoisted = true;
                }
            }

            return base.Visit(node);
        }

        protected override Expression Visit(BinaryExpression node) {
            if (node.Conversion != null) {
                Visit(node.Conversion);
            }
            return base.Visit(node);
        }

        #endregion

        #region processing scope expressions

        private bool Push(Expression scope) {
            // We've seen this scope already
            // (referenced from multiple places in the tree)
            ScopeBindingInfo sbi;
            if (_scopes.TryGetValue(scope, out sbi)) {
                // Push the expression so PostWalk can pop it
                _stack.Push(sbi);
                return false;
            }

            // The parent of the scope is the scope currently
            // on top of the stack, or a null if at top level.
            ScopeBindingInfo parent = null;
            LambdaExpression lambda = null;

            if (_stack.Count > 0) {
                parent = _stack.Peek();
                lambda = parent.Lambda;
            }

            if (scope.NodeType != ExpressionType.Scope) {
                lambda = (LambdaExpression)scope;
            }

            sbi = new ScopeBindingInfo(scope, lambda, parent);

            // Remember we saw the scope already
            _scopes[scope] = sbi;

            // Store it for post processing
            _scopeList.Add(sbi);

            // And push it on the stack.
            _stack.Push(sbi);

            // define variables in this scope
            DefineVariables(scope);

            // if the child of this scope is another scope, merge them
            return MergeWithChildScope(scope);
        }

        // Merges variables from child scopes into the parent
        // This is an optimization for the common patterm:
        //      Expression.Lambda(Expression.Scope(...), ...)
        //
        // It saves both array allocations and speeds up access
        //
        private bool MergeWithChildScope(Expression scope) {

            Expression body;
            if (scope.NodeType == ExpressionType.Scope) {
                body = ((ScopeExpression)scope).Body;
            } else {
                body = ((LambdaExpression)scope).Body;
            }

            if (body.NodeType == ExpressionType.Scope) {

                // While the body is a ScopeExpression, merge the variables
                // into this scope
                do {
                    ScopeExpression se = (ScopeExpression)body;
                    foreach (VariableExpression v in se.Variables) {
                        DefineVariable(v);
                    }
                    body = se.Body;

                } while (body.NodeType == ExpressionType.Scope);

                // we finally found a non-scope body, walk it
                VisitNode(body);

                // return false so we don't walk the body again
                return false;
            }

            // continue normal walking of the tree
            return true;
        }

        private void DefineVariables(Expression scope) {
            if (scope.NodeType == ExpressionType.Scope) {
                ScopeExpression se = (ScopeExpression)scope;
                foreach (VariableExpression v in se.Variables) {
                    DefineVariable(v);
                }
            } else {
                LambdaExpression le = (LambdaExpression)scope;
                foreach (ParameterExpression p in le.Parameters) {
                    DefineVariable(p);
                }
            }
        }

        private void DefineVariable(Expression v) {
            if (_variables.ContainsKey(v)) {
                throw InvalidVariableDefinition(v);
            }

            _variables.Add(v, _stack.Peek().AddVariable(v));
        }

        private ScopeBindingInfo Pop() {
            Debug.Assert(_stack != null && _stack.Count > 0);
            ScopeBindingInfo sbi = _stack.Pop();

            // Remove variables that are no longer in scope
            // (this allows other parallel scopes to reuse variable instances)
            foreach (VariableBindingInfo vbi in sbi.Variables) {
                _variables.Remove(vbi.Variable);
            }

            return sbi;
        }

        #endregion

        #region Closure resolution

        /// <summary>
        /// Called when a variable is referenced inside the current lambda.
        /// 
        /// If the variable is defined on an outer lambda:
        ///   1. Mark the current lambda and lambdas between as closures
        ///   2. Mark the variable as hoisted
        /// </summary>
        private void Reference(Expression variable) {
            Debug.Assert(variable != null && _stack != null && _stack.Count > 0);

            VariableBindingInfo vbi;
            if (!_variables.TryGetValue(variable, out vbi)) {
                throw InvalidVariableReference(variable);
            }

            ScopeBindingInfo referenceScope = _stack.Peek();
            ScopeBindingInfo definingScope = vbi.Scope;

            // Mark all scopes between the use and the definition as closures.
            // If the variable ends up getting hoisted, we need to be able to
            // get at its hoisted locals.
            for (ScopeBindingInfo s = referenceScope; s != definingScope; s = s.Parent) {
                s.IsClosure = true;
            }

            // If it's defined in this lambda, don't hoist it.
            // Only hoist variables that are closed over, or need to be hoisted
            // for some other reason (generators, scope access)
            if (definingScope.Lambda != referenceScope.Lambda) {
                // Cannot close over ref parameters
                if (IsByRefParameter(variable)) {
                    throw InvalidVariableReference(variable);
                }
                vbi.IsHoisted = true;
            }
        }

        private Exception InvalidVariableReference(Expression variable) {
            return new InvalidOperationException(
                string.Format(
                    "Variable '{0}' referenced from scope '{1}', but it is not defined in an outer scope",
                    CompilerHelpers.GetVariableName(variable),
                    CompilerScope.GetName(_stack.Peek().Scope) ?? "<unnamed>"
                )
            );
        }

        private Exception InvalidVariableDefinition(Expression variable) {
            return new InvalidOperationException(
                string.Format(
                    "Variable '{0}' definined in scope '{1}', but it is already defined in '{2}'",
                    CompilerHelpers.GetVariableName(variable),
                    CompilerScope.GetName(_stack.Peek().Scope) ?? "<unnamed>",
                    CompilerScope.GetName(_variables[variable].Scope.Scope) ?? "<unnamed>"
                )
            );
        }

        /// <summary>
        /// Post processing of the tree:
        ///   1. Hoist all locals if necessary (generators, scopes with yield)
        ///   2. Group variables into hoisted, locals, and globals
        ///   3. Finally, create a CompilerScopes for each scope we encountered
        /// </summary>
        private void BindTheScopes() {
            Debug.Assert(_stack.Count == 0);

            // free items we don't need anymore
            _variables = null;
            _scopes = null;
            _stack = null;

            // allocate the result dictionary
            _resultScopes = new Dictionary<Expression, CompilerScope>(_scopeList.Count);

            // walk each scope, and build the CompilerScope
            foreach (ScopeBindingInfo sbi in _scopeList) {
                Debug.Assert(sbi.CompilerScope == null);

                BindScope(sbi);                
            }
        }

        private void BindScope(ScopeBindingInfo sbi) {
            List<Expression> hoisted = new List<Expression>();
            List<Expression> locals = new List<Expression>();

            foreach (VariableBindingInfo vbi in sbi.Variables) {
                if (vbi.IsHoisted || vbi.Scope.HoistAll) {
                    hoisted.Add(vbi.Variable);
                } else {
                    locals.Add(vbi.Variable);
                }
            }

            CompilerScope parent = null;
            if (sbi.Parent != null) {
                parent = sbi.Parent.CompilerScope;
                Debug.Assert(parent != null);
            }

            CompilerScope si = new CompilerScope(
                parent,
                sbi.Scope,
                sbi.IsClosure,
                sbi.HasYield,
                new ReadOnlyCollection<Expression>(hoisted),
                new ReadOnlyCollection<Expression>(locals),
                DefaultReadOnlyCollection<VariableExpression>.Empty
            );

            // Create the scope for the inner generator.
            //
            // The inner generator scope doesn't have any variables, but closes
            // over all variables from the outer generator method.
            //
            // We do this here so we don't have to play games with state inside
            // CompilerScope, by ensuring that the scope chain mirrors the
            // actual generated code.
            if (si.Expression.NodeType == ExpressionType.Generator) {
                si = new CompilerScope(
                    si,
                    sbi.Scope,
                    true,  // isClosure
                    false, // hasYield
                    DefaultReadOnlyCollection<Expression>.Empty,
                    DefaultReadOnlyCollection<Expression>.Empty,
                    new ReadOnlyCollection<VariableExpression>(sbi.GeneratorTemps)                    
                );
            }

            _resultScopes.Add(si.Expression, sbi.CompilerScope = si);
        }

        #endregion

        private static bool IsByRefParameter(Expression var) {
            ParameterExpression pe = var as ParameterExpression;
            return pe != null && pe.IsByRef;
        }
    }
}
