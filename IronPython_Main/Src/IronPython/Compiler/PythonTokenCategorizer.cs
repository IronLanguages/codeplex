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

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using IronPython.Hosting;

namespace IronPython.Compiler {

    public sealed class PythonTokenCategorizer : TokenCategorizer {

        private readonly Tokenizer _tokenizer;

        public override SourceLocation CurrentPosition {
            get { return _tokenizer.TokenStart; }
        }

        public override object CurrentState {
            get { return _tokenizer.CurrentState; }
        }

        // TODO:
        public override bool IsRestartable {
            get { return false; }
        }

        public override ErrorSink ErrorSink {
            get {
                return _tokenizer.Errors;
            }
            set {
                _tokenizer.Errors = value;
            }
        }

        public PythonTokenCategorizer() {
            _tokenizer = new Tokenizer(new PythonErrorSink(false), true);
        }

        public override void Initialize(object state, SourceUnitReader sourceReader, SourceLocation initialLocation) {
            _tokenizer.Initialize(state, sourceReader, initialLocation);
        }

        public override bool SkipToken() {
            return _tokenizer.GetNextToken().Kind != TokenKind.EndOfFile;
        }

        public override TokenInfo ReadToken() {
            TokenInfo result = new TokenInfo();
            Token token = _tokenizer.GetNextToken();
            result.SourceSpan = _tokenizer.TokenSpan;

            switch (token.Kind) {
                case TokenKind.EndOfFile:
                    result.Category = TokenCategory.EndOfStream;
                    break;

                case TokenKind.Comment:
                    result.Category = TokenCategory.Comment;
                    break;

                case TokenKind.Name:
                    result.Category = TokenCategory.Identifier;
                    break;

                case TokenKind.Error:
                    result.Category = TokenCategory.Error;
                    break;

                case TokenKind.Constant:
                    result.Category = (token.Value is string) ? TokenCategory.StringLiteral : TokenCategory.NumericLiteral;
                    break;

                case TokenKind.LeftParenthesis:
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterStart;
                    break;

                case TokenKind.RightParenthesis:
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterEnd;
                    break;

                case TokenKind.LeftBracket:
                case TokenKind.LeftBrace:
                case TokenKind.RightBracket:
                case TokenKind.RightBrace:
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces;
                    break;

                case TokenKind.Colon:
                    result.Category = TokenCategory.Delimiter;
                    break;
                
                case TokenKind.Semicolon:
                    result.Category = TokenCategory.Delimiter;
                    break;

                case TokenKind.Comma:
                    result.Category = TokenCategory.Delimiter;
                    result.Trigger = TokenTriggers.ParameterNext;
                    break;

                case TokenKind.Dot:
                    result.Category = TokenCategory.Operator;
                    result.Trigger = TokenTriggers.MemberSelect;
                    break;

                case TokenKind.NewLine:
                    result.Category = TokenCategory.WhiteSpace;
                    break;
                
                default:
                    if (token.Kind >= TokenKind.FirstKeyword && token.Kind <= TokenKind.LastKeyword) {
                        result.Category = TokenCategory.Keyword;
                        break;
                    }
                    
                    result.Category = TokenCategory.Operator;
                    break;
            }

            return result;
        }
    }
}
