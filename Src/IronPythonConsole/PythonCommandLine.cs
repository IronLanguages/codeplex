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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using SystemThread = System.Threading.Thread;

using IronPython.Hosting;
using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;
using System.Diagnostics;

namespace IronPythonConsole {
    /// <summary>
    /// A simple Python command-line should mimic the standard python.exe
    /// </summary>
    class PythonCommandLine {
        private static bool handleExceptions = true;
        private static bool tabCompletion = false;
        private static bool autoIndent = false;
        private static bool colorfulConsole = false;
        private static bool mta = false;
        private static PythonEngine engine;

        public static bool TabCompletion {
            get {
                return tabCompletion;
            }
            set {
                tabCompletion = value;
            }
        }

        public static bool AutoIndent {
            get {
                return autoIndent;
            }
            set {
                autoIndent = value;
            }
        }

        public static bool ColorfulConsole {
            get {
                return colorfulConsole;
            }
            set {
                colorfulConsole = value;
            }
        }

        private class MTAParameters {
            public readonly PythonEngine engine;
            public readonly List<string> args;
            public int result;

            public MTAParameters(PythonEngine engine, List<string> args) {
                this.engine = engine;
                this.args = args;
            }
        }

        [MTAThread]
        private static void MTAThread(object obj) {
            MTAParameters p = obj as MTAParameters;
            if (p != null) {
                if (p.args.Count > 0)
                    p.result = Run(p.engine, p.args[0]);
                else
                    p.result = Run(p.engine, null);
            }
        }

        [STAThread]
        static int Main(string[] rawArgs) {
            List<string> args = new List<string>(rawArgs);
            Options options = ParseOptions(args);

            if (Options.PrintVersionAndExit) {
                Console.WriteLine(PythonEngine.VersionString);
                return 0;
            }

            engine = new PythonEngine(options);

            try {
#if IRONPYTHON_WINDOW
                MyConsole = new BasicConsole(engine.Sys, ColorfulConsole);
#else
                if (TabCompletion) {
                    UseSuperConsole(engine);
                } else {
                    MyConsole = new BasicConsole(engine.Sys, ColorfulConsole);
                }
#endif
                if (Options.WarningFilters != null)
                    engine.Sys.warnoptions = IronPython.Runtime.List.Make(Options.WarningFilters);

                engine.Sys.SetRecursionLimit(Options.MaximumRecursion);

                // Publish the command line arguments
                if (args == null)
                    engine.Sys.argv = new List();
                else
                    engine.Sys.argv = List.Make(args);


                if (mta) {
                    MTAParameters p = new MTAParameters(engine, args);
                    SystemThread thread = new SystemThread(MTAThread);
                    thread.SetApartmentState(ApartmentState.MTA);
                    thread.Start(p);
                    thread.Join();
                    return p.result;
                } else {
                    return Run(engine, args == null ? null : args.Count > 0 ? args[0] : null);
                }
            } finally {
                try {
                    engine.Shutdown();
                } catch (Exception e) {
                    MyConsole.WriteLine("", Style.Error);
                    MyConsole.WriteLine("Error in sys.exitfunc:", Style.Error);
                    MyConsole.Write(engine.FormatException(e), Style.Error);
                }

                PythonEngine.DumpDebugInfo();
            }
        }

        private static int Run(PythonEngine engine, string fileName) {
            try {
                if (Options.Command != null) {
                    return RunString(engine, Options.Command);
                } else if (fileName == null) {
#if !IRONPYTHON_WINDOW
                    return RunInteractive(engine);
#else
                    return 0;
#endif
                } else {
                    return RunFile(engine, fileName);
                }
            } catch (System.Threading.ThreadAbortException tae) {
                if (tae.ExceptionState is PythonKeyboardInterruptException) {
                    Thread.ResetAbort();
                }
                return -1;
            }
        }

