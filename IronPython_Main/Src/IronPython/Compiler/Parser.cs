/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Hosting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;

using IronPython.Compiler.Ast;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler {

    /// <summary>
    /// Summary description for Parser.
    /// </summary>
    internal class Parser : IDisposable { // TODO: remove IDisposable
        private class TokenizerErrorSink : ErrorSink {
            private readonly Parser _parser;

            public TokenizerErrorSink(Parser parser) {
                Assert.NotNull(parser);
                _parser = parser;
            }

            public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity) {
                if (_parser._errorCode == 0) {
                    _parser._errorCode = errorCode;
                }
                _parser.ErrorSink.Add(sourceUnit, message, span, errorCode, severity);
            }
        }

        // immutable properties:
        private readonly Tokenizer _tokenizer;

        // mutable properties:
        private ErrorSink _errors;
        private ParserSink _sink;

        // resettable properties:
        private SourceUnit _sourceUnit;

        /// <summary>
        /// Language features initialized on parser construction and possibly updated during parsing. 
        /// The code can set the language features (e.g. "from __future__ import division").
        /// </summary>
        private PythonLanguageFeatures _languageFeatures;

        // state:
        private TokenWithSpan _token;
        private TokenWithSpan _lookahead;
        private Stack<FunctionDefinition> _functions;
        private bool _fromFutureAllowed;
        private string _privatePrefix;
        private bool _parsingStarted, _allowIncomplete;
        private SourceUnitReader _sourceReader;
        private int _errorCode;

        public ErrorSink ErrorSink {
            get {
                return _errors;
            }
            set {
                Contract.RequiresNotNull(value, "value");
                _errors = value;
            }
        }

        public ParserSink ParserSink {
            get {
                return _sink;
            }
            set {
                Contract.RequiresNotNull(value, "value");
                _sink = value;
            }
        }

        public int ErrorCode {
            get { return _errorCode; }
        }

        private bool AllowWithStatement {
            get { return (_languageFeatures & PythonLanguageFeatures.AllowWithStatement) == PythonLanguageFeatures.AllowWithStatement; }
        }

        private bool TrueDivision {
            get { return (_languageFeatures & PythonLanguageFeatures.TrueDivision) == PythonLanguageFeatures.TrueDivision; }
        }

        #region Construction

        private Parser(Tokenizer tokenizer, ErrorSink errorSink, ParserSink parserSink, PythonLanguageFeatures languageFeatures) {
            Contract.RequiresNotNull(tokenizer, "tokenizer");
            Contract.RequiresNotNull(errorSink, "errorSink");
            Contract.RequiresNotNull(parserSink, "parserSink");

            tokenizer.Errors = new TokenizerErrorSink(this);

            _tokenizer = tokenizer;
            _errors = errorSink;
            _sink = parserSink;

            Reset(tokenizer.SourceUnit, languageFeatures);
        }

        public static Parser CreateParser(CompilerContext context, PythonEngineOptions options) {
            return CreateParser(context, options, false);
        }

        public static Parser CreateParser(CompilerContext context, PythonEngineOptions options, bool verbatim) {
            return CreateParser(context, options, false, true);
        }

        public static Parser CreateParser(CompilerContext context, PythonEngineOptions options, bool verbatim, bool implyDedent) {
            Contract.RequiresNotNull(context, "context");
            Contract.RequiresNotNull(options, "options");

            PythonCompilerOptions compilerOptions = context.Options as PythonCompilerOptions;
            if (options == null) {
                throw new ArgumentException(Resources.PythonContextRequired);
            }

            SourceUnitReader reader;

            try {
                reader = context.SourceUnit.GetReader();

                if (compilerOptions.SkipFirstLine) {
                    reader.ReadLine();
                }
            } catch (IOException e) {
                context.Errors.Add(context.SourceUnit, e.Message, SourceSpan.Invalid, 0, Severity.Error);
                throw;
            }

            Tokenizer tokenizer = new Tokenizer(context.Errors, verbatim, implyDedent);
            tokenizer.Initialize(null, reader, SourceLocation.MinValue);
            tokenizer.IndentationInconsistencySeverity = options.IndentationInconsistencySeverity;

            Parser result = new Parser(tokenizer, context.Errors, context.ParserSink, compilerOptions.LanguageFeatures);
            result._sourceReader = reader;
            return result;
        }

        #endregion

        public void Reset(SourceUnit sourceUnit, PythonLanguageFeatures languageFeatures) {
            Contract.RequiresNotNull(sourceUnit, "sourceUnit");

            _sourceUnit = sourceUnit;
            _languageFeatures = languageFeatures;
            _token = new TokenWithSpan();
            _lookahead = new TokenWithSpan();
            _fromFutureAllowed = true;
            _functions = null;
            _privatePrefix = null;

            _parsingStarted = false;
            _errorCode = 0;
        }

        public void Reset() {
            Reset(_sourceUnit, _languageFeatures);
        }

        private void StartParsing() {
            if (_parsingStarted)
                throw new InvalidOperationException("Parsing already started. Use Restart to start again.");

            _parsingStarted = true;

            FetchLookahead();
        }

        private SourceLocation GetEnd() {
            Debug.Assert(_token.Token != null, "No token fetched");
            return _token.Span.End;
        }

        private SourceLocation GetStart() {
            Debug.Assert(_token.Token != null, "No token fetched");
            return _token.Span.Start;
        }

        private SourceSpan GetSpan() {
            Debug.Assert(_token.Token != null, "No token fetched");
            return _token.Span;
        }

        private Token NextToken() {
            _token = _lookahead;
            FetchLookahead();
            return _token.Token;
        }

        private Token PeekToken() {
            return _lookahead.Token;
        }

        private void FetchLookahead() {
            _lookahead.Token = _tokenizer.GetNextToken();
            _lookahead.Span = _tokenizer.TokenSpan;
        }

        private bool PeekToken(TokenKind kind) {
            return PeekToken().Kind == kind;
        }

        private bool PeekToken(Token check) {
            return PeekToken() == check;
        }

        private bool PeekName(SymbolId id) {
            NameToken t = PeekToken() as NameToken;
            if (t != null && t.Name == id)
                return true;
            return false;
        }

        private bool Eat(TokenKind kind) {
            if (NextToken().Kind != kind) {
                ReportSyntaxError(_token);
                return false;
            }
            return true;
        }

        private bool EatNoEof(TokenKind kind) {
            if (NextToken().Kind != kind) {
                ReportSyntaxError(_token.Token, _token.Span, ErrorCodes.SyntaxError, false);
                return false;
            }
            return true;
        }

        private bool MaybeEat(TokenKind kind) {
            if (PeekToken().Kind == kind) {
                NextToken();
                return true;
            } else {
                return false;
            }
        }

        private void SkipName(SymbolId id) {
            Debug.Assert(PeekName(id));
            NextToken();
        }

        #region Error Reporting

        private void ReportSyntaxError(TokenWithSpan t) {
            ReportSyntaxError(t, ErrorCodes.SyntaxError);
        }

        private void ReportSyntaxError(TokenWithSpan t, int errorCode) {
            ReportSyntaxError(t.Token, t.Span, errorCode, true);
        }

        private void ReportSyntaxError(Token t, SourceSpan span, int errorCode, bool allowIncomplete) {
            SourceLocation start = span.Start;
            SourceLocation end = span.End;
            if (t.Kind == TokenKind.NewLine || t.Kind == TokenKind.Dedent) {
                if (_tokenizer.IsEndOfFile) {
                    t = EatEndOfInput();
                    end = _token.Span.End;
                }
            }

            if (allowIncomplete && t.Kind == TokenKind.EndOfFile) {
                errorCode |= ErrorCodes.IncompleteStatement;
            }

            ReportSyntaxError(start, end, String.Format(System.Globalization.CultureInfo.InvariantCulture,
                Resources.UnexpectedToken, t.Image), errorCode);
        }

        private void ReportSyntaxError(string message) {
            ReportSyntaxError(_lookahead.Span.Start, _lookahead.Span.End, message);
        }

        internal void ReportSyntaxError(SourceLocation start, SourceLocation end, string message) {
            ReportSyntaxError(start, end, message, ErrorCodes.SyntaxError);
        }

        internal void ReportSyntaxError(SourceLocation start, SourceLocation end, string message, int errorCode) {
            // save the first one, the next error codes may be induced errors:
            if (_errorCode == 0) {
                _errorCode = errorCode;
            }
            _errors.Add(_sourceUnit, message, new SourceSpan(start, end), errorCode, Severity.FatalError);
        }

        #endregion

        private static bool IsPrivateName(SymbolId name) {
            string s = SymbolTable.IdToString(name);
            return s.StartsWith("__") && !s.EndsWith("__");
        }

        private SymbolId FixName(SymbolId name) {
            if (_privatePrefix != null && IsPrivateName(name)) {
                name = SymbolTable.StringToId(string.Format("_{0}{1}", _privatePrefix, SymbolTable.IdToString(name)));
            }

            return name;
        }

        private SymbolId ReadNameMaybeNone() {
            Token t = NextToken();
            if (t == Tokens.NoneToken) {
                return Symbols.None;
            }

            NameToken n = t as NameToken;
            if (n == null) {
                ReportSyntaxError(_token);
                return SymbolId.Empty;
            }
            return FixName(n.Name);
        }

        private SymbolId ReadName() {
            NameToken n = NextToken() as NameToken;
            if (n == null) {
                ReportSyntaxError(_token);
                return SymbolId.Empty;
            }
            return FixName(n.Name);
        }

        #region Public parser interface

        //single_input: Newline | simple_stmt | compound_stmt Newline
        //eval_input: testlist Newline* ENDMARKER
        //file_input: (Newline | stmt)* ENDMARKER
        public PythonAst ParseFile(bool makeModule) {
            StartParsing();

            List<Statement> l = new List<Statement>();

            //
            // A future statement must appear near the top of the module. 
            // The only lines that can appear before a future statement are: 
            // - the module docstring (if any), 
            // - comments, 
            // - blank lines, and 
            // - other future statements. 
            // 

            while (MaybeEat(TokenKind.NewLine)) ;

            if (PeekToken(TokenKind.Constant)) {
                Statement s = ParseStmt();
                l.Add(s);
                _fromFutureAllowed = false;
                ExpressionStatement es = s as ExpressionStatement;
                if (es != null) {
                    ConstantExpression ce = es.Expression as ConstantExpression;
                    if (ce != null && ce.Value is string) {
                        // doc string
                        _fromFutureAllowed = true;
                    }
                }
            }

            while (MaybeEat(TokenKind.NewLine)) ;

            // from __future__
            if (_fromFutureAllowed) {
                while (PeekToken(Tokens.KeywordFromToken)) {
                    Statement s = ParseStmt();
                    l.Add(s);
                    FromImportStatement fis = s as FromImportStatement;
                    if (fis != null && !fis.IsFromFuture) {
                        // end of from __future__
                        break;
                    }
                }
            }

            // the end of from __future__ sequence
            _fromFutureAllowed = false;

            while (true) {
                if (MaybeEat(TokenKind.EndOfFile)) break;
                if (MaybeEat(TokenKind.NewLine)) continue;

                Statement s = ParseStmt();
                l.Add(s);
            }

            Statement[] stmts = l.ToArray();

            SuiteStatement ret = new SuiteStatement(stmts);
            ret.SetLoc(SourceLocation.MinValue, GetEnd());
            return new PythonAst(ret, makeModule, TrueDivision, false);
        }

        private static readonly char[] newLineChar = new char[] { '\n' };
        private static readonly char[] whiteSpace = { ' ', '\t' };

        // Given the interactive text input for a compound statement, calculate what the
        // indentation level of the next line should be
        public static int GetNextAutoIndentSize(string text, int autoIndentTabWidth) {
            Contract.RequiresNotNull(text, "text");

            Debug.Assert(text[text.Length - 1] == '\n');
            string[] lines = text.Split(newLineChar);
            if (lines.Length <= 1) return 0;
            string lastLine = lines[lines.Length - 2];

            // Figure out the number of white-spaces at the start of the last line
            int startingSpaces = 0;
            while (startingSpaces < lastLine.Length && lastLine[startingSpaces] == ' ')
                startingSpaces++;

            // Assume the same indent as the previous line
            int autoIndentSize = startingSpaces;
            // Increase the indent if this looks like the start of a compounds statement.
            // Ideally, we would ask the parser to tell us the exact indentation level
            if (lastLine.TrimEnd(whiteSpace).EndsWith(":"))
                autoIndentSize += autoIndentTabWidth;

            return autoIndentSize;
        }

        //[stmt_list] Newline | compound_stmt Newline
        //stmt_list ::= simple_stmt (";" simple_stmt)* [";"]
        //compound_stmt: if_stmt | while_stmt | for_stmt | try_stmt | funcdef | classdef
        //Returns a simple or coumpound_stmt or null if input is incomplete
        /// <summary>
        /// Parse one or more lines of interactive input
        /// </summary>
        /// <returns>null if input is not yet valid but could be with more lines</returns>
        public PythonAst ParseInteractiveCode(out SourceCodeProperties properties) {
            bool parsingMultiLineCmpdStmt;
            bool isEmptyStmt = false;

            properties = SourceCodeProperties.None;

        
            StartParsing();
            Statement ret = InternalParseInteractiveInput(out parsingMultiLineCmpdStmt, out isEmptyStmt);

            if (_errorCode == 0) {
                if (isEmptyStmt) {
                    properties = SourceCodeProperties.IsEmpty;
                } else if (parsingMultiLineCmpdStmt) {
                    properties = SourceCodeProperties.IsIncompleteStatement;
                }

                if (isEmptyStmt) {
                    return null;
                }

                return new PythonAst(ret, false, TrueDivision, true);
            } else {
                if ((_errorCode & ErrorCodes.IncompleteMask) != 0) {
                    if ((_errorCode & ErrorCodes.IncompleteToken) != 0) {
                        properties = SourceCodeProperties.IsIncompleteToken;
                        return null;
                    }

                    if ((_errorCode & ErrorCodes.IncompleteStatement) != 0) {
                        if (parsingMultiLineCmpdStmt) {
                            properties = SourceCodeProperties.IsIncompleteStatement;
                        } else {
                            properties = SourceCodeProperties.IsIncompleteToken;
                        }
                        return null;
                    }
                }

                properties = SourceCodeProperties.IsInvalid;
                return null;
            }
        }

        private Statement InternalParseInteractiveInput(out bool parsingMultiLineCmpdStmt, out bool isEmptyStmt) {
            Statement s;
            isEmptyStmt = false;
            parsingMultiLineCmpdStmt = false;

            switch (PeekToken().Kind) {
                case TokenKind.NewLine:
                    EatOptionalNewlines();
                    Eat(TokenKind.EndOfFile);
                    if (_tokenizer.EndContinues) {
                        parsingMultiLineCmpdStmt = true;
                        _errorCode = ErrorCodes.IncompleteStatement;
                    } else {
                        isEmptyStmt = true;
                    }
                    return null;

                case TokenKind.KeywordIf:
                case TokenKind.KeywordWhile:
                case TokenKind.KeywordFor:
                case TokenKind.KeywordTry:
                case TokenKind.At:
                case TokenKind.KeywordDef:
                case TokenKind.KeywordClass:
                    //              case TokenKind.KeywordWith: Provisioning for Python 2.6 
                    parsingMultiLineCmpdStmt = true;
                    s = ParseStmt();
                    EatEndOfInput();
                    break;

                default:
                    if (AllowWithStatement && PeekName(Symbols.With)) {
                        parsingMultiLineCmpdStmt = true;
                        s = ParseStmt();
                        EatEndOfInput();
                        break;
                    }

                    //  parseSimpleStmt takes care of one or more simple_stmts and the Newline
                    s = ParseSimpleStmt();
                    EatOptionalNewlines();
                    Eat(TokenKind.EndOfFile);
                    break;
            }
            return s;
        }

        public PythonAst ParseSingleStatement() {
            StartParsing();

            EatOptionalNewlines();
            Statement statement = ParseStmt();
            EatEndOfInput();
            return new PythonAst(statement, false, TrueDivision, true);
        }

        public PythonAst ParseExpression() {
            // TODO: move from source unit  .TrimStart(' ', '\t')

            return new PythonAst(new ReturnStatement(ParseTestListAsExpression()), false, TrueDivision, false);
        }

        private Expression ParseTestListAsExpression() {
            StartParsing();

            Expression expression = ParseTestListAsExpr(false);
            EatEndOfInput();
            return expression;
        }

        private void EatOptionalNewlines() {
            while (MaybeEat(TokenKind.NewLine)) ;
        }

        private Token EatEndOfInput() {
            while (MaybeEat(TokenKind.NewLine) || MaybeEat(TokenKind.Dedent)) {
                ;
            }

            Token t = NextToken();
            if (t.Kind != TokenKind.EndOfFile) {
                ReportSyntaxError(_token);
            }
            return t;
        }

        #endregion

        #region LL(1) Parsing

        //stmt: simple_stmt | compound_stmt
        //compound_stmt: if_stmt | while_stmt | for_stmt | try_stmt | funcdef | classdef
        private Statement ParseStmt() {
            switch (PeekToken().Kind) {
                case TokenKind.KeywordIf:
                    return ParseIfStmt();
                case TokenKind.KeywordWhile:
                    return ParseWhileStmt();
                case TokenKind.KeywordFor:
                    return ParseForStmt();
                case TokenKind.KeywordTry:
                    return ParseTryStatement();
                case TokenKind.At:
                    return ParseDecoratedFuncDef();
                case TokenKind.KeywordDef:
                    return ParseFuncDef();
                case TokenKind.KeywordClass:
                    return ParseClassDef();
                default:
                    if (AllowWithStatement && PeekName(Symbols.With)) {
                        return ParseWithStmt();
                    }
                    return ParseSimpleStmt();
            }
        }

        //simple_stmt: small_stmt (';' small_stmt)* [';'] Newline
        private Statement ParseSimpleStmt() {
            Statement s = ParseSmallStmt();
            if (MaybeEat(TokenKind.Semicolon)) {
                SourceLocation start = s.Start;
                List<Statement> l = new List<Statement>();
                l.Add(s);
                while (true) {
                    if (MaybeEat(TokenKind.NewLine)) break;
                    l.Add(ParseSmallStmt());
                    if (!MaybeEat(TokenKind.Semicolon)) {
                        Eat(TokenKind.NewLine);
                        break;
                    }
                    if (MaybeEat(TokenKind.EndOfFile)) break; // error recovery
                }
                Statement[] stmts = l.ToArray();

                SuiteStatement ret = new SuiteStatement(stmts);
                ret.SetLoc(start, GetEnd());
                return ret;
            } else {
                Eat(TokenKind.NewLine);
                return s;
            }
        }

        /*
        small_stmt: expr_stmt | print_stmt  | del_stmt | pass_stmt | flow_stmt | import_stmt | global_stmt | exec_stmt | assert_stmt

        del_stmt: 'del' exprlist
        pass_stmt: 'pass'
        flow_stmt: break_stmt | continue_stmt | return_stmt | raise_stmt | yield_stmt
        break_stmt: 'break'
        continue_stmt: 'continue'
        return_stmt: 'return' [testlist]
        yield_stmt: 'yield' testlist
        */
        private Statement ParseSmallStmt() {
            switch (PeekToken().Kind) {
                case TokenKind.KeywordPrint:
                    return ParsePrintStmt();
                case TokenKind.KeywordPass:
                    return FinishSmallStmt(new EmptyStatement());
                case TokenKind.KeywordBreak:
                    return FinishSmallStmt(new BreakStatement());
                case TokenKind.KeywordContinue:
                    return FinishSmallStmt(new ContinueStatement());
                case TokenKind.KeywordReturn:
                    return ParseReturnStmt();
                case TokenKind.KeywordFrom:
                    return ParseFromImportStmt();
                case TokenKind.KeywordImport:
                    return ParseImportStmt();
                case TokenKind.KeywordGlobal:
                    return ParseGlobalStmt();
                case TokenKind.KeywordRaise:
                    return ParseRaiseStmt();
                case TokenKind.KeywordAssert:
                    return ParseAssertStmt();
                case TokenKind.KeywordExec:
                    return ParseExecStmt();
                case TokenKind.KeywordDel:
                    return ParseDelStmt();
                case TokenKind.KeywordYield:
                    return ParseYieldStmt();
                default:
                    return ParseExprStmt();
            }
        }

        private Statement ParseDelStmt() {
            NextToken();
            SourceLocation start = GetStart();
            List<Expression> l = ParseExprList();
            DelStatement ret = new DelStatement(l.ToArray());
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private Statement ParseReturnStmt() {
            if (CurrentFunction == null) {
                ReportSyntaxError(IronPython.Resources.MisplacedReturn);
            }
            NextToken();
            Expression expr = null;
            SourceLocation start = GetStart();
            if (!NeverTestToken(PeekToken())) {
                expr = ParseTestListAsExpr(true);
            }
            ReturnStatement ret = new ReturnStatement(expr);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private Statement FinishSmallStmt(Statement stmt) {
            NextToken();
            stmt.SetLoc(GetStart(), GetEnd());
            return stmt;
        }

        private Statement ParseYieldStmt() {
            NextToken();
            SourceLocation start = GetStart();
            FunctionDefinition current = CurrentFunction;
            if (current == null) {
                ReportSyntaxError(IronPython.Resources.MisplacedYield);
            } else {
                current.IsGenerator = true;
            }
            Expression e = ParseTestListAsExpr(false);
            YieldStatement ret = new YieldStatement(e);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private Statement FinishAssignments(Expression right) {
            List<Expression> left = new List<Expression>();

            while (MaybeEat(TokenKind.Assign)) {
                left.Add(right);
                right = ParseTestListAsExpr(false);
            }

            Debug.Assert(left.Count > 0);

            AssignmentStatement assign = new AssignmentStatement(left.ToArray(), right);
            assign.SetLoc(left[0].Start, right.End);
            return assign;
        }

        //expr_stmt: testlist (augassign testlist | ('=' testlist)*)
        //augassign: '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '<<=' | '>>=' | '**=' | '//='
        private Statement ParseExprStmt() {
            Expression ret = ParseTestListAsExpr(false);
            if (PeekToken(TokenKind.Assign)) {
                return FinishAssignments(ret);
            } else {
                PythonOperator op = GetAssignOperator(PeekToken());
                if (op != PythonOperator.None) {
                    NextToken();
                    Expression rhs = ParseTestListAsExpr(false);
                    AugmentedAssignStatement aug = new AugmentedAssignStatement(op, ret, rhs);
                    aug.SetLoc(ret.Start, GetEnd());
                    return aug;
                } else {
                    Statement stmt = new ExpressionStatement(ret);
                    stmt.SetLoc(ret.Span);
                    return stmt;
                }
            }
        }

        private PythonOperator GetAssignOperator(Token t) {
            switch (t.Kind) {
                case TokenKind.AddEqual: return PythonOperator.Add;
                case TokenKind.SubtractEqual: return PythonOperator.Subtract;
                case TokenKind.MultiplyEqual: return PythonOperator.Multiply;
                case TokenKind.DivideEqual: return TrueDivision ? PythonOperator.TrueDivide : PythonOperator.Divide;
                case TokenKind.ModEqual: return PythonOperator.Mod;
                case TokenKind.BitwiseAndEqual: return PythonOperator.BitwiseAnd;
                case TokenKind.BitwiseOrEqual: return PythonOperator.BitwiseOr;
                case TokenKind.XorEqual: return PythonOperator.Xor;
                case TokenKind.LeftShiftEqual: return PythonOperator.LeftShift;
                case TokenKind.RightShiftEqual: return PythonOperator.RightShift;
                case TokenKind.PowerEqual: return PythonOperator.Power;
                case TokenKind.FloorDivideEqual: return PythonOperator.FloorDivide;
                default: return PythonOperator.None;
            }
        }


        private PythonOperator GetBinaryOperator(OperatorToken token) {
            switch (token.Kind) {
                case TokenKind.Add: return PythonOperator.Add;
                case TokenKind.Subtract: return PythonOperator.Subtract;
                case TokenKind.Multiply: return PythonOperator.Multiply;
                case TokenKind.Divide: return TrueDivision ? PythonOperator.TrueDivide : PythonOperator.Divide;
                case TokenKind.Mod: return PythonOperator.Mod;
                case TokenKind.BitwiseAnd: return PythonOperator.BitwiseAnd;
                case TokenKind.BitwiseOr: return PythonOperator.BitwiseOr;
                case TokenKind.Xor: return PythonOperator.Xor;
                case TokenKind.LeftShift: return PythonOperator.LeftShift;
                case TokenKind.RightShift: return PythonOperator.RightShift;
                case TokenKind.Power: return PythonOperator.Power;
                case TokenKind.FloorDivide: return PythonOperator.FloorDivide;
                default:
                    string message = String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        Resources.UnexpectedToken,
                        token.Kind);
                    Debug.Assert(false, message);
                    throw new ArgumentException(message);
            }
        }


        //import_stmt: 'import' dotted_as_name (',' dotted_as_name)*
        private ImportStatement ParseImportStmt() {
            Eat(TokenKind.KeywordImport);
            SourceLocation start = GetStart();

            List<DottedName> l = new List<DottedName>();
            List<SymbolId> las = new List<SymbolId>();
            l.Add(ParseDottedName());
            las.Add(MaybeParseAsName());
            while (MaybeEat(TokenKind.Comma)) {
                l.Add(ParseDottedName());
                las.Add(MaybeParseAsName());
            }
            DottedName[] names = l.ToArray();
            SymbolId[] asNames = las.ToArray();

            ImportStatement ret = new ImportStatement(names, asNames);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //| 'from' dotted_name 'import' ('*' | as_name_list | '(' as_name_list ')' )
        private FromImportStatement ParseFromImportStmt() {
            Eat(TokenKind.KeywordFrom);
            SourceLocation start = GetStart();
            DottedName dname = ParseDottedName();

            Eat(TokenKind.KeywordImport);

            SymbolId[] names;
            SymbolId[] asNames;
            bool fromFuture = false;

            if (MaybeEat(TokenKind.Multiply)) {
                names = FromImportStatement.Star;
                asNames = null;
            } else {
                List<SymbolId> l = new List<SymbolId>();
                List<SymbolId> las = new List<SymbolId>();

                if (MaybeEat(TokenKind.LeftParenthesis)) {
                    ParseAsNameList(l, las);
                    Eat(TokenKind.RightParenthesis);
                } else {
                    ParseAsNameList(l, las);
                }
                names = l.ToArray();
                asNames = las.ToArray();
            }

            // Process from __future__ statement

            if (dname.Names.Count == 1 && dname.Names[0] == Symbols.Future) {
                if (!_fromFutureAllowed) {
                    ReportSyntaxError(IronPython.Resources.MisplacedFuture);
                }
                if (names == FromImportStatement.Star) {
                    ReportSyntaxError(IronPython.Resources.NoFutureStar);
                }
                fromFuture = true;
                foreach (SymbolId name in names) {
                    if (name == Symbols.Division) {
                        _languageFeatures |= PythonLanguageFeatures.TrueDivision;
                    } else if (name == Symbols.WithStmt) {
                        _languageFeatures |= PythonLanguageFeatures.AllowWithStatement;
                    } else if (name == Symbols.NestedScopes) {
                    } else if (name == Symbols.Generators) {
                    } else {
                        fromFuture = false;
                        ReportSyntaxError(IronPython.Resources.UnknownFutureFeature + SymbolTable.IdToString(name));
                    }
                }
            }

            FromImportStatement ret = new FromImportStatement(dname, names, asNames, fromFuture);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        // import_as_name (',' import_as_name)*
        private void ParseAsNameList(List<SymbolId> l, List<SymbolId> las) {
            l.Add(ReadName());
            las.Add(MaybeParseAsName());
            while (MaybeEat(TokenKind.Comma)) {
                if (PeekToken(TokenKind.RightParenthesis)) return;  // the list is allowed to end with a ,
                l.Add(ReadName());
                las.Add(MaybeParseAsName());
            }
        }

        //import_as_name: NAME [NAME NAME]
        //dotted_as_name: dotted_name [NAME NAME]
        private SymbolId MaybeParseAsName() {
            NameToken t = PeekToken() as NameToken;
            if (t != null && t.Name == Symbols.As) {
                NextToken();
                return ReadName();
            }
            return SymbolId.Empty;
        }

        //dotted_name: NAME ('.' NAME)*
        private DottedName ParseDottedName() {
            SourceLocation start = GetStart();
            List<SymbolId> l = new List<SymbolId>();
            l.Add(ReadName());
            while (MaybeEat(TokenKind.Dot)) {
                l.Add(ReadName());
            }
            SymbolId[] names = l.ToArray();
            DottedName ret = new DottedName(names);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //exec_stmt: 'exec' expr ['in' test [',' test]]
        private ExecStatement ParseExecStmt() {
            Eat(TokenKind.KeywordExec);
            SourceLocation start = GetStart();
            Expression code, locals = null, globals = null;
            code = ParseExpr();
            if (MaybeEat(TokenKind.KeywordIn)) {
                globals = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    locals = ParseTest();
                }
            }
            ExecStatement ret = new ExecStatement(code, locals, globals);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //global_stmt: 'global' NAME (',' NAME)*
        private GlobalStatement ParseGlobalStmt() {
            Eat(TokenKind.KeywordGlobal);
            SourceLocation start = GetStart();
            List<SymbolId> l = new List<SymbolId>();
            l.Add(ReadName());
            while (MaybeEat(TokenKind.Comma)) {
                l.Add(ReadName());
            }
            SymbolId[] names = l.ToArray();
            GlobalStatement ret = new GlobalStatement(names);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //raise_stmt: 'raise' [test [',' test [',' test]]]
        private RaiseStatement ParseRaiseStmt() {
            Eat(TokenKind.KeywordRaise);
            SourceLocation start = GetStart();
            Expression type = null, _value = null, traceback = null;

            if (!NeverTestToken(PeekToken())) {
                type = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    _value = ParseTest();
                    if (MaybeEat(TokenKind.Comma)) {
                        traceback = ParseTest();
                    }
                }
            }
            RaiseStatement ret = new RaiseStatement(type, _value, traceback);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //assert_stmt: 'assert' test [',' test]
        private AssertStatement ParseAssertStmt() {
            Eat(TokenKind.KeywordAssert);
            SourceLocation start = GetStart();
            Expression test = ParseTest();
            Expression message = null;
            if (MaybeEat(TokenKind.Comma)) {
                message = ParseTest();
            }
            AssertStatement ret = new AssertStatement(test, message);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //print_stmt: 'print' ( [ test (',' test)* [','] ] | '>>' test [ (',' test)+ [','] ] )
        private PrintStatement ParsePrintStmt() {
            Eat(TokenKind.KeywordPrint);
            SourceLocation start = GetStart();
            Expression dest = null;
            PrintStatement ret;

            bool needNonEmptyTestList = false;
            if (MaybeEat(TokenKind.RightShift)) {
                dest = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    needNonEmptyTestList = true;
                } else {
                    ret = new PrintStatement(dest, new Expression[0], false);
                    ret.SetLoc(start, GetEnd());
                    return ret;
                }
            }

            bool trailingComma;
            List<Expression> l = ParseTestList(out trailingComma);
            if (needNonEmptyTestList && l.Count == 0) {
                ReportSyntaxError(_lookahead);
            }
            Expression[] exprs = l.ToArray();
            ret = new PrintStatement(dest, exprs, trailingComma);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        public string SetPrivatePrefix(SymbolId name) {
            // Remove any leading underscores before saving the prefix
            string oldPrefix = _privatePrefix;

            if (name != SymbolId.Empty) {
                string prefixString = SymbolTable.IdToString(name);
                for (int i = 0; i < prefixString.Length; i++) {
                    if (prefixString[i] != '_') {
                        _privatePrefix = prefixString.Substring(i);
                        return oldPrefix;
                    }
                }
            }
            // Name consists of '_'s only, no private prefix mapping
            _privatePrefix = null;
            return oldPrefix;
        }

        //classdef: 'class' NAME ['(' testlist ')'] ':' suite
        private ClassDefinition ParseClassDef() {
            Eat(TokenKind.KeywordClass);
            SourceLocation start = GetStart();
            SymbolId name = ReadName();
            Expression[] bases = new Expression[0];
            if (MaybeEat(TokenKind.LeftParenthesis)) {
                List<Expression> l = ParseTestList();
                bases = l.ToArray();
                Eat(TokenKind.RightParenthesis);
            }
            SourceLocation mid = GetEnd();

            // Save private prefix
            string savedPrefix = SetPrivatePrefix(name);

            // Parse the class body
            Statement body = ParseSuite();

            // Restore the private prefix
            _privatePrefix = savedPrefix;

            ClassDefinition ret = new ClassDefinition(name, bases, body);
            ret.Header = mid;
            ret.SetLoc(start, GetEnd());
            return ret;
        }


        //  decorators ::=
        //      decorator+
        //  decorator ::=
        //      "@" dotted_name ["(" [argument_list [","]] ")"] NEWLINE
        private List<Expression> ParseDecorators() {
            List<Expression> decorators = new List<Expression>();

            while (MaybeEat(TokenKind.At)) {
                SourceLocation start = GetStart();
                Expression decorator = new NameExpression(ReadName());
                decorator.SetLoc(start, GetEnd());
                while (MaybeEat(TokenKind.Dot)) {
                    SymbolId name = ReadNameMaybeNone();
                    decorator = new MemberExpression(decorator, name);
                    decorator.SetLoc(GetStart(), GetEnd());
                }
                decorator.SetLoc(start, GetEnd());

                if (MaybeEat(TokenKind.LeftParenthesis)) {
                    _sink.StartParameters(GetSpan());
                    Arg[] args = FinishArgumentList(null);
                    decorator = FinishCallExpr(decorator, args);
                }
                decorator.SetLoc(start, GetEnd());
                Eat(TokenKind.NewLine);

                decorators.Add(decorator);
            }

            return decorators;
        }

        // funcdef: [decorators] 'def' NAME parameters ':' suite
        // this gets called with "@" look-ahead
        private FunctionDefinition ParseDecoratedFuncDef() {
            List<Expression> decorators = ParseDecorators();
            FunctionDefinition fnc = ParseFuncDef();
            fnc.Decorators = decorators;

            return fnc;
        }

        // funcdef: [decorators] 'def' NAME parameters ':' suite
        // parameters: '(' [varargslist] ')'
        // this gets called with "def" as the look-ahead
        private FunctionDefinition ParseFuncDef() {
            Eat(TokenKind.KeywordDef);
            SourceLocation start = GetStart();
            SymbolId name = ReadName();

            Eat(TokenKind.LeftParenthesis);

            SourceLocation lStart = GetStart(), lEnd = GetEnd();
            int grouping = _tokenizer.GroupingLevel;

            Parameter[] parameters = ParseVarArgsList(TokenKind.RightParenthesis);

            SourceLocation rStart = GetStart(), rEnd = GetEnd();

            FunctionDefinition ret = new FunctionDefinition(name, parameters, _sourceUnit);
            PushFunction(ret);

            Statement body = ParseSuite();
            FunctionDefinition ret2 = PopFunction();
            System.Diagnostics.Debug.Assert(ret == ret2);

            ret.Body = body;
            ret.Header = rEnd;

            _sink.MatchPair(new SourceSpan(lStart, lEnd), new SourceSpan(rStart, rEnd), grouping);

            ret.SetLoc(start, GetEnd());

            return ret;
        }

        private Parameter ParseParameterName(Dictionary<SymbolId, object> names, ParameterKind kind) {
            SymbolId name = ReadName();
            if (name != SymbolId.Empty) {
                CheckUniqueParameter(names, name);
            }
            Parameter parameter = new Parameter(name, kind);
            parameter.SetLoc(GetStart(), GetEnd());
            return parameter;
        }

        private void CheckUniqueParameter(Dictionary<SymbolId, object> names, SymbolId name) {
            if (names.ContainsKey(name)) {
                ReportSyntaxError(String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    Resources.DuplicateArgumentInFuncDef,
                    SymbolTable.IdToString(name)));
            }
            names[name] = null;
        }

        //varargslist: (fpdef ['=' test] ',')* ('*' NAME [',' '**' NAME] | '**' NAME) | fpdef ['=' test] (',' fpdef ['=' test])* [',']
        //fpdef: NAME | '(' fplist ')'
        //fplist: fpdef (',' fpdef)* [',']
        private Parameter[] ParseVarArgsList(TokenKind terminator) {
            // parameters not doing * or ** today
            List<Parameter> pl = new List<Parameter>();
            Dictionary<SymbolId, object> names = new Dictionary<SymbolId, object>();
            bool needDefault = false;
            for (int position = 0; ; position++) {
                if (MaybeEat(terminator)) break;

                Parameter parameter;

                if (MaybeEat(TokenKind.Multiply)) {
                    parameter = ParseParameterName(names, ParameterKind.List);
                    pl.Add(parameter);
                    if (MaybeEat(TokenKind.Comma)) {
                        Eat(TokenKind.Power);
                        parameter = ParseParameterName(names, ParameterKind.Dictionary);
                        pl.Add(parameter);
                    }
                    Eat(terminator);
                    break;
                } else if (MaybeEat(TokenKind.Power)) {
                    parameter = ParseParameterName(names, ParameterKind.Dictionary);
                    pl.Add(parameter);
                    Eat(terminator);
                    break;
                }

                //
                //  Parsing defparameter:
                //
                //  defparameter ::=
                //      parameter ["=" expression]

                if ((parameter = ParseParameter(position, names)) != null) {
                    pl.Add(parameter);
                    if (MaybeEat(TokenKind.Assign)) {
                        needDefault = true;
                        parameter.DefaultValue = ParseTest();
                    } else if (needDefault) {
                        ReportSyntaxError(IronPython.Resources.DefaultRequired);
                    }
                }
                if (!MaybeEat(TokenKind.Comma)) {
                    Eat(terminator);
                    break;
                }
            }

            return pl.ToArray();
        }

        //  parameter ::=
        //      identifier | "(" sublist ")"
        Parameter ParseParameter(int position, Dictionary<SymbolId, object> names) {
            Token t = NextToken();
            Parameter parameter = null;

            switch (t.Kind) {
                case TokenKind.LeftParenthesis: // sublist
                    Expression ret = ParseSublist(names);
                    Eat(TokenKind.RightParenthesis);
                    TupleExpression tret = ret as TupleExpression;

                    if (tret != null) {
                        parameter = new SublistParameter(position, tret);
                    } else {
                        parameter = new Parameter(((NameExpression)ret).Name);
                    }
                    parameter.SetLoc(ret.Span);
                    break;

                case TokenKind.Name:  // identifier
                    SymbolId name = (SymbolId)t.Value;
                    parameter = new Parameter(name);
                    CompleteParameterName(parameter, name, names);
                    break;

                default:
                    ReportSyntaxError(_token);
                    break;
            }

            return parameter;
        }

        private void CompleteParameterName(Node node, SymbolId name, Dictionary<SymbolId, object> names) {
            SourceSpan span = GetSpan();
            _sink.StartName(span, SymbolTable.IdToString(name));
            CheckUniqueParameter(names, name);
            node.SetLoc(span);
        }

        //  parameter ::=
        //      identifier | "(" sublist ")"
        Expression ParseSublistParameter(Dictionary<SymbolId, object> names) {
            Token t = NextToken();
            Expression ret = null;
            switch (t.Kind) {
                case TokenKind.LeftParenthesis: // sublist
                    ret = ParseSublist(names);
                    Eat(TokenKind.RightParenthesis);
                    break;
                case TokenKind.Name:  // identifier
                    SymbolId name = (SymbolId)t.Value;
                    NameExpression ne = new NameExpression(name);
                    CompleteParameterName(ne, name, names);
                    return ne;
                default:
                    ReportSyntaxError(_token);
                    ret = new ErrorExpression();
                    ret.SetLoc(GetStart(), GetEnd());
                    break;
            }
            return ret;
        }

        //  sublist ::=
        //      parameter ("," parameter)* [","]
        Expression ParseSublist(Dictionary<SymbolId, object> names) {
            bool trailingComma;
            List<Expression> list = new List<Expression>();
            for (; ; ) {
                trailingComma = false;
                list.Add(ParseSublistParameter(names));
                if (MaybeEat(TokenKind.Comma)) {
                    trailingComma = true;
                    switch (PeekToken().Kind) {
                        case TokenKind.LeftParenthesis:
                        case TokenKind.Name:
                            continue;
                        default:
                            break;
                    }
                    break;
                } else {
                    trailingComma = false;
                    break;
                }
            }
            return MakeTupleOrExpr(list, trailingComma);
        }

        //Python2.5 -> old_lambdef: 'lambda' [varargslist] ':' old_test
        private int oldLambdaCount;
        private Expression FinishOldLambdef() {
            SourceLocation start = GetStart();
            Parameter[] parameters;
            parameters = ParseVarArgsList(TokenKind.Colon);
            SourceLocation mid = GetEnd();

            Expression expr = ParseOldTest();
            Statement body = new ReturnStatement(expr);
            body.SetLoc(expr.Start, expr.End);
            FunctionDefinition func = new FunctionDefinition(SymbolTable.StringToId("<lambda$" + (oldLambdaCount++) + ">"), parameters, body, _sourceUnit);
            func.SetLoc(start, GetEnd());
            func.Header = mid;
            LambdaExpression ret = new LambdaExpression(func);
            ret.SetLoc(start, GetEnd());
            return ret;
        }


        //lambdef: 'lambda' [varargslist] ':' test
        private int lambdaCount;
        private Expression FinishLambdef() {
            SourceLocation start = GetStart();
            Parameter[] parameters;
            parameters = ParseVarArgsList(TokenKind.Colon);
            SourceLocation mid = GetEnd();

            Expression expr = ParseTest();
            Statement body = new ReturnStatement(expr);
            body.SetLoc(expr.Start, expr.End);
            FunctionDefinition func = new FunctionDefinition(SymbolTable.StringToId("<lambda$" + (lambdaCount++) + ">"), parameters, body, _sourceUnit);
            func.SetLoc(start, GetEnd());
            func.Header = mid;
            LambdaExpression ret = new LambdaExpression(func);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //while_stmt: 'while' test ':' suite ['else' ':' suite]
        private WhileStatement ParseWhileStmt() {
            Eat(TokenKind.KeywordWhile);
            SourceLocation start = GetStart();
            Expression test = ParseTest();
            SourceLocation mid = GetEnd();
            Statement body = ParseSuite();
            Statement else_ = null;
            if (MaybeEat(TokenKind.KeywordElse)) {
                else_ = ParseSuite();
            }
            WhileStatement ret = new WhileStatement(test, body, else_);
            ret.SetLoc(start, mid, GetEnd());
            ret.Header = mid;
            return ret;
        }

        //with_stmt: 'with' test [ 'as' with_var ] ':' suite
        private WithStatement ParseWithStmt() {
            SkipName(Symbols.With);
            SourceLocation start = GetStart();
            Expression contextManager = ParseTest();
            Expression var = null;
            if (PeekName(Symbols.As)) {
                SkipName(Symbols.As);
                var = ParseTest();
            }

            SourceLocation header = GetEnd();
            Statement body = ParseSuite();
            WithStatement ret = new WithStatement(contextManager, var, body);
            ret.Header = header;
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //for_stmt: 'for' exprlist 'in' testlist ':' suite ['else' ':' suite]
        private ForStatement ParseForStmt() {
            Eat(TokenKind.KeywordFor);
            SourceLocation start = GetStart();
            List<Expression> l = ParseExprList(); //TokenKind.KeywordIn);

            // expr list is something like:
            //  ()
            //  a
            //  a,b
            //  a,b,c
            // we either want just () or a or we want (a,b) and (a,b,c)
            // so we can do tupleExpr.EmitSet() or loneExpr.EmitSet()

            Expression lhs = MakeTupleOrExpr(l, false);
            Eat(TokenKind.KeywordIn);
            Expression list = ParseTestListAsExpr(false);
            SourceLocation header = GetEnd();
            Statement body = ParseSuite();
            Statement else_ = null;
            if (MaybeEat(TokenKind.KeywordElse)) {
                else_ = ParseSuite();
            }
            ForStatement ret = new ForStatement(lhs, list, body, else_);
            ret.Header = header;
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        // if_stmt: 'if' test ':' suite ('elif' test ':' suite)* ['else' ':' suite]
        private IfStatement ParseIfStmt() {
            Eat(TokenKind.KeywordIf);
            SourceLocation start = GetStart();
            List<IfStatementTest> l = new List<IfStatementTest>();
            l.Add(ParseIfStmtTest());

            while (MaybeEat(TokenKind.KeywordElseIf)) {
                l.Add(ParseIfStmtTest());
            }

            Statement else_ = null;
            if (MaybeEat(TokenKind.KeywordElse)) {
                else_ = ParseSuite();
            }

            IfStatementTest[] tests = l.ToArray();
            IfStatement ret = new IfStatement(tests, else_);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private IfStatementTest ParseIfStmtTest() {
            SourceLocation start = GetStart();
            Expression test = ParseTest();
            SourceLocation header = GetEnd();
            Statement suite = ParseSuite();
            IfStatementTest ret = new IfStatementTest(test, suite);
            ret.SetLoc(start, suite.End);
            ret.Header = header;
            return ret;
        }

        //try_stmt: ('try' ':' suite (except_clause ':' suite)+
        //    ['else' ':' suite] | 'try' ':' suite 'finally' ':' suite)
        //# NB compile.c makes sure that the default except clause is last

        // Python 2.5 grammar
        //try_stmt: 'try' ':' suite
        //          (
        //            (except_clause ':' suite)+
        //            ['else' ':' suite]
        //            ['finally' ':' suite]
        //          |
        //            'finally' : suite
        //          )


        private Statement ParseTryStatement() {
            Eat(TokenKind.KeywordTry);
            SourceLocation start = GetStart();
            SourceLocation mid = GetEnd();
            Statement body = ParseSuite();
            Statement finallySuite = null;
            Statement elseSuite = null;
            Statement ret;

            if (MaybeEat(TokenKind.KeywordFinally)) {
                finallySuite = ParseSuite();
                TryStatement tfs = new TryStatement(body, null, elseSuite, finallySuite);
                tfs.Header = mid;
                ret = tfs;
            } else {
                List<TryStatementHandler> handlers = new List<TryStatementHandler>();
                TryStatementHandler dh = null;
                do {
                    TryStatementHandler handler = ParseTryStmtHandler();
                    handlers.Add(handler);

                    if (dh != null) {
                        ReportSyntaxError(dh.Start, dh.End, "default 'except' must be last");
                    }
                    if (handler.Test == null) {
                        dh = handler;
                    }
                } while (PeekToken().Kind == TokenKind.KeywordExcept);

                if (MaybeEat(TokenKind.KeywordElse)) {
                    elseSuite = ParseSuite();
                }

                if (MaybeEat(TokenKind.KeywordFinally)) {
                    finallySuite = ParseSuite();
                }

                TryStatement ts = new TryStatement(body, handlers.ToArray(), elseSuite, finallySuite);
                ts.Header = mid;
                ret = ts;
            }
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //except_clause: 'except' [test [',' test]]
        private TryStatementHandler ParseTryStmtHandler() {
            Eat(TokenKind.KeywordExcept);
            SourceLocation start = GetStart();
            Expression test1 = null, test2 = null;
            if (PeekToken().Kind != TokenKind.Colon) {
                test1 = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    test2 = ParseTest();
                }
            }
            SourceLocation mid = GetEnd();
            Statement body = ParseSuite();
            TryStatementHandler ret = new TryStatementHandler(test1, test2, body);
            ret.Header = mid;
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //suite: simple_stmt NEWLINE | Newline INDENT stmt+ DEDENT
        private Statement ParseSuite() {
            EatNoEof(TokenKind.Colon);

            List<Statement> l = new List<Statement>();
            if (MaybeEat(TokenKind.NewLine)) {
                if (!MaybeEat(TokenKind.Indent)) {
                    NextToken();
                    ReportSyntaxError(_token, ErrorCodes.IndentationError);
                }
                while (true) {
                    Statement s = ParseStmt();

                    l.Add(s);
                    if (MaybeEat(TokenKind.Dedent)) break;
                    if (MaybeEat(TokenKind.EndOfFile)) break;         // error recovery
                }
                Statement[] stmts = l.ToArray();
                SuiteStatement ret = new SuiteStatement(stmts);
                ret.SetLoc(stmts[0].Start, stmts[stmts.Length - 1].End);
                return ret;
            } else {
                //  simple_stmt NEWLINE
                //  ParseSimpleStmt takes care of the NEWLINE
                Statement s = ParseSimpleStmt();
                return s;
            }
        }

        // Python 2.5 -> old_test: or_test | old_lambdef
        private Expression ParseOldTest() {
            if (MaybeEat(TokenKind.KeywordLambda)) {
                return FinishOldLambdef();
            }
            return ParseOrTest();
        }

        // Python2.5 -> test: or_test ['if' or_test 'else' test] | lambdef
        private Expression ParseTest() {
            if (MaybeEat(TokenKind.KeywordLambda)) {
                return FinishLambdef();
            }

            Expression ret = ParseOrTest();
            if (MaybeEat(TokenKind.KeywordIf)) {
                SourceLocation start = ret.Start;
                ret = ParseConditionalTest(ret);
                ret.SetLoc(start, GetEnd());
            }

            return ret;
        }

        // Python2.5 -> or_test: and_test ('or' and_test)*
        private Expression ParseOrTest() {
            Expression ret = ParseAndTest();
            while (MaybeEat(TokenKind.KeywordOr)) {
                SourceLocation start = ret.Start;
                ret = new OrExpression(ret, ParseAndTest());
                ret.SetLoc(start, GetEnd());
            }
            return ret;
        }

        private Expression ParseConditionalTest(Expression trueExpr) {
            Expression test = ParseOrTest();
            Eat(TokenKind.KeywordElse);
            SourceLocation start = test.Start;
            Expression falseExpr = ParseTest();
            test.SetLoc(start, GetEnd());
            return new ConditionalExpression(test, trueExpr, falseExpr);
        }



        // and_test: not_test ('and' not_test)*
        private Expression ParseAndTest() {
            Expression ret = ParseNotTest();
            while (MaybeEat(TokenKind.KeywordAnd)) {
                SourceLocation start = ret.Start;
                ret = new AndExpression(ret, ParseAndTest());
                ret.SetLoc(start, GetEnd());
            }
            return ret;
        }

        //not_test: 'not' not_test | comparison
        private Expression ParseNotTest() {
            if (MaybeEat(TokenKind.KeywordNot)) {
                SourceLocation start = GetStart();
                Expression ret = new UnaryExpression(PythonOperator.Not, ParseNotTest());
                ret.SetLoc(start, GetEnd());
                return ret;
            } else {
                return ParseComparison();
            }
        }
        //comparison: expr (comp_op expr)*
        //comp_op: '<'|'>'|'=='|'>='|'<='|'<>'|'!='|'in'|'not' 'in'|'is'|'is' 'not'
        private Expression ParseComparison() {
            Expression ret = ParseExpr();
            while (true) {
                PythonOperator op;
                switch (PeekToken().Kind) {
                    case TokenKind.LessThan: NextToken(); op = PythonOperator.LessThan; break;
                    case TokenKind.LessThanOrEqual: NextToken(); op = PythonOperator.LessThanOrEqual; break;
                    case TokenKind.GreaterThan: NextToken(); op = PythonOperator.GreaterThan; break;
                    case TokenKind.GreaterThanOrEqual: NextToken(); op = PythonOperator.GreaterThanOrEqual; break;
                    case TokenKind.Equals: NextToken(); op = PythonOperator.Equal; break;
                    case TokenKind.NotEquals: NextToken(); op = PythonOperator.NotEqual; break;
                    case TokenKind.LessThanGreaterThan: NextToken(); op = PythonOperator.NotEqual; break;

                    case TokenKind.KeywordIn: NextToken(); op = PythonOperator.In; break;
                    case TokenKind.KeywordNot: NextToken(); Eat(TokenKind.KeywordIn); op = PythonOperator.NotIn; break;

                    case TokenKind.KeywordIs:
                        NextToken();
                        if (MaybeEat(TokenKind.KeywordNot)) {
                            op = PythonOperator.IsNot;
                        } else {
                            op = PythonOperator.Is;
                        }
                        break;
                    default:
                        return ret;
                }
                Expression rhs = ParseComparison();
                BinaryExpression be = new BinaryExpression(op, ret, rhs);
                be.SetLoc(ret.Start, GetEnd());
                ret = be;
            }
        }

        /*
        expr: xor_expr ('|' xor_expr)*
        xor_expr: and_expr ('^' and_expr)*
        and_expr: shift_expr ('&' shift_expr)*
        shift_expr: arith_expr (('<<'|'>>') arith_expr)*
        arith_expr: term (('+'|'-') term)*
        term: factor (('*'|'/'|'%'|'//') factor)*
        */
        private Expression ParseExpr() {
            return ParseExpr(0);
        }

        private Expression ParseExpr(int precedence) {
            Expression ret = ParseFactor();
            while (true) {
                Token t = PeekToken();
                OperatorToken ot = t as OperatorToken;
                if (ot == null) return ret;

                int prec = ot.Precedence;
                if (prec >= precedence) {
                    NextToken();
                    Expression right = ParseExpr(prec + 1);
                    SourceLocation start = ret.Start;
                    ret = new BinaryExpression(GetBinaryOperator(ot), ret, right);
                    ret.SetLoc(start, GetEnd());
                } else {
                    return ret;
                }
            }
        }

        // factor: ('+'|'-'|'~') factor | power
        private Expression ParseFactor() {
            SourceLocation start = _lookahead.Span.Start;
            Expression ret;
            switch (PeekToken().Kind) {
                case TokenKind.Add:
                    NextToken();
                    ret = new UnaryExpression(PythonOperator.Pos, ParseFactor());
                    break;
                case TokenKind.Subtract:
                    NextToken();
                    ret = FinishUnaryNegate();
                    break;
                case TokenKind.Twiddle:
                    NextToken();
                    ret = new UnaryExpression(PythonOperator.Invert, ParseFactor());
                    break;
                default:
                    return ParsePower();
            }
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private Expression FinishUnaryNegate() {
            // Special case to ensure that System.Int32.MinValue is an int and not a BigInteger
            if (PeekToken().Kind == TokenKind.Constant && _tokenizer.TokenStringEquals("2147483648")) {
                NextToken();
                return new ConstantExpression(-2147483648);
            }

            return new UnaryExpression(PythonOperator.Negate, ParseFactor());
        }

        // power: atom trailer* ['**' factor]
        private Expression ParsePower() {
            Expression ret = ParsePrimary();
            ret = AddTrailers(ret);
            if (MaybeEat(TokenKind.Power)) {
                SourceLocation start = ret.Start;
                ret = new BinaryExpression(PythonOperator.Power, ret, ParseFactor());
                ret.SetLoc(start, GetEnd());
            }
            return ret;
        }



        // atom: '(' [testlist_gexp] ')' | '[' [listmaker] ']' | '{' [dictmaker] '}' | '`' testlist1 '`' | NAME | NUMBER | STRING+
        private Expression ParsePrimary() {
            Token t = NextToken();
            Expression ret;
            switch (t.Kind) {
                case TokenKind.LeftParenthesis:
                    return FinishTupleOrGenExp();
                case TokenKind.LeftBracket:
                    return FinishListValue();
                case TokenKind.LeftBrace:
                    return FinishDictValue();
                case TokenKind.BackQuote:
                    return FinishBackquote();
                case TokenKind.Name:
                    SourceSpan span = GetSpan();
                    SymbolId name = (SymbolId)t.Value;
                    _sink.StartName(span, SymbolTable.IdToString(name));
                    ret = new NameExpression(FixName(name));
                    ret.SetLoc(GetStart(), GetEnd());
                    return ret;
                case TokenKind.Constant:
                    SourceLocation start = GetStart();
                    object cv = t.Value;
                    string cvs = cv as string;
                    if (cvs != null) {
                        cv = FinishStringPlus(cvs);
                    }
                    // todo handle STRING+
                    ret = new ConstantExpression(cv);
                    ret.SetLoc(start, GetEnd());
                    return ret;
                default:
                    ReportSyntaxError(_token.Token, _token.Span, ErrorCodes.SyntaxError, _allowIncomplete || _tokenizer.EndContinues);

                    // error node
                    ret = new ErrorExpression();
                    ret.SetLoc(GetStart(), GetEnd());
                    return ret;
            }
        }

        private string FinishStringPlus(string s) {
            Token t = PeekToken();
            while (true) {
                if (t is ConstantValueToken) {
                    string cvs;
                    if ((cvs = t.Value as String) != null) {
                        s += cvs;
                        NextToken();
                        t = PeekToken();
                        continue;
                    }
                }
                break;
            }
            return s;
        }

        // trailer: '(' [ arglist_genexpr ] ')' | '[' subscriptlist ']' | '.' NAME
        private Expression AddTrailers(Expression ret) {
            while (true) {
                switch (PeekToken().Kind) {
                    case TokenKind.LeftParenthesis:
                        NextToken();
                        Arg[] args = FinishArgListOrGenExpr();
                        CallExpression call = FinishCallExpr(ret, args);
                        call.SetLoc(ret.Start, GetEnd());
                        ret = call;
                        break;
                    case TokenKind.LeftBracket:
                        NextToken();
                        Expression index = ParseSubscriptList();
                        IndexExpression ie = new IndexExpression(ret, index);
                        ie.SetLoc(ret.Start, GetEnd());
                        ret = ie;
                        break;
                    case TokenKind.Dot:
                        NextToken();
                        SymbolId name = ReadNameMaybeNone();
                        MemberExpression fe = new MemberExpression(ret, name);
                        fe.SetLoc(ret.Start, GetEnd());
                        ret = fe;
                        break;
                    default:
                        return ret;
                }
            }
        }

        //subscriptlist: subscript (',' subscript)* [',']
        //subscript: '.' '.' '.' | test | [test] ':' [test] [sliceop]
        //sliceop: ':' [test]
        private Expression ParseSubscriptList() {
            const TokenKind terminator = TokenKind.RightBracket;
            SourceLocation start0 = GetStart();
            bool trailingComma = false;

            List<Expression> l = new List<Expression>();
            while (true) {
                Expression e;
                if (MaybeEat(TokenKind.Dot)) {
                    SourceLocation start = GetStart();
                    Eat(TokenKind.Dot); Eat(TokenKind.Dot);
                    e = new ConstantExpression(PythonOps.Ellipsis);
                    e.SetLoc(start, GetEnd());
                } else if (MaybeEat(TokenKind.Colon)) {
                    e = FinishSlice(null, GetStart());
                } else {
                    e = ParseTest();
                    if (MaybeEat(TokenKind.Colon)) {
                        e = FinishSlice(e, e.Start);
                    }
                }

                l.Add(e);
                if (!MaybeEat(TokenKind.Comma)) {
                    Eat(terminator);
                    trailingComma = false;
                    break;
                }

                trailingComma = true;
                if (MaybeEat(terminator)) {
                    break;
                }
            }
            Expression ret = MakeTupleOrExpr(l, trailingComma, true);
            ret.SetLoc(start0, GetEnd());
            return ret;
        }

        private Expression ParseSliceEnd() {
            Expression e2 = null;
            switch (PeekToken().Kind) {
                case TokenKind.Comma:
                case TokenKind.RightBracket:
                    break;
                default:
                    e2 = ParseTest();
                    break;
            }
            return e2;
        }

        private Expression FinishSlice(Expression e0, SourceLocation start) {
            Expression e1 = null;
            Expression e2 = null;
            bool stepProvided = false;

            switch (PeekToken().Kind) {
                case TokenKind.Comma:
                case TokenKind.RightBracket:
                    break;
                case TokenKind.Colon:
                    // x[?::?]
                    stepProvided = true;
                    NextToken();
                    e2 = ParseSliceEnd();
                    break;
                default:
                    // x[?:val:?]
                    e1 = ParseTest();
                    if (MaybeEat(TokenKind.Colon)) {
                        stepProvided = true;
                        e2 = ParseSliceEnd();
                    }
                    break;
            }
            SliceExpression ret = new SliceExpression(e0, e1, e2, stepProvided);
            ret.SetLoc(start, GetEnd());
            return ret;
        }


        //exprlist: expr (',' expr)* [',']
        private List<Expression> ParseExprList() {
            List<Expression> l = new List<Expression>();
            while (true) {
                Expression e = ParseExpr();
                l.Add(e);
                if (!MaybeEat(TokenKind.Comma)) {
                    break;
                }
                if (NeverTestToken(PeekToken())) {
                    break;
                }
            }
            return l;
        }

        // arglist ::=
        //             test                     rest_of_arguments
        //             test "=" test            rest_of_arguments
        //             test "for" gen_expr_rest
        //
        private Arg[] FinishArgListOrGenExpr() {
            Arg a = null;

            _sink.StartParameters(GetSpan());

            Token t = PeekToken();
            if (t.Kind != TokenKind.RightParenthesis && t.Kind != TokenKind.Multiply && t.Kind != TokenKind.Power) {
                SourceLocation start = GetStart();
                Expression e = ParseTest();
                if (MaybeEat(TokenKind.Assign)) {               //  Keyword argument
                    a = FinishKeywordArgument(e);

                    if (a == null) {                            // Error recovery
                        a = new Arg(e);
                        a.SetLoc(e.Start, GetEnd());
                    }
                } else if (PeekToken(Tokens.KeywordForToken)) {    //  Generator expression
                    a = new Arg(ParseGeneratorExpression(e));
                    Eat(TokenKind.RightParenthesis);
                    a.SetLoc(start, GetEnd());
                    _sink.EndParameters(GetSpan());
                    return new Arg[1] { a };       //  Generator expression is the argument
                } else {
                    a = new Arg(e);
                    a.SetLoc(e.Start, e.End);
                }

                //  Was this all?
                //
                if (MaybeEat(TokenKind.Comma)) {
                    _sink.NextParameter(GetSpan());
                } else {
                    Eat(TokenKind.RightParenthesis);
                    a.SetLoc(start, GetEnd());
                    _sink.EndParameters(GetSpan());
                    return new Arg[1] { a };
                }
            }

            return FinishArgumentList(a);
        }

        private Arg FinishKeywordArgument(Expression t) {
            NameExpression n = t as NameExpression;
            if (n == null) {
                ReportSyntaxError(IronPython.Resources.ExpectedName);
                Arg arg = new Arg(SymbolId.Empty, t);
                arg.SetLoc(t.Start, t.End);
                return arg;
            } else {
                Expression val = ParseTest();
                Arg arg = new Arg(n.Name, val);
                arg.SetLoc(n.Start, val.End);
                return arg;
            }
        }

        private void CheckUniqueArgument(Dictionary<SymbolId, SymbolId> names, Arg arg) {
            if (arg != null && arg.Name != SymbolId.Empty) {
                SymbolId name = arg.Name;
                if (names.ContainsKey(name)) {
                    ReportSyntaxError(IronPython.Resources.DuplicateKeywordArg);
                }
                names[name] = name;
            }
        }

        //arglist: (argument ',')* (argument [',']| '*' test [',' '**' test] | '**' test)
        //argument: [test '='] test    # Really [keyword '='] test
        private Arg[] FinishArgumentList(Arg first) {
            const TokenKind terminator = TokenKind.RightParenthesis;
            List<Arg> l = new List<Arg>();
            Dictionary<SymbolId, SymbolId> names = new Dictionary<SymbolId, SymbolId>();

            if (first != null) {
                l.Add(first);
                CheckUniqueArgument(names, first);
            }

            // Parse remaining arguments
            while (true) {
                if (MaybeEat(terminator)) {
                    break;
                }
                SourceLocation start = GetStart();
                Arg a;
                if (MaybeEat(TokenKind.Multiply)) {
                    Expression t = ParseTest();
                    a = new Arg(Symbols.Star, t);
                } else if (MaybeEat(TokenKind.Power)) {
                    Expression t = ParseTest();
                    a = new Arg(Symbols.StarStar, t);
                } else {
                    Expression e = ParseTest();
                    if (MaybeEat(TokenKind.Assign)) {
                        a = FinishKeywordArgument(e);
                        CheckUniqueArgument(names, a);
                    } else {
                        a = new Arg(e);
                    }
                }
                a.SetLoc(start, GetEnd());
                l.Add(a);
                if (MaybeEat(TokenKind.Comma)) {
                    _sink.NextParameter(GetSpan());
                } else {
                    Eat(terminator);
                    break;
                }
            }

            _sink.EndParameters(GetSpan());

            Arg[] ret = l.ToArray();
            return ret;

        }

        private List<Expression> ParseTestList() {
            bool tmp;
            return ParseTestList(out tmp);
        }

        private Expression ParseTestListAsSafeExpr() {
            bool trailingComma;
            List<Expression> l = ParseTestListSafe(out trailingComma);
            //  the case when no expression was parsed e.g. when we have an empty test list
            if (l.Count == 0 && !trailingComma) {
                ReportSyntaxError("invalid syntax");
            }
            return MakeTupleOrExpr(l, trailingComma);
        }

        //   Python 2.5 -> testlist_safe: old_test [(',' old_test)+ [',']]
        private List<Expression> ParseTestListSafe(out bool trailingComma) {
            List<Expression> l = new List<Expression>();
            trailingComma = false;
            while (true) {
                if (NeverTestToken(PeekToken())) break;
                l.Add(ParseOldTest());
                if (!MaybeEat(TokenKind.Comma)) {
                    trailingComma = false;
                    break;
                }
                trailingComma = true;
            }
            return l;
        }

        //        testlist: test (',' test)* [',']
        //        testlist_safe: test [(',' test)+ [',']]
        //        testlist1: test (',' test)*
        private List<Expression> ParseTestList(out bool trailingComma) {
            List<Expression> l = new List<Expression>();
            trailingComma = false;
            while (true) {
                if (NeverTestToken(PeekToken())) break;
                l.Add(ParseTest());
                if (!MaybeEat(TokenKind.Comma)) {
                    trailingComma = false;
                    break;
                }
                trailingComma = true;
            }
            return l;
        }

        private Expression ParseTestListAsExpr(bool allowEmptyExpr) {
            bool trailingComma;
            List<Expression> l = ParseTestList(out trailingComma);
            //  the case when no expression was parsed e.g. when we have an empty test list
            if (!allowEmptyExpr && l.Count == 0 && !trailingComma) {
                ReportSyntaxError("invalid syntax");
            }
            return MakeTupleOrExpr(l, trailingComma);
        }

        private Expression FinishTestListAsExpr(Expression test) {
            SourceLocation start = GetStart();
            bool trailingComma = true;
            List<Expression> l = new List<Expression>();
            l.Add(test);

            while (true) {
                if (NeverTestToken(PeekToken())) break;
                test = ParseTest();
                l.Add(test);
                if (!MaybeEat(TokenKind.Comma)) {
                    trailingComma = false;
                    break;
                }
                trailingComma = true;
            }

            Expression ret = MakeTupleOrExpr(l, trailingComma);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //
        //  testlist_gexp: test ( genexpr_for | (',' test)* [','] )
        //
        private Expression FinishTupleOrGenExp() {
            SourceLocation lStart = GetStart();
            SourceLocation lEnd = GetEnd();
            int grouping = _tokenizer.GroupingLevel;
            bool hasRightParenthesis;

            Expression ret;
            //  Empty tuple
            if (MaybeEat(TokenKind.RightParenthesis)) {
                ret = MakeTupleOrExpr(new List<Expression>(), false);
                hasRightParenthesis = true;
            } else {
                Expression test = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    // "(" test "," ...
                    ret = FinishTestListAsExpr(test);
                } else if (PeekToken(Tokens.KeywordForToken)) {
                    // "(" test "for" ...
                    ret = ParseGeneratorExpression(test);
                } else {
                    // "(" test ")"
                    ret = test is ParenthesisExpression ? test : new ParenthesisExpression(test);
                }
                hasRightParenthesis = Eat(TokenKind.RightParenthesis);
            }

            SourceLocation rStart = GetStart();
            SourceLocation rEnd = GetEnd();

            if (hasRightParenthesis) {
                _sink.MatchPair(new SourceSpan(lStart, lEnd), new SourceSpan(rStart, rEnd), grouping);
            }

            ret.SetLoc(lStart, rEnd);
            return ret;
        }

        //  genexpr_for  ::= "for" expression_list "in" test [genexpr_iter]
        //  genexpr_iter ::= (genexpr_for | genexpr_if) *
        //
        //  "for" has NOT been eaten before entering this method
        private static int genexp_counter;
        private Expression ParseGeneratorExpression(Expression test) {
            ForStatement root = ParseGenExprFor();
            Statement current = root;

            for (; ; ) {
                if (PeekToken(Tokens.KeywordForToken)) {
                    current = NestGenExpr(current, ParseGenExprFor());
                } else if (PeekToken(Tokens.KeywordIfToken)) {
                    current = NestGenExpr(current, ParseGenExprIf());
                } else {
                    YieldStatement ys = new YieldStatement(test);
                    ys.SetLoc(test.Start, test.End);
                    NestGenExpr(current, ys);
                    break;
                }
            }

            // We pass the outermost iterable in as a parameter because Python semantics
            // say that this one piece is computed at definition time rather than iteration time
            SymbolId fname = SymbolTable.StringToId("__gen_" + System.Threading.Interlocked.Increment(ref genexp_counter));
            Parameter parameter = new Parameter(Symbols.GeneratorParmName, 0);
            FunctionDefinition func = new FunctionDefinition(fname, new Parameter[] { parameter }, root, _sourceUnit);
            func.IsGenerator = true;
            func.SetLoc(root.Start, GetEnd());
            func.Header = root.End;

            //  Transform the root "for" statement
            Expression outermost = root.List;
            NameExpression ne = new NameExpression(Symbols.GeneratorParmName);
            ne.SetLoc(outermost.Span);
            root.List = ne;

            GeneratorExpression ret = new GeneratorExpression(func, outermost);
            ret.SetLoc(root.Start, GetEnd());
            return ret;
        }

        private static Statement NestGenExpr(Statement current, Statement nested) {
            ForStatement fes = current as ForStatement;
            IfStatement ifs;
            if (fes != null) {
                fes.Body = nested;
            } else if ((ifs = current as IfStatement) != null) {
                ifs.Tests[0].Body = nested;
            }
            return nested;
        }

        // Python 2.5 -> "for" expression_list "in" or_test
        private ForStatement ParseGenExprFor() {
            SourceLocation start = GetStart();
            Eat(TokenKind.KeywordFor);
            List<Expression> l = ParseExprList();
            Expression lhs = MakeTupleOrExpr(l, false);
            Eat(TokenKind.KeywordIn);

            Expression test = null;
            test = ParseOrTest();

            ForStatement gef = new ForStatement(lhs, test, null, null);
            SourceLocation end = GetEnd();
            gef.SetLoc(start, end);
            gef.Header = end;
            return gef;
        }

        //  Python 2.5 -> genexpr_if   ::= "if" old_test
        private IfStatement ParseGenExprIf() {
            SourceLocation start = GetStart();
            Eat(TokenKind.KeywordIf);
            Expression test = ParseOldTest();
            IfStatementTest ist = new IfStatementTest(test, null);
            SourceLocation end = GetEnd();
            ist.Header = end;
            ist.SetLoc(start, end);
            IfStatement gei = new IfStatement(new IfStatementTest[] { ist }, null);
            gei.SetLoc(start, end);
            return gei;
        }


        //dictmaker: test ':' test (',' test ':' test)* [',']
        private Expression FinishDictValue() {
            SourceLocation oStart = GetStart();
            SourceLocation oEnd = GetEnd();

            List<SliceExpression> l = new List<SliceExpression>();
            bool prevAllow = _allowIncomplete;
            try {
                _allowIncomplete = true;
                while (true) {
                    if (MaybeEat(TokenKind.RightBrace)) {
                        break;
                    }
                    Expression e1 = ParseTest();
                    Eat(TokenKind.Colon);
                    Expression e2 = ParseTest();
                    SliceExpression se = new SliceExpression(e1, e2, null, false);
                    se.SetLoc(e1.Start, e2.End);
                    l.Add(se);

                    if (!MaybeEat(TokenKind.Comma)) {
                        Eat(TokenKind.RightBrace);
                        break;
                    }
                }
            } finally {
                _allowIncomplete = prevAllow;
            }

            SourceLocation cStart = GetStart();
            SourceLocation cEnd = GetEnd();

            _sink.MatchPair(new SourceSpan(oStart, oEnd), new SourceSpan(cStart, cEnd), 1);

            SliceExpression[] exprs = l.ToArray();
            DictionaryExpression ret = new DictionaryExpression(exprs);
            ret.SetLoc(oStart, cEnd);
            return ret;
        }


        //        /*
        //        listmaker: test ( list_for | (',' test)* [','] )
        //        */
        private Expression FinishListValue() {
            SourceLocation oStart = GetStart();
            SourceLocation oEnd = GetEnd();
            int grouping = _tokenizer.GroupingLevel;

            Expression ret;
            if (MaybeEat(TokenKind.RightBracket)) {
                ret = new ListExpression();
            } else {
                Expression t0 = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    List<Expression> l = ParseTestList();
                    Eat(TokenKind.RightBracket);
                    l.Insert(0, t0);
                    ret = new ListExpression(l.ToArray());
                } else if (PeekToken(Tokens.KeywordForToken)) {
                    ret = FinishListComp(t0);
                } else {
                    Eat(TokenKind.RightBracket);
                    ret = new ListExpression(t0);
                }
            }

            SourceLocation cStart = GetStart();
            SourceLocation cEnd = GetEnd();

            _sink.MatchPair(new SourceSpan(oStart, oEnd), new SourceSpan(cStart, cEnd), grouping);

            ret.SetLoc(oStart, cEnd);
            return ret;
        }

        //        list_iter: list_for | list_if
        private ListComprehension FinishListComp(Expression item) {
            List<ListComprehensionIterator> iters = new List<ListComprehensionIterator>();
            ListComprehensionFor firstFor = ParseListCompFor();
            iters.Add(firstFor);

            while (true) {
                if (PeekToken(Tokens.KeywordForToken)) {
                    iters.Add(ParseListCompFor());
                } else if (PeekToken(Tokens.KeywordIfToken)) {
                    iters.Add(ParseListCompIf());
                } else {
                    break;
                }
            }

            Eat(TokenKind.RightBracket);
            return new ListComprehension(item, iters.ToArray());
        }

        // list_for: 'for' exprlist 'in' testlist_safe [list_iter]
        private ListComprehensionFor ParseListCompFor() {
            Eat(TokenKind.KeywordFor);
            SourceLocation start = GetStart();
            List<Expression> l = ParseExprList();

            // expr list is something like:
            //  ()
            //  a
            //  a,b
            //  a,b,c
            // we either want just () or a or we want (a,b) and (a,b,c)
            // so we can do tupleExpr.EmitSet() or loneExpr.EmitSet()

            Expression lhs = MakeTupleOrExpr(l, false);
            Eat(TokenKind.KeywordIn);

            Expression list = null;
            list = ParseTestListAsSafeExpr();

            ListComprehensionFor ret = new ListComprehensionFor(lhs, list);

            ret.SetLoc(start, GetEnd());
            return ret;
        }

        // list_if: 'if' old_test [list_iter]
        private ListComprehensionIf ParseListCompIf() {
            Eat(TokenKind.KeywordIf);
            SourceLocation start = GetStart();
            Expression test = ParseOldTest();
            ListComprehensionIf ret = new ListComprehensionIf(test);

            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private Expression FinishBackquote() {
            Expression ret;
            SourceLocation start = GetStart();
            Expression expr = ParseTestListAsExpr(false);
            Eat(TokenKind.BackQuote);
            ret = new BackQuoteExpression(expr);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private static Expression MakeTupleOrExpr(List<Expression> l, bool trailingComma) {
            return MakeTupleOrExpr(l, trailingComma, false);
        }

        private static Expression MakeTupleOrExpr(List<Expression> l, bool trailingComma, bool expandable) {
            if (l.Count == 1 && !trailingComma) return l[0];

            Expression[] exprs = l.ToArray();
            TupleExpression te = new TupleExpression(expandable && !trailingComma, exprs);
            if (exprs.Length > 0) {
                te.SetLoc(exprs[0].Start, exprs[exprs.Length - 1].End);
            }
            return te;
        }

        private static bool NeverTestToken(Token t) {
            switch (t.Kind) {
                case TokenKind.AddEqual:
                case TokenKind.SubtractEqual:
                case TokenKind.MultiplyEqual:
                case TokenKind.DivideEqual:
                case TokenKind.ModEqual:
                case TokenKind.BitwiseAndEqual:
                case TokenKind.BitwiseOrEqual:
                case TokenKind.XorEqual:
                case TokenKind.LeftShiftEqual:
                case TokenKind.RightShiftEqual:
                case TokenKind.PowerEqual:
                case TokenKind.FloorDivideEqual:

                case TokenKind.Indent:
                case TokenKind.Dedent:
                case TokenKind.NewLine:
                case TokenKind.Semicolon:

                case TokenKind.Assign:
                case TokenKind.RightBrace:
                case TokenKind.RightBracket:
                case TokenKind.RightParenthesis:

                case TokenKind.Comma:

                case TokenKind.KeywordFor:
                case TokenKind.KeywordIn:
                case TokenKind.KeywordIf:
                    return true;

                default: return false;
            }
        }

        private FunctionDefinition CurrentFunction {
            get {
                if (_functions != null && _functions.Count > 0) {
                    return _functions.Peek();
                }
                return null;
            }
        }

        private FunctionDefinition PopFunction() {
            if (_functions != null && _functions.Count > 0) {
                return _functions.Pop();
            }
            return null;
        }

        private void PushFunction(FunctionDefinition function) {
            if (_functions == null) {
                _functions = new Stack<FunctionDefinition>();
            }
            _functions.Push(function);
        }

        private CallExpression FinishCallExpr(Expression target, params Arg[] args) {
            bool hasArgsTuple = false;
            bool hasKeywordDict = false;
            int keywordCount = 0;
            int extraArgs = 0;

            foreach (Arg arg in args) {
                if (arg.Name == SymbolId.Empty) {
                    if (hasArgsTuple || hasKeywordDict || keywordCount > 0) {
                        ReportSyntaxError(IronPython.Resources.NonKeywordAfterKeywordArg);
                    }
                } else if (arg.Name == Symbols.Star) {
                    if (hasArgsTuple || hasKeywordDict) {
                        ReportSyntaxError(IronPython.Resources.OneListArgOnly);
                    }
                    hasArgsTuple = true; extraArgs++;
                } else if (arg.Name == Symbols.StarStar) {
                    if (hasKeywordDict) {
                        ReportSyntaxError(IronPython.Resources.OneKeywordArgOnly);
                    }
                    hasKeywordDict = true; extraArgs++;
                } else {
                    if (hasArgsTuple || hasKeywordDict) {
                        ReportSyntaxError(IronPython.Resources.KeywordOutOfSequence);
                    }
                    keywordCount++;
                }
            }

            return new CallExpression(target, args, hasArgsTuple, hasKeywordDict, keywordCount, extraArgs);
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            if (_sourceReader != null) {
                _sourceReader.Close();
            }
        }

        #endregion
    }
}
