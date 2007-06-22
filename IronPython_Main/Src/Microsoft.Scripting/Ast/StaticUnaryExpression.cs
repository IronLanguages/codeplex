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
using System.Reflection;

namespace Microsoft.Scripting.Ast {
    // TODO rename to UnaryExpression
    public class StaticUnaryExpression : Expression {
        private readonly Expression _operand;
        private readonly Operators _op;
        private readonly Type _type;
        private readonly MethodInfo _method;

        public StaticUnaryExpression(Operators op, Expression operand, MethodInfo method)
            : base(SourceSpan.None) {
            _operand = operand;
            _method = method;
            _op = op;
            _type = method.ReturnType;
        }

        public StaticUnaryExpression(Operators op, Expression operand, Type type)
            : this(op, operand, type, SourceSpan.None) {
        }

        public StaticUnaryExpression(Operators op, Expression operand, Type type, SourceSpan span)
            : base(span) {
            if (operand == null) throw new ArgumentNullException("operand");

            _operand = operand;
            _op = op;
            _type = type;
        }

        public Expression Operand {
            get { return _operand; }
        }

        public override Type ExpressionType {
            get { return _type; }
        }

        public override void Emit(CodeGen cg) {
            switch (_op) {
                case Operators.Coerce:
                    _operand.EmitAs(cg, _operand.ExpressionType);
                    cg.EmitCast(_operand.ExpressionType, _type);
                    break;
                default:
                    if (_method != null) {
                        _operand.EmitAs(cg, _method.IsStatic ? _method.GetParameters()[0].ParameterType : _method.DeclaringType);
                        cg.EmitCall(_method);
                        break;
                    }
                    throw new NotImplementedException();
            }
        }

        public override object Evaluate(CodeContext context) {
            object x = _operand.Evaluate(context);
            switch (_op) {
                case Operators.Coerce:
                    return context.LanguageContext.Binder.Convert(x, _type);
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

        public static StaticUnaryExpression Convert(Expression expression, Type type) {
            return new StaticUnaryExpression(Operators.Coerce, expression, type, SourceSpan.None);
        }
    }
}
