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
using System.Globalization;
using System.Runtime.Serialization;
using System.Scripting;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Hosting.Shell {

    [Serializable]
    public class InvalidOptionException : Exception {
        public InvalidOptionException() { }
        public InvalidOptionException(string message) : base(message) { }
        public InvalidOptionException(string message, Exception inner) : base(message, inner) { }

#if !SILVERLIGHT // SerializationInfo
        protected InvalidOptionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

    public abstract class OptionsParser {
       
        private List<string> _ignoredArgs = new List<string>();
        private ScriptDomainOptions _globalOptions;
        private string[] _args;
        private int _current = -1;
        private PlatformAdaptationLayer/*!*/ _platform;

        // TODO: remove when hosting configuration is fixed:
        private ScriptEngine _engine;

        protected OptionsParser() {
            _platform = PlatformAdaptationLayer.Default;
        }

        // TODO: remove when hosting configuration is fixed:
        public ScriptEngine Engine {
            get { return _engine; }
            set { _engine = value; }
        }

        public PlatformAdaptationLayer/*!*/ Platform {
            get { return _platform; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _platform = value;
            }
        }

        public ScriptDomainOptions GlobalOptions { 
            get { 
                return _globalOptions; 
            } set { 
                _globalOptions = value; 
            } 
        }

        public virtual ConsoleOptions ConsoleOptions { 
            get {
                throw new NotSupportedException(); 
            } 
            set {
                throw new NotSupportedException(); 
            } 
        }

        public virtual EngineOptions EngineOptions { 
            get { 
                throw new NotSupportedException(); 
            } set { 
                throw new NotSupportedException(); 
            } 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual ConsoleOptions GetDefaultConsoleOptions() {
            return new ConsoleOptions();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual EngineOptions GetDefaultEngineOptions() {
            return new EngineOptions();
        }

        public IList<string> IgnoredArgs { get { return _ignoredArgs; } }
        
        /// <exception cref="InvalidOptionException">On error.</exception>
        public virtual void Parse(string[] args) {
            ContractUtils.RequiresNotNull(args, "args");

            if (_globalOptions == null) _globalOptions = new ScriptDomainOptions();

            _args = args;

            try {
                _current = 0;
                while (_current < args.Length) {
                    ParseArgument(args[_current++]);
                }
            } finally {
                _args = null;
                _current = -1;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected virtual void ParseArgument(string arg) {
            ContractUtils.RequiresNotNull(arg, "arg");

            // the following extension switches are in alphabetic order
            switch (arg) {
                case "-c":
                    ConsoleOptions.Command = PeekNextArg();
                    break;

                case "-h":
                case "-help":
                case "-?":
                    ConsoleOptions.PrintUsageAndExit = true;
                    IgnoreRemainingArgs();
                    break;

                case "-i": ConsoleOptions.Introspection = true; break;

                case "-V":
                    ConsoleOptions.PrintVersionAndExit = true;
                    IgnoreRemainingArgs();
                    break;

                case "-D": 
                    GlobalOptions.DebugMode = true; 
                    break;

                case "-X:AssembliesDir":

                    string dir = PopNextArg();

                    if (!_platform.DirectoryExists(dir)) {
                        throw new InvalidOptionException(String.Format("Directory '{0}' doesn't exist.", dir));
                    }

                    Snippets.Shared.SnippetsDirectory = dir;
                    break;

                case "-X:Interpret": EngineOptions.InterpretedMode = true; break;
                case "-X:Frames": GlobalOptions.Frames = true; break;

                case "-X:LightweightScopes": GlobalOptions.LightweightScopes = true; break;

                case "-X:PassExceptions": ConsoleOptions.HandleExceptions = false; break;
                // TODO: #if !IRONPYTHON_WINDOW
                case "-X:ColorfulConsole": ConsoleOptions.ColorfulConsole = true; break;
                case "-X:ExceptionDetail": EngineOptions.ExceptionDetail = true; break;
                case "-X:TabCompletion": ConsoleOptions.TabCompletion = true; break;
                case "-X:AutoIndent": ConsoleOptions.AutoIndent = true; break;
                //#endif

                // TODO: remove
                case "-X:DumpIL":
                case "-X:ShowIL":
                case "-X:ShowRules": 
                case "-X:DumpTrees": 
                case "-X:ShowTrees": 
                case "-X:ShowScopes":
                    SetCompilerDebugOption(arg.Substring(3));
                    break;

                case "-X:PerfStats": EngineOptions.PerfStats = true; break;
                case "-X:PrivateBinding": GlobalOptions.PrivateBinding = true; break;
                case "-X:SaveAssemblies": Snippets.Shared.SaveSnippets = true; break;
                case "-X:ShowClrExceptions": EngineOptions.ShowClrExceptions = true; break;
                case "-X:TrackPerformance": // accepted but ignored on retail builds
#if DEBUG
                    GlobalOptions.TrackPerformance = true;
#endif
                    break;

                case "-X:PreferComDispatch": 
                    GlobalOptions.PreferComDispatchOverTypeInfo = true; 
                    break;

                case "-X:CachePointersInApartment":
                    GlobalOptions.CachePointersInApartment = true;
                    break;

                default:
                    ConsoleOptions.FileName = arg;
                    // The language-specific parsers may want to do something like this to pass arguments to the script
                    //   PushArgBack();
                    //   EngineOptions.Arguments = PopRemainingArgs();
                    break;
            }
        }

        // Note: this works because it runs before the compiler picks up the
        // environment variable
        internal static void SetCompilerDebugOption(string option) {
#if !SILVERLIGHT
            string env = Environment.GetEnvironmentVariable("lambdacompiler_debug");
            Environment.SetEnvironmentVariable("lambdacompiler_debug", option + " " + env);
#endif
        }

        protected void IgnoreRemainingArgs() {
            while (_current < _args.Length) {
                _ignoredArgs.Add(_args[_current++]);
            }
        }

        protected string[] PopRemainingArgs() {
            string[] result = ArrayUtils.ShiftLeft(_args, _current);
            _current = _args.Length;
            return result;
        }

        protected string PeekNextArg() {
            if (_current < _args.Length)
                return _args[_current];
            else
                throw new InvalidOptionException(String.Format(CultureInfo.CurrentCulture, "Argument expected for the {0} option.", _current > 0 ? _args[_current - 1] : ""));
        }

        protected string PopNextArg() {
            string result = PeekNextArg();
            _current++;
            return result;
        }

        protected void PushArgBack() {
            _current--;
        }

        protected static Exception InvalidOptionValue(string option, string value) {
            return new InvalidOptionException(String.Format("'{0}' is not a valid value for option '{1}'", value, option));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")] // TODO: fix
        public virtual void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments) {

            commandLine = "[options] [file|- [arguments]]";

            options = new string[,] {
                { "-c cmd",                      "Program passed in as string (terminates option list)" },
                { "-h",                          "Display usage" },
#if !IRONPYTHON_WINDOW                      
                { "-i",                          "Inspect interactively after running script" },
#endif                                      
                { "-V",                          "Print the version number and exit" },
                { "-D",                          "Enable application debugging" },

                { "-X:AutoIndent",               "" },
                { "-X:AssembliesDir",            "Set the directory for saving generated assemblies" },
#if !SILVERLIGHT                            
                { "-X:ColorfulConsole",          "Enable ColorfulConsole" },
#endif
                { "-X:ExceptionDetail",          "Enable ExceptionDetail mode" },
                { "-X:Interpret",                "Enable interpreted mode" },
                { "-X:Frames",                   "Generate custom frames" },
                { "-X:LightweightScopes",        "Generate optimized scopes that can be garbage collected" },
                { "-X:ILDebug",                  "Output generated IL code to a text file for debugging" },
                { "-X:MaxRecursion",             "Set the maximum recursion level" },
                { "-X:NoTraceback",              "Do not emit traceback code" },
                { "-X:PassExceptions",           "Do not catch exceptions that are unhandled by script code" },
                { "-X:PrivateBinding",           "Enable binding to private members" },
                { "-X:SaveAssemblies",           "Save generated assemblies" },
                { "-X:ShowTrees",                "Print all ASTs to the console" }, 
                { "-X:DumpTrees",                "Dump all ASTs generated to a file"},
                { "-X:ShowClrExceptions",        "Display CLS Exception information" },
                { "-X:ShowRules",                "Show the AST for rules generated" },
                { "-X:ShowScopes",               "Print all scopes and closures to the console" }, 
                { "-X:SlowOps",                  "Enable fast ops" },
#if !SILVERLIGHT
                { "-X:TabCompletion",            "Enable TabCompletion mode" },
#endif
#if DEBUG
                { "-X:TrackPerformance",         "Track performance sensitive areas" },
                { "-X:CachePointersInApartment", "Cache COM pointers per apartment (prototype)" },
#endif
           };

            environmentVariables = new string[0, 0];

            comments = null;
        }
    }
}
