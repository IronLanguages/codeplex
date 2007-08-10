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
using System.Diagnostics;
using System.Reflection.Emit;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class TypeBinaryExpression : Expression {
        private readonly Expression _expression;
        private readonly Type _typeOperand;

        internal TypeBinaryExpression(SourceSpan span, Expression expression, Type typeOperand)
            : base(span) {
            Debug.Assert(expression != null);
            Debug.Assert(typeOperand != null);

            _expression = expression;
            _typeOperand = typeOperand;
        }

        public Expression Expression {
            get { return _expression; }
        }

        public Type TypeOperand {
            get { return _typeOperand; }
        }

        public override Type ExpressionType {
            get {
                return typeof(bool);
            }
        }

        public override bool IsConstant(object value) {
            // allow constant TypeIs expressions to be optimized away
            if (value is bool && ((bool)value) == true) {
                return _typeOperand.IsAssignableFrom(_expression.ExpressionType);
            }
            return false;
        }

        public override void Emit(CodeGen cg) {
            if (_typeOperand.IsAssignableFrom(_expression.ExpressionType)) {
                // if its always true just emit the bool
                cg.EmitConstant(true);
                return;
            }

            _expression.EmitAsObject(cg);
            cg.Emit(OpCodes.Isinst, _typeOperand);
            cg.Emit(OpCodes.Ldnull);
            cg.Emit(OpCodes.Cgt_Un);
        }

        protected override object DoEvaluate(CodeContext context) {
            return RuntimeHelpers.BooleanToObject(
                _typeOperand.IsInstanceOfType(_expression.Evaluate(context)));
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _expression.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static TypeBinaryExpression TypeIs(Expression expression, Type type) {
            if (!type.IsVisible) throw new ArgumentException(Resources.TypeMustBeVisible);

            return TypeIs(SourceSpan.None, expression, type);
        }

        public static TypeBinaryExpression TypeIs(SourceSpan span, Expression expression, Type type) {
            if (expression == null) {
                throw new ArgumentNullException("expression");
            }
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            return new TypeBinaryExpression(span, expression, type);
        }

    }
}
