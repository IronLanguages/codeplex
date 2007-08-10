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
//#define DUMP_TOKENS

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
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using IronPython.Runtime;
using IronPython.Hosting;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler {

    // TODO: rename states
    public enum LexicalState {
        EXPR_FNAME,
        EXPR_BEG,
        EXPR_END,
        EXPR_ENDARG,
        Initial = EXPR_BEG,
    }

    /// <summary>
    /// IronPython tokenizer
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tokenizer")]
    public sealed partial class Tokenizer { 
        private const int EOF = -1;
        private const int MaxIndent = 80;

        private const int DefaultBufferCapacity = 1024;

        [Serializable] 
        private struct State {
            // indentation state
            public int[] Indent;
            public int IndentLevel;
            public int PendingDedents;
            public int PendingNewlines;

            // TODO: remember incomplete tokens

            // Indentation state used only when we're reporting on inconsistent identation format.
            public StringBuilder[] IndentFormat;

            // grouping state
            public int ParenLevel, BraceLevel, BracketLevel;

            public State(State state) {
                Indent = (int[])state.Indent.Clone();
                PendingNewlines = state.PendingNewlines;
                BracketLevel = state.BraceLevel;
                ParenLevel = state.ParenLevel;
                BraceLevel = state.BraceLevel;
                PendingDedents = state.PendingDedents;
                IndentLevel = state.IndentLevel;
                IndentFormat = (state.IndentFormat != null) ? (StringBuilder[])state.IndentFormat.Clone() : null;
            }

            public State(object dummy) {
                Indent = new int[MaxIndent]; // TODO
                PendingNewlines = 1;
                BracketLevel = ParenLevel = BraceLevel = PendingDedents = IndentLevel = 0;
                IndentFormat = null;
            }
        }

        private State _state;

        // tokenizer properties:
        private readonly bool _verbatim;
        private SourceUnit _sourceUnit;
        private TokenizerBuffer _buffer;
        private ErrorSink _errors;
        private Severity _indentationInconsistencySeverity;

        public object CurrentState {
            get {
                return _state;
            }
        }

        public SourceUnit SourceUnit {
            get {
                return _sourceUnit;
            }
        }

        public ErrorSink Errors { 
            get { return _errors; } 
            set {
                if (value == null) throw new ArgumentNullException("value");
                _errors = value; 
            } 
        }
        
        public Severity IndentationInconsistencySeverity {
            get { return _indentationInconsistencySeverity; }
            set { 
                _indentationInconsistencySeverity = value;

                if (value != Severity.Ignore && _state.IndentFormat == null) {
                    _state.IndentFormat = new StringBuilder[MaxIndent];
                }
            }
        }

        public bool IsEndOfFile {
            get {
                return _buffer.Peek() == EOF;
            }
        }

        public SourceLocation TokenStart {
            get {
                return _buffer.TokenStart;
            }
        }

        public SourceLocation TokenEnd {
            get {
                return _buffer.TokenEnd;
            }
        }

        public SourceSpan TokenSpan {
            get {
                return _buffer.TokenSpan;
            }
        }

        public Tokenizer(ErrorSink errorSink)
            : this(errorSink, false) {
        }

        public Tokenizer(ErrorSink errorSink, bool verbatim) {
            if (errorSink == null) throw new ArgumentNullException("errorSink");

            _errors = errorSink;
            _verbatim = verbatim;
            _state = new State(null);
            _sourceUnit = null;
            _buffer = null;
        }

        public void Initialize(SourceUnit sourceUnit) {
            if (sourceUnit == null) throw new ArgumentNullException("sourceUnit");

            Initialize(null, sourceUnit.GetReader(), SourceLocation.MinValue, DefaultBufferCapacity);
        }

        public void Initialize(object state, SourceUnitReader sourceReader, SourceLocation initialLocation) {
            Initialize(state, sourceReader, initialLocation, DefaultBufferCapacity);
        }

        public void Initialize(object state, SourceUnitReader sourceReader, SourceLocation initialLocation, int bufferCapacity) {
            if (sourceReader == null) throw new ArgumentNullException("sourceReader");

            if (state != null) {
                if (!(state is State)) throw new ArgumentException();
                _state = new State((State)state);
            } else {
                _state = new State(null);
            }

            _sourceUnit = sourceReader.SourceUnit;

            // TODO: we can reuse the buffer if there is enough free space:
            _buffer = new TokenizerBuffer(sourceReader, initialLocation, bufferCapacity, !_sourceUnit.DisableLineFeedLineSeparator);

            DumpBeginningOfUnit();
        }

        public int ReadToken() {

            if (_buffer == null) {
                throw new InvalidOperationException("Uninitialized");
            }

            return (int)GetNextToken().Kind;
        }

        internal bool TokenStringEquals(string str) {
            if (_buffer.TokenLength != str.Length) 
                return false;
            
            for (int i = 0; i < _buffer.TokenLength; i++) {
                if (_buffer.GetChar(i) != str[i])
                    return false;
            }
            return true;
        }

        private bool NextChar(int ch) {
            return _buffer.Read(ch);
        }

        private int NextChar() {
            return _buffer.Read();
        }

        public Token GetNextToken() {
            Debug.Assert(_buffer != null && _sourceUnit != null, "Uninitialized");
            Token result;

            if (_state.PendingDedents != 0) {
                if (_state.PendingDedents == -1) {
                    _state.PendingDedents = 0;
                    result = Tokens.IndentToken;
                } else {
                    _state.PendingDedents--;
                    result = Tokens.DedentToken;
                }
            } else {
                result = Next();
            }

            DumpToken(result);
            return result;
        }

        private Token Next() {
            bool at_beginning = _buffer.AtBeginning;
            _buffer.DiscardToken();
            int ch = NextChar();
                    
            while (true) {
                switch (ch) {
                    case EOF:
                        return ReadEof();

                    case ' ': case '\t': case '\f':
                        ch = SkipWhiteSpace(at_beginning);
                        break;

                    case '#':
                        if (_verbatim)
                            return ReadSingleLineComment();
                        
                        ch = SkipSingleLineComment();
                        break;

                    case '\\':
                        if (_buffer.ReadEolnOpt(NextChar()) > 0) {
                            // discard token '\\<eoln>':
                            _buffer.DiscardToken();

                            ch = NextChar();
                            break;

                        } else {
                            _buffer.Back();
                            goto default;
                        }
                    
                    case '\"': case '\'':
                        return ReadString((char)ch, false, false);

                    case 'u': case 'U':
                        return ReadNameOrUnicodeString();

                    case 'r': case 'R':
                        return ReadNameOrRawString();

                    case '_':
                        return ReadName();

                    case '.':
                        ch = _buffer.Peek();
                        if (ch >= '0' && ch <= '9')
                            return ReadFraction();

                        _buffer.MarkSingleLineTokenEnd();
                        return Tokens.DotToken;

                    case '0': case '1': case '2': case '3': case '4': 
                    case '5': case '6': case '7': case '8': case '9':
                        return ReadNumber(ch);

                    default:

                        if (_buffer.ReadEolnOpt(ch) > 0) {
                            // token marked by the callee:
                            if (ReadNewline()) return Tokens.NewLineToken;

                            // ignore end-of-line and whitespace:
                            _buffer.DiscardToken();
                            ch = NextChar();
                            break;
                        }
                        
                        Token res = NextOperator(ch);
                        if (res != null) {
                            _buffer.MarkSingleLineTokenEnd();
                            return res;
                        }

                        if (IsNameStart(ch)) return ReadName();

                        _buffer.MarkSingleLineTokenEnd();
                        return BadChar(ch);
                }
            }
        }

        private int SkipWhiteSpace(bool atBeginning) {
            int ch;
            do { ch = NextChar(); } while (ch == ' ' || ch == '\t' || ch == '\f');

            _buffer.Back();

            if (atBeginning && ch != '#' && ch != EOF && !_buffer.IsEoln(ch)) {
                _buffer.MarkSingleLineTokenEnd();
                ReportSyntaxError(_buffer.TokenSpan, Resources.InvalidSyntax, ErrorCodes.SyntaxError);
            }

            _buffer.DiscardToken();
            _buffer.SeekRelative(+1);
            return ch;
        }

        private int SkipSingleLineComment() {
            // do single-line comment:
            int ch = _buffer.ReadLine();
            _buffer.MarkSingleLineTokenEnd();

            // discard token '# ...':
            _buffer.DiscardToken();
            _buffer.SeekRelative(+1);

            return ch;
        }

        private Token ReadSingleLineComment() {
            // do single-line comment:
            _buffer.ReadLine();
            _buffer.MarkSingleLineTokenEnd();

            return new CommentToken(_buffer.GetTokenString());
        }

        private Token ReadNameOrUnicodeString() {
            if (NextChar('\"')) return ReadString('\"', false, true);
            if (NextChar('\'')) return ReadString('\'', false, true);
            if (NextChar('r') || NextChar('R')) {
                if (NextChar('\"')) return ReadString('\"', true, true);
                if (NextChar('\'')) return ReadString('\'', true, true);
                _buffer.Back();
            }
            return ReadName();
        }

        private Token ReadNameOrRawString() {
            if (NextChar('\"')) return ReadString('\"', true, false);
            if (NextChar('\'')) return ReadString('\'', true, false);
            return ReadName();
        }

        private Token ReadEof() {
            _buffer.MarkSingleLineTokenEnd();

            if (_state.PendingNewlines-- > 0) {
                // if the input doesn't end in a EOLN or we're in an indented block then add a newline to the end
                SetIndent(0, null);
                return Tokens.NewLineToken;
            } else {
                return Tokens.EndOfFileToken;
            }
        }

        private ErrorToken BadChar(int ch) {
            return new ErrorToken(StringUtils.AddSlashes(((char)ch).ToString()));
        }

        private static bool IsNameStart(int ch) {
            return Char.IsLetter((char)ch) || ch == '_';
        }

        private static bool IsNamePart(int ch) {
            return Char.IsLetterOrDigit((char)ch) || ch == '_';
        }

        private Token ReadString(char quote, bool isRaw, bool isUni) {
            int sadd = 0;
            bool isTriple = false;

            if (NextChar(quote)) {
                if (NextChar(quote)) {
                    isTriple = true; sadd += 3;
                } else {
                    _buffer.Back();
                    sadd++;
                }
            } else {
                sadd++;
            }

            if (isRaw) sadd++;
            if (isUni) sadd++;

            return ContinueString(quote, isRaw, isUni, isTriple, sadd);
        }

        private Token ContinueString(char quote, bool isRaw, bool isUnicode, bool isTriple, int startAdd) {
            bool complete = true;
            bool multi_line = false;
            int end_add = 0;
            int eol_size = 0;

            for (; ; ) {
                int ch = NextChar();

                if (ch == EOF) {

                    if (_verbatim) { 
                        complete = !isTriple; 
                        break; 
                    }

                    _buffer.MarkTokenEnd(multi_line);
                    UnexpectedEndOfString(isTriple, isTriple);
                    return new ErrorToken(Resources.EofInString);

                } else if (ch == quote) {

                    if (isTriple) {
                        if (NextChar(quote) && NextChar(quote)) {
                            end_add += 3; 
                            break;
                        }
                    } else {
                        end_add++; 
                        break;
                    }

                } else if (ch == '\\') {

                    ch = NextChar();

                    if (ch == EOF) {
                        _buffer.Back();
                        
                        if (_verbatim) { 
                            complete = false; 
                            break; 
                        }

                        _buffer.MarkTokenEnd(multi_line);
                        UnexpectedEndOfString(isTriple, isTriple);
                        return new ErrorToken(Resources.EofInString);

                    } else if ((eol_size = _buffer.ReadEolnOpt(ch)) > 0) {

                        // skip \<eoln> unless followed by EOF:
                        if (_buffer.Peek() == EOF) {

                            // backup over the eoln:
                            _buffer.SeekRelative(-eol_size);
                        
                            // incomplete string in the form "abc\
                            _buffer.MarkTokenEnd(multi_line);
                            UnexpectedEndOfString(isTriple, true);
                            return new ErrorToken(Resources.EofInString);
                        }

                        multi_line = true;
                        
                    } else if (ch != quote && ch != '\\') {
                        _buffer.Back();
                    }

                } else if ((eol_size = _buffer.ReadEolnOpt(ch)) > 0) {
                    if (!isTriple) {

                        // backup over the eoln:
                        _buffer.SeekRelative(-eol_size);
                        
                        if (_verbatim) {
                            complete = false;
                            break;
                        }

                        _buffer.MarkTokenEnd(multi_line);
                        UnexpectedEndOfString(isTriple, false);
                        return new ErrorToken((quote == '"') ? Resources.NewLineInDoubleQuotedString : Resources.NewLineInSingleQuotedString);
                    }

                    multi_line = true;
                }
            }

            _buffer.MarkTokenEnd(multi_line);
            
            // TODO: do not create a string, parse in place
            string contents = _buffer.GetTokenSubstring(startAdd, _buffer.TokenLength - startAdd - end_add); //.Substring(_start + startAdd, end - _start - (startAdd + eadd));

            // EOLN should be normalized to '\n' in triple-quoted strings:
            // TODO: do this better
            if (multi_line && isTriple && !_sourceUnit.DisableLineFeedLineSeparator) {
                contents = contents.Replace("\r\n", "\n").Replace("\r", "\n");
            }

            contents = LiteralParser.ParseString(contents, isRaw, isUnicode, complete);
            if (complete) {
                return new ConstantValueToken(contents);
            } else {
                return new IncompleteStringToken(contents, quote == '\'', isRaw, isUnicode, isTriple);
            }
        }

        private void UnexpectedEndOfString(bool isTriple, bool isIncomplete) {
            string message = isTriple ? Resources.EofInTripleQuotedString : Resources.EolInSingleQuotedString;
            int error = isIncomplete ? ErrorCodes.SyntaxError | ErrorCodes.IncompleteToken : ErrorCodes.SyntaxError;

            ReportSyntaxError(_buffer.TokenSpan, message, error);
        }

        private Token ReadNumber(int start) {
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
                    case '.': 
                        return ReadFraction();

                    case 'e': case 'E':
                        return ReadExponent();

                    case 'j': case 'J':
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseImaginary(_buffer.GetTokenString()));

                    case 'l': case 'L':
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseBigInteger(_buffer.GetTokenString(), b));

                    case '0': case '1': case '2': case '3': case '4': 
                    case '5': case '6': case '7': case '8': case '9':
                        break;

                    default:
                        _buffer.Back();
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(ParseInteger(_buffer.GetTokenString(), b));
                }
            }
        }

        private Token ReadHexNumber() {
            while (true) {
                int ch = NextChar();

                switch (ch) {
                    case '0': case '1': case '2': case '3': case '4': 
                    case '5': case '6': case '7': case '8': case '9':
                    case 'a': case 'b': case 'c': case 'd': case 'e': case 'f':
                    case 'A': case 'B': case 'C': case 'D': case 'E': case 'F':
                        break;

                    case 'l': case 'L':
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseBigInteger(_buffer.GetTokenSubstring(2, _buffer.TokenLength - 3), 16));

                    default:
                        _buffer.Back();
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(ParseInteger(_buffer.GetTokenSubstring(2, _buffer.TokenLength - 2), 16));
                }
            }
        }

        private Token ReadFraction() {
            while (true) {
                int ch = NextChar();

                switch (ch) {
                    case '0': case '1': case '2': case '3': case '4': 
                    case '5': case '6': case '7': case '8': case '9':
                        break;

                    case 'e': case 'E':
                        return ReadExponent();

                    case 'j': case 'J':
                        _buffer.MarkSingleLineTokenEnd(); 

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseImaginary(_buffer.GetTokenString()));

                    default:
                        _buffer.Back();
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(ParseFloat(_buffer.GetTokenString()));
                }
            }
        }

        private Token ReadExponent() {
            int ch = NextChar();

            if (ch == '-' || ch == '+') {
                ch = NextChar();
            }

            while (true) {
                switch (ch) {
                    case '0': case '1': case '2': case '3': case '4': 
                    case '5': case '6': case '7': case '8': case '9':
                        ch = NextChar();
                        break;

                    case 'j': case 'J':
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseImaginary(_buffer.GetTokenString()));

                    default:
                        _buffer.Back();
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(ParseFloat(_buffer.GetTokenString()));
                }
            }
        }

        private Token ReadName() {
            int ch;
            
            do { ch = NextChar(); } while (IsNamePart(ch));
            _buffer.Back();

            _buffer.MarkSingleLineTokenEnd();

            SymbolId name = SymbolTable.StringToId(_buffer.GetTokenString());
            if (name == Symbols.None) return Tokens.NoneToken;

            Token result;
            if (Tokens.Keywords.TryGetValue(name, out result)) return result;

            return new NameToken(name);
        }

        public int GroupingLevel {
            get {
                return _state.ParenLevel + _state.BraceLevel + _state.BracketLevel;
            }
        }

        /// <summary>
        /// Returns whether the 
        /// </summary>
        private bool ReadNewline() {
            // Check whether we're currently scanning for inconsistent use of identation characters. If
            // we are we'll switch to using a slower version of this method with the extra checks embedded.
            if (IndentationInconsistencySeverity != Severity.Ignore)
                return ReadNewlineWithChecks();

            int spaces = 0;
            while (true) {
                int ch = NextChar();

                switch (ch) {
                    case ' ': spaces += 1; break;
                    case '\t': spaces += 8 - (spaces % 8); break;
                    case '\f': spaces += 1; break;
                    
                    case '#':
                        if (_verbatim) {
                            _buffer.Back();
                            _buffer.MarkMultiLineTokenEnd();
                            return true;
                        } else {
                            ch = _buffer.ReadLine();
                            break;
                        }

                    case EOF:
                        _buffer.MarkMultiLineTokenEnd();
                        SetIndent(0, null);
                        return true;

                    default:

                        if (_buffer.ReadEolnOpt(ch) > 0) {
                            spaces = 0;
                            break;
                        }

                        _buffer.Back();

                        if (GroupingLevel > 0) {
                            return false;
                        }

                        _buffer.MarkMultiLineTokenEnd();
                        
                        SetIndent(spaces, null);
                        
                        return true;
                }
            }
        }

        // This is another version of ReadNewline with nearly identical semantics. The difference is
        // that checks are made to see that indentation is used consistently. This logic is in a
        // duplicate method to avoid inflicting the overhead of the extra logic when we're not making
        // the checks.
        private bool ReadNewlineWithChecks() {
            // Keep track of the indentation format for the current line
            StringBuilder sb = new StringBuilder(80);

            int spaces = 0;
            while (true) {
                int ch = NextChar();

                switch (ch) {
                    case ' ': spaces += 1; sb.Append(' '); break;
                    case '\t': spaces += 8 - (spaces % 8); sb.Append('\t'); break;
                    case '\f': spaces += 1; sb.Append('\f'); break;
                    
                    case '#':
                        if (_verbatim) {
                            _buffer.Back();
                            _buffer.MarkMultiLineTokenEnd();
                            return true;
                        } else {
                            ch = _buffer.ReadLine();
                            break;
                        }
                   
                    case EOF:
                        _buffer.MarkMultiLineTokenEnd();
                        SetIndent(0, null);
                        return true;
                    
                    default:
                        if (_buffer.ReadEolnOpt(ch) > 0) {
                            spaces = 0; 
                            sb.Length = 0;
                            break;
                        }

                        if (GroupingLevel > 0) {
                            return false;
                        }

                        _buffer.Back();
                        _buffer.MarkMultiLineTokenEnd();
                        
                        // We've captured a line of significant identation (i.e. not pure whitespace).
                        // Check that any of this indentation that's in common with the current indent
                        // level is constructed in exactly the same way (i.e. has the same mix of spaces
                        // and tabs etc.).
                        CheckIndent(sb);

                        SetIndent(spaces, sb);

                        return true;
                }
            }
        }

        private void CheckIndent(StringBuilder sb) {
            if (_state.Indent[_state.IndentLevel] > 0) {
                StringBuilder previousIndent = _state.IndentFormat[_state.IndentLevel];
                int checkLength = previousIndent.Length < sb.Length ? previousIndent.Length : sb.Length;
                for (int i = 0; i < checkLength; i++) {
                    if (sb[i] != previousIndent[i]) {

                        SourceLocation eoln_token_end = _buffer.TokenEnd;
                        
                        // We've hit a difference in the way we're indenting, report it.
                        _errors.Add(_sourceUnit, Resources.InconsistentWhitespace,
                            new SourceSpan(eoln_token_end, eoln_token_end), // TODO: we can report better span - starting at the beginning of the line
                            ErrorCodes.TabError, _indentationInconsistencySeverity
                        );

                        // We only report problems once per module, so switch back to the fast algorithm.
                        _indentationInconsistencySeverity = Severity.Ignore;
                    }
                }
            }
        }

        private void SetIndent(int spaces, StringBuilder chars) {
            int current = _state.Indent[_state.IndentLevel];
            if (spaces == current) {
                return;
            } else if (spaces > current) {
                _state.Indent[++_state.IndentLevel] = spaces;
                if (_state.IndentFormat != null)
                    _state.IndentFormat[_state.IndentLevel] = chars;
                _state.PendingDedents = -1;
                return;
            } else {
                while (spaces < current) {

                    _state.IndentLevel -= 1;
                    _state.PendingDedents += 1;
                    current = _state.Indent[_state.IndentLevel];
                }

                if (spaces != current) {
                    ReportSyntaxError(new SourceSpan(_buffer.TokenEnd, _buffer.TokenEnd),
                        Resources.IndentationMismatch, ErrorCodes.IndentationError);
                }
            }
        }

        private object ParseInteger(string s, int radix) {
            try {
                return LiteralParser.ParseInteger(s, radix);
            } catch (ArgumentException e) {
                ReportSyntaxError(_buffer.TokenSpan, e.Message, ErrorCodes.SyntaxError);
            }
            return 0;
        }

        private object ParseFloat(string s) {
            try {
                return LiteralParser.ParseFloat(s);
            } catch (Exception e) {
                ReportSyntaxError(_buffer.TokenSpan, e.Message, ErrorCodes.SyntaxError);
                return 0.0;
            }
        }

        internal static bool TryGetEncoding(Encoding defaultEncoding, string line, ref Encoding enc) {
            // encoding is "# coding: <encoding name>
            // minimum length is 18
            if (line.Length < 10) return false;
            if (line[0] != '#') return false;

            // we have magic comment line
            int codingIndex;
            if ((codingIndex = line.IndexOf("coding")) == -1) return false;
            if (line.Length <= (codingIndex + 6)) return false;
            if (line[codingIndex + 6] != ':' && line[codingIndex + 6] != '=') return false;

            // it contains coding: or coding=
            int encodingStart = codingIndex + 7;
            while (encodingStart < line.Length) {
                if (!Char.IsWhiteSpace(line[encodingStart])) break;

                encodingStart++;
            }

            // line is coding: [all white space]
            if (encodingStart == line.Length) return false;

            int encodingEnd = encodingStart;
            while (encodingEnd < line.Length) {
                if (Char.IsWhiteSpace(line[encodingEnd])) break;

                encodingEnd++;
            }

            // get the encoding string name
            string encName = line.Substring(encodingStart, encodingEnd - encodingStart);

            // and we have the magic ending as well...
            return StringOps.TryGetEncoding(defaultEncoding, encName, out enc);
        }

        private void ReportSyntaxError(SourceSpan span, string message, int errorCode) {
            _errors.Add(_sourceUnit, message, span, errorCode, Severity.Error);
        }

        [Conditional("DUMP_TOKENS")]
        private void DumpBeginningOfUnit() {
            Console.WriteLine("--- Source unit: '{0}' ---", _sourceUnit.Name);
        }

        [Conditional("DUMP_TOKENS")]
        private void DumpToken(Token token) {
            Console.WriteLine("{0} `{1}`", token.Kind, token.Image.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"));
        }

#if !SILVERLIGHT
        public static TimeSpan Benchmark(string code) {
            Tokenizer t = new Tokenizer(new ErrorSink());
            t.Initialize(new SourceCodeUnit(PythonEngine.CurrentEngine, code));

            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (t.Next() != Tokens.EndOfFileToken) ;
            watch.Stop();

            return watch.Elapsed;
        }
#endif
    }
}
