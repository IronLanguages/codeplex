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

using IronPython.Compiler;

namespace IronPython.Runtime {
    public class ReflectedDelegateType : ReflectedType {
        BuiltinFunction invoker;

        public ReflectedDelegateType(Type delegateType)
            : base(delegateType) {
        }

        public override object Call(object func, params object[] args) {
            if (invoker == null) {
                CreateInvoker();
            }
            
            // put delegate's Target object into args
            Delegate d = func as Delegate;
            Debug.Assert(d != null);
            
            object[] newArgs = new object[args.Length + 1];
            newArgs[0] = d.Target;
            Array.Copy(args, 0, newArgs, 1, args.Length);

            return invoker.Call(newArgs);
        }

        private void CreateInvoker() {
            MethodInfo delegateInfo = type.GetMethod("Invoke");
            Debug.Assert(delegateInfo != null);
            ReflectedMethod unOpt = new ReflectedMethod("invoke", delegateInfo, FunctionType.Method);
            invoker = ReflectOptimizer.MakeFunction(unOpt);
            if (invoker == null) invoker = unOpt;
        }
    }

}
