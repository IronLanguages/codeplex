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

using ScriptSource = Microsoft.Scripting.Hosting.SourceUnit;
using ScriptSourceKind = Microsoft.Scripting.Hosting.SourceCodeKind;
using System.Text;
using System.CodeDom;
using Microsoft.Scripting.Shell;
using System.Diagnostics;

namespace Microsoft.Scripting.Hosting {
    public interface IScriptEngine : IRemotable {
        EngineOptions Options { get; }

        void SetScriptSourceSearchPaths(string[] paths);

        int ExecuteProgram(ScriptSource scriptSource);

        void DumpDebugInfo();

        string FormatException(Exception exception);

        object Execute(IScriptScope scope, ScriptSource scriptSource);
        T Execute<T>(IScriptScope scope, ScriptSource scriptSource);

        ObjectOperations Operations {
            get;
        }

        Version LanguageVersion {
            get;
        }

        IScriptEnvironment Runtime {
            get;
        }

        void Shutdown();
        TextWriter GetOutputWriter(bool isErrorOutput);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        ErrorSink GetCompilerErrorSink();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        CompilerOptions GetDefaultCompilerOptions();

        IScriptScope GetScope(string/*!*/ path);
        IScriptScope/*!*/ CreateScope();
        IScriptScope/*!*/ CreateScope(IAttributesCollection/*!*/ dictionary);

        // TODO: remove
        void PublishModule(string/*!*/ name, IScriptScope/*!*/ scope);

        ScriptSource CreateScriptSourceFromString(string code);
        ScriptSource CreateScriptSourceFromString(string code, ScriptSourceKind kind);
        ScriptSource CreateScriptSourceFromString(string code, string id, ScriptSourceKind kind);
        ScriptSource CreateScriptSourceFromFile(string path);
        ScriptSource CreateScriptSourceFromFile(string path, Encoding encoding);
#if !SILVERLIGHT
        ScriptSource CreateScriptSource(CodeObject/*!*/ content);
#endif
        ScriptSource CreateScriptSource(StreamContentProvider/*!*/ contentProvider, string id, Encoding encoding, ScriptSourceKind kind);
        ScriptSource CreateScriptSource(TextContentProvider contentProvider, string id, ScriptSourceKind kind);

        ICompiledCode Compile(ScriptSource scriptSource);
        ICompiledCode Compile(ScriptSource scriptSource, ErrorSink sink);
        ICompiledCode Compile(ScriptSource scriptSource, CompilerOptions options, ErrorSink sink);
        bool TryGetVariable(IScriptScope scope, string name, out object value);
        object GetVariable(IScriptScope/*!*/ scope, string/*!*/ name);
        T GetVariable<T>(IScriptScope/*!*/ scope, string/*!*/ name);

#if !SILVERLIGHT
        bool TryGetVariableAsHandle(IScriptScope scope, string name, out ObjectHandle value);
        ObjectHandle ExecuteAndGetAsHandle(IScriptScope scope, ScriptSource scriptSource);
#endif
        string LanguageDisplayName {
            get;
        }

        ServiceType GetService<ServiceType>(params object[] args) where ServiceType : class;

        string[] GetRegisteredIdentifiers();
        string[] GetRegisteredExtensions();

        ObjectOperations CreateOperations();
    }

#if !SILVERLIGHT
    internal sealed class RemoteScriptEngine : RemoteWrapper, IScriptEngine {
        private readonly ScriptEngine/*!*/ _engine;

        public RemoteScriptEngine(ScriptEngine/*!*/ engine) {
            Debug.Assert(engine != null);

            _engine = engine;
        }

        public EngineOptions Options {
            get {
                return _engine.Options;
            }
        }

        public override ILocalObject LocalObject {
            get { return _engine; }
        }

        public IScriptEnvironment Runtime {
            get {
                return RemoteWrapper.WrapRemotable<IScriptEnvironment>(_engine.Runtime);
            }
        }

        public void SetScriptSourceSearchPaths(string[] paths) {
            _engine.SetScriptSourceSearchPaths(paths);
        }

        public int ExecuteProgram(ScriptSource scriptSource) {
            return _engine.ExecuteProgram(scriptSource);
        }

        public void DumpDebugInfo() {
            throw new NotSupportedException();
        }

        public string FormatException(Exception exception) {
            return _engine.FormatException(exception);
        }

        public object Execute(IScriptScope scope, ScriptSource scriptSource) {
            return _engine.Execute(scope, scriptSource);
        }

        public T Execute<T>(IScriptScope scope, ScriptSource scriptSource) {
            return _engine.Execute<T>(scope, scriptSource);
        }

        public Version LanguageVersion {
            get { return _engine.LanguageVersion; }
        }

        public ObjectOperations Operations {
            get {
                return _engine.Operations;
            }
        }

        public void Shutdown() {
            _engine.Shutdown();
        }

        public IScriptScope GetScope(string/*!*/ path) {
            return RemoteWrapper.WrapRemotable<IScriptScope>(_engine.GetScope(path));
        }

