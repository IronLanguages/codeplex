/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.IO;
using System.Text;

using IronPython.Runtime;
using IronPython.Hosting;
using System.Diagnostics;

using System.Collections.Generic;
using IronPython.Compiler.Ast;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;

namespace IronPython.Compiler {
    /// <summary>
    /// Summary description for Parser.
    /// </summary>
    public class Parser {
        private readonly CompilerContext context;
        private Tokenizer tokenizer;
        private Token peekedToken;
        private Location savedStart;
        private Location savedEnd;
        private ExternalLineMapping savedExternal;
        private Stack<FunctionDefinition> functions;
        private bool fromFutureAllowed = true;
        private bool allowingIncomplete;
        private string privatePrefix;


        public CompilerContext CompilerContext {
            get { return context; }
        }

        #region Methods to create parser instance

        public static Parser FromString(SystemState state, CompilerContext context, string text) {
            return new Parser(context, new Tokenizer(state, context, text.ToCharArray()));
        }

        public static Parser FromFile(SystemState state, CompilerContext context) {
            return FromFile(state, context, false, false);
        }

        public static Parser FromFile(SystemState state, CompilerContext context, bool skipLine, bool verbatim) {
            string data;
            string fileName = context.SourceFile;

            // we choose ASCII by default, if the file has a Unicode header though
            // we'll automatically get it as unicode.
            Encoding encType = System.Text.Encoding.ASCII;

            if (fileName == "<stdin>") {
                Stream s = Console.OpenStandardInput();
                using (StreamReader sr = new StreamReader(s, encType)) {
                    if (skipLine) {
                        sr.ReadLine();
                    }
                    data = sr.ReadToEnd();
                }
                return new Parser(context, new Tokenizer(data.ToCharArray(), verbatim, state, context));
            }

            byte[] bytes = File.ReadAllBytes(fileName);

            using (StreamReader sr = new StreamReader(new MemoryStream(bytes, false), System.Text.Encoding.ASCII)) {
                string line = sr.ReadLine();
                bool gotEncoding = false;
                // magic encoding must be on line 1 or 2
                if (line != null && !(gotEncoding = TryGetEncoding(state, line, ref encType))) {
                    line = sr.ReadLine();

                    if (line != null) {
                        gotEncoding = TryGetEncoding(state, line, ref encType);
                    }
                }

                if (gotEncoding &&
                    sr.CurrentEncoding != Encoding.ASCII &&
                    encType != sr.CurrentEncoding) {
                    // we have both a BOM & an encoding type, throw an error
                    context.Sink.AddError(fileName, "file has both Unicode marker and PEP-263 file encoding", String.Empty, CodeSpan.Empty, 0, Severity.Error);
                }
            }

            if (encType == null) context.AddError("unknown encoding type", "<unknown>", 0, 0, 0, 0, Severity.Error);

            // re-read w/ the correct encoding type...
            using (StreamReader sr = new StreamReader(new MemoryStream(bytes), encType)) {
                if (skipLine) {
                    sr.ReadLine();
                }
                data = sr.ReadToEnd();
            }

            return new Parser(context, new Tokenizer(data.ToCharArray(), verbatim, state, context));
        }

        #endregion

        private Parser(CompilerContext context, Tokenizer tokenizer) {
            this.tokenizer = tokenizer;
            this.context = context;
        }

        private Token PeekToken() {
            if (peekedToken != null) return peekedToken;

            savedStart = tokenizer.StartLocation;
            savedEnd = tokenizer.EndLocation;
            savedExternal = tokenizer.ExternalLineLocation;

            Token p = NextToken();
            peekedToken = p;

            return p;
        }

