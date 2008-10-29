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
using System.Globalization;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.ComInterop {
    /// <summary>
    /// SimpleArgBuilder produces the value produced by the user as the argument value.  It
    /// also tracks information about the original parameter and is used to create extended
    /// methods for params arrays and param dictionary functions.
    /// </summary>
    internal class SimpleArgBuilder : ArgBuilder {
        private Type _parameterType;

        internal SimpleArgBuilder(Type parameterType) {
            _parameterType = parameterType;
        }

        internal override Expression Marshal(Expression parameter) {
            Debug.Assert(parameter != null);
            return Helpers.Convert(parameter, _parameterType);
        }

        // unmarshal new value back to be _parameterType
        // trivial builder can just return the value.
        // non trivial builders will override this function.
        internal virtual Expression UnmarshalFromRef(Expression value) {
            Debug.Assert(value != null && value.Type == _parameterType);

            return value;
        }

        internal sealed override Expression UpdateFromReturn(Expression parameter, Expression newValue) {
            newValue = UnmarshalFromRef(newValue);
            Debug.Assert(newValue != null && newValue.Type == _parameterType);

            // parameter = newValue
            return Expression.Assign(parameter, Helpers.Convert(newValue, parameter.Type));
        }



        internal override object UnwrapForReflection(object arg) {
            return Convert(arg, _parameterType);
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