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
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;

using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Operations {
    public static partial class PythonOps {
        #region Generated Exception Factories

        // *** BEGIN GENERATED CODE ***


        public static Exception ImportError(string format, params object[] args) {
            return new PythonImportErrorException(string.Format(format, args));
        }

        public static Exception RuntimeError(string format, params object[] args) {
            return new PythonRuntimeErrorException(string.Format(format, args));
        }

        public static Exception UnicodeTranslateError(string format, params object[] args) {
            return new PythonUnicodeTranslateErrorException(string.Format(format, args));
        }

        public static Exception PendingDeprecationWarning(string format, params object[] args) {
            return new PythonPendingDeprecationWarningException(string.Format(format, args));
        }

        public static Exception EnvironmentError(string format, params object[] args) {
            return new PythonEnvironmentErrorException(string.Format(format, args));
        }

        public static Exception LookupError(string format, params object[] args) {
            return new PythonLookupErrorException(string.Format(format, args));
        }

        public static Exception OSError(string format, params object[] args) {
            return new PythonOSErrorException(string.Format(format, args));
        }

        public static Exception DeprecationWarning(string format, params object[] args) {
            return new PythonDeprecationWarningException(string.Format(format, args));
        }

        public static Exception UnicodeError(string format, params object[] args) {
            return new PythonUnicodeErrorException(string.Format(format, args));
        }

        public static Exception FloatingPointError(string format, params object[] args) {
            return new PythonFloatingPointErrorException(string.Format(format, args));
        }

        public static Exception ReferenceError(string format, params object[] args) {
            return new PythonReferenceErrorException(string.Format(format, args));
        }

        public static Exception OverflowWarning(string format, params object[] args) {
            return new PythonOverflowWarningException(string.Format(format, args));
        }

        public static Exception FutureWarning(string format, params object[] args) {
            return new PythonFutureWarningException(string.Format(format, args));
        }

        public static Exception AssertionError(string format, params object[] args) {
            return new PythonAssertionErrorException(string.Format(format, args));
        }

        public static Exception RuntimeWarning(string format, params object[] args) {
            return new PythonRuntimeWarningException(string.Format(format, args));
        }

        public static Exception ImportWarning(string format, params object[] args) {
            return new PythonImportWarningException(string.Format(format, args));
        }

        public static Exception UserWarning(string format, params object[] args) {
            return new PythonUserWarningException(string.Format(format, args));
        }

        public static Exception SyntaxWarning(string format, params object[] args) {
            return new PythonSyntaxWarningException(string.Format(format, args));
        }

        public static Exception Warning(string format, params object[] args) {
            return new PythonWarningException(string.Format(format, args));
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
