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
using System.Runtime.Serialization;

namespace IronPython.Runtime.Exceptions { 
    #region Generated PythonException Classes

    // *** BEGIN GENERATED CODE ***


    [PythonType("ImportError")]
    [Serializable]
    public class PythonImportErrorException : Exception {
        public PythonImportErrorException() : base() { }
        public PythonImportErrorException(string msg) : base(msg) { }
        public PythonImportErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonImportErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("RuntimeError")]
    [Serializable]
    public class PythonRuntimeErrorException : Exception {
        public PythonRuntimeErrorException() : base() { }
        public PythonRuntimeErrorException(string msg) : base(msg) { }
        public PythonRuntimeErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonRuntimeErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("UnicodeTranslateError")]
    [Serializable]
    public class PythonUnicodeTranslateErrorException : PythonUnicodeErrorException {
        public PythonUnicodeTranslateErrorException() : base() { }
        public PythonUnicodeTranslateErrorException(string msg) : base(msg) { }
        public PythonUnicodeTranslateErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonUnicodeTranslateErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("PendingDeprecationWarning")]
    [Serializable]
    public class PythonPendingDeprecationWarningException : PythonWarningException {
        public PythonPendingDeprecationWarningException() : base() { }
        public PythonPendingDeprecationWarningException(string msg) : base(msg) { }
        public PythonPendingDeprecationWarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonPendingDeprecationWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("EnvironmentError")]
    [Serializable]
    public class PythonEnvironmentErrorException : Exception {
        public PythonEnvironmentErrorException() : base() { }
        public PythonEnvironmentErrorException(string msg) : base(msg) { }
        public PythonEnvironmentErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonEnvironmentErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("LookupError")]
    [Serializable]
    public class PythonLookupErrorException : Exception {
        public PythonLookupErrorException() : base() { }
        public PythonLookupErrorException(string msg) : base(msg) { }
        public PythonLookupErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonLookupErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("OSError")]
    [Serializable]
    public class PythonOSErrorException : PythonEnvironmentErrorException {
        public PythonOSErrorException() : base() { }
        public PythonOSErrorException(string msg) : base(msg) { }
        public PythonOSErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonOSErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("DeprecationWarning")]
    [Serializable]
    public class PythonDeprecationWarningException : PythonWarningException {
        public PythonDeprecationWarningException() : base() { }
        public PythonDeprecationWarningException(string msg) : base(msg) { }
        public PythonDeprecationWarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonDeprecationWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("UnicodeError")]
    [Serializable]
    public class PythonUnicodeErrorException : Exception {
        public PythonUnicodeErrorException() : base() { }
        public PythonUnicodeErrorException(string msg) : base(msg) { }
        public PythonUnicodeErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonUnicodeErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("FloatingPointError")]
    [Serializable]
    public class PythonFloatingPointErrorException : Exception {
        public PythonFloatingPointErrorException() : base() { }
        public PythonFloatingPointErrorException(string msg) : base(msg) { }
        public PythonFloatingPointErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonFloatingPointErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("ReferenceError")]
    [Serializable]
    public class PythonReferenceErrorException : Exception {
        public PythonReferenceErrorException() : base() { }
        public PythonReferenceErrorException(string msg) : base(msg) { }
        public PythonReferenceErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonReferenceErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("OverflowWarning")]
    [Serializable]
    public class PythonOverflowWarningException : PythonWarningException {
        public PythonOverflowWarningException() : base() { }
        public PythonOverflowWarningException(string msg) : base(msg) { }
        public PythonOverflowWarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonOverflowWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("FutureWarning")]
    [Serializable]
    public class PythonFutureWarningException : PythonWarningException {
        public PythonFutureWarningException() : base() { }
        public PythonFutureWarningException(string msg) : base(msg) { }
        public PythonFutureWarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonFutureWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("AssertionError")]
    [Serializable]
    public class PythonAssertionErrorException : Exception {
        public PythonAssertionErrorException() : base() { }
        public PythonAssertionErrorException(string msg) : base(msg) { }
        public PythonAssertionErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonAssertionErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("RuntimeWarning")]
    [Serializable]
    public class PythonRuntimeWarningException : PythonWarningException {
        public PythonRuntimeWarningException() : base() { }
        public PythonRuntimeWarningException(string msg) : base(msg) { }
        public PythonRuntimeWarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonRuntimeWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("UserWarning")]
    [Serializable]
    public class PythonUserWarningException : PythonWarningException {
        public PythonUserWarningException() : base() { }
        public PythonUserWarningException(string msg) : base(msg) { }
        public PythonUserWarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonUserWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("SyntaxWarning")]
    [Serializable]
    public class PythonSyntaxWarningException : PythonWarningException {
        public PythonSyntaxWarningException() : base() { }
        public PythonSyntaxWarningException(string msg) : base(msg) { }
        public PythonSyntaxWarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonSyntaxWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    [PythonType("Warning")]
    [Serializable]
    public class PythonWarningException : Exception {
        public PythonWarningException() : base() { }
        public PythonWarningException(string msg) : base(msg) { }
        public PythonWarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
    #if !SILVERLIGHT // SerializationInfo
        protected PythonWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    #endif
    }


    // *** END GENERATED CODE ***

    #endregion
}
