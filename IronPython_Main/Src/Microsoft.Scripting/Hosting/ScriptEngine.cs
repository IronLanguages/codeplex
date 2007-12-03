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
using System.Runtime.Remoting;
using System.Threading;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    public delegate T ModuleBinder<T>(ScriptScope scope);

    public interface IScriptEngine : IRemotable {
        ILanguageProvider LanguageProvider { get; }

        Guid LanguageGuid { get; }
        Guid VendorGuid { get; }
        EngineOptions Options { get; }
        string VersionString { get; }

        // TODO: 
        // exception handling:
        string FormatException(Exception exception);
        void GetExceptionMessage(Exception exception, out string message, out string typeName);

        // configuration:
        void SetSourceUnitSearchPaths(string[] paths);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // TODO: fix
        CompilerOptions GetDefaultCompilerOptions();

        SourceCodeProperties GetCodeProperties(string code, SourceCodeKind kind);
        SourceCodeProperties GetCodeProperties(string code, SourceCodeKind kind, ErrorSink errorSink);
        
        int ExecuteProgram(SourceUnit sourceUnit);
        void PublishModule(IScriptScope module);
        void Shutdown();

        // convenience API:
        void Execute(string code);
        void Execute(string code, IScriptScope module);
        void ExecuteFile(string path);
        void ExecuteFileContent(string path);
        void ExecuteFileContent(string path, IScriptScope module);
        void ExecuteCommand(string code);
        void ExecuteCommand(string code, IScriptScope module);
        void ExecuteInteractiveCode(string code);
        void ExecuteInteractiveCode(string code, IScriptScope module);
        void ExecuteSourceUnit(SourceUnit sourceUnit, IScriptScope module);

        object Evaluate(string expression);
        object Evaluate(string expression, IScriptScope module);
        object EvaluateSourceUnit(SourceUnit sourceUnit, IScriptScope module);

        ObjectOperations Operations {
            get;
        }

        ObjectOperations CreateOperations();

        // code sense:
        bool TryGetVariable(string name, IScriptScope module, out object obj);

        IScriptScope CompileFile(string path, string moduleName);
        ICompiledCode CompileFileContent(string path);
        ICompiledCode CompileFileContent(string path, IScriptScope module);
        ICompiledCode CompileCode(string code);
        ICompiledCode CompileCode(string code, IScriptScope module);
        ICompiledCode CompileExpression(string expression, IScriptScope module);
        ICompiledCode CompileStatements(string statement, IScriptScope module);
        ICompiledCode CompileInteractiveCode(string code);
        ICompiledCode CompileInteractiveCode(string code, IScriptScope module);
        ICompiledCode CompileSourceUnit(SourceUnit sourceUnit, IScriptScope module);
        ICompiledCode CompileSourceUnit(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink);

#if !SILVERLIGHT

        // sourcePath and module can be null
        ICompiledCode CompileCodeDom(System.CodeDom.CodeMemberMethod code, IScriptScope module);

        ObjectHandle EvaluateAndWrap(string expression);
        ObjectHandle EvaluateAndWrap(string expression, IScriptScope module);

        // code sense:
        bool TryGetVariableAndWrap(string name, IScriptScope module, out ObjectHandle obj);

#endif

                
        // TODO: (internal)
        CompilerOptions GetModuleCompilerOptions(ScriptScope module);

        // TODO: output
        TextWriter GetOutputWriter(bool isErrorOutput);

        // TODO: this shouldn't be here:
        ActionBinder DefaultBinder { get; }
        
        // TODO:
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // TODO: fix
        ErrorSink GetCompilerErrorSink();
    }

    public abstract class ScriptEngine : IScriptEngine, ILocalObject {
        private readonly LanguageProvider _provider;
        private readonly EngineOptions _options;
        private readonly LanguageContext _languageContext;
        private ObjectOperations _operations;

        #region Properties

        protected virtual string DefaultSourceCodeUnitName { get { return "<string>"; } }

        public LanguageProvider LanguageProvider {
            get {
                return _provider;
            }
        }

        public LanguageContext LanguageContext {
            get { return _languageContext; }
        }

        public EngineOptions Options {
            get { return _options; }
        }

        public virtual string Copyright {
            get {
                return "Copyright (c) Microsoft Corporation. All rights reserved.";
            }
        }

        public virtual string VersionString {
            get {
                return String.Format("DLR Scripting Engine on .NET {0}", Environment.Version);
            }
        }

        //TODO these three properties should become abstract and updated for all implementations
        public virtual Guid LanguageGuid {
            get {
                return Guid.Empty;
            }
        }

        public virtual Guid VendorGuid {
            get {
                return SymbolGuids.LanguageVendor_Microsoft;
            }
        }

        // TODO: provide default implementation, remove from engine
        public abstract ActionBinder DefaultBinder { get; }

        #endregion

        protected ScriptEngine(LanguageProvider provider, EngineOptions engineOptions, LanguageContext languageContext) {
            Contract.RequiresNotNull(provider, "provider");
            Contract.RequiresNotNull(engineOptions, "engineOptions");
            Contract.RequiresNotNull(languageContext, "languageContext");

#if !SILVERLIGHT // SecurityPermission
            if (engineOptions.ClrDebuggingEnabled) {
                // Currently, AssemblyBuilder.DefineDynamicModule requires high trust for emitting debug information.
                new System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode).Demand();
            }
#endif
            _provider = provider;
            _options = engineOptions;
            _languageContext = languageContext;
        }

        #region IScriptEngine Members

