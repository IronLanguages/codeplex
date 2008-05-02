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

using System;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class ConditionalExpression : Expression {
        private readonly Expression/*!*/ _test;
        private readonly Expression/*!*/ _true;
        private readonly Expression/*!*/ _false;

        internal ConditionalExpression(Annotations annotations, Expression/*!*/ test, Expression/*!*/ ifTrue, Expression/*!*/ ifFalse, Type/*!*/ type)
            : base(annotations, AstNodeType.Conditional, type) {
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
    }

    public partial class Expression {
        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse) {
            return Condition(Annotations.Empty, test, ifTrue, ifFalse);
        }

        public static ConditionalExpression Condition(SourceSpan span, Expression test, Expression ifTrue, Expression ifFalse) {
            return Condition(Annotate(span), test, ifTrue, ifFalse);
        }

        public static ConditionalExpression Condition(Annotations annotations, Expression test, Expression ifTrue, Expression ifFalse) {
            ContractUtils.RequiresNotNull(test, "test");
            ContractUtils.RequiresNotNull(ifTrue, "ifTrue");
            ContractUtils.RequiresNotNull(ifFalse, "ifFalse");

            ContractUtils.Requires(test.Type == typeof(bool), "test", "Test must be bool");
            ContractUtils.Requires(ifTrue.Type == ifFalse.Type, "ifTrue", "Types must match");

            return new ConditionalExpression(annotations, test, ifTrue, ifFalse, ifTrue.Type);
        }
    }
}