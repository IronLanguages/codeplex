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

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Scripting {

    [Serializable]
    public class EngineOptions {
        private bool _clrDebuggingEnabled;
        private bool _exceptionDetail;
        private bool _showClrExceptions;
        private bool _fastEval;
        
        #region Public accessors

        /// <summary>
        /// Enable CLR debugging of script code executed using ScriptEngine.Execute. This allows debugging the script with 
        /// a CLR debugger. It does not apply to the ScriptEngine.Evaluate set of APIs since it is not possible to step
        /// through the script code using a debugger. 
        /// Note that this is independent of the "traceback" Python module.
        /// Also, the generated code will not be garbage-collected, and so this should only be used for
        /// bounded number of executions.
        /// Using this option requires System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode
        ///
        /// This setting does not apply to modules loaded outside of the ScriptEngine APIs (for eg, using "import" in Python).
        /// Such modules are debuggable by default. If permissions are insufficient for supporting debugging, modules
        /// will automatically generate non-debuggable code. Currently, there is no way to ask modules to generate
        /// non-debuggable code.
        /// </summary>
        public bool ClrDebuggingEnabled {
            get { return _clrDebuggingEnabled; }
            set { _clrDebuggingEnabled = value; }
        }

        /// <summary>
        /// Should we interpret the eval expression instead of compiling it?
        /// This yields a HUGE (>100x) performance boost to simple evals.
        /// Its disabled for compatibility
        /// </summary>
        public bool FastEvaluation {
            get { return _fastEval; }
            set { _fastEval = value; }
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

        #endregion

        public EngineOptions Clone() {
            return (EngineOptions)MemberwiseClone();
        }
    }
}
