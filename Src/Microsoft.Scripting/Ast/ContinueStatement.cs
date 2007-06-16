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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class ContinueStatement : Statement {
        private Statement _statement;

        public ContinueStatement()
            : this(null, SourceSpan.None) {
        }

        public ContinueStatement(SourceSpan span)
            : this(null, span) {
        }

        public ContinueStatement(Statement statement)
            : this(statement, SourceSpan.None) {
        }

        public ContinueStatement(Statement statement, SourceSpan span)
            : base(span) {
            _statement = statement;
        }

        public Statement Statement {
            get { return _statement; }
            set { _statement = value; }
        }

        public override void Emit(CodeGen cg) {
            if (!cg.InLoop()) {
                cg.Context.AddError("'continue' not properly in loop", this);
                return;
            }
            
            cg.EmitPosition(Start, End);

            if (_statement != null) {
                cg.CheckAndPushTargets(_statement);
            }

            cg.EmitContinue();

            if (_statement != null) {
                cg.PopTargets();
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                ;
            }
            walker.PostWalk(this);
        }
    }
}