        private static Options ParseOptions(List<string> args) {
            Options options = new Options();

            while (args.Count > 0) {
                switch ((string)args[0]) {
                    case "-O": options.DebugMode = false; break;
                    case "-D": options.EngineDebug = true; break;

                    // the following extension switches are in alphabetic order
                    case "-X:AssembliesDir":
                        args.RemoveAt(0);
                        if (args.Count == 0 || Directory.Exists((string)args[0]) == false) {
                            PrintUsageAndExit();
                        }
                        Options.BinariesDirectory = (string)args[0];
                        break;
                    case "-X:FastEval": Options.FastEvaluation = true; break;
                    case "-X:Frames": Options.Frames = true; break;
                    case "-X:GenerateAsSnippets": Options.GenerateModulesAsSnippets = true; break;
                    case "-X:ILDebug": Options.ILDebug = true; break;
                    case "-c":
                        args.RemoveAt(0);
                        if (args.Count == 0) {
                            PrintUsageAndExit();
                        }
                        Options.Command = (string)args[0];
                        return options;
                    case "-X:PassExceptions": handleExceptions = false; break;
#if !IRONPYTHON_WINDOW
                    case "-X:ColorfulConsole": ColorfulConsole = true; break;
                    case "-X:ExceptionDetail": options.ExceptionDetail = true; break;
                    case "-X:TabCompletion": TabCompletion = true; break;
                    case "-X:AutoIndent": AutoIndent = true; break;
                    case "-i": Options.Introspection = true; break;
                    case "-V":
                        Options.PrintVersionAndExit = true;
                        return options;
#endif
                    case "-X:MTA": mta = true; break;
                    case "-X:NoOptimize": Options.OptimizeReflectCalls = false; break;
                    case "-X:NoTraceback": Options.TraceBackSupport = false; break;
                    case "-X:MaxRecursion":
                        args.RemoveAt(0);
                        int maxRecurVal = 0;
                        if (args.Count == 0 || !Int32.TryParse((string)args[0], out maxRecurVal)) {
                            PrintUsageAndExit();
                        }
                        Options.MaximumRecursion = maxRecurVal;
                        break;
                    case "-X:PrivateBinding": Options.PrivateBinding = true; break;
                    case "-X:Python25": Options.Python25 = true; break;
                    case "-X:SaveAssemblies": Options.SaveAndReloadBinaries = true; break;
                    case "-X:ShowCLSExceptions": options.ShowClsExceptions = true; break;
                    case "-X:StaticMethods": Options.GenerateDynamicMethods = false; break;
                    case "-X:TrackPerformance": // accepted but ignored on retail builds
#if DEBUG
                        Options.TrackPerformance = true;
#endif
                        break;
                    case "-x": Options.SkipFirstLine = true; break;
                    case "-v": options.Verbose = true; break;
                    case "-u": Options.BufferedStandardOutAndError = false; break;
                    case "-S":
                        Options.ImportSite = false;
                        break;
                    case "-E":
                        Options.IgnoreEnvironmentVariables = true;
                        break;
                    case "-t":
                        Options.WarningOnIndentationInconsistency = true;
                        break;
                    case "-tt":
                        Options.ErrorOnIndentationInconsistency = true;
                        break;
                    case "-OO":
                        options.DebugMode = false;
                        Options.StripDocStrings = true;
                        break;
                    case "-Q":
                        args.RemoveAt(0);
                        if (args.Count == 0) {
                            PrintUsageAndExit();
                        }
                        switch ((string)args[0]) {
                            case "old": Options.Division = DivisionOption.Old; break;
                            case "new": Options.Division = DivisionOption.New; break;
                            case "warn": Options.Division = DivisionOption.Warn; break;
                            case "warnall": Options.Division = DivisionOption.WarnAll; break;
                            default: PrintUsageAndExit(); break;
                        }
                        break;
                    case "-Qold": Options.Division = DivisionOption.Old; break;
                    case "-Qnew": Options.Division = DivisionOption.New; break;
                    case "-Qwarn": Options.Division = DivisionOption.Warn; break;
                    case "-Qwarnall": Options.Division = DivisionOption.WarnAll; break;
                    case "-W":
                        args.RemoveAt(0);
                        if (args.Count == 0) {
                            PrintUsageAndExit();
                        }
                        if (Options.WarningFilters == null)
                            Options.WarningFilters = new List<string>();
                        Options.WarningFilters.Add((string)args[0]);
                        break;
                    case "-h":
                    case "-help":
                    case "-?":
                        PrintUsageAndExit();
                        break;
                    default: return options;
                }
                args.RemoveAt(0);
            }
            return options;
        }

        private static void PrintUsageAndExit() {
            PrintUsage();
            Environment.Exit(0);
        }

