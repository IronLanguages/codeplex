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
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    class Import : Statement {
        private readonly string _name;

        public Import(SourceSpan span, string name)
            : base(span) {
            _name = name;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            MSAst.Expression var = tg.GetOrMakeLocal(_name);
            return AstUtils.Assign(
                var, 
                Ast.Call(
                    typeof(ToyHelpers).GetMethod("Import"),
                    AstUtils.CodeContext(),
                    Ast.Constant(_name)
                ), 
                Span
            );
        }
    }
}
