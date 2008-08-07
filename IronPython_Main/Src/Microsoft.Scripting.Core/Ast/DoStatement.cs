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

    public sealed class DoStatement : Expression {
        private readonly Expression _test;
        private readonly Expression _body;

        private readonly LabelTarget _label;

        internal DoStatement(Annotations annotations, LabelTarget label, Expression test, Expression body)
            : base(ExpressionType.DoStatement, typeof(void), annotations, null) {
            _test = test;
            _body = body;
            _label = label;
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Body {
            get { return _body; }
        }

        new public LabelTarget Label {
            get { return _label; }
        }
    }

    public partial class Expression {
        public static DoStatement DoWhile(Expression body, Expression test) {
            return DoWhile(body, test, null, Annotations.Empty);
        }
        public static DoStatement DoWhile(Expression body, Expression test, LabelTarget label) {
            return DoWhile(body, test, label, Annotations.Empty);
        }
        public static DoStatement DoWhile(Expression body, Expression test, LabelTarget label, Annotations annotations) {
            RequiresCanRead(body, "body");
            RequiresCanRead(test, "test");
            ContractUtils.Requires(test.Type == typeof(bool), "test", Strings.ConditionMustBeBoolean);
            return new DoStatement(annotations, label, test, body);
        }
    }
}
