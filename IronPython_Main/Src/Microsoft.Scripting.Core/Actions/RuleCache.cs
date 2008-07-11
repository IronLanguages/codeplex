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

namespace System.Scripting.Actions {
    /// <summary>
    /// This is a cache of all generated rules (per dynamic site class)
    /// </summary>
    internal class RuleCache<T> where T : class {
        private readonly Dictionary<object, RuleTree<T>> _trees = new Dictionary<object, RuleTree<T>>();

        internal void Clear() {
            lock (this) {
                _trees.Clear();
            }
        }

        private RuleTree<T> GetOrMakeRuleTree(CallSiteBinder binder) {
            RuleTree<T> tree;
            object cookie = binder.HashCookie;

            lock (this) {
                if (!_trees.TryGetValue(cookie, out tree)) {
                    tree = RuleTree<T>.MakeRuleTree();
                    _trees[cookie] = tree;
                }
            }

            return tree;
        }

        internal Rule<T>[] FindApplicableRules(CallSiteBinder binder, Type[] types, T previousTarget) {
            RuleTree<T> tree = GetOrMakeRuleTree(binder);
            return tree.FindApplicableRules(types, previousTarget);
        }

        internal void AddRule(CallSiteBinder binder, Type[] args, Rule<T> rule) {
            RuleTree<T> tree = GetOrMakeRuleTree(binder);
            tree.AddRule(args, rule);
        }

        internal void RemoveRule(CallSiteBinder binder, Type[] args, Rule<T> rule) {
            RuleTree<T> tree = GetOrMakeRuleTree(binder);
            tree.RemoveRule(args, rule);
        }
    }
}
