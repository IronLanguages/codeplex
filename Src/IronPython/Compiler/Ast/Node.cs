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


using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    public abstract class Node {
        private SourceLocation _start = SourceLocation.Invalid;
        private SourceLocation _end = SourceLocation.Invalid;

        protected Node() {
        }

        public void SetLoc(SourceLocation start, SourceLocation end) {
            _start = start;
            _end = end;
        }

        public void SetLoc(SourceSpan span) {
            _start = span.Start;
            _end = span.End;
        }

        public SourceLocation Start {
            get { return _start; }
            set { _start = value; }
        }

        public SourceLocation End {
            get { return _end; }
            set { _end = value; }
        }

        public SourceSpan Span {
            get {
                return new SourceSpan(_start, _end);
            }
        }

        public abstract void Walk(PythonWalker walker);
    }
}
