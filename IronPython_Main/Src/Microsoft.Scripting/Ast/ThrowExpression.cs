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
using System.Reflection.Emit;

using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class ThrowExpression : Expression {
        private readonly Expression _val;

        public ThrowExpression(Expression value)
            : this(value, SourceSpan.None) { }

        public ThrowExpression(Expression value, SourceSpan span)
            : base(span) {
            _val = value;
        }

        public Expression Value {
            get {
                return _val;
            }
        }

        public override Type ExpressionType {
            get {
                return typeof(void);
            }
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            if (_val == null) {
                cg.Emit(OpCodes.Rethrow);
            } else {
                _val.EmitAs(cg, typeof(Exception));
                cg.Emit(OpCodes.Throw);
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                if (_val != null) _val.Walk(walker);
            }
            walker.PostWalk(this);
        }

    }
}
