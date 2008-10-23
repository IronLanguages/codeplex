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

using System.Globalization;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.ComInterop {

    internal class ConvertibleArgBuilder : ArgBuilder {
        internal ConvertibleArgBuilder() {
        }

        internal override Expression Marshal(Expression parameter) {
            return Expression.ConvertHelper(parameter, typeof(IConvertible));
        }

        internal override Expression MarshalToRef(Expression parameter) {
            throw Assert.Unreachable;
        }

        internal override Expression UpdateFromReturn(Expression parameter, Expression newValue) {
            //nothing to update as we do not support ByRef for IConvertible.
            return null;
        }

        internal override object UnwrapForReflection(object arg) {
            IConvertible icon = (IConvertible)arg;
            TypeCode tc = icon.GetTypeCode();

            return System.Convert.ChangeType(arg, tc, CultureInfo.CurrentCulture);
        }
    }
}

#endif
