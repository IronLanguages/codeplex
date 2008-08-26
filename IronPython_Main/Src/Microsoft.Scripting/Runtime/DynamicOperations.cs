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
using System.Diagnostics;
using System.Scripting.Actions;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {

    /// <summary>
    /// ObjectOperations provide a large catalogue of object operations such as member access, conversions, 
    /// indexing, and things like addition.  There are several introspection and tool support services available
    /// for more advanced hosts.  
    /// 
    /// You get ObjectOperation instances from ScriptEngine, and they are bound to their engines for the semantics 
    /// of the operations.  There is a default instance of ObjectOperations you can share across all uses of the 
    /// engine.  However, very advanced hosts can create new instances.
    /// </summary>
    public class DynamicOperations {
        /// <summary> a shared instance of CodeContext used for all object operations </summary>
        private readonly CodeContext _context;

        /// <summary> a dictionary of SiteKey's which are used to cache frequently used operations, logically a set </summary>
        private Dictionary<SiteKey, SiteKey> _sites = new Dictionary<SiteKey, SiteKey>();

        /// <summary> the # of sites we had created at the last cleanup </summary>
        private int LastCleanup;

        /// <summary> the total number of sites we've ever created </summary>
        private int SitesCreated;

        /// <summary> the number of sites required before we'll try cleaning up the cache... </summary>
        private const int CleanupThreshold = 20;

        /// <summary> the minimum difference between the average that is required to remove </summary>
        private const int RemoveThreshold = 2;

        /// <summary> the maximum number we'll remove on a single cache cleanup </summary>
        private const int StopCleanupThreshold = CleanupThreshold / 2;

        /// <summary> the number of sites we should clear after if we can't make progress cleaning up otherwise </summary>
        private const int ClearThreshold = 50;

        public DynamicOperations(CodeContext context) {
            ContractUtils.RequiresNotNull(context, "context");

            _context = context;
        }

        #region Basic Operations

        /// <summary>
        /// Calls the provided object with the given parameters and returns the result.
        /// 
        /// The prefered way of calling objects is to convert the object to a strongly typed delegate 
        /// using the ConvertTo methods and then invoking that delegate.
        /// </summary>
        public object Call(object obj, params object[] parameters) {
            // we support a couple of parameters instead of just splatting because JS doesn't yet support splatted arguments for function calls.
            switch (parameters.Length) {
                case 0: {
                        CallSite<Func<CallSite, CodeContext, object, object>> site;
                        site = GetSite<object, object>(OldCallAction.Make(Binder, new CallSignature(0)));
                        return site.Target(site, _context, obj);
                    }
                case 1: {
                        CallSite<Func<CallSite, CodeContext, object, object, object>> site;
                        site = GetSite<object, object, object>(OldCallAction.Make(Binder, new CallSignature(1)));
                        return site.Target(site, _context, obj, parameters[0]);
                    }
                case 2: {
                        CallSite<Func<CallSite, CodeContext, object, object, object, object>> site;
                        site = GetSite<object, object, object, object>(OldCallAction.Make(Binder, new CallSignature(2)));
                        return site.Target(site, _context, obj, parameters[0], parameters[1]);
                    }
                default: {
                        CallSite<Func<CallSite, CodeContext, object, object[], object>> site;
                        site = GetSite<object, object[], object>(OldCallAction.Make(Binder, new CallSignature(new ArgumentInfo(ArgumentKind.List))));
                        return site.Target(site, _context, obj, parameters);
                    }
            }
        }

        public object Create(object obj, params object[] parameters) {
            var site = GetSite<object, object[], object>(OldCreateInstanceAction.Make(Binder, new CallSignature(new ArgumentInfo(ArgumentKind.List))));
            return site.Target(site, _context, obj, parameters);
        }

        /// <summary>
        /// Gets the member name from the object obj.  Throws an exception if the member does not exist or is write-only.
        /// </summary>
        public object GetMember(object obj, string name) {
            CallSite<Func<CallSite, CodeContext, object, object>> site;
            site = GetSite<object, object>(OldGetMemberAction.Make(Binder, name));
            return site.Target(site, _context, obj);
        }

        /// <summary>
        /// Gets the member name from the object obj and converts it to the type T.  Throws an exception if the
        /// member does not exist, is write-only, or cannot be converted.
        /// </summary>
        public T GetMember<T>(object obj, string name) {
            CallSite<Func<CallSite, CodeContext, object, T>> site;
            site = GetSite<object, T>(OldGetMemberAction.Make(Binder, name));
            return site.Target(site, _context, obj);
        }

        /// <summary>
        /// Gets the member name from the object obj.  Returns true if the member is successfully retrieved and 
        /// stores the value in the value out param.
        /// </summary>
        public bool TryGetMember(object obj, string name, out object value) {
            CallSite<Func<CallSite, CodeContext, object, object>> site;
            site = GetSite<object, object>(OldGetMemberAction.Make(Binder, name, GetMemberBindingFlags.NoThrow));
            object res = site.Target(site, _context, obj);

            if (res != OperationFailed.Value) {
                value = res;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Returns true if the object has a member named name, false if the member does not exist.
        /// </summary>
        public bool ContainsMember(object obj, string name) {
            object dummy;
            return TryGetMember(obj, name, out dummy);
        }

        /// <summary>
        /// Removes the member name from the object obj.  Returns true if the member was successfully removed
        /// or false if the member does not exist.
        /// </summary>
        public bool RemoveMember(object obj, string name) {
            CallSite<Func<CallSite, CodeContext, object, bool>> site;
            site = GetSite<object, bool>(OldDeleteMemberAction.Make(Binder, name));
            return site.Target(site, _context, obj);
        }

        /// <summary>
        /// Sets the member name on object obj to value.
        /// </summary>
        public void SetMember(object obj, string name, object value) {
            CallSite<Func<CallSite, CodeContext, object, object, object>> site;
            site = GetSite<object, object, object>(OldSetMemberAction.Make(Binder, name));
            site.Target(site, _context, obj, value);
        }

        /// <summary>
        /// Sets the member name on object obj to value.  This overload can be used to avoid
        /// boxing and casting of strongly typed members.
        /// </summary>
        public void SetMember<T>(object obj, string name, T value) {
            CallSite<Func<CallSite, CodeContext, object, T, object>> site;
            site = GetSite<object, T, object>(OldSetMemberAction.Make(Binder, name));
            site.Target(site, _context, obj, value);
        }

        /// <summary>
        /// Convers the object obj to the type T.
        /// </summary>
        public T ConvertTo<T>(object obj) {
            CallSite<Func<CallSite, CodeContext, object, T>> site;
            site = GetSite<object, T>(OldConvertToAction.Make(Binder, typeof(T)));
            return site.Target(site, _context, obj);
        }

        /// <summary>
        /// Converts the object obj to the type type.
        /// </summary>
        public object ConvertTo(object obj, Type type) {
            CallSite<Func<CallSite, CodeContext, object, object>> site;
            site = GetSite<object, object>(OldConvertToAction.Make(Binder, type));
            return site.Target(site, _context, obj);
        }

        /// <summary>
        /// Converts the object obj to the type T.  Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryConvertTo<T>(object obj, out T result) {
            CallSite<Func<CallSite, CodeContext, object, object>> site;
            site = GetSite<object, object>(OldConvertToAction.Make(Binder, typeof(T), ConversionResultKind.ExplicitTry));
            object res = site.Target(site, _context, obj);

            if (res == null) {
                // conversion failed or it's null -> null on a value type or a nullable type
                result = default(T);
                return obj == null && (!typeof(T).IsValueType || (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>)));
            }

            result = (T)res;
            return true;
        }

        /// <summary>
        /// Converts the object obj to the type type.  Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryConvertTo(object obj, Type type, out object result) {
            CallSite<Func<CallSite, CodeContext, object, object>> site;
            site = GetSite<object, object>(OldConvertToAction.Make(Binder, type, ConversionResultKind.ExplicitTry));
            result = site.Target(site, _context, obj);

            if (result == null) {
                // conversion failed or it's null -> null on a value type or a nullable type
                return obj == null && (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)));
            }

            return true;
        }

        /// <summary>
        /// Performs a generic unary operation on the specified target and returns the result.
        /// </summary>
        public object DoOperation(Operators op, object target) {
            return DoOperation<object, object>(op, target);
        }

        /// <summary>
        /// Performs a generic unary operation on the strongly typed target and returns the value as the specified type
        /// </summary>
        public TResult DoOperation<TTarget, TResult>(Operators op, TTarget target) {
            CallSite<Func<CallSite, CodeContext, TTarget, TResult>> site;
            site = GetSite<TTarget, TResult>(OldDoOperationAction.Make(Binder, op));
            return site.Target(site, _context, target);
        }

        /// <summary>
        /// Performs the generic binary operation on the specified targets and returns the result.
        /// </summary>
        public object DoOperation(Operators op, object target, object other) {
            return DoOperation<object, object, object>(op, target, other);
        }

        /// <summary>
        /// Peforms the generic binary operation on the specified strongly typed targets and returns
        /// the strongly typed result.
        /// </summary>
        public TResult DoOperation<TTarget, TOther, TResult>(Operators op, TTarget target, TOther other) {
            CallSite<Func<CallSite, CodeContext, TTarget, TOther, TResult>> site;
            site = GetSite<TTarget, TOther, TResult>(OldDoOperationAction.Make(Binder, op));
            return site.Target(site, _context, target, other);
        }

        #endregion

        #region Private implementation details

        private ActionBinder Binder {
            get {
                return _context.LanguageContext.Binder;
            }
        }

        /// <summary>
        /// Helper to create a new dynamic site w/ the specified type parameters for the provided action.
        /// 
        /// This will either get the site from the cache or create a new site and return it.  The cache
        /// may be cleaned if it's gotten too big since the last usage.
        /// </summary>
        private CallSite<Func<CallSite, CodeContext, T0, Tret>> GetSite<T0, Tret>(OldDynamicAction action) {
            return GetSiteWorker<CallSite<Func<CallSite, CodeContext, T0, Tret>>>(action, CallSite<Func<CallSite, CodeContext, T0, Tret>>.Create);
        }

        /// <summary>
        /// Helper to create a new dynamic site w/ the specified type parameters for the provided action.
        /// 
        /// This will either get the site from the cache or create a new site and return it.  The cache
        /// may be cleaned if it's gotten too big since the last usage.
        /// </summary>
        private CallSite<Func<CallSite, CodeContext, T0, T1, Tret>> GetSite<T0, T1, Tret>(OldDynamicAction action) {
            return GetSiteWorker<CallSite<Func<CallSite, CodeContext, T0, T1, Tret>>>(action, CallSite<Func<CallSite, CodeContext, T0, T1, Tret>>.Create);
        }

        /// <summary>
        /// Helper to create a new dynamic site w/ the specified type parameters for the provided action.
        /// 
        /// This will either get the site from the cache or create a new site and return it.  The cache
        /// may be cleaned if it's gotten too big since the last usage.
        /// </summary>
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, Tret>> GetSite<T0, T1, T2, Tret>(OldDynamicAction action) {
            return GetSiteWorker<CallSite<Func<CallSite, CodeContext, T0, T1, T2, Tret>>>(action, CallSite<Func<CallSite, CodeContext, T0, T1, T2, Tret>>.Create);
        }

        /// <summary>
        /// Helper to create to get or create the dynamic site - called by the GetSite methods.
        /// </summary>
        private T GetSiteWorker<T>(OldDynamicAction action, Func<OldDynamicAction, T> ctor) where T : CallSite {
            SiteKey sk = new SiteKey(typeof(T), action);

            lock (_sites) {
                SiteKey old;
                if (!_sites.TryGetValue(sk, out old)) {
                    SitesCreated++;
                    if (SitesCreated < 0) {
                        // overflow, just reset back to zero...
                        SitesCreated = 0;
                        LastCleanup = 0;
                    }
                    sk.Site = ctor(sk.Action);
                    _sites[sk] = sk;
                } else {
                    sk = old;
                }

                sk.HitCount++;
            }

            Cleanup();

            return (T)sk.Site;
        }

        /// <summary>
        /// Removes items from the cache that have the lowest usage...
        /// </summary>
        private void Cleanup() {
            lock (_sites) {
                // cleanup only if we have too many sites and we've created a bunch since our last cleanup
                if (_sites.Count > CleanupThreshold && (LastCleanup < SitesCreated - CleanupThreshold)) {
                    LastCleanup = SitesCreated;

                    // calculate the average use, remove up to StopCleanupThreshold that are below average.
                    int totalUse = 0;
                    foreach (SiteKey sk in _sites.Keys) {
                        totalUse += sk.HitCount;
                    }

                    int avgUse = totalUse / _sites.Count;
                    if (avgUse == 1 && _sites.Count > ClearThreshold) {
                        // we only have a bunch of one-off requests
                        _sites.Clear();
                        return;
                    }

                    List<SiteKey> toRemove = null;
                    foreach (SiteKey sk in _sites.Keys) {
                        if (sk.HitCount < (avgUse - RemoveThreshold)) {
                            if (toRemove == null) {
                                toRemove = new List<SiteKey>();
                            }

                            toRemove.Add(sk);
                            if (toRemove.Count > StopCleanupThreshold) {
                                // if we have a setup like weight(100), weight(1), weight(1), weight(1), ... we don't want
                                // to just run through and remove all of the weight(1)'s. 
                                break;
                            }
                        }

                    }

                    if (toRemove != null) {
                        foreach (SiteKey sk in toRemove) {
                            _sites.Remove(sk);
                        }

                        // reset all hit counts so the next time through is fair 
                        // to newly added members which may take precedence.
                        foreach (SiteKey sk in _sites.Keys) {
                            sk.HitCount = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper class for tracking all of our unique dynamic sites and their
        /// usage patterns.  We hash on the combination of the action and site type.
        /// 
        /// We also track the hit count and the key holds the site associated w/ the 
        /// key.  Logically this is a set based upon the action and site-type but we
        /// store it in a dictionary.
        /// </summary>
        private class SiteKey : IEquatable<SiteKey> {
            // the key portion of the data
            public OldDynamicAction Action;
            private readonly Type _siteType;

            // not used for equality, used for caching strategy
            public int HitCount;
            public CallSite Site;

            public SiteKey(Type siteType, OldDynamicAction action) {
                Debug.Assert(siteType != null);
                Debug.Assert(action != null);

                Action = action;
                _siteType = siteType;
            }

            [Confined]
            public override bool Equals(object obj) {
                return Equals(obj as SiteKey);
            }

            [Confined]
            public override int GetHashCode() {
                return Action.GetHashCode() ^ _siteType.GetHashCode();
            }

            #region IEquatable<SiteKey> Members

            [StateIndependent]
            public bool Equals(SiteKey other) {
                if (other == null) return false;

                return other.Action.Equals(Action) &&
                    other._siteType == _siteType;
            }

            #endregion
#if DEBUG
            [Confined]
            public override string ToString() {
                return String.Format("{0} {1}", Action.ToString(), HitCount);
            }
#endif
        }

        #endregion
    }
}
