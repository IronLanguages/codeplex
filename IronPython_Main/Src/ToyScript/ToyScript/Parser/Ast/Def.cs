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

using System.Collections.Generic;

using ToyScript.Runtime;

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace ToyScript.Parser.Ast {
    using Ast = MSAst.Expression;

    class Def : Statement {
        private readonly SourceLocation _header;
        private readonly string _name;
        private readonly string[] _parameters;
        private Statement _body;

        public Def(SourceSpan span, SourceLocation header, string name, string[] parameters, Statement body)
            : base(span) {
            _header = header;
            _name = name;
            _parameters = parameters;
            _body = body;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            // Push a new Scope for evaluating the functions body in
            ToyScope scope = tg.PushNewScope(_name);

            List<MSAst.Expression> names = new List<MSAst.Expression>();
            foreach (string parameter in _parameters) {
                tg.Scope.CreateParameter(parameter);
                names.Add(Ast.Constant(parameter));
            }

            MSAst.Expression body = _body.Generate(tg);
            MSAst.LambdaExpression lambda = scope.FinishScope(body, typeof(ToyCallTarget));

            tg.PopScope();

            return Ast.Assign(
                tg.GetOrMakeLocal(_name),
                Ast.Call(
                    typeof(ToyFunction).GetMethod("Create"),
                    Ast.Constant(_name),
                    Ast.NewArray(typeof(string[]), names),
                    lambda
                )
            );
        }
    }
}
