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
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace System.Scripting.Actions {
    /// <summary>
    /// Provides access to the MetaAction instance within the code generated
    /// by a MetaObject.
    /// </summary>
    internal class ActionSelfExpression : Expression {
        public ActionSelfExpression() : 
            base(ExpressionType.Extension, typeof(MetaAction)) {
        }
    }

    class ActionSelfRewriter : ExpressionTreeVisitor {
        private readonly Expression _self;

        public ActionSelfRewriter(Expression self) {
            _self = self;
        }

        protected override Expression VisitExtension(Expression node) {
            ActionSelfExpression action = node as ActionSelfExpression;
            if (action != null) {
                return _self;
            }

            return base.VisitExtension(node);
        }
    }
}
