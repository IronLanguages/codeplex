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
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
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
    internal class SmallRuleSet<T> : RuleSet<T> {
        private const int MaxRules = 10;
        private readonly IList<Rule<T>> _rules;
        private DynamicMethod _monomorphicTemplate;

        internal SmallRuleSet(IList<Rule<T>> rules) {
            this._rules = rules;
        }

        public override RuleSet<T> AddRule(Rule<T> newRule) {
            // Can the rule become invalidated between its creation and
            // its insertion into the set?
            Debug.Assert(newRule.IsValid, "Adding an invalid rule");

            IList<Rule<T>> newRules = new List<Rule<T>>();
            newRules.Add(newRule);
            foreach (Rule<T> rule in _rules) {
                if (rule.IsValid) {
                    newRules.Add(rule);
                }
            }

            if (newRules.Count > MaxRules) {
                PerfTrack.NoteEvent(PerfTrack.Categories.Rules, "RuleOverflow " + newRule.GetType().Name);
                return EmptyRuleSet<T>.FixedInstance;
            } else {
                return new SmallRuleSet<T>(newRules);
            }
        }

        public override IList<Rule<T>> GetRules() {
            return _rules;
        }

        public override bool HasMonomorphicTarget(T target) {
            Debug.Assert(target != null);

            foreach (Rule<T> rule in _rules) {
                if (target.Equals(rule.MonomorphicRuleSet.RawTarget)) {
                    return true;
                }
            }
            return false;
        }


        protected override T MakeTarget() {
            if (_rules.Count == 1 && this != _rules[0].MonomorphicRuleSet) {
                // use the monomorphic rule if we only have 1 rule.
                return _rules[0].MonomorphicRuleSet.GetOrMakeTarget();
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Rules, "GenerateRule");

            MethodInfo mi = typeof(T).GetMethod("Invoke");
            LambdaCompiler cg = LambdaCompiler.CreateDynamicLambdaCompiler(
                StubName,
                mi.ReturnType,
                ReflectionUtils.GetParameterTypes(mi.GetParameters()),
                null    // SourceUnit
            );

            cg.EmitLineInfo = false;

            if (DynamicSiteHelpers.IsFastTarget(typeof(T))) {
                cg.ContextSlot = new PropertySlot(cg.GetLambdaArgumentSlot(0), typeof(FastCallSite).GetProperty("Context"));
            } else {
                cg.ContextSlot = cg.GetLambdaArgumentSlot(1);
            }

            foreach (Rule<T> rule in _rules) {
                rule.Emit(cg);
            }
            EmitNoMatch(cg);

            if (_rules.Count == 1 &&
                this == _rules[0].MonomorphicRuleSet &&
                _rules[0].TemplateParameterCount > 0 &&
                cg.IsDynamicMethod) {
                _monomorphicTemplate = (DynamicMethod)cg.Method;
            }

            return (T)(object)cg.CreateDelegate(typeof(T));
        }

        private void EmitNoMatch(LambdaCompiler cg) {
            int count = cg.GetLambdaArgumentSlotCount();

            Slot site = cg.GetLambdaArgumentSlot(0);
            Type real = GetRealSiteType(site.Type);
            
            site.EmitGet(cg);
            cg.Emit(OpCodes.Castclass, real);

            PropertyInfo update = real.GetProperty("Update");
            cg.EmitPropertyGet(update);
            for (int i = 0; i < count; i++) {
                cg.GetLambdaArgumentSlot(i).EmitGet(cg);
            }
            cg.Emit(OpCodes.Tailcall);
            cg.EmitCall(update.PropertyType.GetMethod("Invoke"));
            cg.Emit(OpCodes.Ret);
            cg.Finish();
        }

        private Type GetRealSiteType(Type type) {
            if (type == typeof(FastCallSite)) {
                return typeof(FastCallSite<T>);
            } else if (type == typeof(CallSite)) {
                return typeof(CallSite<T>);
            } else {
                throw new InvalidOperationException("Wrong site type");
            }
        }

        internal DynamicMethod MonomorphicTemplate {
            get {
                return _monomorphicTemplate;
            }
        }

        private const string StubName = "_stub_";
    }
}
