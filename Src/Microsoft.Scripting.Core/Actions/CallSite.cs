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
using Microsoft.Scripting.Actions;
using System.Threading;

namespace Microsoft.Runtime.CompilerServices {

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public readonly T Update;

        /// <summary>
        /// The Level 0 cache - a delegate specialized based on the site history.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public T Target;

        /// <summary>
        /// The Level 1 cache - a history of the dynamic site
        /// </summary>
        internal RuleSet<T> Rules;

        /// <summary>
        /// The Level 2 cache - all rules produced for the same generic instantiation
        /// of the dynamic site (all dynamic sites with matching delegate type).
        /// </summary>
        private static readonly Dictionary<object, RuleTree<T>> _cache = new Dictionary<object, RuleTree<T>>();

        // Cached update delegate for all sites with a given T
        private static T _CachedUpdate;

        static T GetUpdateDelegate() {
            if (_CachedUpdate == null) {
                // TODO: is it better to use a static cctor?
                Interlocked.CompareExchange(ref _CachedUpdate, UpdateDelegates.MakeUpdateDelegate<T>(), null);
            }
            return _CachedUpdate;
        }

        private CallSite(CallSiteBinder binder)
            : this(binder, GetUpdateDelegate()) {
        }

        internal CallSite(CallSiteBinder binder, T update)
            : base(binder) {
            Rules = EmptyRuleSet<T>.Instance;
            Target = Update = update;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static CallSite<T> Create(CallSiteBinder binder) {
            return new CallSite<T>(binder);
        }

        private static void ClearRuleCache() {
            lock (_cache) {
                _cache.Clear();
            }
        }

        internal RuleTree<T> RuleCache {
            get {
                RuleTree<T> tree;
                object cookie = _binder.CacheIdentity;

                lock (_cache) {
                    if (!_cache.TryGetValue(cookie, out tree)) {
                        _cache[cookie] = tree = new RuleTree<T>();
                    }
                }

                return tree;
            }
        }
    }
}