        public IScriptScope/*!*/ CreateScope() {
            return RemoteWrapper.WrapRemotable<IScriptScope>(_engine.CreateScope());
        }

        public IScriptScope/*!*/ CreateScope(IAttributesCollection/*!*/ dictionary) {
            return RemoteWrapper.WrapRemotable<IScriptScope>(_engine.CreateScope(dictionary));
        }

        // TODO: remove
        public void PublishModule(string/*!*/ name, IScriptScope/*!*/ scope) {
            _engine.PublishModule(name, scope);
        }
        
        public ScriptSource CreateScriptSourceFromString(string code, ScriptSourceKind kind) {
            return _engine.CreateScriptSourceFromString(code, kind);
        }

        public ScriptSource CreateScriptSourceFromString(string code, string id, ScriptSourceKind kind) {
            return _engine.CreateScriptSourceFromString(code, id, kind);
        }

        public ScriptSource CreateScriptSourceFromString(string code) {
            return _engine.CreateScriptSourceFromString(code);
        }

        public ScriptSource CreateScriptSourceFromFile(string path) {
            return _engine.CreateScriptSourceFromFile(path);
        }

        public ScriptSource CreateScriptSourceFromFile(string path, Encoding encoding) {
            return _engine.CreateScriptSourceFromFile(path, encoding);
        }

#if !SILVERLIGHT
        public ScriptSource CreateScriptSource(CodeObject/*!*/ content) {
            return _engine.CreateScriptSource(content);
        }
#endif

        public ScriptSource CreateScriptSource(StreamContentProvider contentProvider, string id, Encoding encoding, ScriptSourceKind kind) {
            return _engine.CreateScriptSource(contentProvider, id, encoding, kind);
        }

        public ScriptSource CreateScriptSource(TextContentProvider contentProvider, string id, ScriptSourceKind kind) {
            return _engine.CreateScriptSource(contentProvider, id, kind);
        }

        public TextWriter GetOutputWriter(bool isErrorOutput) {
            return _engine.GetOutputWriter(isErrorOutput);
        }

        public ErrorSink GetCompilerErrorSink() {
            return _engine.GetCompilerErrorSink();
        }

        public CompilerOptions GetDefaultCompilerOptions() {
            return _engine.GetDefaultCompilerOptions();
        }

        Type IRemotable.GetType() {
            return typeof(ScriptEngine);
        }

        public string LanguageDisplayName {
            get {
                return _engine.LanguageDisplayName;
            }
        }

