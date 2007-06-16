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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// A simple static try/finally statement.  
    /// 
    /// TODO: Remove DynamicTryStatement and switch to all static exception handling.
    /// </summary>
    public class TryFinallyStatement : Statement {
        private Statement _body;
        private Statement _finallyBody;

        private TryFinallyStatement(Statement body, Statement finallyBody)
            : base(SourceSpan.None) {
            _body = body;
            _finallyBody = finallyBody;
        }

        public override void Emit(CodeGen cg) {
            cg.PushTryBlock();
            cg.BeginExceptionBlock();

            _body.Emit(cg);

            cg.PopTargets();
            cg.BeginFinallyBlock();

            _finallyBody.Emit(cg);

            cg.EndExceptionBlock();
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _body.Walk(walker);
                _finallyBody.Walk(walker);
            }
            walker.PostWalk(this);
        }

        public static TryFinallyStatement TryFinally(Statement body, Statement finallyBody) {
            return new TryFinallyStatement(body, finallyBody);
        }
    }
}
