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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Compiler;
using IronPython.Compiler.Generation;
using IronPython.Hosting;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using PyAst = IronPython.Compiler.Ast;

namespace IronPython.Runtime {
    public sealed class PythonContext : LanguageContext {
        private static readonly Guid PythonLanguageGuid = new Guid("03ed4b80-d10b-442f-ad9a-47dae85b2051");
        private static readonly Guid LanguageVendor_Microsoft = new Guid(-1723120188, -6423, 0x11d2, 0x90, 0x3f, 0, 0xc0, 0x4f, 0xa3, 2, 0xa1);
#if !SILVERLIGHT
        private static int _hookedAssemblyResolve;
#endif

        private readonly PythonEngineOptions/*!*/ _engineOptions;
        private readonly IDictionary<object, object>/*!*/ _modulesDict = new PythonDictionary();
        private readonly Dictionary<SymbolId, ModuleGlobalCache>/*!*/ _builtinCache = new Dictionary<SymbolId, ModuleGlobalCache>();
        private readonly Scope/*!*/ _systemState;
        private readonly Dictionary<Type, string>/*!*/ _builtinModuleNames = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type>/*!*/ _builtinsDict;
        private readonly PythonFileManager/*!*/ _fileManager = new PythonFileManager();
        private readonly Dictionary<object, object> _moduleState = new Dictionary<object, object>();
        private readonly Dictionary<string, object> _errorHandlers = new Dictionary<string, object>();
        private readonly List<object> _searchFunctions = new List<object>();
        /// <summary> stored for copy_reg module, used for reduce protocol </summary>
        internal BuiltinFunction NewObject;
        /// <summary> stored for copy_reg module, used for reduce protocol </summary>
        internal BuiltinFunction PythonReconstructor;

        private Encoding _defaultEncoding = PythonAsciiEncoding.Instance;
        private string _initialVersionString;
#if !SILVERLIGHT
        private string _initialExecutable, _initialPrefix = typeof(PythonContext).Assembly.CodeBase;
#else
        private string _initialExecutable, _initialPrefix = "";
#endif
        private Scope _clrModule;
        private Scope _builtins;
        private ScriptEngine _engine;

        /// <summary>
        /// Creates a new PythonContext not bound to Engine.
        /// </summary>
        public PythonContext(ScriptDomainManager/*!*/ manager)
            : base(manager) {
            _builtinsDict = CreateBuiltinTable();

            // singletons:
            _engineOptions = new PythonEngineOptions();

            DefaultContext.CreateContexts(this);

            // need to run PythonOps 1st so the type system is spun up...
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PythonOps).TypeHandle);

            Binder = new PythonBinder(this, DefaultContext.DefaultCLS);

            if (DefaultContext.Default.LanguageContext.Binder == null) {
                // hack to fix the default language context binder, there's an order of 
                // initialization issue w/ the binder & the default context.
                ((PythonContext)DefaultContext.Default.LanguageContext).Binder = Binder;
            }
            if (DefaultContext.DefaultCLS.LanguageContext.Binder == null) {
                // hack to fix the default language context binder, there's an order of 
                // initialization issue w/ the binder & the default context.
                ((PythonContext)DefaultContext.DefaultCLS.LanguageContext).Binder = Binder;
            }

            InitializeBuiltins();

            _systemState = CreateBuiltinModule("sys", typeof(SysModule), ModuleOptions.NoBuiltins).Scope;
            InitializeSystemState();
#if SILVERLIGHT
            AddToPath("");
#endif

