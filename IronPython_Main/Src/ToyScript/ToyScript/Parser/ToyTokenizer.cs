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
using System.Text.RegularExpressions;
using Microsoft.Scripting.Ast;
using System.Diagnostics;
using Microsoft.Scripting;

using System.Globalization;

using Microsoft.Scripting.Math;


namespace ToyScript.Parser {
    class ToyTokenizer {
        private string _text;
        private int _index;

        private int _line = 1;
        private int _column = 1;

        private enum CharClass {
            Letter,
            Number,
            Space,
            Other
        }

        private static Dictionary<string, TokenKind> _keywords = MakeKeywords();

        private static Dictionary<string, TokenKind> MakeKeywords() {
            Dictionary<string, TokenKind> kws = new Dictionary<string,TokenKind>();
            kws["new"] = TokenKind.KwNew;
            kws["if"] = TokenKind.KwIf;
            kws["else"] = TokenKind.KwElse;
            kws["while"] = TokenKind.KwWhile;
            kws["def"] = TokenKind.KwDef;
            kws["return"] = TokenKind.KwReturn;
            kws["var"] = TokenKind.KwVar;
            kws["print"] = TokenKind.KwPrint;
            kws["import"] = TokenKind.KwImport;
            return kws;
        }

        private static CharClass GetClass(char ch) {
            switch (ch) {
                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    return CharClass.Space;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return CharClass.Number;

                default:
                    if (Char.IsLetter(ch)) return CharClass.Letter;
                    else return CharClass.Other;
            }
        }

        public ToyTokenizer(string text) {
            _text = text;
        }

        public bool EndOfFile {
            get {
                return _index >= _text.Length;
            }
        }

        public SourceLocation Location {
            get { return new SourceLocation(_index, _line, _column); }
        }

        private static bool IsNumberPart(char ch) {
            return ch == '.' || (ch >= '0' && ch <= '9');
        }

        private static bool IsNamePart(char ch) {
            if (ch == '_') return true;
            CharClass cc = GetClass(ch);
            return cc == CharClass.Letter || cc == CharClass.Number;
        }

        private static bool IsSpace(char ch) {
            return GetClass(ch) == CharClass.Space;
        }

        private char PeekChar() {
            return _text[_index];
        }

        public char NextChar() {
            char ch = _text[_index++];
            if (ch == '\n') {
                _line++;
                _column = 1;
            } else {
                _column++;
            }
            return ch;
        }

        public Token Next() {
            SourceLocation start = Location;

            if (_index >= _text.Length) {
                return new Token(new SourceSpan(start, start), TokenKind.EOF, "<eof>");
            }

            char ch = NextChar();

            switch (ch) {
                case '+': return FinishWithEqual(start, TokenKind.Add, TokenKind.AddEqual);
                case '-': return FinishWithEqual(start, TokenKind.Subtract, TokenKind.SubtractEqual);
                case '*': return FinishWithEqual(start, TokenKind.Multiply, TokenKind.MultiplyEqual);
                case '/': return FinishWithEqual(start, TokenKind.Divide, TokenKind.DivideEqual);

                case '<': return FinishWithEqual(start, TokenKind.LessThan, TokenKind.LessThanEqual);
                case '>': return FinishWithEqual(start, TokenKind.GreaterThan, TokenKind.GreaterThanEqual);
                case '!': return FinishWithEqual(start, TokenKind.Bang, TokenKind.NotEqual);
                case '=': return FinishWithEqual(start, TokenKind.Equal, TokenKind.EqualEqual);

                case '.': return FinishToken(start, TokenKind.Dot);
                case ',': return FinishToken(start, TokenKind.Comma);
                case ';': return FinishToken(start, TokenKind.SemiColon);

                case '(': return FinishToken(start, TokenKind.OpenParen);
                case ')': return FinishToken(start, TokenKind.CloseParen);
                case '{': return FinishToken(start, TokenKind.OpenCurly);
                case '}': return FinishToken(start, TokenKind.CloseCurly);
                case '[': return FinishToken(start, TokenKind.OpenBracket);
                case ']': return FinishToken(start, TokenKind.CloseBracket);

                case '"': return FinishString(start);
            }

            switch (GetClass(ch)) {
                case CharClass.Letter:
                    return FinishName(start);
                case CharClass.Number:
                    return FinishToken(start, TokenKind.Number, IsNumberPart);
                case CharClass.Space:
                    SkipTest(IsSpace);
                    return Next();
                default:
                    throw new NotImplementedException("can't handle: " + ch);
            }
        }

        private void SkipTest(Predicate<char> test) {
            while (!EndOfFile && test(PeekChar())) {
                NextChar();
            }
        }

        private Token FinishToken(SourceLocation start, TokenKind kind, Predicate<char> test) {
            SkipTest(test);
            return FinishToken(start, kind);
        }

        private Token FinishToken(SourceLocation start, TokenKind kind) {
            return new Token(new SourceSpan(start, Location), kind, _text.Substring(start.Index, _index - start.Index));
        }

        private Token FinishWithEqual(SourceLocation start, TokenKind kind, TokenKind kindWithEqual) {
            if (!EndOfFile && PeekChar() == '=') {
                NextChar();
                kind = kindWithEqual;
            }
            return FinishToken(start, kind);
        }

        private Token FinishString(SourceLocation start) {
            StringBuilder sb = new StringBuilder();

            for (; ; ) {
                if (EndOfFile) {
                    throw ToyParser.SyntaxError("End of file in the string");
                }

                char ch = NextChar();

                if (ch == '"') {
                    if (EndOfFile || PeekChar() != '"') {
                        break;
                    } else {
                        NextChar();
                    }
                }

                sb.Append(ch);
            }
            return new Token(new SourceSpan(start, Location), TokenKind.String, sb.ToString());
        }

        private Token FinishName(SourceLocation start) {
            SkipTest(IsNamePart);

            string image = _text.Substring(start.Index, _index - start.Index);

            TokenKind kind;
            if (!_keywords.TryGetValue(image, out kind)) {
                kind = TokenKind.Name;
            }
            return new Token(new SourceSpan(start, Location), kind, image);
        }
    }
}

