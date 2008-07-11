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

using System.Runtime.Serialization;
using System.Scripting.Utils;
using System.Security.Permissions;

namespace System.Scripting {

    // TODO: move to Microsoft.Scripting !!!
    [Serializable]
    public class SyntaxErrorException : Exception {
        private SourceSpan _span;

        // TODO: either HAPI needs a different exception or this needs to be a string
        private SourceUnit _sourceUnit;

        private Severity _severity;
        private int _errorCode;

        public SyntaxErrorException() : base() { }

        public SyntaxErrorException(string message) : base(message) { }

        public SyntaxErrorException(string message, Exception innerException)
            : base(message, innerException) {
        }

        public SyntaxErrorException(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode, Severity severity)
            : base(message) {
            ContractUtils.RequiresNotNull(message, "message");

            _span = span;
            _sourceUnit = sourceUnit;
            _severity = severity;
            _errorCode = errorCode;
        }

#if !SILVERLIGHT
        protected SyntaxErrorException(SerializationInfo info, StreamingContext context) 
            : base(info, context) { }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo/*!*/ info, StreamingContext context) {
            ContractUtils.RequiresNotNull(info, "info");

            base.GetObjectData(info, context);
            info.AddValue("Span", _span);
            info.AddValue("SourceUnit", _sourceUnit);
            info.AddValue("Severity", _severity);
            info.AddValue("ErrorCode", _errorCode);
        }
#endif

        /// <summary>
        /// Unmapped span.
        /// </summary>
        public SourceSpan RawSpan {
            get { return _span; }
        }

        public SourceUnit SourceUnit {
            get { return _sourceUnit; }
        }

        public Severity Severity {
            get { return _severity; }
        }

        public int Line {
            get { return _span.Start.Line; }
        }

        public int Column {
            get { return _span.Start.Column; }
        }

        public int ErrorCode {
            get { return _errorCode; }
        }

        // TODO: fix
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetSymbolDocumentName() {
            return _sourceUnit != null ? _sourceUnit.Path : null;
        }

        // TODO: fix
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetCodeLine() {
            return (_sourceUnit != null && Line > 0) ? _sourceUnit.GetCodeLine(Line) : null;
        }
    }
}