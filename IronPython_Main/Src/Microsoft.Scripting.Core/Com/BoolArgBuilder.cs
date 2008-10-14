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
    internal sealed class BoolArgBuilder : SimpleArgBuilder {
        internal BoolArgBuilder(Type parameterType)
            : base(parameterType) {
            Debug.Assert(parameterType == typeof(bool));
        }

        internal override ParameterExpression CreateTemp() {
            return Expression.Variable(typeof(Int16), "TempInt16");
        }

        internal override Expression UnwrapByRef(Expression parameter) {
            // parameter  ? -1 : 0
            return base.UnwrapByRef(
                Expression.Condition(
                    Unwrap(parameter),
                    Expression.Constant((Int16)(-1)),
                    Expression.Constant((Int16)0)
                )
            );
        }

        internal override Expression UpdateFromReturn(Expression parameter, Expression temp) {
            //parameter = temp != 0
            return base.UpdateFromReturn(
                 parameter,
                 Expression.NotEqual(
                     temp,
                     Expression.Constant((Int16)0)
                 )
             );
        }
    }
}

#endif