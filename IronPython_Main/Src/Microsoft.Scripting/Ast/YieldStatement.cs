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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class YieldStatement : Statement {
        private readonly Expression _expr;
        private YieldTarget _target;

        public YieldStatement(Expression expression, SourceSpan span)
            : base(span) {
            _expr = expression;
        }

        public Expression Expression {
            get { return _expr; }
        }

        internal YieldTarget Target {
            get { return _target; }
            set { _target = value; }
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            cg.EmitYield(_expr, _target);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _expr.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
