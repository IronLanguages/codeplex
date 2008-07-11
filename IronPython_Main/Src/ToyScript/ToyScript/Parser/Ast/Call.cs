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

    class Call : Expression {
        private readonly Expression _target;
        private readonly Expression[] _arguments;

        public Call(SourceSpan span, Expression target, Expression[] arguments)
            : base(span) {
            _target = target;
            _arguments = arguments;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            MSAst.Expression target = _target.Generate(tg);
            MSAst.Expression[] arguments = new MSAst.Expression[_arguments.Length];
            for (int i = 0; i < _arguments.Length; i++) {
                arguments[i] = _arguments[i].Generate(tg);
            }

            return tg.Call(target, arguments);
        }
    }
}
