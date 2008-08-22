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
    public sealed class LoopStatement : Expression {
        private readonly Expression _test;
        private readonly Expression _increment;
        private readonly Expression _body;
        private readonly Expression _else;
        private readonly LabelTarget _label;

        /// <summary>
        /// Null test means infinite loop.
        /// </summary>
        internal LoopStatement(Annotations annotations, LabelTarget label, Expression test, Expression increment, Expression body, Expression @else)
            : base(ExpressionType.LoopStatement, typeof(void), annotations) {
            _test = test;
            _increment = increment;
            _body = body;
            _else = @else;
            _label = label;
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
        public static LoopStatement Loop(Expression test, Expression increment, Expression body, Expression @else, LabelTarget label) {
            return Loop(test, increment, body, @else, label, Annotations.Empty);
        }

        public static LoopStatement Loop(Expression test, Expression increment, Expression body, Expression @else, LabelTarget label, Annotations annotations) {
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
            return new LoopStatement(annotations, label, test, increment, body, @else);
        }
    }
}
