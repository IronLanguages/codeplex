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
using System.Text;
using IronPython.Compiler;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace IronPython.Hosting {

    sealed class PythonOptionsParser : OptionsParser {

        private PythonConsoleOptions _consoleOptions;
        private PythonEngineOptions _engineOptions;

        public override ConsoleOptions ConsoleOptions { get { return _consoleOptions; } set { _consoleOptions = (PythonConsoleOptions)value; } } 
        public override EngineOptions EngineOptions { get { return _engineOptions; } set { _engineOptions = (PythonEngineOptions)value; } }
        
        public PythonOptionsParser() {
        }

        public override ConsoleOptions GetDefaultConsoleOptions() {
            return new PythonConsoleOptions();
        }

        public override EngineOptions GetDefaultEngineOptions() {
            return new PythonEngineOptions();
        }

        public override void Parse(string[] args) {
            if (_consoleOptions == null) _consoleOptions = new PythonConsoleOptions();
            if (_engineOptions == null) _engineOptions = new PythonEngineOptions();

            base.Parse(args);
        }

        /// <exception cref="Exception">On error.</exception>
        protected override void ParseArgument(string arg) {
            Contract.RequiresNotNull(arg, "arg");

            Debug.Assert(_consoleOptions != null && _engineOptions != null);

            switch (arg) {
                case "-c":
                    _consoleOptions.Command = PeekNextArg();
                    _engineOptions.Arguments = PopRemainingArgs();
                    _engineOptions.Arguments[0] = arg;
                    break;

                case "-X:MaxRecursion":
                    int max_rec;
                    if (!StringUtils.TryParseInt32(PopNextArg(), out max_rec))
                        throw new InvalidOptionException(String.Format("The argument for the {0} option must be an integer.", arg));

                    _engineOptions.MaximumRecursion = max_rec;
                    break;

                case "-m":
                    _consoleOptions.ModuleToRun = PeekNextArg();
                    _engineOptions.Arguments = PopRemainingArgs();                                   
                    break;
                case "-x": _engineOptions.SkipFirstSourceLine = true; break;
                case "-v": GlobalOptions.Verbose = true; break;
                case "-u": GlobalOptions.BufferedStandardOutAndError = false; break;
                case "-S": _consoleOptions.SkipImportSite = true; break;
                case "-E": _consoleOptions.IgnoreEnvironmentVariables = true; break;
                case "-t": _engineOptions.IndentationInconsistencySeverity = Severity.Warning; break;
                case "-tt": _engineOptions.IndentationInconsistencySeverity = Severity.Error; break;

                case "-Q":
                    string level = PopNextArg();

                    switch (level) {
                        case "old": _engineOptions.DivisionOptions = PythonDivisionOptions.Old; break;
                        case "new": _engineOptions.DivisionOptions = PythonDivisionOptions.New; break;
                        case "warn": _engineOptions.DivisionOptions = PythonDivisionOptions.Warn; break;
                        case "warnall": _engineOptions.DivisionOptions = PythonDivisionOptions.WarnAll; break;
                        default:
                            throw InvalidOptionValue(arg, level);
                    }
                    break;

                case "-Qold": _engineOptions.DivisionOptions = PythonDivisionOptions.Old; break;
                case "-Qnew": _engineOptions.DivisionOptions = PythonDivisionOptions.New; break;
                case "-Qwarn": _engineOptions.DivisionOptions = PythonDivisionOptions.Warn; break;
                case "-Qwarnall": _engineOptions.DivisionOptions = PythonDivisionOptions.WarnAll; break;
                
                case "-W":
                    if (_engineOptions.WarningFilters == null)
                        _engineOptions.WarningFilters = new List<string>();

                    _engineOptions.WarningFilters.Add(PopNextArg());
                    break;

                case "-X:PreferComDispatch": _engineOptions.PreferComDispatchOverTypeInfo = true; break;
                case "-":
                    PushArgBack();
                    _engineOptions.Arguments = PopRemainingArgs();
                    break;
                default:
                    base.ParseArgument(arg);

                    if (ConsoleOptions.FileName != null) {
                        PushArgBack();
                        _engineOptions.Arguments = PopRemainingArgs();
                    }
                    break;
            }
        }

        public override void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments) {
            string [,] standardOptions;
            base.GetHelp(out commandLine, out standardOptions, out environmentVariables, out comments);
            
            string [,] pythonOptions = new string[,] {
#if !IRONPYTHON_WINDOW
                { "-v",                     "Verbose (trace import statements) (also PYTHONVERBOSE=x)" },
#endif
                { "-m module",              "run library module as a script"},
                { "-x",                     "Skip first line of the source" },
                { "-u",                     "Unbuffered stdout & stderr" },
                { "-E",                     "Ignore environment variables" },
                { "-Q arg",                 "Division options: -Qold (default), -Qwarn, -Qwarnall, -Qnew" },
                { "-S",                     "Don't imply 'import site' on initialization" },
                { "-t",                     "Issue warnings about inconsistent tab usage" },
                { "-tt",                    "Issue errors for inconsistent tab usage" },
                { "-W arg",                 "Warning control (arg is action:message:category:module:lineno)" },

                { "-X:MaxRecursion",        "Set the maximum recursion level" },
                { "-X:PreferComDispatch",   "Enable direct support for IDispatch COM objects" },    
            };

            // Append the Python-specific options and the standard options
            options = ArrayUtils.Concatenate(pythonOptions, standardOptions);

            Debug.Assert(environmentVariables.GetLength(0) == 0); // No need to append if the default is empty
            environmentVariables = new string[,] {
                { "IRONPYTHONPATH",        "Path to search for module" },
                { "IRONPYTHONSTARTUP",     "Startup module" }
            };

        }
    }
}