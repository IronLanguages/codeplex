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
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    class Empty : Statement {
        public Empty(SourceSpan span)
            : base(span) {
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            return AstUtils.Empty(Span);
        }
    }
}
