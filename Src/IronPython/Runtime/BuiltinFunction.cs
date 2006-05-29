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
using System.Collections.Generic;

using System.Diagnostics;
using System.Runtime.InteropServices;

using IronPython.Compiler;

namespace IronPython.Runtime {
    /// <summary>
    /// BuiltinFunction is the base class for calling all .NET methods.  The built-in function
    /// class hierachy is defined as:
    /// 
    /// BuiltinFunction
    ///     ReflectedMethodBase
    ///         ReflectedMethod
    ///             ReflectedUnboundMethod
    ///             ReflectedUnboundReverseOp
    ///     ReflectedConstructor
    ///     OptimizedFunction0 .. OptimizedFunction(MAX_ARGS)
    ///     OptimizedFunctionN
    ///     OptimizedFunctionAny
    /// 
    /// BuiltinFunction contains functionality that is common to all of these.  That includes
    /// the target methods, information about function/method, a bound instance (if any), and 
    /// the user-displayed function name.
    /// 
    /// The OptimizedFunction* subclasses use delegate based dispatch.  
    /// 
    /// The ReflectedMethod* subclasses use slower reflection based invocation.
    /// 
    /// Currently all classes use reflection based invocation for keyword argument calls.
    /// </summary>
    [PythonType("builtin_function_or_method")]
    public abstract partial class BuiltinFunction : 
        ICallable, IFastCallable, IFancyCallable, IContextAwareMember, IDynamicObject, ICallableWithCallerContext, IMapping {
        internal MethodBase[] targets;
        internal object inst;
        private FunctionType funcType;
        private string name;

        [PythonName("__new__")]
        public static object Make(object cls, PythonFunction newFunction, object inst) {
            Debug.Assert(!(inst is InstanceArgument));
            return new Method(newFunction, inst, null);
        }

        #region Static factories
        /// <summary>
        /// Create a new builtin function.
        /// </summary>
        /// <param name="info">The helper for performing dispatch</param>
        /// <param name="targets">The target methods we will dispatch to</param>
        /// <param name="functionType">Method, Function, Both and optionally PythonMethod</param>
        public static BuiltinFunction Make(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) {
            Debug.Assert(info.IsStatic);

            return MakeBuiltinFunction(name, info, targets, functionType);
        }

        /// <summary>
        /// Creates a new builtin function from a delegate
        /// </summary>
        /// <param name="d">The delegate that handles dynamic dispatch</param>
        /// <param name="targets">The target methods the delegate will handle dispatch for</param>
        public static BuiltinFunction Make(string name, Delegate d, MethodBase [] targets) {
            return Make(name, new MethodInfo[] { d.Method }, targets, FunctionType.PythonVisible | FunctionType.Function);
        }

        /// <summary>
        /// Creates a new builtin function that will handle dispatch for multiple
        /// numbers of effective parameters.
        /// </summary>
        /// <param name="infos">The methods that will be used for handling dispatch</param>
        /// <param name="targets">The final target methods we will dispatch to</param>
        /// <param name="functionType">Method, Function, Both and optionally PythonMethod</param>
        public static BuiltinFunction Make(string name, MethodInfo[] infos, MethodBase [] targets, FunctionType functionType) {
            if (infos.Length == 1) {
                if (infos[0].GetParameters().Length == 0 || !infos[0].GetParameters()[0].ParameterType.HasElementType)
                    return Make(name, infos[0], targets, functionType);
                else
                    return MakeParams(name, infos[0], targets, functionType);
            }

            return new OptimizedFunctionAny(name, infos, targets, functionType);
        }

        private static OptimizedFunctionN MakeParams(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) {
            Debug.Assert(info.IsStatic);

            return new OptimizedFunctionN(name, info, targets, functionType);
        }
        #endregion

        #region Protected Constructors
        protected BuiltinFunction() { }

        protected BuiltinFunction(string functionName, MethodBase[] originalTargets, FunctionType functionType) {
            Debug.Assert(originalTargets!= null, "originalTargets array is null");

            MethodBase target=originalTargets[0];
            Debug.Assert(target != null, "no targets passed to make BuiltinFunction");
            Debug.Assert(functionName != null, String.Format("name is null for {0}",target.Name));

            funcType = functionType;
            targets = originalTargets;
            name = functionName;

            for (int i = 0; i < originalTargets.Length; i++) {
                UpdateFunctionInfo(originalTargets[i]);
            }
        }

        protected BuiltinFunction(BuiltinFunction from) {
            this.targets = from.targets;
            this.name = from.name;
            this.funcType = from.funcType;
        }

        #endregion

        #region Public API Surface
        public string Name {
            [PythonName("__name__")]
            get {
                return name;
            }
            internal set {
                name = value;
            }
        }

        public string FriendlyName {
            get {
                if (Name == "__init__") return (string)DeclaringType.__name__;
                return Name;
            }
        }
        public string FunctionName {
            [PythonName("func_name")]
            get {
                return Name;
            }
        }

        [PythonName("__call__")]        
        public virtual object Call(ICallerContext context, object[] args, string[] names) {
            // we allow kw-arg binding to ctor's of arbitrary CLS types, but
            // NOT Python built-in types.  After the ctor succeeds we'll set the kw args as
            // arbitrary properties on the CLS type.  If this ends up being a built-in type we'll
            // do the check when we're going to set the kw-args.  This accomplishes 2 things:
            //      1. Our error messages match CPython more closely 
            //      2. The attribute lookup is done lazily only if kw-args are supplied to a ctor

            if(IsContextAware) {
                object[] argsWithContext = new object[args.Length + 1];
                argsWithContext[0] = context;
                Array.Copy(args, 0, argsWithContext, 1, args.Length);
                args = argsWithContext;
            }

            KwArgBinder kwArgBinder = new KwArgBinder(args, names, targets[0].IsConstructor);
            MethodBinding bestBinding = new MethodBinding();
            List<UnboundArgument> bestUnboundArgs = null;
            
            for (int i = 0; i < targets.Length; i++) {                
                object[] realArgs = kwArgBinder.DoBind(targets[i], Name);

                if (realArgs != null) {
                    MethodBinding mb = new MethodBinding();
                    mb.method = targets[i];

                    if (!(targets[i].IsStatic || targets[i].IsConstructor)) {
                        if (!HasInstance) {
                            if (realArgs.Length == 0) {
                                throw Ops.TypeError("bad number of arguments for function {0}", targets[0].Name);
                            }
                            mb.instance = realArgs[0];
                            mb.arguments = new object[realArgs.Length - 1];
                            Array.Copy(realArgs, mb.arguments, realArgs.Length - 1);
                        } else {
                            mb.instance = Instance;
                            mb.arguments = realArgs;
                        }
                    } else {
                        mb.arguments = realArgs;
                    }

                    if (!kwArgBinder.AllowUnboundArgs) {
                        // we can have no better bindings!
                        bestBinding = mb;
                        break;
                    }

                    if (bestBinding.method == null ||
                        (kwArgBinder.UnboundArgs == null ||
                        (bestUnboundArgs != null && bestUnboundArgs.Count > kwArgBinder.UnboundArgs.Count))) {
                        bestBinding = mb;
                        bestUnboundArgs = kwArgBinder.UnboundArgs;
                    }

                }
            }

            if (bestBinding.method != null) {
                // we've bound the arguments to a real method,
                // finally we're going to dispatch back to the 
                // optimized version of the calls.
                object ret = Call(bestBinding.arguments);

                // any unbound arguments left over we assume the user
                // wants to do a property set with.  We'll go ahead and try
                // that - if they fail we'll throw.
                if (bestUnboundArgs != null) {
                    ///!!! if we had a constructor w/ a ref param then we'll try
                    // updating the Tuple here instead of the user's object.

                    if (targets[0].DeclaringType.IsDefined(typeof(PythonTypeAttribute), false)) {
                        throw Ops.TypeError("'{0}' is an invalid keyword argument for this function",
                            bestUnboundArgs[0].Name,
                            Name);
                    }

                    for (int j = 0; j < bestUnboundArgs.Count; j++) {
                        Ops.SetAttr(DefaultContext.Default, ret,
                            SymbolTable.StringToId(bestUnboundArgs[j].Name),
                            bestUnboundArgs[j].Value);
                    }
                }

                return ret;
            }

            if (kwArgBinder.GetError() != null) {
                throw kwArgBinder.GetError();
            }

            throw Ops.TypeError("bad number of arguments for function {0}", FriendlyName);
        }

        [PythonName("__str__")]
        public override string ToString() {
            if (inst != null) {
                return string.Format("<built-in method {0} of {1} object at {2}>",
                    Name,
                    Ops.GetDynamicType(inst).__name__,
                    Ops.HexId(inst));
            }
            return string.Format("<built-in function {0}>", Name);
        }

        public string Documentation {
            [PythonName("__doc__")]
            get {
                StringBuilder sb = new StringBuilder();
                MethodBase[] targets = Targets;
                bool needNewLine = false;
                for (int i = 0; i < targets.Length; i++) {
                    if (targets[i] != null) AddDocumentation(sb, ref needNewLine, targets[i]);
                }
                sb.AppendLine();
                return sb.ToString();
            }
        }

        public object Self {
            [PythonName("__self__")]
            get {
                return Instance;
            }
        }

        // Provides (for reflected methods) a mapping from a signature to the exact target
        // which takes this signature.
        // signature with syntax like the following:
        //    someClass.SomeMethod.__overloads__[str, int]("Foo", 123)
        public BuiltinFunctionOverloadMapper Overloads {
            [PythonName("__overloads__")]
            [Documentation(@"__overloads__() -> dictionary of methods indexed by a tuple of types.

Returns all signature overloads of a builtin method.
Eg. The following will call the overload of WriteLine that takes an int argument.
   System.Console.WriteLine.__overloads__()[int](100)")]
            get {
                // The mapping is actually provided by a class rather than a dictionary
                // since it's hard to generate all the keys of the signature mapping when
                // two type systems are involved.  Creating the mapping object is quite
                // cheap so we don't cache a copy.
                return new BuiltinFunctionOverloadMapper(this);
            }
        }

        public virtual BuiltinFunction Clone() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a descriptor for the built-in function if one is
        /// neededed
        /// </summary>
        public object GetDescriptor() {
            if ((FunctionType & FunctionType.Method) != 0) {
                return new BuiltinMethodDescriptor(this);
            }
            return this;
        }

        /// <summary>
        /// Gets the maximum number of arguments the function can handle
        /// </summary>
        /// <returns></returns>
        public virtual int GetMaximumArguments() {
            int maxArgs = 0;
            foreach (MethodBase mi in targets) {
                maxArgs = Math.Max(maxArgs, mi.GetParameters().Length);
            }
            return maxArgs;
        }


        /// <summary>
        /// True if the method should be visible to non-CLS opt-in callers
        /// </summary>
        public bool IsPythonVisible {
            get {
                return (funcType & FunctionType.PythonVisible) != 0;
            }
        }

        public bool IsContextAware {
            get {
                return (FunctionType & FunctionType.IsContextAware) != 0;
            }
        }

        public FunctionType FunctionType {
            get {
                return funcType;
            }
            internal set {
                funcType = value;
            }
        }

        public DynamicType GetDynamicType() {
            return TypeCache.BuiltinFunction;
        }

        public override bool Equals(object obj) {
            BuiltinFunction bf = obj as BuiltinFunction;
            if (bf == null) return false;

            if (bf.targets.Length != targets.Length) return false;

            for (int i = 0; i < bf.targets.Length; i++) {
                if (bf.targets[i] != targets[i]) return false;
            }
            return true;
        }

        public override int GetHashCode() {
            int res = 6551;
            for (int i = 0; i < targets.Length; i++) {
                res ^= targets[i].GetHashCode();
            }
            return res;
        }
        #endregion

        #region Internal API Surface
        internal ReflectedType DeclaringType {
            get {
                MethodBase target = Targets[0];

                if ((FunctionType & FunctionType.OpsFunction) == 0) {
                    // normal method 
                    return (ReflectedType)Ops.GetDynamicTypeFromType(target.DeclaringType);
                } else {
                    // method declared on ops-type, return the declaring type as
                    // the type we add methods on to.

                    Debug.Assert(OpsReflectedType.OpsTypeToType.ContainsKey(target.DeclaringType), String.Format("Type {0} is not an Ops Type ({1})", target.DeclaringType, Name));
                    return OpsReflectedType.OpsTypeToType[target.DeclaringType];
                }
            }
        }

        internal Type ClrDeclaringType {
            get {
                MethodBase target = Targets[0];

                if ((FunctionType & FunctionType.OpsFunction) == 0) {
                    // normal method 
                    return target.DeclaringType;
                } else {
                    Debug.Assert(OpsReflectedType.OpsTypeToType.ContainsKey(target.DeclaringType), String.Format("Type {0} is not an Ops Type ({1})", target.DeclaringType, Name));
                    return OpsReflectedType.OpsTypeToType[target.DeclaringType].type;
                }
            }
        }

        /// <summary>
        /// Gets the target methods that we'll be calling.  
        /// </summary>
        internal MethodBase[] Targets {
            get {
                return targets;
            }
            set {
                targets = value;
            }
        }

        /// <summary>
        /// Gets the optimized functions that are used for calling the 
        /// target methods.
        /// </summary>
        protected virtual Delegate[] OptimizedTargets {
            get {
                return new Delegate[0];
            }
        }

        /// <summary>
        /// Returns a new BuiltinFunction that includes the 
        /// optimized method that was requested to be added.
        /// </summary>
        /// <param name="adding"></param>
        internal BuiltinFunction ReOptimize(MethodInfo adding) {
            MethodInfo [] infos = new MethodInfo[targets.Length + 1];
            infos[0] = adding;
            Array.Copy(targets, 0, infos, 1, targets.Length);
            targets = infos;
            BuiltinFunction res = Compiler.ReflectOptimizer.MakeFunction(this);
            if (res != null) {
                res.FunctionType = FunctionType | FunctionType.SkipThisCheck;
                
                return res;
            }

            // we now include our base type, we can't survie a type check.
            FunctionType |= FunctionType.SkipThisCheck;
            return null;
        }

        #endregion

        #region IContextAwareMember Members

        public bool IsVisible(ICallerContext context) {
            return IsPythonVisible || ((context.ContextFlags & CallerContextFlags.ShowCls) != 0);
        }

        #endregion

        #region IContainer members

        public int GetLength() {
            return Targets.Length;
        }

        public bool ContainsValue(object value) {
            throw new NotImplementedException();
        }

        #endregion 

        #region IMapping members

        public object GetValue(object key) {
            throw new NotImplementedException();
        }

        public object GetValue(object key, object defaultValue) {
            throw new NotImplementedException();
        }

        public bool TryGetValue(object key, out object value) {
            throw new NotImplementedException();
        }

        // Use indexing on generic methods to provide a new reflected method with targets bound with
        // the supplied type arguments.
        public object this[object key] {
            get {
                // Retrieve the list of type arguments from the index.
                Type[] types;
                Tuple typesTuple = key as Tuple;

                if (typesTuple != null) {
                    types = new Type[typesTuple.Count];
                    for (int i = 0; i < types.Length; i++) {
                        types[i] = Converter.ConvertToType(typesTuple[i]);
                    }
                } else {
                    types = new Type[] { Converter.ConvertToType(key) };
                }

                // Start building a new ReflectedMethod that will contain targets with bound type
                // arguments.
                ReflectedMethod rm = new ReflectedMethod();

                // Search for generic targets with the correct arity (number of type parameters).
                // Compatible targets must be MethodInfos by definition (constructors never take
                // type arguments).
                int arity = types.Length;
                foreach (MethodBase mb in targets) {
                    MethodInfo mi = mb as MethodInfo;
                    if (mi == null)
                        continue;
                    if (mi.ContainsGenericParameters && mi.GetGenericArguments().Length == arity)
                        rm.AddMethod(mi.MakeGenericMethod(types));
                }
                if (rm.Targets == null)
                    throw Ops.TypeError(string.Format("bad type args to this generic method {0}", this));

                rm.Name = Name;
                rm.FunctionType = FunctionType|FunctionType.OptimizeChecked;    // don't want to optimize & whack our dictionary.
                rm.inst = inst;

                return rm;
            }
            set {
                throw new NotImplementedException();
            }
        }

        [PythonName("__delitem__")]
        public void DeleteItem(object key) {
            throw new NotImplementedException();
        }

        #endregion

        #region Protected APIs

        protected virtual bool HasInstance {
            get {
                return inst != null;
            }
        }

        protected virtual object Instance {
            get {
                if (inst != null && (funcType & FunctionType.FunctionMethodMask) == FunctionType.FunctionMethodMask) {                    
                    return new InstanceArgument(inst);
                }
                return inst;
            }
        }

        protected void UpdateFunctionInfo(MethodBase info) {
            ParameterInfo[] parameters = info.GetParameters();
            if (parameters.Length > 0 && parameters[0].ParameterType == typeof(ICallerContext)) {
                FunctionType |= FunctionType.IsContextAware;
            }
        }

        protected Exception BadArgumentError(int count) {
            int min = Int32.MaxValue;
            int max = Int32.MinValue;
            for (int i = 0; i < targets.Length; i++) {
                ParameterInfo[] pis = targets[i].GetParameters();

                if (min > pis.Length) min = pis.Length;
                if (max < pis.Length) max = pis.Length;

                for (int j = 0; j < pis.Length; j++) {
                    if (ReflectionUtil.IsParamArray(pis[j])) {
                        max = Int32.MaxValue;
                        if (min == pis.Length) min--;
                    } else if (ReflectionUtil.IsParamDict(pis[j])) {
                        max = Int32.MaxValue;
                        if (min == pis.Length) min--;
                    }
                }
            }

            if (min == max) throw PythonFunction.TypeErrorForIncorrectArgumentCount(FriendlyName, min, 0, count);
            else if (max == Int32.MaxValue) throw PythonFunction.TypeErrorForIncorrectArgumentCount(FriendlyName, min, 0, count, true, false);
            else throw PythonFunction.TypeErrorForIncorrectArgumentCount(FriendlyName, max, max - min, count);
        }

        /// <summary>
        /// Performs late-bound invocation - used by ReflectedMethod's and
        /// for keyword argument calls.
        /// </summary>
        protected static object Invoke(MethodBinding binding) {
            object result;
            try {
                if (binding.method is ConstructorInfo) {
                    result = ((ConstructorInfo)binding.method).Invoke(binding.arguments);
                } else {
                    result = binding.method.Invoke(binding.instance, binding.arguments);
                }
            } catch (TargetInvocationException tie) {
                throw ExceptionConverter.UpdateForRethrow(tie.InnerException);
            }

            MethodBase info = binding.method;
            ParameterInfo[] parameters = info.GetParameters();
            int results = 0;
            for (int parm = 0; parm < parameters.Length; parm++) {
                if (parameters[parm].ParameterType.IsByRef) results++;
            }
            if (results == 0) {
                return Ops.ToPython(CompilerHelpers.GetReturnType(info), result);
            }

            object[] retValues;
            int retValueIndex = 0;
            if (info is MethodInfo && ((MethodInfo)info).ReturnType != typeof(void)) {
                retValues = new object[results + 1];
                retValues[retValueIndex++] = Ops.ToPython(CompilerHelpers.GetReturnType(info), result);
            } else {
                retValues = new object[results];
            }

            for (int parm = 0; parm < parameters.Length; parm++) {
                Type parmType = parameters[parm].ParameterType;
                if (parmType.IsByRef) {
                    retValues[retValueIndex++] = Ops.ToPython(parmType, binding.arguments[parm]);
                }
            }
            if (retValues.Length == 1) {
                return retValues[0];
            } else {
                return Tuple.MakeTuple(retValues);
            }
        }

        protected struct MethodBinding {
            [Flags]
            public enum MethodBindingSettings {
                ThisCall = 0x1,
                ArgIntoThis = 0x2,
                ParamArray = 0x4
            }
            public MethodBase method;
            public MethodTracker tracker;
            public object[] arguments;
            public object instance;
            public Conversion[] conversions;
            public Conversion instConversion;
            public MethodBindingSettings flags;


            public bool ThisCallWithoutThis {
                get {
                    return (flags & MethodBindingSettings.ArgIntoThis) != 0;
                }
            }
        };

        #endregion

        #region Private APIs

        private static void AddDocumentation(StringBuilder sb, ref bool nl, MethodBase mb) {
            if (nl) {
                sb.Append(System.Environment.NewLine);
            }
            sb.Append(ReflectionUtil.DocOneInfo(mb));
            nl = true;
        }

        #endregion
    }

    [Flags]
    public enum FunctionType {
        None                = 0x0000,   // No flags have been set
        Function            = 0x0001,   // This is a function w/ no instance pointer
        Method              = 0x0002,   // This is a method that requires an instance
        FunctionMethodMask  = 0x0003,   // Built-in functions can encapsulate both methods & functions, in which case both bits are set
        PythonVisible       = 0x0004,   // True is the function/method should be visible from pure-Python code
        IsContextAware      = 0x0008,   // True if the function can receive the ICallerContext parameter.
        SkipThisCheck       = 0x0010,   // we should skip the type check for the this pointer (due to base type, or an InstanceOps method).
        OptimizeChecked     = 0x0020,   // True if we've checked if we could optimize the function, and we can't.
        OpsFunction         = 0x0040,   // True if this is a function/method declared on an Ops type (StringOps, IntOps, etc...)
        Params              = 0x0080,   // True if this is a params method, false otherwise.
    }

    /// <summary>
    /// Wrapper class used when calling mixed built-in functions w/ an
    /// instance value.  The instance value is wrapped as an InstanceArgument
    /// which the generated built-in function detects and unwraps.
    /// </summary>
    public sealed class InstanceArgument {
        public readonly object ArgumentValue;

        public InstanceArgument(object argumentValue) {
            Debug.Assert(!(argumentValue is InstanceArgument), "InstanceArgument double wrapped");
            ArgumentValue = argumentValue;
        }
    }

    [PythonType("method_descriptor")]
    public sealed class BuiltinMethodDescriptor : IDescriptor, IFancyCallable, ICallable, IContextAwareMember, ICallableWithCallerContext {
        internal BuiltinFunction template;

        public BuiltinMethodDescriptor(BuiltinFunction function) {
            template = function;
        }

        #region IDescriptor Members

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (instance != null) {
                CheckSelf(instance);

                BuiltinFunction res = template.Clone() as BuiltinFunction;
                res.inst = instance;
                return res;
            }
            return this;
        }

        #endregion

        public ReflectedType DeclaringType {
            [PythonName("__objclass__")]
            get {
                return template.DeclaringType;
            }
        }

        [PythonName("__eq__")]
        public override bool Equals(object obj) {
            BuiltinMethodDescriptor bmf = obj as BuiltinMethodDescriptor;
            if (bmf == null) return false;
            return template.Equals(bmf.template);
        }

        [PythonName("__hash__")]
        public override int GetHashCode() {
            return template.GetHashCode();
        }

        [PythonName("__str__")]
        public override string ToString() {
            DynamicType dt = DeclaringType;

            if (dt != null) {
                return String.Format("<method {0} of {1} objects>", Ops.Repr(Name), Ops.Repr(dt.__name__));
            } else {
                return String.Format("<method {0} of {1} objects>", Ops.Repr(Name), Ops.Repr("<unknown>"));
            }
        }

        public string Name {
            [PythonName("__name__")]
            get {
                return template.Name;
            }
        }

        public object Documentation {
            [PythonName("__doc__")]
            get {
                return template.Documentation;
            }
        }

        #region ICallable Members

        /// <summary>
        /// Un-bound method call, first parameter should be instance
        /// </summary>
        [PythonName("__call__")]
        public object Call(params object[] args) {
            if (args.Length == 0)
                throw Ops.TypeError("descriptor {0} of {1} needs an argument",
                    Ops.Repr(Name),
                    Ops.Repr(DeclaringType.__name__));

            CheckSelf(args[0]);

            return template.Call(args);
        }

        #endregion

        #region IFancyCallable Members

        public object Call(ICallerContext context, object[] args, string[] names) {
            if (args.Length == 0)
                throw Ops.TypeError("descriptor {0} of {1} needs an argument",
                    Ops.Repr(Name),
                    Ops.Repr(DeclaringType.__name__));

            CheckSelf(args[0]);

            return template.Call(context, args, names);
        }

        #endregion

        #region ICallableWithCallerContext Members

        [PythonName("__call__")]
        public object Call(ICallerContext context, object[] args) {
            if (args.Length == 0)
                throw Ops.TypeError("descriptor {0} of {1} needs an argument",
                    Ops.Repr(Name),
                    Ops.Repr(DeclaringType.__name__));

            CheckSelf(args[0]);

            return template.Call(context, args);
        }

        #endregion

        private void CheckSelf(object self) {
            // if the type has been re-optimized (we also have base type info in here) 
            // then we can't do the type checks right now :(.
            if ((template.FunctionType & FunctionType.SkipThisCheck) != 0) 
                return;

            if ((template.FunctionType & FunctionType.FunctionMethodMask) == FunctionType.Method) {
                // to a fast check on the CLR types, if they match we can avoid the slower
                // check that involves looking up dynamic types.
                if (self.GetType() == template.ClrDeclaringType) return;

                PythonType selfType = Ops.GetDynamicTypeFromType(self.GetType()) as PythonType;
                Debug.Assert(selfType != null);

                ReflectedType declType = DeclaringType;
                if (!selfType.IsSubclassOf(declType)) {
                    // if a conversion exists to the type allow the call.
                    Conversion conv;
                    if (Converter.TryConvert(self, declType.type, out conv) == null) {
                        throw Ops.TypeError("descriptor {0} requires a {1} object but received a {2}",
                            Ops.Repr(Name),
                            Ops.Repr(DeclaringType.__name__),
                            Ops.Repr(Ops.GetDynamicType(self).__name__));
                    }
                }
            }
        }

        #region IContextAwareMember Members

        public bool IsVisible(ICallerContext context) {
            return template.IsVisible(context);
        }

        #endregion        
    
    }

}