        private static void PrintUsage() {
            Console.WriteLine("IronPython console: " + PythonEngine.VersionString);
            Console.WriteLine(PythonEngine.Copyright);
            Console.WriteLine("Usage: ipy [options] [file.py|- [arguments]]");
            Console.WriteLine("Options:");
            Console.WriteLine("  -O:                    Enable optimizations");
#if DEBUG
            Console.WriteLine("  -D                     EngineDebug mode");
#endif
            // the following extension switches should be printed in alphabetic order
            Console.WriteLine("  -X:AssembliesDir       Set the directory for saving generated assemblies");
#if !IRONPYTHON_WINDOW
            Console.WriteLine("  -X:AutoIndent          Automatically insert indentation");
            Console.WriteLine("  -X:ColorfulConsole     Enable ColorfulConsole");
            Console.WriteLine("  -X:ExceptionDetail     Enable ExceptionDetail mode");
#endif
            Console.WriteLine("  -X:FastEval            Enable fast eval");
            Console.WriteLine("  -X:Frames              Generate custom frames");
            Console.WriteLine("  -X:GenerateAsSnippets  Generate code to run in snippet mode");
            Console.WriteLine("  -X:ILDebug             Output generated IL code to a text file for debugging");
            Console.WriteLine("  -X:MaxRecursion        Set the maximum recursion level");
            Console.WriteLine("  -X:MTA                 Run in multithreaded apartment");
            Console.WriteLine("  -X:NoOptimize          Disable optimized methods");
            Console.WriteLine("  -X:NoTraceback         Do not emit traceback code");
            Console.WriteLine("  -X:PassExceptions      Do not catch exceptions that are unhandled by Python code");
            Console.WriteLine("  -X:PrivateBinding      Enable binding to private members");
            Console.WriteLine("  -X:SaveAssemblies      Save generated assemblies");
            Console.WriteLine("  -X:ShowCLSExceptions   Display CLS Exception information");
            Console.WriteLine("  -X:StaticMethods       Generate static methods only");
#if !IRONPYTHON_WINDOW
            Console.WriteLine("  -X:TabCompletion       Enable TabCompletion mode");
#endif
#if DEBUG
            Console.WriteLine("  -X:TrackPerformance    Track performance sensitive areas");
#endif
#if !IRONPYTHON_WINDOW
            Console.WriteLine("  -i                     Inspect interactively after running script");
#endif
            Console.WriteLine("  -x                     Skip first line of the source");
#if !IRONPYTHON_WINDOW
            Console.WriteLine("  -v                     Verbose (trace import statements) (also PYTHONVERBOSE=x)");
#endif
            Console.WriteLine("  -h                     Display usage");
            Console.WriteLine("  -u                     Unbuffered stdout & stderr");
            Console.WriteLine("  -c cmd                 Program passed in as string (terminates option list)");
            Console.WriteLine("  -E                     Ignore environment variables");
            Console.WriteLine("  -OO                    Remove doc-strings in addition to the -O optimizations");
            Console.WriteLine("  -Q arg                 Division options: -Qold (default), -Qwarn, -Qwarnall, -Qnew");
            Console.WriteLine("  -S                     Don't imply 'import site' on initialization");
            Console.WriteLine("  -t                     Issue warnings about inconsistent tab usage");
            Console.WriteLine("  -tt                    Issue errors for inconsistent tab usage");
            Console.WriteLine("  -V                     Print the Python version number and exit");
            Console.WriteLine("  -W arg                 Warning control (arg is action:message:category:module:lineno)");

            //Console.WriteLine("  -d                     Debug output from parser");
            //Console.WriteLine("  -m mod                 Run library module as a script (terminates option list)");

            Console.WriteLine();

            Console.WriteLine("Environment variables:");
            Console.WriteLine("  IRONPYTHONPATH:        Path to search for module");
            Console.WriteLine("  IRONPYTHONSTARTUP:     Startup module");
        }

        private static void InitializePath(PythonEngine engine) {
            if (Options.IgnoreEnvironmentVariables)
                return;
            string path = Environment.GetEnvironmentVariable("IRONPYTHONPATH");
            if (path != null && path.Length > 0) {
                string[] paths = path.Split(';');
                foreach (string p in paths) {
                    engine.AddToPath(p);
                }
            }
        }

        private static string InitializeModules(PythonEngine engine) {
            string version = PythonEngine.VersionString;
            string executable = typeof(PythonCommandLine).Assembly.Location;
            string prefix = Path.GetDirectoryName(executable);
            engine.InitializeModules(prefix, executable, version);
            return version;
        }

        private static void ImportSite(PythonEngine engine) {
            if (!Options.ImportSite)
                return;
            string site = System.Reflection.Assembly.GetExecutingAssembly().Location;
            site = Path.Combine(Path.GetDirectoryName(site), "Lib");
            engine.AddToPath(site);
            try {
                engine.Import("site");
            } catch (Exception e) {
                MyConsole.Write(engine.FormatException(e), Style.Error);
            }
        }

