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
using IronPython.Modules;
using IronPython.Runtime;

namespace IronPython.Hosting {

    /// <summary>
    /// Options to affect the PythonEngine.Execute* APIs. Note that these options only apply to
    /// the direct script code executed by the APIs, but not for other script code that happens
    /// to get called as a result of the execution.
    /// </summary>
    [Flags]
    public enum ExecutionOptions {
        Default             = 0x00,

        // If the statment is just an expression, print it. This is useful to interactive sessions
        // so that the host console does not need to determine if a statement is an expression or not.
        PrintExpressions    = 0x01, 

        // Enable CLI debugging. This allows debugging the script with a CLI debugger. Also, CLI exceptions
        // will have line numbers in the stack-trace.
        // Note that this is independent of the "traceback" Python module.
        // Also, the generated code will not be reclaimed, and so this should only be used for bounded number 
        // of executions.
        EnableDebugging     = 0x02,

        // Call RunInteractive after the script has executed. This is useful for interactive consoles
        // to inspect the scope of the script.
        Introspection       = 0x04,

        // Skip the first line of the code to execute. This is useful for Unix scripts which
        // have the command to execute specified in the first line.
        SkipFirstLine       = 0x08
    }

    public class PythonEngine {

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

        public static object ExecWrapper = null;

        #endregion

        #region Static Non-Public Members
        internal static Options options;
        #endregion

        #region Private Data Members
        private IConsole _console = null;
        private SystemState systemState = new SystemState();
        private Frame topFrame;
        private CompilerContext context = new CompilerContext("<stdin>");
        #endregion

        #region Constructor
        public PythonEngine() {
            // make sure cctor for OutputGenerator has run
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(OutputGenerator).TypeHandle);

            PythonModule mod = new PythonModule("__main__", new Dict(), Sys);
            topFrame = new Frame(mod);
            options = new Options();
        }

        public PythonEngine(Options opts)
            : this() {
            if (opts == null) 
                throw new ArgumentNullException("No options specified for PythonEngine");
            // Save the options. Clone it first to prevent the client from unexpectedly mutating it
            options = opts.Clone();
        }

