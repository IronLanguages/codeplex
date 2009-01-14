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
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection;
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

        // Cache of CallSite constructors for a given delegate type
        private static CacheDict<Type, Func<CallSiteBinder, CallSite>> _SiteCtors;
                
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

        /// <summary>
        /// Creates a CallSite with the given delegate type and binder.
        /// </summary>
        /// <param name="delegateType">The CallSite delegate type.</param>
        /// <param name="binder">The CallSite binder.</param>
        /// <returns>The new CallSite.</returns>
        public static CallSite Create(Type delegateType, CallSiteBinder binder) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            ContractUtils.Requires(delegateType.IsSubclassOf(typeof(Delegate)), "delegateType", Strings.TypeMustBeDerivedFromSystemDelegate);

            if (_SiteCtors == null) {
                // It's okay to just set this, worst case we're just throwing away some data
                _SiteCtors = new CacheDict<Type, Func<CallSiteBinder, CallSite>>(100);
            }
            Func<CallSiteBinder, CallSite> ctor;
            lock (_SiteCtors) {
                if (!_SiteCtors.TryGetValue(delegateType, out ctor)) {
                    MethodInfo method = typeof(CallSite<>).MakeGenericType(delegateType).GetMethod("Create");
                    ctor = (Func<CallSiteBinder, CallSite>)Delegate.CreateDelegate(typeof(Func<CallSiteBinder, CallSite>), method);
                    _SiteCtors.Add(delegateType, ctor);
                }
            }
            return ctor(binder);
        }
    }

    /// <summary>
    /// Dynamic site type.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    public sealed partial class CallSite<T> : CallSite where T : class {
        /// <summary>
        /// The update delegate. Called when the dynamic site experiences cache miss.
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
        /// The Level 1 cache - a history of the dynamic site.
        /// </summary>
        internal RuleSet<T> Rules;

        /// <summary>
        /// The Level 2 cache - all rules produced for the same generic instantiation
        /// of the dynamic site (all dynamic sites with matching delegate type).
        /// </summary>
        private static Dictionary<object, RuleTree<T>> _cache;

        // Cached update delegate for all sites with a given T
        private static T _CachedUpdate;

        private CallSite(CallSiteBinder binder)
            : base(binder) {
            Target = Update = GetUpdateDelegate();
        }

        internal CallSite(CallSiteBinder binder, T update)
            : base(binder) {
            Target = Update = update;
        }

        /// <summary>
        /// Creates an instance of the dynamic call site, initialized with the binder responsible for the
        /// runtime binding of the dynamic operations at this call site.
        /// </summary>
        /// <param name="binder">The binder responsible for the runtime binding of the dynamic operations at this call site.</param>
        /// <returns>The new instance of dynamic call site.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static CallSite<T> Create(CallSiteBinder binder) {
            return new CallSite<T>(binder);
        }

        private T GetUpdateDelegate() {
            // This is intentionally non-static to speed up creation - in particular MakeUpdateDelegate
            // as static generic methods are more expensive than instance methods.  We call a ref helper
            // so we only access the generic static field once.
            return GetUpdateDelegate(ref _CachedUpdate);
        }

        private T GetUpdateDelegate(ref T addr) {
            if (addr == null) {
                // reduce creation cost by not using Interlocked.CompareExchange.  Calling I.CE causes
                // us to spend 25% of our creation time in JIT_GenericHandle.  Instead we'll rarely
                // create 2 delegates with no other harm caused.
                addr = MakeUpdateDelegate();
            }
            return addr;
        }

        /// <summary>
        /// Clears the rule cache ... used by the call site tests.
        /// </summary>
        private static void ClearRuleCache() {
            if (_cache != null) {
                lock (_cache) {
                    _cache.Clear();
                }
            }
        }

        internal RuleTree<T> RuleCache {
            get {
                RuleTree<T> tree;
                object cookie = _binder.CacheIdentity;
                
                if (_cache == null) {
                    Interlocked.CompareExchange(
                        ref _cache,
                         new Dictionary<object, RuleTree<T>>(),
                         null);
                }

                lock (_cache) {
                    if (!_cache.TryGetValue(cookie, out tree)) {
                        _cache[cookie] = tree = new RuleTree<T>();
                    }
                }

                return tree;
            }
        }

        internal T MakeUpdateDelegate() {
            Type target = typeof(T);
            Type[] args;
            MethodInfo invoke = target.GetMethod("Invoke");

            if (target.IsGenericType && IsSimpleSignature(invoke, out args)) {
                MethodInfo method = null;

                if (invoke.ReturnType == typeof(void)) {
                    if (target == DelegateHelpers.GetActionType(args.AddFirst(typeof(CallSite)))) {
                        method = typeof(UpdateDelegates).GetMethod("UpdateAndExecuteVoid" + args.Length, BindingFlags.NonPublic | BindingFlags.Static);
                    }
                } else {
                    if (target == DelegateHelpers.GetFuncType(args.AddFirst(typeof(CallSite)))) {
                        method = typeof(UpdateDelegates).GetMethod("UpdateAndExecute" + (args.Length - 1), BindingFlags.NonPublic | BindingFlags.Static);
                    }
                }
                if (method != null) {
                    return (T)(object)method.MakeGenericMethod(args).CreateDelegate(target);
                }
            }

            return CreateCustomUpdateDelegate(invoke);
        }


        private static bool IsSimpleSignature(MethodInfo invoke, out Type[] sig) {
            ParameterInfo[] pis = invoke.GetParametersCached();
            ContractUtils.Requires(pis.Length > 0 && pis[0].ParameterType == typeof(CallSite), "T");

            Type[] args = new Type[invoke.ReturnType != typeof(void) ? pis.Length : pis.Length - 1];
            bool supported = true;

            for (int i = 1; i < pis.Length; i++) {
                ParameterInfo pi = pis[i];
                if (pi.IsByRefParameter()) {
                    supported = false;
                }
                args[i - 1] = pi.ParameterType;
            }
            if (invoke.ReturnType != typeof(void)) {
                args[args.Length - 1] = invoke.ReturnType;
            }
            sig = args;
            return supported;
        }

        //
        // WARNING: If you're changing this method, make sure you update the
        // pregenerated versions as well, which are generated by
        // generate_dynsites.py
        // The two implementations *must* be kept functionally equivalent!
        //
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private T CreateCustomUpdateDelegate(MethodInfo invoke) {
            var body = new List<Expression>();
            var vars = new List<ParameterExpression>();
            var @params = invoke.GetParametersCached().Map(p => Expression.Parameter(p.ParameterType, p.Name));
            var @return = Expression.Label(invoke.GetReturnType());
            var typeArgs = new[] { typeof(T) };

            var site = @params[0];
            var arguments = @params.RemoveFirst();

            //var @this = (CallSite<T>)site;
            var @this = Expression.Variable(typeof(CallSite<T>), "this");
            vars.Add(@this);
            body.Add(Expression.Assign(@this, Expression.Convert(site, @this.Type)));

            //CallSiteRule<T>[] applicable;
            var applicable = Expression.Variable(typeof(CallSiteRule<T>[]), "applicable");
            vars.Add(applicable);

            //CallSiteRule<T> rule;
            var rule = Expression.Variable(typeof(CallSiteRule<T>), "rule");
            vars.Add(rule);

            //T ruleTarget, startingTarget = @this.Target;
            var ruleTarget = Expression.Variable(typeof(T), "ruleTarget");
            vars.Add(ruleTarget);
            var startingTarget = Expression.Variable(typeof(T), "startingTarget");
            vars.Add(startingTarget);
            body.Add(Expression.Assign(startingTarget, Expression.Field(@this, "Target")));

            //TRet result;
            ParameterExpression result = null;
            if (@return.Type != typeof(void)) {
                vars.Add(result = Expression.Variable(@return.Type, "result"));
            }

            //int count, index;
            var count = Expression.Variable(typeof(int), "count");
            vars.Add(count);
            var index = Expression.Variable(typeof(int), "index");
            vars.Add(index);

            //CallSiteRule<T> originalRule = null;
            var originalRule = Expression.Variable(typeof(CallSiteRule<T>), "originalRule");
            vars.Add(originalRule);

            ////
            //// Create matchmaker and its site. We'll need them regardless.
            //// 
            //// **** THIS DIFFERS FROM THE GENERATED CODE TO NOT EXPOSE the Matchmaker class
            //// **** Instead we close over the match bool here and create a delegate each time
            //// **** through.  This is less efficient than caching the delegate but avoids
            //// **** exposing the large Matchmaker class.
            ////
            //bool match = true;
            //site = CreateMatchmaker(
            //    @this,
            //    (mm_site, %(matchmakerArgs)s) => {
            //        match = false;
            //        %(returnDefault)s;
            //    }
            //);
            var match = Expression.Variable(typeof(bool), "match");
            vars.Add(match);
            var resetMatch = Expression.Assign(match, Expression.Constant(true));
            body.Add(resetMatch);
            body.Add(
                Expression.Assign(
                    site,
                    Expression.Call(
                        typeof(CallSiteOps),
                        "CreateMatchmaker",
                        typeArgs,
                        @this,
                        Expression.Lambda<T>(
                            Expression.Block(
                                Expression.Assign(match, Expression.Constant(false)),
                                Expression.Default(@return.Type)
                            ),
                            new ReadOnlyCollection<ParameterExpression>(@params)
                        )
                    )
                )
            );

            ////
            //// Level 1 cache lookup
            ////
            //if ((applicable = CallSiteOps.GetRules(@this)) != null) {
            //    for (index = 0, count = applicable.Length; index < count; index++) {
            //        rule = applicable[index];

            //        //
            //        // Execute the rule
            //        //
            //        ruleTarget = CallSiteOps.SetTarget(@this, rule);

            //        try {
            //            %(setResult)s ruleTarget(site, %(args)s);
            //            if (match) {
            //                %(returnResult)s;
            //            }
            //        } finally {
            //            if (match) {
            //                //
            //                // Match in Level 1 cache. We saw the arguments that match the rule before and now we
            //                // see them again. The site is polymorphic. Update the delegate and keep running
            //                //
            //                CallSiteOps.SetPolymorphicTarget(@this);
            //            }
            //        }

            //        if (startingTarget == ruleTarget) {
            //            // our rule was previously monomorphic, if we produce another monomorphic
            //            // rule we should try and share code between the two.
            //            originalRule = rule;
            //        }

            //        // Rule didn't match, try the next one
            //        match = true;            
            //    }
            //}
            Expression invokeRule;
            if (@return.Type == typeof(void)) {
                invokeRule = Expression.Block(
                    Expression.Invoke(ruleTarget, new ReadOnlyCollection<Expression>(@params)),
                    IfThen(match, Expression.Return(@return))
                );
            } else {
                invokeRule = Expression.Block(
                    Expression.Assign(result, Expression.Invoke(ruleTarget, new ReadOnlyCollection<Expression>(@params))),
                    IfThen(match, Expression.Return(@return, result))
                );
            }

            var getRule = Expression.Assign(
                ruleTarget,
                Expression.Call(
                    typeof(CallSiteOps),
                    "SetTarget",
                    typeArgs,
                    @this,
                    Expression.Assign(rule, Expression.ArrayAccess(applicable, index))
                )
            );

            var checkOriginalRule = IfThen(
                Expression.Equal(
                    Helpers.Convert(startingTarget, typeof(object)), 
                    Helpers.Convert(ruleTarget, typeof(object))
                ),
                Expression.Assign(originalRule, rule)
            );

            var tryRule = Expression.TryFinally(
                invokeRule,
                IfThen(
                    match,
                    Expression.Call(typeof(CallSiteOps), "SetPolymorphicTarget", typeArgs, @this)
                )
            );

            var @break = Expression.Label();

            var breakIfDone = IfThen(
                Expression.Equal(index, count),
                Expression.Break(@break)
            );

            var incrementIndex = Expression.PreIncrementAssign(index);

            body.Add(
                IfThen(
                    Expression.NotEqual(
                        Expression.Assign(applicable, Expression.Call(typeof(CallSiteOps), "GetRules", typeArgs, @this)),
                        Expression.Constant(null, applicable.Type)
                    ),
                    Expression.Block(
                        Expression.Assign(count, Expression.ArrayLength(applicable)),
                        Expression.Assign(index, Expression.Constant(0)),
                        Expression.Loop(
                            Expression.Block(
                                breakIfDone,
                                getRule,
                                tryRule,
                                checkOriginalRule,
                                resetMatch,
                                incrementIndex
                            ),
                            @break,
                            null
                        )
                    )
                )
            );

            ////
            //// Level 2 cache lookup
            ////
            //var args = new object[] { arg0, arg1, ... };
            var args = Expression.Variable(typeof(object[]), "args");
            vars.Add(args);
            body.Add(
                Expression.Assign(
                    args,
                    Expression.NewArrayInit(typeof(object), arguments.Map(p => Convert(p, typeof(object))))
                )
            );

            ////
            //// Any applicable rules in level 2 cache?
            ////
            //if ((applicable = CallSiteOps.FindApplicableRules(@this, args)) != null) {
            //    count = applicable.Length;
            //    for (index = 0; index < count; index++) {
            //        rule = applicable[index];
            //
            //        //
            //        // Execute the rule
            //        //
            //        ruleTarget = CallSiteOps.SetTarget(@this, rule);
            //
            //        try {
            //            result = ruleTarget(site, arg0);
            //            if (match) {
            //                return result;
            //            }
            //        } finally {
            //            if (match) {
            //                //
            //                // Rule worked. Add it to level 1 cache
            //                //
            //
            //                CallSiteOps.AddRule(@this, rule);
            //                // and then move it to the front of the L2 cache
            //                @this.RuleCache.MoveRule(rule, args);
            //            }
            //        }
            //
            //        if (startingTarget == ruleTarget) {
            //            // If we've gone megamorphic we can still template off the L2 cache
            //            originalRule = rule;
            //        }
            //
            //        // Rule didn't match, try the next one
            //        match = true;
            //    }
            //}

            tryRule = Expression.TryFinally(
                invokeRule,
                IfThen(match, 
                    Expression.Block(
                        Expression.Call(typeof(CallSiteOps), "AddRule", typeArgs, @this, rule),
                        Expression.Call(typeof(CallSiteOps), "MoveRule", typeArgs, @this, rule, args)
                    )
                )
            );

            body.Add(
                IfThen(
                    Expression.NotEqual(
                        Expression.Assign(
                            applicable,
                            Expression.Call(typeof(CallSiteOps), "FindApplicableRules", typeArgs, @this, args)
                        ),
                        Expression.Constant(null, applicable.Type)
                    ),
                    Expression.Block(
                        Expression.Assign(count, Expression.ArrayLength(applicable)),
                        Expression.Assign(index, Expression.Constant(0)),
                        Expression.Loop(
                            Expression.Block(
                                breakIfDone,
                                getRule,
                                tryRule,
                                checkOriginalRule,
                                resetMatch,
                                incrementIndex
                            ),
                            @break,
                            null
                        )
                    )
                )
            );

            ////
            //// Miss on Level 0, 1 and 2 caches. Create new rule
            ////

            //rule = null;
            //for (; ; ) {
            //    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

            //    //
            //    // Execute the rule on the matchmaker site
            //    //
            //    ruleTarget = CallSiteOps.SetTarget(@this, rule);

            //    try {
            //        %(setResult)s ruleTarget(site, %(args)s);
            //        if (match) {
            //            %(returnResult)s;
            //        }
            //    } finally {
            //        if (match) {
            //            //
            //            // The rule worked. Add it to level 1 cache.
            //            //
            //            CallSiteOps.AddRule(@this, rule);
            //        }
            //    }

            //    // Rule we got back didn't work, try another one
            //    match = true;
            //}
            body.Add(Expression.Assign(rule, Expression.Constant(null, rule.Type)));

            getRule = Expression.Assign(
                ruleTarget,
                Expression.Call(
                    typeof(CallSiteOps),
                    "SetTarget",
                    typeArgs,
                    @this,
                    Expression.Assign(
                        rule,
                        Expression.Call(typeof(CallSiteOps), "CreateNewRule", typeArgs, @this, rule, originalRule, args)
                    )
                )
            );

            body.Add(
                Expression.Loop(
                    Expression.Block(getRule, tryRule, resetMatch),
                    null, null
                )
            );

            body.Add(Expression.Default(@return.Type));

            var lambda = Expression.Lambda<T>(
                Expression.Label(
                    @return,
                    Expression.Block(
                        new ReadOnlyCollection<ParameterExpression>(vars),
                        new ReadOnlyCollection<Expression>(body)
                    )
                ),
                "_stub_",
                new ReadOnlyCollection<ParameterExpression>(@params)
            );

            // Need to compile with forceDynamic because T could be invisible,
            // or one of the argument types could be invisible
            return lambda.Compile();
        }

        /// <summary>
        /// Behaves like an "if" statement in imperative languages. The type is
        /// always treated as void regardless of the body's type. The else
        /// branch is empty
        /// </summary>
        private static ConditionalExpression IfThen(Expression test, Expression ifTrue) {
            return Expression.Condition(test, Expression.Void(ifTrue), Expression.Empty());
        }

        private static Expression Convert(Expression arg, Type type) {
            if (TypeUtils.AreReferenceAssignable(type, arg.Type)) {
                return arg;
            }
            return Expression.Convert(arg, type);
        }
    }
}
