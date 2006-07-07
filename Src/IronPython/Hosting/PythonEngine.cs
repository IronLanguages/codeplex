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

    /// <summary>
    /// Options to affect the PythonEngine.Execute* APIs. Note that these options only apply to
    /// the direct script code executed by the APIs, but not for other script code that happens
    /// to get called as a result of the execution.
    /// </summary>
    [Flags]
    public enum ExecutionOptions {
        None = 0x00,

        // Enable CLI debugging. This allows debugging the script with a CLI debugger. Also, CLI exceptions
        // will have line numbers in the stack-trace.
        // Note that this is independent of the "traceback" Python module.
        // Also, the generated code will not be reclaimed, and so this should only be used for bounded number 
        // of executions.
        EnableDebugging = 0x02,

        // Skip the first line of the code to execute. This is useful for Unix scripts which
        // have the command to execute specified in the first line.
        SkipFirstLine = 0x08
    }

    public delegate T ScopeBinder<T>(ModuleScope scope);

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

        private ModuleScope defaultScope;

        private CompilerContext compilerContext = new CompilerContext("<stdin>");
        private PythonFile stdIn, stdOut, stdErr;

        #endregion

        #region Constructor
        public PythonEngine() {
            // make sure cctor for OutputGenerator has run
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(OutputGenerator).TypeHandle);

            defaultScope = new ModuleScope("__main__");
            systemState = new SystemState();
        }

        public PythonEngine(Options options)
            : this() {
            if (options == null)
                throw new ArgumentNullException("options", "No options specified for PythonEngine");
            // Save the options. Clone it first to prevent the client from unexpectedly mutating it
            PythonEngine.options = options.Clone();
        }

        public void Shutdown() {
            object callable;

            if (Sys.TryGetAttr(defaultScope, SymbolTable.SysExitFunc, out callable)) {
                Ops.Call(callable);
            }

            DumpDebugInfo();
        }

        public static void DumpDebugInfo() {
            if (PythonEngine.options.EngineDebug) {
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

        private const ExecutionOptions ExecuteStringOptions = ExecutionOptions.EnableDebugging | ExecutionOptions.SkipFirstLine;
        private const ExecutionOptions ExecuteFileOptions = ExecutionOptions.EnableDebugging | ExecutionOptions.SkipFirstLine;
        private const ExecutionOptions EvaluateStringOptions = ExecutionOptions.EnableDebugging;

        private static void ValidateExecutionOptions(ExecutionOptions userOptions, ExecutionOptions permissibleOptions) {
            ExecutionOptions invalidOptions = userOptions & ~permissibleOptions;
            if (invalidOptions == 0)
                return;

            throw new ArgumentOutOfRangeException("executionOptions", userOptions, invalidOptions.ToString() + " is invalid");
        }

        private void EnsureValidArguments(ModuleScope moduleScope, ExecutionOptions userOptions, ExecutionOptions permissibleOptions) {
            moduleScope.EnsureInitialized(Sys);
            ValidateExecutionOptions(userOptions, permissibleOptions);
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
            if (PythonEngine.options.ExceptionDetail) {
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

        private static void ExecuteSnippet(Parser p, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            Statement s = p.ParseFileInput();
            bool enableDebugging = (executionOptions & ExecutionOptions.EnableDebugging) != 0;
            CompiledCode compiledCode = OutputGenerator.GenerateSnippet(p.CompilerContext, s, false, enableDebugging);
            compiledCode.Run(moduleScope);
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
            if (PythonEngine.options.ShowClsExceptions) {
                result += FormatCLSException(exception);
            }

            return result;
        }
        #endregion

        #region Console Support
        /// <summary>
        /// Execute the code in a new module.
        /// The code is able to be optimized since it does not have to deal with any incoming ModuleScope,
        /// and it can optimize all global variable accesses.
        /// The caller is responsible for setting Sys.argv
        /// </summary>
        /// <param name="moduleName">If this is non-null, the module will be published to sys.modules</param>
        /// <param name="moduleScope">The resulting scope can be inspected using moduleScope. 
        /// Further code may be executed using this scope, and it will be able to access global variables that 
        /// were set in ExecuteFileOptimized. However, any further code run in this scope will not be optimized.</param>
        public void ExecuteFileOptimized(string fileName, string moduleName, ExecutionOptions executionOptions, out ModuleScope moduleScope) {
            CompilerContext context = this.compilerContext.CopyWithNewSourceFile(fileName);
            bool skipLine = (executionOptions & ExecutionOptions.SkipFirstLine) != 0;
            Parser p = Parser.FromFile(Sys, context, skipLine, false);
            Statement s = p.ParseFileInput();

            PythonModule mod = OutputGenerator.GenerateModule(Sys, context, s, moduleName);
            moduleScope = new ModuleScope(mod);

            if (moduleName != null)
                Sys.modules[mod.ModuleName] = mod;
            mod.SetAttr(mod, SymbolTable.File, fileName);

            mod.Initialize();
        }

        public delegate void CommandDispatcher(Delegate consoleCommand);

        // This can be set to a method like System.Windows.Forms.Control.Invoke for Winforms scenario 
        // to cause code to be executed on a separate thread.
        // It will be called with a null argument to indicate that the console session should be terminated.
        public static CommandDispatcher ConsoleCommandDispatcher {
            get { return consoleCommandDispatcher; }
            set { consoleCommandDispatcher = value; }
        }

        public void ExecuteToConsole(string text) { ExecuteToConsole(text, defaultScope); }
        public void ExecuteToConsole(string text, ModuleScope defaultScope) {
            defaultScope.EnsureInitialized(Sys);

            Parser p = Parser.FromString(((ICallerContext)defaultScope).SystemState, compilerContext, text);
            bool isEmptyStmt = false;
            Statement s = p.ParseInteractiveInput(false, out isEmptyStmt);

            //  's' is null when we parse a line composed only of a NEWLINE (interactive_input grammar);
            //  we don't generate anything when 's' is null
            if (s != null) {
                CompiledCode compiledCode = OutputGenerator.GenerateSnippet(compilerContext, s, true, false);
                Exception ex = null;

                if (consoleCommandDispatcher != null) {
                    CallTarget0 runCode = delegate() {
                        try { compiledCode.Run(defaultScope); } catch (Exception e) { ex = e; }
                        return null;
                    };

                    consoleCommandDispatcher(runCode);

                    // We catch and rethrow the exception since it could have been thrown on another thread
                    if (ex != null)
                        throw ex;
                } else {
                    compiledCode.Run(defaultScope);
                }
            }
        }

        public bool ParseInteractiveInput(string text, bool allowIncompleteStatement) {
            Parser p = Parser.FromString(Sys, compilerContext, text);
            return VerifyInteractiveInput(p, allowIncompleteStatement);
        }

        public static bool VerifyInteractiveInput(Parser parser, bool allowIncompleteStatement) {
            bool isEmptyStmt;
            Statement s = parser.ParseInteractiveInput(allowIncompleteStatement, out isEmptyStmt);

            if (s == null)
                return isEmptyStmt;

            return true;
        }
        #endregion

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

        public object Import(string module) {
            defaultScope.EnsureInitialized(Sys);

            object mod = Importer.ImportModule(defaultScope, module, true);
            if (mod != null) {
                string[] names = module.Split('.');
                defaultScope.SetGlobal(SymbolTable.StringToId(names[names.Length - 1]), mod);
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

        #region Get\Set Global
        public void SetGlobal(string name, object value) {
            if (name == null) throw new ArgumentNullException("name");

            SetGlobal(name, value, defaultScope);
        }

        public void SetGlobal(string name, object value, ModuleScope moduleScope) {
            if (name == null) throw new ArgumentNullException("name");

            moduleScope.EnsureInitialized(Sys);
            Ops.SetAttr(moduleScope, moduleScope.Module, SymbolTable.StringToId(name), value);
        }

        public object GetGlobal(string name) {
            if (name == null) throw new ArgumentNullException("name");

            return GetGlobal(name, defaultScope);
        }

        public object GetGlobal(string name, ModuleScope moduleScope) {
            if (name == null) throw new ArgumentNullException("name");
            if (moduleScope == null) throw new ArgumentNullException("moduleScope");

            moduleScope.EnsureInitialized(Sys);

            return Ops.GetAttr(moduleScope, moduleScope.Module, SymbolTable.StringToId(name));
        }

        public ModuleScope DefaultModuleScope { get { return defaultScope; } set { defaultScope = value; } }

        #endregion

        #region Dynamic Execution\Evaluation
        public void Execute(string text) {
            if (text == null) throw new ArgumentNullException("text");

            Execute(text, defaultScope, ExecutionOptions.None);
        }

        public void Execute(string text, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            if (text == null) throw new ArgumentNullException("text");
            if (moduleScope == null) moduleScope = defaultScope;

            Execute(text, null /*fileName*/, moduleScope, executionOptions);
        }

        /// <summary>
        /// Execute the Python code.
        /// The API will throw any exceptions raised by the code. If PythonSystemExit is thrown, the host should 
        /// interpret that in a way that is appropriate for the host.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fileName">This is used for scenarios when a file contains embedded Python code, along with
        /// non-Python code. The host can creating a string with just the Python code, with empty lines in place 
        /// of the non-Python code. The fileName will then be used for debugging and traceback information</param>
        /// <param name="moduleScope">The module context to execute the code in.</param>
        public void Execute(string text, string fileName, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            EnsureValidArguments(moduleScope, executionOptions, ExecuteStringOptions);

            CompilerContext context = compilerContext;

            // When a fileName is passed in, it is used to generated debug info
            if (fileName != null)
                context = compilerContext.CopyWithNewSourceFile(fileName);

            Parser p = Parser.FromString(((ICallerContext)moduleScope).SystemState, context, text);
            ExecuteSnippet(p, moduleScope, executionOptions);
        }

        public void ExecuteFile(string fileName) {
            ExecuteFile(fileName, defaultScope, ExecutionOptions.None);
        }

        public void ExecuteFile(string fileName, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            EnsureValidArguments(moduleScope, executionOptions, ExecuteFileOptions);

            Parser p = Parser.FromFile(Sys, compilerContext.CopyWithNewSourceFile(fileName));
            ExecuteSnippet(p, moduleScope, executionOptions);
        }

        public object Evaluate(string expression) {
            return Evaluate(expression, defaultScope, ExecutionOptions.None);
        }

        public object Evaluate(string expression, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            EnsureValidArguments(moduleScope, executionOptions, EvaluateStringOptions);

            return Builtin.Eval(moduleScope, expression, executionOptions);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T EvaluateAs<T>(string expression) {
            return Converter.Convert<T>(Evaluate(expression));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T EvaluateAs<T>(string expression, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            return Converter.Convert<T>(Evaluate(expression, moduleScope, executionOptions));
        }

        /// <summary>
        /// Create's a strongly typed delegate of type T bound to the default module scope.
        /// 
        /// The delegate's parameter names will be available within the function as argument names.
        /// </summary>
        public DelegateType CreateMethod<DelegateType>(string statements) where DelegateType : class {
            return CreateMethod<DelegateType>(statements, null, defaultScope);
        }

        /// <summary>
        /// Create's a strongly typed delegate of type T bound to the default module scope.
        /// 
        /// The delegate calls a function which consists of the provided method body. If parameters
        /// is null the parameter names are taken from the delegate, otherwise the provided parameter
        /// names are used. 
        /// </summary>
        public DelegateType CreateMethod<DelegateType>(string statements, IList<string> parameters) where DelegateType : class {
            return CreateMethod<DelegateType>(statements, parameters, defaultScope);
        }

        /// <summary>
        /// Creates a strongly typed delegate of type T bound to the specifed module scope.
        /// 
        /// The delegate's parameter names will be available within the function as argument names.
        /// 
        /// Variable's that aren't locals will be retrived at run-time from the provided module scope.
        /// </summary>
        public DelegateType CreateMethod<DelegateType>(string statements, ModuleScope scope) where DelegateType : class {
            return CreateMethod<DelegateType>(statements, null, scope);
        }

        /// <summary>
        /// Creates a strongly typed delegate of type T bound to the specified module scope.
        /// 
        /// The delegate calls a function which consists of the provided method body. If parameters
        /// is null the parameter names are taken from the delegate, otherwise the provided parameter
        /// names are used. 
        /// 
        /// Variables that aren't locals will be retrieved at run-time from the provided module scope.
        /// </summary>
        public DelegateType CreateMethod<DelegateType>(string statements, IList<string> parameters, ModuleScope scope) where DelegateType : class {
            if (scope == null) scope = defaultScope;

            return CreateMethodUnscoped<DelegateType>(statements, parameters)(scope);
        }

        /// <summary>
        /// Creates a strongly typed delegate bound to an expression which returns a value.
        /// 
        /// The delegate's parameter names will be available within the function as locals
        /// </summary>
        public DelegateType CreateLambda<DelegateType>(string expression) where DelegateType : class {
            return CreateLambda<DelegateType>(expression, null, defaultScope);
        }

        /// <summary>
        /// Creates a strongly typed delegate bound to an expression which returns a value.
        /// 
        /// The delegate causes the given expression to be evaluated. If parameters is null then
        /// the delegate's parameter names are used for available locals, otherwise the given parameter
        /// names are used.
        /// </summary>
        public DelegateType CreateLambda<DelegateType>(string expression, IList<string> parameters) where DelegateType : class {
            return CreateLambda<DelegateType>(expression, parameters, defaultScope);
        }

        /// <summary>
        /// Creates a strongly typed delegate bound to an expression which returns a value.
        ///
        /// The delegate's parameter names will be available within the function as locals
        /// 
        /// Variable's that aren't localed will be retrieved at run-time from the provided module scope.
        /// </summary>
        public DelegateType CreateLambda<DelegateType>(string expression, ModuleScope scope) where DelegateType : class {
            return CreateLambda<DelegateType>(expression, null, scope);
        }

        /// <summary>
        /// Creates a strongly typed delegate bound to an expression which returns a value.
        /// 
        /// The delegate causes the given expression to be evaluated. If parameters is null then
        /// the delegate's parameter names are used for available locals, otherwise the given parameter
        /// names are used.
        /// 
        /// Variable's that aren't localed will be retrieved at run-time from the provided module scope.
        /// </summary>
        public DelegateType CreateLambda<DelegateType>(string expression, IList<string> parameters, ModuleScope scope) where DelegateType : class {
            if (scope == null) scope = defaultScope;

            return CreateLambdaUnscoped<DelegateType>(expression, parameters)(scope);
        }

        /// <summary>
        /// Creates a strongly typed delegate bound to an expression.
        /// </summary>
        public ScopeBinder<DelegateType> CreateMethodUnscoped<DelegateType>(string statements) where DelegateType : class {
            return CreateMethodUnscoped<DelegateType>(statements,null);
        }

        public ScopeBinder<DelegateType> CreateLambdaUnscoped<DelegateType>(string expression) where DelegateType : class {
            return CreateLambdaUnscoped<DelegateType>(expression, null);
        }

        /// <summary>
        /// Creates an unbound delegate that can be re-bound to a new module scope using
        /// the returned ScopeBinder.  The result of the re-bind is a strongly typed
        /// delegate that will execute in the provided scope.
        ///
        /// The delegate calls a function which consists of the provided method body. If parameters
        /// is null the parameter names are taken from the delegate, otherwise the provided parameter
        /// names are used. 
        /// </summary>
        public ScopeBinder<DelegateType> CreateMethodUnscoped<DelegateType>(string statements, IList<string> parameters) where DelegateType : class {
            ValidateCreationParameters<DelegateType>();

            Parser p = Parser.FromString(Sys, compilerContext, statements);
            CodeGen cg = CreateDelegateWorker<DelegateType>(p.ParseFunction(), parameters);
            
            return delegate(ModuleScope scope) {
                scope.EnsureInitialized(Sys);
                return cg.CreateDelegate(typeof(DelegateType), scope) as DelegateType;
            };
        }

#if DEBUG
        static int methodIndex;
#endif
        /// <summary>
        /// Creates an unbound delegate to an expression that can be re-bound to a new module scope using
        /// the returned ScopeBinder.  The result of the re-bind is a strongly typed
        /// delegate that will execute in the provided scope.
        /// 
        /// The delegate causes the given expression to be evaluated. If parameters is null then
        /// the delegate's parameter names are used for available locals, otherwise the given parameter
        /// names are used.
        /// </summary>
        public ScopeBinder<DelegateType> CreateLambdaUnscoped<DelegateType>(string expression, IList<string> parameters) where DelegateType : class {
            ValidateCreationParameters<DelegateType>();

            Parser p = Parser.FromString(Sys, compilerContext, expression.TrimStart(' ', '\t'));
            Expression e = p.ParseTestListAsExpression();
            ReturnStatement ret = new ReturnStatement(e);
            int lineCnt = expression.Split('\n').Length;
            ret.SetLoc(new Location(lineCnt, 0), new Location(lineCnt, 10));
            CodeGen cg = CreateDelegateWorker<DelegateType>(ret, parameters);
            
            return delegate(ModuleScope scope) {
                scope.EnsureInitialized(Sys);
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

        #region Compile
        public void Execute(CompiledCode compiledCode) {
            Execute(compiledCode, defaultScope);
        }

        public void Execute(CompiledCode compiledCode, ModuleScope moduleScope) {
            moduleScope.EnsureInitialized(Sys);
            compiledCode.Run(moduleScope);
        }

        public CompiledCode Compile(string text) {
            return Compile(text, ExecutionOptions.None);
        }

        /// <returns>This can be used with the "Execute(CompiledCode compiledCode)" API.
        /// The same CompiledCode can be used in multiple PythonEngines.</returns>
        public CompiledCode Compile(string text, ExecutionOptions executionOptions) {
            ValidateExecutionOptions(executionOptions, ExecuteStringOptions);

            Parser p = Parser.FromString(Sys, compilerContext, text);
            return Compile(p, executionOptions);
        }

        public CompiledCode CompileFile(string fileName) {
            return CompileFile(fileName, ExecutionOptions.None);
        }

        public CompiledCode CompileFile(string fileName, ExecutionOptions executionOptions) {
            ValidateExecutionOptions(executionOptions, ExecuteStringOptions);

            Parser p = Parser.FromFile(Sys, compilerContext.CopyWithNewSourceFile(fileName));
            return Compile(p, executionOptions);
        }

        private static CompiledCode Compile(Parser p, ExecutionOptions executionOptions) {
            Statement s = p.ParseFileInput();

            bool enableDebugging = (executionOptions & ExecutionOptions.EnableDebugging) != 0;
            return OutputGenerator.GenerateSnippet(p.CompilerContext, s, false, enableDebugging);
        }
        #endregion

        #region Dump
        public string FormatException(Exception exception) {
            object pythonEx = ExceptionConverter.ToPython(exception);

            string result = FormatStackTraces(exception);
            result += FormatPythonException(pythonEx);
            if (PythonEngine.options.ShowClsExceptions) {
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
            if (!finalizing) {
                if (stdIn != null) stdIn.Dispose();
                if (stdErr != null) stdErr.Dispose();
                if (stdOut != null) stdOut.Dispose();
            }
        }

        ~PythonEngine() {
            Dispose(true);
        }

        #endregion
    }

}

