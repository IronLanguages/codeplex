/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

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
using IronPython.Compiler.Ast;
using IronPython.Modules;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

namespace IronPython.Hosting {

    public class EngineOptions {
        private bool clrDebuggingEnabled;
        private bool exceptionDetail;
        private bool showClrExceptions;
        private bool skipFirstLine;

        #region Public accessors

        /// <summary>
        /// Skip the first line of the code to execute. This is useful for Unix scripts which
        /// have the command to execute specified in the first line.
        /// This only apply to the script code executed by the PythonEngine APIs, but not for other script code 
        /// that happens to get called as a result of the execution.
        /// </summary>
        public bool SkipFirstLine {
            get { return skipFirstLine; }
            set { skipFirstLine = value; }
        }

        /// <summary>
        /// Enable CLI debugging. This allows debugging the script with a CLI debugger. Also, CLI exceptions
        /// will have line numbers in the stack-trace.
        /// Note that this is independent of the "traceback" Python module.
        /// Also, the generated code will not be reclaimed, and so this should only be used for bounded number 
        /// of executions.
        /// </summary>
        public bool ClrDebuggingEnabled {
            get { return clrDebuggingEnabled; }
            set { clrDebuggingEnabled = value; }
        }

        /// <summary>
        ///  Display exception detail (callstack) when exception gets caught
        /// </summary>
        public bool ExceptionDetail {
            get { return exceptionDetail; }
            set { exceptionDetail = value; }
        }

        public bool ShowClrExceptions {
            get { return showClrExceptions; }
            set { showClrExceptions = value; }
        }

        #endregion

        internal EngineOptions Clone() {
            return (EngineOptions)MemberwiseClone();
        }
    }

    public delegate T ModuleBinder<T>(EngineModule engineModule);

    public class PythonEngine : IDisposable {

        #region Static Public Members

        public const string Copyright = "Copyright (c) Microsoft Corporation. All rights reserved.";

        public static Version Version {
            get {
                return typeof(PythonEngine).Assembly.GetName().Version;
            }
        }

        public static string VersionString {
            get {
                Version pythonVersion = Version;
                string version = String.Format("IronPython {0}.{1}.{2} (Beta) on .NET {3}",
                                  pythonVersion.Major, pythonVersion.Minor, pythonVersion.Build,
                                  Environment.Version);
                return version;
            }
        }

        #endregion

        #region Static Non-Public Members
        internal static Options options = new Options();
        private static CommandDispatcher consoleCommandDispatcher;
        #endregion

        #region Private Data Members
        // Every PythonEngine has its own SystemState. This allows multiple engines to exist simultaneously,
        // while maintaingin unique significant state for every engine..
        private SystemState systemState;

        private EngineModule defaultModule;

        private CompilerContext compilerContext = new CompilerContext("<stdin>");
        private PythonFile stdIn, stdOut, stdErr;

        #endregion

        #region Constructor
        /// <summary>
        /// The caller is responsible for setting Sys.argv to expose command-line options
        /// </summary>
        public PythonEngine() {
            Initialize(new EngineOptions());
        }

        public PythonEngine(EngineOptions engineOptions) {
            if (options == null)
                throw new ArgumentNullException("options", "No options specified for PythonEngine");
            // Clone it first to prevent the client from unexpectedly mutating it
            engineOptions = engineOptions.Clone();
            Initialize(engineOptions);
        }

        public void Shutdown() {
            object callable;

            if (Ops.TryGetAttr(Sys, SymbolTable.SysExitFunc, out callable)) {
                Ops.Call(callable);
            }

            DumpDebugInfo();
        }

        public static void DumpDebugInfo() {
            if (IronPython.Compiler.Options.EngineDebug) {
                PerfTrack.DumpStats();
                try {
                    OutputGenerator.DumpSnippets();
                } catch (NotSupportedException) { } // usually not important info...
            }
        }

        public void InitializeModules(string prefix, string executable, string version) {
            Sys.version = version;
            Sys.prefix = prefix;
            Sys.executable = executable;
            Sys.exec_prefix = prefix;
        }
        #endregion

        #region Non-Public Members

        private void Initialize(EngineOptions engineOptions) {
            // make sure cctor for OutputGenerator has run
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(OutputGenerator).TypeHandle);

