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
        public static readonly Token EofToken = new SymbolToken(TokenKind.EndOfFile, "<eof>");
        public static readonly Token NewlineToken = new SymbolToken(TokenKind.Newline, "<newline>");
        public static readonly Token IndentToken = new SymbolToken(TokenKind.Indent, "<indent>");
        public static readonly Token DedentToken = new SymbolToken(TokenKind.Dedent, "<dedent>");
        public static readonly Token CommentToken = new SymbolToken(TokenKind.Comment, "<comment>");
        public static readonly Token NoneToken = new ConstantValueToken(null);

        public static readonly Token DotToken = new SymbolToken(TokenKind.Dot, ".");

        #region Generated Tokens

        // *** BEGIN GENERATED CODE ***

        public static readonly Token AddToken = new OperatorToken(TokenKind.Add, Operator.Add);
        public static readonly Token AddEqualToken = new SymbolToken(TokenKind.AddEqual, "+=");
        public static readonly Token SubtractToken = new OperatorToken(TokenKind.Subtract, Operator.Subtract);
        public static readonly Token SubEqualToken = new SymbolToken(TokenKind.SubEqual, "-=");
        public static readonly Token PowerToken = new OperatorToken(TokenKind.Power, Operator.Power);
        public static readonly Token PowEqualToken = new SymbolToken(TokenKind.PowEqual, "**=");
        public static readonly Token MultiplyToken = new OperatorToken(TokenKind.Multiply, Operator.Multiply);
        public static readonly Token MulEqualToken = new SymbolToken(TokenKind.MulEqual, "*=");
        public static readonly Token FloorDivideToken = new OperatorToken(TokenKind.FloorDivide, Operator.FloorDivide);
        public static readonly Token FloordivEqualToken = new SymbolToken(TokenKind.FloordivEqual, "//=");
        public static readonly Token DivideToken = new OperatorToken(TokenKind.Divide, Operator.Divide);
        public static readonly Token DivEqualToken = new SymbolToken(TokenKind.DivEqual, "/=");
        public static readonly Token ModToken = new OperatorToken(TokenKind.Mod, Operator.Mod);
        public static readonly Token ModEqualToken = new SymbolToken(TokenKind.ModEqual, "%=");
        public static readonly Token LeftShiftToken = new OperatorToken(TokenKind.LeftShift, Operator.LeftShift);
        public static readonly Token LshiftEqualToken = new SymbolToken(TokenKind.LshiftEqual, "<<=");
        public static readonly Token RightShiftToken = new OperatorToken(TokenKind.RightShift, Operator.RightShift);
        public static readonly Token RshiftEqualToken = new SymbolToken(TokenKind.RshiftEqual, ">>=");
        public static readonly Token BitwiseAndToken = new OperatorToken(TokenKind.BitwiseAnd, Operator.BitwiseAnd);
        public static readonly Token AndEqualToken = new SymbolToken(TokenKind.AndEqual, "&=");
        public static readonly Token BitwiseOrToken = new OperatorToken(TokenKind.BitwiseOr, Operator.BitwiseOr);
        public static readonly Token OrEqualToken = new SymbolToken(TokenKind.OrEqual, "|=");
        public static readonly Token XorToken = new OperatorToken(TokenKind.Xor, Operator.Xor);
        public static readonly Token XorEqualToken = new SymbolToken(TokenKind.XorEqual, "^=");
        public static readonly Token LessThanToken = new OperatorToken(TokenKind.LessThan, Operator.LessThan);
        public static readonly Token GreaterThanToken = new OperatorToken(TokenKind.GreaterThan, Operator.GreaterThan);
        public static readonly Token LessThanOrEqualToken = new OperatorToken(TokenKind.LessThanOrEqual, Operator.LessThanOrEqual);
        public static readonly Token GreaterThanOrEqualToken = new OperatorToken(TokenKind.GreaterThanOrEqual, Operator.GreaterThanOrEqual);
        public static readonly Token EqualToken = new OperatorToken(TokenKind.Equal, Operator.Equal);
        public static readonly Token NotEqualToken = new OperatorToken(TokenKind.NotEqual, Operator.NotEqual);
        public static readonly Token LessThanGreaterThanToken = new SymbolToken(TokenKind.LessThanGreaterThan, "<>");
        public static readonly Token LParenToken = new SymbolToken(TokenKind.LParen, "(");
        public static readonly Token RParenToken = new SymbolToken(TokenKind.RParen, ")");
        public static readonly Token LBracketToken = new SymbolToken(TokenKind.LBracket, "[");
        public static readonly Token RBracketToken = new SymbolToken(TokenKind.RBracket, "]");
        public static readonly Token LBraceToken = new SymbolToken(TokenKind.LBrace, "{");
        public static readonly Token RBraceToken = new SymbolToken(TokenKind.RBrace, "}");
        public static readonly Token CommaToken = new SymbolToken(TokenKind.Comma, ",");
        public static readonly Token ColonToken = new SymbolToken(TokenKind.Colon, ":");
        public static readonly Token BackquoteToken = new SymbolToken(TokenKind.Backquote, "`");
        public static readonly Token SemicolonToken = new SymbolToken(TokenKind.Semicolon, ";");
        public static readonly Token AssignToken = new SymbolToken(TokenKind.Assign, "=");
        public static readonly Token TwidleToken = new SymbolToken(TokenKind.Twidle, "~");
        public static readonly Token AtToken = new SymbolToken(TokenKind.At, "@");

        public static readonly Token KeywordAndToken = new SymbolToken(TokenKind.KeywordAnd, "and");
        public static readonly Token KeywordAssertToken = new SymbolToken(TokenKind.KeywordAssert, "assert");
        public static readonly Token KeywordBreakToken = new SymbolToken(TokenKind.KeywordBreak, "break");
        public static readonly Token KeywordClassToken = new SymbolToken(TokenKind.KeywordClass, "class");
        public static readonly Token KeywordContinueToken = new SymbolToken(TokenKind.KeywordContinue, "continue");
        public static readonly Token KeywordDefToken = new SymbolToken(TokenKind.KeywordDef, "def");
        public static readonly Token KeywordDelToken = new SymbolToken(TokenKind.KeywordDel, "del");
        public static readonly Token KeywordElifToken = new SymbolToken(TokenKind.KeywordElif, "elif");
        public static readonly Token KeywordElseToken = new SymbolToken(TokenKind.KeywordElse, "else");
        public static readonly Token KeywordExceptToken = new SymbolToken(TokenKind.KeywordExcept, "except");
        public static readonly Token KeywordExecToken = new SymbolToken(TokenKind.KeywordExec, "exec");
        public static readonly Token KeywordFinallyToken = new SymbolToken(TokenKind.KeywordFinally, "finally");
        public static readonly Token KeywordForToken = new SymbolToken(TokenKind.KeywordFor, "for");
        public static readonly Token KeywordFromToken = new SymbolToken(TokenKind.KeywordFrom, "from");
        public static readonly Token KeywordGlobalToken = new SymbolToken(TokenKind.KeywordGlobal, "global");
        public static readonly Token KeywordIfToken = new SymbolToken(TokenKind.KeywordIf, "if");
        public static readonly Token KeywordImportToken = new SymbolToken(TokenKind.KeywordImport, "import");
        public static readonly Token KeywordInToken = new SymbolToken(TokenKind.KeywordIn, "in");
        public static readonly Token KeywordIsToken = new SymbolToken(TokenKind.KeywordIs, "is");
        public static readonly Token KeywordLambdaToken = new SymbolToken(TokenKind.KeywordLambda, "lambda");
        public static readonly Token KeywordNotToken = new SymbolToken(TokenKind.KeywordNot, "not");
        public static readonly Token KeywordOrToken = new SymbolToken(TokenKind.KeywordOr, "or");
        public static readonly Token KeywordPassToken = new SymbolToken(TokenKind.KeywordPass, "pass");
        public static readonly Token KeywordPrintToken = new SymbolToken(TokenKind.KeywordPrint, "print");
        public static readonly Token KeywordRaiseToken = new SymbolToken(TokenKind.KeywordRaise, "raise");
        public static readonly Token KeywordReturnToken = new SymbolToken(TokenKind.KeywordReturn, "return");
        public static readonly Token KeywordTryToken = new SymbolToken(TokenKind.KeywordTry, "try");
        public static readonly Token KeywordWhileToken = new SymbolToken(TokenKind.KeywordWhile, "while");
        public static readonly Token KeywordYieldToken = new SymbolToken(TokenKind.KeywordYield, "yield");

        public static readonly Dictionary<SymbolId, Token> Keywords = new Dictionary<SymbolId, Token>();

        static Tokens() {
            Keywords[SymbolTable.StringToId("and")] = KeywordAndToken;
            Keywords[SymbolTable.StringToId("assert")] = KeywordAssertToken;
            Keywords[SymbolTable.StringToId("break")] = KeywordBreakToken;
            Keywords[SymbolTable.StringToId("class")] = KeywordClassToken;
            Keywords[SymbolTable.StringToId("continue")] = KeywordContinueToken;
            Keywords[SymbolTable.StringToId("def")] = KeywordDefToken;
            Keywords[SymbolTable.StringToId("del")] = KeywordDelToken;
            Keywords[SymbolTable.StringToId("elif")] = KeywordElifToken;
            Keywords[SymbolTable.StringToId("else")] = KeywordElseToken;
            Keywords[SymbolTable.StringToId("except")] = KeywordExceptToken;
            Keywords[SymbolTable.StringToId("exec")] = KeywordExecToken;
            Keywords[SymbolTable.StringToId("finally")] = KeywordFinallyToken;
            Keywords[SymbolTable.StringToId("for")] = KeywordForToken;
            Keywords[SymbolTable.StringToId("from")] = KeywordFromToken;
            Keywords[SymbolTable.StringToId("global")] = KeywordGlobalToken;
            Keywords[SymbolTable.StringToId("if")] = KeywordIfToken;
            Keywords[SymbolTable.StringToId("import")] = KeywordImportToken;
            Keywords[SymbolTable.StringToId("in")] = KeywordInToken;
            Keywords[SymbolTable.StringToId("is")] = KeywordIsToken;
            Keywords[SymbolTable.StringToId("lambda")] = KeywordLambdaToken;
            Keywords[SymbolTable.StringToId("not")] = KeywordNotToken;
            Keywords[SymbolTable.StringToId("or")] = KeywordOrToken;
            Keywords[SymbolTable.StringToId("pass")] = KeywordPassToken;
            Keywords[SymbolTable.StringToId("print")] = KeywordPrintToken;
            Keywords[SymbolTable.StringToId("raise")] = KeywordRaiseToken;
            Keywords[SymbolTable.StringToId("return")] = KeywordReturnToken;
            Keywords[SymbolTable.StringToId("try")] = KeywordTryToken;
            Keywords[SymbolTable.StringToId("while")] = KeywordWhileToken;
            Keywords[SymbolTable.StringToId("yield")] = KeywordYieldToken;
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
