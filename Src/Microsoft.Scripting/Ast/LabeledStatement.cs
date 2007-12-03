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
    public sealed class LabeledStatement : Statement {
        private Statement _statement;

        internal LabeledStatement(SourceSpan span, Statement statement)
            : base(AstNodeType.LabeledStatement, span) {
            _statement = statement;
        }

        public Statement Statement {
            get { return _statement; }
        }

        public LabeledStatement Mark(Statement statement) {
            Contract.RequiresNotNull(statement, "statement");
            _statement = statement;
            return this;
        }
    }

    public static partial class Ast {
        public static LabeledStatement Labeled(Statement statement) {
            return Labeled(SourceSpan.None, statement);
        }

        public static LabeledStatement Labeled(SourceSpan span, Statement statement) {
            return new LabeledStatement(span, statement);
        }
    }
}
