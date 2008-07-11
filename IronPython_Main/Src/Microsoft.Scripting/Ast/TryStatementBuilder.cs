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

using System.Scripting;
using System.Linq.Expressions;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
        public static TryStatementBuilder Try(Expression body, SourceSpan span, SourceLocation header) {
            return Expression.Try(Expression.Annotate(span, header), body);
        }

        public static TryStatement TryCatch(Expression body, SourceSpan span, SourceLocation header, params CatchBlock[] handlers) {
            return Expression.TryCatchFinally(body, handlers, null, null, Expression.Annotate(span, header));
        }

        public static TryStatement TryFinally(Expression body, Expression @finally, SourceSpan span, SourceLocation header) {
            return Expression.TryCatchFinally(body, null, @finally, null, Expression.Annotate(span, header));
        }

        public static TryStatement TryCatchFinally(Expression body, Expression @finally, SourceSpan span, SourceLocation header, params CatchBlock[] handlers) {
            return Expression.TryCatchFinally(body, handlers, @finally, null, Expression.Annotate(span, header));
        }
    }
}
