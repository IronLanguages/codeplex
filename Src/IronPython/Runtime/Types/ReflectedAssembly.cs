/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    public class ReflectedAssemblyType : OpsReflectedType {
        static ReflectedAssemblyType AssemblyType;
        Dictionary<Assembly, TopReflectedPackage> assemblyMap = new Dictionary<Assembly, TopReflectedPackage>();

        internal static ReflectedType MakeDynamicType() {
            if (AssemblyType == null) {
                ReflectedAssemblyType rat = new ReflectedAssemblyType();
                if(Interlocked.CompareExchange<ReflectedAssemblyType>(ref AssemblyType, rat, null)==null)
                    return rat;
            }
            return AssemblyType;
        }

        public ReflectedAssemblyType()
            : base("Assembly", typeof(Assembly), typeof(ReflectedAssemblyType), null) {
        }

        public override bool TryGetAttr(ICallerContext context, object self, SymbolId name, out object ret) {
            Assembly asm = self as Assembly;
            TopReflectedPackage reflectedAssembly = GetReflectedAssembly(context.SystemState, asm);

            if (name == SymbolTable.Dict) {
                ret = reflectedAssembly.GetAttrDict(context);
                return true;
            }

            if (base.TryGetAttr(context, self, name, out ret)) {
                return true;
            }

            if (!reflectedAssembly.TryGetAttr(context, name, out ret))
                throw Ops.AttributeError("assembly {0} has no type {1}", asm.GetName().Name, name);

            return true;
        }

        internal TopReflectedPackage GetReflectedAssembly(SystemState state, Assembly assem) {
            Debug.Assert(assem != null);
            lock (this) {
                TopReflectedPackage reflectedAssembly;
                if (assemblyMap.TryGetValue(assem, out reflectedAssembly))
                    return reflectedAssembly;

                reflectedAssembly = new TopReflectedPackage(true);
                reflectedAssembly.LoadAssembly(state, assem);
                assemblyMap[assem] = reflectedAssembly;

                return reflectedAssembly;
            }
        }

        public override List GetAttrNames(ICallerContext context, object self) {
            Assembly asm = self as Assembly;
            TopReflectedPackage reflectedAssembly = GetReflectedAssembly(context.SystemState, asm);

            List ret = base.GetAttrNames(context, self);

            ret.AddRange(reflectedAssembly.GetAttrNames(context));
            return ret;
        }

        public override Dict GetAttrDict(ICallerContext context, object self) {
            List attrs = GetAttrNames(context, self);

            Dict res = new Dict();
            foreach (string o in attrs) {
                res[o] = GetAttr(context, self, SymbolTable.StringToId(o));
            }

            return res;
        }

    }

}
