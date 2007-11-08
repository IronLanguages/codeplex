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

namespace IronPython.Hosting {
   
    /// <summary>
    /// A simple Python command-line should mimic the standard python.exe
    /// </summary>
    public class PythonCommandLine : CommandLine {

        private new PythonConsoleOptions Options { get { return (PythonConsoleOptions)base.Options; } }
        private new PythonEngine Engine { get { return (PythonEngine)base.Engine; } }
        
        public PythonCommandLine() {
        }

        protected override string Logo {
            get {
                return String.Format("IronPython console: {0}{1}{2}{1}",
                    Engine.VersionString, Environment.NewLine, Engine.Copyright);
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
                Engine.Execute("import " + Options.ModuleToRun);
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
                    Engine.AddToPath(Path.GetDirectoryName(fullPath));
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
                foreach (object o in PythonEngine.CurrentEngine.SystemState.path) {
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

        private ScriptModule CreateMainModule() {
            ModuleOptions trueDiv = (Engine.Options.DivisionOptions == PythonDivisionOptions.New) ? ModuleOptions.TrueDivision : ModuleOptions.None;
            ScriptModule module = Engine.CreateModule("__main__", trueDiv | ModuleOptions.PublishModule);
            module.Scope.SetName(Symbols.Doc, null);

            // TODO: 
            // module.Scope.SetName(Symbols.Doc, null);
            return module;
        }

        
        private void InitializePath() {
            Engine.AddToPath(ScriptDomainManager.CurrentManager.PAL.GetCurrentDirectory());

#if !SILVERLIGHT // paths, environment vars
            if (!Options.IgnoreEnvironmentVariables) {
                string path = Environment.GetEnvironmentVariable("IRONPYTHONPATH");
                if (path != null && path.Length > 0) {
                    string[] paths = path.Split(Path.PathSeparator);
                    foreach (string p in paths) {
                        Engine.AddToPath(p);
                    }
                }
            }

            string site = Assembly.GetEntryAssembly().Location;
            site = Path.Combine(Path.GetDirectoryName(site), "Lib");
            Engine.AddToPath(site);
#endif
        }

        private string InitializeModules() {
            string version = Engine.VersionString;
            
            this.Module = CreateMainModule();
            
#if SILVERLIGHT // paths
            string executable = "";
            string prefix = "";
#else
            string executable = Assembly.GetEntryAssembly().Location;
            string prefix = Path.GetDirectoryName(executable);
#endif
            Engine.InitializeModules(prefix, executable, version);
            return version;
        }

        private void ImportSite() {
#if !SILVERLIGHT // paths
            if (Options.SkipImportSite)
                return;

            try {
                // TODO: do better
                Engine.Execute("import site");
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
                        Engine.ExecuteFileContent(startup, Module);
                    } catch (Exception e) {
                        if (e is PythonSystemExitException) throw;
                        Console.Write(Engine.FormatException(e), Style.Error);
                    } finally {
                        Engine.DumpDebugInfo();
                    }
                } else {
                    try {
                        Engine.ExecuteFileContent(startup, Module);
                    } finally {
                        Engine.DumpDebugInfo();
                    }
                }
            }
#endif
        }

        public override int? TryInteractiveAction() {
            try {
                return base.TryInteractiveAction();
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
            int result = 1;

            if (Options.HandleExceptions) {
                try {
                    Engine.ExecuteCommand(command);
                    result = 0;
                } catch (PythonSystemExitException pythonSystemExit) {
                    result = GetEffectiveExitCode(pythonSystemExit);
                } catch (Exception e) {
                    Console.Write(Engine.FormatException(e), Style.Error);
                }
            } else {
                try {
                    Engine.ExecuteCommand(command);
                    result = 0;
                } catch (PythonSystemExitException pythonSystemExit) {
                    result = GetEffectiveExitCode(pythonSystemExit);
                }
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
                
                ScriptModule engineModule = Engine.CreateOptimizedModule(fileName, "__main__", true, 
                    Engine.Options.SkipFirstSourceLine);

                if (Options.Introspection)
                    Module = engineModule;

                engineModule.Execute();
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
