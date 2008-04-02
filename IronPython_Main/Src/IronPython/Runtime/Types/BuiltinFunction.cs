/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    using Ast = Microsoft.Scripting.Ast.Ast;

    /// <summary>
    /// BuiltinFunction represents any standard CLR function exposed to Python.
    /// This is used for both methods on standard Python types such as list or tuple
    /// and for methods from arbitrary .NET assemblies.
    /// 
    /// All calls are made through the optimizedTarget which is created lazily.
    /// 
    /// TODO: Back BuiltinFunction's by MethodGroup's.
    /// </summary>    
    [PythonSystemType("builtin_function_or_method")]
    public class BuiltinFunction :
        PythonTypeSlot, IDynamicObject, ICodeFormattable {
        private string _name;
        private MethodBase[] _targets;
        private FunctionType _funcType;
        private Dictionary<TypeList, BuiltinFunction> _boundGenerics;
        private int _id;        
        [MultiRuntimeAware]
        private static int _curId;

        #region Static factories

        internal static BuiltinFunction MakeMethod(string name, MethodBase info, FunctionType ft) {
            return new BuiltinFunction(name, new MethodBase[] { info }, ft);
        }

        internal static BuiltinFunction MakeMethod(string name, MethodBase[] infos, FunctionType ft) {
            return new BuiltinFunction(name, infos, ft);
        }

        internal static BuiltinFunction MakeOrAdd(BuiltinFunction existing, string name, MethodBase mi, FunctionType funcType) {
            if (existing != null) {
                existing.AddMethod(mi);
                return existing;
            } else {
                return MakeMethod(name, mi, funcType);
            }
        }

        #endregion

        #region Constructors

        internal BuiltinFunction() {
            _id = Interlocked.Increment(ref _curId);
        }

        internal BuiltinFunction(string name, FunctionType functionType)
            : this() {
            Assert.NotNull(name);
            
            _name = name;
            _funcType = functionType;
        }

        internal BuiltinFunction(IList<MethodBase> targets) {
            _targets = ArrayUtils.ToArray(targets);
        }

        private BuiltinFunction(string name, MethodBase[] originalTargets, FunctionType functionType) : this() {
            Assert.NotNull(name);
            Assert.NotNullItems(originalTargets);

            _funcType = functionType;
            _targets = originalTargets;
            _name = name;
        }

        #endregion

        #region Public API Surface

        public int Id {
            get {
                return _id;
            }
        }

        #endregion

        #region Internal API Surface

        internal string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        internal void AddMethod(MethodBase info) {
            Assert.NotNull(info);

            if (_targets != null) {
                MethodBase[] ni = new MethodBase[_targets.Length + 1];
                _targets.CopyTo(ni, 0);
                ni[_targets.Length] = info;
                _targets = ni;
            } else {
                _targets = new MethodBase[] { info };
            }
        }

        internal object CallHelper(CodeContext context, object[] args, string[] names) {
            return CallHelper(context, args, names, null);
        }

        internal object CallHelper(CodeContext context, object[] args, string[] names, object instance) {
            MethodBinder mb = MethodBinder.MakeBinder(context.LanguageContext.Binder, Name, _targets, SymbolTable.StringsToIds(names));
            BindingTarget bt = mb.MakeBindingTarget(instance == null ? CallTypes.None : CallTypes.ImplicitInstance, CompilerHelpers.GetTypes(args));

            if (bt.Success) {
                return bt.Call(context, args);
            } else if (IsBinaryOperator && args.Length == 2) {
                return PythonOps.NotImplemented;
            }
            
            // TODO: Could do better than this to report error messages
            if (instance != null) {
                return mb.CallInstanceReflected(context, instance, args);
            } else {
                return mb.CallReflected(context, CallTypes.None, args);
            }
        }

        /// <summary>
        /// Returns a BuiltinFunction bound to the provided type arguments.  Returns null if the binding
        /// cannot be performed.
        /// </summary>
        internal BuiltinFunction MakeGenericMethod(Type[] types) {
            TypeList tl = new TypeList(types);

            // check for cached method first...
            BuiltinFunction bf;
            if (_boundGenerics != null) {
                lock (_boundGenerics) {
                    if (_boundGenerics.TryGetValue(tl, out bf)) {
                        return bf;
                    }
                }
            }

            // Search for generic targets with the correct arity (number of type parameters).
            // Compatible targets must be MethodInfos by definition (constructors never take
            // type arguments).
            List<MethodBase> targets = new List<MethodBase>(Targets.Count);
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

        internal void DictArgsHelper(IDictionary<object, object> dictArgs, object[] args, out object[] realArgs, out string[] argNames) {
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
        internal PythonTypeSlot GetDescriptor() {
            if ((FunctionType & FunctionType.Method) != 0) {
                return new BuiltinMethodDescriptor(this);
            }
            return this;
        }

        internal ContextId Context {
            get {
                return ContextId.Empty;
            }
        }

        internal Type DeclaringType {
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // TODO: fix
        internal IList<MethodBase> Targets {
            get {
                return _targets;
            }
        }

        /// <summary>
        /// True if the method should be visible to non-CLS opt-in callers
        /// </summary>
        internal bool IsPythonVisible {
            get {
                return (_funcType & FunctionType.AlwaysVisible) != 0;
            }
        }

        internal bool IsReversedOperator {
            get {
                return (FunctionType & FunctionType.ReversedOperator) != 0;
            }
        }

        internal bool IsBinaryOperator {
            get {
                return (FunctionType & FunctionType.BinaryOperator) != 0;
            }
        }

        internal FunctionType FunctionType {
            get {
                return _funcType;
            }
            set {
                _funcType = value;
            }
        }

        #endregion

        #region IContextAwareMember Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        internal override bool IsVisible(CodeContext context, PythonType owner) {
            Debug.Assert(context != null);

            if (context.ModuleContext.ShowCls) {
                return true;
            }

            return IsPythonVisible;
        }

        #endregion

        #region PythonTypeSlot Overrides

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = this;
            return true;
        }

        #endregion                

        #region ICodeFormattable members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<built-in function {0}>", Name);
        }

        #endregion

        #region IDynamicObject Members

        LanguageContext IDynamicObject.LanguageContext {
            get {
                return DefaultContext.Default.LanguageContext;
            }
        }

        RuleBuilder<T> IDynamicObject.GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            switch(action.Kind) {
                case DynamicActionKind.Call: return MakeCallRule<T>((CallAction)action, context, args);
                case DynamicActionKind.DoOperation: return MakeDoOperationRule<T>((DoOperationAction)action, context, args);
            }

            return null;
        }

        private RuleBuilder<T> MakeDoOperationRule<T>(DoOperationAction doOperationAction, CodeContext context, object[] args) {
            switch(doOperationAction.Operation) {
                case Operators.CallSignatures:
                    return PythonDoOperationBinderHelper<T>.MakeCallSignatureRule(context.LanguageContext.Binder, Targets, DynamicHelpers.GetPythonType(args[0]));
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, this, true);
            }
            return null;
        }

        internal NarrowingLevel Level {
            get {
                return IsBinaryOperator ? PythonNarrowing.BinaryOperator : NarrowingLevel.All;
            }
        }   
        private RuleBuilder<T> MakeCallRule<T>(CallAction action, CodeContext context, object[]args) {
            CallBinderHelper<T, CallAction> helper = new CallBinderHelper<T, CallAction>(context, action, args, Targets, Level, IsReversedOperator);
            RuleBuilder<T> rule = helper.MakeRule();
            if (IsBinaryOperator && rule.IsError && args.Length == 3) { // 1 function + 2 args
                // BinaryOperators return NotImplemented on failure.
                rule.Target = rule.MakeReturn(context.LanguageContext.Binder, Ast.ReadField(null, typeof(PythonOps), "NotImplemented"));
            }
            rule.AddTest(MakeFunctionTest(rule.Parameters[0]));
            return rule;
        }

        internal Expression MakeFunctionTest(Expression functionTarget) {
            return Ast.Equal(
                Ast.ReadProperty(
                    Ast.Convert(functionTarget, typeof(BuiltinFunction)),
                    typeof(BuiltinFunction).GetProperty("Id")
                ),
                Ast.Constant(Id)
            );
        }

        #endregion

        #region Public Python APIs

        [PropertyMethod]
        public string Get__module__(CodeContext/*!*/ context) {
            if (Targets.Count > 0) {
                PythonType declaringType = DynamicHelpers.GetPythonTypeFromType(DeclaringType);

                return PythonTypeOps.GetModuleName(context, declaringType.UnderlyingSystemType);
            }
            return null;
        }

        /// <summary>
        /// Provides (for reflected methods) a mapping from a signature to the exact target
        /// which takes this signature.
        /// signature with syntax like the following:
        ///    someClass.SomeMethod.Overloads[str, int]("Foo", 123)
        /// </summary>
        public virtual BuiltinFunctionOverloadMapper Overloads {
            get {
                // The mapping is actually provided by a class rather than a dictionary
                // since it's hard to generate all the keys of the signature mapping when
                // two type systems are involved.  Creating the mapping object is quite
                // cheap so we don't cache a copy.
                return new BuiltinFunctionOverloadMapper(this, null);
            }
        }

        public string func_name {
            get {
                return Name;
            }
        }

        public string __name__ {
            get {
                return Name;
            }
        }

        public virtual string __doc__ {
            get {
                StringBuilder sb = new StringBuilder();
                IList<MethodBase> targets = Targets;
                bool needNewLine = false;
                for (int i = 0; i < targets.Count; i++) {
                    if (targets[i] != null) AddDocumentation(sb, ref needNewLine, targets[i]);
                }
                return sb.ToString();
            }
        }

        public static object __self__ {
            get {
                return null;
            }
        }

        public object __call__(CodeContext context, [ParamDictionary]IDictionary<object, object> dictArgs, params object[] args) {
            object[] realArgs;
            string[] argNames;
            DictArgsHelper(dictArgs, args, out realArgs, out argNames);

            return CallHelper(context, realArgs, argNames, null);
        }

        public BuiltinFunction/*!*/ this[PythonTuple tuple] {
            get {
                return this[tuple._data];
            }
        }

        /// <summary>
        /// Use indexing on generic methods to provide a new reflected method with targets bound with
        /// the supplied type arguments.
        /// </summary>
        public BuiltinFunction/*!*/ this[params object[] key] {
            get {
                // Retrieve the list of type arguments from the index.
                Type[] types = new Type[key.Length];
                for (int i = 0; i < types.Length; i++) {
                    types[i] = Converter.ConvertToType(key[i]);
                }

                BuiltinFunction res = MakeGenericMethod(types);
                if (res == null) {
                    throw PythonOps.TypeError(string.Format("bad type args to this generic method {0}", this.Name));
                }

                return res;
            }
        }
        
        #endregion

        #region Private members

        private void AddDocumentation(StringBuilder sb, ref bool nl, MethodBase mb) {
            if (nl) {
                sb.Append(System.Environment.NewLine);
            }
            sb.Append(DocBuilder.DocOneInfo(mb, Name));
            nl = true;
        }

        private BinderType BinderType {
            get {
                return IsBinaryOperator ? BinderType.BinaryOperator : BinderType.Normal;
            }
        }

        private void EnsureBoundGenericDict() {
            if (_boundGenerics == null) {
                Interlocked.CompareExchange<Dictionary<TypeList, BuiltinFunction>>(
                    ref _boundGenerics,
                    new Dictionary<TypeList, BuiltinFunction>(1),
                    null);
            }
        }

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

        #endregion
    }
}

