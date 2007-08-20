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
    /// A simple static try/finally statement.  REMOVE
    /// 
    /// TODO: Remove DynamicTryStatement and switch to all static exception handling.
    /// </summary>
    public class TryFinallyStatement : Statement {
        private readonly Statement _body;
        private readonly Statement _finally;

        internal TryFinallyStatement(SourceSpan span, Statement body, Statement @finally)
            : base(span) {
            _body = body;
            _finally = @finally;
        }

        public Statement Body {
            get { return _body; }
        }

        public Statement FinallyStatement {
            get { return _finally; }
        }

        public override void Emit(CodeGen cg) {
            cg.PushTryBlock();
            cg.BeginExceptionBlock();

            _body.Emit(cg);

            cg.PopTargets();
            cg.BeginFinallyBlock();

            _finally.Emit(cg);

            cg.EndExceptionBlock();
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _body.Walk(walker);
                _finally.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        // TODO: Remove/rename
        public static TryFinallyStatement SimpleTryFinally(Statement body, Statement @finally) {
            return new TryFinallyStatement(SourceSpan.None, body, @finally);
        }
    }
}
