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
using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Calls {
    /// <summary>
    /// BuiltinFunction represents any standard CLR function exposed to Python.
    /// This is used for both methods on standard Python types such as list or tuple
    /// and for methods from arbitrary .NET assemblies.
    /// 
    /// All calls are made through the optimizedTarget which is created lazily.
    /// </summary>
    [PythonType("builtin_function_or_method")]
    public partial class BuiltinFunction :
        FastCallable, IFancyCallable, IContextAwareMember, IDynamicObject {
        private string name;
        private MethodBase[] targets;
        private FunctionType funcType;
        private FastCallable optimizedTarget;

        [PythonName("__new__")]
        public static object Make(object cls, PythonFunction newFunction, object inst) {
            return new Method(newFunction, inst, null);
        }


        #region Static factories
        public static BuiltinFunction MakeMethod(string name, MethodBase info, FunctionType ft) {
            return new BuiltinFunction(name, new MethodBase[] { info }, ft);
        }

        public static BuiltinFunction MakeMethod(string name, MethodBase[] infos, FunctionType ft) {
            return new BuiltinFunction(name, infos, ft);
        }

        public static BuiltinFunction MakeOrAdd(BuiltinFunction existing, string name, MethodBase mi, FunctionType funcType) {
            if (existing != null) {
                existing.AddMethod(mi);
                return existing;
            } else {
                return MakeMethod(name, mi, funcType);
            }
        }
        #endregion

        #region Private Constructors
        internal BuiltinFunction() { }

        private BuiltinFunction(string name, MethodBase[] originalTargets, FunctionType functionType) {
            Debug.Assert(originalTargets != null, "originalTargets array is null");

            MethodBase target = originalTargets[0];
            Debug.Assert(target != null, "no targets passed to make BuiltinFunction");
            Debug.Assert(name != null, String.Format("name is null for {0}", target.Name));

            funcType = functionType;
            targets = originalTargets;
            this.name = name;

            for (int i = 0; i < originalTargets.Length; i++) {
                UpdateFunctionInfo(originalTargets[i]);
            }
        }

        private BuiltinFunction(string name, MethodBase target, FunctionType functionType)
            : this(name, new MethodBase[] { target }, functionType) {
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
                if (Name == "__init__") return DeclaringType.Name;
                return Name;
            }
        }
        public string FunctionName {
            [PythonName("func_name")]
            get {
                return Name;
            }
        }
        public string Module {
            [PythonName("__module__")]
            get {
                if (targets.Length > 0) {
                    return (string)DeclaringType.dict[SymbolTable.Module];
                }
                return null;
            }
        }
        public FastCallable OptimizedTarget {
            get {
                if (optimizedTarget == null) {
                    if (targets.Length == 0) throw Ops.TypeError("cannot call generic method w/o specifying type");
                    optimizedTarget = MethodBinder.MakeFastCallable(Name, targets, IsBinaryOperator);
                    if (IsReversedOperator) optimizedTarget = new ReversedFastCallableWrapper(optimizedTarget);
                }
                return optimizedTarget;
            }
        }

        public void AddMethod(MethodBase info) {
            if (optimizedTarget != null) optimizedTarget = null;
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

        [PythonName("__call__")]
        public override object Call(ICallerContext context, params object[] args) {
            return OptimizedTarget.Call(context, args);
        }

        public override object CallInstance(ICallerContext context, object instance, params object[] args) {
            return OptimizedTarget.CallInstance(context, instance, args);
        }

        [PythonName("__call__")]
        public object Call(ICallerContext context, object[] args, string[] names) {
            return CallHelper(context, args, names, null);
        }

        internal object CallHelper(ICallerContext context, object[] args, string[] names, object instance) {
            // we allow kw-arg binding to ctor's of arbitrary CLS types, but
            // NOT Python built-in types.  After the ctor succeeds we'll set the kw args as
            // arbitrary properties on the CLS type.  If this ends up being a built-in type we'll
            // do the check when we're going to set the kw-args.  This accomplishes 2 things:
            //      1. Our error messages match CPython more closely 
            //      2. The attribute lookup is done lazily only if kw-args are supplied to a ctor

            CoerceArgs(context, ref args, ref instance);

            KwArgBinder kwArgBinder = new KwArgBinder(context, args, names, targets[0].IsConstructor);
            MethodBinding bestBinding = new MethodBinding();
            List<UnboundArgument> bestUnboundArgs = null;

            for (int i = 0; i < targets.Length; i++) {
                object[] realArgs = kwArgBinder.DoBind(targets[i], Name);

                if (realArgs != null) {
                    MethodBinding mb = new MethodBinding();
                    mb.method = targets[i];

                    if (!CompilerHelpers.IsStatic(targets[i])) {
                        if (instance == null) {
                            if (realArgs.Length == 0) {
                                throw Ops.TypeError("bad number of arguments for function {0}", targets[0].Name);
                            }
                            mb.instance = realArgs[0];
                            mb.arguments = new object[realArgs.Length - 1];
                            Array.Copy(realArgs, mb.arguments, realArgs.Length - 1);
                        } else {
                            mb.instance = instance;
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
                object[] callArgs = bestBinding.arguments;

                ParameterInfo[] pis = bestBinding.method.GetParameters();
                object[] dynamicArgs = new object[pis.Length];
                for (int i = 0; i < pis.Length; i++) {
                    dynamicArgs[i] = Ops.GetDynamicTypeFromType(pis[i].ParameterType);
                }
                FastCallable fc = (FastCallable)Overloads.GetKeywordArgumentOverload(new Tuple(true, dynamicArgs));
                object ret;
                if (instance == null) ret = fc.Call(context, callArgs);
                else ret = fc.CallInstance(context, instance, callArgs);

                // any unbound arguments left over we assume the user
                // wants to do a property set with.  We'll go ahead and try
                // that - if they fail we'll throw.
                if (bestUnboundArgs != null) {
                    // if we had a constructor w/ a ref param then we'll try
                    // updating the Tuple here instead of the user's object.

                    if (targets[0].DeclaringType.IsDefined(typeof(PythonTypeAttribute), false)) {
                        // calling ctor w/ kw-args w/ zero args, let it go, don't do any sets.
                        if (args.Length == names.Length) return ret;

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

        /// <summary>
        /// Updates args & instance based upon if we need to pass the instance or not and
        /// if it's context aware or not.
        /// </summary>
        private void CoerceArgs(ICallerContext context, ref object[] args, ref object instance) {
            if ((FunctionType & FunctionType.OpsFunction) != 0) {
                if (instance != null) {
                    object[] argsWithInstance = new object[args.Length + 1];
                    argsWithInstance[0] = instance;
                    Array.Copy(args, 0, argsWithInstance, 1, args.Length);
                    args = argsWithInstance;
                    instance = null;
                }
            } else if (instance == null && (FunctionType & FunctionType.FunctionMethodMask) == FunctionType.Method) {
                instance = args[0];
                object[] realArgs = new object[args.Length - 1];
                Array.Copy(args, 1, realArgs, 0, realArgs.Length);
                args = realArgs;
            }
        }

        [PythonName("__call__")]
        public object Call(ICallerContext context, [ParamDict]Dict dictArgs, params object[] args) {
            object[] realArgs;
            string[] argNames;
            DictArgsHelper(dictArgs, args, out realArgs, out argNames);

            return CallHelper(context, realArgs, argNames, null);
        }

        internal void DictArgsHelper(Dict dictArgs, object[] args, out object[] realArgs, out string[] argNames) {
            realArgs = new object[args.Length + dictArgs.Count];
            argNames = new string[dictArgs.Count];

            Array.Copy(args, realArgs, args.Length);

            int index = 0;
            foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)dictArgs) {
                argNames[index] = kvp.Key as string;
                realArgs[index + args.Length] = kvp.Value;
                index++;
            }
        }

        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("<built-in function {0}>", Name);
        }

        public virtual string Documentation {
            [PythonName("__doc__")]
            get {
                StringBuilder sb = new StringBuilder();
                MethodBase[] targets = Targets;
                bool needNewLine = false;
                for (int i = 0; i < targets.Length; i++) {
                    if (targets[i] != null) AddDocumentation(sb, ref needNewLine, targets[i]);
                }
                return sb.ToString();
            }
        }

        public object Self {
            [PythonName("__self__")]
            get {
                return null;
            }
        }

        // Provides (for reflected methods) a mapping from a signature to the exact target
        // which takes this signature.
        // signature with syntax like the following:
        //    someClass.SomeMethod.Overloads[str, int]("Foo", 123)
        public virtual BuiltinFunctionOverloadMapper Overloads {
            [Documentation(@"Overloads -> dictionary of methods indexed by a tuple of types.

Returns all signature overloads of a builtin method.
Eg. The following will call the overload of WriteLine that takes an int argument.
   System.Console.WriteLine.Overloads[int](100)")]
            get {
                // The mapping is actually provided by a class rather than a dictionary
                // since it's hard to generate all the keys of the signature mapping when
                // two type systems are involved.  Creating the mapping object is quite
                // cheap so we don't cache a copy.
                return new BuiltinFunctionOverloadMapper(this, null);
            }
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

        public bool IsReversedOperator {
            get {
                return (FunctionType & FunctionType.ReversedOperator) != 0;
            }
        }

        public bool IsBinaryOperator {
            get {
                return (FunctionType & FunctionType.BinaryOperator) != 0;
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


        #endregion

        #region IContextAwareMember Members

        public bool IsVisible(ICallerContext context) {
            return IsPythonVisible || ((context.ContextFlags & CallerContextAttributes.ShowCls) != 0);
        }

        #endregion

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
                BuiltinFunction rm = new BuiltinFunction();

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
                rm.FunctionType = FunctionType | FunctionType.OptimizeChecked;    // don't want to optimize & whack our dictionary.

                return rm;
            }
        }

        #region Protected APIs

        private void UpdateFunctionInfo(MethodBase info) {
            ParameterInfo[] parameters = info.GetParameters();
            if (parameters.Length > 0 && parameters[0].ParameterType == typeof(ICallerContext)) {
                FunctionType |= FunctionType.IsContextAware;
            }
        }

        private struct MethodBinding {
            public MethodBase method;
            public object[] arguments;
            public object instance;
        }

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
        None = 0x0000,   // No flags have been set
        Function = 0x0001,   // This is a function w/ no instance pointer
        Method = 0x0002,   // This is a method that requires an instance
        FunctionMethodMask = 0x0003,   // Built-in functions can encapsulate both methods & functions, in which case both bits are set
        PythonVisible = 0x0004,   // True is the function/method should be visible from pure-Python code
        IsContextAware = 0x0008,   // True if the function can receive the ICallerContext parameter.
        SkipThisCheck = 0x0010,   // we should skip the type check for the this pointer (due to base type, or an InstanceOps method).
        OptimizeChecked = 0x0020,   // True if we've checked if we could optimize the function, and we can't.
        OpsFunction = 0x0040,   // True if this is a function/method declared on an Ops type (StringOps, IntOps, etc...)
        Params = 0x0080,   // True if this is a params method, false otherwise.
        ReversedOperator = 0x0100,   // True if this is a __r*__ method for a CLS overloaded operator method
        BinaryOperator = 0x0200,   // This method represents a binary operator method for a CLS overloaded operator method
    }

    [PythonType("method_descriptor")]
    public sealed class BuiltinMethodDescriptor : IDescriptor, IFancyCallable, ICallable, IContextAwareMember, ICallableWithCallerContext {
        internal BuiltinFunction template;

        public BuiltinMethodDescriptor(BuiltinFunction function) {
            template = function;
        }

        internal object UncheckedGetAttribute(object instance) {
            if (instance == null) return this;
            return new BoundBuiltinFunction(template, instance);
        }

        #region IDescriptor Members

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (instance != null) {
                CheckSelf(instance);
                return UncheckedGetAttribute(instance);
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
                return String.Format("<method {0} of {1} objects>", Ops.Repr(Name), Ops.Repr(dt.Name));
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
            CheckSelfInArgs(args);
            return template.Call(args);
        }

        #endregion

        #region IFancyCallable Members

        [PythonName("__call__")]
        public object Call(ICallerContext context, object[] args, string[] names) {
            CheckSelfInArgs(args);
            return template.Call(context, args, names);
        }

        #endregion

        #region ICallableWithCallerContext Members

        [PythonName("__call__")]
        public object Call(ICallerContext context, object[] args) {
            CheckSelfInArgs(args);
            return template.Call(context, args);
        }

        #endregion

        private void CheckSelfInArgs(object[] args) {
            if (args.Length == 0)
                throw Ops.TypeError("descriptor {0} of {1} needs an argument",
                    Ops.Repr(Name),
                    Ops.Repr(DeclaringType.Name));

            CheckSelf(args[0]);
        }

        private void CheckSelf(object self) {
            // if the type has been re-optimized (we also have base type info in here) 
            // then we can't do the type checks right now :(.
            if ((template.FunctionType & FunctionType.SkipThisCheck) != 0)
                return;

            if ((template.FunctionType & FunctionType.FunctionMethodMask) == FunctionType.Method) {
                CheckSelfWorker(self, template);
            }
        }

        internal static void CheckSelfWorker(object self, BuiltinFunction template) {
            // to a fast check on the CLR types, if they match we can avoid the slower
            // check that involves looking up dynamic types. (self can be null on
            // calls like set.add(None) 
            if (self != null && self.GetType() == template.ClrDeclaringType) return;

            DynamicType selfType = self == null ? NoneTypeOps.TypeInstance : Ops.GetDynamicTypeFromType(self.GetType());
            Debug.Assert(selfType != null);

            ReflectedType declType = template.DeclaringType;
            if (!selfType.IsSubclassOf(declType)) {
                // if a conversion exists to the type allow the call.
                object converted;
                if (!Converter.TryConvert(self, declType.type, out converted) || converted == null) {
                    throw Ops.TypeError("descriptor {0} requires a {1} object but received a {2}",
                        Ops.Repr(template.Name),
                        Ops.Repr(template.DeclaringType.Name),
                        Ops.Repr(Ops.GetPythonTypeName(self)));
                }
            }
            return;
        }

        #region IContextAwareMember Members

        public bool IsVisible(ICallerContext context) {
            return template.IsVisible(context);
        }

        #endregion

    }

    [PythonType("classmethod_descriptor")]
    public class ClassMethodDescriptor : IDescriptor, ICodeFormattable {
        internal BuiltinFunction func;

        internal ClassMethodDescriptor(BuiltinFunction func) {
            this.func = func;
        }

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, null); }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            owner = CheckGetArgs(instance, owner);
            return new Method(func, owner, Ops.GetDynamicType(owner));
        }

        private object CheckGetArgs(object instance, object owner) {
            if (owner == null) {
                if (instance == null) throw Ops.TypeError("__get__(None, None) is invalid");
                owner = Ops.GetDynamicType(instance);
            } else {
                DynamicType dt = owner as DynamicType;
                if (dt == null) {
                    throw Ops.TypeError("descriptor {0} for type {1} needs a type, not a {2}",
                        Ops.StringRepr(func.Name),
                        Ops.StringRepr(func.DeclaringType.Name),
                        Ops.StringRepr(Ops.GetDynamicType(owner)));
                }
                if (!dt.IsSubclassOf(TypeCache.Dict)) {
                    throw Ops.TypeError("descriptor {0} for type {1} doesn't apply to type {2}",
                        Ops.StringRepr(func.Name),
                        Ops.StringRepr(func.DeclaringType.Name),
                        Ops.StringRepr(dt.Name));
                }
            }
            if (instance != null)
                BuiltinMethodDescriptor.CheckSelfWorker(instance, func);

            return owner;
        }
        #endregion

        #region ICodeFormattable Members

        public string ToCodeString() {
            BuiltinFunction bf = func as BuiltinFunction;
            if (bf != null) {
                return String.Format("<method {0} of {1} objects>",
                    Ops.StringRepr(bf.Name),
                    Ops.StringRepr(bf.DeclaringType));
            }

            return String.Format("<classmethod object at {0}>",
                IdDispenser.GetId(this));
        }

        #endregion

        public override bool Equals(object obj) {
            ClassMethodDescriptor cmd = obj as ClassMethodDescriptor;
            if (cmd == null) return false;

            return cmd.func == func;
        }

        public override int GetHashCode() {
            return ~func.GetHashCode();
        }
    }

    [PythonType(typeof(BuiltinFunction))]
    public partial class BoundBuiltinFunction : FastCallable, IFancyCallable {
        BuiltinFunction target;
        private object instance;

        [PythonName("__new__")]
        public static object Make(object cls, object newFunction, object inst) {
            return new Method(newFunction, inst, null);
        }

        public BoundBuiltinFunction(BuiltinFunction target, object instance) {
            this.target = target;
            this.instance = instance;
        }

        public object Self {
            [PythonName("__self__")]
            get { return instance; }
        }

        public string Name {
            [PythonName("__name__")]
            get { return target.Name; }
        }

        [PythonName("__eq__")]
        public override bool Equals(object obj) {
            BoundBuiltinFunction other = obj as BoundBuiltinFunction;
            if (other == null) return false;

            return other.instance == instance && other.target == target;
        }

        [PythonName("__hash__")]
        public override int GetHashCode() {
            return instance.GetHashCode() ^ target.GetHashCode();
        }

        public string Documentation {
            [PythonName("__doc__")]
            get {
                return target.Documentation;
            }
        }

        public BuiltinFunctionOverloadMapper Overloads {
            get {
                return new BuiltinFunctionOverloadMapper(target, instance);
            }
        }

        public object this[object key] {
            get {
                return new BoundBuiltinFunction((BuiltinFunction)target[key], instance);
            }
        }

        public override object Call(ICallerContext context, params object[] args) {
            return target.OptimizedTarget.CallInstance(context, instance, args);
        }

        [PythonName("__call__")]
        public object Call(ICallerContext context, [ParamDict]Dict dictArgs, params object[] args) {
            object[] realArgs;
            string[] argNames;
            target.DictArgsHelper(dictArgs, args, out realArgs, out argNames);

            return target.CallHelper(context, realArgs, argNames, instance);
        }

        public override object CallInstance(ICallerContext context, object instance, params object[] args) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #region IFancyCallable Members

        public object Call(ICallerContext context, object[] args, string[] names) {
            return target.CallHelper(context, args, names, instance);
        }

        #endregion

        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("<built-in method {0} of {1} object at {2}>",
                    Name,
                    Ops.GetPythonTypeName(instance),
                    Ops.HexId(instance));
        }
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
                return GetOverload(key, Targets);
            }
        }

        protected object GetOverload(object key, MethodBase[] targets) {
            // Retrieve the signature from the index.
            Type[] sig = GetSignatureFromKey(key);

            // We can still end up with more than one target since generic and non-generic
            // methods can share the same name and signature. So we'll build up a new
            // reflected method with all the candidate targets. A caller can then index this
            // reflected method if necessary in order to provide generic type arguments and
            // fully disambiguate the target.
            BuiltinFunction rm = new BuiltinFunction();
            rm.Name = function.Name;
            rm.FunctionType = function.FunctionType | FunctionType.OptimizeChecked; // don't allow optimization that would whack the real entry

            // Search for targets with the right number of arguments.
            FindMatchingTargets(sig, targets, rm);

            if (rm.Targets == null)
                throw Ops.TypeError("No match found for the method signature {0}", key);

            if (instance != null) {
                return new BoundBuiltinFunction(rm, instance);
            } else {
                return GetTargetFunction(rm);
            }
        }

        private void FindMatchingTargets(Type[] sig, MethodBase[] targets, BuiltinFunction rm) {
            int args = sig.Length;

            foreach (MethodBase mb in targets) {
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
        }

        private static Type[] GetSignatureFromKey(object key) {
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
            return sig;
        }

        protected BuiltinFunction Function {
            get {
                return function;
            }
        }

        protected virtual MethodBase[] Targets {
            get {
                return function.Targets;
            }
        }

        protected virtual object GetTargetFunction(BuiltinFunction bf) {
            return bf;
        }

        internal virtual object GetKeywordArgumentOverload(object key) {
            return GetOverload(key, Function.Targets);
        }

    }

    public class ConstructorOverloadMapper : BuiltinFunctionOverloadMapper {
        public ConstructorOverloadMapper(ConstructorFunction builtinFunction, object instance)
            : base(builtinFunction, instance) {
        }

        protected override MethodBase[] Targets {
            get {
                return ((ConstructorFunction)Function).ConstructorTargets;
            }
        }

        internal override object GetKeywordArgumentOverload(object key) {
            return base.GetOverload(key, Function.Targets);
        }

        protected override object GetTargetFunction(BuiltinFunction bf) {
            // return a function that's bound to the overloads, we'll
            // the user then calls this w/ the dynamic type, and the bound
            // function drops the class & calls the overload.
            if (bf.Targets[0].DeclaringType != typeof(InstanceOps))
                return new BoundBuiltinFunction(new ConstructorFunction(InstanceOps.OverloadedNew, bf.Targets), bf);
            return base.GetTargetFunction(bf);
        }
    }

    public class ConstructorFunction : BuiltinFunction {
        private MethodBase[] ctors;

        public ConstructorFunction(BuiltinFunction realTarget, MethodBase[] constructors)
            : base() {
            base.Name = "__new__";
            base.Targets = realTarget.Targets;
            base.FunctionType = realTarget.FunctionType;
            this.ctors = constructors;
        }

        public override BuiltinFunctionOverloadMapper Overloads {
            get {
                return new ConstructorOverloadMapper(this, null);
            }
        }

        public override string Documentation {
            get {
                StringBuilder sb = new StringBuilder();
                MethodBase[] targets = ctors;

                for (int i = 0; i < targets.Length; i++) {
                    if (targets[i] != null) sb.AppendLine(ReflectionUtil.DocOneInfo(targets[i]));
                }
                return sb.ToString();

            }
        }

        internal MethodBase[] ConstructorTargets {
            get {
                return ctors;
            }
        }
    }
}