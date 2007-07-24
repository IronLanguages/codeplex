/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Ast;

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
                return EmptyRuleSet<T>.FixedInstance;
            } else {
                return new SmallRuleSet<T>(newRules);
            }
        }

        protected override T MakeTarget(CodeContext context) {
            MethodInfo mi = typeof(T).GetMethod("Invoke");
            CodeGen cg = ScriptDomainManager.CurrentManager.Snippets.Assembly.DefineMethod(
                "_stub_",
                mi.ReturnType,
                Utils.Reflection.GetParameterTypes(mi.GetParameters()),
                new ConstantPool()
            );

            cg.EmitLineInfo = false;
            cg.Binder = context.LanguageContext.Binder;

            if (DynamicSiteHelpers.IsFastTarget(typeof(T))) {
                cg.ContextSlot = new PropertySlot(cg.ArgumentSlots[0], typeof(FastDynamicSite).GetProperty("Context"));
            } else {
                cg.ContextSlot = cg.ArgumentSlots[1];
            }

            foreach (StandardRule<T> rule in _rules) {
                Label nextTest = cg.DefineLabel();
                rule.Emit(cg, nextTest);
                cg.MarkLabel(nextTest);
            }
            EmitNoMatch(cg);

            return (T)(object)cg.CreateDelegate(typeof(T));
        }


        private void EmitNoMatch(CodeGen cg) {
            for (int i = 0; i < cg.ArgumentSlots.Count; i++) {
                cg.ArgumentSlots[i].EmitGet(cg);
            }
            cg.EmitCall(cg.ArgumentSlots[0].Type, "UpdateBindingAndInvoke");
            cg.EmitReturn();
            cg.Finish();
        }
    }
}
