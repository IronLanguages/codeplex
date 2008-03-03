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

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    internal sealed class ErrorListenerProxySink : ErrorSink {
        private readonly ErrorListener _listener;
        private readonly ScriptSource/*!*/ _source;

        public ErrorListenerProxySink(ScriptSource/*!*/ source, ErrorListener listener) {
            _listener = listener;
            _source = source;
        }

        public override void Add(SourceUnit sourceUnit, string/*!*/ message, SourceSpan span, int errorCode, Severity severity) {
            if (_listener != null) {
                ScriptSource scriptSource;
                if (sourceUnit != _source.SourceUnit) {
                    scriptSource = new ScriptSource(_source.Engine.Runtime.GetEngine(sourceUnit.LanguageContext), sourceUnit);
                } else {
                    scriptSource = _source;
                }

                _listener.ReportError(scriptSource, message, span, errorCode, severity);
            } else {
                throw new SyntaxErrorException(message, sourceUnit, span, errorCode, severity);
            }
        }
    }
}