        private static void RunStartup(PythonEngine engine) {
            if (Options.IgnoreEnvironmentVariables)
                return;

            string startup = Environment.GetEnvironmentVariable("IRONPYTHONSTARTUP");
            if (startup != null && startup.Length > 0) {
                if (handleExceptions) {
                    try {
                        engine.ExecuteFile(startup);
                    } catch (Exception e) {
                        if (e is PythonSystemExitException) throw;
                        MyConsole.Write(engine.FormatException(e), Style.Error);
                    } finally {
                        PythonEngine.DumpDebugInfo();
                    }
                } else {
                    try {
                        engine.ExecuteFile(startup);
                    } finally {
                        PythonEngine.DumpDebugInfo();
                    }
                }
            }
        }

        private static int RunFile(PythonEngine engine, string fileName) {
            if (fileName == "-") {
                fileName = "<stdin>";
            } else {
                if (!File.Exists(fileName)) {
                    Console.WriteLine("File {0} does not exist", fileName);
                    System.Environment.Exit(1);
                }
                engine.AddToPath(Path.GetDirectoryName(Path.GetFullPath(fileName)));
            }
            engine.AddToPath(Environment.CurrentDirectory);
            InitializePath(engine);
            InitializeModules(engine);
            ImportSite(engine);
            int result = 1;

            ExecutionOptions executionOptions = ExecutionOptions.None;
            if (Options.SkipFirstLine) executionOptions |= ExecutionOptions.SkipFirstLine;

            if (handleExceptions) {
                try {
                    ModuleScope scope;
#if !IRONPYTHON_WINDOW
                    if (Options.Introspection) {
                        scope = ExecuteFileConsole(fileName, executionOptions);
                    } else {
                        engine.ExecuteFileOptimized(fileName, "__main__", executionOptions, out scope);
                    }
#else
                engine.ExecuteFileOptimized(fileName, "__main__", executionOptions, out scope);
#endif
                    result = 0;
                } catch (PythonSystemExitException pythonSystemExit) {
                    result = pythonSystemExit.GetExitCode(engine.DefaultModuleScope);
                } catch (Exception e) {
                    MyConsole.Write(engine.FormatException(e), Style.Error);
                } finally {
                    PythonEngine.DumpDebugInfo();
                }
            } else {
                try {
                    ModuleScope scope;
#if !IRONPYTHON_WINDOW
                    if (Options.Introspection) {
                        scope = ExecuteFileConsole(fileName, executionOptions);
                    } else {
                        engine.ExecuteFileOptimized(fileName, "__main__", executionOptions, out scope);
                    }
#else
                    engine.ExecuteFileOptimized(fileName, "__main__", executionOptions, out scope);
#endif
                    result = 0;
                } catch (PythonSystemExitException pythonSystemExit) {
                    result = pythonSystemExit.GetExitCode(engine.DefaultModuleScope);
                } finally {
                    PythonEngine.DumpDebugInfo();
                }
            }


            return result;
        }

