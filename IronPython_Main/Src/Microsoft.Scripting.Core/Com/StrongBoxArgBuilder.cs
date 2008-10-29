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
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.ComInterop {

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

        internal override Expression Marshal(Expression parameter) {
            Debug.Assert(false, "passing StrongBox<T> byval");
            throw new NotSupportedException();
        }

        internal override Expression MarshalToRef(Expression parameter) {
            return _innerBuilder.MarshalToRef(
                Expression.Field(Helpers.Convert(parameter, _parameterType), "Value")
            );
        }

        internal override Expression UpdateFromReturn(Expression parameter, Expression newValue) {
            return _innerBuilder.UpdateFromReturn(
                Expression.Field(Helpers.Convert(parameter, _parameterType), "Value"),
                newValue
            );
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
            IStrongBox originalBox = (IStrongBox)originalArg;
            object originalValue = originalBox.Value;
            _innerBuilder.UpdateFromReturn(originalValue, updatedArg);

            originalBox.Value = updatedArg;
        }
    }
}

#endif
