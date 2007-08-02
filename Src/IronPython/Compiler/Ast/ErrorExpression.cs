/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    public class ErrorExpression : Expression {
        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            ag.AddError("Error expression encountered", Span);
            return null;
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                ;
            }
            walker.PostWalk(this);
        }
    }
}