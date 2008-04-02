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
    public sealed class DoStatement : Expression {
        private readonly Expression /*!*/ _test;
        private readonly Expression /*!*/ _body;

        private readonly LabelTarget _label;

        /// <summary>
        /// Called by <see cref="DoStatementBuilder"/>.
        /// </summary>
        internal DoStatement(Annotations annotations, LabelTarget label, Expression /*!*/ test, Expression /*!*/ body)
            : base(annotations, AstNodeType.DoStatement, typeof(void)) {
            _test = test;
            _body = body;
            _label = label;
        }

        public SourceLocation Header {
            get { return Annotations.Get<SourceLocation>(); }
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Body {
            get { return _body; }
        }

        public LabelTarget Label {
            get { return _label; }
        }
    }
}
