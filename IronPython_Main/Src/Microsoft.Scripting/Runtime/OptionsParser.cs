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
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Generation;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {

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
        private readonly LanguageContext/*!*/ _context;

        protected OptionsParser(LanguageContext/*!*/ context) {
            Contract.RequiresNotNull(context, "context");

            _context = context;
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
            Contract.RequiresNotNull(args, "args");

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
            Contract.RequiresNotNull(arg, "arg");

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

                    if (!_context.DomainManager.PAL.DirectoryExists(dir)) {
                        throw new InvalidOptionException(String.Format("Directory '{0}' doesn't exist.", dir));
                    }

                    Snippets.Shared.SnippetsDirectory = dir;
                    break;

                // TODO: remove (needed by SNAP right now)
                case "-X:StaticMethods": break;
                case "-X:NoOptimize": break;
                
                case "-X:Interpret": EngineOptions.InterpretedMode = true; break;
                case "-X:Frames": GlobalOptions.Frames = true; break;

                // TODO: remove (needed by SNAP right now)
                case "-X:GenerateAsSnippets":
                case "-X:TupleBasedOptimizedScopes": GlobalOptions.TupleBasedOptimizedScopes = true; break;

                case "-X:ILDebug": Snippets.Shared.ILDebug = true; break;

                case "-X:PassExceptions": ConsoleOptions.HandleExceptions = false; break;
                // TODO: #if !IRONPYTHON_WINDOW
                case "-X:ColorfulConsole": ConsoleOptions.ColorfulConsole = true; break;
                case "-X:ExceptionDetail": EngineOptions.ExceptionDetail = true; break;
                case "-X:TabCompletion": ConsoleOptions.TabCompletion = true; break;
                case "-X:AutoIndent": ConsoleOptions.AutoIndent = true; break;
                //#endif
                case "-X:NoTraceback": GlobalOptions.DynamicStackTraceSupport = false; break;

                case "-X:ShowRules": GlobalOptions.ShowRules = true; break;
                case "-X:DumpASTs": GlobalOptions.DumpASTs = true; break;
                case "-X:ShowASTs": GlobalOptions.ShowASTs = true; break;

                case "-X:PerfStats": EngineOptions.PerfStats = true; break;
                case "-X:PrivateBinding": GlobalOptions.PrivateBinding = true; break;
                case "-X:SaveAssemblies": Snippets.Shared.SaveSnippets = true; break;
                case "-X:ShowClrExceptions": EngineOptions.ShowClrExceptions = true; break;
                case "-X:TrackPerformance": // accepted but ignored on retail builds
#if DEBUG
                    GlobalOptions.TrackPerformance = true;
#endif
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
                throw new InvalidOptionException(String.Format(CultureInfo.CurrentCulture, Resources.MissingOptionValue, _current > 0 ? _args[_current - 1] : ""));
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
            return new InvalidOptionException(String.Format(Resources.InvalidOptionValue, value, option));
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
                { "-X:DumpASTs",                 "Dump all ASTs generated to a file"},
                { "-X:ExceptionDetail",          "Enable ExceptionDetail mode" },
                { "-X:Interpret",                "Enable interpreted mode" },
                { "-X:Frames",                   "Generate custom frames" },
                { "-X:TupleBasedOptimizedScopes","Use tuples for optimized scopes" },
                { "-X:ILDebug",                   "Output generated IL code to a text file for debugging" },
                { "-X:MaxRecursion",             "Set the maximum recursion level" },
                { "-X:NoTraceback",              "Do not emit traceback code" },
                { "-X:PassExceptions",           "Do not catch exceptions that are unhandled by script code" },
                { "-X:PrivateBinding",           "Enable binding to private members" },
                { "-X:SaveAssemblies",           "Save generated assemblies" },
                { "-X:ShowASTs",                 "Print all ASTs to the console" }, 
                { "-X:ShowClrExceptions",        "Display CLS Exception information" },
                { "-X:ShowRules",                "Show the AST for rules generated" },
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

        protected LanguageContext/*!*/ LanguageContext {
            get {
                return _context;
            }
        }
    }
}
