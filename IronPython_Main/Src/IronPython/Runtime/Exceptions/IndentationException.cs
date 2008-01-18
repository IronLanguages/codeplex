using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace IronPython.Runtime.Exceptions {
    /// <summary>
    /// .NET exception thrown when a Python syntax error is related to incorrect indentation.
    /// </summary>
    [Serializable]
    class IndentationException : SyntaxErrorException {
        public IndentationException(string message) : base(message) { }

        public IndentationException(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode, Severity severity)
            : base(message, sourceUnit, span, errorCode, severity) { }

    }
}
