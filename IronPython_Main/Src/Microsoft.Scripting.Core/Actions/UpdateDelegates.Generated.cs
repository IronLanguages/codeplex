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
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    public static class UpdateDelegates {
        internal static T MakeUpdateDelegate<T>() {
            Type target = typeof(T);

            if (target.IsGenericType) {
                Type generic = target.GetGenericTypeDefinition();
                Type[] gargs = target.GetGenericArguments();
                string method = null;

                switch (gargs.Length) {
                    case 2:
                        if (generic == typeof(DynamicSiteTarget<,>)) {
                            method = "Update1";
                        } else if (generic == typeof(FastDynamicSiteTarget<,>)) {
                            method = "UpdateFast1";
                        } else if (generic == typeof(BigDynamicSiteTarget<,>)) {
                            method = "UpdateBig";
                        } else if (generic == typeof(BigFastDynamicSiteTarget<,>)) {
                            method = "UpdateFastBig";
                        }
                        break;
                    case 3:
                        if (generic == typeof(DynamicSiteTarget<,,>)) {
                            method = "Update2";
                        } else if (generic == typeof(FastDynamicSiteTarget<,,>)) {
                            method = "UpdateFast2";
                        }
                        break;
                    case 4:
                        if (generic == typeof(DynamicSiteTarget<,,,>)) {
                            method = "Update3";
                        } else if (generic == typeof(FastDynamicSiteTarget<,,,>)) {
                            method = "UpdateFast3";
                        }
                        break;
                    case 5:
                        if (generic == typeof(DynamicSiteTarget<,,,,>)) {
                            method = "Update4";
                        } else if (generic == typeof(FastDynamicSiteTarget<,,,,>)) {
                            method = "UpdateFast4";
                        }
                        break;
                    case 6:
                        if (generic == typeof(DynamicSiteTarget<,,,,,>)) {
                            method = "Update5";
                        } else if (generic == typeof(FastDynamicSiteTarget<,,,,,>)) {
                            method = "UpdateFast5";
                        }
                        break;
                    case 7:
                        if (generic == typeof(DynamicSiteTarget<,,,,,,>)) {
                            method = "Update6";
                        } else if (generic == typeof(FastDynamicSiteTarget<,,,,,,>)) {
                            method = "UpdateFast6";
                        }
                        break;
                }

                if (method != null) {
                    return (T)(object)Delegate.CreateDelegate(target, typeof(UpdateDelegates).GetMethod(method).MakeGenericMethod(gargs));
                }
            }

            // TODO: Use LCG to create custom update target
            throw new InvalidOperationException("Invalid delegate type for unpdate target");
        }

        #region Generated Predefined Update Targets

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_update_targets from: generate_dynsites.py

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet Update1<T0, TRet>(CallSite site, CodeContext context, T0 arg0) {
            CallSite<DynamicSiteTarget<T0, TRet>> s = (CallSite<DynamicSiteTarget<T0, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<DynamicSiteTarget<T0, TRet>>(context, site.Action, new object[] { arg0 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet Update2<T0, T1, TRet>(CallSite site, CodeContext context, T0 arg0, T1 arg1) {
            CallSite<DynamicSiteTarget<T0, T1, TRet>> s = (CallSite<DynamicSiteTarget<T0, T1, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<DynamicSiteTarget<T0, T1, TRet>>(context, site.Action, new object[] { arg0, arg1 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet Update3<T0, T1, T2, TRet>(CallSite site, CodeContext context, T0 arg0, T1 arg1, T2 arg2) {
            CallSite<DynamicSiteTarget<T0, T1, T2, TRet>> s = (CallSite<DynamicSiteTarget<T0, T1, T2, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<DynamicSiteTarget<T0, T1, T2, TRet>>(context, site.Action, new object[] { arg0, arg1, arg2 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet Update4<T0, T1, T2, T3, TRet>(CallSite site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            CallSite<DynamicSiteTarget<T0, T1, T2, T3, TRet>> s = (CallSite<DynamicSiteTarget<T0, T1, T2, T3, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<DynamicSiteTarget<T0, T1, T2, T3, TRet>>(context, site.Action, new object[] { arg0, arg1, arg2, arg3 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet Update5<T0, T1, T2, T3, T4, TRet>(CallSite site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            CallSite<DynamicSiteTarget<T0, T1, T2, T3, T4, TRet>> s = (CallSite<DynamicSiteTarget<T0, T1, T2, T3, T4, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<DynamicSiteTarget<T0, T1, T2, T3, T4, TRet>>(context, site.Action, new object[] { arg0, arg1, arg2, arg3, arg4 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet Update6<T0, T1, T2, T3, T4, T5, TRet>(CallSite site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            CallSite<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, TRet>> s = (CallSite<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, TRet>>(context, site.Action, new object[] { arg0, arg1, arg2, arg3, arg4, arg5 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet UpdateBig<T0, TRet>(CallSite site, CodeContext context, T0 arg0) where T0 : Tuple {
            CallSite<BigDynamicSiteTarget<T0, TRet>> s = (CallSite<BigDynamicSiteTarget<T0, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<BigDynamicSiteTarget<T0, TRet>>(context, site.Action, Tuple.GetTupleValues(arg0), site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet UpdateFast1<T0, TRet>(FastCallSite site, T0 arg0) {
            CodeContext context = site.Context;
            FastCallSite<FastDynamicSiteTarget<T0, TRet>> s = (FastCallSite<FastDynamicSiteTarget<T0, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<FastDynamicSiteTarget<T0, TRet>>(context, site.Action, new object[] { arg0 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet UpdateFast2<T0, T1, TRet>(FastCallSite site, T0 arg0, T1 arg1) {
            CodeContext context = site.Context;
            FastCallSite<FastDynamicSiteTarget<T0, T1, TRet>> s = (FastCallSite<FastDynamicSiteTarget<T0, T1, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<FastDynamicSiteTarget<T0, T1, TRet>>(context, site.Action, new object[] { arg0, arg1 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet UpdateFast3<T0, T1, T2, TRet>(FastCallSite site, T0 arg0, T1 arg1, T2 arg2) {
            CodeContext context = site.Context;
            FastCallSite<FastDynamicSiteTarget<T0, T1, T2, TRet>> s = (FastCallSite<FastDynamicSiteTarget<T0, T1, T2, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<FastDynamicSiteTarget<T0, T1, T2, TRet>>(context, site.Action, new object[] { arg0, arg1, arg2 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet UpdateFast4<T0, T1, T2, T3, TRet>(FastCallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            CodeContext context = site.Context;
            FastCallSite<FastDynamicSiteTarget<T0, T1, T2, T3, TRet>> s = (FastCallSite<FastDynamicSiteTarget<T0, T1, T2, T3, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<FastDynamicSiteTarget<T0, T1, T2, T3, TRet>>(context, site.Action, new object[] { arg0, arg1, arg2, arg3 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet UpdateFast5<T0, T1, T2, T3, T4, TRet>(FastCallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            CodeContext context = site.Context;
            FastCallSite<FastDynamicSiteTarget<T0, T1, T2, T3, T4, TRet>> s = (FastCallSite<FastDynamicSiteTarget<T0, T1, T2, T3, T4, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<FastDynamicSiteTarget<T0, T1, T2, T3, T4, TRet>>(context, site.Action, new object[] { arg0, arg1, arg2, arg3, arg4 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet UpdateFast6<T0, T1, T2, T3, T4, T5, TRet>(FastCallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            CodeContext context = site.Context;
            FastCallSite<FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, TRet>> s = (FastCallSite<FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, TRet>>(context, site.Action, new object[] { arg0, arg1, arg2, arg3, arg4, arg5 }, site, ref s._target, ref s._rules);
        }

        /// <summary>
        /// Site update code
        /// </summary>
        public static TRet UpdateFastBig<T0, TRet>(FastCallSite site, T0 arg0) where T0 : Tuple {
            CodeContext context = site.Context;
            FastCallSite<BigFastDynamicSiteTarget<T0, TRet>> s = (FastCallSite<BigFastDynamicSiteTarget<T0, TRet>>)site;
            return (TRet)context.LanguageContext.Binder.UpdateSiteAndExecute<BigFastDynamicSiteTarget<T0, TRet>>(context, site.Action, Tuple.GetTupleValues(arg0), site, ref s._target, ref s._rules);
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
