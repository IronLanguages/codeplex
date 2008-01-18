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
    public sealed class DoStatement : Expression, ISpan {
        private readonly SourceLocation _header;
        private readonly Expression /*!*/ _test;
        private readonly Expression /*!*/ _body;

        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        /// <summary>
        /// Called by <see cref="DoStatementBuilder"/>.
        /// </summary>
        internal DoStatement(SourceLocation start, SourceLocation end, SourceLocation header, Expression /*!*/ test, Expression /*!*/ body)
            : base(AstNodeType.DoStatement, typeof(void)) {
            _start = start;
            _end = end;
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

        public Expression Body {
            get { return _body; }
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }
    }
}
