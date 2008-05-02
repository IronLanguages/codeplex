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
    public sealed class LoopStatement : Expression {
        private readonly Expression _test;
        private readonly Expression _increment;
        private readonly Expression /*!*/ _body;
        private readonly Expression _else;
        private readonly LabelTarget _label;

        /// <summary>
        /// Null test means infinite loop.
        /// </summary>
        internal LoopStatement(Annotations annotations, LabelTarget label, Expression test, Expression increment, Expression /*!*/ body, Expression @else)
            : base(annotations, AstNodeType.LoopStatement, typeof(void)) {
            _test = test;
            _increment = increment;
            _body = body;
            _else = @else;
            _label = label;
        }

        internal SourceLocation Header {
            get { return Annotations.Get<SourceLocation>(); }
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

         new public LabelTarget Label {
            get { return _label; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// TODO: review which of these overloads we actually need
    /// </summary>
    public partial class Expression {
        public static LoopStatement While(Expression test, Expression body, Expression @else) {
            return Loop(SourceSpan.None, SourceLocation.None, null, test, null, body, @else);
        }

        public static LoopStatement While(LabelTarget label, Expression test, Expression body, Expression @else) {
            return Loop(SourceSpan.None, SourceLocation.None, label, test, null, body, @else);
        }

        public static LoopStatement While(SourceSpan span, SourceLocation header, Expression test, Expression body, Expression @else) {
            return Loop(span, header, null, test, null, body, @else);
        }

        public static LoopStatement While(SourceSpan span, SourceLocation header, LabelTarget label, Expression test, Expression body, Expression @else) {
            return Loop(span, header, label, test, null, body, @else);
        }

        public static LoopStatement Infinite(Expression body) {
            return Loop(SourceSpan.None, SourceLocation.None, null, null, null, body, null);
        }

        public static LoopStatement Infinite(LabelTarget label, Expression body) {
            return Loop(SourceSpan.None, SourceLocation.None, label, null, null, body, null);
        }

        public static LoopStatement Infinite(params Expression[] body) {
            return Loop(SourceSpan.None, SourceLocation.None, null, null, null, Block(body), null);
        }

        public static LoopStatement Infinite(LabelTarget label, params Expression[] body) {
            return Loop(SourceSpan.None, SourceLocation.None, label, null, null, Block(body), null);
        }

        public static LoopStatement Loop(Expression test, Expression increment, Expression body, Expression @else) {
            return Loop(SourceSpan.None, SourceLocation.None, null, test, increment, body, @else);
        }

        public static LoopStatement Loop(LabelTarget label, Expression test, Expression increment, Expression body, Expression @else) {
            return Loop(SourceSpan.None, SourceLocation.None, label, test, increment, body, @else);
        }

        public static LoopStatement Loop(SourceSpan span, SourceLocation header, LabelTarget label, Expression test, Expression increment, Expression body, Expression @else) {
            return Loop(Annotate(span, header), label, test, increment, body, @else);
        }
 
        public static LoopStatement Loop(Annotations annotations, LabelTarget label, Expression test, Expression increment, Expression body, Expression @else) {
            ContractUtils.RequiresNotNull(body, "body");
            ContractUtils.Requires(test == null || test.Type == typeof(bool), "test");
            return new LoopStatement(annotations, label, test, increment, body, @else);
        }
    }
}
