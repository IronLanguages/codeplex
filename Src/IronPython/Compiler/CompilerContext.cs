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

using IronPython.Hosting;
using IronPython.Compiler.Ast;

namespace IronPython.Compiler {
    public class CompilerContext {
        /// <summary>
        /// Error reporting for the compiler
        /// </summary>
        private CompilerSink sink;

        /// <summary>
        /// from __future__ import division
        /// </summary>
        private bool trueDivision = Options.Division == DivisionOption.New;

        /// <summary>
        /// store name of current file being compiled in the CompilerContext
        /// </summary>
        private string sourceFile;

        private const int DefaultErrorCode = -1;

        public string SourceFile {
            get {
                return sourceFile;
            }
        }

        public bool TrueDivision {
            get {
                return trueDivision;
            }
            internal set {
                trueDivision = value;
            }
        }

        public CompilerSink Sink {
            get { return sink; }
        }

        public CompilerContext()
            : this("<string>", new SimpleParserSink()) {
        }

        public CompilerContext(string sourceFile)
            : this(sourceFile, new SimpleParserSink()) {
        }

        public CompilerContext(string sourceFile, CompilerSink sink) {
            this.sourceFile = sourceFile;
            this.sink = sink;
        }

        public void AddError(string message, string lineText, int startLine, int startColumn, int endLine, int endColumn, Severity severity) {
            sink.AddError(sourceFile, message, lineText, new CodeSpan(startLine, startColumn, endLine, endColumn), DefaultErrorCode, severity);
        }

        public void AddError(string message, string lineText, int startLine, int startColumn,
                             int endLine, int endColumn, int errorCode, Severity severity) {
            sink.AddError(sourceFile, message, lineText, new CodeSpan(startLine, startColumn, endLine, endColumn), errorCode, severity);
        }

        public void AddError(string message, Node node) {
            AddError(message, node, Severity.Error);
        }

        public void AddError(string message, Node node, Severity severity) {
            sink.AddError(sourceFile, message, null, node.Span, DefaultErrorCode, severity);
        }

        public CompilerContext CopyWithNewSourceFile(string newSourceFile) {
            CompilerContext ret = new CompilerContext(newSourceFile, sink);
            ret.trueDivision = this.trueDivision;
            return ret;
        }
    }
}
