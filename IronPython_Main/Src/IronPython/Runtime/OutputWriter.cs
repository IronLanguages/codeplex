/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
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

        private bool _isErrorOutput;

        public OutputWriter(bool isErrorOutput) {
            _isErrorOutput = isErrorOutput;
        }

        public object Sink {
            get {
                return (_isErrorOutput) ? SystemState.Instance.stderr : SystemState.Instance.stdout;
            }
        }

        public override Encoding Encoding {
            get {
                PythonFile file = Sink as PythonFile;
                return (file != null) ? file.Encoding : null;
            }
        }

        public override void Write(string value) {
            try {
                PythonOps.PrintWithDestNoNewline(Sink, value);
            } catch (Exception e) {
                PythonOps.Print(PythonEngine.CurrentEngine.FormatException(e));
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