            // sys.argv always includes at least one empty string.
            Debug.Assert(PythonOptions.Arguments != null);
            SetSystemStateValue("argv", new List(PythonOptions.Arguments.Length == 0 ? new object[] { String.Empty } : PythonOptions.Arguments));

#if !SILVERLIGHT // AssemblyResolve
            try {
                if (Interlocked.Exchange(ref _hookedAssemblyResolve, 1) == 0) {
                    HookAssemblyResolve();
                }
            } catch (System.Security.SecurityException) {
                // We may not have SecurityPermissionFlag.ControlAppDomain. 
                // If so, we will not look up sys.path for module loads
            }
#endif
        }

        public override EngineOptions/*!*/ Options {
            get { return PythonOptions; }
        }

        /// <summary>
        /// Checks to see if module state has the current value stored already.
        /// </summary>
        public bool HasModuleState(object key) {
            lock (_moduleState) {
                return _moduleState.ContainsKey(key);
            }
        }

        /// <summary>
        /// Gets per-runtime state used by a module.  The module should have a unique key for
        /// each piece of state it needs to store.
        /// </summary>
        public object GetModuleState(object key) {
            lock (_moduleState) {
                Debug.Assert(_moduleState.ContainsKey(key));

                return _moduleState[key];
            }
        }

        /// <summary>
        /// Sets per-runtime state used by a module.  The module should have a unique key for
        /// each piece of state it needs to store.
        /// </summary>
        public void SetModuleState(object key, object value) {
            lock (_moduleState) {
                _moduleState[key] = value;
            }
        }

        public PythonEngineOptions/*!*/ PythonOptions {
            get {
                return _engineOptions;
            }
        }

        public override Guid VendorGuid {
            get {
                return LanguageVendor_Microsoft;
            }
        }

        public override Guid LanguageGuid {
            get {
                return PythonLanguageGuid;
            }
        }

        public Scope/*!*/ SystemState {
            get {
                return _systemState;
            }
        }

        public Scope/*!*/ ClrModule {
            get {
                if (_clrModule == null) {
                    Interlocked.CompareExchange<Scope>(ref _clrModule, CreateBuiltinModule("clr").Scope, null);
                }

                return _clrModule;
            }
        }

        public bool TryGetSystemPath(out List path) {
            object val;
            if (SystemState.Dict.TryGetValue(SymbolTable.StringToId("path"), out val)) {
                path = val as List;
            } else {
                path = null;
            }

            return path != null;
        }

        public object SystemStandardOut {
            get {
                return GetSystemStateValue("stdout");
            }
        }

        public object SystemStandardIn {
            get {
                return GetSystemStateValue("stdin");
            }
        }

        public object SystemStandardError {
            get {
                return GetSystemStateValue("stderr");
            }
        }

        public IDictionary<object, object> SystemStateModules {
            get {
                return _modulesDict;
            }
        }

        // as of 1.5 preferred access is exc_info, these may be null.
        internal object SystemExceptionType {
            set {
                SetSystemStateValue("exc_type", value);
            }
        }

        public object SystemExceptionValue {
            set {
                SetSystemStateValue("exc_value", value);
            }
        }

        public object SystemExceptionTraceBack {
            set {
                SetSystemStateValue("exc_traceback", value);
            }
        }

        internal PythonModule GetModuleByName(string/*!*/ name) {
            Assert.NotNull(name);
            object scopeObj;
            Scope scope;
            if (SystemStateModules.TryGetValue(name, out scopeObj) && (scope = scopeObj as Scope) != null) {
                return EnsurePythonModule(scope);
            }
            return null;
        }

        internal PythonModule GetModuleByPath(string/*!*/ path) {
            Assert.NotNull(path);
            foreach (object scopeObj in SystemStateModules.Values) {
                Scope scope = scopeObj as Scope;
                if (scope != null) {
                    PythonModule module = EnsurePythonModule(scope);
                    if (DomainManager.Platform.PathComparer.Compare(module.GetFile(), path) == 0) {
                        return module;
                    }
                }
            }
            return null;
        }

        public override string DisplayName {
            get {
                return "IronPython 2.0 Beta";
            }
        }

        public override Version LanguageVersion {
            get {
                // Assembly.GetName() can't be called in Silverlight...
                return new AssemblyName(GetType().Assembly.FullName).Version;
            }
        }

        public ScriptEngine ScriptEngine {
            get {
                return _engine;
            }
            // friend: ScriptEngine
            internal set {
                Assert.NotNull(value);
                _engine = value;
            }
        }
        
        /// <summary>
        /// Initializes the sys module on startup.  Called both to load and reload sys
        /// </summary>
        private void InitializeSystemState() {
            // These fields do not get reset on "reload(sys)", we populate them once on startup
            SetSystemStateValue("argv", new List(new object[] { String.Empty }));                
            SetSystemStateValue("modules", _modulesDict);

            _modulesDict["sys"] = _systemState;

            SetSystemStateValue("path", new List(3));
            SetSystemStateValue("ps1", ">>> ");
            SetSystemStateValue("ps1", "... ");

            SetStandardIO();

            SystemExceptionType = SystemExceptionValue = SystemExceptionTraceBack = null;

            SysModule.PerformModuleReload(this, _systemState.Dict);
        }

        public override LambdaExpression ParseSourceCode(CompilerContext context) {
            Contract.RequiresNotNull(context, "context");

            PyAst.PythonAst ast;
            SourceCodeProperties properties = SourceCodeProperties.None;
            bool propertiesSet = false;
            int errorCode = 0;

            using (Parser parser = Parser.CreateParser(context, PythonContext.GetPythonOptions(null))) {
                switch (context.SourceUnit.Kind) {
                    case SourceCodeKind.InteractiveCode:
                        ast = parser.ParseInteractiveCode(out properties);
                        propertiesSet = true;
                        break;

                    case SourceCodeKind.Expression:
                        ast = parser.ParseTopExpression();
                        break;

                    case SourceCodeKind.SingleStatement:
                        ast = parser.ParseSingleStatement();
                        break;

                    case SourceCodeKind.File:
                        ast = parser.ParseFile(true);
                        break;

                    default:
                    case SourceCodeKind.Statements:
                        ast = parser.ParseFile(false);
                        break;
                }

                errorCode = parser.ErrorCode;
            }

            if (!propertiesSet && errorCode != 0) {
                properties = SourceCodeProperties.IsInvalid;
            }

            context.SourceUnit.CodeProperties = properties;

            if (errorCode != 0 || properties == SourceCodeProperties.IsEmpty) {
                return null;
            }

            // TODO: remove when the module is generated by PythonAst.Transform:
            PythonCompilerOptions pco = (PythonCompilerOptions)context.Options;
            pco.TrueDivision = ast.TrueDivision;
            pco.AllowWithStatement = ast.AllowWithStatement;
            pco.AbsoluteImports = ast.AbsoluteImports;

            PyAst.PythonNameBinder.BindAst(ast, context);

            return ast.TransformToAst(context);
        }

        public override StreamReader GetSourceReader(Stream stream, Encoding encoding) {
            Contract.RequiresNotNull(stream, "stream");
            Contract.RequiresNotNull(encoding, "encoding");
            Contract.Requires(stream.CanSeek && stream.CanRead, "stream", "The stream must support seeking and reading");

            // we choose ASCII by default, if the file has a Unicode pheader though
            // we'll automatically get it as unicode.
            Encoding default_encoding = encoding;
            encoding = PythonAsciiEncoding.Instance;

            long start_position = stream.Position;

            StreamReader sr = new StreamReader(stream, PythonAsciiEncoding.Instance);            

            int bytesRead = 0;
            string line;
            line = ReadOneLine(sr, ref bytesRead);

            //string line = sr.ReadLine();
            bool gotEncoding = false;

            // magic encoding must be on line 1 or 2
            if (line != null && !(gotEncoding = Tokenizer.TryGetEncoding(default_encoding, line, ref encoding))) {
                line = ReadOneLine(sr, ref bytesRead);

                if (line != null) {
                    gotEncoding = Tokenizer.TryGetEncoding(default_encoding, line, ref encoding);
                }
            }

            if (gotEncoding && sr.CurrentEncoding != PythonAsciiEncoding.Instance && encoding != sr.CurrentEncoding) {
                // we have both a BOM & an encoding type, throw an error
                throw new IOException("file has both Unicode marker and PEP-263 file encoding");
            }

            if (encoding == null)
                throw new IOException("unknown encoding type");

            if (!gotEncoding) {
                // if we didn't get an encoding seek back to the beginning...
                stream.Seek(start_position, SeekOrigin.Begin);
            } else {
                // if we got an encoding seek to the # of bytes we read (so the StreamReader's
                // buffering doesn't throw us off)
                stream.Seek(bytesRead, SeekOrigin.Begin);
            }

            // re-read w/ the correct encoding type...
            return new StreamReader(stream, encoding);
        }

        /// <summary>
        /// Reads one line keeping track of the # of bytes read
        /// </summary>
        private static string ReadOneLine(StreamReader sr, ref int totalRead) {
            char[] buffer = new char[256];
            StringBuilder builder = null;
            
            int bytesRead = sr.Read(buffer, 0, buffer.Length);

            while (bytesRead > 0) {
                totalRead += bytesRead;

                bool foundEnd = false;
                for (int i = 0; i < bytesRead; i++) {
                    if (buffer[i] == '\r') {
                        if (i + 1 < bytesRead) {
                            if (buffer[i + 1] == '\n') {
                                totalRead -= (bytesRead - (i+2));   // skip cr/lf
                                sr.BaseStream.Seek(i + 1, SeekOrigin.Begin);
                                sr.DiscardBufferedData();
                                foundEnd = true;
                            }
                        } else {
                            totalRead -= (bytesRead - (i + 1)); // skip cr
                            sr.BaseStream.Seek(i, SeekOrigin.Begin);
                            sr.DiscardBufferedData();
                            foundEnd = true;
                        }
                    } else if (buffer[i] == '\n') {
                        totalRead -= (bytesRead - (i + 1)); // skip lf
                        sr.BaseStream.Seek(i + 1, SeekOrigin.Begin);
                        sr.DiscardBufferedData();
                        foundEnd = true;
                    }

                    if (foundEnd) {                        
                        if (builder != null) {
                            builder.Append(buffer, 0, i);
                            return builder.ToString();
                        }
                        return new string(buffer, 0, i);
                    }
                }

                if (builder == null) builder = new StringBuilder();
                builder.Append(buffer, 0, bytesRead);
                bytesRead = sr.Read(buffer, 0, buffer.Length);
            }

            // no string
            if (builder == null) {
                return null;
            }

            // no new-line
            return builder.ToString();
        }

