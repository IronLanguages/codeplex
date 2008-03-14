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

using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Ast;
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
        private IList<StandardRule<T>> _rules;
        private DynamicMethod _monomorphicTemplate;

        internal SmallRuleSet(IList<StandardRule<T>> rules) {
            this._rules = rules;
        }

        public override RuleSet<T> AddRule(StandardRule<T> newRule) {
            // Can the rule become invalidated between its creation and
            // its insertion into the set?
            Debug.Assert(newRule.IsValid, "Adding an invalid rule");

            IList<StandardRule<T>> newRules = new List<StandardRule<T>>();
            newRules.Add(newRule);
            foreach (StandardRule<T> rule in _rules) {
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

        public override StandardRule<T> GetRule(CodeContext context, params object[] args) {
            context = DynamicSiteHelpers.GetEvaluationContext<T>(context, ref args);

            for(int i = 0; i<_rules.Count; i++) {
                StandardRule<T> rule = _rules[i];
                if (!rule.IsValid) {
                    continue;
                }

                CodeContext tmpCtx = context.Scope.GetTemporaryVariableContext(context, rule.ParamVariables, args);
                try {
                    if ((bool)Interpreter.Evaluate(tmpCtx, rule.Test)) {
                        return rule;
                    }
                } finally {
                    tmpCtx.Scope.TemporaryStorage.Clear();
                }
            }
            return null;
        }

        public override bool HasMonomorphicTarget(T target) {
            Debug.Assert(target != null);

            foreach (StandardRule<T> rule in _rules) {
                if (target.Equals(rule.MonomorphicRuleSet.RawTarget)) {
                    return true;
                }
            }
            return false;
        }

        protected override T MakeTarget(CodeContext context) {
            if (_rules.Count == 1 && this != _rules[0].MonomorphicRuleSet) {
                // use the monomorphic rule if we only have 1 rule.
                return _rules[0].MonomorphicRuleSet.GetOrMakeTarget(context);
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
                cg.ContextSlot = new PropertySlot(cg.GetLambdaArgumentSlot(0), typeof(FastDynamicSite).GetProperty("Context"));
            } else {
                cg.ContextSlot = cg.GetLambdaArgumentSlot(1);
            }

            foreach (StandardRule<T> rule in _rules) {
                Label nextTest = cg.DefineLabel();
                rule.Emit(cg, nextTest);
                cg.MarkLabel(nextTest);
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
            for (int i = 0; i < count; i++) {
                cg.GetLambdaArgumentSlot(i).EmitGet(cg);
            }
            cg.EmitCall(cg.GetLambdaArgumentSlot(0).Type, "UpdateBindingAndInvoke");
            cg.Emit(OpCodes.Ret);
            cg.Finish();
        }

        internal DynamicMethod MonomorphicTemplate {
            get {
                return _monomorphicTemplate;
            }
        }

        private const string StubName = "_stub_";
    }
}
