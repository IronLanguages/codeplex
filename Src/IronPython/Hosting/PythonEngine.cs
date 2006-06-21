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
using IronPython.Compiler.AST;
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
        Default             = 0x00,

        // Enable CLI debugging. This allows debugging the script with a CLI debugger. Also, CLI exceptions
        // will have line numbers in the stack-trace.
        // Note that this is independent of the "traceback" Python module.
        // Also, the generated code will not be reclaimed, and so this should only be used for bounded number 
        // of executions.
        EnableDebugging     = 0x02,

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

        #endregion

        #region Static Non-Public Members
        internal static Options options;
        private static CommandDispatcher consoleCommandDispatcher;
        #endregion

        #region Private Data Members
        // Every PythonEngine has its own SystemState. This allows multiple engines to exist simultaneously,
        // while maintaingin unique significant state for every engine..
        private SystemState systemState;

        private ModuleScope defaultScope;
        
        private CompilerContext compilerContext = new CompilerContext("<stdin>");
        #endregion

        #region Constructor
        public PythonEngine() {
            // make sure cctor for OutputGenerator has run
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(OutputGenerator).TypeHandle);

            defaultScope = new ModuleScope("__main__");
            options = new Options();
            systemState = new SystemState();
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

            if (Sys.TryGetAttr(defaultScope, SymbolTable.SysExitFunc, out callable)) {
                Ops.Call(callable);
            }

            DumpDebugInfo();
        }
        public void DumpDebugInfo() {
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
            ValidateExecutionOptions(userOptions, ExecuteFileOptions);
        }

        private string FormatPythonException(object python) {
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

        private void ExecuteSnippet(Parser p, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            Stmt s = p.ParseFileInput();
            bool enableDebugging = (executionOptions & ExecutionOptions.EnableDebugging) != 0;
            CompiledCode compiledCode = OutputGenerator.GenerateSnippet(p.CompilerContext, s, false, enableDebugging);
            compiledCode.Run(moduleScope);
        }

        public delegate bool FilterStackFrame(StackFrame frame);

        // TODO: Make private
        public string FormatException(Exception e, object pythonException, FilterStackFrame fsf) {
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
        #endregion

        #region Console Support
        public void ExecuteFileOptimized(string fileName, string moduleName, ExecutionOptions executionOptions, out ModuleScope moduleScope) {
            CompilerContext context = this.compilerContext.CopyWithNewSourceFile(fileName);
            bool skipLine = (executionOptions & ExecutionOptions.SkipFirstLine) != 0;
            Parser p = Parser.FromFile(Sys, context, skipLine, false);
            Stmt s = p.ParseFileInput();

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
            Stmt s = p.ParseInteractiveInput(false);

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
            return p.ParseInteractiveInput(allowIncompleteStatement) != null;
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

        #region Get\Set Global
        public void SetGlobal(SymbolId name, object val) {
            SetGlobal(name, val, defaultScope);
        }

        public void SetGlobal(SymbolId name, object val, ModuleScope moduleScope) {
            moduleScope.EnsureInitialized(Sys);
            Ops.SetAttr(moduleScope, moduleScope.Module, name, val);
        }

        public object GetGlobal(SymbolId name) {
            return GetGlobal(name, defaultScope);
        }

        public object GetGlobal(SymbolId name, ModuleScope moduleScope) {
            moduleScope.EnsureInitialized(Sys);

            return Ops.GetAttr(moduleScope, moduleScope.Module, name);
        }

        public ModuleScope DefaultModuleScope { get { return defaultScope; } set { defaultScope = value; } }

        #endregion

        #region Dynamic Execution\Evaluation
        public void Execute(string text) {
            Execute(text, defaultScope, ExecutionOptions.Default);
        }

        public void Execute(string text, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            EnsureValidArguments(moduleScope, executionOptions, ExecuteStringOptions);

            Parser p = Parser.FromString(((ICallerContext)moduleScope).SystemState, compilerContext, text);
            ExecuteSnippet(p, moduleScope, executionOptions);
        }

        public void ExecuteFile(string fileName) {
            ExecuteFile(fileName, defaultScope, ExecutionOptions.Default);
        }

        public void ExecuteFile(string fileName, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            EnsureValidArguments(moduleScope, executionOptions, ExecuteFileOptions);

            Parser p = Parser.FromFile(Sys, compilerContext.CopyWithNewSourceFile(fileName));
            ExecuteSnippet(p, moduleScope, executionOptions);
        }

        public object Evaluate(string expr) {
            return Evaluate(expr, defaultScope, ExecutionOptions.Default);
        }

        public object Evaluate(string expr, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            EnsureValidArguments(moduleScope, executionOptions, EvaluateStringOptions);

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

        public T Evaluate<T>(string expr, ModuleScope moduleScope, ExecutionOptions executionOptions) {
            return Converter.Convert<T>(Evaluate(expr, moduleScope, executionOptions));
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
            return Compile(text, ExecutionOptions.Default);
        }

        public CompiledCode Compile(string text, ExecutionOptions executionOptions) {
            ValidateExecutionOptions(executionOptions, ExecuteStringOptions);

            Parser p = Parser.FromString(Sys, compilerContext, text);
            return Compile(p, executionOptions);
        }

        public CompiledCode CompileFile(string fileName) {
            return CompileFile(fileName, ExecutionOptions.Default);
        }

        public CompiledCode CompileFile(string fileName, ExecutionOptions executionOptions) {
            ValidateExecutionOptions(executionOptions, ExecuteStringOptions);

            Parser p = Parser.FromFile(Sys, compilerContext.CopyWithNewSourceFile(fileName));
            return Compile(p, executionOptions);
        }

        private CompiledCode Compile(Parser p, ExecutionOptions executionOptions) {
            Stmt s = p.ParseFileInput();

            bool enableDebugging = (executionOptions & ExecutionOptions.EnableDebugging) != 0;
            return OutputGenerator.GenerateSnippet(p.CompilerContext, s, false, enableDebugging);
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

