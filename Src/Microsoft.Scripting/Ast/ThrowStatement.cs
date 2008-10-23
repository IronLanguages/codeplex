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
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
        [Obsolete("use Expression.Rethrow instead")]
        public static ThrowExpression Rethrow(SourceSpan span) {
            return Expression.Throw(null, Expression.Annotate(span));
        }

        [Obsolete("use Expression.Throw instead")]
        public static ThrowExpression Throw(Expression value, SourceSpan span) {
            return Expression.Throw(value, Expression.Annotate(span));
        }
    }
}
