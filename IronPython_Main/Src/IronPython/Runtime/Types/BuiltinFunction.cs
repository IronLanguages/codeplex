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

using System; using Microsoft;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting;
using System.Text;
using System.Threading;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

using Ast = Microsoft.Linq.Expressions.Expression;

namespace IronPython.Runtime.Types {

    /// <summary>
    /// BuiltinFunction represents any standard CLR function exposed to Python.
    /// This is used for both methods on standard Python types such as list or tuple
    /// and for methods from arbitrary .NET assemblies.
    /// 
    /// All calls are made through the optimizedTarget which is created lazily.
    /// 
    /// TODO: Back BuiltinFunction's by MethodGroup's.
    /// </summary>    
    [PythonType("builtin_function_or_method")]
    public class BuiltinFunction : PythonTypeSlot, ICodeFormattable, IDynamicObject, IDelegateConvertible {
        private readonly BuiltinFunctionData/*!*/ _data;            // information describing the BuiltinFunction
        private readonly object _instance;                          // the bound instance or null if unbound
        private static readonly object _noInstance = new object();  

        #region Static factories

        internal static BuiltinFunction/*!*/ MakeMethod(string name, MethodBase info, Type declaringType, FunctionType ft) {
            return new BuiltinFunction(name, new MethodBase[] { info }, declaringType, ft);
        }

        internal static BuiltinFunction/*!*/ MakeMethod(string name, MethodBase[] infos, Type declaringType, FunctionType ft) {
            return new BuiltinFunction(name, infos, declaringType, ft);
        }

        internal static BuiltinFunction/*!*/ MakeOrAdd(BuiltinFunction existing, string name, MethodBase mi, Type declaringType, FunctionType funcType) {
            PythonBinder.AssertNotExtensionType(declaringType);

            if (existing != null) {
                existing._data.AddMethod(mi);
                return existing;
            } else {
                return MakeMethod(name, mi, declaringType, funcType);
            }
        }

        internal BuiltinFunction/*!*/ BindToInstance(object instance) {
            return new BuiltinFunction(instance, _data);
        }

        #endregion

        #region Constructors

        internal BuiltinFunction(string/*!*/ name, MethodBase/*!*/[]/*!*/ originalTargets, Type/*!*/ declaringType, FunctionType functionType) {
            Assert.NotNull(name);
            Assert.NotNull(declaringType);
            Assert.NotNullItems(originalTargets);

            _data = new BuiltinFunctionData(name, originalTargets, declaringType, functionType);
            _instance = _noInstance;
        }

        /// <summary>
        /// Creates a bound built-in function.  The instance may be null for built-in functions
        /// accessed for None.
        /// </summary>
        private BuiltinFunction(object instance, BuiltinFunctionData/*!*/ data) {
            Assert.NotNull(data);

            _instance = instance;
            _data = data;
        }

        #endregion

        #region Internal API Surface

        internal void AddMethod(MethodInfo mi) {
            _data.AddMethod(mi);
        }

        internal bool TestData(object data) {
            return _data == data;
        }

        internal bool IsUnbound {
            get {
                return _instance == _noInstance;
            }
        }

        internal string Name {
            get {
                return _data.Name;
            }
            set {
                _data.Name = value;
            }
        }

        internal object Call(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage, object instance, object[] args) {
            storage = GetInitializedStorage(context, storage);
            
            object callable;
            if (!GetDescriptor().TryGetBoundValue(context, instance, DynamicHelpers.GetPythonTypeFromType(DeclaringType), out callable)) {
                callable = this;
            }

            return storage.Data.Target(storage.Data, context, callable, args);
        }

        private static SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> GetInitializedStorage(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage) {
            if (storage == null) {
                storage = PythonContext.GetContext(context).GetGenericCallSiteStorage();
            }

            if (storage.Data == null) {
                storage.Data = PythonContext.GetContext(context).MakeSplatSite();
            }
            return storage;
        }

        internal object Call(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], IAttributesCollection, object>>> storage, object instance, object[] args, IAttributesCollection keywordArgs) {
            if (storage == null) {
                storage = PythonContext.GetContext(context).GetGenericKeywordCallSiteStorage();
            }

            if (storage.Data == null) {
                storage.Data = PythonContext.GetContext(context).MakeKeywordSplatSite();
            }

            if (instance != null) {
                return storage.Data.Target(storage.Data, context, this, ArrayUtils.Insert(instance, args), keywordArgs);
            }

            return storage.Data.Target(storage.Data, context, this, args, keywordArgs);
        }
        
