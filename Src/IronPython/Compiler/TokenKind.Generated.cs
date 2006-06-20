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
using System.Collections.Generic;
using System.Text;
using IronPython.Runtime;
using IronPython.Compiler.AST;

namespace IronPython.Compiler {

    public enum TokenKind {
    	EndOfFile = -1,
    	Error = 0,
    	Newline = 1,
    	Indent = 2,
    	Dedent = 3,
    	Comment = 4,
    	Name = 8,
    	Constant = 9,
    	Dot = 31,

        #region Generated Token Kinds

        // *** BEGIN GENERATED CODE ***

        Add = 32,
        AddEqual = 33,
        Subtract = 34,
        SubEqual = 35,
        Power = 36,
        PowEqual = 37,
        Multiply = 38,
        MulEqual = 39,
        FloorDivide = 40,
        FloordivEqual = 41,
        Divide = 42,
        DivEqual = 43,
        Mod = 44,
        ModEqual = 45,
        LeftShift = 46,
        LshiftEqual = 47,
        RightShift = 48,
        RshiftEqual = 49,
        BitwiseAnd = 50,
        AndEqual = 51,
        BitwiseOr = 52,
        OrEqual = 53,
        Xor = 54,
        XorEqual = 55,
        LessThan = 56,
        GreaterThan = 57,
        LessThanOrEqual = 58,
        GreaterThanOrEqual = 59,
        Equal = 60,
        NotEqual = 61,
        LessThanGreaterThan = 62,
        LParen = 63,
        RParen = 64,
        LBracket = 65,
        RBracket = 66,
        LBrace = 67,
        RBrace = 68,
        Comma = 69,
        Colon = 70,
        Backquote = 71,
        Semicolon = 72,
        Assign = 73,
        Twidle = 74,
        At = 75,

        KeywordAnd = 76,
        KeywordAssert = 77,
        KeywordBreak = 78,
        KeywordClass = 79,
        KeywordContinue = 80,
        KeywordDef = 81,
        KeywordDel = 82,
        KeywordElif = 83,
        KeywordElse = 84,
        KeywordExcept = 85,
        KeywordExec = 86,
        KeywordFinally = 87,
        KeywordFor = 88,
        KeywordFrom = 89,
        KeywordGlobal = 90,
        KeywordIf = 91,
        KeywordImport = 92,
        KeywordIn = 93,
        KeywordIs = 94,
        KeywordLambda = 95,
        KeywordNot = 96,
        KeywordOr = 97,
        KeywordPass = 98,
        KeywordPrint = 99,
        KeywordRaise = 100,
        KeywordReturn = 101,
        KeywordTry = 102,
        KeywordWhile = 103,
        KeywordYield = 104,

        // *** END GENERATED CODE ***

        #endregion
    }

    public static class Tokens {
        public static Token EofToken = new SymbolToken(TokenKind.EndOfFile, "<eof>");
        public static Token NewlineToken = new SymbolToken(TokenKind.Newline, "<newline>");
        public static Token IndentToken = new SymbolToken(TokenKind.Indent, "<indent>");
        public static Token DedentToken = new SymbolToken(TokenKind.Dedent, "<dedent>");
        public static Token CommentToken = new SymbolToken(TokenKind.Comment, "<comment>");
        public static Token NoneToken = new ConstantValueToken(null);

        public static Token DotToken = new SymbolToken(TokenKind.Dot, ".");

        #region Generated Tokens

        // *** BEGIN GENERATED CODE ***

