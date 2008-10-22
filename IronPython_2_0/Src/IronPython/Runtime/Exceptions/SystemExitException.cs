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
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Exceptions {
    /// <summary>
    /// .NET exception that is thrown to shutdown the interpretter and exit the system.
    /// </summary>
    [Serializable]
    public sealed class SystemExitException : Exception {
        public SystemExitException() : base() { }
        public SystemExitException(string msg)
            : base(msg) {
        }
        public SystemExitException(string message, Exception innerException)
            : base(message, innerException) {
        }
#if !SILVERLIGHT // SerializationInfo
        private SystemExitException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
        /// <summary>
        /// Result of sys.exit(n)
        /// </summary>
        /// <param name="otherCode">
        /// null if the script exited using "sys.exit(int_value)"
        /// null if the script exited using "sys.exit(None)"
        /// x    if the script exited using "sys.exit(x)" where isinstance(x, int) == False
        /// </param>
        /// <returns>
        /// int_value if the script exited using "sys.exit(int_value)"
        /// 1 otherwise
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        [PythonHidden]
        public int GetExitCode(out object otherCode) {
            otherCode = null;
            object pyObj = PythonExceptions.ToPython(this);

            object args;
            PythonTuple t;

            if (!PythonOps.TryGetBoundAttr(pyObj, Symbols.Arguments, out args) ||
                (t = args as PythonTuple) == null ||
                t.__len__() == 0) {
                return 0;
            } else if (Builtin.isinstance(t[0], TypeCache.Int32)) {
                return Converter.ConvertToInt32(t[0]);
            }

            otherCode = t[0];
            return 1;
        }
    }
}
