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

using System;
using System.Linq.Expressions;
using System.Scripting;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
        [Obsolete("use Expression.Try instead")]
        public static TryStatementBuilder Try(Expression body, SourceSpan span, SourceLocation header) {
            return Expression.Try(Expression.Annotate(span, header), body);
        }

        [Obsolete("use Expression.TryCatch instead")]
        public static TryStatement TryCatch(Expression body, SourceSpan span, SourceLocation header, params CatchBlock[] handlers) {
            return Expression.TryCatch(body, Expression.Annotate(span, header), handlers);
        }

        [Obsolete("use Expression.TryFinally instead")]
        public static TryStatement TryFinally(Expression body, Expression @finally, SourceSpan span, SourceLocation header) {
            return Expression.TryFinally(body, @finally, Expression.Annotate(span, header));
        }

        [Obsolete("use Expression.TryCatchFinally instead")]
        public static TryStatement TryCatchFinally(Expression body, Expression @finally, SourceSpan span, SourceLocation header, params CatchBlock[] handlers) {
            return Expression.TryCatchFinally(body, @finally, Expression.Annotate(span, header), handlers);
        }
    }
}
