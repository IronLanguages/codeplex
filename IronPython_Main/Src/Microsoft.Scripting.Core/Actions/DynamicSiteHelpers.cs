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
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Binders {
    internal static partial class DynamicSiteHelpers {

        private static readonly Dictionary<Type, CreateSite> _siteCtors = new Dictionary<Type, CreateSite>();

        private delegate object CreateSite(CallSiteBinder binder);


        // TODO: do we need this helper?
        // if so, should it live on Expression?
        public static Type MakeCallSiteType(params Type[] types) {
            return typeof(CallSite<>).MakeGenericType(MakeCallSiteDelegate(types));
        }
        // TODO: do we need this helper?
        // if so, should it live on Expression?
        public static Type MakeCallSiteDelegate(params Type[] types) {
            ContractUtils.RequiresNotNull(types, "types");
            return DelegateHelpers.MakeDelegate(types.AddFirst(typeof(CallSite)));
        }

        public static CallSite MakeSite(CallSiteBinder binder, Type siteType) {
            CreateSite ctor;
            lock (_siteCtors) {
                if (!_siteCtors.TryGetValue(siteType, out ctor)) {
                    _siteCtors[siteType] = ctor = (CreateSite)Delegate.CreateDelegate(typeof(CreateSite), siteType.GetMethod("Create"));
                }
            }

            return (CallSite)ctor(binder);
        }

        /// <summary>
        /// Dynamic code generation required by dynamic sites needs to be able to call the delegate by which the
        /// call site is parametrized. If the delegate type is visible, we can generate into assembly (if saving
        /// assemblies). With delegate types that are not visible we must generate LCG in order to skip visibility.
        /// </summary>
        internal static DynamicILGen CreateDynamicMethod(bool visible, string name, Type returnType, Type[] parameters) {
            if (visible) {
                return Snippets.Shared.CreateDynamicMethod(name, returnType, parameters, false);
            } else {
                DynamicMethod dm = Helpers.CreateDynamicMethod(name, returnType, parameters);
                return new DynamicILGenMethod(dm, dm.GetILGenerator());
            }
        }
    }
}
