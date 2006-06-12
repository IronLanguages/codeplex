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

namespace IronPython.Hosting {
    public enum Severity {
        Message,
        Warning,
        Error,
    }

    public static class ErrorCodes {
        // The error flags
        public const int IncompleteMask = 0x000F;

        public const int IncompleteStatement = 0x0001;      // unexpected <eof> found
        public const int IncompleteToken = 0x0002;

        // The actual error values

        public const int ErrorMask = 0x7FFFFFF0;

        public const int SyntaxError = 0x0010;              // general syntax error
        public const int IndentationError = 0x0020;         // invalid intendation
        public const int TabError = 0x0030;                 // invalid tabs
    }

    public struct CodeSpan {
        public int startLine;
        public int startColumn;
        public int endLine;
        public int endColumn;

        public static CodeSpan Empty;

        public CodeSpan(int startLine, int startColumn, int endLine, int endColumn) {
            this.startLine = startLine;
            this.startColumn = startColumn;
            this.endLine = endLine;
            this.endColumn = endColumn;
        }

        public CodeSpan(IronPython.Compiler.Location start, IronPython.Compiler.Location end) {
            this.startLine = start.line;
            this.startColumn = start.column;
            this.endLine = end.line;
            this.endColumn = end.column;
        }
    }

    public abstract class CompilerSink {
        public abstract void AddError(string path, string message, string lineText, CodeSpan location, int errorCode, Severity severity);

        public virtual void MatchPair(CodeSpan start, CodeSpan end, int priority) {
        }

        public virtual void MatchTriple(CodeSpan start, CodeSpan middle, CodeSpan end, int priority) {
        }

        public virtual void EndParameters(CodeSpan span) {
        }

        public virtual void NextParameter(CodeSpan span) {
        }

        public virtual void QualifyName(CodeSpan selector, CodeSpan span, string name) {
        }

        public virtual void StartName(CodeSpan span, string name) {
        }

        public virtual void StartParameters(CodeSpan context) {
        }
    }

    public class CompilerExceptionSink : CompilerSink {
        public override void AddError(string path, string message, string lineText, CodeSpan span, int errorCode, Severity severity) {
            string sev;
            if (severity >= Severity.Error)
                sev = "Error";
            else if (severity >= Severity.Warning)
                sev = "Warning";
            else
                sev = "Message";

            throw new Exception(string.Format("{0}:{1} at {2} {3}:{4}-{5}:{6}", sev, message, path, span.startLine, span.startColumn, span.endLine, span.endColumn));
        }
    }
}
