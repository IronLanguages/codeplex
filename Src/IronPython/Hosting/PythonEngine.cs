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
        private static int counter = 0;
        #endregion

        #region Private Data Members
        private IConsole _console = null;
        private Frame topFrame;
        private CompilerContext context = new CompilerContext("<stdin>");
        private EngineContext engineContext;
        #endregion

        #region Constructor
        public PythonEngine() {
            // make sure cctor for OutputGenerator has run
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(OutputGenerator).TypeHandle);

            engineContext = new EngineContext();
            topFrame = new Frame(engineContext.Module);
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
            if (Sys.TryGetAttr(engineContext, SymbolTable.SysExitFunc, out callable)) {
                try {
                    Ops.Call(callable);
                } catch (Exception e) {
                    MyConsole.WriteLine("", Style.Error);
                    MyConsole.WriteLine("Error in sys.exitfunc:", Style.Error);
                    DumpException(e);
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
        #endregion

        #region Non-Public Members
        private int RunFileInNewModule(string fileName, string moduleName, bool skipLine, out bool exitRaised) {
            CompilerContext context = new CompilerContext(fileName);
            Parser p = Parser.FromFile(engineContext.SystemState, context, skipLine, false);
            Stmt s = p.ParseFileInput();

            PythonModule mod = OutputGenerator.GenerateModule(engineContext.SystemState, context, s, moduleName);

            Sys.modules[mod.ModuleName] = mod;
            engineContext.ResetModule(mod);
            mod.SetAttr(engineContext, SymbolTable.File, fileName);
            exitRaised = false;

            try {
                mod.Initialize();
            } catch (PythonSystemExit e) {
                exitRaised = true;
                return e.GetExitCode(engineContext);
            }

            return 0;
        }

        private int TryInteractiveAction(InteractiveAction interactiveAction, out bool continueInteraction) {
            int result = 1; // assume failure
            continueInteraction = false;

            try {
                result = interactiveAction(out continueInteraction);
            } catch (PythonSystemExit se) {
                return se.GetExitCode(engineContext);
            } catch (ThreadAbortException tae) {
                PythonKeyboardInterrupt pki = tae.ExceptionState as PythonKeyboardInterrupt;
                if (pki != null) {
                    Thread.ResetAbort();
                    bool endOfMscorlib = false;
                    DumpException(tae, ExceptionConverter.ToPython(pki), delegate(StackFrame sf) {
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
                    continueInteraction = true;
                }
            } catch (Exception e) {
                // There should be no unhandled exceptions in the interactive session
                // We catch all exceptions here, and just display it,
                // and keep on going
                DumpException(e);
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
            Parser p = Parser.FromString(engineContext.SystemState, context, text);
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

        private void DumpPythonException(object python) {
            // dump the python exception.
            if (python != null) {
                string str = python as string;
                if (str != null) {
                    MyConsole.WriteLine(str, Style.Error);
                } else {
                    string className = "";
                    object val;
                    if (Ops.TryGetAttr(python, SymbolTable.Class, out val)) {
                        if (Ops.TryGetAttr(val, SymbolTable.Name, out val)) {
                            className = val.ToString();
                        }
                    }
                    MyConsole.WriteLine(className + ": " + python.ToString(), Style.Error);
                }
            }
        }

        private void DumpCLSException(Exception e) {
            MyConsole.WriteLine("CLR Exception: ", Style.Error);

            while (e != null) {
                if (!String.IsNullOrEmpty(e.Message)) {
                    MyConsole.WriteLine("    " + e.GetType().Name + ": " + e.Message, Style.Error);
                } else {
                    MyConsole.WriteLine("    " + e.GetType().Name, Style.Error);
                }

                e = e.InnerException;
            }
        }

        private void DumpStackTraces(Exception e) {
            bool printedHeader = false;

            DumpStackTraces(e, ref printedHeader);
        }

        private void DumpStackTraces(Exception e, ref bool printedHeader) {
            DumpStackTraces(e, null, ref printedHeader);
        }

        private void DumpStackTraces(Exception e, FilterStackFrame fsf, ref bool printedHeader) {
            if (PythonEngine.options.ExceptionDetail) {
                if (!printedHeader) {
                    MyConsole.WriteLine(e.Message, Style.Error);
                    printedHeader = true;
                }
                List<System.Diagnostics.StackTrace> traces = ExceptionConverter.GetExceptionStackTraces(e);

                if (traces != null) {
                    for (int i = 0; i < traces.Count; i++) {
                        for (int j = 0; j < traces[i].FrameCount; j++) {
                            StackFrame curFrame = traces[i].GetFrame(j);
                            if (fsf == null || fsf(curFrame))
                                MyConsole.WriteLine(curFrame.ToString(), Style.Error);
                        }
                    }
                }

                MyConsole.WriteLine(e.StackTrace.ToString(), Style.Error);
                if (e.InnerException != null) DumpStackTraces(e.InnerException, ref printedHeader);
            } else {
                // dump inner most exception first, followed by outer most.
                if (e.InnerException != null) DumpStackTraces(e.InnerException, ref printedHeader);

                if (!printedHeader) {
                    MyConsole.WriteLine("Traceback (most recent call last):", Style.Error);
                    printedHeader = true;
                }
                DumpStackTrace(new StackTrace(e, true), fsf);
                List<StackTrace> traces = ExceptionConverter.GetExceptionStackTraces(e);
                if (traces != null && traces.Count > 0) {
                    for (int i = 0; i < traces.Count; i++) {
                        DumpStackTrace(traces[i], fsf);
                    }
                }
            }
        }

        private void DumpStackTrace(StackTrace st, FilterStackFrame fsf) {
            StackFrame[] frames = st.GetFrames();
            if (frames == null) return;

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

                MyConsole.WriteLine(FrameToString(frame), Style.Error);
            }
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

        private int? Execute(Parser p, PythonModule m) {
            Stmt s = p.ParseFileInput();

            Frame topFrame = new Frame(m);
            FrameCode code = OutputGenerator.GenerateSnippet(context, s, false);

            try {
                code.Run(topFrame);
            } catch (PythonSystemExit e) {
                return e.GetExitCode(engineContext);
            }
            return null; // null indicates that the execution completed normally, without a PythonSystemExit being raised
        }

        internal delegate int InteractiveAction(out bool continueInteraction);
        #endregion

        #region Console Support
        public int RunFileInNewModule(string fileName, ArrayList commandLineArgs, bool introspection, bool skipLine) {
            // Publish the command line arguments
            Sys.argv = new List(commandLineArgs);

            if (introspection) {
                bool continueInteraction;
                int result = TryInteractiveAction(
                    delegate(out bool continueInteractionArgument) {
                        bool exitRaised;
                        int res = RunFileInNewModule(fileName, "__main__", skipLine, out exitRaised);
                        continueInteractionArgument = !exitRaised;
                        return res;
                    },
                    out continueInteraction);

                if (continueInteraction) {
                    topFrame = new Frame(engineContext.Module);
                    return RunInteractiveLoop();
                } else {
                    return result;
                }
            } else {
                bool exitRaised;
                return RunFileInNewModule(fileName, "__main__", skipLine, out exitRaised);
            }
        }

        public int ExecuteToConsole(string text) {
            Parser p = Parser.FromString(engineContext.SystemState, context, text);
            Stmt s = p.ParseFileInput();

            FrameCode code = OutputGenerator.GenerateSnippet(context, s, true);

            try {
                code.Run(new Frame(engineContext.Module));
            } catch (PythonSystemExit e) {
                return e.GetExitCode(engineContext);
            }
            return 0;
        }

        public void InitializeModules(string prefix, string executable, string version) {
            Sys.version = version;
            Sys.prefix = prefix;
            Sys.executable = executable;
            Sys.exec_prefix = prefix;
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
                FrameCode code = OutputGenerator.GenerateSnippet(context, s, true);

                if (ExecWrapper != null) {
                    CallTarget0 t = delegate() {
                        try { code.Run(topFrame); } catch (Exception e) { DumpException(e); }
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
            topFrame.SetGlobal("__doc__", null);
            Sys.modules[engineContext.Module.ModuleName] = engineContext.Module;
            return RunInteractiveLoop();
        }
        #endregion

        public SystemState Sys {
            get {
                return engineContext.SystemState;
            }
        }

        #region Loader Members
        public void AddToPath(string dirName) {
            Sys.path.Append(dirName);
        }

        public void LoadAssembly(Assembly assem) {
            engineContext.SystemState.TopPackage.LoadAssembly(engineContext.SystemState, assem);
        }

        public object Import(string module) {
            object mod = Importer.ImportModule(engineContext, module, true);
            if (mod != null) {
                string[] names = module.Split('.');
                topFrame.SetGlobal(names[names.Length - 1], mod);
            }
            return mod;
        }

        internal static PythonEngine compiledEngine;
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
                return x.GetExitCode(compiledEngine.engineContext);
            } catch (Exception e) {
                compiledEngine.DumpException(e);
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
                    compiledEngine.engineContext.SystemState.ClrModule.AddReference(references[i]);
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
            Sys.__stderr__ = new PythonFile(stream, engineContext.SystemState.DefaultEncoding, "HostedStderr", "w");
            Sys.stderr = Sys.__stderr__;
        }


        public void SetStdout(Stream stream) {
            Sys.__stdout__ = new PythonFile(stream, engineContext.SystemState.DefaultEncoding, "HostedStdout", "w");
            Sys.stdout = Sys.__stdout__;
        }

        public void SetStdin(Stream stream) {
            Sys.__stdin__ = new PythonFile(stream, engineContext.SystemState.DefaultEncoding, "HostedStdin", "w");
            Sys.stdin = Sys.__stdin__;
        }
        #endregion

        #region Get\Set Variable
        public void SetVariable(string name, object val) {
            val = Ops.ToPython(val);
            Ops.SetAttr(engineContext, engineContext.Module, SymbolTable.StringToId(name), val);
        }

        public object GetVariable(string name) {
            return Ops.GetAttr(engineContext, engineContext.Module, SymbolTable.StringToId(name));
        }
        #endregion

        #region Dynamic Execution\Evaluation
        public int Execute(string text) {
            Parser p = Parser.FromString(engineContext.SystemState, context, text);
            int? res = Execute(p, engineContext.Module);
            return (res == null) ? 0 : (int)res;
        }

        public int? ExecuteFile(string fileName) {
            Parser p = Parser.FromFile(engineContext.SystemState, context.CopyWithNewSourceFile(fileName));
            return Execute(p, engineContext.Module);
        }

        public int RunFile(string fileName) {
            Parser p = Parser.FromFile(engineContext.SystemState, context.CopyWithNewSourceFile(fileName));
            Stmt s = p.ParseFileInput();
            string moduleName = "tmp" + counter++;

            PythonModule mod = OutputGenerator.GenerateModule(engineContext.SystemState, p.CompilerContext, s, moduleName);
            foreach (KeyValuePair<SymbolId, object> name in engineContext.Module.__dict__.SymbolAttributes) {
                mod.SetAttr(engineContext, name.Key, name.Value);
            }
            mod.SetAttr(engineContext, SymbolTable.File, fileName);

            try {
                mod.Initialize();
            } catch (PythonSystemExit e) {
                return e.GetExitCode(engineContext);
            }

            return 0;
        }

        public object Evaluate(string expr) {
            return Builtin.Eval(engineContext, expr);
        }

        public T Evaluate<T>(string expr) {
            return Converter.Convert<T>(Builtin.Eval(engineContext, expr));
        }
        #endregion

        #region Compile
        public void Execute(object code) {
            FrameCode fc = code as FrameCode;
            if (fc != null) {
                fc.Run(new Frame(engineContext.Module));
            } else if (code is string) {
                Execute((string)code);
            } else {
                throw new ArgumentException("code object must be string or have been generated via PythonEngine.Compile");
            }
        }

        public object Compile(string text) {
            return Compile(text, false);
        }

        public object Compile(string text, bool printExprStatements) {
            Parser p = Parser.FromString(engineContext.SystemState, context, text);
            Stmt s = p.ParseFileInput();

            return OutputGenerator.GenerateSnippet(context, s, printExprStatements);
        }
        #endregion

        #region Dump
        public delegate bool FilterStackFrame(StackFrame frame);

        public void DumpException(Exception e) {
            object pythonEx = ExceptionConverter.ToPython(e);

            DumpStackTraces(e);
            DumpPythonException(pythonEx);
            if (PythonEngine.options.ShowCLSExceptions) {
                DumpCLSException(e);
            }
        }

        public void DumpException(Exception e, object pythonException, FilterStackFrame fsf) {
            Debug.Assert(pythonException != null);
            Debug.Assert(e != null);

            bool printedHeader = false;
            DumpStackTraces(e, fsf, ref printedHeader);
            DumpPythonException(pythonException);
            if (PythonEngine.options.ShowCLSExceptions) {
                DumpCLSException(e);
            }
        }
        #endregion

        #region COM Support
        public void AssociateComInterface(object o, object i) {
            ComObject co = ComObject.ObjectToComObject(o);
            Type t = Converter.ConvertToType(i);
            if (co != null) {
                co.AddInterface(t);
            }
        }

        public void AssociateHiddenComInterface(Type visibleInterface, Type hiddenInterface) {
            lock (ComObject.HiddenInterfaces) {
                if (!ComObject.HiddenInterfaces.ContainsKey(visibleInterface))
                    ComObject.HiddenInterfaces[visibleInterface] = new List<Type>();

                ComObject.HiddenInterfaces[visibleInterface].Add(hiddenInterface);
            }
        }

        public void AssociateHiddenComAddInterface(Type visibleInterface, IList<Type> hiddenInterfaces) {
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

