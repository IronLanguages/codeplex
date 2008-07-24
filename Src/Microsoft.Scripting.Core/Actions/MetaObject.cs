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
using System.Scripting.Generation;

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

                if (CompilerHelpers.IsSealed(Expression.Type)) {
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

        public virtual MetaObject Restrict(Type type) {
            if (type == Expression.Type && CompilerHelpers.IsSealed(type)) {
                return this;
            }

            if (type == RuntimeType) {
                if (HasValue) {
                    return new RestrictedMetaObject(
                        Expression.ConvertHelper(
                            Expression,
                            CompilerHelpers.GetVisibleType(type)
                        ),
                        Restrictions.Merge(Restrictions.TypeRestriction(Expression, type)),
                        Value
                    );
                }

                return new RestrictedMetaObject(
                    Expression.ConvertHelper(
                        Expression,
                        CompilerHelpers.GetVisibleType(type)
                    ),
                    Restrictions.Merge(Restrictions.TypeRestriction(Expression, type))
                );
            }


            if (HasValue) {
                return new MetaObject(
                    Expression.ConvertHelper(
                        Expression,
                        CompilerHelpers.GetVisibleType(type)
                    ),
                    Restrictions.Merge(Restrictions.TypeRestriction(Expression, type)),
                    Value
                );
            }

            return new MetaObject(
                Expression.ConvertHelper(
                    Expression,
                    CompilerHelpers.GetVisibleType(type)
                ),
                Restrictions.Merge(Restrictions.TypeRestriction(Expression, type))
            );
        }

        // Operations
        internal MetaObject DoAction(StandardAction action, MetaObject[] args) {
            switch (action.Kind) {
                case StandardActionKind.Operation:
                    return Operation((OperationAction)action, args);
                case StandardActionKind.GetMember:
                    return GetMember((GetMemberAction)action, args);
                case StandardActionKind.SetMember:
                    return SetMember((SetMemberAction)action, args);
                case StandardActionKind.DeleteMember:
                    return DeleteMember((DeleteMemberAction)action, args);
                case StandardActionKind.Call:
                    return Call((CallAction)action, args);
                case StandardActionKind.Convert:
                    return Convert((ConvertAction)action, args);
                case StandardActionKind.Create:
                    return Create((CreateAction)action, args);
                case StandardActionKind.Invoke:
                    return Invoke((InvokeAction)action, args);
                default:
                    throw Assert.Unreachable;
            }
        }

        public virtual MetaObject Operation(OperationAction action, MetaObject[] args) {
            return action.Fallback(args);
        }

        public virtual MetaObject Convert(ConvertAction action, MetaObject[] args) {
            return action.Fallback(args);
        }

        public virtual MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
            return action.Fallback(args);
        }

        public virtual MetaObject SetMember(SetMemberAction action, MetaObject[] args) {
            return action.Fallback(args);
        }

        public virtual MetaObject DeleteMember(DeleteMemberAction action, MetaObject[] args) {
            return action.Fallback(args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Call")]
        public virtual MetaObject Call(CallAction action, MetaObject[] args) {
            return action.Fallback(args);
        }

        public virtual MetaObject Invoke(InvokeAction action, MetaObject[] args) {
            return action.Fallback(args);
        }

        public virtual MetaObject Create(CreateAction action, MetaObject[] args) {
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
            Expression[] res = new Expression[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                res[i] = objects[i].Expression;
            }

            return res;
        }
    }
}