        private static readonly Token symAddToken = new OperatorToken(TokenKind.Add, Operator.Add);
        private static readonly Token symAddEqualToken = new SymbolToken(TokenKind.AddEqual, "+=");
        private static readonly Token symSubtractToken = new OperatorToken(TokenKind.Subtract, Operator.Subtract);
        private static readonly Token symSubEqualToken = new SymbolToken(TokenKind.SubEqual, "-=");
        private static readonly Token symPowerToken = new OperatorToken(TokenKind.Power, Operator.Power);
        private static readonly Token symPowEqualToken = new SymbolToken(TokenKind.PowEqual, "**=");
        private static readonly Token symMultiplyToken = new OperatorToken(TokenKind.Multiply, Operator.Multiply);
        private static readonly Token symMulEqualToken = new SymbolToken(TokenKind.MulEqual, "*=");
        private static readonly Token symFloorDivideToken = new OperatorToken(TokenKind.FloorDivide, Operator.FloorDivide);
        private static readonly Token symFloordivEqualToken = new SymbolToken(TokenKind.FloordivEqual, "//=");
        private static readonly Token symDivideToken = new OperatorToken(TokenKind.Divide, Operator.Divide);
        private static readonly Token symDivEqualToken = new SymbolToken(TokenKind.DivEqual, "/=");
        private static readonly Token symModToken = new OperatorToken(TokenKind.Mod, Operator.Mod);
        private static readonly Token symModEqualToken = new SymbolToken(TokenKind.ModEqual, "%=");
        private static readonly Token symLeftShiftToken = new OperatorToken(TokenKind.LeftShift, Operator.LeftShift);
        private static readonly Token symLshiftEqualToken = new SymbolToken(TokenKind.LshiftEqual, "<<=");
        private static readonly Token symRightShiftToken = new OperatorToken(TokenKind.RightShift, Operator.RightShift);
        private static readonly Token symRshiftEqualToken = new SymbolToken(TokenKind.RshiftEqual, ">>=");
        private static readonly Token symBitwiseAndToken = new OperatorToken(TokenKind.BitwiseAnd, Operator.BitwiseAnd);
        private static readonly Token symAndEqualToken = new SymbolToken(TokenKind.AndEqual, "&=");
        private static readonly Token symBitwiseOrToken = new OperatorToken(TokenKind.BitwiseOr, Operator.BitwiseOr);
        private static readonly Token symOrEqualToken = new SymbolToken(TokenKind.OrEqual, "|=");
        private static readonly Token symXorToken = new OperatorToken(TokenKind.Xor, Operator.Xor);
        private static readonly Token symXorEqualToken = new SymbolToken(TokenKind.XorEqual, "^=");
        private static readonly Token symLessThanToken = new OperatorToken(TokenKind.LessThan, Operator.LessThan);
        private static readonly Token symGreaterThanToken = new OperatorToken(TokenKind.GreaterThan, Operator.GreaterThan);
        private static readonly Token symLessThanOrEqualToken = new OperatorToken(TokenKind.LessThanOrEqual, Operator.LessThanOrEqual);
        private static readonly Token symGreaterThanOrEqualToken = new OperatorToken(TokenKind.GreaterThanOrEqual, Operator.GreaterThanOrEqual);
        private static readonly Token symEqualToken = new OperatorToken(TokenKind.Equal, Operator.Equal);
        private static readonly Token symNotEqualToken = new OperatorToken(TokenKind.NotEqual, Operator.NotEqual);
        private static readonly Token symLessThanGreaterThanToken = new SymbolToken(TokenKind.LessThanGreaterThan, "<>");
        private static readonly Token symLParenToken = new SymbolToken(TokenKind.LParen, "(");
        private static readonly Token symRParenToken = new SymbolToken(TokenKind.RParen, ")");
        private static readonly Token symLBracketToken = new SymbolToken(TokenKind.LBracket, "[");
        private static readonly Token symRBracketToken = new SymbolToken(TokenKind.RBracket, "]");
        private static readonly Token symLBraceToken = new SymbolToken(TokenKind.LBrace, "{");
        private static readonly Token symRBraceToken = new SymbolToken(TokenKind.RBrace, "}");
        private static readonly Token symCommaToken = new SymbolToken(TokenKind.Comma, ",");
        private static readonly Token symColonToken = new SymbolToken(TokenKind.Colon, ":");
        private static readonly Token symBackquoteToken = new SymbolToken(TokenKind.Backquote, "`");
        private static readonly Token symSemicolonToken = new SymbolToken(TokenKind.Semicolon, ";");
        private static readonly Token symAssignToken = new SymbolToken(TokenKind.Assign, "=");
        private static readonly Token symTwidleToken = new SymbolToken(TokenKind.Twidle, "~");
        private static readonly Token symAtToken = new SymbolToken(TokenKind.At, "@");

