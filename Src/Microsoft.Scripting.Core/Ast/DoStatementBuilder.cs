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
    public sealed class DoStatementBuilder {
        private readonly Expression _body;
        private readonly SourceLocation _doLocation;
        private readonly SourceSpan _statementSpan;
        private readonly LabelTarget _label;

        internal DoStatementBuilder(SourceSpan statementSpan, SourceLocation location, LabelTarget label, Expression body) {
            Contract.RequiresNotNull(body, "body");

            _body = body;
            _doLocation = location;
            _statementSpan = statementSpan;
            _label = label;
        }

        public DoStatement While(Expression condition) {
            Contract.RequiresNotNull(condition, "condition");
            Contract.Requires(condition.Type == typeof(bool), "condition", "Condition must be boolean");

            return new DoStatement(Ast.Annotations(_statementSpan, _doLocation), _label, condition, _body);
        }
    }

    public static partial class Ast {
        public static DoStatementBuilder Do(params Expression[] body) {
            Contract.RequiresNotNullItems(body, "body");
            return new DoStatementBuilder(SourceSpan.None, SourceLocation.None, null, Block(body));
        }

        public static DoStatementBuilder Do(LabelTarget label, params Expression[] body) {
            Contract.RequiresNotNullItems(body, "body");
            return new DoStatementBuilder(SourceSpan.None, SourceLocation.None, label, Block(body));
        }

        public static DoStatementBuilder Do(Expression body) {
            return new DoStatementBuilder(SourceSpan.None, SourceLocation.None, null, body);
        }

        public static DoStatementBuilder Do(LabelTarget label, Expression body) {
            return new DoStatementBuilder(SourceSpan.None, SourceLocation.None, label, body);
        }

        public static DoStatementBuilder Do(SourceSpan statementSpan, SourceLocation location, Expression body) {
            return new DoStatementBuilder(statementSpan, location, null, body);
        }

        public static DoStatementBuilder Do(SourceSpan statementSpan, SourceLocation location, LabelTarget label, Expression body) {
            return new DoStatementBuilder(statementSpan, location, label, body);
        }
    }
}