        private static bool TryGetEncoding(SystemState state, string line, ref Encoding enc) {
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
            return StringOps.TryGetEncoding(state, encName, out enc);
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

        private Token NextToken() {
            if (peekedToken != null) {
                Token ret = peekedToken;
                peekedToken = null;
                return ret;
            }
            return tokenizer.Next();
        }

        private bool Eat(TokenKind kind) {
            Token t = NextToken();
            if (t.Kind != kind) {
                ReportSyntaxError(t);
                return false;
            }
            return true;
        }

        private bool EatNoEof(TokenKind kind) {
            Token t = NextToken();
            if (t.Kind != kind) {
                ReportSyntaxError(t, ErrorCodes.SyntaxError, false);
                return false;
            }
            return true;
        }

        private bool MaybeEat(TokenKind kind) {
            Token t = PeekToken();
            if (t.Kind == kind) {
                NextToken();
                return true;
            } else {
                return false;
            }
        }

        private void EatName(SymbolId id) {
            if (!PeekName(id))
                ReportSyntaxError(String.Concat("Unexpected Symbol Id :", id));
            NextToken();
        }

        private Location GetStart() {
            if (peekedToken == null) return tokenizer.StartLocation;
            else return savedStart;
        }

        private Location GetEnd() {
            if (peekedToken == null) return tokenizer.EndLocation;
            else return savedEnd;
        }

        private ExternalLineMapping GetExternal() {
            if (peekedToken == null) return tokenizer.ExternalLineLocation;
            else return savedExternal;
        }

        private CodeSpan GetSpan() {
            if (peekedToken == null) {
                return new CodeSpan(tokenizer.StartLocation, tokenizer.EndLocation);
            } else {
                return new CodeSpan(savedStart, savedEnd);
            }
        }

        private void ReportSyntaxError(Token t) {
            ReportSyntaxError(t, ErrorCodes.SyntaxError);
        }

        private void ReportSyntaxError(Token t, int errorCode) {
            ReportSyntaxError(t, errorCode, true);
        }

        private void ReportSyntaxError(Token t, int errorCode, bool allowIncomplete) {
            Location start = GetStart();
            if (t.Kind == TokenKind.NewLine || t.Kind == TokenKind.Dedent) {
                if (tokenizer.IsEndOfFile) {
                    t = EatEndOfInput();
                }
            }
            if (allowIncomplete && t.Kind == TokenKind.EndOfFile) {
                errorCode |= ErrorCodes.IncompleteStatement;
            }
            ReportSyntaxError(start, tokenizer.EndLocation, String.Format("unexpected token {0}", t.Image), errorCode);
        }

        private void ReportSyntaxError(string message) {
            ReportSyntaxError(GetStart(), GetEnd(), message);
        }

        internal void ReportSyntaxError(Location start, Location end, string message) {
            ReportSyntaxError(start, end, message, ErrorCodes.SyntaxError);
        }

        internal void ReportSyntaxError(Location start, Location end, string message, int errorCode) {
            string lineText = tokenizer.GetRawLineForError(start.Line);
            context.AddError(message, lineText, start.Line, start.Column, end.Line, end.Column, errorCode, Severity.Error);
        }

        private static bool IsPrivateName(SymbolId name) {
            string s = name.GetString();
            return s.StartsWith("__") && !s.EndsWith("__");
        }

        private SymbolId FixName(SymbolId name) {
            if (privatePrefix != null && IsPrivateName(name)) {
                name = SymbolTable.StringToId(string.Format("_{0}{1}", privatePrefix, name.GetString()));
            }

            return name;
        }

        private SymbolId ReadNameMaybeNone() {
            Token t = NextToken();
            if (t == Tokens.NoneToken) {
                return SymbolTable.None;
            }

            NameToken n = t as NameToken;
            if (n == null) {
                ReportSyntaxError(t);
                return SymbolTable.Empty;
            }
            return FixName(n.Name);
        }

        private SymbolId ReadName() {
            Token t = NextToken();
            NameToken n = t as NameToken;
            if (n == null) {
                ReportSyntaxError(t);
                return SymbolTable.Empty;
            }
            return FixName(n.Name);
        }

        internal Statement ParseFunction() {
            PushFunction(new FunctionDefinition(SymbolTable.Empty, new Expression[0], new Expression[0], FunctionAttributes.None, context.SourceFile));
            try {
                return ParseFileInput();
            } finally {
                PopFunction();
            }
        }
        #region Public parser interface

        //single_input: Newline | simple_stmt | compound_stmt Newline
        //eval_input: testlist Newline* ENDMARKER
        //file_input: (Newline | stmt)* ENDMARKER
        public Statement ParseFileInput() {
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
                fromFutureAllowed = false;
                if (s is ExpressionStatement) {
                    ConstantExpression ce = ((ExpressionStatement)s).Expression as ConstantExpression;
                    if (ce != null && ce.Value is string) {
                        // doc string
                        fromFutureAllowed = true;
                    }
                }
            }

            while (MaybeEat(TokenKind.NewLine)) ;

            // from __future__
            if (fromFutureAllowed) {
                while (PeekToken(Tokens.KeywordFromToken)) {
                    Statement s = ParseStmt();
                    l.Add(s);
                    if (s is FromImportStatement) {
                        FromImportStatement fis = (FromImportStatement)s;
                        if (!fis.IsFromFuture) {
                            // end of from __future__
                            break;
                        }
                    }
                }
            }

            // the end of from __future__ sequence
            fromFutureAllowed = false;

            while (true) {
                if (MaybeEat(TokenKind.EndOfFile)) break;
                if (MaybeEat(TokenKind.NewLine)) continue;

                Statement s = ParseStmt();
                l.Add(s);
            }

            Statement[] stmts = l.ToArray();

            SuiteStatement ret = new SuiteStatement(stmts);
            Location start = new Location();
            start.Column = start.Line = 1;
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        static readonly char[] newLineChar = new char[] { '\n' };
        static readonly char[] whiteSpace = { ' ', '\t' };

        // Given the interactive text input for a compound statement, calculate what the
        // indentation level of the next line should be
        public static int GetNextAutoIndentSize(string text, int autoIndentTabWidth) {
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
        public Statement ParseInteractiveInput(bool allowIncompleteStatement, out bool isEmptyStmt) {
            bool parsingMultiLineCmpdStmt;
            isEmptyStmt = false;
            try {
                Statement ret = InternalParseInteractiveInput(out parsingMultiLineCmpdStmt, out isEmptyStmt);
                if ((parsingMultiLineCmpdStmt && allowIncompleteStatement) || isEmptyStmt) return null;
                else return ret;
            } catch (PythonSyntaxErrorException se) {
                // Check if it's a real syntax error, or if its just an incomplete multi-line statement
                if ((se.ErrorCode & ErrorCodes.IncompleteMask) != 0) {
                    if ((se.ErrorCode & ErrorCodes.IncompleteToken) != 0) {
                        return null;
                    }
                    if ((se.ErrorCode & ErrorCodes.IncompleteStatement) != 0) {
                        if (allowIncompleteStatement) return null;
                    }
                }

                // It looks like a real syntax error. Rethrow the exception
                throw;
            }
        }

        private Statement InternalParseInteractiveInput(out bool parsingMultiLineCmpdStmt, out bool isEmptyStmt) {
            Statement s;
            isEmptyStmt = false;
            parsingMultiLineCmpdStmt = false;
            Token t = PeekToken();
            switch (t.Kind) {
                case TokenKind.NewLine:
                    EatOptionalNewlines();
                    Eat(TokenKind.EndOfFile);
                    isEmptyStmt = true;
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
                    if (context.AllowWithStatement && PeekName(SymbolTable.With)) {
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



        public Statement ParseSingleStatement() {
            EatOptionalNewlines();
            Statement statement = ParseStmt();
            EatEndOfInput();
            return statement;
        }

        public Expression ParseTestListAsExpression() {
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
                ReportSyntaxError(t);
            }
            return t;
        }



        #endregion

        //stmt: simple_stmt | compound_stmt
        //compound_stmt: if_stmt | while_stmt | for_stmt | try_stmt | funcdef | classdef
        private Statement ParseStmt() {
            Token t = PeekToken();

            switch (t.Kind) {
                case TokenKind.KeywordIf:
                    return ParseIfStmt();
                case TokenKind.KeywordWhile:
                    return ParseWhileStmt();
                case TokenKind.KeywordFor:
                    return ParseForStmt();
                case TokenKind.KeywordTry:
                    return ParseTryStmt();
                case TokenKind.At:
                    return ParseDecoratedFuncDef();
                case TokenKind.KeywordDef:
                    return ParseFuncDef();
                case TokenKind.KeywordClass:
                    return ParseClassDef();
                default:
                    if (context.AllowWithStatement && PeekName(SymbolTable.With)) {
                        return ParseWithStmt();
                    }
                    return ParseSimpleStmt();
            }
        }

        //simple_stmt: small_stmt (';' small_stmt)* [';'] Newline
        private Statement ParseSimpleStmt() {
            Statement s = ParseSmallStmt();
            if (MaybeEat(TokenKind.Semicolon)) {
                Location start = s.Start;
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
                ret.SetLoc(GetExternal(), start, GetEnd());
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
            Token t = PeekToken();
            switch (t.Kind) {
                case TokenKind.KeywordPrint:
                    return ParsePrintStmt();
                case TokenKind.KeywordPass:
                    return FinishSmallStmt(new PassStatement());
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
            Location start = GetStart();
            List<Expression> l = ParseExprList();
            DelStatement ret = new DelStatement(l.ToArray());
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        private Statement ParseReturnStmt() {
            if (CurrentFunction == null) {
                ReportSyntaxError("'return' outside function");
            }
            NextToken();
            Expression expr = null;
            Location start = GetStart();
            if (!NeverTestToken(PeekToken())) {
                expr = ParseTestListAsExpr(true);
            }
            ReturnStatement ret = new ReturnStatement(expr);
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        private Statement FinishSmallStmt(Statement stmt) {
            NextToken();
            stmt.SetLoc(GetExternal(), GetStart(), GetEnd());
            return stmt;
        }

        private Statement ParseYieldStmt() {
            NextToken();
            Location start = GetStart();
            FunctionDefinition current = CurrentFunction;
            int yieldId = 0;
            if (current == null) {
                ReportSyntaxError("'yield' outside function");
            } else {
                yieldId = current.YieldCount++;
            }
            Expression e = ParseTestListAsExpr(false);
            YieldStatement ret = new YieldStatement(e, yieldId);
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //expr_stmt: testlist (augassign testlist | ('=' testlist)*)
        //augassign: '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '<<=' | '>>=' | '**=' | '//='
        private Statement ParseExprStmt() {

            Expression lhs = ParseTestListAsExpr(false);

            if (MaybeEat(TokenKind.Assign)) {
                List<Expression> l = new List<Expression>();
                l.Add(lhs);
                do {
                    Expression e = ParseTestListAsExpr(false);
                    l.Add(e);
                } while (MaybeEat(TokenKind.Assign));

                int last = l.Count - 1;
                Expression rhs = (Expression)l[last];
                l.RemoveAt(last);
                Expression[] lhss = l.ToArray();

                //We check for legal assignment targets during code generation rather than parsing
                Statement ret = new AssignStatement(lhss, rhs);
                ret.SetLoc(GetExternal(), lhs.Start, GetEnd());
                return ret;
            } else {
                BinaryOperator op = GetAssignOp(PeekToken());
                if (op == null) {
                    Statement ret = new ExpressionStatement(lhs);
                    ret.SetLoc(GetExternal(), lhs.Start, GetEnd());
                    return ret;
                } else {
                    NextToken();
                    Expression rhs = ParseTestListAsExpr(false);
                    Statement ret = new AugAssignStatement(op, lhs, rhs);
                    ret.SetLoc(GetExternal(), lhs.Start, GetEnd());
                    return ret;
                }
            }
        }

        //        private bool isExprStmtSep(Token t) {
        //            return isEndOfStmt(t) || getAssignOp(t) != null || t == TokenKind.AssignToken;
        //        }
        //
        //        private bool isEndOfStmt(Token t) {
        //            return t.kind == TokenKind.NEWLINE || t.kind == TokenKind.Semicolon;
        //        }

        private static BinaryOperator GetAssignOp(Token t) {
            switch (t.Kind) {
                case TokenKind.AddEqual: return PythonOperator.Add;
                case TokenKind.SubtractEqual: return PythonOperator.Subtract;
                case TokenKind.MultiplyEqual: return PythonOperator.Multiply;
                case TokenKind.DivEqual: return PythonOperator.Divide;
                case TokenKind.ModEqual: return PythonOperator.Mod;
                case TokenKind.BitwiseAndEqual: return PythonOperator.BitwiseAnd;
                case TokenKind.BitwiseOrEqual: return PythonOperator.BitwiseOr;
                case TokenKind.XorEqual: return PythonOperator.Xor;
                case TokenKind.LeftShiftEqual: return PythonOperator.LeftShift;
                case TokenKind.RightShiftEqual: return PythonOperator.RightShift;
                case TokenKind.PowerEqual: return PythonOperator.Power;
                case TokenKind.FloorDivideEqual: return PythonOperator.FloorDivide;
                default: return null;
            }
        }

        //import_stmt: 'import' dotted_as_name (',' dotted_as_name)*
        private ImportStatement ParseImportStmt() {
            Eat(TokenKind.KeywordImport);
            Location start = GetStart();

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
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //| 'from' dotted_name 'import' ('*' | as_name_list | '(' as_name_list ')' )
        private FromImportStatement ParseFromImportStmt() {
            Eat(TokenKind.KeywordFrom);
            Location start = GetStart();
            DottedName dname = ParseDottedName();

            Eat(TokenKind.KeywordImport);

            IList<SymbolId> names;
            IList<SymbolId> asNames;
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

            if (dname.Names.Count == 1 && dname.Names[0] == SymbolTable.Future) {
                if (!fromFutureAllowed) {
                    ReportSyntaxError("from __future__ imports must occur at the beginning of the file");
                }
                if (names == FromImportStatement.Star) {
                    ReportSyntaxError("future statement does not support import *");
                }
                fromFuture = true;
                foreach (SymbolId name in names) {
                    if (name == SymbolTable.Division) {
                        context.TrueDivision = true;
                    } else if (Options.Python25 && name == SymbolTable.WithStmt) {
                        context.AllowWithStatement = true;
                    } else if (name == SymbolTable.NestedScopes) {
                    } else if (name == SymbolTable.Generators) {
                    } else {
                        fromFuture = false;
                        ReportSyntaxError(string.Format("future feature is not defined: {0}", name.GetString()));
                    }
                }
            }

            FromImportStatement ret = new FromImportStatement(dname, names, asNames, fromFuture);
            ret.SetLoc(GetExternal(), start, GetEnd());
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
            if (t != null && t.Name == SymbolTable.As) {
                NextToken();
                return ReadName();
            }
            return SymbolTable.Empty;
        }

        //dotted_name: NAME ('.' NAME)*
        private DottedName ParseDottedName() {
            Location start = GetStart();
            List<SymbolId> l = new List<SymbolId>();
            l.Add(ReadName());
            while (MaybeEat(TokenKind.Dot)) {
                l.Add(ReadName());
            }
            SymbolId[] names = l.ToArray();
            DottedName ret = new DottedName(names);
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //exec_stmt: 'exec' expr ['in' test [',' test]]
        private ExecStatement ParseExecStmt() {
            Eat(TokenKind.KeywordExec);
            Location start = GetStart();
            Expression code, locals = null, globals = null;
            code = ParseExpr();
            if (MaybeEat(TokenKind.KeywordIn)) {
                globals = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    locals = ParseTest();
                }
            }
            ExecStatement ret = new ExecStatement(code, locals, globals);
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //global_stmt: 'global' NAME (',' NAME)*
        private GlobalStatement ParseGlobalStmt() {
            Eat(TokenKind.KeywordGlobal);
            Location start = GetStart();
            List<SymbolId> l = new List<SymbolId>();
            l.Add(ReadName());
            while (MaybeEat(TokenKind.Comma)) {
                l.Add(ReadName());
            }
            SymbolId[] names = l.ToArray();
            GlobalStatement ret = new GlobalStatement(names);
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //raise_stmt: 'raise' [test [',' test [',' test]]]
        private RaiseStatement ParseRaiseStmt() {
            Eat(TokenKind.KeywordRaise);
            Location start = GetStart();
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
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //assert_stmt: 'assert' test [',' test]
        private AssertStatement ParseAssertStmt() {
            Eat(TokenKind.KeywordAssert);
            Location start = GetStart();
            Expression test = ParseTest();
            Expression message = null;
            if (MaybeEat(TokenKind.Comma)) {
                message = ParseTest();
            }
            AssertStatement ret = new AssertStatement(test, message);
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //print_stmt: 'print' ( [ test (',' test)* [','] ] | '>>' test [ (',' test)+ [','] ] )
        private PrintStatement ParsePrintStmt() {
            Eat(TokenKind.KeywordPrint);
            Location start = GetStart();
            Expression dest = null;
            PrintStatement ret;

            bool needNonEmptyTestList = false;
            if (MaybeEat(TokenKind.RightShift)) {
                dest = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    needNonEmptyTestList = true;
                } else {
                    ret = new PrintStatement(dest, new Expression[0], false);
                    ret.SetLoc(GetExternal(), start, GetEnd());
                    return ret;
                }
            }

            bool trailingComma;
            List<Expression> l = ParseTestList(out trailingComma);
            if (needNonEmptyTestList && l.Count == 0) {
                ReportSyntaxError(PeekToken());
            }
            Expression[] exprs = l.ToArray();
            ret = new PrintStatement(dest, exprs, trailingComma);
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        public string SetPrivatePrefix(SymbolId name) {
            // Remove any leading underscores before saving the prefix
            string oldPrefix = privatePrefix;

            if (name != SymbolTable.Empty) {
                string prefixString = name.GetString();
                for (int i = 0; i < prefixString.Length; i++) {
                    if (prefixString[i] != '_') {
                        privatePrefix = prefixString.Substring(i);
                        return oldPrefix;
                    }
                }
            }
            // Name consists of '_'s only, no private prefix mapping
            privatePrefix = null;
            return oldPrefix;
        }

        //classdef: 'class' NAME ['(' testlist ')'] ':' suite
        private ClassDefinition ParseClassDef() {
            Eat(TokenKind.KeywordClass);
            Location start = GetStart();
            SymbolId name = ReadName();
            Expression[] bases = new Expression[0];
            if (MaybeEat(TokenKind.LeftParenthesis)) {
                List<Expression> l = ParseTestList();
                if (!Options.Python25 && l.Count == 0) {
                    ReportSyntaxError(PeekToken());
                }
                bases = l.ToArray();
                Eat(TokenKind.RightParenthesis);
            }
            Location mid = GetEnd();

            // Save private prefix
            string savedPrefix = SetPrivatePrefix(name);

            // Parse the class body
            Statement body = ParseSuite();

            // Restore the private prefix
            privatePrefix = savedPrefix;

            ClassDefinition ret = new ClassDefinition(name, bases, body);
            ret.Header = mid;
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }


        //  decorators ::=
        //      decorator+
        //  decorator ::=
        //      "@" dotted_name ["(" [argument_list [","]] ")"] NEWLINE
        private List<Expression> ParseDecorators() {
            List<Expression> decorators = new List<Expression>();

            while (MaybeEat(TokenKind.At)) {
                Location start = GetStart();
                Expression decorator = new NameExpression(ReadName());
                decorator.SetLoc(GetExternal(), start, GetEnd());
                while (MaybeEat(TokenKind.Dot)) {
                    SymbolId name = ReadNameMaybeNone();
                    decorator = new FieldExpression(decorator, name);
                    decorator.SetLoc(GetExternal(), GetStart(), GetEnd());
                }
                decorator.SetLoc(GetExternal(), start, GetEnd());

                if (MaybeEat(TokenKind.LeftParenthesis)) {
                    context.Sink.StartParameters(GetSpan());
                    Arg[] args = FinishArgumentList(null);
                    decorator = FinishCallExpr(decorator, args);
                }
                decorator.SetLoc(GetExternal(), start, GetEnd());
                Eat(TokenKind.NewLine);

                decorators.Add(decorator);
            }

            return decorators;
        }

        // funcdef: [decorators] 'def' NAME parameters ':' suite
        // this gets called with "@" look-ahead
        private FunctionDefinition ParseDecoratedFuncDef() {
            Location start = GetStart();
            List<Expression> decorators = ParseDecorators();
            FunctionDefinition fnc = ParseFuncDef();
            Expression root = new NameExpression(fnc.Name);
            root.SetLoc(GetExternal(), start, GetEnd());

            for (int i = decorators.Count; i > 0; i--) {
                Expression decorator = (Expression)decorators[i - 1];
                root = FinishCallExpr(decorator, new Arg(root));
                root.SetLoc(GetExternal(), decorator.Start, decorator.End);
            }
            fnc.Decorators = root;

            return fnc;
        }

        // funcdef: [decorators] 'def' NAME parameters ':' suite
        // parameters: '(' [varargslist] ')'
        // this gets called with "def" as the look-ahead
        private FunctionDefinition ParseFuncDef() {
            Eat(TokenKind.KeywordDef);
            Location start = GetStart();
            SymbolId name = ReadName();

            Eat(TokenKind.LeftParenthesis);

            Location lStart = GetStart(), lEnd = GetEnd();
            int grouping = tokenizer.GroupingLevel;

            Expression[] parameters, defaults;
            FunctionAttributes flags;
            ParseVarArgsList(out parameters, out defaults, out flags, TokenKind.RightParenthesis);

            Location rStart = GetStart(), rEnd = GetEnd();

            FunctionDefinition ret = new FunctionDefinition(name, parameters, defaults, flags, context.SourceFile);
            PushFunction(ret);

            Statement body = ParseSuite();
            FunctionDefinition ret2 = PopFunction();
            System.Diagnostics.Debug.Assert(ret == ret2);

            ret.Body = body;
            ret.Header = rEnd;

            context.Sink.MatchPair(new CodeSpan(lStart, lEnd), new CodeSpan(rStart, rEnd), grouping);

            ret.SetLoc(GetExternal(), start, GetEnd());

            return ret;
        }

        private NameExpression ParseNameExpr(Dictionary<SymbolId, SymbolId> names) {
            SymbolId name = ReadName();
            if (name != SymbolTable.Empty) {
                CheckUniqueParameter(names, name);
            }
            NameExpression ne = new NameExpression(name);
            ne.SetLoc(GetExternal(), GetStart(), GetEnd());
            return ne;
        }

        private void CheckUniqueParameter(Dictionary<SymbolId, SymbolId> names, SymbolId name) {
            if (names.ContainsKey(name)) {
                ReportSyntaxError(string.Format("duplicate argument '{0}' in function definition", name.GetString()));
            }
            names[name] = name;
        }

        //varargslist: (fpdef ['=' test] ',')* ('*' NAME [',' '**' NAME] | '**' NAME) | fpdef ['=' test] (',' fpdef ['=' test])* [',']
        //fpdef: NAME | '(' fplist ')'
        //fplist: fpdef (',' fpdef)* [',']
        private void ParseVarArgsList(out Expression[] parameters, out Expression[] defaults, out FunctionAttributes flags, TokenKind terminator) {
            // parameters not doing * or ** today
            List<Expression> al = new List<Expression>();
            List<Expression> dl = new List<Expression>();
            Dictionary<SymbolId, SymbolId> names = new Dictionary<SymbolId, SymbolId>();
            bool needDefault = false;
            flags = FunctionAttributes.None;
            while (true) {
                if (MaybeEat(terminator)) break;

                if (MaybeEat(TokenKind.Multiply)) {
                    al.Add(ParseNameExpr(names));
                    flags |= FunctionAttributes.ArgumentList;
                    if (MaybeEat(TokenKind.Comma)) {
                        Eat(TokenKind.Power);
                        al.Add(ParseNameExpr(names));
                        flags |= FunctionAttributes.KeywordDictionary;
                    }
                    Eat(terminator);
                    break;
                } else if (MaybeEat(TokenKind.Power)) {
                    al.Add(ParseNameExpr(names));
                    flags |= FunctionAttributes.KeywordDictionary;
                    Eat(terminator);
                    break;
                }

                //
                //  Parsing defparameter:
                //
                //  defparameter ::=
                //      parameter ["=" expression]

                al.Add(ParseParameter(names));
                if (MaybeEat(TokenKind.Assign)) {
                    needDefault = true;
                    dl.Add(ParseTest());
                } else if (needDefault) {
                    ReportSyntaxError("default value must be specified here");
                }
                if (!MaybeEat(TokenKind.Comma)) {
                    Eat(terminator);
                    break;
                }
            }

            parameters = al.ToArray();
            defaults = dl.ToArray();
        }

        //  parameter ::=
        //      identifier | "(" sublist ")"

        Expression ParseParameter(Dictionary<SymbolId, SymbolId> names) {
            Token t = NextToken();
            Expression ret = null;
            switch (t.Kind) {
                case TokenKind.LeftParenthesis: // sublist
                    ret = ParseSublist(names);
                    Eat(TokenKind.RightParenthesis);
                    break;
                case TokenKind.Name:  // identifier
                    CodeSpan span = GetSpan();
                    SymbolId name = (SymbolId)t.Value;
                    context.Sink.StartName(span, name.GetString());
                    name = FixName(name);
                    CheckUniqueParameter(names, name);
                    ret = new NameExpression(name);
                    ret.SetLoc(GetExternal(), span);
                    break;
                default:
                    ReportSyntaxError(t);
                    ret = new ErrorExpression();
                    ret.SetLoc(GetExternal(), GetStart(), GetEnd());
                    break;
            }
            return ret;
        }

        //  sublist ::=
        //      parameter ("," parameter)* [","]
        Expression ParseSublist(Dictionary<SymbolId, SymbolId> names) {
            bool trailingComma;
            List<Expression> list = new List<Expression>();
            for (; ; ) {
                trailingComma = false;
                list.Add(ParseParameter(names));
                if (MaybeEat(TokenKind.Comma)) {
                    trailingComma = true;
                    Token peek = PeekToken();
                    switch (peek.Kind) {
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
            Location start = GetStart();
            Expression[] parameters, defaults;
            FunctionAttributes flags;
            ParseVarArgsList(out parameters, out defaults, out flags, TokenKind.Colon);
            Location mid = GetEnd();

            Expression expr = ParseOldTest();
            Statement body = new ReturnStatement(expr);
            body.SetLoc(GetExternal(), expr.Start, expr.End);
            FunctionDefinition func = new FunctionDefinition(SymbolTable.StringToId("<lambda$" + (oldLambdaCount++) + ">"), parameters, defaults, flags, body, context.SourceFile);
            func.SetLoc(GetExternal(), start, GetEnd());
            func.Header = mid;
            LambdaExpression ret = new LambdaExpression(func);
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }


        //lambdef: 'lambda' [varargslist] ':' test
        private int lambdaCount;
        private Expression FinishLambdef() {
            Location start = GetStart();
            Expression[] parameters, defaults;
            FunctionAttributes flags;
            ParseVarArgsList(out parameters, out defaults, out flags, TokenKind.Colon);
            Location mid = GetEnd();

            Expression expr = ParseTest();
            Statement body = new ReturnStatement(expr);
            body.SetLoc(GetExternal(), expr.Start, expr.End);
            FunctionDefinition func = new FunctionDefinition(SymbolTable.StringToId("<lambda$" + (lambdaCount++) + ">"), parameters, defaults, flags, body, context.SourceFile);
            func.SetLoc(GetExternal(), start, GetEnd());
            func.Header = mid;
            LambdaExpression ret = new LambdaExpression(func);
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //while_stmt: 'while' test ':' suite ['else' ':' suite]
        private WhileStatement ParseWhileStmt() {
            Eat(TokenKind.KeywordWhile);
            Location start = GetStart();
            Expression test = ParseTest();
            Location mid = GetEnd();
            Statement body = ParseSuite();
            Statement else_ = null;
            if (MaybeEat(TokenKind.KeywordElse)) {
                else_ = ParseSuite();
            }
            WhileStatement ret = new WhileStatement(test, body, else_);
            ret.SetLoc(GetExternal(), start, mid, GetEnd());
            return ret;
        }

        //with_stmt: 'with' test [ 'as' with_var ] ':' suite
        private WithStatement ParseWithStmt() {
            EatName(SymbolTable.With);
            Location start = GetStart();
            Expression contextManager = ParseTest();
            Expression var = null;
            if (PeekName(SymbolTable.As)) {
                EatName(SymbolTable.As);
                var = ParseTest();
            }

            Location header = GetEnd();
            Statement body = ParseSuite();
            WithStatement ret = new WithStatement(contextManager, var, body);
            ret.Header = header;
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //for_stmt: 'for' exprlist 'in' testlist ':' suite ['else' ':' suite]
        private ForStatement ParseForStmt() {
            Eat(TokenKind.KeywordFor);
            Location start = GetStart();
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
            Location header = GetEnd();
            Statement body = ParseSuite();
            Statement else_ = null;
            if (MaybeEat(TokenKind.KeywordElse)) {
                else_ = ParseSuite();
            }
            ForStatement ret = new ForStatement(lhs, list, body, else_);
            ret.Header = header;
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        // if_stmt: 'if' test ':' suite ('elif' test ':' suite)* ['else' ':' suite]
        private IfStatement ParseIfStmt() {
            Eat(TokenKind.KeywordIf);
            Location start = GetStart();
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
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        private IfStatementTest ParseIfStmtTest() {
            Location start = GetStart();
            Expression test = ParseTest();
            Location header = GetEnd();
            Statement suite = ParseSuite();
            IfStatementTest ret = new IfStatementTest(test, suite);
            ret.SetLoc(GetExternal(), start, suite.End);
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


        private Statement ParseTryStmt() {
            Eat(TokenKind.KeywordTry);
            Location start = GetStart();
            Location mid = GetEnd();
            Statement body = ParseSuite();
            Statement finallySuite = null;
            TryStatementHandler[] handlers = null;
            Statement elseSuite = null;
            Statement ret;

            if (MaybeEat(TokenKind.KeywordFinally)) {
                finallySuite = ParseSuite();
                TryStatement tfs = new TryStatement(body, handlers, elseSuite, finallySuite);
                tfs.Header = mid;
                ret = tfs;
            } else {
                List<TryStatementHandler> l = new List<TryStatementHandler>();
                do {
                    l.Add(ParseTryStmtHandler());
                } while (PeekToken().Kind == TokenKind.KeywordExcept);
                handlers = l.ToArray();

                if (MaybeEat(TokenKind.KeywordElse)) {
                    elseSuite = ParseSuite();
                }

                if (Options.Python25 && MaybeEat(TokenKind.KeywordFinally)) {
                    finallySuite = ParseSuite();
                }

                TryStatement ts = new TryStatement(body, handlers, elseSuite, finallySuite);
                ts.Header = mid;
                ret = ts;
            }
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //except_clause: 'except' [test [',' test]]
        private TryStatementHandler ParseTryStmtHandler() {
            Eat(TokenKind.KeywordExcept);
            Location start = GetStart();
            Expression test1 = null, test2 = null;
            if (PeekToken().Kind != TokenKind.Colon) {
                test1 = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    test2 = ParseTest();
                }
            }
            Location mid = GetEnd();
            Statement body = ParseSuite();
            TryStatementHandler ret = new TryStatementHandler(test1, test2, body);
            ret.Header = mid;
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //suite: simple_stmt NEWLINE | Newline INDENT stmt+ DEDENT
        private Statement ParseSuite() {
            EatNoEof(TokenKind.Colon);
            Location start = GetStart();
            List<Statement> l = new List<Statement>();
            if (MaybeEat(TokenKind.NewLine)) {
                if (!MaybeEat(TokenKind.Indent)) {
                    ReportSyntaxError(NextToken(), ErrorCodes.IndentationError);
                }

                while (true) {
                    Statement s = ParseStmt();
                    l.Add(s);
                    if (MaybeEat(TokenKind.Dedent)) break;
                    if (MaybeEat(TokenKind.EndOfFile)) break;         // error recovery
                }
                Statement[] stmts = l.ToArray();
                SuiteStatement ret = new SuiteStatement(stmts);
                ret.SetLoc(GetExternal(), start, GetEnd());
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

            if (Options.Python25 == false) {
                return ParseOldTest();
            }

            if (MaybeEat(TokenKind.KeywordLambda)) {
                return FinishLambdef();
            }

            Expression ret = ParseOrTest();
            if (MaybeEat(TokenKind.KeywordIf)) {
                Location start = ret.Start;
                ret = ParseConditionalTest(ret);
                ret.SetLoc(GetExternal(), start, GetEnd());
            }

            return ret;
        }

        // Python2.5 -> or_test: and_test ('or' and_test)*
        private Expression ParseOrTest() {
            Expression ret = ParseAndTest();
            while (MaybeEat(TokenKind.KeywordOr)) {
                Location start = ret.Start;
                ret = new OrExpression(ret, ParseAndTest());
                ret.SetLoc(GetExternal(), start, GetEnd());
            }
            return ret;
        }

        private Expression ParseConditionalTest(Expression trueExpr) {
            Expression test = ParseOrTest();
            Eat(TokenKind.KeywordElse);
            Location start = test.Start;
            Expression falseExpr = ParseTest();
            test.SetLoc(GetExternal(), start, GetEnd());
            return new ConditionalExpression(test, trueExpr, falseExpr);
        }



        // and_test: not_test ('and' not_test)*
        private Expression ParseAndTest() {
            Expression ret = ParseNotTest();
            while (MaybeEat(TokenKind.KeywordAnd)) {
                Location start = ret.Start;
                ret = new AndExpression(ret, ParseAndTest());
                ret.SetLoc(GetExternal(), start, GetEnd());
            }
            return ret;
        }

        //not_test: 'not' not_test | comparison
        private Expression ParseNotTest() {
            if (MaybeEat(TokenKind.KeywordNot)) {
                Location start = GetStart();
                Expression ret = new UnaryExpression(PythonOperator.Not, ParseNotTest());
                ret.SetLoc(GetExternal(), start, GetEnd());
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
                BinaryOperator op;
                Token t = PeekToken();
                switch (t.Kind) {
                    case TokenKind.LessThan: NextToken(); op = PythonOperator.LessThan; break;
                    case TokenKind.LessThanOrEqual: NextToken(); op = PythonOperator.LessThanOrEqual; break;
                    case TokenKind.GreaterThan: NextToken(); op = PythonOperator.GreaterThan; break;
                    case TokenKind.GreaterThanOrEqual: NextToken(); op = PythonOperator.GreaterThanOrEqual; break;
                    case TokenKind.Equal: NextToken(); op = PythonOperator.Equal; break;
                    case TokenKind.NotEqual: NextToken(); op = PythonOperator.NotEqual; break;
                    case TokenKind.LessThanGreaterThan: NextToken(); op = PythonOperator.NotEqual; break;

                    case TokenKind.KeywordIn: NextToken(); op = PythonOperator.In; break;
                    case TokenKind.KeywordNot: NextToken(); Eat(TokenKind.KeywordIn); op = PythonOperator.NotIn; break;

                    case TokenKind.KeywordIs:
                        NextToken();
                        if (MaybeEat(TokenKind.KeywordNot)) op = PythonOperator.IsNot;
                        else op = PythonOperator.Is;
                        break;
                    default:
                        return ret;
                }
                Expression rhs = ParseComparison();
                BinaryExpression be = new BinaryExpression(op, ret, rhs);
                be.SetLoc(GetExternal(), ret.Start, GetEnd());
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

                int prec = ot.Operator.Precedence;
                if (prec >= precedence) {
                    NextToken();
                    Expression right = ParseExpr(prec + 1);
                    Location start = ret.Start;
                    ret = new BinaryExpression((BinaryOperator)ot.Operator, ret, right);
                    ret.SetLoc(GetExternal(), start, GetEnd());
                } else {
                    return ret;
                }
            }
        }

        // factor: ('+'|'-'|'~') factor | power
        private Expression ParseFactor() {
            Token t = PeekToken();
            Location start = GetStart();
            Expression ret;
            switch (t.Kind) {
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
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        private Expression FinishUnaryNegate() {
            Token t = PeekToken();
            // Special case to ensure that System.Int32.MinValue is an int and not a BigInteger
            if (t.Kind == TokenKind.Constant && tokenizer.GetImage().Equals("2147483648")) {
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
                Location start = ret.Start;
                ret = new BinaryExpression(PythonOperator.Power, ret, ParseFactor());
                ret.SetLoc(GetExternal(), start, GetEnd());
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
                    CodeSpan span = GetSpan();
                    SymbolId name = (SymbolId)t.Value;
                    context.Sink.StartName(span, name.GetString());
                    ret = new NameExpression(FixName(name));
                    ret.SetLoc(GetExternal(), GetStart(), GetEnd());
                    return ret;
                case TokenKind.Constant:
                    Location start = GetStart();
                    object cv = t.Value;
                    if (cv is String) {
                        cv = FinishStringPlus((string)cv);
                    }
                    // todo handle STRING+
                    ret = new ConstantExpression(cv);
                    ret.SetLoc(GetExternal(), start, GetEnd());
                    return ret;
                default:
                    ReportSyntaxError(t, ErrorCodes.SyntaxError, allowingIncomplete || tokenizer.EndContinues);

                    // error node
                    ret = new ErrorExpression();
                    ret.SetLoc(GetExternal(), GetStart(), GetEnd());
                    return ret;
            }
        }

        private string FinishStringPlus(string s) {
            Token t = PeekToken();
            //Console.WriteLine("finishing string with " + t);
            while (true) {
                if (t is ConstantValueToken) {
                    object cv = t.Value;
                    if (cv is String) {
                        s += (string)cv;
                        NextToken();
                        t = PeekToken();
                        //Console.WriteLine("have: " + s + " seeing " + t);
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
                Token t = PeekToken();
                switch (t.Kind) {
                    case TokenKind.LeftParenthesis:
                        NextToken();
                        Arg[] args = FinishArgListOrGenExpr();
                        CallExpression call = FinishCallExpr(ret, args);
                        call.SetLoc(GetExternal(), ret.Start, GetEnd());
                        ret = call;
                        break;
                    case TokenKind.LeftBracket:
                        NextToken();
                        Expression index = ParseSubscriptList();
                        IndexExpression ie = new IndexExpression(ret, index);
                        ie.SetLoc(GetExternal(), ret.Start, GetEnd());
                        ret = ie;
                        break;
                    case TokenKind.Dot:
                        NextToken();
                        SymbolId name = ReadNameMaybeNone();
                        FieldExpression fe = new FieldExpression(ret, name);
                        fe.SetLoc(GetExternal(), ret.Start, GetEnd());
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
            Location start0 = GetStart();
            bool trailingComma = false;

            List<Expression> l = new List<Expression>();
            while (true) {
                Expression e;
                if (MaybeEat(TokenKind.Dot)) {
                    Location start = GetStart();
                    Eat(TokenKind.Dot); Eat(TokenKind.Dot);
                    e = new ConstantExpression(Ops.Ellipsis);
                    e.SetLoc(GetExternal(), start, GetEnd());
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
            ret.SetLoc(GetExternal(), start0, GetEnd());
            return ret;
        }

        private Expression ParseSliceEnd() {
            Expression e2 = null;
            Token t = PeekToken();
            switch (t.Kind) {
                case TokenKind.Comma:
                case TokenKind.RightBracket:
                    break;
                default:
                    e2 = ParseTest();
                    break;
            }
            return e2;
        }

        private Expression FinishSlice(Expression e0, Location start) {
            Expression e1 = null;
            Expression e2 = null;
            Token t = PeekToken();

            switch (t.Kind) {
                case TokenKind.Comma:
                case TokenKind.RightBracket:
                    break;
                case TokenKind.Colon:
                    NextToken();
                    e2 = ParseSliceEnd();
                    break;
                default:
                    e1 = ParseTest();
                    if (MaybeEat(TokenKind.Colon)) {
                        e2 = ParseSliceEnd();
                    }
                    break;
            }
            SliceExpression ret = new SliceExpression(e0, e1, e2);
            ret.SetLoc(GetExternal(), start, GetEnd());
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

            context.Sink.StartParameters(GetSpan());

            Token t = PeekToken();
            if (t.Kind != TokenKind.RightParenthesis && t.Kind != TokenKind.Multiply && t.Kind != TokenKind.Power) {
                Location start = GetStart();
                Expression e = ParseTest();
                if (MaybeEat(TokenKind.Assign)) {               //  Keyword argument
                    a = FinishKeywordArgument(e);

                    if (a == null) {                            // Error recovery
                        a = new Arg(e);
                        a.SetLoc(GetExternal(), e.Start, GetEnd());
                    }
                } else if (PeekToken(Tokens.KeywordForToken)) {    //  Generator expression
                    a = new Arg(ParseGeneratorExpression(e));
                    Eat(TokenKind.RightParenthesis);
                    a.SetLoc(GetExternal(), start, GetEnd());
                    context.Sink.EndParameters(GetSpan());
                    return new Arg[1] { a };       //  Generator expression is the argument
                } else {
                    a = new Arg(e);
                    a.SetLoc(GetExternal(), e.Start, e.End);
                }

                //  Was this all?
                //
                if (MaybeEat(TokenKind.Comma)) {
                    context.Sink.NextParameter(GetSpan());
                } else {
                    Eat(TokenKind.RightParenthesis);
                    a.SetLoc(GetExternal(), start, GetEnd());
                    context.Sink.EndParameters(GetSpan());
                    return new Arg[1] { a };
                }
            }

            return FinishArgumentList(a);
        }

        private Arg FinishKeywordArgument(Expression t) {
            NameExpression n = t as NameExpression;
            if (n == null) {
                ReportSyntaxError("expected name");
                Arg arg = new Arg(SymbolTable.StringToId(""), t);
                arg.SetLoc(GetExternal(), t.Start, t.End);
                return arg;
            } else {
                Expression val = ParseTest();
                Arg arg = new Arg(n.Name, val);
                arg.SetLoc(GetExternal(), n.Start, val.End);
                return arg;
            }
        }

        private void CheckUniqueArgument(Dictionary<SymbolId, SymbolId> names, Arg arg) {
            if (arg != null && arg.Name != SymbolTable.Empty) {
                SymbolId name = arg.Name;
                if (names.ContainsKey(name)) {
                    ReportSyntaxError("duplicate keyword argument");
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
                Location start = GetStart();
                Arg a;
                if (MaybeEat(TokenKind.Multiply)) {
                    Expression t = ParseTest();
                    a = new Arg(SymbolTable.Star, t);
                } else if (MaybeEat(TokenKind.Power)) {
                    Expression t = ParseTest();
                    a = new Arg(SymbolTable.StarStar, t);
                } else {
                    Expression e = ParseTest();
                    if (MaybeEat(TokenKind.Assign)) {
                        a = FinishKeywordArgument(e);
                        CheckUniqueArgument(names, a);
                    } else {
                        a = new Arg(e);
                    }
                }
                a.SetLoc(GetExternal(), start, GetEnd());
                l.Add(a);
                if (MaybeEat(TokenKind.Comma)) {
                    context.Sink.NextParameter(GetSpan());
                } else {
                    Eat(terminator);
                    break;
                }
            }

            context.Sink.EndParameters(GetSpan());

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
            Location start = GetStart();
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
            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        //
        //  testlist_gexp: test ( genexpr_for | (',' test)* [','] )
        //
        private Expression FinishTupleOrGenExp() {
            Location lStart = GetStart();
            Location lEnd = GetEnd();
            int grouping = tokenizer.GroupingLevel;

            Expression ret;
            //  Empty tuple
            if (MaybeEat(TokenKind.RightParenthesis)) {
                ret = MakeTupleOrExpr(new List<Expression>(), false);
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
                Eat(TokenKind.RightParenthesis);
            }

            Location rStart = GetStart();
            Location rEnd = GetEnd();

            context.Sink.MatchPair(new CodeSpan(lStart, lEnd), new CodeSpan(rStart, rEnd), grouping);

            ret.SetLoc(GetExternal(), lStart, rEnd);
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
                    YieldStatement ys = new YieldStatement(test, 0);
                    ys.SetLoc(GetExternal(), test.Start, test.End);
                    NestGenExpr(current, ys);
                    break;
                }
            }

            SymbolId fname = SymbolTable.StringToId("__gen_" + System.Threading.Interlocked.Increment(ref genexp_counter));
            NameExpression pname = new NameExpression(SymbolTable.GeneratorParmName);
            FunctionDefinition func = new FunctionDefinition(fname, new Expression[] { pname }, new Expression[] { }, FunctionAttributes.None, root, context.SourceFile);
            func.YieldCount = 1;
            func.SetLoc(GetExternal(), root.Start, GetEnd());
            func.Header = root.End;

            //  Transform the root "for" statement
            Expression outermost = root.List;
            root.List = pname;

            CallExpression gexp = FinishCallExpr(new NameExpression(fname), new Arg(outermost));
            CallExpression iter = FinishCallExpr(new NameExpression(SymbolTable.Iter), new Arg(gexp));

            GeneratorExpression ret = new GeneratorExpression(func, iter);
            ret.SetLoc(GetExternal(), root.Start, GetEnd());
            return ret;
        }

        private static Statement NestGenExpr(Statement current, Statement nested) {
            if (current is ForStatement) {
                ((ForStatement)current).Body = nested;
            } else if (current is IfStatement) {
                ((IfStatement)current).Tests[0].Body = nested;
            }
            return nested;
        }

        // Python 2.5 -> "for" expression_list "in" or_test
        private ForStatement ParseGenExprFor() {
            Location start = GetStart();
            Eat(TokenKind.KeywordFor);
            List<Expression> l = ParseExprList();
            Expression lhs = MakeTupleOrExpr(l, false);
            Eat(TokenKind.KeywordIn);

            Expression test = null;
            if (Options.Python25 == true) {
                test = ParseOrTest();
            } else {
                test = ParseTest();
            }

            ForStatement gef = new ForStatement(lhs, test, null, null);
            Location end = GetEnd();
            gef.SetLoc(GetExternal(), start, end);
            gef.Header = end;
            return gef;
        }

        //  Python 2.5 -> genexpr_if   ::= "if" old_test
        private IfStatement ParseGenExprIf() {
            Location start = GetStart();
            Eat(TokenKind.KeywordIf);
            Expression test = ParseOldTest();
            IfStatementTest ist = new IfStatementTest(test, null);
            Location end = GetEnd();
            ist.Header = end;
            ist.SetLoc(GetExternal(), start, end);
            IfStatement gei = new IfStatement(new IfStatementTest[] { ist }, null);
            gei.SetLoc(GetExternal(), start, end);
            return gei;
        }


        //dictmaker: test ':' test (',' test ':' test)* [',']
        private Expression FinishDictValue() {
            Location oStart = GetStart();
            Location oEnd = GetEnd();

            List<SliceExpression> l = new List<SliceExpression>();
            bool prevAllow = allowingIncomplete;
            try {
                allowingIncomplete = true;
                while (true) {
                    if (MaybeEat(TokenKind.RightBrace)) {
                        break;
                    }
                    Expression e1 = ParseTest();
                    Eat(TokenKind.Colon);
                    Expression e2 = ParseTest();
                    SliceExpression se = new SliceExpression(e1, e2, null);
                    se.SetLoc(GetExternal(), e1.Start, e2.End);
                    l.Add(se);

                    if (!MaybeEat(TokenKind.Comma)) {
                        Eat(TokenKind.RightBrace);
                        break;
                    }
                }
            } finally {
                allowingIncomplete = prevAllow;
            }

            Location cStart = GetStart();
            Location cEnd = GetEnd();

            context.Sink.MatchPair(new CodeSpan(oStart, oEnd), new CodeSpan(cStart, cEnd), 1);

            SliceExpression[] exprs = l.ToArray();
            DictionaryExpression ret = new DictionaryExpression(exprs);
            ret.SetLoc(GetExternal(), oStart, cEnd);
            return ret;
        }


        //        /*
        //        listmaker: test ( list_for | (',' test)* [','] )
        //        */
        private Expression FinishListValue() {
            Location oStart = GetStart();
            Location oEnd = GetEnd();
            int grouping = tokenizer.GroupingLevel;

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

            Location cStart = GetStart();
            Location cEnd = GetEnd();

            context.Sink.MatchPair(new CodeSpan(oStart, oEnd), new CodeSpan(cStart, cEnd), grouping);

            ret.SetLoc(GetExternal(), oStart, cEnd);
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
            Location start = GetStart();
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
            if (Options.Python25 == true) {
                list = ParseTestListAsSafeExpr();
            } else {
                list = ParseTestListAsExpr(false);
            }

            ListComprehensionFor ret = new ListComprehensionFor(lhs, list);

            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        // list_if: 'if' old_test [list_iter]
        private ListComprehensionIf ParseListCompIf() {
            Eat(TokenKind.KeywordIf);
            Location start = GetStart();
            Expression test = ParseOldTest();
            ListComprehensionIf ret = new ListComprehensionIf(test);

            ret.SetLoc(GetExternal(), start, GetEnd());
            return ret;
        }

        private Expression FinishBackquote() {
            Expression ret;
            Location start = GetStart();
            Expression expr = ParseTestListAsExpr(false);
            Eat(TokenKind.BackQuote);
            ret = new BackQuoteExpression(expr);
            ret.SetLoc(GetExternal(), start, GetEnd());
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
                te.SetLoc(exprs[0].ExternalInfo, exprs[0].Start, exprs[exprs.Length - 1].End);
            }
            return te;
        }

        private static bool NeverTestToken(Token t) {
            switch (t.Kind) {
                case TokenKind.AddEqual: return true;
                case TokenKind.SubtractEqual: return true;
                case TokenKind.MultiplyEqual: return true;
                case TokenKind.DivEqual: return true;
                case TokenKind.ModEqual: return true;
                case TokenKind.BitwiseAndEqual: return true;
                case TokenKind.BitwiseOrEqual: return true;
                case TokenKind.XorEqual: return true;
                case TokenKind.LeftShiftEqual: return true;
                case TokenKind.RightShiftEqual: return true;
                case TokenKind.PowerEqual: return true;
                case TokenKind.FloorDivideEqual: return true;

                case TokenKind.Indent: return true;
                case TokenKind.Dedent: return true;
                case TokenKind.NewLine: return true;
                case TokenKind.Semicolon: return true;

                case TokenKind.Assign: return true;
                case TokenKind.RightBrace: return true;
                case TokenKind.RightBracket: return true;
                case TokenKind.RightParenthesis: return true;

                case TokenKind.Comma: return true;

                case TokenKind.KeywordFor: return true;
                case TokenKind.KeywordIn: return true;
                case TokenKind.KeywordIf: return true;

                default: return false;
            }
        }

        private FunctionDefinition CurrentFunction {
            get {
                if (functions != null && functions.Count > 0) {
                    return functions.Peek();
                }
                return null;
            }
        }

        private FunctionDefinition PopFunction() {
            if (functions != null && functions.Count > 0) {
                return functions.Pop();
            }
            return null;
        }

        private void PushFunction(FunctionDefinition function) {
            if (functions == null) {
                functions = new Stack<FunctionDefinition>();
            }
            functions.Push(function);
        }

        private CallExpression FinishCallExpr(Expression target, params Arg[] args) {
            bool hasArgsTuple = false;
            bool hasKeywordDict = false;
            int keywordCount = 0;
            int extraArgs = 0;

            foreach (Arg arg in args) {
                if (arg.Name == SymbolTable.Empty) {
                    if (hasArgsTuple || hasKeywordDict || keywordCount > 0) {
                        ReportSyntaxError("non-keyword arg after keyword arg");
                    }
                } else if (arg.Name == SymbolTable.Star) {
                    if (hasArgsTuple || hasKeywordDict) {
                        ReportSyntaxError("only one * allowed");
                    }
                    hasArgsTuple = true; extraArgs++;
                } else if (arg.Name == SymbolTable.StarStar) {
                    if (hasKeywordDict) {
                        ReportSyntaxError("only on ** allowed");
                    }
                    hasKeywordDict = true; extraArgs++;
                } else {
                    if (hasArgsTuple || hasKeywordDict) {
                        ReportSyntaxError("keywords must come before * args");
                    }
                    keywordCount++;
                }
            }

            return new CallExpression(target, args, hasArgsTuple, hasKeywordDict, keywordCount, extraArgs);
        }
    }
}
