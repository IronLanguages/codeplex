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
        public static SwitchStatementBuilder Switch(LabelTarget label, SourceSpan span, SourceLocation header) {
            return Expression.Switch(label, Expression.Annotate(span, header));
        }

        public static SwitchStatementBuilder Switch(Expression test, SourceSpan span, SourceLocation header) {
            return Expression.Switch(test, Expression.Annotate(span, header));
        }

        public static SwitchStatementBuilder Switch(Expression test, LabelTarget label, SourceSpan span, SourceLocation header) {
            return Expression.Switch(test, label, Expression.Annotate(span, header));
        }
    }
}
