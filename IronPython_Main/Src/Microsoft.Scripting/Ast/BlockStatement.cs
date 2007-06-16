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

using System.Collections.Generic;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class BlockStatement : Statement {
        private readonly Statement[] _statements;

        public IList<Statement> Statements {
            get { return _statements; }
        }

        public BlockStatement(Statement[] statements)
            : this(statements, SourceSpan.None) {
        }

        public BlockStatement(Statement[] statements, SourceSpan span)
            : base(span) {
            Utils.Assert.NotNull(statements); 
            _statements = statements;
        }

        public override object Execute(CodeContext context) {
            object ret = Statement.NextStatement;
            foreach (Statement stmt in _statements) {
                ret = stmt.Execute(context);
                if (ret != Statement.NextStatement) break;
            }
            return ret;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(Span.Start, Span.End);
            // Should emit nop for the colon?
            foreach (Statement stmt in _statements) {
                stmt.Emit(cg);
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                foreach (Statement stmt in _statements) stmt.Walk(walker);
            }
            walker.PostWalk(this);
        }

        #region Factory methods

        public static Statement Block(List<Statement> statements) {
            if (statements.Count == 1) {
                return statements[0];
            } else {
                return Block(statements.ToArray());
            }
        }

        public static BlockStatement Block(params Statement[] statements) {
            return new BlockStatement(statements);
        }

        public static BlockStatement Block(SourceSpan span, params Statement[] statements) {
            return new BlockStatement(statements, span);
        }

        #endregion
    }
}
