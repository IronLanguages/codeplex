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
        [Obsolete("use Expression.Return(LabelTarget, Expression, Annotations) instead")]
        public static ReturnStatement Return(Expression expression, SourceSpan span) {
#pragma warning disable 618
            return Expression.Return(expression, Expression.Annotate(span));
#pragma warning restore 618
        }

        [Obsolete("use Expression.Return(LabelTarget, Annotations) instead")]
        public static ReturnStatement Return(SourceSpan span) {
#pragma warning disable 618
            return Expression.Return(null, Expression.Annotate(span));
#pragma warning restore 618
        }
    }
}
