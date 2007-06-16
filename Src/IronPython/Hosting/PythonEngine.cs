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
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Threading;

using IronPython.Compiler;
using IronPython.Compiler.Generation;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;

namespace IronPython.Hosting {

    using SingletonFactory = SingletonEngineFactory<PythonEngine, PythonEngineOptions, PythonLanguageProvider>;

    public sealed class PythonEngine : ScriptEngine, IDisposable {
        private static readonly Guid PythonLanguageGuid = new Guid("03ed4b80-d10b-442f-ad9a-47dae85b2051");
        
        private readonly PythonScriptCompiler _compiler;
        
        internal static readonly SingletonFactory Factory = new SingletonFactory(
            delegate(PythonLanguageProvider provider, PythonEngineOptions options) { return new PythonEngine(provider, options); },
            GetSetupInformation);

        // singleton:
        private readonly SystemState _systemState;
        private readonly Importer _importer;
        private readonly PythonErrorSink _defaultErrorSink;
        private readonly PythonModuleContext _defaultModuleContext;
        private readonly PythonBinder _binder;
        internal readonly object _exceptionType;
        

#if !SILVERLIGHT
        private static int _hookedAssemblyResolve;
#endif

        public override Guid LanguageGuid {
            get {
                return PythonLanguageGuid;
            }
        }

        public override string VersionString {
            get {
                return String.Format("IronPython {0} ({1}) on .NET {2}", "2.0A1", GetFileVersion(), Environment.Version);
            }
        }
        
        public new PythonEngineOptions Options {
            get { return (PythonEngineOptions)base.Options; }
        }

        public PythonScriptCompiler ScriptCompiler {
            get { return _compiler; }
        }

        public override ScriptCompiler Compiler {
            get {
                return _compiler;
            }
        }

        public override ActionBinder DefaultBinder {
            get {
                return _binder;
            }
        }

        public PythonModuleContext DefaultModuleContext {
            get {
                return _defaultModuleContext;
            }
        }

        #region Construction

        /// <summary>
        /// Provides the current Python engine. Asks the current <see cref="ScriptEnvironment"/> for a one of it doesn't exist yet.
        /// </summary>
        public static PythonEngine CurrentEngine {
            get {
                return Factory.GetInstance();
            }
        }
        
        private static PythonEngineOptions GetSetupInformation() {
            // TODO: look-up appdomain data, config, etc. - maybe unified with the SE.GetSetupInfo
            return new PythonEngineOptions();
        }

        /// <summary>
        /// To be called by <see cref="GetLocalService"/> only.
        /// </summary>
        private PythonEngine(PythonLanguageProvider provider, PythonEngineOptions engineOptions)
            : base(provider, engineOptions) {

            // singletons:
            _defaultErrorSink = new PythonErrorSink(false);
            _defaultModuleContext = new PythonModuleContext(null);
            _importer = new Importer(this);

            DefaultContext.CreateContexts(this);

            _binder = new PythonBinder(DefaultContext.DefaultCLS);
           
            _systemState = new SystemState();
            _compiler = new PythonScriptCompiler(this);
            

            _exceptionType = ExceptionConverter.GetPythonException("Exception");
            _systemState.Initialize();

            // TODO: this should be in SystemState.Initialize but dependencies...
            if (Options.WarningFilters != null)
                _systemState.warnoptions = IronPython.Runtime.List.Make(Options.WarningFilters);

            _systemState.SetRecursionLimit(Options.MaximumRecursion);
            _systemState.argv = List.Make((ICollection)Options.Arguments);

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
            // TODO:
            // SetModuleCodeContext(ScriptDomainManager.CurrentManager.Host.DefaultModule, _defaultModuleContext);
        }

        #endregion

        public override void SetSourceUnitSearchPaths(string[] paths) {
            _systemState.path = List.Make(paths);
        }

        private IAttributesCollection GetGlobalsDictionary(IDictionary<string, object> globals) {
            return new StringDictionaryAdapterDict(globals);
        }

