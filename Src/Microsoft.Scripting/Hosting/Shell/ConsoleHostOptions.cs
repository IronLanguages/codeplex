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

using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting.Shell {

    public class ConsoleHostOptions {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public enum Action {
            None,
            RunConsole,
            RunFile,
            DisplayHelp
        }

        private readonly List<string> _ignoredArgs = new List<string>();
        private string _runFile;
        private string[] _sourceUnitSearchPaths = new string[] { "." };
        private Action _action;
        private readonly List<string> _environmentVars = new List<string>();
        private AssemblyQualifiedTypeName? _languageProvider;

        public List<string> IgnoredArgs { get { return _ignoredArgs; } }
        public string RunFile { get { return _runFile; } set { _runFile = value; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // TODO: fix
        public string[] SourceUnitSearchPaths { get { return _sourceUnitSearchPaths; } set { _sourceUnitSearchPaths = value; } }
        public Action RunAction { get { return _action; } set { _action = value; } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "MTA")]
        public List<string> EnvironmentVars { get { return _environmentVars; } }
        public AssemblyQualifiedTypeName? LanguageProvider { get { return _languageProvider; } set { _languageProvider = value; } } 
        
        public ConsoleHostOptions() {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")] // TODO: fix
        public string[,] GetHelp() {
            return new string[,] {
                { "/help",                     "Displays this help." },
                { "/lang:<extension>",         "Specify language by the associated extension (py, js, vb, rb). Determined by an extension of the first file. Defaults to IronPython." },
                { "/paths:<file-path-list>",   "Semicolon separated list of import paths (/run only)." },
                { "/mta",                      "Starts command line thread in multi-threaded apartment. Not available on Silverlight." },
                { "/setenv:<var1=value1;...>", "Sets specified environment variables for the console process. Not available on Silverlight." },
            };
        }
    }
}
