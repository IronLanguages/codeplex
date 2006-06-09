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
    /// <summary>
    /// ReflectedMethod's are unoptimized BuiltinFunction's.  They use late-bound
    /// invocation to call their target.
    /// 
    /// ReflectedMethod's also currently support certain functionality optimized
    /// methods don't. This includes:
    ///    
    /// context-aware calls
    /// generic-type binding
    /// TryCall
    /// 
    /// </summary>
    public abstract partial class ReflectedMethodBase : BuiltinFunction, ITryCallable, ICallableWithCallerContext {
        protected FastCallable fastCallable;

        #region Constructors

        protected ReflectedMethodBase() { }
        
        public ReflectedMethodBase(string name, MethodBase target)
            : base(name, new MethodBase[] { target }, FunctionType.PythonVisible) {
            UpdateFunctionInfo(target);
        }

        public ReflectedMethodBase(string name, MethodBase target, FunctionType functionType)
            : base(name, new MethodBase[] { target }, functionType) {
            UpdateFunctionInfo(target);
        }

        public ReflectedMethodBase(string name, MethodBase[] targets)
            : base(name, targets, FunctionType.None) {
            for (int i = 0; i < targets.Length; i++) {
                UpdateFunctionInfo(targets[i]);
            }
        }

        #endregion

        #region Public APIs
        public void AddMethod(MethodBase info) {
            if (targets != null) {
                MethodBase[] ni = new MethodBase[targets.Length + 1];
                targets.CopyTo(ni, 0);
                ni[targets.Length] = info;
                targets = ni;
            } else {
                targets = new MethodBase[] { info };
            }
            UpdateFunctionInfo(info);
        }

        public virtual bool TryCall(object[] args, out object ret) {
            //!!! This too is bad
            try {
                ret = Call(null, args);
                return true;
            } catch (ArgumentTypeException) {
                ret = null;
                return false;
            }
        }
        
        #endregion

        #region Protected APIs

        protected FastCallable MakeFastCallable() {
            return new MethodBinder(Name, MethodTracker.GetTrackerArray(targets), FunctionType).MakeFastCallable();
        }

        protected FastCallable MyFastCallable {
            get {
                if (fastCallable == null) {
                    fastCallable = MakeFastCallable();
                }
                return fastCallable;
            }
        }

        #endregion

        #region Callables

        [PythonName("__call__")]
        public override object Call(params object[] args) {
            if (HasInstance) return MyFastCallable.CallInstance(null, Instance, args);
            else return MyFastCallable.Call(null, args);
        }


        [PythonName("__call__")]
        public override object Call(ICallerContext context, params object[] args) {
            if (HasInstance) return MyFastCallable.CallInstance(context, Instance, args);
            else return MyFastCallable.Call(context, args);
        }

        #endregion
    }

    public class ReflectedConstructor : ReflectedMethodBase {
        public ReflectedConstructor(string name, ConstructorInfo info) : base(name, info) { }

        public override string ToString() {
            return string.Format("<constructor# for {0}>", targets[0].DeclaringType.FullName);
        }

        #region Protected overrides

        protected override bool HasInstance {
            get {
                return false;
            }
        }

        protected override object Instance {
            get {
                return null;
            }
        }

        #endregion      

    }

    public class ReflectedMethod : ReflectedMethodBase, IMapping, IContextAwareMember {
        [PythonName("__new__")]
        public static object MakeNew(object cls, object callable, object instance){
            return Ops.GetDescriptor(callable, instance, null);
        }

        public static ReflectedMethod MakeMethod(string name, MethodBase info, FunctionType ft) {
            return new ReflectedMethod(name, new MethodBase[] { info }, null, ft);
        }

        public static ReflectedMethod MakeMethod(string name, MethodBase[] infos, FunctionType ft) {
            return new ReflectedMethod(name, infos, null, ft);
        }

        public ReflectedMethod() { }

        public ReflectedMethod(string name, MethodInfo info, NameType nt) :
            base(name, info, nt == NameType.PythonMethod ? FunctionType.PythonVisible | FunctionType.Method : FunctionType.Method) {
        }

        public ReflectedMethod(string name, MethodInfo info, FunctionType funcType)
            :
            base(name, info, funcType) {
        }

        protected ReflectedMethod(string name, MethodBase[] infos, object instance, FunctionType funcType)
            : base(name, infos) {
            base.inst = instance;
            FunctionType = funcType;
        }

        public override BuiltinFunction Clone() {
            ReflectedMethod ret = new ReflectedMethod(Name, targets, null, FunctionType);
            ret.fastCallable = this.MyFastCallable;
            return ret;
        }
               

        #region IContextAwareMember Members

        bool IContextAwareMember.IsVisible(ICallerContext context) {
            return (context.ContextFlags & CallerContextFlags.ShowCls) != 0 ||
                IsPythonVisible;
        }

        #endregion
    }

    public class ReflectedUnboundMethod : ReflectedMethod {
        public ReflectedUnboundMethod() { }

        public ReflectedUnboundMethod(string name, MethodInfo info, NameType nt) : base(name, info, nt) { }

        public ReflectedUnboundMethod(string name, MethodInfo info, FunctionType funcType) : base(name, info, funcType) { }

        private ReflectedUnboundMethod(string name, MethodBase[] infos, object instance, FunctionType funcType) : base(name, infos, instance, funcType) { }


        protected override bool HasInstance {
            get {
                return Instance != null;
            }
        }

        public override BuiltinFunction Clone() {
            ReflectedUnboundMethod ret = new ReflectedUnboundMethod(Name, targets, null, FunctionType);
            ret.fastCallable = this.MyFastCallable;
            return ret;
        }
       
    }

    public class ReflectedUnboundReverseOp : ReflectedMethod {
        public ReflectedUnboundReverseOp() { }

        public ReflectedUnboundReverseOp(string name, MethodInfo info, NameType nt)
            : base(name, info, nt) {
        }

        private ReflectedUnboundReverseOp(string name, MethodBase[] infos, object instance, FunctionType funcType)
            : base(name, infos, instance, funcType) {
        }

        public override bool TryCall(object[] args, out object ret) {
            if (Instance == null) {
                object tmp = args[0];
                args[0] = args[1];
                args[1] = tmp;
                return base.TryCall(args, out ret);
            }

            object[] nargs = new object[args.Length + 1];
            args.CopyTo(nargs, 0);
            nargs[args.Length] = Instance;

            return base.TryCall(nargs, out ret);
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

        protected override bool HasInstance {
            get {
                return false;
            }
        }

        public override BuiltinFunction Clone() {
            ReflectedUnboundReverseOp ret = new ReflectedUnboundReverseOp(Name, targets, null, FunctionType);
            ret.fastCallable = this.MyFastCallable;
            return ret;
        }       

    }

    // Used to map signatures to specific targets on the embedded reflected method.
    public class BuiltinFunctionOverloadMapper {

        private BuiltinFunction function;

        public BuiltinFunctionOverloadMapper(BuiltinFunction builtinFunction) {
            this.function = builtinFunction;
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
                ReflectedMethod rm = new ReflectedMethod();
                rm.Name = function.Name;
                rm.FunctionType = function.FunctionType|FunctionType.OptimizeChecked; // don't allow optimization that would whack the real entry
                rm.inst = function.inst;

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

                return rm;
            }
            set {
                throw new NotImplementedException();
            }
        }
    }
}