        public override void Shutdown() {
            object callable;

            try {
                if (PythonOps.TryGetBoundAttr(_systemState, Symbols.SysExitFunc, out callable)) {
                    PythonCalls.Call(callable);
                }
            } finally {
                DumpDebugInfo();
            }
        }

        public void DumpDebugInfo() {
            if (ScriptDomainManager.Options.EngineDebug) {
                PerfTrack.DumpStats();
                try {
                    ScriptDomainManager.CurrentManager.Snippets.Dump();
                } catch (NotSupportedException) { } // usually not important info...
            }
        }

        public void InitializeModules(string prefix, string executable, string version) {
            _systemState.SetHostVariables(prefix, executable, version);
        }

        public override TextWriter GetOutputWriter(bool isErrorOutput) {
            return new OutputWriter(isErrorOutput);
        }


        public ScriptModule CreateModule(string moduleName) {
            return CreateModule(moduleName, new Dictionary<string, object>(), ModuleOptions.None);
        }

        public ScriptModule CreateModule(string moduleName, ModuleOptions options) {
            return CreateModule(moduleName, new Dictionary<string, object>(), options);
        }

        /// <summary>
        /// Create a module. A module is required to be able to execute any code.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="globals"></param>
        /// <param name="publishModule">If this is true, the module will be published as sys.modules[moduleName].
        /// The module may later be unpublished by executing "del sys.modules[moduleName]". All resources associated
        /// with the module will be reclaimed after that.
        /// </param>
        public ScriptModule CreateModule(string moduleName, IDictionary<string, object> globals, ModuleOptions options) {
            if (moduleName == null) throw new ArgumentNullException("moduleName");
            if (globals == null) throw new ArgumentNullException("globals");

            IAttributesCollection globalDict = globals as IAttributesCollection ?? GetGlobalsDictionary(globals);
            ScriptModule module = MakePythonModule(moduleName, new Scope(globalDict), options);

            return module;
        }

        public override void PublishModule(IScriptModule module) {
            if (module == null) throw new ArgumentNullException("module");

            // TODO: remote modules here...
            _systemState.modules[module.ModuleName] = module;
        }        

        public ScriptModule MakePythonModule(string name) {
            return MakePythonModule(name, null, ModuleOptions.None);
        }

        public ScriptModule MakePythonModule(string name, Scope scope) {
            return MakePythonModule(name, scope, ModuleOptions.None);
        }

        // scope can be null
        public ScriptModule MakePythonModule(string name, Scope scope, ModuleOptions options) {
            if (name == null) throw new ArgumentNullException("name");
            if (scope == null) scope = new Scope(new SymbolDictionary());

            ScriptModule module = ScriptDomainManager.CurrentManager.CreateModule(name, scope);

            PythonModuleContext moduleContext = (PythonModuleContext)DefaultContext.Default.LanguageContext.EnsureModuleContext(module);
            moduleContext.ShowCls = (options & ModuleOptions.ShowClsMethods) != 0;
            moduleContext.TrueDivision = (options & ModuleOptions.TrueDivision) != 0;
            moduleContext.IsPythonCreatedModule = true;

            if ((options & ModuleOptions.PublishModule) != 0) {
                PublishModule(module);
            }

            return module;
        }        

        public override int ExecuteProgram(SourceUnit sourceUnit) {
            try {
                return base.ExecuteProgram(sourceUnit);
            } catch (PythonSystemExitException e) {
                object obj_code;
                return e.GetExitCode(out obj_code);
            }
        }
        
        public void Execute(string scriptCode, ScriptModule module, IDictionary<string, object> locals) {
            Execute(new SourceCodeUnit(this, scriptCode), module, locals);
        }

