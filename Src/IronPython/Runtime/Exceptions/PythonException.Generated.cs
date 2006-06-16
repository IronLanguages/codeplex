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
    public class PythonImportError:Exception {
        public PythonImportError():base(){ }
        public PythonImportError(string msg):base(msg) { }
        public PythonImportError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("RuntimeError")]
    [Serializable]
    public class PythonRuntimeError:Exception {
        public PythonRuntimeError():base(){ }
        public PythonRuntimeError(string msg):base(msg) { }
        public PythonRuntimeError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("UnicodeTranslateError")]
    [Serializable]
    public class PythonUnicodeTranslateError:PythonUnicodeError {
        public PythonUnicodeTranslateError():base(){ }
        public PythonUnicodeTranslateError(string msg):base(msg) { }
        public PythonUnicodeTranslateError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("PendingDeprecationWarning")]
    [Serializable]
    public class PythonPendingDeprecationWarning:PythonWarning {
        public PythonPendingDeprecationWarning():base(){ }
        public PythonPendingDeprecationWarning(string msg):base(msg) { }
        public PythonPendingDeprecationWarning(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("EnvironmentError")]
    [Serializable]
    public class PythonEnvironmentError:Exception {
        public PythonEnvironmentError():base(){ }
        public PythonEnvironmentError(string msg):base(msg) { }
        public PythonEnvironmentError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("LookupError")]
    [Serializable]
    public class PythonLookupError:Exception {
        public PythonLookupError():base(){ }
        public PythonLookupError(string msg):base(msg) { }
        public PythonLookupError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("OSError")]
    [Serializable]
    public class PythonOSError:PythonEnvironmentError {
        public PythonOSError():base(){ }
        public PythonOSError(string msg):base(msg) { }
        public PythonOSError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("DeprecationWarning")]
    [Serializable]
    public class PythonDeprecationWarning:PythonWarning {
        public PythonDeprecationWarning():base(){ }
        public PythonDeprecationWarning(string msg):base(msg) { }
        public PythonDeprecationWarning(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("UnicodeError")]
    [Serializable]
    public class PythonUnicodeError:Exception {
        public PythonUnicodeError():base(){ }
        public PythonUnicodeError(string msg):base(msg) { }
        public PythonUnicodeError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("FloatingPointError")]
    [Serializable]
    public class PythonFloatingPointError:Exception {
        public PythonFloatingPointError():base(){ }
        public PythonFloatingPointError(string msg):base(msg) { }
        public PythonFloatingPointError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("ReferenceError")]
    [Serializable]
    public class PythonReferenceError:Exception {
        public PythonReferenceError():base(){ }
        public PythonReferenceError(string msg):base(msg) { }
        public PythonReferenceError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("NameError")]
    [Serializable]
    public class PythonNameError:Exception {
        public PythonNameError():base(){ }
        public PythonNameError(string msg):base(msg) { }
        public PythonNameError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("OverflowWarning")]
    [Serializable]
    public class PythonOverflowWarning:PythonWarning {
        public PythonOverflowWarning():base(){ }
        public PythonOverflowWarning(string msg):base(msg) { }
        public PythonOverflowWarning(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("FutureWarning")]
    [Serializable]
    public class PythonFutureWarning:PythonWarning {
        public PythonFutureWarning():base(){ }
        public PythonFutureWarning(string msg):base(msg) { }
        public PythonFutureWarning(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("AssertionError")]
    [Serializable]
    public class PythonAssertionError:Exception {
        public PythonAssertionError():base(){ }
        public PythonAssertionError(string msg):base(msg) { }
        public PythonAssertionError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("RuntimeWarning")]
    [Serializable]
    public class PythonRuntimeWarning:PythonWarning {
        public PythonRuntimeWarning():base(){ }
        public PythonRuntimeWarning(string msg):base(msg) { }
        public PythonRuntimeWarning(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("KeyboardInterrupt")]
    [Serializable]
    public class PythonKeyboardInterrupt:Exception {
        public PythonKeyboardInterrupt():base(){ }
        public PythonKeyboardInterrupt(string msg):base(msg) { }
        public PythonKeyboardInterrupt(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("UserWarning")]
    [Serializable]
    public class PythonUserWarning:PythonWarning {
        public PythonUserWarning():base(){ }
        public PythonUserWarning(string msg):base(msg) { }
        public PythonUserWarning(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("SyntaxWarning")]
    [Serializable]
    public class PythonSyntaxWarning:PythonWarning {
        public PythonSyntaxWarning():base(){ }
        public PythonSyntaxWarning(string msg):base(msg) { }
        public PythonSyntaxWarning(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("UnboundLocalError")]
    [Serializable]
    public class PythonUnboundLocalError:PythonNameError {
        public PythonUnboundLocalError():base(){ }
        public PythonUnboundLocalError(string msg):base(msg) { }
        public PythonUnboundLocalError(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    [PythonType("Warning")]
    [Serializable]
    public class PythonWarning:Exception {
        public PythonWarning():base(){ }
        public PythonWarning(string msg):base(msg) { }
        public PythonWarning(SerializationInfo info, StreamingContext context) : base(info, context) {  }
    }


    // *** END GENERATED CODE ***

    #endregion
}
