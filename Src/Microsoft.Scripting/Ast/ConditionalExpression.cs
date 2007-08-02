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
using System.Diagnostics;

using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class ConditionalExpression : Expression {
        private readonly Expression _test;
        private readonly Expression _true;
        private readonly Expression _false;
        private readonly Type _expressionType;

        internal ConditionalExpression(SourceSpan span, Expression testExpression, Expression trueExpression, Expression falseExpression, bool allowUpcast)
            : base(span) {
            _test = testExpression;
            _true = trueExpression;
            _false = falseExpression;

            if (_true.ExpressionType.IsAssignableFrom(_false.ExpressionType)) {
                _expressionType = _true.ExpressionType;
            } else if (_false.ExpressionType.IsAssignableFrom(_true.ExpressionType)) {
                _expressionType = _false.ExpressionType;
            } else if (allowUpcast) {
                _expressionType = typeof(object);
                _true = Ast.Cast(_true, _expressionType);
                _false = Ast.Cast(_false, _expressionType);
            } else {
                throw new ArgumentException(String.Format("Cannot determine the type of the conditional expression: {0}, {1}.", _true.ExpressionType, _false.ExpressionType));
            }            
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

        public override Type ExpressionType {
            get {
                return _expressionType;
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

        public override void Emit(CodeGen cg) {
            Label eoi = cg.DefineLabel();
            Label next = cg.DefineLabel();
            _test.EmitAs(cg, typeof(bool));
            cg.Emit(OpCodes.Brfalse, next);
            _true.EmitCast(cg, _expressionType);
            cg.Emit(OpCodes.Br, eoi);
            cg.MarkLabel(next);
            _false.EmitCast(cg, _expressionType);
            cg.MarkLabel(eoi);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _test.Walk(walker);
                _true.Walk(walker);
                _false.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static ConditionalExpression Condition(Expression test, Expression trueValue, Expression falseValue) {
            return Condition(SourceSpan.None, test, trueValue, falseValue, false);
        }

        public static ConditionalExpression Condition(Expression test, Expression trueValue, Expression falseValue, bool allowUpcast) {
            return Condition(SourceSpan.None, test, trueValue, falseValue, allowUpcast);
        }

        public static ConditionalExpression Condition(SourceSpan span, Expression test, Expression trueValue, Expression falseValue) {
            return Condition(span, test, trueValue, falseValue, false);
        }

        /// <summary>
        /// AllowUpcast: casts both expressions to Object if neither is a subtype of the other.
        /// </summary>
        public static ConditionalExpression Condition(SourceSpan span, Expression test, Expression trueValue, Expression falseValue, bool allowUpcast) {
            if (test == null) throw new ArgumentNullException("test");
            if (trueValue == null) throw new ArgumentNullException("trueValue");
            if (falseValue == null) throw new ArgumentNullException("falseValue");
            
            return new ConditionalExpression(span, test, trueValue, falseValue, allowUpcast);
        }
    }
}
