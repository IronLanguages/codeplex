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
using System.Diagnostics;
using System.Text;
using System.Linq.Expressions;
using System.Scripting.Utils;

namespace System.Scripting.Actions {
    /// <summary>
    /// Provides access to the MetaAction instance within the code generated
    /// by a MetaObject.
    /// 
    /// TODO: public, rename to CallSiteBinderExpression. Add factory somewhere
    /// (on CallSiteBinder?). This should be exposed for all binders if it's
    /// supported, not just MetaActions.
    /// </summary>
    internal class ActionSelfExpression : Expression {
        internal ActionSelfExpression() :
            base(typeof(CallSiteBinder), false, null) {
        }
    }

    internal sealed class ActionSelfRewriter : ExpressionTreeVisitor {
        private readonly Expression _self;

        internal ActionSelfRewriter(Expression self) {
            Debug.Assert(TypeUtils.AreReferenceAssignable(typeof(CallSiteBinder), self.Type));
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
