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
using System.Linq.Expressions;
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

        public MissingTypeException(string name)
            : this(name, null) {
        }

        public MissingTypeException(string name, Exception e) :
            base(Strings.MissingType(name), e) {
        }

#if !SILVERLIGHT // SerializationInfo
        protected MissingTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")] // TODO: fix
    public sealed class ScriptDomainManager {
        private readonly DynamicRuntimeHostingProvider _hostingProvider;
        private readonly SharedIO _sharedIO;

        // TODO: ReaderWriterLock (Silverlight?)
        private readonly List<LanguageContext> _registeredContexts = new List<LanguageContext>();

        private ScopeAttributesWrapper _scopeWrapper;
        private Scope _globals;
        private readonly DlrConfiguration _configuration;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public PlatformAdaptationLayer Platform {
            get {
                PlatformAdaptationLayer result = _hostingProvider.PlatformAdaptationLayer;
                if (result == null) {
                    throw new InvalidImplementationException();
                }
                return result;
            }
        }

        public SharedIO SharedIO {
            get { return _sharedIO; }
        }

        public DynamicRuntimeHostingProvider Host {
            get { return _hostingProvider; }
        }

        public DlrConfiguration Configuration {
            get { return _configuration; }
        }

        public ScriptDomainManager(DynamicRuntimeHostingProvider hostingProvider, DlrConfiguration configuration) {
            ContractUtils.RequiresNotNull(hostingProvider, "hostingProvider");
            ContractUtils.RequiresNotNull(configuration, "configuration");

            configuration.Freeze();

            _hostingProvider = hostingProvider;
            _configuration = configuration;

            _sharedIO = new SharedIO();

            // create the initial default scope
            _scopeWrapper = new ScopeAttributesWrapper(this);
            _globals = new Scope(_scopeWrapper);
        }

        #region Language Registration

        internal ContextId AssignContextId(LanguageContext language) {
            lock (_registeredContexts) {
                int index = _registeredContexts.Count;
                _registeredContexts.Add(language);

                return new ContextId(index + 1);
            }
        }

        public LanguageContext GetLanguage(Type providerType) {
            ContractUtils.RequiresNotNull(providerType, "providerType");
            return GetLanguage(new AssemblyQualifiedTypeName(providerType));
        }

        public LanguageContext GetLanguage(AssemblyQualifiedTypeName providerName) {
            LanguageContext language;
            if (!_configuration.TryLoadLanguage(this, providerName, out language)) {
                throw Error.UnknownLanguageProviderType();
            }
            return language;
        }

        /// <summary>
        /// Gets the language context of the specified type.  This can be used by language implementors
        /// to get their LanguageContext for an already existing ScriptDomainManager.
        /// </summary>
        public TContextType GetLanguage<TContextType>() where TContextType : LanguageContext {
            return (TContextType)GetLanguage(typeof(TContextType));
        }

        public bool TryGetLanguage(string languageId, out LanguageContext language) {
            ContractUtils.RequiresNotNull(languageId, "languageId");
            return _configuration.TryLoadLanguage(this, languageId, false, out language);
        }

        public LanguageContext GetLanguage(string languageId) {
            LanguageContext language;
            if (!TryGetLanguage(languageId, out language)) {
                throw new ArgumentException("Unknown language identifier");
            }
            return language;
        }

        public bool TryGetLanguageByFileExtension(string extension, out LanguageContext language) {
            ContractUtils.RequiresNotNull(extension, "extension");
            return _configuration.TryLoadLanguage(this, DlrConfiguration.NormalizeExtension(extension), true, out language);
        }

        public LanguageContext GetLanguageByExtension(string extension) {
            LanguageContext language;
            if (!TryGetLanguageByFileExtension(extension, out language)) {
                throw new ArgumentException("Unknown extension");
            }
            return language;
        }

        #endregion

        #region Variables and Modules

        /// <summary>
        /// A collection of environment variables.
        /// </summary>
        public Scope Globals {
            get {
                return _globals;
            }
        }

        public void SetGlobalsDictionary(IAttributesCollection dictionary) {
            ContractUtils.RequiresNotNull(dictionary, "dictionary");

            _scopeWrapper.Dict = dictionary;
        }

        public event EventHandler<AssemblyLoadedEventArgs> AssemblyLoaded;

        public bool LoadAssembly(Assembly assembly) {
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
        public object UseModule(string name) {
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
        public Scope UseModule(string path, string languageId) {
            ContractUtils.RequiresNotNull(path, "path");
            ContractUtils.RequiresNotNull(languageId, "languageId");

            var language = GetLanguage(languageId);
            var source = Host.TryGetSourceFileUnit(language, path, StringUtils.DefaultEncoding, SourceCodeKind.File);
            if (source == null) {
                return null;
            }

            return ExecuteSourceUnit(source);
        }

        private static Scope ExecuteSourceUnit(SourceUnit source) {
            ScriptCode compiledCode = source.Compile();
            Scope scope = compiledCode.CreateScope();
            compiledCode.Run(scope);
            return scope;
        }

        #endregion

        #region ScopeAttributesWrapper

        private class ScopeAttributesWrapper : IAttributesCollection {
            private IAttributesCollection _dict = new SymbolDictionary();
            private readonly TopNamespaceTracker _tracker;

            public ScopeAttributesWrapper(ScriptDomainManager manager) {
                _tracker = new TopNamespaceTracker(manager);
            }

            public IAttributesCollection Dict {
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
                foreach (KeyValuePair<object, object> kvp in _dict) {
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

        #endregion
    }
}
