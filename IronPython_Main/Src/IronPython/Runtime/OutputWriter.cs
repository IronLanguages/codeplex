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

using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using IronPython.Hosting;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime {
    sealed class OutputWriter : TextWriter {
        private PythonContext _context;
        private bool _isErrorOutput;

        public OutputWriter(PythonContext context, bool isErrorOutput) {
            _context = context;
            _isErrorOutput = isErrorOutput;
        }

        public object Sink {
            get {
                return (_isErrorOutput) ? _context.SystemStandardError : _context.SystemStandardOut;
            }
        }

        public override Encoding Encoding {
            get {
                PythonFile file = Sink as PythonFile;
                return (file != null) ? file.Encoding : null;
            }
        }

        public override void Write(string value) {
            // the context arg is only used to get stdout if it's not passed in
            try {
                PythonOps.PrintWithDestNoNewline(DefaultContext.Default, Sink, value);
            } catch (Exception e) {
                PythonOps.PrintWithDest(DefaultContext.Default, _context.SystemStandardOut, _context.FormatException(e));
            }
        }

        public override void Write(char value) {
            Write(value.ToString());
        }

        public override void Write(char[] value) {
            Write(new string(value));
        }

        public override void Flush() {
            if (PythonOps.HasAttr(DefaultContext.Default, Sink, SymbolTable.StringToId("flush"))) {
                PythonOps.Invoke(Sink, SymbolTable.StringToId("flush"));
            }
        }
    }
}
