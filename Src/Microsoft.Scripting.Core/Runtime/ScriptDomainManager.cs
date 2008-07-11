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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Scripting.Actions;
using System.Scripting.Utils;
using System.Threading;

namespace System.Scripting.Runtime {

    [Serializable]
    public class InvalidImplementationException : Exception {
        public InvalidImplementationException()
            : base() {
        }

        public InvalidImplementationException(string message)
            : base(message) {
        }

        public InvalidImplementationException(string message, Exception e)
            : base(message, e) {
        }

#if !SILVERLIGHT // SerializationInfo
        protected InvalidImplementationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

    [Serializable]
    public class MissingTypeException : Exception {
        public MissingTypeException() {
        }

        public MissingTypeException(string name) : this(name, null) {
        }

        public MissingTypeException(string name, Exception e) : 
            base(ResourceUtils.GetString(ResourceUtils.MissingType, name), e) {
        }

#if !SILVERLIGHT // SerializationInfo
        protected MissingTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")] // TODO: fix
    public sealed class ScriptDomainManager {
        private readonly DynamicRuntimeHostingProvider/*!*/ _hostingProvider;
        private readonly SharedIO/*!*/ _sharedIO;

        // TODO: ReaderWriterLock (Silverlight?)
        private readonly object _languageRegistrationLock = new object();
        private readonly Dictionary<string, LanguageRegistration> _languageIds = new Dictionary<string, LanguageRegistration>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, LanguageRegistration> _languageTypes = new Dictionary<string, LanguageRegistration>();
        private readonly List<LanguageContext> _registeredContexts = new List<LanguageContext>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public PlatformAdaptationLayer/*!*/ Platform { 
            get {
                PlatformAdaptationLayer result = _hostingProvider.PlatformAdaptationLayer;
                if (result == null) {
                    throw new InvalidImplementationException();
                }
                return result;
            } 
        }

        public SharedIO/*!*/ SharedIO { get { return _sharedIO; } }
        public DynamicRuntimeHostingProvider/*!*/ Host { get { return _hostingProvider; } }
        private ScopeAttributesWrapper _scopeWrapper;
        private Scope/*!*/ _globals;
        
        private static ScriptDomainOptions _options = new ScriptDomainOptions();// TODO: remove or reduce     

        public ScriptDomainManager(DynamicRuntimeHostingProvider/*!*/ hostingProvider) {
            ContractUtils.RequiresNotNull(hostingProvider, "hostingProvider");

            _hostingProvider = hostingProvider;

            _sharedIO = new SharedIO();

            // create the initial default scope
            _scopeWrapper = new ScopeAttributesWrapper(this);
            _globals = new Scope(_scopeWrapper);
        }
       
        #region Language Registration

        /// <summary>
        /// Singleton for each language.
        /// </summary>
        private sealed class LanguageRegistration {
            private readonly ScriptDomainManager/*!*/ _domainManager;
            private readonly string _assemblyName;
            private readonly string _typeName;
            private LanguageContext _context;
            private Type _type;
            
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // TODO: fix
            public string AssemblyName {
                get { return _assemblyName; }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // TODO: fix
            public string TypeName {
                get { return _typeName; }
            }

            public LanguageContext LanguageContext {
                get { return _context; }
            }

            public LanguageRegistration(ScriptDomainManager/*!*/ domainManager, Type type) {
                Debug.Assert(type != null);

                _type = type;
                _domainManager = domainManager;
            }

            public LanguageRegistration(ScriptDomainManager/*!*/ domainManager, string typeName, string assemblyName) {
                Debug.Assert(typeName != null && assemblyName != null);

                _assemblyName = assemblyName;
                _typeName = typeName;
                _domainManager = domainManager;
            }

            /// <summary>
            /// Must not be called under a lock as it can potentially call a user code.
            /// </summary>
            /// <exception cref="MissingTypeException">languageId</exception>
            /// <exception cref="InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
            public LanguageContext/*!*/ LoadLanguageContext() {
                if (_context == null) {
                    
                    if (_type == null) {
                        try {
                            _type = _domainManager.Platform.LoadAssembly(_assemblyName).GetType(_typeName, true);
                        } catch (Exception e) {
                            throw new MissingTypeException(MakeAssemblyQualifiedName(_assemblyName, _typeName), e);
                        }
                    }

                    lock (_domainManager._languageRegistrationLock) {
                        _domainManager._languageTypes[_type.AssemblyQualifiedName] = this;
                    }

                    Interlocked.CompareExchange(ref _context, ReflectionUtils.CreateInstance<LanguageContext>(_type, _domainManager), null);
                }
                return _context;
            }
        }