        public static Token AddToken {
            get { return symAddToken; }
        }

        public static Token AddEqualToken {
            get { return symAddEqualToken; }
        }

        public static Token SubtractToken {
            get { return symSubtractToken; }
        }

        public static Token SubEqualToken {
            get { return symSubEqualToken; }
        }

        public static Token PowerToken {
            get { return symPowerToken; }
        }

        public static Token PowEqualToken {
            get { return symPowEqualToken; }
        }

        public static Token MultiplyToken {
            get { return symMultiplyToken; }
        }

        public static Token MulEqualToken {
            get { return symMulEqualToken; }
        }

        public static Token FloorDivideToken {
            get { return symFloorDivideToken; }
        }

        public static Token FloordivEqualToken {
            get { return symFloordivEqualToken; }
        }

        public static Token DivideToken {
            get { return symDivideToken; }
        }

        public static Token DivEqualToken {
            get { return symDivEqualToken; }
        }

        public static Token ModToken {
            get { return symModToken; }
        }

        public static Token ModEqualToken {
            get { return symModEqualToken; }
        }

        public static Token LeftShiftToken {
            get { return symLeftShiftToken; }
        }

        public static Token LshiftEqualToken {
            get { return symLshiftEqualToken; }
        }

        public static Token RightShiftToken {
            get { return symRightShiftToken; }
        }

        public static Token RshiftEqualToken {
            get { return symRshiftEqualToken; }
        }

        public static Token BitwiseAndToken {
            get { return symBitwiseAndToken; }
        }

        public static Token AndEqualToken {
            get { return symAndEqualToken; }
        }

        public static Token BitwiseOrToken {
            get { return symBitwiseOrToken; }
        }

        public static Token OrEqualToken {
            get { return symOrEqualToken; }
        }

        public static Token XorToken {
            get { return symXorToken; }
        }

        public static Token XorEqualToken {
            get { return symXorEqualToken; }
        }

        public static Token LessThanToken {
            get { return symLessThanToken; }
        }

        public static Token GreaterThanToken {
            get { return symGreaterThanToken; }
        }

        public static Token LessThanOrEqualToken {
            get { return symLessThanOrEqualToken; }
        }

        public static Token GreaterThanOrEqualToken {
            get { return symGreaterThanOrEqualToken; }
        }

        public static Token EqualToken {
            get { return symEqualToken; }
        }

        public static Token NotEqualToken {
            get { return symNotEqualToken; }
        }

        public static Token LessThanGreaterThanToken {
            get { return symLessThanGreaterThanToken; }
        }

        public static Token LParenToken {
            get { return symLParenToken; }
        }

        public static Token RParenToken {
            get { return symRParenToken; }
        }

        public static Token LBracketToken {
            get { return symLBracketToken; }
        }

        public static Token RBracketToken {
            get { return symRBracketToken; }
        }

        public static Token LBraceToken {
            get { return symLBraceToken; }
        }

        public static Token RBraceToken {
            get { return symRBraceToken; }
        }

        public static Token CommaToken {
            get { return symCommaToken; }
        }

        public static Token ColonToken {
            get { return symColonToken; }
        }

        public static Token BackquoteToken {
            get { return symBackquoteToken; }
        }

        public static Token SemicolonToken {
            get { return symSemicolonToken; }
        }

        public static Token AssignToken {
            get { return symAssignToken; }
        }

        public static Token TwidleToken {
            get { return symTwidleToken; }
        }

        public static Token AtToken {
            get { return symAtToken; }
        }

