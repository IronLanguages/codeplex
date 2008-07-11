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
using Microsoft.Scripting.Hosting.Shell; 

namespace IronPython.Hosting {
    [CLSCompliant(true)]
    public /* TODO: sealed */ class PythonConsoleOptions : ConsoleOptions {

        private bool _ignoreEnvironmentVariables;
        private bool _skipImportSite;
        private bool _skipFistSourceLine;
        private string _runAsModule;

        public bool IgnoreEnvironmentVariables {
            get { return _ignoreEnvironmentVariables; }
            set { _ignoreEnvironmentVariables = value; }
        }

        public bool SkipImportSite {
            get { return _skipImportSite; }
            set { _skipImportSite = value; }
        }

        public string ModuleToRun {
            get { return _runAsModule; }
            set { _runAsModule = value; }
        }

        /// <summary>
        /// Skip the first line of the code to execute. This is useful for executing Unix scripts which
        /// have the command to execute specified in the first line.
        /// This only apply to the script code executed by the ScriptEngine APIs, but not for other script code 
        /// that happens to get called as a result of the execution.
        /// </summary>
        public bool SkipFirstSourceLine {
            get { return _skipFistSourceLine; }
            set { _skipFistSourceLine = value; }
        }


    }
}