#if !SILVERLIGHT
        RemoteWrapper ILocalObject.Wrap() {
            return new RemoteScriptEngine(this);
        }
#endif

        ILanguageProvider IScriptEngine.LanguageProvider {
            get { return LanguageProvider; }
        }

        public virtual void ModuleCreated(ScriptScope module) {
            // nop
        }

        public virtual void SetSourceUnitSearchPaths(string[] paths) {
            // nop
        }

        #endregion        

        #region Object Operations

        public ObjectOperations Operations {
            get {
                if (_operations == null) {
                    Interlocked.CompareExchange(ref _operations, CreateOperations(), null);
                }

                return _operations;
            }
        }

        public ObjectOperations CreateOperations() {
            return new ObjectOperations(GetCodeContext(ScriptDomainManager.CurrentManager.Host.DefaultModule));
        }

        public bool TryGetVariable(string name, IScriptScope module, out object obj) {
            CodeContext context = GetCodeContext(module);
            return context.LanguageContext.TryLookupName(context, SymbolTable.StringToId(name), out obj);
        }
        
        #endregion

        #region Evaluation

        /// <summary>
        /// Base implementation of Evaluate -evaluates the given expression in the scope of the provided
        /// ScriptModule.
        /// </summary>
        public object Evaluate(string expression) {
            return Evaluate(expression, null);
        }

        public object Evaluate(string expression, IScriptScope module) {
            Contract.RequiresNotNull(expression, "expression");
            return CompileExpression(expression, module).Evaluate(module);      
        }

        public object EvaluateSourceUnit(SourceUnit sourceUnit, IScriptScope module) {
            Contract.RequiresNotNull(sourceUnit, "sourceUnit");
            return CompileSourceUnit(sourceUnit, module).Evaluate(module);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T EvaluateAs<T>(string expression) {
            return EvaluateAs<T>(expression, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T EvaluateAs<T>(string expression, IScriptScope module) {
            return Operations.ConvertTo<T>(Evaluate(expression, module));
        }

        #endregion

        #region Parsing

        public SourceCodeProperties GetCodeProperties(string code, SourceCodeKind kind) {
            return GetCodeProperties(code, kind, null);
        }

        public SourceCodeProperties GetCodeProperties(string code, SourceCodeKind kind, ErrorSink errorSink) {
            Contract.RequiresNotNull(code, "code");
            SourceUnit sourceUnit = SourceUnit.CreateSnippet(this, code, kind);
            
            // create compiler context with null error sink:
            CompilerContext compilerContext = new CompilerContext(sourceUnit, null, errorSink ?? new ErrorSink());
            
            _languageContext.UpdateSourceCodeProperties(compilerContext);

            if (!sourceUnit.CodeProperties.HasValue) {
                throw new InvalidImplementationException();
            }

            return sourceUnit.CodeProperties.Value;
        }

        #endregion

        #region Compilation and Execution

        /// <summary>
        /// Compiler options factory.
        /// </summary>
        public virtual CompilerOptions GetDefaultCompilerOptions() {
            return new CompilerOptions();
        }

        /// <summary>
        /// Creates compiler options initialized by the options associated with the module.
        /// </summary>
        public virtual CompilerOptions GetModuleCompilerOptions(ScriptScope module) { // TODO: internal protected
            return GetDefaultCompilerOptions();
        }

        public virtual ErrorSink GetCompilerErrorSink() {
            return new ErrorSink();
        }

        /// <summary>
        /// Execute a source unit as a program and return its exit code.
        /// </summary>
        public virtual int ExecuteProgram(SourceUnit sourceUnit) {
            Contract.RequiresNotNull(sourceUnit, "sourceUnit");

            ExecuteSourceUnit(sourceUnit, null);
            return 0;
        }

        public void ExecuteCommand(string code) {
            ExecuteCommand(code, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void ExecuteCommand(string code, IScriptScope module) {
            CommandDispatcher dispatcher = ScriptDomainManager.CurrentManager.GetCommandDispatcher();

            if (dispatcher != null) {
                Exception exception = null;
                ICompiledCode compiled_code = CompileInteractiveCode(code, module);
                if (compiled_code != null) { // TODO: should throw?

                    CallTarget0 run_code = delegate() {
                        try {
                            PrintInteractiveCodeResult(compiled_code.Evaluate(module));
                        } catch (Exception e) {
                            exception = e;
                        }
                        return null;
                    };

                    dispatcher(run_code);

                    // We catch and rethrow the exception since it could have been thrown on another thread
                    if (exception != null)
                        throw exception;
                }
            } else {
                ExecuteInteractiveCode(code, module);
            }
        }

        // VB should compile ?<expr> to the print statement
        protected virtual void PrintInteractiveCodeResult(object obj) {
            // nop
        }
            
        /// <summary>
        /// Execute a script file in a new module. Convenience API.
        /// </summary>
        public void ExecuteFile(string path) {
            CompileFile(path, Path.GetFileNameWithoutExtension(path)).Execute();
        }

        public void ExecuteFileContent(string path) {
            CompileFileContent(path).Execute();
        }

        public void ExecuteFileContent(string path, IScriptScope module) {
            CompileFileContent(path, module).Execute(module);
        }
        
        public void ExecuteInteractiveCode(string code) {
            ExecuteInteractiveCode(code, null);
        }

        public void ExecuteInteractiveCode(string code, IScriptScope module) {
            ICompiledCode cc = CompileInteractiveCode(code, module);
            if (cc != null) {
                PrintInteractiveCodeResult(cc.Evaluate(module));
            }
        }

        public void Execute(string code) {
            Execute(code, null);
        }

        /// <summary>
        /// Execute a snippet of code within the scope of the specified module. Convenience API.
        /// </summary>
        public void Execute(string code, IScriptScope module) {
            Contract.RequiresNotNull(code, "code");
            ExecuteSourceUnit(SourceUnit.CreateSnippet(this, code), module);
        }

        public void ExecuteSourceUnit(SourceUnit sourceUnit, IScriptScope module) {
            Contract.RequiresNotNull(sourceUnit, "sourceUnit");
            CompileSourceUnit(sourceUnit, module).Execute(module);
        }

        public ICompiledCode CompileSourceUnit(SourceUnit sourceUnit, IScriptScope module) {
            Contract.RequiresNotNull(sourceUnit, "sourceUnit");
            CompilerOptions options = (module != null) ? module.GetCompilerOptions(this) : GetDefaultCompilerOptions();
            return new CompiledCode(_languageContext.CompileSourceCode(sourceUnit, options));
        }

        public ICompiledCode CompileSourceUnit(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink) {
            Contract.RequiresNotNull(sourceUnit, "sourceUnit");
            return new CompiledCode(_languageContext.CompileSourceCode(sourceUnit, options, errorSink));
        }
        
        /// <summary>
        /// Convenience hosting API.
        /// </summary>
        public ICompiledCode CompileCode(string code) {
            return CompileCode(code, null);
        }

        /// <summary>
        /// Convenience hosting API.
        /// </summary>
        public ICompiledCode CompileCode(string code, IScriptScope module) {
            Contract.RequiresNotNull(code, "code");
            return CompileSourceUnit(SourceUnit.CreateSnippet(this, code), module);
        }

        /// <summary>
        /// Comvenience hosting API.
        /// </summary>
        public ICompiledCode CompileExpression(string expression, IScriptScope module) {
            Contract.RequiresNotNull(expression, "expression");
            
            // TODO: remove TrimStart
            return CompileSourceUnit(SourceUnit.CreateSnippet(this, expression.TrimStart(' ', '\t'), SourceCodeKind.Expression), module);
        }

        /// <summary>
        /// Comvenience hosting API.
        /// </summary>
        public ICompiledCode CompileStatements(string statement, IScriptScope module) {
            Contract.RequiresNotNull(statement, "statement");
            return CompileSourceUnit(SourceUnit.CreateSnippet(this, statement, SourceCodeKind.Statements), module);
        }

        /// <summary>
        /// Convenience hosting API.
        /// </summary>
        public IScriptScope CompileFile(string path, string moduleName) {
            Contract.RequiresNotNull(path, "path");
            
            if (moduleName == null) {
                moduleName = Path.GetFileNameWithoutExtension(path);
            }

            SourceUnit sourceUnit = ScriptDomainManager.CurrentManager.Host.TryGetSourceFileUnit(this, path, StringUtils.DefaultEncoding);
            if (sourceUnit == null) {
                throw new FileNotFoundException();
            }

            return ScriptDomainManager.CurrentManager.CompileModule(moduleName, sourceUnit);
        }

        public ICompiledCode CompileFileContent(string path) {
            return CompileFileContent(path, null);
        }
        
        public ICompiledCode CompileFileContent(string path, IScriptScope module) {
            Contract.RequiresNotNull(path, "path");

            SourceUnit sourceUnit = ScriptDomainManager.CurrentManager.Host.TryGetSourceFileUnit(this, path, StringUtils.DefaultEncoding);
            if (sourceUnit == null) {
                throw new FileNotFoundException();
            }

            return CompileSourceUnit(sourceUnit, module);
        }
        
        public ICompiledCode CompileInteractiveCode(string code) {
            return CompileInteractiveCode(code, null);
        }

        public ICompiledCode CompileInteractiveCode(string code, IScriptScope module) {
            Contract.RequiresNotNull(code, "code");
            return CompileSourceUnit(SourceUnit.CreateSnippet(this, code, SourceCodeKind.InteractiveCode), module);
        }

#if !SILVERLIGHT
        public ICompiledCode CompileCodeDom(System.CodeDom.CodeMemberMethod code, IScriptScope module) {
            Contract.RequiresNotNull(code, "code");

            CompilerOptions options = (module != null) ? module.GetCompilerOptions(this) : GetDefaultCompilerOptions();
            return CompileSourceUnit(_languageContext.GenerateSourceCode(code), module);
        }
#endif

        #endregion

        #region ObjectHandle Wrappings
#if !SILVERLIGHT

        public bool TryGetVariableAndWrap(string name, IScriptScope module, out ObjectHandle obj) {
            object local_obj;
            bool result = TryGetVariable(name, module, out local_obj);
            obj = new ObjectHandle(local_obj);
            return result;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/></exception>
        public ObjectHandle EvaluateAndWrap(string expression) {
            return new ObjectHandle(Evaluate(expression));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="module">Can be <c>null</c>.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/></exception>
        public ObjectHandle EvaluateAndWrap(string expression, IScriptScope module) {
            return new ObjectHandle(Evaluate(expression, module));
        }


#endif
        #endregion

        #region CodeContext/LangaugeContext - TODO: move to LanguageContext

        // Gets a LanguageContext for the specified module that captures the current state 
        // of the module which will be used for compilation and execution of the next piece of code against the module.
        private CodeContext GetCodeContext(IScriptScope module) {
            return GetCodeContext(RemoteWrapper.GetLocalArgument<ScriptScope>(module ?? 
                ScriptDomainManager.CurrentManager.Host.DefaultModule, "module"));
        }

        internal protected CodeContext GetCodeContext(ScriptScope module) {
            Contract.RequiresNotNull(module, "module");
            LanguageContext languageContext = GetLanguageContext(module);
            ModuleContext moduleContext = languageContext.EnsureModuleContext(module);
            return new CodeContext(module.Scope, languageContext, moduleContext);
        }

        internal protected virtual LanguageContext GetLanguageContext(ScriptScope module) {
            Contract.RequiresNotNull(module, "module");
            return GetLanguageContext(module.GetCompilerOptions(this));
        }
        
        internal protected virtual LanguageContext GetLanguageContext(CompilerOptions compilerOptions) {
            return InvariantContext.Instance;
        }

        #endregion

        public virtual void PublishModule(IScriptScope module) {
            // nop
        }

        #region Exception handling

        public virtual string FormatException(Exception exception) {
            Contract.RequiresNotNull(exception, "exception");
            return exception.ToString();
        }

        public virtual void GetExceptionMessage(Exception exception, out string message, out string typeName) {
            Contract.RequiresNotNull(exception, "exception");
            message = exception.ToString();
            typeName = exception.GetType().Name;
        }

        #endregion

        #region Console Support

        public virtual TextWriter GetOutputWriter(bool isErrorOutput) {
            return isErrorOutput ? Console.Error : Console.Out;
        }

        #endregion

        public virtual void Shutdown() {
        }

        public void DumpDebugInfo() {
            if (ScriptDomainManager.Options.EngineDebug) {
                PerfTrack.DumpStats();
                try {
                    ScriptDomainManager.CurrentManager.Snippets.Dump();
                } catch (NotSupportedException) { } // usually not important info...
            }
        }

        #region // TODO: Microsoft.Scripting.Vestigial Workarounds (used from MSV instead of PythonEngine)
        
        #endregion

    }

    // TODO: (dependency workaround, should be in Python's assembly): 
    [Flags]
    public enum ModuleOptions {
        None = 0x0000,
        PublishModule = 0x0001,
        TrueDivision = 0x0002,
        ShowClsMethods = 0x0004
    }
}
