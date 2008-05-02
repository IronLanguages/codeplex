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
    using Ast = MSAst.Expression;

    class Assignment : Expression {
        private readonly Expression _lvalue;
        private readonly Expression _rvalue;

        public Assignment(SourceSpan span, Expression lvalue, Expression rvalue)
            : base(span) {
            _lvalue = lvalue;
            _rvalue = rvalue;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            return _lvalue.GenerateAssign(tg, _rvalue.Generate(tg));
        }
    }
}
