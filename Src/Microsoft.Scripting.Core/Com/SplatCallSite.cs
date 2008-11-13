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
using System.Diagnostics;
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting.Binders;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.ComInterop {
    // First attempt at simple splatting call site helper
    internal sealed partial class SplatCallSite {
        internal delegate object SplatCaller(object[] args);

        // TODO: Should free these eventually
        private readonly SynchronizedDictionary<int, SplatCaller> _callers = new SynchronizedDictionary<int, SplatCaller>();
        private readonly CallSiteBinder _binder;

        public SplatCallSite(CallSiteBinder binder) {
            _binder = binder;
        }

        public CallSiteBinder Binder {
            get { return _binder; }
        }

        public object Invoke(object[] args) {
            Debug.Assert(args != null);

            SplatCaller caller;
            if (!_callers.TryGetValue(args.Length, out caller)) {
                _callers[args.Length] = caller = MakeCaller(args.Length);
            }

            return caller(args);
        }
        
        private SplatCaller MakeCaller(int args) {
            MethodInfo mi = GetType().GetMethod("CallHelper" + args);
            if (mi != null) {
                Type siteType = mi.GetParametersCached()[0].ParameterType;
                CallSite site = DynamicSiteHelpers.MakeSite(_binder, siteType);
                return (SplatCaller)Delegate.CreateDelegate(typeof(SplatCaller), site, mi);
            }
            return MakeBigCaller(args);
        }

        /// <summary>
        /// Uses LCG to create method such as this:
        /// 
        /// object SplatCaller(CallSite{T} site, object[] args) {
        ///      return site.Target(site, args[0], args[1], args[2], ...);
        /// }
        /// 
        /// where the CallSite is bound to the delegate
        /// </summary>
        /// <param name="args">the number of arguments</param>
        /// <returns>a SplatCaller delegate.</returns>
        private SplatCaller MakeBigCaller(int args) {
            // Get the dynamic site type
            Type siteDelegateType = DynamicSiteHelpers.MakeCallSiteDelegate(Helpers.RepeatedArray(typeof(object), args + 1));
            Type siteType = typeof(CallSite<>).MakeGenericType(new Type[] { siteDelegateType });

            DynamicILGen gen = Snippets.Shared.CreateDynamicMethod("_stub_SplatCaller", typeof(object), new Type[] { siteType, typeof(object[]) }, false);
            
            // Emit the site's target
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, siteType.GetField("Target"));

            // Emit the site
            gen.Emit(OpCodes.Ldarg_0);

            // Emit the arguments
            for (int i = 0; i < args; i++) {
                gen.Emit(OpCodes.Ldarg_1);
                gen.EmitInt(i);
                gen.Emit(OpCodes.Ldelem_Ref);
            }
            
            // Invoke the target
            gen.EmitCall(siteDelegateType.GetMethod("Invoke"));
            gen.Emit(OpCodes.Ret);

            // Create the delegate and callsite
            CallSite callSite = DynamicSiteHelpers.MakeSite(_binder, siteType);
            return gen.Finish().CreateDelegate<SplatCaller>(callSite);
        }
    }
}
