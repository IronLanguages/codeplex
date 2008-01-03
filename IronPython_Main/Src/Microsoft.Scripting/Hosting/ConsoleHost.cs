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
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Shell;
using System.Threading;
using System.IO;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {

    public abstract class ConsoleHost {
        private int _exitCode;
        private ConsoleHostOptions _options = new ConsoleHostOptions();

        public ConsoleHostOptions Options { get { return _options; } }

        protected ConsoleHost() {
        }

        /// <summary>
        /// Console Host entry-point .exe name.
        /// </summary>
        protected virtual string ExeName {
            get {
#if !SILVERLIGHT
                return Assembly.GetEntryAssembly().GetName().Name;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
            Console.WriteLine(GetHelp());            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual string GetHelp() {
            StringBuilder sb = new StringBuilder();
            
            string[,] options_help = _options.GetHelp();

            sb.AppendLine(String.Format("{0}: {1}.exe [<host-options>] [--] [<language-specific-command-line>]", Resources.Usage, ExeName));
            sb.AppendLine();

            if (options_help != null) {
                sb.AppendLine("Host options:");

                ArrayUtils.PrintTable(sb, options_help);                
            }

            if (_options.ScriptEngine != null) {
                sb.AppendLine(String.Format("{0} command line:", _options.ScriptEngine.LanguageDisplayName));
                sb.AppendLine();
                PrintLanguageHelp(_options.ScriptEngine, sb);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public void PrintLanguageHelp(IScriptEngine provider, StringBuilder output) {
            Contract.RequiresNotNull(provider, "provider");
            Contract.RequiresNotNull(output, "output");

            string command_line, comments;
            string[,] options, environment_variables;

            provider.GetService<OptionsParser>().GetHelp(out command_line, out options, out environment_variables, out comments);

            // only display language specific options if one or more optinos exists.
            if (command_line != null || options != null || environment_variables != null || comments != null) {
                output.AppendLine(String.Format("{0} command line:", _options.ScriptEngine.LanguageDisplayName));
                output.AppendLine();

                if (command_line != null) {
                    output.AppendLine(String.Format("{0}: {1}", Resources.Usage, command_line));
                    output.AppendLine();
                }

                if (options != null) {
                    output.AppendLine(String.Format("{0}:", Resources.Options));
                    ArrayUtils.PrintTable(output, options);
                    output.AppendLine();
                }

                if (environment_variables != null) {
                    output.AppendLine(String.Format("{0}:", Resources.EnvironmentVariables));
                    ArrayUtils.PrintTable(output, environment_variables);
                    output.AppendLine();
                }

                if (comments != null) {
                    output.Append(comments);
                    output.AppendLine();
                }

                output.AppendLine();
            }
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

            Debug.Assert(_options.ScriptEngine != null);

            OptionsParser opt_parser = _options.ScriptEngine.GetService<OptionsParser>();
                
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

            EngineOptions engine_options = (optionsParser != null) ? optionsParser.EngineOptions : null;

            IScriptEngine engine = _options.ScriptEngine; //.GetEngine(engine_options);

            engine.SetScriptSourceSearchPaths(_options.SourceUnitSearchPaths);

            int result = 0;
            foreach (string filePath in _options.Files) {
                SourceUnit sourceUnit = ScriptDomainManager.CurrentManager.Host.TryGetSourceFileUnit(engine, filePath, StringUtils.DefaultEncoding, SourceCodeKind.File);
                if (sourceUnit == null) {
                    throw new FileNotFoundException(string.Format("Source file '{0}' not found.", filePath));
                }
                result = RunFile(engine, sourceUnit);
            }

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual int RunFile(IScriptEngine engine, SourceUnit sourceUnit) {
            try {
                return engine.ExecuteProgram(sourceUnit);
            } catch (Exception e) {
                UnhandledException(engine, e);
                return 1;
            }
        }

        protected virtual int ExecuteFile(string file, string[] args) {
            Contract.RequiresNotNull(file, "file");
            Contract.RequiresNotNull(args, "args");

            Assembly assembly = ScriptDomainManager.CurrentManager.PAL.LoadAssembly(file);
            MethodInfo method = null;

            foreach (Type type in assembly.GetExportedTypes()) {
                method = type.GetMethod("Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (method != null) break;
            }

            if (method == null) {
                throw new MissingMethodException(String.Format("Missing entry point (file '{0}').", file));
            }

            object result = null;
            ParameterInfo[] ps = method.GetParameters();
            if (ps.Length == 1 && ps[0].ParameterType.IsAssignableFrom(typeof(string[]))) {
                result = method.Invoke(null, new object[] { args });
            } else {
                result = method.Invoke(null, ArrayUtils.EmptyObjects);
            }

            return (result is int) ? (int)result : Environment.ExitCode;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        protected virtual int RunCommandLine(OptionsParser optionsParser) {
            Contract.RequiresNotNull(optionsParser, "optionsParser");
            
            CommandLine command_line;
            ConsoleOptions console_options;
            EngineOptions engine_options;

            console_options = optionsParser.ConsoleOptions;
            engine_options = optionsParser.EngineOptions;

            command_line = _options.ScriptEngine.GetService<CommandLine>();

            IScriptEngine engine = _options.ScriptEngine; //.GetEngine(engine_options);

            if (console_options.PrintVersionAndExit) {
                Console.WriteLine("{0} {1} on .NET {2}", engine.LanguageDisplayName, engine.LanguageVersion, typeof(String).Assembly.GetName().Version);
                return 0;
            }

            if (console_options.PrintUsageAndExit) {
                if (optionsParser != null) {
                    StringBuilder sb = new StringBuilder();
                    PrintLanguageHelp(_options.ScriptEngine, sb);
                    Console.Write(sb.ToString());
                }
                return 0;
            }

            engine.SetScriptSourceSearchPaths(_options.SourceUnitSearchPaths);

            IConsole console = _options.ScriptEngine.GetService<IConsole>(command_line, console_options);

            int result;
            if (console_options.HandleExceptions) {
                try {
                    result = command_line.Run(engine, console, console_options);
                } catch (Exception e) {
                    UnhandledException(engine, e);
                    result = 1;
                }

                ScriptEngine se = engine as ScriptEngine;
                if (se != null) {
                    try {
                        se.DumpDebugInfo();
                    } catch {
                        result = 1;
                    }
                }

                return result;
            } else {
                try {
                    return command_line.Run(engine, console, console_options);
                } finally {
                    ScriptEngine se = engine as ScriptEngine;
                    if (se != null) {
                        se.DumpDebugInfo();
                    }
                }
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
