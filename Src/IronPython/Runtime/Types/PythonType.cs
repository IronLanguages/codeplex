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
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using System.Text;
using System.Threading;
using IronPython.Compiler;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    /// <summary>
    /// Represents a PythonType.  Instances of PythonType are created via PythonTypeBuilder.  
    /// </summary>
#if !SILVERLIGHT
    [DebuggerDisplay("PythonType: {Name}")]
#endif
    [PythonSystemType("type")]
    public class PythonType : IMembersList, IDynamicObject, IOldDynamicObject, IWeakReferenceable, ICodeFormattable {
        private readonly Type/*!*/ _underlyingSystemType;   // the underlying CLI system type for this type
        private string _name;                               // the name of the type
        private Dictionary<SymbolId, PythonTypeSlot> _dict; // type-level slots & attributes
        private PythonTypeAttributes _attrs;                // attributes of the type
        private int _version = GetNextVersion();            // version of the type
        private List<WeakReference> _subtypes;              // all of the subtypes of the PythonType
        private PythonContext _pythonContext;               // the context the type was created from, or null for system types.

        // commonly calculatable
        private List<PythonType> _resolutionOrder;          // the search order for methods in the type
        private List<PythonType>/*!*/ _bases;               // the base classes of the type
        private object _ctor;                               // fast implementation of ctor

        // fields that frequently remain null
        private WeakRefTracker _weakrefTracker;             // storage for Python style weak references
        private OldClass _oldClass;                         // the associated OldClass or null for new-style types        
        private CallSite<DynamicSiteTarget<CodeContext, object, object>> _newDirSite;
        private CallSite<DynamicSiteTarget<CodeContext, object, object[], object>> _newCtorSite;
        private CallSite<DynamicSiteTarget<CodeContext, object, string, object>> _newGetattributeSite;
        private CallSite<DynamicSiteTarget<CodeContext, object, object, string, object, object>> _newSetattrSite;

        [MultiRuntimeAware]
        private static int MasterVersion = 1;
        private static readonly Dictionary<Type, PythonType> _pythonTypes = new Dictionary<Type, PythonType>();
        internal static PythonType _pythonTypeType = DynamicHelpers.GetPythonTypeFromType(typeof(PythonType));
        private static readonly WeakReference[] _emptyWeakRef = new WeakReference[0];

        /// <summary>
        /// Creates a new type for a user defined type.  The name, base classes (a tuple of type
        /// objects), and a dictionary of members is provided.
        /// </summary>
        public PythonType(CodeContext/*!*/ context, string name, PythonTuple bases, IAttributesCollection dict) {
            _underlyingSystemType = NewTypeMaker.GetNewType(name, bases, dict);

            InitializeUserType(context, name, bases, dict);
        }

        /// <summary>
        /// Creates a new PythonType object which is backed by the specified .NET type for
        /// storage.  The type is considered a system type which can not be modified
        /// by the user.
        /// </summary>
        /// <param name="underlyingSystemType"></param>
        internal PythonType(Type underlyingSystemType) {
            _bases = new List<PythonType>(1);
            _underlyingSystemType = underlyingSystemType;

            InitializeSystemType();
        }

        /// <summary>
        /// Creates a new PythonType which is a subclass of the specified PythonType.
        /// 
        /// Used for runtime defined new-style classes which require multiple inheritance.  The
        /// primary example of this is the exception system.
        /// </summary>
        internal PythonType(PythonType baseType, string name) {
            _underlyingSystemType = baseType.UnderlyingSystemType;

            IsSystemType = baseType.IsSystemType;
            IsPythonType = baseType.IsPythonType;
            Name = name;
            _bases = new List<PythonType>(1);
            _bases.Add(baseType);
            ResolutionOrder = Mro.Calculate(this, new PythonType[] { baseType });
        }

        /// <summary>
        /// Creates a new PythonType object which represents an Old-style class.
        /// </summary>
        internal PythonType(OldClass oc) {
            EnsureDict();

            _underlyingSystemType = typeof(OldInstance);
            Name = oc.Name;
            OldClass = oc;

            List<PythonType> ocs = new List<PythonType>(oc.BaseClasses.Count);
            foreach (OldClass klass in oc.BaseClasses) {
                ocs.Add(klass.TypeObject);
            }

            List<PythonType> mro = new List<PythonType>();
            mro.Add(this);

            _bases = ocs; 
            _resolutionOrder = mro;
            AddSlot(Symbols.Class, new PythonTypeValueSlot(this));
        }

        #region Public API
        
        public static object __new__(CodeContext/*!*/ context, PythonType cls, string name, PythonTuple bases, IAttributesCollection dict) {
            if (name == null) {
                throw PythonOps.TypeError("type() argument 1 must be string, not None");
            }
            if (bases == null) {
                throw PythonOps.TypeError("type() argument 2 must be tuple, not None");
            }
            if (dict == null) {
                throw PythonOps.TypeError("TypeError: type() argument 3 must be dict, not None");
            }

            EnsureModule(context, dict);

            PythonType meta = FindMetaClass(cls, bases);

            if (meta != TypeCache.OldInstance && meta != TypeCache.PythonType) {
                if (meta != cls) {
                    // the user has a custom __new__ which picked the wrong meta class, call the correct metaclass
                    return PythonCalls.Call(context, meta, name, bases, dict);
                }

                // we have the right user __new__, call our ctor method which will do the actual
                // creation.                   
                return meta.CreateInstance(context, name, bases, dict);
            }

            // no custom user type for __new__
            return new PythonType(context, name, bases, dict);
        }

        internal static PythonType FindMetaClass(PythonType cls, PythonTuple bases) {
            PythonType meta = cls;
            foreach (object dt in bases) {
                PythonType metaCls = DynamicHelpers.GetPythonType(dt);

                if (metaCls == TypeCache.OldClass) continue;

                if (meta.IsSubclassOf(metaCls)) continue;

                if (metaCls.IsSubclassOf(meta)) {
                    meta = metaCls;
                    continue;
                }
                throw PythonOps.TypeError("metaclass conflict {0} and {1}", metaCls.Name, meta.Name);
            }
            return meta;
        }

        public static object __new__(CodeContext/*!*/ context, object cls, object o) {
            return DynamicHelpers.GetPythonType(o);
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static PythonTuple Get__bases__(CodeContext/*!*/ context, PythonType/*!*/ type) {
            object[] res = new object[type.BaseTypes.Count];
            IList<PythonType> bases = type.BaseTypes;
            for (int i = 0; i < bases.Count; i++) {
                PythonType baseType = bases[i];

                if (baseType.IsOldClass) {
                    res[i] = baseType.OldClass;
                } else {
                    res[i] = baseType;
                }
            }

            return PythonTuple.MakeTuple(res);
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static void Set__bases__(CodeContext/*!*/ context, PythonType/*!*/ type, object value) {
            // validate we got a tuple...           
            PythonTuple t = value as PythonTuple;
            if (t == null) throw PythonOps.TypeError("expected tuple of types or old-classes, got {0}", PythonOps.StringRepr(PythonTypeOps.GetName(value)));

            List<PythonType> ldt = new List<PythonType>();

            foreach (object o in t) {
                // gather all the type objects...
                PythonType adt = o as PythonType;
                if (adt == null) {
                    OldClass oc = o as OldClass;
                    if (oc == null) {
                        throw PythonOps.TypeError("expected tuple of types, got {0}", PythonOps.StringRepr(PythonTypeOps.GetName(o)));
                    }

                    adt = oc.TypeObject;
                }

                ldt.Add(adt);
            }

            // Ensure that we are not switching the CLI type
            Type newType = NewTypeMaker.GetNewType(type.Name, t, type.GetMemberDictionary(DefaultContext.Default));
            if (type.UnderlyingSystemType != newType)
                throw PythonOps.TypeErrorForIncompatibleObjectLayout("__bases__ assignment", type, newType);

            // set bases & the new resolution order
            List<PythonType> mro = CalculateMro(type, ldt);

            type.BaseTypes = ldt;
            type._resolutionOrder = mro;
        }

        private static List<PythonType> CalculateMro(PythonType type, IList<PythonType> ldt) {
            List<PythonType> mro = Mro.Calculate(type, ldt);
            for (int i = 0; i < mro.Count; i++) {
                Type newType;
                if (TryReplaceExtensibleWithBase(mro[i].UnderlyingSystemType, out newType)) {
                    mro[i] = DynamicHelpers.GetPythonTypeFromType(newType);
                }
            }
            return mro;
        }

        private static bool TryReplaceExtensibleWithBase(Type curType, out Type newType) {
            if (curType.IsGenericType &&
                curType.GetGenericTypeDefinition() == typeof(Extensible<>)) {
                newType = curType.GetGenericArguments()[0];
                return true;
            }
            newType = null;
            return false;
        }

        [SpecialName]
        public object Call(CodeContext context, params object[] args) {
            return PythonTypeOps.CallParams(context, this, args);
        }

        [SpecialName]
        public object Call(CodeContext context, [ParamDictionary]IAttributesCollection kwArgs, params object[] args) {
            return PythonTypeOps.CallWorker(context, this, kwArgs, args);
        }

        public int __cmp__([NotNull]PythonType other) {
            return Name.CompareTo(other.Name);
        }

        public void __delattr__(CodeContext/*!*/ context, string name) {
            DeleteCustomMember(context, SymbolTable.StringToId(name));
        }

        [SlotField]
        public static PythonTypeSlot __dict__ = new PythonTypeDictSlot(_pythonTypeType);

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static string Get__doc__(CodeContext/*!*/ context, PythonType self) {
            return PythonTypeOps.GetDocumentation(self.UnderlyingSystemType);
        }

        public object __getattribute__(CodeContext/*!*/ context, string name) {
            object value;
            if (TryGetBoundCustomMember(context, SymbolTable.StringToId(name), out value)) {
                return value;
            }

            throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'", Name, name);
        }

        public PythonType this[params Type[] args] {
            get {
                if (UnderlyingSystemType == typeof(Array)) {
                    if (args.Length == 1) {
                        return DynamicHelpers.GetPythonTypeFromType(args[0].MakeArrayType());
                    }
                    throw PythonOps.TypeError("expected one argument to make array type, got {0}", args.Length);
                }

                if (!UnderlyingSystemType.IsGenericTypeDefinition) {
                    throw new InvalidOperationException("MakeGenericType on non-generic type");
                }

                return DynamicHelpers.GetPythonTypeFromType(UnderlyingSystemType.MakeGenericType(args));
            }
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static string Get__module__(CodeContext/*!*/ context, PythonType self) {
            return PythonTypeOps.GetModuleName(context, self.UnderlyingSystemType);
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static PythonTuple Get__mro__(PythonType type) {
            return PythonTypeOps.MroToPython(type.ResolutionOrder);
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static string Get__name__(PythonType type) {
            return type.Name;
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static void Set__name__(PythonType type, string name) {
            if (type.IsSystemType) {
                throw PythonOps.TypeError("can't set attributes of built-in/extension type '{0}'", type.Name);
            }

            type.Name = name;
        }

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            string name = Name;

            if (IsSystemType) {
                if (PythonTypeOps.IsRuntimeAssembly(UnderlyingSystemType.Assembly) || IsPythonType) {
                    string module = Get__module__(context, this);
                    if (module != "__builtin__") {
                        return string.Format("<type '{0}.{1}'>", module, Name);
                    }
                }
                return string.Format("<type '{0}'>", Name);
            } else {
                PythonTypeSlot dts;
                string module = "unknown";
                object modObj;
                if (TryLookupSlot(context, Symbols.Module, out dts) &&
                    dts.TryGetBoundValue(context, this, this, out modObj)) {
                    module = modObj as string;
                }
                return string.Format("<class '{0}.{1}'>", module, name);
            }
        }

        public void __setattr__(CodeContext/*!*/ context, string name, object value) {
            SetCustomMember(context, SymbolTable.StringToId(name), value);
        }

        public List __subclasses__(CodeContext/*!*/ context) {
            List ret = new List();
            IList<WeakReference> subtypes = SubTypes;

            if (subtypes != null) {
                PythonContext pc = PythonContext.GetContext(context);

                foreach (WeakReference wr in subtypes) {
                    if (wr.IsAlive) {
                        PythonType pt = (PythonType)wr.Target;

                        if (pt.PythonContext == null || pt.PythonContext == pc) {
                            ret.AddNoLock(wr.Target);
                        }
                    }
                }
            }

            return ret;
        }

        public virtual List mro() {
            return new List(Get__mro__(this));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator Type(PythonType self) {
            return self.UnderlyingSystemType;
        }

        public static implicit operator TypeTracker(PythonType self) {
            return ReflectionCache.GetTypeTracker(self.UnderlyingSystemType);
        }

        #endregion

        #region Internal API

        /// <summary>
        /// Gets the name of the dynamic type
        /// </summary>
        internal string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        internal int Version {
            get {
                return _version;
            }
        }

        internal bool IsNull {
            get {
                return UnderlyingSystemType == typeof(None);
            }
        }

        /// <summary>
        /// Gets the resolution order used for attribute lookup
        /// </summary>
        internal IList<PythonType> ResolutionOrder {
            get {
                return _resolutionOrder;
            }
            set {
                lock (SyncRoot) {
                    _resolutionOrder = new List<PythonType>(value);
                }
            }
        }

        /// <summary>
        /// Gets the dynamic type that corresponds with the provided static type. 
        /// 
        /// Returns null if no type is available.  TODO: In the future this will
        /// always return a PythonType created by the DLR.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static PythonType/*!*/ GetPythonType(Type type) {
            PythonType res;
            
            lock (_pythonTypes) {
                if (!_pythonTypes.TryGetValue(type, out res)) {
                    res = new PythonType(type);                    

                    _pythonTypes[type] = res;
                }
            }

            return res;
        }

        /// <summary>
        /// Creates an instance of the dynamic type and runs appropriate class initialization
        /// </summary>
        internal object CreateInstance(CodeContext context, params object[] args) {
            ContractUtils.RequiresNotNull(args, "args");

            EnsureConstructor();

            if (_newCtorSite == null) {
                Interlocked.CompareExchange(
                    ref _newCtorSite,
                    CallSite<DynamicSiteTarget<CodeContext, object, object[], object>>.Create(
                        new InvokeBinder(
                            PythonContext.GetContext(context).DefaultBinderState,
                            new CallSignature(new ArgumentInfo(ArgumentKind.List))
                        )
                    ),
                    null
                );
            }

            return _newCtorSite.Target(_newCtorSite, context, _ctor, args);
        }

        /// <summary>
        /// Creates an instance of the object using keyword parameters.
        /// </summary>
        internal object CreateInstance(CodeContext context, object[] args, string[] names) {
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.RequiresNotNull(names, "names");

            EnsureConstructor();

            return PythonOps.CallWithKeywordArgs(context, _ctor, args, names);
        }

        /// <summary>
        /// Gets the underlying system type that is backing this type.  All instances of this
        /// type are an instance of the underlying system type.
        /// </summary>
        internal Type/*!*/ UnderlyingSystemType {
            get {
                return _underlyingSystemType;
            }
        }

        /// <summary>
        /// Gets the extension type for this type.  The extension type provides
        /// a .NET type which can be inherited from to extend sealed classes
        /// or value types which Python allows inheritance from.
        /// </summary>
        internal Type/*!*/ ExtensionType {
            get {
                if (!_underlyingSystemType.IsEnum) {
                    switch (Type.GetTypeCode(_underlyingSystemType)) {
                        case TypeCode.String: return typeof(ExtensibleString);
                        case TypeCode.Int32: return typeof(Extensible<int>);
                        case TypeCode.Double: return typeof(Extensible<double>);
                        case TypeCode.Object:
                            if (_underlyingSystemType == typeof(BigInteger)) {
                                return typeof(Extensible<BigInteger>);
                            } else if (_underlyingSystemType == typeof(Complex64)) {
                                return typeof(ExtensibleComplex);
                            }
                            break;
                    }
                }
                return _underlyingSystemType;
            }
        }

        /// <summary>
        /// Returns true if the specified object is an instance of this type.
        /// </summary>
        internal bool IsInstanceOfType(object instance) {
            IPythonObject dyno = instance as IPythonObject;
            if (dyno != null) {
                return dyno.PythonType.IsSubclassOf(this);
            }

            return UnderlyingSystemType.IsInstanceOfType(instance);
        }

        /// <summary>
        /// Gets the base types from which this type inherits.
        /// </summary>
        internal IList<PythonType>/*!*/ BaseTypes {
            get {
                lock (_bases) return _bases.ToArray();
            }
            set {
                // validate input...
                foreach (PythonType pt in value) {
                    if (pt == null) throw new ArgumentNullException("value", "a PythonType was null while assigning base classes");
                }

                // first update our sub-type list

                lock (_bases) {
                    foreach (PythonType dt in _bases) {
                        dt.RemoveSubType(this);
                    }

                    // set the new bases
                    List<PythonType> newBases = new List<PythonType>(value);

                    // add us as subtypes of our new bases
                    foreach (PythonType dt in newBases) {
                        dt.AddSubType(this);
                    }

                    UpdateVersion();
                    _bases = newBases;
                }
            }
        }

        /// <summary>
        /// Returns true if this type is a subclass of other
        /// </summary>
        internal bool IsSubclassOf(PythonType other) {
            // check for a type match
            if (other == this) {
                return true;
            }

            //Python doesn't have value types inheriting from ValueType, but we fake this for interop
            if (other.UnderlyingSystemType == typeof(ValueType) && UnderlyingSystemType.IsValueType) {
                return true;
            }

            // check the type hierarchy
            List<PythonType> bases = _bases;
            for (int i = 0; i < bases.Count; i++) {
                PythonType baseClass = bases[i];

                if (baseClass.IsSubclassOf(other)) return true;
            }

            return false;
        }

        /// <summary>
        /// True if the type is a system type.  A system type is a type which represents an
        /// underlying .NET type and not a subtype of one of these types.
        /// </summary>
        internal bool IsSystemType {
            get {
                return (_attrs & PythonTypeAttributes.SystemType) != 0;
            }
            set {
                if (value) _attrs |= PythonTypeAttributes.SystemType;
                else _attrs &= (~PythonTypeAttributes.SystemType);
            }
        }

        internal void AddBaseType(PythonType baseType) {
            if (_bases == null) {
                Interlocked.CompareExchange<List<PythonType>>(ref _bases, new List<PythonType>(), null);
            }

            lock (_bases) _bases.Add(baseType);
        }

        internal void SetConstructor(object ctor) {
            _ctor = ctor;
        }

        internal bool IsPythonType {
            get {
                return (_attrs & PythonTypeAttributes.IsPythonType) != 0;
            }
            set {
                if (value) {
                    _attrs |= PythonTypeAttributes.IsPythonType;
                } else {
                    _attrs &= ~PythonTypeAttributes.IsPythonType;
                }
            }
        }

        internal OldClass OldClass {
            get {
                return _oldClass;
            }
            set {
                _oldClass = value;
            }
        }

        internal bool IsOldClass {
            get {
                return _oldClass != null;
            }
        }

        internal PythonContext PythonContext {
            get {
                return _pythonContext;
            }
        }

        internal object SyncRoot {
            get {
                // TODO: This is un-ideal, we should lock on something private.
                return this;
            }
        }

        internal bool IsHiddenMember(string name) {
            PythonTypeSlot dummySlot;
            return !TryResolveSlot(DefaultContext.Default, SymbolTable.StringToId(name), out dummySlot) &&
                    TryResolveSlot(DefaultContext.DefaultCLS, SymbolTable.StringToId(name), out dummySlot);
        }

        #endregion

        #region Type member access

        /// <summary>
        /// Looks up a slot on the dynamic type
        /// </summary>
        internal bool TryLookupSlot(CodeContext context, SymbolId name, out PythonTypeSlot slot) {
            if (IsSystemType) {
                return PythonBinder.GetBinder(context).TryLookupSlot(context, this, name, out slot);
            }

            return _dict.TryGetValue(name, out slot);
        }

        /// <summary>
        /// Searches the resolution order for a slot matching by name
        /// </summary>
        internal bool TryResolveSlot(CodeContext context, SymbolId name, out PythonTypeSlot slot) {            
            for(int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                // don't look at interfaces - users can inherit from them, but we resolve members
                // via methods implemented on types and defined by Python.
                if (dt.IsSystemType && !dt.UnderlyingSystemType.IsInterface) {
                    return PythonBinder.GetBinder(context).TryResolveSlot(context, dt, this, name, out slot);
                }

                if (dt.TryLookupSlot(context, name, out slot)) {
                    return true;
                }
            }

            if (UnderlyingSystemType.IsInterface) {
                return TypeCache.Object.TryResolveSlot(context, name, out slot);
            }

            slot = null;
            return false;
        }

        /// <summary>
        /// Searches the resolution order for a slot matching by name.
        /// 
        /// Includes searching for methods in old-style classes
        /// </summary>
        internal bool TryResolveMixedSlot(CodeContext context, SymbolId name, out PythonTypeSlot slot) {
            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                if (dt.TryLookupSlot(context, name, out slot)) {
                    return true;
                }

                if (dt.OldClass != null) {
                    object ret;
                    if (dt.OldClass.TryLookupSlot(name, out ret)) {
                        slot = ret as PythonTypeSlot;
                        if (slot == null) {
                            slot = new PythonTypeUserDescriptorSlot(ret);
                        }
                        return true;
                    }
                }
            }

            slot = null;
            return false;
        }

        /// <summary>
        /// Internal helper to add a new slot to the type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="slot"></param>
        /// <param name="context">the context the slot is added for</param>
        internal void AddSlot(SymbolId name, PythonTypeSlot slot) {
            Debug.Assert(!IsSystemType);

            _dict[name] = slot;
        }

        internal void SetCustomMember(CodeContext/*!*/ context, SymbolId name, object value) {
            Debug.Assert(context != null);

            PythonTypeSlot dts;
            if (TryResolveSlot(context, name, out dts)) {
                if (dts.TrySetValue(context, null, this, value))
                    return;
            }

            if (PythonType._pythonTypeType.TryResolveSlot(context, name, out dts)) {
                if (dts.TrySetValue(context, this, PythonType._pythonTypeType, value))
                    return;
            }

            if (IsSystemType) {
                throw new MissingMemberException(String.Format("'{0}' object has no attribute '{1}'", Name, SymbolTable.IdToString(name)));
            }

            dts = value as PythonTypeSlot;
            if (dts != null) {
                _dict[name] = dts;
            } else if (IsSystemType) {
                _dict[name] = new PythonTypeValueSlot(value);
            } else {
                _dict[name] = new PythonTypeUserDescriptorSlot(value);
            }

            UpdateVersion();
        }

        internal bool DeleteCustomMember(CodeContext/*!*/ context, SymbolId name) {
            Debug.Assert(context != null);

            PythonTypeSlot dts;
            if (TryResolveSlot(context, name, out dts)) {
                if (dts.TryDeleteValue(context, null, this))
                    return true;
            }

            if (IsSystemType) {
                throw new MissingMemberException(String.Format("can't delete attributes of built-in/extension type '{0}'", Name, SymbolTable.IdToString(name)));
            }

            if (!_dict.Remove(name)) {
                throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture,
                    IronPython.Resources.MemberDoesNotExist,
                    name.ToString()));
            }

            UpdateVersion();
            return true;
        }

        internal bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            PythonTypeSlot dts;
            if (TryResolveSlot(context, name, out dts)) {
                if (dts.TryGetBoundValue(context, null, this, out value)) {
                    return true;
                }
            }

            // search the type
            PythonType myType = DynamicHelpers.GetPythonType(this);
            if (myType.TryResolveSlot(context, name, out dts)) {
                if (dts.TryGetBoundValue(context, this, myType, out value)) {
                    return true;
                }
            }

            value = null;
            return false;
        }

        #endregion

        #region Instance member access

        internal object GetMember(CodeContext context, object instance, SymbolId name) {
            object res;
            if (TryGetMember(context, instance, name, out res)) {
                return res;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture,
                IronPython.Resources.CantFindMember,
                SymbolTable.IdToString(name)));
        }

        internal object GetBoundMember(CodeContext context, object instance, SymbolId name) {
            object value;
            if (TryGetBoundMember(context, instance, name, out value)) {
                return value;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture,
                IronPython.Resources.CantFindMember,
                SymbolTable.IdToString(name)));
        }

        internal void SetMember(CodeContext context, object instance, SymbolId name, object value) {
            if (TrySetMember(context, instance, name, value)) {
                return;
            }

            throw new MissingMemberException(
                String.Format(CultureInfo.CurrentCulture,
                    IronPython.Resources.Slot_CantSet,
                    name));
        }

        internal void DeleteMember(CodeContext context, object instance, SymbolId name) {
            if (TryDeleteMember(context, instance, name)) {
                return;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture, "couldn't delete member {0}", name));
        }

        /// <summary>
        /// Gets a value from a dynamic type and any sub-types.  Values are stored in slots (which serve as a level of 
        /// indirection).  This searches the types resolution order and returns the first slot that
        /// contains the value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal bool TryGetMember(CodeContext context, object instance, SymbolId name, out object value) {
            if (TryGetNonCustomMember(context, instance, name, out value)) {
                return true;
            }

            try {
                if (PythonTypeOps.TryInvokeBinaryOperator(context, instance, SymbolTable.IdToString(name), Symbols.GetBoundAttr, out value)) {
                    return true;
                }
            } catch (MissingMemberException) {
                //!!! when do we let the user see this exception?
            }

            return false;
        }

        /// <summary>
        /// Attempts to lookup a member w/o using the customizer.  Equivelent to object.__getattribute__
        /// but it doens't throw an exception.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal bool TryGetNonCustomMember(CodeContext context, object instance, SymbolId name, out object value) {
            PythonType pt;
            IPythonObject sdo;
            bool hasValue = false;
            value = null;

            // first see if we have the value in the instance dictionary...
            // TODO: Instance checks should also work on functions, 
            if ((pt = instance as PythonType) != null) {
                PythonTypeSlot pts;
                if (pt.TryLookupSlot(context, name, out pts)) {
                    hasValue = pts.TryGetBoundValue(context, null, this, out value);
                }
            } else if ((sdo = instance as IPythonObject) != null) {
                IAttributesCollection iac = sdo.Dict;

                hasValue = iac != null && iac.TryGetValue(name, out value);
            } 

            // then check through all the descriptors.  If we have a data
            // descriptor it takes priority over the value we found in the
            // dictionary.  Otherwise only run a get descriptor if we don't
            // already have a value.
            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                PythonTypeSlot slot;
                object newValue;
                if (dt.TryLookupSlot(context, name, out slot)) {
                    if (!hasValue || slot.IsSetDescriptor(context, this)) {
                        if (slot.TryGetValue(context, instance, this, out newValue))
                            value = newValue;
                            return true;
                    }
                }
            }

            return hasValue;
        }

        /// <summary>
        /// Gets a value from a dynamic type and any sub-types.  Values are stored in slots (which serve as a level of 
        /// indirection).  This searches the types resolution order and returns the first slot that
        /// contains the value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal bool TryGetBoundMember(CodeContext context, object instance, SymbolId name, out object value) {
            object getattr;
            if (TryResolveNonObjectSlot(context, instance, Symbols.GetAttribute, out getattr)) {
                value = InvokeGetAttributeMethod(context, name, getattr);
                return true;
            }

            return TryGetNonCustomBoundMember(context, instance, name, out value);
        }

        private object InvokeGetAttributeMethod(CodeContext context, SymbolId name, object getattr) {
            EnsureGetAttributeSite(context);

            return _newGetattributeSite.Target(_newGetattributeSite, context, getattr, SymbolTable.IdToString(name));
        }

        private void EnsureGetAttributeSite(CodeContext context) {
            if (_newGetattributeSite == null) {
                Interlocked.CompareExchange(
                    ref _newGetattributeSite,
                    CallSite<DynamicSiteTarget<CodeContext, object, string, object>>.Create(
                        new InvokeBinder(
                            PythonContext.GetContext(context).DefaultBinderState,
                            new CallSignature(1)
                        )
                    ),
                    null
                );
            }
        }

        /// <summary>
        /// Attempts to lookup a member w/o using the customizer.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal bool TryGetNonCustomBoundMember(CodeContext context, object instance, SymbolId name, out object value) {
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac != null && iac.TryGetValue(name, out value)) {
                    return true;
                }
            }

            if (TryResolveSlot(context, instance, name, out value)) {
                return true;
            }

            try {
                object getattr;
                if (TryResolveNonObjectSlot(context, instance, Symbols.GetBoundAttr, out getattr)) {
                    value = InvokeGetAttributeMethod(context, name, getattr);
                    return true;
                }
            } catch (MissingMemberException) {
                //!!! when do we let the user see this exception?
            }

            value = null;
            return false;
        }

        private bool TryResolveSlot(CodeContext context, object instance, SymbolId name, out object value) {
            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                PythonTypeSlot slot;
                if (dt.TryLookupSlot(context, name, out slot)) {
                    if (slot.TryGetBoundValue(context, instance, this, out value))
                        return true;
                }
            }

            value = null;
            return false;
        }

        private bool TryResolveNonObjectSlot(CodeContext context, object instance, SymbolId name, out object value) {
            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                if (dt == TypeCache.Object) break;

                PythonTypeSlot slot;
                if (dt.TryLookupSlot(context, name, out slot)) {
                    if (slot.TryGetBoundValue(context, instance, this, out value))
                        return true;
                }
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Sets a value on an instance.  If a slot is available in the most derived type the slot
        /// is set there, otherwise the value is stored directly in the instance.
        /// </summary>
        internal bool TrySetMember(CodeContext context, object instance, SymbolId name, object value) {
            object setattr;
            if (TryResolveNonObjectSlot(context, instance, Symbols.SetAttr, out setattr)) {
                if (_newSetattrSite == null) {
                    Interlocked.CompareExchange(
                        ref _newSetattrSite,
                        CallSite<DynamicSiteTarget<CodeContext, object, object, string, object, object>>.Create(
                            new InvokeBinder(
                                PythonContext.GetContext(context).DefaultBinderState,
                                new CallSignature(4)
                            )
                        ),
                        null
                    );
                }

                _newSetattrSite.Target(_newSetattrSite, context, setattr, instance, SymbolTable.IdToString(name), value);
                return true;                              
            }

            return TrySetNonCustomMember(context, instance, name, value);
        }

        /// <summary>
        /// Attempst to set a value w/o going through the customizer.
        /// 
        /// This enables languages to provide the "base" implementation for setting attributes
        /// so that the customizer can call back here.
        /// </summary>
        internal bool TrySetNonCustomMember(CodeContext context, object instance, SymbolId name, object value) {
            PythonTypeSlot slot;
            if (TryResolveSlot(context, name, out slot)) {
                if (slot.TrySetValue(context, instance, this, value)) {
                    return true;
                }
            }

            // set the attribute on the instance
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac == null) {
                    iac = PythonDictionary.MakeSymbolDictionary();

                    if ((iac = sdo.SetDict(iac)) == null) {
                        return false;
                    }
                }

                iac[name] = value;
                return true;
            }

            return false;
        }

        internal bool TryDeleteMember(CodeContext context, object instance, SymbolId name) {
            try {
                object delattr;
                if (TryResolveNonObjectSlot(context, instance, Symbols.DelAttr, out delattr)) {
                    InvokeGetAttributeMethod(context, name, delattr);
                    return true;
                }
            } catch (MissingMemberException) {
                //!!! when do we let the user see this exception?
            }

            return TryDeleteNonCustomMember(context, instance, name);
        }

        internal bool TryDeleteNonCustomMember(CodeContext context, object instance, SymbolId name) {
            PythonTypeSlot slot;
            if (TryResolveSlot(context, name, out slot)) {
                if (slot.TryDeleteValue(context, instance, this)) {
                    return true;
                }
            }

            // set the attribute on the instance
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac == null) {
                    iac = PythonDictionary.MakeSymbolDictionary();

                    if ((iac = sdo.SetDict(iac)) == null) {
                        return false;
                    }
                }

                return iac.Remove(name);
            }

            return false;
        }

        #endregion

        #region Member lists

        /// <summary>
        /// Returns a list of all slot names for the type and any subtypes.
        /// </summary>
        /// <param name="context">The context that is doing the inquiry of InvariantContext.Instance.</param>
        internal List GetMemberNames(CodeContext context) {
            return GetMemberNames(context, null);
        }

        /// <summary>
        /// Returns a list of all slot names for the type, any subtypes, and the instance.
        /// </summary>
        /// <param name="context">The context that is doing the inquiry of InvariantContext.Instance.</param>
        /// <param name="self">the instance to get instance members from, or null.</param>
        internal List GetMemberNames(CodeContext context, object self) {
            List res = TryGetCustomDir(context, self);
            if (res != null) {
                return res;
            }

            Dictionary<string, string> keys = new Dictionary<string, string>();

            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                if (dt.IsSystemType) {
                    PythonBinder.GetBinder(context).ResolveMemberNames(context, dt, this, keys);
                } else {
                    AddUserTypeMembers(context, keys, dt);
                }
            }

            AddInstanceMembers(self, keys);

            return new List(keys.Keys);
        }

        private List TryGetCustomDir(CodeContext context, object self) {
            if (self != null) {
                object dir;
                if (TryResolveNonObjectSlot(context, self, SymbolTable.StringToId("__dir__"), out dir)) {
                    EnsureDirSite(context);

                    return new List(_newDirSite.Target(_newDirSite, context, dir));
                }
            }

            return null;
        }

        private void EnsureDirSite(CodeContext context) {
            if (_newDirSite == null) {
                Interlocked.CompareExchange(
                    ref _newDirSite,
                    CallSite<DynamicSiteTarget<CodeContext, object, object>>.Create(
                        new InvokeBinder(
                            PythonContext.GetContext(context).DefaultBinderState,
                            new CallSignature(0)
                        )
                    ),
                    null);
            }
        }

        /// <summary>
        /// Adds members from a user defined type.
        /// </summary>
        private void AddUserTypeMembers(CodeContext context, Dictionary<string, string> keys, PythonType dt) {
            int ctxId = context.LanguageContext.ContextId.Id;

            foreach (KeyValuePair<SymbolId, PythonTypeSlot> kvp in dt._dict) {
                if (keys.ContainsKey(SymbolTable.IdToString(kvp.Key))) continue;

                keys[SymbolTable.IdToString(kvp.Key)] = SymbolTable.IdToString(kvp.Key);
            }
        }

        /// <summary>
        /// Adds members from a user defined type instance
        /// </summary>
        private static void AddInstanceMembers(object self, Dictionary<string, string> keys) {
            IPythonObject dyno = self as IPythonObject;
            if (dyno != null) {
                IAttributesCollection iac = dyno.Dict;
                if (iac != null) {
                    lock (iac) {
                        foreach (SymbolId id in iac.SymbolAttributes.Keys) {
                            keys[SymbolTable.IdToString(id)] = SymbolTable.IdToString(id);
                        }
                    }
                }
            }
        }

        internal IAttributesCollection GetMemberDictionary(CodeContext context) {
            return GetMemberDictionary(context, true);
        }

        internal IAttributesCollection GetMemberDictionary(CodeContext context, bool excludeDict) {
            IAttributesCollection iac = PythonDictionary.MakeSymbolDictionary();
            if (IsSystemType) {
                PythonBinder.GetBinder(context).LookupMembers(context, this, iac);
            } else {
                foreach (SymbolId x in _dict.Keys) {
                    if (excludeDict && x.ToString() == "__dict__") {
                        continue;
                    }

                    PythonTypeSlot dts;
                    if (TryLookupSlot(context, x, out dts)) {
                        //??? why check for DTVS?
                        object val;
                        if (dts.TryGetValue(context, null, this, out val)) {
                            if ((dts is PythonTypeValueSlot) || (dts is PythonTypeUserDescriptorSlot)) {
                                iac[x] = val;
                            } else {
                                iac[x] = dts;
                            }
                        }
                    }
                }
            }
            return iac;
        }

        internal IAttributesCollection GetMemberDictionary(CodeContext context, object self) {
            if (self != null) {
                IPythonObject sdo = self as IPythonObject;
                if (sdo != null) return sdo.Dict;

                return null;
            }
            return GetMemberDictionary(context);
        }

        #endregion

        #region User type initialization

        private void InitializeUserType(CodeContext/*!*/ context, string name, PythonTuple bases, IAttributesCollection vars) {
            // we don't support overriding __mro__
            if (vars.ContainsKey(Symbols.MethodResolutionOrder))
                throw new NotImplementedException("Overriding __mro__ of built-in types is not implemented");

            // cannot override mro when inheriting from type
            if (vars.ContainsKey(SymbolTable.StringToId("mro"))) {
                foreach (object o in bases) {
                    PythonType dt = o as PythonType;
                    if (dt != null && dt.IsSubclassOf(TypeCache.PythonType)) {
                        throw new NotImplementedException("Overriding type.mro is not implemented");
                    }
                }
            }
            
            bases = ValidateBases(bases);

            _name = name;
            _ctor = new TypePrepender(this, PythonTypeOps.GetPrependerState(_underlyingSystemType));
            _bases = GetBasesAsList(bases);
            _resolutionOrder = CalculateMro(this, _bases);
            _pythonContext = PythonContext.GetContext(context);

            foreach (PythonType pt in _bases) {
                pt.AddSubType(this);
            }

            BuildUserTypeDictionary(context, vars);
        }

        private void BuildUserTypeDictionary(CodeContext context, IAttributesCollection vars) {
            bool hasDictionary = false, hasWeakRef = false;
            if (vars.ContainsKey(Symbols.Slots)) {
                List<string> slots = NewTypeMaker.SlotsToList(vars[Symbols.Slots]);
                if (slots.Contains("__dict__")) hasDictionary = true;
                if (slots.Contains("__weakref__")) hasWeakRef = true;
            } else {
                hasDictionary = true;
                hasWeakRef = true;
            }

            EnsureModule(context, vars);
            EnsureDoc(vars);
            MakeNewStatic(vars);

            EnsureDict();

            if (hasWeakRef && !vars.ContainsKey(Symbols.WeakRef)) {
                AddSlot(Symbols.WeakRef, new PythonTypeWeakRefSlot(this));
            }

            if (!vars.ContainsKey(Symbols.Dict) && hasDictionary) {
                AddSlot(Symbols.Dict, new PythonTypeDictSlot(this));
            }

            PopulateSlots(vars);
        }

        private void PopulateSlots(IAttributesCollection vars) {
            foreach (KeyValuePair<SymbolId, object> kvp in vars.SymbolAttributes) {
                PythonTypeSlot pts = kvp.Value as PythonTypeSlot;
                if (pts == null) {
                    pts = new PythonTypeUserDescriptorSlot(kvp.Value);
                }

                AddSlot(kvp.Key, pts);
            }
        }

        private static List<PythonType> GetBasesAsList(PythonTuple bases) {
            List<PythonType> newbs = new List<PythonType>();
            foreach (object typeObj in bases) {
                PythonType dt = typeObj as PythonType;
                if (dt == null) {
                    dt = ((OldClass)typeObj).TypeObject;
                }

                newbs.Add(dt);
            }

            return newbs;
        }

        private static void EnsureDoc(IAttributesCollection vars) {
            if (!vars.ContainsKey(Symbols.Doc)) {
                vars[Symbols.Doc] = new PythonTypeValueSlot(null);
            }

        }

        private static void MakeNewStatic(IAttributesCollection vars) {
            object newInst;
            if (vars.TryGetValue(Symbols.NewInst, out newInst) && newInst is PythonFunction) {
                vars[Symbols.NewInst] = new staticmethod(newInst);
            }
        }

        private PythonTuple ValidateBases(PythonTuple bases) {
            PythonTuple newBases = PythonTypeOps.EnsureBaseType(bases);
            for (int i = 0; i < newBases.__len__(); i++) {
                for (int j = 0; j < newBases.__len__(); j++) {
                    if (i != j && newBases[i] == newBases[j]) {
                        OldClass oc = newBases[i] as OldClass;
                        if (oc != null) {
                            throw PythonOps.TypeError("duplicate base class {0}", oc.Name);
                        } else {
                            throw PythonOps.TypeError("duplicate base class {0}", ((PythonType)newBases[i]).Name);
                        }
                    }
                }
            }
            return newBases;
        }

        private static void EnsureModule(CodeContext context, IAttributesCollection dict) {
            if (!dict.ContainsKey(Symbols.Module)) {
                object modName;
                if (context.Scope.TryLookupName(Symbols.Name, out modName)) {
                    dict[Symbols.Module] = modName;
                }
            }
        }

        #endregion

        #region System type initialization

        /// <summary>
        /// Initializes a PythonType that represents a standard .NET type.  The same .NET type
        /// can be shared with the Python type system.  For example object, string, int,
        /// etc... are all the same types.  
        /// </summary>
        private void InitializeSystemType() {
            IsSystemType = true;
            IsPythonType = PythonBinder.IsPythonType(_underlyingSystemType);
            _name = NameConverter.GetTypeName(_underlyingSystemType);
            AddSystemBases();
        }

        private void AddSystemBases() {
            List<PythonType> mro = new List<PythonType>();
            mro.Add(this);

            if (_underlyingSystemType.BaseType != null) {
                Type baseType;
                if (_underlyingSystemType == typeof(bool)) {
                    // bool inherits from int in python
                    baseType = typeof(int);
                } else if (_underlyingSystemType.BaseType == typeof(ValueType)) {
                    // hide ValueType, it doesn't exist in Python
                    baseType = typeof(object);
                } else {
                    baseType = _underlyingSystemType.BaseType;
                }
                _bases.Add(GetPythonType(baseType));

                Type curType = baseType;
                while (curType != null) {
                    Type newType;
                    if (TryReplaceExtensibleWithBase(curType, out newType)) {
                        mro.Add(DynamicHelpers.GetPythonTypeFromType(newType));
                    } else {
                        mro.Add(DynamicHelpers.GetPythonTypeFromType(curType));
                    }
                    curType = curType.BaseType;
                }

                if (!IsPythonType) {
                    AddSystemInterfaces(mro);
                }
            } else if (_underlyingSystemType.IsInterface) {
                foreach (Type i in _underlyingSystemType.GetInterfaces()) {
                    PythonType it = DynamicHelpers.GetPythonTypeFromType(i);
                    mro.Add(it);
                    AddBaseType(it);
                }
            }
            _resolutionOrder = mro;
        }

        private void AddSystemInterfaces(List<PythonType> mro) {
            if (_underlyingSystemType.IsArray) {
                return;
            } 

            Type[] interfaces = _underlyingSystemType.GetInterfaces();
            Dictionary<string, Type> methodMap = new Dictionary<string, Type>();
            bool hasExplicitIface = false;
            List<Type> nonCollidingInterfaces = new List<Type>(interfaces);
            
            foreach (Type iface in interfaces) {
                InterfaceMapping mapping = _underlyingSystemType.GetInterfaceMap(iface);
                
                // grab all the interface methods which would hide other members
                for (int i = 0; i < mapping.TargetMethods.Length; i++) {
                    MethodInfo target = mapping.TargetMethods[i];
                    MethodInfo iTarget = mapping.InterfaceMethods[i];

                    if (!target.IsPrivate) {
                        methodMap[target.Name] = null;
                    } else {
                        hasExplicitIface = true;
                    }
                }

                if (hasExplicitIface) {
                    for (int i = 0; i < mapping.TargetMethods.Length; i++) {
                        MethodInfo target = mapping.TargetMethods[i];
                        MethodInfo iTarget = mapping.InterfaceMethods[i];

                        // any methods which aren't explicit are picked up at the appropriate
                        // time earlier in the MRO so they can be ignored
                        if (target.IsPrivate) {
                            hasExplicitIface = true;

                            Type existing;
                            if (methodMap.TryGetValue(iTarget.Name, out existing)) {
                                if (existing != null) {
                                    // collision, multiple interfaces implement the same name, and
                                    // we're not hidden by another method.  remove both interfaces, 
                                    // but leave so future interfaces get removed
                                    nonCollidingInterfaces.Remove(iface);
                                    nonCollidingInterfaces.Remove(methodMap[iTarget.Name]);
                                    break;
                                }
                            } else {
                                // no collisions so far...
                                methodMap[iTarget.Name] = iface;
                            }
                        } 
                    }
                }
            }

            if (hasExplicitIface) {
                // add any non-colliding interfaces into the MRO
                foreach (Type t in nonCollidingInterfaces) {
                    Debug.Assert(t.IsInterface);

                    mro.Add(DynamicHelpers.GetPythonTypeFromType(t));
                }
            }
        }

        /// <summary>
        /// Creates a __new__ method for the type.  If the type defines interesting constructors
        /// then the __new__ method will call that.  Otherwise if it has only a single argless
        /// </summary>
        private void AddSystemConstructors() {
            if (typeof(Delegate).IsAssignableFrom(_underlyingSystemType)) {
                SetConstructor(new DelegateBuilder(_underlyingSystemType));
            } else if (!_underlyingSystemType.IsAbstract) {
                BuiltinFunction reflectedCtors = GetConstructors();
                if (reflectedCtors == null) {
                    return; // no ctors, no __new__
                }

                SetConstructor(reflectedCtors);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        private BuiltinFunction GetConstructors() {
            Type type = _underlyingSystemType;
            string name = Name;

            return PythonTypeOps.GetConstructorFunction(type, name);
        }

        private void EnsureConstructor() {
            if (_ctor == null) {
                AddSystemConstructors();
                if (_ctor == null) {
#if SILVERLIGHT
                    throw new MissingMethodException(Name + ".ctor");
#else
                    throw new MissingMethodException(Name, ".ctor");
#endif
                }
            }
        }

        #endregion

        #region Private implementation details

        internal void Initialize() {
            EnsureDict();
        }

        private void UpdateVersion() {
            foreach (WeakReference wr in SubTypes) {
                if (wr.IsAlive) {
                    ((PythonType)wr.Target).UpdateVersion();
                }
            }

            _version = GetNextVersion();
        }

        /// <summary>
        /// This will return a unique integer for every version of every type in the system.
        /// This means that DynamicSite code can generate a check to see if it has the correct
        /// PythonType and version with a single integer compare.
        /// 
        /// TODO - This method and related code should fail gracefully on overflow.
        /// </summary>
        private static int GetNextVersion() {
            if (MasterVersion < 0) {
                throw new InvalidOperationException(IronPython.Resources.TooManyVersions);
            }
            return Interlocked.Increment(ref MasterVersion);
        }

        private void EnsureDict() {
            if (_dict == null) {
                Interlocked.CompareExchange<Dictionary<SymbolId, PythonTypeSlot>>(
                    ref _dict,
                    new Dictionary<SymbolId, PythonTypeSlot>(),
                    null);
            }
        }
      
        /// <summary>
        /// Internal helper function to add a subtype
        /// </summary>
        private void AddSubType(PythonType subtype) {
            if (_subtypes == null) {
                Interlocked.CompareExchange<List<WeakReference>>(ref _subtypes, new List<WeakReference>(), null);
            }

            lock (_subtypes) {
                _subtypes.Add(new WeakReference(subtype));
            }
        }

        private void RemoveSubType(PythonType subtype) {
            int i = 0;
            if (_subtypes != null) {
                lock (_subtypes) {
                    while (i < _subtypes.Count) {
                        if (!_subtypes[i].IsAlive || _subtypes[i].Target == subtype) {
                            _subtypes.RemoveAt(i);
                            continue;
                        }
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of weak references to all the subtypes of this class.  May return null
        /// if there are no subtypes of the class.
        /// </summary>
        private IList<WeakReference> SubTypes {
            get {
                if (_subtypes == null) return _emptyWeakRef;

                lock (_subtypes) return _subtypes.ToArray();
            }
        }

        [Flags]
        private enum PythonTypeAttributes {
            None = 0x00,
            Immutable = 0x01,
            SystemType = 0x02,
            IsPythonType = 0x04,
            Initializing = 0x10000000,
            Initialized = 0x20000000,
        }

        #endregion

        #region IOldDynamicObject Members

        // GetRule is hidden instead of explicit so that subclasses can dispatch to the base GetRule implementation
        [PythonHidden] 
        public RuleBuilder<T> GetRule<T>(OldDynamicAction action, CodeContext context, object[] args) where T : class {
            switch(action.Kind) {
                case DynamicActionKind.CreateInstance: return MakeCreateInstanceAction<T>(action, context, args);
                case DynamicActionKind.GetMember: return MakeGetMemberRule<T>(context, (OldGetMemberAction)action);
                case DynamicActionKind.SetMember: return MakeSetMemberRule<T>((OldSetMemberAction)action, context, args);
                case DynamicActionKind.DeleteMember: return MakeDeleteMemberRule<T>((OldDeleteMemberAction)action, context);
            }

            return null;
        }


        private RuleBuilder<T> MakeCreateInstanceAction<T>(OldDynamicAction action, CodeContext context, object[] args) where T : class {
            if (IsSystemType) {
                MethodBase[] ctors = CompilerHelpers.GetConstructors(UnderlyingSystemType, context.LanguageContext.Binder.PrivateBinding);
                RuleBuilder<T> rule;
                if (ctors.Length > 0) {
                    rule = new CallBinderHelper<T, OldCallAction>(context, (OldCallAction)action, args, ctors).MakeRule();
                } else {
                    rule = new RuleBuilder<T>();
                    rule.Target =
                       rule.MakeError(
                           Ast.New(
                               typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                               Ast.Constant("Cannot create instances of " + Name)
                           )
                       );
                }
                rule.AddTest(Ast.Equal(rule.Parameters[0], Ast.Constant(args[0])));
                return rule;
            } else {
                // TODO: Pull in the Python create logic for this when PythonType moves out of MS.Scripting, this provides
                // a minimal level of interop until then.
                RuleBuilder<T> rule = new RuleBuilder<T>();

                // calling NonDefaultNew(context, type, args)
                Expression call = Ast.ComplexCallHelper(
                    typeof(InstanceOps).GetMethod("NonDefaultNew"),
                    ArrayUtils.Insert<Expression>(rule.Context,
                                       Ast.Convert(rule.Parameters[0], typeof(PythonType)),
                                       ArrayUtils.RemoveFirst(rule.Parameters))
                );
                rule.Target = rule.MakeReturn(context.LanguageContext.Binder, call);
                rule.Test = Ast.Equal(rule.Parameters[0], Ast.Constant(args[0]));
                return rule;
            }
        }

        private RuleBuilder<T> MakeGetMemberRule<T>(CodeContext/*!*/ context, OldGetMemberAction action) where T : class {
            Expression body = null;
            RuleBuilder<T> rule = new RuleBuilder<T>();

            if (action.Name == Symbols.Dict || 
                action.Name == Symbols.Class || 
                action.Name == Symbols.Bases ||
                action.Name == Symbols.Name) {
                // __dict__/__class__/__bases__/__name__ are always looked up from the type so
                // we can generate a more general rule that works across multiple types.
                rule.Test = MakeMetaTypeTest<T>(rule, rule.Parameters[0]);
                rule.Target = MakeMetaTypeRule<T>(context, rule, action);
                return rule;
            }

            // normal attribute, need to check the type version
            rule.Test = MakeTypeTest(rule.Parameters[0]);

            VariableExpression tmp = rule.GetTemporary(typeof(object), "result");

            foreach (PythonType pt in this._resolutionOrder) {
                PythonTypeSlot pts;

                if (pt.IsSystemType) {
                    // built-in type, see if we can bind to any .NET members and then quit the search 
                    // because this includes all subtypes.
                    body = CombineBody(body, MakeSystemTypeGetMemberRule<T>(context, pt.UnderlyingSystemType, rule, action));
                    break;
                } else if (pt.IsOldClass) {
                    // mixed new-style/old-style class, search the one slot in it's MRO for the member
                    body = CombineBody(
                        body,
                        Ast.If(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("OldClassTryLookupOneSlot"),
                                Ast.Constant(pt.OldClass),
                                Ast.Constant(action.Name),
                                tmp
                            ),
                            rule.MakeReturn(context.LanguageContext.Binder, tmp)
                        )
                    );
                } else if (pt.TryLookupSlot(context, action.Name, out pts)) {
                    // user defined new style class, see if we have a slot.
                    body = CombineBody(
                        body,
                        Ast.If(
                            Ast.Call(
                                TypeInfo._PythonOps.SlotTryGetBoundValue,
                                rule.Context,
                                Ast.Constant(pts, typeof(PythonTypeSlot)),
                                Ast.Null(),
                                Ast.Constant(this),
                                tmp
                            ),
                            rule.MakeReturn(context.LanguageContext.Binder, tmp)
                        )
                    );
                }
            }

            rule.Target = body;            
            return rule;
        }

        private RuleBuilder<T> MakeSetMemberRule<T>(OldSetMemberAction action, CodeContext context, params object[] args) where T : class {
            RuleBuilder<T> rule;

            if (IsSystemType) {
                MemberTracker tt = MemberTracker.FromMemberInfo(UnderlyingSystemType);
                args = (object[])args.Clone();
                args[0] = tt;
                rule = new SetMemberBinderHelper<T>(context, (OldSetMemberAction)action, args).MakeNewRule();
                rule.Test = MakeSystemTypeTest(rule.Parameters[0]);
                return rule;
            }

            // TODO: We could do better if we resolve to a built-in static field slot or the like.
            rule = new RuleBuilder<T>();
            rule.Test = MakeUserTypeTest(rule.Parameters[0]);
            rule.Target = rule.MakeReturn(
                context.LanguageContext.Binder,
                Ast.Call(
                    typeof(PythonOps).GetMethod("PythonTypeSetCustomMember"),
                    rule.Context,
                    Ast.ConvertHelper(
                        rule.Parameters[0],
                        typeof(PythonType)
                    ),
                    Ast.Constant(action.Name),
                    Ast.ConvertHelper(
                        rule.Parameters[1],
                        typeof(object)
                    )
                )
            );

            return rule;
        }

        private RuleBuilder<T> MakeDeleteMemberRule<T>(OldDeleteMemberAction action, CodeContext context) where T : class {
            RuleBuilder<T> rule;

            if (IsSystemType) {
                MemberTracker tt = MemberTracker.FromMemberInfo(UnderlyingSystemType);
                rule = new DeleteMemberBinderHelper<T>(context, (OldDeleteMemberAction)action, new object[] { tt }).MakeRule();
                rule.Test = MakeSystemTypeTest(rule.Parameters[0]);
            } else {
                rule = new RuleBuilder<T>();
                rule.Test = MakeUserTypeTest(rule.Parameters[0]);
                rule.Target = rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("PythonTypeDeleteCustomMember"),
                        rule.Context,
                        Ast.ConvertHelper(
                            rule.Parameters[0],
                            typeof(PythonType)
                        ),
                        Ast.Constant(action.Name)
                    )
                );
            }

            return rule;
        }

        private static Expression CombineBody(Expression oldBody, Expression newBody) {
            if (oldBody != null) {
                return Ast.Block(oldBody, newBody);
            } 
            return newBody;
        }

        /// <summary>
        /// Makes a type test to test that the provided expression is this specific type
        /// at the current version.
        /// </summary>
        private Expression MakeTypeTest(Expression type) {
            if (IsSystemType) {
                return MakeSystemTypeTest(type);
            } else {
                return Ast.AndAlso(
                    Ast.Equal(type, Ast.Constant(this)),
                    Ast.Call(typeof(PythonOps).GetMethod("CheckSpecificTypeVersion"),
                        Ast.ConvertHelper(type, typeof(PythonType)),
                        Ast.Constant(_version)
                    )
                );
            }
        }

        /// <summary>
        /// Makes a type test to test that the provided expression is this specific type
        /// at the current version.
        /// </summary>
        private Expression MakeMetaTypeTest<T>(RuleBuilder<T> builder, Expression type) where T : class {
            Expression res = builder.MakeTypeTest(GetType(), type);
            PythonType metaType = DynamicHelpers.GetPythonType(this);
            if (!DynamicHelpers.GetPythonType(this).IsSystemType) {
                res = Ast.AndAlso(res,
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CheckTypeVersion"),
                            type,
                            Ast.Constant(metaType._version)
                        )
                    );
            }

            return res;
        }

        /// <summary>
        /// Makes a generic test to verify that the type is a user type.
        /// </summary>
        private static BinaryExpression MakeUserTypeTest(Expression type) {
            return Ast.AndAlso(
                Ast.TypeIs(
                    type,
                    typeof(PythonType)
                ),
                Ast.Not(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("IsPythonType"),
                        Ast.ConvertHelper(
                            type,
                            typeof(PythonType)
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Makes a specific type test for this .NET type (including standard Python types such as object, string, etc...)
        /// </summary>
        private BinaryExpression MakeSystemTypeTest(Expression type) {
            Debug.Assert(IsSystemType);

            return Ast.Equal(type, Ast.Constant(this));
        }

        private Expression MakeSystemTypeGetMemberRule<T>(CodeContext/*!*/ context, Type type, RuleBuilder<T> rule, OldGetMemberAction action) where T : class {
            ActionBinder ab = PythonContext.GetContext(context).Binder;
            string name = SymbolTable.IdToString(action.Name);
            
            MemberGroup mg = ab.GetMember(action, type, name);

            if (mg.Count > 0) {
                return GetTrackerOrError<T>(context, action, rule, ab, name, mg);
            }
            
            // need to lookup on type
            return MakeMetaTypeRule<T>(context, rule, action);
        }

        private Expression MakeMetaTypeRule<T>(CodeContext context, RuleBuilder<T> rule, OldGetMemberAction action) where T : class {
            ActionBinder ab = PythonContext.GetContext(context).Binder;
            string name = SymbolTable.IdToString(action.Name);
            MemberGroup mg = ab.GetMember(action, typeof(PythonType), name); // TODO: meta type?

            PythonType metaType = DynamicHelpers.GetPythonType(this);
            PythonTypeSlot pts;

            foreach (PythonType pt in metaType._resolutionOrder) {
                if (pt.IsSystemType) {
                    // need to lookup on type
                    mg = ab.GetMember(action, typeof(PythonType), name);

                    if (mg.Count > 0) {
                        return GetBoundTrackerOrError<T>(rule, ab, name, mg);                       
                    }
                } else if (pt.OldClass != null) {
                    // mixed new-style/old-style class, just call our version of __getattribute__
                    // and let it sort it out at runtime.
                    return rule.MakeReturn(
                        context.LanguageContext.Binder,
                        Ast.Call(
                            Ast.ConvertHelper(
                                rule.Parameters[0],
                                typeof(PythonType)
                            ),
                            typeof(PythonType).GetMethod("__getattribute__"),
                            rule.Context,
                            Ast.Constant(name)
                        )                        
                    );
                } else if (pt.TryLookupSlot(context, action.Name, out pts)) {
                    // user defined new style class, see if we have a slot.
                    Expression tmp = rule.GetTemporary(typeof(object), "slotRes");
                    return Ast.If(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                            rule.Context,
                            Ast.Constant(pts, typeof(PythonTypeSlot)),
                            rule.Parameters[0],
                            Ast.Constant(metaType),
                            tmp
                        ),
                        rule.MakeReturn(
                            context.LanguageContext.Binder,
                            tmp
                        )
                    );
                }
            }

            // the member doesn't exist anywhere in the type hierarchy, see if
            // we define __getattr__ on our meta type.
            if (metaType.TryResolveSlot(context, Symbols.GetBoundAttr, out pts)) {
                Expression tmp = rule.GetTemporary(typeof(object), "slotRes");
                return Ast.If(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                        rule.Context,
                        Ast.Constant(pts, typeof(PythonTypeSlot)),
                        rule.Parameters[0],
                        Ast.Constant(metaType),
                        tmp
                    ),
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        AstUtils.Call(
                            context.LanguageContext.Binder,
                            typeof(object),
                            rule.Context,
                            tmp,
                            Ast.Constant(name)
                        )
                    )
                );
            } else if (action.IsNoThrow) {
                return MakeNoThrowRule<T>(rule, ab);
            } 
            
            ErrorInfo ei;
            if (context.LanguageContext is PythonContext) {
                ei = ErrorInfo.FromException(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                        Ast.Constant(DynamicHelpers.GetPythonType(this).Name),
                        Ast.Constant(action.Name)
                    )
                );
            } else {
                ei = context.LanguageContext.Binder.MakeMissingMemberErrorInfo(typeof(PythonType), name);
            }

            return ei.MakeErrorForRule(rule, ab);
        }

        private static Expression MakeNoThrowRule<T>(RuleBuilder<T> rule, ActionBinder ab) where T : class {
            return rule.MakeReturn(ab, Ast.Field(null, typeof(OperationFailed).GetField("Value")));
        }

        private Expression GetBoundTrackerOrError<T>(RuleBuilder<T> rule, ActionBinder ab, string name, MemberGroup mg) where T : class {
            MemberTracker tracker = GetTracker(ab, name, mg);
            Expression target = null;

            if (tracker != null) {
                tracker = tracker.BindToInstance(Ast.ConvertHelper(rule.Parameters[0], typeof(PythonType)));
                target = tracker.GetValue(rule.Context, ab, UnderlyingSystemType);
            }            

            if (target == null) {
                target = Ast.Throw(MakeAmbigiousMatchError(mg));
            } else {
                target = rule.MakeReturn(ab, target);
            }

            return target;
        }

        private Expression GetTrackerOrError<T>(CodeContext/*!*/ context, OldGetMemberAction action, RuleBuilder<T> rule, ActionBinder ab, string name, MemberGroup mg) where T : class {
            MemberTracker mt = GetTracker(ab, name, mg);
            
            if (mt != null && mt.MemberType == TrackerTypes.Property) {
                ExtensionPropertyTracker ept = mt as ExtensionPropertyTracker;
                if (ept != null) {
                    MethodInfo mi = ept.GetGetMethod(true);
                    if (mi != null && mi.IsDefined(typeof(WrapperDescriptorAttribute), false)) {
                        mt = mt.BindToInstance(Ast.ConvertHelper(rule.Parameters[0], typeof(PythonType)));
                    }
                }
            }

            Expression target = null;
            if (mt != null) {
                // unfortunately we need to bind using the PythonType for class methods, not the .NET type, so
                // we have a special case here for that.
                PythonCustomTracker cmt = mt as PythonCustomTracker;
                if (cmt != null) {
                    target = cmt.GetBoundPythonValue(rule, ab, this);
                } else {
                    target = mt.GetValue(rule.Context, ab, UnderlyingSystemType);
                }
            }

            if (target != null) {
                if (IsPythonType && !PythonTypeOps.GetSlot(mg, name, ab.PrivateBinding).IsAlwaysVisible) {
                    Expression error;

                    if (action.IsNoThrow) {
                        error = MakeNoThrowRule<T>(rule, ab);
                    } else {
                        error = context.LanguageContext.Binder.MakeMissingMemberErrorInfo(UnderlyingSystemType, name).MakeErrorForRule(rule, ab);
                    }

                    target = Ast.IfThenElse(
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("IsClsVisible"),
                                    rule.Context
                                ),
                                rule.MakeReturn(ab, target),
                                error
                            );
                } else {
                    target = rule.MakeReturn(ab, target);
                }
            } else {
                target = MakeGetErrorTarget<T>(action, rule, ab, mg);
            }

            return target;
        }

        private static Expression MakeGetErrorTarget<T>(OldGetMemberAction action, RuleBuilder<T> rule, ActionBinder ab, MemberGroup mg) where T : class {
            if (action.IsNoThrow) {
                return MakeNoThrowRule<T>(rule, ab);
            } else if (mg.Count == 1) {
                MemberTracker mt = mg[0];

                if (mt.DeclaringType.ContainsGenericParameters) {
                    return ab.MakeContainsGenericParametersError(mt).MakeErrorForRule(rule, ab);
                }                
            } 

            return Ast.Throw(MakeAmbigiousMatchError(mg));
        }

        private static Expression MakeAmbigiousMatchError(MemberGroup members) {
            StringBuilder sb = new StringBuilder();
            foreach (MethodTracker mi in members) {
                if (sb.Length != 0) sb.Append(", ");
                sb.Append(mi.MemberType);
                sb.Append(" : ");
                sb.Append(mi.ToString());
            }

            return Ast.New(typeof(AmbiguousMatchException).GetConstructor(new Type[] { typeof(string) }),
                        Ast.Constant(sb.ToString()));
        }

        private MemberTracker GetTracker(ActionBinder binder, string name, MemberGroup mg) {
            mg = PythonTypeOps.FilterNewSlots(mg);
            TrackerTypes mt = PythonTypeOps.GetMemberType(mg);
            MemberTracker tracker;

            switch (mt) {
                case TrackerTypes.All:  
                    return null;
                case TrackerTypes.Method:
                    tracker = ReflectionCache.GetMethodGroup(name, mg);
                    break;
                case TrackerTypes.TypeGroup:
                case TrackerTypes.Type:
                    tracker = GetTypeGroup(mg);
                    break;
                default:
                    tracker = mg[0];
                    break;
            }

            return tracker;
        }

        private static TypeTracker GetTypeGroup(MemberGroup members) {
            TypeTracker typeTracker = (TypeTracker)members[0];
            for (int i = 1; i < members.Count; i++) {
                typeTracker = TypeGroup.UpdateTypeEntity(typeTracker, (TypeTracker)members[i]);
            }
            return typeTracker;
        }
        
        #endregion

        #region IMembersList Members

        IList<object> IMembersList.GetMemberNames(CodeContext context) {
            IList<object> res = GetMemberNames(context);

            object[] arr = new object[res.Count];
            res.CopyTo(arr, 0);

            Array.Sort(arr);
            return arr;
        }

        #endregion        

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            return _weakrefTracker;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            return Interlocked.CompareExchange<WeakRefTracker>(ref _weakrefTracker, value, null) == null;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            _weakrefTracker = value;
        }

        #endregion

        #region IDynamicObject Members

        [PythonHidden]
        public MetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Binding.MetaPythonType(parameter, Restrictions.Empty, this);
        }

        #endregion
    }
}
