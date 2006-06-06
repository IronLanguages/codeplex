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
using System.Threading;
using SystemThread = System.Threading.Thread;

using IronPython.Hosting;
using IronPython.Compiler;
using IronPython.Runtime;

namespace IronPythonConsole {
    /// <summary>
    /// A simple Python command-line should mimic the standard python.exe
    /// </summary>
    class PythonCommandLine {
        private static bool HandleExceptions = true;
        private static bool PressKeyToContinue = false;
        private static bool tabCompletion = false;
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
                p.result = Run(p.args, p.engine);
            }
        }

        [STAThread]
        static int Main(string [] rawArgs) {
            List<string> args = new List<string>(rawArgs);
            Options options = ParseOptions(args);

            if (Options.PrintVersionAndExit) {
                Console.WriteLine(PythonEngine.VersionString);
                return 0;
            }

            engine = new PythonEngine(options);

            try {
                if (TabCompletion) {
                    UseSuperConsole(engine);
                } else {
                    engine.MyConsole = new BasicConsole();
                }

                if (Options.WarningFilters != null)
                    engine.Sys.warnoptions = IronPython.Runtime.List.Make(Options.WarningFilters);

                engine.Sys.SetRecursionLimit(Options.MaximumRecursion);

                if (mta) {
                    MTAParameters p = new MTAParameters(engine, args);
                    SystemThread thread = new SystemThread(MTAThread);
                    thread.SetApartmentState(ApartmentState.MTA);
                    thread.Start(p);
                    thread.Join();
                    return p.result;
                } else {
                    return Run(args, engine);
                }
            } finally {
                engine.Shutdown();
            }
        }

        private static int Run(List<string> args, PythonEngine engine) {
            try {
                if (Options.Command != null) {
                    return RunString(engine, Options.Command, args);
                } else if (args.Count == 0) {
                    return RunInteractive(engine);
                } else {
                    return RunFile(engine, args);
                }
            } catch (System.Threading.ThreadAbortException tae) {
                if (tae.ExceptionState is IronPython.Runtime.PythonKeyboardInterrupt) {
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
                    case "-X:ColorfulConsole": ColorfulConsole = true; break;
                    case "-X:ExceptionDetail": options.ExceptionDetail = true; break;
                    case "-X:FastEval": Options.FastEval = true; break;
                    case "-X:Frames": Options.Frames = true; break;
                    case "-X:GenerateAsSnippets": Options.GenerateModulesAsSnippets = true; break;
                    case "-X:ILDebug": Options.ILDebug = true; break;
                    case "-X:MTA": mta = true; break;
                    case "-X:NoOptimize": Options.OptimizeReflectCalls = false; break;
                    case "-X:NoTraceback": Options.TracebackSupport = false; break;
                    case "-X:MaxRecursion":
                        args.RemoveAt(0);
                        int maxRecurVal = 0;
                        if (args.Count == 0 || !Int32.TryParse((string)args[0], out maxRecurVal)) {
                            PrintUsageAndExit();
                        }
                        Options.MaximumRecursion = maxRecurVal;
                        break;
                    case "-X:PrivateBinding": Options.PrivateBinding = true; break;
                    case "-X:SaveAssemblies": Options.SaveAndReloadBinaries = true; break;
                    case "-X:ShowCLSExceptions": options.ShowCLSExceptions = true; break;
                    case "-X:StaticMethods": Options.GenerateDynamicMethods = false; break;
                    case "-X:TabCompletion": TabCompletion = true; break;
                    case "-X:TrackPerformance": // accepted but ignored on retail builds
#if DEBUG
                        Options.TrackPerformance = true;
#endif
                        break;
                    case "-i": Options.Introspection = true; break;
                    case "-x": Options.SkipFirstLine = true; break;
                    case "-v": options.Verbose = true; break;
                    case "-u": Options.UnbufferedStdOutAndError = true; break;
                    case "-c":
                        args.RemoveAt(0);
                        if (args.Count == 0) {
                            PrintUsageAndExit();
                        }
                        Options.Command = (string)args[0];
                        return options;
                    case "-V":
                        Options.PrintVersionAndExit = true;
                        return options;
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
                            case "old": Options.Division = Options.DivisionOptions.Old; break;
                            case "new": Options.Division = Options.DivisionOptions.New; break;
                            case "warn": Options.Division = Options.DivisionOptions.Warn; break;
                            case "warnall": Options.Division = Options.DivisionOptions.WarnAll; break;
                            default: PrintUsageAndExit(); break;
                        }
                        break;
                    case "-Qold": Options.Division = Options.DivisionOptions.Old; break;
                    case "-Qnew": Options.Division = Options.DivisionOptions.New; break;
                    case "-Qwarn": Options.Division = Options.DivisionOptions.Warn; break;
                    case "-Qwarnall": Options.Division = Options.DivisionOptions.WarnAll; break;
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
            Console.WriteLine("Usage: IronPythonConsole [options] [file.py|- [arguments]]");
            Console.WriteLine("Options:");
            Console.WriteLine("  -O:                    Enable optimizations");
#if DEBUG
            Console.WriteLine("  -D                     EngineDebug mode");
#endif
            // the following extension switches should be printed in alphabetic order
            Console.WriteLine("  -X:AssembliesDir       Set the directory for saving generated assemblies");
            Console.WriteLine("  -X:ColorfulConsole     Enable ColorfulConsole");
            Console.WriteLine("  -X:ExceptionDetail     Enable ExceptionDetail mode");
            Console.WriteLine("  -X:FastEval            Enable fast eval");
            Console.WriteLine("  -X:Frames              Generate custom frames");
            Console.WriteLine("  -X:GenerateAsSnippets  Generate code to run in snippet mode");
            Console.WriteLine("  -X:ILDebug             Output generated IL code to a text file for debugging");
            Console.WriteLine("  -X:MaxRecursion        Set the maximum recursion level");
            Console.WriteLine("  -X:MTA                 Run in multithreaded apartment");
            Console.WriteLine("  -X:NoOptimize          Disable optimized methods");
            Console.WriteLine("  -X:NoTraceback         Do not emit traceback code");
            Console.WriteLine("  -X:PrivateBinding      Enable binding to private members");
            Console.WriteLine("  -X:SaveAssemblies      Save generated assemblies");
            Console.WriteLine("  -X:ShowCLSExceptions   Display CLS Exception information");
            Console.WriteLine("  -X:StaticMethods       Generate static methods only");
            Console.WriteLine("  -X:TabCompletion       Enable TabCompletion mode");
#if DEBUG
            Console.WriteLine("  -X:TrackPerformance    Track performance sensitive areas");
#endif
            Console.WriteLine("  -i                     Inspect interactively after running script");
            Console.WriteLine("  -x                     Skip first line of the source");
            Console.WriteLine("  -v                     Verbose (trace import statements) (also PYTHONVERBOSE=x)");
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
                engine.MyConsole.Write(engine.FormatException(e), Style.Error);
            }
        }

        private static void RunStartup(PythonEngine engine) {
            if (Options.IgnoreEnvironmentVariables)
                return;

            string startup = Environment.GetEnvironmentVariable("IRONPYTHONSTARTUP");
            if (startup != null && startup.Length > 0) {
                if (HandleExceptions) {
                    try {
                        engine.ExecuteFile(startup);
                    } catch (Exception e) {
                        if (e is PythonSystemExit) throw;
                        engine.MyConsole.Write(engine.FormatException(e), Style.Error);
                    } finally {
                        engine.DumpDebugInfo();
                    }
                } else {
                    try {
                        engine.ExecuteFile(startup);
                    } finally {
                        engine.DumpDebugInfo();
                    }
                }
            }
        }

        // The advanced console functions are in a special non-inlined function so that 
        // dependencies are pulled in only if necessary.
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static void WaitForAnyKey() {
            Console.WriteLine("type any key to continue");
            Console.ReadKey();
        }

        private static int RunFile(PythonEngine engine, List<string> args) {
            string fileName = args[0];
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

            ExecutionOptions executionOptions = ExecutionOptions.Default;
            if (Options.Introspection) executionOptions |= ExecutionOptions.Introspection;
            if (Options.SkipFirstLine) executionOptions |= ExecutionOptions.SkipFirstLine;

            if (HandleExceptions) {
                try {
                    Frame scope;
                    engine.ExecuteFileOptimized(fileName, args, executionOptions, out scope);
                    result = 0;
                } catch (PythonSystemExit pythonSystemExit) {
                    result = pythonSystemExit.GetExitCode(engine.DefaultModuleScope);
                }  catch (Exception e) {
                    engine.MyConsole.Write(engine.FormatException(e), Style.Error);
                } finally {
                    engine.DumpDebugInfo();
                }
            } else {
                try {
                    Frame scope;
                    engine.ExecuteFileOptimized(fileName, args, executionOptions, out scope);
                    result = 0;
                } catch (PythonSystemExit pythonSystemExit) {
                    result = pythonSystemExit.GetExitCode(engine.DefaultModuleScope);
                } finally {
                    engine.DumpDebugInfo();
                }
            }

            if (PressKeyToContinue) {
                WaitForAnyKey();
            }

            return result;
        }

        private static int RunString(PythonEngine engine, string command, List<string> args) {
            engine.AddToPath(Environment.CurrentDirectory);
            InitializePath(engine);
            InitializeModules(engine);
            ImportSite(engine);
            int result = 1;

            args[0] = "-c";
            engine.Sys.argv = IronPython.Runtime.List.Make(args);

            if (HandleExceptions) {
                try {
                    engine.Execute(command, engine.DefaultModuleScope, ExecutionOptions.PrintExpressions);
                    result = 0;
                } catch (PythonSystemExit pythonSystemExit) {
                    result = pythonSystemExit.GetExitCode(engine.DefaultModuleScope);
                } catch(Exception e) {
                    engine.MyConsole.Write(engine.FormatException(e), Style.Error);
                } finally {
                    engine.DumpDebugInfo();
                }
            } else {
                try {
                    engine.Execute(command);
                    result = 0;
                } catch (PythonSystemExit pythonSystemExit) {
                    result = pythonSystemExit.GetExitCode(engine.DefaultModuleScope);
                } finally {
                    engine.DumpDebugInfo();
                }
            }

            if (PressKeyToContinue) {
                WaitForAnyKey();
            }

            return result;
        }

        // The advanced console functions are in a special non-inlined function so that 
        // dependencies are pulled in only if necessary.
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static void UseSuperConsole(PythonEngine engine) {
            engine.MyConsole = new SuperConsole(engine, ColorfulConsole);
        }

        private static int RunInteractive(PythonEngine engine) {
            string version = InitializeModules(engine);
            engine.AddToPath(Environment.CurrentDirectory);
            InitializePath(engine);
            ImportSite(engine);

            AppDomain.CurrentDomain.UnhandledException += DefaultExceptionHandler;

            engine.MyConsole.WriteLine(version, Style.Out);
            engine.MyConsole.WriteLine(PythonEngine.Copyright, Style.Out);

            bool continueInteraction = true;
            int result = 1;
            try {
                RunStartup(engine);
                result = 0;
            } catch (PythonSystemExit pythonSystemExit) {
                return pythonSystemExit.GetExitCode(engine.DefaultModuleScope);                
            } catch (Exception e) {
            }

            if (continueInteraction) {
                result = engine.RunInteractive();
            }

            engine.DumpDebugInfo();
            return (int) result;
        }

        private static void DefaultExceptionHandler(object sender, UnhandledExceptionEventArgs args) {
            engine.MyConsole.WriteLine("Unhandled exception: ", Style.Error);
            engine.MyConsole.Write(engine.FormatException((Exception)args.ExceptionObject), Style.Error);
        }
    }
}
