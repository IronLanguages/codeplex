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
using System.Collections.Generic;
using System.Text;

namespace IronPython.Compiler {
    public partial class Tokenizer {

        Token NextOperator(int ch) {
            switch (ch) {
                #region Generated Tokenize Ops

                // *** BEGIN GENERATED CODE ***

                case '+':
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.AddEqualToken;
                    }
                    SetEnd(); return Tokens.AddToken;
                case '-':
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.SubtractEqualToken;
                    }
                    SetEnd(); return Tokens.SubtractToken;
                case '*':
                    if (NextChar('*')) {
                        if (NextChar('=')) {
                            SetEnd(); return Tokens.PowerEqualToken;
                        }
                        SetEnd(); return Tokens.PowerToken;
                    }
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.MultiplyEqualToken;
                    }
                    SetEnd(); return Tokens.MultiplyToken;
                case '/':
                    if (NextChar('/')) {
                        if (NextChar('=')) {
                            SetEnd(); return Tokens.FloorDivideEqualToken;
                        }
                        SetEnd(); return Tokens.FloorDivideToken;
                    }
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.DivEqualToken;
                    }
                    SetEnd(); return Tokens.DivideToken;
                case '%':
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.ModEqualToken;
                    }
                    SetEnd(); return Tokens.ModToken;
                case '<':
                    if (NextChar('<')) {
                        if (NextChar('=')) {
                            SetEnd(); return Tokens.LeftShiftEqualToken;
                        }
                        SetEnd(); return Tokens.LeftShiftToken;
                    }
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.LessThanOrEqualToken;
                    }
                    if (NextChar('>')) {
                        SetEnd(); return Tokens.LessThanGreaterThanToken;
                    }
                    SetEnd(); return Tokens.LessThanToken;
                case '>':
                    if (NextChar('>')) {
                        if (NextChar('=')) {
                            SetEnd(); return Tokens.RightShiftEqualToken;
                        }
                        SetEnd(); return Tokens.RightShiftToken;
                    }
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.GreaterThanOrEqualToken;
                    }
                    SetEnd(); return Tokens.GreaterThanToken;
                case '&':
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.BitwiseAndEqualToken;
                    }
                    SetEnd(); return Tokens.BitwiseAndToken;
                case '|':
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.BitwiseOrEqualToken;
                    }
                    SetEnd(); return Tokens.BitwiseOrToken;
                case '^':
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.XorEqualToken;
                    }
                    SetEnd(); return Tokens.XorToken;
                case '=':
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.EqualToken;
                    }
                    SetEnd(); return Tokens.AssignToken;
                case '!':
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.NotEqualToken;
                    }
                    return BadChar(NextChar());
                case '(':
                    parenLevel++;
                    SetEnd(); return Tokens.LeftParenthesisToken;
                case ')':
                    parenLevel--;
                    SetEnd(); return Tokens.RightParenthesisToken;
                case '[':
                    bracketLevel++;
                    SetEnd(); return Tokens.LeftBracketToken;
                case ']':
                    bracketLevel--;
                    SetEnd(); return Tokens.RightBracketToken;
                case '{':
                    braceLevel++;
                    SetEnd(); return Tokens.LeftBraceToken;
                case '}':
                    braceLevel--;
                    SetEnd(); return Tokens.RightBraceToken;
                case ',':
                    SetEnd(); return Tokens.CommaToken;
                case ':':
                    SetEnd(); return Tokens.ColonToken;
                case '`':
                    SetEnd(); return Tokens.BackQuoteToken;
                case ';':
                    SetEnd(); return Tokens.SemicolonToken;
                case '~':
                    SetEnd(); return Tokens.TwiddleToken;
                case '@':
                    SetEnd(); return Tokens.AtToken;

                // *** END GENERATED CODE ***

                #endregion
            }

            return null;
        }
    }
}
