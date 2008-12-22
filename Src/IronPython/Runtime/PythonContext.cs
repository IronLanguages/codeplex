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

using System; using Microsoft;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting;
using System.IO;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Security;
using System.Text;
using System.Threading;
using IronPython.Compiler;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpretation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using PyAst = IronPython.Compiler.Ast;
using System.Globalization;

namespace IronPython.Runtime {
    public delegate void CommandDispatcher(Delegate command);

    public sealed class PythonContext : LanguageContext {
        internal const string/*!*/ IronPythonDisplayName = "IronPython 2.0 Beta";
        internal const string/*!*/ IronPythonNames = "IronPython;Python;py";
        internal const string/*!*/ IronPythonFileExtensions = ".py";

        private static readonly Guid PythonLanguageGuid = new Guid("03ed4b80-d10b-442f-ad9a-47dae85b2051");
        private static readonly Guid LanguageVendor_Microsoft = new Guid(-1723120188, -6423, 0x11d2, 0x90, 0x3f, 0, 0xc0, 0x4f, 0xa3, 2, 0xa1);

        // fields used during startup
        private readonly IDictionary<object, object>/*!*/ _modulesDict = new PythonDictionary();
        private readonly Dictionary<SymbolId, ModuleGlobalCache>/*!*/ _builtinCache = new Dictionary<SymbolId, ModuleGlobalCache>();
        private readonly Dictionary<Type, string>/*!*/ _builtinModuleNames = new Dictionary<Type, string>();
        private readonly PythonOptions/*!*/ _options;
        private readonly Scope/*!*/ _systemState;
        private readonly Dictionary<string, Type>/*!*/ _builtinsDict;
        private readonly BinderState _defaultBinderState, _defaultClsBinderState;
        private readonly Dictionary<AttrKey, CallSite<Func<CallSite, object, CodeContext, object>>>/*!*/ _tryGetMemSites
            = new Dictionary<AttrKey, CallSite<Func<CallSite, object, CodeContext, object>>>();
        private Encoding _defaultEncoding = PythonAsciiEncoding.Instance;

        // conditional variables for silverlight/desktop CLR features
#if !SILVERLIGHT
        private static int _hookedAssemblyResolve;
#endif
        private Hosting.PythonService _pythonService;
        private string _initialExecutable, _initialPrefix = GetInitialPrefix();

        // other fields which might only be conditionally used
        private string _initialVersionString;
        private Scope _clrModule;
        private Scope _builtins;

        private PythonFileManager _fileManager;
        private Dictionary<string, object> _errorHandlers;
        private List<object> _searchFunctions;
        private Dictionary<object, object> _moduleState;
        /// <summary> stored for copy_reg module, used for reduce protocol </summary>
        internal BuiltinFunction NewObject;
        /// <summary> stored for copy_reg module, used for reduce protocol </summary>
        internal BuiltinFunction PythonReconstructor;
        private Dictionary<Type, object> _genericSiteStorage;

        private CallSite<Func<CallSite, CodeContext, object, object>>[] _newUnarySites;
        private CallSite<Func<CallSite, CodeContext, object, object, object, object>>[] _newTernarySites;

        private CallSite<Func<CallSite, object, object, int>> _compareSite;
        private Dictionary<AttrKey, CallSite<Func<CallSite, object, object, object>>> _setAttrSites;
        private Dictionary<AttrKey, CallSite<Func<CallSite, object, object>>> _deleteAttrSites;
        private CallSite<Func<CallSite, CodeContext, object, string, PythonTuple, IAttributesCollection, object>> _metaClassSite;
        private CallSite<Func<CallSite, CodeContext, object, string, object>> _writeSite;
        private CallSite<Func<CallSite, object, object, object>> _getIndexSite, _equalSite, _delIndexSite;
        private CallSite<Func<CallSite, CodeContext, object, IList<string>>> _memberNamesSite;
        private CallSite<Func<CallSite, object, IList<string>>> _getMemberNamesSite;
        private CallSite<Func<CallSite, CodeContext, object, object>> _finalizerSite;
        private CallSite<Func<CallSite, CodeContext, PythonFunction, object>> _functionCallSite;
        private CallSite<Func<CallSite, object, object, bool>> _greaterThanSite, _lessThanSite, _equalRetBoolSite, _greaterThanEqualSite, _lessThanEqualSite;
        private CallSite<Func<CallSite, CodeContext, object, object[], object>> _callSplatSite;
        private CallSite<Func<CallSite, CodeContext, object, object[], IAttributesCollection, object>> _callDictSite;
        private CallSite<Func<CallSite, CodeContext, object, string, IAttributesCollection, IAttributesCollection, PythonTuple, int, object>> _importSite;
        private CallSite<Func<CallSite, CodeContext, object, string, IAttributesCollection, IAttributesCollection, PythonTuple, object>> _oldImportSite;
        private CallSite<Func<CallSite, object, bool>> _isCallableSite;
        private CallSite<Func<CallSite, object, object, object>> _addSite, _divModSite, _rdivModSite;
        private CallSite<Func<CallSite, object, object, object, object>> _setIndexSite, _delSliceSite;
        private CallSite<Func<CallSite, object, object, object, object, object>> _setSliceSite;
        private CallSite<Func<CallSite, object, string>> _docSite;

        // conversion sites
        private CallSite<Func<CallSite, object, int>> _intSite;
        private CallSite<Func<CallSite, object, string>> _tryStringSite;
        private CallSite<Func<CallSite, object, object>> _tryIntSite, _hashSite;
        private CallSite<Func<CallSite, object, IEnumerable>> _tryIEnumerableSite;
        private Dictionary<Type, CallSite<Func<CallSite, object, object>>> _implicitConvertSites;
        private Dictionary<string, CallSite<Func<CallSite, object, object, object>>> _binarySites;
        private Dictionary<Type, DefaultPythonComparer> _defaultComparer;
        private CallSite<Func<CallSite, CodeContext, object, object, object, int>> _sharedFunctionCompareSite;
        private CallSite<Func<CallSite, CodeContext, PythonFunction, object, object, int>> _sharedPythonFunctionCompareSite;
        private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object, int>> _sharedBuiltinFunctionCompareSite;

        private CallSite<Func<CallSite, CodeContext, object, object, object>> _propGetSite, _propDelSite;
        private CallSite<Func<CallSite, CodeContext, object, object, object, object>> _propSetSite;
        private CompiledLoader _compiledLoader;
        internal bool _importWarningThrows;
        private CommandDispatcher _commandDispatcher; // can be null
        private ClrModule.ReferencesList _referencesList;
        private string _floatFormat, _doubleFormat;
        private CultureInfo _collateCulture, _ctypeCulture, _timeCulture, _monetaryCulture, _numericCulture;

        /// <summary>
        /// Creates a new PythonContext not bound to Engine.
        /// </summary>
        public PythonContext(ScriptDomainManager/*!*/ manager, IDictionary<string, object> options)
            : base(manager) {
            _options = new PythonOptions(options);
            _builtinsDict = CreateBuiltinTable();

            DefaultContext.CreateContexts(manager, this);

            CodeContext defaultCtx = new CodeContext(new Scope(), this);
            PythonBinder binder = new PythonBinder(manager, this, defaultCtx);
            Binder = binder;
            _defaultBinderState = new BinderState(binder, DefaultContext.Default);

            DefaultContext.CreateClsContexts(manager, this);

            _defaultClsBinderState = new BinderState(binder, DefaultContext.DefaultCLS);

            // need to run PythonOps 1st so the type system is spun up...
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PythonOps).TypeHandle);

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
            SetSystemStateValue("argv", (_options.Arguments.Count == 0) ?
                new List(new object[] { String.Empty }) :
                new List(_options.Arguments)
            );

            if (_options.WarningFilters.Count > 0) {
                _systemState.Dict[SymbolTable.StringToId("warnoptions")] = new List(_options.WarningFilters);
            }

            List path = new List(_options.SearchPaths);
#if !SILVERLIGHT
            try {
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                // Can be null if called from unmanaged code (VS integration scenario)
                if (entryAssembly != null) {
                    string entry = Path.GetDirectoryName(entryAssembly.Location);
                    string lib = Path.Combine(entry, "Lib");
                    path.append(lib);

                    // add DLLs directory if it exists
                    string dlls = Path.Combine(entry, "DLLs");
                    if (Directory.Exists(dlls)) {
                        path.append(dlls);
                    }
                }
            } catch (SecurityException) {
            }
#endif

            _systemState.Dict[SymbolTable.StringToId("path")] = path;

            PythonFunction.SetRecursionLimit(_options.RecursionLimit);

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

