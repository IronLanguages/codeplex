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

#if !SILVERLIGHT

using System.Linq.Expressions;
using System.Scripting.Actions;
using System.Scripting.Utils;

namespace System.Scripting.Com {
    internal class DispCallableMetaObject : MetaObject {
        private readonly DispCallable _callable;

        internal DispCallableMetaObject(Expression expression, DispCallable callable)
            : base(expression, Restrictions.Empty, callable) {
            _callable = callable;
        }

        public override MetaObject Invoke(InvokeAction action, MetaObject[] args) {
            Expression callable = args[0].Expression;
            Expression dispCall = Expression.Convert(callable, typeof(DispCallable));
            return new InvokeBinder(
                action.Arguments,
                args,
                Restrictions.TypeRestriction(callable, Value.GetType()),
                Expression.Property(dispCall, typeof(DispCallable).GetProperty("ComMethodDesc")),
                Expression.Property(dispCall, typeof(DispCallable).GetProperty("DispatchObject")),
                _callable.ComMethodDesc
            ).Invoke();
        }

        public override MetaObject Operation(OperationAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            switch (action.Operation) {
                case "CallSignatures":
                case "Documentation":
                    return Documentation(args);

                case "IsCallable":
                    return IsCallable(args);
            }

            return action.Fallback(args);
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
                Restrictions.Combine(args).Merge(Restrictions.TypeRestriction(Expression, Value.GetType()))
            );
        }

        private MetaObject IsCallable(MetaObject[] args) {
            return new MetaObject(
                // DispCallable is always callable
                Expression.True(),
                Restrictions.Combine(args).Merge(Restrictions.TypeRestriction(Expression, Value.GetType()))
            );
        }
    }
}

#endif
