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
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using CompilerServices = System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Generation {

    /// <summary>
    /// An argument that the user wants to explicitly pass by-reference (with copy-in copy-out semantics).
    /// The user passes a StrongBox[T] object whose value will get updated when the call returns.
    /// </summary>
    internal sealed class ReferenceArgBuilder : SimpleArgBuilder {
        private readonly Type _elementType;
        private VariableExpression _tmp;

        public ReferenceArgBuilder(int index, Type parameterType)
            : base(index, parameterType) {
            Debug.Assert(parameterType.GetGenericTypeDefinition() == typeof(CompilerServices.StrongBox<>));
            _elementType = parameterType.GetGenericArguments()[0];
        }

        public override SimpleArgBuilder Copy(int newIndex) {
            return new ReferenceArgBuilder(newIndex, Type);
        }

        public override int Priority {
            get { return 5; }
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters, bool[] hasBeenUsed) {
            if (_tmp == null) {
                _tmp = context.GetTemporary(_elementType, "outParam");
            }

            // Ideally we'd pass in Expression.Field(parameters[Index], "Value") but due to
            // a bug in partial trust we can't access the generic field.

            // arg is boxType ? &_tmp : throw new ArgumentTypeException()
            hasBeenUsed[Index] = true;
            Type boxType = typeof(CompilerServices.StrongBox<>).MakeGenericType(_elementType);
            return Expression.Condition(
                Expression.TypeIs(parameters[Index], Type),
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
                    typeof(RuntimeHelpers).GetMethod("IncorrectBoxType").MakeGenericMethod(_elementType),
                    Expression.ConvertHelper(parameters[Index], typeof(object))
                )
            );
        }

        internal override Expression UpdateFromReturn(MethodBinderContext context, IList<Expression> parameters) {
            return Expression.AssignField(
                Expression.Convert(parameters[Index], Type),
                Type.GetField("Value"),
                _tmp
            );
        }
    }
}
