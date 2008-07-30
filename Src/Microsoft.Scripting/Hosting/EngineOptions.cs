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

namespace Microsoft.Scripting {

    [Serializable]
    public class EngineOptions {
        private bool _exceptionDetail;
        private bool _showClrExceptions;
        private bool _interpretedMode;
        private readonly bool _perfStats;

        /// <summary>
        /// Interpret code instead of emitting it.
        /// </summary>
        public bool InterpretedMode {
            get { return _interpretedMode; }
            set { _interpretedMode = value; }
        }

        /// <summary>
        ///  Display exception detail (callstack) when exception gets caught
        /// </summary>
        public bool ExceptionDetail {
            get { return _exceptionDetail; }
            set { _exceptionDetail = value; }
        }

        public bool ShowClrExceptions {
            get { return _showClrExceptions; }
            set { _showClrExceptions = value; }
        }

        /// <summary>
        /// Whther to gather performance statistics.
        /// </summary>
        public bool PerfStats {
            get { return _perfStats; }
        }

        public EngineOptions() 
            : this(null) {
        }

        public EngineOptions(IDictionary<string, object> options) {
            _interpretedMode = GetOption(options, "InterpretedMode", false);
            _exceptionDetail = GetOption(options, "ExceptionDetail", false);
            _showClrExceptions = GetOption(options, "ShowClrExceptions", false);
            _perfStats = GetOption(options, "PerfStats", false);
        }

        protected static T GetOption<T>(IDictionary<string, object> options, string name, T defaultValue) {
            object value;
            if (options != null && options.TryGetValue(name, out value)) {
                if (value is T) {
                    return (T)value;
                }
                throw new ArgumentException(String.Format("Invalid value for option {0}", name));
            }
            return defaultValue;
        }
    }
}
