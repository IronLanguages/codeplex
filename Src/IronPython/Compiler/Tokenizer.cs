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
using System.Text;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Hosting;

namespace IronPython.Compiler {
    /// <summary>
    /// Summary description for Tokenizer.
    /// </summary>
    public partial class Tokenizer {
        private readonly char[] data;
        private readonly int length;
        private int index = 0;

        private const int EOF = -1;
        private const int NONE = -2;

        // token indexes in the text buffer
        private int start, end;

        // Token positions in the source text
        public Location startLoc;
        public Location endLoc;

        // Current position in the source text
        public Location current;

        // indentation state
        private const int MAX_INDENT = 80;
        private int[] indent = new int[MAX_INDENT];
        private int indentLevel = 0;
        private int pendingDedents = 0;
        private int pendingNewlines = 1;

        // Indentation state used only when we're reporting on inconsistent identation format.
        private StringBuilder[] indentFormat;

        // grouping state
        private int parenLevel = 0, braceLevel = 0, bracketLevel = 0;
        private bool verbatim;

        // Compiler context
        private CompilerContext context;
        private SystemState systemState;

        public Tokenizer(SystemState state, CompilerContext context, char[] data)
            : this(data, false, state, context) {
        }

        public Tokenizer(char[] data, bool verbatim, SystemState state, CompilerContext context) {
            this.data = data;
            this.length = data.Length;
            this.verbatim = verbatim;

            this.current.line = 1;
            this.startLoc.line = 1;
            this.endLoc.line = 1;

            this.context = context;
            this.systemState = state;

            if (Options.WarningOnIndentationInconsistency || Options.ErrorOnIndentationInconsistency) {
                indentFormat = new StringBuilder[MAX_INDENT];
            }
        }

        private bool CheckingForIndentationInconsistencies {
            get {
                return indentFormat != null;
            }
        }

        protected bool NextChar(int ch) {
            if (PeekChar() == ch) {
                NextChar();
                return true;
            } else {
                return false;
            }
        }

        protected int NextChar() {
            if (index < length) {
                int ret = data[index];
                index++;
                current.column++;
                if (ret == '\n') {
                    current.line++; current.column = 0;
                } else if (ret == '\r') {
                    if (PeekChar() == '\n') {
                        NextChar();
                    } else {
                        current.line++; current.column = 0;
                    }
                    ret = '\n';
                }
                return ret;
            } else {
                index++; current.column++;
                return EOF;
            }
        }

        protected int PeekChar() {
            if ((0 <= index) && (index < length)) return data[index];
            else return EOF;
        }

        protected void Backup() {
            index--;
            current.column--;

            switch (PeekChar()) {
                case '\n':
                    if (data[index - 1] == '\r') index--;
                    goto case '\r';

                case '\r':
                    System.Diagnostics.Debug.Assert(current.column == -1);
                    current.line--;

                    // Calculate new column value
                    for (current.column = 0; current.column < index; current.column++) {
                        int ch = data[index - current.column - 1];
                        if (ch == '\n' || ch == '\r') break;
                    }
                    break;
            }

            System.Diagnostics.Debug.Assert(current.column >= 0);
        }

        internal String GetImage() {
            return new String(data, start, end - start);
        }

        private bool IsBeginningOfFile {
            get {
                return start == 0;
            }
        }

        public bool IsEndOfFile {
            get {
                return PeekChar() == EOF;
            }
        }