        public ICompiledCode Compile(ScriptSource scriptSource) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.Compile(scriptSource));
        }

        public ICompiledCode Compile(ScriptSource scriptSource, ErrorSink errorSink) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.Compile(scriptSource, errorSink));
        }

        public ICompiledCode Compile(ScriptSource scriptSource, CompilerOptions options, ErrorSink errorSink) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.Compile(scriptSource, options, errorSink));
        }

        public ServiceType GetService<ServiceType>(params object[] args) where ServiceType : class {
            ServiceType res = _engine.GetService<ServiceType>(args);
            if (res != null) {
                if (typeof(ServiceType) == typeof(ITokenCategorizer)) {
                    return (ServiceType)(object)new RemoteTokenCategorizer((TokenCategorizer)(object)res);
                }
            }
            return res;
        }

        public string[] GetRegisteredIdentifiers() {
            return _engine.GetRegisteredIdentifiers();
        }

        public string[] GetRegisteredExtensions() {
            return _engine.GetRegisteredExtensions();
        }

        public ObjectHandle ExecuteAndGetAsHandle(IScriptScope scope, ScriptSource scriptSource) {
            return _engine.ExecuteAndGetAsHandle(scope, scriptSource);
        }

        public bool TryGetVariableAsHandle(IScriptScope scope, string name, out ObjectHandle value) {
            object o;
            if (_engine.TryGetVariable(scope, name, out o)) {
                value = new ObjectHandle(o);
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGetVariable(IScriptScope scope, string name, out object value) {
            return _engine.TryGetVariable(scope, name, out value);
        }

        public object GetVariable(IScriptScope/*!*/ scope, string/*!*/ name) {
            return _engine.GetVariable(scope, name);
        }

        public T GetVariable<T>(IScriptScope/*!*/ scope, string/*!*/ name) {
            return _engine.GetVariable<T>(scope, name);
        }

        public ObjectOperations CreateOperations() {
            return _engine.CreateOperations();
        }
    }
#endif

    public class ScriptEngine : IScriptEngine, ILocalObject {
        private readonly LanguageContext/*!*/ _language;
        private readonly ScriptEnvironment/*!*/ _runtime;
        private ObjectOperations _operations;

        internal ScriptEngine(ScriptEnvironment/*!*/ runtime, LanguageContext/*!*/ context) {
            Debug.Assert(runtime != null);
            Debug.Assert(context != null);

            _runtime = runtime;
            _language = context;
        }

        #region Compilation / Execution

        /// <summary>
        /// Execute the code in the specified ScriptScope and return a result.
        /// 
        /// Execute returns an object that is the resulting value of running the code.  
        /// 
        /// When the ScriptSource is a file or statement, the engine decides what is 
        /// an appropriate value to return.  Some languages return the value produced 
        /// by the last expression or statement, but languages that are not expression 
        /// based may return null.
        /// </summary>
        public object Execute(IScriptScope/*!*/ scope, ScriptSource/*!*/ scriptSource) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(scriptSource, "scriptSource");

            Scope localScope = RemoteWrapper.GetLocalArgument<ScriptScope>(scope, "scope").Scope;
            ScriptCode compiledCode = _language.CompileSourceCode(scriptSource, _language.GetModuleCompilerOptions(localScope), null);
            return compiledCode.Run(localScope);
        }

        /// <summary>
        /// Execute the code in the specified ScriptScope and return a result.
        /// 
        /// Execute returns an object that is the resulting value of running the code.  
        /// 
        /// When the ScriptSource is a file or statement, the engine decides what is 
        /// an appropriate value to return.  Some languages return the value produced 
        /// by the last expression or statement, but languages that are not expression 
        /// based may return null.
        /// 
        /// The result is execution is converted to the specified type, using the engine's 
        /// Operations.ConvertTo of T method.  If this method cannot convert to the 
        /// specified type, then it throws an exception.
        /// </summary>
        public T Execute<T>(IScriptScope/*!*/ scope, ScriptSource/*!*/ scriptSource) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(scriptSource, "scriptSource");

            return Operations.ConvertTo<T>(Execute(scope, scriptSource));
        }

        /// <summary>
        /// Execute the code in the specified ScriptScope and return a result.
        /// 
        /// Execute returns an object that is the resulting value of running the code.  
        /// 
        /// When the ScriptSource is a file or statement, the engine decides what is 
        /// an appropriate value to return.  Some languages return the value produced 
        /// by the last expression or statement, but languages that are not expression 
        /// based may return null.
        /// 
        /// ExecuteProgram runs the source as though it were launched from an OS command 
        /// shell and returns a process exit code indicating the success or error condition 
        /// of executing the code.  Each time this method is called it create a fresh ScriptScope 
        /// in which to run the source, and if you were to use GetScope, you'd get whatever 
        /// last ScriptScope the engine created for the source.
        /// </summary>
        public int ExecuteProgram(ScriptSource/*!*/ scriptSource) {
            Contract.RequiresNotNull(scriptSource, "scriptSource");

            // TODO: not correct, should go to language context
            object obj = _language.CompileSourceCode(scriptSource, null, null).Run(new Scope(_language));

            int res;
            if (!Operations.TryConvertTo<int>(obj, out res)) {
                res = 0;
            }
            return res;
        }

        /// <summary>
        /// Compile the provided ScriptSource and returns a CompileCode object that can be executed 
        /// repeatedly in its default scope or in other scopes without having to recompile the code.
        /// </summary>
        public virtual ICompiledCode/*!*/ Compile(ScriptSource/*!*/ scriptSource) {
            Contract.RequiresNotNull(scriptSource, "scriptSource");

            return new CompiledCode(this, _language.CompileSourceCode(scriptSource, null, null));
        }

        /// <summary>
        /// Compile the provided ScriptSource and returns a CompileCode object that can be executed 
        /// repeatedly in its default scope or in other scopes without having to recompile the code.
        /// </summary>
        public virtual ICompiledCode/*!*/ Compile(ScriptSource/*!*/ scriptSource, ErrorSink/*!*/ errorSink) {
            Contract.RequiresNotNull(scriptSource, "scriptSource");
            Contract.RequiresNotNull(errorSink, "errorSink");

            return new CompiledCode(this, _language.CompileSourceCode(scriptSource, null, errorSink));
        }

        /// <summary>
        /// Compile the provided ScriptSource and returns a CompileCode object that can be executed 
        /// repeatedly in its default scope or in other scopes without having to recompile the code.
        /// </summary>
        public ICompiledCode/*!*/ Compile(ScriptSource/*!*/ scriptSource, CompilerOptions/*!*/ compilerOptions) {
            Contract.RequiresNotNull(scriptSource, "scriptSource");
            Contract.RequiresNotNull(compilerOptions, "compilerOptions");

            return new CompiledCode(this, _language.CompileSourceCode(scriptSource, compilerOptions, null));
        }

        /// <summary>
        /// Compile the provided ScriptSource and returns a CompileCode object that can be executed 
        /// repeatedly in its default scope or in other scopes without having to recompile the code.
        /// </summary>
        public ICompiledCode/*!*/ Compile(ScriptSource/*!*/ scriptSource, CompilerOptions/*!*/ compilerOptions, ErrorSink errorSink) {
            Contract.RequiresNotNull(scriptSource, "scriptSource");
            Contract.RequiresNotNull(errorSink, "errorSink");
            Contract.RequiresNotNull(compilerOptions, "compilerOptions");

            return new CompiledCode(this, _language.CompileSourceCode(scriptSource, compilerOptions, errorSink));
        }

        #endregion

        #region Object Operations

        /// <summary>
        /// Returns a default ObjectOperations for the engine.  
        /// 
        /// Because an ObjectOperations object caches rules for the types of 
        /// objects and operations it processes, using the default ObjectOperations for 
        /// many objects could degrade the caching benefits.  Eventually the cache for 
        /// some operations could degrade to a point where ObjectOperations stops caching and 
        /// does a full search for an implementation of the requested operation for the given objects.  
        /// 
        /// Another reason to create a new ObjectOperations instance is to have it bound
        /// to the specific view of a ScriptScope.  Languages may attach per-language
        /// behavior to a ScriptScope which would alter how the operations are performed.
        /// 
        /// For simple hosting situations, this is sufficient behavior.
        /// 
        /// 
        /// </summary>
        public ObjectOperations/*!*/ Operations {
            get {
                if (_operations == null) {
                    Interlocked.CompareExchange(ref _operations, CreateOperations(), null);
                }

                return _operations;
            }
        }

        /// <summary>
        /// Returns a new ObjectOperations object.  See the Operations property for why you might want to call this.
        /// </summary>
        public ObjectOperations/*!*/ CreateOperations() {
            return new ObjectOperations(GetCodeContext(ScriptDomainManager.CurrentManager.Host.DefaultScope));
        }

        /// <summary>
        /// Returns a new ObjectOperations object that inherits any semantics particular to the provided ScriptScope.  
        /// 
        /// See the Operations property for why you might want to call this.
        /// </summary>
        public ObjectOperations/*!*/ CreateOperations(IScriptScope/*!*/ scope) {
            Contract.RequiresNotNull(scope, "scope");

            return new ObjectOperations(GetCodeContext(scope));
        }

        #endregion

        #region Scopes

        public IScriptScope/*!*/ CreateScope() {
            return new ScriptScope(this, new Scope(_language));
        }

        public IScriptScope/*!*/ CreateScope(IAttributesCollection/*!*/ dictionary) {
            Contract.RequiresNotNull(dictionary, "dictionary");
            return new ScriptScope(this, new Scope(_language, dictionary));
        }

        /// <summary>
        /// This method returns the ScriptScope in which a ScriptSource of given path was executed.  
        /// 
        /// The ScriptSource.Path property is the key to finding the ScriptScope.  Hosts need 
        /// to make sure they create a ScriptSource and set its Path property appropriately.
        /// 
        /// GetScope is primarily useful for tools that need to map files to their execution scopes. For example, 
        /// an editor and interpreter tool might run a file Foo that imports or requires a file Bar.  
        /// 
        /// The editor's user might later open the file Bar and want to execute expressions in its context.  
        /// The tool would need to find Bar's ScriptScope for setting the appropriate context in its interpreter window. 
        /// This method helps with this scenario.
        /// </summary>
        public IScriptScope GetScope(string/*!*/ path) {
            Contract.RequiresNotNull(path, "path");
            Scope scope = _language.GetScope(path);
            return (scope != null) ? new ScriptScope(this, scope) : null;
        }

        // TODO: remove
        public void PublishModule(string/*!*/ name, IScriptScope/*!*/ scope) {
            _language.PublishModule(name, RemoteWrapper.GetLocalArgument<ScriptScope>(scope, "scope").Scope);
        }

        #endregion

        #region Source Unit Creation

        /// <summary>
        /// Return a ScriptSource object from string contents with the current engine as the language binding.
        /// 
        /// The default ScriptSourceKind is Expression.
        /// 
        /// The ScriptSource's Path property defaults to <c>null</c>.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromString(string/*!*/ code) {
            Contract.RequiresNotNull(code, "code");

            return CreateScriptSource(new SourceStringContentProvider(code), null, SourceCodeKind.Expression);
        }

        /// <summary>
        /// Return a ScriptSource object from string contents with the current engine as the language binding.
        /// 
        /// The ScriptSource's Path property defaults to <c>null</c>.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromString(string/*!*/ code, ScriptSourceKind kind) {
            Contract.RequiresNotNull(code, "code");
            Contract.Requires(Enum.IsDefined(typeof(ScriptSourceKind), kind), "kind");

            return CreateScriptSource(new SourceStringContentProvider(code), null, kind);
        }

        /// <summary>
        /// Return a ScriptSource object from string contents with the current engine as the language binding.
        /// 
        /// The default ScriptSourceKind is Expression.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromString(string/*!*/ code, string id) {
            Contract.RequiresNotNull(code, "code");

            return CreateScriptSource(new SourceStringContentProvider(code), id, SourceCodeKind.Expression);
        }

        /// <summary>
        /// Return a ScriptSource object from string contents.  These are helpers for creating ScriptSources' with the right language binding.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromString(string/*!*/ code, string id, ScriptSourceKind kind) {
            Contract.RequiresNotNull(code, "code");
            Contract.Requires(Enum.IsDefined(typeof(ScriptSourceKind), kind), "kind");

            return CreateScriptSource(new SourceStringContentProvider(code), id, kind);
        }

        /// <summary>
        /// Return a ScriptSource object from file contents with the current engine as the language binding.  
        /// 
        /// The path's extension does NOT have to be in ScriptRuntime.GetRegisteredFileExtensions 
        /// or map to this language engine with ScriptRuntime.GetEngineByFileExtension.
        /// 
        /// The default ScriptSourceKind is File.
        /// 
        /// The ScriptSource's Path property will be the path argument.
        /// 
        /// The encoding defaults to System.Text.Encoding.Default.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromFile(string/*!*/ path) {
            Contract.RequiresNotNull(path, "path");

            return _runtime.Host.TryGetSourceFileUnit(this, path, StringUtils.DefaultEncoding, SourceCodeKind.File);
        }

        /// <summary>
        /// Return a ScriptSource object from file contents with the current engine as the language binding.  
        /// 
        /// The path's extension does NOT have to be in ScriptRuntime.GetRegisteredFileExtensions 
        /// or map to this language engine with ScriptRuntime.GetEngineByFileExtension.
        /// 
        /// The default ScriptSourceKind is File.
        /// 
        /// The ScriptSource's Path property will be the path argument.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromFile(string/*!*/ path, Encoding/*!*/ encoding) {
            Contract.RequiresNotNull(path, "path");
            Contract.RequiresNotNull(encoding, "encoding");

            return _runtime.Host.TryGetSourceFileUnit(this, path, encoding, SourceCodeKind.File);
        }

        /// <summary>
        /// Return a ScriptSource object from file contents with the current engine as the language binding.  
        /// 
        /// The path's extension does NOT have to be in ScriptRuntime.GetRegisteredFileExtensions 
        /// or map to this language engine with ScriptRuntime.GetEngineByFileExtension.
        /// 
        /// The ScriptSource's Path property will be the path argument.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromFile(string/*!*/ path, Encoding/*!*/ encoding, ScriptSourceKind kind) {
            Contract.RequiresNotNull(path, "path");
            Contract.RequiresNotNull(encoding, "encoding");
            Contract.Requires(Enum.IsDefined(typeof(ScriptSourceKind), kind), "kind");

            ScriptSource res = _runtime.Host.TryGetSourceFileUnit(this, path, encoding, kind);
            if (res == null) {
#if SILVERLIGHT
                throw new FileNotFoundException();
#else
                throw new FileNotFoundException("Host failed to resolve file", path);
#endif
            }

            return res;
        }

