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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {

    public sealed class InvocationExpression : Expression {
        private readonly ReadOnlyCollection<Expression> _arguments;
        private readonly Expression _lambda;

        internal InvocationExpression(Annotations annotations, Expression lambda, Type returnType, CallAction bindingInfo, ReadOnlyCollection<Expression> arguments)
            : base(annotations, AstNodeType.Invoke, returnType, bindingInfo) {
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
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {

        public static InvocationExpression Invoke(Expression expression, params Expression[] arguments) {
            return Invoke(expression, CollectionUtils.ToReadOnlyCollection(arguments));
        }
        public static InvocationExpression Invoke(Expression expression, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(expression, "expression");

            Type delegateType = expression.Type;
            if (delegateType == typeof(Delegate)) {
                throw new ArgumentException(string.Format("Expression of type '{0}' cannot be invoked", delegateType));
            } else if (!AreAssignable(typeof(Delegate), expression.Type)) {
                throw new ArgumentException(string.Format("Expression of type '{0}' cannot be invoked", delegateType));
                // TODO: needs Expression<T>:
                //Type exprType = FindGenericType(typeof(Expression<>), expression.Type);
                //if (exprType == null) {
                //    throw new ArgumentException(string.Format("Expression of type '{0}' cannot be invoked", delegateType));
                //}
                //delegateType = exprType.GetGenericArguments()[0];
            }
            MethodInfo mi = delegateType.GetMethod("Invoke");
            ParameterInfo[] pis = mi.GetParameters();

            ReadOnlyCollection<Expression> argList = CollectionUtils.ToReadOnlyCollection(arguments);

            if (argList.Count != pis.Length) {
                throw new ArgumentException("Incorrect number of arguments supplied for lambda invocation");
            }

            ValidateCallArguments(pis, argList);

            // TODO: needs Expression.Quote:
            //Expression[] newArgs = null;
            //for (int i = 0, n = argList.Count; i < n; i++) {
            //    Expression arg = argList[i];
            //    ParameterInfo p = pis[i];
            //    Contract.RequiresNotNull(arg, "arguments");

            //    Type pType = p.ParameterType;
            //    if (pType.IsByRef) {
            //        pType = pType.GetElementType();
            //    }
            //    if (!AreReferenceAssignable(pType, arg.Type)) {
            //        if (IsSameOrSubclass(typeof(Expression), pType) && AreAssignable(pType, arg.GetType())) {
            //            arg = Expression.Quote(arg);
            //        } else {
            //            throw new ArgumentException(string.Format("Expression of type '{0}' cannot be used for parameter of type '{1}'", arg.Type, pType));
            //        }
            //    }
            //    if (newArgs == null && arg != argList[i]) {
            //        newArgs = new Expression[argList.Count];
            //        for (int j = 0; j < i; j++) {
            //            newArgs[j] = argList[j];
            //        }
            //    }
            //    if (newArgs != null) {
            //        newArgs[i] = arg;
            //    }
            //}
            //if (newArgs != null) {
            //    argList = new ReadOnlyCollection<Expression>(newArgs);
            //}
            return new InvocationExpression(Annotations.Empty, expression, mi.ReturnType, null, argList);
        }

        /// <summary>
        /// A dynamic or unbound invoke
        /// </summary>
        /// <param name="returnType">the type that the method returns, or null for an unbound node</param>
        /// <param name="expression">the instance to call; must be non-null</param>
        /// <param name="bindingInfo">call binding information (method name, named arguments, etc)</param>
        /// <param name="arguments">the arguments to the call</param>
        /// <returns></returns>
        public static InvocationExpression Invoke(Type returnType, Expression expression, CallAction bindingInfo, params Expression[] arguments) {
            return Invoke(Annotations.Empty, returnType, expression, bindingInfo, arguments);
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
        public static InvocationExpression Invoke(Annotations annotations, Type returnType, Expression expression, CallAction bindingInfo, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");

            // Validate ArgumentInfos. For now, excludes the target expression.
            // This needs to be reconciled with MethodCallExpression
            if (bindingInfo.Signature.ArgumentCount != arguments.Length) {
                throw new ArgumentException(
                    string.Format(
                        "Argument count '{0}' must match arguments in the binding information '{1}'",
                        arguments.Length,
                        bindingInfo.Signature.ArgumentCount
                    ),
                    "bindingInfo"
                );
            }

            return new InvocationExpression(annotations, expression, returnType, bindingInfo, CollectionUtils.ToReadOnlyCollection(arguments));
        }


        #region helpers (TODO: merge with DLR versions)

        // from System.Linq.Expressions.Expression
        private static bool AreReferenceAssignable(Type dest, Type src) {
            // WARNING: This actually implements "Is this identity assignable and/or reference assignable?"
            if (dest == src) {
                return true;
            }
            if (!dest.IsValueType && !src.IsValueType && AreAssignable(dest, src)) {
                return true;
            }
            return false;
        }
        // from System.Linq.Expressions.Expression
        private static bool AreAssignable(Type dest, Type src) {
            if (dest == src) {
                return true;
            }
            if (dest.IsAssignableFrom(src)) {
                return true;
            }
            if (dest.IsArray && src.IsArray && dest.GetArrayRank() == src.GetArrayRank() && AreReferenceAssignable(dest.GetElementType(), src.GetElementType())) {
                return true;
            }
            if (src.IsArray && dest.IsGenericType &&
                (dest.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>)
                || dest.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IList<>)
                || dest.GetGenericTypeDefinition() == typeof(System.Collections.Generic.ICollection<>))
                && dest.GetGenericArguments()[0] == src.GetElementType()) {
                return true;
            }
            return false;
        }

        #endregion
    }
}