        public Token Next() {
            if (pendingDedents != 0) {
                if (pendingDedents == -1) {
                    pendingDedents = 0;
                    return Tokens.IndentToken;
                } else {
                    pendingDedents--;
                    return Tokens.DedentToken;
                }
            }
            while (true) {
                SetStart();

                int ch = NextChar();
                switch (ch) {
                    case EOF:
                        SetEnd();
                        return ReadEof();
                    case ' ': case '\t': case '\f':
                        if (IsBeginningOfFile) {
                            SkipInitialWhitespace();
                        }
                        continue;
                    case '#':
                        // do single-line comment
                        return ReadEolComment();

                    case '\\':
                        if (PeekChar() == '\n' || PeekChar() == '\r') {
                            NextChar();
                            return Next();
                        } else {
                            return BadChar(ch);
                        }
                    case '\n':
                        return ReadNewline();
                    case '\"': case '\'':
                        return ReadString((char)ch, false, false);
                    case 'u':  case 'U':
                        if (NextChar('\"')) return ReadString('\"', false, true);
                        if (NextChar('\'')) return ReadString('\'', false, true);
                        if (NextChar('r') || NextChar('R')) {
                            if (NextChar('\"')) return ReadString('\"', true, true);
                            if (NextChar('\'')) return ReadString('\'', true, true);
                            Backup();
                        }
                        return ReadName((char)ch);
                    case 'r': case 'R':
                        if (NextChar('\"')) return ReadString('\"', true, false);
                        if (NextChar('\'')) return ReadString('\'', true, false);
                        return ReadName((char)ch);
                    case '.':
                        if (IsDigit(PeekChar())) {
                            return ReadFloatPostDot();
                        } else {
                            SetEnd();
                            return Tokens.DotToken;
                        }



                    //NUMBERS
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9': 
                        return ReadNumber((char)ch);

                    case '_': return ReadName('_');

                    case '\xEF':
                        if (start == 0 && NextChar('\xBB') && NextChar('\xBF')) {
                            continue;
                        }
                        goto default;

                    default:
                        Token res = NextOperator(ch);
                        if (res != null) return res;

                        if (IsNameStart(ch)) return ReadName((char)ch);
                        return BadChar(ch);
                }
            }
        }

        private void SkipInitialWhitespace() {
            while (true) {
                int ch = NextChar();
                switch (ch) {
                    case ' ':
                    case '\t':
                    case '\f':
                        continue;
                    case '#':
                    case EOF:
                    case '\n':
                        Backup();
                        break;
                    default:
                        ReportSyntaxError("invalid syntax");
                        Backup();
                        break;
                }
                break;
            }
        }

        private ErrorToken BadChar(int ch) {
            SetEnd();
            return new ErrorToken("bad character '" + (char)ch + "'");
        }

        private static bool IsNameStart(int ch) {
            return Char.IsLetter((char)ch) || ch == '_';
        }

        private static bool IsNamePart(int ch) {
            return Char.IsLetterOrDigit((char)ch) || ch == '_';
        }

        private static bool IsDigit(int ch) {
            return Char.IsDigit((char)ch);
        }

        private Token ReadString(char quote, bool isRaw, bool isUni) {
            int sadd = 0;
            bool isTriple = false;
            if (NextChar(quote)) {
                if (NextChar(quote)) {
                    isTriple = true; sadd += 3;
                } else {
                    Backup(); sadd++;
                }
            } else sadd++;

            if (isRaw) sadd++;
            if (isUni) sadd++;

            return ContinueString(quote, isRaw, isUni, isTriple, sadd);
        }

        /// <summary>
        /// Extracts a given line of text back out into a string for error reporting purposes
        /// </summary>
        public string GetRawLineForError(int lineNo) {
            int curLine = 1, curIndex = -1;

            if (lineNo != 1) {
                for (int i = 0; i < data.Length; i++) {
                    if (data[i] == '\r') {
                        if ((i + 1) < data.Length && data[i] == '\n') {
                            i++;
                        }
                        curLine++;
                        if (curLine == lineNo) {
                            curIndex = i + 1;
                            break;
                        }
                    } else if (data[i] == '\n') {
                        curLine++;
                        if (curLine == lineNo) {
                            curIndex = i + 1;
                            break;
                        }
                    }
                }

                if (curIndex == -1) return String.Empty;
            } else {
                curIndex = 0;
            }

            int endIndex = curIndex;
            for (; endIndex < data.Length; endIndex++) {
                if (data[endIndex] == '\r' || data[endIndex] == '\n') {
                    break;
                }
            }

            return new String(data, curIndex, endIndex - curIndex);
        }

        public Token ContinueString(char quote, bool isRaw, bool isUni, bool isTriple) {
            this.start = index;
            return ContinueString(quote, isRaw, isUni, isTriple, 0);
        }

