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

namespace IronPython.Runtime.Exceptions {
    /// <summary>
    /// GeneratorExitException is a standard exception raised by Generator.Close() to allow a caller
    /// to close out a generator.
    /// </summary>
    /// <remarks>GeneratorExit is introduced in Pep342 for Python2.5. </remarks>
    [Serializable]
    public sealed class GeneratorExitException : Exception {
        public GeneratorExitException() {
        }

        public GeneratorExitException(string message)
            : base(message) {
        }
        public GeneratorExitException(string message, Exception innerException)
            : base(message, innerException) {
        }

#if !SILVERLIGHT // SerializationInfo
        private GeneratorExitException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
