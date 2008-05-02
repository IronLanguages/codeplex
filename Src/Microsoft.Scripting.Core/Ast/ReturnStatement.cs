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

namespace Microsoft.Scripting.Ast {
    public sealed class ReturnStatement : Expression {
        private readonly Expression _expr;

        internal ReturnStatement(Annotations annotations, Expression expression)
            : base(annotations, AstNodeType.ReturnStatement, typeof(void)) {
            _expr = expression;
        }

        public Expression Expression {
            get { return _expr; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        public static ReturnStatement Return() {
            return Return(SourceSpan.None, null);
        }

        public static ReturnStatement Return(Expression expression) {
            return Return(SourceSpan.None, expression);
        }

        public static ReturnStatement Return(SourceSpan span, Expression expression) {
            return Return(Annotate(span), expression);
        }

        public static ReturnStatement Return(Annotations annotations, Expression expression) {
            return new ReturnStatement(annotations, expression);
        }
    }
}
