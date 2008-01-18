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
    using Ast = MSAst.Ast;

    class Var : Statement {
        private readonly string _name;
        private readonly Expression _value;

        public Var(SourceSpan span, string name, Expression value)
            : base(span) {
            _name = name;
            _value = value;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            MSAst.Variable var = tg.GetOrMakeLocal(_name);

            if (_value != null) {
                return Ast.Statement(
                    Span,
                    Ast.Assign(
                        var,
                        Ast.ConvertHelper(
                            _value.Generate(tg),
                            var.Type
                        )
                    )
                );
            } else {
                return Ast.Empty(Span);
            }
        }
    }
}
