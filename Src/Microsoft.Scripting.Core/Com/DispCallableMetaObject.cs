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

using System.Collections.Generic;
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

        public override MetaObject BindGetIndex(GetIndexBinder binder, params MetaObject[] args) {
            if (_callable.ComMethodDesc.IsPropertyGet) {
                if (args.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                    return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, args);
                }
                return BindComInvoke(binder.Arguments, args);
            }
            return base.BindGetIndex(binder, args);
        }

        public override MetaObject BindSetIndex(SetIndexBinder binder, params MetaObject[] args) {
            if (_callable.ComMethodDesc.IsPropertyPut) {
                if (args.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                    return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, args);
                }
                return BindComInvoke(binder.Arguments, args);
            }
            return base.BindSetIndex(binder, args);
        }

        public override MetaObject BindInvoke(InvokeBinder binder, MetaObject[] args) {
            if (args.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, args);
            }
            return BindComInvoke(binder.Arguments, args);
        }

        private MetaObject BindComInvoke(IList<ArgumentInfo> argInfo, MetaObject[] args) {
            var callable = Expression;
            var dispCall = Expression.Convert(callable, typeof(DispCallable));
            var methodDesc = Expression.Property(dispCall, typeof(DispCallable).GetProperty("ComMethodDesc"));
            var methodRestriction = Expression.Equal(methodDesc, Expression.Constant(_callable.ComMethodDesc));

            return new ComInvokeBinder(
                argInfo,
                args,
                Restrictions.GetTypeRestriction(callable, Value.GetType()).Merge(Restrictions.GetExpressionRestriction(methodRestriction)),
                methodDesc,
                Expression.Property(dispCall, typeof(DispCallable).GetProperty("DispatchObject")),
                _callable.ComMethodDesc
            ).Invoke();
        }

        [Obsolete("Use UnaryOperation or BinaryOperation")]
        public override MetaObject BindOperation(OperationBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            switch (binder.Operation) {
                case "CallSignatures":
                case "Documentation":
                    return Documentation(args);

                case "IsCallable":
                    return IsCallable(args);

                // TODO: remove when Python switches over to GetIndexBinder
                case "GetItem":
                    if (_callable.ComMethodDesc.IsPropertyGet) {
                        return BindComInvoke(new ArgumentInfo[0], args);
                    }
                    break;

                // TODO: remove when Python switches over to SetIndexBinder
                case "SetItem":
                    if (_callable.ComMethodDesc.IsPropertyPut) {
                        return BindComInvoke(new ArgumentInfo[0], args);
                    }
                    break;
            }

            return binder.FallbackOperation(this, args);
        }

        private MetaObject Documentation(MetaObject[] args) {
            return new MetaObject(
                // this.ComMethodDesc.Signature 
                Expression.Property(
                    Expression.Property(
                        Helpers.Convert(Expression, typeof(DispCallable)),
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
                Expression.Constant(true),
                Restrictions.Combine(args).Merge(Restrictions.GetTypeRestriction(Expression, Value.GetType()))
            );
        }
    }
}

#endif
