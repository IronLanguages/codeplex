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
using System.Runtime.Serialization;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Exceptions {
    [Serializable]
    class ObjectException : Exception, IPythonException {
        private object _instance;
        private PythonType _type;

        public ObjectException(PythonType type, object instance) {
            _instance = instance;
            _type = type;
        }

        public ObjectException(string msg) : base(msg) { }
        public ObjectException(string message, Exception innerException)
            : base(message, innerException) {
        }
#if !SILVERLIGHT // SerializationInfo
        protected ObjectException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif

        public object Instance {
            get {
                return _instance;
            }
        }

        public PythonType Type {
            get {
                return _type;
            }
        }

        #region IPythonException Members

        public object ToPythonException() {
            return this;
        }

        #endregion
    }
}
