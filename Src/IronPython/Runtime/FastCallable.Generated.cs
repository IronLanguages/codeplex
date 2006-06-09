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
using System.Collections.Generic;
using System.Text;

using IronPython.Compiler;

namespace IronPython.Runtime {
    #region Generated CallTargets

    // *** BEGIN GENERATED CODE ***


    public delegate object CallTarget0();
    public delegate object CallTarget1(object arg0);
    public delegate object CallTarget2(object arg0, object arg1);
    public delegate object CallTarget3(object arg0, object arg1, object arg2);
    public delegate object CallTarget4(object arg0, object arg1, object arg2, object arg3);
    public delegate object CallTarget5(object arg0, object arg1, object arg2, object arg3, object arg4);


    public delegate object CallTargetWithContext0(ICallerContext context);
    public delegate object CallTargetWithContext1(ICallerContext context, object arg0);
    public delegate object CallTargetWithContext2(ICallerContext context, object arg0, object arg1);
    public delegate object CallTargetWithContext3(ICallerContext context, object arg0, object arg1, object arg2);
    public delegate object CallTargetWithContext4(ICallerContext context, object arg0, object arg1, object arg2, object arg3);
    public delegate object CallTargetWithContext5(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4);


    // *** END GENERATED CODE ***

    #endregion


    public abstract partial class FastCallable {
        #region Generated FastCallableMakers

        // *** BEGIN GENERATED CODE ***

        public static FastCallable Make(string name, bool needsContext, int nargs, Delegate target) {
            if (needsContext) {
                switch (nargs) {
                    case 0: return new FastCallableWithContext0(name, (CallTargetWithContext0)target);
                    case 1: return new FastCallableWithContext1(name, (CallTargetWithContext1)target);
                    case 2: return new FastCallableWithContext2(name, (CallTargetWithContext2)target);
                    case 3: return new FastCallableWithContext3(name, (CallTargetWithContext3)target);
                    case 4: return new FastCallableWithContext4(name, (CallTargetWithContext4)target);
                    case 5: return new FastCallableWithContext5(name, (CallTargetWithContext5)target);
                }
            }
            else {
                switch (nargs) {
                    case 0: return new FastCallable0(name, (CallTarget0)target);
                    case 1: return new FastCallable1(name, (CallTarget1)target);
                    case 2: return new FastCallable2(name, (CallTarget2)target);
                    case 3: return new FastCallable3(name, (CallTarget3)target);
                    case 4: return new FastCallable4(name, (CallTarget4)target);
                    case 5: return new FastCallable5(name, (CallTarget5)target);
                }
            }
            throw new NotImplementedException();
        }

        // *** END GENERATED CODE ***

        #endregion
    }




    #region Generated ConcreteFastCallables

    // *** BEGIN GENERATED CODE ***

