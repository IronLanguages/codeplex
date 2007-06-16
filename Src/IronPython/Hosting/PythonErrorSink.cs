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
using System.IO;
using System.Text;
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;

using IronPython.Compiler;
using IronPython.Runtime.Operations;

namespace IronPython.Hosting {
    public class PythonErrorSink : ErrorSink {
        private bool _throwExceptionOnError;

        public bool ThrowExceptionOnError {
            get { return _throwExceptionOnError; }
            set { _throwExceptionOnError = value; }
        }

        public PythonErrorSink() : this(false) {
        }

        public PythonErrorSink(bool throwExceptionOnError) {
            _throwExceptionOnError = throwExceptionOnError;
        }

        public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity) {
            CountError(severity);

            if (_throwExceptionOnError)
                throw new CompilerException(FormatExceptionMessage(severity, message, sourceUnit, span));

            string line_text = "";
            string file_name = "";

            if (sourceUnit != null) {

                if (span.Start.Line >= 1) {
                    using (SourceUnitReader reader = sourceUnit.GetReader()) {
                        reader.SeekLine(span.Start.Line);
                        line_text = reader.ReadLine();
                    }
                }

                file_name = sourceUnit.DisplayName;
            }
            
            if (severity == Severity.Warning) {

                if (file_name != "") {
                    message = String.Format("{0} ({1}, line {2})", message, file_name, span.Start.Line);
                }

                throw PythonOps.SyntaxWarning(message, file_name, span.Start.Line, span.Start.Column, line_text, severity);
            }

            switch (errorCode & ErrorCodes.ErrorMask) {
                case ErrorCodes.IndentationError:
                    throw PythonOps.IndentationError(message, file_name, span.Start.Line, span.Start.Column, line_text, errorCode, severity);
                case ErrorCodes.TabError:
                    throw PythonOps.TabError(message, file_name, span.Start.Line, span.Start.Column, line_text, errorCode, severity);
                default:
                    throw PythonOps.SyntaxError(message, file_name, span.Start.Line, span.Start.Column, line_text, errorCode, severity);
            }
        }

        protected static string FormatExceptionMessage(Severity severity, string message, SourceUnit sourceUnit, SourceSpan span) {
            return String.Format("{0}:{1} {2}{3}:{4}-{5}:{6}", severity.ToString(), message,
                (sourceUnit != null) ? "at " + sourceUnit.Name : "",
                span.Start.Line, span.Start.Column, span.End.Line, span.End.Column);
        }
    }
}
