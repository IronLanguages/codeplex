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
    /// <summary>
    /// This uses linear search to find a rule.  Clearly that doesn't scale super well.
    /// We will address this in the future.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RuleTree<T> {
        public static RuleTree<T> MakeRuleTree(LanguageContext context) {
            if (context == null) throw new ArgumentNullException("context");

            return new RuleTree<T>(context);
        }
        private LanguageContext _context;
        private LinkedList<StandardRule<T>> _rules = new LinkedList<StandardRule<T>>();

        private RuleTree(LanguageContext context) {
            Debug.Assert(context != null);
            _context = context;
        }

        public StandardRule<T> GetRule(object[] args) {
            // Create fake context for evaluating the rule tests
            CodeContext context = DynamicSiteHelpers.GetEvaluationContext<T>(_context, ref args);
            
            //TODO insert some instrumentation to catch large sets of rules (but what is large?)

            LinkedListNode<StandardRule<T>> node = _rules.First;
            while (node != null) {
                StandardRule<T> rule = node.Value;
                if (!rule.IsValid) {
                    LinkedListNode<StandardRule<T>> nodeToRemove = node;
                    node = node.Next;
                    _rules.Remove(nodeToRemove);
                    continue;
                }

                using (context.Scope.TemporaryVariableContext(rule.TemporaryVariables, rule.ParamVariables, args)) {
                    if (rule.Test == null || (bool)rule.Test.Evaluate(context)) {
                        // Tentative optimization of moving rule to front of list when found
                        _rules.Remove(node);
                        _rules.AddFirst(node);
                        return rule;
                    }
                }
                node = node.Next;
            }

            return null;
        }

        public void AddRule(StandardRule<T> rule) {
            _rules.AddLast(rule);
        }
    }
}
