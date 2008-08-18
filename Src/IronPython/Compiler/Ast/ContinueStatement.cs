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

using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {

    public class ContinueStatement : Statement {
        public ContinueStatement() {
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            if (ag.InFinally) {
                ag.AddError("'continue' not supported inside 'finally' clause", Span);
                return null;
            } else if (ag.InLoop) {
                return AstUtils.Continue(ag.LoopLabel, Span);
            } else {
                ag.AddError("'continue' not properly in loop", Span);
                return null;
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }

        internal override bool CanThrow {
            get {
                return false;
            }
        }
    }
}
