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

namespace IronPython.Runtime {
    public abstract partial class PythonFunction {
        #region Generated Function IFastCallable Members

        // *** BEGIN GENERATED CODE ***

        public virtual object Call() {
            throw BadArgumentError(0);
        }
        public virtual object Call(object arg0) {
            throw BadArgumentError(1);
        }
        public virtual object Call(object arg0, object arg1) {
            throw BadArgumentError(2);
        }
        public virtual object Call(object arg0, object arg1, object arg2) {
            throw BadArgumentError(3);
        }
        public virtual object Call(object arg0, object arg1, object arg2, object arg3) {
            throw BadArgumentError(4);
        }
        public virtual object Call(object arg0, object arg1, object arg2, object arg3, object arg4) {
            throw BadArgumentError(5);
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
        public override object Call() {
            if(!EnforceRecursion) return target();
            PushFrame();
            try {
                return target();
            } finally {
                PopFrame();
            }
        }
        public override object Call(params object[] args) {
            switch (args.Length) {
                case 0: return Call();
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
                return new Function0(Module, Name, target, argNames, defaults);
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function1 : PythonFunction {
        private CallTarget1 target;
        public Function1(PythonModule globals, string name, CallTarget1 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call() {
            if (defaults.Length < 1) throw BadArgumentError(0);
            if(!EnforceRecursion) return target(defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0) {
            if(!EnforceRecursion) return target(arg0);
            PushFrame();
            try {
                return target(arg0);
            } finally {
                PopFrame();
            }
        }
        public override object Call(params object[] args) {
            switch (args.Length) {
                case 0: return Call();
                case 1: return Call(args[0]);
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
                return new Function1(Module, Name, target, argNames, defaults);
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function2 : PythonFunction {
        private CallTarget2 target;
        public Function2(PythonModule globals, string name, CallTarget2 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call() {
            if (defaults.Length < 2) throw BadArgumentError(0);
            if(!EnforceRecursion) return target(defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0) {
            if (defaults.Length < 1) throw BadArgumentError(1);
            if(!EnforceRecursion) return target(arg0, defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0, object arg1) {
            if(!EnforceRecursion) return target(arg0, arg1);
            PushFrame();
            try {
                return target(arg0, arg1);
            } finally {
                PopFrame();
            }
        }
        public override object Call(params object[] args) {
            switch (args.Length) {
                case 0: return Call();
                case 1: return Call(args[0]);
                case 2: return Call(args[0], args[1]);
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
                return new Function2(Module, Name, target, argNames, defaults);
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function3 : PythonFunction {
        private CallTarget3 target;
        public Function3(PythonModule globals, string name, CallTarget3 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call() {
            if (defaults.Length < 3) throw BadArgumentError(0);
            if(!EnforceRecursion) return target(defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0) {
            if (defaults.Length < 2) throw BadArgumentError(1);
            if(!EnforceRecursion) return target(arg0, defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0, object arg1) {
            if (defaults.Length < 1) throw BadArgumentError(2);
            if(!EnforceRecursion) return target(arg0, arg1, defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0, object arg1, object arg2) {
            if(!EnforceRecursion) return target(arg0, arg1, arg2);
            PushFrame();
            try {
                return target(arg0, arg1, arg2);
            } finally {
                PopFrame();
            }
        }
        public override object Call(params object[] args) {
            switch (args.Length) {
                case 0: return Call();
                case 1: return Call(args[0]);
                case 2: return Call(args[0], args[1]);
                case 3: return Call(args[0], args[1], args[2]);
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
                return new Function3(Module, Name, target, argNames, defaults);
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function4 : PythonFunction {
        private CallTarget4 target;
        public Function4(PythonModule globals, string name, CallTarget4 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call() {
            if (defaults.Length < 4) throw BadArgumentError(0);
            if(!EnforceRecursion) return target(defaults[defaults.Length - 4], defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(defaults[defaults.Length - 4], defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0) {
            if (defaults.Length < 3) throw BadArgumentError(1);
            if(!EnforceRecursion) return target(arg0, defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0, object arg1) {
            if (defaults.Length < 2) throw BadArgumentError(2);
            if(!EnforceRecursion) return target(arg0, arg1, defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0, object arg1, object arg2) {
            if (defaults.Length < 1) throw BadArgumentError(3);
            if(!EnforceRecursion) return target(arg0, arg1, arg2, defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3) {
            if(!EnforceRecursion) return target(arg0, arg1, arg2, arg3);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, arg3);
            } finally {
                PopFrame();
            }
        }
        public override object Call(params object[] args) {
            switch (args.Length) {
                case 0: return Call();
                case 1: return Call(args[0]);
                case 2: return Call(args[0], args[1]);
                case 3: return Call(args[0], args[1], args[2]);
                case 4: return Call(args[0], args[1], args[2], args[3]);
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
                return new Function4(Module, Name, target, argNames, defaults);
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function5 : PythonFunction {
        private CallTarget5 target;
        public Function5(PythonModule globals, string name, CallTarget5 target, string[] argNames, object[] defaults)
            : base(globals, name, argNames, defaults) {
            this.target = target;
        }
        public override object Call() {
            if (defaults.Length < 5) throw BadArgumentError(0);
            if(!EnforceRecursion) return target(defaults[defaults.Length - 5], defaults[defaults.Length - 4], defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(defaults[defaults.Length - 5], defaults[defaults.Length - 4], defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0) {
            if (defaults.Length < 4) throw BadArgumentError(1);
            if(!EnforceRecursion) return target(arg0, defaults[defaults.Length - 4], defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, defaults[defaults.Length - 4], defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0, object arg1) {
            if (defaults.Length < 3) throw BadArgumentError(2);
            if(!EnforceRecursion) return target(arg0, arg1, defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, defaults[defaults.Length - 3], defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0, object arg1, object arg2) {
            if (defaults.Length < 2) throw BadArgumentError(3);
            if(!EnforceRecursion) return target(arg0, arg1, arg2, defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, defaults[defaults.Length - 2], defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3) {
            if (defaults.Length < 1) throw BadArgumentError(4);
            if(!EnforceRecursion) return target(arg0, arg1, arg2, arg3, defaults[defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, arg3, defaults[defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3, object arg4) {
            if(!EnforceRecursion) return target(arg0, arg1, arg2, arg3, arg4);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, arg3, arg4);
            } finally {
                PopFrame();
            }
        }
        public override object Call(params object[] args) {
            switch (args.Length) {
                case 0: return Call();
                case 1: return Call(args[0]);
                case 2: return Call(args[0], args[1]);
                case 3: return Call(args[0], args[1], args[2]);
                case 4: return Call(args[0], args[1], args[2], args[3]);
                case 5: return Call(args[0], args[1], args[2], args[3], args[4]);
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
                return new Function5(Module, Name, target, argNames, defaults);
        }
    }


    // *** END GENERATED CODE ***

    #endregion   


    public partial class FunctionN {
        #region Generated FunctionN IFastCallable Members

        // *** BEGIN GENERATED CODE ***

        public override object Call() {
            return Call(new object[] { });
        }

        public override object Call(object arg0) {
            return Call(new object[] { arg0});
        }

        public override object Call(object arg0, object arg1) {
            return Call(new object[] { arg0, arg1});
        }

        public override object Call(object arg0, object arg1, object arg2) {
            return Call(new object[] { arg0, arg1, arg2});
        }

        public override object Call(object arg0, object arg1, object arg2, object arg3) {
            return Call(new object[] { arg0, arg1, arg2, arg3});
        }

        public override object Call(object arg0, object arg1, object arg2, object arg3, object arg4) {
            return Call(new object[] { arg0, arg1, arg2, arg3, arg4});
        }


        // *** END GENERATED CODE ***

        #endregion
    }


    public partial class Method {
        #region Generated Method IFastCallable Members

        // *** BEGIN GENERATED CODE ***

        public object Call() {
            if (inst != null) return Ops.Call(func, inst);
            throw BadSelf(null);
        }

        public object Call(object arg0) {
            PythonFunction f = func as PythonFunction;
            if(inst != null) {
                if (f != null) return f.Call(inst, arg0);
                return Ops.Call(func, inst, arg0);
            } else {
                if (!Modules.Builtin.IsInstance(arg0, DeclaringClass)) throw BadSelf(arg0);
                if (f != null) return f.Call(arg0);
                return Ops.Call(func, arg0);
            }
        }

        public object Call(object arg0, object arg1) {
            PythonFunction f = func as PythonFunction;
            if(inst != null) {
                if (f != null) return f.Call(inst, arg0, arg1);
                return Ops.Call(func, inst, arg0, arg1);
            } else {
                if (!Modules.Builtin.IsInstance(arg0, DeclaringClass)) throw BadSelf(arg0);
                if (f != null) return f.Call(arg0, arg1);
                return Ops.Call(func, arg0, arg1);
            }
        }

        public object Call(object arg0, object arg1, object arg2) {
            PythonFunction f = func as PythonFunction;
            if(inst != null) {
                if (f != null) return f.Call(inst, arg0, arg1, arg2);
                return Ops.Call(func, inst, arg0, arg1, arg2);
            } else {
                if (!Modules.Builtin.IsInstance(arg0, DeclaringClass)) throw BadSelf(arg0);
                if (f != null) return f.Call(arg0, arg1, arg2);
                return Ops.Call(func, arg0, arg1, arg2);
            }
        }

        public object Call(object arg0, object arg1, object arg2, object arg3) {
            PythonFunction f = func as PythonFunction;
            if(inst != null) {
                if (f != null) return f.Call(inst, arg0, arg1, arg2, arg3);
                return Ops.Call(func, inst, arg0, arg1, arg2, arg3);
            } else {
                if (!Modules.Builtin.IsInstance(arg0, DeclaringClass)) throw BadSelf(arg0);
                if (f != null) return f.Call(arg0, arg1, arg2, arg3);
                return Ops.Call(func, arg0, arg1, arg2, arg3);
            }
        }

        public object Call(object arg0, object arg1, object arg2, object arg3, object arg4) {
            PythonFunction f = func as PythonFunction;
            if(inst != null) {
                if (f != null) return f.Call(inst, arg0, arg1, arg2, arg3, arg4);
                return Ops.Call(func, inst, arg0, arg1, arg2, arg3, arg4);
            } else {
                if (!Modules.Builtin.IsInstance(arg0, DeclaringClass)) throw BadSelf(arg0);
                if (f != null) return f.Call(arg0, arg1, arg2, arg3, arg4);
                return Ops.Call(func, arg0, arg1, arg2, arg3, arg4);
            }
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
