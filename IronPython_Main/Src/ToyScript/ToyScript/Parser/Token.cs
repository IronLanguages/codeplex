/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;
using System.Diagnostics;
using Microsoft.Scripting;

using Microsoft.Scripting.Math;

namespace ToyScript.Parser {
    class Token {
        private SourceSpan _span;
        TokenKind _kind;
        string _image;

        public Token(SourceSpan span, TokenKind kind, string image) {
            _span = span;
            _kind = kind;
            _image = image;
        }

        public SourceSpan Span {
            get {
                return _span;
            }
        }

        public SourceLocation Start {
            get {
                return _span.Start;
            }
        }

        public SourceLocation End {
            get {
                return _span.End;
            }
        }

        public TokenKind Kind {
            get {
                return _kind;
            }
        }

        public string Image {
            get {
                return _image;
            }
        }
    }
}