        public void Shutdown() {
            object callable;
            if (Sys.TryGetAttr(topFrame, SymbolTable.SysExitFunc, out callable)) {
                try {
                    Ops.Call(callable);
                } catch (Exception e) {
                    MyConsole.WriteLine("", Style.Error);
                    MyConsole.WriteLine("Error in sys.exitfunc:", Style.Error);
                    MyConsole.Write(FormatException(e), Style.Error);
                }
            }

            DumpDebugInfo();
        }
        public void DumpDebugInfo() {
            if (PythonEngine.options.EngineDebug) {
                PerfTrack.DumpStats();
                try {
                    OutputGenerator.DumpSnippets();
                } catch (NotSupportedException) { } //!!! usually not important info...
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

        private const ExecutionOptions ExecuteStringOptions = ExecutionOptions.EnableDebugging | ExecutionOptions.PrintExpressions | ExecutionOptions.SkipFirstLine;
        private const ExecutionOptions ExecuteFileOptions = ExecutionOptions.EnableDebugging | ExecutionOptions.PrintExpressions | ExecutionOptions.SkipFirstLine;
        private const ExecutionOptions EvaluateStringOptions = ExecutionOptions.EnableDebugging | ExecutionOptions.PrintExpressions;

        private static void ValidateExecutionOptions(ExecutionOptions userOptions, ExecutionOptions permissibleOptions) {
            ExecutionOptions invalidOptions = userOptions & ~permissibleOptions;
            if (invalidOptions == 0)
                return;

            throw new ArgumentOutOfRangeException("executionOptions", userOptions, invalidOptions.ToString() + " is invalid");
        }

        private void ExecuteFileOptimized(string fileName, string moduleName, ExecutionOptions executionOptions, out Frame moduleScope) {
            CompilerContext context = new CompilerContext(fileName);
            bool skipLine = (executionOptions & ExecutionOptions.SkipFirstLine) != 0;
            Parser p = Parser.FromFile(Sys, context, skipLine, false);
            Stmt s = p.ParseFileInput();

            PythonModule mod = OutputGenerator.GenerateModule(Sys, context, s, moduleName);
            moduleScope = new Frame(mod);

            Sys.modules[mod.ModuleName] = mod;
            mod.SetAttr(mod, SymbolTable.File, fileName);

            mod.Initialize();
        }

        private int TryInteractiveAction(InteractiveAction interactiveAction, out bool continueInteraction) {
            int result = 1; // assume failure
            continueInteraction = false;

            try {
                result = interactiveAction(out continueInteraction);
            } catch (PythonSystemExit se) {
                return se.GetExitCode(topFrame);
            } catch (ThreadAbortException tae) {
                PythonKeyboardInterrupt pki = tae.ExceptionState as PythonKeyboardInterrupt;
                if (pki != null) {
                    Thread.ResetAbort();
                    bool endOfMscorlib = false;
                    string ex = FormatException(tae, ExceptionConverter.ToPython(pki), delegate(StackFrame sf) {
                        // filter out mscorlib methods that show up on the stack initially, 
                        // for example ReadLine / ReadBuffer etc...
                        if (!endOfMscorlib &&
                            sf.GetMethod().DeclaringType != null &&
                            sf.GetMethod().DeclaringType.Assembly == typeof(string).Assembly) {
                            return false;
                        }
                        endOfMscorlib = true;
                        return true;
                    });
                    MyConsole.Write(ex, Style.Error);
                    continueInteraction = true;
                }
            } catch (Exception e) {
                // There should be no unhandled exceptions in the interactive session
                // We catch all exceptions here, and just display it,
                // and keep on going
                MyConsole.Write(FormatException(e), Style.Error);
                continueInteraction = true;
            }

            return result;
        }

        private int RunInteractiveLoop() {
            bool continueInteraction = true;
            int result = 0;
            while (continueInteraction) {
                result = TryInteractiveAction(
                    delegate(out bool continueInteractionArgument) {
                        continueInteractionArgument = DoOneInteractive(topFrame);
                        return 0;
                    },
                    out continueInteraction);
            }

            return result;
        }

        private Stmt ParseInteractiveInput(string text, bool allowIncompleteStatement) {
            Parser p = Parser.FromString(Sys, context, text);
            return p.ParseInteractiveInput(allowIncompleteStatement);
        }

        private bool TreatAsBlankLine(string line, int autoIndentSize) {
            if (line.Length == 0) return true;
            if (autoIndentSize != 0 && line.Trim().Length == 0 && line.Length == autoIndentSize) {
                return true;
            }

            return false;
        }

        private Stmt ReadStatement(out bool continueInteraction) {
            StringBuilder b = new StringBuilder();
            int autoIndentSize = 0;

            MyConsole.Write(Sys.ps1.ToString(), Style.Prompt);

            while (true) {
                string line = MyConsole.ReadLine(autoIndentSize);
                continueInteraction = true;

                if (line == null) {
                    if (ExecWrapper != null) {
                        Ops.Call(ExecWrapper, new object[] { null });
                    }
                    continueInteraction = false;
                    return null;
                }

                bool allowIncompleteStatement = !TreatAsBlankLine(line, autoIndentSize);
                b.Append(line);
                b.Append("\n");

                if (b.ToString() == "\n" || (!allowIncompleteStatement && b.ToString().Trim().Length == 0)) return null;

                Stmt s = ParseInteractiveInput(b.ToString(), allowIncompleteStatement);
                if (s != null) return s;

                if (Options.AutoIndentSize != 0) {
                    autoIndentSize = Parser.GetNextAutoIndentSize(b.ToString(), Options.AutoIndentSize);
                }

                // Keep on reading input
                MyConsole.Write(Sys.ps2.ToString(), Style.Prompt);
            }
        }

        private string FormatPythonException(object python) {
            string result = "";

            // dump the python exception.
            if (python != null) {
                string str = python as string;
                if (str != null) {
                    result += str + Environment.NewLine;
                } else {
                    string className = "";
                    object val;
                    if (Ops.TryGetAttr(python, SymbolTable.Class, out val)) {
                        if (Ops.TryGetAttr(val, SymbolTable.Name, out val)) {
                            className = val.ToString();
                        }
                    }
                    result += className + ": " + python.ToString() + Environment.NewLine;
                }
            }

            return result;
        }

        private string FormatCLSException(Exception e) {
            string result = string.Empty;
            result += "CLR Exception: " + Environment.NewLine;

            while (e != null) {
                if (!String.IsNullOrEmpty(e.Message)) {
                    result += "    " + e.GetType().Name + ": " + e.Message + Environment.NewLine;
                } else {
                    result += "    " + e.GetType().Name + Environment.NewLine;
                }

                e = e.InnerException;
            }

            return result;
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
                List<System.Diagnostics.StackTrace> traces = ExceptionConverter.GetExceptionStackTraces(e);

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
                List<StackTrace> traces = ExceptionConverter.GetExceptionStackTraces(e);
                if (traces != null && traces.Count > 0) {
                    for (int i = 0; i < traces.Count; i++) {
                        result += FormatStackTrace(traces[i], fsf);
                    }
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

        private void ExecuteSnippet(Parser p, Frame moduleScope, ExecutionOptions executionOptions) {
            Stmt s = p.ParseFileInput();
            bool printExprStmts = (executionOptions & ExecutionOptions.PrintExpressions) != 0;
            bool enableDebugging = (executionOptions & ExecutionOptions.EnableDebugging) != 0;
            FrameCode code = OutputGenerator.GenerateSnippet(context, s, printExprStmts, enableDebugging);
            code.Run(moduleScope);
        }

        private delegate bool FilterStackFrame(StackFrame frame);

        private string FormatException(Exception e, object pythonException, FilterStackFrame fsf) {
            Debug.Assert(pythonException != null);
            Debug.Assert(e != null);

            string result = string.Empty;
            bool printedHeader = false;
            result += FormatStackTraces(e, fsf, ref printedHeader);
            result += FormatPythonException(pythonException);
            if (PythonEngine.options.ShowCLSExceptions) {
                result += FormatCLSException(e);
            }

            return result;
        }

        internal delegate int InteractiveAction(out bool continueInteraction);
        #endregion

        #region Console Support
        /// <summary>
        /// Create a new module scope and execute the given script, with optimizations enabled.
        /// Code cannot be optimized if the caller specifies a Frame. Hence, this API creates a new Frame itself.
        /// ExecutionOptions.EnableDebugging is implied.
        /// </summary>
        /// <param name="moduleScope">This is set to the new module scope which is created and used to execute the script.
        /// It will be null if a Frame could not be created (for eg, if the script contains a syntax error).
        /// It can be non-null even if an exception is thrown by the script.</param>
        public void ExecuteFileOptimized(string fileName, IEnumerable<string> commandLineArguments, ExecutionOptions executionOptions, out Frame moduleScope) {
            moduleScope = null;

            ValidateExecutionOptions(executionOptions, ExecuteFileOptions | ExecutionOptions.Introspection);

            // !!! It is invalid to call this API multiple times within the same PythonEngine.
            // To enforce this invariant, we could factor this out into a static method that instantiates a new PythonEngine.
            if ((Sys.argv as List).Count != 0)
                throw new InvalidOperationException("This API can be called just once on the same PythonEngine instance");

            // Publish the command line arguments
            if (commandLineArguments == null)
                Sys.argv = new List();
            else
                Sys.argv = new List(commandLineArguments);

            bool introspection = (executionOptions & ExecutionOptions.Introspection) != 0;

            if (introspection) {
                Frame scopeResult = null;
                bool continueInteraction;
                int result = TryInteractiveAction(
                    delegate(out bool continueInteractionArgument) {
                        ExecuteFileOptimized(fileName, "__main__", executionOptions, out scopeResult);
                        continueInteractionArgument = true;
                        return 0;
                    },
                    out continueInteraction);

                if (continueInteraction) {
                    if (scopeResult == null) {
                        // If there was an error generating a new module (for eg. because of a syntax error),
                        // we will just use the existing "topFrame"
                    } else {
                        topFrame = moduleScope = scopeResult;
                    }
                    RunInteractiveLoop();
                }
            } else {
                ExecuteFileOptimized(fileName, "__main__", executionOptions, out moduleScope);
            }
        }

        public IConsole MyConsole {
            get {
                if (_console == null) {
                    _console = new BasicConsole();
                }
                return _console;
            }
            set {
                _console = value;
            }
        }

        public bool DoOneInteractive(Frame topFrame) {
            bool continueInteraction;
            Stmt s = ReadStatement(out continueInteraction);
            if (continueInteraction == false)
                return false;

            //  's' is null when we parse a line composed only of a NEWLINE (interactive_input grammar);
            //  we don't generate anything when 's' is null
            if (s != null) {
                FrameCode code = OutputGenerator.GenerateSnippet(context, s, true, false);

                if (ExecWrapper != null) {
                    CallTarget0 t = delegate() {
                        try { code.Run(topFrame); } catch (Exception e) { MyConsole.Write(FormatException(e), Style.Error); }
                        return null;
                    };
                    object callable = new Function0(topFrame.__module__, "wrapper", t, new string[0], new object[0]);
                    Ops.Call(ExecWrapper, callable);
                } else {
                    code.Run(topFrame);
                }
            }

            return true;
        }

        public int RunInteractive() {
            topFrame.SetGlobal(SymbolTable.Doc, null);
            Sys.modules[topFrame.Module.ModuleName] = topFrame.Module;
            return RunInteractiveLoop();
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

        public void LoadAssembly(Assembly assem) {
            Sys.TopPackage.LoadAssembly(Sys, assem);
        }

        public object Import(string module) {
            object mod = Importer.ImportModule(topFrame, module, true);
            if (mod != null) {
                string[] names = module.Split('.');
                topFrame.SetGlobal(SymbolTable.StringToId(names[names.Length - 1]), mod);
            }
            return mod;
        }
        #endregion

        #region Helper functions for the engine. 
        internal static PythonEngine compiledEngine;

        // These are not part of the public hosting API even though its marked as public
        public static int ExecuteCompiled(InitializeModule init) {
            // first arg is EXE 
            List args = new List();
            string[] fullArgs = Environment.GetCommandLineArgs();
            args.Add(Path.GetFullPath(fullArgs[0]));
            for (int i = 1; i < fullArgs.Length; i++)
                args.Add(fullArgs[i]);
            compiledEngine.Sys.argv = args;

            try {
                init();
            } catch (PythonSystemExit x) {
                return x.GetExitCode(compiledEngine.topFrame);
            } catch (Exception e) {
                compiledEngine.MyConsole.Write(compiledEngine.FormatException(e), Style.Error);
                return -1;
            }
            return 0;
        }
        #endregion

        public static PythonModule InitializeModule(CustomSymbolDict compiled, string fullName, string []references) {
            if (compiledEngine == null) {
                compiledEngine = new PythonEngine();

                compiledEngine.Sys.prefix = System.IO.Path.GetDirectoryName(fullName);
                compiledEngine.Sys.executable = fullName;
                compiledEngine.Sys.exec_prefix = compiledEngine.Sys.prefix;
        
                compiledEngine.AddToPath(Environment.CurrentDirectory);
            }

            if (references != null) {
                for (int i = 0; i < references.Length; i++) {
                    compiledEngine.Sys.ClrModule.AddReference(references[i]);
                }            
            }

            compiledEngine.LoadAssembly(compiled.GetType().Assembly);
            PythonModule module = new PythonModule(fullName, compiled, compiledEngine.Sys);
            compiledEngine.Sys.modules[fullName] = module;
            module.InitializeBuiltins();
            return module;
        }

        #region IO Stream
        public void SetStderr(Stream stream) {
            Sys.__stderr__ = new PythonFile(stream, Sys.DefaultEncoding, "HostedStderr", "w");
            Sys.stderr = Sys.__stderr__;
        }


        public void SetStdout(Stream stream) {
            Sys.__stdout__ = new PythonFile(stream, Sys.DefaultEncoding, "HostedStdout", "w");
            Sys.stdout = Sys.__stdout__;
        }

        public void SetStdin(Stream stream) {
            Sys.__stdin__ = new PythonFile(stream, Sys.DefaultEncoding, "HostedStdin", "w");
            Sys.stdin = Sys.__stdin__;
        }
        #endregion

        #region Get\Set Variable
        public void SetVariable(string name, object val) {
            val = Ops.ToPython(val);
            Ops.SetAttr(topFrame, topFrame.Module, SymbolTable.StringToId(name), val);
        }

        public object GetVariable(string name) {
            return Ops.GetAttr(topFrame, topFrame.Module, SymbolTable.StringToId(name));
        }

        public Frame DefaultModuleScope { get { return topFrame; } }
        #endregion

        #region Dynamic Execution\Evaluation
        public void Execute(string text) {
            Execute(text, topFrame, ExecutionOptions.Default);
        }

        public void Execute(string text, Frame moduleScope, ExecutionOptions executionOptions) {
            ValidateExecutionOptions(executionOptions, ExecuteStringOptions);

            Parser p = Parser.FromString(((ICallerContext)moduleScope).SystemState, context, text);
            ExecuteSnippet(p, moduleScope, executionOptions);
        }

        public void ExecuteFile(string fileName) {
            ExecuteFile(fileName, topFrame, ExecutionOptions.Default);
        }

        public void ExecuteFile(string fileName, Frame moduleScope, ExecutionOptions executionOptions) {
            ValidateExecutionOptions(executionOptions, ExecuteFileOptions);

            Parser p = Parser.FromFile(Sys, context.CopyWithNewSourceFile(fileName));
            ExecuteSnippet(p, moduleScope, executionOptions);
        }

        public object Evaluate(string expr) {
            return Evaluate(expr, topFrame, ExecutionOptions.Default);
        }

        public object Evaluate(string expr, Frame moduleScope, ExecutionOptions executionOptions) {
            ValidateExecutionOptions(executionOptions, EvaluateStringOptions);

            return Builtin.Eval(
                moduleScope, 
                expr,
                ((ICallerContext)moduleScope).Globals,
                ((ICallerContext)moduleScope).Globals,
                executionOptions);
        }

        public T Evaluate<T>(string expr) {
            return Converter.Convert<T>(Evaluate(expr));
        }

        public T Evaluate<T>(string expr, Frame moduleScope, ExecutionOptions executionOptions) {
            return Converter.Convert<T>(Evaluate(expr, moduleScope, executionOptions));
        }
        #endregion

        #region Compile
        public void Execute(object code) {
            FrameCode fc = code as FrameCode;
            if (fc != null) {
                fc.Run(new Frame(topFrame.Module));
            } else if (code is string) {
                Execute((string)code);
            } else {
                throw new ArgumentException("code object must be string or have been generated via PythonEngine.Compile");
            }
        }

        public object Compile(string text) {
            return Compile(text, ExecutionOptions.Default);
        }

        public object Compile(string text, ExecutionOptions executionOptions) {
            ValidateExecutionOptions(executionOptions, ExecuteStringOptions);

            Parser p = Parser.FromString(Sys, context, text);
            Stmt s = p.ParseFileInput();

            bool printExprStmts = (executionOptions & ExecutionOptions.PrintExpressions) != 0;
            bool enableDebugging = (executionOptions & ExecutionOptions.EnableDebugging) != 0;
            return OutputGenerator.GenerateSnippet(context, s, printExprStmts, enableDebugging);
        }
        #endregion

        #region Dump
        public string FormatException(Exception e) {
            object pythonEx = ExceptionConverter.ToPython(e);

            string result = FormatStackTraces(e);
            result += FormatPythonException(pythonEx);
            if (PythonEngine.options.ShowCLSExceptions) {
                result += FormatCLSException(e);
            }

            return result;
        }
        #endregion

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
    }
}