        private static int RunString(PythonEngine engine, string command) {
            engine.AddToPath(Environment.CurrentDirectory);
            InitializePath(engine);
            InitializeModules(engine);
            ImportSite(engine);
            int result = 1;

            ((List)engine.Sys.argv)[0] = "-c";

            if (handleExceptions) {
                try {
                    //engine.Execute(command, engine.DefaultModuleScope, ExecutionOptions.PrintExpressions);
                    engine.ExecuteToConsole(command);
                    result = 0;
                } catch (PythonSystemExitException pythonSystemExit) {
                    result = pythonSystemExit.GetExitCode(engine.DefaultModuleScope);
                } catch (Exception e) {
                    MyConsole.Write(engine.FormatException(e), Style.Error);
                } finally {
                    PythonEngine.DumpDebugInfo();
                }
            } else {
                try {
                    engine.ExecuteToConsole(command);
                    result = 0;
                } catch (PythonSystemExitException pythonSystemExit) {
                    result = pythonSystemExit.GetExitCode(engine.DefaultModuleScope);
                } finally {
                    PythonEngine.DumpDebugInfo();
                }
            }

            return result;
        }

#if !IRONPYTHON_WINDOW
        // The advanced console functions are in a special non-inlined function so that 
        // dependencies are pulled in only if necessary.
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static void UseSuperConsole(PythonEngine engine) {
            MyConsole = new SuperConsole(engine, ColorfulConsole);
        }
#endif
        public static bool DoOneInteractive(ModuleScope topFrame) {
            bool continueInteraction;
            string s = ReadStatement(out continueInteraction);

            if (continueInteraction == false)
                return false;

            if (s == null) {
                // Is it an empty line?
                MyConsole.Write(String.Empty, Style.Out);
                return true;
            }

            engine.ExecuteToConsole(s, topFrame);

            return true;
        }

#if !IRONPYTHON_WINDOW
        public static ModuleScope ExecuteFileConsole(string fileName, ExecutionOptions executionOptions) {
            ModuleScope moduleScope = null;

            ModuleScope scopeResult = null;
            bool continueInteraction;
            TryInteractiveAction(
                delegate(out bool continueInteractionArgument) {
                    engine.ExecuteFileOptimized(fileName, "__main__", executionOptions, out scopeResult);
                    continueInteractionArgument = true;
                    return 0;
                },
                out continueInteraction);

            if (continueInteraction) {
                if (scopeResult == null) {
                    // If there was an error generating a new module (for eg. because of a syntax error),
                    // we will just use the existing "topFrame"
                } else {
                    engine.DefaultModuleScope = moduleScope = scopeResult;
                }
                RunInteractiveLoop();
            }
            return moduleScope;
        }
#endif
        static IConsole _console;
        public static IConsole MyConsole {
            get {
                if (_console == null) {
                    _console = new BasicConsole(engine.Sys, ColorfulConsole);
                }
                return _console;
            }
            set {
                _console = value;
            }
        }

        public delegate int InteractiveAction(out bool continueInteraction);

