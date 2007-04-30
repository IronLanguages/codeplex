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
    public class ReturnStatement : Statement {
        private readonly Expression _expr;

        public ReturnStatement(Expression expression)
            : this(expression, SourceSpan.None) {
        }

        public ReturnStatement(Expression expression, SourceSpan span)
            : base(span) {
            _expr = expression;
        }

        public Expression Expression {
            get { return _expr; }
        }

        public override object Execute(CodeContext context) {
            return _expr.Evaluate(context);
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            cg.EmitReturn(_expr);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                if (_expr != null) _expr.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