        /// <summary>
        /// Execute the Python code.
        /// The API will throw any exceptions raised by the code. If PythonSystemExit is thrown, the host should 
        /// interpret that in a way that is appropriate for the host.
        /// </summary>
        /// <param name="sourceUnit">Source unit to execute.</param>
        /// <param name="scope">The scope to execute the code in.</param>
        /// <param name="locals">Dictionary of locals</param>
        public void Execute(SourceUnit sourceUnit, ScriptModule module, IDictionary<string, object> locals) {
            if (sourceUnit == null) throw new ArgumentNullException("sourceUnit");
            if (module == null) throw new ArgumentNullException("module");

            ScriptCode code = ScriptCode.FromCompiledCode(sourceUnit.Compile(GetModuleCompilerOptions(module), _defaultErrorSink));
            ModuleContext moduleContext = DefaultContext.Default.LanguageContext.EnsureModuleContext(module);
            code.Run(locals != null ? new Scope(module.Scope, GetAttrDict(locals)) : module.Scope, moduleContext);
        }

        public object Evaluate(string expression, ScriptModule module, IDictionary<string, object> locals) {
            if (expression == null) throw new ArgumentNullException("expression");
            if (module == null) throw new ArgumentNullException("module");

            if (locals != null) {
                Scope newScope = new Scope(module.Scope, GetAttrDict(locals));
                module = MakePythonModule(module.ModuleName, newScope);
            }

            return Evaluate(expression, module);
        }

