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
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronPython {

    [CLSCompliant(true)]
    public enum PythonDivisionOptions {
        Old,
        New,
        Warn,
        WarnAll
    }

    // TODO: rename to PythonOptions
    [CLSCompliant(true)]
    [Serializable]
    public sealed class PythonEngineOptions : EngineOptions {

        private readonly string[]/*!*/ _arguments;
        private readonly string[]/*!*/ _warningFilters;
        private readonly int _recursionLimit;
        private readonly Severity _indentationInconsistencySeverity;
        private readonly PythonDivisionOptions _division;
        private readonly bool _stripDocStrings;
        private readonly bool _optimize;

        public string[]/*!*/ Arguments {
            get { return _arguments; }
        }

        /// <summary>
        ///  Should we strip out all doc strings (the -O command line option).
        /// </summary>
        public bool Optimize {
            get { return _optimize; }
        }
        
        /// <summary>
        ///  Should we strip out all doc strings (the -OO command line option).
        /// </summary>
        public bool StripDocStrings {
            get { return _stripDocStrings; }
        }

        /// <summary>
        ///  List of -W (warning filter) options collected from the command line.
        /// </summary>
        public string[]/*!*/ WarningFilters {
            get { return _warningFilters; }
        }

        public int RecursionLimit {
            get { return _recursionLimit; }
        }

        /// <summary> 
        /// Severity of a findong that indentation is formatted inconsistently.
        /// </summary>
        public Severity IndentationInconsistencySeverity {
            get { return _indentationInconsistencySeverity; }
        }

        /// <summary>
        /// The division options (old, new, warn, warnall)
        /// </summary>
        public PythonDivisionOptions DivisionOptions {
            get { return _division; }
        }

        public PythonEngineOptions() 
            : this(null) {
        }
    
        public PythonEngineOptions(IDictionary<string, object> options) 
            : base(options) {

            _arguments = ArrayUtils.Copy(GetOption(options, "Arguments", ArrayUtils.EmptyStrings));
            _warningFilters = ArrayUtils.Copy(GetOption(options, "WarningFilters", ArrayUtils.EmptyStrings));

            _optimize = GetOption(options, "Optimize", false);
            _stripDocStrings = GetOption(options, "StripDocStrings", false);
            _division = GetOption(options, "DivisionOptions", PythonDivisionOptions.Old);
            _recursionLimit = GetOption(options, "RecursionLimit", Int32.MaxValue);
            _indentationInconsistencySeverity = GetOption(options, "IndentationInconsistencySeverity", Severity.Ignore);
        }
    }
}
