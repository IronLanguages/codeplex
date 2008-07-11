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

using System;
using System.Collections.Generic;
using System.Scripting;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Runtime;

namespace Microsoft.Scripting.Interpretation {

    /// <summary>
    /// Represents variable storage for one lambda/scope expression in the
    /// interpreter.
    /// </summary>
    internal sealed class InterpreterState {

        private class LambdaState {
            internal Dictionary<Expression, object> SpilledStack;
            internal YieldStatement CurrentYield;
        }

        private readonly InterpreterState _parent;
        private readonly Dictionary<Expression, object> _vars = new Dictionary<Expression, object>();
        internal SourceLocation CurrentLocation;

        // per-lambda state
        private readonly LambdaState _lambdaState;

        private InterpreterState(InterpreterState parent, LambdaState lambdaState) {
            _parent = parent;
            _lambdaState = lambdaState;
        }

        public static InterpreterState CreateForTopLambda(LambdaExpression lambda, params object[] args) {
            return CreateForLambda(null, lambda, args);
        }

        public InterpreterState CreateForLambda(LambdaExpression lambda, object[] args) {
            return CreateForLambda(this, lambda, args);
        }

        private static InterpreterState CreateForLambda(InterpreterState parent, LambdaExpression lambda, object[] args) {
            InterpreterState state = new InterpreterState(parent, new LambdaState());

            Debug.Assert(args.Length == lambda.Parameters.Count, "number of parameters should match number of arguments");
            
            //
            // Populate all parameters ...
            //
            for (int i = 0; i < lambda.Parameters.Count; i++ ) {
                state._vars.Add(lambda.Parameters[i], args[i]);
            }

            return state;
        }

        public InterpreterState CreateForScope(ScopeExpression scope) {
            InterpreterState state = new InterpreterState(this, _lambdaState);
            foreach (VariableExpression v in scope.Variables) {
                // initialize variables to default(T)
                object value;
                if (v.Type.IsValueType) {
                    value = Activator.CreateInstance(v.Type);
                } else {
                    value = null;
                }
                state._vars.Add(v, value);
            }
            return state;
        }

        public YieldStatement CurrentYield {
            get { return _lambdaState.CurrentYield; }
            set { _lambdaState.CurrentYield = value; }
        }

        public bool TryGetStackState<T>(Expression node, out T value) {
            object val;
            if (_lambdaState.SpilledStack != null && _lambdaState.SpilledStack.TryGetValue(node, out val)) {
                _lambdaState.SpilledStack.Remove(node);

                value = (T)val;
                return true;
            }

            value = default(T);
            return false;
        }

        public void SaveStackState(Expression node, object value) {
            if (_lambdaState.SpilledStack == null) {
                _lambdaState.SpilledStack = new Dictionary<Expression, object>();
            }

            _lambdaState.SpilledStack[node] = value;
        }

        public object GetValue(Expression variable) {
            InterpreterState state = this;
            for (; ; ) {
                object value;
                if (state._vars.TryGetValue(variable, out value)) {
                    return value;
                }
                state = state._parent;

                // Couldn't find variable
                if (state == null) {
                    throw InvalidVariableReference(variable);
                }
            }
        }

        public void SetValue(Expression variable, object value) {
            InterpreterState state = this;
            for (; ; ) {
                if (state._vars.ContainsKey(variable)) {
                    state._vars[variable] = value;
                    return;
                }
                state = state._parent;

                // Couldn't find variable
                if (state == null) {
                    throw InvalidVariableReference(variable);
                }
            }
        }

        private static Exception InvalidVariableReference(Expression variable) {
            return new InvalidOperationException(string.Format("Variable '{0}' is not defined in an outer scope", variable));
        }
    }
}
