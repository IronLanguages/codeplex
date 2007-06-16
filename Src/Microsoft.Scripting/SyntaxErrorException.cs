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

using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting {
   
    [Serializable]
    public class SyntaxErrorException : Exception {
        int lineNo, columnNo;
        string lineText, file;
        Severity sev;
        int error;

        public SyntaxErrorException() : base() { }
        public SyntaxErrorException(string msg) : base(msg) { }
        public SyntaxErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }
        public SyntaxErrorException(string msg, string fileName, int lineNumber, int columnNumber, string badLineText, int errorCode, Severity severity)
            : base(msg) {
            lineNo = lineNumber;
            columnNo = columnNumber;
            lineText = badLineText;
            file = fileName;
            sev = severity;
            error = errorCode;
        }

#if !SILVERLIGHT // SerializationInfo
        protected SyntaxErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            lineNo = info.GetInt32("line");
            columnNo = info.GetInt32("column");
            lineText = info.GetString("lineText");
            file = info.GetString("file");
            error = info.GetInt32("error");
            sev = (Severity)info.GetInt32("severity");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("line", lineNo);
            info.AddValue("column", columnNo);
            info.AddValue("lineText", lineText);
            info.AddValue("file", file);
            info.AddValue("error", error);
            info.AddValue("severity", (int)sev);

            base.GetObjectData(info, context);
        }
#endif
        public int Line {
            get { return lineNo; }
        }

        public int Column {
            get { return columnNo; }
        }

        public string FileName {
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
    }
}