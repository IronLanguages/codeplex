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
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class MethodCallExpression : Expression {
        private readonly MethodInfo _method;
        private readonly Expression _instance;
        private readonly ReadOnlyCollection<Expression> _arguments;

        internal MethodCallExpression(Annotations annotations, Type returnType, InvokeMemberAction bindingInfo,
            MethodInfo method, Expression instance, ReadOnlyCollection<Expression> /*!*/ arguments)
            : base(annotations, AstNodeType.Call, returnType, bindingInfo) {
            if (IsBound) {
                RequiresBound(instance, "instance");
                RequiresBoundItems(arguments, "arguments");
            }

            _method = method;
            _instance = instance;
            _arguments = arguments;
        }

        public MethodInfo Method {
            get { return _method; }
        }

        public Expression Instance {
            get { return _instance; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments) {
            return Call(Annotations.Empty, null, method, arguments);
        }

        // TODO: should take Annotations instead of SourceSpan
        public static MethodCallExpression Call(SourceSpan span, MethodInfo method, params Expression[] arguments) {
            return Call(span, null, method, arguments);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments) {
            return Call(Annotations.Empty, instance, method, arguments);
        }

        // TODO: should take Annotations instead of SourceSpan
        public static MethodCallExpression Call(SourceSpan span, Expression instance, MethodInfo method, params Expression[] arguments) {
            return Call(Annotate(span), instance, method, arguments);
        }

        public static MethodCallExpression Call(Annotations annotations, Expression instance, MethodInfo method, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.Requires(!method.IsGenericMethodDefinition, "method");
            ContractUtils.Requires(!method.ContainsGenericParameters, "method");
            if (method.IsStatic) {
                ContractUtils.Requires(instance == null, "instance", "Instance must be null for static method");
            } else {
                ContractUtils.RequiresNotNull(instance, "instance");
                if (!TypeUtils.CanAssign(method.DeclaringType, instance.Type)) {
                    throw new ArgumentException(
                        String.Format(
                             "Invalid instance type for {0}.{1}. Expected {0}, got {2}.",
                             method.DeclaringType.Name,
                             method.Name,
                             instance.Type.Name
                        ),
                        "instance"
                    );
                }
            }

            ContractUtils.RequiresNotNullItems(arguments, "arguments");
            ParameterInfo[] parameters = method.GetParameters();

            ValidateCallArguments(parameters, arguments);

            return new MethodCallExpression(annotations, method.ReturnType, null, method, instance, CollectionUtils.ToReadOnlyCollection(arguments));
        }

        /// <summary>
        /// A dynamic or unbound method call
        /// </summary>
        /// <param name="returnType">the type that the method returns, or null for an unbound node</param>
        /// <param name="instance">the instance to call; must be non-null</param>
        /// <param name="bindingInfo">call binding information (method name, named arguments, etc)</param>
        /// <param name="arguments">the arguments to the call</param>
        /// <returns></returns>
        public static MethodCallExpression Call(Type returnType, Expression instance, InvokeMemberAction bindingInfo, params Expression[] arguments) {
            return Call(Annotations.Empty, returnType, instance, bindingInfo, arguments);
        }

        /// <summary>
        /// A dynamic or unbound method call
        /// </summary>
        /// <param name="annotations">annotations for the node</param>
        /// <param name="returnType">the type that the method returns, or null for an unbound node</param>
        /// <param name="instance">the instance to call; must be non-null</param>
        /// <param name="bindingInfo">call binding information (method name, named arguments, etc)</param>
        /// <param name="arguments">the arguments to the call</param>
        /// <returns></returns>
        public static MethodCallExpression Call(Annotations annotations, Type returnType, Expression instance, InvokeMemberAction bindingInfo, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(instance, "instance");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");

            // Validate ArgumentInfos. For now, includes the instance.
            // This needs to be reconciled with InvocationExpression
            if (bindingInfo.Signature.ArgumentCount != arguments.Length + 1) {
                throw new ArgumentException(
                    string.Format(
                        "Argument count (including instance) '{0}' must match arguments in the binding information '{1}'",
                        arguments.Length + 1,
                        bindingInfo.Signature.ArgumentCount
                    ),
                    "bindingInfo"
                );
            }

            return new MethodCallExpression(annotations, returnType, bindingInfo, null, instance, CollectionUtils.ToReadOnlyCollection(arguments));
        }

        private static void ValidateCallArguments(IList<ParameterInfo> parameters, IList<Expression> arguments) {
            ContractUtils.Requires(parameters.Count == arguments.Count, "arguments", "Argument count must match parameter count");

            int count = parameters.Count;
            for (int index = 0; index < count; index++) {
                Type pt = parameters[index].ParameterType;
                ContractUtils.Requires(!TypeUtils.IsGeneric(pt), "arguments");
                if (pt.IsByRef) {
                    pt = pt.GetElementType();
                }
                if (!TypeUtils.CanAssign(pt, arguments[index].Type)) {
                    throw new ArgumentException(
                        String.Format(
                            "Invalid type for argument {0}. Expected {1}, got {2}.",
                            index, pt.Name, arguments[index].Type.Name
                        ),
                        "arguments"
                    );
                }
            }
        }

        internal static MethodCallExpression Call(Expression instance, MethodInfo method, IList<Expression> arguments) {
            if (arguments == null) {
                return Call(instance, method);
            } else {
                Expression[] args = new Expression[arguments.Count];
                arguments.CopyTo(args, 0);
                return Call(instance, method, args);
            }
        }

        /// <summary>
        /// The helper to create the AST method call node. Will add conversions (Expression.Convert())
        /// to parameters and instance if necessary.
        /// </summary>
        public static MethodCallExpression SimpleCallHelper(MethodInfo method, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.Requires(method.IsStatic, "method", "Method must be static");
            return SimpleCallHelper(null, method, arguments);
        }

        /// <summary>
        /// The helper to create the AST method call node. Will add conversions (Expression.Convert())
        /// to parameters and instance if necessary.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static MethodCallExpression SimpleCallHelper(Expression instance, MethodInfo method, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.Requires(instance != null ^ method.IsStatic, "instance");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");

            ParameterInfo[] parameters = method.GetParameters();

            ContractUtils.Requires(arguments.Length == parameters.Length, "arguments", "Incorrect number of arguments");

            if (instance != null) {
                instance = ConvertHelper(instance, method.DeclaringType);
            }

            arguments = ArgumentConvertHelper(arguments, parameters);

            return Call(instance, method, arguments);
        }

        private static Expression[]/*!*/ ArgumentConvertHelper(Expression[] /*!*/ arguments, ParameterInfo[] /*!*/ parameters) {
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

        private static Expression/*!*/ ArgumentConvertHelper(Expression/*!*/ argument, Type/*!*/ type) {
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
            if (parameter == argument) {
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

        // FxCop is just wrong on this one. "method" is required as MethodInfo by the call to "Call" factory
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static Expression ComplexCallHelper(Expression instance, MethodInfo method, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");
            ContractUtils.Requires(instance != null ^ method.IsStatic, "instance");

            ParameterInfo[] parameters = method.GetParameters();
            bool hasParamArray = parameters.Length > 0 && CompilerHelpers.IsParamArray(parameters[parameters.Length - 1]);

            if (instance != null) {
                instance = ConvertHelper(instance, method.DeclaringType);
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
                                paramArray[paramIndex++] = ConvertHelper(arguments[consumed++], elementType);
                            }
                            argument = NewArray(parameter.ParameterType, paramArray);
                        }
                    } else {
                        // No. Create an empty array.
                        argument = NewArray(parameter.ParameterType);
                    }
                } else {
                    if (consumed < arguments.Length) {
                        // We have argument.
                        argument = arguments[consumed++];
                    } else {
                        // Missing argument, try default value.
                        ContractUtils.Requires(!CompilerHelpers.IsMandatoryParameter(parameter), "arguments", "Argument not provided for a mandatory parameter");
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
            return Call(instance, method, clone != null ? clone : arguments);
        }

        private static Expression CreateDefaultValueExpression(ParameterInfo parameter) {
            if (CompilerHelpers.HasDefaultValue(parameter)) {
                return Constant(parameter.DefaultValue, parameter.ParameterType);
            } else {
                // TODO: Handle via compiler constant.
                throw new NotSupportedException("missing parameter value not yet supported");
            }
        }

        public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes) {
            return ArrayIndex(array, (IList<Expression>)indexes);
        }

        public static MethodCallExpression ArrayIndex(Expression array, IList<Expression> indexes) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(indexes, "indexes");

            Type arrayType = array.Type;
            ContractUtils.Requires(arrayType.IsArray, "array", "Array argument must be array.");

            ReadOnlyCollection<Expression> indexList = CollectionUtils.ToReadOnlyCollection(indexes);
            ContractUtils.Requires(array.Type.GetArrayRank() == indexList.Count, "indexes", "Incorrect number of indexes.");

            foreach (Expression e in indexList) {
                ContractUtils.RequiresNotNull(e, "indexes");
                ContractUtils.Requires(e.Type == typeof(int), "indexes", "Array indexes must be ints.");
            }

            MethodInfo mi = array.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
            return Call(array, mi, indexList);
        }

    }
}
