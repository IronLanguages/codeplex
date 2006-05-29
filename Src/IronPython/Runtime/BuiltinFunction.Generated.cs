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

namespace IronPython.Runtime {
    public abstract partial class BuiltinFunction {
        #region Generated BuiltinFunction targets

        // *** BEGIN GENERATED CODE ***

        public virtual object Call() { throw BadArgumentError(0); }
        public virtual object Call(object arg0) { throw BadArgumentError(1); }
        public virtual object Call(object arg0, object arg1) { throw BadArgumentError(2); }
        public virtual object Call(object arg0, object arg1, object arg2) { throw BadArgumentError(3); }
        public virtual object Call(object arg0, object arg1, object arg2, object arg3) { throw BadArgumentError(4); }
        public virtual object Call(object arg0, object arg1, object arg2, object arg3, object arg4) { throw BadArgumentError(5); }
        public virtual object Call(params object[] args) {
            switch(args.Length) {
                case 0: return Call();
                case 1: return Call(args[0]);
                case 2: return Call(args[0], args[1]);
                case 3: return Call(args[0], args[1], args[2]);
                case 4: return Call(args[0], args[1], args[2], args[3]);
                case 5: return Call(args[0], args[1], args[2], args[3], args[4]);
            }
            throw BadArgumentError(args.Length);
        }
        private static BuiltinFunction MakeBuiltinFunction(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) {
            switch (info.GetParameters().Length) {
                case 0: return new OptimizedFunction0(name, info, targets, functionType);
                case 1: return new OptimizedFunction1(name, info, targets, functionType);
                case 2: return new OptimizedFunction2(name, info, targets, functionType);
                case 3: return new OptimizedFunction3(name, info, targets, functionType);
                case 4: return new OptimizedFunction4(name, info, targets, functionType);
                case 5: return new OptimizedFunction5(name, info, targets, functionType);
            }
            throw new InvalidOperationException("too many args");
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated BuiltinFunction context targets

        // *** BEGIN GENERATED CODE ***

        public virtual object Call(ICallerContext context) { throw BadArgumentError(0); }
        public virtual object Call(ICallerContext context, object arg0) { throw BadArgumentError(1); }
        public virtual object Call(ICallerContext context, object arg0, object arg1) { throw BadArgumentError(2); }
        public virtual object Call(ICallerContext context, object arg0, object arg1, object arg2) { throw BadArgumentError(3); }
        public virtual object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) { throw BadArgumentError(4); }
        public virtual object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) { throw BadArgumentError(5); }
        public virtual object Call(ICallerContext context, params object[] args) {
            switch(args.Length) {
                case 0: return Call(context);
                case 1: return Call(context, args[0]);
                case 2: return Call(context, args[0], args[1]);
                case 3: return Call(context, args[0], args[1], args[2]);
                case 4: return Call(context, args[0], args[1], args[2], args[3]);
                case 5: return Call(context, args[0], args[1], args[2], args[3], args[4]);
            }
            throw BadArgumentError(args.Length);
        }

        // *** END GENERATED CODE ***

