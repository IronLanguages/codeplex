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
#if !SILVERLIGHT

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Com {

    /// <summary>
    /// An argument that the user wants to explicitly pass by-reference (with copy-in copy-out semantics).
    /// The user passes a StrongBox[T] object whose value will get updated when the call returns.
    /// </summary>
    internal class ReferenceArgBuilder : SimpleArgBuilder {
        private Type _elementType;
        private VariableExpression _tmp;

        internal ReferenceArgBuilder(Type parameterType)
            : base(parameterType) {
            Debug.Assert(parameterType.GetGenericTypeDefinition() == typeof(StrongBox<>));
            _elementType = parameterType.GetGenericArguments()[0];
        }

        internal override Expression Build(Expression parameter) {
            if (_tmp == null) {
                _tmp = Expression.Variable(_elementType, "outParam");
            }

            // arg is boxType ? &_tmp : throw new ArgumentTypeException()
            //   IncorrectBoxType throws the exception to avoid stack imbalance issues.
            Type boxType = typeof(StrongBox<>).MakeGenericType(_elementType);
            return Expression.Condition(
                Expression.TypeIs(parameter, ParameterType),
                Expression.Comma(
                    Expression.Assign(
                        _tmp,
                        Expression.Field(
                            Expression.ConvertHelper(parameter, boxType),
                            boxType.GetField("Value")
                        )
                    ),
                    _tmp
                ),
                Expression.Call(
                    typeof(RuntimeOps).GetMethod("IncorrectBoxType").MakeGenericMethod(_elementType),
                    Expression.ConvertHelper(parameter, typeof(object))
                )
            );
        }

        internal override VariableExpression[] TemporaryVariables {
            get {
                return new VariableExpression[] { _tmp };
            }
        }

        protected Type ElementType {
            get { return _elementType; }
        }

        protected virtual Expression UpdatedValue() {
            return _tmp;
        }

        internal override Expression UpdateFromReturn(Expression parameter) {
            return Expression.AssignField(
                Expression.Convert(parameter, ParameterType),
                ParameterType.GetField("Value"),
                UpdatedValue()
            );
        }

        internal override object Build(object arg) {
            ContractUtils.RequiresNotNull(arg, "args");
            if (arg == null) {
                Error.FirstArgumentMustBeStrongBox();
            }
            Type argType = arg.GetType();
            if (!argType.IsGenericType || argType.GetGenericTypeDefinition() != typeof(StrongBox<>)) {
                throw Error.UnexpectedType(typeof(StrongBox<>), argType);
            }
            if (argType.GetGenericArguments()[0] != _elementType) {
                throw Error.UnexpectedType(typeof(StrongBox<>).MakeGenericType(_elementType).FullName, TypeUtils.GetTypeForBinding(arg).FullName);
            }

            object value = ((IStrongBox)arg).Value;

            if (value == null) {
                return null;
            }

            return Convert(value, _elementType);
        }

        internal override void UpdateFromReturn(object originalArg, object updatedArg) {
            ((IStrongBox)originalArg).Value = updatedArg;
        }
    }
}

namespace Microsoft.Runtime.CompilerServices {
    public static partial class RuntimeOps {
        // TODO: just emit this in the generated code?
        // Having a helper for this one error is overkill
        public static T IncorrectBoxType<T>(object received) {
            throw Error.UnexpectedType("StrongBox<" + typeof(T).Name + ">", TypeUtils.GetTypeForBinding(received).Name);
        }
    }
}

#endif
