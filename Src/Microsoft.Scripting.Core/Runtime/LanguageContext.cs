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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

// TODO: remove HAPI references:
using Hosting_EngineTextContentProvider = Microsoft.Scripting.Hosting.EngineTextContentProvider;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Provides language specific facilities which are typicalled called by the runtime.
    /// </summary>
    public abstract class LanguageContext
#if !SILVERLIGHT
 : ICloneable
#endif
    {
        private readonly ScriptDomainManager/*!*/ _domainManager;
        private static readonly ModuleGlobalCache _noCache = new ModuleGlobalCache(ModuleGlobalCache.NotCaching);
        private ActionBinder _binder;
        private readonly ContextId _id;

        protected LanguageContext(ScriptDomainManager/*!*/ domainManager) {
            ContractUtils.RequiresNotNull(domainManager, "domainManager");

            _domainManager = domainManager;
            _id = domainManager.AssignContextId(this);
        }
        
        public ActionBinder Binder {
            get {
                return _binder;
            }
            protected set {
                _binder = value;
            }
        }

        /// <summary>
        /// Provides the ContextId which includes members that should only be shown for this LanguageContext.
        /// 
        /// ContextId's are used for filtering by Scope's.
        /// 
        /// TODO: Not virtual, TestContext currently depends on being able to override.
        /// </summary>
        public virtual ContextId ContextId {
            get {
                return _id;
            }
        }

        /// <summary>
        /// Gets the ScriptDomainManager that this LanguageContext is running within.
        /// </summary>
        public ScriptDomainManager/*!*/ DomainManager {
            get { return _domainManager; }
        }

        /// <summary>
        /// Whether the language can parse code and create source units.
        /// </summary>
        internal virtual bool CanCreateSourceCode {
            get { return true; }
        }

        #region Scope

        public virtual Scope GetScope(string/*!*/ path) {
            return null;
        }

        public ScopeExtension/*!*/ GetScopeExtension(Scope/*!*/ scope) {
            ContractUtils.RequiresNotNull(scope, "scope");
            return scope.GetExtension(ContextId);
        }

        public ScopeExtension/*!*/ EnsureScopeExtension(Scope/*!*/ scope) {
            ContractUtils.RequiresNotNull(scope, "scope");
            ScopeExtension extension = scope.GetExtension(ContextId);
            
            if (extension == null) {
                extension = CreateScopeExtension(scope);
                if (extension == null) {
                    throw new InvalidImplementationException("CreateScopeExtension must return a scope extension.");
                }
                return scope.SetExtension(ContextId, extension);
            }

            return extension;
        }

        /// <summary>
        /// Factory for ModuleContext creation. 
        /// It is guaranteed that this method is called once per each ScriptScope the langauge code is executed within.
        /// </summary>
        public virtual ScopeExtension/*!*/ CreateScopeExtension(Scope scope) {
            return new ScopeExtension(scope);
        }

        // TODO: remove
        /// <summary>
        /// Notification sent when a ScriptCode is about to be executed within given ModuleContext.
        /// </summary>
        public virtual void ModuleContextEntering(ScopeExtension newContext) {
            // nop
        }

        // TODO: remove
        public virtual void PublishModule(string/*!*/ name, Scope/*!*/ scope) {
            // nop
        }

        #endregion

        #region Source Code Parsing & Compilation

        /// <summary>
        /// Updates code properties of the specified source unit. 
        /// The default implementation invokes code parsing. 
        /// </summary>
        public virtual void UpdateSourceCodeProperties(CompilerContext/*!*/ context) {
            ContractUtils.RequiresNotNull(context, "context");

            LambdaExpression lambda = ParseSourceCode(context);

            if (!context.SourceUnit.CodeProperties.HasValue) {
                context.SourceUnit.CodeProperties = (lambda != null) ? SourceCodeProperties.None : SourceCodeProperties.IsInvalid;
            }
        }

        public virtual StreamReader/*!*/ GetSourceReader(Stream/*!*/ stream, Encoding defaultEncoding) {
            return new StreamReader(stream, defaultEncoding);
        }

        #endregion


        /// <summary>
        /// Creates the language specific CompilerContext object for code compilation.  The 
        /// language should flow any relevant options from the LanguageContext to the 
        /// newly created CompilerContext.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // TODO: fix
        public virtual CompilerOptions GetCompilerOptions() {
            return new CompilerOptions();
        }

        /// <summary>
        /// Creates compiler options initialized by the options associated with the module.
        /// </summary>
        public virtual CompilerOptions/*!*/ GetCompilerOptions(Scope/*!*/ scope) {
            return GetCompilerOptions();
        }

        /// <summary>
        /// Looks up the name in the provided Scope using the current language's semantics.
        /// </summary>
        public virtual bool TryLookupName(CodeContext context, SymbolId name, out object value) {
            if (context.Scope.TryLookupName(this, name, out value)) {
                return true;
            }

            return TryLookupGlobal(context, name, out value);
        }

        /// <summary>
        /// Looks up the name in the provided scope using the current language's semantics.
        /// 
        /// If the name cannot be found throws the language appropriate exception or returns
        /// the language's appropriate default value.
        /// </summary>
        public virtual object LookupName(CodeContext context, SymbolId name) {
            object value;
            if (!TryLookupName(context, name, out value) || value == Uninitialized.Instance) {
                throw MissingName(name);
            }

            return value;
        }

        /// <summary>
        /// Attempts to set the name in the provided scope using the current language's semantics.
        /// </summary>
        public virtual void SetName(CodeContext context, SymbolId name, object value) {
            context.Scope.SetName(name, value);
        }

        /// <summary>
        /// Attempts to remove the name from the provided scope using the current language's semantics.
        /// </summary>
        public virtual bool RemoveName(CodeContext context, SymbolId name) {
            return context.Scope.RemoveName(this, name);
        }

        /// <summary>
        /// Attemps to lookup a global variable using the language's semantics called from
        /// the provided Scope.  The default implementation will attempt to lookup the variable
        /// at the host level.
        /// </summary>
        public virtual bool TryLookupGlobal(CodeContext context, SymbolId name, out object value) {
            value = null;
            return false;
        }

        /// <summary>
        /// Called when a lookup has failed and an exception should be thrown.  Enables the 
        /// language context to throw the appropriate exception for their language when
        /// name lookup fails.
        /// </summary>
        protected internal virtual Exception MissingName(SymbolId name) {
            return new MissingMemberException(ResourceUtils.GetString(ResourceUtils.NameNotDefined, SymbolTable.IdToString(name)));
        }

        /// <summary>
        /// Returns a ModuleGlobalCache for the given name.  
        /// 
        /// This cache enables fast access to global values when a SymbolId is not defined after searching the Scope's.  Usually
        /// a language implements lookup of the global value via TryLookupGlobal.  When GetModuleCache returns a ModuleGlobalCache
        /// a cached value can be used instead of calling TryLookupGlobal avoiding a possibly more expensive lookup from the 
        /// LanguageContext.  The ModuleGlobalCache can be held onto and have its value updated when the cache is invalidated.
        /// 
        /// By default this returns a cache which indicates no caching should occur and the LanguageContext will be 
        /// consulted when a module value is not available. If a LanguageContext only caches some values it can return 
        /// the value from the base method when the value should not be cached.
        /// </summary>
        protected internal virtual ModuleGlobalCache GetModuleCache(SymbolId name) {
            return _noCache;
        }

        #region ICloneable Members

        public virtual object/*!*/ Clone() {
            return MemberwiseClone();
        }

        #endregion

        /// <summary>
        /// Calls the function with given arguments
        /// </summary>
        /// <param name="context"></param>
        /// <param name="function">The function to call</param>
        /// <param name="args">The argumetns with which to call the function.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Call")] // TODO: fix
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Function")] // TODO: fix
        public virtual object Call(CodeContext context, object function, object[] args) {
            return null;
        }

        /// <summary>
        /// Calls the function with instance as the "this" value.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="function">The function to call</param>
        /// <param name="instance">The instance to pass as "this".</param>
        /// <param name="args">The rest of the arguments.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Function")] // TODO: fix
        public virtual object CallWithThis(CodeContext context, object function, object instance, object[] args) {
            return null;
        }

        /// <summary>
        /// Calls the function with arguments, extra arguments in tuple and dictionary of keyword arguments
        /// </summary>
        /// <param name="context"></param>
        /// <param name="func">The function to call</param>
        /// <param name="args">The arguments</param>
        /// <param name="names">Argument names</param>
        /// <param name="argsTuple">tuple of extra arguments</param>
        /// <param name="kwDict">keyword dictionary</param>
        /// <returns>The result of the function call.</returns>
        public virtual object CallWithArgsKeywordsTupleDict(CodeContext context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
            return null;
        }

        /// <summary>
        /// Calls function with arguments and additional arguments in the tuple
        /// </summary>
        /// <param name="context"></param>
        /// <param name="func">The function to call</param>
        /// <param name="args">Argument array</param>
        /// <param name="argsTuple">Tuple with extra arguments</param>
        /// <returns>The result of calling the function "func"</returns>
        public virtual object CallWithArgsTuple(CodeContext context, object func, object[] args, object argsTuple) {
            return null;
        }

        /// <summary>
        /// Calls the function with arguments, some of which are keyword arguments.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="func">Function to call</param>
        /// <param name="args">Argument array</param>
        /// <param name="names">Names for some of the arguments</param>
        /// <returns>The result of calling the function "func"</returns>
        public virtual object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names) {
            return null;
        }

        // used only by ReflectedEvent.HandlerList
        public virtual bool EqualReturnBool(CodeContext context, object x, object y) {
            return false;
        }

        // used by DynamicHelpers.GetDelegate
        /// <summary>
        /// Checks whether the target is callable with given number of arguments.
        /// </summary>
        public void CheckCallable(object target, int argumentCount) {
            int min, max;
            if (!IsCallable(target, argumentCount, out min, out max)) {
                if (min == max) {
                    throw RuntimeHelpers.SimpleTypeError(String.Format("expected compatible function, but got parameter count mismatch (expected {0} args, target takes {1})", argumentCount, min));
                } else {
                    throw RuntimeHelpers.SimpleTypeError(String.Format("expected compatible function, but got parameter count mismatch (expected {0} args, target takes at least {1} and at most {2})", argumentCount, min, max));
                }
            }
        }

        public virtual bool IsCallable(object target, int argumentCount, out int min, out int max) {
            min = max = 0;
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        public virtual Assembly LoadAssemblyFromFile(string file) {
#if SILVERLIGHT
            return null;
#else
            return Assembly.LoadFile(file);
#endif
        }

        #region ScriptEngine API

        /// <summary>
        /// Parses the source code within a specified compiler context. 
        /// The source unit to parse is held on by the context.
        /// </summary>
        /// <param name="context">Compiler context.</param>
        /// <returns><b>null</b> on failure.</returns>
        /// <remarks>Could also set the code properties and line/file mappings on the source unit.</remarks>
        public abstract LambdaExpression ParseSourceCode(CompilerContext context);

        public virtual string/*!*/ DisplayName {
            get {
                return "unknown";
            }
        }

        public virtual Version LanguageVersion {
            get {
                return new Version(0, 0);
            }
        }

        public virtual void SetScriptSourceSearchPaths(string[] paths) {
        }

#if !SILVERLIGHT
        // Convert a CodeDom to source code, and output the generated code and the line number mappings (if any)
        public virtual SourceUnit/*!*/ GenerateSourceCode(System.CodeDom.CodeObject codeDom, string path, SourceCodeKind kind) {
            throw new NotImplementedException();
        }
#endif

        public virtual TService GetService<TService>(params object[] args) where TService : class {
            return null;
        }

        //TODO these three properties should become abstract and updated for all implementations
        public virtual Guid LanguageGuid {
            get {
                return Guid.Empty;
            }
        }

        public virtual Guid VendorGuid {
            get {
                return Guid.Empty;
            }
        }

        public virtual void Shutdown() {
        }

        public virtual string/*!*/ FormatException(Exception exception) {
            return exception.ToString();
        }

        public virtual EngineOptions/*!*/ Options {
            get {
                return new EngineOptions();
            }
        }

        #region Source Units

        public SourceUnit CreateSnippet(string/*!*/ code) {
            return CreateSnippet(code, null, SourceCodeKind.Statements);
        }

        public SourceUnit CreateSnippet(string/*!*/ code, SourceCodeKind kind) {
            return CreateSnippet(code, null, kind);
        }

        public SourceUnit CreateSnippet(string/*!*/ code, string id) {
            return CreateSnippet(code, id, SourceCodeKind.Statements);
        }

        public SourceUnit CreateSnippet(string/*!*/ code, string id, SourceCodeKind kind) {
            ContractUtils.RequiresNotNull(code, "code");

            return CreateSourceUnit(new SourceStringContentProvider(code), id, kind);
        }

        public SourceUnit CreateFileUnit(string/*!*/ path) {
            return CreateFileUnit(path, StringUtils.DefaultEncoding);
        }

        public SourceUnit CreateFileUnit(string/*!*/ path, Encoding/*!*/ encoding) {
            return CreateFileUnit(path, encoding, SourceCodeKind.File);
        }

        public SourceUnit CreateFileUnit(string/*!*/ path, Encoding/*!*/ encoding, SourceCodeKind kind) {
            ContractUtils.RequiresNotNull(path, "path");
            ContractUtils.RequiresNotNull(encoding, "encoding");
            
            // TODO: remove hosting reference!!!
            TextContentProvider provider = new Hosting_EngineTextContentProvider(this, new FileStreamContentProvider(DomainManager.Platform, path), encoding);
            return CreateSourceUnit(provider, path, kind);
        }

        public SourceUnit CreateFileUnit(string/*!*/ path, string/*!*/ content) {
            ContractUtils.RequiresNotNull(path, "path");
            ContractUtils.RequiresNotNull(content, "content");

            TextContentProvider provider = new SourceStringContentProvider(content);
            return CreateSourceUnit(provider, path, SourceCodeKind.File);
        }

        public SourceUnit/*!*/ CreateSourceUnit(TextContentProvider/*!*/ contentProvider, string path, SourceCodeKind kind) {
            ContractUtils.RequiresNotNull(contentProvider, "contentProvider");
            ContractUtils.Requires(path == null || path.Length > 0, "path", "Empty string is not a valid path.");
            ContractUtils.Requires(EnumBounds.IsValid(kind), "kind");
            ContractUtils.Requires(CanCreateSourceCode);

            return new SourceUnit(this, contentProvider, path, kind);
        }

        #endregion

        #endregion

        private static T GetArg<T>(object[] arg, int index, bool optional) {
            if (!optional && index >= arg.Length) {
                throw new ArgumentException("Invalid number of parameters for the service");
            }

            if (!(arg[index] is T)) {
                throw new ArgumentException(
                    String.Format("Invalid argument type; expecting {0}", typeof(T)),
                    String.Format("arg[{0}]", index));
            }

            return (T)arg[index];
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual ErrorSink/*!*/ GetCompilerErrorSink() {
            return ErrorSink.Null;
        }

        public virtual void GetExceptionMessage(Exception exception, out string message, out string errorTypeName) {
            message = exception.Message;
            errorTypeName = exception.GetType().Name;
        }

        internal protected virtual int ExecuteProgram(SourceUnit/*!*/ program) {
            ContractUtils.RequiresNotNull(program, "program");

            ScriptCode compiledCode = program.Compile();
            object returnValue = compiledCode.Run(compiledCode.MakeOptimizedScope());

            CodeContext context = new CodeContext(new Scope(), this);

            CallSite<DynamicSiteTarget<object, object>> site =
                CallSite<DynamicSiteTarget<object, object>>.Create(ConvertToAction.Make(Binder, typeof(int), ConversionResultKind.ExplicitTry));

            object exitCode = site.Target(site, context, returnValue);
            return (exitCode != null) ? (int)exitCode : 0;
        }
    }
}
