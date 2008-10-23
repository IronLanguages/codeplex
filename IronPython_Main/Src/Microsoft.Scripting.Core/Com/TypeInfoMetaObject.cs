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
#if !SILVERLIGHT // ComObject

using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.ComInterop {
    internal sealed class TypeInfoMetaObject : MetaObject {
        private readonly Type _comType;

        internal TypeInfoMetaObject(Expression parameter, Type comType, ComObjectWithTypeInfo self)
            : base(parameter, Restrictions.Empty, self) {
            _comType = comType;
        }

        /// <summary>
        /// The rule test now checks to ensure that the wrapper is of the correct type so that any cast against on the RCW will succeed.
        /// Note that the test must NOT test the wrapper itself since the wrapper is a surrogate for the RCW instance and would cause a  
        /// memory leak when a wrapped RCW goes out of scope.  So, the test asks the argument (which is an RCW wrapper) to identify its 
        /// RCW's type.  On the rule creation side, the type is encoded in the test so that when the rule cache is searched the test will 
        /// succeed only if the wrapper's returned RCW type matches that expected by the test. 
        /// </summary>
        internal Restrictions MakeComRestrictions(Type type, PropertyInfo testProperty, object targetObject) {
            Restrictions r1 = Restrictions.GetTypeRestriction(Expression, type);
            Restrictions r2 = Restrictions.GetExpressionRestriction(
                Expression.Equal(
                    Expression.Property(
                        Expression.ConvertHelper(Expression, type),
                        testProperty
                    ),
                    Expression.Constant(targetObject)
                )
            );
            return r1.Merge(r2);
        }

        private Restrictions MakeRestrictions() {
            return MakeComRestrictions(typeof(ComObjectWithTypeInfo), typeof(ComObjectWithTypeInfo).GetProperty("ComType"), _comType);
        }

        public override MetaObject BindInvokeMemberl(InvokeMemberBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.FallbackInvokeMember(UnwrapSelf(), args);
        }

        public override MetaObject BindConvert(ConvertBinder action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.FallbackConvert(UnwrapSelf());
        }

        public override MetaObject BindCreateInstance(CreateInstanceBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.FallbackCreateInstance(UnwrapSelf(), args);
        }

        public override MetaObject BindDeleteMember(DeleteMemberBinder action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.FallbackDeleteMember(UnwrapSelf());
        }

        public override MetaObject BindGetMember(GetMemberBinder action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.FallbackGetMember(UnwrapSelf());
        }

        public override MetaObject BindInvoke(InvokeBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.FallbackInvoke(UnwrapSelf(), args);
        }

        public override MetaObject BindOperation(OperationBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.FallbackOperation(UnwrapSelf(), args);
        }

        public override MetaObject BindSetMember(SetMemberBinder action, MetaObject value) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.FallbackSetMember(UnwrapSelf(), value);
        }

        private MetaObject UnwrapSelf() {
            Expression self = Expression.Property(
                Expression,
                typeof(ComObject).GetProperty("Obj")
            );
            return new MetaObject(
                Expression.ConvertHelper(self, _comType),
                MakeRestrictions(),
                ((ComObject)Value).Obj
            );
        }
    }    
}

#endif
