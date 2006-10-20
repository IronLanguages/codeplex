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

using IronPython.Compiler.Ast;

namespace IronPython.Compiler {
    /// <summary>
    /// Summary description for Token.
    /// </summary>
    public abstract class Token {
        private readonly TokenKind kind;

        protected Token(TokenKind kind) {
            this.kind = kind;
        }

        public TokenKind Kind {
            get { return kind; }
        }

        public virtual object Value {
            get {
                throw new NotSupportedException("no value for this token");
            }
        }

        public override string ToString() {
            return base.ToString() + "(" + kind + ")";
        }

        public abstract String Image {
            get;
        }
    }

    public class ErrorToken : Token {
        private readonly String message;

        public ErrorToken(String message)
            : base(TokenKind.Error) {
            this.message = message;
        }

        public String Message {
            get { return message; }
        }

        public override String Image {
            get {
                return message;
            }
        }

        public override object Value {
            get {
                return message;
            }
        }
    }

    public class ConstantValueToken : Token {
        private readonly object value;

        public ConstantValueToken(object value)
            : base(TokenKind.Constant) {
            this.value = value;
        }

        public object Constant {
            get { return this.value; }
        }

        public override object Value {
            get {
                return value;
            }
        }

        public override String Image {
            get {
                if (value == null) return "None";

                return value.ToString();
            }
        }
    }

    public class IncompleteStringToken : ConstantValueToken {
        private bool quote;
        private bool isRaw;
        private bool isUni;
        private bool isTri;

        public IncompleteStringToken(object value, bool quote, bool isRaw, bool isUnicode, bool isTripleQuoted)
            : base(value) {
            this.quote = quote;
            this.isRaw = isRaw;
            this.isUni = isUnicode;
            this.isTri = isTripleQuoted;
        }

        /// <summary>
        /// True if the quotation is written using ', false if written using "
        /// </summary>
        public bool IsSingleTickQuote {
            get { return quote; }
            set { quote = value; }
        }

        /// <summary>
        /// True if the string is a raw-string (preceeded w/ r character)
        /// </summary>
        public bool IsRaw {
            get { return isRaw; }
            set { isRaw = value; }
        }

        /// <summary>
        /// True if the string is Unicode string (preceeded w/ a u character)
        /// </summary>
        public bool IsUnicode {
            get { return isUni; }
            set { isUni = value; }
        }

        /// <summary>
        /// True if the string is triple quoted (''' or """)
        /// </summary>
        public bool IsTripleQuoted {
            get { return isTri; }
            set { isTri = value; }
        }
    }

    public class CommentToken : SymbolToken {
        private readonly string comment;

        public CommentToken(string value)
            : base(TokenKind.Comment, "<comment>") {
            comment = value;
        }

        public string Comment {
            get { return comment; }
        }

        public override string Image {
            get {
                return comment;
            }
        }

        public override object Value {
            get {
                return comment;
            }
        }
    }

    public class NameToken : Token {
        private readonly SymbolId value;

        public NameToken(SymbolId value)
            : base(TokenKind.Name) {
            this.value = value;
        }

        public SymbolId Name {
            get { return this.value; }
        }

        public override object Value {
            get {
                return value;
            }
        }

        public override String Image {
            get {
                return value.ToString();
            }
        }
    }

    public class OperatorToken : Token {
        private readonly PythonOperator op;

        public OperatorToken(TokenKind kind, PythonOperator op)
            : base(kind) {
            this.op = op;
        }

        public PythonOperator Operator {
            get { return op; }
        }

        public override object Value {
            get {
                return op;
            }
        }

        public override String Image {
            get {
                return op.Symbol;
            }
        }
    }

    public class SymbolToken : Token {
        private readonly String image;

        public SymbolToken(TokenKind kind, String image)
            : base(kind) {
            this.image = image;
        }

        public String Symbol {
            get { return image; }
        }

        public override object Value {
            get {
                return image;
            }
        }

        public override String Image {
            get {
                return image;
            }
        }
    }
}
