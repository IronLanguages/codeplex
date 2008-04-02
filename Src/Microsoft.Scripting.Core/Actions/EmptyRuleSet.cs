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
    internal class EmptyRuleSet<T> : RuleSet<T> {
        public static readonly RuleSet<T> Instance = new EmptyRuleSet<T>(true);
        public static readonly RuleSet<T> FixedInstance = new EmptyRuleSet<T>(false);

        private bool _supportAdding;

        private EmptyRuleSet(bool supportAdding) {
            this._supportAdding = supportAdding;
        }

        public override RuleSet<T> AddRule(Rule<T> newRule) {
            if (_supportAdding) return newRule.MonomorphicRuleSet;
            else return this;
        }

        public override IList<Rule<T>> GetRules() {
            return null;
        }

        public override bool HasMonomorphicTarget(T target) {
            return false;
        }

        protected override T MakeTarget() {
            throw new InvalidOperationException("Cannot create target for an empty rule set");
        }
    }
}
