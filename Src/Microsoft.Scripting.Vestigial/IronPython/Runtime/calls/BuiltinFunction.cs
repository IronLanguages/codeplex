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
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Internal.Generation;
using IronPython.Runtime;

using Ops = IronPython.Runtime.Operations.Ops;
using MethodBinder = IronPython.Compiler.MethodBinder;
using PythonBinder = IronPython.Runtime.Calls.PythonBinder;
using BinderType = IronPython.Compiler.BinderType;
using KwArgBinder = IronPython.Runtime.Calls.KwArgBinder;
using UnboundArgument = IronPython.Runtime.Calls.UnboundArgument;
using NoneTypeOps = IronPython.Runtime.Types.NoneTypeOps;
using InstanceOps = IronPython.Runtime.Operations.InstanceOps;
using System.Threading;

namespace Microsoft.Scripting {
    
    /// <summary>
    /// BuiltinFunction represents any standard CLR function exposed to Python.
    /// This is used for both methods on standard Python types such as list or tuple
    /// and for methods from arbitrary .NET assemblies.
    /// 
    /// All calls are made through the optimizedTarget which is created lazily.
    /// </summary>
    public partial class BuiltinFunction :
        FastCallable, IFancyCallable, IContextAwareMember {
        private string _name;
        private MethodBase[] _targets;
        private FunctionType _funcType;
        private FastCallable _optimizedTarget;
        private Dictionary<MethodBase, FastCallable> _optKwTargets;
        private Dictionary<TypeList, FastCallable> _genMethods;
        private int _id;
        private static int _curId;

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
        internal BuiltinFunction() {
            _id = Interlocked.Increment(ref _curId);
        }

        private BuiltinFunction(string name, MethodBase[] originalTargets, FunctionType functionType) : this() {
            Debug.Assert(originalTargets != null, "originalTargets array is null");

            MethodBase target = originalTargets[0];
            Debug.Assert(target != null, "no targets passed to make BuiltinFunction");
            Debug.Assert(name != null, String.Format("name is null for {0}", target.Name));

            _funcType = functionType;
            _targets = originalTargets;
            this._name = name;
        }

        private BuiltinFunction(string name, MethodBase target, FunctionType functionType)
            : this(name, new MethodBase[] { target }, functionType) {
        }

        #endregion

        #region Public API Surface

        public string Name {
            get {
                return _name;
            }
            internal set {
                _name = value;
            }
        }

        public string FriendlyName {
            get {
                if (Name == "__init__") return DeclaringType.Name;
                return Name;
            }
        }

        public FastCallable OptimizedTarget {
            get {
                if (_optimizedTarget == null) {
                    if (_targets.Length == 0) throw Ops.TypeError("cannot call generic method w/o specifying type");
                    _optimizedTarget = MethodBinder.MakeFastCallable(PythonBinder.Default, Name, _targets, IsBinaryOperator ? BinderType.BinaryOperator : BinderType.Normal);
                    if (IsReversedOperator) _optimizedTarget = new ReversedFastCallableWrapper(_optimizedTarget);
                }
                return _optimizedTarget;
            }
        }

        public void AddMethod(MethodBase info) {
            if (_optimizedTarget != null) _optimizedTarget = null;
            if (_targets != null) {
                MethodBase[] ni = new MethodBase[_targets.Length + 1];
                _targets.CopyTo(ni, 0);
                ni[_targets.Length] = info;
                _targets = ni;
            } else {
                _targets = new MethodBase[] { info };
            }
        }

        public override object CallInstance(CodeContext context, object instance, params object[] args) {
            return OptimizedTarget.CallInstance(context, instance, args);
        }

        [OperatorMethod]
        public override object Call(CodeContext context, params object[] args) {
            return OptimizedTarget.Call(context, args);
        }

        [OperatorMethod]
        public object Call(CodeContext context, object[] args, string[] names) {
            return CallHelper(context, args, names, null);
        }

        [OperatorMethod]
        public object Call(CodeContext context, [ParamDictionary]IDictionary<object,object> dictArgs, params object[] args) {
            object[] realArgs;
            string[] argNames;
            DictArgsHelper(dictArgs, args, out realArgs, out argNames);

            return CallHelper(context, realArgs, argNames, null);
        }

        internal object CallHelper(CodeContext context, object[] args, string[] names, object instance) {
            // we allow kw-arg binding to ctor's of arbitrary CLS types, but
            // NOT Python built-in types.  After the ctor succeeds we'll set the kw args as
            // arbitrary properties on the CLS type.  If this ends up being a built-in type we'll
            // do the check when we're going to set the kw-args.  This accomplishes 2 things:
            //      1. Our error messages match CPython more closely 
            //      2. The attribute lookup is done lazily only if kw-args are supplied to a ctor

            KwArgBinder kwArgBinder = new KwArgBinder(context, instance, args, names, _targets[0].IsConstructor);
            MethodBase bestTarget = null;
            object[] bestArgs = null;
            List<UnboundArgument> bestUnboundArgs = null;

            for (int i = 0; i < _targets.Length; i++) {
                object[] realArgs = kwArgBinder.DoBind(_targets[i], Name);

                if (realArgs != null) {
                    if (!kwArgBinder.AllowUnboundArgs) {
                        // we can have no better bindings!
                        bestTarget = _targets[i];
                        bestArgs = realArgs;

                        break;
                    }

                    if (bestTarget == null ||
                        (kwArgBinder.UnboundArgs == null ||
                        (bestUnboundArgs != null && bestUnboundArgs.Count > kwArgBinder.UnboundArgs.Count))) {
                        bestTarget = _targets[i];
                        bestArgs = realArgs;

                        bestUnboundArgs = kwArgBinder.UnboundArgs;
                    }

                }
            }

            if (bestTarget != null) {
                FastCallable fc;
                if (_optKwTargets == null || !_optKwTargets.TryGetValue(bestTarget, out fc)) {
                    // we've bound the arguments to a real method,
                    // finally we're going to dispatch back to the 
                    // optimized version of the calls.
                    ParameterInfo[] pis = bestTarget.GetParameters();
                    object[] dynamicArgs = new object[pis.Length];
                    for (int i = 0; i < pis.Length; i++) {
                        dynamicArgs[i] = DynamicHelpers.GetDynamicTypeFromType(pis[i].ParameterType);
                    }
                    fc = (FastCallable)Overloads.GetKeywordArgumentOverload(new Tuple(true, dynamicArgs));
                    if (_optKwTargets == null) _optKwTargets = new Dictionary<MethodBase, FastCallable>();
                    
                    _optKwTargets[bestTarget] = fc;
                }

                object ret;
                if (CompilerHelpers.IsStatic(bestTarget)) {
                    ret = fc.Call(context, bestArgs);
                } else {
                    ret = fc.CallInstance(context, instance, bestArgs);
                }

                // any unbound arguments left over we assume the user
                // wants to do a property set with.  We'll go ahead and try
                // that - if they fail we'll throw.
                if (bestUnboundArgs != null) {
                    // if we had a constructor w/ a ref param then we'll try
                    // updating the Tuple here instead of the user's object.

                    if (_targets[0].DeclaringType.IsDefined(typeof(PythonTypeAttribute), false)) {
                        // calling ctor w/ kw-args w/ zero args, let it go, don't do any sets.
                        if (args.Length == names.Length) return ret;

                        throw Ops.TypeError("'{0}' is an invalid keyword argument for this function",
                            bestUnboundArgs[0].Name,
                            Name);
                    }

                    for (int j = 0; j < bestUnboundArgs.Count; j++) {
                        Ops.SetAttr(context,
                            ret,
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


        internal void DictArgsHelper(IDictionary<object,object> dictArgs, object[] args, out object[] realArgs, out string[] argNames) {
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
        public DynamicTypeSlot GetDescriptor() {
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
                return (_funcType & FunctionType.AlwaysVisible) != 0;
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
                return _funcType;
            }
            internal set {
                _funcType = value;
            }
        }

        public int Id {
            get {
                return _id;
            }
        }

        #endregion

        #region Internal API Surface
        internal DynamicType DeclaringType {
            get {
                MethodBase target = Targets[0];

                if ((FunctionType & FunctionType.OpsFunction) == 0) {
                    // normal method 
                    return DynamicHelpers.GetDynamicTypeFromType(target.DeclaringType);
                } else {
                    // method declared on ops-type, return the declaring type as
                    // the type we add methods on to.

                    Debug.Assert(ExtensionTypeAttribute.IsExtensionType(target.DeclaringType), String.Format("Type {0} is not an Ops Type ({1})", target.DeclaringType, Name));
                    return ExtensionTypeAttribute.GetExtendedTypeFromExtension(target.DeclaringType);
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
                    Debug.Assert(ExtensionTypeAttribute.IsExtensionType(target.DeclaringType), String.Format("Type {0} is not an Ops Type ({1})", target.DeclaringType, Name));
                    return ExtensionTypeAttribute.GetExtendedTypeFromExtension(target.DeclaringType).UnderlyingSystemType;
                }
            }
        }

        /// <summary>
        /// Gets the target methods that we'll be calling.  
        /// </summary>
        public MethodBase[] Targets {
            get {
                return _targets;
            }
            set {
                _targets = value;
            }
        }


        #endregion

        #region IContextAwareMember Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public override bool IsVisible(CodeContext context, DynamicMixin owner) {
            Debug.Assert(context != null);

            if (context.LanguageContext.ShowCls) {
                return true;
            }

            return IsPythonVisible;
        }

        #endregion

        internal ContextId Context {
            get {
                return ContextId.Empty;
            }
        }

        private class TypeList {
            private Type[] _types;

            public TypeList(Type[] types) {
                _types = types;
            }

            public override bool Equals(object obj) {
                TypeList tl = obj as TypeList;
                if (tl == null) return false;

                if (tl._types.Length != _types.Length) return false;
                for (int i = 0; i < _types.Length; i++) {
                    if (tl._types[i] != _types[i]) return false;
                }
                return true;
            }

            public override int GetHashCode() {
                int res = 6551;
                foreach (Type t in _types) {
                    res = (res << 5) ^ t.GetHashCode();
                }
                return res;
            }
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

                TypeList tl = new TypeList(types);
                lock (this) {
                    FastCallable res;
                    if (_genMethods != null && _genMethods.TryGetValue(tl, out res)) {
                        return res;
                    }
                }

                // Start building a new ReflectedMethod that will contain targets with bound type
                // arguments.
                BuiltinFunction rm = new BuiltinFunction();

                // Search for generic targets with the correct arity (number of type parameters).
                // Compatible targets must be MethodInfos by definition (constructors never take
                // type arguments).
                int arity = types.Length;
                foreach (MethodBase mb in _targets) {
                    MethodInfo mi = mb as MethodInfo;
                    if (mi == null)
                        continue;
                    if (mi.ContainsGenericParameters && mi.GetGenericArguments().Length == arity)
                        rm.AddMethod(mi.MakeGenericMethod(types));
                }
                if (rm.Targets == null)
                    throw Ops.TypeError(string.Format("bad type args to this generic method {0}", this));

                rm.Name = Name;
                rm.FunctionType = FunctionType;

                lock (this) {
                    if (_genMethods == null) _genMethods = new Dictionary<TypeList, FastCallable>();
                    _genMethods[tl] = rm;
                }

                return rm;
            }
        }

        #region DynamicTypeSlot Overrides

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = this;
            return true;
        }

        #endregion
    }

    public sealed class BuiltinMethodDescriptor : DynamicTypeSlot, IFancyCallable, IContextAwareMember, ICallableWithCodeContext {
        internal BuiltinFunction template;

        public BuiltinMethodDescriptor(BuiltinFunction function) {
            template = function;
        }

        internal object UncheckedGetAttribute(object instance) {
            if (instance == null) return this;
            return new BoundBuiltinFunction(template, instance);
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            if (instance != null) {
                CheckSelf(instance);
                value = UncheckedGetAttribute(instance);
                return true;
            }
            value = this;
            return true;
        }

        public override bool TryGetBoundValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            return TryGetValue(context, instance, owner, out value);
        }

        public BuiltinFunction Template {
            get { return template; }
        }

        public DynamicType DeclaringType {
            get {
                return template.DeclaringType;
            }
        }

        public override bool Equals(object obj) {
            BuiltinMethodDescriptor bmf = obj as BuiltinMethodDescriptor;
            if (bmf == null) return false;
            return template.Equals(bmf.template);
        }

        public override int GetHashCode() {
            return template.GetHashCode();
        }

        public override string ToString() {
            DynamicType dt = DeclaringType;

            if (dt != null) {
                return String.Format("<method {0} of {1} objects>", Ops.Repr(Name), Ops.Repr(dt.Name));
            } else {
                return String.Format("<method {0} of {1} objects>", Ops.Repr(Name), Ops.Repr("<unknown>"));
            }
        }

        public string Name {
            get {
                return template.Name;
            }
        }

        #region IFancyCallable Members

        [OperatorMethod]
        public object Call(CodeContext context, object[] args, string[] names) {
            CheckSelfInArgs(args);
            return template.Call(context, args, names);
        }

        #endregion

        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, object[] args) {
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

            DynamicType selfType = self == null ? NoneTypeOps.TypeInstance : DynamicHelpers.GetDynamicTypeFromType(self.GetType());
            Debug.Assert(selfType != null);

            DynamicType declType = template.DeclaringType;
            if (!selfType.IsSubclassOf(declType)) {
                // if a conversion exists to the type allow the call.
                object converted;
                if (!Converter.TryConvert(self, declType.UnderlyingSystemType, out converted) || converted == null) {
                    throw Ops.TypeError("descriptor {0} requires a {1} object but received a {2}",
                        Ops.Repr(template.Name),
                        Ops.Repr(template.DeclaringType.Name),
                        Ops.Repr(Ops.GetPythonTypeName(self)));
                }
            }
            return;
        }

        #region IContextAwareMember Members

        public override bool IsVisible(CodeContext context, DynamicMixin owner) {
            return template.IsVisible(context, owner);
        }

        #endregion

    }
   
