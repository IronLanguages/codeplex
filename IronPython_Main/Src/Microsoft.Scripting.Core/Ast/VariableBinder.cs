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
using System.Diagnostics;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// The base class for LambdaBinder and RuleBinder.
    /// </summary>
    internal abstract class VariableBinder : Walker {
        /// <summary>
        /// List to store all context statements for further processing - storage allocation
        /// </summary>
        private List<LambdaInfo> _lambdas;

        /// <summary>
        /// The dictionary of all lambdas and their infos in the tree.
        /// </summary>
        private Dictionary<LambdaExpression, LambdaInfo> _infos;

        /// <summary>
        /// The dictionary of all generators and their infos in the tree.
        /// </summary>
        private Dictionary<GeneratorLambdaExpression, GeneratorInfo> _generators;

        /// <summary>
        /// The dictionary of all variables and their infos in the tree
        /// </summary>
        private Dictionary<Expression, VariableInfo> _definitions = new Dictionary<Expression, VariableInfo>();

        /// <summary>
        /// Stack to keep track of the lambda nesting.
        /// </summary>
        private Stack<LambdaInfo> _stack = new Stack<LambdaInfo>();

        protected List<LambdaInfo> Lambdas {
            get { return _lambdas; }
        }

        protected Dictionary<LambdaExpression, LambdaInfo> Infos {
            get { return _infos; }
        }

        protected Dictionary<GeneratorLambdaExpression, GeneratorInfo> Generators {
            get { return _generators; }
        }

        protected Stack<LambdaInfo> Stack {
            get { return _stack; }
        }

        protected VariableBinder() {
        }

        #region Walker overrides

        protected override bool Walk(VariableExpression node) {
            Reference(node);
            return true;
        }

        protected override bool Walk(ParameterExpression node) {
            Reference(node);
            return true;
        }

        protected override bool Walk(CatchBlock node) {
            // CatchBlock is not required to have target variable
            if (node.Variable != null) {
                Reference(node.Variable);
            }
            return true;
        }

        protected override bool Walk(LambdaExpression node) {
            return Push(node);
        }

        protected override void PostWalk(LambdaExpression node) {
            LambdaInfo li = Pop();
            Debug.Assert(li.Lambda == node);
        }

        protected override bool Walk(GeneratorLambdaExpression node) {
            return Push(node);
        }

        protected override void PostWalk(GeneratorLambdaExpression node) {
            LambdaInfo li = Pop();
            Debug.Assert(li.Lambda == node);

            if (_generators == null) {
                _generators = new Dictionary<GeneratorLambdaExpression, GeneratorInfo>();
            }

            // the info may already exist if we've already processed this lambda
            // (i.e. it appears in multiple places in the tree)
            if (!_generators.ContainsKey(node)) {
                _generators.Add(node, YieldLabelBuilder.BuildYieldTargets(node, li));
            }
        }

        #endregion

        private bool Push(LambdaExpression lambda) {
            if (_infos == null) {
                _infos = new Dictionary<LambdaExpression, LambdaInfo>();
                _lambdas = new List<LambdaInfo>();
            }

            // We've seen this lambda already
            // (referenced from multiple LambdaExpressions)
            LambdaInfo li;
            if (_infos.TryGetValue(lambda, out li)) {
                // Push the expression so PostWalk can pop it
                _stack.Push(li);
                return false;
            }

            // The parent of the lambda is the lambda currently
            // on top of the stack, or a null if at top level.
            LambdaInfo parent = _stack.Count > 0 ? _stack.Peek() : null;
            li = new LambdaInfo(lambda, parent);

            // Add the lambda to the list.
            // The lambdas are added in prefix order so they
            // will have to be reversed for name binding
            _lambdas.Add(li);

            // Remember we saw the lambda already
            _infos[lambda] = li;

            // And push it on the stack.
            _stack.Push(li);

            DefineParameters(li, lambda.Parameters, 0);

            // Mark the lambda's variables as referenced within this lambda.
            // Treat them this way so we have a uniform way of binding local
            // variables, so the lambda's variables are just a shorthand way
            // of denoting a reference.
            foreach (VariableExpression v in lambda.Variables) {
                Reference(v);
            }

            return true;
        }

        // Defines the parameters as belonging to this lambda
        protected void DefineParameters(LambdaInfo li, IEnumerable<ParameterExpression> parameters, int startIndex) {
            foreach (ParameterExpression p in parameters) {
                _definitions.Add(p, new VariableInfo(p, li, startIndex++));
            }
        }

        private LambdaInfo Pop() {
            Debug.Assert(_stack != null && _stack.Count > 0);
            return _stack.Pop();
        }

        #region Closure resolution

        /// <summary>
        /// Called when a variable is referenced inside the current lambda.
        /// 
        ///   1. If the variable is already defined on another lambda, we move
        ///      its definition to the common parent lambda, marking closures
        ///      along the way.
        ///      
        ///   2. If the variable has not been defined yet, assume it's defined
        ///      on the current lambda
        /// </summary>
        protected virtual void Reference(Expression variable) {
            Debug.Assert(variable != null && _stack != null && _stack.Count > 0);

            LambdaInfo current = _stack.Peek();

            // Add the variable to the reference slots
            current.ReferenceSlots[variable] = null;

            // Find where we think the variable is defined
            VariableInfo vi;
            if (!_definitions.TryGetValue(variable, out vi)) {
                // Not found: assume it is defined here
                VariableExpression v = variable as VariableExpression;
                if (v == null) {
                    // parameters are added when the rule/lambda is encountered,
                    // so we shouldn't get here except by an invalid tree
                    throw InvalidParameterReference(variable);
                }

                if (v.NodeType == AstNodeType.GlobalVariable) {
                    // Global variables go on the root of the tree
                    while (current.Parent != null) {
                        current = current.Parent;
                    }
                }

                _definitions.Add(variable, new VariableInfo(v, current));
                return;
            }

            LambdaInfo definingLambda = vi.LambdaInfo;
            
            if (definingLambda == current || vi.IsGlobal) {
                // If it's already defined in this lambda or it's global, nothing to do
                return;
            }

            LambdaInfo common = FindCommonParent(current, definingLambda);

            if (variable.NodeType == AstNodeType.Parameter && common != definingLambda) {
                // parameter referenced outside its defining lambda
                throw InvalidParameterReference(variable);
            }

            // Mark all scopes between the use and the definition
            // as closures/environment
            MarkClosures(current, common);
            MarkClosures(definingLambda, common);

            vi.LiftToClosure();

            // assume it's defined on the common parent
            vi.LambdaInfo = common;
        }

        private Exception InvalidParameterReference(Expression variable) {
            return new ArgumentException(
                string.Format(
                    "Parameter '{0}' referenced from lambda '{1}', but is not defined in an outer scope",
                    ((ParameterExpression)variable).Name,
                    _stack.Peek().Lambda.Name ?? "<unnamed>"
                )
            );
        }

        private static void MarkClosures(LambdaInfo reference, LambdaInfo definition) {
            if (reference == definition) {
                // Nothing to do
                return;
            }

            reference.IsClosure = true;

            // Every lambda in between is a closure and has an environment
            for (LambdaInfo li = reference.Parent; li != definition; li = li.Parent) {
                li.IsClosure = true;
                li.HasEnvironment = true;
            }

            definition.HasEnvironment = true;
        }

        // Simple algorithm to find common parent of two lambdas
        // Complexity is O(N) where N is the distance between each child lambda
        // and the root
        private static LambdaInfo FindCommonParent(LambdaInfo first, LambdaInfo second) {
            List<LambdaInfo> firstPath = new List<LambdaInfo>();
            List<LambdaInfo> secondPath = new List<LambdaInfo>();

            for (LambdaInfo li = first; li != null; li = li.Parent) {
                firstPath.Add(li);
            }
            for (LambdaInfo li = second; li != null; li = li.Parent) {
                secondPath.Add(li);
            }

            // must have common parent
            Debug.Assert(firstPath[firstPath.Count - 1] == secondPath[secondPath.Count - 1]);

            int firstIndex = firstPath.Count - 2;
            int secondIndex = secondPath.Count - 2;

            while (firstIndex >= 0 && secondIndex >= 0 && firstPath[firstIndex] == secondPath[secondIndex]) {
                firstIndex--;
                secondIndex--;
            }

            return firstPath[firstIndex + 1];
        }

        /// <summary>
        /// Post processing of the tree:
        ///   1. Populate LambdaInfos with list of variables definied there
        ///   2. Validate that variables defined on a LambdaExpression are
        ///      also used there (using the same variable in two subtrees
        ///      without using it in the common parent is not allowed)
        ///   3. Lift all locals to environment if necessary (generators,
        ///      lambdas marked with EmitLocalDictionary, etc)
        /// </summary>
        protected void BindTheScopes() {
            // Move all variables to their defining lambda
            foreach (KeyValuePair<Expression, VariableInfo> pair in _definitions) {
                Expression v = pair.Key;
                VariableInfo vi = pair.Value;
                LambdaInfo li = vi.LambdaInfo;

                li.Variables.Add(v, vi);
            }

            if (_lambdas != null) {
                // Process the lambdas in post-order so that
                // all children are processed before parent
                int i = _lambdas.Count;
                while (i-- > 0) {
                    LambdaInfo li = _lambdas[i];
                    LambdaExpression lambda = li.Lambda;
                    if (!lambda.IsGlobal && (lambda is GeneratorLambdaExpression || lambda.EmitLocalDictionary || ScriptDomainManager.Options.Frames)) {
                        LiftLocalsToClosure(li);
                    }
                }
            }
        }

        private static void LiftLocalsToClosure(LambdaInfo lambdaInfo) {
            // Lift all parameters and locals
            foreach (VariableInfo vi in lambdaInfo.Variables.Values) {
                AstNodeType kind = vi.Variable.NodeType;
                if (kind == AstNodeType.Parameter ||
                    kind == AstNodeType.LocalVariable ||
                    kind == AstNodeType.TemporaryVariable && lambdaInfo.Lambda is GeneratorLambdaExpression) {
                    vi.LiftToClosure();
                }
            }
            lambdaInfo.HasEnvironment = true;
        }

        #endregion
    }
}
