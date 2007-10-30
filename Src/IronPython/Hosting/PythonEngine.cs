/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using Microsoft.Scripting.Utils;

namespace IronPython.Hosting {

    using SingletonFactory = SingletonEngineFactory<PythonEngine, PythonEngineOptions, PythonLanguageProvider>;

    public sealed class PythonEngine : ScriptEngine, IDisposable {
        private static readonly Guid PythonLanguageGuid = new Guid("03ed4b80-d10b-442f-ad9a-47dae85b2051");
        
        internal static readonly SingletonFactory Factory = new SingletonFactory(
            delegate(PythonLanguageProvider provider, PythonEngineOptions options) { return new PythonEngine(provider, options); },
            GetSetupInformation);

        // singleton:
        private readonly SystemState _systemState;
        private readonly Importer _importer;
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
                return String.Format("IronPython {0} ({1}) on .NET {2}", "2.0A5", GetFileVersion(), Environment.Version);
            }
        }
        
        public new PythonEngineOptions Options {
            get { return (PythonEngineOptions)base.Options; }
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

        internal PythonContext PythonContext {
            get {
                return (PythonContext)LanguageContext;
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
            : base(provider, engineOptions, new PythonContext()) {

            // singletons:
            _defaultModuleContext = new PythonModuleContext(null);
            _importer = new Importer(this);

            PythonContext.PythonEngine = this;
            DefaultContext.CreateContexts(this);

            _binder = new PythonBinder(DefaultContext.DefaultCLS);
           
            _systemState = new SystemState();

            _exceptionType = ExceptionConverter.GetPythonException("Exception");
            _systemState.Initialize();
#if SILVERLIGHT
            AddToPath(".");
#endif

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

            if (PythonOps.TryGetBoundAttr(_systemState, Symbols.SysExitFunc, out callable)) {
                PythonCalls.Call(callable);
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
            Contract.RequiresNotNull(moduleName, "moduleName");
            Contract.RequiresNotNull(globals, "globals");

            IAttributesCollection globalDict = globals as IAttributesCollection ?? GetGlobalsDictionary(globals);
            ScriptModule module = MakePythonModule(moduleName, new Scope(globalDict), options);

            return module;
        }

        public override void PublishModule(IScriptModule module) {
            Contract.RequiresNotNull(module, "module");

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
            Contract.RequiresNotNull(name, "name");
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
            Execute(SourceUnit.CreateSnippet(this, scriptCode), module, locals);
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
            Contract.RequiresNotNull(sourceUnit, "sourceUnit");
            Contract.RequiresNotNull(module, "module");

            ScriptCode code = LanguageContext.CompileSourceCode(sourceUnit, GetModuleCompilerOptions(module), GetCompilerErrorSink());
            ModuleContext moduleContext = LanguageContext.EnsureModuleContext(module);
            code.Run(locals != null ? new Scope(module.Scope, GetAttrDict(locals)) : module.Scope, moduleContext);
        }

        public object Evaluate(string expression, ScriptModule module, IDictionary<string, object> locals) {
            Contract.RequiresNotNull(expression, "expression");
            Contract.RequiresNotNull(module, "module");

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
            List<DynamicStackFrame> dynamicFrames = new List<DynamicStackFrame>(RuntimeHelpers.GetDynamicStackFrames(e));
            IList<StackTrace> traces = ExceptionHelpers.GetExceptionStackTraces(e);
            if (traces != null && traces.Count > 0) {
                for (int i = traces.Count - 1; i >= 0; i--) {
                    result += FormatStackTrace(traces[i], dynamicFrames, fsf);
                }
            }
            result += FormatStackTrace(new StackTrace(e, true), dynamicFrames, fsf);

            //TODO: we would like to be able to assert this;
            // right now, we cannot, because we are not using dynamic frames for non-interpreted dynamic methods.
            // (we create the frames, but we do not consume them in FormatStackTrace.)
            //Debug.Assert(dynamicFrames.Count == 0);
            
            return result;
        }

        private string FormatStackTrace(StackTrace st, List<DynamicStackFrame> dynamicFrames, FilterStackFrame fsf) {
            string result = "";

            StackFrame[] frames = st.GetFrames();
            if (frames == null) return result;

            for (int i = frames.Length - 1; i >= 0; i--) {
                StackFrame frame = frames[i];
                MethodBase method = frame.GetMethod();
                Type parentType = method.DeclaringType;
                if (parentType != null) {
                    string typeName = parentType.FullName;
                    if (typeName == "Microsoft.Scripting.Ast.CodeBlock" && method.Name == "DoExecute") {
                        // Evaluated frame -- Replace with dynamic frame
                        Debug.Assert(dynamicFrames.Count > 0);
                        //if (dynamicFrames.Count == 0) continue;
                        result += FrameToString(dynamicFrames[dynamicFrames.Count-1]) + Environment.NewLine;
                        dynamicFrames.RemoveAt(dynamicFrames.Count - 1);
                        continue;
                    }
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

                // TODO: also try to use dynamic frames for non-interpreted dynamic methods
                result += FrameToString(frame) + Environment.NewLine;
            }

            return result;
        }

        private string FrameToString(DynamicStackFrame frame) {
            return String.Format("  File {0}, line {1}, in {2}",
                frame.GetFileName(), frame.GetFileLineNumber(), frame.GetMethodName());
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
            Contract.RequiresNotNull(fileName, "fileName");
            Contract.RequiresNotNull(moduleName, "moduleName");

            SourceUnit sourceUnit = SourceUnit.CreateFileUnit(this, fileName, _systemState.DefaultEncoding);
            PythonCompilerOptions options = (PythonCompilerOptions)GetDefaultCompilerOptions();
            options.SkipFirstLine = skipFirstLine;
            ScriptModule module = ScriptDomainManager.CurrentManager.CompileModule(moduleName, ScriptModuleKind.Default, null, options, null, sourceUnit); 

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

        public override ErrorSink GetCompilerErrorSink() {
            return new CompilerErrorSink();
        }

        public override CompilerOptions GetDefaultCompilerOptions() {
            return new PythonCompilerOptions(Options.DivisionOptions == PythonDivisionOptions.New);
        }

        public override CompilerOptions GetModuleCompilerOptions(ScriptModule module) {
            Assert.NotNull(module);

            PythonCompilerOptions result = new PythonCompilerOptions();
            PythonModuleContext moduleContext = (PythonModuleContext)LanguageContext.GetModuleContext(module);

            if (moduleContext != null) {
                result.TrueDivision = moduleContext.TrueDivision;
            } else {
                result.TrueDivision = Options.DivisionOptions == PythonDivisionOptions.New;
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
            Contract.RequiresNotNull(names, "names");

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
            return IsObjectCallable(obj) ? GetObjectDocumentation(obj).Replace("\r", "").Split('\n') : null;
        }

        public override string GetObjectDocumentation(object obj) {
            // TODO:
            return PythonOps.ToString(PythonOps.GetBoundAttr(DefaultContext.Default, obj, Symbols.Doc));
        }

        #endregion

        #region // TODO: workarounds

        protected override IList<object> Ops_GetAttrNames(CodeContext context, object obj) {
            return PythonOps.GetAttrNames(context, obj);
        }

        protected override bool Ops_TryGetAttr(CodeContext context, object obj, SymbolId id, out object value) {
            return PythonOps.TryGetBoundAttr(context, obj, id, out value);
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
