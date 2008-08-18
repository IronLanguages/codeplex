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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Scripting.Actions {

    /// <summary>
    /// This holds a set of rules for a particular DynamicSite.  Any given
    /// SmallRuleSet instance is immutable and therefore they may be cached
    /// and shared.  At the moment, the only ones that are shared are
    /// SmallRuleSets with a single rule.
    /// 
    /// When a new rule is added, then a new SmallRuleSet will be created
    /// that contains all existing rules that are still considered valid with
    /// the new rule added to the front of the list.  The target generated for
    /// this type will simply try each of the rules in order and emit the
    /// standard DynamicSite.UpdateBindingAndInvoke fallback call at the end.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SmallRuleSet<T> : RuleSet<T> where T : class {
        private T _target;
        private const int MaxRules = 10;
        private readonly Rule<T>[] _rules;

        internal SmallRuleSet(Rule<T>[] rules) {
            _rules = rules;
        }

        internal SmallRuleSet(T target, Rule<T>[] rules) {
            _rules = rules;
            _target = target;
        }

        internal override RuleSet<T> AddRule(Rule<T> newRule) {
            List<Rule<T>> newRules = new List<Rule<T>>();
            newRules.Add(newRule);
            foreach (Rule<T> rule in _rules) {
                newRules.Add(rule);
            }

            if (newRules.Count > MaxRules) {
                return EmptyRuleSet<T>.FixedInstance;
            } else {
                return new SmallRuleSet<T>(newRules.ToArray());
            }
        }

        internal override Rule<T>[] GetRules() {
            return _rules;
        }

        internal override T GetTarget() {
            if (_target == null) {
                _target = MakeTarget();
            }
            return _target;
        }

        private T MakeTarget() {
            if (_rules.Length == 1 && this != _rules[0].RuleSet) {
                // Use the the rule's own set if we only have 1 rule
                return _rules[0].RuleSet.GetTarget();
            }

            LambdaExpression stitched = Stitcher.Stitch<T>(_rules);
            MethodInfo method;
            T t = LambdaCompiler.CompileLambda<T>(stitched, !typeof(T).IsVisible, out method);

            if (_rules.Length == 1) {
                _rules[0].TemplateMethod = method;
            }

            return t;
        }

        internal void SetRawTarget(T target) {
            _target = target;
        }
    }
}