#if !SILVERLIGHT
        // Convert a CodeDom to source code, and output the generated code and the line number mappings (if any)
        public override SourceUnit/*!*/ GenerateSourceCode(System.CodeDom.CodeObject codeDom, string path, SourceCodeKind kind) {
            return new PythonCodeDomCodeGen().GenerateCode((System.CodeDom.CodeMemberMethod)codeDom, this, path, kind);
        }
#endif

        #region Scopes

        public override Scope GetScope(string/*!*/ path) {
            PythonModule module = GetModuleByPath(path);
            return (module != null) ? module.Scope : null;
        }
        
        public PythonModule GetPythonModule(Scope scope) {
            return (PythonModule)GetScopeExtension(scope);
        }
        
        public PythonModule EnsurePythonModule(Scope scope) {
            return (PythonModule)EnsureScopeExtension(scope);
        }

        public override ScopeExtension CreateScopeExtension(Scope scope) {
            return CreatePythonModule(null, null, scope, ModuleOptions.None);
        }

        // TODO: remove
        public override void ModuleContextEntering(ScopeExtension newContext) {
            if (newContext == null) return;

            PythonModule newPythonContext = (PythonModule)newContext;

            // code executed in the scope of module cannot disable TrueDivision:

            // TODO: doesn't work for evals (test_future.py) 
            //if (newPythonContext.TrueDivision && !_trueDivision) {
            //    throw new InvalidOperationException("Code cannot be executed in this module (TrueDivision cannot be disabled).");
            //}

            // flow options into the module if they're set
            PythonCompilerOptions pco = ((PythonCompilerOptions)newPythonContext.CompilerContext.Options);
            newPythonContext.TrueDivision |= pco.TrueDivision;
            newPythonContext.AllowWithStatement |= pco.AllowWithStatement;
            newPythonContext.AbsoluteImports |= pco.AbsoluteImports;
        }

        internal PythonModule/*!*/ CompileAndInitializeModule(string moduleName, string fileName, SourceUnit sourceUnit) {
            ScriptCode compiledCode;
            return CompileModule(fileName, moduleName, sourceUnit, ModuleOptions.Initialize, false, out compiledCode);
        }

        public PythonModule/*!*/ CompileModule(string fileName, string moduleName, ModuleOptions options) {
            ScriptCode compiledCode;
            return CompileModule(fileName, moduleName, options, false, out compiledCode);
        }

        public PythonModule/*!*/ CompileModule(string fileName, string moduleName, ModuleOptions options, bool skipFirstLine) {
            ScriptCode compiledCode;
            return CompileModule(fileName, moduleName, options, skipFirstLine, out compiledCode);
        }

        public PythonModule/*!*/ CompileModule(string fileName, string moduleName, ModuleOptions options, bool skipFirstLine, out ScriptCode compiledCode) {
            SourceUnit sourceCode = CreateFileUnit(String.IsNullOrEmpty(fileName) ? null : fileName, DefaultEncoding);
            return CompileModule(fileName, moduleName, sourceCode, options, skipFirstLine, out compiledCode);
        }

        public PythonModule/*!*/ CompileModule(string fileName, string moduleName, SourceUnit sourceCode, ModuleOptions options, bool skipFirstLine) {
            ScriptCode compiledCode;
            return CompileModule(fileName, moduleName, sourceCode, options, skipFirstLine, out compiledCode);
        }

        public PythonModule/*!*/ CompileModule(string fileName, string moduleName, SourceUnit sourceCode, ModuleOptions options, bool skipFirstLine,
            out ScriptCode compiledCode) {

            Contract.RequiresNotNull(fileName, "fileName");
            Contract.RequiresNotNull(moduleName, "moduleName");
            Contract.RequiresNotNull(sourceCode, "sourceCode");

            PythonCompilerOptions compilerOptions = GetPythonCompilerOptions();
            compilerOptions.SkipFirstLine = skipFirstLine;

            options |= ModuleOptions.Optimized;     // Below we always generate optimized scope.
            compilerOptions.Module = options;

            compiledCode = sourceCode.Compile(compilerOptions, ThrowingErrorSink.Default);
            Scope scope = compiledCode.MakeOptimizedScope();
            scope.SetExtension(ContextId, CreatePythonModule(moduleName, fileName, scope, options));
            return CreateModule(moduleName, fileName, scope, compiledCode, options);
        }

        public PythonModule/*!*/ CreateModule(string moduleName) {
            return CreateModule(moduleName, null, PythonDictionary.MakeSymbolDictionary(), ModuleOptions.None);
        }

        public PythonModule/*!*/ CreateBuiltinModule(string moduleName, Type type) {
            return CreateBuiltinModule(moduleName, type, ModuleOptions.None);
        }

        public PythonModule/*!*/ CreateBuiltinModule(string moduleName, Type type, ModuleOptions options) {
            PythonDictionary dict = new PythonDictionary(new ModuleDictionaryStorage(type));

            if (type == typeof(Builtin)) {
                Builtin.PerformModuleReload(this, dict);
            } else if (type != typeof(SysModule)) { // will be performed by hand later, see InitializeSystemState
                MethodInfo reload = type.GetMethod("PerformModuleReload");
                if (reload != null) {
                    Debug.Assert(reload.IsStatic);

                    reload.Invoke(null, new object[] { this, dict });
                }
            }

            //IronPython.Runtime.Types.PythonModuleOps.PopulateModuleDictionary(this, dict, type);
            return CreateModule(moduleName, null, new Scope(dict), null, options);
        }

        public PythonModule/*!*/ CreateModule(string moduleName, ModuleOptions options) {
            return CreateModule(moduleName, null, PythonDictionary.MakeSymbolDictionary(), options);
        }

        public PythonModule/*!*/ CreateModule(string moduleName, string fileName, IDictionary<string, object> globals, ModuleOptions options) {
            Contract.RequiresNotNull(moduleName, "moduleName");
            Contract.RequiresNotNull(globals, "globals");

            IAttributesCollection globalDict = globals as IAttributesCollection ?? new PythonDictionary(new StringDictionaryStorage(globals));
            return CreateModule(moduleName, fileName, globalDict, options);
        }

        public PythonModule/*!*/ CreateModule(string moduleName, string fileName, IAttributesCollection globals, ModuleOptions options) {
            Contract.RequiresNotNull(moduleName, "moduleName");
            Contract.RequiresNotNull(globals, "globals");

            return CreateModule(moduleName, fileName, new Scope(globals), null, options);
        }

        private PythonModule/*!*/ CreateModule(string moduleName, string fileName, Scope scope, ScriptCode scriptCode, ModuleOptions options) {
            if (scope == null) {
                scope = new Scope(PythonDictionary.MakeSymbolDictionary());
            }

            PythonModule module = CreatePythonModule(moduleName, fileName, scope, options);
            module.ShowCls = (options & ModuleOptions.ShowClsMethods) != 0;
            module.TrueDivision = (options & ModuleOptions.TrueDivision) != 0;
            module.AllowWithStatement = (options & ModuleOptions.WithStatement) != 0;
            module.AbsoluteImports = (options & ModuleOptions.AbsoluteImports) != 0;
            module.IsPythonCreatedModule = true;

            if ((options & ModuleOptions.Initialize) != 0) {
                Importer.InitializeModule(this, moduleName, module, scriptCode, true);
            } else if ((options & ModuleOptions.PublishModule) != 0) {
                PublishModule(moduleName, module);
            }

            return module;
        }

        internal PythonModule/*!*/ CreatePythonModule(string moduleName, string fileName, Scope/*!*/ scope, ModuleOptions options) {
            Contract.RequiresNotNull(scope, "scope");

            PythonModule module = new PythonModule(scope);
            module = (PythonModule)scope.SetExtension(ContextId, module);

            // adds __builtin__ variable if necessary.  Python adds the module directly to
            // __main__ and __builtin__'s dictionary for all other modules.  Our callers
            // pass the appropriate flags to control this behavior.
            if ((options & ModuleOptions.NoBuiltins) == 0 && !scope.ContainsName(Symbols.Builtins)) {
                if ((options & ModuleOptions.ModuleBuiltins) != 0) {
                    module.Scope.SetName(Symbols.Builtins, BuiltinModuleInstance); // TODO: PythonContext.Id, 
                } else {
                    module.Scope.SetName(Symbols.Builtins, BuiltinModuleInstance.Dict); // TODO: PythonContext.Id, 
                }
            }

            // do not set names if null to make attribute getter pas thru:
            if (moduleName != null) {
                module.SetName(moduleName);
            }

            if (fileName != null) {
                module.SetFile(fileName);
            }

            // If the filename is __init__.py then this is the initialization code
            // for a package and we need to set the __path__ variable appropriately
            if (fileName != null && Path.GetFileName(fileName) == "__init__.py") {
                string dirname = Path.GetDirectoryName(fileName);
                string dir_path = DomainManager.Platform.GetFullPath(dirname);
                module.Scope.SetName(Symbols.Path, PythonOps.MakeList(dir_path));
            }

            return module;
        }

        public void PublishModule(string/*!*/ name, PythonModule/*!*/ module) {
            Contract.RequiresNotNull(name, "name");
            Contract.RequiresNotNull(module, "module");
            SystemStateModules[name] = module.Scope;
        }

        // TODO: remove
        public override void PublishModule(string/*!*/ name, Scope/*!*/ scope) {
            PublishModule(name, DefaultContext.DefaultPythonContext.EnsurePythonModule(scope));
        }

        internal PythonModule GetReloadableModule(Scope/*!*/ scope) {
            Assert.NotNull(scope);

            PythonModule module = (PythonModule)GetScopeExtension(scope);

            if (module == null || !module.IsPythonCreatedModule) {
                throw PythonOps.TypeError("can only reload Python modules");
            }

            object name;
            if (!scope.TryLookupName(Symbols.Name, out name) || !(name is string)) {
                throw PythonOps.SystemError("nameless module");
            }

            if (!SystemStateModules.ContainsKey(name)) {
                throw PythonOps.ImportError("module {0} not in sys.modules", name);
            }

            return module;
        }

        #endregion

        /// <summary>
        /// Python's global scope includes looking at built-ins.  First check built-ins, and if
        /// not there then fallback to any DLR globals.
        /// </summary>
        public override bool TryLookupGlobal(CodeContext context, SymbolId name, out object value) {
            object builtins;
            if (!context.Scope.ModuleScope.TryGetName(Symbols.Builtins, out builtins)) {
                value = null;
                return false;
            }

            Scope scope = builtins as Scope;
            if (scope != null && scope.TryGetName(name, out value)) return true;

            IAttributesCollection dict = builtins as IAttributesCollection;
            if (dict != null && dict.TryGetValue(name, out value)) return true;

            return base.TryLookupGlobal(context, name, out value);
        }

        protected override Exception MissingName(SymbolId name) {
            throw PythonOps.NameError(name);
        }

        protected override ModuleGlobalCache GetModuleCache(SymbolId name) {
            ModuleGlobalCache res;
            if (!TryGetModuleGlobalCache(name, out res)) {
                res = base.GetModuleCache(name);
            }

            return res;
        }

        public override object Call(CodeContext context, object function, object[] args) {
            return PythonOps.CallWithContext(context, function, args);
        }

        public override object CallWithThis(CodeContext context, object function, object instance, object[] args) {
            return PythonOps.CallWithContextAndThis(context, function, instance, args);
        }

        public override object CallWithArgsKeywordsTupleDict(CodeContext context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
            return PythonOps.CallWithArgsTupleAndKeywordDictAndContext(context, func, args, names, argsTuple, kwDict);
        }

        public override object CallWithArgsTuple(CodeContext context, object func, object[] args, object argsTuple) {
            return PythonOps.CallWithArgsTupleAndContext(context, func, args, argsTuple);
        }

        public override object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names) {
            return PythonOps.CallWithKeywordArgs(context, func, args, names);
        }

        public override bool EqualReturnBool(CodeContext context, object x, object y) {
            return PythonOps.EqualRetBool(x, y);
        }

        public override bool IsCallable(object obj, int argumentCount, out int min, out int max) {
            return PythonOps.IsCallable(obj, argumentCount, out min, out max);
        }

        #region Assembly Loading

        public override Assembly LoadAssemblyFromFile(string file) {
#if !SILVERLIGHT
            // check all files in the path...
            List path;
            if (TryGetSystemPath(out path)) {
                IEnumerator ie = PythonOps.GetEnumerator(path);
                while (ie.MoveNext()) {
                    string str;
                    if (Converter.TryConvertToString(ie.Current, out str)) {
                        string fullName = Path.Combine(str, file);
                        Assembly res;

                        if (TryLoadAssemblyFromFileWithPath(fullName, out res)) return res;
                        if (TryLoadAssemblyFromFileWithPath(fullName + ".EXE", out res)) return res;
                        if (TryLoadAssemblyFromFileWithPath(fullName + ".DLL", out res)) return res;
                    }
                }
            }
#endif
            return null;
        }