#if !SILVERLIGHT
        /// <summary>
        /// This method returns a ScriptSource object from a System.CodeDom.CodeObject.  
        /// This is a factory method for creating a ScriptSources with this language binding.
        /// 
        /// The expected CodeDom support is extremely minimal for syntax-independent expression of semantics.  
        /// 
        /// Languages may do more, but hosts should only expect CodeMemberMethod support, 
        /// and only sub nodes consisting of the following:
        ///     CodeSnippetStatement
        ///     CodeSnippetExpression
        ///     CodePrimitiveExpression
        ///     CodeMethodInvokeExpression
        ///     CodeExpressionStatement (for holding MethodInvoke)
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSource(CodeObject/*!*/ content) {
            Contract.RequiresNotNull(content, "content");

            return _language.GenerateSourceCode(content, null, SourceCodeKind.File);
        }

        /// <summary>
        /// This method returns a ScriptSource object from a System.CodeDom.CodeObject.  
        /// This is a factory method for creating a ScriptSources with this language binding.
        /// 
        /// The expected CodeDom support is extremely minimal for syntax-independent expression of semantics.  
        /// 
        /// Languages may do more, but hosts should only expect CodeMemberMethod support, 
        /// and only sub nodes consisting of the following:
        ///     CodeSnippetStatement
        ///     CodeSnippetExpression
        ///     CodePrimitiveExpression
        ///     CodeMethodInvokeExpression
        ///     CodeExpressionStatement (for holding MethodInvoke)
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSource(CodeObject/*!*/ content, string path) {
            Contract.RequiresNotNull(content, "content");

            return _language.GenerateSourceCode(content, path, SourceCodeKind.File);
        }

        /// <summary>
        /// This method returns a ScriptSource object from a System.CodeDom.CodeObject.  
        /// This is a factory method for creating a ScriptSources with this language binding.
        /// 
        /// The expected CodeDom support is extremely minimal for syntax-independent expression of semantics.  
        /// 
        /// Languages may do more, but hosts should only expect CodeMemberMethod support, 
        /// and only sub nodes consisting of the following:
        ///     CodeSnippetStatement
        ///     CodeSnippetExpression
        ///     CodePrimitiveExpression
        ///     CodeMethodInvokeExpression
        ///     CodeExpressionStatement (for holding MethodInvoke)
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSource(CodeObject/*!*/ content, SourceCodeKind kind) {
            Contract.RequiresNotNull(content, "content");

            return _language.GenerateSourceCode(content, null, kind);
        }

        /// <summary>
        /// This method returns a ScriptSource object from a System.CodeDom.CodeObject.  
        /// This is a factory method for creating a ScriptSources with this language binding.
        /// 
        /// The expected CodeDom support is extremely minimal for syntax-independent expression of semantics.  
        /// 
        /// Languages may do more, but hosts should only expect CodeMemberMethod support, 
        /// and only sub nodes consisting of the following:
        ///     CodeSnippetStatement
        ///     CodeSnippetExpression
        ///     CodePrimitiveExpression
        ///     CodeMethodInvokeExpression
        ///     CodeExpressionStatement (for holding MethodInvoke)
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSource(CodeObject/*!*/ content, string id, SourceCodeKind kind) {
            Contract.RequiresNotNull(content, "content");

            return _language.GenerateSourceCode(content, id, kind);
        }
