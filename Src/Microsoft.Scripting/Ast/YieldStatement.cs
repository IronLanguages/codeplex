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

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class YieldStatement : Statement {
        private readonly Expression /*!*/ _expr;
        private YieldTarget _target;

        internal YieldStatement(SourceSpan span, Expression /*!*/ expression)
            : base(AstNodeType.YieldStatement, span) {
            _expr = expression;
        }

        public Expression Expression {
            get { return _expr; }
        }


        // TODO: Remove !!!
        internal YieldTarget Target {
            get { return _target; }
            set { _target = value; }
        }
    }

    /// <summary>
    /// Factory methods
    /// </summary>
    public static partial class Ast {
        public static YieldStatement Yield(Expression expression) {
            return Yield(SourceSpan.None, expression);
        }

        public static YieldStatement Yield(SourceSpan span, Expression expression) {
            Contract.Requires(expression != null, "expression");
            return new YieldStatement(span, expression);
        }
    }
}
