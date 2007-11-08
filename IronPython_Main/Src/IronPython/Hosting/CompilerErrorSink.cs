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
using System.IO;
using System.Text;
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;

using IronPython.Compiler;
using IronPython.Runtime.Operations;

namespace IronPython.Hosting {
    internal class CompilerErrorSink : ErrorSink {
        public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity) {
            CountError(severity);

            if (severity == Severity.Warning) {
                throw PythonOps.SyntaxWarning(message, sourceUnit, span, errorCode);
            } else {
                throw PythonOps.SyntaxError(message, sourceUnit, span, errorCode);
            }
        }
    }
}
