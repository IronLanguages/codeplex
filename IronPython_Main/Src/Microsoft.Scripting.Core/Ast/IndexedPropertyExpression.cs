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
using System.Scripting.Utils;
using System.Text;

namespace System.Linq.Expressions {
    public class IndexedPropertyExpression : Expression {
        private readonly Expression _instance;
        private readonly MethodInfo _getter;
        private readonly MethodInfo _setter;
        private readonly ReadOnlyCollection<Expression> _arguments;

        internal IndexedPropertyExpression(
                Annotations annotations,
                Expression instance,
                MethodInfo getter,
                MethodInfo setter,
                ReadOnlyCollection<Expression> arguments,
                Type type,
                CallSiteBinder binder
            )
            : base(ExpressionType.IndexedProperty, type, false, annotations, getter != null, setter != null, binder) {

            if (IsBound) {
                RequiresBound(instance, "instance");
                RequiresBoundItems(arguments, "arguments");
            }

            _instance = instance;
            _getter = getter;
            _setter = setter;
            _arguments = arguments;
        }

        public Expression Object {
            get { return _instance; }
        }

        public MethodInfo GetMethod {
            get { return _getter; }
        }

        public MethodInfo SetMethod {
            get { return _setter; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }

        // tries to infer property name when accessors use common naming convention
        private string Name {
            get {
                const int prefixSize = 4;

                string name = null;
                if (_getter != null) {
                    if (!_getter.Name.StartsWith("get_")) {
                        //getter has nonconforming name
                        return "<property>";
                    }
                    name = _getter.Name.Substring(prefixSize);
                }
                if (_setter != null) {
                    if (!_setter.Name.StartsWith("set_")) {
                        //getter has nonconforming name
                        return "<property>";
                    }
                    string setName = _setter.Name.Substring(prefixSize);
                    if (name != null && name != setName) {
                        // getter and setter do not agree on the name
                        return "<property>";
                    }
                    name = setName;
                }
                return name;
            }
        }

        internal override void BuildString(StringBuilder builder) {
            Debug.Assert(_arguments != null, "arguments should not be null");
            ContractUtils.RequiresNotNull(builder, "builder");

            int start = 0;
            Expression ob = _instance;

            if (ob != null) {
                ob.BuildString(builder);
                builder.Append(".");
            }
            builder.Append(Name);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        public static IndexedPropertyExpression Property(Expression instance, PropertyInfo property, params Expression[] arguments) {
            return Property(instance, property, arguments.ToReadOnly());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        public static IndexedPropertyExpression Property(Expression instance, PropertyInfo property, IEnumerable<Expression> arguments) {
            return Property(instance, property, arguments, Annotations.Empty);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        public static IndexedPropertyExpression Property(Expression instance, PropertyInfo property, IEnumerable<Expression> arguments, Annotations annotations) {
            ContractUtils.RequiresNotNull(property, "property");

            MethodInfo getter = property.GetGetMethod(true);
            MethodInfo setter = property.GetSetMethod(true);

            return Property(instance, getter, setter, arguments, annotations);
        }


        public static IndexedPropertyExpression Property(Expression instance, MethodInfo getter, MethodInfo setter, params Expression[] arguments) {
            return Property(instance, getter, setter, arguments.ToReadOnly());
        }

        public static IndexedPropertyExpression Property(Expression instance, MethodInfo getter, MethodInfo setter, IEnumerable<Expression> arguments) {
            return Property(instance, getter, setter, arguments, Annotations.Empty);
        }

        public static IndexedPropertyExpression Property(Expression instance, MethodInfo getter, MethodInfo setter, IEnumerable<Expression> arguments, Annotations annotations) {
            ContractUtils.Requires(!(getter == null && setter == null), "getter", Strings.NoGetterSetter);

            ReadOnlyCollection<Expression> argList = arguments.ToReadOnly();

            // type of property is the type returned by the getter or the type of the last parameter of the setter
            // if both getter and setter specified, all their parameter types should match 
            // with exception of the last seter parameter which should match the type returned by the get method.
            // accessor parameters cannot be ByRef.

            Type propertyType = null;
            ParameterInfo[] getParameters = null;

            if (getter != null) {
                propertyType = getter.ReturnType;
                getParameters = getter.GetParameters();
                ValidateAccessor(instance, getter, getParameters, ref argList);
            }

            if (setter != null) {
                ParameterInfo[] setParameters = setter.GetParameters();
                ContractUtils.Requires(setParameters.Length > 0, "setter", Strings.SetterHasNoParams);

                // valueType is the type of the value passed to the setter (last parameter)
                Type valueType = setParameters[setParameters.Length - 1].ParameterType;
                ContractUtils.Requires(!valueType.IsByRef, "setter", Strings.PropertyCannotHaveRefType);

                if (getter != null) {
                    ContractUtils.Requires(setter.ReturnType == typeof(void), "setter", Strings.SetterMustBeVoid);
                    ContractUtils.Requires(propertyType == valueType, "setter", Strings.PropertyTyepMustMatchSetter);
                    ContractUtils.Requires(!(getter.IsStatic ^ setter.IsStatic), "getter", Strings.BothAccessorsMustBeStatic);
                    ContractUtils.Requires(getParameters.Length == setParameters.Length - 1, "getter", Strings.IndexesOfSetGetMustMatch);

                    for (int i = 0; i < getParameters.Length; i++) {
                        ContractUtils.Requires(getParameters[i].ParameterType == setParameters[i].ParameterType, "getter", Strings.IndexesOfSetGetMustMatch);
                    }
                } else {
                    propertyType = valueType;
                    ValidateAccessor(instance, setter, ArrayUtils.RemoveLast(setParameters), ref argList);
                }
            }
            ContractUtils.Requires(propertyType != typeof(void), getter != null ? "getter" : "setter", Strings.PropertyTypeCannotBeVoid);
            return new IndexedPropertyExpression(annotations, instance, getter, setter, argList, propertyType, null);
        }

        private static void ValidateAccessor(Expression instance, MethodInfo method, ParameterInfo[] indexes, ref ReadOnlyCollection<Expression> arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");

            ValidateMethodInfo(method);
            ContractUtils.Requires((method.CallingConvention & CallingConventions.VarArgs) == 0, "method", Strings.AccessorsCannotHaveVarArgs);            
            if (method.IsStatic) {
                ContractUtils.Requires(instance == null, "instance", Strings.OnlyStaticMethodsHaveNullExpr); 
            } else {
                RequiresCanRead(instance, "instance");
                ValidateCallInstanceType(instance.Type, method);
            }

            ValidateAccessorArgumentTypes(method, indexes, ref arguments);
        }

        private static void ValidateAccessorArgumentTypes(MethodInfo method, ParameterInfo[] indexes, ref ReadOnlyCollection<Expression> arguments) {
            if (indexes.Length > 0) {
                if (indexes.Length != arguments.Count) {
                    throw Error.IncorrectNumberOfMethodCallArguments(method);
                }
                Expression[] newArgs = null;
                for (int i = 0, n = indexes.Length; i < n; i++) {
                    Expression arg = arguments[i];
                    ParameterInfo pi = indexes[i];
                    RequiresCanRead(arg, "arguments");

                    Type pType = pi.ParameterType;
                    ContractUtils.Requires(!pType.IsByRef, "indexes", Strings.AccessorsCannotHaveByRefArgs);
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
    }
}
