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

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System.Threading;
using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Hosting.Shell;
using System.Collections.Generic;

namespace IronPython.Hosting {
#if !SILVERLIGHT
    /// <summary>
    /// A simple Python command-line should mimic the standard python.exe
    /// </summary>
    public class PythonCommandLine : CommandLine {
        private PythonContext PythonContext {
            get { return (PythonContext)Language; }
        }
        
        private new PythonConsoleOptions Options { get { return (PythonConsoleOptions)base.Options; } }
        
        public PythonCommandLine() {
        }

        protected override string/*!*/ Logo {
            get {
                return GetLogoDisplay().Replace("\n", Environment.NewLine);
            }
        }

        /// <summary>
        /// Returns the display look for IronPython.  
        /// 
        /// The returned string uses This \n instead of Environment.NewLine for it's line seperator 
        /// because it is intended to be outputted through the Python I/O system.
        /// </summary>
        public static string GetLogoDisplay() {
            return GetVersionString() +
                   "\nType \"help\", \"copyright\", \"credits\" or \"license\" for more information.\n";
        }

        private string/*!*/ VersionString {
            get {
                return GetVersionString();                    
            }
        }

        private static string GetVersionString() {
            return String.Format("{0} ({1}) on .NET {2}",
                                PythonContext.GetPythonDisplayName(),
                                PythonContext.GetPythonVersion().ToString(),
                                Environment.Version);
        }

        private int GetEffectiveExitCode(SystemExitException/*!*/ e) {
            object nonIntegerCode;
            int exitCode = e.GetExitCode(out nonIntegerCode);
            if (nonIntegerCode != null) {
                Console.WriteLine(nonIntegerCode.ToString(), Style.Error);
            }
            return exitCode;
        }

        protected override void Shutdown() {
            try {
                Language.Shutdown();
            } catch (Exception e) {
                Console.WriteLine("", Style.Error);
                Console.WriteLine("Error in sys.exitfunc:", Style.Error);
                Console.Write(Language.FormatException(e), Style.Error);
            }
        }

        protected override int Run() {
            if (Options.ModuleToRun != null) {
                CodeContext ctx = new CodeContext(new Scope(), Language);
                object ret = Importer.ImportModule(ctx, null, Options.ModuleToRun, false, -1);
                if (ret == null) {
                    Console.WriteLine(String.Format("ImportError: No module named {0}", Options.ModuleToRun), Style.Error);
                    return 1;
                } else {
                    return 0;
                }
            }

            return base.Run();
        }

        #region Initialization

        protected override void Initialize() {
            Debug.Assert(Language != null);

            Console.Output = new OutputWriter(PythonContext, false);
            Console.ErrorOutput = new OutputWriter(PythonContext, true);
            
            // TODO: must precede path initialization! (??? - test test_importpkg.py)
            
            if (Options.Command == null && Options.FileName != null) {
                if (Options.FileName == "-") {
                    Options.FileName = "<stdin>";
                } else {
#if !SILVERLIGHT
                    if (!File.Exists(Options.FileName)) {
                        Console.WriteLine(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "File {0} does not exist.",
                                Options.FileName),
                            Style.Error);
                        Environment.Exit(1);
                    }
#endif
                    string fullPath = Language.DomainManager.Platform.GetFullPath(Options.FileName);
                    PythonContext.AddToPath(Path.GetDirectoryName(fullPath));
                }
            }

            Language.DomainManager.LoadAssembly(typeof(string).Assembly);
            Language.DomainManager.LoadAssembly(typeof(System.Diagnostics.Debug).Assembly);

            InitializePath();
            InitializeArguments();
            InitializeModules();
            InitializeExtensionDLLs();
            ImportSite();
        }

        private void InitializeArguments() {
            if (Options.ModuleToRun != null) {
                // if the user used the -m option we need to update sys.argv to arv[0] is the full path
                // to the module we'll run.  If we don't find the module we'll have an import error
                // and this doesn't matter.
                List path;
                if (PythonContext.TryGetSystemPath(out path)) {
                    foreach (object o in path) {
                        string str = o as string;
                        if (str == null) continue;

                        string libpath = Path.Combine(str, Options.ModuleToRun + ".py");
                        if (File.Exists(libpath)) {
                            // cast to List is a little scary but safe during startup
                            ((List)PythonContext.SystemState.Dict[SymbolTable.StringToId("argv")])[0] = libpath;
                            break;
                        }
                    }
                }
            }
        }

