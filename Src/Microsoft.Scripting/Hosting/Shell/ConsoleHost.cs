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
using System.Runtime.CompilerServices;
using System.Scripting;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using System.Text;
using System.Threading;

namespace Microsoft.Scripting.Hosting.Shell {

    public abstract class ConsoleHost {
        private int _exitCode;
        private ConsoleHostOptionsParser _optionsParser;
        private ScriptRuntime _runtime;
        private ScriptEngine _engine;
        private OptionsParser _languageOptionsParser;

        public ConsoleHostOptions Options { get { return _optionsParser.Options; } }
        public ScriptDomainOptions GlobalOptions { get { return _optionsParser.GlobalOptions; } }
        public ScriptRuntimeSetup RuntimeConfig { get { return _optionsParser.RuntimeConfig; } }

        public ScriptEngine Engine { get { return _engine; } }
        public ScriptRuntime Runtime { get { return _runtime; } }

        protected ConsoleHost() {
        }

        protected void InitializeOptionsParser() {
            if (_optionsParser == null) {
                ScriptRuntimeSetup setup = CreateScriptEnvironmentSetup();
                ConsoleHostOptions options = new ConsoleHostOptions();
                _optionsParser = new ConsoleHostOptionsParser(options, ScriptDomainManager.Options, setup);
            }
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

        #region Customization

        protected virtual void ParseHostOptions(string[] args) {
            _optionsParser.Parse(args);
        }

        /// <summary>
        /// Gets the ConsoleHostOptions so that languages can provide values from 
        /// their own command line parser.
        /// </summary>
        public virtual ConsoleHostOptions HostOptions {
            get {
                return _optionsParser.Options;
            }
        }

        protected virtual ScriptRuntimeSetup CreateScriptEnvironmentSetup() {
            return new ScriptRuntimeSetup(true);
        }

        protected virtual ScriptEngine CreateEngine() {
            ScriptEngine engine;
            if (Options.LanguageId != null) {
                return Runtime.GetEngine(Options.LanguageId);
            } else if (Options.RunFile != null && Runtime.TryGetEngineByFileExtension(Path.GetExtension(Options.RunFile), out engine)) {
                return engine;
            } else {
                throw new InvalidOptionException("No language specified.");
            }
        }

        protected virtual CommandLine CreateCommandLine() {
            return new CommandLine();
        }

        protected virtual OptionsParser CreateOptionsParser() {
            return new DefaultOptionsParser();
        }

        protected virtual IConsole CreateConsole(ScriptEngine engine, CommandLine commandLine, ConsoleOptions options) {
            ContractUtils.RequiresNotNull(commandLine, "commandLine");
            ContractUtils.RequiresNotNull(options, "options");

            if (options.TabCompletion) {
                return CreateSuperConsole(commandLine, options.ColorfulConsole);
            } else {
                return new BasicConsole(options.ColorfulConsole);
            }
        }

        // The advanced console functions are in a special non-inlined function so that 
        // dependencies are pulled in only if necessary.
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static IConsole CreateSuperConsole(CommandLine commandLine, bool isColorful) {
            return new SuperConsole(commandLine, isColorful);
        }

        #endregion

        /// <summary>
        /// To be called from entry point.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public int Run(string[] args) {
            InitializeOptionsParser();

            try {
                ParseHostOptions(args);
            } catch (InvalidOptionException e) {
                Console.Error.WriteLine("Invalid argument: " + e.Message);
                return _exitCode = 1;
            }

            SetEnvironment();

            // TODO: this initialization needs to be fixed when hosting config is fixed:
            _runtime = ScriptRuntime.Create(RuntimeConfig);
            _languageOptionsParser = CreateOptionsParser();

            try {
                _engine = CreateEngine();
            } catch (Exception e) {
                Console.Error.WriteLine(e.Message);
                return _exitCode = 1;
            }

            _engine.SetScriptSourceSearchPaths(Options.SourceUnitSearchPaths);

            _languageOptionsParser.Engine = _engine;
            _languageOptionsParser.Platform = _runtime.Host.PlatformAdaptationLayer;
            _languageOptionsParser.EngineOptions = _engine.Options;
            _languageOptionsParser.GlobalOptions = GlobalOptions;

            try {
                _languageOptionsParser.Parse(Options.IgnoredArgs.ToArray());
            } catch (InvalidOptionException e) {
                Console.Error.WriteLine(e.Message);
                return _exitCode = -1;
            }

            if (Options.RunAction == ConsoleHostOptions.Action.DisplayHelp) {
                PrintHelp();
                return _exitCode = 0;
            }

            Execute();
            return _exitCode;
        }

