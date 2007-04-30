/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
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
using Microsoft.Scripting.Internal.Generation;
using System.Text;

namespace Microsoft.Scripting.Hosting {

    public interface IScriptEnvironment : IRemotable {
        IScriptHost Host { get; }
        
        // convenience API:
#if !SILVERLIGHT
        SourceFileUnit CreateSourceFileUnit(string path);
        void RedirectIO(TextReader input, TextWriter output, TextWriter errorOutput);
#endif

        // language providers (TODO: register):
        string[] GetRegisteredFileExtensions();
        string[] GetRegisteredLanguageIdentifiers();
        ILanguageProvider GetLanguageProvider(string languageId);
        ILanguageProvider GetLanguageProvider(Type languageProviderType);
        ILanguageProvider GetLanguageProviderByFileExtension(string extension);
        
        // modules:
        IScriptModule CreateModule(string name, params ICompiledCode[] compiledCodes);
        IScriptModule CreateModule(string name, IAttributesCollection dictionary, params ICompiledCode[] compiledCodes);
        IScriptModule CompileModule(string name, params SourceUnit[] sourceUnits);
        IScriptModule CompileModule(string name, CompilerOptions options, ErrorSink errorSink, IAttributesCollection dictionary,  params SourceUnit[] sourceUnits);

        void PublishModule(IScriptModule module);
        void PublishModule(IScriptModule module, string publicName);
        IDictionary<string, IScriptModule> GetPublishedModules();
        
        ICompiledCode CompileSourceUnit(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink);

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
            if (domain == null) throw new ArgumentNullException("domain");

            if (domain == AppDomain.CurrentDomain) {
                return Create(setup);
            }

            RemoteScriptEnvironment rse;
            if (!RemoteScriptEnvironment.TryCreate(domain, setup, out rse))
                throw new InvalidOperationException("Environment already created in the specified AppDomain");

            return rse;
        }

        public static IScriptEnvironment GetEnvironment(AppDomain domain) {
            if (domain == null) throw new ArgumentNullException("domain");

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

        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        /// <exception cref="ArgumentException"><paramref name="type"/></exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language provider's implementation failed to instantiate.</exception>
        public ILanguageProvider GetLanguageProvider(string languageId) {
            return _manager.GetLanguageProvider(languageId);
        }

        public ILanguageProvider GetLanguageProvider(Type languageProviderType) {
            return _manager.GetLanguageProvider(languageProviderType);
        }

        public ILanguageProvider GetLanguageProviderByFileExtension(string extension) {
            return _manager.GetLanguageProviderByFileExtension(extension);
        }

        #region Compilation, Module Creation

        public IScriptModule CreateModule(string name, params ICompiledCode[] compiledCodes) {
            return CreateModule(name, null, compiledCodes);
        }

        /// <summary>
        /// Creates a module.
        /// </summary>
        /// <c>dictionary</c> can be <c>null</c>
        /// <returns></returns>
        public IScriptModule CreateModule(string name, IAttributesCollection dictionary, params ICompiledCode[] compiledCodes) {
            Utils.Array.CheckNonNullElements(compiledCodes, "compiledCodes");

            ScriptCode[] script_codes = new ScriptCode[compiledCodes.Length];
            for (int i = 0; i < compiledCodes.Length; i++) {
                script_codes[i] = ScriptCode.FromCompiledCode(RemoteWrapper.TryGetLocal<CompiledCode>(compiledCodes[i]));
                if (script_codes[i] == null) {
                    throw new ArgumentException(Resources.RemoteCodeModuleComposition, String.Format("{0}[{1}]", "compiledCodes", i));
                }
            }

            return _manager.CreateModule(name, new Scope(dictionary), script_codes);
        }

        public IScriptModule CompileModule(string name, params SourceUnit[] sourceUnits) {
            return CompileModule(name, null, null, null, sourceUnits);
        }

        /// <summary>
        /// Compiles a list of source units into a single module.
        /// <c>options</c> can be <c>null</c>
        /// <c>errroSink</c> can be <c>null</c>
        /// <c>dictionary</c> can be <c>null</c>
        /// </summary>
        public IScriptModule CompileModule(string name, CompilerOptions options, ErrorSink errorSink, IAttributesCollection dictionary, 
            params SourceUnit[] sourceUnits) {

            return _manager.CompileModule(name, new Scope(dictionary), options, errorSink, sourceUnits);
        }

        /// <summary>
        /// Compiles a source unit in the current environment.
        /// Used by remote hosts that need to compile source units in different app-domain than they are running in.
        /// <c>options</c> can be <c>null</c>
        /// <c>errorSink</c> can be <c>null</c>
        /// </summary>
        public ICompiledCode CompileSourceUnit(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink) {
            if (sourceUnit == null) throw new ArgumentNullException("sourceUnit");
            return sourceUnit.Compile(options, errorSink);
        }

        public void PublishModule(IScriptModule module) {
            _manager.PublishModule(RemoteWrapper.GetLocalArgument<ScriptModule>(module, "module"));
        }

        public void PublishModule(IScriptModule module, string publicName) {
            _manager.PublishModule(RemoteWrapper.GetLocalArgument<ScriptModule>(module, "module"), publicName);
        }

        public IDictionary<string, IScriptModule> GetPublishedModules() {
            IDictionary<string, ScriptModule> local_modules = _manager.GetPublishedModules();

            IDictionary<string, IScriptModule> result = new Dictionary<string, IScriptModule>(local_modules.Count);
            foreach (KeyValuePair<string, ScriptModule> local_module in local_modules) {
                result.Add(local_module.Key, local_module.Value);
            }

            return result;
        }

        #endregion

        #region Variables

        public IAttributesCollection Variables { 
            get { return _manager.Variables; } 
            set { _manager.Variables = value; } 
        }

        public void SetVariables(IAttributesCollection dictionary) {
            if (dictionary == null) throw new ArgumentNullException("dictionary");
            _manager.Variables = dictionary;
        }

        public object GetVariable(CodeContext context, SymbolId name) {
            return _manager.GetVariable(context, name);
        }

        public void SetVariable(CodeContext context, SymbolId name, object value) {
            _manager.SetVariable(context, name, value);
        }

        #endregion

        #region Convenience API (not available for Silverlight to make the assembly smaller)

#if !SILVERLIGHT

        public SourceFileUnit CreateSourceFileUnit(string path) {
            string extension = Path.GetExtension(path);
            return new SourceFileUnit(_manager.GetLanguageProviderByFileExtension(extension).GetEngine(), path, Encoding.Default);
        }

        public void RedirectIO(TextReader input, TextWriter output, TextWriter errorOutput) {
            if (input != null) Console.SetIn(input);
            if (output != null) Console.SetOut(output);
            if (errorOutput != null) Console.SetError(errorOutput);
        }
#endif
        
        #endregion
    }
}
