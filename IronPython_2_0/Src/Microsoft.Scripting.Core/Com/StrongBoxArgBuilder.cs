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
    internal class StrongBoxArgBuilder : ArgBuilder {
        private Type _parameterType;
        private Type _elementType;
        private ArgBuilder _innerBuilder;

        internal StrongBoxArgBuilder(Type parameterType, ArgBuilder innerBuilder) {
            _innerBuilder = innerBuilder;
            _parameterType = parameterType;
            Debug.Assert(_parameterType.GetGenericTypeDefinition() == typeof(StrongBox<>));
            _elementType = _parameterType.GetGenericArguments()[0];
        }

        internal override Expression Unwrap(Expression parameter) {
            return Expression.ConvertHelper(parameter, _parameterType);
        }

        internal override Expression UnwrapByRef(Expression parameter) {
            // temp = { parameter is _parameterType ? parameter.Value : throw error }
            return _innerBuilder.UnwrapByRef(
                Expression.Condition(
                    Expression.TypeIs(parameter, _parameterType),
                    Expression.Field(
                        //TODO: could use DirectCast as we really need to be sure we are modifying the original argument.
                        Expression.ConvertHelper(parameter, _parameterType),
                        "Value"
                    ),
                    Expression.Call(
                        typeof(RuntimeOps).GetMethod("IncorrectBoxType").MakeGenericMethod(_parameterType.GetGenericArguments()[0]),
                        Expression.ConvertHelper(parameter, typeof(object))
                    )
                )
            );
        }

        internal override Expression UpdateFromReturn(Expression parameter) {
            // we are updating parameter, but also passing it to base so that it had a chance to copyback 
            // if parameter itself is a temp in the outer builder.
            // parameter = { parameter.Value = temp, parameter }
            return _innerBuilder.UpdateFromReturn(
                Expression.Field(
                    Expression.TypeAs(parameter, _parameterType),
                    _parameterType.GetField("Value")
                )
            );
        }

        internal override ParameterExpression[] TemporaryVariables {
            get {
                return _innerBuilder.TemporaryVariables;
            }
        }



        internal override object UnwrapForReflection(object arg) {
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

            return SimpleArgBuilder.Convert(value, _elementType);
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