#if !SILVERLIGHT // AssemblyResolve, files, path
        private bool TryLoadAssemblyFromFileWithPath(string path, out Assembly res) {
            if (File.Exists(path)) {
                try {
                    res = Assembly.LoadFile(path);
                    if (res != null) return true;
                } catch { }
            }
            res = null;
            return false;
        }

        internal static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            AssemblyName an = new AssemblyName(args.Name);
            return DefaultContext.Default.LanguageContext.LoadAssemblyFromFile(an.Name);
        }

        /// <summary>
        /// We use Assembly.LoadFile to load assemblies from a path specified by the script (in LoadAssemblyFromFileWithPath).
        /// However, when the CLR loader tries to resolve any of assembly references, it will not be able to
        /// find the dependencies, unless we can hook into the CLR loader.
        /// </summary>
        private static void HookAssemblyResolve() {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(PythonContext.CurrentDomain_AssemblyResolve);
        }
#endif
        #endregion

        public override void SetScriptSourceSearchPaths(string[] paths) {
            SetSystemStateValue("path", new List(paths));
        }
        
        public override TextWriter GetOutputWriter(bool isErrorOutput) {
            return new OutputWriter(this, isErrorOutput);
        }

        public override void Shutdown() {
            object callable;

            try {
                if (_systemState.TryGetName(Symbols.SysExitFunc, out callable)) {
                    PythonCalls.Call(callable);
                }
            } finally {
                if (PythonOptions.PerfStats) {
                    PerfTrack.DumpStats();
                }
            }
        }

        private static string GetInformationalVersion() {
            AssemblyInformationalVersionAttribute attribute = GetAssemblyAttribute<AssemblyInformationalVersionAttribute>();
            return attribute != null ? attribute.InformationalVersion : "";
        }

        private static string GetFileVersion() {
#if !SILVERLIGHT // file version
            AssemblyFileVersionAttribute attribute = GetAssemblyAttribute<AssemblyFileVersionAttribute>();
            return attribute != null ? attribute.Version : "";
#else
            return "1.0.0.0";
#endif
        }

        private static T GetAssemblyAttribute<T>() where T : Attribute {
            Assembly asm = typeof(ScriptEngine).Assembly;
            object[] attributes = asm.GetCustomAttributes(typeof(T), false);
            if (attributes != null && attributes.Length > 0) {
                return (T)attributes[0];
            } else {
                Debug.Assert(false, String.Format("Cannot find attribute {0}", typeof(T).Name));
                return null;
            }
        }

        // TODO: ExceptionFormatter service
        #region Stack Traces and Exceptions

        public override string FormatException(Exception exception) {
            SyntaxErrorException syntax_error = exception as SyntaxErrorException;
            if (syntax_error != null) {
                return FormatPythonSyntaxError(syntax_error);
            }

            object pythonEx = PythonExceptions.ToPython(exception);

            string result = FormatStackTraces(exception) + FormatPythonException(pythonEx) + Environment.NewLine;

            if (Options.ShowClrExceptions) {
                result += FormatCLSException(exception);
            }

            return result;
        }

        internal static string FormatPythonSyntaxError(SyntaxErrorException e) {
            string sourceLine = e.GetCodeLine();

            return String.Format(
                "  File \"{1}\", line {2}{0}" +
                "    {3}{0}" +
                "    {4}^{0}" +
                "{5}: {6}{0}",
                Environment.NewLine,
                e.GetSymbolDocumentName(),
                e.Line > 0 ? e.Line.ToString() : "?",
                (sourceLine != null) ? sourceLine.Replace('\t', ' ') : null,
                new String(' ', e.Column != 0 ? e.Column - 1 : 0),
                GetPythonExceptionClassName(PythonExceptions.ToPython(e)), e.Message);
        }

        private static string FormatCLSException(Exception e) {
            StringBuilder result = new StringBuilder();
            result.AppendLine("CLR Exception: ");
            while (e != null) {
                result.Append("    ");
                result.AppendLine(e.GetType().Name);
                if (!String.IsNullOrEmpty(e.Message)) {
                    result.AppendLine(": ");
                    result.AppendLine(e.Message);
                } else {
                    result.AppendLine();
                }

                e = e.InnerException;
            }

            return result.ToString();
        }

        internal static string FormatPythonException(object pythonException) {
            string result = "";

            // dump the python exception.
            if (pythonException != null) {
                string str = pythonException as string;
                if (str != null) {
                    result += str;
                } else if (pythonException is StringException) {
                    result += pythonException.ToString();
                } else {
                    result += GetPythonExceptionClassName(pythonException) + ": " + pythonException.ToString();
                }
            }

            return result;
        }

        private static string GetPythonExceptionClassName(object pythonException) {
            string className = "";
            object val;
            if (PythonOps.TryGetBoundAttr(pythonException, Symbols.Class, out val)) {
                if (PythonOps.TryGetBoundAttr(val, Symbols.Name, out val)) {
                    className = val.ToString();
                    if (PythonOps.TryGetBoundAttr(pythonException, Symbols.Module, out val)) {
                        string moduleName = val.ToString();
                        if (moduleName != PythonExceptions.DefaultExceptionModule) {
                            className = moduleName + "." + className;
                        }
                    }
                }
            }
            return className;
        }

        
