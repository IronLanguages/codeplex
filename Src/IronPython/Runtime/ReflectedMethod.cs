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
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using System.Diagnostics;
using IronPython.Compiler;
using IronPython.Modules;
using IronMath;

namespace IronPython.Runtime {


    public class ReflectedUnboundReverseOp : BuiltinFunction {
        public ReflectedUnboundReverseOp() { }

        //private ReflectedUnboundReverseOp(string name, MethodBase[] infos, object instance, FunctionType funcType)
        //    : base(name, infos, funcType) {
        //    inst = instance;
        //}

        public override bool TryCall(object arg, out object ret) {
            throw new NotImplementedException();
        }

        internal override bool TryCall(object arg0, object arg1, out object ret) {
            return base.TryCall(arg1, arg0, out ret);
        }

        //!!! not needed today but obviously will be trouble not to do right in the future
        //public override object Call(params object[] args) {
        //    if (Instance == null) {
        //        return base.Call(args);
        //    }
        //    object[] nargs = new object[args.Length + 1];
        //    args.CopyTo(nargs, 0);
        //    nargs[args.Length] = Instance;

        //    return base.Call(nargs);
        //}

        //public override BuiltinFunction Clone() {
        //    ReflectedUnboundReverseOp ret = new ReflectedUnboundReverseOp(Name, targets, null, FunctionType);
        //    ret.optimizedTarget = this.OptimizedTarget;
        //    return ret;
        //}       

    }

    // Used to map signatures to specific targets on the embedded reflected method.
    public class BuiltinFunctionOverloadMapper {

        private BuiltinFunction function;
        private object instance;

        public BuiltinFunctionOverloadMapper(BuiltinFunction builtinFunction, object instance) {
            this.function = builtinFunction;
            this.instance = instance;
        }

        public override string ToString() {
            Dict overloadList = new Dict();
            foreach (MethodBase mb in function.Targets) {
                string key = ReflectionUtil.CreateAutoDoc(mb);
                overloadList[key] = function;
            }
            return overloadList.ToString();
        }

        public object this[object key] {
            get {
                // Retrieve the signature from the index.
                Type[] sig;
                Tuple sigTuple = key as Tuple;

                if (sigTuple != null) {
                    sig = new Type[sigTuple.Count];
                    for (int i = 0; i < sig.Length; i++) {
                        sig[i] = Converter.ConvertToType(sigTuple[i]);
                    }
                } else {
                    sig = new Type[] { Converter.ConvertToType(key) };
                }

                // We can still end up with more than one target since generic and non-generic
                // methods can share the same name and signature. So we'll build up a new
                // reflected method with all the candidate targets. A caller can then index this
                // reflected method if necessary in order to provide generic type arguments and
                // fully disambiguate the target.
                BuiltinFunction rm = new BuiltinFunction();
                rm.Name = function.Name;
                rm.FunctionType = function.FunctionType|FunctionType.OptimizeChecked; // don't allow optimization that would whack the real entry


                // Search for targets with the right number of arguments.
                int args = sig.Length;
                foreach (MethodBase mb in function.Targets) {
                    ParameterInfo[] pis = mb.GetParameters();
                    if (pis.Length != args)
                        continue;

                    // Check each parameter type for an exact match.
                    bool match = true;
                    for (int i = 0; i < args; i++)
                        if (pis[i].ParameterType != sig[i]) {
                            match = false;
                            break;
                        }
                    if (!match)
                        continue;

                    // Okay, we have a match, add it to the list.
                    rm.AddMethod(mb);
                }
                if (rm.Targets == null)
                    throw Ops.TypeError("No match found for the method signature {0}", key);

                if (instance != null) {
                    return new BoundBuiltinFunction(rm, instance);
                } else {
                    return rm;
                }
            }
        }
    }
}
