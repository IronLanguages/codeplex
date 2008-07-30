/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Scripting;
using ToyScript.Parser.Ast;

namespace ToyScript.Parser {
    class ToyParser {
        private ToyTokenizer _tokenizer;
        private Token _nextToken;

        public ToyParser(SourceUnit source) {
            _tokenizer = new ToyTokenizer(source);
            _nextToken = _tokenizer.Next();
        }

        public Token PeekToken() {
            return _nextToken;
        }

        public Token NextToken() {
            Token ret = _nextToken;
            _nextToken = _tokenizer.Next();
            return ret;
        }

        public bool MaybeEatToken(TokenKind kind) {
            if (PeekToken().Kind == kind) {
                NextToken();
                return true;
            } else {
                return false;
            }
        }

        public Token EatToken(TokenKind kind) {
            Token ret = NextToken();
            if (ret.Kind != kind) {
                throw SyntaxError(string.Format("expected {0} but found {1}", kind, ret.Kind));
            }
            return ret;
        }

        internal static Exception SyntaxError(string message) {
            return new SyntaxErrorException(message);
        }

        public Token ParseName() {
            return EatToken(TokenKind.Name);
        }


        public Statement ParseFile() {
            List<Statement> stmts = new List<Statement>();
            while (!_tokenizer.EndOfFile) {
                stmts.Add(ParseStatement());
            }
            SourceSpan span = SourceSpan.None;
            if (stmts.Count > 0) {
                span = new SourceSpan(stmts[0].Start, stmts[stmts.Count - 1].End);
            }
            return new Block(span, stmts.ToArray());
        }

