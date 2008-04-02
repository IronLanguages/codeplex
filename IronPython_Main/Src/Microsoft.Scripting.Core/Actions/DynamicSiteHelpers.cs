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
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    public static partial class DynamicSiteHelpers {
        private static readonly Dictionary<Type, CreateSite> _siteCtors = new Dictionary<Type, CreateSite>();
        private static readonly Dictionary<Type, CreateFastSite> _fastSiteCtors = new Dictionary<Type, CreateFastSite>();

        /// <summary>
        /// Checks to see if a given delegate is used as a "fast" target.  A fast target
        /// does not include a CodeContext in its parameter list but is able to retrieve
        /// such a delegate from it's DynamicSite if needed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        internal static bool IsFastTarget(Type type) {
            return type.Name.StartsWith("Fast") || type.Name.StartsWith("BigFast");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static bool IsBigTarget(Type type) {
            return type.Name.StartsWith("Big");
        }

        private delegate object CreateSite(DynamicAction action);
        private delegate object CreateFastSite(CodeContext context, DynamicAction action);

        public static CallSite MakeSite(DynamicAction action, Type siteType) {
            CreateSite ctor;
            lock (_siteCtors) {
                if (!_siteCtors.TryGetValue(siteType, out ctor)) {
                    _siteCtors[siteType] = ctor = (CreateSite)Delegate.CreateDelegate(typeof(CreateSite), siteType.GetMethod("Create"));
                }
            }

            return (CallSite)ctor(action);
        }

        public static FastCallSite MakeFastSite(CodeContext context, DynamicAction action, Type siteType) {
            Debug.Assert(typeof(FastCallSite).IsAssignableFrom(siteType.BaseType));
            CreateFastSite ctor;
            lock (_fastSiteCtors) {
                if (!_fastSiteCtors.TryGetValue(siteType, out ctor)) {
                    _fastSiteCtors[siteType] = ctor = (CreateFastSite)Delegate.CreateDelegate(typeof(CreateFastSite), siteType.GetMethod("Create"));
                }
            }

            return (FastCallSite)ctor(context, action);
        }

        //
        // Initialization of dynamic sites stored in static fields 
        //

        public static void InitializeFields(CodeContext context, Type type) {
            InitializeFields(context, type, false);
        }

        public static void InitializeFields(CodeContext context, Type type, bool reusable) {
            if (type == null) return;

            const string slotStorageName = "#SlotStorage";
            foreach (FieldInfo fi in type.GetFields()) {
                if (fi.Name.StartsWith(slotStorageName)) {
                    object value;
                    if (reusable) {
                        value = ConstantPool.GetConstantDataReusable(Int32.Parse(fi.Name.Substring(slotStorageName.Length)));
                    } else {
                        value = ConstantPool.GetConstantData(Int32.Parse(fi.Name.Substring(slotStorageName.Length)));
                    }
                    Debug.Assert(value != null);
                    FastCallSite fds = value as FastCallSite;
                    if (fds != null) {
                        fds.Context = context;
                    }
                    fi.SetValue(null, value);
                }
            }
        }

        internal static Type GetTupleTypeFromTarget(Type target) {
            Debug.Assert(target.IsGenericType);
            Debug.Assert(target.IsSubclassOf(typeof(Delegate)));
            Type[] arguments = target.GetGenericArguments();
            Debug.Assert(arguments.Length == 2);
            return arguments[0];
        }

        private static Type MakeBigDynamicSite(Type site, Type target, params Type[] types) {
            Debug.Assert(types.Length > 1);
            Type tupleType = Tuple.MakeTupleType(ArrayUtils.RemoveLast(types));
            return site.MakeGenericType(target.MakeGenericType(tupleType, types[types.Length - 1]));
        }

        internal static void UpdateSite<T>(object site, ref RuleSet<T> rules, Rule<T> rule) where T : class {
            lock (site) {
                rules = rules.AddRule(rule);
            }
        }
    }
}
