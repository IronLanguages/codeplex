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
using System.IO;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Utils;

using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

namespace IronPython.Hosting {
   
    /// <summary>
    /// A simple Python command-line should mimic the standard python.exe
    /// </summary>
    public class PythonCommandLine : CommandLine {
        private PythonContext/*!*/ _context;
        
        private new PythonConsoleOptions Options { get { return (PythonConsoleOptions)base.Options; } }
        
        public PythonCommandLine(PythonContext/*!*/ context) {
            _context = context;
        }

        protected override string Logo {
            get {
                return VersionString + 
                    Environment.NewLine + 
                    "Copyright (c) Microsoft Corporation. All rights reserved." + 
                    Environment.NewLine;
            }
        }

        private string VersionString {
            get {
                return String.Format("{0} ({1}) on .NET {2}",
                    _context.DisplayName,
                    Engine.LanguageVersion.ToString(),
                    Environment.Version);
                    
            }
        }

        private int GetEffectiveExitCode(SystemExitException e) {
            object nonIntegerCode;
            int exitCode = e.GetExitCode(out nonIntegerCode);
            if (nonIntegerCode != null) {
                Console.WriteLine(nonIntegerCode.ToString(), Style.Error);
            }
            return exitCode;
        }

        protected override void Shutdown(ScriptEngine engine) {
            Contract.RequiresNotNull(engine, "engine");

            try {
                engine.Shutdown();
            } catch (Exception e) {
                Console.WriteLine("", Style.Error);
                Console.WriteLine("Error in sys.exitfunc:", Style.Error);
                Console.Write(engine.FormatException(e), Style.Error);
            }
        }

        protected override int Run() {
            if (Options.ModuleToRun != null) {
                CodeContext ctx = new CodeContext(new Scope(), _context);
                object ret = Importer.ImportModule(ctx, ctx.Scope.ModuleScope.Dict, Options.ModuleToRun, false, -1);
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
                    string fullPath = _context.DomainManager.Platform.GetFullPath(Options.FileName);
                    _context.AddToPath(Path.GetDirectoryName(fullPath));
                }
            }

            _context.DomainManager.LoadAssembly(typeof(string).Assembly);
            _context.DomainManager.LoadAssembly(typeof(System.Diagnostics.Debug).Assembly);

            InitializePath();
            InitializeArguments();
            InitializeModules();
            ImportSite();
        }

        private void InitializeArguments() {
            if (Options.ModuleToRun != null) {
                // if the user used the -m option we need to update sys.argv to arv[0] is the full path
                // to the module we'll run.  If we don't find the module we'll have an import error
                // and this doesn't matter.
                List path;
                if (_context.TryGetSystemPath(out path)) {
                    foreach (object o in path) {
                        string str = o as string;
                        if (str == null) continue;

                        string libpath = Path.Combine(str, Options.ModuleToRun + ".py");
                        if (File.Exists(libpath)) {
                            // cast to List is a little scary but safe during startup
                            ((List)_context.SystemState.Dict[SymbolTable.StringToId("argv")])[0] = libpath;
                            break;
                        }
                    }
                }
            }
        }

        private ScriptScope/*!*/ CreateMainModule() {
            ModuleOptions trueDiv = (_context.PythonOptions.DivisionOptions == PythonDivisionOptions.New) ? ModuleOptions.TrueDivision : ModuleOptions.None;
            PythonModule module = _context.CreateModule("__main__", trueDiv | ModuleOptions.PublishModule | ModuleOptions.ModuleBuiltins);
            module.Scope.SetName(Symbols.Doc, null);
            return Engine.CreateScope(module.Scope.Dict);
        }

        
        private void InitializePath() {
            _context.AddToPath(_context.DomainManager.Platform.CurrentDirectory);

#if !SILVERLIGHT // paths, environment vars
            if (!Options.IgnoreEnvironmentVariables) {
                string path = Environment.GetEnvironmentVariable("IRONPYTHONPATH");
                if (path != null && path.Length > 0) {
                    string[] paths = path.Split(Path.PathSeparator);
                    foreach (string p in paths) {
                        _context.AddToPath(p);
                    }
                }
            }

            string entry = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string site = Path.Combine(entry, "Lib");
            _context.AddToPath(site);

            // add DLLs directory if it exists            
            string dlls = Path.Combine(entry, "DLLs");
            if (Directory.Exists(dlls)) {
                _context.AddToPath(dlls);
            }
#endif
        }

