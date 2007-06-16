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
    public class AndExpression : Expression {
        private readonly Expression _left, _right;

        public AndExpression(Expression left, Expression right, SourceSpan span)
            : base(span) {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");

            _left = left;
            _right = right;
        }

        public Expression Right {
            get { return _right; }
        }

        public Expression Left {
            get { return _left; }
        }

        public override object Evaluate(CodeContext context) {
            object ret = _left.Evaluate(context);
            if (context.LanguageContext.IsTrue(ret)) return _right.Evaluate(context);
            else return ret;
        }

        public override void Emit(CodeGen cg) {
            Slot lhsSlot = cg.GetLocalTmp(typeof(object));
            
            _left.EmitAsObject(cg);
            lhsSlot.EmitSet(cg);
            lhsSlot.EmitGet(cg);
            cg.EmitLanguageContext();            
            lhsSlot.EmitGet(cg);
            cg.EmitCall(typeof(LanguageContext), "IsTrue");
            //cg.emitNonzero(left);
            Label l = cg.DefineLabel();
            cg.Emit(OpCodes.Brfalse, l);
            cg.Emit(OpCodes.Pop);
            _right.EmitAsObject(cg);
            cg.MarkLabel(l);

            cg.FreeLocalTmp(lhsSlot);
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
