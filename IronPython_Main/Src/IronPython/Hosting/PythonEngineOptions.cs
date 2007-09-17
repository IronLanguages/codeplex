/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using Microsoft.Scripting;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;

namespace IronPython {

    [CLSCompliant(true)]
    public enum PythonDivisionOptions {
        Old,
        New,
        Warn,
        WarnAll
    }

    [CLSCompliant(true)]
    [Serializable]
    public sealed class PythonEngineOptions : EngineOptions {

        private string[] _arguments = ArrayUtils.EmptyStrings;
        private bool _skipFistSourceLine;
        private List<string> _warningFilters;
        private int _maximumRecursion = Int32.MaxValue;
        private Severity _indentationInconsistencySeverity;
        private PythonDivisionOptions _division;
        private bool _preferComDispatchOverTypeInfo;

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

        /// <summary>
        /// The division options (old, new, warn, warnall)
        /// </summary>
        public PythonDivisionOptions DivisionOptions {
            get { return _division; }
            set { _division = value; }
        }

        /// <summary>
        /// Use pure IDispatch-based invocation when calling methods/proeprties 
        /// on System.__ComObject
        /// </summary>
        public bool PreferComDispatchOverTypeInfo {
            get { return _preferComDispatchOverTypeInfo; }
            set { _preferComDispatchOverTypeInfo = value; }
        }

        public PythonEngineOptions Clone() {
            PythonEngineOptions result = (PythonEngineOptions)MemberwiseClone();
            result._arguments = (string[])_arguments.Clone();
            result._warningFilters = new List<string>(_warningFilters);
            return result;
        }
    }
}
