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
using System.Diagnostics;
using System.CodeDom;

using IronPython.Hosting;
using IronPython.CodeDom;


namespace IronPython.Compiler.Ast {
    public abstract class Node {
        private Location start;
        private Location end;

        protected Node() {
            start.Line = -1;
            start.Column = -1;
            end.Line = -1;
            end.Column = -1;
        }

        public void SetLoc(Location start, Location end) {
            this.start = start;
            this.end = end;
        }

        public void SetLoc(CodeSpan span) {
            start.Line = span.StartLine;
            start.Column = span.StartColumn;
            end.Line = span.EndLine;
            end.Column = span.EndColumn;
        }

        internal CodeSpan Span {
            get {
                return new CodeSpan(start.Line, start.Column, end.Line, end.Column);
            }
        }

        public abstract void Walk(IAstWalker walker);

        public Location Start {
            get { return start; }
            set { start = value; }
        }
        public Location End {
            get { return end; }
            set { end = value; }
        }
    }
}
