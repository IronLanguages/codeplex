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

using IronPython.Runtime;
using IronPython.Hosting;
using System.Diagnostics;

using System.Collections.Generic;

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
        private Stack<FuncDef> functions;
        private bool fromFutureAllowed = true;
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
                    context.Sink.AddError(fileName, "file has both Unicode marker and PEP-263 file encoding", 0, Severity.Error);
                }
            }

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

            savedStart = tokenizer.startLoc;
            savedEnd = tokenizer.endLoc;

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
            return PeekToken().kind == kind;
        }

        private bool PeekToken(Token check) {
            return PeekToken() == check;
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
            if (t.kind != kind) {
                ReportSyntaxError(t);
                return false;
            }
            return true;
        }

        private bool MaybeEat(TokenKind kind) {
            Token t = PeekToken();
            if (t.kind == kind) {
                NextToken();
                return true;
            } else {
                return false;
            }
        }

        private Location GetStart() {
            if (peekedToken == null) return tokenizer.startLoc;
            else return savedStart;
        }

        private Location GetEnd() {
            if (peekedToken == null) return tokenizer.endLoc;
            else return savedEnd;
        }

        private CodeSpan GetSpan() {
            if (peekedToken == null) {
                return new CodeSpan(tokenizer.startLoc, tokenizer.endLoc);
            } else {
                return new CodeSpan(savedStart, savedEnd);
            }
        }

        private void ReportSyntaxError(Token t) {
            ReportSyntaxError(t, ErrorCodes.SyntaxError);
        }

        private void ReportSyntaxError(Token t, int errorCode) {
            Location start = GetStart();
            if (t.kind == TokenKind.Newline || t.kind == TokenKind.Dedent) {
                if (tokenizer.IsEndOfFile) {
                    t = EatEndOfInput();
                }
            }
            if (t.kind == TokenKind.EndOfFile) {
                errorCode |= ErrorCodes.IncompleteStatement;
            }
            ReportSyntaxError(start, tokenizer.endLoc, String.Format("unexpected token {0}", t.GetImage()), errorCode);
        }

        private void ReportSyntaxError(string message) {
            ReportSyntaxError(GetStart(), GetEnd(), message);
        }

        private void ReportSyntaxError(string message, int errorCode) {
            ReportSyntaxError(GetStart(), GetEnd(), message, errorCode);
        }

        internal void ReportSyntaxError(Location start, Location end, string message) {
            ReportSyntaxError(start, end, message, ErrorCodes.SyntaxError);
        }

        internal void ReportSyntaxError(Location start, Location end, string message, int errorCode) {
            string lineText = tokenizer.GetRawLineForError(start.line);
            context.AddError(message, lineText, start.line, start.column, end.line, end.column, errorCode, Severity.Error);
        }

        private static bool IsPrivateName(Name name) {
            string s = name.GetString();
            return s.StartsWith("__") && !s.EndsWith("__");
        }

        private Name FixName(Name name) {
            if (privatePrefix != null && IsPrivateName(name)) {
                name = Name.Make(string.Format("_{0}{1}", privatePrefix, name.GetString()));
            }

            return name;
        }

        private Name ReadNameMaybeNone() {
            Token t = NextToken();
            if (t == Tokens.NoneToken) {
                return Name.None;
            }

            NameToken n = t as NameToken;
            if (n == null) {
                ReportSyntaxError(t);
                return null;
            }
            return FixName(n.value);
        }

        private Name ReadName() {
            Token t = NextToken();
            NameToken n = t as NameToken;
            if (n == null) {
                ReportSyntaxError(t);
                return null;
            }
            return FixName(n.value);
        }

        #region Public parser interface

        //single_input: Newline | simple_stmt | compound_stmt Newline
        //eval_input: testlist Newline* ENDMARKER
        //file_input: (Newline | stmt)* ENDMARKER
        public Stmt ParseFileInput() {
            List<Stmt> l = new List<Stmt>();

            //
            // A future statement must appear near the top of the module. 
            // The only lines that can appear before a future statement are: 
            // - the module docstring (if any), 
            // - comments, 
            // - blank lines, and 
            // - other future statements. 
            // 

            while (MaybeEat(TokenKind.Newline)) ;

            if (PeekToken(TokenKind.Constant)) {
                Stmt s = ParseStmt();
                l.Add(s);
                fromFutureAllowed = false;
                if (s is ExprStmt) {
                    ConstantExpr ce = ((ExprStmt)s).expr as ConstantExpr;
                    if (ce != null && ce.value is string) {
                        // doc string
                        fromFutureAllowed = true;
                    }
                }
            }

            while (MaybeEat(TokenKind.Newline)) ;

            // from __future__
            if (fromFutureAllowed) {
                while (PeekToken(Tokens.KeywordFromToken)) {
                    Stmt s = ParseStmt();
                    l.Add(s);
                    if (s is FromImportStmt) {
                        FromImportStmt fis = (FromImportStmt)s;
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
                if (MaybeEat(TokenKind.Newline)) continue;

                Stmt s = ParseStmt();
                l.Add(s);
            }

            Stmt[] stmts = l.ToArray();

            SuiteStmt ret = new SuiteStmt(stmts);
            Location start;
            start.column = start.line = 1;
            ret.SetLoc(start, GetEnd());
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
        public Stmt ParseInteractiveInput(bool allowIncompleteStatement) {
            bool parsingMultiLineCmpdStmt;
            try {
                Stmt ret = InternalParseInteractiveInput(out parsingMultiLineCmpdStmt);
                if (parsingMultiLineCmpdStmt && allowIncompleteStatement) return null;
                else return ret;
            } catch (PythonSyntaxError se) {
                // Check if it's a real syntax error, or if its just an incomplete multi-line statement
                //!!! This code shouldn't be using string matching to determine this
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

        private Stmt InternalParseInteractiveInput(out bool parsingMultiLineCmpdStmt) {
            Stmt s;
            parsingMultiLineCmpdStmt = false;
            EatInitialNewlines();
            Token t = PeekToken();
            switch (t.kind) {
                case TokenKind.KeywordIf:
                case TokenKind.KeywordWhile:
                case TokenKind.KeywordFor:
                case TokenKind.KeywordTry:
                case TokenKind.At:
                case TokenKind.KeywordDef:
                case TokenKind.KeywordClass:
                    parsingMultiLineCmpdStmt = true;
                    s = ParseStmt();
                    EatEndOfInput();
                    break;
                default:
                    //  parseSimpleStmt takes care of one or more simple_stmts and the Newline
                    s = ParseSimpleStmt();
                    break;
            }
            return s;
        }



        public Stmt ParseSingleStatement() {
            EatInitialNewlines();
            Stmt statement = ParseStmt();
            EatEndOfInput();
            return statement;
        }

        public Expr ParseTestListAsExpression() {
            Expr expression = ParseTestListAsExpr(false);
            EatEndOfInput();
            return expression;
        }

        private void EatInitialNewlines() {
            while (MaybeEat(TokenKind.Newline)) ;
        }

        private Token EatEndOfInput() {
            while (MaybeEat(TokenKind.Newline) || MaybeEat(TokenKind.Dedent)) {
                ;
            }
            Token t = NextToken();
            if (t.kind != TokenKind.EndOfFile) {
                ReportSyntaxError(t);
            }
            return t;
        }



        #endregion

        //stmt: simple_stmt | compound_stmt
        //compound_stmt: if_stmt | while_stmt | for_stmt | try_stmt | funcdef | classdef
        private Stmt ParseStmt() {
            Token t = PeekToken();

            switch (t.kind) {
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
                    return ParseSimpleStmt();
            }
        }

        //simple_stmt: small_stmt (';' small_stmt)* [';'] Newline
        private Stmt ParseSimpleStmt() {
            Stmt s = ParseSmallStmt();
            if (MaybeEat(TokenKind.Semicolon)) {
                Location start = s.start;
                List<Stmt> l = new List<Stmt>();
                l.Add(s);
                while (true) {
                    if (MaybeEat(TokenKind.Newline)) break;
                    l.Add(ParseSmallStmt());
                    if (!MaybeEat(TokenKind.Semicolon)) {
                        Eat(TokenKind.Newline);
                        break;
                    }
                    if (MaybeEat(TokenKind.EndOfFile)) break; // error recovery
                }
                Stmt[] stmts = l.ToArray();

                SuiteStmt ret = new SuiteStmt(stmts);
                ret.SetLoc(start, GetEnd());
                return ret;
            } else {
                Eat(TokenKind.Newline);
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
        private Stmt ParseSmallStmt() {
            Token t = PeekToken();
            switch (t.kind) {
                case TokenKind.KeywordPrint:
                    return ParsePrintStmt();
                case TokenKind.KeywordPass:
                    return FinishSmallStmt(new PassStmt());
                case TokenKind.KeywordBreak:
                    return FinishSmallStmt(new BreakStmt());
                case TokenKind.KeywordContinue:
                    return FinishSmallStmt(new ContinueStmt());
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

        private Stmt ParseDelStmt() {
            NextToken();
            Location start = GetStart();
            List<Expr> l = ParseExprList();
            DelStmt ret = new DelStmt(l.ToArray());
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private Stmt ParseReturnStmt() {
            if (CurrentFunction == null) {
                ReportSyntaxError("'return' outside function");
            }
            NextToken();
            Expr expr = null;
            Location start = GetStart();
            if (!NeverTestToken(PeekToken())) {
                expr = ParseTestListAsExpr(true);
            }
            ReturnStmt ret = new ReturnStmt(expr);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private Stmt FinishSmallStmt(Stmt stmt) {
            NextToken();
            stmt.SetLoc(GetStart(), GetEnd());
            return stmt;
        }

        private Stmt ParseYieldStmt() {
            NextToken();
            Location start = GetStart();
            FuncDef current = CurrentFunction;
            int yieldId = 0;
            if (current == null) {
                ReportSyntaxError("'yield' outside function");
            } else {
                yieldId = current.yieldCount++;
            }
            Expr e = ParseTestListAsExpr(false);
            YieldStmt ret = new YieldStmt(e, yieldId);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //expr_stmt: testlist (augassign testlist | ('=' testlist)*)
        //augassign: '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '<<=' | '>>=' | '**=' | '//='
        private Stmt ParseExprStmt() {

            Expr lhs = ParseTestListAsExpr(false);

            if (MaybeEat(TokenKind.Assign)) {
                List<Expr> l = new List<Expr>();
                l.Add(lhs);
                do {
                    Expr e = ParseTestListAsExpr(false);
                    l.Add(e);
                } while (MaybeEat(TokenKind.Assign));

                int last = l.Count - 1;
                Expr rhs = (Expr)l[last];
                l.RemoveAt(last);
                Expr[] lhss = l.ToArray();

                //We check for legal assignment targets during code generation rather than parsing
                Stmt ret = new AssignStmt(lhss, rhs);
                ret.SetLoc(lhs.start, GetEnd());
                return ret;
            } else {
                BinaryOperator op = GetAssignOp(PeekToken());
                if (op == null) {
                    Stmt ret = new ExprStmt(lhs);
                    ret.SetLoc(lhs.start, GetEnd());
                    return ret;
                } else {
                    NextToken();
                    Expr rhs = ParseTestListAsExpr(false);
                    Stmt ret = new AugAssignStmt(op, lhs, rhs);
                    ret.SetLoc(lhs.start, GetEnd());
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
            switch (t.kind) {
                case TokenKind.AddEqual: return Operator.Add;
                case TokenKind.SubEqual: return Operator.Subtract;
                case TokenKind.MulEqual: return Operator.Multiply;
                case TokenKind.DivEqual: return Operator.Divide;
                case TokenKind.ModEqual: return Operator.Mod;
                case TokenKind.AndEqual: return Operator.BitwiseAnd;
                case TokenKind.OrEqual: return Operator.BitwiseOr;
                case TokenKind.XorEqual: return Operator.Xor;
                case TokenKind.LshiftEqual: return Operator.LeftShift;
                case TokenKind.RshiftEqual: return Operator.RightShift;
                case TokenKind.PowEqual: return Operator.Power;
                case TokenKind.FloordivEqual: return Operator.FloorDivide;
                default: return null;
            }
        }

        //import_stmt: 'import' dotted_as_name (',' dotted_as_name)*
        private ImportStmt ParseImportStmt() {
            Eat(TokenKind.KeywordImport);
            Location start = GetStart();

            List<DottedName> l = new List<DottedName>();
            List<Name> las = new List<Name>();
            l.Add(ParseDottedName());
            las.Add(MaybeParseAsName());
            while (MaybeEat(TokenKind.Comma)) {
                l.Add(ParseDottedName());
                las.Add(MaybeParseAsName());
            }
            DottedName[] names = l.ToArray();
            Name[] asNames = las.ToArray();

            ImportStmt ret = new ImportStmt(names, asNames);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //| 'from' dotted_name 'import' ('*' | as_name_list | '(' as_name_list ')' )
        private FromImportStmt ParseFromImportStmt() {
            Eat(TokenKind.KeywordFrom);
            Location start = GetStart();
            DottedName dname = ParseDottedName();

            Eat(TokenKind.KeywordImport);

            Name[] names;
            Name[] asNames;
            bool fromFuture = false;

            if (MaybeEat(TokenKind.Multiply)) {
                names = FromImportStmt.Star;
                asNames = null;
            } else {
                List<Name> l = new List<Name>();
                List<Name> las = new List<Name>();

                if (MaybeEat(TokenKind.LParen)) {
                    ParseAsNameList(l, las);
                    Eat(TokenKind.RParen);
                } else {
                    ParseAsNameList(l, las);
                }
                names = l.ToArray();
                asNames = las.ToArray();
                las.CopyTo(asNames);
            }

            // Process from __future__ statement

            if (dname.names.Length == 1 && dname.names[0] == Name.Make("__future__")) {
                if (!fromFutureAllowed) {
                    ReportSyntaxError("from __future__ imports must occur at the beginning of the file");
                }
                if (names == FromImportStmt.Star) {
                    ReportSyntaxError("future statement does not support import *");
                }
                fromFuture = true;
                foreach (Name name in names) {
                    if (name == Name.Make("division")) {
                        context.TrueDivision = true;
                    } else if (name == Name.Make("nested_scopes")) {
                    } else if (name == Name.Make("generators")) {
                    } else {
                        fromFuture = false;
                        ReportSyntaxError(string.Format("future feature is not defined: {0}", name.GetString()));
                    }
                }
            }

            FromImportStmt ret = new FromImportStmt(dname, names, asNames, fromFuture);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        // import_as_name (',' import_as_name)*
        private void ParseAsNameList(List<Name> l, List<Name> las) {
            l.Add(ReadName());
            las.Add(MaybeParseAsName());
            while (MaybeEat(TokenKind.Comma)) {
                if (PeekToken(TokenKind.RParen)) return;  // the list is allowed to end with a ,
                l.Add(ReadName());
                las.Add(MaybeParseAsName());
            }
        }

        //import_as_name: NAME [NAME NAME]
        //dotted_as_name: dotted_name [NAME NAME]
        private static readonly Name AS_NAME = Name.Make("as");
        private Name MaybeParseAsName() {
            NameToken t = PeekToken() as NameToken;
            if (t == null) return null;
            if (t.value == AS_NAME) {
                NextToken();
                return ReadName();
            }
            return null;
        }

        //dotted_name: NAME ('.' NAME)*
        private DottedName ParseDottedName() {
            Location start = GetStart();
            List<Name> l = new List<Name>();
            l.Add(ReadName());
            while (MaybeEat(TokenKind.Dot)) {
                l.Add(ReadName());
            }
            Name[] names = l.ToArray();
            DottedName ret = new DottedName(names);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //exec_stmt: 'exec' expr ['in' test [',' test]]
        private ExecStmt ParseExecStmt() {
            Eat(TokenKind.KeywordExec);
            Location start = GetStart();
            Expr code, locals = null, globals = null;
            code = ParseExpr();
            if (MaybeEat(TokenKind.KeywordIn)) {
                globals = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    locals = ParseTest();
                }
            }
            ExecStmt ret = new ExecStmt(code, locals, globals);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //global_stmt: 'global' NAME (',' NAME)*
        private GlobalStmt ParseGlobalStmt() {
            Eat(TokenKind.KeywordGlobal);
            Location start = GetStart();
            List<Name> l = new List<Name>();
            l.Add(ReadName());
            while (MaybeEat(TokenKind.Comma)) {
                l.Add(ReadName());
            }
            Name[] names = l.ToArray();
            GlobalStmt ret = new GlobalStmt(names);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //raise_stmt: 'raise' [test [',' test [',' test]]]
        private RaiseStmt ParseRaiseStmt() {
            Eat(TokenKind.KeywordRaise);
            Location start = GetStart();
            Expr type = null, _value = null, traceback = null;

            if (!NeverTestToken(PeekToken())) {
                type = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    _value = ParseTest();
                    if (MaybeEat(TokenKind.Comma)) {
                        traceback = ParseTest();
                    }
                }
            }
            RaiseStmt ret = new RaiseStmt(type, _value, traceback);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //assert_stmt: 'assert' test [',' test]
        private AssertStmt ParseAssertStmt() {
            Eat(TokenKind.KeywordAssert);
            Location start = GetStart();
            Expr test = ParseTest();
            Expr message = null;
            if (MaybeEat(TokenKind.Comma)) {
                message = ParseTest();
            }
            AssertStmt ret = new AssertStmt(test, message);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //print_stmt: 'print' ( [ test (',' test)* [','] ] | '>>' test [ (',' test)+ [','] ] )
        private PrintStmt ParsePrintStmt() {
            Eat(TokenKind.KeywordPrint);
            Location start = GetStart();
            Expr dest = null;
            PrintStmt ret;

            bool needNonEmptyTestList = false;
            if (MaybeEat(TokenKind.RightShift)) {
                dest = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    needNonEmptyTestList = true;
                } else {
                    ret = new PrintStmt(dest, new Expr[0], false);
                    ret.SetLoc(start, GetEnd());
                    return ret;
                }
            }

            bool trailingComma;
            List<Expr> l = ParseTestList(out trailingComma);
            if (needNonEmptyTestList && l.Count == 0) {
                ReportSyntaxError(PeekToken());
            }
            Expr[] exprs = l.ToArray();
            ret = new PrintStmt(dest, exprs, trailingComma);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        public string SetPrivatePrefix(Name name) {
            // Remove any leading underscores before saving the prefix
            string oldPrefix = privatePrefix;

            if (name != null) {
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
        private ClassDef ParseClassDef() {
            Eat(TokenKind.KeywordClass);
            Location start = GetStart();
            Name name = ReadName();
            Expr[] bases = new Expr[0];
            if (MaybeEat(TokenKind.LParen)) {
                List<Expr> l = ParseTestList();
                bases = l.ToArray();
                Eat(TokenKind.RParen);
            }
            Location mid = GetEnd();

            // Save private prefix
            string savedPrefix = SetPrivatePrefix(name);

            // Parse the class body
            Stmt body = ParseSuite();

            // Restore the private prefix
            privatePrefix = savedPrefix;

            ClassDef ret = new ClassDef(name, bases, body);
            ret.header = mid;
            ret.SetLoc(start, GetEnd());
            return ret;
        }


        //  decorators ::=
        //      decorator+
        //  decorator ::=
        //      "@" dotted_name ["(" [argument_list [","]] ")"] NEWLINE
        private List<Expr> ParseDecorators() {
            List<Expr> decorators = new List<Expr>();

            while (MaybeEat(TokenKind.At)) {
                Location start = GetStart();
                Expr decorator = new NameExpr(ReadName());
                decorator.SetLoc(start, GetEnd());
                while (MaybeEat(TokenKind.Dot)) {
                    Name name = ReadNameMaybeNone();
                    decorator = new FieldExpr(decorator, name);
                    decorator.SetLoc(GetStart(), GetEnd());
                }
                decorator.SetLoc(start, GetEnd());

                if (MaybeEat(TokenKind.LParen)) {
                    context.Sink.StartParameters(GetSpan());
                    Arg[] args = FinishArgumentList(null);
                    decorator = FinishCallExpr(decorator, args);
                }
                decorator.SetLoc(start, GetEnd());
                Eat(TokenKind.Newline);

                decorators.Add(decorator);
            }

            return decorators;
        }

        // funcdef: [decorators] 'def' NAME parameters ':' suite
        // this gets called with "@" look-ahead
        private FuncDef ParseDecoratedFuncDef() {
            Location start = GetStart();
            List<Expr> decorators = ParseDecorators();
            FuncDef fnc = ParseFuncDef();
            Expr root = new NameExpr(fnc.name);
            root.SetLoc(start, GetEnd());

            for (int i = decorators.Count; i > 0; i--) {
                Expr decorator = (Expr)decorators[i - 1];
                root = FinishCallExpr(decorator, new Arg(root));
                root.SetLoc(decorator.start, decorator.end);
            }
            fnc.decorators = root;

            return fnc;
        }

        // funcdef: [decorators] 'def' NAME parameters ':' suite
        // parameters: '(' [varargslist] ')'
        // this gets called with "def" as the look-ahead
        private FuncDef ParseFuncDef() {
            Eat(TokenKind.KeywordDef);
            Location start = GetStart();
            Name name = ReadName();

            Eat(TokenKind.LParen);

            Location lStart = GetStart(), lEnd = GetEnd();
            int grouping = tokenizer.GroupingLevel;

            Expr[] parameters, defaults;
            FuncDefType flags;
            ParseVarArgsList(out parameters, out defaults, out flags, TokenKind.RParen);

            Location rStart = GetStart(), rEnd = GetEnd();

            FuncDef ret = new FuncDef(name, parameters, defaults, flags, context.SourceFile);
            PushFunction(ret);

            Stmt body = ParseSuite();
            FuncDef ret2 = PopFunction();
            System.Diagnostics.Debug.Assert(ret == ret2);

            ret.body = body;
            ret.header = rEnd;

            context.Sink.MatchPair(new CodeSpan(lStart, lEnd), new CodeSpan(rStart, rEnd), grouping);

            ret.SetLoc(start, GetEnd());

            return ret;
        }

        private NameExpr ParseNameExpr(Dictionary<Name,Name> names) {
            Name name = ReadName();
            if (name != null) {
                CheckUniqueParameter(names, name);
            }
            NameExpr ne = new NameExpr(name);
            ne.SetLoc(GetStart(), GetEnd());
            return ne;
        }

        private void CheckUniqueParameter(Dictionary<Name,Name> names, Name name) {
            if (names.ContainsKey(name)) {
                ReportSyntaxError(string.Format("duplicate argument '{0}' in function definition", name.GetString()));
            }
            names[name] = name;
        }

        //varargslist: (fpdef ['=' test] ',')* ('*' NAME [',' '**' NAME] | '**' NAME) | fpdef ['=' test] (',' fpdef ['=' test])* [',']
        //fpdef: NAME | '(' fplist ')'
        //fplist: fpdef (',' fpdef)* [',']
        private void ParseVarArgsList(out Expr[] parameters, out Expr[] defaults, out FuncDefType flags, TokenKind terminator) {
            // parameters not doing * or ** today
            List<Expr> al = new List<Expr>();
            List<Expr> dl = new List<Expr>();
            Dictionary<Name,Name> names = new Dictionary<Name,Name>();
            bool needDefault = false;
            flags = FuncDefType.None;
            while (true) {
                if (MaybeEat(terminator)) break;

                if (MaybeEat(TokenKind.Multiply)) {
                    al.Add(ParseNameExpr(names));
                    flags |= FuncDefType.ArgList;
                    if (MaybeEat(TokenKind.Comma)) {
                        Eat(TokenKind.Power);
                        al.Add(ParseNameExpr(names));
                        flags |= FuncDefType.KeywordDict;
                    }
                    Eat(terminator);
                    break;
                } else if (MaybeEat(TokenKind.Power)) {
                    al.Add(ParseNameExpr(names));
                    flags |= FuncDefType.KeywordDict;
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

        Expr ParseParameter(Dictionary<Name,Name> names) {
            Token t = NextToken();
            Expr ret = null;
            switch (t.kind) {
                case TokenKind.LParen: // sublist
                    ret = ParseSublist(names);
                    Eat(TokenKind.RParen);
                    break;
                case TokenKind.Name:  // identifier
                    CodeSpan span = GetSpan();
                    Name name = (Name)t.GetValue();
                    context.Sink.StartName(span, name.GetString());
                    name = FixName(name);
                    CheckUniqueParameter(names, name);
                    ret = new NameExpr(name);
                    ret.SetLoc(span);
                    break;
                default:
                    ReportSyntaxError(t);
                    ret = new ErrorExpr();
                    ret.SetLoc(GetStart(), GetEnd());
                    break;
            }
            return ret;
        }

        //  sublist ::=
        //      parameter ("," parameter)* [","]
        Expr ParseSublist(Dictionary<Name,Name> names) {
            bool trailingComma;
            List<Expr> list = new List<Expr>();
            for (; ; ) {
                trailingComma = false;
                list.Add(ParseParameter(names));
                if (MaybeEat(TokenKind.Comma)) {
                    trailingComma = true;
                    Token peek = PeekToken();
                    switch (peek.kind) {
                        case TokenKind.LParen:
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

        //lambdef: 'lambda' [varargslist] ':' test
        private int lambdaCount;
        private Expr FinishLambdef() {
            Location start = GetStart();
            Expr[] parameters, defaults;
            FuncDefType flags;
            ParseVarArgsList(out parameters, out defaults, out flags, TokenKind.Colon);
            Location mid = GetEnd();

            Expr expr = ParseTest();
            Stmt body = new ReturnStmt(expr);
            body.SetLoc(expr.start, expr.end);
            FuncDef func = new FuncDef(Name.Make("<lambda$" + (lambdaCount++) +">"), parameters, defaults, flags, body, context.SourceFile);
            func.SetLoc(start, GetEnd());
            func.header = mid;
            LambdaExpr ret = new LambdaExpr(func);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //while_stmt: 'while' test ':' suite ['else' ':' suite]
        private WhileStmt ParseWhileStmt() {
            Eat(TokenKind.KeywordWhile);
            Location start = GetStart();
            Expr test = ParseTest();
            Location mid = GetEnd();
            Stmt body = ParseSuite();
            Stmt else_ = null;
            if (MaybeEat(TokenKind.KeywordElse)) {
                else_ = ParseSuite();
            }
            WhileStmt ret = new WhileStmt(test, body, else_);
            ret.SetLoc(start, mid, GetEnd());
            return ret;
        }

        //for_stmt: 'for' exprlist 'in' testlist ':' suite ['else' ':' suite]
        private ForStmt ParseForStmt() {
            Eat(TokenKind.KeywordFor);
            Location start = GetStart();
            List<Expr> l = ParseExprList(); //TokenKind.KeywordIn);

            // expr list is something like:
            //  ()
            //  a
            //  a,b
            //  a,b,c
            // we either want just () or a or we want (a,b) and (a,b,c)
            // so we can do tupleExpr.EmitSet() or loneExpr.EmitSet()
            
            Expr lhs = MakeTupleOrExpr(l, false); 
            Eat(TokenKind.KeywordIn);
            Expr list = ParseTestListAsExpr(false);
            Location header = GetEnd();
            Stmt body = ParseSuite();
            Stmt else_ = null;
            if (MaybeEat(TokenKind.KeywordElse)) {
                else_ = ParseSuite();
            }
            ForStmt ret = new ForStmt(lhs, list, body, else_);
            ret.header = header;
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        // if_stmt: 'if' test ':' suite ('elif' test ':' suite)* ['else' ':' suite]
        private IfStmt ParseIfStmt() {
            Eat(TokenKind.KeywordIf);
            Location start = GetStart();
            List<IfStmtTest> l = new List<IfStmtTest>();
            l.Add(ParseIfStmtTest());

            while (MaybeEat(TokenKind.KeywordElif)) {
                l.Add(ParseIfStmtTest());
            }

            Stmt else_ = null;
            if (MaybeEat(TokenKind.KeywordElse)) {
                else_ = ParseSuite();
            }

            IfStmtTest[] tests = l.ToArray();
            IfStmt ret = new IfStmt(tests, else_);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private IfStmtTest ParseIfStmtTest() {
            Location start = GetStart();
            Expr test = ParseTest();
            Location header = GetEnd();
            Stmt suite = ParseSuite();
            IfStmtTest ret = new IfStmtTest(test, suite);
            ret.SetLoc(start, suite.end);
            ret.header = header;
            return ret;
        }

        //try_stmt: ('try' ':' suite (except_clause ':' suite)+
        //    ['else' ':' suite] | 'try' ':' suite 'finally' ':' suite)
        //# NB compile.c makes sure that the default except clause is last

        private Stmt ParseTryStmt() {
            Eat(TokenKind.KeywordTry);
            Location start = GetStart();
            Location mid = GetEnd();
            Stmt body = ParseSuite();
            Stmt ret;

            if (MaybeEat(TokenKind.KeywordFinally)) {
                Stmt finally_ = ParseSuite();
                TryFinallyStmt tfs = new TryFinallyStmt(body, finally_);
                tfs.header = mid;
                ret = tfs;
            } else {
                List<TryStmtHandler> l = new List<TryStmtHandler>();
                do {
                    l.Add(ParseTryStmtHandler());
                } while (PeekToken().kind == TokenKind.KeywordExcept);
                TryStmtHandler[] handlers = l.ToArray();

                Stmt else_ = null;
                if (MaybeEat(TokenKind.KeywordElse)) {
                    else_ = ParseSuite();
                }

                TryStmt ts = new TryStmt(body, handlers, else_);
                ts.header = mid;
                ret = ts;
            }
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //except_clause: 'except' [test [',' test]]
        private TryStmtHandler ParseTryStmtHandler() {
            Eat(TokenKind.KeywordExcept);
            Location start = GetStart();
            Expr test1 = null, test2 = null;
            if (PeekToken().kind != TokenKind.Colon) {
                test1 = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    test2 = ParseTest();
                }
            }
            Location mid = GetEnd();
            Stmt body = ParseSuite();
            TryStmtHandler ret = new TryStmtHandler(test1, test2, body);
            ret.header = mid;
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //suite: simple_stmt NEWLINE | Newline INDENT stmt+ DEDENT
        private Stmt ParseSuite() {
            Eat(TokenKind.Colon);
            Location start = GetStart();
            List<Stmt> l = new List<Stmt>();
            if (MaybeEat(TokenKind.Newline)) {
                if (!MaybeEat(TokenKind.Indent)) {
                    ReportSyntaxError(NextToken(), ErrorCodes.IndentationError);
                }

                while (true) {
                    Stmt s = ParseStmt();
                    l.Add(s);
                    if (MaybeEat(TokenKind.Dedent)) break;
                    if (MaybeEat(TokenKind.EndOfFile)) break;         // error recovery
                }
                Stmt[] stmts = l.ToArray();
                SuiteStmt ret = new SuiteStmt(stmts);
                ret.SetLoc(start, GetEnd());
                return ret;
            } else {
                //  simple_stmt NEWLINE
                //  ParseSimpleStmt takes care of the NEWLINE
                Stmt s = ParseSimpleStmt();
                return s;
            }
        }


        // test: and_test ('or' and_test)* | lambdef
        private Expr ParseTest() {
            if (MaybeEat(TokenKind.KeywordLambda)) {
                return FinishLambdef();
            }
            Expr ret = ParseAndTest();
            while (MaybeEat(TokenKind.KeywordOr)) {
                Location start = ret.start;
                ret = new OrExpr(ret, ParseTest());
                ret.SetLoc(start, GetEnd());
            }
            return ret;
        }

        // and_test: not_test ('and' not_test)*
        private Expr ParseAndTest() {
            Expr ret = ParseNotTest();
            while (MaybeEat(TokenKind.KeywordAnd)) {
                Location start = ret.start;
                ret = new AndExpr(ret, ParseAndTest());
                ret.SetLoc(start, GetEnd());
            }
            return ret;
        }

        //not_test: 'not' not_test | comparison
        private Expr ParseNotTest() {
            if (MaybeEat(TokenKind.KeywordNot)) {
                Location start = GetStart();
                Expr ret = new UnaryExpr(Operator.Not, ParseNotTest());
                ret.SetLoc(start, GetEnd());
                return ret;
            } else {
                return ParseComparison();
            }
        }
        //comparison: expr (comp_op expr)*
        //comp_op: '<'|'>'|'=='|'>='|'<='|'<>'|'!='|'in'|'not' 'in'|'is'|'is' 'not'
        private Expr ParseComparison() {
            Expr ret = ParseExpr();
            while (true) {
                BinaryOperator op;
                Token t = PeekToken();
                switch (t.kind) {
                    case TokenKind.LessThan: NextToken(); op = Operator.LessThan; break;
                    case TokenKind.LessThanOrEqual: NextToken(); op = Operator.LessThanOrEqual; break;
                    case TokenKind.GreaterThan: NextToken(); op = Operator.GreaterThan; break;
                    case TokenKind.GreaterThanOrEqual: NextToken(); op = Operator.GreaterThanOrEqual; break;
                    case TokenKind.Equal: NextToken(); op = Operator.Equal; break;
                    case TokenKind.NotEqual: NextToken(); op = Operator.NotEqual; break;
                    case TokenKind.LessThanGreaterThan: NextToken(); op = Operator.NotEqual; break;

                    case TokenKind.KeywordIn: NextToken(); op = Operator.In; break;
                    case TokenKind.KeywordNot: NextToken(); Eat(TokenKind.KeywordIn); op = Operator.NotIn; break;

                    case TokenKind.KeywordIs:
                        NextToken();
                        if (MaybeEat(TokenKind.KeywordNot)) op = Operator.IsNot;
                        else op = Operator.Is;
                        break;
                    default:
                        return ret;
                }
                Expr rhs = ParseComparison();
                BinaryExpr be = new BinaryExpr(op, ret, rhs);
                be.SetLoc(ret.start, GetEnd());
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
        private Expr ParseExpr() {
            return ParseExpr(0);
        }

        private Expr ParseExpr(int precedence) {
            Expr ret = ParseFactor();
            while (true) {
                Token t = PeekToken();
                OperatorToken ot = t as OperatorToken;
                if (ot == null) return ret;

                int prec = ot.op.precedence;
                if (prec >= precedence) {
                    NextToken();
                    Expr right = ParseExpr(prec + 1);
                    Location start = ret.start;
                    ret = new BinaryExpr((BinaryOperator)ot.op, ret, right);
                    ret.SetLoc(start, GetEnd());
                } else {
                    return ret;
                }
            }
        }

        // factor: ('+'|'-'|'~') factor | power
        private Expr ParseFactor() {
            Token t = PeekToken();
            Location start = GetStart();
            Expr ret;
            switch (t.kind) {
                case TokenKind.Add:
                    NextToken();
                    ret = new UnaryExpr(Operator.Pos, ParseFactor());
                    break;
                case TokenKind.Subtract:
                    NextToken();
                    ret = FinishUnaryNegate();
                    break;
                case TokenKind.Twidle:
                    NextToken();
                    ret = new UnaryExpr(Operator.Invert, ParseFactor());
                    break;
                default:
                    return ParsePower();
            }
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private Expr FinishUnaryNegate() {
            Token t = PeekToken();
            // Special case to ensure that System.Int32.MinValue is an int and not a BigInteger
            if (t.kind == TokenKind.Constant && tokenizer.GetImage().Equals("2147483648")) {
                NextToken();
                return new ConstantExpr(-2147483648);
            }
         
            return new UnaryExpr(Operator.Neg, ParseFactor());
        }

        // power: atom trailer* ['**' factor]
        private Expr ParsePower() {
            Expr ret = ParsePrimary();
            ret = AddTrailers(ret);
            if (MaybeEat(TokenKind.Power)) {
                Location start = ret.start;
                ret = new BinaryExpr(Operator.Power, ret, ParseFactor());
                ret.SetLoc(start, GetEnd());
            }
            return ret;
        }



        // atom: '(' [testlist_gexp] ')' | '[' [listmaker] ']' | '{' [dictmaker] '}' | '`' testlist1 '`' | NAME | NUMBER | STRING+
        private Expr ParsePrimary() {
            Token t = NextToken();
            Expr ret;
            switch (t.kind) {
                case TokenKind.LParen:
                    return FinishTupleOrGenExp();
                case TokenKind.LBracket:
                    return FinishListValue();
                case TokenKind.LBrace:
                    return FinishDictValue();
                case TokenKind.Backquote:
                    return FinishBackquote();
                case TokenKind.Name:
                    CodeSpan span = GetSpan();
                    Name name = (Name)t.GetValue();
                    context.Sink.StartName(span, name.GetString());
                    ret = new NameExpr(FixName(name));
                    ret.SetLoc(GetStart(), GetEnd());
                    return ret;
                case TokenKind.Constant:
                    Location start = GetStart();
                    object cv = t.GetValue();
                    if (cv is String) {
                        cv = FinishStringPlus((string)cv);
                    }
                    // todo handle STRING+
                    ret = new ConstantExpr(cv);
                    ret.SetLoc(start, GetEnd());
                    return ret;
                default:                    
                    ReportSyntaxError(t);

                    // error node
                    ret = new ErrorExpr();
                    ret.SetLoc(GetStart(), GetEnd());
                    return ret;
            }
        }

        private string FinishStringPlus(string s) {
            Token t = PeekToken();
            //Console.WriteLine("finishing string with " + t);
            while (true) {
                if (t is ConstantValueToken) {
                    object cv = t.GetValue();
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
        private Expr AddTrailers(Expr ret) {
            while (true) {
                Token t = PeekToken();
                switch (t.kind) {
                    case TokenKind.LParen:
                        NextToken();
                        Arg[] args = FinishArgListOrGenExpr();
                        CallExpr call = FinishCallExpr(ret, args);
                        call.SetLoc(ret.start, GetEnd());
                        ret = call;
                        break;
                    case TokenKind.LBracket:
                        NextToken();
                        Expr index = ParseSubscriptList();
                        IndexExpr ie = new IndexExpr(ret, index);
                        ie.SetLoc(ret.start, GetEnd());
                        ret = ie;
                        break;
                    case TokenKind.Dot:
                        NextToken();
                        Name name = ReadNameMaybeNone();
                        FieldExpr fe = new FieldExpr(ret, name);
                        fe.SetLoc(ret.start, GetEnd());
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
        private Expr ParseSubscriptList() {
            const TokenKind terminator = TokenKind.RBracket;
            Location start0 = GetStart();
            bool trailingComma = false;

            List<Expr> l = new List<Expr>();
            while (true) {
                Expr e;
                if (MaybeEat(TokenKind.Dot)) {
                    Location start = GetStart();
                    Eat(TokenKind.Dot); Eat(TokenKind.Dot);
                    e = new ConstantExpr(Ops.Ellipsis);
                    e.SetLoc(start, GetEnd());
                } else if (MaybeEat(TokenKind.Colon)) {
                    e = FinishSlice(null, GetStart());
                } else {
                    e = ParseTest();
                    if (MaybeEat(TokenKind.Colon)) {
                        e = FinishSlice(e, e.start);
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
            Expr ret = MakeTupleOrExpr(l, trailingComma, true);
            ret.SetLoc(start0, GetEnd());
            return ret;
        }

        private Expr ParseSliceEnd() {
            Expr e2 = null;
            Token t = PeekToken();
            switch (t.kind) {
                case TokenKind.Comma:
                case TokenKind.RBracket:
                    break;
                default:
                    e2 = ParseTest();
                    break;
            }
            return e2;
        }

        private Expr FinishSlice(Expr e0, Location start) {
            Expr e1 = null;
            Expr e2 = null;
            Token t = PeekToken();

            switch (t.kind) {
                case TokenKind.Comma:
                case TokenKind.RBracket:
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
            SliceExpr ret = new SliceExpr(e0, e1, e2);
            ret.SetLoc(start, GetEnd());
            return ret;
        }


        //exprlist: expr (',' expr)* [',']
        private List<Expr> ParseExprList() {
            List<Expr> l = new List<Expr>();
            while (true) {
                Expr e = ParseExpr();
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
            if (t.kind != TokenKind.RParen && t.kind != TokenKind.Multiply && t.kind != TokenKind.Power) {
                Location start = GetStart();
                Expr e = ParseTest();
                if (MaybeEat(TokenKind.Assign)) {               //  Keyword argument
                    a = FinishKeywordArgument(e);

                    if (a == null) {                            // Error recovery
                        a = new Arg(e);
                        a.SetLoc(e.start, GetEnd());
                    }
                } else if (PeekToken(Tokens.KeywordForToken)) {    //  Generator expression
                    a = new Arg(ParseGeneratorExpression(e));
                    Eat(TokenKind.RParen);
                    a.SetLoc(start, GetEnd());
                    context.Sink.EndParameters(GetSpan());
                    return new Arg[1] { a };       //  Generator expression is the argument
                } else {
                    a = new Arg(e);
                    a.SetLoc(e.start, e.end);
                }

                //  Was this all?
                //
                if (MaybeEat(TokenKind.Comma)) {
                    context.Sink.NextParameter(GetSpan());
                } else {
                    Eat(TokenKind.RParen);
                    a.SetLoc(start, GetEnd());
                    context.Sink.EndParameters(GetSpan());
                    return new Arg[1] { a };
                }
            }

            return FinishArgumentList(a);
        }

        private Arg FinishKeywordArgument(Expr t) {
            NameExpr n = t as NameExpr;
            if (n == null) {
                ReportSyntaxError("expected name");
                Arg arg = new Arg(Name.Make(""), t);
                arg.SetLoc(t.start, t.end);
                return arg;
            } else {
                Expr val = ParseTest();
                Arg arg = new Arg(n.name, val);
                arg.SetLoc(n.start, val.end);
                return arg;
            }
        }

        private void CheckUniqueArgument(Dictionary<Name, Name> names, Arg arg) {
            if (arg != null && arg.name != null) {
                Name name = arg.name;
                if (names.ContainsKey(name)) {
                    ReportSyntaxError("duplicate keyword argument");
                }
                names[name] = name;
            }
        }

        //arglist: (argument ',')* (argument [',']| '*' test [',' '**' test] | '**' test)
        //argument: [test '='] test    # Really [keyword '='] test
        private Arg[] FinishArgumentList(Arg first) {
            const TokenKind terminator = TokenKind.RParen;
            List<Arg> l = new List<Arg>();
            Dictionary<Name, Name> names = new Dictionary<Name, Name>();

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
                    Expr t = ParseTest();
                    a = new Arg(CallExpr.ParamsName, t);
                } else if (MaybeEat(TokenKind.Power)) {
                    Expr t = ParseTest();
                    a = new Arg(CallExpr.DictionaryName, t);
                } else {
                    Expr e = ParseTest();
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

        private List<Expr> ParseTestList() {
            bool tmp;
            return ParseTestList(out tmp);
        }

        //        testlist: test (',' test)* [',']
        //        testlist_safe: test [(',' test)+ [',']]
        //        testlist1: test (',' test)*
        private List<Expr> ParseTestList(out bool trailingComma) {
            List<Expr> l = new List<Expr>();
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

        private Expr ParseTestListAsExpr(bool allowEmptyExpr) {
            bool trailingComma;
            List<Expr> l = ParseTestList(out trailingComma);
            //  the case when no expression was parsed e.g. when we have an empty test list
            if (!allowEmptyExpr && l.Count == 0 && !trailingComma) {
                ReportSyntaxError("invalid syntax");
            }
            return MakeTupleOrExpr(l, trailingComma);
        }

        private Expr FinishTestListAsExpr(Expr test) {
            Location start = GetStart();
            bool trailingComma = true;
            List<Expr> l = new List<Expr>();
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

            Expr ret = MakeTupleOrExpr(l, trailingComma);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        //
        //  testlist_gexp: test ( genexpr_for | (',' test)* [','] )
        //
        private Expr FinishTupleOrGenExp() {
            Location lStart = GetStart();
            Location lEnd = GetEnd();
            int grouping = tokenizer.GroupingLevel;

            Expr ret;
            //  Empty tuple
            if (MaybeEat(TokenKind.RParen)) {
                ret = MakeTupleOrExpr(new List<Expr>(), false);
            } else {
                Expr test = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    // "(" test "," ...
                    ret = FinishTestListAsExpr(test);
                } else if (PeekToken(Tokens.KeywordForToken)) {
                    // "(" test "for" ...
                    ret = ParseGeneratorExpression(test);
                } else {
                    // "(" test ")"
                    ret = test is ParenExpr ? test : new ParenExpr(test);
                }
                Eat(TokenKind.RParen);
            }

            Location rStart = GetStart();
            Location rEnd = GetEnd();

            context.Sink.MatchPair(new CodeSpan(lStart, lEnd), new CodeSpan(rStart, rEnd), grouping);

            ret.SetLoc(lStart, rEnd);
            return ret;
        }

        //  genexpr_for  ::= "for" expression_list "in" test [genexpr_iter]
        //  genexpr_iter ::= (genexpr_for | genexpr_if) *
        //
        //  "for" has NOT been eaten before entering this method
        private static int genexp_counter;
        private Expr ParseGeneratorExpression(Expr test) {
            ForStmt root = ParseGenExprFor();
            Stmt current = root;

            for (; ; ) {
                if (PeekToken(Tokens.KeywordForToken)) {
                    current = NestGenExpr(current, ParseGenExprFor());
                } else if (PeekToken(Tokens.KeywordIfToken)) {
                    current = NestGenExpr(current, ParseGenExprIf());
                } else {
                    YieldStmt ys = new YieldStmt(test, 0);
                    ys.SetLoc(test.start, test.end);
                    NestGenExpr(current, ys);
                    break;
                }
            }

            Name fname = Name.Make("__gen_" + System.Threading.Interlocked.Increment(ref genexp_counter));
            NameExpr pname = new NameExpr(Name.Make("__gen_$_parm__"));
            FuncDef func = new FuncDef(fname, new Expr[] { pname }, new Expr[] { }, FuncDefType.None, root, context.SourceFile);
            func.yieldCount = 1;
            func.SetLoc(root.start, GetEnd());
            func.header = root.end;

            //  Transform the root "for" statement
            Expr outermost = root.list;
            root.list = pname;

            CallExpr gexp = FinishCallExpr(new NameExpr(fname), new Arg(outermost));
            CallExpr iter = FinishCallExpr(new NameExpr(Name.Make("iter")), new Arg(gexp));

            GenExpr ret = new GenExpr(func, iter);
            ret.SetLoc(root.start, GetEnd());
            return ret;
        }

        private static Stmt NestGenExpr(Stmt current, Stmt nested) {
            if (current is ForStmt) {
                ((ForStmt)current).body = nested;
            } else if (current is IfStmt) {
                ((IfStmt)current).tests[0].body = nested;
            }
            return nested;
        }

        // "for" expression_list "in" test
        private ForStmt ParseGenExprFor() {
            Location start = GetStart();
            Eat(TokenKind.KeywordFor);
            List<Expr> l = ParseExprList();
            Expr lhs = MakeTupleOrExpr(l, false);
            Eat(TokenKind.KeywordIn);
            Expr test = ParseTest();
            ForStmt gef = new ForStmt(lhs, test, null, null);
            Location end = GetEnd();
            gef.SetLoc(start, end);
            gef.header = end;
            return gef;
        }

        //  genexpr_if   ::= "if" test
        private IfStmt ParseGenExprIf() {
            Location start = GetStart();
            Eat(TokenKind.KeywordIf);
            Expr test = ParseTest();
            IfStmtTest ist = new IfStmtTest(test, null);
            Location end = GetEnd();
            ist.header = end;
            ist.SetLoc(start, end);
            IfStmt gei = new IfStmt(new IfStmtTest[] { ist }, null);
            gei.SetLoc(start, end);
            return gei;
        }

        //dictmaker: test ':' test (',' test ':' test)* [',']
        private Expr FinishDictValue() {
            Location oStart = GetStart();
            Location oEnd = GetEnd();

            List<SliceExpr> l = new List<SliceExpr>();
            while (true) {
                if (MaybeEat(TokenKind.RBrace)) {
                    break;
                }
                Expr e1 = ParseTest();
                Eat(TokenKind.Colon);
                Expr e2 = ParseTest();
                SliceExpr se = new SliceExpr(e1, e2, null);
                se.SetLoc(e1.start, e2.end);
                l.Add(se);

                if (!MaybeEat(TokenKind.Comma)) {
                    Eat(TokenKind.RBrace);
                    break;
                }
            }
            Location cStart = GetStart();
            Location cEnd = GetEnd();

            context.Sink.MatchPair(new CodeSpan(oStart, oEnd), new CodeSpan(cStart, cEnd), 1);

            SliceExpr[] exprs = l.ToArray();
            DictExpr ret = new DictExpr(exprs);
            ret.SetLoc(oStart, cEnd);
            return ret;
        }


        //        /*
        //        listmaker: test ( list_for | (',' test)* [','] )
        //        */
        private Expr FinishListValue() {
            Location oStart = GetStart();
            Location oEnd = GetEnd();
            int grouping = tokenizer.GroupingLevel;

            Expr ret;
            if (MaybeEat(TokenKind.RBracket)) {
                ret = new ListExpr();
            } else {
                Expr t0 = ParseTest();
                if (MaybeEat(TokenKind.Comma)) {
                    List<Expr> l = ParseTestList();
                    Eat(TokenKind.RBracket);
                    l.Insert(0, t0);
                    ret = new ListExpr(l.ToArray());
                } else if (PeekToken(Tokens.KeywordForToken)) {
                    ret = FinishListComp(t0);
                } else {
                    Eat(TokenKind.RBracket);
                    ret = new ListExpr(t0);
                }
            }

            Location cStart = GetStart();
            Location cEnd = GetEnd();

            context.Sink.MatchPair(new CodeSpan(oStart, oEnd), new CodeSpan(cStart, cEnd), grouping);

            ret.SetLoc(oStart, cEnd);
            return ret;
        }

        //        list_iter: list_for | list_if
        private ListComp FinishListComp(Expr item) {
            List<ListCompIter> iters = new List<ListCompIter>();
            ListCompFor firstFor = ParseListCompFor();
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

            Eat(TokenKind.RBracket);
            return new ListComp(item, iters.ToArray());
        }

        // list_for: 'for' exprlist 'in' testlist_safe [list_iter]
        private ListCompFor ParseListCompFor() {
            Eat(TokenKind.KeywordFor);
            Location start = GetStart();
            List<Expr> l = ParseExprList();

            // expr list is something like:
            //  ()
            //  a
            //  a,b
            //  a,b,c
            // we either want just () or a or we want (a,b) and (a,b,c)
            // so we can do tupleExpr.EmitSet() or loneExpr.EmitSet()

            Expr lhs = MakeTupleOrExpr(l, false); 
            Eat(TokenKind.KeywordIn);
            Expr list = ParseTestListAsExpr(false);

            ListCompFor ret = new ListCompFor(lhs, list);

            ret.SetLoc(start, GetEnd());
            return ret;
        }

        // list_if: 'if' test [list_iter]
        private ListCompIf ParseListCompIf() {
            Eat(TokenKind.KeywordIf);
            Location start = GetStart();
            Expr test = ParseTest();

            ListCompIf ret = new ListCompIf(test);

            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private Expr FinishBackquote() {
            Expr ret;
            Location start = GetStart();
            Expr expr = ParseTestListAsExpr(false);
            Eat(TokenKind.Backquote);
            ret = new BackquoteExpr(expr);
            ret.SetLoc(start, GetEnd());
            return ret;
        }

        private static Expr MakeTupleOrExpr(List<Expr> l, bool trailingComma) {
            return MakeTupleOrExpr(l, trailingComma, false);
        }

        private static Expr MakeTupleOrExpr(List<Expr> l, bool trailingComma, bool expandable) {
            if (l.Count == 1 && !trailingComma) return l[0];

            Expr[] exprs = l.ToArray();
            TupleExpr te = new TupleExpr(expandable && !trailingComma, exprs);
            if (exprs.Length > 0) {
                te.SetLoc(exprs[0].start, exprs[exprs.Length - 1].end);
            }
            return te;
        }

        private static bool NeverTestToken(Token t) {
            switch (t.kind) {
                case TokenKind.AddEqual: return true;
                case TokenKind.SubEqual: return true;
                case TokenKind.MulEqual: return true;
                case TokenKind.DivEqual: return true;
                case TokenKind.ModEqual: return true;
                case TokenKind.AndEqual: return true;
                case TokenKind.OrEqual: return true;
                case TokenKind.XorEqual: return true;
                case TokenKind.LshiftEqual: return true;
                case TokenKind.RshiftEqual: return true;
                case TokenKind.PowEqual: return true;
                case TokenKind.FloordivEqual: return true;

                case TokenKind.Indent: return true;
                case TokenKind.Dedent: return true;
                case TokenKind.Newline: return true;
                case TokenKind.Semicolon: return true;

                case TokenKind.Assign: return true;
                case TokenKind.RBrace: return true;
                case TokenKind.RBracket: return true;
                case TokenKind.RParen: return true;

                case TokenKind.Comma: return true;

                case TokenKind.KeywordFor: return true;
                case TokenKind.KeywordIn: return true;
                case TokenKind.KeywordIf: return true;

                default: return false;
            }
        }

        private FuncDef CurrentFunction {
            get {
                if (functions != null && functions.Count > 0) {
                    return functions.Peek();
                }
                return null;
            }
        }

        private FuncDef PopFunction() {
            if (functions != null && functions.Count > 0) {
                return functions.Pop();
            }
            return null;
        }

        private void PushFunction(FuncDef function) {
            if (functions == null) {
                functions = new Stack<FuncDef>();
            }
            functions.Push(function);
        }

        private CallExpr FinishCallExpr(Expr target, params Arg[] args) {
            bool hasArgsTuple = false;
            bool hasKeywordDict = false;
            int keywordCount = 0;
            int extraArgs = 0;

            foreach (Arg arg in args) {
                if (arg.name == null) {
                    if (hasArgsTuple || hasKeywordDict || keywordCount > 0) {
                        ReportSyntaxError("non-keyword arg after keyword arg");
                    }
                } else if (arg.name == CallExpr.ParamsName) {
                    if (hasArgsTuple || hasKeywordDict) {
                        ReportSyntaxError("only one * allowed");
                    }
                    hasArgsTuple = true; extraArgs++;
                } else if (arg.name == CallExpr.DictionaryName) {
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

            return new CallExpr(target, args, hasArgsTuple, hasKeywordDict, keywordCount, extraArgs);
        }
    }
}
