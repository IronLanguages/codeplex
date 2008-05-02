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

using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// This is a cache of all generated rules (per ActionBinder)
    /// </summary>
    internal class RuleCache {
        private readonly Dictionary<DynamicAction, ActionRuleCache> _rules = new Dictionary<DynamicAction, ActionRuleCache>();

        internal void Clear() {
            _rules.Clear();
        }

        private ActionRuleCache FindActionRuleCache(DynamicAction action) {
            ActionRuleCache actionRuleCache;
            lock (this) {
                if (!_rules.TryGetValue(action, out actionRuleCache)) {
                    actionRuleCache = new ActionRuleCache();
                    _rules[action] = actionRuleCache;
                }
            }

            return actionRuleCache;
        }

        internal Rule<T>[] FindApplicableRules<T>(DynamicAction action, Type[] types) {
            ActionRuleCache actionRuleCache = FindActionRuleCache(action);
            RuleTree<T> tree = actionRuleCache.GetOrMakeTree<T>();

            return tree.FindApplicableRules(types);
        }

        internal void AddRule<T>(DynamicAction action, Type[] args, Rule<T> rule) {
            ActionRuleCache actionRuleCache = FindActionRuleCache(action);
            actionRuleCache.AddRule<T>(args, rule);
        }

        internal void RemoveRule<T>(DynamicAction action, Type[] args, Rule<T> rule) {
            ActionRuleCache actionRuleCache = FindActionRuleCache(action);
            actionRuleCache.RemoveRule<T>(args, rule);
        }

        /// <summary>
        /// All the cached rules for a given Action (per LanguageBinder)
        /// </summary>
        private class ActionRuleCache {
            private Dictionary<Type, object> _trees = new Dictionary<Type, object>();

            internal ActionRuleCache() {
            }

            internal RuleTree<T> GetOrMakeTree<T>() {
                lock (_trees) {
                    if (!_trees.ContainsKey(typeof(T))) {
                        _trees[typeof(T)] = RuleTree<T>.MakeRuleTree();
                    }
                    return (RuleTree<T>)_trees[typeof(T)];
                }
            }

            internal void AddRule<T>(Type[] args, Rule<T> newRule) {
                GetOrMakeTree<T>().AddRule(args, newRule);
            }

            internal void RemoveRule<T>(Type[] args, Rule<T> rule) {
                GetOrMakeTree<T>().RemoveRule(args, rule);
            }
        }
    }
}
