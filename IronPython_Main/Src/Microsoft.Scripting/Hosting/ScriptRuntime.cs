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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Scripting;
using System.IO;
using System.Diagnostics;
using Microsoft.Scripting.Generation;
using System.Text;
using System.Runtime.Remoting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using System.Threading;
using System.Security.Permissions;

namespace Microsoft.Scripting.Hosting {

    public sealed class ScriptRuntime 
#if !SILVERLIGHT
        : MarshalByRefObject 
#endif
    {
        private readonly Dictionary<LanguageContext, ScriptEngine>/*!*/ _engines;
        private readonly ScriptDomainManager/*!*/ _manager;
        private readonly ScriptIO/*!*/ _io;
        private readonly ScriptHost/*!*/ _host;
        private ScriptScope _globals;

        internal ScriptDomainManager/*!*/ Manager {
            get { return _manager; }
        }

        public ScriptHost/*!*/ Host {
            get { return _host; }
        }

        // TODO: remove
        public ScriptDomainOptions/*!*/ GlobalOptions {
            get { return _manager.GlobalOptions; }
            set { _manager.GlobalOptions = value; }
        }

        public ScriptIO/*!*/ IO {
            get { return _io; }
        }

        private ScriptRuntime(ScriptDomainManager/*!*/ manager, ScriptHost/*!*/ host) {
            Assert.NotNull(manager);
            _manager = manager;
            _host = host;
            _io = new ScriptIO(_manager.SharedIO);
            _engines = new Dictionary<LanguageContext, ScriptEngine>();

            bool freshEngineCreated;
            _globals = new ScriptScope(GetEngineNoLockNoNotification(manager.Globals.Language, out freshEngineCreated), manager.Globals);
        }

        public static ScriptRuntime/*!*/ Create() {
            return CreateInternal(null, null);
        }

        public static ScriptRuntime/*!*/ Create(ScriptRuntimeSetup/*!*/ setup) {
            Contract.RequiresNotNull(setup, "setup");
            return CreateInternal(null, setup);
        }

        public static ScriptRuntime/*!*/ CreateInternal(AppDomain domain, ScriptRuntimeSetup setup) {
            if (domain != null && domain != AppDomain.CurrentDomain) {
#if SILVERLIGHT
                throw Assert.Unreachable;
#else
                return RemoteRuntimeFactory.CreateRuntime(domain, setup);
#endif
            }
            
            if (setup == null) {
                setup = GetSetupInformation();
            }

            ScriptHost host = ReflectionUtils.CreateInstance<ScriptHost>(setup.HostType, setup.HostArguments);

            ScriptHostProxy hostProxy = new ScriptHostProxy(host);

            ScriptDomainManager manager = new ScriptDomainManager(hostProxy);

            // set the manager up:
            setup.RegisterLanguages(manager);
            
            ScriptRuntime runtime = new ScriptRuntime(manager, host);

            // runtime needs to be all set at this point, host code is called:
            Thread.MemoryBarrier();

            host.SetRuntime(runtime);
            return runtime;
        }

        #region Remoting

#if !SILVERLIGHT
        public static ScriptRuntime/*!*/ Create(AppDomain/*!*/ domain) {
            return CreateInternal(domain, null);
        }
        
        public static ScriptRuntime/*!*/ Create(AppDomain/*!*/ domain, ScriptRuntimeSetup/*!*/ setup) {
            Contract.RequiresNotNull(domain, "domain");
            return CreateInternal(domain, setup);
        }

        private sealed class RemoteRuntimeFactory : MarshalByRefObject {
            public readonly ScriptRuntime/*!*/ Runtime;

            public RemoteRuntimeFactory(ScriptRuntimeSetup setup) {
                Runtime = ScriptRuntime.CreateInternal(null, setup);
            }

            public static ScriptRuntime/*!*/ CreateRuntime(AppDomain/*!*/ domain, ScriptRuntimeSetup setup) {
                RemoteRuntimeFactory rd = (RemoteRuntimeFactory)domain.CreateInstanceAndUnwrap(typeof(RemoteRuntimeFactory).Assembly.FullName,
                    typeof(RemoteRuntimeFactory).FullName, false, BindingFlags.Default, null, new object[] { setup }, null, null, null);

                return rd.Runtime;
            }
        }

        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
        #endregion

        #region Configuration

        private static ScriptRuntimeSetup/*!*/ GetSetupInformation() {
#if !SILVERLIGHT
            ScriptRuntimeSetup result;

            // setup provided by app-domain creator:
            result = ScriptRuntimeSetup.GetAppDomainAssociated(AppDomain.CurrentDomain);
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
            return new ScriptRuntimeSetup(true);
        }

        #endregion

        public string[] GetRegisteredFileExtensions() {
            return _manager.GetRegisteredFileExtensions();
        }

        public string[] GetRegisteredLanguageIdentifiers() {
            return _manager.GetRegisteredLanguageIdentifiers();
        }

        internal string[] GetRegisteredFileExtensions(LanguageContext context) {
            return _manager.GetRegisteredFileExtensions(context);
        }

        internal string[] GetRegisteredLanguageIdentifiers(LanguageContext context) {
            return _manager.GetRegisteredLanguageIdentifiers(context);
        }

        #region Engines

        public ScriptEngine/*!*/ GetEngine(string/*!*/ languageId) {
            Contract.RequiresNotNull(languageId, "languageId");

            ScriptEngine engine;
            if (!TryGetEngine(languageId, out engine)) {
                throw new ArgumentException(Resources.UnknownLanguageId);
            }

            return engine;
        }

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public ScriptEngine/*!*/ GetEngineByFileExtension(string/*!*/ extension) {
            Contract.RequiresNotNull(extension, "extension");

            ScriptEngine engine;
            if (!TryGetEngineByFileExtension(extension, out engine)) {
                throw new ArgumentException(Resources.UnknownLanguageId); // TODO: wrong resource
            }

            return engine;
        }

        public bool TryGetEngine(string languageId, out ScriptEngine engine) {
            LanguageContext language;
            if (!_manager.TryGetLanguageContext(languageId, out language)) {
                engine = null;
                return false;
            }

            engine = GetEngine(language);
            return true;
        }

        public bool TryGetEngineByFileExtension(string extension, out ScriptEngine engine) {
            LanguageContext language;
            if (!_manager.TryGetLanguageContextByFileExtension(extension, out language)) {
                engine = null;
                return false;
            }

            engine = GetEngine(language);
            return true;
        }

        // TODO: remove
        public ScriptEngine GetEngine(Type/*!*/ languageContextType) {
            return GetEngine(_manager.GetLanguageContext(languageContextType));
        }

        internal ScriptEngine/*!*/ GetEngine(LanguageContext/*!*/ language) {
            Assert.NotNull(language);

            ScriptEngine engine;
            bool freshEngineCreated;
            lock (_engines) {
                engine = GetEngineNoLockNoNotification(language, out freshEngineCreated);
            }

            if (freshEngineCreated && !ReferenceEquals(language, _manager.InvariantContext)) {
                _host.EngineCreated(engine);
            }

            return engine;
        }

        private ScriptEngine/*!*/ GetEngineNoLockNoNotification(LanguageContext/*!*/ language, out bool freshEngineCreated) {
            Debug.Assert(_engines != null, "Invalid ScriptRuntime initialiation order");

            ScriptEngine engine;
            if (freshEngineCreated = !_engines.TryGetValue(language, out engine)) {
                engine = new ScriptEngine(this, language);
                Thread.MemoryBarrier();
                _engines.Add(language, engine);
            }
            return engine;
        }

        #endregion

        #region Compilation, Module Creation

        public ScriptScope/*!*/ CreateScope() {
            return InvariantEngine.CreateScope();
        }
        
        public ScriptScope/*!*/ CreateScope(IAttributesCollection/*!*/ dictionary) {
            Contract.RequiresNotNull(dictionary, "dictionary");
            return InvariantEngine.CreateScope(dictionary);
        }

        public ScriptScope/*!*/ ExecuteSourceUnit(ScriptSource/*!*/ source) {
            Scope scope = _manager.ExecuteSourceUnit(source.SourceUnit);
            return new ScriptScope(source.Engine, scope); 
        }

        #endregion

        #region TODO: New API

        // TODO: file IO exceptions, parse exceptions, execution exceptions, etc.
        /// <exception cref="ArgumentException">
        /// path is empty, contains one or more of the invalid characters defined in GetInvalidPathChars or doesn't have an extension.
        /// </exception>
        public ScriptScope/*!*/ ExecuteFile(string/*!*/ path) {
            Contract.RequiresNotEmpty(path, "path");
            string extension = Path.GetExtension(path);
            
            ScriptEngine engine;
            if(!TryGetEngineByFileExtension(extension, out engine)) {
                throw new ArgumentException(String.Format("File extension '{0}' is not associated with any language.", extension));
            }

            ScriptSource source = engine.CreateScriptSourceFromFile(path, StringUtils.DefaultEncoding);
            ScriptScope scope = engine.CreateScope();
            source.Execute(scope);

            return scope;
        }

        /// <summary>
        /// This property returns the "global object" or name bindings of the ScriptRuntime as a ScriptScope.  
        /// 
        /// You can set the globals scope, which you might do if you created a ScriptScope with an 
        /// IAttributesCollection so that your host could late bind names.
        /// </summary>
        public ScriptScope Globals {
            get { return _globals; }
            set {
                Contract.RequiresNotNull(value, "value");

                // TODO: this is wrong, we ignore other parts of the scope here
                _globals = value;
                _manager.SetGlobalsDictionary(_globals.Scope.Dict);
            }
        }

        /// <summary>
        /// This method walks the assembly's namespaces and name bindings to ScriptRuntime.Globals 
        /// to represent the types available in the assembly.  Each top-level namespace name gets 
        /// bound in Globals to a dynamic object representing the namespace.  Within each top-level 
        /// namespace object, nested namespace names are bound to dynamic objects representing each 
        /// tier of nested namespaces.  When this method encounters the same namespace-qualified name, 
        /// it merges names together objects representing the namespaces.
        /// </summary>
        /// <param name="assembly"></param>
        public void LoadAssembly(Assembly assembly) {
            _manager.LoadAssembly(assembly);
        }

        #endregion

        internal ScriptEngine InvariantEngine {
            get {
                return GetEngine(_manager.InvariantContext);
            }
        }

        internal ScriptDomainManager DomainManager {
            get {
                return _manager;
            }
        }

        public PlatformAdaptationLayer PAL {
            get {
                return DomainManager.PAL;
            }
        }
    }
}
