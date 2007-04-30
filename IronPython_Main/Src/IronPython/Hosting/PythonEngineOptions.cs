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

using Microsoft.Scripting.Internal;
using System;
using Microsoft.Scripting;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;

namespace IronPython {

    [CLSCompliant(true)]
    [Serializable]
    public /* TODO: sealed*/ class PythonEngineOptions : EngineOptions {

        private string[] _arguments = Utils.Array.EmptyStrings;
        private bool _skipFistSourceLine;
        private List<string> _warningFilters;
        private int _maximumRecursion = Int32.MaxValue;
        private Severity _indentationInconsistencySeverity;
        
        /// <summary>
        /// Skip the first line of the code to execute. This is useful for executing Unix scripts which
        /// have the command to execute specified in the first line.
        /// This only apply to the script code executed by the PythonEngine APIs, but not for other script code 
        /// that happens to get called as a result of the execution.
        /// </summary>
        public bool SkipFirstSourceLine {
            get { return _skipFistSourceLine; }
            set { _skipFistSourceLine = value; }
        }

        public string[] Arguments {
            get { return _arguments; }
            set { _arguments = value; }
        }

        /// <summary>
        ///  List of -W (warning filter) options collected from the command line.
        /// </summary>
        public IList<string> WarningFilters {
            get { return _warningFilters; }
            set { _warningFilters = new List<string>(value); }
        }

        public int MaximumRecursion {
            get { return _maximumRecursion; }
            set { _maximumRecursion = value; }
        }

        /// <summary> 
        /// Severity of a findong that indentation is formatted inconsistently.
        /// </summary>
        public Severity IndentationInconsistencySeverity {
            get { return _indentationInconsistencySeverity; }
            set { _indentationInconsistencySeverity = value; }
        }

        public new PythonEngineOptions Clone() {
            PythonEngineOptions result = (PythonEngineOptions)MemberwiseClone();
            result._arguments = (string[])_arguments.Clone();
            result._warningFilters = new List<string>(_warningFilters);
            return result;
        }
    }
}