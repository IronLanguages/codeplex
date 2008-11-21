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
using System.Diagnostics;

namespace Microsoft.Scripting.ComInterop {
    // ComFallbackMetaObject just delegates everything to the binder.
    // Note that before performing FallBack on a ComObject we need to unwrap it so that
    // binder would act upon the actual object (typically Rcw)
    internal class ComFallbackMetaObject : MetaObject {
        internal ComFallbackMetaObject(Expression expression, Restrictions restrictions, object arg)
            : base(expression, restrictions, arg) {
        }

        #region MetaObject

        public override MetaObject BindConvert(ConvertBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");

            // TODO: we should not need this code soon because C# won't try to
            // do a dynamic conversion to an interface
            if (binder.Type.IsInterface) {
                // Converting a COM object to any interface is always considered possible - it will result in 
                // a QueryInterface at runtime
                return new MetaObject(
                    Expression.Convert(
                        ComObject.RcwFromComObject(Expression),
                        binder.Type
                    ),
                    Restrictions.Merge(Restrictions.GetTypeRestriction(Expression, LimitType))
                );
            }

            return binder.FallbackConvert(UnwrapSelf());
        }

        public override MetaObject BindBinaryOperation(BinaryOperationBinder binder, MetaObject arg) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackBinaryOperation(UnwrapSelf(), arg);
        }

        public override MetaObject BindCreateInstance(CreateInstanceBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackCreateInstance(UnwrapSelf(), args);
        }

        public override MetaObject BindDeleteIndex(DeleteIndexBinder binder, MetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackDeleteIndex(UnwrapSelf(), indexes);
        }

        public override MetaObject BindDeleteMember(DeleteMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackDeleteMember(UnwrapSelf());
        }

        public override MetaObject BindGetIndex(GetIndexBinder binder, MetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetIndex(UnwrapSelf(), indexes);
        }

        public override MetaObject BindInvoke(InvokeBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvoke(UnwrapSelf(), args);
        }

        public override MetaObject BindSetIndex(SetIndexBinder binder, MetaObject[] indexes, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetIndex(UnwrapSelf(), indexes, value);
        }

        public override MetaObject BindUnaryOperation(UnaryOperationBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackUnaryOperation(UnwrapSelf());
        }

        public override MetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetMember(UnwrapSelf());
        }

        public override MetaObject BindInvokeMember(InvokeMemberBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvokeMember(UnwrapSelf(), args);
        }

        public override MetaObject BindSetMember(SetMemberBinder binder, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetMember(UnwrapSelf(), value);
        }

        protected virtual MetaObject UnwrapSelf() {
            ComObject co = base.Value as ComObject;
            Debug.Assert(co != null, "Expecting ComObject.");

            return new MetaObject(
                ComObject.RcwFromComObject(Expression),
                Restrictions.Merge(Restrictions.GetTypeRestriction(Expression, LimitType)),
                co.RuntimeCallableWrapper
            );
        }

        #endregion
    }
}

#endif
