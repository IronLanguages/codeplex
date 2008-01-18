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

namespace Microsoft.Scripting.Hosting {

    public interface IScriptEnvironment : IRemotable {
        IScriptHost Host { get; }

        ScriptIO/*!*/ IO { get; }

        // language providers (TODO: register):
        string[] GetRegisteredFileExtensions();
        string[] GetRegisteredLanguageIdentifiers();

        IScriptEngine GetEngine(string languageId);
        IScriptEngine GetEngineByFileExtension(string extension);
        IScriptEngine GetEngine(Type languageContextType);  // TODO: Remove me
        
        // modules:
        IScriptScope/*!*/ CreateScope();
        // IScriptScope/*!*/ CreateScope(string/*!*/ languageId);
        IScriptScope/*!*/ CreateScope(IAttributesCollection/*!*/ dictionary);
        // IScriptScope/*!*/ CreateScope(string/*!*/ languageId, IAttributesCollection/*!*/ dictionary);

        IScriptScope/*!*/ ExecuteSourceUnit(SourceUnit/*!*/ sourceUnit);

#if OBSOLETE
        IScriptScope CompileModule(string name, SourceUnit sourceCode);
        IScriptScope CompileModule(string name, CompilerOptions options, ErrorSink errorSink, IAttributesCollection dictionary, SourceUnit sourceCode);
#endif

        // TODO:
        // Delegate CreateDelegate(IObjectHandle remoteCallableObject, Type delegateType);

        // TODO: remove
        ScriptDomainOptions GlobalOptions { get; set; }

        IScriptScope ExecuteFile(string path);

        void LoadAssembly(Assembly asm);
    }

    public sealed class ScriptEnvironment : IScriptEnvironment, ILocalObject {

        private readonly ScriptDomainManager/*!*/ _manager;
        private readonly ScriptIO/*!*/ _io;
        private ScriptScope/*!*/ _globals;
            
        public IScriptHost Host {
            get { return _manager.Host; }
        }

        // TODO: remove
        public ScriptDomainOptions GlobalOptions {
            get { return _manager.GlobalOptions; }
            set { _manager.GlobalOptions = value; }
        }

        public ScriptIO/*!*/ IO {
            get { return _io; }
        }

        internal ScriptEnvironment(ScriptDomainManager/*!*/ manager) {
            Debug.Assert(manager != null);
            _manager = manager;
            _io = new ScriptIO(_manager.SharedIO);
        }

        public static IScriptEnvironment Create(ScriptEnvironmentSetup setup) {
            ScriptDomainManager manager;
            if (!ScriptDomainManager.TryCreateLocal(setup, out manager))
                throw new InvalidOperationException("Environment already created in the current AppDomain");

            return manager.Environment;
        }

        public static IScriptEnvironment GetEnvironment() {
            return ScriptDomainManager.CurrentManager.Environment;
        }

#if !SILVERLIGHT
        RemoteWrapper ILocalObject.Wrap() {
            return new RemoteScriptEnvironment(_manager);
        }
        
        public static IScriptEnvironment Create() {
            return Create(null);
        }

        public static IScriptEnvironment Create(ScriptEnvironmentSetup setup, AppDomain domain) {
            Contract.RequiresNotNull(domain, "domain");

            if (domain == AppDomain.CurrentDomain) {
                return Create(setup);
            }

            RemoteScriptEnvironment rse;
            if (!RemoteScriptEnvironment.TryCreate(domain, setup, out rse))
                throw new InvalidOperationException("Environment already created in the specified AppDomain");

            return rse;
        }

        public static IScriptEnvironment GetEnvironment(AppDomain domain) {
            Contract.RequiresNotNull(domain, "domain");

            if (domain == AppDomain.CurrentDomain) {
                return GetEnvironment();
            }

            // TODO:
            throw new NotImplementedException("TODO");
        }
#endif
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

        public IScriptEngine GetEngine(string languageId) {
            return _manager.GetEngine(languageId);
        }

        public IScriptEngine GetEngineByFileExtension(string extension) {
            return _manager.GetEngineByFileExtension(extension);
        }

        /// <summary>
        /// Temporary, shouldn't exist
        /// </summary>
        public IScriptEngine GetEngine(Type languageContextType) {
            return _manager.GetEngine(languageContextType);
        }

        #region Compilation, Module Creation

        public IScriptScope/*!*/ CreateScope() {
            return InvariantEngine.CreateScope();
        }
        
        public IScriptScope/*!*/ CreateScope(IAttributesCollection/*!*/ dictionary) {
            Contract.RequiresNotNull(dictionary, "dictionary");
            return InvariantEngine.CreateScope(dictionary);
        }

        public IScriptScope/*!*/ ExecuteSourceUnit(SourceUnit/*!*/ sourceUnit) {
            return GetEngine(sourceUnit.LanguageContext.GetType()).CreateScope(_manager.ExecuteSourceUnit(sourceUnit).Dict);
        }

        #endregion

        #region TODO: New API

        // TODO: file IO exceptions, parse exceptions, execution exceptions, etc.
        /// <exception cref="ArgumentException">
        /// path is empty, contains one or more of the invalid characters defined in GetInvalidPathChars or doesn't have an extension.
        /// </exception>
        public IScriptScope/*!*/ ExecuteFile(string/*!*/ path) {
            Contract.RequiresNotEmpty(path, "path");
            string extension = Path.GetExtension(path);
            
            ScriptEngine engine;
            if(!_manager.TryGetEngineByFileExtension(extension, out engine)) {
                throw new ArgumentException(String.Format("File extension '{0}' is not associated with any language.", extension));
            }

            SourceUnit unit = engine.CreateScriptSourceFromFile(path, StringUtils.DefaultEncoding);
            IScriptScope scope = engine.CreateScope();
            engine.Execute(scope, unit);

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

        internal IScriptEngine InvariantEngine {
            get {
                return GetEngine(typeof(InvariantContext));
            }
        }
    }
}
