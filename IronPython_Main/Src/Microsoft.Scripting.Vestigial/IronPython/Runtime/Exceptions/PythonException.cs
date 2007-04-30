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
using System.Runtime.Serialization;
using System.Text;
using IronPython.Hosting;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;

namespace IronPython.Runtime.Exceptions {
    [Serializable]
    public class StopIterationException : InvalidOperationException {
        public StopIterationException() {
        }

        public StopIterationException(string message)
            : base(message) {
        }
        public StopIterationException(string message, Exception innerException)
            : base(message, innerException) {
        }

#if !SILVERLIGHT // SerializationInfo
        protected StopIterationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }


    [PythonType("SystemExit")]
    [Serializable]
    public class PythonSystemExitException : Exception {
        public PythonSystemExitException() : base() { }
        public PythonSystemExitException(string msg)
            : base(msg) {
        }
        public PythonSystemExitException(string message, Exception innerException)
            : base(message, innerException) {
        }
#if !SILVERLIGHT // SerializationInfo
        protected PythonSystemExitException(SerializationInfo info, StreamingContext context) : base(info, context) { }
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
        public int GetExitCode(out object otherCode) {
            otherCode = null;
            object pyObj = ExceptionConverter.ToPython(this);

            object args;
            if (!Ops.TryGetBoundAttr(pyObj, Symbols.Arguments, out args)) return 0;
            Tuple t = args as Tuple;

            if (t == null || t.Count == 0) return 0;

            if (TypeCache.Int32.IsInstanceOfType(t[0]))
                return Converter.ConvertToInt32(t[0]);

            otherCode = t[0];
            return 1;
        }
    }

    class PythonIndentationError : SyntaxErrorException {
        public PythonIndentationError(string msg) : base(msg) { }
        public PythonIndentationError(string msg, string filename, int lineNumber, int columnNumber, string badLineText, int errorCode, Severity severity)
            : base(msg, filename, lineNumber, columnNumber, badLineText, errorCode, severity) { }

    }

    class PythonTabError : PythonIndentationError {
        public PythonTabError(string msg) : base(msg) { }
        public PythonTabError(string msg, string filename, int lineNumber, int columnNumber, string badLineText, int errorCode, Severity severity)
            : base(msg, filename, lineNumber, columnNumber, badLineText, errorCode, severity) { }
    }

    //  Wrapper to allow throwing strings
    [Serializable]
    public sealed class StringException : Exception {
        object value;

        public StringException() { }

        public StringException(string message)
            : base(message) {
            value = message;
        }

        public StringException(string name, object value)
            : base(name) {
            this.value = value;
        }

        public StringException(string message, Exception innerException)
            : base(message, innerException) {
        }

#if !SILVERLIGHT // SerializationInfo
        private StringException(SerializationInfo info, StreamingContext context) : base(info, context) {
            value = info.GetValue("value", typeof(object));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("value", value);

            base.GetObjectData(info, context);
        }
#endif

        public override string ToString() {
            return base.Message;
        }

        public object Value {
            get {
                return value;
            }
        }        
    }

    //	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, Inherited=true)]
    //	public class PythonVarArgsAttribute:Attribute {
    //		public PythonVarArgsAttribute() {
    //		}
    //	}
}