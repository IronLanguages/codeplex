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
using System.Text;

using IronPython.Compiler;
using IronPython.Runtime.Operations;
using IronPython.Hosting;

namespace IronPython.Compiler {
    internal class SimpleParserSink : CompilerSink {
        public override void AddError(string path, string message, string lineText, CodeSpan span, int errorCode, Severity severity) {
            if (severity == Severity.Warning) {
                throw Ops.SyntaxWarning(message, path, span.StartLine, span.StartColumn, lineText, severity);
            }

            switch (errorCode & ErrorCodes.ErrorMask) {
                case ErrorCodes.IndentationError:
                    throw Ops.IndentationError(message, path, span.StartLine, span.StartColumn, lineText, errorCode, severity);
                case ErrorCodes.TabError:
                    throw Ops.TabError(message, path, span.StartLine, span.StartColumn, lineText, errorCode, severity);
                default:
                    throw Ops.SyntaxError(message, path, span.StartLine, span.StartColumn, lineText, errorCode, severity);
            }
        }
    }
}
