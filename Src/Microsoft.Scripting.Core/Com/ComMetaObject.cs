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
using Microsoft.Scripting.Binders;
using Microsoft.Scripting.Utils;
using System.Collections.Generic;

namespace Microsoft.Scripting.ComInterop {
    internal class ComMetaObject : MetaObject {
        internal ComMetaObject(Expression expression, Restrictions restrictions, object arg)
            : base(expression, restrictions, arg) {
        }

        #region MetaObject

        public override MetaObject BindInvokeMember(InvokeMemberBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(args.AddFirst(WrapSelf()));
        }

        public override MetaObject BindConvert(ConvertBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf());
        }

        public override MetaObject BindCreateInstance(CreateInstanceBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(args.AddFirst(WrapSelf()));
        }

        public override MetaObject BindDeleteMember(DeleteMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf());
        }

        public override MetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf());
        }

        public override MetaObject BindInvoke(InvokeBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(args.AddFirst(WrapSelf()));
        }

        public override MetaObject BindSetMember(SetMemberBinder binder, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), value);
        }

        public override IEnumerable<string> GetDynamicMemberNames() {
            return ComObject.ObjectToComObject(Value).MemberNames;
        }
        
        public override MetaObject BindBinaryOperation(BinaryOperationBinder binder, MetaObject arg) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), arg);
        }

        public override MetaObject BindDeleteIndex(DeleteIndexBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), args);
        }

        public override MetaObject BindGetIndex(GetIndexBinder binder, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), args);
        }

        public override MetaObject BindSetIndex(SetIndexBinder binder, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), args);
        }

        public override MetaObject BindUnaryOperation(UnaryOperationBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf());
        }

        #endregion

        private MetaObject WrapSelf() {
            return new MetaObject(
                ComObject.RcwToComObject(Expression),
                Restrictions.GetExpressionRestriction(
                    Expression.AndAlso(
                        Expression.NotEqual(
                            Helpers.Convert(Expression, typeof(object)),
                            Expression.Constant(null)
                        ),
                        Expression.Call(
                            typeof(System.Runtime.InteropServices.Marshal).GetMethod("IsComObject"),
                            Helpers.Convert(Expression, typeof(object))
                        )
                    )
                )
            );
        }

        internal static MetaObject GetComMetaObject(Expression expression, object arg) {
            return new ComMetaObject(expression, Restrictions.Empty, arg);
        }
    }
}

#endif
