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

using System;
using System.Dynamic;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.ComInterop;

namespace SiteTest.Actions {
    class TestInvokeBinder : InvokeBinder {
        public TestInvokeBinder()
            : base(new CallInfo(0)) {
        }

        public TestInvokeBinder(CallInfo callInfo)
            : base(callInfo) {
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
            DynamicMetaObject res;
            if (ComBinder.TryBindInvoke(this, target, args, out res)) {
                return res;
            }

            if (target.NeedsDeferral()) {
                var deferArgs = new DynamicMetaObject[args.Length + 1];
                deferArgs[0] = target;
                args.CopyTo(deferArgs, 1);
                return Defer(deferArgs);
            }
            // Note: we could just return one expression with a conditional
            // but doing it this way more accurately simulates a normal
            // binder (produces seperate rules for error and success cases)
            if (target.RuntimeType.IsSubclassOf(typeof(Delegate))) {
                var exprs = DynamicUtils.GetExpressions(args);
                for (int i = 0, n = args.Length; i < n; i++) {
                    exprs[i] = Expression.Convert(exprs[i], typeof(object));
                }

                return new DynamicMetaObject(
                    Expression.Call(
                        Expression.Convert(target.Expression, typeof(Delegate)),
                        typeof(Delegate).GetMethod("DynamicInvoke"),
                        Expression.NewArrayInit(typeof(object), exprs)
                    ),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args)).Merge(
                        BindingRestrictions.GetExpressionRestriction(Expression.TypeIs(target.Expression, typeof(Delegate)))
                    )
                );
            }
            return onBindingError ?? new DynamicMetaObject(
                Expression.Throw(Expression.New(typeof(BindingException)), typeof(object)),
                target.Restrictions.Merge(BindingRestrictions.Combine(args)).Merge(
                    BindingRestrictions.GetExpressionRestriction(Expression.Not(Expression.TypeIs(target.Expression, typeof(Delegate))))
                )
            );
        }
    }
}
