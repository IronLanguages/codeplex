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
using System.Scripting.Runtime;

namespace System.Scripting.Com {
    /// <summary>
    /// SimpleArgBuilder produces the value produced by the user as the argument value.  It
    /// also tracks information about the original parameter and is used to create extended
    /// methods for params arrays and param dictionary functions.
    /// </summary>
    internal class SimpleArgBuilder : ArgBuilder {
        private int _index;
        private Type _parameterType;

        internal SimpleArgBuilder(int index, Type parameterType) {
            _index = index;
            _parameterType = parameterType;
        }

        internal override object Build(object[] args) {
            return Convert(args[_index], _parameterType);
        }

        internal override Expression ToExpression(IList<Expression> parameters) {
            Debug.Assert(_index < parameters.Count);
            Debug.Assert(parameters[_index] != null);
            return ConvertExpression(parameters[_index], _parameterType);
        }

        internal static Expression ConvertExpression(Expression expr, Type toType) {
            if (toType.IsAssignableFrom(expr.Type)) {
                return expr;
            }
            return Expression.Convert(expr, toType);
        }

        internal int Index {
            get {
                return _index;
            }
        }

        protected override Type Type {
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

            return System.Convert.ChangeType(obj, toType);
        }
    }
}

#endif