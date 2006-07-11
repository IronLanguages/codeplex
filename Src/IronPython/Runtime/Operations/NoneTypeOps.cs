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
using System.Reflection;
using System.Diagnostics;
using System.Threading;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// The type representing Python's None and the CLS null
    /// </summary>
    public static class NoneTypeOps {
        internal static ReflectedType InstanceOfNoneType;
        public static ReflectedType MakeDynamicType() {
            if (InstanceOfNoneType == null) {
                OpsReflectedType ret = new OpsReflectedType("NoneType", typeof(void), typeof(NoneTypeOps), null);
                if (Interlocked.CompareExchange<ReflectedType>(ref InstanceOfNoneType, ret, null) == null)
                    return ret;
            }
            return InstanceOfNoneType;
        }


        [StaticOpsMethodAttribute("__init__")]
        public static void InitMethod(params object[] prms) {
            // nop
        }

        internal static int NoneHashCode = 0x1e1a1dd0;  // same as CPython.
        [StaticOpsMethodAttribute("__hash__")]
        public static int HashMethod() {
            return NoneHashCode;
        }

        [StaticOpsMethodAttribute("__repr__")]
        public static string ReprMethod() {
            return "None";
        }

        [StaticOpsMethodAttribute("__str__")]
        public static new string ToString() {
            return "None";
        }


        [PythonName("__new__")]
        public static object NewMethod(object type, params object[] prms) {
            if (type == InstanceOfNoneType) {
                throw Ops.TypeError("cannot create instances of 'NoneType'");
            }
            // someone is using  None.__new__ or type(None).__new__ to create
            // a new instance.  Call the type they want to create the instance for.
            return Ops.Call(type, prms);
        }

        [StaticOpsMethodAttribute("__delattr__")]
        public static void DelAttrMethod(string name) {
            InstanceOfNoneType.DelAttr(DefaultContext.Default, IronPython.Modules.Builtin.None, SymbolTable.StringToId(name));
        }

        [StaticOpsMethodAttribute("__getattribute__")]
        public static object GetAttributeMethod(string name) {
            return InstanceOfNoneType.GetAttr(DefaultContext.Default, IronPython.Modules.Builtin.None, SymbolTable.StringToId(name));
        }

        [StaticOpsMethodAttribute("__setattr__")]
        public static void SetAttrMethod(string name, object value) {
            InstanceOfNoneType.SetAttr(DefaultContext.Default, IronPython.Modules.Builtin.None, SymbolTable.StringToId(name), value);
        }
    }
}
