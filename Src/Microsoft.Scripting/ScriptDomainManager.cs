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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {

    public delegate void CommandDispatcher(Delegate command);

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
            base(String.Format(Resources.MissingType, name), e) {
        }

#if !SILVERLIGHT // SerializationInfo
        protected MissingTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")] // TODO: fix
    public sealed class ScriptDomainManager {

        #region Fields and Initialization

        private static readonly object _singletonLock = new object();
        private static ScriptDomainManager _singleton;

        private readonly Dictionary<Type, ScriptEngine>/*!*/ _engines = new Dictionary<Type, ScriptEngine>(); // TODO: Key Should be LC, not Type
        private readonly PlatformAdaptationLayer/*!*/ _pal;
        private readonly IScriptHost/*!*/ _host;
        private readonly Snippets/*!*/ _snippets;
        private readonly ScriptEnvironment/*!*/ _environment;
        private readonly InvariantContext/*!*/ _invariantContext;

        private CommandDispatcher _commandDispatcher; // can be null
        
        // singletons:
        public PlatformAdaptationLayer/*!*/ PAL { get { return _pal; } }
        public Snippets/*!*/ Snippets { get { return _snippets; } }
        public ScriptEnvironment/*!*/ Environment { get { return _environment; } }

        /// <summary>
        /// Gets the <see cref="ScriptDomainManager"/> associated with the current AppDomain. 
        /// If there is none, creates and initializes a new environment using setup information associated with the AppDomain 
        /// or stored in a configuration file.
        /// </summary>
        public static ScriptDomainManager CurrentManager {
            get {
                ScriptDomainManager result;
                TryCreateLocal(null, out result);
                return result;
            }
        }

        public IScriptHost/*!*/ Host {
            get { return _host; }
        }

        /// <summary>
        /// Creates a new local <see cref="ScriptDomainManager"/> unless it already exists. 
        /// Returns either <c>true</c> and the newly created environment initialized according to the provided setup information
        /// or <c>false</c> and the existing one ignoring the specified setup information.
        /// </summary>
        internal static bool TryCreateLocal(ScriptEnvironmentSetup setup, out ScriptDomainManager manager) {

            bool new_created = false;

            if (_singleton == null) {

                if (setup == null) {
                    setup = GetSetupInformation();
                }

                lock (_singletonLock) {
                    if (_singleton == null) {
                        ScriptDomainManager singleton = new ScriptDomainManager(setup);
                        Utilities.MemoryBarrier();
                        _singleton = singleton;
                        new_created = true;
                    }
                }

            }

            manager = _singleton;
            return new_created;
        }

        private static ScriptEnvironmentSetup GetSetupInformation() {
#if !SILVERLIGHT
            ScriptEnvironmentSetup result;

            // setup provided by app-domain creator:
            result = ScriptEnvironmentSetup.GetAppDomainAssociated(AppDomain.CurrentDomain);
            if (result != null) {
                return result;
            }

            // setup provided in a configuration file:
            // This will load System.Configuration.dll which costs ~350 KB of memory. However, this does not normally 
            // need to be loaded in simple scenarios (like running the console hosts). Hence, the working set cost
            // is only paid in hosted scenarios.
            ScriptConfiguration config = System.Configuration.ConfigurationManager.GetSection(ScriptConfiguration.Section) as ScriptConfiguration;
            if (config != null) {
                // TODO:
                //return config;
            }
#endif

            // default setup:
            return new ScriptEnvironmentSetup(true);
        }

        /// <summary>
        /// Initializes environment according to the setup information.
        /// </summary>
        private ScriptDomainManager(ScriptEnvironmentSetup/*!*/ setup) {
            Debug.Assert(setup != null);

            _invariantContext = new InvariantContext(this);

            // create local environment for the host:
            _environment = new ScriptEnvironment(this);
            
            // create PAL (default always available):
            _pal = setup.CreatePAL();

            // let setup register language contexts listed on it:
            setup.RegisterLanguages(this);

            // initialize snippets:
            _snippets = new Snippets();

            // create a local host unless a remote one has already been created:
            _host = setup.CreateScriptHost(_environment);
        }

        #endregion
       
        #region Language Registration

        /// <summary>
        /// Singleton for each language.
        /// </summary>
        private sealed class LanguageRegistration {

            private string _assemblyName;
            private string _typeName;
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

            public LanguageRegistration(Type type) {
                Debug.Assert(type != null);

                _type = type;
                _assemblyName = null;
                _typeName = null;
                _context = null;
            }

            public LanguageRegistration(string typeName, string assemblyName) {
                Debug.Assert(typeName != null && assemblyName != null);

                _assemblyName = assemblyName;
                _typeName = typeName;
                _context = null;
            }

            /// <summary>
            /// Must not be called under a lock as it can potentially call a user code.
            /// </summary>
            /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
            /// <exception cref="InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
            public LanguageContext LoadLanguageContext(ScriptDomainManager manager) {
                if (_context == null) {
                    
                    if (_type == null) {
                        try {
                            _type = ScriptDomainManager.CurrentManager.PAL.LoadAssembly(_assemblyName).GetType(_typeName, true);
                        } catch (Exception e) {
                            throw new MissingTypeException(MakeAssemblyQualifiedName(_assemblyName, _typeName), e);
                        }
                    }

                    lock (manager._languageRegistrationLock) {
                        manager._languageTypes[_type.AssemblyQualifiedName] = this;
                    }

                    // needn't to be locked, we can create multiple LPs:
                    LanguageContext context = ReflectionUtils.CreateInstance<LanguageContext>(_type, manager);
                    Utilities.MemoryBarrier();
                    _context = context;
                }
                return _context;
            }
        }

        // TODO: ReaderWriterLock (Silverlight?)
        private readonly object _languageRegistrationLock = new object();
        private readonly Dictionary<string, LanguageRegistration> _languageIds = new Dictionary<string, LanguageRegistration>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, LanguageRegistration> _languageTypes = new Dictionary<string, LanguageRegistration>();
        private readonly List<LanguageContext> _registeredContexts = new List<LanguageContext>();

        public void RegisterLanguageContext(string assemblyName, string typeName, params string[] identifiers) {
            RegisterLanguageContext(assemblyName, typeName, false, identifiers);
        }

        public void RegisterLanguageContext(string assemblyName, string typeName, bool overrideExistingIds, params string[] identifiers) {
            Contract.RequiresNotNull(identifiers, "identifiers");

            LanguageRegistration singleton_desc;
            bool add_singleton_desc = false;
            string aq_name = MakeAssemblyQualifiedName(typeName, assemblyName);

            lock (_languageRegistrationLock) {
                if (!_languageTypes.TryGetValue(aq_name, out singleton_desc)) {
                    add_singleton_desc = true;
                    singleton_desc = new LanguageRegistration(typeName, assemblyName);
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

        public bool RemoveLanguageMapping(string identifier) {
            Contract.RequiresNotNull(identifier, "identifier");
            
            lock (_languageRegistrationLock) {
                return _languageIds.Remove(identifier);
            }
        }

        /// <summary>
        /// Throws an exception on failure.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        /// <exception cref="ArgumentException"><paramref name="type"/></exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
        internal LanguageContext GetLanguageContext(Type type) {
            Contract.RequiresNotNull(type, "type");
            if (!type.IsSubclassOf(typeof(LanguageContext))) throw new ArgumentException("Invalid type - should be subclass of LanguageContext"); // TODO

            LanguageRegistration desc = null;
            
            lock (_languageRegistrationLock) {
                if (!_languageTypes.TryGetValue(type.AssemblyQualifiedName, out desc)) {
                    desc = new LanguageRegistration(type);
                    _languageTypes[type.AssemblyQualifiedName] = desc;
                }
            }

            if (desc != null) {
                return desc.LoadLanguageContext(this);
            }

            // not found, not registered:
            throw new ArgumentException(Resources.UnknownLanguageProviderType);
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
            Contract.RequiresNotNull(languageId, "languageId");

            bool result;
            LanguageRegistration desc;

            lock (_languageRegistrationLock) {
                result = _languageIds.TryGetValue(languageId, out desc);
            }

            languageContext = result ? desc.LoadLanguageContext(this) : null;

            return result;
        }

        public ScriptEngine GetEngineByFileExtension(string extension) {
            LanguageContext lc;
            if (!TryGetLanguageContextByFileExtension(extension, out lc)) {
                throw new ArgumentException(Resources.UnknownLanguageId);
            }
            return GetEngine(lc);
        }

        public bool TryGetEngine(string languageId, out ScriptEngine engine) {
            LanguageContext lc;
            if (!TryGetLanguageContext(languageId, out lc)) {
                engine = null;
                return false;
            }

            engine = GetEngine(lc);
            return true;
        }

        public bool TryGetEngineByFileExtension(string extension, out ScriptEngine engine) {
            LanguageContext lc;
            if (!TryGetLanguageContextByFileExtension(extension, out lc)) {
                engine = null;
                return false;
            }

            engine = GetEngine(lc);
            return true;
        }

        public ScriptEngine GetEngine(string/*!*/ languageId) {
            Contract.RequiresNotNull(languageId, "languageId");

            LanguageContext lc;
            if (!TryGetLanguageContext(languageId, out lc)) {
                throw new ArgumentException(Resources.UnknownLanguageId);
            }

            return GetEngine(lc);
        }

        public ScriptEngine GetEngine(Type languageContextType) {
            return GetEngine(GetLanguageContext(languageContextType));
        }

        internal ScriptEngine/*!*/ GetEngine(LanguageContext/*!*/ language) {
            Assert.NotNull(language);
            ScriptEngine engine;
            if (!_engines.TryGetValue(language.GetType(), out engine)) {
                engine = new ScriptEngine(_environment, language);
                _engines[language.GetType()] = engine;
                _host.EngineCreated(engine);
            }
            return engine;
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
        private bool IsExtensionId(string id) {
            return id.StartsWith(".");
        }

        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
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
                    results.Add(desc.LoadLanguageContext(this));
                }
            }

            return results.ToArray();
        }

        private static string MakeAssemblyQualifiedName(string typeName, string assemblyName) {
            return String.Concat(typeName, ", ", assemblyName);
        }

        #endregion

        #region Variables

        private IAttributesCollection _variables;

        /// <summary>
        /// A collection of environment variables or <c>null</c> for calling back to the host on each variable access.
        /// It's up to the host to set the property via <see cref="ScriptEnvironment"/> and to ensure its correct behavior and thread safety.
        /// </summary>
        internal IAttributesCollection Variables { get { return _variables; } set { _variables = value; } }

        public void SetVariable(CodeContext context, SymbolId name, object value) {
            IAttributesCollection variables = _variables;
            
            if (variables != null) {
                variables[name] = value;
            } else {
                if (!_host.TrySetVariable(name, value)) {
                    // TODO:
                    throw context.LanguageContext.MissingName(name);
                }
            }
        }

        public object GetVariable(CodeContext context, SymbolId name) {
            IAttributesCollection variables = _variables;

            if (variables != null) {
                return variables[name];
            } else {
                object result;
                
                if (!_host.TryGetVariable(name, out result)) {
                    // TODO:
                    throw context.LanguageContext.MissingName(name);
                }

                return result;
            }
        }

        #endregion

        #region Modules // OBSOLETE

        public StringComparer/*!*/ PathComparer {
            get {
                return StringComparer.Ordinal;
            }
        }

        // TODO: remove
        private Dictionary<string, ScriptScope> _modules = new Dictionary<string, ScriptScope>();
        
        /// <summary>
        /// Uses the hosts search path and semantics to resolve the provided name to a SourceUnit.
        /// 
        /// If the host provides a SourceUnit which is equal to an already loaded SourceUnit the
        /// previously loaded module is returned.
        /// 
        /// Returns null if a module could not be found.
        /// </summary>
        /// <param name="name">an opaque parameter which has meaning to the host.  Typically a filename without an extension.</param>
        public ScriptScope UseModule(string name) {
            Contract.RequiresNotNull(name, "name");
            
            SourceUnit su = _host.ResolveSourceFileUnit(name);
            if (su == null) {
                return null;
            }
        
            // TODO: remove (JS test in MerlinWeb relies on scope reuse)
            ScriptScope result;
            lock (_modules) {
                if (_modules.TryGetValue(name, out result)) {
                    return result;
                }
            }
            result = ExecuteSourceUnit(su);
            lock (_modules) {
                _modules[name] = result;
            }

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
        public ScriptScope UseModule(string/*!*/ path, string/*!*/ languageId) {
            Contract.RequiresNotNull(path, "path");
            Contract.RequiresNotNull(languageId, "languageId");

            ScriptEngine engine = GetEngine(languageId);

            SourceUnit su = _host.TryGetSourceFileUnit(engine, path, StringUtils.DefaultEncoding, SourceCodeKind.File);
            if (su == null) {
                return null;
            }

            return ExecuteSourceUnit(su);
        }

        // TODO:
        public ScriptScope ExecuteSourceUnit(SourceUnit/*!*/ sourceUnit) {
            ScriptCode compiledCode = sourceUnit.LanguageContext.CompileSourceCode(sourceUnit);
            Scope scope = compiledCode.MakeOptimizedScope();
            compiledCode.Run(scope);
            return new ScriptScope(scope);
        }

        #endregion

        #region Scopes

        public IScriptScope/*!*/ CreateScope(IAttributesCollection dictionary) {
            return new Scope(_invariantContext, dictionary).ToScriptScope();
        }

        #endregion
        
        #region Command Dispatching

        // This can be set to a method like System.Windows.Forms.Control.Invoke for Winforms scenario 
        // to cause code to be executed on a separate thread.
        // It will be called with a null argument to indicate that the console session should be terminated.
        // Can be null.

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // TODO: fix
        public CommandDispatcher GetCommandDispatcher() {
            return _commandDispatcher;
        }

        public CommandDispatcher SetCommandDispatcher(CommandDispatcher dispatcher) {
            return Interlocked.Exchange(ref _commandDispatcher, dispatcher);
        }

        public void DispatchCommand(Delegate command) {
            CommandDispatcher dispatcher = _commandDispatcher;
            if (dispatcher != null) {
                dispatcher(command);
            }
        }

        #endregion

        #region TODO

        // TODO: remove or reduce
        public ScriptDomainOptions GlobalOptions {
            get {
                return _options;
            }
            set {
                Contract.RequiresNotNull(value, "value");
                _options = value;
            }
        }

        // TODO: remove or reduce     
        private static ScriptDomainOptions _options = new ScriptDomainOptions();

        // TODO: remove or reduce
        public static ScriptDomainOptions Options {
            get { return _options; }
        }

        #endregion

        internal string[] GetRegisteredLanguageIdentifiers(LanguageContext context) {
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

        internal string[] GetRegisteredFileExtensions(LanguageContext context) {
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
    }
}