        private string InitializeModules() {
            string version = VersionString;
            
            this.Module = CreateMainModule();
            
#if SILVERLIGHT // paths
            string executable = "";
            string prefix = "";
#else
            string executable = Assembly.GetEntryAssembly().Location;
            string prefix = Path.GetDirectoryName(executable);
#endif
            _context.SetHostVariables(prefix, executable, version);
            return version;
        }

        private void ImportSite() {
            if (Options.SkipImportSite)
                return;

            try {
                CodeContext ctx = new CodeContext(new Scope(), _context);
                Importer.ImportModule(ctx, ctx.Scope.ModuleScope.Dict, "site", false, -1);
            } catch (Exception e) {
                Console.Write(Engine.FormatException(e), Style.Error);
            }
        }

        #endregion

        #region Interactive

        protected override int RunInteractive() {
            PrintLogo();

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
                        ExecuteCommand(Module, _context.CreateFileUnit(startup));
                    } catch (Exception e) {
                        if (e is SystemExitException) throw;
                        Console.Write(Engine.FormatException(e), Style.Error);
                    }
                } else {
                    ExecuteCommand(Module, _context.CreateFileUnit(startup));
                }
            }
#endif
        }

        public override int? TryInteractiveAction() {
            try {
                try {
                    return base.TryInteractiveAction();
                } finally {
                    // sys.exc_info() is normally cleared after functions exit. But interactive console enters statements
                    // directly instead of using functions. So clear explicitly.
                    PythonOps.ClearCurrentException();
                }
            } catch (SystemExitException se) {
                return GetEffectiveExitCode(se);
            }
        }

         // TODO: this is very hacky; the problem is that command line is currently a hybrid between a host and language API
        protected override void ExecuteCommand(string/*!*/ command) {
            ExecuteCommand(Module, _context.CreateSnippet(command, SourceCodeKind.InteractiveCode));
        }

        private void ExecuteCommand(ScriptScope/*!*/ scope, SourceUnit/*!*/ commandUnit) {
            Engine.CreateScriptSourceFromString(commandUnit.GetCode(), commandUnit.Kind).Compile(
                Engine.GetCompilerOptions(scope), new Listener(commandUnit)).Execute(scope);
        }

        private class Listener : ErrorListener {
            private SourceUnit _commandUnit;

            public Listener(SourceUnit/*!*/ commandUnit) {
                _commandUnit = commandUnit;
            }

            public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity) {
                ThrowingErrorSink.Default.Add(_commandUnit, message, span, errorCode, severity);
            }
        }
        
        protected override string ReadLine(int autoIndentSize) {
            string res = base.ReadLine(autoIndentSize);

            _context.DomainManager.DispatchCommand(null);

            return res;
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
                    Console.Write(Engine.FormatException(e), Style.Error);
                    return 1;
                }
            } 

            return RunCommandWorker(command);            
        }

        private int RunCommandWorker(string command) {
            int result = 1;
            try {
                ScriptScope module = CreateMainModule();
                if (Options.Introspection) {
                    Module = module;
                }

                ExecuteCommand(module, _context.CreateSnippet(command, SourceCodeKind.File));
                result = 0;
            } catch (SystemExitException pythonSystemExit) {
                result = GetEffectiveExitCode(pythonSystemExit);
            }
            return result;
        }

        #endregion

        #region File

        protected override int RunFile(string filename) {

            // TODO: must precede path initialization! (??? - test test_importpkg.py)
            //if (filename == "-") {
            //    filename = "<stdin>";
            //} else {
            //    if (!File.Exists(filename)) {
            //        Console.WriteLine(String.Format("File {0} does not exist.", filename), Style.Error);
            //        return 1;
            //    }
            //    Engine.AddToPath(Path.GetDirectoryName(Path.GetFullPath(filename)));
            //}

            int result = 1;
            if (Options.HandleExceptions) {
                try {
                    result = RunFileWorker(filename);
                } catch (Exception e) {
                    Console.Write(Engine.FormatException(e), Style.Error);
                }
            } else {
                result = RunFileWorker(filename);
            }

            return result;
        }        
        
        private int RunFileWorker(string fileName) {
            try {
                ScriptCode compiledCode;
                PythonModule module = _context.CompileModule(fileName, "__main__", ModuleOptions.PublishModule | ModuleOptions.Optimized | ModuleOptions.ModuleBuiltins, Options.SkipFirstSourceLine, out compiledCode);

                if (Options.Introspection) {
                    Module = Engine.CreateScope(module.Scope.Dict);
                }

                compiledCode.Run(module.Scope, module);
                return 0;
            } catch (SystemExitException pythonSystemExit) {
                
                // disable introspection when exited:
                Options.Introspection = false;

                return GetEffectiveExitCode(pythonSystemExit);
            }
        }

        #endregion
    }
}
