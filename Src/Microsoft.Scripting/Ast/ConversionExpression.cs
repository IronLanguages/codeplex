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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class ConversionExpression : Expression {
        private readonly Expression _expression;
        private readonly Type _conversion;

        internal ConversionExpression(SourceSpan span, Expression expression, Type conversion)
            : base(span) {
            _expression = expression;
            _conversion = conversion;
        }

        public override Type ExpressionType {
            get {
                return _conversion;
            }
        }

        public override void Emit(CodeGen cg) {
            _expression.Emit(cg);
            cg.EmitConvert(_expression.ExpressionType, _conversion);
        }

        protected override object DoEvaluate(CodeContext context) {
            object value = _expression.Evaluate(context);
            return context.LanguageContext.Binder.Convert(value, _conversion);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _expression.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
    public static partial class Ast {
        public static ConversionExpression Convert(Expression expression, Type conversion) {
            return Convert(SourceSpan.None, expression, conversion);
        }

        public static ConversionExpression Convert(SourceSpan span, Expression expression, Type conversion) {
            if (expression == null) {
                throw new ArgumentNullException("expression");
            }
            if (conversion == null) {
                throw new ArgumentNullException("conversion");
            }

            return new ConversionExpression(span, expression, conversion);
        }
    }
}
