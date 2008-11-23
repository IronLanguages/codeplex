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

namespace Microsoft.Scripting.Com {
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
            Restrictions r1 = Restrictions.TypeRestriction(Expression, type);
            Restrictions r2 = Restrictions.ExpressionRestriction(
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

        public override MetaObject Call(CallAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(UnwrapSelf(), args);
        }

        public override MetaObject Convert(ConvertAction action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(UnwrapSelf());
        }

        public override MetaObject Create(CreateAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(UnwrapSelf(), args);
        }

        public override MetaObject DeleteMember(DeleteMemberAction action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(UnwrapSelf());
        }

        public override MetaObject GetMember(GetMemberAction action) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(UnwrapSelf());
        }

        public override MetaObject Invoke(InvokeAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(UnwrapSelf(), args);
        }

        public override MetaObject Operation(OperationAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(UnwrapSelf(), args);
        }

        public override MetaObject SetMember(SetMemberAction action, MetaObject value) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(UnwrapSelf(), value);
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
