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


using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
        public static MethodCallExpression Call(MethodInfo method, SourceSpan span, params Expression[] arguments) {
            return Expression.Call(null, method, Expression.Annotate(span), arguments);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, SourceSpan span, params Expression[] arguments) {
            return Expression.Call(instance, method, Expression.Annotate(span), arguments);
        }
    }
}