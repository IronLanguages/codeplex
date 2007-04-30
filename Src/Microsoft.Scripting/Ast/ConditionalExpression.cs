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
using System.Diagnostics;

using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class ConditionalExpression : Expression {
        private readonly Expression _test;
        private readonly Expression _true;
        private readonly Expression _false;


        public ConditionalExpression(Expression testExpression, Expression trueExpression, Expression falseExpression, SourceSpan span)
            : base(span) {
            _test = testExpression;
            _true = trueExpression;
            _false = falseExpression;
        }

        public Expression FalseExpression {
            get { return _false; }
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression TrueExpression {
            get { return _true; }
        }

        public override System.Type ExpressionType {
            get {
                if (_true.ExpressionType != _false.ExpressionType) {
                    return typeof(object);
                } else {
                    return _false.ExpressionType;
                }
            }
        }

        public override object Evaluate(CodeContext context) {
            object ret = _test.Evaluate(context);
            if (context.LanguageContext.IsTrue(ret)) {
                return _true.Evaluate(context);
            } else {
                return _false.Evaluate(context);
            }
        }

        public override void EmitAs(CodeGen cg, System.Type asType) {
            Label eoi = cg.DefineLabel();
            Label next = cg.DefineLabel();
            cg.EmitTestTrue(_test);
            cg.Emit(OpCodes.Brfalse, next);
            _true.EmitAs(cg, asType);
            cg.Emit(OpCodes.Br, eoi);
            cg.MarkLabel(next);
            _false.EmitAs(cg, asType);
            cg.MarkLabel(eoi);
        }

        public override void Emit(CodeGen cg) {
            EmitAs(cg, typeof(object));
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _test.Walk(walker);
                _true.Walk(walker);
                _false.Walk(walker);
            }
            walker.PostWalk(this);
        }

        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse) {
            return new ConditionalExpression(test, ifTrue, ifFalse, SourceSpan.None);
        }
    }
}
