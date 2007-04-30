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
using System.Diagnostics;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public abstract class Node {
        private SourceLocation _start;
        private SourceLocation _end;

        protected Node() {
            _start = SourceLocation.Invalid;
            _end = SourceLocation.Invalid;
        }

        protected Node(SourceSpan span) {
            _start = span.Start;
            _end = span.End;
        }

        public void SetLoc(SourceLocation start, SourceLocation end) {
            _start = start;
            _end = end;
        }

        public void SetLoc(SourceSpan span) {
            _start = span.Start;
            _end = span.End;
        }

        public abstract void Walk(Walker walker);

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
    }
}