        public void RegisterLanguageContext(string assemblyName, string typeName, params string[] identifiers) {
            RegisterLanguageContext(assemblyName, typeName, false, identifiers);
        }

        public void RegisterLanguageContext(string assemblyName, string typeName, bool overrideExistingIds, params string[] identifiers) {
            ContractUtils.RequiresNotNull(identifiers, "identifiers");

            LanguageRegistration singleton_desc;
            bool add_singleton_desc = false;
            string aq_name = MakeAssemblyQualifiedName(typeName, assemblyName);

            lock (_languageRegistrationLock) {
                if (!_languageTypes.TryGetValue(aq_name, out singleton_desc)) {
                    add_singleton_desc = true;
                    singleton_desc = new LanguageRegistration(this, typeName, assemblyName);
                }

                // check for conflicts:
                if (!overrideExistingIds) {
                    for (int i = 0; i < identifiers.Length; i++) {
                        LanguageRegistration desc;
                        if (_languageIds.TryGetValue(identifiers[i], out desc) && !ReferenceEquals(desc, singleton_desc)) {
                            throw new InvalidOperationException("Conflicting Ids");
                        }
                    }
                }

                // add singleton LP-desc:
                if (add_singleton_desc)
                    _languageTypes.Add(aq_name, singleton_desc);

                // add id mapping to the singleton LP-desc:
                for (int i = 0; i < identifiers.Length; i++) {
                    _languageIds[identifiers[i]] = singleton_desc;
                }
            }
        }

        public bool RemoveLanguageMapping(string/*!*/ identifier) {
            ContractUtils.RequiresNotNull(identifier, "identifier");
            
            lock (_languageRegistrationLock) {
                return _languageIds.Remove(identifier);
            }
        }

        /// <summary>
        /// Throws an exception on failure.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        /// <exception cref="ArgumentException"><paramref name="type"/></exception>
        /// <exception cref="MissingTypeException">languageId</exception>
        /// <exception cref="InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
        public LanguageContext/*!*/ GetLanguageContext(Type/*!*/ type) {
            ContractUtils.RequiresNotNull(type, "type");
            if (!type.IsSubclassOf(typeof(LanguageContext))) throw new ArgumentException("Invalid type - should be subclass of LanguageContext"); // TODO

            LanguageRegistration desc = null;
            
            lock (_languageRegistrationLock) {
                if (!_languageTypes.TryGetValue(type.AssemblyQualifiedName, out desc)) {
                    desc = new LanguageRegistration(this, type);
                    _languageTypes[type.AssemblyQualifiedName] = desc;
                }
            }

            if (desc != null) {
                return desc.LoadLanguageContext();
            }

            // not found, not registered:
            throw new ArgumentException(ResourceUtils.GetString(ResourceUtils.UnknownLanguageProviderType));
        }

        /// <summary>
        /// Gets the language context of the specified type.  This can be used by language implementors
        /// to get their LanguageContext for an already existing ScriptDomainManager.
        /// </summary>
        public TContextType/*!*/ GetLanguageContext<TContextType>() where TContextType : LanguageContext {
            return (TContextType)GetLanguageContext(typeof(TContextType));
        }