            _collateCulture = _ctypeCulture = _timeCulture = _monetaryCulture = _numericCulture = CultureInfo.InvariantCulture;
        }

        public override LanguageOptions/*!*/ Options {
            get { return PythonOptions; }
        }

        /// <summary>
        /// Checks to see if module state has the current value stored already.
        /// </summary>
        public bool HasModuleState(object key) {
            EnsureModuleState();

            lock (_moduleState) {
                return _moduleState.ContainsKey(key);
            }
        }

        private void EnsureModuleState() {
            if (_moduleState == null) {
                Interlocked.CompareExchange(ref _moduleState, new Dictionary<object, object>(), null);
            }
        }

        /// <summary>
        /// Gets per-runtime state used by a module.  The module should have a unique key for
        /// each piece of state it needs to store.
        /// </summary>
        public object GetModuleState(object key) {
            EnsureModuleState();

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
            EnsureModuleState();

            lock (_moduleState) {
                _moduleState[key] = value;
            }
        }

        /// <summary>
        /// Sets per-runtime state used by a module and returns the previous value.  The module
        /// should have a unique key for each piece of state it needs to store.
        /// </summary>
        public object GetSetModuleState(object key, object value) {
            EnsureModuleState();

            lock (_moduleState) {
                object result;
                _moduleState.TryGetValue(key, out result);
                _moduleState[key] = value;
                return result;
            }
        }

        /// <summary>
        /// Sets per-runtime state used by a module and returns the previous value.  The module
        /// should have a unique key for each piece of state it needs to store.
        /// </summary>
        private object GetOrCreateModuleState(object key, Func<object> value) {
            EnsureModuleState();

            lock (_moduleState) {
                object result;
                if (!_moduleState.TryGetValue(key, out result)) {
                    _moduleState[key] = result = value();
                }
                return result;
            }
        }

        internal PythonType EnsureModuleException(object key, IAttributesCollection dict, string name, string module) {
            return (PythonType)(dict[SymbolTable.StringToId(name)] = GetOrCreateModuleState(
                key,
                () => PythonExceptions.CreateSubType(this, PythonExceptions.Exception, name, module, "")
            ));
        }

        internal void EnsureModuleException(object key, PythonType baseType, IAttributesCollection dict, string name, string module) {
            dict[SymbolTable.StringToId(name)] = GetOrCreateModuleState(
                key,
                () => PythonExceptions.CreateSubType(this, baseType, name, module, "")
            );
        }

        internal PythonOptions/*!*/ PythonOptions {
            get {
                return _options;
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

        internal bool TryGetSystemPath(out List path) {
            object val;
            if (SystemState.Dict.TryGetValue(SymbolTable.StringToId("path"), out val)) {
                path = val as List;
            } else {
                path = null;
            }

            return path != null;
        }

        internal object SystemStandardOut {
            get {
                return GetSystemStateValue("stdout");
            }
        }

        internal object SystemStandardIn {
            get {
                return GetSystemStateValue("stdin");
            }
        }

        internal object SystemStandardError {
            get {
                return GetSystemStateValue("stderr");
            }
        }

        internal IDictionary<object, object> SystemStateModules {
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

        internal object SystemExceptionValue {
            set {
                SetSystemStateValue("exc_value", value);
            }
        }

        internal object SystemExceptionTraceBack {
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

        public override Version LanguageVersion {
            get {
                // Assembly.GetName() can't be called in Silverlight...
                return GetPythonVersion();
            }
        }

        internal static Version GetPythonVersion() {
            return new AssemblyName(typeof(PythonContext).Assembly.FullName).Version;
        }

        internal string FloatFormat {
            get {
                return _floatFormat;
            }
            set {
                _floatFormat = value;
            }
        }

        internal string DoubleFormat {
            get {
                return _doubleFormat;
            }
            set {
                _doubleFormat = value;
            }
        }

        /// <summary>
        /// Initializes the sys module on startup.  Called both to load and reload sys
        /// </summary>
        private void InitializeSystemState() {
            // These fields do not get reset on "reload(sys)", we populate them once on startup
            SetSystemStateValue("argv", List.FromArrayNoCopy(new object[] { String.Empty }));
            SetSystemStateValue("modules", _modulesDict);

            _modulesDict["sys"] = _systemState;

            SetSystemStateValue("path", new List(3));
            SetSystemStateValue("ps1", ">>> ");
            SetSystemStateValue("ps2", "... ");

            SetStandardIO();

            SystemExceptionType = SystemExceptionValue = SystemExceptionTraceBack = null;

            SysModule.PerformModuleReload(this, _systemState.Dict);
        }

        internal LambdaExpression ParseSourceCode(SourceUnit/*!*/ sourceUnit, PythonCompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink, out bool disableInterpreter) {
            Assert.NotNull(sourceUnit, options, errorSink);

            PyAst.PythonAst ast;
            ScriptCodeParseResult properties = ScriptCodeParseResult.Complete;
            bool propertiesSet = false;
            int errorCode = 0;

            CompilerContext context = new CompilerContext(sourceUnit, options, errorSink);
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
                properties = ScriptCodeParseResult.Invalid;
            }

            context.SourceUnit.CodeProperties = properties;

            if (errorCode != 0 || properties == ScriptCodeParseResult.Empty) {
                disableInterpreter = false;
                return null;
            }

            // TODO: remove when the module is generated by PythonAst.Transform:
            options.TrueDivision = ast.TrueDivision;
            options.AllowWithStatement = ast.AllowWithStatement;
            options.AbsoluteImports = ast.AbsoluteImports;
            if (options.ModuleName == null) {
#if !SILVERLIGHT
                if (context.SourceUnit.HasPath && context.SourceUnit.Path.IndexOfAny(Path.GetInvalidFileNameChars()) == -1) {
                    options.ModuleName = Path.GetFileNameWithoutExtension(context.SourceUnit.Path);
#else
                if (context.SourceUnit.HasPath) {                    
                    options.ModuleName = context.SourceUnit.Path;
#endif
                } else {
                    options.ModuleName = "<module>";
                }
            }

            PyAst.PythonNameBinder.BindAst(ast, context);
            
            LambdaExpression res = ast.TransformToAst(context);
            disableInterpreter = ast.DisableInterpreter;
#if DEBUG && !SILVERLIGHT
            if (disableInterpreter) {
                try {
                    // we don't force compilation in SaveAssemblies mode as it slows us down too much...
                    // we also don't force compliation on large source files - this is for test_compile
                    // where the compiler stack overflows when interpreting.  DLR will switch to a non-recurisve
                    // strategy for walking the trees in the future and this 2nd check should go away then.
                    if (Environment.GetEnvironmentVariable("DLR_SaveAssemblies") != null ||  
                        sourceUnit.GetCode().Length > 50000) {
                        disableInterpreter = false;
                    }
                } catch (SecurityException) {
                }
            }
#endif
            return res;
        }

        internal ScriptCode CompileSourceCode(SourceUnit/*!*/ sourceUnit, CompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink, bool interpret) {
            var pythonOptions = (PythonCompilerOptions)options;

            if (sourceUnit.Kind == SourceCodeKind.File) {
                pythonOptions.Module |= ModuleOptions.Initialize;
            }

            bool disableInterpreter;
            var lambda = ParseSourceCode(sourceUnit, pythonOptions, errorSink, out disableInterpreter);

            if (lambda == null) {
                return null;
            }

            if (interpret && !disableInterpreter) { // TODO: enable -X:Interpret flag: || _options.InterpretedMode
                // TODO: fix generated DLR ASTs
                lambda = new GlobalLookupRewriter().RewriteLambda(lambda);
                return new InterpretedScriptCode(lambda, sourceUnit);
            } else if ((pythonOptions.Module & ModuleOptions.Optimized) != 0) {
                return new OptimizedScriptCode(lambda, sourceUnit);
            } else {
                // TODO: fix generated DLR ASTs
                lambda = new GlobalLookupRewriter().RewriteLambda(lambda);
                return new ScriptCode(lambda, sourceUnit);
            }
        }        

        protected override ScriptCode CompileSourceCode(SourceUnit/*!*/ sourceUnit, CompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink) {
            return CompileSourceCode(sourceUnit, options, errorSink, false);
        }

        protected override ScriptCode/*!*/ LoadCompiledCode(DlrMainCallTarget/*!*/ method, string path) {            
            SourceUnit su = new SourceUnit(this, NullTextContentProvider.Null, path, SourceCodeKind.File);
            return new OnDiskScriptCode(method, su);
        }

        public override SourceCodeReader/*!*/ GetSourceReader(Stream/*!*/ stream, Encoding/*!*/ defaultEncoding) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(defaultEncoding, "defaultEncoding");
            ContractUtils.Requires(stream.CanSeek && stream.CanRead, "stream", "The stream must support seeking and reading");

            // we choose ASCII by default, if the file has a Unicode pheader though
            // we'll automatically get it as unicode.
            Encoding encoding = PythonAsciiEncoding.Instance;

            long startPosition = stream.Position;

            StreamReader sr = new StreamReader(stream, PythonAsciiEncoding.Instance);

            int bytesRead = 0;
            string line;
            line = ReadOneLine(sr, ref bytesRead);

            //string line = sr.ReadLine();
            bool gotEncoding = false;

            // magic encoding must be on line 1 or 2
            if (line != null && !(gotEncoding = Tokenizer.TryGetEncoding(defaultEncoding, line, ref encoding))) {
                line = ReadOneLine(sr, ref bytesRead);

                if (line != null) {
                    gotEncoding = Tokenizer.TryGetEncoding(defaultEncoding, line, ref encoding);
                }
            }

            if (gotEncoding && sr.CurrentEncoding != PythonAsciiEncoding.Instance && encoding != sr.CurrentEncoding) {
                // we have both a BOM & an encoding type, throw an error
                throw new IOException("file has both Unicode marker and PEP-263 file encoding");
            }

            if (encoding == null) {
                throw new IOException("unknown encoding type");
            }

            if (!gotEncoding) {
                // if we didn't get an encoding seek back to the beginning...
                stream.Seek(startPosition, SeekOrigin.Begin);
            } else {
                // if we got an encoding seek to the # of bytes we read (so the StreamReader's
                // buffering doesn't throw us off)
                stream.Seek(bytesRead, SeekOrigin.Begin);
            }

            // re-read w/ the correct encoding type...
            return new SourceCodeReader(new StreamReader(stream, encoding), encoding);
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
                                totalRead -= (bytesRead - (i + 2));   // skip cr/lf
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
            return new IronPython.Hosting.PythonCodeDomCodeGen().GenerateCode((System.CodeDom.CodeMemberMethod)codeDom, this, path, kind);
        }
#endif

        #region Scopes

        public override Scope GetScope(string/*!*/ path) {
            PythonModule module = GetModuleByPath(path);
            return (module != null) ? module.Scope : null;
        }

        internal PythonModule GetPythonModule(Scope scope) {
            return (PythonModule)scope.GetExtension(ContextId);
        }

        internal PythonModule EnsurePythonModule(Scope scope) {
            return (PythonModule)EnsureScopeExtension(scope);
        }

        public override ScopeExtension CreateScopeExtension(Scope scope) {
            return CreatePythonModule(null, scope, ModuleOptions.None);
        }

        internal PythonModule/*!*/ CompileAndInitializeModule(string moduleName, string fileName, SourceUnit sourceUnit) {
            ScriptCode compiledCode;
            return CompileModule(fileName, moduleName, sourceUnit, ModuleOptions.Initialize, out compiledCode);
        }

        /// <summary>
        /// Compiles the code stored in the specified filename with the given module name and options.  Returns the PythonModule
        /// instance and the ScriptCode to be run.
        /// </summary>
        internal PythonModule/*!*/ CompileModule(string fileName, string moduleName, ModuleOptions options, out ScriptCode compiledCode) {
            SourceUnit sourceCode = CreateFileUnit(String.IsNullOrEmpty(fileName) ? null : fileName, DefaultEncoding);

            return CompileModule(fileName, moduleName, sourceCode, options, out compiledCode);
        }

        internal PythonModule/*!*/ CompileModule(string fileName, string moduleName, SourceUnit sourceCode, ModuleOptions options) {
            ScriptCode compiledCode;
            return CompileModule(fileName, moduleName, sourceCode, options, out compiledCode);
        }

        internal PythonModule/*!*/ CompileModule(string fileName, string moduleName, SourceUnit sourceCode, ModuleOptions options, out ScriptCode scriptCode) {
            ContractUtils.RequiresNotNull(fileName, "fileName");
            ContractUtils.RequiresNotNull(moduleName, "moduleName");
            ContractUtils.RequiresNotNull(sourceCode, "sourceCode");

            scriptCode = GetScriptCode(sourceCode, moduleName, options);
            Scope scope = scriptCode.CreateScope();
            scope.SetExtension(ContextId, CreatePythonModule(fileName, scope, options));
            return CreateModule(fileName, scope, scriptCode, options);
        }

        internal ScriptCode GetScriptCode(SourceUnit sourceCode, string moduleName, ModuleOptions options) {
            ScriptCode compiledCode;
            PythonCompilerOptions compilerOptions = GetPythonCompilerOptions();
            compilerOptions.SkipFirstLine = (options & ModuleOptions.SkipFirstLine) != 0;
            compilerOptions.ModuleName = moduleName;

            options |= ModuleOptions.Optimized;     // Below we always generate optimized scope.
            compilerOptions.Module = options;

            compiledCode = sourceCode.Compile(compilerOptions, ThrowingErrorSink.Default);
            return compiledCode;
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

        internal PythonModule/*!*/ CreateBuiltinModule(string moduleName, Type type) {
            return CreateBuiltinModule(moduleName, type, ModuleOptions.NoBuiltins);
        }

        internal PythonModule/*!*/ CreateBuiltinModule(string moduleName, Type type, ModuleOptions options) {
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

            PythonModule mod = CreateModule(null, new Scope(dict), null, options);
            mod.Scope.SetName(Symbols.Name, moduleName);
            return mod;
        }

        public PythonModule/*!*/ CreateModule() {
            return CreateModule(ModuleOptions.None);
        }

        public PythonModule/*!*/ CreateModule(ModuleOptions options) {
            return CreateModule(null, new Scope(PythonDictionary.MakeSymbolDictionary()), null, options);
        }

        public PythonModule/*!*/ CreateModule(string fileName, Scope scope, ScriptCode scriptCode, ModuleOptions options) {
            if (scope == null) {
                scope = new Scope(PythonDictionary.MakeSymbolDictionary());
            }

            PythonModule module = CreatePythonModule(fileName, scope, options);
            module.ShowCls = (options & ModuleOptions.ShowClsMethods) != 0;            
            module.TrueDivision = (options & ModuleOptions.TrueDivision) != 0;
            module.AllowWithStatement = (options & ModuleOptions.WithStatement) != 0;
            module.AbsoluteImports = (options & ModuleOptions.AbsoluteImports) != 0;
            module.PrintFunction = (options & ModuleOptions.PrintFunction) != 0;
            
            module.IsPythonCreatedModule = true;

            if ((options & ModuleOptions.Initialize) != 0) {
                scriptCode.Run(module.Scope);
            }

            return module;
        }

        private PythonModule/*!*/ CreatePythonModule(string fileName, Scope/*!*/ scope, ModuleOptions options) {
            ContractUtils.RequiresNotNull(scope, "scope");

            PythonModule module = new PythonModule(scope);
            module.BinderState = new BinderState(Binder);
            module = (PythonModule)scope.SetExtension(ContextId, module);

            // adds __builtin__ variable if necessary.  Python adds the module directly to
            // __main__ and __builtin__'s dictionary for all other modules.  Our callers
            // pass the appropriate flags to control this behavior.
            if ((options & ModuleOptions.NoBuiltins) == 0 && !scope.ContainsName(Symbols.Builtins)) {
                if ((options & ModuleOptions.ModuleBuiltins) != 0) {
                    module.Scope.SetName(Symbols.Builtins, BuiltinModuleInstance);
                } else {
                    module.Scope.SetName(Symbols.Builtins, BuiltinModuleInstance.Dict);
                }
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
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(module, "module");
            SystemStateModules[name] = module.Scope;
        }

        internal PythonModule GetReloadableModule(Scope/*!*/ scope) {
            Assert.NotNull(scope);

            PythonModule module = (PythonModule)scope.GetExtension(ContextId);

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
        public override bool TryLookupGlobal(Scope scope, SymbolId name, out object value) {
            object builtins;
            if (!scope.ModuleScope.TryGetName(Symbols.Builtins, out builtins)) {
                value = null;
                return false;
            }

            Scope builtinsScope = builtins as Scope;
            if (builtinsScope != null && builtinsScope.TryGetName(name, out value)) return true;

            IAttributesCollection dict = builtins as IAttributesCollection;
            if (dict != null && dict.TryGetValue(name, out value)) return true;

            return base.TryLookupGlobal(scope, name, out value);
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

        #region Assembly Loading

        public override Assembly LoadAssemblyFromFile(string file) {
#if !SILVERLIGHT
            // check all files in the path...
            List path;
            if (TryGetSystemPath(out path)) {
                IEnumerator ie = PythonOps.GetEnumerator(path);
                while (ie.MoveNext()) {
                    string str;
                    if (TryConvertToString(ie.Current, out str)) {
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
            if (File.Exists(path) && Path.IsPathRooted(path)) {
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

        public override ICollection<string> GetSearchPaths() {
            List<string> result = new List<string>();
            List paths;
            if (TryGetSystemPath(out paths)) {
                IEnumerator ie = PythonOps.GetEnumerator(paths);
                while (ie.MoveNext()) {
                    string str;
                    if (TryConvertToString(ie.Current, out str)) {
                        result.Add(str);
                    }
                }
            }
            return result;
        }

        public override void SetSearchPaths(ICollection<string> paths) {
            SetSystemStateValue("path", new List(paths));
        }

        public override void Shutdown() {
            object callable;

            try {
                if (_systemState.TryGetName(Symbols.SysExitFunc, out callable)) {
                    PythonCalls.Call(new CodeContext(new Scope(), this), callable);
                }
            } finally {
                if (PythonOptions.PerfStats) {
                    PerfTrack.DumpStats();
                }
            }
        }

        // TODO: ExceptionFormatter service
        #region Stack Traces and Exceptions

        public override string FormatException(Exception exception) {
            ContractUtils.RequiresNotNull(exception, "exception");

            SyntaxErrorException syntax_error = exception as SyntaxErrorException;
            if (syntax_error != null) {
                return FormatPythonSyntaxError(syntax_error);
            }

            object pythonEx = PythonExceptions.ToPython(exception);

            string result = FormatStackTraces(exception) + FormatPythonException(pythonEx);

            if (Options.ShowClrExceptions) {
                result += Environment.NewLine;
                result += FormatCLSException(exception);
            }

            return result;
        }

        internal static string FormatPythonSyntaxError(SyntaxErrorException e) {
            string sourceLine = GetSourceLine(e);

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

        internal static string GetSourceLine(SyntaxErrorException e) {
            if (e.SourceCode == null) {
                return null;
            }
            try {
                using (StringReader reader = new StringReader(e.SourceCode)) {
                    char[] buffer = new char[80];
                    int curLine = 1;
                    StringBuilder line = new StringBuilder();
                    int bytesRead;

                    // we can't use SourceUnit.GetCodeLines because Python includes the new lines
                    // in the syntax error and the codeop standard library depends upon this
                    // being correct
                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0 && curLine <= e.Line) {
                        for (int i = 0; i < bytesRead; i++) {
                            if (curLine == e.Line) {
                                line.Append(buffer[i]);
                            }

                            if (buffer[i] == '\n') {
                                curLine++;
                            }

                            if (curLine > e.Line) {
                                break;
                            }
                        }
                    }

                    return line.ToString();
                }
            } catch (IOException) {
                return null;
            }
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
                    result += GetPythonExceptionClassName(pythonException);

                    string excepStr = PythonOps.ToString(pythonException);

                    if (!String.IsNullOrEmpty(excepStr)) {
                        result += ": " + excepStr;
                    }
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
            DynamicStackFrame[] dfs = ScriptingRuntimeHelpers.GetDynamicStackFrames(e);
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

            return String.Format("  File \"{0}\", line {1}, in {2}",
                frame.GetFileName(),
                lineNumber == 0 ? "unknown" : lineNumber.ToString(),
                methodName);
        }

#endif

        #endregion

        public static PythonContext/*!*/ GetContext(CodeContext/*!*/ context) {
            Debug.Assert(context != null);

            PythonContext result;
            if (((result = context.LanguageContext as PythonContext) == null)) {
                result = (PythonContext)context.LanguageContext.DomainManager.GetLanguage(typeof(PythonContext));
            }

            return result;
        }

        /// <summary>
        /// Gets Python module for the module scope the context holds on to.
        /// Returns null if there is no PythonModule associated with teh global scope.
        /// </summary>
        internal static PythonModule GetModule(CodeContext/*!*/ context) {
            return context.GlobalScope.GetExtension(context.LanguageContext.ContextId) as PythonModule;
        }

        /// <summary>
        /// Ensures the module scope is associated with a Python module and returns it.
        /// </summary>
        internal static PythonModule/*!*/ EnsureModule(CodeContext/*!*/ context) {
            return GetContext(context).EnsurePythonModule(context.GlobalScope);
        }

        public override TService GetService<TService>(params object[] args) {
            if (typeof(TService) == typeof(TokenizerService)) {
                return (TService)(object)new Tokenizer();
            }

            return base.GetService<TService>(args);
        }


        /// <summary>
        /// Returns (and creates if necessary) the PythonService that is associated with this PythonContext.
        /// 
        /// The PythonService is used for providing remoted convenience helpers for the DLR hosting APIs.
        /// </summary>
        internal Hosting.PythonService GetPythonService(Microsoft.Scripting.Hosting.ScriptEngine engine) {
            if (_pythonService == null) {
                Interlocked.CompareExchange(ref _pythonService, new Hosting.PythonService(this, engine), null);
            }

            return _pythonService;
        }

        internal static PythonOptions GetPythonOptions(CodeContext context) {
            return DefaultContext.DefaultPythonContext._options;
        }

        internal void InsertIntoPath(int index, string directory) {
            List path;
            if (TryGetSystemPath(out path)) {
                path.insert(index, directory);
            }
        }

        internal void AddToPath(string directory) {
            List path;
            if (TryGetSystemPath(out path)) {
                path.append(directory);
            }
        }

        internal PythonCompilerOptions GetPythonCompilerOptions() {
            PythonLanguageFeatures features = PythonLanguageFeatures.Default;

            if (PythonOptions.DivisionOptions == PythonDivisionOptions.New) {
                features |= PythonLanguageFeatures.TrueDivision;
            }

            if (PythonOptions.PythonVersion == new Version(2, 6)) {
                features |= PythonLanguageFeatures.Python26;
            }

            return new PythonCompilerOptions(features);
        }

        public override CompilerOptions GetCompilerOptions() {
            return GetPythonCompilerOptions();
        }

        public override CompilerOptions/*!*/ GetCompilerOptions(Scope/*!*/ scope) {
            Assert.NotNull(scope);

            PythonCompilerOptions res = GetPythonCompilerOptions();

            PythonModule module = GetPythonModule(scope);
            if (module != null) {
                res.LanguageFeatures |= module.LanguageFeatures;
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
        internal Dictionary<Type, string> BuiltinModuleNames {
            get {
                return _builtinModuleNames;
            }
        }

        private void InitializeBuiltins() {
            // create the __builtin__ module
            PythonDictionary dict = new PythonDictionary(new ModuleDictionaryStorage(typeof(Builtin)));

            Builtin.PerformModuleReload(this, dict);

            //IronPython.Runtime.Types.PythonModuleOps.PopulateModuleDictionary(this, dict, type);
            Scope builtinModule = CreateModule(null, new Scope(dict), null, ModuleOptions.NoBuiltins).Scope;
            builtinModule.SetName(Symbols.Name, "__builtin__");

            _modulesDict["__builtin__"] = builtinModule;
        }

        private Dictionary<string, Type> CreateBuiltinTable() {
            Dictionary<string, Type> builtinTable = new Dictionary<string, Type>();

            // We should register builtins, if any, from IronPython.dll
            LoadBuiltins(builtinTable, typeof(PythonContext).Assembly);

            // Load builtins from IronPython.Modules
            Assembly ironPythonModules = null;

            try {
                ironPythonModules = DomainManager.Platform.LoadAssembly(GetIronPythonAssembly("IronPython.Modules"));
            } catch (FileNotFoundException) {
                // IronPython.Modules is not available, continue without it...
            }

            if (ironPythonModules != null) {
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
            }

            return builtinTable;
        }

        internal void LoadBuiltins(Dictionary<string, Type> builtinTable, Assembly assem) {
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

#if DEBUG && !SILVERLIGHT
            try {
                Debug.Assert(Assembly.GetExecutingAssembly().GetName().Version.ToString() == "2.0.0.5000");
            } catch (SecurityException) {
            }
#endif

            return baseName + ", Version=2.0.0.5000, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
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
                        EnsurePythonModule(res).ModuleChanged += new EventHandler<ModuleChangeEventArgs>(BuiltinsChanged);
                    }
                    return res;
                }
            }
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

        internal void SetHostVariables(string prefix, string executable, string versionString) {
            _initialVersionString = versionString;
            _initialExecutable = executable ?? "";
            _initialPrefix = prefix;

            SetHostVariables(SystemState.Dict);
        }

        internal string InitialPrefix {
            get {
                return _initialPrefix;
            }
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

        private static string GetInitialPrefix() {
#if !SILVERLIGHT
            try {
                return typeof(PythonContext).Assembly.CodeBase;
            } catch (SecurityException) {
                // we don't have permissions to get paths...
                return String.Empty;
            }
#else
            return String.Empty;
#endif
        }

        /// <summary>
        /// Gets the member names associated with the object
        /// TODO: Move "GetMemberNames" functionality into MetaObject implementations
        /// </summary>
        protected override IList<string> GetMemberNames(object obj) {
            IList<string> result = base.GetMemberNames(obj);
            if (result.Count == 0) {
                result = GetMemberNamesSite.Target(GetMemberNamesSite, obj);
            }
            return result;
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

        internal PythonFileManager RawFileManager {
            get {
                return _fileManager;
            }
        }

        internal PythonFileManager/*!*/ FileManager {
            get {
                if (_fileManager == null) {
                    Interlocked.CompareExchange(ref _fileManager, new PythonFileManager(), null);
                }

                return _fileManager;
            }
        }

        public override int ExecuteProgram(SourceUnit/*!*/ program) {
            try {
                PythonCompilerOptions pco = (PythonCompilerOptions)GetCompilerOptions();
                pco.ModuleName = "__main__";
                pco.Module |= ModuleOptions.Initialize;

                program.Execute(pco, ErrorSink.Default);
            } catch (SystemExitException e) {
                object obj;
                return e.GetExitCode(out obj);
            }

            return 0;
        }

        /// <summary> Dictionary of error handlers for string codecs. </summary>
        internal Dictionary<string, object> ErrorHandlers {
            get {
                if (_errorHandlers == null) {
                    Interlocked.CompareExchange(ref _errorHandlers, new Dictionary<string, object>(), null);
                }

                return _errorHandlers;
            }
        }

        /// <summary> Table of functions used for looking for additional codecs. </summary>
        internal List<object> SearchFunctions {
            get {
                if (_searchFunctions == null) {
                    Interlocked.CompareExchange(ref _searchFunctions, new List<object>(), null);
                }

                return _searchFunctions;
            }
        }

        /// <summary>
        /// Gets a SiteLocalStorage when no call site is available.
        /// </summary>
        internal SiteLocalStorage<T> GetGenericSiteStorage<T>() {
            if (_genericSiteStorage == null) {
                Interlocked.CompareExchange(ref _genericSiteStorage, new Dictionary<Type, object>(), null);
            }

            lock (_genericSiteStorage) {
                object res;
                if (!_genericSiteStorage.TryGetValue(typeof(T), out res)) {
                    _genericSiteStorage[typeof(T)] = res = new SiteLocalStorage<T>();
                }
                return (SiteLocalStorage<T>)res;
            }
        }

        internal SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> GetGenericCallSiteStorage() {
            return GetGenericSiteStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>>();

        }

        internal SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], IAttributesCollection, object>>> GetGenericKeywordCallSiteStorage() {
            return GetGenericSiteStorage<CallSite<Func<CallSite, CodeContext, object, object[], IAttributesCollection, object>>>();

        }

        #region Object Operations

        public override ConvertBinder/*!*/ CreateConvertBinder(Type/*!*/ toType, bool explicitCast) {
            return new ConversionBinder(DefaultBinderState, toType, explicitCast ? ConversionResultKind.ExplicitCast : ConversionResultKind.ImplicitCast);
        }

        public override DeleteMemberBinder/*!*/ CreateDeleteMemberBinder(string/*!*/ name, bool ignoreCase) {
            return new PythonDeleteMemberBinder(DefaultBinderState, name, ignoreCase);
        }

        public override GetMemberBinder/*!*/ CreateGetMemberBinder(string/*!*/ name, bool ignoreCase) {
            return new CompatibilityGetMember(DefaultBinderState, name, ignoreCase);
        }

        public override InvokeBinder/*!*/ CreateInvokeBinder(params ArgumentInfo/*!*/[]/*!*/ arguments) {
            return new CompatibilityInvokeBinder(DefaultBinderState, arguments);
        }

        [Obsolete]
        public override OperationBinder/*!*/ CreateOperationBinder(string/*!*/ operation) {
            return new PythonOperationBinder(DefaultBinderState, operation);
        }

        public override SetMemberBinder/*!*/ CreateSetMemberBinder(string/*!*/ name, bool ignoreCase) {
            return new PythonSetMemberBinder(DefaultBinderState, name, ignoreCase);
        }

        public override CreateInstanceBinder/*!*/ CreateCreateBinder(params ArgumentInfo/*!*/[]/*!*/ arguments) {
            return new CreateFallback(
                new CompatibilityInvokeBinder(DefaultBinderState, arguments),
                arguments
            );
        }

        #endregion

        #region Per-Runtime Call Sites

        private bool InvokeOperatorWorker(CodeContext/*!*/ context, UnaryOperators oper, object target, out object result) {
            if (_newUnarySites == null) {
                Interlocked.CompareExchange(
                    ref _newUnarySites,
                    new CallSite<Func<CallSite, CodeContext, object, object>>[(int)UnaryOperators.Maximum],
                    null
                );
            }

            if (_newUnarySites[(int)oper] == null) {
                Interlocked.CompareExchange(
                    ref _newUnarySites[(int)oper],
                    CallSite<Func<CallSite, CodeContext, object, object>>.Create(
                        new PythonInvokeBinder(
                            DefaultBinderState,
                            new CallSignature(0)
                        )
                    ),
                    null
                );
            }
            CallSite<Func<CallSite, CodeContext, object, object>> site = _newUnarySites[(int)oper];

            SymbolId symbol = GetUnarySymbol(oper);
            PythonType pt = DynamicHelpers.GetPythonType(target);
            PythonTypeSlot pts;
            object callable;

            if (pt.TryResolveMixedSlot(context, symbol, out pts) &&
                pts.TryGetBoundValue(context, target, pt, out callable)) {

                result = site.Target(site, context, callable);
                return true;
            }

            result = null;
            return false;
        }

        private static SymbolId GetUnarySymbol(UnaryOperators oper) {
            SymbolId symbol;
            switch (oper) {
                case UnaryOperators.Repr: symbol = Symbols.Repr; break;
                case UnaryOperators.Length: symbol = Symbols.Length; break;
                case UnaryOperators.Hash: symbol = Symbols.Hash; break;
                case UnaryOperators.String: symbol = Symbols.String; break;
                default: throw new ArgumentException();
            }
            return symbol;
        }

        private bool InvokeOperatorWorker(CodeContext/*!*/ context, TernaryOperators oper, object target, object value1, object value2, out object result) {

            if (_newTernarySites == null) {
                Interlocked.CompareExchange(
                    ref _newTernarySites,
                    new CallSite<Func<CallSite, CodeContext, object, object, object, object>>[(int)TernaryOperators.Maximum],
                    null
                );
            }

            if (_newTernarySites[(int)oper] == null) {
                Interlocked.CompareExchange(
                    ref _newTernarySites[(int)oper],
                    CallSite<Func<CallSite, CodeContext, object, object, object, object>>.Create(
                        new PythonInvokeBinder(
                            DefaultBinderState,
                            new CallSignature(2)
                        )
                    ),
                    null
                );
            }
            CallSite<Func<CallSite, CodeContext, object, object, object, object>> site = _newTernarySites[(int)oper];

            SymbolId symbol = GetTernarySymbol(oper);
            PythonType pt = DynamicHelpers.GetPythonType(target);
            PythonTypeSlot pts;
            object callable;

            if (pt.TryResolveMixedSlot(context, symbol, out pts) &&
                pts.TryGetBoundValue(context, target, pt, out callable)) {

                result = site.Target(site, context, callable, value1, value2);
                return true;
            }

            result = null;
            return false;
        }

        private static SymbolId GetTernarySymbol(TernaryOperators oper) {
            SymbolId symbol;
            switch (oper) {
                case TernaryOperators.SetDescriptor: symbol = Symbols.SetDescriptor; break;
                case TernaryOperators.GetDescriptor: symbol = Symbols.GetDescriptor; break;
                default: throw new ArgumentException();
            }
            return symbol;
        }

        internal static object InvokeUnaryOperator(CodeContext/*!*/ context, UnaryOperators oper, object target, string errorMsg) {
            object res;
            if (PythonContext.GetContext(context).InvokeOperatorWorker(context, oper, target, out res)) {
                return res;
            }

            throw PythonOps.TypeError(errorMsg);
        }

        internal static object InvokeUnaryOperator(CodeContext/*!*/ context, UnaryOperators oper, object target) {
            object res;
            if (PythonContext.GetContext(context).InvokeOperatorWorker(context, oper, target, out res)) {
                return res;
            }

            throw PythonOps.TypeError(String.Empty);
        }

        internal static bool TryInvokeTernaryOperator(CodeContext/*!*/ context, TernaryOperators oper, object target, object value1, object value2, out object res) {
            return PythonContext.GetContext(context).InvokeOperatorWorker(context, oper, target, value1, value2, out res);
        }

        internal CallSite<Func<CallSite, object, object, int>> CompareSite {
            get {
                if (_compareSite == null) {
                    Interlocked.CompareExchange(ref _compareSite,
                        CallSite<Func<CallSite, object, object, int>>.Create(
                            new PythonOperationBinder(
                                DefaultBinderState,
                                StandardOperators.Compare
                            )
                        ),
                        null
                    );
                }

                return _compareSite;
            }
        }

        internal bool TryGetBoundAttr(CodeContext/*!*/ context, object o, SymbolId name, out object ret) {
            CallSite<Func<CallSite, object, CodeContext, object>> site = GetTryGetMemberSite(context, o, name);

            try {
                ret = site.Target(site, o, context);
            } catch (MissingMemberException) {
                ret = null;
                return false;
            }
            return ret != OperationFailed.Value;
        }

        internal object GetAttr(CodeContext/*!*/ context, object o, SymbolId name) {
            CallSite<Func<CallSite, object, CodeContext, object>> site = GetTryGetMemberSite(context, o, name);

            object ret = site.Target(site, o, context);

            if (ret == OperationFailed.Value) {
                if (o is OldClass) {
                    throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'",
                        ((OldClass)o).__name__, SymbolTable.IdToString(name));
                } else {
                    throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'", DynamicHelpers.GetPythonType(o).Name, SymbolTable.IdToString(name));
                }
            }
            return ret;
        }

        private CallSite<Func<CallSite, object, CodeContext, object>> GetTryGetMemberSite(CodeContext context, object o, SymbolId name) {
            AttrKey key = new AttrKey(CompilerHelpers.GetType(o), name, PythonOps.IsClsVisible(context));

            CallSite<Func<CallSite, object, CodeContext, object>> site;

            lock (_tryGetMemSites) {
                if (!_tryGetMemSites.TryGetValue(key, out site)) {
                    _tryGetMemSites[key] = site = CallSite<Func<CallSite, object, CodeContext, object>>.Create(
                        new PythonGetMemberBinder(
                            PythonOps.IsClsVisible(context) ? DefaultClsBinderState : DefaultBinderState,
                            SymbolTable.IdToString(name),
                            true
                        )
                    );
                }
            }
            return site;
        }

        internal void SetAttr(CodeContext/*!*/ context, object o, SymbolId name, object value) {
            CallSite<Func<CallSite, object, object, object>> site;
            if (_setAttrSites == null) {
                Interlocked.CompareExchange(ref _setAttrSites, new Dictionary<AttrKey, CallSite<Func<CallSite, object, object, object>>>(), null);
            }

            lock (_setAttrSites) {
                AttrKey key = new AttrKey(CompilerHelpers.GetType(o), name);
                if (!_setAttrSites.TryGetValue(key, out site)) {
                    _setAttrSites[key] = site = CallSite<Func<CallSite, object, object, object>>.Create(
                        new PythonSetMemberBinder(
                            DefaultBinderState,
                            SymbolTable.IdToString(name)
                        )
                    );
                }
            }

            site.Target.Invoke(site, o, value);
        }

        internal void DeleteAttr(CodeContext/*!*/ context, object o, SymbolId name) {
            AttrKey key = new AttrKey(CompilerHelpers.GetType(o), name);

            if (_deleteAttrSites == null) {
                Interlocked.CompareExchange(ref _deleteAttrSites, new Dictionary<AttrKey, CallSite<Func<CallSite, object, object>>>(), null);
            }

            CallSite<Func<CallSite, object, object>> site;
            lock (_deleteAttrSites) {
                if (!_deleteAttrSites.TryGetValue(key, out site)) {
                    _deleteAttrSites[key] = site = CallSite<Func<CallSite, object, object>>.Create(
                        new PythonDeleteMemberBinder(
                            DefaultBinderState,
                            SymbolTable.IdToString(name)
                        )
                    );
                }
            }

            site.Target(site, o);
        }

        internal CallSite<Func<CallSite, CodeContext, object, string, PythonTuple, IAttributesCollection, object>> MetaClassCallSite {
            get {
                if (_metaClassSite == null) {
                    Interlocked.CompareExchange(
                        ref _metaClassSite,
                        CallSite<Func<CallSite, CodeContext, object, string, PythonTuple, IAttributesCollection, object>>.Create(
                            new PythonInvokeBinder(
                                _defaultBinderState,
                                new CallSignature(3)
                            )
                        ),
                        null
                    );
                }

                return _metaClassSite;
            }
        }

        internal CallSite<Func<CallSite, CodeContext, object, string, object>> WriteCallSite {
            get {
                if (_writeSite == null) {
                    Interlocked.CompareExchange(
                        ref _writeSite,
                        CallSite<Func<CallSite, CodeContext, object, string, object>>.Create(
                            new PythonInvokeBinder(
                                _defaultBinderState,
                                new CallSignature(1)
                            )
                        ),
                        null
                    );
                }

                return _writeSite;
            }
        }

        internal CallSite<Func<CallSite, object, object, object>> GetIndexSite {
            get {
                if (_getIndexSite == null) {
                    Interlocked.CompareExchange(
                        ref _getIndexSite,
                        CallSite<Func<CallSite, object, object, object>>.Create(
                            new PythonOperationBinder(
                                _defaultBinderState,
                                StandardOperators.GetItem
                            )
                        ),
                        null
                    );
                }

                return _getIndexSite;
            }
        }

        internal void DelIndex(object target, object index) {
            if (_delIndexSite == null) {
                Interlocked.CompareExchange(
                    ref _delIndexSite,
                    CallSite<Func<CallSite, object, object, object>>.Create(
                        new PythonOperationBinder(
                            _defaultBinderState,
                            StandardOperators.DeleteItem
                        )
                    ),
                    null
                );
            }


            _delIndexSite.Target(_delIndexSite, target, index);
        }

        internal void DelSlice(object target, object start, object end) {
            if (_delSliceSite == null) {
                Interlocked.CompareExchange(
                    ref _delSliceSite,
                    CallSite<Func<CallSite, object, object, object, object>>.Create(
                        new PythonOperationBinder(
                            _defaultBinderState,
                            StandardOperators.DeleteSlice
                        )
                    ),
                    null
                );
            }


            _delSliceSite.Target(_delSliceSite, target, start, end);
        }

        internal void SetIndex(object a, object b, object c) {
            if (_setIndexSite == null) {
                Interlocked.CompareExchange(
                    ref _setIndexSite,
                    CallSite<Func<CallSite, object, object, object, object>>.Create(
                        new PythonOperationBinder(
                            _defaultBinderState,
                            StandardOperators.SetItem
                        )
                    ),
                    null
                );
            }

            _setIndexSite.Target(_setIndexSite, a, b, c);
        }

        internal void SetSlice(object a, object start, object end, object value) {
            if (_setSliceSite == null) {
                Interlocked.CompareExchange(
                    ref _setSliceSite,
                    CallSite<Func<CallSite, object, object, object, object, object>>.Create(
                        new PythonOperationBinder(
                            _defaultBinderState,
                            StandardOperators.SetSlice
                        )
                    ),
                    null
                );
            }

            _setSliceSite.Target(_setSliceSite, a, start, end, value);
        }

        internal CallSite<Func<CallSite, object, object, object>> EqualSite {
            get {
                if (_equalSite == null) {
                    Interlocked.CompareExchange(
                        ref _equalSite,
                        CallSite<Func<CallSite, object, object, object>>.Create(
                            new PythonOperationBinder(
                                _defaultBinderState,
                                StandardOperators.GetItem
                            )
                        ),
                        null
                    );
                }

                return _equalSite;
            }
        }

        internal CallSite<Func<CallSite, CodeContext, object, IList<string>>> MemberNamesSite {
            get {
                if (_memberNamesSite == null) {
                    Interlocked.CompareExchange(
                        ref _memberNamesSite,
                        CallSite<Func<CallSite, CodeContext, object, IList<string>>>.Create(
                            new PythonOperationBinder(
                                _defaultBinderState,
                                StandardOperators.MemberNames
                            )
                        ),
                        null
                    );
                }

                return _memberNamesSite;
            }
        }

        internal CallSite<Func<CallSite, object, IList<string>>> GetMemberNamesSite {
            get {
                if (_getMemberNamesSite == null) {
                    Interlocked.CompareExchange(
                        ref _getMemberNamesSite,
                        CallSite<Func<CallSite, object, IList<string>>>.Create(
                            CreateOperationBinder(StandardOperators.MemberNames)
                        ),
                        null
                    );
                }

                return _getMemberNamesSite;
            }
        }

        internal CallSite<Func<CallSite, CodeContext, object, object>> FinalizerSite {
            get {
                if (_finalizerSite == null) {
                    Interlocked.CompareExchange(
                        ref _finalizerSite,
                        CallSite<Func<CallSite, CodeContext, object, object>>.Create(
                            new PythonInvokeBinder(
                                DefaultBinderState,
                                new CallSignature(0)
                            )
                        ),
                        null
                    );
                }

                return _finalizerSite;
            }
        }

        internal CallSite<Func<CallSite, CodeContext, PythonFunction, object>> FunctionCallSite {
            get {
                if (_functionCallSite == null) {
                    Interlocked.CompareExchange(
                        ref _functionCallSite,
                        CallSite<Func<CallSite, CodeContext, PythonFunction, object>>.Create(
                            new PythonInvokeBinder(
                                _defaultBinderState,
                                new CallSignature(0)
                            )
                        ),
                        null
                    );
                }

                return _functionCallSite;
            }
        }

        class AttrKey : IEquatable<AttrKey> {
            private Type _type;
            private SymbolId _name;
            private bool _showCls;

            public AttrKey(Type type, SymbolId name) {
                _type = type;
                _name = name;
            }

            public AttrKey(Type type, SymbolId name, bool showCls)
                : this(type, name) {
                _showCls = showCls;
            }

            #region IEquatable<AttrKey> Members

            public bool Equals(AttrKey other) {
                if (other == null) return false;

                return _type == other._type && _name == other._name && _showCls == other._showCls;
            }

            #endregion

            public override bool Equals(object obj) {
                return Equals(obj as AttrKey);
            }

            public override int GetHashCode() {
                return _type.GetHashCode() ^ _name.GetHashCode() ^ (_showCls ? 1 : 0);
            }
        }

        public string GetDocumentation(object o) {
            if (_docSite == null) {
                _docSite = CallSite<Func<CallSite, object, string>>.Create(
                    new PythonOperationBinder(
                        DefaultBinderState,
                        "Documentation"
                    )
                );
            }
            return _docSite.Target(_docSite, o);
        }

        #endregion

        #region Conversions

        internal Int32 ConvertToInt32(object value) {
            if (_intSite == null) {
                Interlocked.CompareExchange(ref _intSite, MakeExplicitConvertSite<int>(), null);
            }

            return _intSite.Target.Invoke(_intSite, value);
        }

        internal bool TryConvertToString(object str, out string res) {
            if (_tryStringSite == null) {
                Interlocked.CompareExchange(ref _tryStringSite, MakeExplicitTrySite<string>(), null);
            }

            res = _tryStringSite.Target(_tryStringSite, str);
            return res != null;
        }

        internal bool TryConvertToInt32(object val, out int res) {
            if (_tryIntSite == null) {
                Interlocked.CompareExchange(ref _tryIntSite, MakeExplicitStructTrySite<int>(), null);
            }

            object objRes = _tryIntSite.Target(_tryIntSite, val);
            if (objRes != null) {
                res = (int)objRes;
                return true;
            }
            res = 0;
            return false;
        }

        internal bool TryConvertToIEnumerable(object enumerable, out IEnumerable res) {
            if (_tryIEnumerableSite == null) {
                Interlocked.CompareExchange(ref _tryIEnumerableSite, MakeExplicitTrySite<IEnumerable>(), null);
            }

            res = _tryIEnumerableSite.Target(_tryIEnumerableSite, enumerable);
            return res != null;
        }

        private CallSite<Func<CallSite, object, T>> MakeExplicitTrySite<T>() where T : class {
            return MakeTrySite<T, T>(ConversionResultKind.ExplicitTry);
        }

        private CallSite<Func<CallSite, object, object>> MakeExplicitStructTrySite<T>() where T : struct {
            return MakeTrySite<T, object>(ConversionResultKind.ExplicitTry);
        }

        private CallSite<Func<CallSite, object, TRet>> MakeTrySite<T, TRet>(ConversionResultKind kind) {
            return CallSite<Func<CallSite, object, TRet>>.Create(
                new ConversionBinder(
                    DefaultBinderState,
                    typeof(T),
                    kind
                )
            );
        }

        internal object ImplicitConvertTo<T>(object value) {
            if (_implicitConvertSites == null) {
                Interlocked.CompareExchange(ref _implicitConvertSites, new Dictionary<Type, CallSite<Func<CallSite, object, object>>>(), null);
            }

            CallSite<Func<CallSite, object, object>> site;
            lock (_implicitConvertSites) {
                if (!_implicitConvertSites.TryGetValue(typeof(T), out site)) {
                    _implicitConvertSites[typeof(T)] = site = MakeImplicitConvertSite<T>();
                }
            }

            return site.Target(site, value);
        }

        /*
                public static String ConvertToString(object value) { return _stringSite.Invoke(DefaultContext.Default, value); }
                public static BigInteger ConvertToBigInteger(object value) { return _bigIntSite.Invoke(DefaultContext.Default, value); }
                public static Double ConvertToDouble(object value) { return _doubleSite.Invoke(DefaultContext.Default, value); }
                public static Complex64 ConvertToComplex64(object value) { return _complexSite.Invoke(DefaultContext.Default, value); }
                public static Boolean ConvertToBoolean(object value) { return _boolSite.Invoke(DefaultContext.Default, value); }
                public static Int64 ConvertToInt64(object value) { return _int64Site.Invoke(DefaultContext.Default, value); }
                */
        private CallSite<Func<CallSite, object, T>> MakeExplicitConvertSite<T>() {
            return MakeConvertSite<T>(ConversionResultKind.ExplicitCast);
        }

        private CallSite<Func<CallSite, object, object>> MakeImplicitConvertSite<T>() {
            return CallSite<Func<CallSite, object, object>>.Create(
                new ConversionBinder(
                    _defaultBinderState,
                    typeof(T),
                    ConversionResultKind.ImplicitCast
                )
            );
        }

        private CallSite<Func<CallSite, object, T>> MakeConvertSite<T>(ConversionResultKind kind) {
            return CallSite<Func<CallSite, object, T>>.Create(
                new ConversionBinder(
                    _defaultBinderState,
                    typeof(T),
                    kind
                )
            );
        }

        /// <summary>
        /// Invokes the specified operation on the provided arguments and returns the new resulting value.
        /// 
        /// operation is usually a value from StandardOperators (standard CLR/DLR operator) or 
        /// OperatorStrings (a Python specific operator)
        /// </summary>
        internal object Operation(string operation, object self, object other) {
            if (_binarySites == null) {
                Interlocked.CompareExchange(
                    ref _binarySites,
                    new Dictionary<string, CallSite<Func<CallSite, object, object, object>>>(),
                    null
                );
            }

            CallSite<Func<CallSite, object, object, object>> site;
            lock (_binarySites) {
                if (!_binarySites.TryGetValue(operation, out site)) {
                    _binarySites[operation] = site = CallSite<Func<CallSite, object, object, object>>.Create(
                        new PythonOperationBinder(
                            _defaultBinderState,
                            operation
                        )
                    );
                }
            }

            return site.Target(site, self, other);
        }

        internal bool GreaterThan(object self, object other) {
            return Comparison(self, other, StandardOperators.GreaterThan, ref _greaterThanSite);
        }

        internal bool LessThan(object self, object other) {
            return Comparison(self, other, StandardOperators.LessThan, ref _lessThanSite);
        }

        internal bool GreaterThanOrEqual(object self, object other) {
            return Comparison(self, other, StandardOperators.GreaterThanOrEqual, ref _greaterThanEqualSite);
        }

        internal bool LessThanOrEqual(object self, object other) {
            return Comparison(self, other, StandardOperators.LessThanOrEqual, ref _lessThanEqualSite);
        }

        internal bool Equal(object self, object other) {
            return Comparison(self, other, StandardOperators.Equal, ref _equalRetBoolSite);
        }

        internal bool NotEqual(object self, object other) {
            return !Equal(self, other);
        }

        private bool Comparison(object self, object other, string operation, ref CallSite<Func<CallSite, object, object, bool>> comparisonSite) {
            if (comparisonSite == null) {
                Interlocked.CompareExchange(
                    ref comparisonSite,
                    CreateComparisonSite(operation),
                    null
                );
            }

            return comparisonSite.Target(comparisonSite, self, other);
        }

        private CallSite<Func<CallSite, object, object, bool>> CreateComparisonSite(string op) {
            return CallSite<Func<CallSite, object, object, bool>>.Create(
                Binders.BinaryOperationRetBool(
                    DefaultBinderState,
                    op
                )
            );
        }

        internal object Call(object func, params object[] args) {
            if (_callSplatSite == null) {
                Interlocked.CompareExchange(
                    ref _callSplatSite,
                    MakeSplatSite(),
                    null
                );
            }

            return _callSplatSite.Target(_callSplatSite, DefaultBinderState.Context, func, args);
        }

        internal object CallWithContext(CodeContext/*!*/ context, object func, params object[] args) {
            if (_callSplatSite == null) {
                Interlocked.CompareExchange(
                    ref _callSplatSite,
                    MakeSplatSite(),
                    null
                );
            }

            return _callSplatSite.Target(_callSplatSite, context, func, args);
        }

        internal CallSite<Func<CallSite, CodeContext, object, object[], object>> MakeSplatSite() {
            return CallSite<Func<CallSite, CodeContext, object, object[], object>>.Create(Binders.InvokeSplat(DefaultBinderState));
        }

        internal object CallWithKeywords(object func, object[] args, IAttributesCollection dict) {
            if (_callDictSite == null) {
                Interlocked.CompareExchange(
                    ref _callDictSite,
                    MakeKeywordSplatSite(),
                    null
                );
            }

            return _callDictSite.Target(_callDictSite, DefaultBinderState.Context, func, args, dict);
        }

        internal CallSite<Func<CallSite, CodeContext, object, object[], IAttributesCollection, object>> MakeKeywordSplatSite() {
            return CallSite<Func<CallSite, CodeContext, object, object[], IAttributesCollection, object>>.Create(Binders.InvokeKeywords(DefaultBinderState));
        }

        internal CallSite<Func<CallSite, CodeContext, object, string, IAttributesCollection, IAttributesCollection, PythonTuple, int, object>> ImportSite {
            get {
                if (_importSite == null) {
                    Interlocked.CompareExchange(
                        ref _importSite,
                        CallSite<Func<CallSite, CodeContext, object, string, IAttributesCollection, IAttributesCollection, PythonTuple, int, object>>.Create(
                            new PythonInvokeBinder(
                                DefaultBinderState,
                                new CallSignature(5)
                            )
                        ),
                        null
                    );
                }

                return _importSite;
            }
        }

        internal CallSite<Func<CallSite, CodeContext, object, string, IAttributesCollection, IAttributesCollection, PythonTuple, object>> OldImportSite {
            get {
                if (_oldImportSite == null) {
                    Interlocked.CompareExchange(
                        ref _oldImportSite,
                        CallSite<Func<CallSite, CodeContext, object, string, IAttributesCollection, IAttributesCollection, PythonTuple, object>>.Create(
                            new PythonInvokeBinder(
                                DefaultBinderState,
                                new CallSignature(4)
                            )
                        ),
                        null
                    );
                }

                return _oldImportSite;
            }
        }

        internal bool IsCallable(object o) {
            if (_isCallableSite == null) {
                Interlocked.CompareExchange(
                    ref _isCallableSite,
                    CallSite<Func<CallSite, object, bool>>.Create(
                        new PythonOperationBinder(
                            DefaultBinderState,
                            StandardOperators.IsCallable
                        )
                    ),
                    null
                );
            }

            return _isCallableSite.Target(_isCallableSite, o);
        }

        internal int Hash(object o) {
            if (_hashSite == null) {
                Interlocked.CompareExchange(
                    ref _hashSite,
                    CallSite<Func<CallSite, object, object>>.Create(
                        new PythonOperationBinder(
                            DefaultBinderState,
                            OperatorStrings.Hash
                        )
                    ),
                    null
                );
            }

            object res = _hashSite.Target(_hashSite, o);
            if (res is int) {
                return (int)res;
            } else if (res is BigInteger) {
                // Python 2.5 defines the result of returning a long as hashing the long
                return Hash(res);
            }

            return ConvertToInt32(res);
        }

        internal object Add(object x, object y) {
            if (_addSite == null) {
                Interlocked.CompareExchange(
                    ref _addSite,
                    CallSite<Func<CallSite, object, object, object>>.Create(
                        new PythonOperationBinder(DefaultBinderState, StandardOperators.Add)
                    ),
                    null
                );
            }

            return _addSite.Target(_addSite, x, y);
        }

        internal object DivMod(object x, object y) {
            if (_divModSite == null) {
                Interlocked.CompareExchange(
                    ref _divModSite,
                    CallSite<Func<CallSite, object, object, object>>.Create(
                        new PythonOperationBinder(DefaultBinderState, StandardOperators.DivMod)
                    ),
                    null
                );
            }

            object ret = _divModSite.Target(_divModSite, x, y);
            if (ret != NotImplementedType.Value) {
                return ret;
            }

            if (_rdivModSite == null) {
                Interlocked.CompareExchange(
                    ref _rdivModSite,
                    CallSite<Func<CallSite, object, object, object>>.Create(
                        new PythonOperationBinder(DefaultBinderState, "Reverse" + StandardOperators.DivMod)
                    ),
                    null
                );
            }

            ret = _rdivModSite.Target(_rdivModSite, x, y);
            if (ret != NotImplementedType.Value) {
                return ret;
            }

            throw PythonOps.TypeErrorForBinaryOp("divmod", x, y);

        }

        #endregion

        #region Compiled Code Support

        internal CompiledLoader GetCompiledLoader() {
            if (_compiledLoader == null) {
                if (Interlocked.CompareExchange(ref _compiledLoader, new CompiledLoader(), null) == null) {
                    SymbolId meta_path = SymbolTable.StringToId("meta_path");
                    object path;
                    List lstPath;

                    if (!SystemState.Dict.TryGetValue(meta_path, out path) || ((lstPath = path as List) == null)) {
                        SystemState.Dict[meta_path] = lstPath = new List();
                    }

                    lstPath.append(_compiledLoader);
                }
            }

            return _compiledLoader;
        }

        #endregion

        internal BinderState DefaultBinderState {
            get {
                return _defaultBinderState;
            }
        }

        internal BinderState DefaultClsBinderState {
            get {
                return _defaultClsBinderState;
            }
        }

        internal ClrModule.ReferencesList ReferencedAssemblies {
            get {
                if (_referencesList == null) {
                    Interlocked.CompareExchange(ref _referencesList, new ClrModule.ReferencesList(), null);
                }

                return _referencesList;
            }
        }

        internal CultureInfo CollateCulture {
            get { return _collateCulture; }
            set { _collateCulture = value; }
        }

        internal CultureInfo CTypeCulture {
            get { return _ctypeCulture; }
            set { _ctypeCulture = value; }
        }

        internal CultureInfo TimeCulture {
            get { return _timeCulture; }
            set { _timeCulture = value; }
        }

        internal CultureInfo MonetaryCulture {
            get { return _monetaryCulture; }
            set { _monetaryCulture = value; }
        }

        internal CultureInfo NumericCulture {
            get { return _numericCulture; }
            set { _numericCulture = value; }
        }

        #region Command Dispatching

        // This can be set to a method like System.Windows.Forms.Control.Invoke for Winforms scenario 
        // to cause code to be executed on a separate thread.
        // It will be called with a null argument to indicate that the console session should be terminated.
        // Can be null.

        internal CommandDispatcher GetSetCommandDispatcher(CommandDispatcher newDispatcher) {
            return Interlocked.Exchange(ref _commandDispatcher, newDispatcher);
        }

        internal void DispatchCommand(Action command) {
            CommandDispatcher dispatcher = _commandDispatcher;
            if (dispatcher != null) {
                dispatcher(command);
            } else if (command != null) {
                command();
            }
        }

        #endregion

        internal CallSite<Func<CallSite, CodeContext, object, object, object>> PropertyGetSite {
            get {
                if (_propGetSite == null) {
                    Interlocked.CompareExchange(ref _propGetSite,
                        CallSite<Func<CallSite, CodeContext, object, object, object>>.Create(
                            new PythonInvokeBinder(
                                DefaultBinderState,
                                new CallSignature(1)
                            )
                        ),
                        null
                    );
                }

                return _propGetSite;
            }
        }

        internal CallSite<Func<CallSite, CodeContext, object, object, object>> PropertyDeleteSite {
            get {
                if (_propDelSite == null) {
                    Interlocked.CompareExchange(ref _propDelSite,
                        CallSite<Func<CallSite, CodeContext, object, object, object>>.Create(
                            new PythonInvokeBinder(
                                DefaultBinderState,
                                new CallSignature(1)
                            )
                        ),
                        null
                    );
                }

                return _propDelSite;
            }
        }

        internal CallSite<Func<CallSite, CodeContext, object, object, object, object>> PropertySetSite {
            get {
                if (_propSetSite == null) {
                    Interlocked.CompareExchange(ref _propSetSite,
                        CallSite<Func<CallSite, CodeContext, object, object, object, object>>.Create(
                            new PythonInvokeBinder(
                                DefaultBinderState,
                                new CallSignature(2)
                            )
                        ),
                        null
                    );
                }

                return _propSetSite;
            }
        }

        internal new PythonBinder Binder {
            get {
                return (PythonBinder)base.Binder;
            }
            set {
                base.Binder = value;
            }
        }

        private class DefaultPythonComparer : IComparer {
            private CallSite<Func<CallSite, object, object, int>> _site;
            public DefaultPythonComparer(PythonContext context) {
                _site = CallSite<Func<CallSite, object, object, int>>.Create(
                    Binders.BinaryOperationRetType(
                        context.DefaultBinderState,
                        StandardOperators.Compare,
                        typeof(int)
                    )
                );
            }

            public int Compare(object x, object y) {
                return _site.Target(_site, x, y);
            }
        }

        private class FunctionComparer<T> : IComparer {
            private T _cmpfunc;
            private CallSite<Func<CallSite, CodeContext, T, object, object, int>> _funcSite;
            private CodeContext/*!*/ _context;

            public FunctionComparer(PythonContext/*!*/ context, T cmpfunc)
                : this(context, cmpfunc, MakeCompareSite<T>(context)) { 
            }
            
            public FunctionComparer(PythonContext/*!*/ context, T cmpfunc, CallSite<Func<CallSite, CodeContext, T, object, object, int>> site) {
                _cmpfunc = cmpfunc;
                _context = context.DefaultBinderState.Context;
                _funcSite = site;
            }

            public int Compare(object o1, object o2) {
                return _funcSite.Target(_funcSite, _context, _cmpfunc, o1, o2);
            }
        }

        private static CallSite<Func<CallSite, CodeContext, T, object, object, int>> MakeCompareSite<T>(PythonContext context) {
            return CallSite<Func<CallSite, CodeContext, T, object, object, int>>.Create(
                Binders.InvokeAndConvert(
                    context.DefaultBinderState,
                    2,
                    typeof(int)
                )
            );
        }

        /// <summary>
        /// Gets a function which can be used for comparing two values.  If cmp is not null
        /// then the comparison will use the provided comparison function.  Otherwise
        /// it will use the normal Python semantics.
        /// 
        /// If type is null then a generic comparison function is returned.  If type is 
        /// not null a comparison function is returned that's used for just that type.
        /// </summary>
        internal IComparer GetComparer(object cmp, Type type) {
            if (type == null) {
                // no type information, return the generic version...                
                if (cmp == null) {
                    return new DefaultPythonComparer(this);
                } else if (cmp is PythonFunction) {
                    return new FunctionComparer<PythonFunction>(this, (PythonFunction)cmp);
                } else if (cmp is BuiltinFunction) {
                    return new FunctionComparer<BuiltinFunction>(this, (BuiltinFunction)cmp);
                }

                return new FunctionComparer<object>(this, cmp);
            }

            if (cmp == null) {
                if (_defaultComparer == null) {
                    Interlocked.CompareExchange(
                        ref _defaultComparer,
                        new Dictionary<Type, DefaultPythonComparer>(),
                        null
                    );
                }

                lock (_defaultComparer) {
                    DefaultPythonComparer comparer;
                    if (!_defaultComparer.TryGetValue(type, out comparer)) {
                        _defaultComparer[type] = comparer = new DefaultPythonComparer(this);
                    }
                    return comparer;
                }
            } else if (cmp is PythonFunction) {
                if (_sharedPythonFunctionCompareSite == null) {
                    _sharedPythonFunctionCompareSite = MakeCompareSite<PythonFunction>(this);
                }

                return new FunctionComparer<PythonFunction>(this, (PythonFunction)cmp, _sharedPythonFunctionCompareSite);
            } else if (cmp is BuiltinFunction) {
                if (_sharedBuiltinFunctionCompareSite == null) {
                    _sharedBuiltinFunctionCompareSite = MakeCompareSite<BuiltinFunction>(this);
                }

                return new FunctionComparer<BuiltinFunction>(this, (BuiltinFunction)cmp, _sharedBuiltinFunctionCompareSite);
            }

            if (_sharedFunctionCompareSite == null) {
                _sharedFunctionCompareSite = MakeCompareSite<object>(this);
            }

            return new FunctionComparer<object>(this, cmp, _sharedFunctionCompareSite);
            
        }
    }

    /// <summary>
    /// List of unary operators which we have sites for to enable fast dispatch that
    /// doesn't collide with other operators.
    /// </summary>
    enum UnaryOperators {
        Repr,
        Length,
        Hash,
        String,

        Maximum
    }

    enum TernaryOperators {
        SetDescriptor,
        GetDescriptor,

        Maximum
    }
}