        private Token ContinueString(char quote, bool isRaw, bool isUni, bool isTriple, int sadd) {
            bool complete = true;
            int eadd = 0;
            for (; ; ) {
                int ch = NextChar();
                if (ch == EOF) {
                    if (verbatim) { complete = !isTriple; break; }
                    SetEnd();
                    UnexpectedEndOfString(isTriple);
                    return new ErrorToken("<eof> while reading string");
                } else if (ch == quote) {
                    if (isTriple) {
                        if (NextChar(quote) && NextChar(quote)) {
                            eadd += 3; break;
                        }
                    } else {
                        eadd++; break;
                    }
                } else if (ch == '\\') {
                    int peek = PeekChar();
                    switch (peek) {
                        case '\\':
                        case '\n':
                        case '\r':
                            NextChar();
                            continue;
                        case EOF:
                            if (verbatim) { complete = false; break; }
                            SetEnd();
                            UnexpectedEndOfString(isTriple);
                            return new ErrorToken("EOF in single-quoted string");
                        default:
                            if (peek == quote) {
                                NextChar();
                            }
                            continue;
                    }
                    break;
                } else if (ch == '\n' || ch == '\r') {
                    if (!isTriple) {
                        if (verbatim) { complete = false; break; }
                        SetEnd();
                        UnexpectedEndOfString(isTriple);
                        return new ErrorToken("NEWLINE in single-quoted string");
                    }
                }
            }

            SetEnd();

            int end = this.end;
            if (end >= length) end = length;

            string contents = new string(data, start + sadd, end - start - (sadd + eadd));
            if (isTriple) {
                contents = contents.Replace("\r\n", "\n"); //!!! partially right, Mac is still ToDo
            }
            contents = LiteralParser.ParseString(contents, isRaw, isUni, complete);
            if (complete) {
                return new ConstantValueToken(contents);
            } else {
                return new IncompleteStringToken(contents, quote == '\'', isRaw, isUni, isTriple);
            }
        }

        private void UnexpectedEndOfString(bool isTriple) {
            if (isTriple) {
                ReportSyntaxError("EOF while scanning triple-quoted string", ErrorCodes.SyntaxError | ErrorCodes.IncompleteToken);
            } else {
                ReportSyntaxError("EOL while scanning single-quoted string");
            }
        }

        private Token ReadNumber(char start) {
            int b = 10;
            if (start == '0') {
                if (NextChar('x') || NextChar('X')) {
                    return ReadHexNumber();
                }
                b = 8;
            }
            while (true) {
                int ch = NextChar();
                switch (ch) {
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':
                        continue;
                    case '.': return ReadFloatPostDot();
                    case 'e': case 'E':
                        return ReadFloatPostE();
                    case 'j': case 'J':
                        SetEnd();
                        return new ConstantValueToken(LiteralParser.ParseImaginary(GetImage()));
                    case 'l': case 'L':
                        SetEnd();
                        return new ConstantValueToken(LiteralParser.ParseBigInteger(GetImage(), b));
                    default:
                        Backup();
                        SetEnd();
                        return new ConstantValueToken(ParseInteger(GetImage(), b));
                }
            }
        }

        private Token ReadHexNumber() {
            while (true) {
                int ch = NextChar();
                string s;
                switch (ch) {
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':
                    case 'a': case 'b': case 'c': case 'd': case 'e': case 'f':
                    case 'A': case 'B': case 'C': case 'D': case 'E': case 'F':
                        continue;
                    case 'l': case 'L':
                        SetEnd();
                        s = GetImage();
                        s = s.Substring(2, s.Length - 3);
                        return new ConstantValueToken(LiteralParser.ParseBigInteger(s, 16));
                    default:
                        Backup();
                        SetEnd();
                        s = GetImage();
                        s = s.Substring(2, s.Length - 2);
                        return new ConstantValueToken(ParseInteger(s, 16));
                }
            }
        }

