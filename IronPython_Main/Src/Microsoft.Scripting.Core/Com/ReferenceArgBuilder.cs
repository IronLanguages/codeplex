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

#if !SILVERLIGHT

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Scripting.Utils;

namespace System.Scripting.Com {

    /// <summary>
    /// An argument that the user wants to explicitly pass by-reference (with copy-in copy-out semantics).
    /// The user passes a StrongBox[T] object whose value will get updated when the call returns.
    /// </summary>
    internal class ReferenceArgBuilder : SimpleArgBuilder {
        private Type _elementType;
        private VariableExpression _tmp;

        internal ReferenceArgBuilder(int index, Type parameterType)
            : base(index, parameterType) {
            Debug.Assert(parameterType.GetGenericTypeDefinition() == typeof(StrongBox<>));
            _elementType = parameterType.GetGenericArguments()[0];
        }

        internal override Expression ToExpression(IList<Expression> parameters) {
            if (_tmp == null) {
                _tmp = Expression.Variable(_elementType, "outParam");
            }

            // Ideally we'd pass in Expression.ReadField(parameters[Index], "Value") but due to
            // a bug in partial trust we can't access the generic field.

            // arg is boxType ? &_tmp : throw new ArgumentTypeException()
            //   IncorrectBoxType throws the exception to avoid stack imbalance issues.
            Type boxType = typeof(StrongBox<>).MakeGenericType(_elementType);
            return Expression.Condition(
                Expression.TypeIs(parameters[Index], BoxType),
                Expression.Comma(
                    Expression.Assign(
                        _tmp,
                        Expression.Field(
                            Expression.ConvertHelper(parameters[Index], boxType),
                            boxType.GetField("Value")
                        )
                    ),
                    _tmp
                ),
                Expression.Call(
                    typeof(RuntimeOps).GetMethod("IncorrectBoxType").MakeGenericMethod(_elementType),
                    Expression.ConvertHelper(parameters[Index], typeof(object))
                )
            );
        }

        protected Type BoxType {
            get {
                return this.Type;
            }
        }

        internal Type ElementType {
            get {
                return _elementType;
            }
        }

        internal override VariableExpression[] TemporaryVariables {
            get {
                return new VariableExpression[] { _tmp };
            }
        }

        protected virtual Expression UpdatedValue() {
            return _tmp;
        }

        internal override Expression UpdateFromReturn(IList<Expression> parameters) {
            return Expression.AssignField(
                Expression.Convert(parameters[Index], BoxType),
                BoxType.GetField("Value"),
                UpdatedValue()
            );
        }

        internal override object Build(object[] args) {
            object arg = args[Index];

            if (arg == null) {
                throw new ArgumentTypeException("expected StrongBox, but found null");
            }
            Type argType = arg.GetType();
            if (!argType.IsGenericType || argType.GetGenericTypeDefinition() != typeof(StrongBox<>)) {
                throw new ArgumentTypeException("expected StrongBox<>");
            }
            if (argType.GetGenericArguments()[0] != _elementType) {
                throw new ArgumentTypeException(String.Format("Expected type {0}, got {1}", typeof(StrongBox<>).MakeGenericType(_elementType).FullName, TypeUtils.GetTypeForBinding(arg).FullName));
            }

            object value = ((IStrongBox)arg).Value;

            if (value == null) {
                return null;
            }

            return Convert(value, _elementType);
        }

        internal override void UpdateFromReturn(object callArg, object[] args) {
            ((IStrongBox)args[Index]).Value = callArg;
        }
    }
}

namespace System.Runtime.CompilerServices {
    public static partial class RuntimeOps {
        // TODO: just emit this in the generated code?
        // Having a helper for this one error is overkill
        public static T IncorrectBoxType<T>(object received) {
            throw Error.UnexpectedType("StrongBox<" + typeof(T).Name + ">", TypeUtils.GetTypeForBinding(received).Name);
        }
    }
}

#endif
