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
using System.Diagnostics;
using System.Reflection;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Utils;
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class MethodCallExpression : Expression {
        private readonly MethodInfo _method;
        private readonly Expression _instance;
        private readonly ReadOnlyCollection<Expression> _arguments;

        internal MethodCallExpression(
            Annotations annotations,
            Type returnType,
            CallSiteBinder bindingInfo,
            MethodInfo method,
            Expression instance,
            ReadOnlyCollection<Expression> arguments)
            : base(annotations, ExpressionType.Call, returnType, bindingInfo) {

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

        public Expression Object {
            get { return _instance; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }

        internal override void BuildString(StringBuilder builder) {
            Debug.Assert(_arguments != null, "arguments should not be null");
            ContractUtils.RequiresNotNull(builder, "builder");

            int start = 0;
            Expression ob = _instance;

            // TODO: we go through a dynamic helper untill we can guarantee
            // that ExtensionAttribute is always available.
            if (TypeUtils.ExtensionAttributeType != null) {
                if (Attribute.GetCustomAttribute(_method, TypeUtils.ExtensionAttributeType) != null) {
                    start = 1;
                    ob = _arguments[0];
                }
            }

            if (ob != null) {
                ob.BuildString(builder);
                builder.Append(".");
            }
            builder.Append(_method.Name);
            builder.Append("(");
            for (int i = start, n = _arguments.Count; i < n; i++) {
                if (i > start)
                    builder.Append(", ");
                _arguments[i].BuildString(builder);
            }
            builder.Append(")");
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {

        #region Call

        //CONFORMING
        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments) {
            return Call(null, method, Annotations.Empty, arguments);
        }

        public static MethodCallExpression Call(MethodInfo method, Annotations annotations, IEnumerable<Expression> arguments) {
            return Call(null, method, annotations, arguments);
        }

        //CONFORMING
        public static MethodCallExpression Call(Expression instance, MethodInfo method) {
            return Call(instance, method, Annotations.Empty, new Expression[0]);
        }

        //CONFORMING
        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments) {
            return Call(instance, method, Annotations.Empty, arguments);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments) {
            return Call(instance, method, Annotations.Empty, arguments);
        }

        //CONFORMING
        public static MethodCallExpression Call(Expression instance, string methodName, Type[] typeArguments, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(instance, "instance");
            ContractUtils.RequiresNotNull(methodName, "methodName");
            if (arguments == null) arguments = new Expression[] { };

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            return Expression.Call(instance, FindMethod(instance.Type, methodName, typeArguments, arguments, flags), arguments);
        }

        //CONFORMING
        public static MethodCallExpression Call(Type type, string methodName, Type[] typeArguments, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(methodName, "methodName");

            if (arguments == null) arguments = new Expression[] { };
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            return Expression.Call(null, FindMethod(type, methodName, typeArguments, arguments, flags), arguments);
        }

        //CONFORMING
        public static MethodCallExpression Call(Expression instance, MethodInfo method, Annotations annotations, IEnumerable<Expression> arguments) {
            ReadOnlyCollection<Expression> argList = CollectionUtils.ToReadOnlyCollection(arguments);
            ValidateCallArgs(instance, method, ref argList);
            return new MethodCallExpression(annotations, method.ReturnType, null, method, instance, argList);
        }

        //CONFORMING
        private static void ValidateCallArgs(Expression instance, MethodInfo method, ref ReadOnlyCollection<Expression> arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arguments, "arguments");

            ValidateMethodInfo(method);
            if (!method.IsStatic) {
                ContractUtils.RequiresNotNull(instance, "instance");
                ValidateCallInstanceType(instance.Type, method);
            }
            ValidateArgumentTypes(method, ref arguments);
        }

        //CONFORMING
        private static void ValidateCallInstanceType(Type instanceType, MethodInfo method) {
            if (!TypeUtils.AreReferenceAssignable(method.DeclaringType, instanceType)) {
                if (instanceType.IsValueType) {
                    if (TypeUtils.AreReferenceAssignable(method.DeclaringType, typeof(System.Object))) {
                        return;
                    }
                    if (TypeUtils.AreReferenceAssignable(method.DeclaringType, typeof(System.ValueType))) {
                        return;
                    }
                    if (instanceType.IsEnum && TypeUtils.AreReferenceAssignable(method.DeclaringType, typeof(System.Enum))) {
                        return;
                    }
                    // A call to an interface implemented by a struct is legal whether the struct has
                    // been boxed or not.
                    if (method.DeclaringType.IsInterface) {
                        foreach (Type interfaceType in instanceType.GetInterfaces())
                            if (TypeUtils.AreReferenceAssignable(method.DeclaringType, interfaceType))
                                return;
                    }
                }
                throw Error.MethodNotDefinedForType(method, instanceType);
            }
        }

        //CONFORMING
        private static void ValidateArgumentTypes(MethodInfo method, ref ReadOnlyCollection<Expression> arguments) {
            ParameterInfo[] pis = method.GetParameters();
            if (pis.Length > 0) {
                if (pis.Length != arguments.Count) {
                    throw Error.IncorrectNumberOfMethodCallArguments(method);
                }
                Expression[] newArgs = null;
                for (int i = 0, n = pis.Length; i < n; i++) {
                    Expression arg = arguments[i];
                    ParameterInfo pi = pis[i];
                    ContractUtils.RequiresNotNull(arg, "arguments");
                    Type pType = pi.ParameterType;
                    if (pType.IsByRef) {
                        pType = pType.GetElementType();
                    }
                    TypeUtils.ValidateType(pType);
                    if (!TypeUtils.AreReferenceAssignable(pType, arg.Type)) {
                        if (TypeUtils.IsSameOrSubclass(typeof(Expression), pType) && TypeUtils.AreAssignable(pType, arg.GetType())) {
                            arg = Expression.Quote(arg);
                        } else {
                            throw Error.ExpressionTypeDoesNotMatchMethodParameter(arg.Type, pType, method);
                        }
                    }
                    if (newArgs == null && arg != arguments[i]) {
                        newArgs = new Expression[arguments.Count];
                        for (int j = 0; j < i; j++) {
                            newArgs[j] = arguments[j];
                        }
                    }
                    if (newArgs != null) {
                        newArgs[i] = arg;
                    }
                }
                if (newArgs != null) {
                    arguments = new ReadOnlyCollection<Expression>(newArgs);
                }

            } else if (arguments.Count > 0) {
                throw Error.IncorrectNumberOfMethodCallArguments(method);
            }
        }

        //CONFORMING
        private static MethodInfo FindMethod(Type type, string methodName, Type[] typeArgs, Expression[] args, BindingFlags flags) {
            MemberInfo[] members = type.FindMembers(MemberTypes.Method, flags, Type.FilterNameIgnoreCase, methodName);
            if (members == null || members.Length == 0)
                throw Error.MethodDoesNotExistOnType(methodName, type);

            MethodInfo method;

            MethodInfo[] methodInfos = ArrayUtils.ConvertAll<MemberInfo, MethodInfo>(members, delegate(MemberInfo t) { return (MethodInfo)t; });
            int count = FindBestMethod(methodInfos, typeArgs, args, out method);

            if (count == 0)
                throw Error.MethodWithArgsDoesNotExistOnType(methodName, type);
            if (count > 1)
                throw Error.MethodWithMoreThanOneMatch(methodName, type);
            return method;
        }

        //CONFORMING
        private static int FindBestMethod(IEnumerable<MethodInfo> methods, Type[] typeArgs, Expression[] args, out MethodInfo method) {
            int count = 0;
            method = null;
            foreach (MethodInfo mi in methods) {
                MethodInfo moo = ApplyTypeArgs(mi, typeArgs);
                if (moo != null && IsCompatible(moo, args)) {
                    // favor public over non-public methods
                    if (method == null || (!method.IsPublic && moo.IsPublic)) {
                        method = moo;
                        count = 1;
                    }
                        // only count it as additional method if they both public or both non-public
                    else if (method.IsPublic == moo.IsPublic) {
                        count++;
                    }
                }
            }
            return count;
        }

        //CONFORMING
        private static bool IsCompatible(MethodBase m, Expression[] args) {
            ParameterInfo[] parms = m.GetParameters();
            if (parms.Length != args.Length)
                return false;
            for (int i = 0; i < args.Length; i++) {
                Expression arg = args[i];
                ContractUtils.RequiresNotNull(arg, "argument");
                Type argType = arg.Type;
                Type pType = parms[i].ParameterType;
                if (pType.IsByRef) {
                    pType = pType.GetElementType();
                }
                if (!TypeUtils.AreReferenceAssignable(pType, argType) &&
                    !(TypeUtils.IsSameOrSubclass(typeof(Expression), pType) && TypeUtils.AreAssignable(pType, arg.GetType()))) {
                    return false;
                }
            }
            return true;
        }

        //CONFORMING
        private static MethodInfo ApplyTypeArgs(MethodInfo m, Type[] typeArgs) {
            if (typeArgs == null || typeArgs.Length == 0) {
                if (!m.IsGenericMethodDefinition)
                    return m;
            } else {
                if (m.IsGenericMethodDefinition && m.GetGenericArguments().Length == typeArgs.Length)
                    return m.MakeGenericMethod(typeArgs);
            }
            return null;
        }


        #endregion

        /// <summary>
        /// A dynamic or unbound method call
        /// </summary>
        /// <param name="returnType">the type that the method returns, or null for an unbound node</param>
        /// <param name="instance">the instance to call; must be non-null</param>
        /// <param name="bindingInfo">call binding information (method name, named arguments, etc)</param>
        /// <param name="arguments">the arguments to the call</param>
        /// <returns></returns>
        public static MethodCallExpression Call(Type returnType, Expression instance, OldInvokeMemberAction bindingInfo, params Expression[] arguments) {
            return Call(returnType, instance, bindingInfo, Annotations.Empty, arguments);
        }

        /// <summary>
        /// A dynamic or unbound method call
        /// </summary>
        /// <param name="returnType">the type that the method returns, or null for an unbound node</param>
        /// <param name="instance">the instance to call; must be non-null</param>
        /// <param name="bindingInfo">call binding information (method name, named arguments, etc)</param>
        /// <param name="annotations">annotations for the node</param>
        /// <param name="arguments">the arguments to the call</param>
        /// <returns></returns>
        public static MethodCallExpression Call(Type returnType, Expression instance, OldInvokeMemberAction bindingInfo, Annotations annotations, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(instance, "instance");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");

            ReadOnlyCollection<Expression> argumentList = CollectionUtils.ToReadOnlyCollection(arguments);
            ContractUtils.RequiresNotNullItems(argumentList, "arguments");

            // Validate ArgumentInfos. For now, includes the instance.
            // This needs to be reconciled with InvocationExpression
            if (bindingInfo.Signature.ArgumentCount != argumentList.Count + 1) {
                throw new ArgumentException(
                    string.Format(
                        "Argument count (including instance) '{0}' must match arguments in the binding information '{1}'",
                        argumentList.Count + 1,
                        bindingInfo.Signature.ArgumentCount
                    ),
                    "bindingInfo"
                );
            }

            return new MethodCallExpression(annotations, returnType, bindingInfo, null, instance, argumentList);
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
                            argument = NewArrayInit(elementType, paramArray);
                        }
                    } else {
                        // No. Create an empty array.
                        argument = NewArrayInit(parameter.ParameterType.GetElementType());
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


        #region ArrayIndex


        //CONFORMING
        public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes) {
            return ArrayIndex(array, (IEnumerable<Expression>)indexes);
        }

        //CONFORMING
        public static MethodCallExpression ArrayIndex(Expression array, IEnumerable<Expression> indexes) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(indexes, "indexes");

            Type arrayType = array.Type;
            if (!arrayType.IsArray)
                throw Error.ArgumentMustBeArray();

            ReadOnlyCollection<Expression> indexList = CollectionUtils.ToReadOnlyCollection(indexes);
            if (arrayType.GetArrayRank() != indexList.Count)
                throw Error.IncorrectNumberOfIndexes();

            foreach (Expression e in indexList) {
                ContractUtils.RequiresNotNull(e, "indexes");
                if (e.Type != typeof(int))
                    throw Error.ArgumentMustBeArrayIndexType();
            }

            MethodInfo mi = array.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
            return Call(array, mi, indexList);
        }

        #endregion

    }
}
