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

using System; using Microsoft;
using System.Runtime.Serialization;

namespace IronPython.Runtime.Exceptions {
    #region Generated RuntimeWarningException

    // *** BEGIN GENERATED CODE ***
    // generated by function: gen_one_exception_specialized from: generate_exceptions.py


    [Serializable]
    public class RuntimeWarningException : WarningException {
        public RuntimeWarningException() : base() { }
        public RuntimeWarningException(string msg) : base(msg) { }
        public RuntimeWarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
#if !SILVERLIGHT // SerializationInfo
        protected RuntimeWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }


    // *** END GENERATED CODE ***

    #endregion

}
