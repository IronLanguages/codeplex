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
using Microsoft.Scripting.Utils;
using System.Globalization;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Hosting {

    public class ConsoleHostOptions {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public enum Action {
            None,
            RunConsole,
            RunFiles,
            DisplayHelp
        }

        private readonly List<string/*!*/> _ignoredArgs = new List<string/*!*/>();
        private readonly List<string/*!*/> _files = new List<string/*!*/>();
        private string[] _sourceUnitSearchPaths = ArrayUtils.EmptyStrings;
        private Action _action;
        private bool _displayLogo = true;
        private bool _isMTA;
        private readonly List<string/*!*/> _environmentVars = new List<string/*!*/>(); 
        private ScriptEngine _scriptEngine;

        public List<string/*!*/> IgnoredArgs { get { return _ignoredArgs; } }
        public List<string/*!*/> Files { get { return _files; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // TODO: fix
        public string[] SourceUnitSearchPaths { get { return _sourceUnitSearchPaths; } set { _sourceUnitSearchPaths = value; } }
        public Action RunAction { get { return _action; } set { _action = value; } }
        public ScriptEngine ScriptEngine { get { return _scriptEngine; } set { _scriptEngine = value; } }
        public bool DisplayLogo { get { return _displayLogo; } set { _displayLogo = value; } }
        public bool IsMTA { get { return _isMTA; } set { _isMTA = value; } }
        public List<string/*!*/> EnvironmentVars { get { return _environmentVars; } }
        
        public ConsoleHostOptions() {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")] // TODO: fix
        public string[,] GetHelp() {
            return new string[,] {
                { "/help",                     "Displays this help." },
                { "/lang:<extension>",         "Specify language by the associated extension (py, js, vb, rb). Determined by an extension of the first file. Defaults to IronPython." },
                { "/run:<files>",              "Executes specified files (semicolon separated list) one by one via the language engine." },
                { "/execute:<file>",           "Execute a specified .exe file using its static entry point." },
                { "/paths:<file-path-list>",   "Semicolon separated list of import paths (/run only)." },
                { "/nologo",                   "Do not display host logo." },
                { "/mta",                      "Starts command line thread in multi-threaded apartment. Not available on Silverlight." },
                { "/setenv:<var1=value1;...>", "Sets specified environment variables for the console process. Not available on Silverlight." },
#if DEBUG
                { "/X:ShowASTs",               "Print generated Abstract Syntax Trees to the console" },
                { "/X:DumpASTs",               "Write generated ASTs as files in the current directory" },
                { "/X:ShowRules",              "Print generated action dispatch rules to the console" },
#endif
            };
        }
    }

    public class ConsoleHostOptionsParser {
        public ConsoleHostOptions Options { get { return _options; } set { _options = value; } }
        private ConsoleHostOptions/*!*/ _options;
        private ScriptRuntime/*!*/ _env;

        public ConsoleHostOptionsParser(ConsoleHostOptions/*!*/ options, ScriptRuntime/*!*/ environment) {
            Contract.RequiresNotNull(options, "options");
            Contract.RequiresNotNull(environment, "environment");

            _options = options;
            _env = environment;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Parse(string[] args) {
            Contract.RequiresNotNull(args, "args");

            Debug.Assert(_options != null);

            int i = 0;
            while (i < args.Length) {
                string name, value;
                string current = args[i++];
                ParseOption(current, out name, out value);

                switch (name) {
                    case "console":
                        _options.RunAction = ConsoleHostOptions.Action.RunConsole;
                        break;

                    case "run":
                        OptionValueRequired(name, value);
                        _options.RunAction = ConsoleHostOptions.Action.RunFiles;
                        _options.Files.AddRange(value.Split(';'));
                        break;

                    case "lang":
                        OptionValueRequired(name, value);
                        _options.ScriptEngine = GetScriptEngine(value);
                        break;

                    case "path":
                    case "paths":
                        OptionValueRequired(name, value);
                        _options.SourceUnitSearchPaths = value.Split(';');
                        break;

                    case "nologo":
                        _options.DisplayLogo = false;
                        break;

                    case "mta":
                        OptionNotAvailableOnSilverlight(name);
                        _options.IsMTA = true;
                        break;

                    case "setenv":
                        OptionNotAvailableOnSilverlight(name);
                        _options.EnvironmentVars.AddRange(value.Split(';'));
                        break;

                    case "x":
                        switch(value) {
                            case "ShowASTs": ScriptDomainManager.Options.ShowASTs = true; break;
                            case "DumpASTs": ScriptDomainManager.Options.DumpASTs = true; break;
                            case "ShowRules": ScriptDomainManager.Options.ShowRules = true; break;
                            default: _options.IgnoredArgs.Add(current); break;
                        }
                        break;

                    case "help":
                    case "?":
                        _options.RunAction = ConsoleHostOptions.Action.DisplayHelp;
                        return;

                    // first unknown/non-option:
                    case null:
                    default:
                        _options.IgnoredArgs.Add(current);
                        goto case "";

                    // host/passthru argument separator
                    case "/":
                    case "":
                        // ignore all arguments starting with the next one (arguments are not parsed):
                        while (i < args.Length) {
                            _options.IgnoredArgs.Add(args[i++]);
                        }
                        break;
                }
            }

            switch (_options.RunAction) {
                case ConsoleHostOptions.Action.RunFiles:
                    if (_options.Files.Count == 0)
                        throw new InvalidOptionException("No file to run.");

                    if (_options.ScriptEngine == null)
                        _options.ScriptEngine = GetScriptEngine(StringUtils.GetSuffix(_options.Files[0], '.', false));

                    break;

                case ConsoleHostOptions.Action.RunConsole:
                    if (_options.ScriptEngine == null)
                        _options.ScriptEngine = GetScriptEngine("py");
                    break;

                case ConsoleHostOptions.Action.None:
                    _options.RunAction = ConsoleHostOptions.Action.RunConsole;
                    goto case ConsoleHostOptions.Action.RunConsole;
            }
        }

        /// <summary>
        /// name == null means that the argument doesn't specify an option; the value contains the entire argument
        /// name == "" means that the option name is empty (argument separator); the value is null then
        /// </summary>
        private void ParseOption(string arg, out string name, out string value) {
            Debug.Assert(arg != null);

            int colon = arg.IndexOf(':');

            if (colon >= 0) {
                name = arg.Substring(0, colon);
                value = arg.Substring(colon + 1);
            } else {
                name = arg;
                value = null;
            }

            if (name.StartsWith("--")) name = name.Substring("--".Length);
            else if (name.StartsWith("-") && name.Length > 1) name = name.Substring("-".Length);
            else if (name.StartsWith("/") && name.Length > 1) name = name.Substring("/".Length);
            else {
                value = name;
                name = null;
            }

            if (name != null) {
                name = name.ToLower(CultureInfo.InvariantCulture);
            }
        }

        protected void OptionValueRequired(string optionName, string value) {
            if (value == null) {
                throw new InvalidOptionException(String.Format(CultureInfo.CurrentCulture, Resources.MissingOptionValue, optionName));
            }
        }

        [Conditional("SILVERLIGHT")]
        private void OptionNotAvailableOnSilverlight(string optionName) {
            throw new InvalidOptionException(String.Format("Option '{0}' is not available on Silverlight.", optionName));
        }

        private ScriptEngine GetScriptEngine(string languageId) {
            Debug.Assert(languageId != null);

            try {
                return _env.GetEngine(languageId);
            } catch (Exception e) {
                throw new InvalidOperationException(String.Format("Cannot create language provider corresponding to the language id '{0}'.", languageId), e);
            }
        }
    }
}