            systemState = new SystemState(engineOptions);
            defaultModule = new EngineModule("defaultModule", new Dictionary<string, object>(), systemState);
        }

        internal void EnsureValidModule(EngineModule engineModule) {
            if (engineModule.CallerContext.SystemState != Sys)
                throw new ArgumentOutOfRangeException("module", "An EngineModule can only be used with the PythonEngine that was used to create it");
        }

        internal ModuleScope GetModuleScope(EngineModule engineModule, IDictionary<string, object> locals) {
            EnsureValidModule(engineModule);
            return engineModule.GetModuleScope(locals);
        }

        private static string FormatPythonException(object python) {
            string result = "";

            // dump the python exception.
            if (python != null) {
                string str = python as string;
                if (str != null) {
                    result += str;
                } else if (python is StringException) {
                    result += python.ToString();
                } else {
                    string className = "";
                    object val;
                    if (Ops.TryGetAttr(python, SymbolTable.Class, out val)) {
                        if (Ops.TryGetAttr(val, SymbolTable.Name, out val)) {
                            className = val.ToString();
                            if (Ops.TryGetAttr(python, SymbolTable.Module, out val)) {
                                string moduleName = val.ToString();
                                if (moduleName != ExceptionConverter.defaultExceptionModule) {
                                    className = moduleName + "." + className;
                                }
                            }
                        }
                    }
                    result += className + ": " + python.ToString();
                }
            }

            return result + Environment.NewLine;
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

        private string FormatStackTraces(Exception e) {
            bool printedHeader = false;

            return FormatStackTraces(e, ref printedHeader);
        }

        private string FormatStackTraces(Exception e, ref bool printedHeader) {
            return FormatStackTraces(e, null, ref printedHeader);
        }

        private string FormatStackTraces(Exception e, FilterStackFrame fsf, ref bool printedHeader) {
            string result = "";
            if (Sys.EngineOptions.ExceptionDetail) {
                if (!printedHeader) {
                    result = e.Message + Environment.NewLine;
                    printedHeader = true;
                }
                IList<System.Diagnostics.StackTrace> traces = ExceptionConverter.GetExceptionStackTraces(e);

                if (traces != null) {
                    for (int i = 0; i < traces.Count; i++) {
                        for (int j = 0; j < traces[i].FrameCount; j++) {
                            StackFrame curFrame = traces[i].GetFrame(j);
                            if (fsf == null || fsf(curFrame))
                                result += curFrame.ToString() + Environment.NewLine;
                        }
                    }
                }

                result += e.StackTrace.ToString() + Environment.NewLine;
                if (e.InnerException != null) result += FormatStackTraces(e.InnerException, ref printedHeader);
            } else {
                // dump inner most exception first, followed by outer most.
                if (e.InnerException != null) result += FormatStackTraces(e.InnerException, ref printedHeader);

                if (!printedHeader) {
                    result += "Traceback (most recent call last):" + Environment.NewLine;
                    printedHeader = true;
                }
                result += FormatStackTrace(new StackTrace(e, true), fsf);
                IList<StackTrace> traces = ExceptionConverter.GetExceptionStackTraces(e);
                if (traces != null && traces.Count > 0) {
                    for (int i = 0; i < traces.Count; i++) {
                        result += FormatStackTrace(traces[i], fsf);
                    }
                }
            }

            return result;
        }

        private static string FormatStackTrace(StackTrace st, FilterStackFrame fsf) {
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
                        typeName.StartsWith("IronPythonConsole.")) {
                        continue;
                    }
                }

                if (fsf != null && !fsf(frame)) continue;