    public partial class BoundBuiltinFunction : FastCallable, IFancyCallable {
        private BuiltinFunction _target;
        private object _instance;

        public BoundBuiltinFunction(BuiltinFunction target, object instance) {
            this._target = target;
            this._instance = instance;
        }

        public object Self {
            get { return _instance; }
        }

        public string Name {
            get { return _target.Name; }
        }

        public override bool Equals(object obj) {
            BoundBuiltinFunction other = obj as BoundBuiltinFunction;
            if (other == null) return false;

            return other._instance == _instance && other._target == _target;
        }

        public override int GetHashCode() {
            return _instance.GetHashCode() ^ _target.GetHashCode();
        }

        public BuiltinFunctionOverloadMapper Overloads {
            get {
                return new BuiltinFunctionOverloadMapper(_target, _instance);
            }
        }

        public object this[object key] {
            get {
                return new BoundBuiltinFunction((BuiltinFunction)_target[key], _instance);
            }
        }

        public override object Call(CodeContext context, params object[] args) {
            return _target.OptimizedTarget.CallInstance(context, _instance, args);
        }

        [OperatorMethod]
        public object Call(CodeContext context, [ParamDictionary]IDictionary<object,object> dictArgs, params object[] args) {
            object[] realArgs;
            string[] argNames;
            _target.DictArgsHelper(dictArgs, args, out realArgs, out argNames);

