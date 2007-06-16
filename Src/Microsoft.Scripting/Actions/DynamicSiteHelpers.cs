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
            return type.Name.StartsWith("Fast");
        }

        //
        // Dynamic sites
        //
        public static Slot MakeSlot(Action action, CodeGen cg, params Type[] types) {
            Type siteType = MakeDynamicSiteType(types);
            return cg.TypeGen.AddStaticField(siteType, Action.MakeName(action));
        }

        public static DynamicSite MakeSite(Action action, Type siteType) {
            Debug.Assert(siteType.BaseType == typeof(DynamicSite));
            return (DynamicSite)siteType.GetConstructor(new Type[] { typeof(Action) }).Invoke(new object[] { action });
        }

        //
        // Fast Dynamic sites
        //
                
        public static Slot MakeFastSlot(Action action, CodeGen cg, params Type[] types) {
            Type siteType = MakeFastDynamicSiteType(types);
            return cg.TypeGen.AddStaticField(siteType, Action.MakeName(action));
        }

        public static FastDynamicSite MakeFastSite(CodeContext context, Action action, Type siteType) {
            Debug.Assert(siteType.BaseType == typeof(FastDynamicSite));
            return (FastDynamicSite)siteType.GetConstructor(new Type[] { typeof(CodeContext), typeof(Action) }).Invoke(new object[] { context, action });
        }

        //
        // Initialization of dynamic sites stored in static fields 
        //

        public static void InitializeFields(CodeContext context, Type type) {
            if (type == null) return;

            foreach (FieldInfo fi in type.GetFields()) {
                if (typeof(DynamicSite).IsAssignableFrom(fi.FieldType)) {
                    Action action = Action.ParseName(fi.Name);
                    fi.SetValue(null, MakeSite(action, fi.FieldType));
                } else if (typeof(FastDynamicSite).IsAssignableFrom(fi.FieldType)) {
                    Action action = Action.ParseName(fi.Name);
                    fi.SetValue(null, MakeFastSite(context, action, fi.FieldType));
                }
            }
        }
    }
}
