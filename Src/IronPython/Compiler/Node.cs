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


namespace IronPython.Compiler.AST {
    public abstract class Node {
        public Location start;
        public Location end;

        protected Node() {
            start.line = -1;
            start.column = -1;
            end.line = -1;
            end.column = -1;
        }

        public void SetLoc(Location start, Location end) {
            this.start = start;
            this.end = end;
        }

        public void SetLoc(CodeSpan span) {
            start.line = span.startLine;
            start.column = span.startColumn;
            end.line = span.endLine;
            end.column = span.endColumn;
        }

        internal CodeSpan Span {
            get {
                return new CodeSpan(start.line, start.column, end.line, end.column);
            }
        }

        public abstract void Walk(IAstWalker w);
    }
}
