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
                        SetEnd(); return Tokens.SubEqualToken;
                    }
                    SetEnd(); return Tokens.SubtractToken;
                case '*':
                    if (NextChar('*')) {
                        if (NextChar('=')) {
                            SetEnd(); return Tokens.PowEqualToken;
                        }
                        SetEnd(); return Tokens.PowerToken;
                    }
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.MulEqualToken;
                    }
                    SetEnd(); return Tokens.MultiplyToken;
                case '/':
                    if (NextChar('/')) {
                        if (NextChar('=')) {
                            SetEnd(); return Tokens.FloordivEqualToken;
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
                            SetEnd(); return Tokens.LshiftEqualToken;
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
                            SetEnd(); return Tokens.RshiftEqualToken;
                        }
                        SetEnd(); return Tokens.RightShiftToken;
                    }
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.GreaterThanOrEqualToken;
                    }
                    SetEnd(); return Tokens.GreaterThanToken;
                case '&':
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.AndEqualToken;
                    }
                    SetEnd(); return Tokens.BitwiseAndToken;
                case '|':
                    if (NextChar('=')) {
                        SetEnd(); return Tokens.OrEqualToken;
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
                    SetEnd(); return Tokens.LParenToken;
                case ')':
                    parenLevel--;
                    SetEnd(); return Tokens.RParenToken;
                case '[':
                    bracketLevel++;
                    SetEnd(); return Tokens.LBracketToken;
                case ']':
                    bracketLevel--;
                    SetEnd(); return Tokens.RBracketToken;
                case '{':
                    braceLevel++;
                    SetEnd(); return Tokens.LBraceToken;
                case '}':
                    braceLevel--;
                    SetEnd(); return Tokens.RBraceToken;
                case ',':
                    SetEnd(); return Tokens.CommaToken;
                case ':':
                    SetEnd(); return Tokens.ColonToken;
                case '`':
                    SetEnd(); return Tokens.BackquoteToken;
                case ';':
                    SetEnd(); return Tokens.SemicolonToken;
                case '~':
                    SetEnd(); return Tokens.TwidleToken;
                case '@':
                    SetEnd(); return Tokens.AtToken;

                // *** END GENERATED CODE ***

                #endregion
            }

            return null;
        }
    }
}
