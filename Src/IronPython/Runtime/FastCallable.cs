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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using System.Threading;
using IronPython.Compiler;

namespace IronPython.Runtime {
    public delegate object CallTargetN(params object[] args);
    public delegate object CallTargetWithContextN(ICallerContext context, params object[] args);

    public abstract partial class FastCallable {

        public static Delegate MakeDelegate(MethodInfo mi) {
            if (mi.ReturnType != typeof(object)) return null;
            if (!mi.IsStatic) return null;

            ParameterInfo[] pis = mi.GetParameters();
            int nargs = pis.Length;
            bool needsContext = false;
            int argIndex = 0;
            if (pis.Length > 0 && pis[0].ParameterType == typeof(ICallerContext)) {
                needsContext = true;
                nargs -= 1;
                argIndex += 1;
            }

            if (nargs > Ops.MaximumCallArgs) return null;

            while (argIndex < pis.Length) {
                if (pis[argIndex++].ParameterType != typeof(object)) return null;
            }

            return CodeGen.CreateDelegate(mi, GetTargetType(needsContext, nargs));
        }

        //public static FastCallable Make(MethodInfo mi) {
        //}

        public static Type GetTargetType(bool needsContext, int nargs) {
            if (needsContext) {
                switch (nargs) {
                    case 0: return typeof(CallTargetWithContext0);
                    case 1: return typeof(CallTargetWithContext1);
                    case 2: return typeof(CallTargetWithContext2);
                    case 3: return typeof(CallTargetWithContext3);
                    case 4: return typeof(CallTargetWithContext4);
                    case 5: return typeof(CallTargetWithContext5);
                }
            } else {
                switch (nargs) {
                    case 0: return typeof(CallTarget0);
                    case 1: return typeof(CallTarget1);
                    case 2: return typeof(CallTarget2);
                    case 3: return typeof(CallTarget3);
                    case 4: return typeof(CallTarget4);
                    case 5: return typeof(CallTarget5);
                }
            }
            throw new NotImplementedException();
        }


        protected int minArgs, maxArgs;
        protected string name;
        protected FastCallable(string name, int minArgs, int maxArgs) {
            this.name = name;
            this.minArgs = minArgs;
            this.maxArgs = maxArgs;
        }

        public virtual object Call(ICallerContext context) { throw BadArgumentError(CallType.None, 0); }
        public virtual object Call(ICallerContext context, object arg0) { throw BadArgumentError(CallType.None, 1); }
        public virtual object Call(ICallerContext context, object arg0, object arg1) { throw BadArgumentError(CallType.None, 2); }
        public virtual object Call(ICallerContext context, object arg0, object arg1, object arg2) { throw BadArgumentError(CallType.None, 3); }
        public virtual object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) { throw BadArgumentError(CallType.None, 4); }
        public virtual object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) { throw BadArgumentError(CallType.None, 5); }
        public virtual object Call(ICallerContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                case 4: return Call(context, args[0], args[1], args[2], args[3]);
                case 5: return Call(context, args[0], args[1], args[2], args[3], args[4]);
            }
            throw BadArgumentError(CallType.None, args.Length);
        }
        public virtual object CallInstance(ICallerContext context, object instance) { throw BadArgumentError(CallType.ImplicitInstance, 1); }
        public virtual object CallInstance(ICallerContext context, object instance, object arg0) { throw BadArgumentError(CallType.ImplicitInstance, 2); }
        public virtual object CallInstance(ICallerContext context, object instance, object arg0, object arg1) { throw BadArgumentError(CallType.ImplicitInstance, 3); }
        public virtual object CallInstance(ICallerContext context, object instance, object arg0, object arg1, object arg2) { throw BadArgumentError(CallType.ImplicitInstance, 4); }
        public virtual object CallInstance(ICallerContext context, object instance, object arg0, object arg1, object arg2, object arg3) { throw BadArgumentError(CallType.ImplicitInstance, 5); }
        public virtual object CallInstance(ICallerContext context, object instance, object arg0, object arg1, object arg2, object arg3, object arg4) { throw BadArgumentError(CallType.ImplicitInstance, 6); }
        public virtual object CallInstance(ICallerContext context, object instance, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context, instance);
                case 1: return Call(context, instance, args[0]);
                case 2: return Call(context, instance, args[0], args[1]);
                case 3: return Call(context, instance, args[0], args[1], args[2]);
                case 4: return Call(context, instance, args[0], args[1], args[2], args[3]);
            }
            throw BadArgumentError(CallType.ImplicitInstance, args.Length + 1);
        }

        protected Exception BadArgumentError(CallType callType, int argCount) {
            int errMinArgs = minArgs;
            int errMaxArgs = maxArgs;
            if (callType == CallType.ImplicitInstance) {
                argCount -= 1;
                errMinArgs -= 1;
                errMaxArgs -= 1;
            }

            // This generates Python style error messages assuming that all arg counts in between min and max are allowed
            //It's possible that discontinuous sets of arg counts will produce a weird error message
            return PythonFunction.TypeErrorForIncorrectArgumentCount(name, errMaxArgs, errMaxArgs - errMinArgs, argCount);
        }
   
        public static object[] PrependInstance(object instance, object[] args) {
            object[] nargs = new object[args.Length + 1];
            nargs[0] = instance;
            for (int i=0; i < args.Length; i++) {
                nargs[i+1] = args[i];
            }
            return nargs;
        }
    }

    public class FastCallableUgly : FastCallable {
        private MethodBinder binder;

        //!!! bad args passed to parent
        public FastCallableUgly(MethodBinder binder)
            : base("", 0, 0) {
            this.binder = binder;
        }

        public override object Call(ICallerContext context) {
            return binder.Call(context, CallType.None, new object[] { });
        }
        public override object Call(ICallerContext context, object arg0) {
            return binder.Call(context, CallType.None, new object[] { arg0 });
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            return binder.Call(context, CallType.None, new object[] { arg0, arg1 });
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            return binder.Call(context, CallType.None, new object[] { arg0, arg1, arg2 });
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            return binder.Call(context, CallType.None, new object[] { arg0, arg1, arg2, arg3 });
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return binder.Call(context, CallType.None, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }

        public override object Call(ICallerContext context, params object[] args) {
            return binder.Call(context, CallType.None, args);
        }

        public override object CallInstance(ICallerContext context, object arg0) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0 });
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0, arg1 });
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0, arg1, arg2 });
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0, arg1, arg2, arg3 });
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0, arg1, arg2, arg3, arg4, arg5 });
        }

        public override object CallInstance(ICallerContext context, object instance, params object[] args) {
            object[] nargs = new object[args.Length + 1];
            nargs[0] = instance;
            args.CopyTo(nargs, 1);
            args = nargs;
            return binder.Call(context, CallType.ImplicitInstance, args);
        }
    }
}