            return _target.CallHelper(context, realArgs, argNames, _instance);
        }

        public override object CallInstance(CodeContext context, object instance, params object[] args) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public BuiltinFunction Target {
            get {
                return _target;
            }
        }

        #region IFancyCallable Members

        public object Call(CodeContext context, object[] args, string[] names) {
            return _target.CallHelper(context, args, names, _instance);
        }

        #endregion
    }

    // Used to map signatures to specific targets on the embedded reflected method.
    public class BuiltinFunctionOverloadMapper {
        private BuiltinFunction function;
        private object instance;

        public BuiltinFunctionOverloadMapper(BuiltinFunction builtinFunction, object instance) {
            this.function = builtinFunction;
            this.instance = instance;
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
            rm.FunctionType = function.FunctionType; 

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

        public BuiltinFunction Function {
            get {
                return function;
            }
        }

        public virtual MethodBase[] Targets {
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

        public override MethodBase[] Targets {
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
            if (realTarget == null) throw new ArgumentNullException("realTarget");

            base.Name = realTarget.Name;
            base.Targets = realTarget.Targets;
            base.FunctionType = realTarget.FunctionType;
            this.ctors = constructors;
        }

        public override BuiltinFunctionOverloadMapper Overloads {
            get {
                return new ConstructorOverloadMapper(this, null);
            }
        }
        
        internal MethodBase[] ConstructorTargets {
            get {
                return ctors;
            }
        }
    }
}