        #region Printing help

        protected virtual void PrintHelp() {
            Console.WriteLine(GetHelp());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual string GetHelp() {
            StringBuilder sb = new StringBuilder();

            string[,] optionsHelp = Options.GetHelp();

            sb.AppendLine(String.Format("Usage: {0}.exe [<dlr-options>] [--] [<language-specific-command-line>]", ExeName));
            sb.AppendLine();

            sb.AppendLine("DLR options (both slash or dash could be used to prefix options):");
            ArrayUtils.PrintTable(sb, optionsHelp);
            sb.AppendLine();

            sb.AppendLine("Language specific command line:");
            PrintLanguageHelp(sb);
            sb.AppendLine();

            return sb.ToString();
        }

        public void PrintLanguageHelp(StringBuilder output) {
            ContractUtils.RequiresNotNull(output, "output");

            string commandLine, comments;
            string[,] options, environmentVariables;

            CreateOptionsParser().GetHelp(out commandLine, out options, out environmentVariables, out comments);

            // only display language specific options if one or more optinos exists.
            if (commandLine != null || options != null || environmentVariables != null || comments != null) {
                if (commandLine != null) {
                    output.AppendLine(commandLine);
                    output.AppendLine();
                }

                if (options != null) {
                    output.AppendLine("Options:");
                    ArrayUtils.PrintTable(output, options);
                    output.AppendLine();
                }

                if (environmentVariables != null) {
                    output.AppendLine("Environment variables:");
                    ArrayUtils.PrintTable(output, environmentVariables);
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
            if (Options.IsMTA) {
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
            Debug.Assert(_engine != null);

            switch (Options.RunAction) {
                case ConsoleHostOptions.Action.None:
                case ConsoleHostOptions.Action.RunConsole:
                    _exitCode = RunCommandLine();
                    break;

                case ConsoleHostOptions.Action.RunFile:
                    _exitCode = RunFile();
                    break;

                default:
                    throw Assert.Unreachable;
            }
        }

        private void SetEnvironment() {
            Debug.Assert(Options.EnvironmentVars != null);

#if !SILVERLIGHT
            foreach (string env in Options.EnvironmentVars) {
                if (!String.IsNullOrEmpty(env)) {
                    string[] var_def = env.Split('=');
                    System.Environment.SetEnvironmentVariable(var_def[0], (var_def.Length > 1) ? var_def[1] : "");
                }
            }
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private int RunFile() {
            Debug.Assert(_engine != null);

            int result = 0;
            try {
                return _engine.CreateScriptSourceFromFile(Options.RunFile).ExecuteProgram();
#if SILVERLIGHT 
            } catch (ExitProcessException e) {
                result = e.ExitCode;
#endif
            } catch (Exception e) {
                UnhandledException(Engine, e);
                result = 1;
            } finally {
                try {
                    Snippets.Shared.Dump();
                } catch (Exception) {
                    result = 1;
                }
            }

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private int RunCommandLine() {
            Debug.Assert(_engine != null);

            CommandLine commandLine = CreateCommandLine();
            ConsoleOptions consoleOptions = _languageOptionsParser.ConsoleOptions;

            if (consoleOptions.PrintVersionAndExit) {
                Console.WriteLine("{0} {1} on .NET {2}", Engine.LanguageDisplayName, Engine.LanguageVersion, typeof(String).Assembly.GetName().Version);
                return 0;
            }

            if (consoleOptions.PrintUsageAndExit) {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Usage: {0}.exe ", ExeName);
                PrintLanguageHelp(sb);
                Console.Write(sb.ToString());
                return 0;
            }

            IConsole console = CreateConsole(Engine, commandLine, consoleOptions);

            int exitCode = 0;
            try {
                if (consoleOptions.HandleExceptions) {
                    try {
                        exitCode = commandLine.Run(Engine, console, consoleOptions);
                    } catch (Exception e) {
                        UnhandledException(Engine, e);
                        exitCode = 1;
                    }
                } else {
                    exitCode = commandLine.Run(Engine, console, consoleOptions);
                }
            } finally {
                try {
                    Snippets.Shared.Dump();
                } catch (Exception) {
                    exitCode = 1;
                }
            }

            return exitCode;
        }

        protected virtual void UnhandledException(ScriptEngine engine, Exception e) {
            Console.Error.Write("Unhandled exception");
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