        private static readonly Token kwAndToken = new SymbolToken(TokenKind.KeywordAnd, "and");
        private static readonly Token kwAssertToken = new SymbolToken(TokenKind.KeywordAssert, "assert");
        private static readonly Token kwBreakToken = new SymbolToken(TokenKind.KeywordBreak, "break");
        private static readonly Token kwClassToken = new SymbolToken(TokenKind.KeywordClass, "class");
        private static readonly Token kwContinueToken = new SymbolToken(TokenKind.KeywordContinue, "continue");
        private static readonly Token kwDefToken = new SymbolToken(TokenKind.KeywordDef, "def");
        private static readonly Token kwDelToken = new SymbolToken(TokenKind.KeywordDel, "del");
        private static readonly Token kwElifToken = new SymbolToken(TokenKind.KeywordElif, "elif");
        private static readonly Token kwElseToken = new SymbolToken(TokenKind.KeywordElse, "else");
        private static readonly Token kwExceptToken = new SymbolToken(TokenKind.KeywordExcept, "except");
        private static readonly Token kwExecToken = new SymbolToken(TokenKind.KeywordExec, "exec");
        private static readonly Token kwFinallyToken = new SymbolToken(TokenKind.KeywordFinally, "finally");
        private static readonly Token kwForToken = new SymbolToken(TokenKind.KeywordFor, "for");
        private static readonly Token kwFromToken = new SymbolToken(TokenKind.KeywordFrom, "from");
        private static readonly Token kwGlobalToken = new SymbolToken(TokenKind.KeywordGlobal, "global");
        private static readonly Token kwIfToken = new SymbolToken(TokenKind.KeywordIf, "if");
        private static readonly Token kwImportToken = new SymbolToken(TokenKind.KeywordImport, "import");
        private static readonly Token kwInToken = new SymbolToken(TokenKind.KeywordIn, "in");
        private static readonly Token kwIsToken = new SymbolToken(TokenKind.KeywordIs, "is");
        private static readonly Token kwLambdaToken = new SymbolToken(TokenKind.KeywordLambda, "lambda");
        private static readonly Token kwNotToken = new SymbolToken(TokenKind.KeywordNot, "not");
        private static readonly Token kwOrToken = new SymbolToken(TokenKind.KeywordOr, "or");
        private static readonly Token kwPassToken = new SymbolToken(TokenKind.KeywordPass, "pass");
        private static readonly Token kwPrintToken = new SymbolToken(TokenKind.KeywordPrint, "print");
        private static readonly Token kwRaiseToken = new SymbolToken(TokenKind.KeywordRaise, "raise");
        private static readonly Token kwReturnToken = new SymbolToken(TokenKind.KeywordReturn, "return");
        private static readonly Token kwTryToken = new SymbolToken(TokenKind.KeywordTry, "try");
        private static readonly Token kwWhileToken = new SymbolToken(TokenKind.KeywordWhile, "while");
        private static readonly Token kwYieldToken = new SymbolToken(TokenKind.KeywordYield, "yield");


        public static Token KeywordAndToken {
            get { return kwAndToken; }
        }

        public static Token KeywordAssertToken {
            get { return kwAssertToken; }
        }

        public static Token KeywordBreakToken {
            get { return kwBreakToken; }
        }

        public static Token KeywordClassToken {
            get { return kwClassToken; }
        }

        public static Token KeywordContinueToken {
            get { return kwContinueToken; }
        }

        public static Token KeywordDefToken {
            get { return kwDefToken; }
        }

        public static Token KeywordDelToken {
            get { return kwDelToken; }
        }

        public static Token KeywordElifToken {
            get { return kwElifToken; }
        }

        public static Token KeywordElseToken {
            get { return kwElseToken; }
        }

        public static Token KeywordExceptToken {
            get { return kwExceptToken; }
        }

        public static Token KeywordExecToken {
            get { return kwExecToken; }
        }

