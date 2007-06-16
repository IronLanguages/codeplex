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
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Calls {
    public abstract partial class PythonFunction {
        #region Generated FunctionMaker

        // *** BEGIN GENERATED CODE ***

        private static PythonFunction MakeFunction(CodeContext context, string name, Delegate target, string[] argNames, object[] defaults, FunctionAttributes attributes) {
            if (attributes == FunctionAttributes.None) {
                if (target.GetType() == typeof(CallTarget0)) {
                    return new Function0(context, name, (CallTarget0)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTargetWithContext0)) {
                    return new FunctionWithContext0(context, name, (CallTargetWithContext0)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTarget1)) {
                    return new Function1(context, name, (CallTarget1)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTargetWithContext1)) {
                    return new FunctionWithContext1(context, name, (CallTargetWithContext1)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTarget2)) {
                    return new Function2(context, name, (CallTarget2)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTargetWithContext2)) {
                    return new FunctionWithContext2(context, name, (CallTargetWithContext2)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTarget3)) {
                    return new Function3(context, name, (CallTarget3)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTargetWithContext3)) {
                    return new FunctionWithContext3(context, name, (CallTargetWithContext3)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTarget4)) {
                    return new Function4(context, name, (CallTarget4)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTargetWithContext4)) {
                    return new FunctionWithContext4(context, name, (CallTargetWithContext4)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTarget5)) {
                    return new Function5(context, name, (CallTarget5)target, argNames, defaults);
                }
                if (target.GetType() == typeof(CallTargetWithContext5)) {
                    return new FunctionWithContext5(context, name, (CallTargetWithContext5)target, argNames, defaults);
                }
                return new FunctionN(context, name, (CallTargetWithContextN)target, argNames, defaults);
            } else {
                return new FunctionX(context, name, (CallTargetWithContextN)target, argNames, defaults, attributes);
            }
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated Function FastCallable Members

        // *** BEGIN GENERATED CODE ***

        public override object Call(CodeContext context) {
            throw BadArgumentError(0);
        }
        public override object Call(CodeContext context, object arg0) {
            throw BadArgumentError(1);
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            throw BadArgumentError(2);
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            throw BadArgumentError(3);
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            throw BadArgumentError(4);
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            throw BadArgumentError(5);
        }
        public override object CallInstance(CodeContext context, object arg0) {
            return Call(context, arg0);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1) {
            return Call(context, arg0, arg1);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2) {
            return Call(context, arg0, arg1, arg2);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            return Call(context, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
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
        public Function0(CodeContext context, string name, CallTarget0 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (!EnforceRecursion) return target();
            PushFrame();
            try {
                return target();
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                default: throw BadArgumentError(args.Length);
            }
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class FunctionWithContext0 : PythonFunction {
        private CallTargetWithContext0 target;
        public FunctionWithContext0(CodeContext context, string name, CallTargetWithContext0 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (!EnforceRecursion) return target(Context);
            PushFrame();
            try {
                return target(Context);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                default: throw BadArgumentError(args.Length);
            }
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function1 : PythonFunction {
        private CallTarget1 target;
        public Function1(CodeContext context, string name, CallTarget1 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (Defaults.Length < 1) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0) {
            if (!EnforceRecursion) return target(arg0);
            PushFrame();
            try {
                return target(arg0);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                default: throw BadArgumentError(args.Length);
            }
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class FunctionWithContext1 : PythonFunction {
        private CallTargetWithContext1 target;
        public FunctionWithContext1(CodeContext context, string name, CallTargetWithContext1 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (Defaults.Length < 1) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Context, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0) {
            if (!EnforceRecursion) return target(Context, arg0);
            PushFrame();
            try {
                return target(Context, arg0);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                default: throw BadArgumentError(args.Length);
            }
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function2 : PythonFunction {
        private CallTarget2 target;
        public Function2(CodeContext context, string name, CallTarget2 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (Defaults.Length < 2) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0) {
            if (Defaults.Length < 1) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(arg0, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            if (!EnforceRecursion) return target(arg0, arg1);
            PushFrame();
            try {
                return target(arg0, arg1);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                default: throw BadArgumentError(args.Length);
            }
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class FunctionWithContext2 : PythonFunction {
        private CallTargetWithContext2 target;
        public FunctionWithContext2(CodeContext context, string name, CallTargetWithContext2 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (Defaults.Length < 2) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Context, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0) {
            if (Defaults.Length < 1) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(Context, arg0, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, arg0, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            if (!EnforceRecursion) return target(Context, arg0, arg1);
            PushFrame();
            try {
                return target(Context, arg0, arg1);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                default: throw BadArgumentError(args.Length);
            }
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function3 : PythonFunction {
        private CallTarget3 target;
        public Function3(CodeContext context, string name, CallTarget3 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (Defaults.Length < 3) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0) {
            if (Defaults.Length < 2) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(arg0, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            if (Defaults.Length < 1) throw BadArgumentError(2);
            if (!EnforceRecursion) return target(arg0, arg1, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            if (!EnforceRecursion) return target(arg0, arg1, arg2);
            PushFrame();
            try {
                return target(arg0, arg1, arg2);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                default: throw BadArgumentError(args.Length);
            }
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class FunctionWithContext3 : PythonFunction {
        private CallTargetWithContext3 target;
        public FunctionWithContext3(CodeContext context, string name, CallTargetWithContext3 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (Defaults.Length < 3) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Context, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0) {
            if (Defaults.Length < 2) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(Context, arg0, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, arg0, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            if (Defaults.Length < 1) throw BadArgumentError(2);
            if (!EnforceRecursion) return target(Context, arg0, arg1, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, arg0, arg1, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            if (!EnforceRecursion) return target(Context, arg0, arg1, arg2);
            PushFrame();
            try {
                return target(Context, arg0, arg1, arg2);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                default: throw BadArgumentError(args.Length);
            }
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function4 : PythonFunction {
        private CallTarget4 target;
        public Function4(CodeContext context, string name, CallTarget4 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (Defaults.Length < 4) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0) {
            if (Defaults.Length < 3) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(arg0, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            if (Defaults.Length < 2) throw BadArgumentError(2);
            if (!EnforceRecursion) return target(arg0, arg1, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            if (Defaults.Length < 1) throw BadArgumentError(3);
            if (!EnforceRecursion) return target(arg0, arg1, arg2, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            if (!EnforceRecursion) return target(arg0, arg1, arg2, arg3);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, arg3);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                case 4: return Call(context, args[0], args[1], args[2], args[3]);
                default: throw BadArgumentError(args.Length);
            }
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class FunctionWithContext4 : PythonFunction {
        private CallTargetWithContext4 target;
        public FunctionWithContext4(CodeContext context, string name, CallTargetWithContext4 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (Defaults.Length < 4) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Context, Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0) {
            if (Defaults.Length < 3) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(Context, arg0, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, arg0, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            if (Defaults.Length < 2) throw BadArgumentError(2);
            if (!EnforceRecursion) return target(Context, arg0, arg1, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, arg0, arg1, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            if (Defaults.Length < 1) throw BadArgumentError(3);
            if (!EnforceRecursion) return target(Context, arg0, arg1, arg2, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, arg0, arg1, arg2, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            if (!EnforceRecursion) return target(Context, arg0, arg1, arg2, arg3);
            PushFrame();
            try {
                return target(Context, arg0, arg1, arg2, arg3);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
            switch (args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                case 4: return Call(context, args[0], args[1], args[2], args[3]);
                default: throw BadArgumentError(args.Length);
            }
        }
    }

    [PythonType(typeof(PythonFunction))]
    public class Function5 : PythonFunction {
        private CallTarget5 target;
        public Function5(CodeContext context, string name, CallTarget5 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (Defaults.Length < 5) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Defaults[Defaults.Length - 5], Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Defaults[Defaults.Length - 5], Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0) {
            if (Defaults.Length < 4) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(arg0, Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            if (Defaults.Length < 3) throw BadArgumentError(2);
            if (!EnforceRecursion) return target(arg0, arg1, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            if (Defaults.Length < 2) throw BadArgumentError(3);
            if (!EnforceRecursion) return target(arg0, arg1, arg2, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            if (Defaults.Length < 1) throw BadArgumentError(4);
            if (!EnforceRecursion) return target(arg0, arg1, arg2, arg3, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, arg3, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (!EnforceRecursion) return target(arg0, arg1, arg2, arg3, arg4);
            PushFrame();
            try {
                return target(arg0, arg1, arg2, arg3, arg4);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
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
    }

    [PythonType(typeof(PythonFunction))]
    public class FunctionWithContext5 : PythonFunction {
        private CallTargetWithContext5 target;
        public FunctionWithContext5(CodeContext context, string name, CallTargetWithContext5 target, string[] argNames, object[] defaults)
            : base(context, name, argNames, defaults) {
            this.target = target;
        }
        public override Delegate Target { get { return target; } }
        public override object Call(CodeContext context) {
            if (Defaults.Length < 5) throw BadArgumentError(0);
            if (!EnforceRecursion) return target(Context, Defaults[Defaults.Length - 5], Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, Defaults[Defaults.Length - 5], Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0) {
            if (Defaults.Length < 4) throw BadArgumentError(1);
            if (!EnforceRecursion) return target(Context, arg0, Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, arg0, Defaults[Defaults.Length - 4], Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            if (Defaults.Length < 3) throw BadArgumentError(2);
            if (!EnforceRecursion) return target(Context, arg0, arg1, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, arg0, arg1, Defaults[Defaults.Length - 3], Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            if (Defaults.Length < 2) throw BadArgumentError(3);
            if (!EnforceRecursion) return target(Context, arg0, arg1, arg2, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, arg0, arg1, arg2, Defaults[Defaults.Length - 2], Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            if (Defaults.Length < 1) throw BadArgumentError(4);
            if (!EnforceRecursion) return target(Context, arg0, arg1, arg2, arg3, Defaults[Defaults.Length - 1]);
            PushFrame();
            try {
                return target(Context, arg0, arg1, arg2, arg3, Defaults[Defaults.Length - 1]);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (!EnforceRecursion) return target(Context, arg0, arg1, arg2, arg3, arg4);
            PushFrame();
            try {
                return target(Context, arg0, arg1, arg2, arg3, arg4);
            } finally {
                PopFrame();
            }
        }
        public override object Call(CodeContext context, params object[] args) {
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
    }


    // *** END GENERATED CODE ***

    #endregion


    public partial class FunctionN {
        #region Generated FunctionN FastCallable Members

        // *** BEGIN GENERATED CODE ***

        public override object Call(CodeContext context) {
            return Call(context, new object[] { });
        }

        public override object Call(CodeContext context, object arg0) {
            return Call(context, new object[] { arg0 });
        }

        public override object Call(CodeContext context, object arg0, object arg1) {
            return Call(context, new object[] { arg0, arg1 });
        }

        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            return Call(context, new object[] { arg0, arg1, arg2 });
        }

        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            return Call(context, new object[] { arg0, arg1, arg2, arg3 });
        }

        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return Call(context, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }


        // *** END GENERATED CODE ***

        #endregion
    }


    public partial class Method {
        public override object CallInstance(CodeContext context, object arg0) {
            return Call(context, arg0);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1) {
            return Call(context, arg0, arg1);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2) {
            return Call(context, arg0, arg1, arg2);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            return Call(context, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return Call(context, arg0, arg1, arg2, arg3, arg4);
        }



        #region Generated Method FastCallable Members

        // *** BEGIN GENERATED CODE ***

        public override object Call(CodeContext context) {
            FastCallable fc = _func as FastCallable;
            if (fc != null) {
                if (_inst != null) return fc.CallInstance(context, _inst);
                else return fc.Call(context);
            } else {
                if (_inst != null) return PythonOps.CallWithContext(context, _func, _inst);
                throw BadSelf(null);
            }
        }

        public override object Call(CodeContext context, object arg0) {
            FastCallable fc = _func as FastCallable;
            if (fc != null) {
                if (_inst != null) return fc.CallInstance(context, _inst, arg0);
                else return fc.Call(context, CheckSelf(arg0));
            } else {
                if (_inst != null) return PythonOps.CallWithContext(context, _func, _inst, arg0);
                return PythonOps.CallWithContext(context, _func, CheckSelf(arg0));
            }
        }

        public override object Call(CodeContext context, object arg0, object arg1) {
            FastCallable fc = _func as FastCallable;
            if (fc != null) {
                if (_inst != null) return fc.CallInstance(context, _inst, arg0, arg1);
                else return fc.Call(context, CheckSelf(arg0), arg1);
            } else {
                if (_inst != null) return PythonOps.CallWithContext(context, _func, _inst, arg0, arg1);
                return PythonOps.CallWithContext(context, _func, CheckSelf(arg0), arg1);
            }
        }

        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            FastCallable fc = _func as FastCallable;
            if (fc != null) {
                if (_inst != null) return fc.CallInstance(context, _inst, arg0, arg1, arg2);
                else return fc.Call(context, CheckSelf(arg0), arg1, arg2);
            } else {
                if (_inst != null) return PythonOps.CallWithContext(context, _func, _inst, arg0, arg1, arg2);
                return PythonOps.CallWithContext(context, _func, CheckSelf(arg0), arg1, arg2);
            }
        }

        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            FastCallable fc = _func as FastCallable;
            if (fc != null) {
                if (_inst != null) return fc.CallInstance(context, _inst, arg0, arg1, arg2, arg3);
                else return fc.Call(context, CheckSelf(arg0), arg1, arg2, arg3);
            } else {
                if (_inst != null) return PythonOps.CallWithContext(context, _func, _inst, arg0, arg1, arg2, arg3);
                return PythonOps.CallWithContext(context, _func, CheckSelf(arg0), arg1, arg2, arg3);
            }
        }

        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            FastCallable fc = _func as FastCallable;
            if (fc != null) {
                if (_inst != null) return fc.CallInstance(context, _inst, arg0, arg1, arg2, arg3, arg4);
                else return fc.Call(context, CheckSelf(arg0), arg1, arg2, arg3, arg4);
            } else {
                if (_inst != null) return PythonOps.CallWithContext(context, _func, _inst, arg0, arg1, arg2, arg3, arg4);
                return PythonOps.CallWithContext(context, _func, CheckSelf(arg0), arg1, arg2, arg3, arg4);
            }
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
