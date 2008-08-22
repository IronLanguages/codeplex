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

using System.Linq.Expressions;
using System.Scripting.Utils;

namespace System.Scripting.Actions {
    public class MetaObject {
        private readonly Expression _expression;
        private readonly Restrictions _restrictions;
        private readonly object _value;
        private readonly bool _hasValue;

        public MetaObject(Expression expression, Restrictions restrictions) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(restrictions, "restrictions");

            _expression = expression;
            _restrictions = restrictions;
        }

        public MetaObject(Expression expression, Restrictions restrictions, object value)
            : this(expression, restrictions) {
            _value = value;
            _hasValue = true;
        }

        public Expression Expression {
            get {
                return _expression;
            }
        }

        public Restrictions Restrictions {
            get {
                return _restrictions;
            }
        }

        public object Value {
            get {
                return _value;
            }
        }

        public bool HasValue {
            get {
                return _hasValue;
            }
        }

        public Type RuntimeType {
            get {
                if (_hasValue) {
                    if (_value != null) {
                        return _value.GetType();
                    } else {
                        return typeof(None);    // TODO: Return something else ???
                    }
                } else {
                    return null;                // TODO: Return something else ???
                }
            }
        }

        /// <summary>
        /// Checks to see if the known type is good enough for performing operations
        /// on.  This is possible if the type has a value or of the known type of the
        /// expression is a sealed type.
        /// </summary>
        public virtual bool NeedsDeferral {
            get {
                if (HasValue) {
                    return false;
                }

                if (Expression.Type.IsSealedOrValueType()) {
                    return typeof(IDynamicObject).IsAssignableFrom(Expression.Type);
                }

                return true;
            }
        }

        public Type LimitType {
            get {
                return RuntimeType ?? Expression.Type;
            }
        }

        public bool IsDynamicObject {
            get {
                // We can skip _hasValue check as it implies _value == null
                return _value is IDynamicObject;
            }
        }

        public bool IsByRef {
            get {
                ParameterExpression pe = _expression as ParameterExpression;
                return pe != null && pe.IsByRef;
            }
        }

        public virtual MetaObject Restrict(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            if (type == Expression.Type && type.IsSealedOrValueType()) {
                return this;
            }

            if (type == RuntimeType) {
                if (HasValue) {
                    return new RestrictedMetaObject(
                        Expression.ConvertHelper(
                            Expression,
                            TypeUtils.GetVisibleType(type)
                        ),
                        Restrictions.Merge(Restrictions.TypeRestriction(Expression, type)),
                        Value
                    );
                }

                return new RestrictedMetaObject(
                    Expression.ConvertHelper(
                        Expression,
                        TypeUtils.GetVisibleType(type)
                    ),
                    Restrictions.Merge(Restrictions.TypeRestriction(Expression, type))
                );
            }


            if (HasValue) {
                return new MetaObject(
                    Expression.ConvertHelper(
                        Expression,
                        TypeUtils.GetVisibleType(type)
                    ),
                    Restrictions.Merge(Restrictions.TypeRestriction(Expression, type)),
                    Value
                );
            }

            return new MetaObject(
                Expression.ConvertHelper(
                    Expression,
                    TypeUtils.GetVisibleType(type)
                ),
                Restrictions.Merge(Restrictions.TypeRestriction(Expression, type))
            );
        }

        public virtual MetaObject Operation(OperationAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(args);
        }

        public virtual MetaObject Convert(ConvertAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(args);
        }

        public virtual MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(args);
        }

        public virtual MetaObject SetMember(SetMemberAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(args);
        }

        public virtual MetaObject DeleteMember(DeleteMemberAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Call")]
        public virtual MetaObject Call(CallAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(args);
        }

        public virtual MetaObject Invoke(InvokeAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(args);
        }

        public virtual MetaObject Create(CreateAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            return action.Fallback(args);
        }

        // Internal helpers

        internal static Type[] GetTypes(MetaObject[] objects) {
            Type[] res = new Type[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                res[i] = objects[i].RuntimeType ?? objects[i].Expression.Type;
            }

            return res;
        }

        public static Expression[] GetExpressions(MetaObject[] objects) {
            ContractUtils.RequiresNotNull(objects, "objects");

            Expression[] res = new Expression[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                MetaObject mo = objects[i];
                ContractUtils.RequiresNotNull(mo, "objects");
                Expression expr = mo.Expression;
                ContractUtils.RequiresNotNull(expr, "objects");
                res[i] = expr;
            }

            return res;
        }
    }
}
