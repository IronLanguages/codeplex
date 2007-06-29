/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions {
    public static partial class DynamicSiteHelpers {

        /// <summary>
        /// Checks to see if a given delegate is used as a "fast" target.  A fast target
        /// does not include a CodeContext in its parameter list but is able to retrieve
        /// such a delegate from it's DynamicSite if needed.
        /// </summary>
        public static bool IsFastTarget(Type type) {
            return type.Name.StartsWith("Fast") || type.Name.StartsWith("BigFast");
        }

        public static bool IsBigTarget(Type type) {
            return type.Name.StartsWith("Big");
        }

        //
        // Dynamic sites
        //
        public static Slot MakeSlot(Action action, CodeGen cg, params Type[] types) {
            Type siteType = MakeDynamicSiteType(types);
            return cg.TypeGen.AddStaticField(siteType, "#" + types.Length + "#" + Action.MakeName(action));
        }

        public static DynamicSite MakeSite(Action action, Type siteType, int size) {
            Debug.Assert(typeof(DynamicSite).IsAssignableFrom(siteType));
            if (size > MaximumArity) {
                return (DynamicSite)siteType.GetConstructor(new Type[] { typeof(Action) }).Invoke(new object[] { action });
            }
            return (DynamicSite)siteType.GetConstructor(new Type[] { typeof(Action) }).Invoke(new object[] { action });
        }

        //
        // Fast Dynamic sites
        //
                
        public static Slot MakeFastSlot(Action action, CodeGen cg, params Type[] types) {
            Type siteType = MakeFastDynamicSiteType(types);
            return cg.TypeGen.AddStaticField(siteType, "#" + types.Length + "#" + Action.MakeName(action));
        }

        public static FastDynamicSite MakeFastSite(CodeContext context, Action action, Type siteType, int size) {
            Debug.Assert(typeof(FastDynamicSite).IsAssignableFrom(siteType.BaseType));
            if (size > MaximumArity) {
                return (FastDynamicSite)siteType.GetConstructor(new Type[] { typeof(CodeContext), typeof(Action) }).Invoke(new object[] { context, action});
            }
            return (FastDynamicSite)siteType.GetConstructor(new Type[] { typeof(CodeContext), typeof(Action) }).Invoke(new object[] { context, action });
        }

        //
        // Initialization of dynamic sites stored in static fields 
        //

        public static void InitializeFields(CodeContext context, Type type) {
            if (type == null) return;

            foreach (FieldInfo fi in type.GetFields()) {
                Action action;
                if (fi.Name.StartsWith("#")) {
                    int end = fi.Name.IndexOf('#', 1);
                    int size = Int32.Parse(fi.Name.Substring(1, end - 1));
                    action = Action.ParseName(fi.Name.Substring(end + 1));

                    if (typeof(DynamicSite).IsAssignableFrom(fi.FieldType)) {
                        fi.SetValue(null, MakeSite(action, fi.FieldType, size));
                    } else if (typeof(FastDynamicSite).IsAssignableFrom(fi.FieldType)) {
                        fi.SetValue(null, MakeFastSite(context, action, fi.FieldType, size));
                    }
                }
            }
        }

        public static void InsertArguments(Scope scope, params object[] args) {
            for (int i = 0; i < args.Length; i++) {
                scope.SetName(SymbolTable.StringToId("$arg" + i.ToString()), args[i]);
            }
        }

        private static Type MakeBigDynamicSiteType(params Type[] types) {
            if (types.Length < 2) throw new ArgumentException("must have at least 2 types");

            Type tupleType = NewTuple.MakeTupleType(Utils.Array.RemoveLast(types));

            return typeof(BigDynamicSite<,>).MakeGenericType(tupleType, types[types.Length - 1]);            
        }


        public static Type MakeBigFastDynamicSiteType(params Type[] types) {
            if (types.Length < 2) throw new ArgumentException("must have at least 2 types");

            Type tupleType = NewTuple.MakeTupleType(Utils.Array.RemoveLast(types));

            return typeof(BigFastDynamicSite<,>).MakeGenericType(tupleType, types[types.Length - 1]);
        }

        private class BigUninitializedTargetHelper<T0, Tret> where T0 : NewTuple {
            public Tret BigInvoke(BigDynamicSite<T0, Tret> site, CodeContext context, T0 arg0) {
                return site.UpdateBindingAndInvoke(context, arg0);
            }

            public Tret BigFastInvoke(BigFastDynamicSite<T0, Tret> site, T0 arg0) {
                return site.UpdateBindingAndInvoke(arg0);
            }
        }

        public static Delegate MakeUninitializedBigTarget(Type targetType) {
            Type dType = typeof(BigUninitializedTargetHelper<,>).MakeGenericType(targetType.GetGenericArguments());
            return Delegate.CreateDelegate(targetType, Activator.CreateInstance(dType), "BigInvoke");
        }

        public static Delegate MakeUninitializedBigFastTarget(Type targetType) {
            Type dType = typeof(BigUninitializedTargetHelper<,>).MakeGenericType(targetType.GetGenericArguments());
            return Delegate.CreateDelegate(targetType, Activator.CreateInstance(dType), "BigFastInvoke");
        }
    }
}
