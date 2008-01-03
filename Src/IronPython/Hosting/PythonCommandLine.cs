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
using System.Collections.Generic;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using System.IO;
using IronPython.Compiler;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Shell;
using System.Reflection;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Utils;
using IronPython.Runtime.Calls;

namespace IronPython.Hosting {
   
    /// <summary>
    /// A simple Python command-line should mimic the standard python.exe
    /// </summary>
    public class PythonCommandLine : CommandLine {

        private new PythonConsoleOptions Options { get { return (PythonConsoleOptions)base.Options; } }
        
        public PythonCommandLine() {
        }

        protected override string Logo {
            get {
                return String.Format("IronPython console: {0}{1}{2}{1}",
                    Engine.LanguageVersion, Environment.NewLine, "Copyright (c) Microsoft Corporation. All rights reserved.");
            }
        }
        
        private int GetEffectiveExitCode(PythonSystemExitException e) {
            object nonIntegerCode;
            int exitCode = e.GetExitCode(out nonIntegerCode);
            if (nonIntegerCode != null) {
                Console.WriteLine(nonIntegerCode.ToString(), Style.Error);
            }
            return exitCode;
        }

        protected override void Shutdown(IScriptEngine engine) {
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
                Engine.ExecuteProgram(Engine.CreateScriptSourceFromString("import " + Options.ModuleToRun, SourceCodeKind.Statements));
                return 0;
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
                    string fullPath = ScriptDomainManager.CurrentManager.PAL.GetFullPath(Options.FileName);
                    DefaultContext.DefaultPythonContext.AddToPath(Path.GetDirectoryName(fullPath));
                }
            }

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
                foreach (object o in PythonContext.GetSystemState(null).path) {
                    string str = o as string;
                    if (str == null) continue;

                    string libpath = Path.Combine(str, Options.ModuleToRun + ".py");
                    if (File.Exists(libpath)) {
                        // cast to List is a little scary but safe during startup
                        ((List)SystemState.Instance.argv)[0] = libpath;
                        break;
                    }
                }
            }
        }

        private ScriptScope/*!*/ CreateMainModule() {
            ModuleOptions trueDiv = (PythonContext.GetPythonOptions(null).DivisionOptions == PythonDivisionOptions.New) ? ModuleOptions.TrueDivision : ModuleOptions.None;
            PythonModule module = DefaultContext.DefaultPythonContext.CreateModule("__main__", trueDiv | ModuleOptions.PublishModule);
            module.Scope.SetName(Symbols.Doc, null);
            return module.Scope.ToScriptScope();
        }

        
        private void InitializePath() {
            DefaultContext.DefaultPythonContext.AddToPath(ScriptDomainManager.CurrentManager.PAL.CurrentDirectory);

#if !SILVERLIGHT // paths, environment vars
            if (!Options.IgnoreEnvironmentVariables) {
                string path = Environment.GetEnvironmentVariable("IRONPYTHONPATH");
                if (path != null && path.Length > 0) {
                    string[] paths = path.Split(Path.PathSeparator);
                    foreach (string p in paths) {
                        DefaultContext.DefaultPythonContext.AddToPath(p);
                    }
                }
            }

            string entry = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string site = Path.Combine(entry, "Lib");
            DefaultContext.DefaultPythonContext.AddToPath(site);

            // add DLLs directory if it exists            
            string dlls = Path.Combine(entry, "DLLs");
            if (Directory.Exists(dlls)) {
                DefaultContext.DefaultPythonContext.AddToPath(dlls);
            }
#endif
        }

        private string InitializeModules() {
            string version = Engine.LanguageVersion.ToString();
            
            this.Module = CreateMainModule();
            
#if SILVERLIGHT // paths
            string executable = "";
            string prefix = "";
#else
            string executable = Assembly.GetEntryAssembly().Location;
            string prefix = Path.GetDirectoryName(executable);
#endif
            PythonContext.GetSystemState(null).SetHostVariables(prefix, executable, version);
            return version;
        }

        private void ImportSite() {
#if !SILVERLIGHT // paths
            if (Options.SkipImportSite)
                return;

            try {
                // TODO: do better
                Engine.ExecuteProgram(Engine.CreateScriptSourceFromString("import site", SourceCodeKind.SingleStatement));
            } catch (Exception e) {
                Console.Write(Engine.FormatException(e), Style.Error);
            }
#endif

        }

        #endregion

        #region Interactive

        protected override int RunInteractive() {
            PrintLogo();

            int result = 1;
            try {
                RunStartup();
                result = 0;
            } catch (PythonSystemExitException pythonSystemExit) {
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
                        Engine.Execute(Module, Engine.CreateScriptSourceFromFile(startup));
                    } catch (Exception e) {
                        if (e is PythonSystemExitException) throw;
                        Console.Write(Engine.FormatException(e), Style.Error);
                    } finally {
                        Engine.DumpDebugInfo();
                    }
                } else {
                    try {
                        Engine.Execute(Module, Engine.CreateScriptSourceFromFile(startup));
                    } finally {
                        Engine.DumpDebugInfo();
                    }
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
            } catch (PythonSystemExitException se) {
                return GetEffectiveExitCode(se);
            }
        }

        protected override string ReadLine(int autoIndentSize) {
            string res = base.ReadLine(autoIndentSize);

            ScriptDomainManager.CurrentManager.DispatchCommand(null);

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
                if (Options.Introspection)
                    Module = module;

                Engine.Execute(module, Engine.CreateScriptSourceFromString(command, SourceCodeKind.File));
                result = 0;
            } catch (PythonSystemExitException pythonSystemExit) {
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
                // TODO: move to compiler options
                ScriptDomainManager.Options.AssemblyGenAttributes |= Microsoft.Scripting.Generation.AssemblyGenAttributes.EmitDebugInfo;
                
                ScriptCode compiledCode;
                PythonModule module = DefaultContext.DefaultPythonContext.CompileModule(fileName, "__main__", ModuleOptions.PublishModule | ModuleOptions.Optimized, Options.SkipFirstSourceLine, out compiledCode);

                if (Options.Introspection) {
                    Module = module.Scope.ToScriptScope();
                }

                compiledCode.Run(module.Scope, module);
                return 0;
            } catch (PythonSystemExitException pythonSystemExit) {
                
                // disable introspection when exited:
                Options.Introspection = false;

                return GetEffectiveExitCode(pythonSystemExit);
            }
        }

        #endregion
    }
}