        /// <summary>
        /// Returns a BuiltinFunction bound to the provided type arguments.  Returns null if the binding
        /// cannot be performed.
        /// </summary>
        internal BuiltinFunction MakeGenericMethod(Type[] types) {
            TypeList tl = new TypeList(types);

            // check for cached method first...
            BuiltinFunction bf;
            if (_data.BoundGenerics != null) {
                lock (_data.BoundGenerics) {
                    if (_data.BoundGenerics.TryGetValue(tl, out bf)) {
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
            bf = new BuiltinFunction(Name, targets.ToArray(), DeclaringType, FunctionType);

            EnsureBoundGenericDict();

            lock (_data.BoundGenerics) {
                _data.BoundGenerics[tl] = bf;
            }

            return bf;
        }

        internal void DictArgsHelper(IDictionary<object, object> dictArgs, object[] args, out object[] realArgs, out string[] argNames) {
            realArgs = ArrayOps.CopyArray(args, args.Length + dictArgs.Count);
            argNames = new string[dictArgs.Count];

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
        internal PythonTypeSlot/*!*/ GetDescriptor() {
            if ((FunctionType & FunctionType.Method) != 0) {
                return new BuiltinMethodDescriptor(this);
            }
            return this;
        }

        internal Type DeclaringType {
            get {
                return _data.DeclaringType;
            }
        }

        /// <summary>
        /// Gets the target methods that we'll be calling.  
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // TODO: fix
        internal IList<MethodBase> Targets {
            get {
                return _data.Targets;
            }
        }

        /// <summary>
        /// True if the method should be visible to non-CLS opt-in callers
        /// </summary>
        internal override bool IsAlwaysVisible {
            get {
                return (_data.Type & FunctionType.AlwaysVisible) != 0;
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
                return _data.Type;
            }
            set {
                _data.Type = value;
            }
        }

        /// <summary>
        /// Makes a test for the built-in function against the private _data 
        /// which is unique per built-in function.
        /// </summary>
        internal Expression/*!*/ MakeBoundFunctionTest(Expression/*!*/ functionTarget) {
            Debug.Assert(functionTarget.Type == typeof(BuiltinFunction));

            return Ast.Call(
                typeof(PythonOps).GetMethod("TestBoundBuiltinFunction"),
                functionTarget,
                Ast.Constant(_data, typeof(object))
            );
        }
        
        #endregion

        #region PythonTypeSlot Overrides

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = this;
            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        internal override Expression/*!*/ MakeGetExpression(PythonBinder/*!*/ binder, Expression/*!*/ codeContext, Expression instance, Expression/*!*/ owner, Expression/*!*/ error) {
            return Ast.Constant(this);
        }

        #endregion                

        #region ICodeFormattable members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            if (IsUnbound) {
                return string.Format("<built-in function {0}>", Name);
            }

            return string.Format("<built-in method {0} of {1} object at {2}>",
                __name__,
                PythonOps.GetPythonTypeName(__self__),
                PythonOps.HexId(__self__));
        }

        #endregion

        #region IDynamicObject Members

        MetaObject/*!*/ IDynamicObject.GetMetaObject(Expression/*!*/ parameter) {
            return new Binding.MetaBuiltinFunction(parameter, Restrictions.Empty, this);
        }

        #endregion
                        
        #region Public Python APIs

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cls")]
        public static object/*!*/ __new__(object cls, object newFunction, object inst) {
            return new Method(newFunction, inst, null);
        }

        public int __cmp__(CodeContext/*!*/ context, [NotNull]BuiltinFunction/*!*/  other) {
            if (other == this) {
                return 0;
            }

            if (!IsUnbound && !other.IsUnbound) {
                int result = PythonOps.Compare(__self__, other.__self__);
                if (result != 0) {
                    return result;
                }

                if (_data == other._data) {
                    return 0;
                }
            }

            int res = String.CompareOrdinal(__name__, other.__name__);
            if (res != 0) {
                return res;
            }

            res = String.CompareOrdinal(Get__module__(context), other.Get__module__(context));
            if (res != 0) {
                return res;
            }
            
            long lres = IdDispenser.GetId(this) - IdDispenser.GetId(other);
            return lres > 0 ? 1 : -1;
        }

        public int __hash__(CodeContext/*!*/ context) {
            return PythonOps.Hash(context, _instance) ^ PythonOps.Hash(context, _data);
        }

        [SpecialName, PropertyMethod]
        public string Get__module__(CodeContext/*!*/ context) {
            if (Targets.Count > 0) {
                PythonType declaringType = DynamicHelpers.GetPythonTypeFromType(DeclaringType);

                return PythonTypeOps.GetModuleName(context, declaringType.UnderlyingSystemType);
            }
            return null;
        }

        [SpecialName, PropertyMethod]
        public void Set__module__(string value) {
            // Do nothing but don't return an error
        }

        /// <summary>
        /// Provides (for reflected methods) a mapping from a signature to the exact target
        /// which takes this signature.
        /// signature with syntax like the following:
        ///    someClass.SomeMethod.Overloads[str, int]("Foo", 123)
        /// </summary>
        public virtual BuiltinFunctionOverloadMapper Overloads {
            [PythonHidden]
            get {
                // The mapping is actually provided by a class rather than a dictionary
                // since it's hard to generate all the keys of the signature mapping when
                // two type systems are involved.  Creating the mapping object is quite
                // cheap so we don't cache a copy.
                return new BuiltinFunctionOverloadMapper(this, IsUnbound ? null : _instance);
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

        public object __self__ {
            get {
                if (IsUnbound) {
                    return null;
                }

                return _instance;
            }
        }

        public object __call__(CodeContext/*!*/ context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], IAttributesCollection, object>>> storage, [ParamDictionary]IAttributesCollection dictArgs, params object[] args) {
            return Call(context, storage, null, args, dictArgs);
        }

        public BuiltinFunction/*!*/ this[PythonTuple tuple] {
            [PythonHidden]
            get {
                return this[tuple._data];
            }
        }

        /// <summary>
        /// Use indexing on generic methods to provide a new reflected method with targets bound with
        /// the supplied type arguments.
        /// </summary>
        public BuiltinFunction/*!*/ this[params object[] key] {
            [PythonHidden]
            get {
                // Retrieve the list of type arguments from the index.
                Type[] types = new Type[key.Length];
                for (int i = 0; i < types.Length; i++) {
                    types[i] = Converter.ConvertToType(key[i]);
                }

                BuiltinFunction res = MakeGenericMethod(types);
                if (res == null) {
                    bool hasGenerics = false;
                    foreach (MethodBase mb in Targets) {
                        MethodInfo mi = mb as MethodInfo;
                        if (mi != null && mi.ContainsGenericParameters) {
                            hasGenerics = true;
                        }
                    }

                    if (hasGenerics) {
                        throw PythonOps.TypeError(string.Format("bad type args to this generic method {0}", Name));
                    } else {
                        throw PythonOps.TypeError(string.Format("{0} is not a generic method and is unsubscriptable", Name));
                    }
                }

                if (IsUnbound) {
                    return res;
                }

                return new BuiltinFunction(_instance, res._data);
            }
        }

        internal bool IsOnlyGeneric {
            get {
                foreach (MethodBase mb in Targets) {
                    if (!mb.IsGenericMethod || !mb.ContainsGenericParameters) {
                        return false;
                    }
                }

                return true;
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
            if (_data.BoundGenerics == null) {
                Interlocked.CompareExchange<Dictionary<TypeList, BuiltinFunction>>(
                    ref _data.BoundGenerics,
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

        #region IDelegateConvertible Members

        Delegate IDelegateConvertible.ConvertToDelegate(Type type) {
            // see if we have any functions which are compatible with the delegate type...
            ParameterInfo[] delegateParams = type.GetMethod("Invoke").GetParameters();

            // if we have overloads then we need to do the overload resolution at runtime
            if (Targets.Count == 1) {
                MethodInfo mi = Targets[0] as MethodInfo;
                if (mi != null) {
                    ParameterInfo[] methodParams = mi.GetParameters();
                    if (methodParams.Length == delegateParams.Length) {
                        bool match = true;
                        for (int i = 0; i < methodParams.Length; i++) {
                            if (delegateParams[i].ParameterType != methodParams[i].ParameterType) {
                                match = false;
                                break;
                            }
                        }

                        if (match) {
                            if (IsUnbound) {
                                return Delegate.CreateDelegate(type, mi);
                            } else {
                                return Delegate.CreateDelegate(type, _instance, mi);
                            }
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region BuiltinFunctionData

        private sealed class BuiltinFunctionData {
            public string/*!*/ Name;
            public MethodBase/*!*/[]/*!*/ Targets;
            public readonly Type/*!*/ DeclaringType;
            public FunctionType Type;
            public Dictionary<TypeList, BuiltinFunction> BoundGenerics;

            public BuiltinFunctionData(string name, MethodBase[] targets, Type declType, FunctionType functionType) {
                Name = name;
                Targets = targets;
                DeclaringType = declType;
                Type = functionType;
            }

            internal void AddMethod(MethodBase/*!*/ info) {
                Assert.NotNull(info);

                MethodBase[] ni = new MethodBase[Targets.Length + 1];
                Targets.CopyTo(ni, 0);
                ni[Targets.Length] = info;
                Targets = ni;
            }
        }

        #endregion
    }
}