        public static int TryInteractiveAction(InteractiveAction interactiveAction, out bool continueInteraction) {
            int result = 1; // assume failure
            continueInteraction = false;

            try {
                result = interactiveAction(out continueInteraction);
            } catch (PythonSystemExitException se) {
                return se.GetExitCode(engine.DefaultModuleScope);
            } catch (ThreadAbortException tae) {
                PythonKeyboardInterruptException pki = tae.ExceptionState as PythonKeyboardInterruptException;
                if (pki != null) {
                    Thread.ResetAbort();
                    bool endOfMscorlib = false;
                    string ex = engine.FormatException(tae, ExceptionConverter.ToPython(pki), delegate(StackFrame sf) {
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
                MyConsole.Write(engine.FormatException(e), Style.Error);
                continueInteraction = true;
            }

            return result;
        }

        private static bool TreatAsBlankLine(string line, int autoIndentSize) {
            if (line.Length == 0) return true;
            if (autoIndentSize != 0 && line.Trim().Length == 0 && line.Length == autoIndentSize) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Read a statement, which can potentially be a multiple-line statement suite (like a class declaration).
        /// </summary>
        /// <param name="continueInteraction">Should the console session continue, or did the user indicate 
        /// that it should be terminated?</param>
        /// <returns>Expression to evaluate. null for empty input</returns>
        public static string ReadStatement(out bool continueInteraction) {
            StringBuilder b = new StringBuilder();
            int autoIndentSize = 0;

            MyConsole.Write(engine.Sys.ps1.ToString(), Style.Prompt);

            while (true) {
                string line = MyConsole.ReadLine(autoIndentSize);
                continueInteraction = true;

                if (line == null) {
                    if (PythonEngine.ConsoleCommandDispatcher != null) {
                        PythonEngine.ConsoleCommandDispatcher(null);
                    }
                    continueInteraction = false;
                    return null;
                }

                bool allowIncompleteStatement = !TreatAsBlankLine(line, autoIndentSize);
                b.Append(line);
                b.Append("\n");

                if (b.ToString() == "\n" || (!allowIncompleteStatement && b.ToString().Trim().Length == 0)) return null;

                bool s = engine.ParseInteractiveInput(b.ToString(), allowIncompleteStatement);
                if (s) return b.ToString();

                if (AutoIndent && Options.AutoIndentSize != 0) {
                    autoIndentSize = Parser.GetNextAutoIndentSize(b.ToString(), Options.AutoIndentSize);
                }

                // Keep on reading input
                MyConsole.Write(engine.Sys.ps2.ToString(), Style.Prompt);
            }
        }

        private static int RunInteractiveLoop() {
            bool continueInteraction = true;
            int result = 0;
            while (continueInteraction) {
                result = TryInteractiveAction(
                    delegate(out bool continueInteractionArgument) {
                        continueInteractionArgument = DoOneInteractive(engine.DefaultModuleScope);
                        return 0;
                    },
                    out continueInteraction);
            }

            return result;
        }

        private static int RunInteractive() {
            engine.DefaultModuleScope.SetGlobal(SymbolTable.Doc, null);
            engine.Sys.modules[engine.DefaultModuleScope.Module.ModuleName] = engine.DefaultModuleScope.Module;
            return RunInteractiveLoop();
        }

#if !IRONPYTHON_WINDOW
        private static int RunInteractive(PythonEngine engine) {
            string version = InitializeModules(engine);
            engine.AddToPath(Environment.CurrentDirectory);
            InitializePath(engine);
            ImportSite(engine);

            AppDomain.CurrentDomain.UnhandledException += DefaultExceptionHandler;

            MyConsole.WriteLine(version, Style.Out);
            MyConsole.WriteLine(PythonEngine.Copyright, Style.Out);

            bool continueInteraction = true;
            int result = 1;
            try {
                RunStartup(engine);
                result = 0;
            } catch (PythonSystemExitException pythonSystemExit) {
                return pythonSystemExit.GetExitCode(engine.DefaultModuleScope);
            } catch (Exception) {
            }

            if (continueInteraction) {
                result = RunInteractive();
            }

            PythonEngine.DumpDebugInfo();
            return (int)result;
        }
#endif
        private static void DefaultExceptionHandler(object sender, UnhandledExceptionEventArgs args) {
            MyConsole.WriteLine("Unhandled exception: ", Style.Error);
            MyConsole.Write(engine.FormatException((Exception)args.ExceptionObject), Style.Error);
        }
    }
}

namespace IronPythonConsole {
    public interface IConsole {
        // Read a single line of interactive input
        // autoIndentSize is the indentation level to be used for the current suite of a compound statement.
        // The console can ignore this argument if it does not want to support auto-indentation
        string ReadLine(int autoIndentSize);

        void Write(string text, Style style);
        void WriteLine(string text, Style style);
    }

    public enum Style {
        Prompt, Out, Error
    }

    public class BasicConsole : IConsole {

        public ConsoleColor PromptColor = Console.ForegroundColor;
        public ConsoleColor OutColor = Console.ForegroundColor;
        public ConsoleColor ErrorColor = Console.ForegroundColor;

        public void SetupColors() {
            PromptColor = ConsoleColor.DarkGray;
            OutColor = ConsoleColor.DarkBlue;
            ErrorColor = ConsoleColor.DarkRed;
        }

        private SystemState sys;
        private AutoResetEvent ctrlCEvent;
        private Thread MainEngineThread = Thread.CurrentThread;

        public BasicConsole(SystemState systemState, bool colorful) {
            if (colorful)
                SetupColors();
            sys = systemState;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            ctrlCEvent = new AutoResetEvent(false);
        }

        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC) {
                e.Cancel = true;
                ctrlCEvent.Set();
                MainEngineThread.Abort(new PythonKeyboardInterruptException(""));
            }
        }

        #region IConsole Members

        public string ReadLine(int autoIndentSize) {
            Write("".PadLeft(autoIndentSize), Style.Prompt);

            string res = Console.In.ReadLine();
            if (res == null) {
                // we have a race - the Ctrl-C event is delivered
                // after ReadLine returns.  We need to wait for a little
                // bit to see which one we got.  This will cause a slight
                // delay when shutting down the process via ctrl-z, but it's
                // not really perceptible.  In the ctrl-C case we will return
                // as soon as the event is signaled.
                if (ctrlCEvent != null && ctrlCEvent.WaitOne(100, false)) {
                    // received ctrl-C
                    return "";
                } else {
                    // received ctrl-Z
                    return null;
                }
            }
            return "".PadLeft(autoIndentSize) + res;
        }

        public void Write(string text, Style style) {
            switch (style) {
                case Style.Prompt: WriteColor(text, style, PromptColor); break;
                case Style.Out: WriteColor(text, style, OutColor); break;
                case Style.Error: WriteColor(text, style, ErrorColor); break;
            }
        }

        private void WriteColor(string s, Style style, ConsoleColor c) {
            ConsoleColor origColor = Console.ForegroundColor;
            Console.ForegroundColor = c;

            Ops.PrintWithDestNoNewline(sys, sys.stdout, s);
            Ops.Invoke(sys.stdout, SymbolTable.StringToId("flush"));

            Console.ForegroundColor = origColor;
        }

        public void WriteLine(string text, Style style) {
            Write(text + Environment.NewLine, style);
        }

        #endregion
    }
}