        private static IAttributesCollection GetAttrDict(IDictionary<string, object> locals) {
            return locals as IAttributesCollection ?? new StringDictionaryAdapterDict(locals);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T EvaluateAs<T>(string expression, ScriptModule module, IDictionary<string, object> locals) {
            return ConvertObject<T>(Evaluate(expression, module, locals));
        }

        public override T ConvertObject<T>(object value) {
            return Converter.Convert<T>(value);
        }

        #region Non-Public Members

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
                        if (moduleName != ExceptionConverter.defaultExceptionModule) {
                            className = moduleName + "." + className;
                        }
                    }
                }
            }
            return className;
        }

        internal static string FormatPythonSyntaxError(SyntaxErrorException e) {
            return String.Format(
                "  File \"{1}\", line {2}{0}" +
                "    {3}{0}" +
                "    {4}^{0}" +
                "{5}: {6}{0}", 
                Environment.NewLine,
                e.FileName, e.Line > 0 ? e.Line.ToString() : "?",
                (e.LineText != null) ? e.LineText.Replace('\t', ' ') : null,
                new String(' ', e.Column != 0 ? e.Column - 1 : 0),
                GetPythonExceptionClassName(ExceptionConverter.ToPython(e)), e.Message);
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

#if SILVERLIGHT // stack trace
        private string FormatStackTraces(Exception e) {
            
            // TODO:
            
            string[] frames = e.ToString().Split('\n');

            if (frames.Length == 0) return e.ToString();

            StringBuilder result = new StringBuilder();
            result.AppendLine("Traceback (most recent call last):");

            const string prefix = "   at ";
            
            for (int i = frames.Length - 1; i >=1 ; i--) {
                if (!frames[i].StartsWith(prefix) ||
                    frames[i].StartsWith(prefix + "IronPython.") ||
                    frames[i].StartsWith(prefix + "ReflectOpt.") ||
                    frames[i].StartsWith(prefix + "System.Reflection.") ||
                    frames[i].StartsWith(prefix + "System.Runtime") ||
                    frames[i].StartsWith(prefix + "Microsoft.Scripting") ||
                    frames[i].StartsWith(prefix + "IronPythonConsole.")) {
                    continue;
                }

                int end = frames[i].IndexOfAny(new char[] { '(', '#', '$' });
                if (end == -1) end = frames[i].Length;
                result.AppendFormat("  in {0}()", frames[i].Substring(prefix.Length, end - prefix.Length));
                result.AppendLine();
            }

            if (Options.ExceptionDetail) {
                result.AppendLine(e.Message);
                result.AppendLine(e.StackTrace);
            }
            
            return result.ToString();
        }
#else
        private string FormatStackTraces(Exception e) {
            bool printedHeader = false;

            return FormatStackTraces(e, ref printedHeader);
        }

        private string FormatStackTraces(Exception e, ref bool printedHeader) {
            return FormatStackTraces(e, null, ref printedHeader);
        }

        private string FormatStackTraces(Exception e, FilterStackFrame fsf, ref bool printedHeader) {
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
                            if (fsf == null || fsf(curFrame))
                                result += curFrame.ToString() + Environment.NewLine;
                        }
                    }
                }

                if(e.StackTrace != null)      result += e.StackTrace.ToString() + Environment.NewLine;
                if (e.InnerException != null) result += FormatStackTraces(e.InnerException, ref printedHeader);
            } else {
                result = FormatStackTraceNoDetail(e, fsf, ref printedHeader);
            }

            return result;
        }

        internal string FormatStackTraceNoDetail(Exception e, FilterStackFrame fsf, ref bool printedHeader) {
            string result = String.Empty;
            // dump inner most exception first, followed by outer most.
            if (e.InnerException != null) result += FormatStackTraceNoDetail(e.InnerException, fsf, ref printedHeader);

            if (!printedHeader) {
                result += "Traceback (most recent call last):" + Environment.NewLine;
                printedHeader = true;
            }
            result += FormatStackTrace(new StackTrace(e, true), fsf);
            IList<StackTrace> traces = ExceptionHelpers.GetExceptionStackTraces(e);
            if (traces != null && traces.Count > 0) {
                for (int i = 0; i < traces.Count; i++) {
                    result += FormatStackTrace(traces[i], fsf);
                }
            }
            return result;
        }

        private string FormatStackTrace(StackTrace st, FilterStackFrame fsf) {
            string result = "";

            StackFrame[] frames = st.GetFrames();
            if (frames == null) return result;

            for (int i = frames.Length - 1; i >= 0; i--) {
                StackFrame frame = frames[i];
                Type parentType = frame.GetMethod().DeclaringType;
                if (parentType != null) {
                    string typeName = parentType.FullName;
                    if (typeName.StartsWith("IronPython.") ||
                        typeName.StartsWith("ReflectOpt.") ||
                        typeName.StartsWith("System.Reflection.") ||
                        typeName.StartsWith("System.Runtime") ||
                        typeName.StartsWith("Microsoft.Scripting") ||
                        typeName.StartsWith("IronPythonConsole.")) {
                        continue;
                    }
                }

                if (fsf != null && !fsf(frame)) continue;

                result += FrameToString(frame) + Environment.NewLine;
            }

            return result;
        }

        private string FrameToString(StackFrame frame) {
            if (frame.GetMethod().DeclaringType != null &&
                frame.GetMethod().DeclaringType.Assembly == ScriptDomainManager.CurrentManager.Snippets.Assembly.AssemblyBuilder) {
                string methodName;
                int dollar;

                if (frame.GetMethod().Name == "Run") methodName = "-toplevel-";
                else if ((dollar = frame.GetMethod().Name.IndexOf('$')) == -1) methodName = frame.GetMethod().Name;
                else methodName = frame.GetMethod().Name.Substring(0, dollar);

                return String.Format("  File {0}, line {1}, in {2}",
                    frame.GetFileName(),
                    frame.GetFileLineNumber(),
                    methodName);
            } else {
                string methodName;
                int dollar;

                if ((dollar = frame.GetMethod().Name.IndexOf('$')) == -1) methodName = frame.GetMethod().Name;
                else methodName = frame.GetMethod().Name.Substring(0, dollar);

                string filename = frame.GetFileName();
                string line = frame.GetFileLineNumber().ToString();
                if (String.IsNullOrEmpty(filename)) {
                    if (frame.GetMethod().DeclaringType != null) {
                        filename = frame.GetMethod().DeclaringType.Assembly.GetName().Name;
                        line = "unknown";
                    }
                }

                return String.Format("  File {0}, line {1}, in {2}",
                    filename,
                    line,
                    methodName);
            }
        }

        public delegate bool FilterStackFrame(StackFrame frame);

        private string FormatException(Exception exception, object pythonException, FilterStackFrame filter) {
            Debug.Assert(pythonException != null);
            Debug.Assert(exception != null);

            string result = string.Empty;
            bool printedHeader = false;
            result += FormatStackTraces(exception, filter, ref printedHeader);
            result += FormatPythonException(pythonException);
            if (Options.ShowClrExceptions) {
                result += FormatCLSException(exception);
            }

            return result;
        }
