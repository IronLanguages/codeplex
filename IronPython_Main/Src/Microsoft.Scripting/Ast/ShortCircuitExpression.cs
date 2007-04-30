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
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class ShortCircuitExpression : Expression {
        private readonly UnaryOperator _testOp;
        private readonly MethodInfo _resultOp;
        private readonly Expression _left, _right;

        public ShortCircuitExpression(UnaryOperator testOp, MethodInfo resultOp, Expression left, Expression right) {
            if (testOp == null) throw new ArgumentNullException("testOp");
            if (resultOp == null) throw new ArgumentNullException("resultOp");
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");

            this._testOp = testOp;
            this._resultOp = resultOp;
            this._left = left;
            this._right = right;
            this.Start = left.Start;
            this.End = right.End;
        }

        public UnaryOperator TestOperator {
            get { return _testOp; }
        }

        public MethodInfo ResultOperator {
            get { return _resultOp; }
        }

        public Expression Right {
            get { return _right; }
        }

        public Expression Left {
            get { return _left; }
        }

        public override object Evaluate(CodeContext context) {
            object left = _left.Evaluate(context);
            if (!((bool)_testOp.Evaluate(left))) {
                object right = _right.Evaluate(context);
                return _resultOp.Invoke(null, new object[] { left,  right });
            } else {
                return left;
            }
        }

        public override void Emit(CodeGen cg) {
            _left.Emit(cg);
            cg.Emit(OpCodes.Dup);
            cg.EmitCall(_testOp.TargetMethod);
            Label l = cg.DefineLabel();
            cg.Emit(OpCodes.Brtrue, l);
            _right.Emit(cg);
            // UNDONE: EmitSite needed as well?
            cg.EmitCall(_resultOp);
            cg.MarkLabel(l);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _left.Walk(walker);
                _right.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
