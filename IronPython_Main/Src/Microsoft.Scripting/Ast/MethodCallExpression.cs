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
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public sealed class MethodCallExpression : Expression {
        private readonly MethodInfo _method;
        private readonly Expression _instance;
        private readonly ReadOnlyCollection<Expression> _arguments;
        private readonly ParameterInfo[] _parameterInfos;

        internal MethodCallExpression(MethodInfo /*!*/ method, Expression instance, ReadOnlyCollection<Expression> /*!*/ arguments, ParameterInfo[] /*!*/ parameters)
            : base(AstNodeType.Call, method.ReturnType) {
            _method = method;
            _instance = instance;
            _arguments = arguments;
            _parameterInfos = parameters;
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

        // TODO: Remove !!!
        internal ParameterInfo[] ParameterInfos {
            get { return _parameterInfos; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments) {
            return Call(null, method, arguments);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments) {
            Contract.RequiresNotNull(method, "method");
            Contract.Requires(!method.IsGenericMethodDefinition, "method");
            Contract.Requires(!method.ContainsGenericParameters, "method");
            if (method.IsStatic) {
                Contract.Requires(instance == null, "instance", "Instance must be null for static method");
            } else {
                Contract.RequiresNotNull(instance, "instance");
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

            Contract.RequiresNotNullItems(arguments, "arguments");
            ParameterInfo[] parameters = method.GetParameters();

            ValidateCallArguments(parameters, arguments);

            return new MethodCallExpression(method, instance, CollectionUtils.ToReadOnlyCollection(arguments), parameters);
        }

        private static void ValidateCallArguments(IList<ParameterInfo> parameters, IList<Expression> arguments) {
            Contract.Requires(parameters.Count == arguments.Count, "arguments", "Argument count must match parameter count");

            int count = parameters.Count;
            for (int index = 0; index < count; index++) {
                Type pt = parameters[index].ParameterType;
                Contract.Requires(!TypeUtils.IsGeneric(pt), "arguments");
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
        /// The helper to create the AST method call node. Will add conversions (Ast.Convert())
        /// to parameters and instance if necessary.
        /// </summary>
        public static MethodCallExpression SimpleCallHelper(MethodInfo method, params Expression[] arguments) {
            Contract.RequiresNotNull(method, "method");
            Contract.Requires(method.IsStatic, "method", "Method must be static");
            return SimpleCallHelper(null, method, arguments);
        }

        /// <summary>
        /// The helper to create the AST method call node. Will add conversions (Ast.Convert())
        /// to parameters and instance if necessary.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static MethodCallExpression SimpleCallHelper(Expression instance, MethodInfo method, params Expression[] arguments) {
            Contract.RequiresNotNull(method, "method");
            Contract.Requires(instance != null ^ method.IsStatic, "instance");
            Contract.RequiresNotNullItems(arguments, "arguments");

            ParameterInfo[] parameters = method.GetParameters();

            Contract.Requires(arguments.Length == parameters.Length, "arguments", "Incorrect number of arguments");

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

        public static Expression/*!*/ ComplexCallHelper(Delegate/*!*/ method, Expression[]/*!*/ arguments) {
            return ComplexCallHelper(method, arguments, CodeContext());
        }

        // TODO: codeContextExpression shouldn't be necessary as soon as we get Scope statement rigt
        public static Expression/*!*/ ComplexCallHelper(Delegate/*!*/ method, Expression[]/*!*/ arguments,
            Expression/*!*/ codeContextExpression) {

            Contract.RequiresNotNull(method, "method");
            Contract.RequiresNotNullItems(arguments, "arguments");
            Contract.RequiresNotNull(codeContextExpression, "codeContextExpression");
            
            // We prefer to peek inside the delegate and call the target method directly. However, we need to
            // exclude DynamicMethods since Delegate.Method returns a dummy MethodInfo, and we cannot emit a call to it.
            bool inlineDelegateCall = true;
            if (method.Method.DeclaringType == null) {
                Debug.Assert(method.Method.GetType().FullName == "System.Reflection.Emit.DynamicMethod+RTDynamicMethod");
                inlineDelegateCall = false;
            }

            Expression instanceForCall = (method.Target == null) ? null : RuntimeConstant(method.Target);

            ParameterInfo[] parameters = method.Method.GetParameters();
            if (parameters.Length > 0) {
                List<Expression> realArgs = new List<Expression>(arguments.Length);

                int paramIndex = 0;
                if (method.Target != null && method.Method.IsStatic) {
                    // This is a closed-instance delegate. ie. The target method is a static method with
                    // a first argument of, say, type T. And the delegate closes over an instance of T.
                    // In such a case, we have to pass the delegate target as an actual argument
                    Debug.Assert(parameters[paramIndex].ParameterType.IsAssignableFrom(method.Target.GetType()));

                    if (inlineDelegateCall) {
                        realArgs.Add(instanceForCall);
                        instanceForCall = null;
                    } 
                    
                    paramIndex++;
                }

                // Does the target method expect a CodeContext?
                if (parameters[paramIndex].ParameterType == typeof(CodeContext)) {
                    realArgs.Add(codeContextExpression);
                    paramIndex++;
                }

                realArgs.AddRange(arguments);
                arguments = realArgs.ToArray();
            }

            bool hasParamArray = parameters.Length > 0 && CompilerHelpers.IsParamArray(parameters[parameters.Length - 1]) 
                || method is CallTargetN || method is CallTargetWithContextN;

            if (inlineDelegateCall) {
                // Call the target method directly
                return ComplexCallHelperInternal(instanceForCall, method.Method, parameters, hasParamArray, arguments);
            } else {
                // Invoke the delegate object
                MethodInfo invokeMethod = method.GetType().GetMethod("Invoke");
                return ComplexCallHelperInternal(RuntimeConstant(method), invokeMethod, invokeMethod.GetParameters(), hasParamArray, arguments);
            }
        }

        /// <summary>
        /// The complex call helper to create the AST method call node.
        /// Will add conversions (Ast.Convert()), deals with default parameter values and params arrays.
        /// </summary>
        public static Expression ComplexCallHelper(MethodInfo method, params Expression[] arguments) {
            Contract.RequiresNotNull(method, "method");
            Contract.Requires(method.IsStatic, "method", "Method must be static");
            return ComplexCallHelper(null, method, arguments);
        }

        public static Expression ComplexCallHelper(Expression instance, MethodInfo method, params Expression[] arguments) {
            Contract.RequiresNotNull(method, "method");
            Contract.RequiresNotNullItems(arguments, "arguments");
            Contract.Requires(instance != null ^ method.IsStatic, "instance");
            
            ParameterInfo[] parameters = method.GetParameters();
            bool hasParamArray = parameters.Length > 0 && CompilerHelpers.IsParamArray(parameters[parameters.Length - 1]);
            return ComplexCallHelperInternal(instance, method, parameters, hasParamArray, arguments);
        }
        
        /// <summary>
        /// The complex call helper to create the AST method call node.
        /// Will add conversions (Ast.Convert()), deals with default parameter values and params arrays.
        /// </summary>
        private static Expression ComplexCallHelperInternal(Expression instance, MethodInfo method, ParameterInfo[] parameters, bool hasParamArray,
            params Expression[] arguments) {
            Assert.NotNullItems(parameters);

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
                        Contract.Requires(!CompilerHelpers.IsMandatoryParameter(parameter), "arguments", "Argument not provided for a mandatory parameter");
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
            Contract.Requires(consumed == arguments.Length, "arguments", "Incorrect number of arguments");
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
    }
}
