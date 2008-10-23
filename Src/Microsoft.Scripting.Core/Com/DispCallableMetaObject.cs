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
#if !SILVERLIGHT

using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.ComInterop {
    internal class DispCallableMetaObject : MetaObject {
        private readonly DispCallable _callable;

        internal DispCallableMetaObject(Expression expression, DispCallable callable)
            : base(expression, Restrictions.Empty, callable) {
            _callable = callable;
        }

        public override MetaObject BindInvoke(InvokeBinder action, MetaObject[] args) {
            var callable = Expression;
            var dispCall = Expression.Convert(callable, typeof(DispCallable));
            var methodDesc = Expression.Property(dispCall, typeof(DispCallable).GetProperty("ComMethodDesc"));
            var methodRestriction = Expression.Equal(methodDesc, Expression.Constant(_callable.ComMethodDesc));

            return new ComInvokeBinder(
                action.Arguments,
                args,
                Restrictions.GetTypeRestriction(callable, Value.GetType()).Merge(Restrictions.GetExpressionRestriction(methodRestriction)),
                methodDesc,
                Expression.Property(dispCall, typeof(DispCallable).GetProperty("DispatchObject")),
                _callable.ComMethodDesc
            ).Invoke();
        }

        public override MetaObject BindOperation(OperationBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            switch (action.Operation) {
                case "CallSignatures":
                case "Documentation":
                    return Documentation(args);

                case "IsCallable":
                    return IsCallable(args);
            }

            return action.FallbackOperation(this, args);
        }

        private MetaObject Documentation(MetaObject[] args) {
            return new MetaObject(
                // this.ComMethodDesc.Signature 
                Expression.Property(
                    Expression.Property(
                        Expression.ConvertHelper(Expression, typeof(DispCallable)),
                        typeof(DispCallable).GetProperty("ComMethodDesc")
                    ),
                    typeof(ComMethodDesc).GetProperty("Signature")
                ),
                Restrictions.Combine(args).Merge(Restrictions.GetTypeRestriction(Expression, Value.GetType()))
            );
        }

        private MetaObject IsCallable(MetaObject[] args) {
            return new MetaObject(
                // DispCallable is always callable
                Expression.True(),
                Restrictions.Combine(args).Merge(Restrictions.GetTypeRestriction(Expression, Value.GetType()))
            );
        }
    }
}

#endif
