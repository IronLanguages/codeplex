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
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public delegate bool TryGetMemberCustomizer(CodeContext context, object instance, SymbolId name, out object value);
    public delegate void SetMemberCustomizer(CodeContext context, object instance, SymbolId name, object value);
    public delegate void DeleteMemberCustomizer(CodeContext context, object instance, SymbolId name);

    public delegate PythonTypeSlot CreateTypeSlot(object value);

    public delegate bool UnaryOperator(CodeContext context, object self, out object res);
    public delegate bool BinaryOperator(CodeContext context, object self, object other, out object res);
    public delegate bool TernaryOperator(CodeContext context, object self, object value1, object value2, out object res);

    /// <summary>
    /// Represents a PythonType.  Instances of PythonType are created via PythonTypeBuilder.  
    /// </summary>
#if !SILVERLIGHT
    [DebuggerDisplay("PythonType: {Name}")]
#endif
    public class PythonType : ICustomMembers, IConstructorWithCodeContext, IDynamicObject {
        private string _name;                               // the name of the type
        internal PythonTypeAttributes _attrs;              // attributes of the type
        private List<PythonType> _resolutionOrder;        // the search order for methods in the type
        private Dictionary<SymbolId, SlotInfo> _dict;       // type-level slots & attributes
        private VTable _operators;                          // table of operators for fast dispatch    
        private ContextId _context;                         // the context this type was created from
        internal PythonTypeBuilder _builder;              // the builder who created this, or null if we're fully initialized        
        private TryGetMemberCustomizer _getboundmem;        // customized delegate for getting a member
        private SetMemberCustomizer _setmem;                // customized delegate for setting a member
        private DeleteMemberCustomizer _delmem;             // customized delegate fr deleting values.
        private CreateTypeSlot _slotCreator;                // used for creating default value slot (used for Python user types so we can implement user-defined descriptor protocol).
        private List<object> _contextTags;                  // tag info specific to the context
        private int _version = GetNextVersion();            // version of the type
        private int _altVersion;                            // the alternate version of  the type, when the version is DynamicVersion
        private bool _hasGetAttribute;                      // true if the type has __getattribute__, false otherwise.

        private List<PythonType> _bases;                   // the base classes of the type
        private object _ctor;                               // fast implementation of ctor
        private List<WeakReference> _subtypes;              // all of the subtypes of the PythonType
        private Type _underlyingSystemType;                 // the underlying CLI system type for this type
        private Type _extensionType;                        // a type that can be extended but acts like the underlying system type
        private Type _impersonationType;                    // the type we should pretend to be
        private List<ConversionInfo> _conversions;          // list of built-in conversions 
        private List<bool> _allowKeywordCtor;               // true if a context disallows keyword args constructing the type.
        private bool _extended;
        private DynamicSite<object, object[], object> _ctorSite;

        public const int DynamicVersion = Int32.MinValue;   // all lookups should be dynamic
        private static int MasterVersion = 1, MasterAlternateVersion;
        private static Dictionary<Type, PythonType> _pythonTypes = new Dictionary<Type, PythonType>();
        internal static PythonType _pythonTypeType = DynamicHelpers.GetPythonTypeFromType(typeof(PythonType));
        private static WeakReference[] _emptyWeakRef = new WeakReference[0];
        private static PythonType _nullType = DynamicHelpers.GetPythonTypeFromType(typeof(None));

        public PythonType(Type underlyingSystemType) {
            _bases = new List<PythonType>(1);
            UnderlyingSystemType = underlyingSystemType;
            _resolutionOrder = new List<PythonType>(1);
            _resolutionOrder.Add(this);
        }

        /// <summary>
        /// Gets the dynamic type that corresponds with the provided static type. 
        /// 
        /// Returns null if no type is available.  TODO: In the future this will
        /// always return a PythonType created by the DLR.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PythonType GetPythonType(Type type) {
            lock (_pythonTypes) {
                PythonType res;
                if (_pythonTypes.TryGetValue(type, out res)) return res;
            }

            return null;
        }

        /// <summary>
        /// Sets the dynamic type that corresponds with the given CLR type.
        /// 
        /// Deprecated on introduction and will be removed.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pythonType"></param>
        public static PythonType SetPythonType(Type type, PythonType pythonType) {
            Contract.RequiresNotNull(pythonType, "pythonType");

            lock (_pythonTypes) {
                // HACK: Work around until Ops doesn't have SavePythonType and this is entirely thread safe.
                PythonType res;
                if (_pythonTypes.TryGetValue(type, out res)) return res;

                _pythonTypes[type] = pythonType;
            }
            return pythonType;
        }

        /// <summary>
        /// Creates an instance of the dynamic type and runs appropriate class initialization
        /// </summary>
        public object CreateInstance(CodeContext context, params object[] args) {
            Contract.RequiresNotNull(args, "args");

            Initialize();

            if (_ctor != null) {
                if (_ctorSite == null) _ctorSite = DynamicSite<object, object[], object>.Create(CallAction.Make(new CallSignature(new ArgumentInfo(ArgumentKind.List))));
                return _ctorSite.Invoke(context, _ctor, args);
            }
#if SILVERLIGHT
            throw new MissingMethodException(Name + ".ctor");
#else
            throw new MissingMethodException(Name, ".ctor");
#endif
        }

        /// <summary>
        /// Creats an instance of the object using keyword parameters.
        /// </summary>
        public object CreateInstance(CodeContext context, object[] args, string[] names) {
            Contract.RequiresNotNull(names, "names");
            Contract.RequiresNotNull(names, "names");

            Initialize();

            IFancyCallable ifc = _ctor as IFancyCallable;
            if (ifc == null) throw new InvalidOperationException(
                String.Format(
                    CultureInfo.CurrentCulture,
                    IronPython.Resources.KeywordCreateUnavailable,
                    _ctor.GetType()));

            return ifc.Call(context, args, names);
        }

        public bool CanConvertTo(Type to) {
            Initialize();

            if (_conversions != null) {
                for (int i = 0; i < _conversions.Count; i++) {
                    if (_conversions[i].To == to) {
                        return true;
                    }
                }
            }
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public bool TryConvertTo(object value, PythonType to, out object result) {
            Initialize();

            if (_conversions != null) {
                for (int i = 0; i < _conversions.Count; i++) {
                    if (_conversions[i].To == to.UnderlyingSystemType) {
                        result = _conversions[i].Converter(value);
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }

        public bool CanConvertFrom(Type type) {
            Initialize();

            if (_conversions != null) {
                for (int i = 0; i < _conversions.Count; i++) {
                    if (_conversions[i].To == this.UnderlyingSystemType &&
                        _conversions[i].From.IsAssignableFrom(type)) {
                        return true;
                    }
                }
            }
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public bool TryConvertFrom(object value, out object result) {
            Initialize();

            if (_conversions != null) {
                for (int i = 0; i < _conversions.Count; i++) {
                    if (_conversions[i].To == this.UnderlyingSystemType &&
                        _conversions[i].From.IsAssignableFrom(value.GetType())) {

                        result = _conversions[i].Converter(value);
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Gets the underlying system type that is backing this type.  All instances of this
        /// type are an instance of the underlying system type.
        /// </summary>
        public Type UnderlyingSystemType {
            get {
                // if we already have the underlying system type don't
                // do a full initialization.  This saves us several type inits
                // on startup.
                if (_underlyingSystemType == null) {
                    Initialize();
                }

                return _underlyingSystemType;
            }
            set {
                _underlyingSystemType = value;
            }
        }

        /// <summary>
        /// Gets the extension type for this type.  The extension type provides
        /// extra methods that logically appear on this type.
        /// </summary>
        public Type ExtensionType {
            get {
                if (_extensionType == null) {
                    return _underlyingSystemType;
                }
                return _extensionType;
            }
            internal set {
                _extensionType = value;
            }
        }

        /// <summary>
        /// Substitutes the provided types for the parameters of the generic type definition and
        /// returns a new PythonType for the constructed type.
        /// </summary>
        public Type MakeGenericType(params PythonType[] types) {
            Initialize();

            Contract.RequiresNotNull(types, "types");
            if (!UnderlyingSystemType.ContainsGenericParameters)
                throw new InvalidOperationException(IronPython.Resources.InvalidOperation_MakeGenericOnNonGeneric);

            Type[] sysTypes = new Type[types.Length];
            for (int i = 0; i < sysTypes.Length; i++) sysTypes[i] = types[i].UnderlyingSystemType;

            //!!! propagate attributes?
            return _underlyingSystemType.MakeGenericType(sysTypes);
        }

        /// <summary>
        /// Returns true if the specified object is an instance of this type.
        /// </summary>
        public bool IsInstanceOfType(object instance) {
            Initialize();

            IPythonObject dyno = instance as IPythonObject;
            if (dyno != null) {
                return dyno.PythonType.IsSubclassOf(this);
            }

            return UnderlyingSystemType.IsInstanceOfType(instance);
        }

        /// <summary>
        /// Gets a list of weak references to all the subtypes of this class.  May return null
        /// if there are no subtypes of the class.
        /// </summary>
        public IList<WeakReference> SubTypes {
            get {
                if (_subtypes == null) return _emptyWeakRef;

                lock (_subtypes) return _subtypes.ToArray();
            }
        }

        /// <summary>
        /// Gets a description of where this type from.  This may include if it's a system type,
        /// language specific information, or other information.  This should be used for displaying
        /// information about the type to the user.  For programmatic discovery other properties should
        /// be used.
        /// </summary>
        public string TypeCategory {
            get {
                if (IsSystemType) return "built-in type";

                return "user-type"; //!!! language info as well?
            }
        }

        /// <summary>
        /// Gets the base types from which this type inherits.
        /// </summary>
        public IList<PythonType> BaseTypes {
            get {
                Initialize();

                lock (_bases) return _bases.ToArray();
            }
            internal set {
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
        public bool IsSubclassOf(PythonType other) {
            Initialize();

            // check for a type match
            if (other.CanonicalPythonType == this.CanonicalPythonType) {
                return true;
            }

            //Python doesn't have value types inheriting from ValueType, but we fake this for interop
            if (other.UnderlyingSystemType == typeof(ValueType) && UnderlyingSystemType.IsValueType) {
                return true;
            }

            // check for a match on UnderlyingSystemType if other is
            // a system type.
            if ((other._attrs & PythonTypeAttributes.SystemType) != 0) {
                if (UnderlyingSystemType == other.UnderlyingSystemType) {
                    return true;
                }
            }

            // check the type hierarchy
            List<PythonType> bases = _bases;
            for (int i = 0; i < bases.Count; i++) {
                PythonType baseClass = bases[i];

                if (baseClass.IsSubclassOf(other)) return true;
            }

            // check for our impersonation type
            if (_impersonationType != null) {
                if (_impersonationType == other.UnderlyingSystemType ||
                    _impersonationType.IsSubclassOf(other.UnderlyingSystemType))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// True if the type is a system type.  A system type is a type which represents an
        /// underlying .NET type and not a subtype of one of these types.
        /// </summary>
        public bool IsSystemType {
            get {
                return (_attrs & PythonTypeAttributes.SystemType) != 0;
            }
            internal set {
                if (value) _attrs |= PythonTypeAttributes.SystemType;
                else _attrs &= (~PythonTypeAttributes.SystemType);
            }
        }

        /// <summary>
        /// Gets the type that this type impersonates.  This type will appear to
        /// be equivalent to the type it's impersonating.  Returns null if 
        /// the type doesn't impersonate a type.
        /// </summary>
        public Type ImpersonationType {
            get {
                Initialize();

                return _impersonationType;
            }
            internal set {
                _impersonationType = value;
            }
        }

        /// <summary>
        /// Always returns this type - unless this type is impersonating another one.  In
        /// that case it will return the type that is being impersonated.
        /// </summary>
        public PythonType CanonicalPythonType {
            get {
                if (ImpersonationType != null) {
                    return DynamicHelpers.GetPythonTypeFromType(ImpersonationType);
                } else {
                    return this;
                }
            }
        }

        /// <summary>
        /// Internal helper function to add a subtype
        /// </summary>
        internal void AddSubType(PythonType subtype) {
            if (_subtypes == null) {
                Interlocked.CompareExchange<List<WeakReference>>(ref _subtypes, new List<WeakReference>(), null);
            }

            lock (_subtypes) {
                _subtypes.Add(new WeakReference(subtype));
            }
        }

        internal void RemoveSubType(PythonType subtype) {
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

        internal void AddBaseType(PythonType baseType) {
            if (_bases == null) {
                Interlocked.CompareExchange<List<PythonType>>(ref _bases, new List<PythonType>(), null);
            }

            lock (_bases) _bases.Add(baseType);
        }

        internal void AddConversion(Type from, Type to, CallTarget1 conversion) {
            if (_conversions == null) _conversions = new List<ConversionInfo>();

            ConversionInfo ci = new ConversionInfo();
            ci.From = from;
            ci.To = to;
            ci.Converter = conversion;
            _conversions.Add(ci);
        }


        internal void SetConstructor(object ctor) {
            _ctor = ctor;
        }

        private class ConversionInfo {
            public Type From;
            public Type To;
            public CallTarget1 Converter;

        }

        #region IConstructorWithCodeContext Members

        object IConstructorWithCodeContext.Construct(CodeContext context, params object[] args) {
            return CreateInstance(context, args);
        }

        #endregion

        internal PythonTypeBuilder Builder {
            get {
                return (PythonTypeBuilder)_builder;
            }
            set {
                Debug.Assert(_builder == null || value == null);
                _builder = value;
            }
        }


        public bool IsExtended {
            get {
                return _extended;
            }
            set {
                _extended = value;
            }
        }

        internal void DisallowConstructorKeywordArguments(ContextId context) {
            if (_allowKeywordCtor == null) _allowKeywordCtor = new List<bool>();

            while (_allowKeywordCtor.Count <= context.Id)
                _allowKeywordCtor.Add(true);
            _allowKeywordCtor[context.Id] = false;
        }

        private void UpdateVersion() {
            foreach (WeakReference wr in SubTypes) {
                if (wr.IsAlive) {
                    ((PythonType)wr.Target).UpdateVersion();
                }
            }

            if (_version != DynamicVersion) {
                _version = GetNextVersion();
                _altVersion = 0;
            } else {
                _altVersion = GetNextAlternateVersion();
            }
        }

        #region Object overrides

        // TODO Remove this method.  It was needed when Equals was overridden to depend on 
        // the impersonationType, but now that that's gone this can probably go as well.
        // Currently only pickling depends on this where identity of types is checked.
        public override int GetHashCode() {
            if (_impersonationType == null) return ~_underlyingSystemType.GetHashCode();

            return ~_impersonationType.GetHashCode();
        }

        #endregion

        #region IDynamicObject Members

        public LanguageContext LanguageContext {
            get { return InvariantContext.Instance; }
        }

        public StandardRule<T> GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            if (action.Kind == DynamicActionKind.CreateInstance) {
                if (IsSystemType) {
                    MethodBase[] ctors = CompilerHelpers.GetConstructors(UnderlyingSystemType);
                    StandardRule<T> rule;
                    if (ctors.Length > 0) {
                        rule = new CallBinderHelper<T, CallAction>(context, (CallAction)action, args, ctors).MakeRule();
                    } else {
                        rule = new StandardRule<T>();
                        rule.SetTarget(
                           rule.MakeError(
                               Ast.New(
                                   typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                                   Ast.Constant("Cannot create instances of " + Name)
                               )
                           )
                       );
                    }
                    rule.AddTest(Ast.Equal(rule.Parameters[0], Ast.RuntimeConstant(args[0])));
                    return rule;
                } else {
                    // TODO: Pull in the Python create logic for this when PythonType moves out of MS.Scripting, this provides
                    // a minimal level of interop until then.
                    StandardRule<T> rule = new StandardRule<T>();
                    Expression call = Ast.ComplexCallHelper(
                        Ast.Convert(rule.Parameters[0], typeof(IConstructorWithCodeContext)),
                        typeof(IConstructorWithCodeContext).GetMethod("Construct"),
                        ArrayUtils.Insert((Expression)Ast.CodeContext(), ArrayUtils.RemoveFirst(rule.Parameters))
                    );

                    rule.SetTarget(rule.MakeReturn(context.LanguageContext.Binder, call));
                    rule.SetTest(Ast.Equal(rule.Parameters[0], Ast.RuntimeConstant(args[0])));
                    return rule;
                }
            }
            return null;
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator Type(PythonType self) {
            return self.UnderlyingSystemType;
        }

        public static implicit operator TypeTracker(PythonType self) {
            return self.ToTypeTracker();
        }

        public TypeTracker ToTypeTracker() {
            return ReflectionCache.GetTypeTracker(UnderlyingSystemType);
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

        private static int GetNextAlternateVersion() {
            if (MasterAlternateVersion  < 0) {
                throw new InvalidOperationException(IronPython.Resources.TooManyVersions);
            }
            return Interlocked.Increment(ref MasterAlternateVersion);
        }

        public void Mutate() {
            UpdateVersion();
        }

        #region Public API Surface
      
        /// <summary>
        /// Looks up a slot on the dynamic type
        /// Just looks into the context sensitive slots.
        /// </summary>
        public bool TryLookupContextSlot(CodeContext context, SymbolId name, out PythonTypeSlot slot) {
            Contract.RequiresNotNull(context, "context");
            Contract.Requires(context.LanguageContext.ContextId != ContextId.Empty, "context", "Default context not allowed.");
            
            Initialize();

            SlotInfo si;
            bool success = _dict.TryGetValue(name, out si);

            if (success) {
                // check for a context-sensitive slot only
                if (si.SlotValues != null && si.SlotValues.Count > context.LanguageContext.ContextId.Id) {
                    slot = si.SlotValues[context.LanguageContext.ContextId.Id];
                    if (slot != null) return true;
                }
            }

            slot = null;
            return false;
        }

        /// <summary>
        /// Looks up a slot on the dynamic type
        /// </summary>
        public bool TryLookupSlot(CodeContext context, SymbolId name, out PythonTypeSlot slot) {
            Initialize();

            SlotInfo si;
            if (_dict.TryGetValue(name, out si)) {
                return TryExtractVisibleSlot(context, si, out slot);
            } else {
                slot = null;
                return false;
            }
        }

        private bool TryExtractVisibleSlot(CodeContext context, SlotInfo si, out PythonTypeSlot slot) {
            // check for a context-sensitive slot first...
            if (si.SlotValues != null && si.SlotValues.Count > context.LanguageContext.ContextId.Id) {
                slot = si.SlotValues[context.LanguageContext.ContextId.Id];
                if (slot != null) {
                    return slot.IsVisible(context, this);
                }
            }

            // then see if we have a normal slot
            if (si.DefaultValue != null && si.DefaultValue.IsVisible(context, this)) {
                slot = si.DefaultValue;
                return true;
            }

            slot = null;
            return false;
        }

        /// <summary>
        /// Searches the resolution order for a slot matching by name
        /// </summary>
        public bool TryResolveSlot(CodeContext context, SymbolId name, out PythonTypeSlot slot) {
            for(int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                if (dt.TryLookupSlot(context, name, out slot)) {
                    return true;
                }
            }

            slot = null;
            return false;
        }

        private enum CaseInsensitiveMatch {
            NoMatch,                    // name doesn't exist on the type at all
            ExactMatch,                 // name exists with the exact casing
            InexactMatch,               // there's exactly one match, but with different casing
            AmbiguousMatch              // multiple instances of the different casings, none matches exactly
        }

        /// <summary>
        /// Searches the resolution order for the slots that match the name in case insensitive manner.
        /// </summary>
        public bool TryResolveSlotCaseInsensitive(CodeContext context, SymbolId name, out PythonTypeSlot slot, out SymbolId actualName) {
            // Initialize the output parameters
            slot = null;
            actualName = SymbolId.Invalid;
            bool ambiguous = false;

            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonTypeSlot candidate;
                SymbolId candidateName;
                switch (_resolutionOrder[i].TryLookupSlotCaseInsensitive(context, name, out candidate, out candidateName)) {
                    case CaseInsensitiveMatch.ExactMatch:
                        // exact match - search is over
                        slot = candidate;
                        actualName = candidateName;
                        return true;

                    case CaseInsensitiveMatch.InexactMatch:
                        // inexact match. If we already have inexact candidate, we have ambiguous lookup,
                        // unless we find exact match later
                        if (slot == null) {
                            // first possible match encountered
                            slot = candidate;
                            actualName = candidateName;
                        } else {
                            // if the name is the same, we are ok, in that case continue to use the first
                            // match we found. If the name doesn't match, we have ambiguous result, unless
                            // we find exact match later.
                            if (candidateName != actualName) {
                                ambiguous = true;
                            }
                        }
                        break;

                    case CaseInsensitiveMatch.AmbiguousMatch:
                        // ambiguous match. We need to find an exact match to succeed.
                        ambiguous = true;
                        break;

                    case CaseInsensitiveMatch.NoMatch:
                        // no match - keep looking with the parent.
                        break;
                }
            }

            if (slot != null && !ambiguous) {
                return true;
            } else {
                slot = null;
                actualName = SymbolId.Invalid;
                return false;
            }
        }

        /// <summary>
        /// Looks up the slots on the dynamic mixin, using case insensitive comparison.
        /// Matching slots are added to the list.
        /// </summary>
        private CaseInsensitiveMatch TryLookupSlotCaseInsensitive(CodeContext context, SymbolId name, out PythonTypeSlot slot, out SymbolId actualName) {
            bool ambiguous = false;

            // Initialize the result
            slot = null;
            actualName = SymbolId.Invalid;

            foreach (KeyValuePair<SymbolId, SlotInfo> kvp in _dict) {
                PythonTypeSlot current;
                if (kvp.Key.CaseInsensitiveEquals(name) &&
                    TryExtractVisibleSlot(context, kvp.Value, out current)) {

                    // We have case insensitive match. Is it an exact match?
                    if (kvp.Key == name) {
                        slot = current;
                        actualName = kvp.Key;
                        return CaseInsensitiveMatch.ExactMatch;
                    }

                    // We have case insensitive match only. Is it the first one?
                    if (slot == null) {
                        slot = current;
                        actualName = kvp.Key;
                    } else {
                        // Already have case insensitive match, so unless we find exact match later,
                        // this is an ambiguous match.
                        ambiguous = true;
                    }
                }
            }

            // Do we have at least one candidate?
            if (slot != null) {
                // Do we have more than one?
                if (ambiguous) {
                    slot = null;
                    actualName = SymbolId.Invalid;
                    return CaseInsensitiveMatch.AmbiguousMatch;
                } else {
                    // Exactly one candidate based on insensitive match
                    return CaseInsensitiveMatch.InexactMatch;
                }
            } else {
                // nothing found
                return CaseInsensitiveMatch.NoMatch;
            }
        }

        #region Instance Access Helpers

        public object GetMember(CodeContext context, object instance, SymbolId name) {
            object res;
            if (TryGetMember(context, instance, name, out res)) {
                return res;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture,
                IronPython.Resources.CantFindMember, 
                SymbolTable.IdToString(name)));
        }

        public object GetBoundMember(CodeContext context, object instance, SymbolId name) {
            object value;
            if (TryGetBoundMember(context, instance, name, out value)) {
                return value;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture,
                IronPython.Resources.CantFindMember,
                SymbolTable.IdToString(name)));
        }

        public void SetMember(CodeContext context, object instance, SymbolId name, object value) {
            if (TrySetMember(context, instance, name, value)) {
                return;
            }

            throw new MissingMemberException(
                String.Format(CultureInfo.CurrentCulture,
                    IronPython.Resources.Slot_CantSet, 
                    name));
        }

        public void DeleteMember(CodeContext context, object instance, SymbolId name) {
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
        public bool TryGetMember(CodeContext context, object instance, SymbolId name, out object value) {
            Initialize();

            //if (_getmem != null) {
            //    return _getmem(context, instance, name, out value);
            //}

            return TryGetNonCustomMember(context, instance, name, out value);
        }

        /// <summary>
        /// Attempts to lookup a member w/o using the customizer.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public bool TryGetNonCustomMember(CodeContext context, object instance, SymbolId name, out object value) {
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac != null && iac.TryGetValue(name, out value)) {
                    return true;
                }
            }

            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                PythonTypeSlot slot;
                if (dt.TryLookupSlot(context, name, out slot)) {
                    if (slot.TryGetValue(context, instance, this, out value))
                        return true;
                }
            }

            try {
                if (TryInvokeBinaryOperator(context, Operators.GetBoundMember, instance, name.ToString(), out value)) {
                    return true;
                }
            } catch (MissingMemberException) {
                //!!! when do we let the user see this exception?
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Gets a value from a dynamic type and any sub-types.  Values are stored in slots (which serve as a level of 
        /// indirection).  This searches the types resolution order and returns the first slot that
        /// contains the value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public bool TryGetBoundMember(CodeContext context, object instance, SymbolId name, out object value) {
            Initialize();

            if (_getboundmem != null) {
                return _getboundmem(context, instance, name, out value);
            }

            return TryGetNonCustomBoundMember(context, instance, name, out value);
        }


        /// <summary>
        /// Attempts to lookup a member w/o using the customizer.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public bool TryGetNonCustomBoundMember(CodeContext context, object instance, SymbolId name, out object value) {
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac != null && iac.TryGetValue(name, out value)) {
                    return true;
                }
            }

            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                PythonTypeSlot slot;
                if (dt.TryLookupSlot(context, name, out slot)) {
                    if (slot.TryGetBoundValue(context, instance, this, out value))
                        return true;
                }
            }

            try {
                if (TryInvokeBinaryOperator(context, Operators.GetBoundMember, instance, name.ToString(), out value)) {
                    return true;
                }
            } catch (MissingMemberException) {
                //!!! when do we let the user see this exception?
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Sets a value on an instance.  If a slot is available in the most derived type the slot
        /// is set there, otherwise the value is stored directly in the instance.
        /// </summary>
        public bool TrySetMember(CodeContext context, object instance, SymbolId name, object value) {
            Initialize();

            if (_setmem != null) {
                _setmem(context, instance, name, value);
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
        public bool TrySetNonCustomMember(CodeContext context, object instance, SymbolId name, object value) {
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
                    iac = new SymbolDictionary();

                    if ((iac = sdo.SetDict(iac))==null) {
                        return false;
                    }
                }

                iac[name] = value;
                return true;
            }

            object dummy;
            return TryInvokeTernaryOperator(context, Operators.SetMember, instance, SymbolTable.IdToString(name), value, out dummy);
        }

        public bool TryDeleteMember(CodeContext context, object instance, SymbolId name) {
            Initialize();

            if (_delmem != null) {
                _delmem(context, instance, name);
                return true;
            }

            return TryDeleteNonCustomMember(context, instance, name);
        }

        public bool TryDeleteNonCustomMember(CodeContext context, object instance, SymbolId name) {
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
                    iac = new SymbolDictionary();

                    if ((iac = sdo.SetDict(iac))==null) {
                        return false;
                    }
                }

                return iac.Remove(name);
            }

            try {
                object value;
                if (TryInvokeBinaryOperator(context, Operators.DeleteMember, instance, name.ToString(), out value)) {
                    return true;
                }
            } catch (MissingMemberException) {
                //!!! when do we let the user see this exception?
            }


            return false;
        }
        #endregion

        /// <summary>
        /// Returns a list of all slot names for this type.
        /// </summary>
        /// <param name="context">The context that is doing the inquiry of InvariantContext.Instance.</param>
        public IList<SymbolId> GetTypeMembers(CodeContext context) {
            Initialize();

            int ctxId = context.LanguageContext.ContextId.Id;
            Dictionary<SymbolId, SymbolId> keys = new Dictionary<SymbolId, SymbolId>();

            foreach (KeyValuePair<SymbolId, SlotInfo> kvp in _dict) {
                if (keys.ContainsKey(kvp.Key)) continue;

                if (kvp.Value.SlotValues != null) {
                    if (kvp.Value.SlotValues.Count > ctxId &&
                        kvp.Value.SlotValues[ctxId] != null) {

                        if (kvp.Value.SlotValues[ctxId].IsVisible(context, this)) {
                            keys[kvp.Key] = kvp.Key;
                            continue;
                        }
                    }
                }

                if (kvp.Value.DefaultValue != null && kvp.Value.DefaultValue.IsVisible(context, this))
                    keys[kvp.Key] = kvp.Key;
            }

            return new List<SymbolId>(keys.Keys);
        }

        /// <summary>
        /// Returns a list of all slot names for the type and any subtypes.
        /// </summary>
        /// <param name="context">The context that is doing the inquiry of InvariantContext.Instance.</param>
        public IList<SymbolId> GetMemberNames(CodeContext context) {
            return GetMemberNames(context, null);
        }

        /// <summary>
        /// Returns a list of all slot names for the type, any subtypes, and the instance.
        /// </summary>
        /// <param name="context">The context that is doing the inquiry of InvariantContext.Instance.</param>
        /// <param name="self">the instance to get instance members from, or null.</param>
        public IList<SymbolId> GetMemberNames(CodeContext context, object self) {
            //!!! context, object keys ? 
            Initialize();

            int ctxId = context.LanguageContext.ContextId.Id;
            Dictionary<SymbolId, SymbolId> keys = new Dictionary<SymbolId, SymbolId>();

            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];
                dt.Initialize();

                foreach (KeyValuePair<SymbolId, SlotInfo> kvp in dt._dict) {                    
                    if (keys.ContainsKey(kvp.Key)) continue;

                    if (kvp.Value.SlotValues != null) {
                        if (kvp.Value.SlotValues.Count > ctxId &&
                            kvp.Value.SlotValues[ctxId] != null) {

                            if (kvp.Value.SlotValues[ctxId].IsVisible(context, this)) {
                                keys[kvp.Key] = kvp.Key;
                            }
                            continue;
                        }
                    }

                    if (kvp.Value.DefaultValue != null && kvp.Value.DefaultValue.IsVisible(context, this))
                        keys[kvp.Key] = kvp.Key;
                }                
            }

            IPythonObject dyno = self as IPythonObject;
            if (dyno != null) {
                IAttributesCollection iac = dyno.Dict;
                if (iac != null) {
                    lock (iac) {
                        foreach (SymbolId id in iac.SymbolAttributes.Keys) {
                            keys[id] = id;
                        }
                    }
                }
            }

            object names;
            if (self != null && TryInvokeUnaryOperator(context, Operators.GetMemberNames, self, out names)) {
                IList<SymbolId> symNames = names as IList<SymbolId>;
                if (symNames == null) throw new InvalidOperationException(String.Format("GetMemberNames returned bad list: {0}", names));

                foreach (SymbolId si in symNames) {
                    keys[si] = si;
                }
            }

            return new List<SymbolId>(keys.Keys);
        }        

        public IAttributesCollection GetMemberDictionary(CodeContext context) {
            Initialize();

            IAttributesCollection iac = new SymbolDictionary();
            foreach (SymbolId x in _dict.Keys) {             
                if(x.ToString() == "__dict__") continue;

                PythonTypeSlot dts;
                if (TryLookupSlot(context, x, out dts)) {
                    //??? why check for DTVS?
                    object val;
                    if (dts.TryGetValue(context, null, this, out val)) {
                        if (dts is PythonTypeValueSlot)
                            iac[x] = val;
                        else
                            iac[x] = dts;
                    }
                }
            }
            return iac;
        }

        public IAttributesCollection GetMemberDictionary(CodeContext context, object self) {
            if (self != null) {
                IPythonObject sdo = self as IPythonObject;
                if (sdo != null) return sdo.Dict;

                return null;
            }
            return GetMemberDictionary(context);
        }

        /// <summary>
        /// Attempts to invoke the specific unary operator with the given instance
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public bool TryInvokeUnaryOperator(CodeContext context, Operators op, object self, out object ret) {
            Contract.RequiresNotNull(context, "context"); 
            
            Initialize();

            int opIndex = (int)op;

            if (_operators != null) {
                // check context specific version, if available
                if (_operators.ContextSpecific != null) {
                    ContextId ctxId = context.LanguageContext.ContextId;
                    if (_operators.ContextSpecific.Length > ctxId.Id) {
                        VTable vt = _operators.ContextSpecific[ctxId.Id];

                        if (vt != null && vt.UnaryOperators != null && opIndex < vt.UnaryOperators.Length) {
                            UnaryOperator ct = vt.UnaryOperators[opIndex].Operator;
                            if (ct != null && ct(context, self, out ret)) {
                                return true;
                            }
                        }
                    }
                }

                // check context neutral version
                if (_operators.UnaryOperators != null && opIndex < _operators.UnaryOperators.Length) {
                    UnaryOperator ct = _operators.UnaryOperators[opIndex].Operator;
                    if (ct != null && ct(context, self, out ret)) {
                        return true;
                    }
                }
            }

            ret = null;
            return false;
        }

        /// <summary>
        /// Attempts to invoke the specific binary operator with the given instance
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public bool TryInvokeBinaryOperator(CodeContext context, Operators op, object self, object other, out object ret) {
            Contract.RequiresNotNull(context, "context"); 
            
            Initialize();

            int opIndex = (int)op;

            if (_operators != null) {
                // check context specific version, if available
                if (_operators.ContextSpecific != null) {
                    ContextId ctxId = context.LanguageContext.ContextId;
                    if (_operators.ContextSpecific.Length > ctxId.Id) {
                        VTable vt = _operators.ContextSpecific[ctxId.Id];

                        if (vt != null && vt.BinaryOperators != null && opIndex < vt.BinaryOperators.Length) {
                            BinaryOperator ct = vt.BinaryOperators[opIndex].Operator;
                            if (ct != null && ct(context, self, other, out ret)) {
                                return true;
                            }
                        }
                    }
                }

                // check context neutral version
                if (_operators.BinaryOperators != null && opIndex < _operators.BinaryOperators.Length) {
                    BinaryOperator ct = _operators.BinaryOperators[opIndex].Operator;
                    if (ct != null && ct(context, self, other, out ret)) {
                        return true;
                    }
                }
            }

            ret = null;
            return false;
        }

        /// <summary>
        /// Attempts to invoke the specific ternary operator with the given instance
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public bool TryInvokeTernaryOperator(CodeContext context, Operators op, object self, object value1, object value2, out object ret) {
            Contract.RequiresNotNull(context, "context");

            Initialize();

            int opIndex = (int)op;

            if (_operators != null) {
                // check context specific version, if available
                if (_operators.ContextSpecific != null) {
                    ContextId ctxId = context.LanguageContext.ContextId;
                    if (_operators.ContextSpecific.Length > ctxId.Id) {
                        VTable vt = _operators.ContextSpecific[ctxId.Id];

                        if (vt != null && vt.TernaryOperators != null && opIndex < vt.TernaryOperators.Length) {
                            TernaryOperator ct = vt.TernaryOperators[opIndex].Operator;
                            if (ct != null && ct(context, self, value1, value2, out ret)) {
                                return true;
                            }
                        }
                    }
                }

                // check context neutral version    
                if (_operators.TernaryOperators != null &&
                    opIndex < _operators.TernaryOperators.Length) {

                    TernaryOperator ct = _operators.TernaryOperators[opIndex].Operator;
                    if (ct != null && ct(context, self, value1, value2, out ret)) {
                        return true;
                    }
                }
            }

            ret = null;
            return false;
        }

        public object InvokeUnaryOperator(CodeContext context, Operators op, object self) {
            object ret;
            if (TryInvokeUnaryOperator(context, op, self, out ret)) {
                return ret;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture, "missing operator {0}", op.ToString()));
        }

        public object InvokeBinaryOperator(CodeContext context, Operators op, object self, object other) {
            object ret;
            if (TryInvokeBinaryOperator(context, op, self, other, out ret)) {
                return ret;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture, "missing operator {0}", op.ToString()));
        }

        public object InvokeTernaryOperator(CodeContext context, Operators op, object self, object value1, object value2) {
            object ret;
            if (TryInvokeTernaryOperator(context, op, self, value1, value2, out ret)) {
                return ret;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture, "missing operator {0}", op.ToString()));
        }


        public bool HasDynamicMembers(CodeContext context) {
            Initialize();

            if (_operators != null) {
                if (HasGetMem(_operators.BinaryOperators)) return true;

                int ctxId = context.LanguageContext.ContextId.Id;
                if (_operators.ContextSpecific != null &&
                    _operators.ContextSpecific.Length > ctxId && 
                    _operators.ContextSpecific[ctxId] != null) {
                    if (HasGetMem(_operators.ContextSpecific[ctxId].BinaryOperators))
                        return true;
                }
            }

            return false;
        }

        private bool HasGetMem(OperatorReference<BinaryOperator>[] table) {
            int getMemId = (int)Operators.GetBoundMember;
            int altGetMemId = (int)Operators.GetMember;

            if (table != null) {
                if (table.Length > getMemId && table[getMemId].Operator != null) {
                    return true;
                }
                if (table.Length > altGetMemId && table[altGetMemId].Operator != null) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Provides languages an opportunity to track additional language-specific data related
        /// to the type.  Languages store information under the contexts they've allocated and
        /// get and set their own data.
        /// </summary>
        public object GetContextTag(ContextId context) {
            if (_contextTags != null && context.Id < _contextTags.Count) {
                return _contextTags[context.Id];
            }

            return null;
        }

        /// <summary>
        /// Sets the context tag for language independent data for the type.
        /// </summary>
        public void SetContextTag(ContextId context, object value) {
            if (_contextTags == null)
                _contextTags = new List<object>(context.Id+1);

            while (context.Id >= _contextTags.Count) {
                _contextTags.Add(null);                
            }

            _contextTags[context.Id] = value;
        }

        /// <summary>
        /// Gets the name of the dynamic type
        /// </summary>
        public string Name {
            get {
                return _name;
            }
            internal set {
                _name = value;
            }
        }
       
        public int Version {
            get {
                return _version;
            }
        }

        /// <summary>
        /// Temporary until DynamicVersion goes away and we handle dynamic cases in-line.  This provides
        /// a version number which can be used to disambiguate types with a version == DynamicVersion.
        /// </summary>
        public int AlternateVersion {
            get {
                return _altVersion;
            }
        }

        public bool IsNull {
            get { return Object.ReferenceEquals(this, _nullType); }
        }

        /// <summary>
        /// Gets the resolution order used for attribute lookup
        /// </summary>
        public IList<PythonType> ResolutionOrder {
            get {
                Initialize();

                return _resolutionOrder;
            }
            internal set {
                lock (SyncRoot) {
                    _resolutionOrder = new List<PythonType>(value);
                }
            }
        }
        
        /// <summary>
        /// True if the type is immutable, false if changes can be made
        /// </summary>
        public bool IsImmutable {
            get {
                Initialize();

                return (_attrs & PythonTypeAttributes.Immutable) != 0;
            }
            internal set {
                if (value) _attrs |= PythonTypeAttributes.Immutable;
                else _attrs &= (~PythonTypeAttributes.Immutable);
            }
        }
        
        /// <summary>
        /// Gets the ContextId for the context that created this type.
        /// </summary>
        public ContextId TypeContext {
            get {
                Initialize();

                return _context;
            }
            internal set {
                _context = value;
            }
        }
        
        #endregion

        public TryGetMemberCustomizer CustomBoundGetter {
            get {                
                return _getboundmem;
            }
            set {
                if (value != null) {
                    _version = DynamicVersion;
                    _altVersion = GetNextAlternateVersion();
                } else {
                    _version = GetNextVersion();
                    _altVersion = 0;
                }
                _getboundmem = value;
            }
        }

        public SetMemberCustomizer CustomSetter {
            get {
                return _setmem;
            }
            internal set {
                _setmem = value;
            }
        }

        public DeleteMemberCustomizer CustomDeleter {
            get {
                return _delmem;
            }
            internal set {
                _delmem = value;
            }
        }

        public static PythonType NullType {
            get {
                return _nullType;
            }
        }

        public bool HasGetAttribute {
            get {
                return _hasGetAttribute;
            }
            internal set {
                _hasGetAttribute = value;
            }
        }

        #region Internal API Surface

        /// <summary>
        /// Internal helper to add a new slot to the type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="slot"></param>
        internal void AddSlot(SymbolId name, PythonTypeSlot slot) {
            AddSlot(ContextId.Empty, name, slot);
        }

        /// <summary>
        /// Internal helper to add a new slot to the type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="slot"></param>
        /// <param name="context">the context the slot is added for</param>
        internal void AddSlot(ContextId context, SymbolId name, PythonTypeSlot slot) {
            EnsureDict();

            SlotInfo si;
            if (!_dict.TryGetValue(name, out si)) {
                if (context == ContextId.Empty) {
                    _dict[name] = new SlotInfo(slot);
                    return;
                } else {
                    _dict[name] = si = new SlotInfo();                    
                }
            } else if (context == ContextId.Empty) {
                si.DefaultValue = slot;
                return;
            }

            if (si.SlotValues == null) si.SlotValues = new List<PythonTypeSlot>(context.Id + 1);
            if (si.SlotValues.Count <= context.Id) {
                while (si.SlotValues.Count < context.Id) {
                    si.SlotValues.Add(null);
                }
                si.SlotValues.Add(slot);
            } else {
                si.SlotValues[context.Id] = slot;
            }
        }


        /// <summary>
        /// Removes the provided symbol as published under the specified context.
        /// </summary>
        internal bool RemoveSlot(ContextId context, SymbolId name) {
            SlotInfo si;
            if (_dict != null && _dict.TryGetValue(name, out si)) {
                if (si.SlotValues != null && si.SlotValues.Count > context.Id && si.SlotValues[context.Id] != null) {
                    si.SlotValues[context.Id] = null;
                    return true;
                }

                if (si.SlotValues == null) {
                    _dict.Remove(name);
                } else {
                    si.DefaultValue = null;
                }
                return true;
            }

            return false;
        }


        /// <summary>
        /// Adds a new unary operator to the type.  
        /// </summary>
        internal void AddOperator(Operators op, UnaryOperator target) {
            if (_operators == null) _operators = new VTable();

            AddOperator<UnaryOperator>(op, ref _operators.UnaryOperators, target);
            if (_operators.ContextSpecific != null) {
                int opIndex = (int)op;

                for (int i = 0; i < _operators.ContextSpecific.Length; i++) {
                    VTable vt = _operators.ContextSpecific[i];
                    if (vt == null) continue;

                    if(vt.UnaryOperators != null && opIndex<vt.UnaryOperators.Length && vt.UnaryOperators[opIndex].Inheritance != 0) {
                        // non-inherited value hides inherited value
                        vt.UnaryOperators[opIndex].Inheritance = 0;
                        vt.UnaryOperators[opIndex].Operator = null;                        
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new binary operator to the type.  
        /// </summary>
        internal void AddOperator(Operators op, BinaryOperator target) {
            if (_operators == null) _operators = new VTable();

            AddOperator<BinaryOperator>(op, ref _operators.BinaryOperators, target);
            if (_operators.ContextSpecific != null) {
                int opIndex = (int)op;

                for (int i = 0; i < _operators.ContextSpecific.Length; i++) {
                    VTable vt = _operators.ContextSpecific[i];
                    if (vt == null) continue;

                    if (vt.BinaryOperators != null && opIndex < vt.BinaryOperators.Length && vt.BinaryOperators[opIndex].Inheritance != 0) {
                        // non-inherited value hides inherited value
                        vt.BinaryOperators[opIndex].Inheritance = 0;
                        vt.BinaryOperators[opIndex].Operator = null;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new ternary operator to the type.  
        /// </summary>
        internal void AddOperator(Operators op, TernaryOperator target) {
            if (_operators == null) _operators = new VTable();

            AddOperator<TernaryOperator>(op, ref _operators.TernaryOperators, target);
            if (_operators.ContextSpecific != null) {
                int opIndex = (int)op;

                for (int i = 0; i < _operators.ContextSpecific.Length; i++) {
                    VTable vt = _operators.ContextSpecific[i];
                    if (vt == null) continue;

                    if (vt.TernaryOperators != null && opIndex < vt.TernaryOperators.Length && vt.TernaryOperators[opIndex].Inheritance != 0) {
                        // non-inherited value hides inherited value
                        vt.TernaryOperators[opIndex].Inheritance = 0;
                        vt.TernaryOperators[opIndex].Operator = null;                        
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new unary operator to the type that is context-limited
        /// </summary>
        internal void AddOperator(ContextId context, Operators op, UnaryOperator target) {
            if (context == ContextId.Empty) {
                AddOperator(op, target);
                return;
            }

            EnsureContextSlots(context);

            AddOperator<UnaryOperator>(op, ref _operators.ContextSpecific[context.Id].UnaryOperators, target);
        }

        /// <summary>
        /// Adds a new binary operator to the type that is context-limited  
        /// </summary>
        internal void AddOperator(ContextId context, Operators op, BinaryOperator target) {
            if (context == ContextId.Empty) {
                AddOperator(op, target);
                return;
            }

            EnsureContextSlots(context);

            AddOperator<BinaryOperator>(op, ref _operators.ContextSpecific[context.Id].BinaryOperators, target);
        }

        /// <summary>
        /// Adds a new ternary operator to the type that is context-limited
        /// </summary>
        internal void AddOperator(ContextId context, Operators op, TernaryOperator target) {
            if (context == ContextId.Empty) {
                AddOperator(op, target);
                return;
            }

            EnsureContextSlots(context);

            AddOperator<TernaryOperator>(op, ref _operators.ContextSpecific[context.Id].TernaryOperators, target);
        }
        
        internal CreateTypeSlot SlotCreator  {
            get {
                return _slotCreator;
            }
            set {
                _slotCreator = value;
            }
        }

        internal object SyncRoot {
            get { 
                // TODO: This is un-ideal, we should lock on something private.
                return this; 
            }
        }

        public event EventHandler<PythonTypeChangedEventArgs> OnChange;

        /* 
         *                                  
         * class A(object):                 
         *      xyz=1 [ctx1]                
         *      abc=1 [allCtx]              
         *      baz=1 [ctx1]                
         *      baz=2 [allCtx]              
         *                                  
         * class B(A):                      
         *      xyz=2 [allCtx]              
         *      abc=2 [ctx1]                
         *                                      
         * class C(B):
         * 
         * x = B()
         * x.xyz from neutral ctx - 2           
         * x.xyz from ctx1        - 2
         * x.abc from neutral ctx - 1
         * x.abc from ctx1        - 2
         * 
         * y = C()
         * c.baz from ctx1        - 1
         * c.baz from allCtx      - 2
         * 
         */
        internal void Commit() {
            // update our vtable by propagating bases vtable up
            for (int i = 1; i < _resolutionOrder.Count; i++) {
                PythonType curType = _resolutionOrder[i];
                curType.Initialize();

                PropagateOneSet(curType._operators, i, ref _operators);

                if (curType._operators != null && curType._operators.ContextSpecific != null) {
                    for (int j = 0; j < curType._operators.ContextSpecific.Length; j++) {
                        VTable ops = curType._operators.ContextSpecific[j];

                        if (ops != null) {
                            EnsureContextSlots(j);

                            if (_operators.ContextSpecific[j] == null) _operators.ContextSpecific[j] = new VTable();
                            VTable dest = _operators.ContextSpecific[j];

                            PropagateOperators(ops.UnaryOperators, _operators.UnaryOperators, i, ref dest.UnaryOperators);
                            PropagateOperators(ops.BinaryOperators, _operators.BinaryOperators, i, ref dest.BinaryOperators);
                            PropagateOperators(ops.TernaryOperators, _operators.TernaryOperators, i, ref dest.TernaryOperators);
                        }
                    }
                }
            }

        }

        #endregion

        #region Private implementation details

        internal void Initialize() {
            if (_builder == null) {
                Debug.Assert(_dict != null);
                Debug.Assert((_attrs & PythonTypeAttributes.Initialized) != 0);
                return;
            }

            EnsureDict();

            InitializeWorker();

            if (_getboundmem != null || _setmem != null || _delmem != null) {
                _version = DynamicVersion;
                UpdateVersion();
            }
        }

        private void EnsureDict() {
            if (_dict == null) {
                Interlocked.CompareExchange<Dictionary<SymbolId, SlotInfo>>(
                    ref _dict,
                    new Dictionary<SymbolId, SlotInfo>(),
                    null);
            }
        }

        private void InitializeWorker() {
            lock (SyncRoot) {
                PythonTypeBuilder dtb = _builder;

                if (dtb == null || 
                    (_attrs & PythonTypeAttributes.Initializing)!=0) return;

                _attrs |= PythonTypeAttributes.Initializing;
                try {
                    dtb.Initialize();
                } finally {
                    _attrs &= ~PythonTypeAttributes.Initializing;
                }
                _attrs |= PythonTypeAttributes.Initialized;

                _builder = null;
            }
        }

        /// <summary>
        /// private helper function to ensure that we've created slots for context-specific languages
        /// </summary>
        /// <param name="context"></param>
        private void EnsureContextSlots(ContextId context) {
            EnsureContextSlots(context.Id);
        }

        /// <summary>
        /// private helper function to ensure that we've created slots for context-specific languages
        /// </summary>
        /// <param name="id"></param>
        private void EnsureContextSlots(int id) {
            if (_operators == null) _operators = new VTable();

            if (_operators.ContextSpecific == null) _operators.ContextSpecific = new VTable[id + 1];
            else if (_operators.ContextSpecific.Length <= id) Array.Resize<VTable>(ref _operators.ContextSpecific, id + 1);

            if (_operators.ContextSpecific[id] == null)
                Interlocked.CompareExchange<VTable>(ref _operators.ContextSpecific[id], new VTable(), null);
        }

        /// <summary>
        /// Helper function for adding an operator to one of our tables
        /// </summary>
        private static void AddOperator<T>(Operators op, ref OperatorReference<T>[] table, T target) {
            int opCode = (int)op;

            if (table == null) {
                table = new OperatorReference<T>[opCode + 1];
            } else if (table.Length <= opCode) {
                Array.Resize<OperatorReference<T>>(ref table, opCode + 1);
            }

            table[opCode] = new OperatorReference<T>(target);
        }

        [Flags]
        internal enum PythonTypeAttributes {
            None = 0x00,
            Immutable = 0x01,
            SystemType = 0x02,
            Initializing = 0x10000000,
            Initialized = 0x20000000,
        }

        private struct OperatorReference<T> {
            public T Operator;
            public int Inheritance;

            public OperatorReference(T op) {
                Operator = op;
                Inheritance = 0;
            }

            public OperatorReference(OperatorReference<T> op, int depth) {
                Operator = op.Operator;
                Inheritance = op.Inheritance + depth;
            }

            public override string ToString() {
                if (Operator == null) return "Undefined Operator";

                return String.Format("[Operator: {0}, Inheritance: {1}]", Operator, Inheritance);
            }
        }

        private class VTable {
            public OperatorReference<UnaryOperator>[] UnaryOperators;
            public OperatorReference<BinaryOperator>[] BinaryOperators;
            public OperatorReference<TernaryOperator>[] TernaryOperators;

            public VTable[] ContextSpecific;
        }

        private class SlotInfo {
            public SlotInfo() { }
            public SlotInfo(PythonTypeSlot defaultValue) {
                DefaultValue = defaultValue;
            }

            public PythonTypeSlot DefaultValue;
            public List<PythonTypeSlot> SlotValues;
        }

        private static void PropagateOneSet(VTable operators, int depth, ref VTable destination) {
            if (operators != null) {
                if (destination == null) destination = new VTable();
                
                PropagateOperators(operators.UnaryOperators, depth, ref destination.UnaryOperators);
                PropagateOperators(operators.BinaryOperators, depth, ref destination.BinaryOperators);
                PropagateOperators(operators.TernaryOperators, depth, ref destination.TernaryOperators);
            }
        }

        private static void PropagateOperators<T>(OperatorReference<T>[] from, OperatorReference<T>[] nonCtx, int depth, ref OperatorReference<T>[] to) {
            if (from != null) {
                if (to == null) to = new OperatorReference<T>[from.Length];
                if (to.Length < from.Length) Array.Resize(ref to, from.Length);

                for (int i = 0; i < from.Length; i++) {
                    if(to[i].Operator == null && ContextualReplacesDefault<T>(from, nonCtx, i, depth)) 
                        to[i] = new OperatorReference<T>(from[i], depth);
                }
            }
        }

        private static bool ContextualReplacesDefault<T>(OperatorReference<T>[] from, OperatorReference<T>[] nonCtx, int opIndex, int depth) {
            if (nonCtx == null || opIndex >= nonCtx.Length || nonCtx[opIndex].Operator == null) return true;

            return nonCtx[opIndex].Inheritance >= from[opIndex].Inheritance+depth;
        }

        private static void PropagateOperators<T>(OperatorReference<T>[] from, int depth, ref OperatorReference<T>[] to) {
            if (from != null) {
                if (to == null) to = new OperatorReference<T>[from.Length];
                if (to.Length < from.Length) Array.Resize(ref to, from.Length);

                for (int i = 0; i < from.Length; i++) {
                    if (to[i].Operator == null)
                        to[i] = new OperatorReference<T>(from[i], depth);
                }
            }
        }


        #endregion

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            PythonTypeSlot dts;
            if (TryResolveSlot(context, name, out dts)) {
                if(dts.TryGetValue(context, null, this, out value))
                    return true;
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

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            PythonTypeSlot dts;
            if (TryResolveSlot(context, name, out dts)) {
                if (dts.TryGetBoundValue(context, null, this, out value)) {
                    return true;
                }
            }

            // search the type
            PythonType myType = DynamicHelpers.GetPythonType(this);
            if(myType.TryResolveSlot(context, name, out dts)) {
                if (dts.TryGetBoundValue(context, this, myType, out value)) {
                    return true;
                }
            }

            value = null;
            return false;
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            PythonTypeSlot dts;
            if (TryResolveSlot(context, name, out dts)) {
                if(dts.TrySetValue(context, null, this, value))
                    return;
            }

            if (PythonType._pythonTypeType.TryLookupSlot(context, name, out dts)) {
                if (dts.TrySetValue(context, this, PythonType._pythonTypeType, value))
                    return;            
            }

            if (IsImmutable) {
                throw new MissingMemberException(String.Format("'{0}' object has no attribute '{1}'", Name, SymbolTable.IdToString(name)));
            }

            EventHandler<PythonTypeChangedEventArgs> dtc = OnChange;            
            object previous = null;

            if (dtc != null) {
                SlotInfo tmp;
                if (_dict.TryGetValue(name, out tmp)) {
                    previous = tmp.DefaultValue;
                }
            }

            dts = value as PythonTypeSlot;
            if (dts != null) {
                _dict[name] = new SlotInfo(dts);
            } else if (_slotCreator == null) {
                _dict[name] = new SlotInfo(new PythonTypeValueSlot(value));
            } else {
                _dict[name] = new SlotInfo(_slotCreator(value));
            }

            UpdateVersion();
            if (dtc != null) {
                dtc(this, new PythonTypeChangedEventArgs(context, name, ChangeType.Added, previous, value));
            }
            
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            PythonTypeSlot dts;
            if (TryResolveSlot(context, name, out dts)) {
                if (dts.TryDeleteValue(context, null, this))
                    return true;
            }

            if (IsImmutable) {
                throw new MissingMemberException(String.Format("can't delete attributes of built-in/extension type '{0}'", Name, SymbolTable.IdToString(name)));
            }

            EventHandler<PythonTypeChangedEventArgs> dtc = OnChange;
            object previous = null;

            if (dtc != null) {
                SlotInfo tmp;
                if (_dict.TryGetValue(name, out tmp)) {
                    previous = tmp.DefaultValue;
                }
            }

            if (!_dict.Remove(name)) {
                throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture,
                    IronPython.Resources.MemberDoesNotExist, 
                    name.ToString()));
            }

            UpdateVersion();
            if (dtc != null) {
                dtc(this, new PythonTypeChangedEventArgs(context, name, ChangeType.Removed, previous, null));
            }
            return true;
        }

        IList<object> IMembersList.GetMemberNames(CodeContext context) {
            List<object> res = new List<object>();
            foreach(SymbolId x in GetMemberNames(context)){
                res.Add(x.ToString());
            }
            return res;
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            Dictionary<object, object> res = new Dictionary<object, object>();
            foreach (KeyValuePair<SymbolId, object> kvp in GetMemberDictionary(context).SymbolAttributes) {
                res[kvp.Key.ToString()] = kvp.Value;
            }
            return res;
        }

        #endregion        
}
}