        private Token ReadFloatPostDot() {
            while (true) {
                int ch = NextChar();
                switch (ch) {
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':
                        continue;
                    case 'e': case 'E':
                        return ReadFloatPostE();
                    case 'j': case 'J':
                        SetEnd();
                        return new ConstantValueToken(LiteralParser.ParseImaginary(GetImage()));
                    default:
                        Backup();
                        SetEnd();
                        return new ConstantValueToken(ParseFloat(GetImage()));
                }
            }
        }

        private Token ReadFloatPostE() {
            int ch = NextChar();

            if (ch == '-' || ch == '+') {
                ch = NextChar();
            }
            while (true) {
                switch (ch) {
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':
                        ch = NextChar();
                        continue;
                    case 'j': case 'J':
                        SetEnd();
                        return new ConstantValueToken(LiteralParser.ParseImaginary(GetImage()));
                    default:
                        Backup();
                        SetEnd();
                        return new ConstantValueToken(ParseFloat(GetImage()));
                }
            }
        }

        private Token ReadName(char first) {
            int ch = NextChar();
            while (IsNamePart(ch)) {
                ch = NextChar();
            }
            Backup();
            SetEnd();
            SymbolId name = SymbolTable.StringToId(new String(this.data, start, end - start));
            if (name == SymbolTable.None) return Tokens.NoneToken;

            Token ret;
            if (Tokens.Keywords.TryGetValue(name, out ret)) return ret;
            else return new NameToken(name);
        }

        private void SetNewLine(Location loc) {
            startLoc = loc;
            endLoc = loc;
            endLoc.column++;
        }

        private void SetStart() {
            start = index;
            startLoc.column = current.column;
            startLoc.line = current.line;
        }

        private void SetEnd() {
            end = index;
            endLoc.column = current.column;
            endLoc.line = current.line;
        }

        private void SetEnd(int revert) {
            end = index - revert;
            endLoc.column = current.column - revert;
            endLoc.line = current.line;
        }

        public int GroupingLevel {
            get {
                return parenLevel + braceLevel + bracketLevel;
            }
        }

        private bool InGrouping() {
            return parenLevel != 0 || braceLevel != 0 || bracketLevel != 0;
        }

        private void SetIndent(int spaces, StringBuilder chars) {
            int current = indent[indentLevel];
            if (spaces == current) {
                return;
            } else if (spaces > current) {
                indent[++indentLevel] = spaces;
                if (indentFormat != null)
                    indentFormat[indentLevel] = chars;
                pendingDedents = -1;
                return;
            } else {
                while (spaces < current) {
                    
                    indentLevel -= 1;
                    pendingDedents += 1;
                    current = indent[indentLevel];                    
                }
                if (spaces != current) {
                    ReportSyntaxError("unindent does not match any outer indentation level on line " + this.current.line.ToString(), ErrorCodes.IndentationError);
                }
            }
        }

        private Token ReadNewline() {
            // Check whether we're currently scanning for inconsistent use of identation characters. If
            // we are we'll switch to using a slower version of this method with the extra checks embedded.
            if (CheckingForIndentationInconsistencies)
                return ReadNewlineWithChecks();

            Location newLine = startLoc;
            int spaces = 0;
            while (true) {
                int ch = NextChar();
                switch (ch) {
                    case ' ': spaces += 1; continue;
                    case '\t': spaces += 8-(spaces % 8); continue;
                    case '\f': spaces += 1; continue;
                    case '\n': spaces = 0; continue;
                    case '#': ReadToEol(); spaces = 0; continue;
                    case EOF:
                        SetIndent(0, null);
                        return Tokens.NewlineToken;
                    default:
                        if (InGrouping()) {
                            Backup();
                            return Next();
                        }

                        SetIndent(spaces, null);
                        Backup();
                        SetNewLine(newLine);
                        return Tokens.NewlineToken;
                }
            }
        }

