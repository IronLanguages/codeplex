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

using System.Scripting;
using MSAst = System.Linq.Expressions;

namespace ToyScript.Parser.Ast {
    class Parentesized : Expression {
        private readonly Expression _expression;

        public Parentesized(SourceSpan span, Expression expression)
            : base(span) {
            _expression = expression;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            return _expression.Generate(tg);
        }
    }
}
