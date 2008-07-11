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
using System.Scripting;
using System.Scripting.Utils;
using Microsoft.Scripting;

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
        private List<string> _warningFilters;
        private int _maximumRecursion = Int32.MaxValue;
        private Severity _indentationInconsistencySeverity;
        private PythonDivisionOptions _division;
        private bool _stripDocStrings;
        private bool _optimize;

        public string[] Arguments {
            get { return _arguments; }
            set { _arguments = value; }
        }

        /// <summary>
        ///  Should we strip out all doc strings (the -O command line option).
        /// </summary>
        public bool Optimize {
            get { return _optimize; }
            set { _optimize = value; }
        }
        
        /// <summary>
        ///  Should we strip out all doc strings (the -OO command line option).
        /// </summary>
        public bool StripDocStrings {
            get { return _stripDocStrings; }
            set { _stripDocStrings = value; }
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

        public PythonEngineOptions Clone() {
            PythonEngineOptions result = (PythonEngineOptions)MemberwiseClone();
            result._arguments = (string[])_arguments.Clone();
            result._warningFilters = new List<string>(_warningFilters);
            return result;
        }
    }
}
