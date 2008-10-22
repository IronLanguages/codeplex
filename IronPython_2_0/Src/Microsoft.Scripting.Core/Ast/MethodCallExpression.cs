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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;
using System.Text;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public sealed class MethodCallExpression : Expression {
        private readonly MethodInfo _method;
        private readonly Expression _instance;
        private readonly ReadOnlyCollection<Expression> _arguments;

        internal MethodCallExpression(
            Annotations annotations,
            Type returnType,
            MethodInfo method,
            Expression instance,
            ReadOnlyCollection<Expression> arguments)
            : base(ExpressionType.Call, returnType, annotations) {

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

            if (Attribute.GetCustomAttribute(_method, typeof(ExtensionAttribute)) != null) {
                start = 1;
                ob = _arguments[0];
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

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitMethodCall(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {

        #region Call

        public static MethodCallExpression Call(MethodInfo method, Expression arg0) {
            return Call(null, method, Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0 }));
        }
        
        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1) {
            return Call(null, method, Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1 }));
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2) {
            return Call(null, method, Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2 }));
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            return Call(null, method, Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2, arg3 }));
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) {
            return Call(null, method, Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2, arg3, arg4 }));
        }

        //CONFORMING
        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments) {
            return Call(null, method, Annotations.Empty, arguments);
        }

        public static MethodCallExpression Call(MethodInfo method, Annotations annotations, IEnumerable<Expression> arguments) {
            return Call(null, method, annotations, arguments);
        }

        //CONFORMING
        public static MethodCallExpression Call(Expression instance, MethodInfo method) {
            return Call(instance, method, Annotations.Empty, EmptyReadOnlyCollection<Expression>.Instance);
        }

        //CONFORMING
        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments) {
            return Call(instance, method, Annotations.Empty, arguments);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1) {
            return Call(instance, method, Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1 }));
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1, Expression arg2) {
            return Call(instance, method, Annotations.Empty, new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2 }));
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
            ReadOnlyCollection<Expression> argList = arguments.ToReadOnly();
            ValidateCallArgs(instance, method, ref argList);
            return new MethodCallExpression(annotations, method.ReturnType, method, instance, argList);
        }

        //CONFORMING
        private static void ValidateCallArgs(Expression instance, MethodInfo method, ref ReadOnlyCollection<Expression> arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arguments, "arguments");

            ValidateMethodInfo(method);
            if (method.IsStatic) {
                ContractUtils.Requires(instance == null, "instance", Strings.OnlyStaticMethodsHaveNullExpr);
            } else {
                RequiresCanRead(instance, "instance");
                ValidateCallInstanceType(instance.Type, method);
            }
            ValidateArgumentTypes(method, ExpressionType.Call, ref arguments);
        }

        //CONFORMING
        private static void ValidateCallInstanceType(Type instanceType, MethodInfo method) {
            if (!TypeUtils.IsValidInstanceType(method, instanceType)) {
                throw Error.MethodNotDefinedForType(method, instanceType);
            }
        }

        //CONFORMING
        private static void ValidateArgumentTypes(MethodBase method, ExpressionType nodeKind, ref ReadOnlyCollection<Expression> arguments) {
            Debug.Assert(nodeKind == ExpressionType.Invoke || nodeKind == ExpressionType.Call || nodeKind == ExpressionType.Dynamic || nodeKind == ExpressionType.New);

            bool invoke = nodeKind == ExpressionType.Invoke;
            ParameterInfo[] pis = method.GetParameters();

            if (nodeKind == ExpressionType.Dynamic) {
                pis = pis.RemoveFirst(); // ignore CallSite argument
            }

            if (pis.Length != arguments.Count) {
                // TODO: this is for LinqV1 compat, can we just have one exception?
                switch (nodeKind) {
                    case ExpressionType.New:
                        throw Error.IncorrectNumberOfConstructorArguments();
                    case ExpressionType.Invoke:
                        throw Error.IncorrectNumberOfLambdaArguments();
                    case ExpressionType.Dynamic:
                    case ExpressionType.Call:
                        throw Error.IncorrectNumberOfMethodCallArguments(method);                    
                    default:
                        throw Assert.Unreachable;
                }
            }
            Expression[] newArgs = null;
            for (int i = 0, n = pis.Length; i < n; i++) {
                Expression arg = arguments[i];
                ParameterInfo pi = pis[i];
                RequiresCanRead(arg, "arguments");
                Type pType = pi.ParameterType;
                if (pType.IsByRef) {
                    pType = pType.GetElementType();
                }
                TypeUtils.ValidateType(pType);
                if (!TypeUtils.AreReferenceAssignable(pType, arg.Type)) {
                    if (TypeUtils.IsSameOrSubclass(typeof(Expression), pType) && TypeUtils.AreAssignable(pType, arg.GetType())) {
                        arg = Expression.Quote(arg);
                    } else {
                        // TODO: this is for LinqV1 compat, can we just have one exception?
                        switch (nodeKind) {
                            case ExpressionType.New:
                                throw Error.ExpressionTypeDoesNotMatchConstructorParameter(arg.Type, pType);
                            case ExpressionType.Invoke:
                                throw Error.ExpressionTypeDoesNotMatchParameter(arg.Type, pType);
                            case ExpressionType.Dynamic:
                            case ExpressionType.Call:
                                throw Error.ExpressionTypeDoesNotMatchMethodParameter(arg.Type, pType, method);
                            default:
                                throw Assert.Unreachable;
                        }
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
        }

        //CONFORMING
        private static MethodInfo FindMethod(Type type, string methodName, Type[] typeArgs, Expression[] args, BindingFlags flags) {
            MemberInfo[] members = type.FindMembers(MemberTypes.Method, flags, Type.FilterNameIgnoreCase, methodName);
            if (members == null || members.Length == 0)
                throw Error.MethodDoesNotExistOnType(methodName, type);

            MethodInfo method;

            var methodInfos = members.Map(t => (MethodInfo)t);
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
            ParameterInfo[] parms = m.GetParametersCached();
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
        /// The helper to create the AST method call node. Will add conversions (Expression.Convert())
        /// to parameters and instance if necessary.
        /// </summary>
        public static MethodCallExpression SimpleCallHelper(MethodInfo method, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.Requires(method.IsStatic, "method", Strings.MustBeStatic);
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

            ParameterInfo[] parameters = method.GetParametersCached();

            ContractUtils.Requires(arguments.Length == parameters.Length, "arguments", Strings.IncorrectArgNumber);

            if (instance != null) {
                instance = ConvertHelper(instance, method.DeclaringType);
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
            return new MethodCallExpression(Annotations.Empty, method.ReturnType, method, instance, finalArgs);
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
            ContractUtils.Requires(method.IsStatic, "method", Strings.MustBeStatic);
            return ComplexCallHelper(null, method, arguments);
        }

        // FxCop is just wrong on this one. "method" is required as MethodInfo by the call to "Call" factory
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static Expression ComplexCallHelper(Expression instance, MethodInfo method, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");
            ContractUtils.Requires(instance != null ^ method.IsStatic, "instance");

            ParameterInfo[] parameters = method.GetParametersCached();
            bool hasParamArray = parameters.Length > 0 && parameters[parameters.Length - 1].IsParamArray();

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
                        ContractUtils.Requires(!parameter.IsMandatoryParameter(), "arguments", Strings.ArgumentNotProvided);
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
            ContractUtils.Requires(consumed == arguments.Length, "arguments", Strings.IncorrectArgNumber);
            return Call(instance, method, clone != null ? clone : arguments);
        }

        private static Expression CreateDefaultValueExpression(ParameterInfo parameter) {
            if (parameter.HasDefaultValue()) {
                return Constant(parameter.DefaultValue, parameter.ParameterType);
            } else {
                // TODO: Handle via compiler constant.
                throw Error.MissingValueNotSupported();
            }
        }


        #region ArrayIndex

        //CONFORMING
        public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes) {
            return ArrayIndex(array, (IEnumerable<Expression>)indexes);
        }

        //CONFORMING
        // Note: it's okay to not include Annotations here. This node is
        // deprecated in favor of ArrayAccess
        public static MethodCallExpression ArrayIndex(Expression array, IEnumerable<Expression> indexes) {
            RequiresCanRead(array, "array");
            ContractUtils.RequiresNotNull(indexes, "indexes");

            Type arrayType = array.Type;
            if (!arrayType.IsArray)
                throw Error.ArgumentMustBeArray();

            ReadOnlyCollection<Expression> indexList = indexes.ToReadOnly();
            if (arrayType.GetArrayRank() != indexList.Count)
                throw Error.IncorrectNumberOfIndexes();

            foreach (Expression e in indexList) {
                RequiresCanRead(e, "indexes");
                if (e.Type != typeof(int)) {
                    throw Error.ArgumentMustBeArrayIndexType();
                }
            }

            MethodInfo mi = array.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
            return Call(array, mi, indexList);
        }

        #endregion

    }
}