#if SILVERLIGHT // stack trace
        private string FormatStackTraces(Exception e) {

            StringBuilder result = new StringBuilder();
            result.AppendLine("Traceback (most recent call last):");
            DynamicStackFrame[] dfs = RuntimeHelpers.GetDynamicStackFrames(e);
            for (int i = 0; i < dfs.Length; ++i) {
                DynamicStackFrame frame = dfs[i];
                result.AppendFormat("  at {0} in {1}, line {2}\n", frame.GetMethodName(), frame.GetFileName(), frame.GetFileLineNumber());
            }

            if (Options.ExceptionDetail) {
                result.AppendLine(e.Message);
            }
            
            return result.ToString();
        }
#else
        private string FormatStackTraces(Exception e) {
            bool printedHeader = false;

            return FormatStackTraces(e, ref printedHeader);
        }

        private string FormatStackTraces(Exception e, ref bool printedHeader) {
            string result = "";
            if (Options.ExceptionDetail) {
                if (!printedHeader) {
                    result = e.Message + Environment.NewLine;
                    printedHeader = true;
                }
                IList<System.Diagnostics.StackTrace> traces = ExceptionHelpers.GetExceptionStackTraces(e);

                if (traces != null) {
                    for (int i = 0; i < traces.Count; i++) {
                        for (int j = 0; j < traces[i].FrameCount; j++) {
                            StackFrame curFrame = traces[i].GetFrame(j);
                            result += curFrame.ToString() + Environment.NewLine;
                        }
                    }
                }

                if (e.StackTrace != null) result += e.StackTrace.ToString() + Environment.NewLine;
                if (e.InnerException != null) result += FormatStackTraces(e.InnerException, ref printedHeader);
            } else {
                result = FormatStackTraceNoDetail(e, ref printedHeader);
            }

            return result;
        }

        internal string FormatStackTraceNoDetail(Exception e, ref bool printedHeader) {
            string result = String.Empty;
            // dump inner most exception first, followed by outer most.
            if (e.InnerException != null) result += FormatStackTraceNoDetail(e.InnerException, ref printedHeader);

            if (!printedHeader) {
                result += "Traceback (most recent call last):" + Environment.NewLine;
                printedHeader = true;
            }

            foreach (DynamicStackFrame frame in ExceptionHelpers.GetStackFrames(e, true)) {
                MethodBase method = frame.GetMethod();
                if (method.DeclaringType != null &&
                    method.DeclaringType.FullName.StartsWith("IronPython.")) {
                    continue;
                }

                result += FrameToString(frame) + Environment.NewLine;
            }
            return result;
        }

        private string FrameToString(DynamicStackFrame frame) {
            string methodName = frame.GetMethodName();
            int lineNumber = frame.GetFileLineNumber();

            return String.Format("  File {0}, line {1}, in {2}",
                frame.GetFileName(),
                lineNumber == 0 ? "unknown" : lineNumber.ToString(),
                methodName);
        }

        private string FormatException(Exception exception, object pythonException) {
            Debug.Assert(pythonException != null);
            Debug.Assert(exception != null);

            string result = string.Empty;
            bool printedHeader = false;
            result += FormatStackTraces(exception, ref printedHeader);
            result += FormatPythonException(pythonException);
            if (Options.ShowClrExceptions) {
                result += FormatCLSException(exception);
            }

            return result;
        }
