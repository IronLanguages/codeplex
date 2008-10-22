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
using System.Text;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public sealed class ConditionalExpression : Expression {
        private readonly Expression _test;
        private readonly Expression _true;
        private readonly Expression _false;

        internal ConditionalExpression(Annotations annotations, Expression test, Expression ifTrue, Expression ifFalse, Type type)
            : base(ExpressionType.Conditional, type, annotations) {
            _test = test;
            _true = ifTrue;
            _false = ifFalse;
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression IfTrue {
            get { return _true; }
        }

        public Expression IfFalse {
            get { return _false; }
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");

            builder.Append("IIF(");
            _test.BuildString(builder);
            builder.Append(", ");
            _true.BuildString(builder);
            builder.Append(", ");
            _false.BuildString(builder);
            builder.Append(")");
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitConditional(this);
        }
    }

    public partial class Expression {
        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse) {
            return Condition(test, ifTrue, ifFalse, Annotations.Empty);
        }

        //CONFORMING
        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse, Annotations annotations) {
            RequiresCanRead(test, "test");
            RequiresCanRead(ifTrue, "ifTrue");
            RequiresCanRead(ifFalse, "ifFalse");

            if (test.Type != typeof(bool)) {
                throw Error.ArgumentMustBeBoolean();
            }
            if (ifTrue.Type != ifFalse.Type) {
                throw Error.ArgumentTypesMustMatch();
            }

            return new ConditionalExpression(annotations, test, ifTrue, ifFalse, ifTrue.Type);
        }
    }
}