        #endregion
    }

    #region Generated Builtin Functions

    // *** BEGIN GENERATED CODE ***


    [PythonType("builtin_function_or_method")]
    public class OptimizedFunction0 : BuiltinFunction {
        CallTarget0 target = null;

        public OptimizedFunction0(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) : base(name, targets, functionType) {
            target = IronPython.Compiler.CodeGen.CreateDelegate(info, typeof(CallTarget0)) as CallTarget0;
        }
        public OptimizedFunction0(OptimizedFunction0 from) : base(from) {
            target = from.target;
        }
        public override object Call() {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(0);
            return target();
        }
        public override object Call(ICallerContext context) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(0);
            return target();
        }
        protected override Delegate[] OptimizedTargets {
            get {
                return new Delegate[] { target };
            }
        }
        public override int GetMaximumArguments() {
            return 0;
        }
        public override BuiltinFunction Clone() {
            return new OptimizedFunction0(this);
        }
    }

    [PythonType("builtin_function_or_method")]
    public class OptimizedFunction1 : BuiltinFunction {
        CallTarget1 target = null;

        public OptimizedFunction1(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) : base(name, targets, functionType) {
            target = IronPython.Compiler.CodeGen.CreateDelegate(info, typeof(CallTarget1)) as CallTarget1;
        }
        public OptimizedFunction1(OptimizedFunction1 from) : base(from) {
            target = from.target;
        }
        public override object Call(object arg0) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(1);
            return target(arg0);
        }
        public override object Call() {
            if(HasInstance)
                return target(Instance);
            else
                throw BadArgumentError(0);
        }
        public override object Call(ICallerContext context, object arg0) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(1);
            return target(arg0);
        }
        public override object Call(ICallerContext context) {
            if(HasInstance) return target(Instance);
            if(IsContextAware) return target((object)context);

            throw BadArgumentError(0);
        }
        protected override Delegate[] OptimizedTargets {
            get {
                return new Delegate[] { target };
            }
        }
        public override int GetMaximumArguments() {
            return 1;
        }
        public override BuiltinFunction Clone() {
            return new OptimizedFunction1(this);
        }
    }

    [PythonType("builtin_function_or_method")]
    public class OptimizedFunction2 : BuiltinFunction {
        CallTarget2 target = null;

        public OptimizedFunction2(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) : base(name, targets, functionType) {
            target = IronPython.Compiler.CodeGen.CreateDelegate(info, typeof(CallTarget2)) as CallTarget2;
        }
        public OptimizedFunction2(OptimizedFunction2 from) : base(from) {
            target = from.target;
        }
        public override object Call(object arg0, object arg1) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(2);
            return target(arg0, arg1);
        }
        public override object Call(object arg0) {
            if(HasInstance)
                return target(Instance, arg0);
            else
                throw BadArgumentError(1);
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(2);
            return target(arg0, arg1);
        }
        public override object Call(ICallerContext context, object arg0) {
            if(HasInstance) return target(Instance, arg0);
            if(IsContextAware) return target(context, arg0);

            throw BadArgumentError(1);
        }
        public override object Call(ICallerContext context) {
            if(!HasInstance || !IsContextAware) throw BadArgumentError(0);
            return target(context, Instance);
        }
        protected override Delegate[] OptimizedTargets {
            get {
                return new Delegate[] { target };
            }
        }
        public override int GetMaximumArguments() {
            return 2;
        }
        public override BuiltinFunction Clone() {
            return new OptimizedFunction2(this);
        }
    }

    [PythonType("builtin_function_or_method")]
    public class OptimizedFunction3 : BuiltinFunction {
        CallTarget3 target = null;

        public OptimizedFunction3(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) : base(name, targets, functionType) {
            target = IronPython.Compiler.CodeGen.CreateDelegate(info, typeof(CallTarget3)) as CallTarget3;
        }
        public OptimizedFunction3(OptimizedFunction3 from) : base(from) {
            target = from.target;
        }
        public override object Call(object arg0, object arg1, object arg2) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(3);
            return target(arg0, arg1, arg2);
        }
        public override object Call(object arg0, object arg1) {
            if(HasInstance)
                return target(Instance, arg0, arg1);
            else
                throw BadArgumentError(2);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(3);
            return target(arg0, arg1, arg2);
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if(HasInstance) return target(Instance, arg0, arg1);
            if(IsContextAware) return target(context, arg0, arg1);

            throw BadArgumentError(2);
        }
        public override object Call(ICallerContext context, object arg0) {
            if(!HasInstance || !IsContextAware) throw BadArgumentError(1);
            return target(context, Instance, arg0);
        }
        protected override Delegate[] OptimizedTargets {
            get {
                return new Delegate[] { target };
            }
        }
        public override int GetMaximumArguments() {
            return 3;
        }
        public override BuiltinFunction Clone() {
            return new OptimizedFunction3(this);
        }
    }

    [PythonType("builtin_function_or_method")]
    public class OptimizedFunction4 : BuiltinFunction {
        CallTarget4 target = null;

        public OptimizedFunction4(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) : base(name, targets, functionType) {
            target = IronPython.Compiler.CodeGen.CreateDelegate(info, typeof(CallTarget4)) as CallTarget4;
        }
        public OptimizedFunction4(OptimizedFunction4 from) : base(from) {
            target = from.target;
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(4);
            return target(arg0, arg1, arg2, arg3);
        }
        public override object Call(object arg0, object arg1, object arg2) {
            if(HasInstance)
                return target(Instance, arg0, arg1, arg2);
            else
                throw BadArgumentError(3);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(4);
            return target(arg0, arg1, arg2, arg3);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if(HasInstance) return target(Instance, arg0, arg1, arg2);
            if(IsContextAware) return target(context, arg0, arg1, arg2);

            throw BadArgumentError(3);
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if(!HasInstance || !IsContextAware) throw BadArgumentError(2);
            return target(context, Instance, arg0, arg1);
        }
        protected override Delegate[] OptimizedTargets {
            get {
                return new Delegate[] { target };
            }
        }
        public override int GetMaximumArguments() {
            return 4;
        }
        public override BuiltinFunction Clone() {
            return new OptimizedFunction4(this);
        }
    }

    [PythonType("builtin_function_or_method")]
    public class OptimizedFunction5 : BuiltinFunction {
        CallTarget5 target = null;

        public OptimizedFunction5(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) : base(name, targets, functionType) {
            target = IronPython.Compiler.CodeGen.CreateDelegate(info, typeof(CallTarget5)) as CallTarget5;
        }
        public OptimizedFunction5(OptimizedFunction5 from) : base(from) {
            target = from.target;
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3, object arg4) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(5);
            return target(arg0, arg1, arg2, arg3, arg4);
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3) {
            if(HasInstance)
                return target(Instance, arg0, arg1, arg2, arg3);
            else
                throw BadArgumentError(4);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(5);
            return target(arg0, arg1, arg2, arg3, arg4);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if(HasInstance) return target(Instance, arg0, arg1, arg2, arg3);
            if(IsContextAware) return target(context, arg0, arg1, arg2, arg3);

            throw BadArgumentError(4);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if(!HasInstance || !IsContextAware) throw BadArgumentError(3);
            return target(context, Instance, arg0, arg1, arg2);
        }
        protected override Delegate[] OptimizedTargets {
            get {
                return new Delegate[] { target };
            }
        }
        public override int GetMaximumArguments() {
            return 5;
        }
        public override BuiltinFunction Clone() {
            return new OptimizedFunction5(this);
        }
    }

    [PythonType("builtin_function_or_method")]
    public class OptimizedFunctionN : BuiltinFunction {
        CallTargetN target = null;

        public OptimizedFunctionN(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) : base(name, targets, functionType) {
            target = IronPython.Compiler.CodeGen.CreateDelegate(info, typeof(CallTargetN)) as CallTargetN;
        }
        public OptimizedFunctionN(OptimizedFunctionN from) : base(from) {
            target = from.target;
        }
        public override object Call() {
            if(HasInstance) return target(new object[]{Instance, });
            return target( new object[]{});
        }

        public override object Call(ICallerContext context) {
            if(HasInstance) return target(new object[]{Instance, });
            return target( new object[]{});
        }
        public override object Call(object arg0) {
            if(HasInstance) return target(new object[]{Instance, arg0});
            return target( new object[]{arg0});
        }

        public override object Call(ICallerContext context, object arg0) {
            if(HasInstance) return target(new object[]{Instance, arg0});
            return target( new object[]{arg0});
        }
        public override object Call(object arg0, object arg1) {
            if(HasInstance) return target(new object[]{Instance, arg0, arg1});
            return target( new object[]{arg0, arg1});
        }

        public override object Call(ICallerContext context, object arg0, object arg1) {
            if(HasInstance) return target(new object[]{Instance, arg0, arg1});
            return target( new object[]{arg0, arg1});
        }
        public override object Call(object arg0, object arg1, object arg2) {
            if(HasInstance) return target(new object[]{Instance, arg0, arg1, arg2});
            return target( new object[]{arg0, arg1, arg2});
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if(HasInstance) return target(new object[]{Instance, arg0, arg1, arg2});
            return target( new object[]{arg0, arg1, arg2});
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3) {
            if(HasInstance) return target(new object[]{Instance, arg0, arg1, arg2, arg3});
            return target( new object[]{arg0, arg1, arg2, arg3});
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if(HasInstance) return target(new object[]{Instance, arg0, arg1, arg2, arg3});
            return target( new object[]{arg0, arg1, arg2, arg3});
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3, object arg4) {
            if(HasInstance) return target(new object[]{Instance, arg0, arg1, arg2, arg3, arg4});
            return target( new object[]{arg0, arg1, arg2, arg3, arg4});
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if(HasInstance) return target(new object[]{Instance, arg0, arg1, arg2, arg3, arg4});
            return target( new object[]{arg0, arg1, arg2, arg3, arg4});
        }

        public override object Call(params object[] args) {
            if(HasInstance) {
                object [] newArgs = new object[args.Length+1];
                newArgs[0] = Instance;
                Array.Copy(args, 0, newArgs, 1, args.Length);
                return target(newArgs);
            }
            return target(args);
        }
        public override object Call(ICallerContext context, params object[] args) {
            if (!IsContextAware) return Call(args);

            if (Instance == null) {
                object[] newArgs = new object[args.Length + 1];
                newArgs[0] = context;
                Array.Copy(args, 0, newArgs, 1, args.Length);
                return Call(newArgs);
            } else {
                object [] newArgs = new object[args.Length+2];
                newArgs[0] = context;
                newArgs[1] = Instance;
                Array.Copy(args, 0, newArgs, 2, args.Length);
                return target(newArgs);
            }
        }

        protected override Delegate[] OptimizedTargets {
            get {
                return new Delegate[] { target };
            }
        }
        public override int GetMaximumArguments() {
            return int.MaxValue;
        }
        public override BuiltinFunction Clone() {
            return new OptimizedFunctionN(this);
        }
    }
    [PythonType("builtin_function_or_method")]
    public class OptimizedFunctionAny : BuiltinFunction {
        CallTargetN targetN;
        CallTarget0 target0;
        CallTarget1 target1;
        CallTarget2 target2;
        CallTarget3 target3;
        CallTarget4 target4;
        CallTarget5 target5;
        public OptimizedFunctionAny(string name, MethodInfo[] infos, MethodBase[] targets, FunctionType functionType) : base(name, targets, functionType) {
            for (int i = 0; i < infos.Length; i++) {
                Debug.Assert(infos[i].IsStatic);

                switch(infos[i].GetParameters().Length) {
                    case 0: target0 = IronPython.Compiler.CodeGen.CreateDelegate(infos[i], typeof(CallTarget0)) as CallTarget0; break;
                    case 1:
                    if (!infos[i].GetParameters()[0].ParameterType.HasElementType) 
                        target1 = IronPython.Compiler.CodeGen.CreateDelegate(infos[i], typeof(CallTarget1)) as CallTarget1;
                    else
                        targetN = IronPython.Compiler.CodeGen.CreateDelegate(infos[i], typeof(CallTargetN)) as CallTargetN;
                    break;
                    case 2: target2 = IronPython.Compiler.CodeGen.CreateDelegate(infos[i], typeof(CallTarget2)) as CallTarget2; break;
                    case 3: target3 = IronPython.Compiler.CodeGen.CreateDelegate(infos[i], typeof(CallTarget3)) as CallTarget3; break;
                    case 4: target4 = IronPython.Compiler.CodeGen.CreateDelegate(infos[i], typeof(CallTarget4)) as CallTarget4; break;
                    case 5: target5 = IronPython.Compiler.CodeGen.CreateDelegate(infos[i], typeof(CallTarget5)) as CallTarget5; break;
                }
            }
        }
        public OptimizedFunctionAny(OptimizedFunctionAny from) : base(from) {
            target0 = from.target0;
            target1 = from.target1;
            target2 = from.target2;
            target3 = from.target3;
            target4 = from.target4;
            target5 = from.target5;
            targetN = from.targetN;
        }
        public override object Call() {
            if(HasInstance) {
                if(target1 != null) return target1(Instance);
                if (targetN != null) return targetN(new object[] {Instance,  });
                throw BadArgumentError(0);
            }
            if (target0 != null) return target0();
            else if (targetN != null) return targetN(new object[] {  });
            else throw BadArgumentError(0);
        }
        public override object Call(object arg0) {
            if(HasInstance) {
                if(target2 != null) return target2(Instance, arg0);
                if (targetN != null) return targetN(new object[] {Instance, arg0 });
                throw BadArgumentError(1);
            }
            if (target1 != null) return target1(arg0);
            else if (targetN != null) return targetN(new object[] { arg0 });
            else throw BadArgumentError(1);
        }
        public override object Call(object arg0, object arg1) {
            if(HasInstance) {
                if(target3 != null) return target3(Instance, arg0, arg1);
                if (targetN != null) return targetN(new object[] {Instance, arg0, arg1 });
                throw BadArgumentError(2);
            }
            if (target2 != null) return target2(arg0, arg1);
            else if (targetN != null) return targetN(new object[] { arg0, arg1 });
            else throw BadArgumentError(2);
        }
        public override object Call(object arg0, object arg1, object arg2) {
            if(HasInstance) {
                if(target4 != null) return target4(Instance, arg0, arg1, arg2);
                if (targetN != null) return targetN(new object[] {Instance, arg0, arg1, arg2 });
                throw BadArgumentError(3);
            }
            if (target3 != null) return target3(arg0, arg1, arg2);
            else if (targetN != null) return targetN(new object[] { arg0, arg1, arg2 });
            else throw BadArgumentError(3);
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3) {
            if(HasInstance) {
                if(target5 != null) return target5(Instance, arg0, arg1, arg2, arg3);
                if (targetN != null) return targetN(new object[] {Instance, arg0, arg1, arg2, arg3 });
                throw BadArgumentError(4);
            }
            if (target4 != null) return target4(arg0, arg1, arg2, arg3);
            else if (targetN != null) return targetN(new object[] { arg0, arg1, arg2, arg3 });
            else throw BadArgumentError(4);
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3, object arg4) {
            if(HasInstance) {
                if(targetN != null) return targetN(new object[]{ Instance, arg0, arg1, arg2, arg3, arg4});
                if (targetN != null) return targetN(new object[] {Instance, arg0, arg1, arg2, arg3, arg4 });
                throw BadArgumentError(5);
            }
            if (target5 != null) return target5(arg0, arg1, arg2, arg3, arg4);
            else if (targetN != null) return targetN(new object[] { arg0, arg1, arg2, arg3, arg4 });
            else throw BadArgumentError(5);
        }
        public override object Call(params object[] args) {
            if(Instance == null) {
                switch (args.Length) {
                    case 0: return Call();
                    case 1: return Call(args[0]);
                    case 2: return Call(args[0],args[1]);
                    case 3: return Call(args[0],args[1],args[2]);
                    case 4: return Call(args[0],args[1],args[2],args[3]);
                    case 5: return Call(args[0],args[1],args[2],args[3],args[4]);
                }
                if (targetN != null) return targetN(args);
            } else {
                switch (args.Length) {
                    case 0: if(target1 != null) return target1(Instance); break;
                    case 1: if(target2 != null) return target2(Instance, args[0]); break;
                    case 2: if(target3 != null) return target3(Instance, args[0],args[1]); break;
                    case 3: if(target4 != null) return target4(Instance, args[0],args[1],args[2]); break;
                    case 4: if(target5 != null) return target5(Instance, args[0],args[1],args[2],args[3]); break;
                    case 5: if(targetN != null) return targetN(new object[]{ Instance, args[0],args[1],args[2],args[3],args[4]}); break;
                }
                if (targetN != null) {
                    object [] newArgs = new object[args.Length+1];
                    newArgs[0] = Instance;
                    Array.Copy(args, 0, newArgs, 1, args.Length);
                    return targetN(newArgs);
                }
            }
            throw BadArgumentError(args.Length);
        }
        public override object Call(ICallerContext context) {
            if(!IsContextAware) return Call();
            if(HasInstance) {
                if(target2 != null) return target2(context, Instance);
                else if(targetN != null) return targetN(new object[]{context, Instance});
                throw BadArgumentError(0);
            }
            return Call((object)context);
        }

        public override object Call(ICallerContext context, object arg0) {
            if(!IsContextAware) return Call(arg0);
            if(!HasInstance) return Call((object)context, arg0);
            if(target3 != null) return target3(context, Instance, arg0);
            if (targetN != null) return targetN(new object[] {context, Instance, arg0 });
            throw BadArgumentError(0);
        }

        public override object Call(ICallerContext context, object arg0, object arg1) {
            if(!IsContextAware) return Call(arg0, arg1);
            if(!HasInstance) return Call((object)context, arg0, arg1);
            if(target4 != null) return target4(context, Instance, arg0, arg1);
            if (targetN != null) return targetN(new object[] {context, Instance, arg0, arg1 });
            throw BadArgumentError(1);
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if(!IsContextAware) return Call(arg0, arg1, arg2);
            if(!HasInstance) return Call((object)context, arg0, arg1, arg2);
            if (targetN != null) return targetN(new object[] {context, Instance, arg0, arg1, arg2 });
            throw BadArgumentError(2);
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if(!IsContextAware) return Call(arg0, arg1, arg2, arg3);
            if(!HasInstance) return Call((object)context, arg0, arg1, arg2, arg3);
            if (targetN != null) return targetN(new object[] {context, Instance, arg0, arg1, arg2, arg3 });
            throw BadArgumentError(3);
        }

        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if(IsContextAware) return Call(new object[]{context, arg0, arg1, arg2, arg3, arg4});
            return Call(arg0, arg1, arg2, arg3, arg4);
        }

        public override object Call(ICallerContext context, params object[] args) {
            if (!IsContextAware) return Call(args);

            if (Instance == null) {
                object[] newArgs = new object[args.Length + 1];
                newArgs[0] = context;
                Array.Copy(args, 0, newArgs, 1, args.Length);
                return Call(newArgs);
            } else {
                switch (args.Length) {
                    case 0: if(target2 != null) return target2(context, Instance); break;
                    case 1: if(target3 != null) return target3(context, Instance, args[0]); break;
                    case 2: if(target4 != null) return target4(context, Instance, args[0],args[1]); break;
                    case 3: if(target5 != null) return target5(context, Instance, args[0],args[1],args[2]); break;
                }
                if (targetN != null) {
                    object [] newArgs = new object[args.Length+2];
                    newArgs[0] = context;
                    newArgs[1] = Instance;
                    Array.Copy(args, 0, newArgs, 2, args.Length);
                    return targetN(newArgs);
                }
                throw BadArgumentError(args.Length);
            }
        }

        protected override Delegate[] OptimizedTargets {
            get {
                Delegate[] delegates = new Delegate[] {
                    target0,
                    target1,
                    target2,
                    target3,
                    target4,
                    target5,
                    targetN
                }
                ;
                return delegates;
            }
        }
        public override int GetMaximumArguments() {
            if (targetN != null) return int.MaxValue;
            if (target5 != null) return 5;
            if (target4 != null) return 4;
            if (target3 != null) return 3;
            if (target2 != null) return 2;
            if (target1 != null) return 1;
            return 0;
        }
        public override BuiltinFunction Clone() {
            return new OptimizedFunctionAny(this);
        }
    }

    // *** END GENERATED CODE ***

    #endregion
}
