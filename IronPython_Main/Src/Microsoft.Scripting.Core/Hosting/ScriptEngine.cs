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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using System.Text;
using System.CodeDom;
using Microsoft.Scripting.Shell;
using System.Diagnostics;
using System.Security.Permissions;

namespace Microsoft.Scripting.Hosting {
    
    /// <summary>
    /// Represents a language in Hosting API. 
    /// Hosting API counterpart for <see cref="LanguageContext"/>.
    /// </summary>
    [DebuggerDisplay("{_language.DisplayName}")]
    public sealed class ScriptEngine 
#if !SILVERLIGHT
        : MarshalByRefObject 
#endif
    {
        private readonly LanguageContext/*!*/ _language;
        private readonly ScriptRuntime/*!*/ _runtime;
        private ObjectOperations _operations;
        private Scope _dummyScope;

        internal ScriptEngine(ScriptRuntime/*!*/ runtime, LanguageContext/*!*/ context) {
            Debug.Assert(runtime != null);
            Debug.Assert(context != null);

            _runtime = runtime;
            _language = context;
        }

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
            return new ObjectOperations(new DynamicOperations(GetCodeContext(null)));
        }

        /// <summary>
        /// Returns a new ObjectOperations object that inherits any semantics particular to the provided ScriptScope.  
        /// 
        /// See the Operations property for why you might want to call this.
        /// </summary>
        public ObjectOperations/*!*/ CreateOperations(ScriptScope/*!*/ scope) {
            Contract.RequiresNotNull(scope, "scope");

            return new ObjectOperations(new DynamicOperations(GetCodeContext(scope)));
        }

        #endregion

        #region Scopes

        public ScriptScope/*!*/ CreateScope() {
            return new ScriptScope(this, new Scope());
        }

        public ScriptScope/*!*/ CreateScope(IAttributesCollection/*!*/ dictionary) {
            Contract.RequiresNotNull(dictionary, "dictionary");
            return new ScriptScope(this, new Scope(dictionary));
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
        public ScriptScope GetScope(string/*!*/ path) {
            Contract.RequiresNotNull(path, "path");
            Scope scope = _language.GetScope(path);
            return (scope != null) ? new ScriptScope(this, scope) : null;
        }

        // TODO: remove
        public void PublishModule(string/*!*/ name, ScriptScope/*!*/ scope) {
            _language.PublishModule(name, scope.Scope);
        }

        #endregion

        #region Source Unit Creation

        /// <summary>
        /// Return a ScriptSource object from string contents with the current engine as the language binding.
        /// 
        /// The default SourceCodeKind is Expression.
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
        public ScriptSource/*!*/ CreateScriptSourceFromString(string/*!*/ code, SourceCodeKind kind) {
            Contract.RequiresNotNull(code, "code");
            Contract.Requires(EnumBounds.IsValid(kind), "kind");

            return CreateScriptSource(new SourceStringContentProvider(code), null, kind);
        }

        /// <summary>
        /// Return a ScriptSource object from string contents with the current engine as the language binding.
        /// 
        /// The default SourceCodeKind is Expression.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromString(string/*!*/ code, string id) {
            Contract.RequiresNotNull(code, "code");

            return CreateScriptSource(new SourceStringContentProvider(code), id, SourceCodeKind.Expression);
        }

        /// <summary>
        /// Return a ScriptSource object from string contents.  These are helpers for creating ScriptSources' with the right language binding.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromString(string/*!*/ code, string id, SourceCodeKind kind) {
            Contract.RequiresNotNull(code, "code");
            Contract.Requires(EnumBounds.IsValid(kind), "kind");
            
            return CreateScriptSource(new SourceStringContentProvider(code), id, kind);
        }

        /// <summary>
        /// Return a ScriptSource object from file contents with the current engine as the language binding.  
        /// 
        /// The path's extension does NOT have to be in ScriptRuntime.GetRegisteredFileExtensions 
        /// or map to this language engine with ScriptRuntime.GetEngineByFileExtension.
        /// 
        /// The default SourceCodeKind is File.
        /// 
        /// The ScriptSource's Path property will be the path argument.
        /// 
        /// The encoding defaults to System.Text.Encoding.Default.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromFile(string/*!*/ path) {
            return CreateScriptSourceFromFile(path, StringUtils.DefaultEncoding, SourceCodeKind.File);
        }

        /// <summary>
        /// Return a ScriptSource object from file contents with the current engine as the language binding.  
        /// 
        /// The path's extension does NOT have to be in ScriptRuntime.GetRegisteredFileExtensions 
        /// or map to this language engine with ScriptRuntime.GetEngineByFileExtension.
        /// 
        /// The default SourceCodeKind is File.
        /// 
        /// The ScriptSource's Path property will be the path argument.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromFile(string/*!*/ path, Encoding/*!*/ encoding) {
            return CreateScriptSourceFromFile(path, encoding, SourceCodeKind.File);
        }

        /// <summary>
        /// Return a ScriptSource object from file contents with the current engine as the language binding.  
        /// 
        /// The path's extension does NOT have to be in ScriptRuntime.GetRegisteredFileExtensions 
        /// or map to this language engine with ScriptRuntime.GetEngineByFileExtension.
        /// 
        /// The ScriptSource's Path property will be the path argument.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSourceFromFile(string/*!*/ path, Encoding/*!*/ encoding, SourceCodeKind kind) {
            Contract.RequiresNotNull(path, "path");
            Contract.RequiresNotNull(encoding, "encoding");
            Contract.Requires(EnumBounds.IsValid(kind), "kind");
            if (!_language.CanCreateSourceCode) throw new NotSupportedException("Invariant engine cannot create scripts");

            return new ScriptSource(this, _language.CreateFileUnit(path, encoding, kind));
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
            return CreateScriptSource(content, null, SourceCodeKind.File);
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
            return CreateScriptSource(content, path, SourceCodeKind.File);
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
            return CreateScriptSource(content, null, kind);
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
        public ScriptSource/*!*/ CreateScriptSource(CodeObject/*!*/ content, string path, SourceCodeKind kind) {
            Contract.RequiresNotNull(content, "content");
            if (!_language.CanCreateSourceCode) throw new NotSupportedException("Invariant engine cannot create scripts");

            return new ScriptSource(this, _language.GenerateSourceCode(content, path, kind));
        }
