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
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Scripting.Actions {

    internal class BigRuleSet {
        private CodeContext _context;
        private Dictionary<Type, object> _trees = new Dictionary<Type,object>();

        public BigRuleSet(CodeContext context) {
            Debug.Assert(context != null);
            _context = context;
        }

        private RuleTree<T> GetOrMakeTree<T>() {
            lock (_trees) {
                if (!_trees.ContainsKey(typeof(T))) {
                    _trees[typeof(T)] = RuleTree<T>.MakeRuleTree(_context);
                }
                return (RuleTree<T>)_trees[typeof(T)];
            }
        }

        public void AddRule<T>(StandardRule<T> newRule) {
            RuleTree<T> rules = GetOrMakeTree<T>();

            // These locks are used to protect the internal dictionaries in the RuleTreeNode types
            // It should be investigated if there is a lock-free read design that could work here
            lock (rules) {
                rules.AddRule(newRule);
            }
        }

        public StandardRule<T> FindRule<T>(object[] args) {
            RuleTree<T> rules = GetOrMakeTree<T>();

            lock (rules) {
                return rules.GetRule(args);
            }
        }
    }
}
