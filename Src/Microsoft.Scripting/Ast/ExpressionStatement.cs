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

using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class ExpressionStatement : Statement {
        private readonly Expression _expression;

        public ExpressionStatement(Expression expression)
            : this(expression, SourceSpan.None) {
        }

        public ExpressionStatement(Expression expression, SourceSpan span)
            : base(span) {
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }

        public override object Execute(CodeContext context) {
            _expression.Evaluate(context);
            return NextStatement;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            // expression needs to be emitted incase it has side-effects.
            _expression.EmitAs(cg, typeof(void));
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _expression.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
