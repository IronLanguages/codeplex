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
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {

    // TODO: rename to DoWhileExpression
    public sealed class DoStatement : Expression {
        private readonly Expression _test;
        private readonly Expression _body;
        private readonly LabelTarget _break;
        private readonly LabelTarget _continue;

        internal DoStatement(Expression body, Expression test, LabelTarget @break, LabelTarget @continue, Annotations annotations)
            : base(ExpressionType.DoStatement, typeof(void), annotations) {
            _test = test;
            _body = body;
            _break = @break;
            _continue = @continue;
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Body {
            get { return _body; }
        }

        public LabelTarget BreakLabel {
            get { return _break; }
        }

        public LabelTarget ContinueLabel {
            get { return _continue; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitDoWhile(this);
        }
    }

    public partial class Expression {
        public static DoStatement DoWhile(Expression body, Expression test) {
            return DoWhile(body, test, null, null, null);
        }
        public static DoStatement DoWhile(Expression body, Expression test, LabelTarget @break, LabelTarget @continue) {
            return DoWhile(body, test, @break, @continue, null);
        }
        public static DoStatement DoWhile(Expression body, Expression test, LabelTarget @break, LabelTarget @continue, Annotations annotations) {
            RequiresCanRead(body, "body");
            RequiresCanRead(test, "test");
            ContractUtils.Requires(test.Type == typeof(bool), "test", Strings.ConditionMustBeBoolean);
            // TODO: lift the restriction on break, and allow loops to have non-void type
            ContractUtils.Requires(@break == null || @break.Type == typeof(void), "break", Strings.LabelTypeMustBeVoid);
            ContractUtils.Requires(@continue == null || @continue.Type == typeof(void), "continue", Strings.LabelTypeMustBeVoid);
            return new DoStatement(body, test, @break, @continue, annotations);
        }
    }
}
