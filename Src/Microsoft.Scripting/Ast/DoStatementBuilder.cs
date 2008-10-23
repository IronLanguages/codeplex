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
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class DoStatementBuilder {
        private readonly Expression _body;
        private readonly Annotations _annotations;
        private readonly LabelTarget _break;
        private readonly LabelTarget _continue;

        internal DoStatementBuilder(Annotations annotations, LabelTarget @break, LabelTarget @continue, Expression body) {
            ContractUtils.RequiresNotNull(body, "body");

            _body = body;
            _annotations = annotations;
            _break = @break;
            _continue = @continue;
        }

        public DoStatement While(Expression condition) {
            return Expression.DoWhile(_body, condition, _break, _continue, _annotations);
        }
    }

    public partial class Utils {
        public static DoStatementBuilder Do(params Expression[] body) {
            ContractUtils.RequiresNotNullItems(body, "body");
            return new DoStatementBuilder(null, null, null, Expression.Block(body));
        }

        public static DoStatementBuilder Do(LabelTarget @break, LabelTarget @continue, params Expression[] body) {
            ContractUtils.RequiresNotNullItems(body, "body");
            return new DoStatementBuilder(null, @break, @continue, Expression.Block(body));
        }

        public static DoStatementBuilder Do(LabelTarget @break, LabelTarget @continue, Annotations annotations, params Expression[] body) {
            return new DoStatementBuilder(annotations, @break, @continue, Expression.Block(body));
        }

        [Obsolete("use a Do overload without SourceSpan")]
        public static DoStatementBuilder Do(SourceSpan statementSpan, SourceLocation location, params Expression[] body) {
            return Do(null, null, Expression.Annotate(statementSpan, location), body);
        }

        [Obsolete("use a Do overload without SourceSpan")]
        public static DoStatementBuilder Do(SourceSpan statementSpan, SourceLocation location, LabelTarget @break, LabelTarget @continue, params Expression[] body) {
            return Do(@break, @continue, Expression.Annotate(statementSpan, location), body);
        }
    }
}
