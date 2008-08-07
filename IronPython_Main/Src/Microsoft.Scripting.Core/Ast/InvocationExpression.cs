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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Scripting.Actions;
using System.Scripting.Utils;
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class InvocationExpression : Expression {
        private readonly ReadOnlyCollection<Expression> _arguments;
        private readonly Expression _lambda;

        internal InvocationExpression(Annotations annotations, Expression lambda, Type returnType, CallSiteBinder bindingInfo, ReadOnlyCollection<Expression> arguments)
            : base(ExpressionType.Invoke, returnType, annotations, bindingInfo) {
            if (IsBound) {
                RequiresBound(lambda, "lambda");
                RequiresBoundItems(arguments, "arguments");
            }
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
                builder.Append(",");
                _arguments[i].BuildString(builder);
            }
            builder.Append(")");
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
            MethodInfo mi = delegateType.GetMethod("Invoke");
            ParameterInfo[] pis = mi.GetParameters();

            ReadOnlyCollection<Expression> argList = arguments.ToReadOnly();
            if (argList.Count != pis.Length) {
                throw Error.IncorrectNumberOfLambdaArguments();
            }

            Expression[] newArgs = null;
            for (int i = 0, n = argList.Count; i < n; i++) {
                Expression arg = argList[i];
                ParameterInfo p = pis[i];
                RequiresCanRead(arg, "arguments");
                Type pType = p.ParameterType;
                if (pType.IsByRef) {
                    pType = pType.GetElementType();
                }
                if (!TypeUtils.AreReferenceAssignable(pType, arg.Type)) {
                    if (TypeUtils.IsSameOrSubclass(typeof(Expression), pType) && TypeUtils.AreAssignable(pType, arg.GetType())) {
                        arg = Expression.Quote(arg);
                    } else {
                        throw Error.ExpressionTypeDoesNotMatchParameter(arg.Type, pType);
                    }
                }
                if (newArgs == null && arg != argList[i]) {
                    newArgs = new Expression[argList.Count];
                    for (int j = 0; j < i; j++) {
                        newArgs[j] = argList[j];
                    }
                }
                if (newArgs != null) {
                    newArgs[i] = arg;
                }
            }
            if (newArgs != null) {
                argList = new ReadOnlyCollection<Expression>(newArgs);
            }

            return new InvocationExpression(annotations, expression, mi.ReturnType, null, argList);
        }

        /// <summary>
        /// A dynamic or unbound invoke
        /// </summary>
        /// <param name="annotations">annotations for the node</param>
        /// <param name="returnType">the type that the method returns, or null for an unbound node</param>
        /// <param name="expression">the callable object to call; must be non-null</param>
        /// <param name="bindingInfo">invoke binding information (method name, named arguments, etc)</param>
        /// <param name="arguments">the arguments to the call</param>
        /// <returns></returns>
        public static InvocationExpression Invoke(Annotations annotations, Type returnType, Expression expression, InvokeAction bindingInfo, params Expression[] arguments) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");

            RequiresCanRead(arguments, "arguments");
            var args = arguments.ToReadOnly();

            // Validate ArgumentInfos. For now, excludes the target expression.
            // This needs to be reconciled with MethodCallExpression
            if (bindingInfo.Arguments.Count > 0 && bindingInfo.Arguments.Count != args.Count) {
                throw Error.ArgumentCountMustMatchBinding(
                    args.Count + 1,
                    bindingInfo.Arguments.Count
                );
            }

            return new InvocationExpression(annotations, expression, returnType, bindingInfo, arguments.ToReadOnly());
        }

        public static InvocationExpression Invoke(Annotations annotations, Type returnType, Expression expression, CallSiteBinder binder, params Expression[] arguments) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(binder, "binder");
            RequiresCanRead(arguments, "arguments");

            return new InvocationExpression(annotations, expression, returnType, binder, arguments.ToReadOnly());
        }
    }
}