        // This is another version of ReadNewline with nearly identical semantics. The difference is
        // that checks are made to see that indentation is used consistently. This logic is in a
        // duplicate method to avoid inflicting the overhead of the extra logic when we're not making
        // the checks.
        private Token ReadNewlineWithChecks() {
            // Keep track of the indentation format for the current line
            StringBuilder sb = new StringBuilder(80);

            Location newLine = startLoc;
            int spaces = 0;
            while (true) {
                int ch = NextChar();
                switch (ch) {
                    case ' ': spaces += 1; sb.Append(' '); continue;
                    case '\t': spaces += 8-(spaces%8); sb.Append('\t'); continue;
                    case '\f': spaces += 1; sb.Append('\f'); continue;
                    case '\n': spaces = 0; sb.Length = 0; continue;
                    case '#': ReadToEol(); spaces = 0; sb.Length = 0; continue;
                    case EOF:
                        SetIndent(0, null);
                        return Tokens.NewlineToken;
                    default:
                        if (InGrouping()) {
                            Backup();
                            return Next();
                        }

                        // We've captured a line of significant identation (i.e. not pure whitespace).
                        // Check that any of this indentation that's in common with the current indent
                        // level is constructed in exactly the same way (i.e. has the same mix of spaces
                        // and tabs etc.).
                        if (indent[indentLevel] > 0) {
                            StringBuilder previousIndent = indentFormat[indentLevel];
                            int checkLength = previousIndent.Length < sb.Length ? previousIndent.Length : sb.Length;
                            for (int i = 0; i < checkLength; i++)
                                if (sb[i] != previousIndent[i]) {
                                    // We've hit a difference in the way we're indenting, report it.
                                    string message = String.Format("inconsistent use of tabs and spaces in indentation ({0}, line {1})", context.SourceFile, current.line);
                                    if (Options.ErrorOnIndentationInconsistency) {
                                        context.AddError("inconsistent use of tabs and spaces in indentation",
                                            GetRawLineForError(current.line),
                                            current.line, current.column, current.line, current.column,
                                            ErrorCodes.TabError, Severity.Error);
                                    }

                                    Ops.PrintWithDest(systemState, systemState.stderr, message);
                                    // We only report problems once per module, so switch back to the fast
                                    // algorithm.
                                    indentFormat = null;
                                }
                        }

                        SetIndent(spaces, sb);

                        Backup();
                        SetNewLine(newLine);
                        return Tokens.NewlineToken;
                }
            }
        }

        private void ReadToEol() {
            while (true) {
                int ch = NextChar();
                switch (ch) {
                    case '\n': case EOF: return;
                }
            }
        }

        private Token ReadEolComment() {
            StringBuilder comment = null;
            if (verbatim) comment = new StringBuilder();
            while (true) {
                int ch = NextChar();
                switch (ch) {
                    case '\n':
                        SetEnd();
                        if (verbatim) {
                            return new CommentToken(comment.ToString());
                        } else {
                            return ReadNewline();
                        }

                    case EOF:
                        SetEnd(1);
                        if (verbatim) {
                            return new CommentToken(comment.ToString());
                        } else {
                            return ReadEof();
                        }
                    default:
                        if (verbatim) comment.Append((char)ch);
                        break;
                }
            }
        }

        private Token ReadEof() {
            if (pendingNewlines-- > 0) {
            // if the input doesn't end in a '\n' or we're in an indented block then add a newline to the end
            //if (pendingNewlines-- > 0 && (indentLevel > 0 || data[length-1] != '\n')) {
                return ReadNewline();
            } else {
                return Tokens.EofToken;
            }
        }

        private object ParseInteger(string s, int radix) {
            try {
                return LiteralParser.ParseInteger(s, radix);
            } catch (ArgumentException e) {
                ReportSyntaxError(e.Message);
            }
            return 0;
        }

        private object ParseFloat(string s) {
            try {
                return LiteralParser.ParseFloat(s);
            } catch (Exception e) {
                ReportSyntaxError(e.Message);
                return 0.0;
            }
        }

        private void ReportSyntaxError(string message) {
            ReportSyntaxError(message, ErrorCodes.SyntaxError);
        }

        private void ReportSyntaxError(string message, int errorCode) {
            Debug.Assert(context != null);
            string lineText = GetRawLineForError(startLoc.line);
            context.AddError(message, lineText, startLoc.line, startLoc.column, endLoc.line, endLoc.column, errorCode, Severity.Error);
        }
    }
}
