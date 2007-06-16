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

using System.Reflection.Emit;
using System.Collections.Generic;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {

    public class IfStatement : Statement {
        private readonly IfStatementTest[] _tests;
        private readonly Statement _elseStmt;

        public IfStatement(IfStatementTest[] tests, Statement else_)
            : base(SourceSpan.None) {
            _tests = tests;
            _elseStmt = else_;
        }

        public IfStatement(IfStatementTest[] tests, Statement else_, SourceSpan span)
            : base(span) {
            _tests = tests;
            _elseStmt = else_;
        }

        public IList<IfStatementTest> Tests {
            get { return _tests; }
        }

        public Statement ElseStatement {
            get { return _elseStmt; }
        }

        public override object Execute(CodeContext context) {
            foreach (IfStatementTest t in _tests) {
                object val = t.Test.Evaluate(context);
                if (context.LanguageContext.IsTrue(val)) {
                    return t.Body.Execute(context);
                }
            }
            if (_elseStmt != null) {
                return _elseStmt.Execute(context);
            }
            return NextStatement;
        }

        public override void Emit(CodeGen cg) {
            Label eoi = cg.DefineLabel();
            foreach (IfStatementTest t in _tests) {
                Label next = cg.DefineLabel();
                cg.EmitPosition(t.Start, t.Header);
                t.Test.EmitAs(cg, typeof(bool));
                cg.Emit(OpCodes.Brfalse, next);
                t.Body.Emit(cg);
                // optimize no else case                
                cg.EmitSequencePointNone();     // hide compiler generated branch.
                cg.Emit(OpCodes.Br, eoi);
                cg.MarkLabel(next);
            }
            if (_elseStmt != null) {
                _elseStmt.Emit(cg);
            }
            cg.MarkLabel(eoi);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                foreach (IfStatementTest t in _tests) t.Walk(walker);
                if (_elseStmt != null) _elseStmt.Walk(walker);
            }
            walker.PostWalk(this);
        }

        #region FactoryMethods

        public static IfStatement IfThen(Expression condition, Statement body) {
            return IfThenElse(condition, body, null, SourceSpan.None);
        }

        public static IfStatement IfThen(Expression condition, Statement body, SourceSpan span) {
            return IfThenElse(condition, body, null, span);
        }

        public static IfStatement IfThenElse(Expression condition, Statement body, Statement else_, SourceSpan span) {
            return new IfStatement(
                new IfStatementTest[] {
                    new IfStatementTest(condition, body)
                },
                else_,
                span
            );
        }

        #endregion
    }
}
