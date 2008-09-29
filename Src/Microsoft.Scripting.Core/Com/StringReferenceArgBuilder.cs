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
    /// <summary>
    /// This allows passing a string as a BSTR, given a StrongBox{string}
    /// ReferenceArgBuilder can be used when COM and the CLR agree on the data
    /// layout of the data type, like for integral types. However, strings for
    /// are represented as BSTR and System.String respectively in the two
    /// worlds. ReferenceArgBuilder cannot be used as is in such cases
    /// </summary>
    internal sealed class StringReferenceArgBuilder : ReferenceArgBuilder {
        private VariableExpression _unmanagedTemp;

        internal StringReferenceArgBuilder(Type parameterType)
            : base(parameterType) {
            Debug.Assert(ElementType == typeof(string));
        }

        internal override Expression Build(Expression parameter) {
            if (_unmanagedTemp == null) {
                _unmanagedTemp = Expression.Variable(typeof(IntPtr), "unmanagedOutParam");
            }

            // (_unmanagedTemp = Marshal.StringToBSTR(<string from underlying builder>)), &_unmanagedTemp

            return Expression.Comma(
                Expression.Assign(
                    _unmanagedTemp,
                    Expression.Call(
                        typeof(Marshal).GetMethod("StringToBSTR"),
                        base.Build(parameter))),
                _unmanagedTemp);
        }

        internal override VariableExpression[] TemporaryVariables {
            get {
                return base.TemporaryVariables.AddFirst(_unmanagedTemp);
            }
        }

        protected override Expression UpdatedValue() {
            // Marhsal.PtrToStringBSTR(_unmanagedTemp)
            return Expression.Call(
                typeof(Marshal).GetMethod("PtrToStringBSTR"),
                _unmanagedTemp);
        }

        internal List<Expression> Clear() {
            List<Expression> exprs = new List<Expression>();
            Expression expr;

            // Marhsal.FreeBSTR(_unmanagedTemp)
            expr = Expression.Call(
                typeof(Marshal).GetMethod("FreeBSTR"),
                _unmanagedTemp
            );
            exprs.Add(expr);
            return exprs;
        }

        internal override object Build(object arg) {
            // If the argument is null, Type.InvokeMember will not know to marshal it as a VT_BSTR. Hence, wrap it up in a BStrWrapper
            return new BStrWrapper((string)base.Build(arg));
        }
    }
}

#endif
