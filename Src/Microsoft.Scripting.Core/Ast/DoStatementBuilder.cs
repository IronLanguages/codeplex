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

using System.Scripting.Utils;

namespace System.Linq.Expressions {
    public sealed class DoStatementBuilder {
        private readonly Expression _body;
        private readonly Annotations _annotations;
        private readonly LabelTarget _label;

        internal DoStatementBuilder(Annotations annotations, LabelTarget label, Expression body) {
            ContractUtils.RequiresNotNull(body, "body");

            _body = body;
            _annotations = annotations;
            _label = label;
        }

        public DoStatement While(Expression condition) {
            ContractUtils.RequiresNotNull(condition, "condition");
            ContractUtils.Requires(condition.Type == typeof(bool), "condition", "Condition must be boolean");

            return new DoStatement(_annotations, _label, condition, _body);
        }
    }

    public partial class Expression {
        public static DoStatementBuilder Do(params Expression[] body) {
            ContractUtils.RequiresNotNullItems(body, "body");
            return new DoStatementBuilder(Annotations.Empty, null, Block(body));
        }

        public static DoStatementBuilder Do(LabelTarget label, params Expression[] body) {
            ContractUtils.RequiresNotNullItems(body, "body");
            return new DoStatementBuilder(Annotations.Empty, label, Block(body));
        }

        public static DoStatementBuilder Do(LabelTarget label, Annotations annotations, params Expression[] body) {
            return new DoStatementBuilder(annotations, label, Block(body));
        }
    }
}
