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


using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// Represents an infinite loop. It can be exited with "break".
    /// </summary>
#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(Expression.LoopExpressionProxy))]
#endif
    public sealed class LoopExpression : Expression {
        private readonly Expression _body;
        private readonly LabelTarget _break;
        private readonly LabelTarget _continue;

        internal LoopExpression(Expression body, LabelTarget @break, LabelTarget @continue) {
            _body = body;
            _break = @break;
            _continue = @continue;
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        protected override Type TypeImpl() {
            return _break == null ? typeof(void) : _break.Type;
        }

        /// <summary>
        /// Returns the node type of this Expression. Extension nodes should return
        /// ExpressionType.Extension when overriding this method.
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> of the expression.</returns>
        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Loop;
        }

        /// <summary>
        /// Gets the <see cref="Expression"/> that is the body of the loop.
        /// </summary>
        public Expression Body {
            get { return _body; }
        }

        /// <summary>
        /// Gets the <see cref="LabelTarget"/> that is used by the loop body as a break statement target.
        /// </summary>
        public LabelTarget BreakLabel {
            get { return _break; }
        }

        /// <summary>
        /// Gets the <see cref="LabelTarget"/> that is used by the loop body as a continue statement target.
        /// </summary>
        public LabelTarget ContinueLabel {
            get { return _continue; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitLoop(this);
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates a <see cref="LoopExpression"/> with the given body.
        /// </summary>
        /// <param name="body">The body of the loop.</param>
        /// <returns>The created <see cref="LoopExpression"/>.</returns>
        public static LoopExpression Loop(Expression body) {
            return Loop(body, null);
        }

        /// <summary>
        /// Creates a <see cref="LoopExpression"/> with the given body and break target.
        /// </summary>
        /// <param name="body">The body of the loop.</param>
        /// <param name="break">The break target used by the loop body.</param>
        /// <returns>The created <see cref="LoopExpression"/>.</returns>
        public static LoopExpression Loop(Expression body, LabelTarget @break) {
            return Loop(body, @break, null);
        }

        /// <summary>
        /// Creates a <see cref="LoopExpression"/> with the given body.
        /// </summary>
        /// <param name="body">The body of the loop.</param>
        /// <param name="break">The break target used by the loop body.</param>
        /// <param name="continue">The continue target used by the loop body.</param>
        /// <returns>The created <see cref="LoopExpression"/>.</returns>
        public static LoopExpression Loop(Expression body, LabelTarget @break, LabelTarget @continue) {
            RequiresCanRead(body, "body");
            ContractUtils.Requires(@continue == null || @continue.Type == typeof(void), "continue", Strings.LabelTypeMustBeVoid);
            return new LoopExpression(body, @break, @continue);
        }
    }
}
