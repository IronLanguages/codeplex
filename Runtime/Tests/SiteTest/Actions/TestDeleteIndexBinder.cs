/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Dynamic;

namespace SiteTest.Actions {
    class TestDeleteIndexBinder : DeleteIndexBinder {
        public TestDeleteIndexBinder()
            : base(new CallInfo(0)) {
        }

        public TestDeleteIndexBinder(CallInfo callInfo)
            : base(callInfo) {
        }

        public override DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
            return onBindingError ?? new DynamicMetaObject(
                Expression.Throw(Expression.New(typeof(BindingException))),
                target.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType))
                .Merge(BindingRestrictions.Combine(args))
            );
        }
    }
}
