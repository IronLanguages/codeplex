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
using System.Diagnostics;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// This uses linear search to find a rule.  Clearly that doesn't scale super well.
    /// We will address this in the future.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RuleTree<T> where T : class {
        private RuleTable _ruleTable = new RuleTable();

        internal static RuleTree<T> MakeRuleTree() {
            return new RuleTree<T>();
        }

        private RuleTree() {
        }

        /// <summary>
        /// Looks through the rule list, prunes invalid rules and returns rules that apply
        /// </summary>
        internal Rule<T>[] FindApplicableRules(Type[] types) {
            //
            // 1. Get the rule list that would apply to the arguments at hand
            //
            LinkedList<Rule<T>> list = GetRuleList(types);

            lock (list) {
                //
                // Clone the list for execution
                //
                int live = list.Count;
                Rule<T>[] rules = null;

                if (live > 0) {
                    rules = new Rule<T>[live];
                    int index = 0;

                    LinkedListNode<Rule<T>> node = list.First;
                    while (node != null) {
                        rules[index++] = node.Value;
                        node = node.Next;
                    }
                    Debug.Assert(index == live);
                }
                //
                // End of lock
                //

                return rules;
            }
        }

        private LinkedList<Rule<T>> GetRuleList(Type[] types) {
            LinkedList<Rule<T>> ruleList;
            lock (_ruleTable) {
                RuleTable curTable = _ruleTable;
                foreach (Type objType in types) {
                    if (curTable.NextTable == null) {
                        curTable.NextTable = new Dictionary<Type, RuleTable>(1);
                    }

                    RuleTable nextTable;
                    if (!curTable.NextTable.TryGetValue(objType, out nextTable)) {
                        curTable.NextTable[objType] = nextTable = new RuleTable();
                    }

                    curTable = nextTable;
                }

                if (curTable.Rules == null) {
                    curTable.Rules = new LinkedList<Rule<T>>();
                }

                ruleList = curTable.Rules;
            }
            return ruleList;
        }

        internal void AddRule(Type[] args, Rule<T> rule) {
            LinkedList<Rule<T>> list = GetRuleList(args);
            lock (list) {
                list.AddLast(rule);
            }
        }

        internal void RemoveRule(Type[] args, Rule<T> rule) {
            LinkedList<Rule<T>> list = GetRuleList(args);
            lock (list) {
                LinkedListNode<Rule<T>> node = list.First;
                EqualityComparer<Rule<T>> cmp = EqualityComparer<Rule<T>>.Default;
                while (node != null) {
                    if (cmp.Equals(node.Value, rule)) {
                        list.Remove(node);
                        break;
                    }
                    node = node.Next;
                }
            }
        }

        internal void MoveRule(Rule<T> rule, Type[] args) {
            LinkedList<Rule<T>> list = GetRuleList(args);
            lock (list) {
                LinkedListNode<Rule<T>> node = list.First;
                while (node != null) {
                    if (node.Value == rule) {
                        list.Remove(node);
                        list.AddFirst(node);
                        break;
                    }
                    node = node.Next;
                }
            }
        }

        private class RuleTable {
            internal Dictionary<Type, RuleTable> NextTable;
            internal LinkedList<Rule<T>> Rules;
        }
    }
}