        protected override Scope/*!*/ CreateScope() {
            ModuleOptions trueDiv = (PythonContext.PythonOptions.DivisionOptions == PythonDivisionOptions.New) ? ModuleOptions.TrueDivision : ModuleOptions.None;
            PythonModule module = PythonContext.CreateModule(trueDiv | ModuleOptions.ModuleBuiltins);
            PythonContext.PublishModule("__main__", module);
            module.Scope.SetName(Symbols.Doc, null);
            module.Scope.SetName(Symbols.Name, "__main__");
            return module.Scope;
        }
        
        private void InitializePath() {
            PythonContext.AddToPath(PythonContext.DomainManager.Platform.CurrentDirectory);

#if !SILVERLIGHT // paths, environment vars
            if (!Options.IgnoreEnvironmentVariables) {
                string path = Environment.GetEnvironmentVariable("IRONPYTHONPATH");
                if (path != null && path.Length > 0) {
                    string[] paths = path.Split(Path.PathSeparator);
                    foreach (string p in paths) {
                        PythonContext.AddToPath(p);
                    }
                }
            }

            string entry = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string site = Path.Combine(entry, "Lib");
            PythonContext.AddToPath(site);

            // add DLLs directory if it exists            
            string dlls = Path.Combine(entry, "DLLs");
            if (Directory.Exists(dlls)) {
                PythonContext.AddToPath(dlls);
            }
#endif
        }

        private string InitializeModules() {
            string version = VersionString;
            
#if SILVERLIGHT // paths
            string executable = "";
            string prefix = "";
#else
            string executable = Assembly.GetEntryAssembly().Location;
            string prefix = Path.GetDirectoryName(executable);
#endif
            PythonContext.SetHostVariables(prefix, executable, version);
            return version;
        }

        /// <summary>
        /// Loads any extension DLLs present in sys.prefix\DLLs directory and adds references to them.
        /// 
        /// This provides an easy drop-in location for .NET assemblies which should be automatically referenced
        /// (exposed via import), COM libraries, and pre-compiled Python code.
        /// </summary>
        private void InitializeExtensionDLLs() {
            string dir = Path.Combine(PythonContext.InitialPrefix, "DLLs");
            if (Directory.Exists(dir)) {
                CodeContext ctx = new CodeContext(Scope, PythonContext);

                foreach (string file in Directory.GetFiles(dir)) {
                    if (file.ToLower().EndsWith(".dll")) {
                        try {
                            ClrModule.AddReference(ctx, new FileInfo(file).Name);
                        } catch {
                        }
                    }
                }
            }
        }

        private void ImportSite() {
            if (Options.SkipImportSite)
                return;

            try {
                CodeContext ctx = new CodeContext(new Scope(), Language);
                Importer.ImportModule(ctx, null, "site", false, -1);
            } catch (Exception e) {
                Console.Write(Language.FormatException(e), Style.Error);
            }
        }

        #endregion

        #region Interactive

        protected override int RunInteractive() {
            PrintLogo();

            if (Scope == null) {
                Scope = CreateScope();
            }

            int result = 1;
            try {
                RunStartup();
                result = 0;
            } catch (SystemExitException pythonSystemExit) {
                return GetEffectiveExitCode(pythonSystemExit);
            } catch (Exception) {
            }

            result = RunInteractiveLoop();

            return (int)result;
        }
        
        private void RunStartup() {
            if (Options.IgnoreEnvironmentVariables)
                return;

#if !SILVERLIGHT // Environment.GetEnvironmentVariable
            string startup = Environment.GetEnvironmentVariable("IRONPYTHONSTARTUP");
            if (startup != null && startup.Length > 0) {
                if (Options.HandleExceptions) {
                    try {
                        ExecuteCommand(Language.CreateFileUnit(startup));
                    } catch (Exception e) {
                        if (e is SystemExitException) throw;
                        Console.Write(Language.FormatException(e), Style.Error);
                    }
                } else {
                    ExecuteCommand(Language.CreateFileUnit(startup));
                }
            }
#endif
        }



        protected override int? TryInteractiveAction() {
            try {
                try {
                    return TryInteractiveActionWorker();
                } finally {
                    // sys.exc_info() is normally cleared after functions exit. But interactive console enters statements
                    // directly instead of using functions. So clear explicitly.
                    PythonOps.ClearCurrentException();
                }
            } catch (SystemExitException se) {
                return GetEffectiveExitCode(se);
            }
        }