    public class FastCallableAny : FastCallable {
        public CallTarget0 target0;
        public CallTarget1 target1;
        public CallTarget2 target2;
        public CallTarget3 target3;
        public CallTarget4 target4;
        public CallTarget5 target5;
        public CallTargetN targetN;
        public FastCallableAny(string name, int minArgs, int maxArgs) : base(name, minArgs, maxArgs) { }
        public override object Call(ICallerContext context) {
            if (target0 != null) return target0();
            return base.Call(context);
        }
        public override object Call(ICallerContext context, object arg0) {
            if (target1 != null) return target1(arg0);
            return base.Call(context, arg0);
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if (target2 != null) return target2(arg0, arg1);
            return base.Call(context, arg0, arg1);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if (target3 != null) return target3(arg0, arg1, arg2);
            return base.Call(context, arg0, arg1, arg2);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (target4 != null) return target4(arg0, arg1, arg2, arg3);
            return base.Call(context, arg0, arg1, arg2, arg3);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (target5 != null) return target5(arg0, arg1, arg2, arg3, arg4);
            return base.Call(context, arg0, arg1, arg2, arg3, arg4);
        }
        public override object CallInstance(ICallerContext context, object arg0) {
            if (target1 != null) return target1(arg0);
            return base.CallInstance(context, arg0);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1) {
            if (target2 != null) return target2(arg0, arg1);
            return base.CallInstance(context, arg0, arg1);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2) {
            if (target3 != null) return target3(arg0, arg1, arg2);
            return base.CallInstance(context, arg0, arg1, arg2);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (target4 != null) return target4(arg0, arg1, arg2, arg3);
            return base.CallInstance(context, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (target5 != null) return target5(arg0, arg1, arg2, arg3, arg4);
            return base.CallInstance(context, arg0, arg1, arg2, arg3, arg4);
        }
        public override object Call(ICallerContext context, params object[] args) {
            switch(args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                case 4: return Call(context, args[0], args[1], args[2], args[3]);
                case 5: return Call(context, args[0], args[1], args[2], args[3], args[4]);
            }
            if (targetN != null) return targetN(args);
            throw BadArgumentError(CallType.None, args.Length);
        }
        public override object CallInstance(ICallerContext context, object instance, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return CallInstance(context, instance, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }
        public override object CallInstance(ICallerContext context, object instance, params object[] args) {
            switch(args.Length) {
                case 0: return CallInstance(context, instance);
                case 1: return CallInstance(context, instance, args[0]);
                case 2: return CallInstance(context, instance, args[0], args[1]);
                case 3: return CallInstance(context, instance, args[0], args[1], args[2]);
                case 4: return CallInstance(context, instance, args[0], args[1], args[2], args[3]);
            }
            if (targetN != null) return targetN(PrependInstance(instance, args));
            throw BadArgumentError(CallType.None, args.Length+1);
        }
    }
    public class FastCallableWithContextAny : FastCallable {
        public CallTargetWithContext0 target0;
        public CallTargetWithContext1 target1;
        public CallTargetWithContext2 target2;
        public CallTargetWithContext3 target3;
        public CallTargetWithContext4 target4;
        public CallTargetWithContext5 target5;
        public CallTargetWithContextN targetN;
        public FastCallableWithContextAny(string name, int minArgs, int maxArgs) : base(name, minArgs, maxArgs) { }
        public override object Call(ICallerContext context) {
            if (target0 != null) return target0(context);
            return base.Call(context);
        }
        public override object Call(ICallerContext context, object arg0) {
            if (target1 != null) return target1(context, arg0);
            return base.Call(context, arg0);
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if (target2 != null) return target2(context, arg0, arg1);
            return base.Call(context, arg0, arg1);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if (target3 != null) return target3(context, arg0, arg1, arg2);
            return base.Call(context, arg0, arg1, arg2);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (target4 != null) return target4(context, arg0, arg1, arg2, arg3);
            return base.Call(context, arg0, arg1, arg2, arg3);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (target5 != null) return target5(context, arg0, arg1, arg2, arg3, arg4);
            return base.Call(context, arg0, arg1, arg2, arg3, arg4);
        }
        public override object CallInstance(ICallerContext context, object arg0) {
            if (target1 != null) return target1(context, arg0);
            return base.CallInstance(context, arg0);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1) {
            if (target2 != null) return target2(context, arg0, arg1);
            return base.CallInstance(context, arg0, arg1);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2) {
            if (target3 != null) return target3(context, arg0, arg1, arg2);
            return base.CallInstance(context, arg0, arg1, arg2);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (target4 != null) return target4(context, arg0, arg1, arg2, arg3);
            return base.CallInstance(context, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (target5 != null) return target5(context, arg0, arg1, arg2, arg3, arg4);
            return base.CallInstance(context, arg0, arg1, arg2, arg3, arg4);
        }
        public override object Call(ICallerContext context, params object[] args) {
            switch(args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                case 4: return Call(context, args[0], args[1], args[2], args[3]);
                case 5: return Call(context, args[0], args[1], args[2], args[3], args[4]);
            }
            if (targetN != null) return targetN(context, args);
            throw BadArgumentError(CallType.None, args.Length);
        }
        public override object CallInstance(ICallerContext context, object instance, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return CallInstance(context, instance, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }
        public override object CallInstance(ICallerContext context, object instance, params object[] args) {
            switch(args.Length) {
                case 0: return CallInstance(context, instance);
                case 1: return CallInstance(context, instance, args[0]);
                case 2: return CallInstance(context, instance, args[0], args[1]);
                case 3: return CallInstance(context, instance, args[0], args[1], args[2]);
                case 4: return CallInstance(context, instance, args[0], args[1], args[2], args[3]);
            }
            if (targetN != null) return targetN(context, PrependInstance(instance, args));
            throw BadArgumentError(CallType.None, args.Length+1);
        }
    }
    public class FastCallable0 : FastCallable {
        public CallTarget0 target0;
        public FastCallable0(string name, CallTarget0 target) : base(name, 0, 0) {
            this.target0 = target;
        }
        public override object Call(ICallerContext context) {
            if (target0 != null) return target0();
            return base.Call(context);
        }
    }
    public class FastCallableWithContext0 : FastCallable {
        public CallTargetWithContext0 target0;
        public FastCallableWithContext0(string name, CallTargetWithContext0 target) : base(name, 0, 0) {
            this.target0 = target;
        }
        public override object Call(ICallerContext context) {
            if (target0 != null) return target0(context);
            return base.Call(context);
        }
    }
    public class FastCallable1 : FastCallable {
        public CallTarget1 target1;
        public FastCallable1(string name, CallTarget1 target) : base(name, 1, 1) {
            this.target1 = target;
        }
        public override object Call(ICallerContext context, object arg0) {
            if (target1 != null) return target1(arg0);
            return base.Call(context, arg0);
        }
        public override object CallInstance(ICallerContext context, object arg0) {
            if (target1 != null) return target1(arg0);
            return base.CallInstance(context, arg0);
        }
    }
    public class FastCallableWithContext1 : FastCallable {
        public CallTargetWithContext1 target1;
        public FastCallableWithContext1(string name, CallTargetWithContext1 target) : base(name, 1, 1) {
            this.target1 = target;
        }
        public override object Call(ICallerContext context, object arg0) {
            if (target1 != null) return target1(context, arg0);
            return base.Call(context, arg0);
        }
        public override object CallInstance(ICallerContext context, object arg0) {
            if (target1 != null) return target1(context, arg0);
            return base.CallInstance(context, arg0);
        }
    }
    public class FastCallable2 : FastCallable {
        public CallTarget2 target2;
        public FastCallable2(string name, CallTarget2 target) : base(name, 2, 2) {
            this.target2 = target;
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if (target2 != null) return target2(arg0, arg1);
            return base.Call(context, arg0, arg1);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1) {
            if (target2 != null) return target2(arg0, arg1);
            return base.CallInstance(context, arg0, arg1);
        }
    }
    public class FastCallableWithContext2 : FastCallable {
        public CallTargetWithContext2 target2;
        public FastCallableWithContext2(string name, CallTargetWithContext2 target) : base(name, 2, 2) {
            this.target2 = target;
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if (target2 != null) return target2(context, arg0, arg1);
            return base.Call(context, arg0, arg1);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1) {
            if (target2 != null) return target2(context, arg0, arg1);
            return base.CallInstance(context, arg0, arg1);
        }
    }
    public class FastCallable3 : FastCallable {
        public CallTarget3 target3;
        public FastCallable3(string name, CallTarget3 target) : base(name, 3, 3) {
            this.target3 = target;
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if (target3 != null) return target3(arg0, arg1, arg2);
            return base.Call(context, arg0, arg1, arg2);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2) {
            if (target3 != null) return target3(arg0, arg1, arg2);
            return base.CallInstance(context, arg0, arg1, arg2);
        }
    }
    public class FastCallableWithContext3 : FastCallable {
        public CallTargetWithContext3 target3;
        public FastCallableWithContext3(string name, CallTargetWithContext3 target) : base(name, 3, 3) {
            this.target3 = target;
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if (target3 != null) return target3(context, arg0, arg1, arg2);
            return base.Call(context, arg0, arg1, arg2);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2) {
            if (target3 != null) return target3(context, arg0, arg1, arg2);
            return base.CallInstance(context, arg0, arg1, arg2);
        }
    }
    public class FastCallable4 : FastCallable {
        public CallTarget4 target4;
        public FastCallable4(string name, CallTarget4 target) : base(name, 4, 4) {
            this.target4 = target;
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (target4 != null) return target4(arg0, arg1, arg2, arg3);
            return base.Call(context, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (target4 != null) return target4(arg0, arg1, arg2, arg3);
            return base.CallInstance(context, arg0, arg1, arg2, arg3);
        }
    }
    public class FastCallableWithContext4 : FastCallable {
        public CallTargetWithContext4 target4;
        public FastCallableWithContext4(string name, CallTargetWithContext4 target) : base(name, 4, 4) {
            this.target4 = target;
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (target4 != null) return target4(context, arg0, arg1, arg2, arg3);
            return base.Call(context, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (target4 != null) return target4(context, arg0, arg1, arg2, arg3);
            return base.CallInstance(context, arg0, arg1, arg2, arg3);
        }
    }
    public class FastCallable5 : FastCallable {
        public CallTarget5 target5;
        public FastCallable5(string name, CallTarget5 target) : base(name, 5, 5) {
            this.target5 = target;
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (target5 != null) return target5(arg0, arg1, arg2, arg3, arg4);
            return base.Call(context, arg0, arg1, arg2, arg3, arg4);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (target5 != null) return target5(arg0, arg1, arg2, arg3, arg4);
            return base.CallInstance(context, arg0, arg1, arg2, arg3, arg4);
        }
    }
    public class FastCallableWithContext5 : FastCallable {
        public CallTargetWithContext5 target5;
        public FastCallableWithContext5(string name, CallTargetWithContext5 target) : base(name, 5, 5) {
            this.target5 = target;
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (target5 != null) return target5(context, arg0, arg1, arg2, arg3, arg4);
            return base.Call(context, arg0, arg1, arg2, arg3, arg4);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (target5 != null) return target5(context, arg0, arg1, arg2, arg3, arg4);
            return base.CallInstance(context, arg0, arg1, arg2, arg3, arg4);
        }
    }

    // *** END GENERATED CODE ***

    #endregion

}
