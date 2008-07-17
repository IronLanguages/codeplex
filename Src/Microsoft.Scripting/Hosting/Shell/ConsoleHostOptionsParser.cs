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
using System.Globalization;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Hosting.Shell {

    public class ConsoleHostOptionsParser {
        private readonly ConsoleHostOptions _options;
        private readonly ScriptDomainOptions _globalOptions;
        private readonly ScriptRuntimeSetup _runtimeConfig;

        public ConsoleHostOptions Options { get { return _options; } }
        public ScriptDomainOptions GlobalOptions { get { return _globalOptions; } }
        public ScriptRuntimeSetup RuntimeConfig { get { return _runtimeConfig; } }

        public ConsoleHostOptionsParser(ConsoleHostOptions options, ScriptDomainOptions globalOptions, ScriptRuntimeSetup runtimeConfig) {
            ContractUtils.RequiresNotNull(options, "options");
            ContractUtils.RequiresNotNull(globalOptions, "globalOptions");
            ContractUtils.RequiresNotNull(runtimeConfig, "runtimeConfig");

            _options = options;
            _globalOptions = globalOptions;
            _runtimeConfig = runtimeConfig;
        }

        // TODO: this should be on ScriptRuntimeSetup and should distinguish id from extension
        private bool IsRegisteredName(string name) {
            foreach (LanguageProviderSetup language in _runtimeConfig.LanguageProviders) {
                if (Array.IndexOf(language.Names, name) != -1) {
                    return true;
                }
            }
            return false;
        }

        /// <exception cref="InvalidOptionException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Parse(string[] args) {
            ContractUtils.RequiresNotNull(args, "args");

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

                        _options.RunAction = ConsoleHostOptions.Action.RunFile;
                        _options.RunFile = value;
                        break;

                    case "lang":
                        OptionValueRequired(name, value);

                        if (!IsRegisteredName(value)) {
                            throw new InvalidOptionException(String.Format("Unknown language id '{0}'.", value));
                        }

                        _options.LanguageId = value;
                        break;

                    case "path":
                    case "paths":
                        OptionValueRequired(name, value);
                        _options.SourceUnitSearchPaths = value.Split(';');
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
                        switch (value) {
                            case "ShowTrees":
                            case "DumpTrees":
                            case "ShowRules":
                            case "ShowScopes":
                                OptionsParser.SetCompilerDebugOption(value);
                                break;
                            default: _options.IgnoredArgs.Add(current); break;
                        }
                        break;

                    case "d":
                        _globalOptions.DebugMode = true;
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
                throw new InvalidOptionException(String.Format(CultureInfo.CurrentCulture, "Argument expected for the {0} option.", optionName));
            }
        }

        [Conditional("SILVERLIGHT")]
        private void OptionNotAvailableOnSilverlight(string optionName) {
            throw new InvalidOptionException(String.Format("Option '{0}' is not available on Silverlight.", optionName));
        }
    }
}
