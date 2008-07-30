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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Text;
using System.Threading;

namespace System.Scripting.Actions {

    //
    // A CallSite provides a fast mechanism for call-site caching of dynamic dispatch
    // behvaior. Each site will hold onto a delegate that provides a fast-path dispatch
    // based on previous types that have been seen at the call-site. This delegate will
    // call UpdateAndExecute if it is called with types that it hasn't seen before.
    // Updating the binding will typically create (or lookup) a new delegate
    // that supports fast-paths for both the new type and for any types that 
    // have been seen previously.
    // 
    // DynamicSites will generate the fast-paths specialized for sets of runtime argument
    // types. However, they will generate exactly the right amount of code for the types
    // that are seen in the program so that int addition will remain as fast as it would
    // be with custom implementation of the addition, and the user-defined types can be
    // as fast as ints because they will all have the same optimal dynamically generated
    // fast-paths.
    // 
    // DynamicSites don't encode any particular caching policy, but use their
    // CallSiteBinding to encode a caching policy.
    //


    /// <summary>
    /// A Dynamic Call Site base class. This type is used as a parameter type to the
    /// dynamic site targets. The first parameter of the delegate (T) below must be
    /// of this type.
    /// </summary>
    public abstract class CallSite {
        /// <summary>
        /// The Binder responsible for binding operations at this call site.
        /// This binder is invoked by the UpdateAndExecute below if all Level 0,
        /// Level 1 and Level 2 caches experience cache miss.
        /// </summary>
        internal readonly CallSiteBinder _binder;

        // only CallSite<T> derives from this
        internal CallSite(CallSiteBinder binder) {
            _binder = binder;
        }

        /// <summary>
        /// Class responsible for binding dynamic operations on the dynamic site.
        /// </summary>
        public CallSiteBinder Binder {
            get { return _binder; }
        }
    }

    /// <summary>
    /// Dynamic site type.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    public sealed class CallSite<T> : CallSite where T : class {
        /// <summary>
        /// The update delegate. Called when the dynamic site experiences cache miss
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public readonly T Update;

        /// <summary>
        /// The Level 0 cache - a delegate specialized based on the site history.
        /// </summary>
        public T Target;

        /// <summary>
        /// The Level 1 cache - a history of the dynamic site
        /// </summary>
        private RuleSet<T> _rules;

        /// <summary>
        /// The Level 2 cache - all rules produced for the same generic instantiation
        /// of the dynamic site (all dynamic sites with matching delegate type).
        /// </summary>
        private static readonly RuleCache<T> _cache = new RuleCache<T>();

        private CallSite(CallSiteBinder binder)
            : this(binder, UpdateDelegates.MakeUpdateDelegate<T>()) {
        }

        private CallSite(CallSiteBinder binder, T update)
            : this(binder, update, update) {
        }

        private CallSite(CallSiteBinder binder, T update, T target)
            : base(binder) {
            _rules = EmptyRuleSet<T>.Instance;
            Update = update;
            Target = target;
        }

        private void AddRule(Rule<T> rule) {
            lock (this) {
#if DEBUG
                bool wasMegamorphic = _rules == EmptyRuleSet<T>.FixedInstance;
#endif

                _rules = _rules.AddRule(rule);
#if DEBUG
                if (!wasMegamorphic && _rules == EmptyRuleSet<T>.FixedInstance) {
                    PerfTrack.NoteEvent(PerfTrack.Categories.Rules, "MegaMorphic " + _binder.ToString());
                }
#endif
            }
        }

        /// <summary>
        /// Gets a rule, updates the site that called, and then returns the result of executing the rule.
        /// 
        /// This method is called on a level 0 cache miss.
        /// </summary>
        /// <param name="args">The arguments to the rule as provided from the call site at runtime.</param>
        /// <returns>The result of executing the rule.</returns>
        public object UpdateAndExecute(object[] args) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            Rule<T>[] applicable;
            Rule<T> rule;
            T ruleTarget, startingTarget = Target;
            object result;

            int count, index;
            Rule<T> originalMonomorphicRule = null;

            //
            // Create matchmaker, its site and reflected caller. We'll need them regardless.
            //
            Type typeofT = typeof(T);               // Calculate this only once

            Matchmaker mm = new Matchmaker();
            CallSite site = CreateMatchmakerCallSite(mm);
            MatchCallerTarget caller = MatchCaller.GetCaller(typeofT);

            //
            // Capture the site's rule set. Since it can change on the site asynchronously,
            // this ensures that we work with the same rule set throughout this function.
            //
            RuleSet<T> siteRules = _rules;

            //
            // Level 1 cache lookup
            //
            IList<Rule<T>> history = siteRules.GetRules();
#if DEBUG
            if (history == null) {
                PerfTrack.NoteEvent(PerfTrack.Categories.Rules, "FirstLookup " + _binder.GetType().ToString());
            }
#endif

