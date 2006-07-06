/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace IronPython.Runtime.Exceptions {
    #region Generated PythonException Classes

    // *** BEGIN GENERATED CODE ***


    [PythonType("ImportError")]
    [Serializable]
    public class PythonImportErrorException : Exception {
        public PythonImportErrorException() : base() { }
        public PythonImportErrorException(string msg) : base(msg) { }
        public PythonImportErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("RuntimeError")]
    [Serializable]
    public class PythonRuntimeErrorException : Exception {
        public PythonRuntimeErrorException() : base() { }
        public PythonRuntimeErrorException(string msg) : base(msg) { }
        public PythonRuntimeErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("UnicodeTranslateError")]
    [Serializable]
    public class PythonUnicodeTranslateErrorException : PythonUnicodeErrorException {
        public PythonUnicodeTranslateErrorException() : base() { }
        public PythonUnicodeTranslateErrorException(string msg) : base(msg) { }
        public PythonUnicodeTranslateErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("PendingDeprecationWarning")]
    [Serializable]
    public class PythonPendingDeprecationWarningException : PythonWarningException {
        public PythonPendingDeprecationWarningException() : base() { }
        public PythonPendingDeprecationWarningException(string msg) : base(msg) { }
        public PythonPendingDeprecationWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("EnvironmentError")]
    [Serializable]
    public class PythonEnvironmentErrorException : Exception {
        public PythonEnvironmentErrorException() : base() { }
        public PythonEnvironmentErrorException(string msg) : base(msg) { }
        public PythonEnvironmentErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("LookupError")]
    [Serializable]
    public class PythonLookupErrorException : Exception {
        public PythonLookupErrorException() : base() { }
        public PythonLookupErrorException(string msg) : base(msg) { }
        public PythonLookupErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("OSError")]
    [Serializable]
    public class PythonOSErrorException : PythonEnvironmentErrorException {
        public PythonOSErrorException() : base() { }
        public PythonOSErrorException(string msg) : base(msg) { }
        public PythonOSErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("DeprecationWarning")]
    [Serializable]
    public class PythonDeprecationWarningException : PythonWarningException {
        public PythonDeprecationWarningException() : base() { }
        public PythonDeprecationWarningException(string msg) : base(msg) { }
        public PythonDeprecationWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("UnicodeError")]
    [Serializable]
    public class PythonUnicodeErrorException : Exception {
        public PythonUnicodeErrorException() : base() { }
        public PythonUnicodeErrorException(string msg) : base(msg) { }
        public PythonUnicodeErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("FloatingPointError")]
    [Serializable]
    public class PythonFloatingPointErrorException : Exception {
        public PythonFloatingPointErrorException() : base() { }
        public PythonFloatingPointErrorException(string msg) : base(msg) { }
        public PythonFloatingPointErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("ReferenceError")]
    [Serializable]
    public class PythonReferenceErrorException : Exception {
        public PythonReferenceErrorException() : base() { }
        public PythonReferenceErrorException(string msg) : base(msg) { }
        public PythonReferenceErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("NameError")]
    [Serializable]
    public class PythonNameErrorException : Exception {
        public PythonNameErrorException() : base() { }
        public PythonNameErrorException(string msg) : base(msg) { }
        public PythonNameErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("OverflowWarning")]
    [Serializable]
    public class PythonOverflowWarningException : PythonWarningException {
        public PythonOverflowWarningException() : base() { }
        public PythonOverflowWarningException(string msg) : base(msg) { }
        public PythonOverflowWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("FutureWarning")]
    [Serializable]
    public class PythonFutureWarningException : PythonWarningException {
        public PythonFutureWarningException() : base() { }
        public PythonFutureWarningException(string msg) : base(msg) { }
        public PythonFutureWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("AssertionError")]
    [Serializable]
    public class PythonAssertionErrorException : Exception {
        public PythonAssertionErrorException() : base() { }
        public PythonAssertionErrorException(string msg) : base(msg) { }
        public PythonAssertionErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("RuntimeWarning")]
    [Serializable]
    public class PythonRuntimeWarningException : PythonWarningException {
        public PythonRuntimeWarningException() : base() { }
        public PythonRuntimeWarningException(string msg) : base(msg) { }
        public PythonRuntimeWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("KeyboardInterrupt")]
    [Serializable]
    public class PythonKeyboardInterruptException : Exception {
        public PythonKeyboardInterruptException() : base() { }
        public PythonKeyboardInterruptException(string msg) : base(msg) { }
        public PythonKeyboardInterruptException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("UserWarning")]
    [Serializable]
    public class PythonUserWarningException : PythonWarningException {
        public PythonUserWarningException() : base() { }
        public PythonUserWarningException(string msg) : base(msg) { }
        public PythonUserWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("SyntaxWarning")]
    [Serializable]
    public class PythonSyntaxWarningException : PythonWarningException {
        public PythonSyntaxWarningException() : base() { }
        public PythonSyntaxWarningException(string msg) : base(msg) { }
        public PythonSyntaxWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("UnboundLocalError")]
    [Serializable]
    public class PythonUnboundLocalErrorException : PythonNameErrorException {
        public PythonUnboundLocalErrorException() : base() { }
        public PythonUnboundLocalErrorException(string msg) : base(msg) { }
        public PythonUnboundLocalErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("Warning")]
    [Serializable]
    public class PythonWarningException : Exception {
        public PythonWarningException() : base() { }
        public PythonWarningException(string msg) : base(msg) { }
        public PythonWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    // *** END GENERATED CODE ***

    #endregion
}
