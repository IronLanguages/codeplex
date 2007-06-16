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

    public class TypeBinaryExpression : Expression {
        private readonly Expression _expression;
        private readonly Type _typeOperand;

        public TypeBinaryExpression(Expression expression, Type typeOperand, SourceSpan span)
            : base(span) {
            if (expression == null) throw new ArgumentNullException("expression");
            if (typeOperand == null) throw new ArgumentNullException("typeOperand");

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

        public override void Emit(CodeGen cg) {
            _expression.EmitAsObject(cg);
            cg.Emit(OpCodes.Isinst, _typeOperand);
            cg.Emit(OpCodes.Ldnull);
            cg.Emit(OpCodes.Cgt_Un);
        }

        public override object Evaluate(CodeContext context) {
            return RuntimeHelpers.BooleanToObject(
                _typeOperand.IsInstanceOfType(_expression.Evaluate(context)));
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _expression.Walk(walker);
            }
            walker.PostWalk(this);
        }

        public static TypeBinaryExpression TypeIs(Expression expression, Type typeOperand) {
            return new TypeBinaryExpression(expression, typeOperand, SourceSpan.None);
        }
    }
}