                result += FrameToString(frame) + Environment.NewLine;
            }

            return result;
        }

        private static string FrameToString(StackFrame frame) {
            if (frame.GetMethod().DeclaringType != null &&
                frame.GetMethod().DeclaringType.Assembly == OutputGenerator.Snippets.myAssembly) {
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

        private void ExecuteSnippet(Parser p, ModuleScope moduleScope) {
            Statement s = p.ParseFileInput();
            CompiledCode compiledCode = OutputGenerator.GenerateSnippet(p.CompilerContext, s, false, Sys.EngineOptions.ClrDebuggingEnabled);
            compiledCode.Run(moduleScope);
        }

        private CompiledCode Compile(Parser p) {
            Statement s = p.ParseFileInput();

            CompiledCode compiledCode = OutputGenerator.GenerateSnippet(p.CompilerContext, s, false, Sys.EngineOptions.ClrDebuggingEnabled);
            compiledCode.engine = this;
            return compiledCode;
        }

        public delegate bool FilterStackFrame(StackFrame frame);

        // TODO: Make private
        public string FormatException(Exception exception, object pythonException, FilterStackFrame filter) {
            Debug.Assert(pythonException != null);
            Debug.Assert(exception != null);

            string result = string.Empty;
            bool printedHeader = false;
            result += FormatStackTraces(exception, filter, ref printedHeader);
            result += FormatPythonException(pythonException);
            if (Sys.EngineOptions.ShowClrExceptions) {
                result += FormatCLSException(exception);
            }

            return result;
        }
        #endregion

        #region Factory methods to create new modules

        public EngineModule CreateModule() {
            return CreateModule(String.Empty, new Dictionary<string, object>(), false);
        }

        public EngineModule CreateModule(string moduleName, bool publishModule) {
            return CreateModule(moduleName, new Dictionary<string, object>(), publishModule);
        }

        /// <summary>
        /// Create a module. A module is required to be able to execute any code.
        /// </summary>
        /// <param name="publishModule">If this is true, the module will be published as sys.modules[moduleName].
        /// The module may later be unpublished by executing "del sys.modules[moduleName]". All resources associated
        /// with the module will be reclaimed after that.
        /// </param>
        public EngineModule CreateModule(string moduleName, IDictionary<string, object> globals, bool publishModule) {
            if (moduleName == null) throw new ArgumentException("moduleName");
            if (globals == null) throw new ArgumentException("globals");

            EngineModule engineModule = new EngineModule(moduleName, globals, Sys);
            if (publishModule) {
                Sys.modules[moduleName] = engineModule.Module;
            }
            return engineModule;
        }

        /// <summary>
        /// Create a module with optimized code. The restriction is that the user cannot specify a globals 
        /// dictionary of her liking.
        /// </summary>
        public OptimizedEngineModule CreateOptimizedModule(string fileName, string moduleName, bool publishModule) {
            if (fileName == null) throw new ArgumentException("fileName");
            if (moduleName == null) throw new ArgumentException("moduleName");

            CompilerContext context = this.compilerContext.CopyWithNewSourceFile(fileName);
            Parser p = Parser.FromFile(Sys, context, Sys.EngineOptions.SkipFirstLine, false);
            Statement s = p.ParseFileInput();

            PythonModule module = OutputGenerator.GenerateModule(Sys, context, s, moduleName);
            ModuleScope moduleScope = new ModuleScope(module);
            OptimizedEngineModule engineModule = new OptimizedEngineModule(moduleScope);

            module.SetAttr(module, SymbolTable.File, fileName);

            if (publishModule) {
                Sys.modules[moduleName] = module;
            }

            return engineModule;
        }

        #endregion

        #region Console Support
        public delegate void CommandDispatcher(Delegate consoleCommand);

        // This can be set to a method like System.Windows.Forms.Control.Invoke for Winforms scenario 
        // to cause code to be executed on a separate thread.
        // It will be called with a null argument to indicate that the console session should be terminated.
        public static CommandDispatcher ConsoleCommandDispatcher {
            get { return consoleCommandDispatcher; }
            set { consoleCommandDispatcher = value; }
        }

        public void ExecuteToConsole(string text) { ExecuteToConsole(text, defaultModule, null); }

        public void ExecuteToConsole(string text, EngineModule engineModule, IDictionary<string, object> locals) {
            ModuleScope moduleScope = GetModuleScope(engineModule, locals);

            Parser p = Parser.FromString(Sys, compilerContext, text);
            bool isEmptyStmt = false;
            Statement s = p.ParseInteractiveInput(false, out isEmptyStmt);

            //  's' is null when we parse a line composed only of a NEWLINE (interactive_input grammar);
            //  we don't generate anything when 's' is null
            if (s != null) {
                CompiledCode compiledCode = OutputGenerator.GenerateSnippet(compilerContext, s, true, false);
                Exception ex = null;

                if (consoleCommandDispatcher != null) {
                    CallTarget0 runCode = delegate() {
                        try { compiledCode.Run(moduleScope); } catch (Exception e) { ex = e; }
                        return null;
                    };

                    consoleCommandDispatcher(runCode);

                    // We catch and rethrow the exception since it could have been thrown on another thread
                    if (ex != null)
                        throw ex;
                } else {
                    compiledCode.Run(moduleScope);
                }
            }
        }

        public bool ParseInteractiveInput(string text, bool allowIncompleteStatement) {
            Parser p = Parser.FromString(Sys, compilerContext, text);
            return VerifyInteractiveInput(p, allowIncompleteStatement);
        }

        #endregion

        private static bool VerifyInteractiveInput(Parser parser, bool allowIncompleteStatement) {
            bool isEmptyStmt;
            Statement s = parser.ParseInteractiveInput(allowIncompleteStatement, out isEmptyStmt);

            if (s == null)
                return isEmptyStmt;

            return true;
        }

        public SystemState Sys {
            get {
                return systemState;
            }
        }

        #region Loader Members
        public void AddToPath(string dirName) {
            Sys.path.Append(dirName);
        }

        public void LoadAssembly(Assembly assembly) {
            Sys.TopPackage.LoadAssembly(Sys, assembly);
        }

        public object Import(string moduleName) {
            EnsureValidModule(defaultModule);

            object mod = Importer.ImportModule(defaultModule.CallerContext, moduleName, true);
            if (mod != null) {
                string[] names = moduleName.Split('.');
                defaultModule.CallerContext.Globals[SymbolTable.StringToId(names[names.Length - 1])] = mod;
            }
            return mod;
        }
        #endregion

        #region IO Stream
        public void SetStandardError(Stream stream) {
            Sys.__stderr__ = stdErr = new PythonFile(stream, Sys.DefaultEncoding, "HostedStderr", "w");
            Sys.stderr = Sys.__stderr__;
        }

        public void SetStandardOutput(Stream stream) {
            Sys.__stdout__ = stdOut = new PythonFile(stream, Sys.DefaultEncoding, "HostedStdout", "w");
            Sys.stdout = Sys.__stdout__;
        }

        public void SetStandardInput(Stream stream) {
            Sys.__stdin__ = stdIn = new PythonFile(stream, Sys.DefaultEncoding, "HostedStdin", "w");
            Sys.stdin = Sys.__stdin__;
        }
        #endregion

        #region Get\Set Globals of DefaultModule
        public IDictionary<string, object> Globals {
            get { return defaultModule.Globals; }
        }

        public EngineModule DefaultModule {
            get { return defaultModule; }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                EnsureValidModule(value);
                defaultModule = value;
            }
        }

        #endregion

        #region Dynamic Execution\Evaluation
        public void Execute(string scriptCode) {
            Execute(scriptCode, defaultModule);
        }

        public void Execute(string scriptCode, EngineModule engineModule) {
            Execute(scriptCode, null /*fileName*/, engineModule, null);
        }

        public void Execute(string scriptCode, EngineModule engineModule, IDictionary<string, object> locals) {
            Execute(scriptCode, null /*sourceFileName*/, engineModule, locals);
        }

        /// <summary>
        /// Execute the Python code.
        /// The API will throw any exceptions raised by the code. If PythonSystemExit is thrown, the host should 
        /// interpret that in a way that is appropriate for the host.
        /// </summary>
        /// <param name="fileName">This is used for scenarios when a file contains embedded Python code, along with
        /// non-Python code. The host can creating a string with just the Python code, with empty lines in place 
        /// of the non-Python code. The fileName will then be used for debugging and traceback information</param>
        /// <param name="moduleScope">The module context to execute the code in.</param>
        public void Execute(string scriptCode, string fileName, EngineModule engineModule, IDictionary<string, object> locals) {
            CompiledCode compiledCode = Compile(scriptCode, fileName);
            compiledCode.Execute(engineModule, locals);
        }

        public void ExecuteFile(string fileName) {
            ExecuteFile(fileName, defaultModule);
        }

        public void ExecuteFile(string fileName, EngineModule engineModule) {
            ExecuteFile(fileName, engineModule, null);
        }

        public void ExecuteFile(string fileName, EngineModule engineModule, IDictionary<string, object> locals) {
            CompiledCode compiledCode = CompileFile(fileName);
            compiledCode.Execute(engineModule, locals);
        }

        public object Evaluate(string expression) {
            return Evaluate(expression, defaultModule);
        }

        public object Evaluate(string expression, EngineModule engineModule) {
            return Evaluate(expression, engineModule, null);
        }

        public object Evaluate(string expression, EngineModule engineModule, IDictionary<string, object> locals) {
            ModuleScope moduleScope = GetModuleScope(engineModule, locals);

            return Builtin.Eval(moduleScope, expression);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T EvaluateAs<T>(string expression) {
            return EvaluateAs<T>(expression, defaultModule);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T EvaluateAs<T>(string expression, EngineModule engineModule) {
            return EvaluateAs<T>(expression, engineModule, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T EvaluateAs<T>(string expression, EngineModule engineModule, IDictionary<string, object> locals) {
            object result = Evaluate(expression, engineModule, locals);
            return Converter.Convert<T>(result);
        }

        #region CreateMethod and CreateDelegate

        /// <summary>
        /// Create's a strongly typed delegate of type T bound to the default EngineModule.
        /// 
        /// The delegate's parameter names will be available within the function as argument names.
        /// </summary>
        public DelegateType CreateMethod<DelegateType>(string statements) where DelegateType : class {
            return CreateMethod<DelegateType>(statements, null, defaultModule);
        }

        /// <summary>
        /// Create's a strongly typed delegate of type T bound to the default EngineModule.
        /// 
        /// The delegate calls a function which consists of the provided method body. If parameters
        /// is null the parameter names are taken from the delegate, otherwise the provided parameter
        /// names are used. 
        /// </summary>
        public DelegateType CreateMethod<DelegateType>(string statements, IList<string> parameters) where DelegateType : class {
            return CreateMethod<DelegateType>(statements, parameters, defaultModule);
        }

        /// <summary>
        /// Creates a strongly typed delegate of type T bound to the specifed EngineModule.
        /// 
        /// The delegate's parameter names will be available within the function as argument names.
        /// 
        /// Variable's that aren't locals will be retrived at run-time from the provided EngineModule.
        /// </summary>
        public DelegateType CreateMethod<DelegateType>(string statements, EngineModule engineModule) where DelegateType : class {
            return CreateMethod<DelegateType>(statements, null, engineModule);
        }

        /// <summary>
        /// Creates a strongly typed delegate of type T bound to the specified EngineModule.
        /// 
        /// The delegate calls a function which consists of the provided method body. If parameters
        /// is null the parameter names are taken from the delegate, otherwise the provided parameter
        /// names are used. 
        /// 
        /// Variables that aren't locals will be retrieved at run-time from the provided EngineModule.
        /// </summary>
        public DelegateType CreateMethod<DelegateType>(string statements, IList<string> parameters, EngineModule engineModule) where DelegateType : class {
            if (engineModule == null) throw new ArgumentNullException("engineModule");

            return CreateMethodUnscoped<DelegateType>(statements, parameters)(engineModule);
        }

        /// <summary>
        /// Creates a strongly typed delegate bound to an expression which returns a value.
        /// 
        /// The delegate's parameter names will be available within the function as locals
        /// </summary>
        public DelegateType CreateLambda<DelegateType>(string expression) where DelegateType : class {
            return CreateLambda<DelegateType>(expression, null, defaultModule);
        }

        /// <summary>
        /// Creates a strongly typed delegate bound to an expression which returns a value.
        /// 
        /// The delegate causes the given expression to be evaluated. If parameters is null then
        /// the delegate's parameter names are used for available locals, otherwise the given parameter
        /// names are used.
        /// </summary>
        public DelegateType CreateLambda<DelegateType>(string expression, IList<string> parameters) where DelegateType : class {
            return CreateLambda<DelegateType>(expression, parameters, defaultModule);
        }

        /// <summary>
        /// Creates a strongly typed delegate bound to an expression which returns a value.
        ///
        /// The delegate's parameter names will be available within the function as locals
        /// 
        /// Variable's that aren't localed will be retrieved at run-time from the provided EngineModule.
        /// </summary>
        public DelegateType CreateLambda<DelegateType>(string expression, EngineModule engineModule) where DelegateType : class {
            return CreateLambda<DelegateType>(expression, null, engineModule);
        }

        /// <summary>
        /// Creates a strongly typed delegate bound to an expression which returns a value.
        /// 
        /// The delegate causes the given expression to be evaluated. If parameters is null then
        /// the delegate's parameter names are used for available locals, otherwise the given parameter
        /// names are used.
        /// 
        /// Variable's that aren't localed will be retrieved at run-time from the provided EngineModule.
        /// </summary>
        public DelegateType CreateLambda<DelegateType>(string expression, IList<string> parameters, EngineModule engineModule) where DelegateType : class {
            if (engineModule == null) throw new ArgumentNullException("engineModule");

            return CreateLambdaUnscoped<DelegateType>(expression, parameters)(engineModule);
        }

        /// <summary>
        /// Creates a strongly typed delegate bound to an expression.
        /// </summary>
        public ModuleBinder<DelegateType> CreateMethodUnscoped<DelegateType>(string statements) where DelegateType : class {
            return CreateMethodUnscoped<DelegateType>(statements, null);
        }

        public ModuleBinder<DelegateType> CreateLambdaUnscoped<DelegateType>(string expression) where DelegateType : class {
            return CreateLambdaUnscoped<DelegateType>(expression, null);
        }

        /// <summary>
        /// Creates an unbound delegate that can be re-bound to a new EngineModule using
        /// the returned ModuleBinder.  The result of the re-bind is a strongly typed
        /// delegate that will execute in the provided EngineModule.
        ///
        /// The delegate calls a function which consists of the provided method body. If parameters
        /// is null the parameter names are taken from the delegate, otherwise the provided parameter
        /// names are used. 
        /// </summary>
        public ModuleBinder<DelegateType> CreateMethodUnscoped<DelegateType>(string statements, IList<string> parameters) where DelegateType : class {
            ValidateCreationParameters<DelegateType>();

            Parser p = Parser.FromString(Sys, compilerContext, statements);
            CodeGen cg = CreateDelegateWorker<DelegateType>(p.ParseFunction(), parameters);

            return delegate(EngineModule engineModule) {
                ModuleScope scope = GetModuleScope(engineModule, null);
                return cg.CreateDelegate(typeof(DelegateType), scope) as DelegateType;
            };
        }

#if DEBUG
        static int methodIndex;
#endif
        /// <summary>
        /// Creates an unbound delegate to an expression that can be re-bound to a new EngineModule using
        /// the returned ModuleBinder.  The result of the re-bind is a strongly typed
        /// delegate that will execute in the provided EngineModule.
        /// 
        /// The delegate causes the given expression to be evaluated. If parameters is null then
        /// the delegate's parameter names are used for available locals, otherwise the given parameter
        /// names are used.
        /// </summary>
        public ModuleBinder<DelegateType> CreateLambdaUnscoped<DelegateType>(string expression, IList<string> parameters) where DelegateType : class {
            ValidateCreationParameters<DelegateType>();

            Parser p = Parser.FromString(Sys, compilerContext, expression.TrimStart(' ', '\t'));
            Expression e = p.ParseTestListAsExpression();
            ReturnStatement ret = new ReturnStatement(e);
            int lineCnt = expression.Split('\n').Length;
            ret.SetLoc(new Location(lineCnt, 0), new Location(lineCnt, 10));
            CodeGen cg = CreateDelegateWorker<DelegateType>(ret, parameters);

            return delegate(EngineModule engineModule) {
                ModuleScope scope = GetModuleScope(engineModule, null);
                return cg.CreateDelegate(typeof(DelegateType), scope) as DelegateType;
            };
        }

        private static void ValidateCreationParameters<T>() where T : class {
            if (typeof(T) == typeof(MulticastDelegate) || typeof(T) == typeof(Delegate))
                throw new ArgumentException("T must be a concrete delegate, not MulticastDelegate or Delegate");
            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
                throw new ArgumentException("T must be a subclass of Delegate");
        }

        private CodeGen CreateDelegateWorker<DelegateType>(Statement s, IList<string> parameters) where DelegateType : class {
            NameExpression[] paramExpr = GetParameterExpressions<DelegateType>(parameters);
            FunctionDefinition fd = new FunctionDefinition(SymbolTable.Text, paramExpr, new Expression[0], FunctionAttributes.None, "<engine>");
            fd.Body = s;
            // create a method that corresponds w/ the delegate's signature - we also
            // add a 1st parameter which is the module scope, and we bind the method
            // against that.
            MethodInfo mi = typeof(DelegateType).GetMethod("Invoke");
            CodeGen cg = CreateTargetMethod(mi);
            List<ReturnFixer> fixers = PromoteArgumentsToLocals(paramExpr, cg);

            // bind the slots
            Compiler.Ast.Binder.Bind(fd, compilerContext);

            foreach (KeyValuePair<SymbolId, Compiler.Ast.Binding> kv in fd.Names) {
                Slot slot = cg.Names.Globals.GetOrMakeSlot(kv.Key);
                if (kv.Value.IsGlobal) {
                    cg.Names.SetSlot(kv.Key, slot);
                } else {
                    cg.Names.EnsureLocalSlot(kv.Key);
                }
            }

            // finally emit the function into the method, if we
            // have ref/out params then we wrap it in a try/finally
            // that updates them before the function ends.
            if (fixers != null) {
                cg.PushTryBlock();
                cg.BeginExceptionBlock();
            }

            fd.EmitFunctionImplementation(cg, null);

            if (fixers != null) {
                cg.PopTargets();
                Slot returnVar = cg.GetLocalTmp(typeof(bool));
                cg.PushFinallyBlock(returnVar);
                cg.BeginFinallyBlock();

                foreach (ReturnFixer rf in fixers) {
                    rf.FixReturn(cg);
                }

                cg.EndExceptionBlock();
                cg.PopTargets();
            }
            cg.Finish();
            return cg;
        }

        private static List<ReturnFixer> PromoteArgumentsToLocals(NameExpression[] paramExpr, CodeGen cg) {
            List<ReturnFixer> fixers = null;
            for (int i = 0; i < paramExpr.Length; i++) {
                ReturnFixer rf = CompilerHelpers.EmitArgument(cg, cg.GetArgumentSlot(i + 1));
                if (rf != null) {
                    if (fixers == null) fixers = new List<ReturnFixer>();
                    fixers.Add(rf);
                }
                Slot localSlot = cg.GetLocalTmp(typeof(object));
                localSlot.EmitSet(cg);
                cg.Names.SetSlot(paramExpr[i].Name, localSlot);
            }
            return fixers;
        }

        private CodeGen CreateTargetMethod(MethodInfo mi) {
            CodeGen cg;
#if DEBUG
            if (Options.SaveAndReloadBinaries) {
                TypeGen delegateGen = OutputGenerator.Snippets.DefinePublicType("DelegateGen$" + Interlocked.Increment(ref methodIndex).ToString(), typeof(object));

                cg = delegateGen.DefineUserHiddenMethod(MethodAttributes.Public | MethodAttributes.Static,
                    "pythonFunc" + Interlocked.Increment(ref methodIndex).ToString(),
                    mi.ReturnType,
                    PrependScope(mi.GetParameters()));
            } else
#endif
                cg = OutputGenerator.Snippets.DefineDynamicMethod("pythonFunction",
                    mi.ReturnType,
                    PrependScope(mi.GetParameters()));

            cg.ContextSlot = cg.GetArgumentSlot(0);
            cg.ModuleSlot = cg.ContextSlot;
            // setup the namespace for the method - get the arguments &
            // bind any globals into the module scope or ensure we have
            // a local slot for them.
            cg.Names = CodeGen.CreateLocalNamespace(cg);
            cg.Names.Globals = new GlobalEnvironmentNamespace(new EnvironmentNamespace(new GlobalEnvironmentFactory()), cg.ModuleSlot);
            cg.doNotCacheConstants = true;

            return cg;
        }

        private Type[] PrependScope(ParameterInfo[] pis) {
            Type[] res = new Type[pis.Length + 1];
            res[0] = typeof(ModuleScope);
            for (int i = 0; i < pis.Length; i++) {
                res[i + 1] = pis[i].ParameterType;
            }
            return res;
        }

        private NameExpression[] GetParameterExpressions<T>(IList<string> parameters) {
            NameExpression[] exprs;
            MethodInfo mi = typeof(T).GetMethod("Invoke");
            ParameterInfo[] pis = mi.GetParameters();
            if (parameters == null) {
                exprs = new NameExpression[pis.Length];
                for (int i = 0; i < pis.Length; i++) {
                    exprs[i] = new NameExpression(SymbolTable.StringToId(pis[i].Name));
                }
            } else {
                if (parameters.Count != pis.Length) {
                    throw new ArgumentException("delegate argument length and parameter name lengths differ");
                }

                exprs = new NameExpression[parameters.Count];
                for (int i = 0; i < exprs.Length; i++) {
                    exprs[i] = new NameExpression(SymbolTable.StringToId(parameters[i]));
                }
            }
            return exprs;
        }

        #endregion

        #endregion

        #region Compile
        public CompiledCode Compile(string scriptCode) {
            return Compile(scriptCode, null);
        }

        public CompiledCode Compile(string scriptCode, string sourceFileName) {
            if (scriptCode == null) throw new ArgumentNullException("scriptCode");

            // When a sourceFileName is passed in, it is used to generated debug info
            CompilerContext context = compilerContext;
            if (sourceFileName != null)
                context = compilerContext.CopyWithNewSourceFile(sourceFileName);

            Parser p = Parser.FromString(Sys, context, scriptCode);
            return Compile(p);
        }

        public CompiledCode CompileFile(string fileName) {
            if (fileName == null) throw new ArgumentException("fileName");
            Parser p = Parser.FromFile(Sys, compilerContext.CopyWithNewSourceFile(fileName));
            return Compile(p);
        }

        #endregion

        #region Dump
        public string FormatException(Exception exception) {
            object pythonEx = ExceptionConverter.ToPython(exception);

            string result = FormatStackTraces(exception);
            result += FormatPythonException(pythonEx);
            if (Sys.EngineOptions.ShowClrExceptions) {
                result += FormatCLSException(exception);
            }

            return result;
        }
        #endregion

#if COM_GAC_CRAWLER
        #region COM Support
        private void AssociateComInterface(object o, object i) {
            ComObject co = ComObject.ObjectToComObject(o);
            Type t = Converter.ConvertToType(i);
            if (co != null) {
                co.AddInterface(t);
            }
        }

        private void AssociateHiddenComInterface(Type visibleInterface, Type hiddenInterface) {
            lock (ComObject.HiddenInterfaces) {
                if (!ComObject.HiddenInterfaces.ContainsKey(visibleInterface))
                    ComObject.HiddenInterfaces[visibleInterface] = new List<Type>();

                ComObject.HiddenInterfaces[visibleInterface].Add(hiddenInterface);
            }
        }

        private void AssociateHiddenComAddInterface(Type visibleInterface, IList<Type> hiddenInterfaces) {
            lock (ComObject.HiddenInterfaces) {
                if (!ComObject.HiddenInterfaces.ContainsKey(visibleInterface))
                    ComObject.HiddenInterfaces[visibleInterface] = new List<Type>();

                foreach (Type hiddenInterface in hiddenInterfaces)
                    ComObject.HiddenInterfaces[visibleInterface].Add(hiddenInterface);
            }
        }
        #endregion
#endif

        #region IDisposable Members

        public void Dispose() {
            Dispose(false);
        }

        private void Dispose(bool finalizing) {
            // if we're finalizing we shouldn't access other managed objects, as
            // their finalizers may have already run            
            if (!finalizing) {
                if (stdIn != null) stdIn.Dispose();
                if (stdErr != null) stdErr.Dispose();
                if (stdOut != null) stdOut.Dispose();                
            }

            if (!finalizing || !AppDomain.CurrentDomain.IsFinalizingForUnload()) {
                // ClrModule holds onto SystemState, SystemState holds onto ClrModule...
                // no problem, right?  But there's a reference from AppDomain.CurrentDomain.AssemblyResolve
                // to ClrModule, which gives them both a static root.  Threfore it's safe
                // to dispose of systemState unless the domain is unloading.                
                systemState.Dispose();
            }
        }

        ~PythonEngine() {
            Dispose(true);
        }

        #endregion
    }

}

