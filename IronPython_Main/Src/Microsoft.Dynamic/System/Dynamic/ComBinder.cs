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

#if !SILVERLIGHT

using System; using Microsoft;
using Microsoft.Scripting;
using System.Collections.Generic;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;

// Will be moved into its own assembly soon
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Scripting")]
namespace Microsoft.Scripting {

    /// <summary>
    /// Provides helper methods to bind COM objects dynamically.
    /// </summary>
    public static class ComBinder {

        /// <summary>
        /// Determines if an object is a COM object.
        /// </summary>
        /// <param name="value">The object to test.</param>
        /// <returns>True if the object is a COM object, False otherwise.</returns>
        public static bool IsComObject(object value) {
            return ComObject.IsComObject(value);
        }

        public static bool TryBindGetMember(GetMemberBinder binder, DynamicMetaObject instance, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindGetMember(binder);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        public static bool TryBindSetMember(SetMemberBinder binder, DynamicMetaObject instance, DynamicMetaObject value, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindSetMember(binder, value);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        public static bool TryBindInvoke(InvokeBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindInvoke(binder, args);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        public static bool TryBindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindInvokeMember(binder, args);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        public static bool TryBindGetIndex(GetIndexBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindGetIndex(binder, args);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        public static bool TryBindSetIndex(SetIndexBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, DynamicMetaObject value, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindSetIndex(binder, args, value);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        public static IEnumerable<string> GetDynamicMemberNames(object value) {
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.Requires(IsComObject(value), "value", Strings.ComObjectExpected);

            return ComObject.ObjectToComObject(value).MemberNames;
        }

        // IEnumerable<KeyValuePair<string, object>> is a standard idiom
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IEnumerable<KeyValuePair<string, object>> GetDynamicDataMembers(object value) {
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.Requires(IsComObject(value), "value", Strings.ComObjectExpected);

            return ComObject.ObjectToComObject(value).DataMembers;
        }

        private static bool TryGetMetaObject(ref DynamicMetaObject instance) {
            // If we're already a COM MO don't make a new one
            // (we do this to prevent recursion if we call Fallback from COM)
            if (instance is ComUnwrappedMetaObject) {
                return false;
            }

            if (IsComObject(instance.Value)) {
                instance = new ComMetaObject(instance.Expression, instance.Restrictions, instance.Value);
                return true;
            }

            return false;
        }
    }
}

#endif