        internal string[] GetLanguageIdentifiers(Type type, bool extensionsOnly) {
            if (type != null && !type.IsSubclassOf(typeof(LanguageContext))) {
                throw new ArgumentException("Invalid type - should be subclass of LanguageContext"); // TODO
            }

            bool get_all = type == null;
            List<string> result = new List<string>();

            lock (_languageTypes) {
                LanguageRegistration singleton_desc = null;
                if (!get_all && !_languageTypes.TryGetValue(type.AssemblyQualifiedName, out singleton_desc)) {
                    return ArrayUtils.EmptyStrings;
                }

                foreach (KeyValuePair<string, LanguageRegistration> entry in _languageIds) {
                    if (get_all || ReferenceEquals(entry.Value, singleton_desc)) {
                        if (!extensionsOnly || IsExtensionId(entry.Key)) {
                            result.Add(entry.Key);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        /// <exception cref="ArgumentNullException"><paramref name="languageId"/></exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
        public bool TryGetLanguageContext(string/*!*/ languageId, out LanguageContext languageContext) {
            ContractUtils.RequiresNotNull(languageId, "languageId");

            bool result;
            LanguageRegistration desc;

            lock (_languageRegistrationLock) {
                result = _languageIds.TryGetValue(languageId, out desc);
            }

            languageContext = result ? desc.LoadLanguageContext() : null;

            return result;
        }

        public bool TryGetLanguageContextByFileExtension(string extension, out LanguageContext languageContext) {
            if (String.IsNullOrEmpty(extension)) {
                languageContext = null;
                return false;
            }

            // TODO: separate hashtable for extensions (see CodeDOM config)
            if (extension[0] != '.') extension = '.' + extension;
            return TryGetLanguageContext(extension, out languageContext);
        }

        public string[] GetRegisteredFileExtensions() {
            return GetLanguageIdentifiers(null, true);
        }

        public string[] GetRegisteredLanguageIdentifiers() {
            return GetLanguageIdentifiers(null, false);
        }

        // TODO: separate hashtable for extensions (see CodeDOM config)
        private static bool IsExtensionId(string id) {
            return id.StartsWith(".");
        }

        /// <exception cref="MissingTypeException">languageId</exception>
        /// <exception cref="InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
        public LanguageContext[] GetLanguageContexts(bool usedOnly) {
            List<LanguageContext> results = new List<LanguageContext>(_languageIds.Count);

            List<LanguageRegistration> to_be_loaded = usedOnly ? null : new List<LanguageRegistration>();
            
            lock (_languageRegistrationLock) {
                foreach (LanguageRegistration desc in _languageIds.Values) {
                    if (desc.LanguageContext != null) {
                        results.Add(desc.LanguageContext);
                    } else if (!usedOnly) {
                        to_be_loaded.Add(desc);
                    }
                }
            }

            if (!usedOnly) {
                foreach (LanguageRegistration desc in to_be_loaded) {
                    results.Add(desc.LoadLanguageContext());
                }
            }

            return results.ToArray();
        }

        private static string MakeAssemblyQualifiedName(string typeName, string assemblyName) {
            return String.Concat(typeName, ", ", assemblyName);
        }

        #endregion

        #region Variables and Modules

        /// <summary>
        /// A collection of environment variables.
        /// </summary>
        public Scope/*!*/ Globals { 
            get { 
                return _globals; 
            }
        }

        public void SetGlobalsDictionary(IAttributesCollection/*!*/ dictionary) {
            ContractUtils.RequiresNotNull(dictionary, "dictionary");

            _scopeWrapper.Dict = dictionary;
        }

        public event EventHandler<AssemblyLoadedEventArgs> AssemblyLoaded;

        public bool LoadAssembly(Assembly/*!*/ assembly) {
            ContractUtils.RequiresNotNull(assembly, "assembly");

            EventHandler<AssemblyLoadedEventArgs> assmLoaded = AssemblyLoaded;
            if (assmLoaded != null) {
                assmLoaded(this, new AssemblyLoadedEventArgs(assembly));
            }

            return _scopeWrapper.LoadAssembly(assembly);
        }

        // TODO: fix when this moves up:
        // <exception cref="AmbiguousFileNameException">Multiple matching files were found.</exception>
        /// <summary>
        /// Uses the hosts search path and semantics to resolve the provided name to a SourceUnit.
        /// 
        /// If the host provides a SourceUnit which is equal to an already loaded SourceUnit the
        /// previously loaded module is returned.
        /// 
        /// Returns null if a module could not be found.
        /// </summary>
        /// <param name="name">an opaque parameter which has meaning to the host.  Typically a filename without an extension.</param>
        /// <exception cref="FileNotFoundException">No file matches the specified name.</exception>
        /// <exception cref="ArgumentNullException">Name is a <c>null</c> reference.</exception>
        /// <exception cref="ArgumentException">Name is not valid.</exception>
        public object UseModule(string/*!*/ name) {
            ContractUtils.RequiresNotNull(name, "name");
            
            SourceUnit source = Host.ResolveSourceFileUnit(name);
            
            // TODO: remove (JS test in MerlinWeb relies on scope reuse)
            object result;
            if (Globals.TryGetName(SymbolTable.StringToId(name), out result)) {
                return result;
            }

            result = ExecuteSourceUnit(source);
            Globals.SetName(SymbolTable.StringToId(name), result);

            return result;
        }

        /// <summary>
        /// Requests a SourceUnit from the provided path and compiles it to a ScriptScope.
        /// 
        /// If the host provides a SourceUnit which is equal to an already loaded SourceUnit the
        /// previously loaded module is returned.
        /// 
        /// Returns null if a module could not be found.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="path"/></exception>
        /// <exception cref="ArgumentException">no language registered</exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language provider's implementation failed to instantiate.</exception>
        public Scope UseModule(string/*!*/ path, string/*!*/ languageId) {
            ContractUtils.RequiresNotNull(path, "path");
            ContractUtils.RequiresNotNull(languageId, "languageId");

            LanguageContext language;
            TryGetLanguageContext(languageId, out language);

            SourceUnit source = Host.TryGetSourceFileUnit(language, path, StringUtils.DefaultEncoding, SourceCodeKind.File);
            if (source == null) {
                return null;
            }

            return ExecuteSourceUnit(source);
        }

        private static Scope/*!*/ ExecuteSourceUnit(SourceUnit/*!*/ source) {
            ScriptCode compiledCode = source.Compile();
            Scope scope = compiledCode.CreateScope();
            compiledCode.Run(scope);
            return scope;
        }

        #endregion

        #region TODO: Options

        // TODO: remove or reduce
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ScriptDomainOptions GlobalOptions {
            get {
                return _options;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _options = value;
            }
        }

        // TODO: remove or reduce
        public static ScriptDomainOptions Options {
            get { return _options; }
        }

        #endregion

        public string[] GetRegisteredLanguageIdentifiers(LanguageContext context) {
            List<string> res = new List<string>();
            lock (_languageRegistrationLock) {
                foreach (KeyValuePair<string, LanguageRegistration> kvp in _languageIds) {
                    if (kvp.Key.StartsWith(".")) continue;

                    if (kvp.Value.LanguageContext == context) {
                        res.Add(kvp.Key);
                    }
                }
            }
            return res.ToArray();
        }

        public string[] GetRegisteredFileExtensions(LanguageContext context) {
            // TODO: separate hashtable for extensions (see CodeDOM config)
            List<string> res = new List<string>();
            lock (_languageRegistrationLock) {
                foreach (KeyValuePair<string, LanguageRegistration> kvp in _languageIds) {
                    if (!kvp.Key.StartsWith(".")) continue;

                    if (kvp.Value.LanguageContext == context) {
                        res.Add(kvp.Key);
                    }
                }
            }            

            return res.ToArray();            
        }

        internal ContextId AssignContextId(LanguageContext lc) {
            lock(_registeredContexts) {
                int index = _registeredContexts.Count;
                _registeredContexts.Add(lc);

                return new ContextId(index + 1);
            }
        }

        private class ScopeAttributesWrapper : IAttributesCollection {
            private IAttributesCollection/*!*/ _dict = new SymbolDictionary();
            private readonly TopNamespaceTracker/*!*/ _tracker;

            public ScopeAttributesWrapper(ScriptDomainManager/*!*/ manager) {
                _tracker = new TopNamespaceTracker(manager);
            }

            public IAttributesCollection/*!*/ Dict {
                set {
                    Assert.NotNull(_dict);

                    _dict = value;
                }
            }

            public bool LoadAssembly(Assembly asm) {                
                return _tracker.LoadAssembly(asm);
            }

            public List<Assembly> GetLoadedAssemblies() {
                return _tracker._packageAssemblies;
            }

            #region IAttributesCollection Members

            public void Add(SymbolId name, object value) {
                _dict[name] = value;
            }

            public bool TryGetValue(SymbolId name, out object value) {
                if (!_dict.TryGetValue(name, out value)) {
                    value = _tracker.TryGetPackageAny(name);                    
                }
                return value != null;
            }

            public bool Remove(SymbolId name) {
                return _dict.Remove(name);
            }

            public bool ContainsKey(SymbolId name) {
                return _dict.ContainsKey(name) || _tracker.TryGetPackageAny(name) != null;
            }

            public object this[SymbolId name] {
                get {
                    object value;
                    if (TryGetValue(name, out value)) {
                        return value;
                    }

                    throw new KeyNotFoundException();
                }
                set {
                    Add(name, value);
                }
            }

            public IDictionary<SymbolId, object> SymbolAttributes {
                get { return _dict.SymbolAttributes; }
            }

            public void AddObjectKey(object name, object value) {
                _dict.AddObjectKey(name, value);
            }

            public bool TryGetObjectValue(object name, out object value) {
                return _dict.TryGetObjectValue(name, out value);
            }

            public bool RemoveObjectKey(object name) {
                return _dict.RemoveObjectKey(name);
            }

            public bool ContainsObjectKey(object name) {
                return _dict.ContainsObjectKey(name);
            }

            public IDictionary<object, object> AsObjectKeyedDictionary() {
                return _dict.AsObjectKeyedDictionary();
            }

            public int Count {
                get {
                    int count = _dict.Count + _tracker.Count;
                    foreach (object o in _tracker.Keys) {
                        if (ContainsObjectKey(o)) {
                            count--;
                        }
                    }
                    return count;
                }
            }

            public ICollection<object> Keys {
                get {
                    List<object> keys = new List<object>(_dict.Keys);
                    foreach (object o in _tracker.Keys) {
                        if (!_dict.ContainsObjectKey(o)) {
                            keys.Add(o);
                        }
                    }
                    return keys; 
                }
            }

            #endregion

            #region IEnumerable<KeyValuePair<object,object>> Members

            public IEnumerator<KeyValuePair<object, object>> GetEnumerator() {
                foreach(KeyValuePair<object, object> kvp in _dict) {
                    yield return kvp;
                }
                foreach (KeyValuePair<object, object> kvp in _tracker) {
                    if (!_dict.ContainsObjectKey(kvp.Key)) {
                        yield return kvp;
                    }
                }
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator() {
                foreach (KeyValuePair<object, object> kvp in _dict) {
                    yield return kvp.Key;
                }
                foreach (KeyValuePair<object, object> kvp in _tracker) {
                    if (!_dict.ContainsObjectKey(kvp.Key)) {
                        yield return kvp.Key;
                    }
                }
            }

            #endregion
        }

        public Assembly[] GetLoadedAssemblyList() {
            return _scopeWrapper.GetLoadedAssemblies().ToArray();
        }
    }
}