            if (history != null) {
                count = history.Count;
                for (index = 0; index < count; index++) {
                    rule = history[index];

                    if (startingTarget == rule.RuleSet.GetTarget()) {
                        // our rule was previously monomorphic, if we produce another monomorphic
                        // rule we should try and share code between the two.
                        originalMonomorphicRule = rule;
                    }

                    if (!rule.IsValid) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.Rules, "Invalid L1 " + _binder.ToString());
                        continue;
                    }

                    mm.Reset();

                    //
                    // Execute the rule
                    //
                    Target = ruleTarget = rule.RuleSet.GetTarget();

                    try {
                        result = caller(ruleTarget, site, args);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // Match in Level 1 cache. We saw the arguments that match the rule before and now we
                            // see them again. The site is polymorphic. Update the delegate and keep running
                            // (unless the delegate was already asynchronously updated by another thread)
                            //
                            if (Target == ruleTarget) {
                                PerfTrack.NoteEvent(PerfTrack.Categories.Rules, "Polymorphic " + _binder.GetType().ToString());
                                Interlocked.CompareExchange<T>(ref Target, siteRules.GetTarget(), ruleTarget);
                            }
                        }
                    }
                }
            }

            //
            // Level 2 cache lookup
            //
            Type[] argTypes = CompilerHelpers.GetTypes(args);
            applicable = _cache.FindApplicableRules(_binder, argTypes, startingTarget);

            //
            // Any applicable rules in level 2 cache?
            //
            if (applicable != null) {
                count = applicable.Length;
                for (index = 0; index < count; index++) {
                    rule = applicable[index];

                    if (startingTarget == rule.RuleSet.GetTarget()) {
                        // If we've gone megamorphic we can still template off the L2 cache
                        originalMonomorphicRule = rule;
                    }

                    mm.Reset();

                    //
                    // Execute the rule
                    //
                    Target = ruleTarget = rule.RuleSet.GetTarget();

                    try {
                        result = caller(ruleTarget, site, args);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // Rule worked. Add it to level 1 cache
                            //

                            AddRule(rule);
                        }
                    }

                    // Rule didn't match, try the next one
                }
            }


            //
            // Miss on Level 0, 1 and 2 caches. Create new rule
            //

            for (; ; ) {
                // newRule is here just for debugging purposes to compare before & after templating
                Rule<T> newRule = rule = CreateNewRule(originalMonomorphicRule, args);

                //
                // Add the rule to the level 2 cache. This is an optimistic add so that cache miss
                // on another site can find this existing rule rather than building a new one.  We
                // add the originally added rule, not the templated one, to the global cache.  That
                // allows sites to template on their own.
                //
                _cache.AddRule(_binder, argTypes, rule);

                //
                // Execute the rule on the matchmaker site
                //

                mm.Reset();

                Target = ruleTarget = rule.RuleSet.GetTarget();

                try {
                    result = caller(ruleTarget, site, args);
                    if (mm.Match) {
                        return result;
                    }
                } finally {
                    if (mm.Match) {
                        //
                        // The rule worked. Add it to level 1 cache.
                        //
                        AddRule(rule);
                    }
                }

                //
                // The rule didn't work and since we optimistically added it into the
                // level 2 cache. Remove it now since the rule is no good.
                //
                _cache.RemoveRule(_binder, argTypes, rule);
            }
        }

        private CallSite CreateMatchmakerCallSite(Matchmaker mm) {
            return new CallSite<T>(_binder, mm.CreateMatchMakingDelegate<T>());
        }

        private Rule<T> CreateNewRule(Rule<T> originalMonomorphicRule, object[] args) {
            NoteRuleCreation(_binder, args);

            Rule<T> rule = _binder.Bind<T>(args);

            //
            // Check the produced rule
            //
            if (rule == null || rule.Binding == null) {
                throw Error.NoOrInvalidRuleProduced();
            }
            ExpressionWriter.Dump(rule);


            if (originalMonomorphicRule != null) {
                // compare our new rule and our original monomorphic rule.  If they only differ from constants
                // then we'll want to re-use the code gen if possible.
                rule = AutoRuleTemplate.CopyOrCreateTemplatedRule(originalMonomorphicRule, rule);
            }

            return rule;
        }

        [Conditional("DEBUG")]
        private static void NoteRuleCreation(CallSiteBinder binder, object[] args) {
            StringBuilder sb = new StringBuilder("MakeRule ");
            sb.Append(binder.ToString());
            sb.Append(" ");
            foreach (object arg in args) {
                sb.Append(CompilerHelpers.GetType(arg).Name);
                sb.Append(" ");
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Rules, sb.ToString());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static CallSite<T> Create(CallSiteBinder binder) {
            return new CallSite<T>(binder);
        }

        // TODO: Make internal and create friendly UnitTests
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void ClearRuleCache() {
            _cache.Clear();
        }
    }
}
