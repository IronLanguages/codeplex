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
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using System.Text;

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// Represents property or array indexing
    /// </summary>
    public sealed class IndexExpression : Expression {
        private readonly Expression _instance;
        private readonly PropertyInfo _indexer;
        private readonly ReadOnlyCollection<Expression> _arguments;

        internal IndexExpression(
            Expression instance,
            PropertyInfo indexer,
            Annotations annotations,
            ReadOnlyCollection<Expression> arguments,
            Type type,
            bool canRead,
            bool canWrite)
            : base(ExpressionType.Index, type, false, annotations, canRead, canWrite) {

            if (indexer == null) {
                Debug.Assert(instance != null && instance.Type.IsArray);
                Debug.Assert(instance.Type.GetArrayRank() == arguments.Count);
                Debug.Assert(instance.Type.GetElementType() == type);
            }

            _instance = instance;
            _indexer = indexer;
            _arguments = arguments;
        }

        public Expression Object {
            get { return _instance; }
        }

        /// <summary>
        /// If this is an indexed property, returns the property
        /// If this is an array indexing operation, returns null
        /// </summary>
        public PropertyInfo Indexer {
            get { return _indexer; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }

        internal override void BuildString(StringBuilder builder) {            
            Debug.Assert(builder != null);

            if (_instance != null) {
                _instance.BuildString(builder);
            } else {
                Debug.Assert(_indexer != null);
                builder.Append(_indexer.DeclaringType.Name);
            }

            if (_indexer != null) {
                builder.Append(".");
                builder.Append(_indexer.Name);
            }
            builder.Append("[");
            for (int i = 0, n = _arguments.Count; i < n; i++) {
                if (i > 0) {
                    builder.Append(", ");
                }
                _arguments[i].BuildString(builder);
            }
            builder.Append("]");
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitIndex(this);
        }
    }


    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {

        public static IndexExpression MakeIndex(Expression instance, PropertyInfo indexer, Annotations annotations, IEnumerable<Expression> arguments) {
            if (indexer != null) {
                return Property(instance, indexer, annotations, arguments);
            } else {
                return ArrayAccess(instance, annotations, arguments);
            }
        }

        #region ArrayAccess

        public static IndexExpression ArrayAccess(Expression array, params Expression[] indexes) {
            return ArrayAccess(array, null, (IEnumerable<Expression>)indexes);
        }

        public static IndexExpression ArrayAccess(Expression array, IEnumerable<Expression> indexes) {
            return ArrayAccess(array, null, indexes);
        }

        public static IndexExpression ArrayAccess(Expression array, Annotations annotations, params Expression[] indexes) {
            return ArrayAccess(array, annotations, (IEnumerable<Expression>)indexes);
        }

        public static IndexExpression ArrayAccess(Expression array, Annotations annotations, IEnumerable<Expression> indexes) {
            RequiresCanRead(array, "array");

            Type arrayType = array.Type;
            if (!arrayType.IsArray) {
                throw Error.ArgumentMustBeArray();
            }

            var indexList = indexes.ToReadOnly();
            if (arrayType.GetArrayRank() != indexList.Count) {
                throw Error.IncorrectNumberOfIndexes();
            }

            foreach (Expression e in indexList) {
                RequiresCanRead(e, "indexes");
                if (e.Type != typeof(int)) {
                    throw Error.ArgumentMustBeArrayIndexType();
                }
            }

            return new IndexExpression(array, null, annotations, indexList, arrayType.GetElementType(), true, true);
        }

        #endregion

        #region Property

        public static IndexExpression Property(Expression instance, PropertyInfo indexer, params Expression[] arguments) {
            return Property(instance, indexer, null, (IEnumerable<Expression>)arguments);
        }
        
        public static IndexExpression Property(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) {
            return Property(instance, indexer, null, arguments);
        }

        public static IndexExpression Property(Expression instance, PropertyInfo indexer, Annotations annotations, params Expression[] arguments) {
            return Property(instance, indexer, annotations, (IEnumerable<Expression>)arguments);
        }

        public static IndexExpression Property(Expression instance, PropertyInfo indexer, Annotations annotations, IEnumerable<Expression> arguments) {
            var argList = arguments.ToReadOnly();
            ValidateIndexedProperty(instance, indexer, ref argList);
            return new IndexExpression(instance, indexer, annotations, argList, indexer.PropertyType, indexer.CanRead, indexer.CanWrite);
        }

        // CTS places no restrictions on properties (see ECMA-335 8.11.3),
        // so we validate that the property conforms to CLS rules here.
        //
        // TODO: Do we still need all of this now that we take PropertyInfo?
        // Does reflection help us out at all? Expression.Property skips all of
        // these checks, so either it needs more checks or we need less here.
        private static void ValidateIndexedProperty(Expression instance, PropertyInfo property, ref ReadOnlyCollection<Expression> argList) {

            // If both getter and setter specified, all their parameter types
            // should match, with exception of the last setter parameter which
            // should match the type returned by the get method.
            // Accessor parameters cannot be ByRef.

            ContractUtils.RequiresNotNull(property, "property");
            ContractUtils.Requires(!property.PropertyType.IsByRef, "property", Strings.PropertyCannotHaveRefType);
            ContractUtils.Requires(property.PropertyType != typeof(void), "property", Strings.PropertyTypeCannotBeVoid);

            ParameterInfo[] getParameters = null;
            MethodInfo getter = property.GetGetMethod(true);
            if (getter != null) {
                getParameters = getter.GetParameters();
                ValidateAccessor(instance, getter, getParameters, ref argList);
            }

            MethodInfo setter = property.GetSetMethod(true);
            if (setter != null) {
                ParameterInfo[] setParameters = setter.GetParameters();
                ContractUtils.Requires(setParameters.Length > 0, "property", Strings.SetterHasNoParams);

                // valueType is the type of the value passed to the setter (last parameter)
                Type valueType = setParameters[setParameters.Length - 1].ParameterType;
                ContractUtils.Requires(!valueType.IsByRef, "property", Strings.PropertyCannotHaveRefType);
                ContractUtils.Requires(setter.ReturnType == typeof(void), "property", Strings.SetterMustBeVoid);
                ContractUtils.Requires(property.PropertyType == valueType, "property", Strings.PropertyTyepMustMatchSetter);

                if (getter != null) {
                    ContractUtils.Requires(!(getter.IsStatic ^ setter.IsStatic), "property", Strings.BothAccessorsMustBeStatic);
                    ContractUtils.Requires(getParameters.Length == setParameters.Length - 1, "property", Strings.IndexesOfSetGetMustMatch);

                    for (int i = 0; i < getParameters.Length; i++) {
                        ContractUtils.Requires(getParameters[i].ParameterType == setParameters[i].ParameterType, "property", Strings.IndexesOfSetGetMustMatch);
                    }
                } else {
                    ValidateAccessor(instance, setter, setParameters.RemoveLast(), ref argList);
                }
            }

            if (getter == null && setter == null) {
                throw Error.PropertyDoesNotHaveAccessor(property);
            }
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

        #endregion
    }
}