#endif

        #endregion

        public static PythonContext/*!*/ GetContext(CodeContext/*!*/ context) {
            Debug.Assert(context != null);

            PythonContext result;
            if (((result = context.LanguageContext as PythonContext) == null)) {
                result = context.LanguageContext.DomainManager.GetLanguageContext<PythonContext>();
            }

            return result;
        }

        public override ServiceType GetService<ServiceType>(params object[] args) {
            if (typeof(ServiceType) == typeof(CommandLine)) {
                return (ServiceType)(object)new PythonCommandLine(this);
            } else if (typeof(ServiceType) == typeof(OptionsParser)) {
                return (ServiceType)(object)new PythonOptionsParser(this);
            } else if (typeof(ServiceType) == typeof(TokenCategorizer)) {
                return (ServiceType)(object)new PythonTokenCategorizer();
            }

            return base.GetService<ServiceType>(args);
        }

        public static PythonEngineOptions GetPythonOptions(CodeContext context) {
            return DefaultContext.DefaultPythonContext._engineOptions;
        }

        public void AddToPath(string directory) {
            List path;
            if (TryGetSystemPath(out path)) {
                path.append(directory);
            }
        }

        public PythonCompilerOptions GetPythonCompilerOptions() {
            return new PythonCompilerOptions(PythonOptions.DivisionOptions == PythonDivisionOptions.New);
        }
        
        public override CompilerOptions GetCompilerOptions() {
            return GetPythonCompilerOptions();
        }

        public override CompilerOptions/*!*/ GetCompilerOptions(Scope/*!*/ scope) {
            Assert.NotNull(scope);

            PythonModule module = GetPythonModule(scope);

            PythonCompilerOptions res = new PythonCompilerOptions();
            if (module != null) {
                res.TrueDivision = module.TrueDivision;
                res.AllowWithStatement = module.AllowWithStatement;
                res.AbsoluteImports = module.AbsoluteImports;
            } else {
                res.TrueDivision = PythonOptions.DivisionOptions == PythonDivisionOptions.New;
            }

            return res;
        }

        public override void GetExceptionMessage(Exception exception, out string message, out string typeName) {
            object pythonEx = PythonExceptions.ToPython(exception);

            message = FormatPythonException(PythonExceptions.ToPython(exception));
            typeName = GetPythonExceptionClassName(pythonEx);
        }

        /// <summary>
        /// Gets or sets the default encoding for this system state / engine.
        /// </summary>
        public Encoding DefaultEncoding {
            get { return _defaultEncoding; }
            set { _defaultEncoding = value; }
        }

        public string GetDefaultEncodingName() {
            return DefaultEncoding.WebName.ToLower().Replace('-', '_');
        }

        /// <summary>
        /// Dictionary from name to type of all known built-in module names.
        /// </summary>
        internal Dictionary<string, Type> Builtins {
            get {
                return _builtinsDict;
            }
        }

        /// <summary>
        /// Dictionary from type to name of all built-in modules.
        /// </summary>
        public Dictionary<Type, string> BuiltinModuleNames {
            get {
                return _builtinModuleNames;
            }
        }

        private void InitializeBuiltins() {
            // create the __builtin__ module
            PythonDictionary dict = new PythonDictionary(new ModuleDictionaryStorage(typeof(Builtin)));

            Builtin.PerformModuleReload(this, dict);

            //IronPython.Runtime.Types.PythonModuleOps.PopulateModuleDictionary(this, dict, type);
            Scope builtinModule = CreateModule("__builtin__", null, new Scope(dict), null, ModuleOptions.NoBuiltins).Scope;

            _modulesDict["__builtin__"] = builtinModule;
        }

        private Dictionary<string, Type> CreateBuiltinTable() {
            Dictionary<string, Type> builtinTable = new Dictionary<string, Type>();

            // We should register builtins, if any, from IronPython.dll
            LoadBuiltins(builtinTable, typeof(PythonContext).Assembly);

            // Load builtins from IronPython.Modules
            Assembly ironPythonModules = DomainManager.Platform.LoadAssembly(GetIronPythonAssembly("IronPython.Modules"));
            LoadBuiltins(builtinTable, ironPythonModules);

            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                // we make our nt package show up as a posix package
                // on unix platforms.  Because we build on top of the 
                // CLI for all file operations we should be good from
                // there, but modules that check for the presence of
                // names (e.g. os) will do the right thing.
                Debug.Assert(builtinTable.ContainsKey("nt"));
                builtinTable["posix"] = builtinTable["nt"];
                builtinTable.Remove("nt");
            }

            return builtinTable;
        }

        private void LoadBuiltins(Dictionary<string, Type> builtinTable, Assembly assem) {
            object[] attrs = assem.GetCustomAttributes(typeof(PythonModuleAttribute), false);
            if (attrs.Length > 0) {
                foreach (PythonModuleAttribute pma in attrs) {
                    builtinTable[pma.Name] = pma.Type;
                    BuiltinModuleNames[pma.Type] = pma.Name;
                }
            }
        }

        public static string GetIronPythonAssembly(string baseName) {
#if SIGNED
            return baseName + ", Version=" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35";
#else
        return baseName;
#endif
        }
        
        /// <summary>
        /// TODO: Remove me, or stop caching built-ins.  This is broken if the user changes __builtin__
        /// </summary>
        public Scope BuiltinModuleInstance {
            get {
                lock (this) {
                    Scope res = _builtins;
                    if (res == null) {
                        res = _builtins = (Scope)SystemStateModules["__builtin__"];
                        _builtins.ModuleChanged += new EventHandler<ModuleChangeEventArgs>(BuiltinsChanged);
                    }
                    return res;
                }
            }
        }

        internal PythonModule CreateBuiltinModule(string name) {
            Type type;
            if (Builtins.TryGetValue(name, out type)) {
                // RuntimeHelpers.RunClassConstructor
                // run the type's .cctor before doing any custom reflection on the type.
                // This allows modules to lazily initialize PythonType's to custom values
                // rather than having them get populated w/ the ReflectedType.  W/o this the
                // cctor runs after we've done a bunch of reflection over the type that doesn't
                // force the cctor to run.
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                return CreateBuiltinModule(name, type);
            }

            return null;
        }

        private void BuiltinsChanged(object sender, ModuleChangeEventArgs e) {
            ModuleGlobalCache mgc;
            lock (_builtinCache) {
                if (_builtinCache.TryGetValue(e.Name, out mgc)) {
                    switch (e.ChangeType) {
                        case ModuleChangeType.Delete: mgc.Value = Uninitialized.Instance; break;
                        case ModuleChangeType.Set: mgc.Value = e.Value; break;
                    }
                } else {
                    // shouldn't be able to delete before it was set
                    object value = e.ChangeType == ModuleChangeType.Set ? e.Value : Uninitialized.Instance;
                    _builtinCache[e.Name] = new ModuleGlobalCache(value);
                }
            }
        }

        internal bool TryGetModuleGlobalCache(SymbolId name, out ModuleGlobalCache cache) {
            lock (_builtinCache) {
                if (!_builtinCache.TryGetValue(name, out cache)) {
                    // only cache values currently in built-ins, everything else will have
                    // no caching policy and will fall back to the LanguageContext.
                    object value;
                    if (BuiltinModuleInstance.TryGetName(name, out value)) {
                        _builtinCache[name] = cache = new ModuleGlobalCache(value);
                    }
                }
            }
            return cache != null;
        }

        public void SetHostVariables(string prefix, string executable, string versionString) {
            _initialVersionString = versionString;
            _initialExecutable = executable ?? "";
            _initialPrefix = prefix;

            SetHostVariables(SystemState.Dict);
        }

        internal void SetHostVariables(IAttributesCollection dict) {
            dict[SymbolTable.StringToId("executable")] = _initialExecutable;
            dict[SymbolTable.StringToId("exec_prefix")] = SystemState.Dict[SymbolTable.StringToId("prefix")] = _initialPrefix;
            SetVersionVariables(dict, 2, 5, 0, "release", _initialVersionString);
        }

        private void SetVersionVariables(IAttributesCollection dict, byte major, byte minor, byte build, string level, string versionString) {
            dict[SymbolTable.StringToId("hexversion")] = ((int)major << 24) + ((int)minor << 16) + ((int)build << 8);
            dict[SymbolTable.StringToId("version_info")] = PythonTuple.MakeTuple((int)major, (int)minor, (int)build, level, 0);
            dict[SymbolTable.StringToId("version")] = String.Format("{0}.{1}.{2} ({3})", major, minor, build, versionString);
        }

        internal object GetSystemStateValue(string name) {
            object val;
            if (SystemState.Dict.TryGetValue(SymbolTable.StringToId(name), out val)) {
                return val;
            }
            return null;
        }

        private void SetSystemStateValue(string name, object value) {
            SystemState.Dict[SymbolTable.StringToId(name)] = value;
        }

        private void SetStandardIO() {
            SharedIO io = DomainManager.SharedIO;
            
            PythonFile stdin = PythonFile.CreateConsole(this, io, ConsoleStreamType.Input, "<stdin>");
            PythonFile stdout = PythonFile.CreateConsole(this, io, ConsoleStreamType.Output, "<stdout>");
            PythonFile stderr = PythonFile.CreateConsole(this, io, ConsoleStreamType.ErrorOutput, "<stderr>");

            SetSystemStateValue("__stdin__", stdin);
            SetSystemStateValue("stdin", stdin);

            SetSystemStateValue("__stdout__", stdout);
            SetSystemStateValue("stdout", stdout);

            SetSystemStateValue("__stderr__", stderr);
            SetSystemStateValue("stderr", stderr);
        }

        internal PythonFileManager/*!*/ FileManager {
            get {
                return _fileManager;
            }
        }

        protected override int ExecuteProgram(SourceUnit/*!*/ program) {
            try {
                program.Execute(GetCompilerOptions(), ErrorSink.Default);
            } catch (SystemExitException e) {
                object obj;
                return e.GetExitCode(out obj);
            }

            return 0;
        }

        /// <summary> Dictionary of error handlers for string codecs. </summary>
        internal Dictionary<string, object> ErrorHandlers {
            get {
                return _errorHandlers;
            }
        }

        /// <summary> Table of functions used for looking for additional codecs. </summary>
        internal List<object> SearchFunctions {
            get {
                return _searchFunctions;
            }
        }
    }
}
