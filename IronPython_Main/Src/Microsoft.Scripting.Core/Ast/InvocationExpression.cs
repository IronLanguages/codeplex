/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using System.Text;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public sealed class InvocationExpression : Expression {
        private readonly ReadOnlyCollection<Expression> _arguments;
        private readonly Expression _lambda;

        internal InvocationExpression(Expression lambda, Annotations annotations, ReadOnlyCollection<Expression> arguments, Type returnType)
            : base(ExpressionType.Invoke, returnType, annotations) {

            _lambda = lambda;
            _arguments = arguments;
        }

        public Expression Expression {
            get { return _lambda; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");
            
            builder.Append("Invoke(");
            _lambda.BuildString(builder);
            for (int i = 0, n = _arguments.Count; i < n; i++) {
                builder.Append(", ");
                _arguments[i].BuildString(builder);
            }
            builder.Append(")");
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitInvocation(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {

        //CONFORMING
        public static InvocationExpression Invoke(Expression expression, params Expression[] arguments) {
            return Invoke(expression, arguments.ToReadOnly());
        }

        //CONFORMING
        public static InvocationExpression Invoke(Expression expression, IEnumerable<Expression> arguments) {
            return Invoke(expression, Annotations.Empty, arguments);
        }

        //CONFORMING
        public static InvocationExpression Invoke(Expression expression, Annotations annotations, IEnumerable<Expression> arguments) {
            RequiresCanRead(expression, "expression");

            Type delegateType = expression.Type;
            if (delegateType == typeof(Delegate)) {
                throw Error.ExpressionTypeNotInvocable(delegateType);
            } else if (!TypeUtils.AreAssignable(typeof(Delegate), expression.Type)) {
                Type exprType = TypeUtils.FindGenericType(typeof(Expression<>), expression.Type);
                if (exprType == null) {
                    throw Error.ExpressionTypeNotInvocable(expression.Type);
                }
                delegateType = exprType.GetGenericArguments()[0];
            }

            var mi = delegateType.GetMethod("Invoke");
            var args = arguments.ToReadOnly();
            ValidateArgumentTypes(mi, ExpressionType.Invoke, ref args);
            return new InvocationExpression(expression, annotations, args, mi.ReturnType);
        }
    }
}