#endif

        /// <summary>
        /// These methods return ScriptSource objects from stream contents with the current engine as the language binding.  
        /// 
        /// The default SourceCodeKind is File.
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
        /// The default SourceCodeKind is File.
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
        public ScriptSource/*!*/ CreateScriptSource(StreamContentProvider/*!*/ content, string id, Encoding/*!*/ encoding, SourceCodeKind kind) {
            Contract.RequiresNotNull(content, "content");
            Contract.RequiresNotNull(encoding, "encoding");
            Contract.Requires(EnumBounds.IsValid(kind), "kind");
            
            return CreateScriptSource(new EngineTextContentProvider(_language, content, encoding), id, kind);
        }

        /// <summary>
        /// This method returns a ScriptSource with the content provider supplied with the current engine as the language binding.
        /// 
        /// This helper lets you own the content provider so that you can implement a stream over internal host data structures, such as an editor's text representation.
        /// </summary>
        public ScriptSource/*!*/ CreateScriptSource(TextContentProvider/*!*/ contentProvider, string id, SourceCodeKind kind) {
            Contract.RequiresNotNull(contentProvider, "contentProvider");
            Contract.Requires(EnumBounds.IsValid(kind), "kind");
            if (!_language.CanCreateSourceCode) throw new NotSupportedException("Invariant engine cannot create scripts");
            
            return new ScriptSource(this, _language.CreateSourceUnit(contentProvider, id, kind));
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
        public object GetVariable(ScriptScope/*!*/ scope, string/*!*/ name) {
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
        public bool RemoveVariable(ScriptScope/*!*/ scope, string/*!*/ name) {
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
        public void SetVariable(ScriptScope/*!*/ scope, string/*!*/ name, object value) {
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
        public bool TryGetVariable(ScriptScope/*!*/ scope, string/*!*/ name, out object value) {
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
        public T GetVariable<T>(ScriptScope/*!*/ scope, string/*!*/ name) {
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
        public bool TryGetVariable<T>(ScriptScope/*!*/ scope, string/*!*/ name, out T value) {
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
        public bool ContainsVariable(ScriptScope/*!*/ scope, string/*!*/ name) {
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
        public ObjectHandle GetVariableHandle(ScriptScope/*!*/ scope, string/*!*/ name) {
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
        public void SetVariable(ScriptScope scope/*!*/, string/*!*/ name, ObjectHandle value) {
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
        public bool TryGetVariableHandle(ScriptScope/*!*/ scope, string/*!*/ name, out ObjectHandle value) {
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
#endif
        #endregion

        #region Additional Services

        /// <summary>
        /// This method returns a language-specific service.  
        /// 
        /// It provides a point of extensibility for a language implementation 
        /// to offer more functionality than the standard engine members discussed here.
        /// </summary>
        public TService GetService<TService>(params object[] args) where TService : class {
            return _language.GetService<TService>(ArrayUtils.Insert((object)this, args));
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
        public ScriptRuntime/*!*/ Runtime {
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public CompilerOptions/*!*/ GetCompilerOptions() {
            return _language.GetCompilerOptions();
        }

        public CompilerOptions/*!*/ GetCompilerOptions(ScriptScope/*!*/ scope) {
            return _language.GetCompilerOptions(scope.Scope);
        }

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
        private CodeContext GetCodeContext(ScriptScope scriptScope) {
            Scope scope;
            
            if (scriptScope != null) {
                scope = scriptScope.Scope;
            } else {
                // TODO: we need a scope to CodeContext; could we allow CodeContext w/o scope?
                if (_dummyScope == null) {
                    Interlocked.CompareExchange(ref _dummyScope, new Scope(), null);
                }
                scope = _dummyScope;
            }

            return new CodeContext(scope, _language);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ErrorSink GetCompilerErrorSink() {
            return _language.GetCompilerErrorSink();
        }
        
        #endregion

        #region Remote API
#if !SILVERLIGHT

        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }

#endif
        public void GetExceptionMessage(Exception exception, out string message, out string errorTypeName) {
            _language.GetExceptionMessage(exception, out message, out errorTypeName);
        }

        #endregion
    }
}
