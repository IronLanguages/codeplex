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
using System.Collections.ObjectModel;
using Microsoft.Linq.Expressions;
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {

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
        private readonly CallSiteRule<T>[] _rules;

        internal SmallRuleSet(CallSiteRule<T>[] rules) {
            _rules = rules;
        }

        internal SmallRuleSet(T target, CallSiteRule<T>[] rules) {
            _rules = rules;
            _target = target;
        }

        internal override RuleSet<T> AddRule(CallSiteRule<T> newRule) {
            List<CallSiteRule<T>> newRules = new List<CallSiteRule<T>>();
            newRules.Add(newRule);
            foreach (CallSiteRule<T> rule in _rules) {
                newRules.Add(rule);
            }

            if (newRules.Count > MaxRules) {
                return EmptyRuleSet<T>.FixedInstance;
            } else {
                return new SmallRuleSet<T>(newRules.ToArray());
            }
        }

        internal override CallSiteRule<T>[] GetRules() {
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

            Expression<T> stitched = Stitch(_rules);
            MethodInfo method;
            T t = LambdaCompiler.CompileLambda<T>(stitched, !typeof(T).IsVisible, out method);

            if (_rules.Length == 1) {
                _rules[0].TemplateMethod = method;
            }

            return t;
        }

        private static Expression<T> Stitch(CallSiteRule<T>[] rules) {
            Type targetType = typeof(T);
            Type siteType = typeof(CallSite<T>);

            // TODO: we could cache this on Rule<T>
            MethodInfo invoke = targetType.GetMethod("Invoke");

            int length = rules.Length;
            Expression[] body = new Expression[length + 1];
            for (int i = 0; i < length; i++) {
                body[i] = rules[i].Binding;
            }

            var @params = CallSiteRule<T>.Parameters.AddFirst(Expression.Parameter(typeof(CallSite), "$site"));

            body[rules.Length] = Expression.Label(
                CallSiteRule<T>.ReturnLabel,
                Expression.Call(
                    Expression.Field(
                        Expression.Convert(@params[0], siteType),
                        siteType.GetField("Update")
                    ),
                    invoke,
                    new ReadOnlyCollection<Expression>(@params)
                )
            );

            return new Expression<T>(
                Annotations.Empty,
                "_stub_",
                Expression.Comma(body),
                new ReadOnlyCollection<ParameterExpression>(@params)
            );
        }

        internal void SetRawTarget(T target) {
            _target = target;
        }
    }
}
