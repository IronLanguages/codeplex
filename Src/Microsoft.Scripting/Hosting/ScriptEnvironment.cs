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
        
        // convenience API:
#if !SILVERLIGHT
        void RedirectIO(TextReader input, TextWriter output, TextWriter errorOutput);
#endif

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

        Delegate GetDelegate(object callableObject, Type delegateType);
        
        // TODO:
        // Delegate CreateDelegate(IObjectHandle remoteCallableObject, Type delegateType);

        // TODO: remove
        ScriptDomainOptions GlobalOptions { get; set; }

    }

    public sealed class ScriptEnvironment : IScriptEnvironment, ILocalObject {

        private readonly ScriptDomainManager _manager;
        
        public IScriptHost Host {
            get { return _manager.Host; }
        }

        // TODO: remove
        public ScriptDomainOptions GlobalOptions {
            get { return _manager.GlobalOptions; }
            set { _manager.GlobalOptions = value; }
        }

        internal ScriptEnvironment(ScriptDomainManager manager) {
            Debug.Assert(manager != null);
            _manager = manager;
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
            return _manager.CreateScope(null);
        }
        
        public IScriptScope/*!*/ CreateScope(IAttributesCollection/*!*/ dictionary) {
            Contract.RequiresNotNull(dictionary, "dictionary");
            return _manager.CreateScope(dictionary);
        }

        public IScriptScope/*!*/ ExecuteSourceUnit(SourceUnit/*!*/ sourceUnit) {
            return _manager.ExecuteSourceUnit(sourceUnit);
        }

        #endregion

        #region Variables // TODO: remove

        public IAttributesCollection Variables { 
            get { return _manager.Variables; } 
            set { _manager.Variables = value; } 
        }

        public void SetVariables(IAttributesCollection dictionary) {
            Contract.RequiresNotNull(dictionary, "dictionary");
            _manager.Variables = dictionary;
        }

        public object GetVariable(CodeContext context, SymbolId name) {
            return _manager.GetVariable(context, name);
        }

        public void SetVariable(CodeContext context, SymbolId name, object value) {
            _manager.SetVariable(context, name, value);
        }

        #endregion

        #region Object Operations

        public Delegate GetDelegate(object callableObject, Type delegateType) {
            return RuntimeHelpers.GetDelegate(callableObject, delegateType);            
        }

        #endregion

        #region Convenience API (not available for Silverlight to make the assembly smaller)

#if !SILVERLIGHT

        public void RedirectIO(TextReader input, TextWriter output, TextWriter errorOutput) {
            if (input != null) Console.SetIn(input);
            if (output != null) Console.SetOut(output);
            if (errorOutput != null) Console.SetError(errorOutput);
        }
#endif
        
        #endregion
    }
}
