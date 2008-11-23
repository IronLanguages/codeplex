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
using System.Globalization;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Com {
    /// <summary>
    /// SimpleArgBuilder produces the value produced by the user as the argument value.  It
    /// also tracks information about the original parameter and is used to create extended
    /// methods for params arrays and param dictionary functions.
    /// </summary>
    internal class SimpleArgBuilder : ArgBuilder {
        private Type _parameterType;
        protected ParameterExpression _unmanagedTemp;

        internal SimpleArgBuilder(Type parameterType) {
            _parameterType = parameterType;
        }

        internal virtual ParameterExpression CreateTemp() {
            return Expression.Variable(_parameterType, "Temp" + _parameterType.Name);
        }

        internal override ParameterExpression[] TemporaryVariables {
            get {
                if (_unmanagedTemp != null) {
                    return new ParameterExpression[] { _unmanagedTemp };
                } else {
                    return base.TemporaryVariables;
                }
            }
        }

        internal override Expression Unwrap(Expression parameter) {
            Debug.Assert(parameter != null);
            return ConvertExpression(parameter, _parameterType);
        }

        internal override Expression UnwrapByRef(Expression parameter) {
            Debug.Assert(parameter != null);

            if (_unmanagedTemp == null) {
                _unmanagedTemp = CreateTemp();
            }

            Debug.Assert(_unmanagedTemp != null);

            return Expression.Comma(
                Expression.Assign(
                    _unmanagedTemp,
                    ConvertExpression(parameter, _unmanagedTemp.Type)
                ),
                _unmanagedTemp
            );
        }

        internal override Expression UpdateFromReturn(Expression parameter, Expression newValue) {
            Debug.Assert(newValue != null && newValue.Type == _parameterType);

            // parameter = newValue
            return Expression.Assign(
                parameter,
                ConvertExpression(newValue, parameter.Type)
            );
        }

        internal override Expression UpdateFromReturn(Expression parameter) {
            if (_unmanagedTemp != null) {
                return UpdateFromReturn(parameter, _unmanagedTemp);
            } else {
                return base.UpdateFromReturn(parameter);
            }
        }




        internal override object UnwrapForReflection(object arg) {
            return Convert(arg, _parameterType);
        }

        internal static Expression ConvertExpression(Expression expr, Type toType) {
            if (TypeUtils.AreReferenceAssignable(toType, expr.Type)) {
                return expr;
            }
            return Expression.ConvertHelper(expr, toType);
        }

        protected Type ParameterType {
            get {
                return _parameterType;
            }
        }

        internal static object Convert(object obj, Type toType) {
            if (obj == null) {
                if (!toType.IsValueType) {
                    return null;
                }
            } else {
                if (toType.IsValueType) {
                    if (toType == obj.GetType()) {
                        return obj;
                    }
                } else {
                    if (toType.IsAssignableFrom(obj.GetType())) {
                        return obj;
                    }
                }
            }

            return System.Convert.ChangeType(obj, toType, CultureInfo.InvariantCulture);
        }
    }
}

#endif
