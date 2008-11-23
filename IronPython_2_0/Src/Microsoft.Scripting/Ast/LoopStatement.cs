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
    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Utils {
        public static LoopStatement While(Expression test, Expression body, Expression @else) {
            return Expression.Loop(test, null, body, @else, null, null);
        }

        public static LoopStatement While(Expression test, Expression body, Expression @else, LabelTarget @break, LabelTarget @continue) {
            return Expression.Loop(test, null, body, @else, @break, @continue);
        }

        public static LoopStatement While(Expression test, Expression body, Expression @else, LabelTarget @break, LabelTarget @continue, SourceLocation header, SourceSpan span) {
            return Expression.Loop(test, null, body, @else, @break, @continue, Expression.Annotate(header, span));
        }

        public static LoopStatement Infinite(Expression body) {
            return Expression.Loop(null, null, body, null, null, null);
        }

        public static LoopStatement Infinite(Expression body, LabelTarget @break, LabelTarget @continue) {
            return Expression.Loop(null, null, body, null, @break, @continue);
        }

        public static LoopStatement Loop(Expression test, Expression increment, Expression body, Expression @else) {
            return Expression.Loop(test, increment, body, @else, null, null);
        }

        public static LoopStatement Loop(Expression test, Expression increment, Expression body, Expression @else, LabelTarget @break, LabelTarget @continue, SourceLocation header, SourceSpan span) {
            return Expression.Loop(test, increment, body, @else, @break, @continue, Expression.Annotate(span, header));
        }
    }
}
