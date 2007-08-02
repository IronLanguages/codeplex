/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Shell;
using System.Threading;
using System.IO;

namespace Microsoft.Scripting.Hosting {

    public abstract class ConsoleHost {
        private int _exitCode;
        private ConsoleHostOptions _options = new ConsoleHostOptions();

        public ConsoleHostOptions Options { get { return _options; } }

        public ConsoleHost() {
        }

        /// <summary>
        /// Console Host entry-point .exe name.
        /// </summary>
        protected virtual string ExeName {
            get {
#if !SILVERLIGHT
                return GetType().Assembly.GetName().Name;
#else
                return "ConsoleHost";
#endif
            }
        }

        /// <summary>
        /// Console Host version.
        /// </summary>
        protected virtual Version Version { get { return new Version(2, 0, 0, 0); } }

        /// <summary>
        /// Console Host name.
        /// </summary>
        protected virtual string Name { get { return null; } }

        protected virtual void Initialize() {
            // A console application needs just the simple setup.
            // The full setup is potentially expensive as it can involve loading System.Configuration.dll
            ScriptEnvironmentSetup setup = new ScriptEnvironmentSetup(true);
            ScriptDomainManager manager;
            ScriptDomainManager.TryCreateLocal(setup, out manager);
        }

        /// <summary>
        /// To be called from entry point.
        /// </summary>
        public int Run(string[] args) {
            
            try {
                Initialize();
            } catch (Exception e) {
                Console.Error.WriteLine("Console host failed to initialize:");
                PrintException(Console.Error, e);
                return 1;
            }

            try {
                new ConsoleHostOptionsParser(_options).Parse(args);
            } catch (Exception e) {
                Console.Error.WriteLine("Invalid argument:");
                PrintException(Console.Error, e);
                return 1;
            }

            Execute();
            return _exitCode;
        }

        #region Printing logo and help

        protected virtual void PrintLogo() {
            // nop by default
        }

        protected virtual void PrintHelp() {

            string[,] options_help = _options.GetHelp();

            Console.WriteLine("{0}: {1} [<host-options>] [--] [<language-specific-command-line>]", Resources.Usage, ExeName);
            Console.WriteLine();

            if (options_help != null) {
                Console.WriteLine("Host options:");

                Utils.Array.PrintTable(Console.Out, options_help);
                Console.WriteLine();
            }

            if (_options.LanguageProvider != null) {
                PrintLanguageSpecificOptions(_options.LanguageProvider);
            }
        }

        private void PrintLanguageSpecificOptions(ILanguageProvider provider) {
            Debug.Assert(provider != null);

            Console.WriteLine("{0} command line:", provider.LanguageDisplayName);
            Console.WriteLine();
            provider.GetOptionsParser().PrintHelp(Console.Out);
            Console.WriteLine();
        }

        #endregion

        private void Execute() {

#if !SILVERLIGHT
            if (_options.IsMTA) {
                Thread thread = new Thread(ExecuteInternal);
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();
                thread.Join();
                return;
            }
#endif            
            ExecuteInternal();
        }

        protected virtual void ExecuteInternal() {

            Debug.Assert(_options.LanguageProvider != null);

            if (_options.DisplayLogo) {
                PrintLogo();
            }
            
            if (_options.RunAction == ConsoleHostOptions.Action.DisplayHelp) {
                PrintHelp();
                _exitCode = 0; 
                return;
            }

            SetEnvironment();

            if (_options.RunAction == ConsoleHostOptions.Action.ExecuteFile) {
                _exitCode = ExecuteFile(_options.Files[_options.Files.Count - 1], _options.IgnoredArgs.ToArray());
                return;
            }
            
            OptionsParser opt_parser = _options.LanguageProvider.GetOptionsParser();
                
            opt_parser.GlobalOptions = ScriptDomainManager.Options;

            try {
                opt_parser.Parse(_options.IgnoredArgs.ToArray());
            } catch (InvalidOptionException e) {
                Console.Error.WriteLine(e.Message);
                _exitCode = -1;
                return;
            }

            switch (_options.RunAction) {
                case ConsoleHostOptions.Action.RunConsole:
                    _exitCode = RunCommandLine(opt_parser);
                    break;

                case ConsoleHostOptions.Action.RunFiles:
                    _exitCode = RunFiles(opt_parser);
                    break;
            }
        }

