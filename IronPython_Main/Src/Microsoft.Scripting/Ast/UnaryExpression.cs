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

namespace Microsoft.Scripting.Ast {
    public class UnaryExpression : Expression {
        private readonly Expression _operand;
        private readonly Operators _op;
        private readonly Type _type;
        private readonly MethodInfo _method;

        internal UnaryExpression(SourceSpan span, Expression operand, Operators op, Type type, MethodInfo method)
            : base(span) {
            _operand = operand;
            _op = op;
            _type = type;
            _method = method;
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
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static UnaryExpression Cast(Expression expression, Type type) {
            return Cast(SourceSpan.None, expression, type);
        }

        public static UnaryExpression Cast(SourceSpan span, Expression expression, Type type) {
            return new UnaryExpression(span, expression, Operators.Coerce, type, null);
        }

        public static UnaryExpression Not(Expression expression, MethodInfo not) {
            return new UnaryExpression(SourceSpan.None, expression, Operators.Not, not.ReturnType, not);
        }

        public static UnaryExpression Unary(Operators op, Expression expression, MethodInfo method) {
            return new UnaryExpression(SourceSpan.None, expression, op, method.ReturnType, method);
        }
    }
}
