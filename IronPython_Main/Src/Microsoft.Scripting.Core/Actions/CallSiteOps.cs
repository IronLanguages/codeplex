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


using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
namespace Microsoft.Runtime.CompilerServices {

    // Conceptually these are instance methods on CallSite<T> but
    // we don't want users to see them

    /// <summary>
    /// Do not use this type. It is for internal use by CallSite
    /// </summary>
    public static class CallSiteOps {

        [Obsolete("do not use this method", true)]
        public static T SetTarget<T>(CallSite<T> site, CallSiteRule<T> rule) where T : class {
            return site.Target = rule.RuleSet.GetTarget();
        }

        [Obsolete("do not use this method", true)]
        public static void AddRule<T>(CallSite<T> site, CallSiteRule<T> rule) where T : class {
            lock (site) {
                if (site.Rules == null) {
                    site.Rules = rule.RuleSet;
                } else {
                    site.Rules = site.Rules.AddRule(rule);
                }
            }
        }

        [Obsolete("do not use this method", true)]
        public static void MoveRule<T>(CallSite<T> site, CallSiteRule<T> rule, object [] args) where T : class {
            site.RuleCache.MoveRule(rule, args);
        }

        [Obsolete("do not use this method", true)]
        public static CallSite CreateMatchmaker<T>(CallSite<T> site, T matchmaker) where T : class {
            return new CallSite<T>(site.Binder, matchmaker);
        }

        [Obsolete("do not use this method", true)]
        public static CallSiteRule<T> CreateNewRule<T>(CallSite<T> site, CallSiteRule<T> oldRule, CallSiteRule<T> originalRule, object[] args) where T : class {
            if (oldRule != null) {
                //
                // The rule didn't work and since we optimistically added it into the
                // level 2 cache. Remove it now since the rule is no good.
                //
                site.RuleCache.RemoveRule(args, oldRule);
            }

            Expression binding = site.Binder.Bind(args, CallSiteRule<T>.Parameters, CallSiteRule<T>.ReturnLabel);

            //
            // Check the produced rule
            //
            if (binding == null) {
                throw Error.NoOrInvalidRuleProduced();
            }

            var rule = new CallSiteRule<T>(binding);

            if (originalRule != null) {
                // compare our new rule and our original monomorphic rule.  If they only differ from constants
                // then we'll want to re-use the code gen if possible.
                rule = AutoRuleTemplate.CopyOrCreateTemplatedRule(originalRule, rule);
            }

            //
            // Add the rule to the level 2 cache. This is an optimistic add so that cache miss
            // on another site can find this existing rule rather than building a new one.  We
            // add the originally added rule, not the templated one, to the global cache.  That
            // allows sites to template on their own.
            //
            site.RuleCache.AddRule(args, rule);

            return rule;
        }

        [Obsolete("do not use this method", true)]
        public static CallSiteRule<T>[] FindApplicableRules<T>(CallSite<T> site, object[] args) where T : class {
            return site.RuleCache.FindApplicableRules(args);
        }

        [Obsolete("do not use this method", true)]
        public static void SetPolymorphicTarget<T>(CallSite<T> site) where T : class {
            T target = site.Rules.GetTarget();
            // If the site has gone megamorphic, we'll have an empty RuleSet
            // with no target. In that case, we don't want to clear out the
            // target
            if (target != null) {
                site.Target = target;
            }
        }

        [Obsolete("do not use this method", true)]
        public static CallSiteRule<T>[] GetRules<T>(CallSite<T> site) where T : class {
            return (site.Rules == null) ? null : site.Rules.GetRules();
        }
    }
}
