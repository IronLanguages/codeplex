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

#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using CompilerServices = System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Actions.ComDispatch {

    /// <summary>
    /// An argument that the user wants to explicitly pass by-reference (with copy-in copy-out semantics).
    /// The user passes a CompilerServices.StrongBox[T] object whose value will get updated when the call returns.
    /// </summary>
    internal class ReferenceArgBuilder : SimpleArgBuilder {
        private Type _elementType;
        private VariableExpression _tmp;

        internal ReferenceArgBuilder(int index, Type parameterType)
            : base(index, parameterType) {
            Debug.Assert(parameterType.GetGenericTypeDefinition() == typeof(CompilerServices.StrongBox<>));
            _elementType = parameterType.GetGenericArguments()[0];
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters) {
            if (_tmp == null) {
                _tmp = context.GetTemporary(_elementType, "outParam");
            }

            // Ideally we'd pass in Expression.ReadField(parameters[Index], "Value") but due to
            // a bug in partial trust we can't access the generic field.

            // arg is boxType ? &_tmp : throw new ArgumentTypeException()
            //   IncorrectBoxType throws the exception to avoid stack imbalance issues.
            Type boxType = typeof(CompilerServices.StrongBox<>).MakeGenericType(_elementType);
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
                    typeof(RuntimeHelpers).GetMethod("IncorrectBoxType").MakeGenericMethod(_elementType),
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

        internal override object Build(CodeContext context, object[] args) {
            object arg = args[Index];

            if (arg == null) {
                throw RuntimeHelpers.SimpleTypeError("expected StrongBox, but found null");
            }
            Type argType = arg.GetType();
            if (!argType.IsGenericType || argType.GetGenericTypeDefinition() != typeof(CompilerServices.StrongBox<>)) {
                throw RuntimeHelpers.SimpleTypeError("expected StrongBox<>");
            }
            if (argType.GetGenericArguments()[0] != _elementType) {
                throw RuntimeHelpers.SimpleTypeError(String.Format("Expected type {0}, got {1}", typeof(CompilerServices.StrongBox<>).MakeGenericType(_elementType).FullName, CompilerHelpers.GetType(arg).FullName));
            }

            object value = ((CompilerServices.IStrongBox)arg).Value;

            if (value == null) return null;
            return context.LanguageContext.Binder.Convert(value, _elementType);
        }

        internal override void UpdateFromReturn(object callArg, object[] args) {
            ((CompilerServices.IStrongBox)args[Index]).Value = callArg;
        }
    }
}

#endif
