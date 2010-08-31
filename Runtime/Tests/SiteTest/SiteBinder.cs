/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace SiteTest {
    delegate Expression RuleBuilder(ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel);

    internal class SiteBinder : CallSiteBinder {
        /// <summary>
        /// Clear the internal L2 cache of this binder
        /// </summary>
        public void ClearL2()
        {
            Type st = typeof(SiteBinder);
            FieldInfo cachefi = st.GetField("Cache", BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<Type, object> cache = cachefi.GetValue(this) as Dictionary<Type, object>;
            if (cache != null)
                cache.Clear();
        }

        /// <summary>
        /// Add a rule to the L2 cache of this binder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rule"></param>
        public void AddToL2<T>(T rule) where T : class {
            this.CacheTarget<T>(rule);
        }

        /// <summary>
        /// Private queue holding the sequence of rules to
        /// be returned by successive calls to MakeRule
        /// </summary>
        private Queue<RuleBuilder> _rulesToUse = new Queue<RuleBuilder>();

        /// <summary>
        /// Clears any currently pending rules from the queue
        /// and Enqueues all provided rules in order.
        /// </summary>
        /// <param name="rules">Rules to use in order from first to last</param>
        public void SetRules(params RuleBuilder[] rules) {
            _rulesToUse.Clear();
            foreach (RuleBuilder rule in rules) {
                _rulesToUse.Enqueue(rule);
            }
        }

        /// <summary>
        /// A global log of the sequence of actions
        /// taken during a dyn sites invocation.
        /// 
        /// @TODO - Not terribly happy that this is global, but a better solution has yet to present itself
        /// </summary>
        public static SiteLog Log = new SiteLog();

        /// <summary>
        /// Produces a rule for the specified Action for the given arguments.
        /// 
        /// If _rulesToUse is not null, it is used to produce the rule.  Otherwise
        /// we fall back to the base implementation.
        /// </summary>
        public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            if (_rulesToUse.Count == 0) {
                throw new ApplicationException("SiteBinder requested more rules than were supplied!");
            }

            Log.Log(SiteLog.EventType.MakeRule, "Custom");
            // call the function to make the rule
            return _rulesToUse.Dequeue()(parameters, returnLabel);
        }
    }
}
