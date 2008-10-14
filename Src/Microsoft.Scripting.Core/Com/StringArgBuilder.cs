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
using Microsoft.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Microsoft.Scripting.Com {

    internal class StringArgBuilder : SimpleArgBuilder {
        internal StringArgBuilder(Type parameterType)
            : base(parameterType) {
            Debug.Assert(parameterType == typeof(string));
        }

        internal override ParameterExpression CreateTemp() {
            return Expression.Variable(typeof(IntPtr), "TempIntPtr");
        }

        internal override Expression UnwrapByRef(Expression parameter) {
            // temp = Marshal.StringToBSTR(parameter)
            return base.UnwrapByRef(
                Expression.Call(
                    typeof(Marshal).GetMethod("StringToBSTR"),
                    Unwrap(parameter)
                )
            );
        }

        internal override Expression UpdateFromReturn(Expression parameter, Expression temp) {
            // parameter = Marshal.PtrToStringBSTR(temp)
            return base.UpdateFromReturn(
                parameter,
                Expression.Call(
                    typeof(Marshal).GetMethod("PtrToStringBSTR"),
                    temp
                )
            );
        }

        internal List<Expression> Clear() {
            List<Expression> exprs = new List<Expression>();
            Expression expr;

            // Marshal.FreeBSTR(_unmanagedTemp)
            expr = Expression.Call(
                typeof(Marshal).GetMethod("FreeBSTR"),
                _unmanagedTemp
            );
            exprs.Add(expr);
            return exprs;
        }

        internal override object UnwrapForReflection(object arg) {
            // If the argument is null, Type.InvokeMember will not know to marshal it as a VT_BSTR. Hence, wrap it up in a BStrWrapper
            return new BStrWrapper((string)base.UnwrapForReflection(arg));
        }
    }
}

#endif
