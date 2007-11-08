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

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace ToyScript.Parser.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    class Index : Expression {
        private readonly Expression _target;
        private readonly Expression _index;

        private static bool Helper = false;

        public Index(SourceSpan span, Expression target, Expression index)
            : base(span) {
            _target = target;
            _index = index;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            if (Helper) {
                return Ast.Call(
                    typeof(ToyHelpers).GetMethod("GetItem"),
                    Ast.ConvertHelper(_target.Generate(tg), typeof(object)),
                    Ast.ConvertHelper(_index.Generate(tg), typeof(object))
                );
            } else {
                return Ast.Action.Operator(
                    Operators.GetItem,
                    typeof(object),
                    _target.Generate(tg),
                    _index.Generate(tg)
                );
            }
        }

        protected internal override MSAst.Expression GenerateAssign(ToyGenerator tg, MSAst.Expression right) {
            if (Helper) {
                return Ast.Call(
                    typeof(ToyHelpers).GetMethod("SetItem"),
                    Ast.ConvertHelper(_target.Generate(tg), typeof(object)),
                    Ast.ConvertHelper(_index.Generate(tg), typeof(object)),
                    Ast.ConvertHelper(right, typeof(object))
                );
            } else {
                return Ast.Action.Operator(
                    Operators.SetItem,
                    typeof(object),
                    _target.Generate(tg),
                    _index.Generate(tg),
                    right
                );
            }
        }
    }
}
