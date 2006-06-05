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
using IronPython.Runtime;

namespace IronPython.Compiler {
    /// <summary>
    /// Summary description for Token.
    /// </summary>
    public abstract class Token {
        public readonly TokenKind kind;
        protected Token(TokenKind kind) {
            this.kind = kind;
        }
        public virtual object GetValue() {
            throw new NotSupportedException("no value for this token");
        }

        public override string ToString() {
            return base.ToString() + "(" + kind + ")";
        }

        public abstract String GetImage();
    }

    public class ErrorToken : Token {
        public readonly String message;
        public ErrorToken(String message)
            : base(TokenKind.Error) {
            this.message = message;
        }

        public override String GetImage() {
            return message;
        }

        public override object GetValue() {
            return message;
        }
    }

    public class ConstantValueToken : Token {
        public readonly object value;

        public ConstantValueToken(object value)
            : base(TokenKind.Constant) {
            this.value = value;
        }

        public override object GetValue() {
            return value;
        }

        public override String GetImage() {
            if (value == null) return "None";

            return value.ToString();
        }
    }

    public class IncompleteStringToken : ConstantValueToken {
        public ConstantValueToken token;
        public bool quote;
        public bool isRaw;
        public bool isUni;
        public bool isTri;

        public IncompleteStringToken(object value, bool quote, bool isRaw, bool isUni, bool isTri)
            : base(value) {
            this.quote = quote;
            this.isRaw = isRaw;
            this.isUni = isUni;
            this.isTri = isTri;
        }
    }

    public class CommentToken : SymbolToken {
        public readonly string comment;

        public CommentToken(string value) : base(TokenKind.Comment, "<comment>") {
            comment = value;
        }

        public override string GetImage() {
            return comment;
        }

        public override object GetValue() {
            return comment;
        }
    }

    public class NameToken : Token {
        public readonly SymbolId value;
        public NameToken(SymbolId value)
            : base(TokenKind.Name) {
            this.value = value;
        }

        public override object GetValue() {
            return value;
        }

        public override String GetImage() {
            return value.ToString();
        }
    }

    public class OperatorToken : Token {
        public readonly Operator op;
        public OperatorToken(TokenKind kind, Operator op)
            : base(kind) {
            this.op = op;
        }

        public override object GetValue() {
            return op;
        }

        public override String GetImage() {
            return op.symbol;
        }
    }

    public class SymbolToken : Token {
        public readonly String image;
        public SymbolToken(TokenKind kind, String image)
            : base(kind) {
            this.image = image;
        }

        public override object GetValue() {
            return image;
        }

        public override String GetImage() {
            return image;
        }
    }
}
