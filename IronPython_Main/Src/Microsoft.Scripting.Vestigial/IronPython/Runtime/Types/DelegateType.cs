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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Reflection.Emit;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Compiler.Generation;
using IronPython.Compiler;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Internal;

[assembly: PythonExtensionType(typeof(Delegate), typeof(DelegateOps))]
namespace IronPython.Runtime.Types {
    public static class DelegateOps {
        [StaticOpsMethod("__new__")]
        public static object MakeNew(CodeContext context, DynamicType type, object function) {
            if (type == null) throw Ops.TypeError("expected type for 1st param, got {0}", Ops.GetDynamicType(type));

            return Ops.GetDelegate(function, type.UnderlyingSystemType);
        }

        public class DynamicTypeDelegateCallSlot : DynamicTypeSlot {
            private BuiltinFunction _invoker;

            private void CreateInvoker(DynamicType dt) {
                MethodInfo delegateInfo = dt.UnderlyingSystemType.GetMethod("Invoke");
                Debug.Assert(delegateInfo != null);
                _invoker = BuiltinFunction.MakeMethod("invoke", delegateInfo, FunctionType.Function | FunctionType.AlwaysVisible);
            }

            public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
                if (_invoker == null) CreateInvoker((DynamicType)owner);

                value = new DelegateInvoker(instance, _invoker);
                return true;
            }
        }

        public class DelegateInvoker : ICallableWithCodeContext {
            private BuiltinFunction _invoker;
            private object _instance;

            public DelegateInvoker(object instance, BuiltinFunction invoker) {
                _instance = instance;
                _invoker = invoker;
            }

            #region ICallableWithCodeContext Members

            [OperatorMethodAttribute]
            public object Call(CodeContext context, object[] args) {
                return _invoker.CallInstance(context, _instance, args);
            }

            #endregion
        }
    }    
}
