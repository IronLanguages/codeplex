/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Calls {
    public abstract partial class PythonFunction {
        #region Generated Function FastCallable Members

        // *** BEGIN GENERATED CODE ***

        public override object Call(ICallerContext context) {
            throw BadArgumentError(0);
        }
        public override object Call(ICallerContext context, object arg0) {
            throw BadArgumentError(1);
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            throw BadArgumentError(2);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            throw BadArgumentError(3);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            throw BadArgumentError(4);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            throw BadArgumentError(5);
        }
        public override object CallInstance(ICallerContext context, object arg0) {
            return Call(context, arg0);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1) {
            return Call(context, arg0, arg1);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2) {
            return Call(context, arg0, arg1, arg2);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            return Call(context, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return Call(context, arg0, arg1, arg2, arg3, arg4);
        }

        // *** END GENERATED CODE ***

        #endregion
    }

    #region Generated FunctionNs

    // *** BEGIN GENERATED CODE ***


    [PythonType(typeof(PythonFunction))]
    public class Function0 : PythonFunction {
        private CallTarget0 target;
        public Function0(PythonModule globals, string name, CallTarget0 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call(ICallerContext context) {
            if (!EnforceRecursion) return target();
            PushFrame();
            try {
                return target();
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                default: throw BadArgumentError(args.Length);
            }
        }
        public override bool Equals(object obj) {
            Function0 other = obj as Function0;
            if (other == null) return false;

            return target == other.target;
        }
        public override int GetHashCode() {
            return target.GetHashCode();
        }
        public override object Clone() {
            return new Function0(Module, Name, target, ArgNames, Defaults);
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function1 : PythonFunction {
        private CallTarget1 target;
        public Function1(PythonModule globals, string name, CallTarget1 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call(ICallerContext context) {
            if (Defaults.Length < 1) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0) {
            if (!EnforceRecursion) return target(arg0);
            PushFrame();
            try {
                return target(arg0);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                default: throw BadArgumentError(args.Length);
            }
        }
        public override bool Equals(object obj) {
            Function1 other = obj as Function1;
            if (other == null) return false;

            return target == other.target;
        }
        public override int GetHashCode() {
            return target.GetHashCode();
        }
        public override object Clone() {
            return new Function1(Module, Name, target, ArgNames, Defaults);
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function2 : PythonFunction {
        private CallTarget2 target;
        public Function2(PythonModule globals, string name, CallTarget2 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call(ICallerContext context) {
            if (Defaults.Length < 2) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0) {
            if (Defaults.Length < 1) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(arg0, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if (!EnforceRecursion) return target(arg0, arg1);
            PushFrame();
            try {
                return target(arg0, arg1);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                default: throw BadArgumentError(args.Length);
            }
        }
        public override bool Equals(object obj) {
            Function2 other = obj as Function2;
            if (other == null) return false;

            return target == other.target;
        }
        public override int GetHashCode() {
            return target.GetHashCode();
        }
        public override object Clone() {
            return new Function2(Module, Name, target, ArgNames, Defaults);
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function3 : PythonFunction {
        private CallTarget3 target;
        public Function3(PythonModule globals, string name, CallTarget3 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call(ICallerContext context) {
            if (Defaults.Length < 3) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0) {
            if (Defaults.Length < 2) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(arg0, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if (Defaults.Length < 1) throw BadArgumentError(2);
            if (!EnforceRecursion) return target(arg0, arg1, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if (!EnforceRecursion) return target(arg0, arg1, arg2);
            PushFrame();
            try {
                return target(arg0, arg1, arg2);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                default: throw BadArgumentError(args.Length);
            }
        }
        public override bool Equals(object obj) {
            Function3 other = obj as Function3;
            if (other == null) return false;

            return target == other.target;
        }
        public override int GetHashCode() {
            return target.GetHashCode();
        }
        public override object Clone() {
            return new Function3(Module, Name, target, ArgNames, Defaults);
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function4 : PythonFunction {
        private CallTarget4 target;
        public Function4(PythonModule globals, string name, CallTarget4 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call(ICallerContext context) {
            if (Defaults.Length < 4) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0) {
            if (Defaults.Length < 3) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(arg0, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if (Defaults.Length < 2) throw BadArgumentError(2);
            if (!EnforceRecursion) return target(arg0, arg1, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if (Defaults.Length < 1) throw BadArgumentError(3);
            if (!EnforceRecursion) return target(arg0, arg1, arg2, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (!EnforceRecursion) return target(arg0, arg1, arg2, arg3);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, arg3);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                case 4: return Call(context, args[0], args[1], args[2], args[3]);
                default: throw BadArgumentError(args.Length);
            }
        }
        public override bool Equals(object obj) {
            Function4 other = obj as Function4;
            if (other == null) return false;

            return target == other.target;
        }
        public override int GetHashCode() {
            return target.GetHashCode();
        }
        public override object Clone() {
            return new Function4(Module, Name, target, ArgNames, Defaults);
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function5 : PythonFunction {
        private CallTarget5 target;
        public Function5(PythonModule globals, string name, CallTarget5 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call(ICallerContext context) {
            if (Defaults.Length < 5) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Defaults[Defaults.Length - 5], Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Defaults[Defaults.Length - 5], Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0) {
            if (Defaults.Length < 4) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(arg0, Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if (Defaults.Length < 3) throw BadArgumentError(2);
            if (!EnforceRecursion) return target(arg0, arg1, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if (Defaults.Length < 2) throw BadArgumentError(3);
            if (!EnforceRecursion) return target(arg0, arg1, arg2, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (Defaults.Length < 1) throw BadArgumentError(4);
            if (!EnforceRecursion) return target(arg0, arg1, arg2, arg3, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, arg3, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (!EnforceRecursion) return target(arg0, arg1, arg2, arg3, arg4);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, arg3, arg4);
            } finally {
                PopFrame();
            }
        }
        public override object Call(ICallerContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                case 4: return Call(context, args[0], args[1], args[2], args[3]);
                case 5: return Call(context, args[0], args[1], args[2], args[3], args[4]);
                default: throw BadArgumentError(args.Length);
            }
        }
        public override bool Equals(object obj) {
            Function5 other = obj as Function5;
            if (other == null) return false;

            return target == other.target;
        }
        public override int GetHashCode() {
            return target.GetHashCode();
        }
        public override object Clone() {
            return new Function5(Module, Name, target, ArgNames, Defaults);
        }
    }


    // *** END GENERATED CODE ***

    #endregion


    public partial class FunctionN {
        #region Generated FunctionN FastCallable Members

        // *** BEGIN GENERATED CODE ***

        public override object Call(ICallerContext context) {
            return Call(new object[] { });
        }

        public override object Call(ICallerContext context, object arg0) {
            return Call(new object[] { arg0 });
        }

        public override object Call(ICallerContext context, object arg0, object arg1) {
            return Call(new object[] { arg0, arg1 });
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            return Call(new object[] { arg0, arg1, arg2 });
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            return Call(new object[] { arg0, arg1, arg2, arg3 });
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return Call(new object[] { arg0, arg1, arg2, arg3, arg4 });
        }


        // *** END GENERATED CODE ***

        #endregion
    }


    public partial class Method {
        public override object CallInstance(ICallerContext context, object arg0) {
            return Call(context, arg0);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1) {
            return Call(context, arg0, arg1);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2) {
            return Call(context, arg0, arg1, arg2);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            return Call(context, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return Call(context, arg0, arg1, arg2, arg3, arg4);
        }



        #region Generated Method FastCallable Members

        // *** BEGIN GENERATED CODE ***

        public override object Call(ICallerContext context) {
            FastCallable fc = func as FastCallable;
            if (fc != null) {
                if (inst != null) return fc.CallInstance(context, inst);
                else return fc.Call(context);
            } else {
                if (inst != null) return Ops.CallWithContext(context, func, inst);
                throw BadSelf(null);
            }
        }

        public override object Call(ICallerContext context, object arg0) {
            FastCallable fc = func as FastCallable;
            if (fc != null) {
                if (inst != null) return fc.CallInstance(context, inst, arg0);
                else return fc.Call(context, CheckSelf(arg0));
            } else {
                if (inst != null) return Ops.CallWithContext(context, func, inst, arg0);
                return Ops.CallWithContext(context, func, CheckSelf(arg0));
            }
        }

        public override object Call(ICallerContext context, object arg0, object arg1) {
            FastCallable fc = func as FastCallable;
            if (fc != null) {
                if (inst != null) return fc.CallInstance(context, inst, arg0, arg1);
                else return fc.Call(context, CheckSelf(arg0), arg1);
            } else {
                if (inst != null) return Ops.CallWithContext(context, func, inst, arg0, arg1);
                return Ops.CallWithContext(context, func, CheckSelf(arg0), arg1);
            }
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            FastCallable fc = func as FastCallable;
            if (fc != null) {
                if (inst != null) return fc.CallInstance(context, inst, arg0, arg1, arg2);
                else return fc.Call(context, CheckSelf(arg0), arg1, arg2);
            } else {
                if (inst != null) return Ops.CallWithContext(context, func, inst, arg0, arg1, arg2);
                return Ops.CallWithContext(context, func, CheckSelf(arg0), arg1, arg2);
            }
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            FastCallable fc = func as FastCallable;
            if (fc != null) {
                if (inst != null) return fc.CallInstance(context, inst, arg0, arg1, arg2, arg3);
                else return fc.Call(context, CheckSelf(arg0), arg1, arg2, arg3);
            } else {
                if (inst != null) return Ops.CallWithContext(context, func, inst, arg0, arg1, arg2, arg3);
                return Ops.CallWithContext(context, func, CheckSelf(arg0), arg1, arg2, arg3);
            }
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            FastCallable fc = func as FastCallable;
            if (fc != null) {
                if (inst != null) return fc.CallInstance(context, inst, arg0, arg1, arg2, arg3, arg4);
                else return fc.Call(context, CheckSelf(arg0), arg1, arg2, arg3, arg4);
            } else {
                if (inst != null) return Ops.CallWithContext(context, func, inst, arg0, arg1, arg2, arg3, arg4);
                return Ops.CallWithContext(context, func, CheckSelf(arg0), arg1, arg2, arg3, arg4);
            }
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
