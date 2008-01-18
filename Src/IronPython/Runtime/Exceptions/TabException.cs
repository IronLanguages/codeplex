using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;

namespace IronPython.Runtime.Exceptions {
    /// <summary>
    /// .NET Exception thrown when a Python syntax error is related to incorrect tabs.
    /// </summary>
    [Serializable]
    sealed class TabException : IndentationException {
        public TabException(string message) : base(message) { }

        public TabException(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode, Severity severity)
            : base(message, sourceUnit, span, errorCode, severity) { }
    }
}
