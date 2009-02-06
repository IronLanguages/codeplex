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
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class Utils {

        [Obsolete("use Expression.Call instead")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "span")]
        public static MethodCallExpression Call(MethodInfo method, SourceSpan span, params Expression[] arguments) {
            return Expression.Call(null, method, arguments);
        }

        [Obsolete("use Expression.Call instead")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "span")]
        public static MethodCallExpression Call(Expression instance, MethodInfo method, SourceSpan span, params Expression[] arguments) {
            return Expression.Call(instance, method, arguments);
        }


        /// <summary>
        /// The helper to create the AST method call node. Will add conversions (Utils.Convert)
        /// to parameters and instance if necessary.
        /// </summary>
        public static MethodCallExpression SimpleCallHelper(MethodInfo method, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.Requires(method.IsStatic, "method", "Method must be static");
            return SimpleCallHelper(null, method, arguments);
        }

        /// <summary>
        /// The helper to create the AST method call node. Will add conversions (Utils.Convert)
        /// to parameters and instance if necessary.
        /// </summary>
        public static MethodCallExpression SimpleCallHelper(Expression instance, MethodInfo method, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.Requires(instance != null ^ method.IsStatic, "instance");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");

            ParameterInfo[] parameters = method.GetParameters();

            ContractUtils.Requires(arguments.Length == parameters.Length, "arguments", "Incorrect number of arguments");

            if (instance != null) {
                instance = Convert(instance, method.DeclaringType);
            }

            Expression[] convertedArguments = ArgumentConvertHelper(arguments, parameters);

            ReadOnlyCollection<Expression> finalArgs;
            if (convertedArguments == arguments) {
                // we didn't convert anything, just convert the users original
                // array to a ROC.
                finalArgs = convertedArguments.ToReadOnly();
            } else {
                // we already copied the array so just stick it in a ROC.
                finalArgs = new ReadOnlyCollection<Expression>(convertedArguments);
            }

            // the arguments are now all correct, avoid re-validating the call parameters and
            // directly create the expression.
            return Expression.Call(instance, method, finalArgs);
        }

        private static Expression[] ArgumentConvertHelper(Expression[] arguments, ParameterInfo[] parameters) {
            Debug.Assert(arguments != null);
            Debug.Assert(arguments != null);

            Expression[] clone = null;
            for (int arg = 0; arg < arguments.Length; arg++) {
                Expression argument = arguments[arg];
                if (!CompatibleParameterTypes(parameters[arg].ParameterType, argument.Type)) {
                    // Clone the arguments array if needed
                    if (clone == null) {
                        clone = new Expression[arguments.Length];
                        // Copy the expressions into the clone
                        for (int i = 0; i < arg; i++) {
                            clone[i] = arguments[i];
                        }
                    }

                    argument = ArgumentConvertHelper(argument, parameters[arg].ParameterType);
                }

                if (clone != null) {
                    clone[arg] = argument;
                }
            }
            return clone ?? arguments;
        }

        private static Expression ArgumentConvertHelper(Expression argument, Type type) {
            if (argument.Type != type) {
                if (type.IsByRef) {
                    type = type.GetElementType();
                }
                if (argument.Type != type) {
                    argument = Convert(argument, type);
                }
            }
            return argument;
        }

        private static bool CompatibleParameterTypes(Type parameter, Type argument) {
            if (parameter == argument ||
                (!parameter.IsValueType && !argument.IsValueType && parameter.IsAssignableFrom(argument))) {
                return true;
            }
            if (parameter.IsByRef && parameter.GetElementType() == argument) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// The complex call helper to create the AST method call node.
        /// Will add conversions (Expression.Convert()), deals with default parameter values and params arrays.
        /// </summary>
        public static Expression ComplexCallHelper(MethodInfo method, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.Requires(method.IsStatic, "method", "Method must be static");
            return ComplexCallHelper(null, method, arguments);
        }

        // FxCop is wrong on this one. "method" is required as MethodInfo by the call to "Call" factory
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static Expression ComplexCallHelper(Expression instance, MethodInfo method, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");
            ContractUtils.Requires(instance != null ^ method.IsStatic, "instance");

            ParameterInfo[] parameters = method.GetParameters();
            bool hasParamArray = parameters.Length > 0 && parameters[parameters.Length - 1].IsParamArray();

            if (instance != null) {
                instance = Convert(instance, method.DeclaringType);
            }

            Expression[] clone = null;

            int current = 0;    // current parameter being populated
            int consumed = 0;   // arguments so far consumed

            // Validate the argument array, or populate the clone
            while (current < parameters.Length) {
                ParameterInfo parameter = parameters[current];
                Expression argument;

                // last parameter ... params array?
                if ((current == parameters.Length - 1) && hasParamArray) {
                    // do we have any arguments to pass in?
                    if (consumed < arguments.Length) {
                        // Exactly one argument? If it is array of the right type, it goes directly
                        if ((consumed == arguments.Length - 1) &&
                            CompatibleParameterTypes(parameter.ParameterType, arguments[consumed].Type)) {
                            argument = arguments[consumed++];
                        } else {
                            Type elementType = parameter.ParameterType.GetElementType();
                            Expression[] paramArray = new Expression[arguments.Length - consumed];
                            int paramIndex = 0;
                            while (consumed < arguments.Length) {
                                paramArray[paramIndex++] = Convert(arguments[consumed++], elementType);
                            }
                            argument = Expression.NewArrayInit(elementType, paramArray);
                        }
                    } else {
                        // No. Create an empty array.
                        argument = Expression.NewArrayInit(parameter.ParameterType.GetElementType());
                    }
                } else {
                    if (consumed < arguments.Length) {
                        // We have argument.
                        argument = arguments[consumed++];
                    } else {
                        // Missing argument, try default value.
                        ContractUtils.Requires(!parameter.IsMandatoryParameter(), "arguments", "Argument not provided for a mandatory parameter");
                        argument = CreateDefaultValueExpression(parameter);
                    }
                }

                // Add conversion if needed
                argument = ArgumentConvertHelper(argument, parameter.ParameterType);

                // Do we need to make array clone?
                if (clone == null && !(current < arguments.Length && (object)argument == (object)arguments[current])) {
                    clone = new Expression[parameters.Length];
                    for (int i = 0; i < current; i++) {
                        clone[i] = arguments[i];
                    }
                }

                if (clone != null) {
                    clone[current] = argument;
                }

                // Next parameter
                current++;
            }
            ContractUtils.Requires(consumed == arguments.Length, "arguments", "Incorrect number of arguments");
            return Expression.Call(instance, method, clone != null ? clone : arguments);
        }

        private static Expression CreateDefaultValueExpression(ParameterInfo parameter) {
            if (parameter.HasDefaultValue()) {
                return Expression.Constant(parameter.DefaultValue, parameter.ParameterType);
            } else {
                throw new NotSupportedException("missing parameter value not supported");
            }
        }

    }
}