#endif

        /// <summary>
        /// These methods return ScriptSource objects from stream contents with the current engine as the language binding.  
        /// 
        /// The default ScriptSourceKind is File.
        /// 
        /// The encoding defaults to Encoding.Default.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSource(StreamContentProvider/*!*/ content, string id) {
            Contract.RequiresNotNull(content, "content");

            return CreateScriptSource(content, id, StringUtils.DefaultEncoding, SourceCodeKind.File);
        }

        /// <summary>
        /// These methods return ScriptSource objects from stream contents with the current engine as the language binding.  
        /// 
        /// The default ScriptSourceKind is File.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSource(StreamContentProvider/*!*/ content, string id, Encoding/*!*/ encoding) {
            Contract.RequiresNotNull(content, "content");
            Contract.RequiresNotNull(encoding, "encoding");

            return CreateScriptSource(content, id, encoding, SourceCodeKind.File);
        }

        /// <summary>
        /// These methods return ScriptSource objects from stream contents with the current engine as the language binding.  
        /// 
        /// The encoding defaults to Encoding.Default.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSource(StreamContentProvider/*!*/ content, string id, Encoding/*!*/ encoding, ScriptSourceKind kind) {
            Contract.RequiresNotNull(content, "content");
            Contract.RequiresNotNull(encoding, "encoding");
            Contract.Requires(Enum.IsDefined(typeof(ScriptSourceKind), kind), "kind");

            return CreateScriptSource(new EngineTextContentProvider(_language, content, encoding), id, kind);
        }

        /// <summary>
        /// This method returns a ScriptSource with the content provider supplied with the current engine as the language binding.
        /// 
        /// This helper lets you own the content provider so that you can implement a stream over internal host data structures, such as an editor's text representation.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSource(TextContentProvider/*!*/ contentProvider, string id, ScriptSourceKind kind) {
            Contract.RequiresNotNull(contentProvider, "contentProvider");
            Contract.Requires(Enum.IsDefined(typeof(ScriptSourceKind), kind), "kind");

            return _language.CreateSourceUnit(contentProvider, id, kind);
        }

        #endregion

        #region Scope Variable Access

        /// <summary>
        /// Fetches the value of a variable stored in the scope.
        /// 
        /// If there is no engine associated with the scope (see ScriptRuntime.CreateScope), then the name lookup is 
        /// a literal lookup of the name in the scope's dictionary.  Therefore, it is case-sensitive for example.  
        /// 
        /// If there is a default engine, then the name lookup uses that language's semantics.
        /// </summary>
        public object GetVariable(IScriptScope/*!*/ scope, string/*!*/ name) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(name, "name");

            return _language.LookupName(GetCodeContext(scope), SymbolTable.StringToId(name));
        }

        /// <summary>
        /// This method removes the variable name and returns whether 
        /// the variable was bound in the scope when you called this method.
        /// 
        /// If there is no engine associated with the scope (see ScriptRuntime.CreateScope), 
        /// then the name lookup is a literal lookup of the name in the scope's dictionary.  Therefore, 
        /// it is case-sensitive for example.  If there is a default engine, then the name lookup uses that language's semantics.
        /// 
        /// Some languages may refuse to remove some variables.  If the scope has a default language that has bound 
        /// variables that cannot be removed, the language engine throws an exception.
        /// </summary>
        public bool RemoveVariable(IScriptScope/*!*/ scope, string/*!*/ name) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(name, "name");

            return _language.RemoveName(GetCodeContext(scope), SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Assigns a value to a variable in the scope, overwriting any previous value.
        /// 
        /// If there is no engine associated with the scope (see ScriptRuntime.CreateScope), 
        /// then the name lookup is a literal lookup of the name in the scope's dictionary.  Therefore, 
        /// it is case-sensitive for example.  
        /// 
        /// If there is a default engine, then the name lookup uses that language's semantics.
        /// </summary>
        public void SetVariable(IScriptScope/*!*/ scope, string/*!*/ name, object value) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(name, "name");

            _language.SetName(GetCodeContext(scope), SymbolTable.StringToId(name), value);
        }

        /// <summary>
        /// Fetches the value of a variable stored in the scope and returns 
        /// a Boolean indicating success of the lookup.  
        /// 
        /// When the method's result is false, then it assigns null to value.
        /// 
        /// If there is no engine associated with the scope (see ScriptRuntime.CreateScope), 
        /// then the name lookup is a literal lookup of the name in the scope's dictionary.  Therefore, 
        /// it is case-sensitive for example.  
        /// 
        /// If there is a default engine, then the name lookup uses that language's semantics.
        /// </summary>
        public bool TryGetVariable(IScriptScope/*!*/ scope, string/*!*/ name, out object value) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(name, "name");

            return _language.TryLookupName(GetCodeContext(scope), SymbolTable.StringToId(name), out value);
        }

        /// <summary>
        /// Fetches the value of a variable stored in the scope.
        /// 
        /// If there is no engine associated with the scope (see ScriptRuntime.CreateScope), then the name lookup is 
        /// a literal lookup of the name in the scope's dictionary.  Therefore, it is case-sensitive for example.  
        /// 
        /// If there is a default engine, then the name lookup uses that language's semantics.
        /// 
        /// Throws an exception if the engine cannot perform the requested type conversion.
        /// </summary>
        public T GetVariable<T>(IScriptScope/*!*/ scope, string/*!*/ name) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(name, "name");

            return Operations.ConvertTo<T>(GetVariable(scope, name));
        }

        /// <summary>
        /// Fetches the value of a variable stored in the scope and returns 
        /// a Boolean indicating success of the lookup.  
        /// 
        /// When the method's result is false, then it assigns default(T) to value.
        /// 
        /// If there is no engine associated with the scope (see ScriptRuntime.CreateScope), 
        /// then the name lookup is a literal lookup of the name in the scope's dictionary.  Therefore, 
        /// it is case-sensitive for example.  
        /// 
        /// If there is a default engine, then the name lookup uses that language's semantics.
        /// 
        /// Throws an exception if the engine cannot perform the requested type conversion, 
        /// then it return false and assigns value to default(T).
        /// </summary>
        public bool TryGetVariable<T>(IScriptScope/*!*/ scope, string/*!*/ name, out T value) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(name, "name");

            object res;
            if (TryGetVariable(scope, name, out res)) {
                return Operations.TryConvertTo<T>(res, out value);
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// This method returns whether the variable is bound in this scope.
        /// 
        /// If there is no engine associated with the scope (see ScriptRuntime.CreateScope), 
        /// then the name lookup is a literal lookup of the name in the scope's dictionary.  Therefore, 
        /// it is case-sensitive for example.  
        /// 
        /// If there is a default engine, then the name lookup uses that language's semantics.
        /// </summary>
        public bool ContainsVariable(IScriptScope/*!*/ scope, string/*!*/ name) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(name, "name");

            object dummy;
            return scope.TryGetVariable(name, out dummy);
        }

        #endregion

        #region Remoting Support
