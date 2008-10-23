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
    public sealed class LoopExpression : Expression {
        private readonly Expression _test;
        private readonly Expression _increment;
        private readonly Expression _body;
        private readonly Expression _else;
        private readonly LabelTarget _break;
        private readonly LabelTarget _continue;


        /// <summary>
        /// Null test means infinite loop.
        /// </summary>
        internal LoopExpression(Expression test, Expression increment, Expression body, Expression @else, LabelTarget @break, LabelTarget @continue, Annotations annotations)
            : base(annotations) {
            _test = test;
            _increment = increment;
            _body = body;
            _else = @else;
            _break = @break;
            _continue = @continue;
        }

        protected override Type GetExpressionType() {
            return typeof(void);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.LoopStatement;
        }

        internal override Expression.NodeFlags GetFlags() {
            return NodeFlags.CanRead;
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Increment {
            get { return _increment; }
        }

        public Expression Body {
            get { return _body; }
        }

        public Expression ElseStatement {
            get { return _else; }
        }

        public LabelTarget BreakLabel {
            get { return _break; }
        }

        public LabelTarget ContinueLabel {
            get { return _continue; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitLoop(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// TODO: review which of these overloads we actually need
    /// </summary>
    public partial class Expression {
        public static LoopExpression Loop(Expression test, Expression increment, Expression body, Expression @else, LabelTarget @break, LabelTarget @continue) {
            return Loop(test, increment, body, @else, @break, @continue, null);
        }

        public static LoopExpression Loop(Expression test, Expression increment, Expression body, Expression @else, LabelTarget @break, LabelTarget @continue, Annotations annotations) {
            RequiresCanRead(body, "body");
            if (test != null) {
                RequiresCanRead(test, "test");
                ContractUtils.Requires(test.Type == typeof(bool), "test", Strings.ArgumentMustBeBoolean);
            }
            if (increment != null) {
                RequiresCanRead(increment, "increment");
            }
            if (@else != null) {
                RequiresCanRead(@else, "else");
            }
            // TODO: lift the restriction on break, and allow loops to have non-void type
            ContractUtils.Requires(@break == null || @break.Type == typeof(void), "break", Strings.LabelTypeMustBeVoid);
            ContractUtils.Requires(@continue == null || @continue.Type == typeof(void), "continue", Strings.LabelTypeMustBeVoid);
            return new LoopExpression(test, increment, body, @else, @break, @continue, annotations);
        }
    }
}
