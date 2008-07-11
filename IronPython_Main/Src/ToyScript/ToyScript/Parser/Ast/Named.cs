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
    using Ast = MSAst.Expression;

    class Named : Expression {
        private readonly string _name;

        public Named(SourceSpan span, string name)
            : base(span) {
            _name = name;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            return GetVariable(tg);
        }

        protected internal override MSAst.Expression GenerateAssign(ToyGenerator tg, MSAst.Expression right) {
            MSAst.Expression variable = GetVariable(tg);

            return Ast.Assign(
                variable,
                Ast.ConvertHelper(right, variable.Type)
            );
        }

        private MSAst.Expression GetVariable(ToyGenerator tg) {
            MSAst.Expression variable = tg.LookupName(_name);
            if (variable == null) {
                variable = tg.GetOrMakeGlobal(_name);
            }
            return variable;
        }
    }
}