#if !SILVERLIGHT

        /// <summary>
        /// Fetches the value of a variable stored in the scope and returns an the wrapped object as an ObjectHandle.
        /// 
        /// If there is no engine associated with the scope (see ScriptRuntime.CreateScope), then the name lookup is 
        /// a literal lookup of the name in the scope's dictionary.  Therefore, it is case-sensitive for example.  
        /// 
        /// If there is a default engine, then the name lookup uses that language's semantics.
        /// </summary>
        public ObjectHandle GetVariableAsHandle(IScriptScope/*!*/ scope, string/*!*/ name) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(name, "name");

            return new ObjectHandle(GetVariable(scope, name));
        }

        /// <summary>
        /// Assigns a value to a variable in the scope, overwriting any previous value.
        /// 
        /// The ObjectHandle value is unwrapped before performing the assignment.
        /// 
        /// If there is no engine associated with the scope (see ScriptRuntime.CreateScope), 
        /// then the name lookup is a literal lookup of the name in the scope's dictionary.  Therefore, 
        /// it is case-sensitive for example.  
        /// 
        /// If there is a default engine, then the name lookup uses that language's semantics.
        /// </summary>
        public void SetVariable(IScriptScope scope/*!*/, string/*!*/ name, ObjectHandle value) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(name, "name");

            SetVariable(scope, name, value.Unwrap());
        }

        /// <summary>
        /// Fetches the value of a variable stored in the scope and returns 
        /// a Boolean indicating success of the lookup.  
        /// 
        /// When the method's result is false, then it assigns null to the value.  Otherwise
        /// an ObjectHandle wrapping the object is assigned to value.
        /// 
        /// If there is no engine associated with the scope (see ScriptRuntime.CreateScope), 
        /// then the name lookup is a literal lookup of the name in the scope's dictionary.  Therefore, 
        /// it is case-sensitive for example.  
        /// 
        /// If there is a default engine, then the name lookup uses that language's semantics.
        /// </summary>
        public bool TryGetVariableAsHandle(IScriptScope/*!*/ scope, string/*!*/ name, out ObjectHandle value) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(name, "name");

            object res;
            if (TryGetVariable(scope, name, out res)) {
                value = new ObjectHandle(res);
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Execute the code in the specified ScriptScope and return a result.
        /// 
        /// ExecuteAndGetAsHandle returns an ObjectHandle wrapping the resulting value 
        /// of running the code.  
        /// 
        /// When the ScriptSource is a file or statement, the engine decides what is 
        /// an appropriate value to return.  Some languages return the value produced 
        /// by the last expression or statement, but languages that are not expression 
        /// based may return null.
        /// </summary>
        public ObjectHandle ExecuteAndGetAsHandle(IScriptScope/*!*/ scope, ScriptSource/*!*/ scriptSource) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(scriptSource, "scriptSource");

            return new ObjectHandle(Execute(scope, scriptSource));
        }

        RemoteWrapper ILocalObject.Wrap() {
            return new RemoteScriptEngine(this);
        }
