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


using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;


namespace Microsoft.Scripting {

    /// <summary>
    /// This holds a set of rules for a particular DynamicSite.  Any given
    /// SmallRuleSet instance is immutable and therefore they may be cached
    /// and shared.  At the moment, the only ones that are shared are
    /// SmallRuleSets with a single rule.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class SmallRuleSet<T> where T : class {
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

        internal SmallRuleSet<T> AddRule(CallSiteRule<T> newRule) {
            var temp = _rules.AddFirst(newRule);
            if (_rules.Length < MaxRules) {
                return new SmallRuleSet<T>(temp);
            } else {
                Array.Copy(temp, _rules, MaxRules);
                return this;
            }
        }

        // moves rule +2 up.
        internal void MoveRule(int i) {
            var rule = _rules[i];

            _rules[i] = _rules[i - 1];
            _rules[i - 1] = _rules[i - 2];
            _rules[i - 2] = rule;
        }

        internal CallSiteRule<T>[] GetRules() {
            return _rules;
        }

        internal T GetTarget() {
            if (_target == null) {
                _target = MakeTarget();
            }
            return _target;
        }

        private T MakeTarget() {
            Debug.Assert(_rules.Length > 1 || this == _rules[0].RuleSet);

#if !MICROSOFT_SCRIPTING_CORE
            // We cannot compile rules in the heterogeneous app domains since they
            // may come from less trusted sources
            if (!AppDomain.CurrentDomain.IsHomogenous) {
                throw Error.HomogenousAppDomainRequired();
            }
#endif
            return Stitch().Compile();
        }

        internal Expression<T> Stitch() {
            Type targetType = typeof(T);
            Type siteType = typeof(CallSite<T>);

            int length = _rules.Length;

            var body = new ReadOnlyCollectionBuilder<Expression>(length + 2);
            for (int i = 0; i < length; i++) {
                body.Add(_rules[i].Binding);
            }

            var site = Expression.Parameter(typeof(CallSite), "$site");
            var @params = CallSiteRule<T>.Parameters.AddFirst(site);

            body.Add(Expression.Label(CallSiteBinder.UpdateLabel));
            body.Add(
                Expression.Label(
                    CallSiteRule<T>.ReturnLabel,
                    Expression.Condition(
                        Expression.Call(
                            typeof(CallSiteOps).GetMethod("NeedsUpdate"),
                            @params.First()
                        ),
                        Expression.Default(CallSiteRule<T>.ReturnLabel.Type),
                        Expression.Invoke(
                            Expression.Property(
                                Expression.Convert(site, siteType),
                                typeof(CallSite<T>).GetProperty("Update")
                            ),
                            new TrueReadOnlyCollection<Expression>(@params)
                        )
                    )
                )
            );

            return new Expression<T>(
                "CallSite.Target",
                Expression.Block(body),
                true, // always compile the rules with tail call optimization
                new TrueReadOnlyCollection<ParameterExpression>(@params)
            );
        }
    }
}
