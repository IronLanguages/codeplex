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
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;


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
        private Dictionary<TypeList, BuiltinFunction> _boundGenerics;
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

        public BuiltinFunction() {
            _id = Interlocked.Increment(ref _curId);
        }

        public BuiltinFunction(string name, FunctionType functionType)
            : this() {
            Utils.Assert.NotNull(name);
            
            _name = name;
            _funcType = functionType;
        }

        #region Private Constructors

        private BuiltinFunction(string name, MethodBase[] originalTargets, FunctionType functionType) : this() {
            Utils.Assert.NotNull(name);
            Utils.Assert.NotNullItems(originalTargets);

            _funcType = functionType;
            _targets = originalTargets;
            _name = name;
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

        public void AddMethod(MethodBase info) {
            Utils.Assert.NotNull(info);

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
            if (IsReversedOperator && args.Length == 1) {
                return MethodBinder.MakeBinder(context.LanguageContext.Binder, Name, Targets, BinderType).CallReflected(context, CallType.ImplicitInstance, args[0], instance);
            }

            return MethodBinder.MakeBinder(context.LanguageContext.Binder, Name, Targets, BinderType).
                CallReflected(context, CallType.ImplicitInstance, Utils.Array.Insert(instance, args));
        }

        private BinderType BinderType {
            get {
                return IsBinaryOperator ? BinderType.BinaryOperator : BinderType.Normal;
            }
        }

        [OperatorMethod]
        public override object Call(CodeContext context, params object[] args) {
            if (IsReversedOperator && args.Length == 2) {
                return MethodBinder.MakeBinder(context.LanguageContext.Binder, Name, Targets, BinderType).CallReflected(context, CallType.None, args[1], args[0]);
            }

            return MethodBinder.MakeBinder(context.LanguageContext.Binder, Name, Targets, BinderType).CallReflected(context, CallType.None, args);
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

        public object CallHelper(CodeContext context, object[] args, string[] names, object instance) {
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
                object ret;
                if (CompilerHelpers.IsStatic(bestTarget)) {
                    ret = MethodBinder.MakeBinder(context.LanguageContext.Binder, Name, new MethodBase[] { bestTarget }, BinderType.Normal).CallReflected(context, CallType.None, bestArgs);
                } else {
                    ret = MethodBinder.MakeBinder(context.LanguageContext.Binder, Name, new MethodBase[] { bestTarget }, BinderType.Normal).
                        CallReflected(context, CallType.ImplicitInstance, Utils.Array.Insert(instance, bestArgs));
                }

                // any unbound arguments left over we assume the user
                // wants to do a property set with.  We'll go ahead and try
                // that - if they fail we'll throw.
                if (bestUnboundArgs != null) {
                    // if we had a constructor w/ a ref param then we'll try
                    // updating the Tuple here instead of the user's object.

                    if (!DynamicHelpers.GetDynamicTypeFromType(_targets[0].DeclaringType).AllowConstructorArguments(context.LanguageContext.ContextId)) {
                        // calling ctor w/ kw-args w/ zero args, let it go, don't do any sets.
                        if (args.Length == names.Length) return ret;

                        throw RuntimeHelpers.SimpleTypeError(String.Format("'{0}' is an invalid keyword argument for this function",
                            bestUnboundArgs[0].Name,
                            Name));
                    }

                    for (int j = 0; j < bestUnboundArgs.Count; j++) {                   
                        context.LanguageContext.SetMember(context, 
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

            throw RuntimeHelpers.SimpleTypeError(String.Format("bad number of arguments for function {0}", FriendlyName));
        }

        /// <summary>
        /// Returns a BuiltinFunction bound to the provided type arguments.  Returns null if the binding
        /// cannot be performed.
        /// </summary>
        public BuiltinFunction MakeGenericMethod(Type[] types) {
            TypeList tl = new TypeList(types);

            // check for cached method first...
            BuiltinFunction bf;
            if (_boundGenerics != null) {
                lock(_boundGenerics) {
                    if (_boundGenerics.TryGetValue(tl, out bf)) {
                        return bf;
                    }
                }
            }

            // Search for generic targets with the correct arity (number of type parameters).
            // Compatible targets must be MethodInfos by definition (constructors never take
            // type arguments).
            List<MethodBase> targets = new List<MethodBase>(Targets.Length);            
            foreach (MethodBase mb in Targets) {
                MethodInfo mi = mb as MethodInfo;
                if (mi == null)
                    continue;
                if (mi.ContainsGenericParameters && mi.GetGenericArguments().Length == types.Length)
                    targets.Add(mi.MakeGenericMethod(types));
            }

            if (targets.Count == 0) {
                return null;
            }

            // Build a new ReflectedMethod that will contain targets with bound type arguments & cache it.
            bf = new BuiltinFunction(Name, targets.ToArray(), FunctionType);

            EnsureBoundGenericDict();

            lock (_boundGenerics) {
                _boundGenerics[tl] = bf;
            }

            return bf;
        }

        private void EnsureBoundGenericDict() {
            if (_boundGenerics == null) {
                Interlocked.CompareExchange<Dictionary<TypeList, BuiltinFunction>>(
                    ref _boundGenerics,
                    new Dictionary<TypeList, BuiltinFunction>(1),
                    null);
            }
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
            set {
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
        public DynamicType DeclaringType {
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

            if (context.ModuleContext.ShowCls) {
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

        #region DynamicTypeSlot Overrides

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = this;
            return true;
        }

        #endregion
        
        private class TypeList {
            private Type[] _types;

            public TypeList(Type[] types) {
                Debug.Assert(types != null);
                _types = types;
            }

            public override bool Equals(object obj) {
                TypeList tl = obj as TypeList;
                if (tl == null || _types.Length != tl._types.Length) return false;

                for (int i = 0; i < _types.Length; i++) {
                    if (_types[i] != tl._types[i]) return false;
                }
                return true;
            }

            public override int GetHashCode() {
                int hc = 6551;
                foreach (Type t in _types) {
                    hc = (hc << 5) ^ t.GetHashCode();
                }
                return hc;
            }
        }
    }

    public sealed class BuiltinMethodDescriptor : DynamicTypeSlot, IFancyCallable, IContextAwareMember, ICallableWithCodeContext {
        internal BuiltinFunction template;

        public BuiltinMethodDescriptor(BuiltinFunction function) {
            template = function;
        }

        public object UncheckedGetAttribute(object instance) {
            if (instance == null) return this;
            return new BoundBuiltinFunction(template, instance);
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            if (instance != null) {
                CheckSelf(context, instance);
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
                return String.Format("<method '{0}' of '{1}' objects>", Name, dt.Name);
            } else {
                return String.Format("<method '{0}' of '{1}' objects>", Name, "<unknown>");
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
            CheckSelfInArgs(context, args);
            return template.Call(context, args, names);
        }

        #endregion

        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, object[] args) {
            CheckSelfInArgs(context, args);
            return template.Call(context, args);
        }

        #endregion

        private void CheckSelfInArgs(CodeContext context, object[] args) {
            if (args.Length == 0)
                throw RuntimeHelpers.SimpleTypeError(String.Format("descriptor '{0}' of '{1}' needs an argument",
                    Name,
                    DeclaringType.Name));

            CheckSelf(context, args[0]);
        }

        private void CheckSelf(CodeContext context, object self) {
            // if the type has been re-optimized (we also have base type info in here) 
            // then we can't do the type checks right now :(.
            if ((template.FunctionType & FunctionType.SkipThisCheck) != 0)
                return;

            if ((template.FunctionType & FunctionType.FunctionMethodMask) == FunctionType.Method) {
                CheckSelfWorker(context, self, template);
            }
        }

        public static void CheckSelfWorker(CodeContext context, object self, BuiltinFunction template) {
            // to a fast check on the CLR types, if they match we can avoid the slower
            // check that involves looking up dynamic types. (self can be null on
            // calls like set.add(None) 
            if (self != null && self.GetType() == template.ClrDeclaringType) return;

            DynamicType selfType = self == null ? DynamicType.NullType : DynamicHelpers.GetDynamicTypeFromType(self.GetType());
            Debug.Assert(selfType != null);

            DynamicType declType = template.DeclaringType;
            if (!selfType.IsSubclassOf(declType)) {
                // if a conversion exists to the type allow the call.
                context.LanguageContext.Binder.Convert(self, declType.UnderlyingSystemType);
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

        public override object Call(CodeContext context, params object[] args) {
            return _target.CallInstance(context, _instance, args);
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

        public MethodBase[] ConstructorTargets {
            get {
                return ctors;
            }
        }
    }
}