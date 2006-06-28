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
using System.Runtime.Serialization;
using System.Text;
using IronPython.Hosting;

using IronPython.Modules;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Exceptions {
    [Serializable]
    public class ArgumentTypeException : Exception {
        public ArgumentTypeException()
            : base() {
        }

        public ArgumentTypeException(string message)
            : base(message) {
        }

        public ArgumentTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
    [Serializable]
    public class StopIterationException : InvalidOperationException {
        public StopIterationException() {
        }

        public StopIterationException(string message)
            : base(message) {
        }

        public StopIterationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [PythonType("SystemExit")]
    [Serializable]
    public class PythonSystemExit : Exception {
        public PythonSystemExit() : base() { }
        public PythonSystemExit(string msg)
            : base(msg) {
        }
        public PythonSystemExit(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public int GetExitCode(ICallerContext context) {
            object pyObj = ExceptionConverter.ToPython(this);

            object args;
            if (!Ops.TryGetAttr(pyObj, SymbolTable.Arguments, out args)) return 0;
            Tuple t = args as Tuple;

            if (t == null || t.Count == 0) return 0;

            try {
                return Converter.ConvertToInt32(t[0]);
            } catch {
            }

            try {
                Ops.PrintWithDest(context.SystemState, context.SystemState.stderr, t[0]);
            } catch {
            }

            return 1;
        }
    }

    [PythonType("SyntaxError")]
    [Serializable]
    public class PythonSyntaxError : Exception, ICustomExceptionConversion {
        int lineNo, columnNo;
        string lineText, file;
        Severity sev;
        int error;

        public PythonSyntaxError() : base() { }
        public PythonSyntaxError(string msg) : base(msg) { }
        public PythonSyntaxError(string msg, string filename, int lineNumber, int columnNumber, string badLineText, int errorCode, Severity severity)
            : base(msg) {
            lineNo = lineNumber;
            columnNo = columnNumber;
            lineText = badLineText;
            file = filename;
            sev = severity;
            error = errorCode;
        }

        public PythonSyntaxError(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public int Line {
            get { return lineNo; }
        }

        public int Column {
            get { return columnNo; }
        }

        public string Filename {
            get { return file; }
        }

        public string LineText {
            get { return lineText; }
        }

        public Severity Severity {
            get { return sev; }
        }

        public int ErrorCode {
            get { return error; }
        }

        protected virtual string PythonExceptionName {
            get { return "SyntaxError"; }
        }

        #region ICustomExceptionConversion Members

        public object ToPythonException() {
            IPythonType exType = ExceptionConverter.GetPythonException(PythonExceptionName);
            object inst = Ops.Call(exType);

            Ops.SetAttr(DefaultContext.Default, inst, SymbolTable.ExceptionMessage, base.Message);

            Ops.SetAttr(DefaultContext.Default, inst, SymbolTable.Arguments, Tuple.MakeTuple(
                base.Message,
                Tuple.MakeTuple(
                    file,
                    lineNo,
                    columnNo == 0 ? null : (object)columnNo,
                    lineText
                )
                ));

            Ops.SetAttr(DefaultContext.Default, inst, SymbolTable.ExceptionFilename, file);
            Ops.SetAttr(DefaultContext.Default, inst, SymbolTable.ExceptionLineNumber, lineNo);
            if (columnNo != 0) Ops.SetAttr(DefaultContext.Default, inst, SymbolTable.ExceptionOffset, columnNo);
            else Ops.SetAttr(DefaultContext.Default, inst, SymbolTable.ExceptionOffset, null);

            Ops.SetAttr(DefaultContext.Default, inst, SymbolTable.Text, lineText);
            // print_file_and_line
            return inst;
        }

        #endregion
    }

    class PythonIndentationError : PythonSyntaxError {
        public PythonIndentationError(string msg) : base(msg) { }
        public PythonIndentationError(string msg, string filename, int lineNumber, int columnNumber, string badLineText, int errorCode, Severity severity)
            : base(msg, filename, lineNumber, columnNumber, badLineText, errorCode, severity) { }

        protected override string PythonExceptionName {
            get {
                return "IndentationError";
            }
        }
    }

    class PythonTabError : PythonIndentationError {
        public PythonTabError(string msg) : base(msg) { }
        public PythonTabError(string msg, string filename, int lineNumber, int columnNumber, string badLineText, int errorCode, Severity severity)
            : base(msg, filename, lineNumber, columnNumber, badLineText, errorCode, severity) { }

        protected override string PythonExceptionName {
            get {
                return "TabError";
            }
        }
    }

    //  Wrapper to allow throwing strings
    [Serializable]
    public sealed class StringException : Exception, ICustomExceptionConversion {
        object value;

        public StringException(string name, object value)
            : base(name) {
            this.value = value;
        }

        public StringException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override string ToString() {
            return base.Message;
        }

        public object Value {
            get {
                return value;
            }
        }
        #region ICustomExceptionConversion Members

        public object ToPythonException() {
            return this;
        }

        #endregion
    }

    //	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, Inherited=true)]
    //	public class PythonVarArgsAttribute:Attribute {
    //		public PythonVarArgsAttribute() {
    //		}
    //	}
}