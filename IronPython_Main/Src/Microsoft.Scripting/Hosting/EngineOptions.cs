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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {

    [Serializable]
    public class EngineOptions {
        private bool _exceptionDetail;
        private bool _showClrExceptions;
        private bool _interpret;
        private bool _pdc;
        private bool _perfStats;

        /// <summary>
        /// Interpret code instead of emitting it.
        /// </summary>
        public bool InterpretedMode {
            get { return _interpret; }
            set { _interpret = value; }
        }

        /// <summary>
        /// Try to selectively emit code to improve performance.
        /// ProfileDrivenCompilation == true implies InterpretedMode == true.
        /// </summary>
        public bool ProfileDrivenCompilation {
            get {
                return _pdc;
            }
            set {
                _pdc = value;
                if (_pdc) {
                    _interpret = true;
                }
            }
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
            set { _perfStats = value; }
        }

        internal protected EngineOptions() {

        }
    }
}
