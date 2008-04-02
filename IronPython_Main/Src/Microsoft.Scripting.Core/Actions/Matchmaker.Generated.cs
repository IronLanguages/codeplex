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
using System.CodeDom.Compiler;
using System.Reflection;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// This class is used to look for matching rules in the caches
    /// by executing individual rules against the site whose fallback
    /// code delegates here.
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public class Matchmaker {
        private bool _match = true;

        internal bool Match {
            get { return _match; }
        }

        internal void Reset() {
            _match = true;
        }

        internal object CreateMatchMakingSite(object/*!*/ site, Type/*!*/ tt) {
            Delegate mismatch = GetMismatchDelegate(tt);
            ReflectedCaller clone = ReflectedCaller.Create(site.GetType().GetMethod("Clone"));
            return clone.Invoke(site, mismatch);
        }

        private Delegate GetMismatchDelegate(Type tt) {
            MethodInfo matchMaker;
            if (DynamicSiteHelpers.IsBigTarget(tt)) {
                if (DynamicSiteHelpers.IsFastTarget(tt)) {
                    matchMaker = MakeMismatchBigTarget(tt, "MismatchBigFast");
                } else {
                    matchMaker = MakeMismatchBigTarget(tt, "MismatchBig");
                }
            } else if (DynamicSiteHelpers.IsFastTarget(tt)) {
                matchMaker = MakeMismatchTarget(tt, "MismatchFast");
            } else {
                matchMaker = MakeMismatchTarget(tt, "Mismatch");
            }

            return Delegate.CreateDelegate(tt, this, matchMaker);
        }

        private static MethodInfo MakeMismatchTarget(Type tt, string name) {
            Type[] args = tt.GetGenericArguments();
            MethodInfo gmethod = typeof(Matchmaker).GetMethod(name + (args.Length - 1));
            return gmethod.MakeGenericMethod(args);
        }

        private static MethodInfo MakeMismatchBigTarget(Type tt, string name) {
            Type[] args = tt.GetGenericArguments();
            MethodInfo gmethod = typeof(Matchmaker).GetMethod(name);
            return gmethod.MakeGenericMethod(args);
        }

        #region Generated Matchmaker

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_matchmaker from: generate_dynsites.py


        //
        // Mismatch routines for fast dynamic sites
        //

        // Mismatch detection, arity 1
        public static TRet MismatchFast1<T0, TRet>(Matchmaker mm, FastCallSite site, T0 arg0) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, arity 2
        public static TRet MismatchFast2<T0, T1, TRet>(Matchmaker mm, FastCallSite site, T0 arg0, T1 arg1) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, arity 3
        public static TRet MismatchFast3<T0, T1, T2, TRet>(Matchmaker mm, FastCallSite site, T0 arg0, T1 arg1, T2 arg2) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, arity 4
        public static TRet MismatchFast4<T0, T1, T2, T3, TRet>(Matchmaker mm, FastCallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, arity 5
        public static TRet MismatchFast5<T0, T1, T2, T3, T4, TRet>(Matchmaker mm, FastCallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, arity 6
        public static TRet MismatchFast6<T0, T1, T2, T3, T4, T5, TRet>(Matchmaker mm, FastCallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, big
        public static TRet MismatchBigFast<T0, TRet>(Matchmaker mm, FastCallSite site, T0 arg0) where T0 : Tuple {
            mm._match = false;
            return default(TRet);
        }

        //
        // Mismatch routines for dynamic sites
        //

        // Mismatch detection, arity 1
        public static TRet Mismatch1<T0, TRet>(Matchmaker mm, CallSite site, CodeContext context, T0 arg0) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, arity 2
        public static TRet Mismatch2<T0, T1, TRet>(Matchmaker mm, CallSite site, CodeContext context, T0 arg0, T1 arg1) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, arity 3
        public static TRet Mismatch3<T0, T1, T2, TRet>(Matchmaker mm, CallSite site, CodeContext context, T0 arg0, T1 arg1, T2 arg2) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, arity 4
        public static TRet Mismatch4<T0, T1, T2, T3, TRet>(Matchmaker mm, CallSite site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, arity 5
        public static TRet Mismatch5<T0, T1, T2, T3, T4, TRet>(Matchmaker mm, CallSite site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, arity 6
        public static TRet Mismatch6<T0, T1, T2, T3, T4, T5, TRet>(Matchmaker mm, CallSite site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            mm._match = false;
            return default(TRet);
        }
        // Mismatch detection, big
        public static TRet MismatchBig<T0, TRet>(Matchmaker mm, CallSite site, CodeContext context, T0 arg0) where T0 : Tuple {
            mm._match = false;
            return default(TRet);
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