        public static Token KeywordFinallyToken {
            get { return kwFinallyToken; }
        }

        public static Token KeywordForToken {
            get { return kwForToken; }
        }

        public static Token KeywordFromToken {
            get { return kwFromToken; }
        }

        public static Token KeywordGlobalToken {
            get { return kwGlobalToken; }
        }

        public static Token KeywordIfToken {
            get { return kwIfToken; }
        }

        public static Token KeywordImportToken {
            get { return kwImportToken; }
        }

        public static Token KeywordInToken {
            get { return kwInToken; }
        }

        public static Token KeywordIsToken {
            get { return kwIsToken; }
        }

        public static Token KeywordLambdaToken {
            get { return kwLambdaToken; }
        }

        public static Token KeywordNotToken {
            get { return kwNotToken; }
        }

        public static Token KeywordOrToken {
            get { return kwOrToken; }
        }

        public static Token KeywordPassToken {
            get { return kwPassToken; }
        }

        public static Token KeywordPrintToken {
            get { return kwPrintToken; }
        }

        public static Token KeywordRaiseToken {
            get { return kwRaiseToken; }
        }

        public static Token KeywordReturnToken {
            get { return kwReturnToken; }
        }

        public static Token KeywordTryToken {
            get { return kwTryToken; }
        }

        public static Token KeywordWhileToken {
            get { return kwWhileToken; }
        }

        public static Token KeywordYieldToken {
            get { return kwYieldToken; }
        }


        private static readonly Dictionary<SymbolId, Token> kws = new Dictionary<SymbolId, Token>();

        public static IDictionary<SymbolId, Token> Keywords {
            get { return kws; }
        }
        static Tokens() {
            Keywords[SymbolTable.StringToId("and")] = kwAndToken;
            Keywords[SymbolTable.StringToId("assert")] = kwAssertToken;
            Keywords[SymbolTable.StringToId("break")] = kwBreakToken;
            Keywords[SymbolTable.StringToId("class")] = kwClassToken;
            Keywords[SymbolTable.StringToId("continue")] = kwContinueToken;
            Keywords[SymbolTable.StringToId("def")] = kwDefToken;
            Keywords[SymbolTable.StringToId("del")] = kwDelToken;
            Keywords[SymbolTable.StringToId("elif")] = kwElifToken;
            Keywords[SymbolTable.StringToId("else")] = kwElseToken;
            Keywords[SymbolTable.StringToId("except")] = kwExceptToken;
            Keywords[SymbolTable.StringToId("exec")] = kwExecToken;
            Keywords[SymbolTable.StringToId("finally")] = kwFinallyToken;
            Keywords[SymbolTable.StringToId("for")] = kwForToken;
            Keywords[SymbolTable.StringToId("from")] = kwFromToken;
            Keywords[SymbolTable.StringToId("global")] = kwGlobalToken;
            Keywords[SymbolTable.StringToId("if")] = kwIfToken;
            Keywords[SymbolTable.StringToId("import")] = kwImportToken;
            Keywords[SymbolTable.StringToId("in")] = kwInToken;
            Keywords[SymbolTable.StringToId("is")] = kwIsToken;
            Keywords[SymbolTable.StringToId("lambda")] = kwLambdaToken;
            Keywords[SymbolTable.StringToId("not")] = kwNotToken;
            Keywords[SymbolTable.StringToId("or")] = kwOrToken;
            Keywords[SymbolTable.StringToId("pass")] = kwPassToken;
            Keywords[SymbolTable.StringToId("print")] = kwPrintToken;
            Keywords[SymbolTable.StringToId("raise")] = kwRaiseToken;
            Keywords[SymbolTable.StringToId("return")] = kwReturnToken;
            Keywords[SymbolTable.StringToId("try")] = kwTryToken;
            Keywords[SymbolTable.StringToId("while")] = kwWhileToken;
            Keywords[SymbolTable.StringToId("yield")] = kwYieldToken;
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