#endif
        #endregion

        #region Additional Services

        /// <summary>
        /// This method returns a language-specific service.  
        /// 
        /// It provides a point of extensibility for a language implementation 
        /// to offer more functionality than the standard engine members discussed here.
        /// </summary>
        public virtual ServiceType GetService<ServiceType>(params object[] args) where ServiceType : class {
            return _language.GetService<ServiceType>(ArrayUtils.Insert((object)this, args));
        }

        #endregion

        #region Misc. engine information

        /// <summary>
        /// This property returns the EngineOptions this engine is using.  
        /// 
        /// EngineOptions lets you control behaviors such as whether debugging is enabled, 
        /// whether code should be interpreted or compiled, etc.
        /// </summary>
        public EngineOptions/*!*/ Options {
            get {
                return _language.Options;
            }
        }

        /// <summary>
        /// This property returns the ScriptRuntime for the context in which this engine executes.
        /// </summary>
        public IScriptEnvironment/*!*/ Runtime {
            get {
                return _runtime;
            }
        }

        /// <summary>
        /// This property returns a display name for the engine or language that is suitable for UI.
        /// </summary>
        public string/*!*/ LanguageDisplayName {
            get {
                return _language.DisplayName;
            }
        }

        /// <summary>
        /// These methods return unique identifiers for this engine that map to this engine and its language.
        /// </summary>
        public string/*!*/[]/*!*/ GetRegisteredIdentifiers() {
            return _runtime.GetRegisteredLanguageIdentifiers(_language);
        }

        /// <summary>
        /// These methods return file extensions for this engine that map to this engine and its language.
        /// </summary>
        public string/*!*/[]/*!*/ GetRegisteredExtensions() {
            return _runtime.GetRegisteredFileExtensions(_language);
        }

        /// <summary>
        /// This property returns the engine's version as a string.  The format is language-dependent.
        /// </summary>
        public Version LanguageVersion {
            get {
                return _language.LanguageVersion;
            }
        }

        #endregion

        /// <summary>
        /// This method sets the search paths used by the engine for loading files when a script wants 
        /// to import or require another file of code.  
        /// 
        /// This setting affects this engine's processing of code that loads other files.  When hosts 
        /// call ScriptRuntime.ExecuteFile, the host's resolution or the default host's DLRPath 
        /// controls partial file name resolution.
        /// </summary>
        public void SetScriptSourceSearchPaths(string/*!*/[]/*!*/ paths) {
            Contract.RequiresNotNull(paths, "paths");
            Contract.RequiresNotNullItems(paths, "paths");

            _language.SetScriptSourceSearchPaths(paths);
        }

        /// <summary>
        /// This method returns a string in the style of this engine's language to describe the exception argument.
        /// </summary>
        public string/*!*/ FormatException(Exception/*!*/ exception) {
            Contract.RequiresNotNull(exception, "exception");

            return _language.FormatException(exception);
        }

        #region Private implementation details

        // Gets a LanguageContext for the specified module that captures the current state 
        // of the module which will be used for compilation and execution of the next piece of code against the module.
        private CodeContext GetCodeContext(IScriptScope/*!*/ scope) {
            Debug.Assert(scope != null);

            ScriptScope localScope = RemoteWrapper.GetLocalArgument<ScriptScope>(scope, "scope");
            CodeContext res = new CodeContext(localScope.Scope, _language);

            return res;
        }

        #endregion

        #region Internal API Surface

        internal LanguageContext/*!*/ LanguageContext {
            get {
                return _language;
            }
        }

        public void Shutdown() {
            _language.Shutdown();
        }

        public TextWriter GetOutputWriter(bool isErrorOutput) {
            return _language.GetOutputWriter(isErrorOutput);
        }

        public ErrorSink GetCompilerErrorSink() {
            return _language.GetCompilerErrorSink();
        }

        public CompilerOptions GetDefaultCompilerOptions() {
            return _language.GetDefaultCompilerOptions();
        }

        #endregion

        #region Obsolete

        public void DumpDebugInfo() {
            if (ScriptDomainManager.Options.EngineDebug) {
                PerfTrack.DumpStats();
                try {
                    ScriptDomainManager.CurrentManager.Snippets.Dump();
                } catch (NotSupportedException) { } // usually not important info...
            }
        }

        Type/*!*/ IRemotable.GetType() {
            return typeof(ScriptEngine);
        }

        #endregion
    }
}
