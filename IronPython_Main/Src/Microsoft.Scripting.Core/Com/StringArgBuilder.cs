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

namespace Microsoft.Scripting.ComInterop {

    internal class StringArgBuilder : SimpleArgBuilder {
        internal StringArgBuilder(Type parameterType)
            : base(parameterType) {
            Debug.Assert(parameterType == typeof(string));
        }

        internal override Expression MarshalToRef(Expression parameter) {
            // Marshal.StringToBSTR(parameter)
            return Expression.Call(
                typeof(Marshal).GetMethod("StringToBSTR"),
                Marshal(parameter)
            );
        }

        internal override Expression UnmarshalFromRef(Expression value) {
            // Marshal.PtrToStringBSTR(temp)
            return Expression.Call(
                typeof(Marshal).GetMethod("PtrToStringBSTR"),
                value
            );
        }

        internal override object UnwrapForReflection(object arg) {
            // If the argument is null, Type.InvokeMember will not know to marshal it as a VT_BSTR. Hence, wrap it up in a BStrWrapper
            return new BStrWrapper((string)base.UnwrapForReflection(arg));
        }
    }
}

#endif
