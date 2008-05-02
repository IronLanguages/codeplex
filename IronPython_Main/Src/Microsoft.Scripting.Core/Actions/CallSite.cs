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

namespace Microsoft.Scripting.Actions {

    //
    // A DynamicSite provides a fast mechanism for call-site caching of dynamic dispatch
    // behvaior. Each site will hold onto a delegate that provides a fast-path dispatch
    // based on previous types that have been seen at the call-site. This delegate will
    // call UpdateBindingAndInvoke if it is called with types that it hasn't seen
    // before.  Updating the binding will typically create (or lookup) a new delegate
    // that supports fast-paths for both the new type and for any types that 
    // have been seen previously.
    // 
    // DynamicSites are designed to replace a lot of currently hard-coded fast-paths,
    // for example, addition in IronPython before dynamic sites looked like this:
    // object Add(object x, object y) {
    //   if (x is int) return IntOps.Add((int)x, y);
    //   if (x is double) return DoubleOps.Add((double)x, y);
    //   [lots more special cases]
    //   Fall-back to dynamic case
    // }
    // 
    // and then in IntOps, we'd have
    // object Add(int x, object y) {
    //   if (y is int) return x + (int)y; // modulo overflow handling
    //   if (y is double) return (double)x + (double)y;
    //   [lots more special cases]
    // }
    // 
    // This was very fast for types that were in the fast-path; however, it was a lot
    // of code (~180K dll size) and it was still extremely slow on any types
    // that weren't in the fast-path, i.e. user-defined types that overloaded the
    // + operator.
    // 
    // DynamicSites will generate the same kind of fast-paths; however, they will
    // generate exactly the right amount of code for the types that are seen
    // in the program so that int addition will remain as fast as it was before,
    // and user-defined types can be as fast as ints because they will all have
    // the same optimal dynamically generated fast-paths.
    // 
    // DynamicSites don't encode any particular caching policy, but use their
    // CallSiteBinding to encode a caching policy.
    //


    /// <summary>
    /// Dynamic site which requires the code context to be passed in
    /// as a parameter into its Invoke method.
    /// </summary>
    public abstract class CallSite {
        private readonly DynamicAction _action;

        protected CallSite(DynamicAction action) {
            this._action = action;
        }

        public DynamicAction Action {
            get {
                return _action;
            }
        }

        public ActionBinder Binder {
            get {
                return _action.Binder;
            }
        }
    }

    /// <summary>
    /// Dynamic site
    /// </summary>
    public sealed class CallSite<T> : CallSite {
        /// <summary>
        /// RuleSet - keeps history of the dynamic site
        /// </summary>
        private RuleSet<T> _rules;

        /// <summary>
        /// The update delegate. Called when the dynamic site experiences cache miss
        /// </summary>
        private readonly T _update;

        /// <summary>
        /// The cache itself - a delegate specialized based on the site history.
        /// </summary>
        internal T _target;

        internal CallSite(DynamicAction action)
            : this(action, UpdateDelegates.MakeUpdateDelegate<T>()) {
        }

        internal CallSite(DynamicAction action, T update)
            : this(action, update, update) {
        }

        internal CallSite(DynamicAction action, T update, T target)
            : base(action) {
            _rules = RuleSet<T>.EmptyRules;
            _update = update;
            _target = target;
        }

        internal RuleSet<T> Rules {
            get { return _rules; }
        }

        public T Target {
            get { return _target; }
        }

        public T Update {
            get { return _update; }
        }

        internal void AddRule(Rule<T> rule) {
            lock (this) {
                _rules = _rules.AddRule(rule);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static CallSite<T> Create(DynamicAction action) {
            return new CallSite<T>(action);
        }

        public CallSite<T> Clone(T update) {
            return new CallSite<T>(Action, update);
        }
    }
}
