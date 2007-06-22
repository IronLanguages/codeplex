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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using IronPython.Runtime;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "IronPython.Runtime.Exceptions.TraceBackFrame..ctor(System.Object,System.Object,System.Object)", MessageId = "0#globals")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "IronPython.Runtime.Exceptions.TraceBackFrame.Globals", MessageId = "Globals")]

namespace IronPython.Runtime.Exceptions {
    [PythonType("traceback")]
    [Serializable]
    public class TraceBack {
        TraceBack next;
        TraceBackFrame frame;
        int line, offset;

        public TraceBack(TraceBack nextTraceBack, TraceBackFrame fromFrame) {
            next = nextTraceBack;
            frame = fromFrame;
        }

        public TraceBack Next {
            [PythonName("tb_next")]
            get {
                return next;
            }
        }

        public object ModuleScope {
            [PythonName("tb_frame")]
            get {
                return frame;
            }
        }

        public int Line {
            [PythonName("tb_lineno")]
            get {
                return line;
            }
        }

        public int Offset {
            [PythonName("tb_lasti")]
            get {
                return offset;
            }
        }

        internal void SetLine(int lineNumber) {
            line = lineNumber;
        }

        internal void SetOffset(int ilOffset) {
            offset = ilOffset;
        }
    }

    [PythonType("frame")]
    [Serializable]
    public class TraceBackFrame {
        private object _globals;
        private object _locals;
        private object _code;

        public object Globals {
            [PythonName("f_globals")]
            get {
                return _globals;
            }
        }

        public object Locals {
            [PythonName("f_locals")]
            get {
                return _locals;
            }
        }

        public object Code {
            [PythonName("f_code")]
            get {
                return _code;
            }
        }

        public TraceBackFrame(object globals, object locals, object code) {
            this._globals = globals;
            this._locals = locals;
            this._code = code;
        }
    }
}
