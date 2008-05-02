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
using System.Threading;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    internal class Rule {
        private readonly Expression _binding;
        private readonly Function<bool>[] _validators;              // the list of validates which indicate when the rule is no longer valid
        private readonly object[] _template;                        // the templated parameters for this rule 

        // TODO revisit these fields and their uses when LambdaExpression moves down
        private readonly ParameterExpression[] _parameters;             // TODO: Remove me when we can refer to params as expressions
        internal AnalyzedRule _analyzed;                                // TODO: Remove me when the above 2 are gone

        internal Rule(Expression binding, Function<bool>[] validators, object[] template, ParameterExpression[] parameters) {
            _binding = binding;
            _validators = validators;
            _template = template;
            _parameters = parameters;
        }

        internal Expression Binding {
            get { return _binding; }
        }

        internal Function<bool>[] Validators {
            get {
                return _validators;
            }
        }

        internal object[] Template {
            get {
                return _template;
            }
        }

        internal int TemplateParameterCount {
            get {
                if (_template == null) return 0;
                return _template.Length;
            }
        }

        /// <summary>
        /// Gets the logical parameters to the dynamic site in the form of Variables.
        /// </summary>
        internal ParameterExpression[] Parameters {
            get { return _parameters; }
        }
    }

    internal class Rule<T> : Rule {
        private SmallRuleSet<T> _monomorphicRuleSet;

        internal Rule(Expression binding, Function<bool>[] validators, object[] template, ParameterExpression[] parameters)
            : base(binding, validators, template, parameters) {
        }

        /// <summary>
        /// If not valid, this indicates that the given Test can never return true and therefore
        /// this rule should be removed from any RuleSets when convenient in order to 
        /// reduce memory usage and the number of active rules.
        /// </summary>
        public bool IsValid {
            get {
                if (Validators == null) return true;

                foreach (Function<bool> v in Validators) {
                    if (!v()) return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Each rule holds onto an immutable RuleSet that contains this rule only.
        /// This should heavily optimize monomorphic call sites.
        /// </summary>
        internal SmallRuleSet<T> MonomorphicRuleSet {
            get {
                if (_monomorphicRuleSet == null) {
                    _monomorphicRuleSet = new SmallRuleSet<T>(new Rule<T>[] { this });
                }
                return _monomorphicRuleSet;
            }
        }

        private static Type ReturnType {
            get {
                return typeof(T).GetMethod("Invoke").ReturnType;
            }
        }

        // First parameter is site, second is code context
        private const int FirstParameterIndex = 2;
        internal void EnsureAnalyzed() {
            if (_analyzed == null) {
                AnalyzedRule ar = RuleBinder.Bind(this, ReturnType, FirstParameterIndex);
                Interlocked.CompareExchange<AnalyzedRule>(ref _analyzed, ar, null);
            }
        }

        internal void Emit(LambdaCompiler cg) {
            // Need to make sure we aren't generating into two different CodeGens at the same time
            lock (this) {
                // First, finish binding my variable references
                EnsureAnalyzed();

                LambdaInfo top = _analyzed.Top;
                Compiler tc = new Compiler(_analyzed);

                cg.InitializeRule(tc, top);
                cg.CreateReferenceSlots();
                cg.EmitExpression(Binding);
            }
        }
    }
}