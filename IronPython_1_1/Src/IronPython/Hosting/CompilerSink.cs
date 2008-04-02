/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
        private int startLine;
        private int startColumn;
        private int endLine;
        private int endColumn;

        public static readonly CodeSpan Empty;

        public CodeSpan(int startLine, int startColumn, int endLine, int endColumn) {
            this.startLine = startLine;
            this.startColumn = startColumn;
            this.endLine = endLine;
            this.endColumn = endColumn;
        }

        internal CodeSpan(IronPython.Compiler.Location start, IronPython.Compiler.Location end) {
            this.startLine = start.Line;
            this.startColumn = start.Column;
            this.endLine = end.Line;
            this.endColumn = end.Column;
        }

        public override bool Equals(object obj) {
            if (!(obj is CodeSpan)) return false;

            CodeSpan other = (CodeSpan)obj;
            return startColumn == other.startColumn &&
                    startLine == other.startLine &&
                    endColumn == other.endColumn &&
                    endLine == other.endLine;
        }

        public override int GetHashCode() {
            // 7 bits for each column (0-128), 9 bits for each row (0-512), xor helps if
            // we have a bigger file.
            return (startColumn) ^ (endColumn << 7) ^ (startLine << 14) ^ (endLine << 23);
        }

        public static bool operator ==(CodeSpan self, CodeSpan other) {
            return self.startColumn == other.startColumn &&
                    self.startLine == other.startLine &&
                    self.endColumn == other.endColumn &&
                    self.endLine == other.endLine;
        }

        public static bool operator !=(CodeSpan self, CodeSpan other) {
            return self.startColumn != other.startColumn ||
                    self.startLine != other.startLine ||
                    self.endColumn != other.endColumn ||
                    self.endLine != other.endLine;
        }

        public int StartLine {
            get { return startLine; }
            set { startLine = value; }
        }

        public int StartColumn {
            get { return startColumn; }
            set { startColumn = value; }
        }

        public int EndLine {
            get { return endLine; }
            set { endLine = value; }
        }

        public int EndColumn {
            get { return endColumn; }
            set { endColumn = value; }
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
        public override void AddError(string path, string message, string lineText, CodeSpan location, int errorCode, Severity severity) {
            string sev;
            if (severity >= Severity.Error)
                sev = "Error";
            else if (severity >= Severity.Warning)
                sev = "Warning";
            else
                sev = "Message";

            throw new CompilerException(string.Format("{0}:{1} at {2} {3}:{4}-{5}:{6}", sev, message, path, location.StartLine, location.StartColumn, location.EndLine, location.EndColumn));
        }
    }

    /// <summary>
    /// Private class used for raising compiler exceptions.
    /// </summary>
    class CompilerException : Exception {
        public CompilerException(string msg)
            : base(msg) {
        }

        public CompilerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
