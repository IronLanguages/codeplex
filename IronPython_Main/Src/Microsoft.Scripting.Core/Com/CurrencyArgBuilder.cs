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

#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Com {
    internal sealed class CurrencyArgBuilder : SimpleArgBuilder {
        internal CurrencyArgBuilder(Type parameterType)
            : base(parameterType) {
            Debug.Assert(parameterType == typeof(CurrencyWrapper));
        }

        internal override ParameterExpression CreateTemp() {
            return Expression.Variable(typeof(Int64), "TempInt64");
        }

        internal override Expression Unwrap(Expression parameter) {
            // parameter.WrappedObject
            return Expression.Property(
                base.Unwrap(parameter),
                "WrappedObject"
            );
        }

        internal override Expression UnwrapByRef(Expression parameter) {
            // temp = Decimal.ToOACurrency(parameter.WrappedObject)
            return base.UnwrapByRef(
                Expression.Call(
                    typeof(Decimal).GetMethod("ToOACurrency"),
                    Unwrap(parameter)
                )
            );
        }

        internal override Expression UpdateFromReturn(Expression parameter, Expression temp) {
            // parameter = new CurrencyWrapper(Decimal.FromOACurrency(temp))
            return base.UpdateFromReturn(
                parameter,
                Expression.New(
                    typeof(CurrencyWrapper).GetConstructor(new Type[] { typeof(Decimal) }),
                    Expression.Call(
                        typeof(Decimal).GetMethod("FromOACurrency"),
                        temp
                    )
                )
            );
        }

        internal override object UnwrapForReflection(object arg) {
            arg = base.UnwrapForReflection(arg);
            CurrencyWrapper cw = arg as CurrencyWrapper;
            Debug.Assert(cw != null);
            return cw.WrappedObject;
        }
    }
}

#endif