        /// <summary>
        /// Attempts to run a single interaction and handle any language-specific
        /// exceptions.  Base classes can override this and call the base implementation
        /// surrounded with their own exception handling.
        /// 
        /// Returns null if successful and execution should continue, or an exit code.
        /// </summary>
        private int? TryInteractiveActionWorker() {
            int? result = null;

            try {
                result = RunOneInteraction();
#if SILVERLIGHT // ThreadAbortException.ExceptionState
            } catch (ThreadAbortException) {
#else
            } catch (ThreadAbortException tae) {
                KeyboardInterruptException pki = tae.ExceptionState as KeyboardInterruptException;
                if (pki != null) {
                    Console.WriteLine(Language.FormatException(tae), Style.Error);
                    Thread.ResetAbort();
                }
#endif
            }

            return result;
        }

        /// <summary>
        /// Parses a single interactive command and executes it.  
        /// 
        /// Returns null if successful and execution should continue, or the appropiate exit code.
        /// </summary>
        private int? RunOneInteraction() {
            bool continueInteraction;
            string s = ReadStatement(out continueInteraction);

            if (continueInteraction == false) {
                PythonContext.DispatchCommand(null); // Notify dispatcher that we're done
                return 0;
            }

            if (String.IsNullOrEmpty(s)) {
                // Is it an empty line?
                Console.Write(String.Empty, Style.Out);
                return null;
            }


            SourceUnit su = Language.CreateSnippet(s, "<stdin>", SourceCodeKind.InteractiveCode);
            PythonCompilerOptions pco = (PythonCompilerOptions)Language.GetCompilerOptions(Scope);
            pco.Module |= ModuleOptions.ExecOrEvalCode;

            Action action = delegate() {
                try {
                    su.Compile(pco, ErrorSink).Run(Scope);
                } catch (Exception e) {
                    if (e is SystemExitException) {
                        throw;
                    }
                    // Need to handle exceptions in the delegate so that they're not wrapped
                    // in a TargetInvocationException
                    UnhandledException(e);
                }
            };

            try {
                PythonContext.DispatchCommand(action);
            } catch (SystemExitException sx) {
                object dummy;
                return sx.GetExitCode(out dummy);
            }

            return null;
        }

        protected override ErrorSink/*!*/ ErrorSink {
            get { return ThrowingErrorSink.Default; }
        }

        protected override int GetNextAutoIndentSize(string text) {
            return Parser.GetNextAutoIndentSize(text, Options.AutoIndentSize);
        }

        #endregion

        #region Command

        protected override int RunCommand(string command) {
            if (Options.HandleExceptions) {
                try {
                    return RunCommandWorker(command);
                } catch (Exception e) {
                    Console.Write(Language.FormatException(e), Style.Error);
                    return 1;
                }
            } 

            return RunCommandWorker(command);            
        }

        private int RunCommandWorker(string command) {
            int result = 1;
            try {
                Scope = CreateScope();
                ExecuteCommand(Language.CreateSnippet(command, SourceCodeKind.File));
                result = 0;
            } catch (SystemExitException pythonSystemExit) {
                result = GetEffectiveExitCode(pythonSystemExit);
            }
            return result;
        }

        #endregion

        #region File

        protected override int RunFile(string/*!*/ fileName) {
            int result = 1;
            if (Options.HandleExceptions) {
                try {
                    result = RunFileWorker(fileName);
                } catch (Exception e) {
                    Console.Write(Language.FormatException(e), Style.Error);
                }
            } else {
                result = RunFileWorker(fileName);
            }

            return result;
        }        
        
        private int RunFileWorker(string/*!*/ fileName) {
            ScriptCode compiledCode;
            ModuleOptions modOpt = ModuleOptions.Optimized | ModuleOptions.ModuleBuiltins;
            if(Options.SkipFirstSourceLine) {
                modOpt |= ModuleOptions.SkipFirstLine;

            }
            PythonModule module = PythonContext.CompileModule(fileName, "__main__", modOpt, out compiledCode);
            PythonContext.PublishModule("__main__", module);
            Scope = module.Scope;

            try {
                compiledCode.Run(Scope);
            } catch (SystemExitException pythonSystemExit) {
                
                // disable introspection when exited:
                Options.Introspection = false;

                return GetEffectiveExitCode(pythonSystemExit);
            }

            return 0;
        }

        #endregion

        public override IList<string> GetGlobals(string name) {
            IList<string> res = base.GetGlobals(name);
            foreach (SymbolId builtinName in PythonContext.BuiltinModuleInstance.Keys) {
                string strName = SymbolTable.IdToString(builtinName);
                if (strName.StartsWith(name)) {
                    res.Add(strName);
                }
            }

            return res;
        }

        protected override void UnhandledException(Exception e) {
            PythonOps.PrintException(new CodeContext(Scope, Language), e, Console);
        }
    }
#endif
}
