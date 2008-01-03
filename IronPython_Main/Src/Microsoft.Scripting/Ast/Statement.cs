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
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Scripting.Ast {
    public abstract class Statement : Node {
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        protected Statement(AstNodeType nodeType, SourceSpan span)
            : base(nodeType) {
            _start = span.Start;
            _end = span.End;
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }

        public SourceSpan Span {
            get {
                return new SourceSpan(_start, _end);
            }
        }
    }
}
