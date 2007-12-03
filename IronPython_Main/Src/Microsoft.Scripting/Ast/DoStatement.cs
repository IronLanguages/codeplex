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
    public sealed class DoStatement : Statement {
        private readonly SourceLocation _header;
        private readonly Expression /*!*/ _test;
        private readonly Statement /*!*/ _body;

        /// <summary>
        /// Called by <see cref="DoStatementBuilder"/>.
        /// </summary>
        internal DoStatement(SourceSpan span, SourceLocation header, Expression /*!*/ test, Statement /*!*/ body)
            : base(AstNodeType.DoStatement, span) {
            _header = header;
            _test = test;
            _body = body;
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public Expression Test {
            get { return _test; }
        }

        public Statement Body {
            get { return _body; }
        }                
    }
}
