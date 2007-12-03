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

using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Ast {

    public sealed class IfStatement : Statement {
        private readonly ReadOnlyCollection<IfStatementTest> _tests;
        private readonly Statement _else;

        internal IfStatement(SourceSpan span, ReadOnlyCollection<IfStatementTest> /*!*/ tests, Statement @else)
            : base(AstNodeType.IfStatement, span) {
            _tests = tests;
            _else = @else;
        }

        public ReadOnlyCollection<IfStatementTest> Tests {
            get { return _tests; }
        }

        public Statement ElseStatement {
            get { return _else; }
        }
    }
}
