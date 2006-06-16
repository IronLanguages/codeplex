/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using IronPython.Runtime;

namespace IronPython.Runtime.Exceptions {
    [PythonType("traceback")]
    public class TraceBack {
        TraceBack next;
        TraceBackFrame frame;
        int line, offset;
        bool userSupplied;

        public TraceBack(TraceBack nextTraceBack, TraceBackFrame fromFrame) {
            next = nextTraceBack;
            frame = fromFrame;
        }

        public void UpdateFromStackTrace(StackTrace st) {
            // extract line info & IL offsets from stack trace
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

        public bool IsUserSupplied {
            get {
                return userSupplied;
            }
            set {
                userSupplied = value;
            }
        }
    }

    public class TraceBackFrame {
        private object globals;
        private object locals;
        private object code;

        public object Globals {
            [PythonName("f_globals")]
            get {
                return globals;
            }
        }

        public object Locals {
            [PythonName("f_locals")]
            get {
                return locals;
            }
        }

        public object Code {
            [PythonName("f_code")]
            get {
                return code;
            }
        }

        public TraceBackFrame(object globals, object locals, object code) {
            this.globals = globals;
            this.locals = locals;
            this.code = code;
        }
    }
}
