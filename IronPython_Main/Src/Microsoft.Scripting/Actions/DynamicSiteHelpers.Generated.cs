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
using System.Reflection;

using Microsoft.Scripting.Utils;
using System.Collections.Generic;

namespace Microsoft.Scripting.Actions {
    public static partial class DynamicSiteHelpers {
        private static readonly Dictionary<ValueArray<Type>, ReflectedCaller>/*!*/ _executeSites = new Dictionary<ValueArray<Type>, ReflectedCaller>();

        #region Generated DynamicSiteHelpers

        // *** BEGIN GENERATED CODE ***

        public static readonly int MaximumArity = 7;

        public static Type MakeDynamicSiteType(params Type[] types) {
            Type genType;
            switch (types.Length) {
                case 2: genType = typeof(DynamicSite<,>); break;
                case 3: genType = typeof(DynamicSite<,,>); break;
                case 4: genType = typeof(DynamicSite<,,,>); break;
                case 5: genType = typeof(DynamicSite<,,,,>); break;
                case 6: genType = typeof(DynamicSite<,,,,,>); break;
                case 7: genType = typeof(DynamicSite<,,,,,,>); break;
                default:
                    return MakeBigDynamicSiteType(types);
            }

            return genType.MakeGenericType(types);
        }

        public static Type MakeFastDynamicSiteType(params Type[] types) {
            Type genType;
            switch (types.Length) {
                case 2: genType = typeof(FastDynamicSite<,>); break;
                case 3: genType = typeof(FastDynamicSite<,,>); break;
                case 4: genType = typeof(FastDynamicSite<,,,>); break;
                case 5: genType = typeof(FastDynamicSite<,,,,>); break;
                case 6: genType = typeof(FastDynamicSite<,,,,,>); break;
                case 7: genType = typeof(FastDynamicSite<,,,,,,>); break;
                default:
                    return MakeBigFastDynamicSiteType(types);
            }

            return genType.MakeGenericType(types);
        }

        public static object Execute(CodeContext/*!*/ context, ActionBinder/*!*/ binder, DynamicAction/*!*/ action, Type/*!*/[]/*!*/ types, object[]/*!*/ args) {
            Contract.RequiresNotNull(types, "types");
            Contract.RequiresNotNull(args, "args");
            Contract.RequiresNotNull(context, "context");
            Contract.RequiresNotNull(binder, "binder");
            Contract.RequiresNotNull(action, "action");

            ReflectedCaller rc;

            lock (_executeSites) {
                ValueArray<Type> array = new ValueArray<Type>(types);
                if (!_executeSites.TryGetValue(array, out rc)) {
                    Type siteType;

                    switch (args.Length) {
                        case 1: siteType = typeof(DynamicSiteTarget<,>).MakeGenericType(types); break;
                        case 2: siteType = typeof(DynamicSiteTarget<,,>).MakeGenericType(types); break;
                        case 3: siteType = typeof(DynamicSiteTarget<,,,>).MakeGenericType(types); break;
                        case 4: siteType = typeof(DynamicSiteTarget<,,,,>).MakeGenericType(types); break;
                        case 5: siteType = typeof(DynamicSiteTarget<,,,,,>).MakeGenericType(types); break;
                        case 6: siteType = typeof(DynamicSiteTarget<,,,,,,>).MakeGenericType(types); break;
                        default:
                            Type tupleType = Tuple.MakeTupleType(ArrayUtils.RemoveLast(types));
                            siteType = typeof(BigDynamicSiteTarget<,>).MakeGenericType(tupleType, types[types.Length - 1]);
                            break;
                    }
                    MethodInfo target = typeof(ActionBinder).GetMethod("ExecuteRule").MakeGenericMethod(siteType);
                    _executeSites[array] = rc = ReflectedCaller.Create(target);
                }
            }

            return rc.Invoke(binder, context, action, args);
        }

        private class UninitializedTargetHelper<T0, T1, T2, T3, T4, T5, Tret> {
            public Tret Invoke1(DynamicSite<T0, Tret> site, CodeContext context, T0 arg0) {
                return site.UpdateBindingAndInvoke(context, arg0);
            }
            public Tret FastInvoke1(FastDynamicSite<T0, Tret> site, T0 arg0) {
                return site.UpdateBindingAndInvoke(arg0);
            }
            public Tret Invoke2(DynamicSite<T0, T1, Tret> site, CodeContext context, T0 arg0, T1 arg1) {
                return site.UpdateBindingAndInvoke(context, arg0, arg1);
            }
            public Tret FastInvoke2(FastDynamicSite<T0, T1, Tret> site, T0 arg0, T1 arg1) {
                return site.UpdateBindingAndInvoke(arg0, arg1);
            }
            public Tret Invoke3(DynamicSite<T0, T1, T2, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2) {
                return site.UpdateBindingAndInvoke(context, arg0, arg1, arg2);
            }
            public Tret FastInvoke3(FastDynamicSite<T0, T1, T2, Tret> site, T0 arg0, T1 arg1, T2 arg2) {
                return site.UpdateBindingAndInvoke(arg0, arg1, arg2);
            }
            public Tret Invoke4(DynamicSite<T0, T1, T2, T3, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
                return site.UpdateBindingAndInvoke(context, arg0, arg1, arg2, arg3);
            }
            public Tret FastInvoke4(FastDynamicSite<T0, T1, T2, T3, Tret> site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
                return site.UpdateBindingAndInvoke(arg0, arg1, arg2, arg3);
            }
            public Tret Invoke5(DynamicSite<T0, T1, T2, T3, T4, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
                return site.UpdateBindingAndInvoke(context, arg0, arg1, arg2, arg3, arg4);
            }
            public Tret FastInvoke5(FastDynamicSite<T0, T1, T2, T3, T4, Tret> site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
                return site.UpdateBindingAndInvoke(arg0, arg1, arg2, arg3, arg4);
            }
            public Tret Invoke6(DynamicSite<T0, T1, T2, T3, T4, T5, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
                return site.UpdateBindingAndInvoke(context, arg0, arg1, arg2, arg3, arg4, arg5);
            }
            public Tret FastInvoke6(FastDynamicSite<T0, T1, T2, T3, T4, T5, Tret> site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
                return site.UpdateBindingAndInvoke(arg0, arg1, arg2, arg3, arg4, arg5);
            }
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
