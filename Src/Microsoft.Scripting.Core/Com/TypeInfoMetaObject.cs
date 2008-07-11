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

#if !SILVERLIGHT // ComObject

using System.Reflection;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Utils;

namespace System.Scripting.Com {
    internal sealed class TypeInfoMetaObject : MetaObject {
        private readonly Type _comType;

        internal TypeInfoMetaObject(Expression parameter, Type comType)
            : base(parameter, Restrictions.Empty) {
            _comType = comType;
        }

        
        public Restrictions MakeComRestrictions(Type type, PropertyInfo testProperty, object targetObject) {
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
            return action.Fallback(UnwrapComObject(args));
        }

        public override MetaObject Convert(ConvertAction action, MetaObject[] args) {
            return action.Fallback(UnwrapComObject(args));
        }

        public override MetaObject Create(CreateAction action, MetaObject[] args) {
            return action.Fallback(UnwrapComObject(args));
        }

        public override MetaObject DeleteMember(DeleteMemberAction action, MetaObject[] args) {
            return action.Fallback(UnwrapComObject(args));
        }

        public override MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
            return action.Fallback(UnwrapComObject(args));
        }

        public override MetaObject Invoke(InvokeAction action, MetaObject[] args) {
            return action.Fallback(UnwrapComObject(args));
        }

        public override MetaObject Operation(OperationAction action, MetaObject[] args) {
            return action.Fallback(UnwrapComObject(args));
        }

        public override MetaObject SetMember(SetMemberAction action, MetaObject[] args) {
            return action.Fallback(UnwrapComObject(args));
        }

        private MetaObject[] UnwrapComObject(MetaObject[] args) {
            MetaObject[] copy = ArrayUtils.Copy(args);

            // Replace self with unwrapped Com object value
            copy[0] = new MetaObject(
                Expression.Convert(
                    Expression.Property(
                        Expression,
                        typeof(ComObject).GetProperty("Obj")
                    ),
                    _comType
                ),
                MakeRestrictions()
            );

            return copy;
        }
    }
}

#endif
