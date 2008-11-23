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
using System; using Microsoft;


using Microsoft.Scripting;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    internal class ThrowingErrorSink : ErrorSink {
        public static new readonly ThrowingErrorSink/*!*/ Default = new ThrowingErrorSink();

        private ThrowingErrorSink() {
        }

        public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity) {
            if (severity == Severity.Warning) {
                throw PythonOps.SyntaxWarning(message, sourceUnit, span, errorCode);
            } else {
                throw PythonOps.SyntaxError(message, sourceUnit, span, errorCode);
            }
        }
    }
}
