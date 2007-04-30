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
using System.Text;
using IronPython.Compiler;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Internal.Generation;

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
            if (arg == null) throw new ArgumentNullException("arg");

            Debug.Assert(_consoleOptions != null && _engineOptions != null);

            switch (arg) {
                case "-c":
                    _consoleOptions.Command = PeekNextArg();
                    _engineOptions.Arguments = PopRemainingArgs();
                    _engineOptions.Arguments[0] = arg;
                    break;

                case "-h":
                case "-help":
                case "-?":
                    _consoleOptions.PrintUsageAndExit = true;
                    IgnoreRemainingArgs();
                    break;

                case "-V": 
                    _consoleOptions.PrintVersionAndExit = true;
                    IgnoreRemainingArgs();
                    break;

                case "-O": GlobalOptions.DebugMode = false; break;
                case "-D": GlobalOptions.EngineDebug = true; break;

                // the following extension switches are in alphabetic order
                case "-X:AssembliesDir":

                    string dir = PopNextArg();

                    if (!ScriptDomainManager.CurrentManager.PAL.DirectoryExists(dir))
                        throw new System.IO.DirectoryNotFoundException(String.Format("Directory '{0}' doesn't exist.", dir));

                    GlobalOptions.BinariesDirectory = dir;
                    break;


                case "-X:FastEval": _engineOptions.FastEvaluation = true; break;
                case "-X:Frames": GlobalOptions.Frames = true; break;
                case "-X:GenerateAsSnippets": GlobalOptions.GenerateModulesAsSnippets = true; break;
                case "-X:GenerateReleaseAssemblies": GlobalOptions.AssemblyGenAttributes &= ~AssemblyGenAttributes.GenerateDebugAssemblies; break;
                case "-X:ILDebug": GlobalOptions.AssemblyGenAttributes |= AssemblyGenAttributes.ILDebug; break;

                case "-X:PassExceptions": _consoleOptions.HandleExceptions = false; break;
// TODO: #if !IRONPYTHON_WINDOW
                case "-X:ColorfulConsole": _consoleOptions.ColorfulConsole = true; break;
                case "-X:ExceptionDetail": _engineOptions.ExceptionDetail = true; break;
                case "-X:TabCompletion": _consoleOptions.TabCompletion = true; break;
                case "-X:AutoIndent": _consoleOptions.AutoIndent = true; break;
                case "-i": _consoleOptions.Introspection = true; break;
//#endif
                case "-X:NoOptimize": GlobalOptions.DebugCodeGeneration = true; break;
                case "-X:NoTraceback": GlobalOptions.DynamicStackTraceSupport = false; break;
                
                case "-X:MaxRecursion":
                    int max_rec;
                    if (!Utils.TryParseInt32(PopNextArg(), out max_rec))
                        throw new InvalidOptionException(String.Format("The argument for the {0} option must be an integer.", arg));

                    _engineOptions.MaximumRecursion = max_rec;
                    break;

                case "-X:PrivateBinding": GlobalOptions.PrivateBinding = true; break;
                case "-X:Python25": GlobalOptions.Python25 = true; break;
                case "-X:SaveAssemblies": GlobalOptions.AssemblyGenAttributes |= AssemblyGenAttributes.SaveAndReloadAssemblies; break;
                case "-X:ShowClrExceptions": _engineOptions.ShowClrExceptions = true; break;
                case "-X:SlowOps": GlobalOptions.FastOps = false; break;
                case "-X:StaticMethods": GlobalOptions.AssemblyGenAttributes |= AssemblyGenAttributes.GenerateStaticMethods; break;
                case "-X:TrackPerformance": // accepted but ignored on retail builds
#if DEBUG
                    GlobalOptions.TrackPerformance = true;
#endif
                    break;

                case "-x": _engineOptions.SkipFirstSourceLine = true; break;
                case "-v": GlobalOptions.Verbose = true; break;
                case "-u": GlobalOptions.BufferedStandardOutAndError = false; break;
                case "-S": _consoleOptions.ImportSite = false; break;
                case "-E": _consoleOptions.IgnoreEnvironmentVariables = true; break;
                case "-t": _engineOptions.IndentationInconsistencySeverity = Severity.Warning; break;
                case "-tt": _engineOptions.IndentationInconsistencySeverity = Severity.Error; break;
                case "-OO":
                    GlobalOptions.DebugMode = false;
                    GlobalOptions.StripDocStrings = true;
                    break;

                case "-Q":
                    string level = PopNextArg();

                    switch (level) {
                        case "old": GlobalOptions.Division = DivisionOption.Old; break;
                        case "new": GlobalOptions.Division = DivisionOption.New; break;
                        case "warn": GlobalOptions.Division = DivisionOption.Warn; break;
                        case "warnall": GlobalOptions.Division = DivisionOption.WarnAll; break;
                        default:
                            throw InvalidOptionValue(arg, level);
                    }
                    break;

                case "-Qold": GlobalOptions.Division = DivisionOption.Old; break;
                case "-Qnew": GlobalOptions.Division = DivisionOption.New; break;
                case "-Qwarn": GlobalOptions.Division = DivisionOption.Warn; break;
                case "-Qwarnall": GlobalOptions.Division = DivisionOption.WarnAll; break;
                
                case "-W":
                    if (_engineOptions.WarningFilters == null)
                        _engineOptions.WarningFilters = new List<string>();

                    _engineOptions.WarningFilters.Add(PopNextArg());
                    break;

                default:
                    _consoleOptions.FileName = arg;
                    PushArgBack();
                    _engineOptions.Arguments = PopRemainingArgs();
                    break;
            }
        }

        public override void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments) {

            commandLine = "[options] [file|- [arguments]]";
            
            options = new string[,] {
                { "-c cmd",                 "Program passed in as string (terminates option list)" },
                { "-h",                     "Display usage" },
                { "-x",                     "Skip first line of the source" },
                { "-V",                     "Print the Python version number and exit" },
                { "-O",                     "Enable optimizations" },
#if !IRONPYTHON_WINDOW
                { "-i",                     "Inspect interactively after running script" },
                { "-v",                     "Verbose (trace import statements) (also PYTHONVERBOSE=x)" },
#endif
#if DEBUG
                { "-D",                     "EngineDebug mode" },
#endif
                { "-x",                     "Skip first line of the source" },
                { "-u",                     "Unbuffered stdout & stderr" },
                { "-E",                     "Ignore environment variables" },
                { "-OO",                    "Remove doc-strings in addition to the -O optimizations" },
                { "-Q arg",                 "Division options: -Qold (default), -Qwarn, -Qwarnall, -Qnew" },
                { "-S",                     "Don't imply 'import site' on initialization" },
                { "-t",                     "Issue warnings about inconsistent tab usage" },
                { "-tt",                    "Issue errors for inconsistent tab usage" },
                { "-W arg",                 "Warning control (arg is action:message:category:module:lineno)" },

                { "-X:AutoIndent",          "" },
                { "-X:AssembliesDir",       "Set the directory for saving generated assemblies" },
#if !SILVERLIGHT
                { "-X:ColorfulConsole",     "Enable ColorfulConsole" },
#endif
                { "-X:ExceptionDetail",     "Enable ExceptionDetail mode" },
                { "-X:FastEval",            "Enable fast eval" },
                { "-X:Frames",              "Generate custom frames" },
                { "-X:GenerateAsSnippets",  "Generate code to run in snippet mode" },
                { "-X:ILDebug",             "Output generated IL code to a text file for debugging" },
                { "-X:MaxRecursion",        "Set the maximum recursion level" },
#if !SILVERLIGHT
                { "-X:MTA",                 "Run in multithreaded apartment" },
#endif
                { "-X:NoOptimize",          "Disable JIT optimization in generated code" },
                { "-X:NoTraceback",         "Do not emit traceback code" },
                { "-X:PassExceptions",      "Do not catch exceptions that are unhandled by Python code" },
                { "-X:PrivateBinding",      "Enable binding to private members" },
                { "-X:SaveAssemblies",      "Save generated assemblies" },
                { "-X:ShowClrExceptions",   "Display CLS Exception information" },
                { "-X:SlowOps",             "Enable fast ops" },
                { "-X:StaticMethods",       "Generate static methods only" },
#if !SILVERLIGHT
                { "-X:TabCompletion",       "Enable TabCompletion mode" },
#endif
#if DEBUG
                { "-X:TrackPerformance",    "Track performance sensitive areas" },
#endif
           };

            environmentVariables = new string[,] {
                { "IRONPYTHONPATH",        "Path to search for module" },
                { "IRONPYTHONSTARTUP",     "Startup module" }
            };

            comments = null;
        }
    }
}