        private void SetEnvironment() {
            Debug.Assert(_options.EnvironmentVars != null);

#if !SILVERLIGHT
            foreach (string env in _options.EnvironmentVars) {
                if (!String.IsNullOrEmpty(env)) {
                    string[] var_def = env.Split('=');
                    System.Environment.SetEnvironmentVariable(var_def[0], (var_def.Length > 1) ? var_def[1] : "");
                }
            }
#endif
        }

        private int RunFiles(OptionsParser optionsParser) {

            EngineOptions engine_options = (optionsParser != null) ? optionsParser.EngineOptions : new EngineOptions();

            IScriptEngine engine = _options.LanguageProvider.GetEngine(engine_options);

            engine.SetSourceUnitSearchPaths(_options.SourceUnitSearchPaths);

            int result = 0;
            foreach (string file_path in _options.Files) {
                result = RunFile(engine, new SourceFileUnit(engine, file_path, Encoding.Default));
            }

            return result;
        }

        protected virtual int RunFile(IScriptEngine engine, SourceUnit sourceUnit) {
            try {
                return engine.ExecuteProgram(sourceUnit);
            } catch (Exception e) {
                UnhandledException(engine, e);
                return 1;
            }
        }

        protected virtual int ExecuteFile(string file, string[] args) {
            Assembly assembly = ScriptDomainManager.CurrentManager.PAL.LoadAssembly(file);
            MethodInfo method = null;

            foreach (Type type in assembly.GetExportedTypes()) {
                method = type.GetMethod("Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (method != null) break;
            }

            if (method == null) {
                throw new Exception(String.Format("Missing entry point (file '{0}').", file));
            }

            object result = null;
            ParameterInfo[] ps = method.GetParameters();
            if (ps.Length == 1 && ps[0].ParameterType.IsAssignableFrom(typeof(string[]))) {
                result = method.Invoke(null, new object[] { args });
            } else {
                result = method.Invoke(null, RuntimeHelpers.EmptyObjectArray);
            }

            return (result is int) ? (int)result : Environment.ExitCode;
        }

        protected virtual int RunCommandLine(OptionsParser optionsParser) {
            if (optionsParser == null) throw new ArgumentNullException("optionsParser");
            
            CommandLine command_line;
            ConsoleOptions console_options;
            EngineOptions engine_options;

            console_options = optionsParser.ConsoleOptions;
            engine_options = optionsParser.EngineOptions;

            command_line = _options.LanguageProvider.GetCommandLine();

            IScriptEngine engine = _options.LanguageProvider.GetEngine(engine_options);

            if (console_options.PrintVersionAndExit) {
                Console.WriteLine(engine.VersionString);
                return 0;
            }

            if (console_options.PrintUsageAndExit) {
                if (optionsParser != null) {
                    optionsParser.PrintHelp(Console.Out);
                }
                return 0;
            }

            engine.SetSourceUnitSearchPaths(_options.SourceUnitSearchPaths);

            IConsole console = _options.LanguageProvider.GetConsole(command_line, engine, console_options);

            if (console_options.HandleExceptions) {
                try {
                    return command_line.Run(engine, console, console_options);
                } catch (Exception e) {
                    UnhandledException(engine, e);
                    return 1;
                }
            } else {
                return command_line.Run(engine, console, console_options);
            }
        }

        protected virtual void UnhandledException(IScriptEngine engine, Exception e) {
            Console.Error.Write(Resources.UnhandledException);
            Console.Error.WriteLine(':');
            Console.Error.WriteLine(engine.FormatException(e));
        }

        protected static void PrintException(TextWriter output, Exception e) {
            Debug.Assert(output != null);

            while (e != null) {
                output.WriteLine(e);
                e = e.InnerException;
            }
        }
    }
}