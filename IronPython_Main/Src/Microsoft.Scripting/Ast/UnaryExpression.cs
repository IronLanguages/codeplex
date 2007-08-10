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
using Microsoft.Scripting.Generation;
using System.Diagnostics;

namespace Microsoft.Scripting.Ast {
    public enum UnaryOperators {
        Cast,
        Not
    }

    public class UnaryExpression : Expression {
        private readonly Expression _operand;
        private readonly UnaryOperators _op;
        private readonly Type _expressionType;

        internal UnaryExpression(SourceSpan span, Expression expression, UnaryOperators op, Type type)
            : base(span) {

            _operand = expression;
            _op = op;
            _expressionType = type;
        }

        public Expression Operand {
            get { return _operand; }
        }

        public override Type ExpressionType {
            get { return _expressionType; }
        }

        public UnaryOperators Operator {
            get { return _op; }
        }

        public override void Emit(CodeGen cg) {
            switch (_op) {
                case UnaryOperators.Cast:
                    _operand.EmitCast(cg, _expressionType);
                    break;

                case UnaryOperators.Not:
                    _operand.EmitAs(cg, typeof(bool));
                    cg.Emit(OpCodes.Ldc_I4_0);
                    cg.Emit(OpCodes.Ceq);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        protected override object DoEvaluate(CodeContext context) {
            object x = _operand.Evaluate(context);
            switch (_op) {
                case UnaryOperators.Cast:
                    // TODO: static cast only:
                    return context.LanguageContext.Binder.Convert(x, _expressionType);

                case UnaryOperators.Not:
                    return ((bool)context.LanguageContext.Binder.Convert(x, typeof(bool))) ? RuntimeHelpers.False : RuntimeHelpers.True;

                default:
                    throw new NotImplementedException();
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _operand.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static UnaryExpression Cast(Expression expression, Type type) {
            return Cast(SourceSpan.None, expression, type);
        }

        public static UnaryExpression Cast(SourceSpan span, Expression expression, Type type) {
            if (expression == null) throw new ArgumentNullException("expression");
            if (type == null) throw new ArgumentNullException("type");
            if (!type.IsVisible) throw new ArgumentException(Resources.TypeMustBeVisible);

            return new UnaryExpression(span, expression, UnaryOperators.Cast, type);
        }

        public static UnaryExpression Not(Expression expression) {
            return Not(SourceSpan.None, expression);
        }

        public static UnaryExpression Not(SourceSpan span, Expression expression) {
            if (expression == null) throw new ArgumentNullException("expression");

            return new UnaryExpression(span, expression, UnaryOperators.Not, typeof(bool));
        }
    }
}