        public Statement ParseStatement() {
            Token tok = PeekToken();

            switch (tok.Kind) {
                case TokenKind.SemiColon:
                    return new Empty(tok.Span);
                case TokenKind.OpenCurly:
                    return ParseBlockStatement();
                case TokenKind.KwWhile:
                    return ParseWhileStatement();
                case TokenKind.KwIf:
                    return ParseIfStatement();
                case TokenKind.KwDef:
                    return ParseDefStatement();
                case TokenKind.KwReturn:
                    return ParseReturnStatement();
                case TokenKind.KwVar:
                    return ParseVarStatement();
                case TokenKind.KwPrint:
                    return ParsePrintStatement();
                case TokenKind.KwImport:
                    return ParseImportStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        private Statement ParseExpressionStatement() {
            Expression expr = ParseExpression();
            MaybeEatToken(TokenKind.SemiColon);
            return new ExpressionStatement(expr.Span, expr);
        }

        private Statement ParseReturnStatement() {
            Token @return = EatToken(TokenKind.KwReturn);
            if (MaybeEatToken(TokenKind.SemiColon)) {
                return new Return(@return.Span);
            } else {
                Expression expr = ParseExpression();
                MaybeEatToken(TokenKind.SemiColon);
                return new Return(new SourceSpan(@return.Start, expr.End), expr);
            }
        }

        private Statement ParsePrintStatement() {
            Token print = EatToken(TokenKind.KwPrint);
            Expression expr = ParseExpression();
            MaybeEatToken(TokenKind.SemiColon);
            return new Print(new SourceSpan(print.Start, expr.End), expr);
        }

        private Statement ParseImportStatement() {
            Token import = EatToken(TokenKind.KwImport);
            Token name = ParseName();
            MaybeEatToken(TokenKind.SemiColon);
            return new Import(new SourceSpan(import.Start, name.End), name.Image);
        }

        private Statement ParseVarStatement() {
            Token var = EatToken(TokenKind.KwVar);
            Token name = ParseName();

            if (MaybeEatToken(TokenKind.Equal)) {
                Expression value = ParseExpression();
                MaybeEatToken(TokenKind.SemiColon);
                return new Var(new SourceSpan(var.Start, value.End), name.Image, value);
            } else {
                MaybeEatToken(TokenKind.SemiColon);
                return new Var(new SourceSpan(var.Start, name.End), name.Image, null);
            }
        }


        private Statement ParseBlockStatement() {
            Token open = EatToken(TokenKind.OpenCurly);
            List<Statement> stmts = new List<Statement>();
            Token close;
            for (; ; ) {
                close = PeekToken();
                if (close.Kind == TokenKind.CloseCurly) {
                    NextToken();
                    break;
                }
                stmts.Add(ParseStatement());
            }
            return new Block(new SourceSpan(open.Start, close.End), stmts.ToArray());
        }

        private Statement ParseWhileStatement() {
            Token @while = EatToken(TokenKind.KwWhile);
            Expression test = ParseExpression();
            Statement body = ParseStatement();
            return new While(new SourceSpan(@while.Start, body.End), test, body);
        }

        private Statement ParseIfStatement() {
            Token @if = EatToken(TokenKind.KwIf);
            Expression test = ParseExpression();
            Statement body = ParseStatement();
            Statement @else;
            SourceLocation end;
            if (MaybeEatToken(TokenKind.KwElse)) {
                @else = ParseStatement();
                end = @else.End;
            } else {
                @else = null;
                end = body.End;
            }
            return new If(new SourceSpan(@if.Start, end), test, body, @else);
        }

        private Statement ParseDefStatement() {
            Token def = EatToken(TokenKind.KwDef);
            string name = ParseName().Image;
            EatToken(TokenKind.OpenParen);
            List<string> parameters = new List<string>();
            Token close;
            for (; ; ) {
                close = PeekToken();
                if (close.Kind == TokenKind.CloseParen) {
                    NextToken();
                    break;
                }
                // Comma is optional
                MaybeEatToken(TokenKind.Comma);

                string parameter = ParseName().Image;
                //TODO add default values
                parameters.Add(parameter);
            }

            Statement body = ParseStatement();

            return new Def(new SourceSpan(def.Start, body.End), close.End, name, parameters.ToArray(), body);
        }

        public Expression ParseExpression() {
            Expression left = ParsePostfixExpression();

            Token tok = PeekToken();
            switch (tok.Kind) {
                case TokenKind.Add:
                    return FinishBinaryExpression(Operator.Add, left);
                case TokenKind.Subtract:
                    return FinishBinaryExpression(Operator.Subtract, left);
                case TokenKind.Multiply:
                    return FinishBinaryExpression(Operator.Multiply, left);
                case TokenKind.Divide:
                    return FinishBinaryExpression(Operator.Divide, left);

                case TokenKind.LessThan:
                    return FinishBinaryExpression(Operator.LessThan, left);
                case TokenKind.LessThanEqual:
                    return FinishBinaryExpression(Operator.LessThanOrEqual, left);
                case TokenKind.GreaterThan:
                    return FinishBinaryExpression(Operator.GreaterThan, left);
                case TokenKind.GreaterThanEqual:
                    return FinishBinaryExpression(Operator.GreaterThanOrEqual, left);
                case TokenKind.EqualEqual:
                    return FinishBinaryExpression(Operator.Equals, left);
                case TokenKind.NotEqual:
                    return FinishBinaryExpression(Operator.NotEquals, left);

                case TokenKind.Equal:
                    return FinishAssignment(left);

                default:
                    return left;
            }
        }

        private Expression FinishAssignment(Expression left) {
            EatToken(TokenKind.Equal);
            Expression right = ParseExpression();
            return new Assignment(new SourceSpan(left.Start, right.End), left, right);
        }

        private Expression FinishBinaryExpression(Operator op, Expression left) {
            NextToken();
            Expression right = ParseExpression();
            return new Binary(new SourceSpan(left.Start, right.End), op, left, right);
        }

        Expression ParsePostfixExpression() {
            Expression expr = ParsePrimaryExpression();
            return FinishExpressionTerminal(expr);
        }

        Expression ParsePrimaryExpression() {
            Token tok = NextToken();

            switch (tok.Kind) {
                case TokenKind.OpenParen:
                    Expression expr = ParseExpression();
                    Token end = EatToken(TokenKind.CloseParen);
                    return new Parentesized(new SourceSpan(tok.Start, end.End), expr);

                case TokenKind.Number:
                    return new Constant(tok.Span, Double.Parse(tok.Image));

                case TokenKind.String:
                    return new Constant(tok.Span, tok.Image);

                case TokenKind.Name:
                    return new Named(tok.Span, tok.Image);

                case TokenKind.KwNew:
                    return ParseNew(tok.Span);

                default:
                    throw SyntaxError(tok.Image);
            }
        }

        Expression ParseMemberExpression() {
            Expression expression = ParsePrimaryExpression();
            while (PeekToken().Kind == TokenKind.Dot) {
                expression = ParseMember(expression);
            }
            return expression;
        }

        private Expression ParseNew(SourceSpan start) {
            Expression constructor = ParseMemberExpression();
            EatToken(TokenKind.OpenParen);

            Token end;
            List<Expression> arguments = new List<Expression>();
            for(;;) {
                end = PeekToken();
                if (end.Kind == TokenKind.CloseParen) {
                    NextToken();
                    break;
                }

                arguments.Add(ParseExpression());
            };

            return new New(new SourceSpan(constructor.Start, end.End), constructor, arguments.ToArray());
        }

        private Expression FinishExpressionTerminal(Expression expr) {
            for (; ; ) {
                Token tok = PeekToken();
                switch (tok.Kind) {
                    case TokenKind.Dot:
                        expr = ParseMember(expr);
                        break;
                    case TokenKind.OpenParen:
                        expr = FinishCall(expr);
                        break;
                    case TokenKind.OpenBracket:
                        expr = FinishIndex(expr);
                        break;
                    default:
                        return expr;
                }
            }
        }

        private Expression ParseMember(Expression expr) {
            EatToken(TokenKind.Dot);
            Token name = ParseName();
            return new Member(new SourceSpan(expr.Start, name.End), expr, name.Image);
        }

        private Expression FinishCall(Expression target) {
            EatToken(TokenKind.OpenParen);

            List<Expression> args = new List<Expression>();

            if (PeekToken().Kind != TokenKind.CloseParen) {
                args.Add(ParseExpression());
                while (MaybeEatToken(TokenKind.Comma)) {
                    args.Add(ParseExpression());
                }
            }

            Token close = EatToken(TokenKind.CloseParen);

            return new Call(new SourceSpan(target.Start, close.End), target, args.ToArray());
        }

        private Expression FinishIndex(Expression target) {
            EatToken(TokenKind.OpenBracket);
            Expression index = ParseExpression();
            Token close = EatToken(TokenKind.CloseBracket);
            return new Index(new SourceSpan(target.Start, close.End), target, index);
        }

        public Statement ParseInteractiveStatement() {
            if (PeekToken().Kind != TokenKind.EOF) {
                Statement s = ParseStatement();
                ExpressionStatement es = s as ExpressionStatement;
                if (es != null) {
                    Expression expr = es.Expression;
                    s = new Print(expr.Span, expr);
                }
                return s;
            } else {
                return new Empty(SourceSpan.None);
            }
        }
    }
}