#endif
        public override string FormatException(Exception exception) {
            SyntaxErrorException syntax_error = exception as SyntaxErrorException;
            if (syntax_error != null) {
                return PythonEngine.FormatPythonSyntaxError(syntax_error);
            }
            
            object pythonEx = ExceptionConverter.ToPython(exception);

            string result = FormatStackTraces(exception) + FormatPythonException(pythonEx) + Environment.NewLine;

            if (Options.ShowClrExceptions) {
                result += FormatCLSException(exception);
            }

            return result;
        }

        public override void GetExceptionMessage(Exception exception, out string message, out string typeName) {
            object pythonEx = ExceptionConverter.ToPython(exception);

            message =  FormatPythonException(ExceptionConverter.ToPython(exception));
            typeName = GetPythonExceptionClassName(pythonEx);
        }

        #endregion

        #region Factory methods to create new modules

        /// <summary>
        /// Create a module with optimized code. The restriction is that the user cannot specify a globals 
        /// dictionary of her liking.
        /// </summary>
        public ScriptModule CreateOptimizedModule(string fileName, string moduleName, bool publishModule) {
            return CreateOptimizedModule(fileName, moduleName, publishModule, false);
        }

        //TODO simplify
        public ScriptModule CreateOptimizedModule(string fileName, string moduleName, bool publishModule, bool skipFirstLine) {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (moduleName == null) throw new ArgumentNullException("moduleName");

            SourceFileUnit file_unit = new SourceFileUnit(this, fileName, moduleName, _systemState.DefaultEncoding);
            PythonCompilerOptions options = (PythonCompilerOptions)GetDefaultCompilerOptions();
            options.SkipFirstLine = skipFirstLine;
            ScriptModule module = file_unit.CompileToModule(options, GetDefaultErrorSink());

            if (publishModule) {
                _systemState.modules[moduleName] = module;
            }

            return module;
        }
        

        #endregion

        #region Loader Members

        public void AddToPath(string directory) {
            _systemState.path.Append(directory);
        }
        #endregion

        #region IDisposable Members

        public void Dispose() {
            Dispose(false);
        }

        private void Dispose(bool finalizing) {
            // if we're finalizing we shouldn't access other managed objects, as
            // their finalizers may have already run            
            if (!finalizing) {
                _systemState.CloseStandardIOStreams();
            }
        }

        ~PythonEngine() {
            Dispose(true);
        }

        #endregion

        private static T GetAssemblyAttribute<T>() where T : Attribute {
            Assembly asm = typeof(PythonEngine).Assembly;
            object[] attributes = asm.GetCustomAttributes(typeof(T), false);
            if (attributes != null && attributes.Length > 0) {
                return (T)attributes[0];
            } else {
                Debug.Assert(false, String.Format("Cannot find attribute {0}", typeof(T).Name));
                return null;
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

        public override StreamReader GetSourceReader(Stream stream, ref Encoding encoding) {
            Debug.Assert(stream != null && encoding != null);
            Debug.Assert(stream.CanSeek && stream.CanRead);
            
            // we choose ASCII by default, if the file has a Unicode pheader though
            // we'll automatically get it as unicode.
            Encoding default_encoding = encoding;
            encoding = Utils.AsciiEncoding;

            long start_position = stream.Position;

            StreamReader sr = new StreamReader(stream, Utils.AsciiEncoding);
            string line = sr.ReadLine();
            bool gotEncoding = false;

            // magic encoding must be on line 1 or 2
            if (line != null && !(gotEncoding = Tokenizer.TryGetEncoding(default_encoding, line, ref encoding))) {
                line = sr.ReadLine();

                if (line != null) {
                    gotEncoding = Tokenizer.TryGetEncoding(default_encoding, line, ref encoding);
                }
            }

            if (gotEncoding && sr.CurrentEncoding != Utils.AsciiEncoding && encoding != sr.CurrentEncoding)
            {
                // we have both a BOM & an encoding type, throw an error
                throw new IOException("file has both Unicode marker and PEP-263 file encoding");
            }

            if (encoding == null)
                throw new IOException("unknown encoding type");

            // re-read w/ the correct encoding type...
            stream.Seek(start_position, SeekOrigin.Begin);

            return new StreamReader(stream, encoding);
        }

        public override SourceUnit CreateStandardInputSourceUnit(string code) {
            return new SourceCodeUnit(this, code, "<stdin>");
        }

        public SystemState SystemState {
            get {
                return _systemState;
            }
        }

        public Importer Importer {
            get {
                return _importer;
            }
        }

        #region Factories

        public override ErrorSink GetDefaultErrorSink() {
            return _defaultErrorSink;
        }

        public override CompilerOptions GetDefaultCompilerOptions() {
            return new PythonCompilerOptions(Options.DivisionOptions == PythonDivisionOptions.New);
        }

        public override CompilerOptions GetModuleCompilerOptions(ScriptModule module) {
            Utils.Assert.NotNull(module);

            PythonCompilerOptions result = new PythonCompilerOptions(Options.DivisionOptions == PythonDivisionOptions.New);
            PythonModuleContext moduleContext = (PythonModuleContext)DefaultContext.Default.LanguageContext.GetModuleContext(module);

            if (moduleContext != null) {
                result.TrueDivision = moduleContext.TrueDivision;
            }

            return result;
        }

        protected override LanguageContext GetLanguageContext(ScriptModule module) {
            Debug.Assert(module != null);
            return new PythonContext(this, (PythonCompilerOptions)GetModuleCompilerOptions(module));
        }

        protected override LanguageContext GetLanguageContext(CompilerOptions compilerOptions) {
            Debug.Assert(compilerOptions != null);
            return new PythonContext(this, (PythonCompilerOptions)compilerOptions);
        }

        #endregion

        #region Runtime Code Sense

        protected override string[] FormatObjectMemberNames(IList<object> names) {
            if (names == null) throw new ArgumentNullException("value");

            string[] result = new string[names.Count];

            for (int i = 0; i < names.Count; i++) {
                try {
                    result[i] = PythonOps.ToString(names[i]);
                } catch (ArgumentTypeException e) {
                    // TODO: is this ok?
                    result[i] = String.Format("<Exception: {0}>", e.Message);
                }
            }

            return result;
        }

        public override string[] GetObjectCallSignatures(object obj) {
            // TODO:
            return IsObjectCallable(obj) ? GetObjectDocumentation(obj).Split('\n') : null;
        }

        public override string GetObjectDocumentation(object obj) {
            // TODO:
            return PythonOps.ToString(PythonOps.GetAttr(DefaultContext.Default, obj, Symbols.Doc));
        }

        #endregion

        #region // TODO: workarounds

        protected override IList<object> Ops_GetAttrNames(CodeContext context, object obj) {
            return PythonOps.GetAttrNames(context, obj);
        }

        protected override bool Ops_TryGetAttr(CodeContext context, object obj, SymbolId id, out object value) {
            return PythonOps.TryGetAttr(context, obj, id, out value);
        }

        protected override bool Ops_IsCallable(CodeContext context, object obj) {
            return PythonOps.IsCallable(context, obj);
        }

        protected override object Ops_Call(CodeContext context, object obj, object[] args) {
            return PythonOps.CallWithContext(context, obj, args);
        }

        #endregion

#if !SILVERLIGHT
        /// <summary>
        /// We use Assembly.LoadFile to load assemblies from a path specified by the script (in LoadAssemblyFromFileWithPath).
        /// However, when the CLR loader tries to resolve any of assembly references, it will not be able to
        /// find the dependencies, unless we can hook into the CLR loader.
        /// </summary>
        private static void HookAssemblyResolve() {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(PythonContext.CurrentDomain_AssemblyResolve);
        }
#endif
    }
}
