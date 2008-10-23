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
    internal class ComMetaObject : MetaObject {
        internal ComMetaObject(Expression expression, Restrictions restrictions, object arg)
            : base(expression, restrictions, arg) {
        }

        #region MetaObject

        public override MetaObject BindInvokeMemberl(InvokeMemberBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(args.AddFirst(WrapSelf()));
        }

        public override MetaObject BindConvert(ConvertBinder action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(WrapSelf());
        }

        public override MetaObject BindCreateInstance(CreateInstanceBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(args.AddFirst(WrapSelf()));
        }

        public override MetaObject BindDeleteMember(DeleteMemberBinder action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(WrapSelf());
        }

        public override MetaObject BindGetMember(GetMemberBinder action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(WrapSelf());
        }

        public override MetaObject BindInvoke(InvokeBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(args.AddFirst(WrapSelf()));
        }

        public override MetaObject BindOperation(OperationBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(args.AddFirst(WrapSelf()));
        }

        public override MetaObject BindSetMember(SetMemberBinder action, MetaObject value) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Defer(WrapSelf(), value);
        }

        #endregion

        private MetaObject WrapSelf() {
            return new MetaObject(
                Expression.Call(
                    typeof(ComObject).GetMethod("ObjectToComObject"),
                    Expression.ConvertHelper(Expression, typeof(object))
                ),
                Restrictions.GetExpressionRestriction(
                    Expression.AndAlso(
                        Expression.NotEqual(
                            Expression.ConvertHelper(Expression, typeof(object)),
                            Expression.Null()
                        ),
                        Expression.Call(
                            typeof(System.Runtime.InteropServices.Marshal).GetMethod("IsComObject"),
                            Expression.ConvertHelper(Expression, typeof(object))
                        )
                    )
                )
            );
        }

        internal static MetaObject GetComMetaObject(Expression expression, object arg) {
            return new ComMetaObject(expression, Restrictions.Empty, arg);
        }

        private static readonly Type ComObjectType = typeof(object).Assembly.GetType("System.__ComObject");

        internal static bool IsComObject(object obj) {
            // we can't use System.Runtime.InteropServices.Marshal.IsComObject(obj) since it doesn't work in partial trust
            return obj != null && ComObjectType.IsAssignableFrom(obj.GetType());
        }
    }
}

#endif
