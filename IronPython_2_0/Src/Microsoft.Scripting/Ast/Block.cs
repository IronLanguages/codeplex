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


using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;
using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
        /// <summary>
        /// Creates a list of expressions whose value is the value of the last expression.
        /// </summary>
        public static Block Block(SourceSpan span, IEnumerable<Expression> expressions) {
            return Expression.Block(Expression.Annotate(span), expressions);
        }

        public static Block Block(SourceSpan span, params Expression[] expressions) {
            return Expression.Block(Expression.Annotate(span), (IList<Expression>)expressions);
        }
        
        public static Block Block(SourceSpan span, Expression arg0) {
            return Expression.Block(Expression.Annotate(span), new ReadOnlyCollection<Expression>(new[] { arg0 }));
        }
        
        public static Block Block(SourceSpan span, Expression arg0, Expression arg1) {
            return Expression.Block(Expression.Annotate(span), new ReadOnlyCollection<Expression>(new[] { arg0, arg1 }));
        }
        
        public static Block Block(SourceSpan span, Expression arg0, Expression arg1, Expression arg2) {
            return Expression.Block(Expression.Annotate(span), new ReadOnlyCollection<Expression>(new[] { arg0, arg1, arg2 }));
        }

        /// <summary>
        /// Creates a list of expressions whose value is the value of the last expression.
        /// </summary>
        public static Block Comma(SourceSpan span, IEnumerable<Expression> expressions) {
            return Expression.Comma(Expression.Annotate(span), expressions);
        }

        public static Block Comma(SourceSpan span, params Expression[] expressions) {
            return Expression.Comma(Expression.Annotate(span), (IList<Expression>)expressions);
        }
        
        public static Block Comma(SourceSpan span, Expression arg0) {
            return Expression.Comma(Expression.Annotate(span), new ReadOnlyCollection<Expression>(new[] { arg0 }));
        }
    }
}
