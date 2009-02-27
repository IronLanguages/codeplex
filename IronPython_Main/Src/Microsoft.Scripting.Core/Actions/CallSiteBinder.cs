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
using System.Collections.ObjectModel;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;

namespace Microsoft.Runtime.CompilerServices {
    /// <summary>
    /// Class responsible for runtime binding of the dynamic operations on the dynamic call site.
    /// </summary>
    public abstract class CallSiteBinder {

        private static readonly LabelTarget _updateLabel = Expression.Label("CallSiteBinder.UpdateLabel");

        /// <summary>
        /// Initializes a new instance of the <see cref="CallSiteBinder"/> class.
        /// </summary>
        protected CallSiteBinder() {
        }

        /// <summary>
        /// Gets a label that can be used to cause the binding to be updated. It
        /// indicates that the expression's binding is no longer valid.
        /// This is typically used when the "version" of a dynamic object has
        /// changed.
        /// </summary>
        public static LabelTarget UpdateLabel {
            get { return _updateLabel; }
        }

        /// <summary>
        /// Performs the runtime binding of the dynamic operation on a set of arguments.
        /// </summary>
        /// <param name="args">An array of arguments to the dynamic operation.</param>
        /// <param name="parameters">The array of <see cref="ParameterExpression"/> instances that represent the parameters of the call site in the binding process.</param>
        /// <param name="returnLabel">A LabelTarget used to return the result of the dynamic binding.</param>
        /// <returns>
        /// An Expression that performs tests on the dynamic operation arguments, and
        /// performs the dynamic operation if hte tests are valid. If the tests fail on
        /// subsequent occurrences of the dynamic operation, Bind will be called again
        /// to produce a new <see cref="Expression"/> for the new argument types.
        /// </returns>
        public abstract Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel);


        /// <summary>
        /// The Level 2 cache - all rules produced for the same binder.
        /// </summary>
        internal Dictionary<Type, object> Cache;

        // keep alive primary binder.
        private CallSiteBinder theBinder;

        internal RuleCache<T> GetRuleCache<T>() where T : class {
            // make sure we have cache.
            if (Cache == null) {
                // to improve rule sharing try to get the primary binder and share with it.
                theBinder = GetPrimaryBinderInstance();

                // primary binder must have cache.
                if (theBinder.Cache == null) {
                    System.Threading.Interlocked.CompareExchange(
                            ref theBinder.Cache,
                            new Dictionary<Type, object>(),
                            null);
                }

                Cache = theBinder.Cache;
            }

            object ruleCache;
            var cache = Cache;
            lock (cache) {
                if (!cache.TryGetValue(typeof(T), out ruleCache)) {
                    cache[typeof(T)] = ruleCache = new RuleCache<T>();
                }
            }

            RuleCache<T> result = ruleCache as RuleCache<T>;
            System.Diagnostics.Debug.Assert(result != null);

            return result;
        }


        /// <summary>
        /// Trivial binder atomizer.
        /// </summary>
        private static WeakUniqueSet<CallSiteBinder> _binders = new WeakUniqueSet<CallSiteBinder>();
        private CallSiteBinder GetPrimaryBinderInstance() {
            return _binders.GetUniqueFor(this);
        }
    }
}
