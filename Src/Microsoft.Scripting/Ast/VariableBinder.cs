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
using System.Diagnostics;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// The base class for ClosureBinder and RuleBinder.
    /// </summary>
    abstract class VariableBinder : Walker {
        /// <summary>
        /// List to store all context statements for further processing - storage allocation
        /// </summary>
        private List<LambdaInfo> _lambdas;

        /// <summary>
        /// The dictionary of all lambdas and their infos in the tree.
        /// </summary>
        private Dictionary<LambdaExpression, LambdaInfo> _infos;

        /// <summary>
        /// Stack to keep track of the lambda nesting.
        /// </summary>
        private Stack<LambdaInfo> _stack;

        protected List<LambdaInfo> Lambdas {
            get { return _lambdas; }
        }

        protected Dictionary<LambdaExpression, LambdaInfo> Infos {
            get { return _infos; }
        }

        protected Stack<LambdaInfo> Stack {
            get { return _stack; }
        }

        private Stack<LambdaInfo> NonNullStack {
            get {
                if (_stack == null) {
                    _stack = new Stack<LambdaInfo>();
                }
                return _stack;
            }
        }

        protected VariableBinder() {
        }

        #region Walker overrides

        protected internal override bool Walk(BoundAssignment node) {
            Reference(node.Variable);
            return true;
        }

        protected internal override bool Walk(BoundExpression node) {
            Reference(node.Variable);
            return true;
        }

        protected internal override bool Walk(DeleteStatement node) {
            Reference(node.Variable);
            return true;
        }

        protected internal override bool Walk(CatchBlock node) {
            // CatchBlock is not required to have target variable
            if (node.Variable != null) {
                Reference(node.Variable);
            }
            return true;
        }

        protected internal override bool Walk(LambdaExpression node) {
            return Push(node);
        }

        protected internal override void PostWalk(LambdaExpression node) {
            LambdaInfo li = Pop();
            Debug.Assert(li.Lambda == node);
        }

        protected internal override bool Walk(GeneratorCodeBlock node) {
            return Push(node);
        }

        protected internal override void PostWalk(GeneratorCodeBlock node) {
            LambdaInfo li = Pop();
            Debug.Assert(li.Lambda == node);
            Debug.Assert(li.TopTargets == null);

            // Build the yield targets and store them in the cbi
            YieldLabelBuilder.BuildYieldTargets(node, li);
        }

        #endregion

        protected virtual void Reference(Variable variable) {
            Debug.Assert(variable != null);
            _stack.Peek().Reference(variable);
        }

        private bool Push(LambdaExpression lambda) {
            if (_infos == null) {
                _infos = new Dictionary<LambdaExpression, LambdaInfo>();
                _lambdas = new List<LambdaInfo>();
            }

            // We've seen this lambda already
            // (referenced from multiple CodeBlockExpressions)
            if (_infos.ContainsKey(lambda)) {
                return false;
            }

            Stack<LambdaInfo> stack = NonNullStack;

            // The parent of the lambda is the lambda currently
            // on top of the stack, or a null if at top level.
            LambdaInfo parent = stack.Count > 0 ? stack.Peek() : null;
            LambdaInfo li = new LambdaInfo(lambda, parent);

            // Add the lambda to the list.
            // The lambdas are added in prefix order so they
            // will have to be reversed for name binding
            _lambdas.Add(li);

            // Remember we saw the lambda already
            _infos[lambda] = li;

            // And push it on the stack.
            stack.Push(li);

            return true;
        }

        private LambdaInfo Pop() {
            return NonNullStack.Pop();
        }

        private void AddGeneratorTemps(int count) {
            Debug.Assert(_stack.Count > 0);
            _stack.Peek().AddGeneratorTemps(count);
        }

        #region Closure resolution

        protected void BindTheScopes() {
            if (_lambdas != null) {
                // Process the lambdas in post-order so that
                // all children are processed before parent
                int i = _lambdas.Count;
                while (i-- > 0) {
                    LambdaInfo li = _lambdas[i];
                    if (!li.Lambda.IsGlobal) {
                        BindLambda(li);
                    }
                }
            }
        }

        private void BindLambda(LambdaInfo lambdaInfo) {
            // If the function is generator or needs custom frame,
            // lift locals to closure
            LambdaExpression lambda = lambdaInfo.Lambda;
            if (lambda is GeneratorCodeBlock || lambda.EmitLocalDictionary) {
                LiftLocalsToClosure(lambdaInfo);
            }
            ResolveClosure(lambdaInfo);
        }

        private static void LiftLocalsToClosure(LambdaInfo lambdaInfo) {
            // Lift all parameters
            foreach (Variable p in lambdaInfo.Lambda.Parameters) {
                p.LiftToClosure();
            }
            // Lift all locals
            foreach (Variable d in lambdaInfo.Lambda.Variables) {
                if (d.Kind == VariableKind.Local) {
                    d.LiftToClosure();
                }
            }
            lambdaInfo.HasEnvironment = true;
        }

        private void ResolveClosure(LambdaInfo li) {
            LambdaExpression lambda = li.Lambda;

            foreach (VariableReference r in li.References.Values) {
                Debug.Assert(r.Variable != null);

                if (r.Variable.Lambda == lambda) {
                    // local reference => no closure
                    continue;
                }

                // Global variables as local
                if (r.Variable.Kind == VariableKind.Global ||
                    (r.Variable.Kind == VariableKind.Local && r.Variable.Lambda.IsGlobal)) {
                    continue;
                }

                // Lift the variable into the closure
                r.Variable.LiftToClosure();

                // Mark all parent scopes between the use and the definition
                // as closures/environment
                LambdaInfo current = li;
                do {
                    current.IsClosure = true;

                    LambdaInfo parent = current.Parent;

                    if (parent == null) {
                        throw new ArgumentException(
                            String.Format(
                                "Cannot resolve variable '{0}' " +
                                "referenced from lambda '{1}' " +
                                "and defined in lambda {2}).\n" +
                                "Is LambdaExpression.Parent set correctly?",
                                SymbolTable.IdToString(r.Variable.Name),
                                li.Lambda.Name ?? "<unnamed>",
                                r.Variable.Lambda != null ? (r.Variable.Lambda.Name ?? "<unnamed>") : "<unknown>"
                            )
                        );
                    }

                    parent.HasEnvironment = true;
                    current = parent;
                } while (current.Lambda != r.Variable.Lambda);
            }
        }

        private LambdaInfo GetLambdaInfo(LambdaExpression lambda) {
            return _infos[lambda];
        }

        #endregion
    }
}
